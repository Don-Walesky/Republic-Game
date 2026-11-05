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
using Republic.Core.Government.Advisers.Models;
using Republic.Core.Government.Advisers.Services;
using Republic.Core.Government;
using Republic.Core.Government.Campaign.Services;
using System.Linq;
using Republic.Core.Identity.Models;
using Republic.Core.Media.Models;
using Republic.Core.Media.Services;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.NationalStrategy.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;

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
    /// Gets the sovereign Corruption Index percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("corruption_index")]
    public double CorruptionIndex { get; init; } = 25.0;

    /// <summary>
    /// Gets the formatted sovereign Corruption percentage string (e.g. "25.0%").
    /// </summary>
    [JsonPropertyName("corruption")]
    public string Corruption { get; init; } = "25.0%";

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
    /// Gets sensitive or high-impact media stories currently awaiting a presidential decision.
    /// </summary>
    [JsonPropertyName("pending_media_decisions")]
    public IReadOnlyList<MediaReportSnapshot> PendingMediaDecisions { get; init; } = Array.Empty<MediaReportSnapshot>();

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
    /// Gets the inbound, pending, and operational global business proposals and industrialists.
    /// </summary>
    [JsonPropertyName("global_businesses")]
    public IReadOnlyList<GlobalBusinessSnapshot> GlobalBusinesses { get; init; } = Array.Empty<GlobalBusinessSnapshot>();

    /// <summary>
    /// Gets the count of active global businesses authorized and operating in the nation.
    /// </summary>
    [JsonPropertyName("operating_businesses_count")]
    public int OperatingBusinessesCount { get; init; }

    /// <summary>
    /// Gets the count of pending inbound global business proposals awaiting presidential authorization.
    /// </summary>
    [JsonPropertyName("pending_business_proposals_count")]
    public int PendingBusinessProposalsCount { get; init; }

    /// <summary>
    /// Gets the total quarterly corporate tax revenue contributed by active global businesses in REPU.
    /// </summary>
    [JsonPropertyName("total_business_quarterly_tax")]
    public double TotalBusinessQuarterlyTax { get; init; }

    /// <summary>
    /// Gets the formatted total quarterly corporate tax revenue contributed by active global businesses.
    /// </summary>
    [JsonPropertyName("total_business_quarterly_tax_formatted")]
    public string TotalBusinessQuarterlyTaxFormatted { get; init; } = "R 0";

    /// <summary>
    /// Gets the total direct skilled and industrial jobs created by operating global businesses.
    /// </summary>
    [JsonPropertyName("total_business_jobs_created")]
    public int TotalBusinessJobsCreated { get; init; }

    /// <summary>
    /// Gets the total permanent Gross Domestic Product boost generated by operating global businesses in REPU.
    /// </summary>
    [JsonPropertyName("total_business_gdp_boost")]
    public double TotalBusinessGdpBoost { get; init; }

    /// <summary>
    /// Gets the simulation day on which the presidency began.
    /// </summary>
    [JsonPropertyName("presidency_start_day")]
    public long PresidencyStartDay { get; init; } = 1;

    /// <summary>
    /// Gets the simulation day on which sovereign currency was created/managed, unlocking Wave 2 businesses.
    /// </summary>
    [JsonPropertyName("currency_creation_day")]
    public long? CurrencyCreationDay { get; init; }

    /// <summary>
    /// Gets the comparative diplomatic profiles for peer sovereign nations, distinguishing confirmed metrics from intelligence estimates.
    /// </summary>
    [JsonPropertyName("diplomatic_profiles")]
    public IReadOnlyList<DiplomaticCountryProfileSnapshot> DiplomaticProfiles { get; init; } = Array.Empty<DiplomaticCountryProfileSnapshot>();

    /// <summary>
    /// Gets the active, pending, and inbound flexible bilateral trade proposals.
    /// </summary>
    [JsonPropertyName("diplomatic_proposals")]
    public IReadOnlyList<BilateralTradeProposalSnapshot> DiplomaticProposals { get; init; } = Array.Empty<BilateralTradeProposalSnapshot>();

    /// <summary>
    /// Gets the ratified bilateral treaties, agreements, and active recurring trade contracts.
    /// </summary>
    [JsonPropertyName("diplomatic_agreements")]
    public IReadOnlyList<DiplomaticAgreementSnapshot> DiplomaticAgreements { get; init; } = Array.Empty<DiplomaticAgreementSnapshot>();

    /// <summary>
    /// Gets the list of natural resources and goods where this nation has active production or exportable surplus.
    /// </summary>
    [JsonPropertyName("export_opportunities")]
    public IReadOnlyList<string> ExportOpportunities { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of critical economic resources that this nation does not domestically extract and requires through trade.
    /// </summary>
    [JsonPropertyName("import_needs")]
    public IReadOnlyList<string> ImportNeeds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the executive diplomacy briefing summary.
    /// </summary>
    [JsonPropertyName("diplomacy_summary")]
    public string DiplomacySummary { get; init; } = "Diplomacy: Sovereign diplomatic missions operational.";

    /// <summary>
    /// Gets the enum identifier of the active national strategic direction.
    /// </summary>
    [JsonPropertyName("strategic_direction")]
    public string StrategicDirection { get; init; } = "Industrialisation";

    /// <summary>
    /// Gets the user-friendly display name of the national strategic direction.
    /// </summary>
    [JsonPropertyName("strategic_direction_display")]
    public string StrategicDirectionDisplay { get; init; } = "Industrialisation & Manufacturing";

    /// <summary>
    /// Gets the empirical strategic progress index percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("strategic_progress_index")]
    public double StrategicProgressIndex { get; init; } = 15.0;

    /// <summary>
    /// Gets the formatted strategic progress string (e.g. "25.0%").
    /// </summary>
    [JsonPropertyName("strategic_progress")]
    public string StrategicProgress { get; init; } = "15.0%";

    /// <summary>
    /// Gets the descriptive policy focus statement for the active strategic direction.
    /// </summary>
    [JsonPropertyName("strategic_focus_statement")]
    public string StrategicFocusStatement { get; init; } = string.Empty;

    /// <summary>
    /// Gets the days remaining in the policy realignment transition period.
    /// </summary>
    [JsonPropertyName("strategic_transition_remaining")]
    public int StrategicTransitionRemaining { get; init; }

    /// <summary>
    /// Gets the priority economic and institutional sectors aligned with the active direction.
    /// </summary>
    [JsonPropertyName("strategic_priority_sectors")]
    public IReadOnlyList<string> StrategicPrioritySectors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the concise strategic direction summary.
    /// </summary>
    [JsonPropertyName("strategic_summary")]
    public string StrategicSummary { get; init; } = "Industrialisation & Manufacturing (Progress: 15.0%)";

    /// <summary>
    /// Gets the authoritative composite military progression level (1 to 30).
    /// </summary>
    [JsonPropertyName("military_level")]
    public int MilitaryLevel { get; init; } = 12;

    /// <summary>
    /// Gets the authoritative domestic counterintelligence defense rating (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("counterintelligence_level")]
    public double CounterintelligenceLevel { get; init; } = 65.0;

    /// <summary>
    /// Gets the formatted counterintelligence rating string.
    /// </summary>
    [JsonPropertyName("counterintelligence_display")]
    public string CounterintelligenceDisplay { get; init; } = "65.0% (Domestic Security Counter-Surveillance)";

    /// <summary>
    /// Gets whether Tier 1 Basic Espionage operations are currently unlocked.
    /// </summary>
    [JsonPropertyName("basic_espionage_unlocked")]
    public bool BasicEspionageUnlocked { get; init; }

    /// <summary>
    /// Gets the operational explanation or prerequisite lock reason for basic espionage.
    /// </summary>
    [JsonPropertyName("basic_espionage_lock_reason")]
    public string BasicEspionageLockReason { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether Tier 2 Advanced Espionage operations are currently unlocked.
    /// </summary>
    [JsonPropertyName("advanced_espionage_unlocked")]
    public bool AdvancedEspionageUnlocked { get; init; }

    /// <summary>
    /// Gets the operational explanation or prerequisite lock reason for advanced espionage.
    /// </summary>
    [JsonPropertyName("advanced_espionage_lock_reason")]
    public string AdvancedEspionageLockReason { get; init; } = string.Empty;

    /// <summary>
    /// Gets the executive intelligence and security summary string.
    /// </summary>
    [JsonPropertyName("intelligence_summary")]
    public string IntelligenceSummary { get; init; } = "Intelligence: Defense intelligence network operational.";

    /// <summary>
    /// Gets the active, scheduled, and past covert intelligence operations initiated by this sovereign country.
    /// </summary>
    [JsonPropertyName("active_intelligence_operations")]
    public IReadOnlyList<IntelligenceOperationSnapshot> ActiveIntelligenceOperations { get; init; } = Array.Empty<IntelligenceOperationSnapshot>();

    /// <summary>
    /// Gets the classified dossier reports gathered on foreign nations.
    /// Restricted strictly to the player's sovereign nation (never leaked to peer clients).
    /// </summary>
    [JsonPropertyName("acquired_intelligence_dossiers")]
    public IReadOnlyList<IntelligenceDossierSnapshot> AcquiredIntelligenceDossiers { get; init; } = Array.Empty<IntelligenceDossierSnapshot>();

    [JsonPropertyName("emails")]
    public IReadOnlyList<EmailSnapshot> Emails { get; init; } = Array.Empty<EmailSnapshot>();

    [JsonPropertyName("unread_email_count")]
    public int UnreadEmailCount { get; init; }

    [JsonPropertyName("unread_legislative_count")]
    public int UnreadLegislativeCount { get; init; }

    [JsonPropertyName("unread_diplomatic_count")]
    public int UnreadDiplomaticCount { get; init; }

    [JsonPropertyName("latest_legislator_whisper")]
    public string LatestLegislatorWhisper { get; init; } = string.Empty;

    [JsonPropertyName("press_secretary_name")]
    public string PressSecretaryName { get; init; } = "Elena Vance";

    [JsonPropertyName("press_briefing_headline")]
    public string PressBriefingHeadline { get; init; } = string.Empty;

    [JsonPropertyName("press_briefing_summary")]
    public string PressBriefingSummary { get; init; } = string.Empty;

    [JsonPropertyName("press_briefing_date")]
    public string PressBriefingDate { get; init; } = string.Empty;

    [JsonPropertyName("central_bank_governor_name")]
    public string CentralBankGovernorName { get; init; } = "Dr. Marcus Thorne";

    [JsonPropertyName("central_bank_interest_rate")]
    public double CentralBankInterestRate { get; init; } = 4.5;

    [JsonPropertyName("central_bank_foreign_reserves")]
    public double CentralBankForeignReserves { get; init; }

    [JsonPropertyName("inflation_rate")]
    public double InflationRate { get; init; } = 3.5;

    [JsonPropertyName("inflation_rate_formatted")]
    public string InflationRateFormatted { get; init; } = "3.5%";

    [JsonPropertyName("inflation_cost_multiplier")]
    public double InflationCostMultiplier { get; init; } = 1.0;

    [JsonPropertyName("inflation_impact_on_costs")]
    public string InflationImpactOnCosts { get; init; } = "+0.5% Cost Index";

    [JsonPropertyName("budget_revenue")]
    public double BudgetRevenue { get; init; }

    [JsonPropertyName("budget_revenue_formatted")]
    public string BudgetRevenueFormatted { get; init; } = "R 1.25B / Qtr";

    [JsonPropertyName("budget_expenditure")]
    public double BudgetExpenditure { get; init; }

    [JsonPropertyName("budget_expenditure_formatted")]
    public string BudgetExpenditureFormatted { get; init; } = "R 1.10B / Qtr";

    [JsonPropertyName("budget_balance")]
    public double BudgetBalance { get; init; }

    [JsonPropertyName("budget_balance_formatted")]
    public string BudgetBalanceFormatted { get; init; } = "+R 150M Surplus";

    [JsonPropertyName("budget_defense_allocation")]
    public double BudgetDefenseAllocation { get; init; } = 500_000_000.0;

    [JsonPropertyName("budget_healthcare_allocation")]
    public double BudgetHealthcareAllocation { get; init; } = 400_000_000.0;

    [JsonPropertyName("budget_education_allocation")]
    public double BudgetEducationAllocation { get; init; } = 350_000_000.0;

    [JsonPropertyName("budget_infrastructure_allocation")]
    public double BudgetInfrastructureAllocation { get; init; } = 600_000_000.0;

    [JsonPropertyName("budget_science_allocation")]
    public double BudgetScienceAllocation { get; init; } = 250_000_000.0;

    [JsonPropertyName("foreign_reserves")]
    public double ForeignReserves { get; init; } = 45_000_000.0;

    [JsonPropertyName("foreign_reserves_formatted")]
    public string ForeignReservesFormatted { get; init; } = "R 45.0M";

    [JsonPropertyName("forex_unlocked")]
    public bool ForexUnlocked { get; init; }

    [JsonPropertyName("forex_unlock_threshold")]
    public double ForexUnlockThreshold { get; init; } = 150_000_000.0;

    [JsonPropertyName("forex_unlock_progress_pct")]
    public double ForexUnlockProgressPct { get; init; } = 30.0;

    [JsonPropertyName("national_debt_domestic")]
    public double NationalDebtDomestic { get; init; } = 150_000_000.0;

    [JsonPropertyName("national_debt_external")]
    public double NationalDebtExternal { get; init; } = 100_000_000.0;

    [JsonPropertyName("national_debt_total")]
    public double NationalDebtTotal { get; init; } = 250_000_000.0;

    [JsonPropertyName("national_debt_total_formatted")]
    public string NationalDebtTotalFormatted { get; init; } = "R 250.0M";

    [JsonPropertyName("debt_service_quarterly")]
    public double DebtServiceQuarterly { get; init; } = 3_625_000.0;

    [JsonPropertyName("debt_service_quarterly_formatted")]
    public string DebtServiceQuarterlyFormatted { get; init; } = "R 3.6M / Qtr";

    [JsonPropertyName("stock_market_name")]
    public string StockMarketName { get; init; } = "Republic Composite Index (RCI)";

    [JsonPropertyName("stock_market_points")]
    public double StockMarketPoints { get; init; } = 2500.0;

    [JsonPropertyName("stock_market_change_pct")]
    public double StockMarketChangePct { get; init; } = 1.4;

    [JsonPropertyName("stock_market_direction")]
    public string StockMarketDirection { get; init; } = "Bullish ↗️";

    [JsonPropertyName("stock_market_sentiment")]
    public string StockMarketSentiment { get; init; } = "Moderate Expansion";

    [JsonPropertyName("currency_regime")]
    public string CurrencyRegime { get; init; } = "Sovereign Managed Float";

    [JsonPropertyName("exchange_rate_usd")]
    public double ExchangeRateToUSD { get; init; } = 1.25;

    [JsonPropertyName("bills")]
    public IReadOnlyList<CountryBillSnapshot> Bills { get; init; } = Array.Empty<CountryBillSnapshot>();

    [JsonPropertyName("pending_bill")]
    public CountryBillSnapshot? PendingBill { get; init; }

    [JsonPropertyName("player_gender")]
    public string PlayerGender { get; init; } = "Female";

    [JsonPropertyName("player_title")]
    public string PlayerTitle { get; init; } = "Madam President";

    [JsonPropertyName("player_avatar")]
    public string PlayerAvatar { get; init; } = "👩‍💼";

    [JsonPropertyName("legislature_seats_ruling")]
    public int LegislatureSeatsRuling { get; init; } = 48;

    [JsonPropertyName("legislature_seats_opposition")]
    public int LegislatureSeatsOpposition { get; init; } = 42;

    [JsonPropertyName("legislature_seats_independent")]
    public int LegislatureSeatsIndependent { get; init; } = 10;

    [JsonPropertyName("legislature_chamber_status")]
    public string LegislatureChamberStatus { get; init; } = "Hung Parliament / Minority";

    [JsonPropertyName("legislature_has_majority")]
    public bool LegislatureHasMajority { get; init; }

    [JsonPropertyName("lawmakers")]
    public IReadOnlyList<LawmakerSnapshot> Lawmakers { get; init; } = Array.Empty<LawmakerSnapshot>();

    [JsonPropertyName("military_personnel")]
    public int MilitaryPersonnel { get; init; }

    [JsonPropertyName("military_weapons_inventory")]
    public int MilitaryWeaponsInventory { get; init; }

    [JsonPropertyName("military_composite_readiness")]
    public double MilitaryCompositeReadiness { get; init; } = 80.0;

    [JsonPropertyName("military_recent_operations")]
    public IReadOnlyList<MilitaryActionSnapshot> RecentMilitaryOperations { get; init; } = Array.Empty<MilitaryActionSnapshot>();

    [JsonPropertyName("target_countries")]
    public IReadOnlyList<string> TargetCountries { get; init; } = Array.Empty<string>();

    [JsonPropertyName("onboarding_step")]
    public string OnboardingStep { get; init; } = "InOffice";

    [JsonPropertyName("selected_leadership_archetypes")]
    public IReadOnlyList<string> SelectedLeadershipArchetypes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("leadership_doctrine_title")]
    public string LeadershipDoctrineTitle { get; init; } = string.Empty;

    [JsonPropertyName("selected_development_plans")]
    public IReadOnlyList<string> SelectedDevelopmentPlans { get; init; } = Array.Empty<string>();

    [JsonPropertyName("selected_development_plan_names")]
    public IReadOnlyList<string> SelectedDevelopmentPlanNames { get; init; } = Array.Empty<string>();

    [JsonPropertyName("starting_objectives")]
    public IReadOnlyList<NationalObjective> StartingObjectives { get; init; } = Array.Empty<NationalObjective>();

    [JsonPropertyName("strategic_opportunities")]
    public IReadOnlyList<StrategicOpportunity> StrategicOpportunities { get; init; } = Array.Empty<StrategicOpportunity>();

    /// <summary>
    /// Gets the President's General Adviser (Principal Strategic Guide).
    /// </summary>
    [JsonPropertyName("general_adviser")]
    public GeneralAdviser GeneralAdviser { get; init; } = new();

    /// <summary>
    /// Gets the Presidential Press Secretary (Media & Communications).
    /// </summary>
    [JsonPropertyName("press_secretary")]
    public PresidentialPressSecretary PressSecretary { get; init; } = new();

    /// <summary>
    /// Gets the 5 Special Adviser slots (Slot 1 active, Slots 2-5 locked).
    /// </summary>
    [JsonPropertyName("special_advisers")]
    public IReadOnlyList<SpecialAdviserSlot> SpecialAdvisers { get; init; } = Array.Empty<SpecialAdviserSlot>();

    /// <summary>
    /// Gets the sovereign gems available for unlocking special adviser positions and buffs.
    /// </summary>
    [JsonPropertyName("sovereign_gems")]
    public int SovereignGems { get; init; } = 500;

    /// <summary>
    /// Gets the current presidential campaign purse in REPU.
    /// </summary>
    [JsonPropertyName("campaign_purse")]
    public decimal CampaignPurse { get; init; } = 5_000_000m;

    /// <summary>
    /// Gets the formatted presidential campaign purse string.
    /// </summary>
    [JsonPropertyName("campaign_purse_formatted")]
    public string CampaignPurseFormatted { get; init; } = "R 5,000,000 REPU";

    /// <summary>
    /// Gets the campaign polling support percentage.
    /// </summary>
    [JsonPropertyName("campaign_polling")]
    public decimal CampaignPolling { get; init; } = 35.0m;

    /// <summary>
    /// Gets the Campaign Accountant General overseeing campaign finances and compliance.
    /// </summary>
    [JsonPropertyName("campaign_accountant_general")]
    public CampaignAccountantGeneral AccountantGeneral { get; init; } = new();

    /// <summary>
    /// Gets the available campaign decisions reflecting the 70:30 consequence design principle.
    /// </summary>
    [JsonPropertyName("available_campaign_decisions")]
    public IReadOnlyList<CampaignDecision> AvailableCampaignDecisions { get; init; } = Array.Empty<CampaignDecision>();

    /// <summary>
    /// Gets the completed campaign decisions and their executed consequences.
    /// </summary>
    [JsonPropertyName("completed_campaign_decisions")]
    public IReadOnlyList<CampaignDecision> CompletedCampaignDecisions { get; init; } = Array.Empty<CampaignDecision>();

    /// <summary>
    /// Gets the competitive AI campaign opponents.
    /// </summary>
    [JsonPropertyName("campaign_ai_opponents")]
    public IReadOnlyList<AiCampaignOpponent> AiOpponents { get; init; } = Array.Empty<AiCampaignOpponent>();

    /// <summary>
    /// Gets the 5 key interest groups and their endorsement status.
    /// </summary>
    [JsonPropertyName("campaign_interest_groups")]
    public IReadOnlyList<CampaignInterestGroup> InterestGroups { get; init; } = Array.Empty<CampaignInterestGroup>();

    /// <summary>
    /// Gets recent campaign news stories and media coverage.
    /// </summary>
    [JsonPropertyName("campaign_news")]
    public IReadOnlyList<CampaignNewsItem> CampaignNews { get; init; } = Array.Empty<CampaignNewsItem>();

    /// <summary>
    /// Gets the percentage of undecided/swing voters.
    /// </summary>
    [JsonPropertyName("campaign_undecided_polling")]
    public decimal CampaignUndecidedPolling { get; init; } = 10.0m;

    /// <summary>
    /// Gets the multi-candidate popular vote breakdown on election night.
    /// </summary>
    [JsonPropertyName("election_breakdown")]
    public IReadOnlyDictionary<string, decimal> ElectionBreakdown { get; init; } = new Dictionary<string, decimal>();

    /// <summary>
    /// Gets whether national election night has been resolved.
    /// </summary>
    [JsonPropertyName("election_resolved")]
    public bool ElectionResolved { get; init; }

    /// <summary>
    /// Gets whether the presidential campaign won election night.
    /// </summary>
    [JsonPropertyName("election_won")]
    public bool ElectionWon { get; init; }

    /// <summary>
    /// Gets the final election night vote percentage tally.
    /// </summary>
    [JsonPropertyName("election_tally")]
    public decimal ElectionTally { get; init; }

    /// <summary>
    /// Gets the descriptive outcome summary of election night.
    /// </summary>
    [JsonPropertyName("election_summary")]
    public string ElectionSummary { get; init; } = string.Empty;

    [JsonPropertyName("is_registered")]
    public bool IsRegistered { get; init; } = true;

    [JsonPropertyName("is_email_verified")]
    public bool IsEmailVerified { get; init; } = true;

    [JsonPropertyName("player_display_name")]
    public string PlayerDisplayName { get; init; } = string.Empty;

    [JsonPropertyName("capital_city")]
    public string CapitalCity { get; init; } = string.Empty;

    [JsonPropertyName("geography")]
    public string Geography { get; init; } = string.Empty;

    [JsonPropertyName("demographics")]
    public string Demographics { get; init; } = string.Empty;

    [JsonPropertyName("political_culture")]
    public string PoliticalCulture { get; init; } = string.Empty;

    [JsonPropertyName("constitution")]
    public string Constitution { get; init; } = string.Empty;

    [JsonPropertyName("primary_resource")]
    public string PrimaryResource { get; init; } = string.Empty;

    [JsonPropertyName("verification_token")]
    public string VerificationToken { get; init; } = string.Empty;



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
        string? lastAiDecision = null,
        IEnumerable<Country>? worldCountries = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var dashboard = country.GetDashboard(simulationTime);



        string onboardingStep = "InOffice";

        bool isRegistered = true;

        bool isEmailVerified = true;

        string playerDisplayName = string.Empty;
        string playerGender = "Female";
        string playerTitle = "Madam President";
        string playerAvatar = "👩‍💼";
        string verificationToken = string.Empty;
        var selectedLeadership = new List<string>();
        string leadershipDoctrine = string.Empty;
        var selectedPlans = new List<string>();
        var selectedPlanNames = new List<string>();




        SaveEnvelope<FullGameState>? envelope = null;
        if (!string.IsNullOrWhiteSpace(saveFilePath) && File.Exists(saveFilePath))
        {
            try
            {
                envelope = PresidentialDeskBridge.LoadAuthoritativeState(saveFilePath);
                if (envelope?.State != null)

                {

                    onboardingStep = envelope.State.OnboardingStep.ToString();

                    if (envelope.State.Account != null)

                    {

                        isRegistered = true;

                        isEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified;
                        verificationToken = envelope.State.Account.PersonalIdentity.VerificationToken ?? string.Empty;


                        playerDisplayName = envelope.State.Account.GetPublicIdentity();
                        if (envelope.State.Account.PersonalIdentity != null)
                        {
                            var pi = envelope.State.Account.PersonalIdentity;
                            if (!string.IsNullOrWhiteSpace(pi.Gender)) playerGender = pi.Gender;
                            if (!string.IsNullOrWhiteSpace(pi.HonorificTitle)) playerTitle = pi.HonorificTitle;
                            if (!string.IsNullOrWhiteSpace(pi.AvatarSymbol)) playerAvatar = pi.AvatarSymbol;
                        }

                        if (envelope.State.Account.SelectedLeadershipArchetypes != null && envelope.State.Account.SelectedLeadershipArchetypes.Count > 0)
                        {
                            selectedLeadership = envelope.State.Account.SelectedLeadershipArchetypes.Select(a => a.ToString()).ToList();
                            leadershipDoctrine = envelope.State.Account.LeadershipDoctrineTitle;
                        }

                        if (envelope.State.Account.SelectedDevelopmentPlans != null && envelope.State.Account.SelectedDevelopmentPlans.Count > 0)
                        {
                            selectedPlans = envelope.State.Account.SelectedDevelopmentPlans.Select(p => p.ToString()).ToList();
                            selectedPlanNames = envelope.State.Account.SelectedDevelopmentPlans.Select(p => NationalDevelopmentPlanRegistry.Get(p).DisplayName).ToList();
                        }

                    }

                }

            }

            catch

            {

                // keep defaults

            }

        }



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
        var corruptionIndex = country.CorruptionIndex;
        var formattedCorruption = $"{corruptionIndex:0.0}%";
        var stateCapacitySummary = $"Civil Service: {civilQuality * 100:0.#}% | Corruption: {corruptionIndex:0.0}% (Control: {corrControl * 100:0.#}%) | Infrastructure: {infraCondition * 100:0.#}%";

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
            ResponseStatus = r.ResponseStatus.ToString(),
            RequiresDecision = r.RequiresDecision,
            PredictedEffect = r.PredictedEffect,
            AutoPublishTimerSeconds = r.AutoPublishTimerSeconds
        }).ToList();

        var pendingMediaDecisions = mediaReportSnapshots
            .Where(r => r.RequiresDecision && !r.IsSuppressed && r.ResponseStatus == "Unanswered")
            .ToList();

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

        // Multiplayer Diplomacy and Flexible Bilateral Trade
        var profileFactory = new CountryProfileFactory();
        var allWorldCountries = new List<Country>();
        if (worldCountries != null)
        {
            allWorldCountries.AddRange(worldCountries);
        }

        if (!allWorldCountries.Any(c => c.Id != country.Id))
        {
            // Fallback: seed standard peer nations if world registry has only player nation
            int[] peerSeeds = { 101, 102, 103, 104, 105 };
            foreach (var seed in peerSeeds)
            {
                var peer = profileFactory.CreateCountry(seed, simulationTime);
                allWorldCountries.Add(peer);
            }
        }

        var selfProfile = DiplomaticCountryProfile.Create(country, country, simulationTime);
        var diplomaticProfileSnapshots = new List<DiplomaticCountryProfileSnapshot>
        {
            new DiplomaticCountryProfileSnapshot
            {
                CountryId = selfProfile.CountryId,
                CountryName = selfProfile.CountryName,
                CapitalCity = selfProfile.CapitalCity,
                GovernmentType = selfProfile.GovernmentType,
                Region = selfProfile.Region,
                PrimaryResource = selfProfile.PrimaryResource,
                IsSelf = true,
                IsAuthorizedIntelligence = true,
                TreasuryDisplay = selfProfile.TreasuryDisplay,
                TreasuryIsConfirmed = true,
                GdpDisplay = selfProfile.GdpDisplay,
                ApprovalDisplay = selfProfile.ApprovalDisplay,
                ApprovalIsConfirmed = true,
                StabilityDisplay = selfProfile.StabilityDisplay,
                StabilityIsConfirmed = true,
                MilitaryReadinessDisplay = selfProfile.MilitaryReadinessDisplay,
                MilitaryIsConfirmed = true,
                ExportOpportunities = selfProfile.ExportOpportunities,
                ImportNeeds = selfProfile.ImportNeeds,
                AvailableCommodities = selfProfile.AvailableCommodities,
                DiplomaticRelationStatus = "Sovereign Republic (Self)",
                DiplomaticReputation = 100.0,
                ActiveAgreements = selfProfile.ActiveAgreements
            }
        };

        foreach (var foreignCountry in allWorldCountries.Where(c => c.Id != country.Id))
        {
            var p = DiplomaticCountryProfile.Create(country, foreignCountry, simulationTime);
            diplomaticProfileSnapshots.Add(new DiplomaticCountryProfileSnapshot
            {
                CountryId = p.CountryId,
                CountryName = p.CountryName,
                CapitalCity = p.CapitalCity,
                GovernmentType = p.GovernmentType,
                Region = p.Region,
                PrimaryResource = p.PrimaryResource,
                IsSelf = false,
                IsAuthorizedIntelligence = p.IsAuthorizedIntelligence,
                TreasuryDisplay = p.TreasuryDisplay,
                TreasuryIsConfirmed = p.TreasuryIsConfirmed,
                GdpDisplay = p.GdpDisplay,
                ApprovalDisplay = p.ApprovalDisplay,
                ApprovalIsConfirmed = p.ApprovalIsConfirmed,
                StabilityDisplay = p.StabilityDisplay,
                StabilityIsConfirmed = p.StabilityIsConfirmed,
                MilitaryReadinessDisplay = p.MilitaryReadinessDisplay,
                MilitaryIsConfirmed = p.MilitaryIsConfirmed,
                ExportOpportunities = p.ExportOpportunities,
                ImportNeeds = p.ImportNeeds,
                AvailableCommodities = p.AvailableCommodities,
                DiplomaticRelationStatus = p.DiplomaticRelationStatus,
                DiplomaticReputation = p.DiplomaticReputation,
                ActiveAgreements = p.ActiveAgreements
            });
        }

        var tradeProposalSnapshots = country.FlexibleTradeProposals.Select(tp =>
        {
            var proposer = allWorldCountries.FirstOrDefault(c => c.Id == tp.ProposerCountryId) ?? (tp.ProposerCountryId == country.Id ? country : null);
            var target = allWorldCountries.FirstOrDefault(c => c.Id == tp.TargetCountryId) ?? (tp.TargetCountryId == country.Id ? country : null);
            var proposerName = proposer?.Name ?? tp.ProposerCountryId;
            var targetName = target?.Name ?? tp.TargetCountryId;

            return new BilateralTradeProposalSnapshot
            {
                Id = tp.Id,
                ProposerCountryId = tp.ProposerCountryId,
                ProposerCountryName = proposerName,
                TargetCountryId = tp.TargetCountryId,
                TargetCountryName = targetName,
                OfferedResourceType = tp.OfferedResourceType,
                OfferedQuantity = tp.OfferedQuantity,
                RequestedResourceType = tp.RequestedResourceType,
                RequestedQuantity = tp.RequestedQuantity,
                CurrencyPayment = tp.CurrencyPayment,
                CurrencyPaymentFormatted = RepuTreasury.Format(Math.Abs(tp.CurrencyPayment)),
                IsRecurring = tp.IsRecurring,
                TotalDeliveryCycles = tp.TotalDeliveryCycles,
                RemainingDeliveryCycles = tp.RemainingDeliveryCycles,
                RevisionNumber = tp.RevisionNumber,
                LastModifiedByCountryId = tp.LastModifiedByCountryId,
                CanAccept = tp.CanBeAcceptedBy(country.Id),
                CanCounter = tp.Involves(country.Id) && tp.Status is FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered,
                CanReject = tp.Involves(country.Id) && tp.Status is FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered,
                Status = tp.Status.ToString(),
                Summary = tp.FormatSummary(),
                CreatedAt = tp.CreatedAt.ToString(),
                StatusMessage = tp.StatusMessage
            };
        }).ToList();

        var diplomaticAgreementSnapshots = country.Agreements.Select(a =>
        {
            var partnerId = string.Equals(a.SourceCountryId, country.Id, StringComparison.OrdinalIgnoreCase) ? a.TargetCountryId : a.SourceCountryId;
            var partner = allWorldCountries.FirstOrDefault(c => c.Id == partnerId);
            var partnerName = partner?.Name ?? partnerId;

            return new DiplomaticAgreementSnapshot
            {
                Id = a.Id,
                PartnerCountryId = partnerId,
                PartnerCountryName = partnerName,
                AgreementType = a.AgreementType.ToString(),
                Status = a.Status.ToString(),
                ProposedBoundary = a.ProposedBoundary.ToString(),
                DecidedBoundary = a.DecidedBoundary?.ToString() ?? "Pending",
                Summary = a.ToString()
            };
        }).ToList();

        var activeTreatyCount = diplomaticAgreementSnapshots.Count(a => a.Status is "Accepted" or "Completed");
        var pendingDealCount = tradeProposalSnapshots.Count(p => p.Status is "Pending" or "Countered");
        var diplomacySummaryText = $"Diplomacy: {diplomaticProfileSnapshots.Count - 1} Foreign Nations Registered | {pendingDealCount} Active Proposals | {activeTreatyCount} Ratified Treaties";

        // Strategic Direction integration
        var strategyService = new NationalStrategyService();
        var strategyProfile = strategyService.EnsureStrategyProfile(country, simulationTime);
        strategyService.UpdateStrategicProgress(country, simulationTime);

        // Intelligence & Espionage progression integration
        var intelProgService = new IntelligenceProgressionService();
        intelProgService.DegradeIntelligenceFreshness(country, simulationTime);
        var (basicUnlocked, basicReason, _) = intelProgService.CheckOperationPrerequisites(country, IntelligenceOperationType.StrategicIntelligence);
        var (advUnlocked, advReason, _) = intelProgService.CheckOperationPrerequisites(country, IntelligenceOperationType.AdvancedEspionage);

        var intelOpSnapshots = country.IntelligenceOperations.Select(op => new IntelligenceOperationSnapshot
        {
            Id = op.Id,
            InitiatorCountryId = op.InitiatorCountryId,
            TargetCountryId = op.TargetCountryId,
            TargetCountryName = op.TargetCountryName,
            OperationType = op.Type.ToString(),
            Title = op.Title,
            FinancialCost = op.FinancialCost,
            FinancialCostFormatted = RepuTreasury.Format(op.FinancialCost),
            RequiredMilitaryLevel = op.RequiredMilitaryLevel,
            Status = op.Status.ToString(),
            RiskRating = op.RiskRating,
            SuccessProbability = op.SuccessProbability,
            IsDetected = op.IsDetected,
            OutcomeMessage = op.OutcomeMessage
        }).ToList();

        var dossierSnapshots = country.AcquiredIntelligence.Select(d => new IntelligenceDossierSnapshot
        {
            Id = d.Id,
            TargetCountryId = d.TargetCountryId,
            TargetCountryName = d.TargetCountryName,
            OperationType = d.OperationType.ToString(),
            Title = d.Title,
            AcquiredDay = d.AcquiredDay,
            FreshnessDaysRemaining = d.FreshnessDaysRemaining,
            Confidence = d.Confidence.ToString(),
            ReportSummary = d.ReportSummary,
            ConfirmedTreasury = d.ConfirmedTreasury,
            EstimatedTreasury = d.EstimatedTreasury,
            ConfirmedMilitaryReadiness = d.ConfirmedMilitaryReadiness,
            EstimatedMilitaryReadiness = d.EstimatedMilitaryReadiness,
            ConfirmedGdp = d.ConfirmedGdp,
            EstimatedGdp = d.EstimatedGdp,
            DiscoveredProjects = d.DiscoveredProjects.AsReadOnly(),
            DiscoveredVulnerabilities = d.DiscoveredVulnerabilities.AsReadOnly()
        }).ToList();

        var activeOpsCount = intelOpSnapshots.Count(o => o.Status is "Preparing" or "InProgress");
        var intelSummaryText = $"Intelligence: Level {country.MilitaryLevel} Armed Forces | {activeOpsCount} Active Operations | {dossierSnapshots.Count} Acquired Dossiers";

        // Legislature chamber & lawmakers integration
        var chamber = country.Legislature ?? new Republic.Core.Legislature.Models.ParliamentaryChamber();
        chamber.InitializeDefaultLawmakers();

        var lawmakerSnapshots = chamber.Lawmakers.Select(l => new LawmakerSnapshot
        {
            Id = l.Id,
            Name = l.Name,
            Constituency = l.Constituency,
            OriginalParty = l.OriginalParty,
            CurrentParty = l.CurrentParty,
            SeatWeight = l.SeatWeight,
            Stance = l.Stance,
            Loyalty = l.LoyaltyToOriginal,
            HasDefected = l.HasDefectedToRulingParty,
            DefectionIncentive = l.DefectionIncentive
        }).ToList();

        // Bills and legislative integration
        var billSnapshots = country.Bills.Select(b => new CountryBillSnapshot
        {
            Title = b.Title,
            Status = b.Status.ToString(),
            Description = b.Description,
            Category = b.Category,
            PositiveConsequence = b.PositiveConsequence,
            NegativeConsequence = b.NegativeConsequence,
            GovernancePrinciple = b.GovernancePrinciple,
            RequiredMajorityPercentage = b.RequiredMajorityPercentage,
            VetoThresholdPercentage = b.VetoThresholdPercentage,
            TotalVotes = b.TotalVotes,
            AyesCount = b.AyesCount,
            NaysCount = b.NaysCount,
            IsPassed = b.IsPassed,
            IsVetoed = b.IsVetoed,
            VetoOverridden = b.VetoOverridden,
            VotedBoundary = b.VotedBoundary?.ToString()
        }).ToList();

        CountryBillSnapshot? pendingBillSnapshot = null;
        if (country.PendingBill != null)
        {
            var pb = country.PendingBill;
            pendingBillSnapshot = new CountryBillSnapshot
            {
                Title = pb.Title,
                Status = pb.Status.ToString(),
                Description = pb.Description,
                Category = pb.Category,
                PositiveConsequence = pb.PositiveConsequence,
                NegativeConsequence = pb.NegativeConsequence,
                GovernancePrinciple = pb.GovernancePrinciple,
                RequiredMajorityPercentage = pb.RequiredMajorityPercentage,
                VetoThresholdPercentage = pb.VetoThresholdPercentage,
                TotalVotes = pb.TotalVotes,
                AyesCount = pb.AyesCount,
                NaysCount = pb.NaysCount,
                IsPassed = pb.IsPassed,
                IsVetoed = pb.IsVetoed,
                VetoOverridden = pb.VetoOverridden,
                VotedBoundary = pb.VotedBoundary?.ToString()
            };
        }

        // Executive Emails & Messaging integration
        var emailList = new List<EmailSnapshot>();
        Republic.Core.Communication.Services.InGameCommunicationService.EnsureDefaultCommunications(country, envelope?.State);

        if (envelope?.State?.Workspace?.Emails != null && envelope.State.Workspace.Emails.Count > 0)
        {
            foreach (var em in envelope.State.Workspace.Emails)
            {
                emailList.Add(new EmailSnapshot
                {
                    Id = em.Id,
                    Sender = em.Sender,
                    SenderTitle = em.SenderTitle,
                    Recipient = em.Recipient,
                    Subject = em.Subject,
                    Body = em.Body,
                    DateSent = em.DateSent.ToString("MMM dd, HH:mm"),
                    IsRead = em.IsRead,
                    Folder = em.Folder,
                    Channel = string.IsNullOrWhiteSpace(em.Channel) ? "Executive" : em.Channel,
                    Urgency = string.IsNullOrWhiteSpace(em.Urgency) ? "Routine" : em.Urgency,
                    ActionRequired = em.ActionRequired,
                    ActionType = em.ActionType,
                    RelatedEntityId = em.RelatedEntityId,
                    AvatarSymbol = string.IsNullOrWhiteSpace(em.AvatarSymbol) ? "✉️" : em.AvatarSymbol
                });
            }
        }
        else
        {
            emailList.Add(new EmailSnapshot
            {
                Id = "mail_speaker_1",
                Sender = "Speaker of the House",
                SenderTitle = "Speaker's Chambers",
                Recipient = "Presidential Desk",
                Subject = "Legislative Calendar & Parliamentary Order",
                Body = "Mr. President, the National Assembly stands ready for executive statutory proposals. Every bill undergoes rigorous committee review and roll-call voting.",
                DateSent = conciseTime,
                IsRead = false,
                Folder = "Inbox",
                Channel = "Legislative",
                Urgency = "Routine",
                ActionRequired = country.PendingBill != null,
                ActionType = "view_chamber",
                AvatarSymbol = "🏛️"
            });
        }

        int unreadEmailCount = emailList.Count(e => !e.IsRead);
        int unreadLegCount = emailList.Count(e => !e.IsRead && e.Channel.Equals("Legislative", StringComparison.OrdinalIgnoreCase));
        int unreadDipCount = emailList.Count(e => !e.IsRead && e.Channel.Equals("Diplomatic", StringComparison.OrdinalIgnoreCase));
        string latestWhisper = emailList.FirstOrDefault(e => e.Channel.Equals("Legislative", StringComparison.OrdinalIgnoreCase))?.Body
            ?? "Legislative order calm. Assembly in regular session.";

        string pressHeadline = country.MediaReports.FirstOrDefault(r => !r.IsSuppressed)?.Headline ?? "Executive Administration Advances National Strategic Priorities";
        string pressSummary = country.MediaReports.FirstOrDefault(r => !r.IsSuppressed)?.ArticleSummary ?? "The Presidential Administration affirmed today's focus on fiscal stabilization, infrastructure expansion, and regional diplomatic stability.";
        var foreignNames = allWorldCountries.Where(c => c.Id != country.Id).Select(c => c.Name).ToList();

        AdviserService.SynchronizeAdvisers(country);
        CampaignService.InitializeCampaign(country);

        // Global Businesses & Inbound Industrialists integration
        var globalBizService = new Republic.Core.Economy.Businesses.Services.GlobalBusinessService();
        globalBizService.UpdateBusinessApproaches(country, simulationTime);

        var globalBizSnapshots = country.GlobalBusinesses.Select(b => new GlobalBusinessSnapshot
        {
            Id = b.Id,
            BusinessName = b.BusinessName,
            IndustrialistName = b.IndustrialistName,
            Industry = b.Industry,
            Wave = (int)b.Wave,
            ScheduledDay = b.ScheduledDay,
            PresentingMinister = b.PresentingMinister,
            ProposalTitle = b.ProposalTitle,
            Description = b.Description,
            CapitalInvestment = b.CapitalInvestment,
            CapitalInvestmentFormatted = RepuTreasury.Format(b.CapitalInvestment),
            ProjectedJobs = b.ProjectedJobs,
            QuarterlyTaxContribution = b.QuarterlyTaxContribution,
            QuarterlyTaxContributionFormatted = RepuTreasury.Format(b.QuarterlyTaxContribution),
            GdpContribution = b.GdpContribution,
            QuarterlyExportContribution = b.QuarterlyExportContribution,
            StockMarketConfidenceBonus = b.StockMarketConfidenceBonus,
            ForeignReservesInflow = b.ForeignReservesInflow,
            Status = b.Status.ToString(),
            DecidedOnDay = b.DecidedOnDay,
            DecisionMessage = b.DecisionMessage
        }).ToList();

        var opBizCount = country.GlobalBusinesses.Count(b => b.Status == Republic.Core.Economy.Businesses.Models.GlobalBusinessDecisionStatus.Accepted);
        var pendBizCount = country.GlobalBusinesses.Count(b => b.Status == Republic.Core.Economy.Businesses.Models.GlobalBusinessDecisionStatus.Pending);
        var totBizTax = country.GlobalBusinesses
            .Where(b => b.Status == Republic.Core.Economy.Businesses.Models.GlobalBusinessDecisionStatus.Accepted)
            .Sum(b => b.QuarterlyTaxContribution);
        var totBizJobs = country.GlobalBusinesses
            .Where(b => b.Status == Republic.Core.Economy.Businesses.Models.GlobalBusinessDecisionStatus.Accepted)
            .Sum(b => b.ProjectedJobs);
        var totBizGdp = country.GlobalBusinesses
            .Where(b => b.Status == Republic.Core.Economy.Businesses.Models.GlobalBusinessDecisionStatus.Accepted)
            .Sum(b => b.GdpContribution);

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
            CorruptionIndex = corruptionIndex,
            Corruption = formattedCorruption,
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
            PendingMediaDecisions = pendingMediaDecisions.AsReadOnly(),
            MediaSummary = mediaSummaryText,
            PrivateInvestors = privateInvestorSnapshots.AsReadOnly(),
            InvestmentProposals = investmentProposalSnapshots.AsReadOnly(),
            InvestmentSummary = investmentSummaryText,
            GlobalBusinesses = globalBizSnapshots.AsReadOnly(),
            OperatingBusinessesCount = opBizCount,
            PendingBusinessProposalsCount = pendBizCount,
            TotalBusinessQuarterlyTax = totBizTax,
            TotalBusinessQuarterlyTaxFormatted = $"{RepuTreasury.Format(totBizTax)} / Qtr",
            TotalBusinessJobsCreated = totBizJobs,
            TotalBusinessGdpBoost = totBizGdp,
            PresidencyStartDay = country.PresidencyStartDay,
            CurrencyCreationDay = country.CurrencyCreationDay,
            DiplomaticProfiles = diplomaticProfileSnapshots.AsReadOnly(),
            DiplomaticProposals = tradeProposalSnapshots.AsReadOnly(),
            DiplomaticAgreements = diplomaticAgreementSnapshots.AsReadOnly(),
            ExportOpportunities = selfProfile.ExportOpportunities,
            ImportNeeds = selfProfile.ImportNeeds,
            DiplomacySummary = diplomacySummaryText,
            StrategicDirection = strategyProfile.Direction.ToString(),
            StrategicDirectionDisplay = strategyProfile.Direction.GetDisplayName(),
            StrategicProgressIndex = strategyProfile.ProgressIndex,
            StrategicProgress = $"{strategyProfile.ProgressIndex:0.0}%",
            StrategicFocusStatement = strategyProfile.FocusStatement,
            StrategicTransitionRemaining = strategyProfile.TransitionDaysRemaining,
            StrategicPrioritySectors = strategyProfile.PrioritySectors.AsReadOnly(),
            StrategicSummary = strategyProfile.FormatSummary(),
            MilitaryLevel = country.MilitaryLevel,
            CounterintelligenceLevel = Math.Round(country.CounterintelligenceLevel, 1),
            CounterintelligenceDisplay = $"{country.CounterintelligenceLevel:0.0}% (Domestic Counter-Surveillance)",
            BasicEspionageUnlocked = basicUnlocked,
            BasicEspionageLockReason = basicReason,
            AdvancedEspionageUnlocked = advUnlocked,
            AdvancedEspionageLockReason = advReason,
            IntelligenceSummary = intelSummaryText,
            ActiveIntelligenceOperations = intelOpSnapshots.AsReadOnly(),
            AcquiredIntelligenceDossiers = dossierSnapshots.AsReadOnly(),
            Emails = emailList.AsReadOnly(),
            UnreadEmailCount = unreadEmailCount,
            UnreadLegislativeCount = unreadLegCount,
            UnreadDiplomaticCount = unreadDipCount,
            LatestLegislatorWhisper = latestWhisper,
            PressSecretaryName = "Elena Vance",
            PressBriefingHeadline = pressHeadline,
            PressBriefingSummary = pressSummary,
            PressBriefingDate = conciseTime,
            CentralBankGovernorName = country.CentralBank?.GovernorName ?? "Dr. Marcus Thorne",
            CentralBankInterestRate = country.CentralBank?.BenchmarkInterestRate ?? 4.5,
            CentralBankForeignReserves = country.CentralBank?.ForeignReserves ?? (country.Treasury.Balance * 0.45),
            InflationRate = country.InflationRate,
            InflationRateFormatted = $"{country.InflationRate:0.1}%",
            InflationCostMultiplier = country.GetInflationCostMultiplier(),
            InflationImpactOnCosts = $"+{Math.Max(0.0, country.InflationRate - 3.0):0.1}% Project Cost Index",
            BudgetRevenue = Math.Round((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate), 2),
            BudgetRevenueFormatted = $"{RepuTreasury.Format((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate))} / Qtr",
            BudgetExpenditure = Math.Round((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0), 2),
            BudgetExpenditureFormatted = $"{RepuTreasury.Format((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0))} / Qtr",
            BudgetBalance = Math.Round(((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate)) - ((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0)), 2),
            BudgetBalanceFormatted = (((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate)) - ((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0)) >= 0 ? "+" : "") + $"{RepuTreasury.Format(((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate)) - ((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0)))} {(((country.GrossDomesticProduct * 0.15 * taxPolicy.IncomeTaxRate) + (country.GrossDomesticProduct * 0.10 * taxPolicy.CorporateTaxRate) + (Math.Max(0.0, country.TradeBalance) * taxPolicy.ImportTariffRate)) - ((country.MinistryBudget?.TotalQuarterlyExpenditures ?? 2_100_000_000.0) + (country.Debt?.QuarterlyDebtService ?? 0.0)) >= 0 ? "Surplus" : "Deficit")}",
            BudgetDefenseAllocation = country.MinistryBudget?.DefenseAllocation ?? 500_000_000.0,
            BudgetHealthcareAllocation = country.MinistryBudget?.HealthcareAllocation ?? 400_000_000.0,
            BudgetEducationAllocation = country.MinistryBudget?.EducationAllocation ?? 350_000_000.0,
            BudgetInfrastructureAllocation = country.MinistryBudget?.InfrastructureAllocation ?? 600_000_000.0,
            BudgetScienceAllocation = country.MinistryBudget?.ScienceAllocation ?? 250_000_000.0,
            ForeignReserves = country.CentralBank?.ForeignReserves ?? 45_000_000.0,
            ForeignReservesFormatted = RepuTreasury.Format(country.CentralBank?.ForeignReserves ?? 45_000_000.0),
            ForexUnlocked = country.CentralBank?.ForexUnlocked ?? false,
            ForexUnlockThreshold = 150_000_000.0,
            ForexUnlockProgressPct = Math.Clamp(((country.CentralBank?.ForeignReserves ?? 45_000_000.0) / 150_000_000.0) * 100.0, 0.0, 100.0),
            NationalDebtDomestic = country.Debt?.DomesticDebt ?? 150_000_000.0,
            NationalDebtExternal = country.Debt?.ExternalDebt ?? 100_000_000.0,
            NationalDebtTotal = country.Debt?.TotalDebt ?? 250_000_000.0,
            NationalDebtTotalFormatted = RepuTreasury.Format(country.Debt?.TotalDebt ?? 250_000_000.0),
            DebtServiceQuarterly = country.Debt?.QuarterlyDebtService ?? 3_625_000.0,
            DebtServiceQuarterlyFormatted = $"{RepuTreasury.Format(country.Debt?.QuarterlyDebtService ?? 3_625_000.0)} / Qtr",
            StockMarketName = country.StockMarket?.IndexName ?? "Republic Composite Index (RCI)",
            StockMarketPoints = country.StockMarket?.CurrentPoints ?? 2500.0,
            StockMarketChangePct = country.StockMarket?.ChangePercent ?? 0.0,
            StockMarketDirection = country.StockMarket?.Direction ?? "Bullish ↗️",
            StockMarketSentiment = country.StockMarket?.Sentiment ?? "Moderate Expansion",
            CurrencyRegime = country.CentralBank?.CurrencyRegime ?? "Sovereign Managed Float",
            ExchangeRateToUSD = country.CentralBank?.ExchangeRateToUSD ?? 1.25,
            Bills = billSnapshots.AsReadOnly(),
            PendingBill = pendingBillSnapshot,
            PlayerGender = playerGender,
            PlayerTitle = playerTitle,
            PlayerAvatar = playerAvatar,
            LegislatureSeatsRuling = chamber.RulingPartySeats,
            LegislatureSeatsOpposition = chamber.OppositionSeats,
            LegislatureSeatsIndependent = chamber.IndependentSeats,
            LegislatureChamberStatus = chamber.ChamberStatus,
            LegislatureHasMajority = chamber.HasWorkingMajority,
            Lawmakers = lawmakerSnapshots.AsReadOnly(),
            MilitaryPersonnel = country.MilitaryLevel * 2500,
            MilitaryWeaponsInventory = country.MilitaryLevel * 85,
            MilitaryCompositeReadiness = secAssessment.CompositeReadiness,
            TargetCountries = foreignNames.AsReadOnly(),
            StatusMessage = !string.IsNullOrWhiteSpace(statusMessage) ? statusMessage : "Connected to live Republic simulation state",
            ExportedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            SimulationTick = simulationTick,
            SaveFilePath = saveFilePath ?? string.Empty,

            OnboardingStep = onboardingStep,

            IsRegistered = isRegistered,

            IsEmailVerified = isEmailVerified,

            PlayerDisplayName = playerDisplayName,

            CapitalCity = country.CapitalCity,

            Geography = country.Profile?.Region.ToString() ?? "Temperate Coast",

            Demographics = country.Profile != null ? $"{country.Profile.Population:N0} citizens ({country.Profile.DominantLanguage})" : string.Empty,

            PoliticalCulture = country.Profile?.PoliticalCulture ?? string.Empty,

            Constitution = country.Profile?.ConstitutionName ?? "National Democratic Constitution",

            PrimaryResource = country.Profile?.PrimaryResource ?? "Mineral Reserves",
            VerificationToken = verificationToken,
            SelectedLeadershipArchetypes = selectedLeadership.AsReadOnly(),
            LeadershipDoctrineTitle = leadershipDoctrine,
            SelectedDevelopmentPlans = selectedPlans.AsReadOnly(),
            SelectedDevelopmentPlanNames = selectedPlanNames.AsReadOnly(),
            StartingObjectives = country.StartingObjectives != null ? country.StartingObjectives.ToList().AsReadOnly() : Array.Empty<NationalObjective>(),
            StrategicOpportunities = country.Opportunities != null ? country.Opportunities.ToList().AsReadOnly() : Array.Empty<StrategicOpportunity>(),
            GeneralAdviser = country.GeneralAdviser ?? new GeneralAdviser(),
            PressSecretary = country.PressSecretary ?? new PresidentialPressSecretary(),
            SpecialAdvisers = country.SpecialAdvisers != null ? country.SpecialAdvisers.ToList().AsReadOnly() : SpecialAdviserSlot.CreateDefaultRoster().AsReadOnly(),
            SovereignGems = country.SovereignGems,
            CampaignPurse = country.CampaignOffice?.Purse ?? 5_000_000m,
            CampaignPurseFormatted = $"R {country.CampaignOffice?.Purse ?? 5_000_000m:N0} REPU",
            CampaignPolling = country.CampaignOffice?.Polling ?? 35.0m,
            AccountantGeneral = country.CampaignOffice?.AccountantGeneral ?? new CampaignAccountantGeneral(),
            AvailableCampaignDecisions = country.CampaignOffice?.AvailableDecisions != null ? country.CampaignOffice.AvailableDecisions.ToList().AsReadOnly() : Array.Empty<CampaignDecision>(),
            CompletedCampaignDecisions = country.CampaignOffice?.CompletedDecisions != null ? country.CampaignOffice.CompletedDecisions.ToList().AsReadOnly() : Array.Empty<CampaignDecision>(),
            ElectionResolved = country.CampaignOffice?.ElectionResolved ?? false,
            ElectionWon = country.CampaignOffice?.ElectionWon ?? false,
            ElectionTally = country.CampaignOffice?.ElectionTally ?? 0m,
            ElectionSummary = country.CampaignOffice?.ElectionSummary ?? string.Empty,

            AiOpponents = country.CampaignOffice?.AiOpponents != null ? country.CampaignOffice.AiOpponents.ToList().AsReadOnly() : Array.Empty<AiCampaignOpponent>(),
            InterestGroups = country.CampaignOffice?.InterestGroups != null ? country.CampaignOffice.InterestGroups.ToList().AsReadOnly() : Array.Empty<CampaignInterestGroup>(),
            CampaignNews = country.CampaignOffice?.CampaignNews != null ? country.CampaignOffice.CampaignNews.ToList().AsReadOnly() : Array.Empty<CampaignNewsItem>(),
            CampaignUndecidedPolling = country.CampaignOffice?.UndecidedPolling ?? 10.0m,
            ElectionBreakdown = country.CampaignOffice?.ElectionBreakdown != null ? country.CampaignOffice.ElectionBreakdown : new Dictionary<string, decimal>(),

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

        return Create(country, simulationTime, simulationTick, saveFilePath, latestDecisionResult, statusMessage, resolvedAiDecision, world.Countries);
    }
}

