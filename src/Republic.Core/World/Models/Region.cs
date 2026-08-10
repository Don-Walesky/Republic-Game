namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing a regional geographic sector.
/// </summary>
public sealed class Region
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string CountryId { get; init; } = string.Empty;
    public string Biome { get; init; } = "Temperate";
    public string ClimateZone { get; init; } = "Humid Continental";
    public string TerrainType { get; init; } = "Plains";
    public double PopulationDensity { get; set; } = 120.0;
}
