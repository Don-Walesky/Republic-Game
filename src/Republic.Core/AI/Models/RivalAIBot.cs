namespace Republic.Core.AI.Models;

/// <summary>
/// Domain model representing an autonomous AI rival nation leader.
/// </summary>
public sealed class RivalAIBot
{
    public string CountryId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public RivalAIBehavior Behavior { get; set; } = RivalAIBehavior.Opportunistic;
    public double AggressionIndex { get; set; } = 0.5; // 0.0 to 1.0
    public double CooperationIndex { get; set; } = 0.5; // 0.0 to 1.0
}
