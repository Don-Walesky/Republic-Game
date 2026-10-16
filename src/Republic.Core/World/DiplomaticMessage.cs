namespace Republic.Core.World;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Minimal message exchanged between sovereign players in multiplayer diplomacy.
/// </summary>
public record DiplomaticMessage
{
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
    /// Gets the message text content.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

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
    public DiplomaticMessage(string from, string to, string text, string? timestamp = null)
    {
        From = from ?? string.Empty;
        To = to ?? string.Empty;
        Text = text ?? string.Empty;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
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
