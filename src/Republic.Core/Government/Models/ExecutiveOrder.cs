namespace Republic.Core.Government.Models;

/// <summary>
/// Model representing a presidential executive order or decree.
/// </summary>
public sealed class ExecutiveOrder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public double TreasuryCost { get; set; }
    public double StabilityImpact { get; set; }
    public double PopularityImpact { get; set; }
    public bool IsIssued { get; set; }
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
}
