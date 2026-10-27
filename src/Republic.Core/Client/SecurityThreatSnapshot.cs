namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a national security threat report.
/// </summary>
public sealed record SecurityThreatSnapshot
{
    [JsonPropertyName("threat_id")]
    public string ThreatId { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = "DomesticSecurity";

    [JsonPropertyName("severity")]
    public string Severity { get; init; } = "Moderate";

    [JsonPropertyName("confidence")]
    public string Confidence { get; init; } = "HighProbability";

    [JsonPropertyName("source_agency")]
    public string SourceAgency { get; init; } = "National Intelligence Service";

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Active";

    [JsonPropertyName("details")]
    public string Details { get; init; } = string.Empty;

    [JsonPropertyName("recommended_response")]
    public string RecommendedResponse { get; init; } = string.Empty;

    [JsonPropertyName("is_confirmed")]
    public bool IsConfirmed { get; init; }
}
