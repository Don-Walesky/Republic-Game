namespace Republic.Core.Decisions.Models;

/// <summary>
/// Domain model representing a outcome effect on simulation metrics.
/// </summary>
public sealed class PolicyEffect
{
    public string TargetMetric { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public double DeltaValue { get; set; }
    public string Description { get; set; } = string.Empty;
}
