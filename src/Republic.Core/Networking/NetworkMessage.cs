namespace Republic.Core.Networking;

using System.Text.Json.Serialization;

/// <summary>
/// Unified message envelope exchanged over the TCP networking layer between host and peers.
/// Messages are formatted as newline-delimited JSON strings.
/// </summary>
public sealed record NetworkMessage
{
    /// <summary>
    /// Gets the message type discriminator (e.g. "clock", "country", "welcome", "ping").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the sending peer or host.
    /// </summary>
    [JsonPropertyName("sender_id")]
    public string SenderId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the synchronized world clock snapshot if <see cref="Type"/> is "clock".
    /// </summary>
    [JsonPropertyName("clock")]
    public NetworkClockSnapshot? Clock { get; init; }

    /// <summary>
    /// Gets the country snapshot payload if <see cref="Type"/> is "country".
    /// </summary>
    [JsonPropertyName("country")]
    public CountrySnapshot? Country { get; init; }

    /// <summary>
    /// Gets the diplomatic recognition snapshot payload if <see cref="Type"/> is "diplomacy".
    /// </summary>
    [JsonPropertyName("diplomacy")]
    public DiplomaticSnapshot? Diplomacy { get; init; }

    /// <summary>
    /// Gets the diplomatic message payload if <see cref="Type"/> is "message".
    /// </summary>
    [JsonPropertyName("message")]
    public Republic.Core.World.DiplomaticMessage? Message { get; init; }

    /// <summary>
    /// Gets the trade proposal payload if <see cref="Type"/> is "trade".
    /// </summary>
    [JsonPropertyName("trade")]
    public Republic.Core.World.TradeProposal? Trade { get; init; }
}
