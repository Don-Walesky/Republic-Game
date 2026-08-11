namespace Republic.Core.World.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for geography, biomes, regional sectors, and provincial administration.
/// </summary>
public interface IGeographyService
{
    Region RegisterRegion(Region region);
    IReadOnlyList<Region> GetRegionsForCountry(string countryId);
    IReadOnlyList<Region> GetAllRegions();

    ProvinceState RegisterProvince(ProvinceState province);
    IReadOnlyList<ProvinceState> GetProvincesForCountry(string countryId);
    IReadOnlyList<ProvinceState> GetAllProvinces();
    Task<bool> InvestInRegionalInfrastructureAsync(string provinceId, decimal investment, CancellationToken cancellationToken = default);
    Task<double> UpdateProvincialStabilityAsync(string provinceId, double delta, CancellationToken cancellationToken = default);
    Task SimulateProvincialTurnAsync(CancellationToken cancellationToken = default);
}
