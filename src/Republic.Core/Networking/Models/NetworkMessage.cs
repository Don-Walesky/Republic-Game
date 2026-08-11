namespace Republic.Core.Networking.Models;

using System;

/// <summary>
/// Network packet type for multiplayer cabinet co-op session communication.
/// </summary>
public enum NetworkMessageType
{
    JoinLobby,
    AssignPortfolio,
    ProposeDirective,
    VoteOnDirective,
    SyncGameState,
    PlayerDisconnected
}

/// <summary>
/// Domain model representing a synchronized network message across cabinet players.
/// </summary>
public sealed class NetworkMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public NetworkMessageType Type { get; set; } = NetworkMessageType.JoinLobby;
    public string SenderClientId { get; set; } = string.Empty;
    public string SenderRole { get; set; } = "President";
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
