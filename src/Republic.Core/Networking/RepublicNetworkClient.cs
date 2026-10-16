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
