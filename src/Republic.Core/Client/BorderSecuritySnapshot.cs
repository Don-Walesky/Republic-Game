namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable client data contract capturing national border security and frontier interception metrics.
/// </summary>
public sealed record BorderSecuritySnapshot
{
    [JsonPropertyName("funding_level")]
    public double FundingLevel { get; init; }

    [JsonPropertyName("patrol_strength")]
    public int PatrolStrength { get; init; }

    [JsonPropertyName("surveillance_tech_level")]
    public double SurveillanceTechLevel { get; init; }

    [JsonPropertyName("contraband_interception_rate")]
    public double ContrabandInterceptionRate { get; init; }

    [JsonPropertyName("infiltration_detection_chance")]
    public double InfiltrationDetectionChance { get; init; }

    [JsonPropertyName("effectiveness_score")]
    public double EffectivenessScore { get; init; }

    [JsonPropertyName("status_summary")]
    public string StatusSummary { get; init; } = string.Empty;
}

