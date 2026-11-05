namespace Republic.Core.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Serializable client data contract capturing State Investigative Bureau (SIB) counter-intelligence and threat auditing.
/// </summary>
public sealed record InvestigativeBureauSnapshot
{
    [JsonPropertyName("is_commissioned")]
    public bool IsCommissioned { get; init; }

    [JsonPropertyName("bureau_name")]
    public string BureauName { get; init; } = string.Empty;

    [JsonPropertyName("counter_intelligence_rating")]
    public double CounterIntelligenceRating { get; init; }

    [JsonPropertyName("foreign_spy_detection_rate")]
    public double ForeignSpyDetectionRate { get; init; }

    [JsonPropertyName("internal_threat_audit_score")]
    public double InternalThreatAuditScore { get; init; }

    [JsonPropertyName("is_concealed")]
    public bool IsConcealed { get; init; }

    [JsonPropertyName("neutralized_foreign_spies_count")]
    public int NeutralizedForeignSpiesCount { get; init; }

    [JsonPropertyName("active_investigations")]
    public IReadOnlyList<string> ActiveInvestigations { get; init; } = Array.Empty<string>();

    [JsonPropertyName("status_summary")]
    public string StatusSummary { get; init; } = string.Empty;
}

