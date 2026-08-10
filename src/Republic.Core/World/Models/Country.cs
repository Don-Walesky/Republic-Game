namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing a sovereign nation entity.
/// </summary>
public sealed class Country
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string CapitalCity { get; init; } = string.Empty;
    public string GovernmentType { get; set; } = "Democratic Republic";
    public double TerritorySizeSqKm { get; init; } = 500000;
    public List<string> NationalTraits { get; init; } = new();
    public double BaselineStability { get; set; } = 75.0;
}
