namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for regional geography and biome sectors.
/// </summary>
public sealed class GeographyService : IGeographyService
{
    private readonly List<Region> _regions = new();
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public GeographyService(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Region RegisterRegion(Region region)
    {
        ArgumentNullException.ThrowIfNull(region);
        lock (_lock)
        {
            _regions.Add(region);
        }

        _logger?.LogInfo($"Region registered: '{region.Name}' (Biome: {region.Biome}, Terrain: {region.TerrainType})");
        return region;
    }

    public IReadOnlyList<Region> GetRegionsForCountry(string countryId)
    {
        lock (_lock)
        {
            return _regions.Where(r => r.CountryId == countryId).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<Region> GetAllRegions()
    {
        lock (_lock)
        {
            return _regions.ToList().AsReadOnly();
        }
    }
}
