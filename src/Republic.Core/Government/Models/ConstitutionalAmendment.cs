namespace Republic.Core.Government.Models;

/// <summary>
/// Status of a proposed constitutional law amendment in assembly.
/// </summary>
public enum ConstitutionalAmendmentStatus
{
    Drafted,
    Proposed,
    Passed,
    Rejected,
    Enacted
}

/// <summary>
/// Domain model representing a constitutional amendment, supermajority thresholds, and systemic rule changes.
/// </summary>
public sealed class ConstitutionalAmendment
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TargetLawCategory { get; init; } = "ExecutivePowers";
    public double SupermajorityRatioRequired { get; init; } = 0.66; // 2/3 supermajority by default
    public int VotesInFavor { get; set; }
    public int TotalVotesCast { get; set; }
    public ConstitutionalAmendmentStatus Status { get; set; } = ConstitutionalAmendmentStatus.Drafted;
    public DateTimeOffset ProposedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EnactedAt { get; set; }
}
