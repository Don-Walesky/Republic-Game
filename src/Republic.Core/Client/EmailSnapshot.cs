namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing an executive in-game email.
/// </summary>
public sealed record EmailSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; init; } = string.Empty;

    [JsonPropertyName("recipient")]
    public string Recipient { get; init; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("date_sent")]
    public string DateSent { get; init; } = string.Empty;

    [JsonPropertyName("is_read")]
    public bool IsRead { get; init; }

    [JsonPropertyName("folder")]
    public string Folder { get; init; } = "Inbox";

    [JsonPropertyName("action_required")]
    public bool ActionRequired { get; init; }
}

