namespace Republic.Core.Intelligence.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Distinct categories of state security, threat investigation, and covert foreign intelligence operations.
/// </summary>
public enum IntelligenceOperationType
{
    // Tier 0: Domestic security & border reconnaissance (Unlocked at Level 1+)
    DomesticThreatInvestigation = 0,
    MilitaryReconnaissance = 1,
    CounterintelligenceSweep = 2,

    // Tier 1: Basic foreign espionage (Unlocked at configurable Military Level 15+)
    StrategicIntelligence = 3,
    PoliticalInfiltration = 4,

    // Tier 2: Advanced covert espionage (Unlocked at configurable Military Level 20+)
    IndustrialSabotage = 5,
    AdvancedEspionage = 6,
    CyberIntrusion = 7
}

/// <summary>
/// Status lifecycle of a covert or security intelligence operation.
/// </summary>
public enum IntelligenceOperationStatus
{
    Draft,
    Preparing,
    InProgress,
    Succeeded,
    Detected,
    Compromised,
    Failed,
    Cancelled
}

/// <summary>
/// Confidence classification for gathered foreign intelligence.
/// </summary>
public enum IntelligenceDossierConfidence
{
    ConfirmedFact,
    HighConfidenceEstimate,
    LowConfidenceEstimate,
    StaleOrOutdated
}

/// <summary>
/// Structured dossier report resulting from successful or partially successful intelligence operations.
/// Differentiates confirmed facts from estimates and degrades in freshness over time.
/// </summary>
public sealed class IntelligenceDossierReport
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("initiator_country_id")]
    public string InitiatorCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_name")]
    public string TargetCountryName { get; set; } = string.Empty;

    [JsonPropertyName("operation_type")]
    public IntelligenceOperationType OperationType { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("acquired_day")]
    public long AcquiredDay { get; init; }

    [JsonPropertyName("freshness_days_remaining")]
    public int FreshnessDaysRemaining { get; set; } = 30;

    [JsonPropertyName("confidence")]
    public IntelligenceDossierConfidence Confidence { get; set; } = IntelligenceDossierConfidence.HighConfidenceEstimate;

    [JsonPropertyName("report_summary")]
    public string ReportSummary { get; set; } = string.Empty;

    [JsonPropertyName("confirmed_treasury")]
    public double? ConfirmedTreasury { get; set; }

    [JsonPropertyName("estimated_treasury")]
    public double? EstimatedTreasury { get; set; }

    [JsonPropertyName("confirmed_military_readiness")]
    public double? ConfirmedMilitaryReadiness { get; set; }

    [JsonPropertyName("estimated_military_readiness")]
    public double? EstimatedMilitaryReadiness { get; set; }

    [JsonPropertyName("confirmed_gdp")]
    public double? ConfirmedGdp { get; set; }

    [JsonPropertyName("estimated_gdp")]
    public double? EstimatedGdp { get; set; }

    [JsonPropertyName("discovered_projects")]
    public List<string> DiscoveredProjects { get; set; } = new();

    [JsonPropertyName("discovered_vulnerabilities")]
    public List<string> DiscoveredVulnerabilities { get; set; } = new();

    /// <summary>
    /// Degrades intelligence freshness over time. If expired, degrades confidence to StaleOrOutdated.
    /// </summary>
    public void Degrade(RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(currentTime);
        long elapsedDays = currentTime.DayNumber - AcquiredDay;
        FreshnessDaysRemaining = Math.Max(0, 30 - (int)elapsedDays);

        if (FreshnessDaysRemaining <= 0)
        {
            Confidence = IntelligenceDossierConfidence.StaleOrOutdated;
            ReportSummary = $"[OUTDATED DOSSIER] Intelligence collected on Day {AcquiredDay} is now stale. Target may have mobilized reserves or altered state finances.";
        }
    }
}

/// <summary>
/// Domain model for an active, scheduled, or completed covert intelligence operation.
/// </summary>
public sealed class IntelligenceOperation
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("initiator_country_id")]
    public string InitiatorCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_name")]
    public string TargetCountryName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public IntelligenceOperationType Type { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("financial_cost")]
    public double FinancialCost { get; init; } = 35_000_000.0;

    [JsonPropertyName("required_military_level")]
    public int RequiredMilitaryLevel { get; init; } = 1;

    [JsonPropertyName("required_readiness")]
    public double RequiredReadiness { get; init; } = 50.0;

    [JsonPropertyName("preparation_days")]
    public int PreparationDays { get; init; } = 1;

    [JsonPropertyName("start_day")]
    public long StartDay { get; set; } = 1;

    [JsonPropertyName("scheduled_completion_day")]
    public long ScheduledCompletionDay { get; set; } = 2;

    [JsonPropertyName("status")]
    public IntelligenceOperationStatus Status { get; set; } = IntelligenceOperationStatus.Preparing;

    [JsonPropertyName("risk_rating")]
    public double RiskRating { get; init; } = 0.35; // Base risk of failure/exposure (0.0 to 1.0)

    [JsonPropertyName("success_probability")]
    public double SuccessProbability { get; set; } = 0.70;

    [JsonPropertyName("detection_probability")]
    public double DetectionProbability { get; set; } = 0.25;

    [JsonPropertyName("is_detected")]
    public bool IsDetected { get; set; }

    [JsonPropertyName("diplomatic_penalty_applied")]
    public bool DiplomaticPenaltyApplied { get; set; }

    [JsonPropertyName("outcome_message")]
    public string OutcomeMessage { get; set; } = string.Empty;

    [JsonPropertyName("result_dossier")]
    public IntelligenceDossierReport? ResultDossier { get; set; }
}

/// <summary>
/// Configurable thresholds and prerequisites for unlocking basic and advanced espionage operations.
/// </summary>
public sealed class IntelligenceProgressionConfig
{
    public int BasicEspionageMinMilitaryLevel { get; set; } = 15;
    public int AdvancedEspionageMinMilitaryLevel { get; set; } = 20;

    public double BasicEspionageMinReadiness { get; set; } = 60.0;
    public double AdvancedEspionageMinReadiness { get; set; } = 75.0;

    public double BasicEspionageMinBudget { get; set; } = 250_000_000.0;
    public double AdvancedEspionageMinBudget { get; set; } = 400_000_000.0;

    public int IntelligenceFreshnessLifespanDays { get; set; } = 30;
}
