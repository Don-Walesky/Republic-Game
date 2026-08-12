namespace Republic.Core.Demographics.Classes.Models;

/// <summary>
/// Domain model tracking approval rating and political influence weight of a demographic class.
/// </summary>
public sealed class ClassApproval
{
    public DemographicClass ClassType { get; init; }
    public double ApprovalRating { get; set; } = 70.0; // 0.0 to 100.0
    public double InfluenceWeight { get; set; } = 0.2; // 0.0 to 1.0 sum
    public double RebellionRiskIndex => Math.Max(0.0, (100.0 - ApprovalRating) * InfluenceWeight);
}
