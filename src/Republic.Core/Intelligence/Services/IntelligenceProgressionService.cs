namespace Republic.Core.Intelligence.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Crises.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Intelligence.Models;
using Republic.Core.Media.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative implementation of the <see cref="IIntelligenceProgressionService"/> governing military-level gated espionage,
/// counterintelligence defenses, uncertain outcome calculations, intelligence dossier degradation, and diplomatic fallout.
/// </summary>
public sealed class IntelligenceProgressionService : IIntelligenceProgressionService
{
    private readonly ILogger? _logger;
    private readonly Random _random = new(1337);

    public IntelligenceProgressionConfig Config { get; set; } = new();

    public IntelligenceProgressionService(ILogger? logger = null, IntelligenceProgressionConfig? config = null)
    {
        _logger = logger;
        if (config != null)
        {
            Config = config;
        }
    }

    /// <inheritdoc />
    public (bool IsUnlocked, string Reason, int RequiredLevel) CheckOperationPrerequisites(
        Country initiator,
        IntelligenceOperationType operationType)
    {
        ArgumentNullException.ThrowIfNull(initiator);

        int currentLevel = initiator.MilitaryLevel;
        double currentReadiness = initiator.LatestSecurityAssessment?.CompositeReadiness ?? initiator.BaselineStability;
        double currentDefenseBudget = initiator.MinistryBudget?.DefenseAllocation ?? 500_000_000.0;

        switch (operationType)
        {
            case IntelligenceOperationType.DomesticThreatInvestigation:
            case IntelligenceOperationType.MilitaryReconnaissance:
            case IntelligenceOperationType.CounterintelligenceSweep:
                // Tier 0: Unlocked by default for sovereign nations
                return (true, "Operational capability fully authorized.", 1);

            case IntelligenceOperationType.StrategicIntelligence:
            case IntelligenceOperationType.PoliticalInfiltration:
                // Tier 1: Basic espionage
                if (currentLevel < Config.BasicEspionageMinMilitaryLevel)
                {
                    return (false,
                        $"Requires Military Level {Config.BasicEspionageMinMilitaryLevel} (Current: {currentLevel}). Expand armed forces personnel, equipment, or defense budget to unlock.",
                        Config.BasicEspionageMinMilitaryLevel);
                }
                if (currentReadiness < Config.BasicEspionageMinReadiness)
                {
                    return (false,
                        $"Armed forces readiness is insufficient ({currentReadiness:0.#}% / Required: {Config.BasicEspionageMinReadiness:0.#}%). Elevate DEFCON watch or conduct joint exercises.",
                        Config.BasicEspionageMinMilitaryLevel);
                }
                if (currentDefenseBudget < Config.BasicEspionageMinBudget)
                {
                    return (false,
                        $"Defense ministry allocation is below minimum threshold (R {currentDefenseBudget:N0} / Required: R {Config.BasicEspionageMinBudget:N0}).",
                        Config.BasicEspionageMinMilitaryLevel);
                }
                return (true, "Basic foreign espionage network authorized.", Config.BasicEspionageMinMilitaryLevel);

            case IntelligenceOperationType.IndustrialSabotage:
            case IntelligenceOperationType.AdvancedEspionage:
            case IntelligenceOperationType.CyberIntrusion:
                // Tier 2: Advanced espionage
                if (currentLevel < Config.AdvancedEspionageMinMilitaryLevel)
                {
                    return (false,
                        $"Requires Military Level {Config.AdvancedEspionageMinMilitaryLevel} (Current: {currentLevel}). Advanced espionage demands high-tier defense modernization and cyber corps.",
                        Config.AdvancedEspionageMinMilitaryLevel);
                }
                if (currentReadiness < Config.AdvancedEspionageMinReadiness)
                {
                    return (false,
                        $"Armed forces readiness is insufficient for high-risk covert operations ({currentReadiness:0.#}% / Required: {Config.AdvancedEspionageMinReadiness:0.#}%).",
                        Config.AdvancedEspionageMinMilitaryLevel);
                }
                if (currentDefenseBudget < Config.AdvancedEspionageMinBudget)
                {
                    return (false,
                        $"Defense ministry allocation is below advanced threshold (R {currentDefenseBudget:N0} / Required: R {Config.AdvancedEspionageMinBudget:N0}).",
                        Config.AdvancedEspionageMinMilitaryLevel);
                }
                return (true, "Advanced strategic intelligence operations unlocked.", Config.AdvancedEspionageMinMilitaryLevel);

            default:
                return (true, "Standard operational readiness.", 1);
        }
    }

    /// <inheritdoc />
    public (bool Success, string Message, IntelligenceOperation? Operation) LaunchOperation(
        Country initiator,
        Country target,
        IntelligenceOperationType operationType,
        string title,
        RepublicTime simulationTime,
        double? customCost = null)
    {
        ArgumentNullException.ThrowIfNull(initiator);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(simulationTime);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (string.Equals(initiator.Id, target.Id, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Cannot initiate covert foreign operations against one's own sovereign territory.", null);
        }

        var (isUnlocked, reason, reqLevel) = CheckOperationPrerequisites(initiator, operationType);
        if (!isUnlocked)
        {
            return (false, $"Operation locked: {reason}", null);
        }

        double cost = customCost ?? GetDefaultCostForOperation(operationType);
        if (initiator.Treasury.Balance < cost)
        {
            return (false, $"Insufficient treasury. Covert deployment requires R {cost:N0}, but treasury holds R {initiator.Treasury.Balance:N0}.", null);
        }

        // Deduct operational treasury funds
        initiator.Treasury.Withdraw(cost);

        int prepDays = GetPreparationDaysForOperation(operationType);
        double baseRisk = GetBaseRiskForOperation(operationType);

        var op = new IntelligenceOperation
        {
            InitiatorCountryId = initiator.Id,
            TargetCountryId = target.Id,
            TargetCountryName = target.Name,
            Type = operationType,
            Title = title.Trim(),
            FinancialCost = cost,
            RequiredMilitaryLevel = reqLevel,
            PreparationDays = prepDays,
            StartDay = simulationTime.DayNumber,
            ScheduledCompletionDay = simulationTime.DayNumber + prepDays,
            Status = prepDays == 0 ? IntelligenceOperationStatus.InProgress : IntelligenceOperationStatus.Preparing,
            RiskRating = baseRisk
        };

        initiator.StoreIntelligenceOperation(op);

        _logger?.LogInfo($"[Intelligence] Launched operation '{op.Title}' [{op.Type}] by {initiator.Name} targeting {target.Name}. Cost: R {cost:N0}");
        return (true, $"Covert operation '{op.Title}' initiated against {target.Name}. Scheduled completion: Day {op.ScheduledCompletionDay}.", op);
    }

    /// <inheritdoc />
    public (bool Success, string Message, IntelligenceOperation Operation) ResolveOperation(
        IntelligenceOperation operation,
        Country initiator,
        Country target,
        RepublicTime simulationTime,
        double? forcedSuccessRoll = null,
        double? forcedDetectionRoll = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(initiator);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(simulationTime);

        // Calculate dynamic success probability
        int levelAdvantage = initiator.MilitaryLevel - operation.RequiredMilitaryLevel;
        double defMinisterBonus = initiator.GetMinister(CabinetPortfolio.Defense)?.Competence ?? 0.5;
        double targetCounter = target.CounterintelligenceLevel;

        double baseSuccess = 0.65;
        double successProb = Math.Clamp(baseSuccess + (levelAdvantage * 0.03) + (defMinisterBonus * 0.15) - (targetCounter * 0.003), 0.20, 0.95);
        operation.SuccessProbability = Math.Round(successProb, 2);

        // Calculate dynamic detection probability
        double baseDetect = operation.RiskRating;
        double civilDefense = initiator.StateCapacity?.CivilServiceQuality ?? 0.3;
        double detectProb = Math.Clamp(baseDetect + (targetCounter * 0.004) - (civilDefense * 0.15), 0.10, 0.90);
        operation.DetectionProbability = Math.Round(detectProb, 2);

        double successRoll = forcedSuccessRoll ?? _random.NextDouble();
        double detectRoll = forcedDetectionRoll ?? _random.NextDouble();

        bool isSuccess = successRoll <= successProb;
        bool isDetected = detectRoll <= detectProb;

        operation.IsDetected = isDetected;

        if (isSuccess)
        {
            operation.Status = isDetected ? IntelligenceOperationStatus.Detected : IntelligenceOperationStatus.Succeeded;

            // Generate structured intelligence dossier
            var dossier = GenerateIntelligenceDossier(operation, target, simulationTime);
            operation.ResultDossier = dossier;
            initiator.StoreAcquiredIntelligence(dossier);

            operation.OutcomeMessage = isDetected
                ? $"Objective achieved: Classified dossier secured on {target.Name}. However, target security services detected hostile infiltration!"
                : $"Complete mission success: Intelligence successfully exfiltrated from {target.Name} without compromise.";
        }
        else
        {
            operation.Status = isDetected ? IntelligenceOperationStatus.Compromised : IntelligenceOperationStatus.Failed;
            operation.OutcomeMessage = isDetected
                ? $"Mission compromised: Covert operatives apprehended or neutralized inside {target.Name} territory. Diplomatic incident erupted!"
                : $"Mission aborted: Operatives unable to penetrate target secure communications. Tactical retreat ordered.";
        }

        // Apply diplomatic fallout and security alert if detected
        if (isDetected && !operation.DiplomaticPenaltyApplied)
        {
            ApplyDetectionConsequences(operation, initiator, target, simulationTime);
        }

        initiator.StoreIntelligenceOperation(operation);
        return (isSuccess, operation.OutcomeMessage, operation);
    }

    /// <inheritdoc />
    public (bool Success, string Message, int ThreatsNeutralized) ConductCounterintelligenceSweep(
        Country country,
        RepublicTime simulationTime,
        IEnumerable<Country>? allWorldCountries = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        const double sweepCost = 15_000_000.0;
        if (country.Treasury.Balance < sweepCost)
        {
            return (false, $"Insufficient treasury. Counterintelligence sweep requires R {sweepCost:N0}.", 0);
        }

        country.Treasury.Withdraw(sweepCost);

        int neutralized = 0;

        // Scan all other countries for hostile operations targeting this nation
        if (allWorldCountries != null)
        {
            foreach (var other in allWorldCountries.Where(c => c.Id != country.Id))
            {
                var hostileOps = other.IntelligenceOperations.Where(o =>
                    string.Equals(o.TargetCountryId, country.Id, StringComparison.OrdinalIgnoreCase) &&
                    o.Status is IntelligenceOperationStatus.Preparing or IntelligenceOperationStatus.InProgress).ToList();

                foreach (var op in hostileOps)
                {
                    op.IsDetected = true;
                    op.Status = IntelligenceOperationStatus.Compromised;
                    op.OutcomeMessage = $"Operation intercepted and neutralized by {country.Name} counterintelligence sweep.";
                    ApplyDetectionConsequences(op, other, country, simulationTime);
                    neutralized++;
                }
            }
        }

        // Cleanse domestic threat list of unconfirmed/active threats
        var espionageThreats = country.ActiveSecurityThreats
            .Where(t => t.Category.Contains("Espionage", StringComparison.OrdinalIgnoreCase) ||
                        t.Category.Contains("Wiretap", StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var t in espionageThreats)
        {
            t.Status = "Neutralized";
            neutralized++;
        }

        var message = $"Domestic counterintelligence sweep completed across all state ministries and communications channels. {neutralized} hostile trace(s) neutralized.";
        _logger?.LogInfo($"[Counterintelligence] {country.Name}: {message}");

        return (true, message, neutralized);
    }

    /// <inheritdoc />
    public void DegradeIntelligenceFreshness(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        foreach (var dossier in country.AcquiredIntelligence)
        {
            dossier.Degrade(simulationTime);
        }
    }

    private void ApplyDetectionConsequences(
        IntelligenceOperation operation,
        Country initiator,
        Country target,
        RepublicTime simulationTime)
    {
        operation.DiplomaticPenaltyApplied = true;

        // 1. Diplomatic fallout: initiator suffers international embarrassment and diplomatic blowback
        initiator.BaselineStability = Math.Max(5.0, initiator.BaselineStability - 4.0);
        initiator.ApprovalRating = Math.Max(5.0, initiator.ApprovalRating - 3.0);

        // 2. Alert target country's security leadership
        var threatReport = new SecurityThreatReport
        {
            ThreatId = $"INTEL-ALERT-{Guid.NewGuid():N}"[..18],
            Title = $"Hostile Foreign Espionage Intercepted [{operation.Type}]",
            Category = "Espionage",
            Severity = CrisisSeverity.Severe,
            Confidence = Republic.Core.Government.Officials.Models.IntelligenceConfidence.Confirmed,
            SourceAgency = "Ministry of National Security",
            Status = "Active Breach Investigation",
            Details = $"State counterintelligence intercepted clandestine operatives connected to {initiator.Name}. Operational objective: {operation.Title}.",
            RecommendedResponse = "File formal diplomatic protest, expel hostile diplomatic staff, and heighten counter-surveillance protocols."
        };

        target.ActiveSecurityThreats.Add(threatReport);

        // 3. Target security assessment alert
        if (target.LatestSecurityAssessment != null)
        {
            target.LatestSecurityAssessment.Threats.Add(threatReport);
        }

        // 4. Public media leak
        var mediaReport = new MediaReport
        {
            OutletId = "global-intel-monitor",
            OutletName = "Global Security Review",
            Headline = $"DIPLOMATIC CRISIS: {initiator.Name.ToUpperInvariant()} SPY NETWORK EXPOSED IN {target.Name.ToUpperInvariant()}",
            ArticleSummary = $"Classified foreign operatives linked to the {initiator.Name} government were intercepted conducting {operation.Type} against state infrastructure.",
            Category = "Security",
            Sentiment = MediaReportSentiment.Critical,
            EventSource = "EspionageIncident",
            PublishedDay = simulationTime.DayNumber,
            PublicApprovalImpact = -4.0,
            StabilityImpact = -5.0
        };

        initiator.MediaReports.Add(mediaReport);
        initiator.HappinessRating = Math.Max(10.0, initiator.HappinessRating - 4.0);
        initiator.BaselineStability = Math.Max(10.0, initiator.BaselineStability - 5.0);
    }

    private IntelligenceDossierReport GenerateIntelligenceDossier(
        IntelligenceOperation operation,
        Country target,
        RepublicTime simulationTime)
    {
        var report = new IntelligenceDossierReport
        {
            InitiatorCountryId = operation.InitiatorCountryId,
            TargetCountryId = target.Id,
            TargetCountryName = target.Name,
            OperationType = operation.Type,
            Title = operation.Title,
            AcquiredDay = simulationTime.DayNumber,
            FreshnessDaysRemaining = Config.IntelligenceFreshnessLifespanDays
        };

        var actualTreasury = target.Treasury.Balance;
        var actualReadiness = target.LatestSecurityAssessment?.CompositeReadiness ?? target.BaselineStability;
        var actualGdp = target.GrossDomesticProduct;

        switch (operation.Type)
        {
            case IntelligenceOperationType.AdvancedEspionage:
            case IntelligenceOperationType.CyberIntrusion:
            case IntelligenceOperationType.IndustrialSabotage:
                // Tier 2: Confirmed facts
                report.Confidence = IntelligenceDossierConfidence.ConfirmedFact;
                report.ConfirmedTreasury = actualTreasury;
                report.ConfirmedMilitaryReadiness = Math.Round(actualReadiness, 1);
                report.ConfirmedGdp = actualGdp;
                report.ReportSummary = $"Confirmed Signals & Human Intelligence: Verified sovereign treasury R {actualTreasury:N0}, armed forces readiness {actualReadiness:0.#}%, and total GDP R {actualGdp:N0}.";
                report.DiscoveredProjects = target.Projects?.Where(p => p.IsActive).Select(p => p.Name).ToList() ?? new();
                report.DiscoveredVulnerabilities.Add(actualTreasury < 100_000_000.0 ? "Low sovereign liquidity reserves" : "High treasury accumulation");
                report.DiscoveredVulnerabilities.Add(actualReadiness < 65.0 ? "Vulnerable armed forces readiness" : "Robust frontline military readiness");
                break;

            case IntelligenceOperationType.StrategicIntelligence:
            case IntelligenceOperationType.PoliticalInfiltration:
                // Tier 1: High confidence estimates
                report.Confidence = IntelligenceDossierConfidence.HighConfidenceEstimate;
                report.EstimatedTreasury = Math.Round(actualTreasury * (0.92 + (_random.NextDouble() * 0.16)), 0);
                report.EstimatedMilitaryReadiness = Math.Round(Math.Clamp(actualReadiness + (_random.NextDouble() * 8.0 - 4.0), 10.0, 100.0), 1);
                report.EstimatedGdp = Math.Round(actualGdp * (0.95 + (_random.NextDouble() * 0.10)), 0);
                report.ReportSummary = $"Strategic Analysis Dossier: Estimated sovereign treasury R {report.EstimatedTreasury:N0} (+/- 8%), estimated military readiness {report.EstimatedMilitaryReadiness:0.#}%.";
                report.DiscoveredProjects = target.Projects?.Take(2).Select(p => p.Name).ToList() ?? new();
                break;

            default:
                // Tier 0: Reconnaissance estimates
                report.Confidence = IntelligenceDossierConfidence.LowConfidenceEstimate;
                report.EstimatedMilitaryReadiness = Math.Round(Math.Clamp(actualReadiness + (_random.NextDouble() * 15.0 - 7.5), 10.0, 100.0), 1);
                report.EstimatedTreasury = Math.Round(actualTreasury * (0.85 + (_random.NextDouble() * 0.30)), 0);
                report.ReportSummary = $"Tactical Reconnaissance Report: Border forces estimate readiness at approximately {report.EstimatedMilitaryReadiness:0.#}%.";
                break;
        }

        return report;
    }

    private static double GetDefaultCostForOperation(IntelligenceOperationType type) => type switch
    {
        IntelligenceOperationType.DomesticThreatInvestigation => 5_000_000.0,
        IntelligenceOperationType.MilitaryReconnaissance => 10_000_000.0,
        IntelligenceOperationType.CounterintelligenceSweep => 15_000_000.0,
        IntelligenceOperationType.StrategicIntelligence => 30_000_000.0,
        IntelligenceOperationType.PoliticalInfiltration => 35_000_000.0,
        IntelligenceOperationType.IndustrialSabotage => 60_000_000.0,
        IntelligenceOperationType.AdvancedEspionage => 65_000_000.0,
        IntelligenceOperationType.CyberIntrusion => 50_000_000.0,
        _ => 25_000_000.0
    };

    private static int GetPreparationDaysForOperation(IntelligenceOperationType type) => type switch
    {
        IntelligenceOperationType.DomesticThreatInvestigation => 0,
        IntelligenceOperationType.MilitaryReconnaissance => 0,
        IntelligenceOperationType.CounterintelligenceSweep => 0,
        IntelligenceOperationType.StrategicIntelligence => 1,
        IntelligenceOperationType.PoliticalInfiltration => 2,
        IntelligenceOperationType.IndustrialSabotage => 3,
        IntelligenceOperationType.AdvancedEspionage => 3,
        IntelligenceOperationType.CyberIntrusion => 2,
        _ => 1
    };

    private static double GetBaseRiskForOperation(IntelligenceOperationType type) => type switch
    {
        IntelligenceOperationType.DomesticThreatInvestigation => 0.10,
        IntelligenceOperationType.MilitaryReconnaissance => 0.20,
        IntelligenceOperationType.CounterintelligenceSweep => 0.15,
        IntelligenceOperationType.StrategicIntelligence => 0.35,
        IntelligenceOperationType.PoliticalInfiltration => 0.45,
        IntelligenceOperationType.IndustrialSabotage => 0.65,
        IntelligenceOperationType.AdvancedEspionage => 0.60,
        IntelligenceOperationType.CyberIntrusion => 0.50,
        _ => 0.30
    };
}
