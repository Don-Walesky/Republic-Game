namespace Republic.Core.Government.Models;

/// <summary>
/// Domain model representing a constitutional reform or systemic government overhaul.
/// </summary>
public sealed class ConstitutionalReform
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public GovernmentType TargetSystem { get; init; } = GovernmentType.PresidentialRepublic;
    public double ExecutivePowerIndex { get; init; } = 0.7; // 0.0 to 1.0
    public double StabilityBonus { get; init; } = 5.0;
    public double ApprovalBonus { get; init; } = 8.0;
}
