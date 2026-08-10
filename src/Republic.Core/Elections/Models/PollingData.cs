namespace Republic.Core.Elections.Models;

/// <summary>
/// Domain model summarizing presidential campaign polling numbers.
/// </summary>
public sealed class PollingData
{
    public double IncumbentApprovalPercentage { get; set; } = 52.0;
    public double OppositionApprovalPercentage { get; set; } = 41.0;
    public double UndecidedVotersPercentage { get; set; } = 7.0;
    public double ProjectedMargin => IncumbentApprovalPercentage - OppositionApprovalPercentage;
}
