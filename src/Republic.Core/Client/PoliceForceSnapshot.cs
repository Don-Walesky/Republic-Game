namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable client data contract capturing national police force capacity, order, and crime rate index.
/// </summary>
public sealed record PoliceForceSnapshot
{
    [JsonPropertyName("is_established")]
    public bool IsEstablished { get; init; }

    [JsonPropertyName("force_name")]
    public string ForceName { get; init; } = string.Empty;

    [JsonPropertyName("officer_count")]
    public int OfficerCount { get; init; }

    [JsonPropertyName("quarterly_funding")]
    public double QuarterlyFunding { get; init; }

    [JsonPropertyName("crime_rate_index")]
    public double CrimeRateIndex { get; init; }

    [JsonPropertyName("public_order_score")]
    public double PublicOrderScore { get; init; }

    [JsonPropertyName("community_trust_score")]
    public double CommunityTrustScore { get; init; }

    [JsonPropertyName("status_summary")]
    public string StatusSummary { get; init; } = string.Empty;
}

