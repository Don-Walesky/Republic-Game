namespace Republic.Core.Networking.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Diagnostics;
using Republic.Core.Networking.Models;

/// <summary>
/// Server host manager managing co-op lobby connections, portfolio assignments, and action packet broadcasting.
/// </summary>
public sealed class CabinetMultiplayerServer
{
    private readonly Dictionary<string, string> _connectedClients = new(); // ClientId -> Role
    private readonly List<NetworkMessage> _messageLog = new();
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public CabinetMultiplayerServer(ILogger? logger = null)
    {
        _logger = logger;
    }

    public bool ConnectPlayer(string clientId, string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        lock (_lock)
        {
            if (_connectedClients.ContainsKey(clientId)) return false;
            _connectedClients[clientId] = role;
        }

        _logger?.LogInfo($"[Multiplayer Server] Player connected: '{clientId}' assigned role '{role}'.");
        BroadcastMessage(new NetworkMessage
        {
            Type = NetworkMessageType.JoinLobby,
            SenderClientId = clientId,
            SenderRole = role,
            Payload = $"Player joined as {role}"
        });

        return true;
    }

    public void BroadcastMessage(NetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        lock (_lock)
        {
            _messageLog.Add(message);
        }
        _logger?.LogInfo($"[Multiplayer Server] Broadcast [{message.Type}] from {message.SenderRole}: '{message.Payload}'");
    }

    public IReadOnlyDictionary<string, string> GetConnectedPlayers()
    {
        lock (_lock)
        {
            return new Dictionary<string, string>(_connectedClients);
        }
    }

    public IReadOnlyList<NetworkMessage> GetMessageLog()
    {
        lock (_lock)
        {
            return _messageLog.ToList().AsReadOnly();
        }
    }
}
