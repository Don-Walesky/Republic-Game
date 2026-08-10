namespace Republic.Core.Decisions.Models;

/// <summary>
/// Model representing a selectable choice within a decision prompt.
/// </summary>
public sealed class DecisionOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double TreasuryCost { get; set; }
    public ulong DurationTicks { get; set; }
    public List<PolicyEffect> Effects { get; set; } = new();
}
