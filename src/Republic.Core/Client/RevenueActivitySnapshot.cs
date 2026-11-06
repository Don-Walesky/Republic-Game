namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Domain snapshot encapsulating a strategic sovereign revenue-driving program,
/// its capital setup requirement, recurring quarterly revenue yield, and enactment status.
/// </summary>
public sealed record RevenueActivitySnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("setup_cost")]
    public double SetupCost { get; init; }

    [JsonPropertyName("setup_cost_formatted")]
    public string SetupCostFormatted { get; init; } = string.Empty;

    [JsonPropertyName("quarterly_revenue_boost")]
    public double QuarterlyRevenueBoost { get; init; }

    [JsonPropertyName("quarterly_revenue_boost_formatted")]
    public string QuarterlyRevenueBoostFormatted { get; init; } = string.Empty;

    [JsonPropertyName("is_enacted")]
    public bool IsEnacted { get; init; }
}
