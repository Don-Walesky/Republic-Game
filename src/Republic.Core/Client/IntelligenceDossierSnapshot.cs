namespace Republic.Core.Client;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a classified foreign intelligence dossier owned by the sovereign nation.
/// Differentiates confirmed facts from estimates and indicates freshness.
/// </summary>
public sealed record IntelligenceDossierSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_name")]
    public string TargetCountryName { get; init; } = string.Empty;

    [JsonPropertyName("operation_type")]
    public string OperationType { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("acquired_day")]
    public long AcquiredDay { get; init; }

    [JsonPropertyName("freshness_days_remaining")]
    public int FreshnessDaysRemaining { get; init; }

    [JsonPropertyName("confidence")]
    public string Confidence { get; init; } = string.Empty;

    [JsonPropertyName("report_summary")]
    public string ReportSummary { get; init; } = string.Empty;

    [JsonPropertyName("confirmed_treasury")]
    public double? ConfirmedTreasury { get; init; }

    [JsonPropertyName("estimated_treasury")]
    public double? EstimatedTreasury { get; init; }

    [JsonPropertyName("confirmed_military_readiness")]
    public double? ConfirmedMilitaryReadiness { get; init; }

    [JsonPropertyName("estimated_military_readiness")]
    public double? EstimatedMilitaryReadiness { get; init; }

    [JsonPropertyName("confirmed_gdp")]
    public double? ConfirmedGdp { get; init; }

    [JsonPropertyName("estimated_gdp")]
    public double? EstimatedGdp { get; init; }

    [JsonPropertyName("discovered_projects")]
    public IReadOnlyList<string> DiscoveredProjects { get; init; } = System.Array.Empty<string>();

    [JsonPropertyName("discovered_vulnerabilities")]
    public IReadOnlyList<string> DiscoveredVulnerabilities { get; init; } = System.Array.Empty<string>();
}
