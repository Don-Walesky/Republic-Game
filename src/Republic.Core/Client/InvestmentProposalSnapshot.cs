namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a commercial investment proposal submitted to the administration.
/// </summary>
public sealed record InvestmentProposalSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("investor_id")]
    public string InvestorId { get; init; } = string.Empty;

    [JsonPropertyName("investor_name")]
    public string InvestorName { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("sector")]
    public string Sector { get; init; } = "Manufacturing";

    [JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;

    [JsonPropertyName("investor_contribution")]
    public double InvestorContribution { get; init; }

    [JsonPropertyName("requested_government_funding")]
    public double RequestedGovernmentFunding { get; init; }

    [JsonPropertyName("requested_tax_concession")]
    public double RequestedTaxConcession { get; init; }

    [JsonPropertyName("estimated_construction_days")]
    public int EstimatedConstructionDays { get; init; }

    [JsonPropertyName("projected_jobs_created")]
    public int ProjectedJobsCreated { get; init; }

    [JsonPropertyName("projected_gdp_boost")]
    public double ProjectedGdpBoost { get; init; }

    [JsonPropertyName("projected_annual_tax_revenue")]
    public double ProjectedAnnualTaxRevenue { get; init; }

    [JsonPropertyName("risks_and_conditions")]
    public string RisksAndConditions { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Pending";

    [JsonPropertyName("created_at_day")]
    public long CreatedAtDay { get; init; }

    [JsonPropertyName("outcome_message")]
    public string OutcomeMessage { get; init; } = string.Empty;
}
