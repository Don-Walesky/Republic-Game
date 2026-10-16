namespace Republic.Core.Networking;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;

/// <summary>
/// Authoritative TCP host facilitating single source of truth for multiplayer world state
/// (clock, sovereign countries, treasuries, diplomacy, and trade).
/// Validates player ownership, enforces deduplication, and persists state to a single file across restarts.
/// </summary>
public sealed class RepublicHost : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly int _requestedPort;
    private readonly ConcurrentDictionary<string, ConnectedClient> _clients = new();
    private readonly ConcurrentDictionary<string, CountrySnapshot> _knownCountries = new();
    private readonly ConcurrentDictionary<string, DiplomaticSnapshot> _diplomacy = new();
    private readonly ConcurrentDictionary<string, TradeProposal> _trades = new();
    private readonly ConcurrentDictionary<string, RepuTreasury> _treasuries = new();
    private readonly ConcurrentDictionary<string, string> _countryOwners = new();
    private readonly ConcurrentDictionary<string, string> _playerCountries = new();
    private readonly ConcurrentDictionary<string, bool> _processedRequests = new();
    private readonly object _saveLock = new();
    private readonly CancellationTokenSource _cts = new();

    private TcpListener? _listener;
    private Task? _acceptTask;
    private NetworkClockSnapshot _currentClock;
    private int _boundPort;
    private bool _isStarted;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the file path used for host world state persistence across restarts.
    /// If null or empty, automatic world state persistence is disabled.
    /// </summary>
    public string? SaveFilePath { get; set; }

    /// <summary>
    /// Gets the authoritative world state tracking entities, messages, and trade proposals.
    /// </summary>
    public WorldState World { get; } = new();

    /// <summary>
    /// Event invoked whenever a connected peer or host updates their country state snapshot.
    /// </summary>
    public event Action<CountrySnapshot>? OnCountryUpdated;

    /// <summary>
    /// Event invoked whenever a bilateral diplomatic recognition relation is updated.
    /// </summary>
    public event Action<DiplomaticSnapshot>? OnDiplomacyUpdated;

    /// <summary>
    /// Event invoked whenever a diplomatic message is received.
    /// </summary>
    public event Action<DiplomaticMessage>? OnMessageReceived;

    /// <summary>
    /// Event invoked whenever a trade proposal is proposed, accepted, or rejected.
    /// </summary>
    public event Action<TradeProposal>? OnTradeUpdated;

    /// <summary>
    /// Event invoked whenever the authoritative shared world clock is updated.
    /// </summary>
    public event Action<NetworkClockSnapshot>? OnClockUpdated;

    /// <summary>
    /// Event invoked whenever the count of connected client peers changes.
    /// </summary>
    public event Action<int>? OnClientCountChanged;

    /// <summary>
    /// Gets the actual TCP port the host is actively listening on.
    /// </summary>
    public int Port => _boundPort;

    /// <summary>
    /// Gets a value indicating whether the host is actively listening and accepting connections.
    /// </summary>
    public bool IsRunning => _isStarted && !_disposed;

    /// <summary>
    /// Gets the number of actively connected remote client peers.
    /// </summary>
    public int ConnectedClientCount => _clients.Count;

    /// <summary>
    /// Gets the most recent synchronized clock snapshot.
    /// </summary>
    public NetworkClockSnapshot CurrentClock => _currentClock;

    /// <summary>
    /// Gets a dictionary of all known sovereign country snapshots received across the session.
    /// </summary>
    public IReadOnlyDictionary<string, CountrySnapshot> KnownCountries => _knownCountries;

    /// <summary>
    /// Gets a dictionary of active diplomatic recognition records.
    /// </summary>
    public IReadOnlyDictionary<string, DiplomaticSnapshot> KnownDiplomacy => _diplomacy;

    /// <summary>
    /// Gets a dictionary of active and resolved trade proposals.
    /// </summary>
    public IReadOnlyDictionary<string, TradeProposal> KnownTrades => _trades;

    /// <summary>
    /// Gets a dictionary mapping country identifiers to their authorized owning player or connection ID.
    /// </summary>
    public IReadOnlyDictionary<string, string> CountryOwners => _countryOwners;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicHost"/> class.
    /// </summary>
    /// <param name="port">The TCP port to bind (default 7777, or 0 for dynamic ephemeral port allocation).</param>
    /// <param name="saveFilePath">Optional single-file path for persistent world state storage.</param>
    public RepublicHost(int port = 7777, string? saveFilePath = null)
    {
        _requestedPort = port;
        SaveFilePath = saveFilePath;
        _currentClock = new NetworkClockSnapshot
        {
            Tick = 0,
            Day = 0,
            TimeString = "Day 0, 00:00 WAT",
            IsPaused = false
        };

        if (!string.IsNullOrWhiteSpace(SaveFilePath) && File.Exists(SaveFilePath))
        {
            LoadWorld(SaveFilePath);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicHost"/> class anchored to an existing simulation time system.
    /// </summary>
    /// <param name="timeSystem">Authoritative time system provider.</param>
    /// <param name="port">The TCP port to bind (default 7777).</param>
    /// <param name="saveFilePath">Optional single-file path for persistent world state storage.</param>
    public RepublicHost(ITimeSystem timeSystem, int port = 7777, string? saveFilePath = null)
        : this(port, saveFilePath)
    {
        ArgumentNullException.ThrowIfNull(timeSystem);
        if (_currentClock.Tick == 0)
        {
            _currentClock = NetworkClockSnapshot.FromRepublicTime(
                timeSystem.CurrentRepublicTime,
                timeSystem.CurrentTick,
                timeSystem.IsPaused);
        }
    }

    /// <summary>
    /// Starts the TCP listener and begins accepting incoming client connections asynchronously.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_isStarted)
        {
            return;
        }

        _listener = new TcpListener(IPAddress.Any, _requestedPort);
        _listener.Start();
        _boundPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _isStarted = true;

        _acceptTask = Task.Run(AcceptConnectionsLoopAsync);
    }

    /// <summary>
    /// Attempts to register or claim sovereign ownership of the specified country for the given player.
    /// </summary>
    public bool TryClaimCountry(string playerId, string countryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);

        if (_countryOwners.TryGetValue(countryId, out var existingOwner))
        {
            return existingOwner == playerId;
        }

        if (_countryOwners.TryAdd(countryId, playerId))
        {
            _playerCountries[playerId] = countryId;
            SaveWorld();
            return true;
        }

        return _countryOwners.TryGetValue(countryId, out var currentOwner) && currentOwner == playerId;
    }

    /// <summary>
    /// Checks whether the specified player identity owns the country.
    /// </summary>
    public bool IsOwner(string playerId, string countryId)
    {
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }

        return _countryOwners.TryGetValue(countryId, out var owner) && owner == playerId;
    }

    /// <summary>
    /// Gets the registered owning player identifier for the specified country.
    /// </summary>
    public string? GetCountryOwner(string countryId)
    {
        return _countryOwners.TryGetValue(countryId, out var owner) ? owner : null;
    }

    /// <summary>
    /// Sets or updates the host country's state snapshot and broadcasts it to all connected peers.
    /// </summary>
    public void SetHostCountry(CountrySnapshot country)
    {
        ArgumentNullException.ThrowIfNull(country);
        _countryOwners[country.Id] = "host";
        _playerCountries["host"] = country.Id;
        BroadcastCountry(country);
        SaveWorld();
    }

    /// <summary>
    /// Broadcasts an updated country state snapshot to all connected client peers.
    /// </summary>
    public void BroadcastCountry(CountrySnapshot country)
    {
        ArgumentNullException.ThrowIfNull(country);
        _knownCountries[country.Id] = country;
        OnCountryUpdated?.Invoke(country);

        var message = new NetworkMessage
        {
            Type = "country",
            SenderId = "host",
            Country = country
        };

        _ = BroadcastMessageAsync(message);
    }

    /// <summary>
    /// Registers a sovereign treasury with the host for authoritative trade settlement.
    /// </summary>
    public void RegisterTreasury(string countryId, RepuTreasury treasury)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        ArgumentNullException.ThrowIfNull(treasury);
        _treasuries[countryId] = treasury;
    }

    /// <summary>
    /// Registers a sovereign country with the host, wiring its treasury and recording it on the world state.
    /// </summary>
    public void RegisterCountry(Republic.Core.World.Models.Country country)
    {
        ArgumentNullException.ThrowIfNull(country);
        _treasuries[country.Id] = country.Treasury;
        _countryOwners.TryAdd(country.Id, "host");
        World.Countries.Add(country);
        SaveWorld();
    }

    /// <summary>
    /// Retrieves or initializes a sovereign treasury for the specified country identifier.
    /// </summary>
    public RepuTreasury GetOrCreateTreasury(string countryId, CountrySnapshot? snapshot = null)
    {
        if (_treasuries.TryGetValue(countryId, out var existing))
        {
            return existing;
        }

        var balance = ParseTreasuryBalance(snapshot?.Treasury);
        var created = new RepuTreasury(balance, countryId);
        _treasuries[countryId] = created;
        return created;
    }

    private static double ParseTreasuryBalance(string? treasuryStr)
    {
        if (string.IsNullOrWhiteSpace(treasuryStr))
        {
            return 15_000_000_000.0;
        }

        var sb = new StringBuilder();
        bool hasDot = false;
        foreach (var c in treasuryStr)
        {
            if (char.IsAsciiDigit(c))
            {
                sb.Append(c);
            }
            else if (c == '.' && !hasDot)
            {
                sb.Append(c);
                hasDot = true;
            }
        }

        if (double.TryParse(sb.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 15_000_000_000.0;
    }

    private static double ParseTaxRate(string? taxRateStr)
    {
        if (string.IsNullOrWhiteSpace(taxRateStr))
        {
            return 0.25;
        }

        var cleaned = taxRateStr.Replace("%", "").Trim();
        if (double.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed > 1.0 ? parsed / 100.0 : parsed;
        }

        return 0.25;
    }

    private static double ParseApproval(string? approvalStr)
    {
        if (string.IsNullOrWhiteSpace(approvalStr))
        {
            return 70.0;
        }

        var cleaned = approvalStr.Replace("%", "").Trim();
        if (double.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 70.0;
    }

    /// <summary>
    /// Submits and validates a presidential tax reform decision on the authoritative host.
    /// Enforces player ownership, deduplication, and constitutional limits.
    /// </summary>
    public bool SubmitTaxDecision(string playerId, string countryId, double proposedRate, string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);

        // 1. Ownership validation
        if (!IsOwner(playerId, countryId))
        {
            return false;
        }

        // 2. Idempotency / deduplication check
        if (!string.IsNullOrEmpty(requestId) && !_processedRequests.TryAdd(requestId, true))
        {
            return true;
        }

        // 3. Constitutional bounds: 0% to 70%
        if (proposedRate < 0.0 || proposedRate > 0.70)
        {
            return false;
        }

        // 4. Apply authoritative changes
        var country = World.Countries.Find(c => c.Id == countryId);
        if (country != null)
        {
            var oldRate = country.TaxPolicy.IncomeTaxRate;
            country.TaxPolicy.IncomeTaxRate = proposedRate;
            var diff = (proposedRate - oldRate) * 100.0;
            country.HappinessRating = Math.Clamp(country.HappinessRating - (0.5 * diff), 0.0, 100.0);

            var snap = new CountrySnapshot
            {
                Id = country.Id,
                Name = country.Name,
                Treasury = $"R {country.Treasury.LiquidBalance:N0}",
                Approval = $"{country.HappinessRating:0.#}%",
                TaxRate = $"{proposedRate * 100:0.#}%"
            };
            _knownCountries[countryId] = snap;
            OnCountryUpdated?.Invoke(snap);
            _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = snap });
        }
        else if (_knownCountries.TryGetValue(countryId, out var existing))
        {
            var oldRate = ParseTaxRate(existing.TaxRate);
            var oldApproval = ParseApproval(existing.Approval);
            var diff = (proposedRate - oldRate) * 100.0;
            var newApproval = Math.Clamp(oldApproval - (0.5 * diff), 0.0, 100.0);

            var snap = existing with
            {
                TaxRate = $"{proposedRate * 100:0.#}%",
                Approval = $"{newApproval:0.#}%"
            };
            _knownCountries[countryId] = snap;
            OnCountryUpdated?.Invoke(snap);
            _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = snap });
        }

        SaveWorld();
        return true;
    }

    /// <summary>
    /// Submits and validates a treasury transaction request on the authoritative host.
    /// </summary>
    public bool SubmitTreasuryTransaction(string playerId, string countryId, string action, double amount, string? targetCountryId = null, string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);

        if (amount <= 0)
        {
            return false;
        }

        if (!IsOwner(playerId, countryId))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(requestId) && !_processedRequests.TryAdd(requestId, true))
        {
            return true;
        }

        var treasury = GetOrCreateTreasury(countryId);
        if (string.Equals(action, "withdraw", StringComparison.OrdinalIgnoreCase))
        {
            if (!treasury.TryWithdraw(amount))
            {
                return false;
            }
        }
        else if (string.Equals(action, "deposit", StringComparison.OrdinalIgnoreCase))
        {
            treasury.Deposit(amount);
        }
        else if (string.Equals(action, "transfer", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(targetCountryId))
            {
                return false;
            }
            var targetTreasury = GetOrCreateTreasury(targetCountryId);
            if (!treasury.TryWithdraw(amount))
            {
                return false;
            }
            targetTreasury.Deposit(amount);

            if (_knownCountries.TryGetValue(targetCountryId, out var targetSnap))
            {
                var updatedTarget = targetSnap with { Treasury = $"R {targetTreasury.Balance:N0}" };
                _knownCountries[targetCountryId] = updatedTarget;
                OnCountryUpdated?.Invoke(updatedTarget);
                _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = updatedTarget });
            }
        }
        else
        {
            return false;
        }

        if (_knownCountries.TryGetValue(countryId, out var sourceSnap))
        {
            var updatedSource = sourceSnap with { Treasury = $"R {treasury.Balance:N0}" };
            _knownCountries[countryId] = updatedSource;
            OnCountryUpdated?.Invoke(updatedSource);
            _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = updatedSource });
        }

        SaveWorld();
        return true;
    }

    /// <summary>
    /// Submits and validates a trade proposal on the authoritative host.
    /// Validates proposing country ownership and solvency.
    /// </summary>
    public bool SubmitTradeProposal(string playerId, TradeProposal trade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentNullException.ThrowIfNull(trade);

        if (!IsOwner(playerId, trade.From))
        {
            return false;
        }

        if (string.Equals(trade.From, trade.To, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (trade.Amount <= 0)
        {
            return false;
        }

        var fromTreasury = GetOrCreateTreasury(trade.From);
        if (fromTreasury.Balance < trade.Amount)
        {
            return false;
        }

        if (_trades.ContainsKey(trade.Id))
        {
            return false;
        }

        _trades[trade.Id] = trade;
        World.TradeProposals.Add(trade);
        SaveWorld();
        OnTradeUpdated?.Invoke(trade);
        _ = BroadcastMessageAsync(new NetworkMessage { Type = "trade", SenderId = "host", Trade = trade });
        return true;
    }

    /// <summary>
    /// Validates and accepts a pending trade proposal on the authoritative host.
    /// Enforces that only the recipient country's owner can accept, and updates treasuries exactly once.
    /// </summary>
    public bool AcceptTrade(string playerId, string tradeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeId);

        if (!_trades.TryGetValue(tradeId, out var trade))
        {
            return false;
        }

        if (!IsOwner(playerId, trade.To))
        {
            return false;
        }

        lock (trade)
        {
            if (!string.Equals(trade.Status, TradeProposalStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // Already resolved! Cannot be applied twice.
                return false;
            }

            var fromTreasury = GetOrCreateTreasury(trade.From);
            var toTreasury = GetOrCreateTreasury(trade.To);

            if (trade.Accept(fromTreasury, toTreasury))
            {
                if (_knownCountries.TryGetValue(trade.From, out var fromSnap))
                {
                    var updatedFrom = fromSnap with { Treasury = $"R {fromTreasury.Balance:N0}" };
                    _knownCountries[trade.From] = updatedFrom;
                    OnCountryUpdated?.Invoke(updatedFrom);
                    _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = updatedFrom });
                }

                if (_knownCountries.TryGetValue(trade.To, out var toSnap))
                {
                    var updatedTo = toSnap with { Treasury = $"R {toTreasury.Balance:N0}" };
                    _knownCountries[trade.To] = updatedTo;
                    OnCountryUpdated?.Invoke(updatedTo);
                    _ = BroadcastMessageAsync(new NetworkMessage { Type = "country", SenderId = "host", Country = updatedTo });
                }

                SaveWorld();
                OnTradeUpdated?.Invoke(trade);
                _ = BroadcastMessageAsync(new NetworkMessage { Type = "trade", SenderId = "host", Trade = trade });
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Rejects or cancels a pending trade proposal on the authoritative host.
    /// </summary>
    public bool RejectTrade(string playerId, string tradeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeId);

        if (!_trades.TryGetValue(tradeId, out var trade))
        {
            return false;
        }

        if (!IsOwner(playerId, trade.To) && !IsOwner(playerId, trade.From))
        {
            return false;
        }

        lock (trade)
        {
            if (!string.Equals(trade.Status, TradeProposalStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            trade.Reject();
            SaveWorld();
            OnTradeUpdated?.Invoke(trade);
            _ = BroadcastMessageAsync(new NetworkMessage { Type = "trade", SenderId = "host", Trade = trade });
            return true;
        }
    }

    /// <summary>
    /// Broadcasts a diplomatic recognition update to all connected peers.
    /// </summary>
    public void BroadcastDiplomacy(DiplomaticSnapshot diplomacy)
    {
        ArgumentNullException.ThrowIfNull(diplomacy);
        var key = $"{diplomacy.FromCountryId}:{diplomacy.ToCountryId}";
        _diplomacy[key] = diplomacy;
        OnDiplomacyUpdated?.Invoke(diplomacy);
        _ = BroadcastMessageAsync(new NetworkMessage
        {
            Type = "diplomacy",
            SenderId = "host",
            Diplomacy = diplomacy
        });
    }

    /// <summary>
    /// Broadcasts a diplomatic message to all connected peers and records it on the world state.
    /// </summary>
    public void BroadcastDiplomaticMessage(DiplomaticMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        World.Messages.Add(message);
        OnMessageReceived?.Invoke(message);
        _ = BroadcastMessageAsync(new NetworkMessage
        {
            Type = "message",
            SenderId = "host",
            Message = message
        });
    }

    /// <summary>
    /// Broadcasts a trade proposal to all connected peers and records it on the world state.
    /// </summary>
    public void BroadcastTradeProposal(TradeProposal trade)
    {
        ArgumentNullException.ThrowIfNull(trade);
        _trades[trade.Id] = trade;
        World.TradeProposals.Add(trade);
        OnTradeUpdated?.Invoke(trade);
        _ = BroadcastMessageAsync(new NetworkMessage
        {
            Type = "trade",
            SenderId = "host",
            Trade = trade
        });
    }

    /// <summary>
    /// Updates the authoritative simulation clock and broadcasts the new time to all connected client peers.
    /// </summary>
    public void UpdateClock(RepublicTime time, ulong tick = 0, bool isPaused = false)
    {
        var snapshot = NetworkClockSnapshot.FromRepublicTime(time, tick, isPaused);
        UpdateClock(snapshot);
    }

    /// <summary>
    /// Updates the authoritative simulation clock from a snapshot and broadcasts it to all peers.
    /// </summary>
    public void UpdateClock(NetworkClockSnapshot clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _currentClock = clock;
        OnClockUpdated?.Invoke(clock);

        var message = new NetworkMessage
        {
            Type = "clock",
            SenderId = "host",
            Clock = clock
        };

        _ = BroadcastMessageAsync(message);
    }

    /// <summary>
    /// Persists the full shared world state (clock, countries, treasuries, ownership, diplomacy, trades)
    /// to a single file on the host.
    /// </summary>
    public void SaveWorld(string? filePath = null)
    {
        var path = filePath ?? SaveFilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        lock (_saveLock)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var data = new SharedWorldSaveData
                {
                    FormatVersion = 1,
                    SavedAt = DateTimeOffset.UtcNow,
                    Clock = _currentClock,
                    Countries = _knownCountries.Values.ToList(),
                    TreasuryBalances = _treasuries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Balance),
                    CountryOwners = _countryOwners.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    Diplomacy = _diplomacy.Values.ToList(),
                    Messages = World.Messages.ToList(),
                    Trades = _trades.Values.ToList(),
                    ProcessedRequestIds = _processedRequests.Keys.ToList()
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                });

                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, path, overwrite: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RepublicHost] SaveWorld failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Restores the full shared world state from a single file on the host.
    /// </summary>
    public bool LoadWorld(string? filePath = null)
    {
        var path = filePath ?? SaveFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return false;
        }

        lock (_saveLock)
        {
            try
            {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SharedWorldSaveData>(json, JsonOptions);
                if (data == null)
                {
                    return false;
                }

                if (data.Clock != null)
                {
                    _currentClock = data.Clock;
                }

                foreach (var c in data.Countries)
                {
                    _knownCountries[c.Id] = c;
                }

                foreach (var (cId, bal) in data.TreasuryBalances)
                {
                    _treasuries[cId] = new RepuTreasury(bal, cId);
                }

                foreach (var (cId, owner) in data.CountryOwners)
                {
                    _countryOwners[cId] = owner;
                    _playerCountries[owner] = cId;
                }

                foreach (var d in data.Diplomacy)
                {
                    var key = $"{d.FromCountryId}:{d.ToCountryId}";
                    _diplomacy[key] = d;
                }

                World.Messages.Clear();
                foreach (var m in data.Messages)
                {
                    World.Messages.Add(m);
                }

                _trades.Clear();
                World.TradeProposals.Clear();
                foreach (var tr in data.Trades)
                {
                    _trades[tr.Id] = tr;
                    World.TradeProposals.Add(tr);
                }

                foreach (var reqId in data.ProcessedRequestIds)
                {
                    _processedRequests[reqId] = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RepublicHost] LoadWorld failed: {ex.Message}");
                return false;
            }
        }
    }

    private async Task AcceptConnectionsLoopAsync()
    {
        while (!_cts.IsCancellationRequested && _listener != null)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false);
                var clientId = Guid.NewGuid().ToString("N");
                var client = new ConnectedClient(clientId, tcpClient);

                if (_clients.TryAdd(clientId, client))
                {
                    OnClientCountChanged?.Invoke(_clients.Count);

                    // Send initial synchronization messages to the newly joined peer
                    _ = Task.Run(async () =>
                    {
                        // 1. Send current authoritative clock
                        await client.SendMessageAsync(new NetworkMessage
                        {
                            Type = "clock",
                            SenderId = "host",
                            Clock = _currentClock
                        }).ConfigureAwait(false);

                        // 2. Send all currently known country snapshots
                        foreach (var country in _knownCountries.Values)
                        {
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "country",
                                SenderId = "host",
                                Country = country
                            }).ConfigureAwait(false);
                        }

                        // 3. Send all active bilateral diplomatic recognition snapshots
                        foreach (var dip in _diplomacy.Values)
                        {
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "diplomacy",
                                SenderId = "host",
                                Diplomacy = dip
                            }).ConfigureAwait(false);
                        }

                        // 4. Send all recent diplomatic messages
                        foreach (var msg in World.Messages)
                        {
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "message",
                                SenderId = "host",
                                Message = msg
                            }).ConfigureAwait(false);
                        }

                        // 5. Send all active trade proposals
                        foreach (var tr in _trades.Values)
                        {
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "trade",
                                SenderId = "host",
                                Trade = tr
                            }).ConfigureAwait(false);
                        }

                        // Begin listening for incoming client messages
                        await HandleClientReadLoopAsync(client).ConfigureAwait(false);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception) when (_disposed || !_isStarted)
            {
                break;
            }
            catch (Exception)
            {
                if (_cts.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task HandleClientReadLoopAsync(ConnectedClient client)
    {
        try
        {
            while (!_cts.IsCancellationRequested && client.IsConnected)
            {
                var line = await client.Reader.ReadLineAsync(_cts.Token).ConfigureAwait(false);
                if (line == null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    var message = JsonSerializer.Deserialize<NetworkMessage>(line, JsonOptions);
                    if (message == null)
                    {
                        continue;
                    }

                    if (message.Type == "claim_country")
                    {
                        var countryId = message.CountryId ?? message.Country?.Id ?? string.Empty;
                        var claimed = TryClaimCountry(message.SenderId, countryId);
                        await client.SendMessageAsync(new NetworkMessage
                        {
                            Type = "decision_result",
                            SenderId = "host",
                            RequestId = message.RequestId,
                            CountryId = countryId,
                            Success = claimed,
                            Result = claimed ? "Success" : "Rejected",
                            StatusMessage = claimed
                                ? $"Claimed ownership of {countryId}"
                                : $"Country {countryId} is already owned by another sovereign."
                        }).ConfigureAwait(false);
                    }
                    else if (message.Type == "country" && message.Country != null)
                    {
                        var countryId = message.Country.Id;
                        if (!_countryOwners.ContainsKey(countryId))
                        {
                            TryClaimCountry(message.SenderId, countryId);
                            _knownCountries[countryId] = message.Country;
                            GetOrCreateTreasury(countryId, message.Country);
                            OnCountryUpdated?.Invoke(message.Country);
                            SaveWorld();
                            await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                        }
                        else if (IsOwner(message.SenderId, countryId))
                        {
                            _knownCountries[countryId] = message.Country;
                            GetOrCreateTreasury(countryId, message.Country);
                            OnCountryUpdated?.Invoke(message.Country);
                            SaveWorld();
                            await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                        }
                        else
                        {
                            // Unauthorized country state modification attempt
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "decision_result",
                                SenderId = "host",
                                RequestId = message.RequestId,
                                CountryId = countryId,
                                Success = false,
                                Result = "Rejected",
                                StatusMessage = $"Rejected: You do not own country '{countryId}'."
                            }).ConfigureAwait(false);
                        }
                    }
                    else if (message.Type == "tax_decision" || (message.Type == "action" && message.Action == "tax"))
                    {
                        var countryId = message.CountryId ?? _playerCountries.GetValueOrDefault(message.SenderId) ?? string.Empty;
                        var rate = message.TaxRate ?? 0.0;
                        var success = SubmitTaxDecision(message.SenderId, countryId, rate, message.RequestId);
                        await client.SendMessageAsync(new NetworkMessage
                        {
                            Type = "decision_result",
                            SenderId = "host",
                            RequestId = message.RequestId,
                            CountryId = countryId,
                            Success = success,
                            Result = success ? "Success" : "Rejected",
                            StatusMessage = success
                                ? $"Successfully applied: Tax rate updated to {rate * 100:0.#}%"
                                : $"Rejected: Tax decision for '{countryId}' rejected (unauthorized or invalid)."
                        }).ConfigureAwait(false);
                    }
                    else if (message.Type == "trade" && message.Trade != null)
                    {
                        var trade = message.Trade;
                        if (string.Equals(trade.Status, TradeProposalStatus.Accepted.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var accepted = AcceptTrade(message.SenderId, trade.Id);
                            if (!accepted)
                            {
                                await client.SendMessageAsync(new NetworkMessage
                                {
                                    Type = "decision_result",
                                    SenderId = "host",
                                    RequestId = message.RequestId,
                                    Success = false,
                                    Result = "Rejected",
                                    StatusMessage = $"Rejected: Trade '{trade.Id}' acceptance rejected (unauthorized or not in Pending status)."
                                }).ConfigureAwait(false);
                            }
                        }
                        else if (string.Equals(trade.Status, TradeProposalStatus.Rejected.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var rejected = RejectTrade(message.SenderId, trade.Id);
                            if (!rejected)
                            {
                                await client.SendMessageAsync(new NetworkMessage
                                {
                                    Type = "decision_result",
                                    SenderId = "host",
                                    RequestId = message.RequestId,
                                    Success = false,
                                    Result = "Rejected",
                                    StatusMessage = $"Rejected: Trade '{trade.Id}' rejection rejected."
                                }).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            var proposed = SubmitTradeProposal(message.SenderId, trade);
                            if (!proposed)
                            {
                                await client.SendMessageAsync(new NetworkMessage
                                {
                                    Type = "decision_result",
                                    SenderId = "host",
                                    RequestId = message.RequestId,
                                    Success = false,
                                    Result = "Rejected",
                                    StatusMessage = $"Rejected: Trade proposal '{trade.Id}' rejected (unauthorized or insufficient balance)."
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (message.Type == "treasury" || message.Type == "treasury_transaction")
                    {
                        var countryId = message.CountryId ?? _playerCountries.GetValueOrDefault(message.SenderId) ?? string.Empty;
                        var action = message.Action ?? "withdraw";
                        var amount = message.Amount ?? 0.0;
                        var success = SubmitTreasuryTransaction(message.SenderId, countryId, action, amount, message.TargetCountryId, message.RequestId);
                        await client.SendMessageAsync(new NetworkMessage
                        {
                            Type = "decision_result",
                            SenderId = "host",
                            RequestId = message.RequestId,
                            CountryId = countryId,
                            Success = success,
                            Result = success ? "Success" : "Rejected",
                            StatusMessage = success ? "Treasury transaction applied." : "Treasury transaction rejected."
                        }).ConfigureAwait(false);
                    }
                    else if (message.Type == "diplomacy" && message.Diplomacy != null)
                    {
                        if (IsOwner(message.SenderId, message.Diplomacy.FromCountryId) || !_countryOwners.ContainsKey(message.Diplomacy.FromCountryId))
                        {
                            var key = $"{message.Diplomacy.FromCountryId}:{message.Diplomacy.ToCountryId}";
                            _diplomacy[key] = message.Diplomacy;
                            OnDiplomacyUpdated?.Invoke(message.Diplomacy);
                            SaveWorld();
                            await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                        }
                        else
                        {
                            await client.SendMessageAsync(new NetworkMessage
                            {
                                Type = "decision_result",
                                SenderId = "host",
                                RequestId = message.RequestId,
                                Success = false,
                                Result = "Rejected",
                                StatusMessage = "Rejected: You do not own the source diplomatic country."
                            }).ConfigureAwait(false);
                        }
                    }
                    else if (message.Type == "message" && message.Message != null)
                    {
                        World.Messages.Add(message.Message);
                        OnMessageReceived?.Invoke(message.Message);
                        SaveWorld();
                        await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                    }
                }
                catch (JsonException)
                {
                    // Ignore malformed line
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
        catch (IOException)
        {
            // Socket disconnected
        }
        finally
        {
            RemoveClient(client.Id);
        }
    }

    private async Task BroadcastMessageAsync(NetworkMessage message, string? excludeClientId = null)
    {
        foreach (var client in _clients.Values)
        {
            if (excludeClientId != null && client.Id == excludeClientId)
            {
                continue;
            }

            try
            {
                await client.SendMessageAsync(message).ConfigureAwait(false);
            }
            catch
            {
                RemoveClient(client.Id);
            }
        }
    }

    private void RemoveClient(string clientId)
    {
        if (_clients.TryRemove(clientId, out var client))
        {
            client.Dispose();
            OnClientCountChanged?.Invoke(_clients.Count);
        }
    }

    /// <summary>
    /// Stops the host, persists the shared world state, and disconnects all active peers cleanly.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        SaveWorld();

        _disposed = true;
        _isStarted = false;

        _cts.Cancel();
        _listener?.Stop();

        foreach (var client in _clients.Values)
        {
            client.Dispose();
        }
        _clients.Clear();

        _cts.Dispose();
    }

    private sealed class ConnectedClient : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public string Id { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }
        public bool IsConnected => _tcpClient.Connected;

        public ConnectedClient(string id, TcpClient tcpClient)
        {
            Id = id;
            _tcpClient = tcpClient;
            _stream = tcpClient.GetStream();
            Reader = new StreamReader(_stream, Encoding.UTF8);
            Writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true };
        }

        public async Task SendMessageAsync(NetworkMessage message)
        {
            var json = JsonSerializer.Serialize(message, JsonOptions);
            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await Writer.WriteLineAsync(json).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Dispose()
        {
            try { Reader.Dispose(); } catch { }
            try { Writer.Dispose(); } catch { }
            try { _stream.Dispose(); } catch { }
            try { _tcpClient.Dispose(); } catch { }
            _sendLock.Dispose();
        }
    }
}
