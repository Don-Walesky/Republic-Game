namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing portfolio-specific advice from a cabinet minister.
/// </summary>
public sealed record MinisterialAdviceSnapshot
{
    [JsonPropertyName("portfolio")]
    public string Portfolio { get; init; } = string.Empty;

    [JsonPropertyName("minister_name")]
    public string MinisterName { get; init; } = "Vacant";

    [JsonPropertyName("is_appointed")]
    public bool IsAppointed { get; init; }

    [JsonPropertyName("headline")]
    public string Headline { get; init; } = string.Empty;

    [JsonPropertyName("detailed_advice")]
    public string DetailedAdvice { get; init; } = string.Empty;

    [JsonPropertyName("risk_level")]
    public string RiskLevel { get; init; } = "Low";

    [JsonPropertyName("confidence_rating")]
    public double ConfidenceRating { get; init; } = 80.0;

    [JsonPropertyName("competence_rating")]
    public double CompetenceRating { get; init; }

    [JsonPropertyName("loyalty_rating")]
    public double LoyaltyRating { get; init; }

    [JsonPropertyName("integrity_rating")]
    public double IntegrityRating { get; init; }

    [JsonPropertyName("recommended_action")]
    public string RecommendedAction { get; init; } = string.Empty;

    [JsonPropertyName("action_cost")]
    public double ActionCost { get; init; }

    [JsonPropertyName("is_self_serving")]
    public bool IsSelfServing { get; init; }

    [JsonPropertyName("warning_notes")]
    public string WarningNotes { get; init; } = string.Empty;
}
