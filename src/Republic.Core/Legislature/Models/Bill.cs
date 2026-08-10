namespace Republic.Core.Legislature.Models;

/// <summary>
/// Domain model representing a proposed legislative bill submitted for parliamentary vote.
/// </summary>
public sealed class Bill
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double RequiredMajorityPercentage { get; init; } = 50.0; // 50.0% simple, 66.0% super-majority
    public bool IsVotedOn { get; set; }
    public bool IsPassed { get; set; }
    public bool IsVetoed { get; set; }
}
