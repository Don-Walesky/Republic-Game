namespace Republic.Core.World;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Message exchanged between sovereign players in multiplayer diplomacy or foreign affairs.
/// Represents official diplomatic cables, bilateral communiqués, and treaty correspondence.
/// </summary>
public record DiplomaticMessage
{
    /// <summary>
    /// Gets the unique message identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the sovereign sender ID or country name.
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign recipient ID or country name.
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// Gets the diplomatic cable subject line.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Gets the message text content.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Gets the message classification type (e.g. BilateralOffer, CounterOffer, TreatyRatification, Communique).
    /// </summary>
    [JsonPropertyName("message_type")]
    public string MessageType { get; init; } = "Communique";

    /// <summary>
    /// Gets the associated trade proposal or agreement ID, if any.
    /// </summary>
    [JsonPropertyName("offered_deal_id")]
    public string OfferedDealId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this cable has been reviewed by the head of state.
    /// </summary>
    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets the diplomatic urgency level (e.g. Routine, Priority, Urgent, Crisis).
    /// </summary>
    [JsonPropertyName("urgency")]
    public string Urgency { get; init; } = "Routine";

    /// <summary>
    /// Gets the formatted timestamp when the message was dispatched.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiplomaticMessage"/> class.
    /// </summary>
    public DiplomaticMessage() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiplomaticMessage"/> class with explicit properties.
    /// </summary>
    public DiplomaticMessage(string from, string to, string text, string? timestamp = null, string? subject = null, string? messageType = null, string? dealId = null)
    {
        From = from ?? string.Empty;
        To = to ?? string.Empty;
        Text = text ?? string.Empty;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        Subject = subject ?? (!string.IsNullOrWhiteSpace(text) && text.Length > 30 ? text[..30] + "..." : "Diplomatic Communiqué");
        MessageType = messageType ?? "Communique";
        OfferedDealId = dealId ?? string.Empty;
    }
}

/// <summary>
/// Minimal Message type alias for diplomatic message exchange.
/// </summary>
public sealed record Message : DiplomaticMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </summary>
    public Message() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class with explicit properties.
    /// </summary>
    public Message(string from, string to, string text, string? timestamp = null)
        : base(from, to, text, timestamp)
    {
    }
}
