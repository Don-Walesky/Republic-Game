namespace Republic.Core.Networking;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal TCP client connecting to a Republic host to synchronize the world clock and receive peer country states.
/// </summary>
public sealed class RepublicNetworkClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConcurrentDictionary<string, CountrySnapshot> _knownCountries = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private Task? _readTask;
    private bool _disposed;

    /// <summary>
    /// Event invoked whenever a synchronized world clock snapshot is received from the host.
    /// </summary>
    public event Action<NetworkClockSnapshot>? OnClockSynchronized;

    /// <summary>
    /// Event invoked whenever a country snapshot is received from the host or another peer.
    /// </summary>
    public event Action<CountrySnapshot>? OnCountryReceived;

    /// <summary>
    /// Event invoked whenever a diplomatic recognition update is received from the host.
    /// </summary>
    public event Action<DiplomaticSnapshot>? OnDiplomacyReceived;

    /// <summary>
    /// Event invoked whenever a diplomatic message is received.
    /// </summary>
    public event Action<Republic.Core.World.DiplomaticMessage>? OnMessageReceived;

    /// <summary>
    /// Event invoked whenever a trade proposal is received or updated.
    /// </summary>
    public event Action<Republic.Core.World.TradeProposal>? OnTradeReceived;

    /// <summary>
    /// Event invoked whenever a decision result or rejection message is received from the host.
    /// </summary>
    public event Action<NetworkMessage>? OnDecisionResultReceived;

    /// <summary>
    /// Event invoked when the client is disconnected from the host.
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Gets the unique identifier for this client session.
    /// </summary>
    public string ClientId { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets a value indicating whether the client is currently connected to the host.
    /// </summary>
    public bool IsConnected => _tcpClient != null && _tcpClient.Connected && !_disposed;

    /// <summary>
    /// Gets the most recent synchronized simulation clock snapshot received from the host.
    /// </summary>
    public NetworkClockSnapshot? CurrentClock { get; private set; }

    /// <summary>
    /// Gets all known sovereign country snapshots received across the multiplayer session.
    /// </summary>
    public IReadOnlyDictionary<string, CountrySnapshot> KnownCountries => _knownCountries;

    /// <summary>
    /// Connects to the Republic host at the specified host and port.
    /// </summary>
    public async Task ConnectAsync(string host = "127.0.0.1", int port = 7777, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port, linkedCts.Token).ConfigureAwait(false);

        _stream = _tcpClient.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true };

        _readTask = Task.Run(ReadIncomingMessagesLoopAsync);
    }

    /// <summary>
    /// Sends a sovereign country snapshot to the host to be synchronized across all peers.
    /// </summary>
    public async Task SendCountrySnapshotAsync(CountrySnapshot country)
    {
        ArgumentNullException.ThrowIfNull(country);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "country",
            SenderId = ClientId,
            Country = country
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a diplomatic recognition update to the host.
    /// </summary>
    public async Task SendDiplomacyAsync(DiplomaticSnapshot diplomacy)
    {
        ArgumentNullException.ThrowIfNull(diplomacy);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "diplomacy",
            SenderId = ClientId,
            Diplomacy = diplomacy
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a diplomatic message to the host to be recorded and broadcast.
    /// </summary>
    public async Task SendDiplomaticMessageAsync(Republic.Core.World.DiplomaticMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var netMsg = new NetworkMessage
        {
            Type = "message",
            SenderId = ClientId,
            Message = message
        };

        await SendMessageAsync(netMsg).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a trade proposal, acceptance, or rejection to the host.
    /// </summary>
    public async Task SendTradeProposalAsync(Republic.Core.World.TradeProposal trade)
    {
        ArgumentNullException.ThrowIfNull(trade);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "trade",
            SenderId = ClientId,
            Trade = trade
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Claims ownership of the specified sovereign country on the authoritative host.
    /// </summary>
    public async Task ClaimCountryAsync(string countryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "claim_country",
            SenderId = ClientId,
            CountryId = countryId
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a tax decision request to the authoritative host for validation and application.
    /// </summary>
    public async Task SendTaxDecisionRequestAsync(string countryId, double proposedRate, string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "tax_decision",
            SenderId = ClientId,
            CountryId = countryId,
            TaxRate = proposedRate,
            RequestId = requestId ?? Guid.NewGuid().ToString("N")
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a treasury transaction request to the authoritative host for validation and application.
    /// </summary>
    public async Task SendTreasuryTransactionAsync(string countryId, string action, double amount, string? targetCountryId = null, string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "treasury_transaction",
            SenderId = ClientId,
            CountryId = countryId,
            Action = action,
            Amount = amount,
            TargetCountryId = targetCountryId,
            RequestId = requestId ?? Guid.NewGuid().ToString("N")
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a national project decision request (e.g. Road program start or cancel) to the authoritative host for validation.
    /// </summary>
    public async Task SendProjectDecisionRequestAsync(string countryId, string projectName, bool start, string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var message = new NetworkMessage
        {
            Type = "project_decision",
            SenderId = ClientId,
            CountryId = countryId,
            ProjectName = projectName,
            Action = start ? "start_project" : "cancel_project",
            RequestId = requestId ?? Guid.NewGuid().ToString("N")
        };

        await SendMessageAsync(message).ConfigureAwait(false);
    }

    private async Task SendMessageAsync(NetworkMessage message)
    {
        if (_writer == null || !IsConnected)
        {
            return;
        }

        var json = JsonSerializer.Serialize(message, JsonOptions);
        await _sendLock.WaitAsync(_cts.Token).ConfigureAwait(false);
        try
        {
            await _writer.WriteLineAsync(json).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReadIncomingMessagesLoopAsync()
    {
        try
        {
            while (!_cts.IsCancellationRequested && IsConnected && _reader != null)
            {
                var line = await _reader.ReadLineAsync(_cts.Token).ConfigureAwait(false);
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

                    if (message.Type == "clock" && message.Clock != null)
                    {
                        CurrentClock = message.Clock;
                        OnClockSynchronized?.Invoke(message.Clock);
                    }
                    else if (message.Type == "country" && message.Country != null)
                    {
                        _knownCountries[message.Country.Id] = message.Country;
                        OnCountryReceived?.Invoke(message.Country);
                    }
                    else if (message.Type == "diplomacy" && message.Diplomacy != null)
                    {
                        OnDiplomacyReceived?.Invoke(message.Diplomacy);
                    }
                    else if (message.Type == "message" && message.Message != null)
                    {
                        OnMessageReceived?.Invoke(message.Message);
                    }
                    else if (message.Type == "trade" && message.Trade != null)
                    {
                        OnTradeReceived?.Invoke(message.Trade);
                    }
                    else if (message.Type == "decision_result")
                    {
                        OnDecisionResultReceived?.Invoke(message);
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
            // Disconnected
        }
        finally
        {
            OnDisconnected?.Invoke();
        }
    }

    /// <summary>
    /// Closes the network connection and disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts.Cancel();

        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _tcpClient?.Dispose(); } catch { }

        _sendLock.Dispose();
        _cts.Dispose();
    }
}
