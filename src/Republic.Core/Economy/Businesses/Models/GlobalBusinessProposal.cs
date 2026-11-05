namespace Republic.Core.Economy.Businesses.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Identifies whether the business approaches during the initial presidential term
/// or during the second wave following sovereign national currency creation.
/// </summary>
public enum GlobalBusinessWave
{
    FirstWave = 1,
    SecondWave = 2
}

/// <summary>
/// Status lifecycle of an inbound global enterprise proposal.
/// </summary>
public enum GlobalBusinessDecisionStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2
}

/// <summary>
/// Domain model representing an influential global commercial enterprise or industrialist
/// seeking authorization to establish commercial facilities and invest in the player's sovereign nation.
/// </summary>
public sealed class GlobalBusinessProposal
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("business_name")]
    public string BusinessName { get; init; } = string.Empty;

    [JsonPropertyName("industrialist_name")]
    public string IndustrialistName { get; init; } = string.Empty;

    [JsonPropertyName("industry")]
    public string Industry { get; init; } = "Manufacturing";

    [JsonPropertyName("wave")]
    public GlobalBusinessWave Wave { get; init; } = GlobalBusinessWave.FirstWave;

    [JsonPropertyName("scheduled_day")]
    public int ScheduledDay { get; init; } = 30;

    [JsonPropertyName("presenting_minister")]
    public string PresentingMinister { get; set; } = "Minister of Finance";

    [JsonPropertyName("proposal_title")]
    public string ProposalTitle { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("capital_investment")]
    public double CapitalInvestment { get; init; } = 15_000_000.0;

    [JsonPropertyName("projected_jobs")]
    public int ProjectedJobs { get; init; } = 2000;

    [JsonPropertyName("quarterly_tax_contribution")]
    public double QuarterlyTaxContribution { get; init; } = 3_500_000.0;

    [JsonPropertyName("gdp_contribution")]
    public double GdpContribution { get; init; } = 150_000_000.0;

    [JsonPropertyName("quarterly_export_contribution")]
    public double QuarterlyExportContribution { get; init; } = 10_000_000.0;

    [JsonPropertyName("stock_market_confidence_bonus")]
    public double StockMarketConfidenceBonus { get; init; } = 35.0;

    [JsonPropertyName("foreign_reserves_inflow")]
    public double ForeignReservesInflow { get; init; } = 8_000_000.0;

    [JsonPropertyName("status")]
    public GlobalBusinessDecisionStatus Status { get; set; } = GlobalBusinessDecisionStatus.Pending;

    [JsonPropertyName("decided_on_day")]
    public int? DecidedOnDay { get; set; }

    [JsonPropertyName("decision_message")]
    public string? DecisionMessage { get; set; }
}

