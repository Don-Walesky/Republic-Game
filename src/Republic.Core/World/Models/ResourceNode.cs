namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing a natural or strategic resource node.
/// </summary>
public sealed class ResourceNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string ResourceType { get; init; } = "Iron";
    public double Abundance { get; set; } = 10000.0;
    public double ExtractionRate { get; set; } = 50.0;
    public bool IsRenewable { get; init; }
    public string LocationRegionId { get; init; } = string.Empty;
}
