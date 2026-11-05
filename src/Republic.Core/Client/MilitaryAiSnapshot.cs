namespace Republic.Core.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Serializable client data contract capturing sovereign Military AI threat modeling and detection bonus.
/// </summary>
public sealed record MilitaryAiSnapshot
{
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    [JsonPropertyName("ai_system_name")]
    public string AiSystemName { get; init; } = string.Empty;

    [JsonPropertyName("analysis_level")]
    public int AnalysisLevel { get; init; }

    [JsonPropertyName("computing_funding_level")]
    public double ComputingFundingLevel { get; init; }

    [JsonPropertyName("threat_detection_bonus")]
    public double ThreatDetectionBonus { get; init; }

    [JsonPropertyName("predictive_threat_summaries")]
    public IReadOnlyList<string> PredictiveThreatSummaries { get; init; } = Array.Empty<string>();

    [JsonPropertyName("status_summary")]
    public string StatusSummary { get; init; } = string.Empty;
}

