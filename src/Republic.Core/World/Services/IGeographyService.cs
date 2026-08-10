namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for geography, biomes, and regional sectors.
/// </summary>
public interface IGeographyService
{
    Region RegisterRegion(Region region);
    IReadOnlyList<Region> GetRegionsForCountry(string countryId);
    IReadOnlyList<Region> GetAllRegions();
}
