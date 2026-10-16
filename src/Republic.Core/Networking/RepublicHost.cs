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
    private readonly CancellationTokenSource _cts = new();

    private TcpListener? _listener;
    private Task? _acceptTask;
    private NetworkClockSnapshot _currentClock;
    private int _boundPort;
    private bool _isStarted;
    private bool _disposed;

    /// <summary>
    /// Event invoked whenever a connected peer or host updates their country state snapshot.
    /// </summary>
    public event Action<CountrySnapshot>? OnCountryUpdated;

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
                        OnCountryUpdated?.Invoke(message.Country);

                        // Broadcast foreign country state update to all other connected peers
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
