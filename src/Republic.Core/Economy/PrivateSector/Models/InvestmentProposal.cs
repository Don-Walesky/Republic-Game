namespace Republic.Core.Economy.PrivateSector.Models;

/// <summary>
/// Status lifecycle of a private-sector investment proposal submitted to the President.
/// </summary>
public enum InvestmentProposalStatus
{
    /// <summary>
    /// Pending review in the Presidential Briefing and email inbox.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Formally accepted by the President. Public co-funding committed and construction initialized.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Formally declined by the administration.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Terms renegotiated between the administration and the investor.
    /// </summary>
    Negotiated = 3,

    /// <summary>
    /// Withdrawn by the investor due to unfavorable terms or excessive state friction.
    /// </summary>
    Withdrawn = 4,

    /// <summary>
    /// Fully built, operational, and actively generating GDP, jobs, and tax revenue.
    /// </summary>
    Completed = 5
}

/// <summary>
/// Domain model representing a formal commercial investment proposal submitted by a private-sector actor.
/// </summary>
public sealed class InvestmentProposal
{
    /// <summary>
    /// Gets the unique identifier of the proposal.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the proposing investor.
    /// </summary>
    public string InvestorId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the corporate display name of the investor.
    /// </summary>
    public string InvestorName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the title of the proposed commercial venture.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the industrial sector of the proposed project.
    /// </summary>
    public string Sector { get; init; } = "Manufacturing";

    /// <summary>
    /// Gets the target regional province or capital development zone.
    /// </summary>
    public string Location { get; init; } = "Capital Industrial District";

    /// <summary>
    /// Gets or sets the private capital committed by the investor in REPU.
    /// </summary>
    public double InvestorContribution { get; set; } = 1_000_000.0;

    /// <summary>
    /// Gets or sets the requested government co-funding contribution in REPU.
    /// </summary>
    public double RequestedGovernmentFunding { get; set; } = 300_000.0;

    /// <summary>
    /// Gets or sets the requested corporate tax concession percentage (e.g. 5.0%).
    /// </summary>
    public double RequestedTaxConcession { get; set; } = 3.0;

    /// <summary>
    /// Gets the estimated construction / commissioning duration in simulation days.
    /// </summary>
    public int EstimatedConstructionDays { get; init; } = 7;

    /// <summary>
    /// Gets the projected direct jobs created upon completion.
    /// </summary>
    public int ProjectedJobsCreated { get; init; } = 500;

    /// <summary>
    /// Gets the projected annualized boost to national Gross Domestic Product in REPU.
    /// </summary>
    public double ProjectedGdpBoost { get; init; } = 1_000_000_000.0;

    /// <summary>
    /// Gets the projected annual tax receipt expansion for the national treasury.
    /// </summary>
    public double ProjectedAnnualTaxRevenue { get; init; } = 80_000.0;

    /// <summary>
    /// Gets or sets the commercial risks, infrastructure prerequisites, or civic conditions noted in the proposal.
    /// </summary>
    public string RisksAndConditions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lifecycle status of the proposal.
    /// </summary>
    public InvestmentProposalStatus Status { get; set; } = InvestmentProposalStatus.Pending;

    /// <summary>
    /// Gets the simulation day on which this proposal was registered.
    /// </summary>
    public long CreatedAtDay { get; init; } = 1;

    /// <summary>
    /// Gets or sets the outcome message recorded upon executive decision.
    /// </summary>
    public string OutcomeMessage { get; set; } = string.Empty;
}
