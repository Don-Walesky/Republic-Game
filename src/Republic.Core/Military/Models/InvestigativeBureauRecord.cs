namespace Republic.Core.Military.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing a federal investigative agency and counter-intelligence bureau (CIA/FBI-style).
/// Can be commissioned via the State Investigative Bureau national development project.
/// Once operational, delivers elite counter-intelligence, intercepts hostile foreign spies, and investigates internal subversion.
/// </summary>
public sealed class InvestigativeBureauRecord
{
    [JsonPropertyName("is_commissioned")]
    public bool IsCommissioned { get; set; }

    [JsonPropertyName("bureau_name")]
    public string BureauName { get; set; } = "State Investigative Bureau (SIB)";

    [JsonPropertyName("counter_intelligence_rating")]
    public double CounterIntelligenceRating { get; set; } = 40.0; // Baseline before commissioning

    [JsonPropertyName("foreign_spy_detection_rate")]
    public double ForeignSpyDetectionRate => IsCommissioned
        ? Math.Clamp(Math.Round(55.0 + (CounterIntelligenceRating * 0.40), 1), 30.0, 95.0)
        : 20.0;

    [JsonPropertyName("internal_threat_audit_score")]
    public double InternalThreatAuditScore => IsCommissioned
        ? Math.Clamp(Math.Round(50.0 + (CounterIntelligenceRating * 0.45), 1), 25.0, 98.0)
        : 25.0;

    [JsonPropertyName("is_concealed")]
    public bool IsConcealed { get; set; } = true; // Hard for foreign adversaries to detect true capabilities

    [JsonPropertyName("neutralized_foreign_spies_count")]
    public int NeutralizedForeignSpiesCount { get; set; }

    [JsonPropertyName("active_investigations")]
    public List<string> ActiveInvestigations { get; set; } = new();

    [JsonPropertyName("status_summary")]
    public string StatusSummary => IsCommissioned
        ? $"Operational & Classified 🛡️: SIB counter-espionage active (Detection Rate: {ForeignSpyDetectionRate:0.0}%). Neutralized Spies: {NeutralizedForeignSpiesCount}."
        : "Uncommissioned 🔒: Domestic counter-intelligence fragmented. Requires commissioning the State Investigative Bureau national project.";

    /// <summary>
    /// Formally commissions the bureau following completion of the national project.
    /// </summary>
    public void Commission()
    {
        IsCommissioned = true;
        CounterIntelligenceRating = 85.0;
        IsConcealed = true;
        ActiveInvestigations.Add("Foreign Embassy Surveillance & Diplomatic Cable Wiretap");
        ActiveInvestigations.Add("Subversive Domestic Financing Audit");
    }

    /// <summary>
    /// Records the interception and neutralisation of a foreign espionage operative.
    /// </summary>
    public void RecordNeutralizedSpy(string foreignOriginCountry)
    {
        NeutralizedForeignSpiesCount++;
        ActiveInvestigations.Add($"Operative from {foreignOriginCountry} intercepted and neutralized on Day {DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000}.");
    }
}

