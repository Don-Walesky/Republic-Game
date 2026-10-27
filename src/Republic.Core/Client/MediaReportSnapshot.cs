namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a published or leaked media report.
/// </summary>
public sealed record MediaReportSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("outlet_id")]
    public string OutletId { get; init; } = string.Empty;

    [JsonPropertyName("outlet_name")]
    public string OutletName { get; init; } = string.Empty;

    [JsonPropertyName("leaning")]
    public string Leaning { get; init; } = "StateAligned";

    [JsonPropertyName("headline")]
    public string Headline { get; init; } = string.Empty;

    [JsonPropertyName("article_summary")]
    public string ArticleSummary { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = "General";

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; init; } = "Neutral";

    [JsonPropertyName("event_source")]
    public string EventSource { get; init; } = string.Empty;

    [JsonPropertyName("outlet_credibility")]
    public double OutletCredibility { get; init; }

    [JsonPropertyName("outlet_reach")]
    public double OutletReach { get; init; }

    [JsonPropertyName("published_day")]
    public long PublishedDay { get; init; }

    [JsonPropertyName("public_approval_impact")]
    public double PublicApprovalImpact { get; init; }

    [JsonPropertyName("stability_impact")]
    public double StabilityImpact { get; init; }

    [JsonPropertyName("is_suppressed")]
    public bool IsSuppressed { get; init; }

    [JsonPropertyName("is_underground_leak")]
    public bool IsUndergroundLeak { get; init; }

    [JsonPropertyName("response_status")]
    public string ResponseStatus { get; init; } = "Pending";
}
