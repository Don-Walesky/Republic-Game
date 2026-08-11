namespace Republic.Core.Tests.World;

using System;
using System.Threading.Tasks;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;
using Republic.Core.World.Services;
using Xunit;

public sealed class GeographyServiceTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());

    [Fact]
    public void RegisterProvince_AddsProvinceToRegistry()
    {
        var service = new GeographyService(_eventBus);
        var province = new ProvinceState
        {
            Name = "Aethelgard",
            CountryId = "Arcadia",
            Terrain = TerrainType.Mountainous,
            Population = 350000
        };

        var registered = service.RegisterProvince(province);

        Assert.NotNull(registered);
        var list = service.GetProvincesForCountry("Arcadia");
        Assert.Single(list);
        Assert.Equal("Aethelgard", list[0].Name);
        Assert.Equal(TerrainType.Mountainous, list[0].Terrain);
    }

    [Fact]
    public async Task InvestInRegionalInfrastructureAsync_IncreasesIndexAndOutput()
    {
        var service = new GeographyService(_eventBus);
        var province = service.RegisterProvince(new ProvinceState
        {
            Name = "Valoria Coast",
            CountryId = "Arcadia",
            Terrain = TerrainType.Coastal,
            InfrastructureIndex = 40.0,
            ResourceOutput = 50000m
        });

        bool infraEventFired = false;
        _eventBus.Subscribe<RegionalInfrastructureBuiltEvent>((e, _) =>
        {
            if (e.ProvinceId == province.Id) infraEventFired = true;
            return ValueTask.CompletedTask;
        });

        bool success = await service.InvestInRegionalInfrastructureAsync(province.Id, 250000m);
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.True(success);
        Assert.True(province.InfrastructureIndex > 40.0);
        Assert.True(province.ResourceOutput > 50000m);
        Assert.True(infraEventFired);
    }

    [Fact]
    public async Task UpdateProvincialStabilityAsync_UpdatesStabilityAndRebellionRisk()
    {
        var service = new GeographyService(_eventBus);
        var province = service.RegisterProvince(new ProvinceState
        {
            Name = "Ironmarch",
            CountryId = "Arcadia",
            LocalStability = 80.0
        });

        double newStability = await service.UpdateProvincialStabilityAsync(province.Id, -45.0);

        Assert.Equal(35.0, newStability);
        Assert.Equal(65.0, province.RebellionRisk);
    }

    [Fact]
    public async Task UpdateProvincialStabilityAsync_EmitsRebellionRiskEvent_WhenStabilityDropsLow()
    {
        var service = new GeographyService(_eventBus);
        var province = service.RegisterProvince(new ProvinceState
        {
            Name = "Sunspire Desert",
            CountryId = "Arcadia",
            LocalStability = 60.0
        });

        bool rebellionFired = false;
        _eventBus.Subscribe<RebellionRiskElevatedEvent>((e, _) =>
        {
            if (e.ProvinceId == province.Id) rebellionFired = true;
            return ValueTask.CompletedTask;
        });

        await service.UpdateProvincialStabilityAsync(province.Id, -25.0);
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.True(rebellionFired);
        Assert.True(province.RebellionRisk > 50.0);
    }

    [Fact]
    public async Task SimulateProvincialTurnAsync_AdvancesPopulationAndDriftsStability()
    {
        var service = new GeographyService(_eventBus);
        var province = service.RegisterProvince(new ProvinceState
        {
            Name = "Eldoria Capital",
            CountryId = "Arcadia",
            Population = 500000,
            LocalStability = 90.0,
            InfrastructureIndex = 95.0
        });

        await service.SimulateProvincialTurnAsync();

        Assert.True(province.Population > 500000);
        Assert.True(province.LocalStability >= 90.0);
    }
}
