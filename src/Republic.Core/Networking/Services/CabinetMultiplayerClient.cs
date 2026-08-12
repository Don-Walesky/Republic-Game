namespace Republic.Core.Networking.Services;

using System;
using System.Collections.Generic;

using Republic.Core.Diagnostics;
using Republic.Core.Networking.Models;

/// <summary>
/// Client session handling connection state, directive proposals, and voting synchronization in cabinet co-op mode.
/// </summary>
public sealed class CabinetMultiplayerClient
{
    private readonly CabinetMultiplayerServer _server;
    private readonly List<NetworkMessage> _receivedMessages = new();
    private readonly ILogger? _logger;

    public string ClientId { get; }
    public string Role { get; set; }
    public bool IsConnected { get; private set; }

    public CabinetMultiplayerClient(string clientId, string role, CabinetMultiplayerServer server, ILogger? logger = null)
    {
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        Role = role ?? throw new ArgumentNullException(nameof(role));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger;
    }

    public bool Connect()
    {
        IsConnected = _server.ConnectPlayer(ClientId, Role);
        return IsConnected;
    }

    public void ProposeDirective(string directiveTitle, string details)
    {
        if (!IsConnected) throw new InvalidOperationException("Client must be connected to propose directives.");

        var message = new NetworkMessage
        {
            Type = NetworkMessageType.ProposeDirective,
            SenderClientId = ClientId,
            SenderRole = Role,
            Payload = $"{directiveTitle} | {details}"
        };

        _server.BroadcastMessage(message);
    }

    public void VoteOnDirective(string directiveId, bool voteAye)
    {
        if (!IsConnected) throw new InvalidOperationException("Client must be connected to vote.");

        var message = new NetworkMessage
        {
            Type = NetworkMessageType.VoteOnDirective,
            SenderClientId = ClientId,
            SenderRole = Role,
            Payload = $"Directive:{directiveId} | Vote:{(voteAye ? "AYE" : "NAY")}"
        };

        _server.BroadcastMessage(message);
    }

    public void ReceiveMessage(NetworkMessage message)
    {
        if (message != null)
        {
            _receivedMessages.Add(message);
        }
    }

    public IReadOnlyList<NetworkMessage> GetReceivedMessages() => _receivedMessages.AsReadOnly();
}
