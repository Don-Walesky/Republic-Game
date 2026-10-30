namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a legislative bill and its 50:50 consequences.
/// </summary>
public sealed record CountryBillSnapshot
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Pending";

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = "Economy";

    [JsonPropertyName("positive_consequence")]
    public string PositiveConsequence { get; init; } = string.Empty;

    [JsonPropertyName("negative_consequence")]
    public string NegativeConsequence { get; init; } = string.Empty;

    [JsonPropertyName("governance_principle")]
    public string GovernancePrinciple { get; init; } = string.Empty;

    [JsonPropertyName("required_majority_percentage")]
    public double RequiredMajorityPercentage { get; init; } = 50.0;

    [JsonPropertyName("veto_threshold_percentage")]
    public double VetoThresholdPercentage { get; init; } = 66.7;

    [JsonPropertyName("total_votes")]
    public int TotalVotes { get; init; } = 100;

    [JsonPropertyName("ayes_count")]
    public int AyesCount { get; init; }

    [JsonPropertyName("nays_count")]
    public int NaysCount { get; init; }

    [JsonPropertyName("is_passed")]
    public bool IsPassed { get; init; }

    [JsonPropertyName("is_vetoed")]
    public bool IsVetoed { get; init; }

    [JsonPropertyName("veto_overridden")]
    public bool VetoOverridden { get; init; }

    [JsonPropertyName("voted_boundary")]
    public string? VotedBoundary { get; init; }
}

