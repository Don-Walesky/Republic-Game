namespace Republic.Core.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Economy.PrivateSector.Services;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Government.Officials.Services;
using Republic.Core.Media.Models;
using Republic.Core.Media.Services;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Serializable data contract representing the live Republic simulation state displayed in the Godot presidential desk.
/// Encapsulates national identity, treasury balance, public approval, stability, simulation clock, and political status.
/// </summary>
public sealed record PresidentialDeskSnapshot
{
    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    [JsonPropertyName("country_name")]
    public string CountryName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the numerical sovereign treasury balance in REPU.
    /// </summary>
    [JsonPropertyName("treasury_balance")]
    public double TreasuryBalance { get; init; }

    /// <summary>
    /// Gets the formatted sovereign treasury string in standard REPU notation.
    /// </summary>
    [JsonPropertyName("treasury")]
    public string Treasury { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign public approval rating percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("approval_rating")]
    public double ApprovalRating { get; init; }

    /// <summary>
    /// Gets the formatted public approval percentage string (e.g. "78%").
    /// </summary>
    [JsonPropertyName("approval")]
    public string Approval { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign baseline stability rating percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("stability_rating")]
    public double StabilityRating { get; init; }

    /// <summary>
    /// Gets the formatted stability percentage string (e.g. "85%").
    /// </summary>
    [JsonPropertyName("stability")]
    public string Stability { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authoritative Republic simulation day number.
    /// </summary>
    [JsonPropertyName("simulation_day")]
    public long SimulationDay { get; init; }

    /// <summary>
    /// Gets the concise simulation time string (e.g. "Day 1, 00:00 WAT").
    /// </summary>
    [JsonPropertyName("simulation_time")]
    public string SimulationTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed simulation timestamp string (e.g. "Day 1, 1 January 2030 00:00 WAT").
    /// </summary>
    [JsonPropertyName("simulation_time_detailed")]
    public string SimulationTimeDetailed { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the sovereign ruling political party, or "None" if unestablished.
    /// </summary>
    [JsonPropertyName("ruling_party")]
    public string RulingParty { get; init; } = "None";

    /// <summary>
    /// Gets the status or description of the most recent national democratic election.
    /// </summary>
    [JsonPropertyName("election")]
    public string Election { get; init; } = "No election";

    /// <summary>
    /// Gets the name of the sovereign opposition political party, or "None" if unestablished.
    /// </summary>
    [JsonPropertyName("opposition_party")]
    public string OppositionParty { get; init; } = "None";

    /// <summary>
    /// Gets the public political support rating of the ruling party.
    /// </summary>
    [JsonPropertyName("polling_support")]
    public double PollingSupport { get; init; }

    /// <summary>
    /// Gets the public political support rating of the opposition party.
    /// </summary>
    [JsonPropertyName("opposition_support")]
    public double OppositionSupport { get; init; }

    /// <summary>
    /// Gets the scheduled simulation day for the next national election.
    /// </summary>
    [JsonPropertyName("next_election_day")]
    public long NextElectionDay { get; init; } = 30;

    /// <summary>
    /// Gets the executive cabinet summary text showing ministerial appointments.
    /// </summary>
    [JsonPropertyName("cabinet_summary")]
    public string CabinetSummary { get; init; } = "Finance: Vacant | Interior: Vacant";

    /// <summary>
    /// Gets the name of the appointed Minister of Finance, or "Vacant".
    /// </summary>
    [JsonPropertyName("finance_minister")]
    public string FinanceMinister { get; init; } = "Vacant";

    /// <summary>
    /// Gets the name of the appointed Minister of the Interior, or "Vacant".
    /// </summary>
    [JsonPropertyName("interior_minister")]
    public string InteriorMinister { get; init; } = "Vacant";

    /// <summary>
    /// Gets the name of the appointed Minister of Infrastructure, or "Vacant".
    /// </summary>
    [JsonPropertyName("infrastructure_minister")]
    public string InfrastructureMinister { get; init; } = "Vacant";

    /// <summary>
    /// Gets the name of the appointed Minister of Defense, or "Vacant".
    /// </summary>
    [JsonPropertyName("defense_minister")]
    public string DefenseMinister { get; init; } = "Vacant";

    /// <summary>
    /// Gets the civil service quality factor (0.0 to 1.0) governed by the Finance portfolio.
    /// </summary>
    [JsonPropertyName("civil_service_quality")]
    public double CivilServiceQuality { get; init; } = 0.3;

    /// <summary>
    /// Gets the corruption control factor (0.0 to 1.0) governed by the Interior portfolio.
    /// </summary>
    [JsonPropertyName("corruption_control")]
    public double CorruptionControl { get; init; } = 0.3;

    /// <summary>
    /// Gets the infrastructure condition factor (0.0 to 1.0) governed by the Infrastructure portfolio.
    /// </summary>
    [JsonPropertyName("infrastructure_condition")]
    public double InfrastructureCondition { get; init; } = 0.3;

    /// <summary>
    /// Gets the structured collection of cabinet portfolios and appointed minister states.
    /// </summary>
    [JsonPropertyName("cabinet")]
    public IReadOnlyList<CabinetMinisterSnapshot> Cabinet { get; init; } = Array.Empty<CabinetMinisterSnapshot>();

    /// <summary>
    /// Gets the live connection status message.
    /// </summary>
    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; } = "Connected to live Republic simulation state";

    /// <summary>
    /// Gets the UTC timestamp at which this state snapshot was exported.
    /// </summary>
    [JsonPropertyName("exported_at_utc")]
    public string ExportedAtUtc { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique sovereign country identifier.
    /// </summary>
    [JsonPropertyName("country_id")]
    public string CountryId { get; init; } = "player-country";

    /// <summary>
    /// Gets the personal income tax rate as a decimal (e.g. 0.25 for 25%).
    /// </summary>
    [JsonPropertyName("income_tax_rate")]
    public double IncomeTaxRate { get; init; } = 0.25;

    /// <summary>
    /// Gets the formatted personal income tax rate percentage string (e.g. "25%").
    /// </summary>
    [JsonPropertyName("income_tax_rate_formatted")]
    public string IncomeTaxRateFormatted { get; init; } = "25%";

    /// <summary>
    /// Gets the corporate tax rate as a decimal (e.g. 0.20 for 20%).
    /// </summary>
    [JsonPropertyName("corporate_tax_rate")]
    public double CorporateTaxRate { get; init; } = 0.20;

    /// <summary>
    /// Gets the import tariff rate as a decimal (e.g. 0.10 for 10%).
    /// </summary>
    [JsonPropertyName("import_tariff_rate")]
    public double ImportTariffRate { get; init; } = 0.10;

    /// <summary>
    /// Gets the explanatory outcome message or citizen feedback from the latest presidential decision.
    /// </summary>
    [JsonPropertyName("latest_decision_result")]
    public string LatestDecisionResult { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name and status of the current active national development project, or "None" if no project is active.
    /// </summary>
    [JsonPropertyName("active_project")]
    public string ActiveProject { get; init; } = "None";

    /// <summary>
    /// Gets the simulation engine tick at which this state snapshot was exported.
    /// </summary>
    [JsonPropertyName("simulation_tick")]
    public ulong SimulationTick { get; init; }

    /// <summary>
    /// Gets the path to the authoritative persistent save file from which this snapshot was projected.
    /// </summary>
    [JsonPropertyName("save_file_path")]
    public string SaveFilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive summary of the latest decision made by an autonomous AI government.
    /// </summary>
    [JsonPropertyName("last_ai_decision")]
    public string LastAiDecision { get; init; } = "[No AI decisions recorded]";

    /// <summary>
    /// Gets the formatted Gross Domestic Product (GDP) string in standard REPU notation.
    /// </summary>
    [JsonPropertyName("gdp_formatted")]
    public string GdpFormatted { get; init; } = "R 10,000,000";

    /// <summary>
    /// Gets the concise summary of the latest quarterly fiscal report.
    /// </summary>
    [JsonPropertyName("fiscal_report_summary")]
    public string FiscalReportSummary { get; init; } = "No quarterly fiscal reports yet recorded";

    /// <summary>
    /// Gets the formatted net surplus or deficit string in standard REPU notation.
    /// </summary>
    [JsonPropertyName("fiscal_deficit_formatted")]
    public string FiscalDeficitFormatted { get; init; } = "Balanced";

    /// <summary>
    /// Gets the active emergency crisis status, or "None" if stable.
    /// </summary>
    [JsonPropertyName("active_crisis")]
    public string ActiveCrisis { get; init; } = "None";

    /// <summary>
    /// Gets the explanatory situational details for the current active crisis.
    /// </summary>
    [JsonPropertyName("active_crisis_details")]
    public string ActiveCrisisDetails { get; init; } = "State institutions stable. Sovereign order maintained.";

    /// <summary>
    /// Gets the summary of completed capital development projects.
    /// </summary>
    [JsonPropertyName("completed_projects_summary")]
    public string CompletedProjectsSummary { get; init; } = "None";

    /// <summary>
    /// Gets the institutional state capacity breakdown summary string.
    /// </summary>
    [JsonPropertyName("state_capacity_summary")]
    public string StateCapacitySummary { get; init; } = "Civil Service: 30% | Corruption Control: 30% | Infrastructure: 30%";

    /// <summary>
    /// Gets the executive briefing summary of the most recent offline progression catch-up.
    /// </summary>
    [JsonPropertyName("offline_briefing_summary")]
    public string OfflineBriefingSummary { get; init; } = "Offline Status: Real-time simulation aligned. Zero pending offline cycles.";

    /// <summary>
    /// Gets the active DEFCON defense readiness alert level.
    /// </summary>
    [JsonPropertyName("defcon_level")]
    public string DefconLevel { get; init; } = "DEFCON 5 (Peacetime Watch)";

    /// <summary>
    /// Gets the composite military readiness score formatted string.
    /// </summary>
    [JsonPropertyName("military_readiness")]
    public string MilitaryReadiness { get; init; } = "85.0% (Armed Forces Operational)";

    /// <summary>
    /// Gets the list of proactive Chief of Staff recommendations prioritized by urgency and national impact.
    /// </summary>
    [JsonPropertyName("chief_of_staff_recommendations")]
    public IReadOnlyList<ChiefOfStaffRecommendationSnapshot> ChiefOfStaffRecommendations { get; init; } = Array.Empty<ChiefOfStaffRecommendationSnapshot>();

    /// <summary>
    /// Gets the executive briefing text summarizing the Chief of Staff's top priority matter.
    /// </summary>
    [JsonPropertyName("chief_of_staff_briefing")]
    public string ChiefOfStaffBriefing { get; init; } = "All executive departments reporting normal operations.";

    /// <summary>
    /// Gets the portfolio-specific advice generated by the six constitutional cabinet ministries.
    /// </summary>
    [JsonPropertyName("ministerial_advice")]
    public IReadOnlyList<MinisterialAdviceSnapshot> MinisterialAdvice { get; init; } = Array.Empty<MinisterialAdviceSnapshot>();

    /// <summary>
    /// Gets the active domestic and external security threat reports.
    /// </summary>
    [JsonPropertyName("security_threats")]
    public IReadOnlyList<SecurityThreatSnapshot> SecurityThreats { get; init; } = Array.Empty<SecurityThreatSnapshot>();

    /// <summary>
    /// Gets the comprehensive national security assessment summary.
    /// </summary>
    [JsonPropertyName("security_assessment_summary")]
    public string SecurityAssessmentSummary { get; init; } = "Sovereign frontiers secure. Peacetime readiness maintained.";

    /// <summary>
    /// Gets the domestic security threat index (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("domestic_threat_index")]
    public double DomesticThreatIndex { get; init; } = 20.0;

    /// <summary>
    /// Gets the external security threat index (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("external_threat_index")]
    public double ExternalThreatIndex { get; init; } = 15.0;

    /// <summary>
    /// Gets the active media outlets operating within the sovereign republic.
    /// </summary>
    [JsonPropertyName("media_outlets")]
    public IReadOnlyList<MediaOutletSnapshot> MediaOutlets { get; init; } = Array.Empty<MediaOutletSnapshot>();

    /// <summary>
    /// Gets the published and leaked media reports evaluating recent events.
    /// </summary>
    [JsonPropertyName("media_reports")]
    public IReadOnlyList<MediaReportSnapshot> MediaReports { get; init; } = Array.Empty<MediaReportSnapshot>();

    /// <summary>
    /// Gets the media landscape briefing summary.
    /// </summary>
    [JsonPropertyName("media_summary")]
    public string MediaSummary { get; init; } = "Media Environment: Free press operating under standard constitutional guarantees.";

    /// <summary>
    /// Gets the active commercial enterprises, industrialists, and private-sector investors.
    /// </summary>
    [JsonPropertyName("private_investors")]
    public IReadOnlyList<PrivateInvestorSnapshot> PrivateInvestors { get; init; } = Array.Empty<PrivateInvestorSnapshot>();

    /// <summary>
    /// Gets the inbound and processed commercial investment proposals.
    /// </summary>
    [JsonPropertyName("investment_proposals")]
    public IReadOnlyList<InvestmentProposalSnapshot> InvestmentProposals { get; init; } = Array.Empty<InvestmentProposalSnapshot>();

    /// <summary>
    /// Gets the private-sector investment status summary.
    /// </summary>
    [JsonPropertyName("investment_summary")]
    public string InvestmentSummary { get; init; } = "Private Sector: Corporate investment channels open.";

    /// <summary>
    /// Factory helper to build a <see cref="PresidentialDeskSnapshot"/> from live simulation state.
    /// Reuses existing core models and services without duplicating domain rules.
    /// </summary>
    public static PresidentialDeskSnapshot Create(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? saveFilePath = null,
        string? latestDecisionResult = null,
        string? statusMessage = null,
        string? lastAiDecision = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var dashboard = country.GetDashboard(simulationTime);

        var rulingPartyName = country.RulingParty?.Name;
        if (string.IsNullOrWhiteSpace(rulingPartyName))
        {
            rulingPartyName = "None";
        }

        var oppositionPartyName = country.OppositionParty?.Name ?? "None";
        var pollingSupport = country.RulingParty?.Support ?? 0.0;
        var oppositionSupport = country.OppositionParty?.Support ?? 0.0;
        long nextElectionDay = country.LastElection != null
            ? country.LastElection.Boundary.DayNumber + 30
            : simulationTime.DayNumber + 30;

        string electionText;
        if (country.LastElection != null)
        {
            var election = country.LastElection;
            electionText = $"Day {election.Boundary.DayNumber} ({election.WinnerPartyName} victory, {election.WinnerSupport:0.#}% vs {election.LoserSupport:0.#}%)";
        }
        else
        {
            electionText = "No election";
        }

        var conciseTime = $"Day {simulationTime.DayNumber}, {simulationTime.Time:HH:mm} WAT";
        var detailedTime = $"Day {simulationTime.DayNumber}, {simulationTime.Date:d MMMM yyyy} {simulationTime.Time:HH:mm} WAT";

        var formattedTreasury = $"R {country.Treasury.Balance:N0}";

        var taxPolicy = country.TaxPolicy ?? new Republic.Core.Economy.Budget.Models.TaxPolicy();
        var formattedTaxRate = $"{taxPolicy.IncomeTaxRate * 100:0.#}%";
        var decisionResult = latestDecisionResult ?? country.LatestCitizenFeedback?.Description ?? string.Empty;
        var resolvedAiDecision = !string.IsNullOrWhiteSpace(lastAiDecision)
            ? lastAiDecision
            : (country.LastAIDecision ?? "[No AI decisions recorded]");

        var activeProj = country.Projects?.FirstOrDefault(p => p.IsActive);
        string activeProjectText = activeProj != null
            ? $"{activeProj.Name} (Finishes: {activeProj.FinishBoundary})"
            : "None";

        var financeMinister = country.GetMinister(CabinetPortfolio.Finance);
        var interiorMinister = country.GetMinister(CabinetPortfolio.Interior);
        var infraMinister = country.GetMinister(CabinetPortfolio.Infrastructure);
        var defenseMinister = country.GetMinister(CabinetPortfolio.Defense);

        var financeName = financeMinister?.Name ?? "Vacant";
        var interiorName = interiorMinister?.Name ?? "Vacant";
        var infraName = infraMinister?.Name ?? "Vacant";
        var defenseName = defenseMinister?.Name ?? "Vacant";

        var financeDesc = financeMinister != null
            ? $"{financeMinister.Name} ({financeMinister.Competence * 100:0.#}% Comp)"
            : "Vacant (30% Cap)";

        var interiorDesc = interiorMinister != null
            ? $"{interiorMinister.Name} ({interiorMinister.Competence * 100:0.#}% Comp)"
            : "Vacant (30% Cap)";

        var cabinetSummary = $"Finance: {financeDesc} | Interior: {interiorDesc}";

        var civilQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3;
        var corrControl = country.StateCapacity?.CorruptionControl ?? 0.3;
        var infraCondition = country.StateCapacity?.InfrastructureCondition ?? 0.3;
        var stateCapacitySummary = $"Civil Service: {civilQuality * 100:0.#}% | Corruption Control: {corrControl * 100:0.#}% | Infrastructure: {infraCondition * 100:0.#}%";

        var cabinetList = new List<CabinetMinisterSnapshot>();
        foreach (var p in Enum.GetValues<CabinetPortfolio>())
        {
            var m = country.GetMinister(p);
            string? capMetric = null;
            double? capValue = null;

            switch (p)
            {
                case CabinetPortfolio.Finance:
                    capMetric = "Civil Service Quality";
                    capValue = civilQuality;
                    break;
                case CabinetPortfolio.Interior:
                    capMetric = "Corruption Control";
                    capValue = corrControl;
                    break;
                case CabinetPortfolio.Infrastructure:
                    capMetric = "Infrastructure Condition";
                    capValue = infraCondition;
                    break;
            }

            cabinetList.Add(new CabinetMinisterSnapshot
            {
                Portfolio = p.ToString(),
                IsAppointed = m != null,
                Status = m != null ? "Appointed" : "Vacant",
                MinisterId = m?.Id ?? string.Empty,
                MinisterName = m?.Name ?? "Vacant",
                Competence = m?.Competence ?? 0.0,
                Loyalty = m?.Loyalty ?? 0.0,
                Integrity = m?.Integrity ?? 0.0,
                Experience = m?.Experience ?? 0.0,
                PoliticalConnections = m?.PoliticalConnections ?? 0.0,
                MonthlySalary = m?.MonthlySalary ?? 0.0,
                FactionAlignmentId = m?.FactionAlignmentId ?? string.Empty,
                StateCapacityMetric = capMetric,
                StateCapacityValue = capValue
            });
        }

        var gdpFormatted = $"R {country.GrossDomesticProduct:N0}";

        string fiscalSummary;
        string deficitFormatted;
        if (country.LatestFiscalReport != null)
        {
            var r = country.LatestFiscalReport;
            var netPrefix = r.NetSurplusOrDeficit >= 0 ? "+" : "";
            fiscalSummary = $"Q{r.QuarterIndex}: Rev R {r.TotalTaxRevenue:N0} | Exp R {r.TotalExpenditures:N0} | Net {netPrefix}R {r.NetSurplusOrDeficit:N0}";
            deficitFormatted = r.NetSurplusOrDeficit >= 0
                ? $"Surplus: +R {r.NetSurplusOrDeficit:N0}"
                : $"Deficit: -R {Math.Abs(r.NetSurplusOrDeficit):N0}";
        }
        else
        {
            fiscalSummary = "No quarterly fiscal reports yet recorded";
            deficitFormatted = "Balanced";
        }

        string activeCrisis;
        string activeCrisisDetails;
        if (country.Treasury.Balance < 500_000.0)
        {
            activeCrisis = "Fiscal Insolvency Risk";
            activeCrisisDetails = $"Treasury balance ({formattedTreasury}) is below the emergency operating threshold (R 500,000). Immediate fiscal stabilization required.";
        }
        else if (dashboard.ApprovalRating < 35.0)
        {
            activeCrisis = "Civil Unrest Alert";
            activeCrisisDetails = $"Public approval ({dashboard.Approval}) has dropped below critical threshold (35%). Mass demonstrations threaten public order.";
        }
        else
        {
            activeCrisis = "None";
            activeCrisisDetails = "State institutions stable. Sovereign order maintained.";
        }

        var completedProjects = country.Projects?.Where(p => p.Completed).ToList();
        string completedSummary = (completedProjects != null && completedProjects.Count > 0)
            ? string.Join(", ", completedProjects.Select(p => p.Name))
            : "None";

        var officialsService = new ActiveOfficialsService();
        var cosRecs = officialsService.EvaluateChiefOfStaffRecommendations(country, simulationTime);
        var cosSnapshots = cosRecs.Select(r => new ChiefOfStaffRecommendationSnapshot
        {
            Id = r.Id,
            Title = r.Title,
            Summary = r.Summary,
            Category = r.Category,
            Priority = r.Priority.ToString(),
            IsUrgent = r.IsUrgent,
            Status = r.Status.ToString(),
            RecommendedAction = r.RecommendedAction,
            EstimatedCost = r.EstimatedCost,
            OutcomeMessage = r.OutcomeMessage
        }).ToList();

        var topPendingRec = cosRecs.FirstOrDefault(r => r.Status == RecommendationStatus.Pending);
        string cosBriefing = topPendingRec != null
            ? $"Urgent: {topPendingRec.Title} - {topPendingRec.Summary} Recommended: {topPendingRec.RecommendedAction}"
            : "Chief of Staff: All executive departments reporting normal operations.";

        var ministerialAdvice = officialsService.GenerateMinisterialAdvice(country, simulationTime);
        var minSnapshots = ministerialAdvice.Select(a => new MinisterialAdviceSnapshot
        {
            Portfolio = a.Portfolio.ToString(),
            MinisterName = a.MinisterName,
            IsAppointed = a.IsAppointed,
            Headline = a.Headline,
            DetailedAdvice = a.DetailedAdvice,
            RiskLevel = a.RiskLevel,
            ConfidenceRating = a.ConfidenceRating,
            CompetenceRating = a.CompetenceRating,
            LoyaltyRating = a.LoyaltyRating,
            IntegrityRating = a.IntegrityRating,
            RecommendedAction = a.RecommendedAction,
            ActionCost = a.ActionCost,
            IsSelfServing = a.IsSelfServing,
            WarningNotes = a.WarningNotes
        }).ToList();

        var secAssessment = officialsService.EvaluateSecurityLeadership(country, simulationTime);
        var secThreatSnapshots = secAssessment.Threats.Select(t => new SecurityThreatSnapshot
        {
            ThreatId = t.ThreatId,
            Title = t.Title,
            Category = t.Category,
            Severity = t.Severity.ToString(),
            Confidence = t.Confidence.ToString(),
            SourceAgency = t.SourceAgency,
            Status = t.Status,
            Details = t.Details,
            RecommendedResponse = t.RecommendedResponse,
            IsConfirmed = t.IsConfirmed
        }).ToList();

        string defconLevelText = secAssessment.Defcon switch
        {
            Republic.Core.Military.Models.DefconLevel.Defcon1_MaximumReadiness => "DEFCON 1 (Maximum Readiness)",
            Republic.Core.Military.Models.DefconLevel.Defcon2_ArmedForcesMobilized => "DEFCON 2 (Armed Forces Mobilized)",
            Republic.Core.Military.Models.DefconLevel.Defcon3_AirForceStandby => "DEFCON 3 (Air Force Standby)",
            Republic.Core.Military.Models.DefconLevel.Defcon4_HeightenedWatch => "DEFCON 4 (Heightened Watch)",
            _ => "DEFCON 5 (Peacetime Watch)"
        };

        string readinessText = $"{secAssessment.CompositeReadiness:0.0}% (Armed Forces {(secAssessment.CompositeReadiness >= 75.0 ? "Operational" : (secAssessment.CompositeReadiness >= 50.0 ? "Reduced Readiness" : "Critical Vulnerability"))})";

        // Media environment integration
        var mediaService = new MediaSimulationService();
        var mediaOutlets = mediaService.EnsureCountryMediaOutlets(country);
        mediaService.GenerateMediaReports(country, simulationTime, applySimulationImpacts: false);

        var mediaOutletSnapshots = mediaOutlets.Select(o => new MediaOutletSnapshot
        {
            Id = o.Id,
            Name = o.Name,
            Leaning = o.Leaning.ToString(),
            AudienceReach = o.AudienceReach,
            Credibility = o.Credibility,
            GovernmentAttitude = o.GovernmentAttitude,
            IsCensored = o.IsCensored,
            ConsecutiveStrikes = o.ConsecutiveStrikes
        }).ToList();

        var mediaReportSnapshots = country.MediaReports.Select(r => new MediaReportSnapshot
        {
            Id = r.Id,
            OutletId = r.OutletId,
            OutletName = r.OutletName,
            Leaning = r.Leaning.ToString(),
            Headline = r.Headline,
            ArticleSummary = r.ArticleSummary,
            Category = r.Category,
            Sentiment = r.Sentiment.ToString(),
            EventSource = r.EventSource,
            OutletCredibility = r.OutletCredibility,
            OutletReach = r.OutletReach,
            PublishedDay = r.PublishedDay,
            PublicApprovalImpact = r.PublicApprovalImpact,
            StabilityImpact = r.StabilityImpact,
            IsSuppressed = r.IsSuppressed,
            IsUndergroundLeak = r.IsUndergroundLeak,
            ResponseStatus = r.ResponseStatus.ToString()
        }).ToList();

        var topHeadline = country.MediaReports.FirstOrDefault(r => !r.IsSuppressed)?.Headline
                          ?? country.MediaReports.FirstOrDefault()?.Headline
                          ?? "No active press reports filed";
        var mediaSummaryText = $"Media Environment: {mediaOutlets.Count} Outlets Active | Top Story: {topHeadline}";

        // Private sector & investment integration
        var privateSectorService = new PrivateSectorService();
        var privateInvestors = privateSectorService.EnsureCountryPrivateInvestors(country);
        privateSectorService.GenerateInvestmentProposals(country, simulationTime);
        privateSectorService.CheckInvestmentCompletions(country, simulationTime);

        var privateInvestorSnapshots = privateInvestors.Select(i => new PrivateInvestorSnapshot
        {
            Id = i.Id,
            Name = i.Name,
            Sector = i.Sector,
            InvestorType = i.InvestorType,
            CapitalCapacity = i.CapitalCapacity,
            RiskTolerance = i.RiskTolerance.ToString(),
            GovernmentRelationship = i.GovernmentRelationship
        }).ToList();

        var investmentProposalSnapshots = country.InvestmentProposals.Select(p => new InvestmentProposalSnapshot
        {
            Id = p.Id,
            InvestorId = p.InvestorId,
            InvestorName = p.InvestorName,
            Title = p.Title,
            Sector = p.Sector,
            Location = p.Location,
            InvestorContribution = p.InvestorContribution,
            RequestedGovernmentFunding = p.RequestedGovernmentFunding,
            RequestedTaxConcession = p.RequestedTaxConcession,
            EstimatedConstructionDays = p.EstimatedConstructionDays,
            ProjectedJobsCreated = p.ProjectedJobsCreated,
            ProjectedGdpBoost = p.ProjectedGdpBoost,
            ProjectedAnnualTaxRevenue = p.ProjectedAnnualTaxRevenue,
            RisksAndConditions = p.RisksAndConditions,
            Status = p.Status.ToString(),
            CreatedAtDay = p.CreatedAtDay,
            OutcomeMessage = p.OutcomeMessage
        }).ToList();

        var pendingPropCount = country.InvestmentProposals.Count(p => p.Status == InvestmentProposalStatus.Pending);
        var activeInvCount = country.ActivePrivateInvestments.Count(p => p.IsActive);
        var investmentSummaryText = $"Private Sector: {privateInvestors.Count} Enterprises | {pendingPropCount} Pending Proposals | {activeInvCount} Active Facilities";

        return new PresidentialDeskSnapshot
        {
            CountryName = country.Name,
            CountryId = country.Id,
            TreasuryBalance = country.Treasury.Balance,
            Treasury = formattedTreasury,
            ApprovalRating = dashboard.ApprovalRating,
            Approval = dashboard.Approval,
            StabilityRating = dashboard.BaselineStability,
            Stability = dashboard.Stability,
            IncomeTaxRate = taxPolicy.IncomeTaxRate,
            IncomeTaxRateFormatted = formattedTaxRate,
            CorporateTaxRate = taxPolicy.CorporateTaxRate,
            ImportTariffRate = taxPolicy.ImportTariffRate,
            LatestDecisionResult = decisionResult,
            ActiveProject = activeProjectText,
            SimulationDay = simulationTime.DayNumber,
            SimulationTime = conciseTime,
            SimulationTimeDetailed = detailedTime,
            RulingParty = rulingPartyName,
            OppositionParty = oppositionPartyName,
            PollingSupport = pollingSupport,
            OppositionSupport = oppositionSupport,
            NextElectionDay = nextElectionDay,
            Election = electionText,
            CabinetSummary = cabinetSummary,
            FinanceMinister = financeName,
            InteriorMinister = interiorName,
            InfrastructureMinister = infraName,
            DefenseMinister = defenseName,
            CivilServiceQuality = civilQuality,
            CorruptionControl = corrControl,
            InfrastructureCondition = infraCondition,
            Cabinet = cabinetList.AsReadOnly(),
            GdpFormatted = gdpFormatted,
            FiscalReportSummary = fiscalSummary,
            FiscalDeficitFormatted = deficitFormatted,
            ActiveCrisis = activeCrisis,
            ActiveCrisisDetails = activeCrisisDetails,
            CompletedProjectsSummary = completedSummary,
            StateCapacitySummary = stateCapacitySummary,
            OfflineBriefingSummary = !string.IsNullOrWhiteSpace(country.LastOfflineCatchUpSummary)
                ? country.LastOfflineCatchUpSummary
                : "Offline Status: Real-time simulation aligned. Zero pending offline cycles.",
            DefconLevel = defconLevelText,
            MilitaryReadiness = readinessText,
            ChiefOfStaffRecommendations = cosSnapshots.AsReadOnly(),
            ChiefOfStaffBriefing = cosBriefing,
            MinisterialAdvice = minSnapshots.AsReadOnly(),
            SecurityThreats = secThreatSnapshots.AsReadOnly(),
            SecurityAssessmentSummary = secAssessment.AssessmentSummary,
            DomesticThreatIndex = secAssessment.DomesticThreatIndex,
            ExternalThreatIndex = secAssessment.ExternalThreatIndex,
            MediaOutlets = mediaOutletSnapshots.AsReadOnly(),
            MediaReports = mediaReportSnapshots.AsReadOnly(),
            MediaSummary = mediaSummaryText,
            PrivateInvestors = privateInvestorSnapshots.AsReadOnly(),
            InvestmentProposals = investmentProposalSnapshots.AsReadOnly(),
            InvestmentSummary = investmentSummaryText,
            StatusMessage = !string.IsNullOrWhiteSpace(statusMessage) ? statusMessage : "Connected to live Republic simulation state",
            ExportedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            SimulationTick = simulationTick,
            SaveFilePath = saveFilePath ?? string.Empty,
            LastAiDecision = resolvedAiDecision
        };
    }

    /// <summary>
    /// Factory helper to build a <see cref="PresidentialDeskSnapshot"/> from a world state and player country,
    /// resolving autonomous AI government decisions across registered nations.
    /// </summary>
    public static PresidentialDeskSnapshot Create(
        Republic.Core.World.WorldState world,
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? saveFilePath = null,
        string? latestDecisionResult = null,
        string? statusMessage = null,
        string? lastAiDecision = null)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(country);

        string? resolvedAiDecision = lastAiDecision;
        if (string.IsNullOrWhiteSpace(resolvedAiDecision))
        {
            var otherCountry = world.Countries?.FirstOrDefault(c => c.Id != country.Id && !string.IsNullOrWhiteSpace(c.LastAIDecision));
            resolvedAiDecision = otherCountry?.LastAIDecision;
        }

        return Create(country, simulationTime, simulationTick, saveFilePath, latestDecisionResult, statusMessage, resolvedAiDecision);
    }
}

