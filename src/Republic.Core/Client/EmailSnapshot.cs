namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing an executive in-game message,
/// legislative floor dispatch, or foreign diplomatic cable.
/// </summary>
public sealed record EmailSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; init; } = string.Empty;

    [JsonPropertyName("sender_title")]
    public string SenderTitle { get; init; } = string.Empty;

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

    [JsonPropertyName("channel")]
    public string Channel { get; init; } = "Executive"; // Legislative, Diplomatic, Executive, Cabinet, Security, Media

    [JsonPropertyName("urgency")]
    public string Urgency { get; init; } = "Routine"; // Routine, Priority, Confidential, Urgent

    [JsonPropertyName("action_required")]
    public bool ActionRequired { get; init; }

    [JsonPropertyName("action_type")]
    public string ActionType { get; init; } = string.Empty;

    [JsonPropertyName("related_entity_id")]
    public string RelatedEntityId { get; init; } = string.Empty;

    [JsonPropertyName("avatar_symbol")]
    public string AvatarSymbol { get; init; } = "✉️";
}
