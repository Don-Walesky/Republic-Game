namespace Republic.Core.Tests.World;

using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.World.Services;

public sealed class WorldServicesTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());
    private readonly TestLogger _logger = new();

    [Fact]
    public void CountryService_RegisterAndQuery_UpdatesStability()
    {
        var service = new CountryService(_eventBus, _logger);
        var country = new Country
        {
            Id = "country-1",
            Name = "Aethelgard",
            CapitalCity = "Highland",
            GovernmentType = "Democracy",
            BaselineStability = 75.0,
        };

        service.RegisterCountry(country);
        var fetched = service.GetCountry("country-1");

        Assert.NotNull(fetched);
        Assert.Equal("Aethelgard", fetched.Name);
        Assert.Single(service.GetAllCountries());

        var updated = service.UpdateStability("country-1", -15.0);
        Assert.True(updated);
        Assert.Equal(60.0, fetched.BaselineStability);
    }

    [Fact]
    public void GeographyService_RegisterAndFilterRegions()
    {
        var service = new GeographyService(_logger);
        var r1 = new Region { Id = "r1", Name = "Northlands", CountryId = "c1", Biome = "Tundra", TerrainType = "Mountainous" };
        var r2 = new Region { Id = "r2", Name = "Southbasin", CountryId = "c2", Biome = "Savannah", TerrainType = "Plains" };

        service.RegisterRegion(r1);
        service.RegisterRegion(r2);

        var c1Regions = service.GetRegionsForCountry("c1");
        Assert.Single(c1Regions);
        Assert.Equal("Northlands", c1Regions[0].Name);
        Assert.Equal(2, service.GetAllRegions().Count);
    }

    [Fact]
    public void ResourceService_RegisterAndExtractResource()
    {
        var service = new ResourceService(_eventBus, _logger);
        var node = new ResourceNode
        {
            Id = "node-iron",
            LocationRegionId = "r1",
            ResourceType = "Iron",
            Abundance = 100.0,
            IsRenewable = false,
        };

        service.RegisterNode(node);
        Assert.Single(service.GetNodesForRegion("r1"));

        var extracted = service.ExtractResource("node-iron", 30.0);
        Assert.Equal(30.0, extracted);
        Assert.Equal(70.0, node.Abundance);

        var overExtracted = service.ExtractResource("node-iron", 100.0);
        Assert.Equal(70.0, overExtracted);
        Assert.Equal(0.0, node.Abundance);
    }

    [Fact]
    public void DemographicService_UpdatePopulationAndHappiness()
    {
        var service = new DemographicService(_eventBus, _logger);
        var demo = service.GetDemographics();
        demo.GrowthRate = 0.05; // 5% per tick period

        service.UpdatePopulation(1_000_000);
        service.UpdateHappiness(85.5);

        Assert.Equal(1_000_000, service.GetDemographics().TotalPopulation);
        Assert.Equal(85.5, service.GetDemographics().HappinessRating);

        service.AdvanceDemographicsTick();
        Assert.True(service.GetDemographics().TotalPopulation > 1_000_000);
    }

    [Fact]
    public void EconomicService_DepositAndWithdrawTreasury()
    {
        var service = new EconomicService(_eventBus, _logger);
        var initialBalance = service.GetIndicators().TreasuryBalance;
        service.DepositTreasury(50_000.0);

        Assert.Equal(initialBalance + 50_000.0, service.GetIndicators().TreasuryBalance);

        var success = service.WithdrawTreasury(20_000.0);
        Assert.True(success);
        Assert.Equal(initialBalance + 30_000.0, service.GetIndicators().TreasuryBalance);

        var fail = service.WithdrawTreasury(initialBalance + 100_000.0);
        Assert.False(fail);
        Assert.Equal(initialBalance + 30_000.0, service.GetIndicators().TreasuryBalance);

        service.AdvanceEconomyTick();
        Assert.True(service.GetIndicators().GrossDomesticProduct > 0);
    }

    [Fact]
    public void PoliticalCultureService_RegisterFactionAndUpdateApproval()
    {
        var service = new PoliticalCultureService(_eventBus, _logger);
        var faction = new Faction
        {
            Id = "f1",
            Name = "Reform Party",
            Ideology = "Progressive",
            InfluencePercentage = 40.0,
            ApprovalRating = 50.0,
        };

        service.RegisterFaction(faction);
        Assert.Single(service.GetFactions());

        var updated = service.UpdateApproval("f1", 62.0);
        Assert.True(updated);
        Assert.Equal(62.0, service.GetFactions()[0].ApprovalRating);
        Assert.NotNull(service.GetConstitution());
    }

    [Fact]
    public async Task WorldManager_AdvancesTick_CallsDomainServices()
    {
        var world = new WorldManager(_eventBus, _logger);
        await world.CreateAsync("Full Simulation Test");

        world.Economic.DepositTreasury(10_000.0);
        world.Demographics.UpdatePopulation(500_000);

        world.AdvanceTo(10);

        Assert.Equal(10UL, world.Current.CurrentTick);
        Assert.NotNull(world.Countries);
        Assert.NotNull(world.Geography);
        Assert.NotNull(world.Resources);
        Assert.NotNull(world.PoliticalCulture);
    }
}
