namespace Republic.Core.World.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for regional geography, biome sectors, and provincial simulation.
/// </summary>
public sealed class GeographyService : IGeographyService
{
    private readonly List<Region> _regions = new();
    private readonly List<ProvinceState> _provinces = new();
    private readonly IEventBus? _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GeographyService"/> class.
    /// </summary>
    public GeographyService(IEventBus? eventBus = null, ILogger? logger = null)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeographyService"/> class with only logger.
    /// </summary>
    public GeographyService(ILogger? logger) : this(null, logger)
    {
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

    public ProvinceState RegisterProvince(ProvinceState province)
    {
        ArgumentNullException.ThrowIfNull(province);
        lock (_lock)
        {
            _provinces.Add(province);
        }

        _logger?.LogInfo($"Province registered: '{province.Name}' (Terrain: {province.Terrain}, Pop: {province.Population:N0})");
        return province;
    }

    public IReadOnlyList<ProvinceState> GetProvincesForCountry(string countryId)
    {
        lock (_lock)
        {
            return _provinces.Where(p => p.CountryId == countryId).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<ProvinceState> GetAllProvinces()
    {
        lock (_lock)
        {
            return _provinces.ToList().AsReadOnly();
        }
    }

    public async Task<bool> InvestInRegionalInfrastructureAsync(string provinceId, decimal investment, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceId);
        if (investment <= 0m) return false;

        ProvinceState? province;
        lock (_lock)
        {
            province = _provinces.FirstOrDefault(p => p.Id == provinceId);
        }

        if (province == null) return false;

        double boost = (double)investment / 10000.0;
        province.InfrastructureIndex = Math.Min(100.0, province.InfrastructureIndex + boost);
        province.ResourceOutput += investment * 0.15m;
        province.LastUpdated = DateTimeOffset.UtcNow;

        _logger?.LogInfo($"Invested {investment:C} in province '{province.Name}'. New Infrastructure Index: {province.InfrastructureIndex:0.0}");

        if (_eventBus != null)
        {
            await _eventBus.PublishAsync(new RegionalInfrastructureBuiltEvent(province.Id, province.Name, province.InfrastructureIndex, investment, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    public async Task<double> UpdateProvincialStabilityAsync(string provinceId, double delta, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceId);

        ProvinceState? province;
        lock (_lock)
        {
            province = _provinces.FirstOrDefault(p => p.Id == provinceId);
        }

        if (province == null) return 0.0;

        double prevStability = province.LocalStability;
        province.LocalStability = Math.Clamp(province.LocalStability + delta, 0.0, 100.0);
        province.RebellionRisk = Math.Max(0.0, 100.0 - province.LocalStability);
        province.LastUpdated = DateTimeOffset.UtcNow;

        _logger?.LogInfo($"Provincial stability for '{province.Name}' changed from {prevStability:0.0} to {province.LocalStability:0.0}");

        if (_eventBus != null)
        {
            await _eventBus.PublishAsync(new ProvinceStabilityChangedEvent(province.Id, province.Name, prevStability, province.LocalStability, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

            if (province.RebellionRisk > 50.0)
            {
                await _eventBus.PublishAsync(new RebellionRiskElevatedEvent(province.Id, province.Name, province.RebellionRisk, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            }
        }

        return province.LocalStability;
    }

    public async Task SimulateProvincialTurnAsync(CancellationToken cancellationToken = default)
    {
        List<ProvinceState> snapshot;
        lock (_lock)
        {
            snapshot = _provinces.ToList();
        }

        foreach (var province in snapshot)
        {
            // Population growth based on stability & terrain
            double growthRate = province.LocalStability > 60.0 ? 0.005 : -0.002;
            province.Population = (int)(province.Population * (1.0 + growthRate));

            // Drift stability toward infrastructure index
            double drift = (province.InfrastructureIndex - province.LocalStability) * 0.05;
            province.LocalStability = Math.Clamp(province.LocalStability + drift, 0.0, 100.0);
            province.RebellionRisk = Math.Max(0.0, 100.0 - province.LocalStability);
            province.LastUpdated = DateTimeOffset.UtcNow;
        }

        await Task.CompletedTask;
    }
}
