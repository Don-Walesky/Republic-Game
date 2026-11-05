namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing an inbound global enterprise approach,
/// proposal terms, projected contributions, and status for the client dashboard.
/// </summary>
public sealed record GlobalBusinessSnapshot
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
    public int Wave { get; init; } = 1;

    [JsonPropertyName("scheduled_day")]
    public int ScheduledDay { get; init; }

    [JsonPropertyName("presenting_minister")]
    public string PresentingMinister { get; init; } = string.Empty;

    [JsonPropertyName("proposal_title")]
    public string ProposalTitle { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("capital_investment")]
    public double CapitalInvestment { get; init; }

    [JsonPropertyName("capital_investment_formatted")]
    public string CapitalInvestmentFormatted { get; init; } = string.Empty;

    [JsonPropertyName("projected_jobs")]
    public int ProjectedJobs { get; init; }

    [JsonPropertyName("quarterly_tax_contribution")]
    public double QuarterlyTaxContribution { get; init; }

    [JsonPropertyName("quarterly_tax_contribution_formatted")]
    public string QuarterlyTaxContributionFormatted { get; init; } = string.Empty;

    [JsonPropertyName("gdp_contribution")]
    public double GdpContribution { get; init; }

    [JsonPropertyName("quarterly_export_contribution")]
    public double QuarterlyExportContribution { get; init; }

    [JsonPropertyName("stock_market_confidence_bonus")]
    public double StockMarketConfidenceBonus { get; init; }

    [JsonPropertyName("foreign_reserves_inflow")]
    public double ForeignReservesInflow { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Pending";

    [JsonPropertyName("decided_on_day")]
    public int? DecidedOnDay { get; init; }

    [JsonPropertyName("decision_message")]
    public string? DecisionMessage { get; init; }
}

