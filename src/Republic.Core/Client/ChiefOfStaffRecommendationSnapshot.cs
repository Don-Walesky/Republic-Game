namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a proactive Chief of Staff recommendation.
/// </summary>
public sealed record ChiefOfStaffRecommendationSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; init; } = "Medium";

    [JsonPropertyName("is_urgent")]
    public bool IsUrgent { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Pending";

    [JsonPropertyName("recommended_action")]
    public string RecommendedAction { get; init; } = string.Empty;

    [JsonPropertyName("estimated_cost")]
    public double EstimatedCost { get; init; }

    [JsonPropertyName("outcome_message")]
    public string OutcomeMessage { get; init; } = string.Empty;
}
