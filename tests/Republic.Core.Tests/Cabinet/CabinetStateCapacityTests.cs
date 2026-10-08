namespace Republic.Core.Tests.Cabinet;

using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Events;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class CabinetStateCapacityTests
{
    private readonly RepublicClock _clock;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;

    public CabinetStateCapacityTests()
    {
        _clock = RepublicClock.CreateControlled();
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Cabinet Capacity World").GetAwaiter().GetResult();
    }

    private Country CreateBaselineCountry(string name, double baselineInputs = 1.0)
    {
        var inputs = StateCapacityInputs.All(baselineInputs);
        return Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            stateCapacity: inputs);
    }

    [Fact]
    public void FinanceMinister_CompetencePoint4_IntegrityPoint9_SetsCivilServiceQualityToPoint4()
    {
        // Given a sovereign country with all baseline inputs at 1.0
        var country = CreateBaselineCountry("Financial State", 1.0);

        // When a finance minister with competence 0.4 and integrity 0.9 is appointed
        var financeMinister = new Minister
        {
            Name = "Dr. Elena Rostova",
            Competence = 0.4,
            Integrity = 0.9,
            Loyalty = 0.8,
            PoliticalConnections = 0.7,
            Experience = 0.6
        };

        country.AppointMinister(financeMinister, CabinetPortfolio.Finance);

        // Then civil service quality is set to min(0.4, 0.9) = 0.4
        Assert.Equal(0.4, country.StateCapacity.CivilServiceQuality);
    }

    [Fact]
    public void VacantInterior_SetsCorruptionControlToPoint3()
    {
        // Given a sovereign country with all baseline inputs at 1.0
        var country = CreateBaselineCountry("Interior State", 1.0);

        // First appoint an interior minister
        var interiorMinister = new Minister
        {
            Name = "Hon. Marcus Holloway",
            Competence = 0.8,
            Integrity = 0.85
        };
        country.AppointMinister(interiorMinister, CabinetPortfolio.Interior);
        Assert.Equal(0.8, country.StateCapacity.CorruptionControl);

        // When interior portfolio becomes vacant (dismissed or vacated)
        country.DismissMinister(CabinetPortfolio.Interior);

        // Then corruption control is set to 0.3, not 1.0 or 0.0
        Assert.Equal(0.3, country.StateCapacity.CorruptionControl);
        Assert.Null(country.GetMinister(CabinetPortfolio.Interior));
    }

    [Fact]
    public void ReplacingWeakFinanceMinister_CompetencePoint9_IntegrityPoint8_RaisesCivilServiceQualityToPoint8()
    {
        // Given a sovereign country with a weak finance minister (comp 0.4, integrity 0.9 -> civil service 0.4)
        var country = CreateBaselineCountry("Reform Republic", 1.0);
        var weakMinister = new Minister
        {
            Name = "Old Minister",
            Competence = 0.4,
            Integrity = 0.9
        };
        country.AppointMinister(weakMinister, CabinetPortfolio.Finance);
        Assert.Equal(0.4, country.StateCapacity.CivilServiceQuality);

        // When replacing with a competent and upright minister (comp 0.9, integrity 0.8)
        var newMinister = new Minister
        {
            Name = "Dr. Elena Rostova",
            Competence = 0.9,
            Integrity = 0.8
        };
        country.AppointMinister(newMinister, CabinetPortfolio.Finance);

        // Then civil service quality raises to min(0.9, 0.8) = 0.8
        Assert.Equal(0.8, country.StateCapacity.CivilServiceQuality);
    }

    [Fact]
    public void CountryIsolation_CountryADoesNotChangeCountryB()
    {
        // Given two isolated sovereign countries
        var countryA = CreateBaselineCountry("Country A", 1.0);
        var countryB = CreateBaselineCountry("Country B", 1.0);

        // When Country A appoints ministers
        var ministerA = new Minister
        {
            Name = "Minister Alpha",
            Competence = 0.4,
            Integrity = 0.4
        };
        countryA.AppointMinister(ministerA, CabinetPortfolio.Finance);

        // Country A inputs change
        Assert.Equal(0.4, countryA.StateCapacity.CivilServiceQuality);
        Assert.Equal(0.3, countryA.StateCapacity.CorruptionControl); // Interior vacant in A
        Assert.Equal(0.3, countryA.StateCapacity.InfrastructureCondition); // Infra vacant in A

        // Country B inputs remain completely untouched
        Assert.Equal(1.0, countryB.StateCapacity.CivilServiceQuality);
        Assert.Equal(1.0, countryB.StateCapacity.CorruptionControl);
        Assert.Equal(1.0, countryB.StateCapacity.InfrastructureCondition);
        Assert.Equal(1.0, countryB.StateCapacityScore);
        Assert.Null(countryB.GetMinister(CabinetPortfolio.Finance));
    }

    [Fact]
    public void StateCapacityScore_RemainsMinimumOfSevenInputsAfterAppointment()
    {
        // Given a sovereign country with baseline inputs at 0.9
        var country = CreateBaselineCountry("Minimum Rule Republic", 0.9);

        // Appoint Finance (comp 0.8, integrity 0.7 => civil service = 0.7)
        country.AppointMinister(new Minister { Competence = 0.8, Integrity = 0.7 }, CabinetPortfolio.Finance);

        // Appoint Interior (comp 0.9, integrity 0.9 => corruption control = 0.9)
        country.AppointMinister(new Minister { Competence = 0.9, Integrity = 0.9 }, CabinetPortfolio.Interior);

        // Appoint Infrastructure (comp 0.5, experience 0.6 => infrastructure condition = 0.5)
        country.AppointMinister(new Minister { Competence = 0.5, Experience = 0.6 }, CabinetPortfolio.Infrastructure);

        // Inputs summary:
        // GovEff: 0.9, AdminComp: 0.9, InstQual: 0.9, Stability: 0.9
        // CivilServiceQuality: 0.7
        // CorruptionControl: 0.9
        // InfrastructureCondition: 0.5  <-- Bottleneck
        var snapshot = country.EvaluateStateCapacity();

        Assert.Equal(0.5, snapshot.Score);
        Assert.Equal(0.5, country.StateCapacityScore);
        Assert.Equal("50%", snapshot.FormattedScore);
        Assert.Equal("Infrastructure condition", snapshot.BottleneckFactor);
    }

    [Fact]
    public void InfrastructureMinister_CompetenceAndExperience_SetInfrastructureConditionToMinimum()
    {
        var country = CreateBaselineCountry("Infra Republic", 1.0);

        // Test comp < experience
        var infra1 = new Minister { Competence = 0.45, Experience = 0.85 };
        country.AppointMinister(infra1, CabinetPortfolio.Infrastructure);
        Assert.Equal(0.45, country.StateCapacity.InfrastructureCondition);

        // Test experience < comp
        var infra2 = new Minister { Competence = 0.90, Experience = 0.65 };
        country.AppointMinister(infra2, CabinetPortfolio.Infrastructure);
        Assert.Equal(0.65, country.StateCapacity.InfrastructureCondition);

        // Test vacating sets to 0.3
        country.VacatePortfolio(CabinetPortfolio.Infrastructure);
        Assert.Equal(0.3, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void LoyaltyAndPoliticalConnections_DoNotRaiseCapacity()
    {
        var country = CreateBaselineCountry("Loyalty Republic", 1.0);

        // High loyalty and political connections, but modest competence and integrity
        var minister = new Minister
        {
            Name = "Party Loyalist",
            Loyalty = 1.0,
            PoliticalConnections = 1.0,
            Competence = 0.5,
            Integrity = 0.5
        };

        country.AppointMinister(minister, CabinetPortfolio.Finance);

        // Civil service is 0.5, not 1.0
        Assert.Equal(0.5, country.StateCapacity.CivilServiceQuality);
        Assert.Equal(1.0, minister.Loyalty);
        Assert.Equal(1.0, minister.PoliticalConnections);
    }

    [Fact]
    public void AppointingMinister_SpendsNothingFromTreasury()
    {
        var country = CreateBaselineCountry("Frugal Republic", 1.0);
        country.Treasury.Deposit(100_000.0);
        var initialBalance = country.Treasury.Balance;

        country.AppointMinister(new Minister { Competence = 0.8, Integrity = 0.8 }, CabinetPortfolio.Finance);
        country.AppointMinister(new Minister { Competence = 0.8, Integrity = 0.8 }, CabinetPortfolio.Interior);
        country.AppointMinister(new Minister { Competence = 0.8, Experience = 0.8 }, CabinetPortfolio.Infrastructure);

        Assert.Equal(initialBalance, country.Treasury.Balance);
    }

    [Fact]
    public async Task CabinetService_AppointMinisterAsync_UpdatesCountryStateCapacity()
    {
        var service = new CabinetService(_world, _eventBus, logger: _logger);
        var country = CreateBaselineCountry("Service Republic", 1.0);

        var minister = new Minister
        {
            Name = "Minister of Finance",
            Competence = 0.75,
            Integrity = 0.85
        };

        var appointed = await service.AppointMinisterAsync(country, minister, CabinetPortfolio.Finance);

        Assert.True(appointed.IsAppointed);
        Assert.Equal(CabinetPortfolio.Finance, appointed.Portfolio);
        Assert.Equal(0.75, country.StateCapacity.CivilServiceQuality);
        Assert.Same(appointed, country.GetMinister(CabinetPortfolio.Finance));

        // Dismissing through CabinetService
        await service.DismissMinisterAsync(country, CabinetPortfolio.Finance);
        Assert.Equal(0.3, country.StateCapacity.CivilServiceQuality);
        Assert.Null(country.GetMinister(CabinetPortfolio.Finance));
    }
}
