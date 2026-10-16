namespace Republic.Core.Networking;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Time;

/// <summary>
/// Minimal TCP host facilitating shared simulation clock sync and country state broadcast
/// between peer clients in Republic multiplayer sessions.
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
    private readonly ConcurrentDictionary<string, Republic.Core.World.TradeProposal> _trades = new();
    private readonly ConcurrentDictionary<string, Republic.Core.Economy.Treasury.RepuTreasury> _treasuries = new();
    private readonly CancellationTokenSource _cts = new();

    private TcpListener? _listener;
    private Task? _acceptTask;
    private NetworkClockSnapshot _currentClock;
    private int _boundPort;
    private bool _isStarted;
    private bool _disposed;

    /// <summary>
    /// Gets the authoritative world state tracking entities, messages, and trade proposals.
    /// </summary>
    public Republic.Core.World.WorldState World { get; } = new();

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
    public event Action<Republic.Core.World.DiplomaticMessage>? OnMessageReceived;

    /// <summary>
    /// Event invoked whenever a trade proposal is proposed, accepted, or rejected.
    /// </summary>
    public event Action<Republic.Core.World.TradeProposal>? OnTradeUpdated;

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
    public IReadOnlyDictionary<string, Republic.Core.World.TradeProposal> KnownTrades => _trades;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicHost"/> class.
    /// </summary>
    /// <param name="port">The TCP port to bind (default 7777, or 0 for dynamic ephemeral port allocation).</param>
    public RepublicHost(int port = 7777)
    {
        _requestedPort = port;
        _currentClock = new NetworkClockSnapshot
        {
            Tick = 0,
            Day = 0,
            TimeString = "Day 0, 00:00 WAT",
            IsPaused = false
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicHost"/> class anchored to an existing simulation time system.
    /// </summary>
    /// <param name="timeSystem">Authoritative time system provider.</param>
    /// <param name="port">The TCP port to bind (default 7777).</param>
    public RepublicHost(ITimeSystem timeSystem, int port = 7777)
        : this(port)
    {
        ArgumentNullException.ThrowIfNull(timeSystem);
        _currentClock = NetworkClockSnapshot.FromRepublicTime(
            timeSystem.CurrentRepublicTime,
            timeSystem.CurrentTick,
            timeSystem.IsPaused);
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
    /// Sets or updates the host country's state snapshot and broadcasts it to all connected peers.
    /// </summary>
    public void SetHostCountry(CountrySnapshot country)
    {
        ArgumentNullException.ThrowIfNull(country);
        BroadcastCountry(country);
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
    public void RegisterTreasury(string countryId, Republic.Core.Economy.Treasury.RepuTreasury treasury)
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
        World.Countries.Add(country);
    }

    /// <summary>
    /// Retrieves or initializes a sovereign treasury for the specified country identifier.
    /// </summary>
    public Republic.Core.Economy.Treasury.RepuTreasury GetOrCreateTreasury(string countryId, CountrySnapshot? snapshot = null)
    {
        if (_treasuries.TryGetValue(countryId, out var existing))
        {
            return existing;
        }

        var balance = ParseTreasuryBalance(snapshot?.Treasury);
        var created = new Republic.Core.Economy.Treasury.RepuTreasury(balance, countryId);
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
    public void BroadcastDiplomaticMessage(Republic.Core.World.DiplomaticMessage message)
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
    public void BroadcastTradeProposal(Republic.Core.World.TradeProposal trade)
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

                    if (message.Type == "country" && message.Country != null)
                    {
                        _knownCountries[message.Country.Id] = message.Country;
                        GetOrCreateTreasury(message.Country.Id, message.Country);
                        OnCountryUpdated?.Invoke(message.Country);

                        // Broadcast foreign country state update to all other connected peers
                        await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                    }
                    else if (message.Type == "diplomacy" && message.Diplomacy != null)
                    {
                        var key = $"{message.Diplomacy.FromCountryId}:{message.Diplomacy.ToCountryId}";
                        _diplomacy[key] = message.Diplomacy;
                        OnDiplomacyUpdated?.Invoke(message.Diplomacy);

                        await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                    }
                    else if (message.Type == "message" && message.Message != null)
                    {
                        World.Messages.Add(message.Message);
                        OnMessageReceived?.Invoke(message.Message);

                        await BroadcastMessageAsync(message, excludeClientId: client.Id).ConfigureAwait(false);
                    }
                    else if (message.Type == "trade" && message.Trade != null)
                    {
                        var trade = message.Trade;
                        _trades[trade.Id] = trade;

                        if (string.Equals(trade.Status, Republic.Core.World.TradeProposalStatus.Accepted.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
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
                            }
                        }
                        else if (string.Equals(trade.Status, Republic.Core.World.TradeProposalStatus.Rejected.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            trade.Reject();
                        }

                        World.TradeProposals.Add(trade);
                        OnTradeUpdated?.Invoke(trade);

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
    /// Stops the host and disconnects all active peers cleanly.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

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
