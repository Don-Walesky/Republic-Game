namespace Republic.Core.World.Models;

using System;

/// <summary>
/// Defines primary terrain classifications influencing regional productivity and defense.
/// </summary>
public enum TerrainType
{
    Plains,
    Coastal,
    Mountainous,
    Industrial,
    Arid,
    Forest
}

/// <summary>
/// Domain model representing a province or administrative territory within a country.
/// </summary>
public sealed class ProvinceState
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public TerrainType Terrain { get; set; } = TerrainType.Plains;
    public int Population { get; set; } = 250_000;
    public double LocalStability { get; set; } = 75.0;
    public double InfrastructureIndex { get; set; } = 50.0;
    public decimal ResourceOutput { get; set; } = 100_000m;
    public double RebellionRisk { get; set; } = 5.0;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
