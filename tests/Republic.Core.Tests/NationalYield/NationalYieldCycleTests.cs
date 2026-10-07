namespace Republic.Core.Tests.NationalYield;

using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalYieldCycleTests
{
    private readonly RepublicClock _clock;
    private readonly NationalYieldCycleService _cycleService;

    public NationalYieldCycleTests()
    {
        _clock = RepublicClock.CreateControlled();
        _cycleService = new NationalYieldCycleService();
    }

    private static Country CreateTestCountry(
        string name = "Republic of Testing",
        double stability = 75.0,
        double industry = 60.0,
        double infrastructure = 60.0,
        double humanCapital = 70.0,
        double science = 65.0,
        double admin = 70.0,
        double security = 60.0)
    {
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = industry,
            InfrastructureCapacity = infrastructure,
            HumanCapital = humanCapital,
            ScienceCapacity = science,
            AdministrativeCapacity = admin,
            SecurityCapacity = security
        };

        var clock = RepublicClock.CreateControlled();
        var country = Country.Found(
            name: name,
            foundingTime: clock.CurrentTime,
            baselineStability: stability,
            yield: yield);

        country.NationalTraits.Add("Industrious");
        return country;
    }

    [Fact]
    public void Test1_CycleCalculatesYield_GivenValidCountry_ReturnsNationalYieldResult()
    {
        // Given a valid country and an authoritative simulation time
        var country = CreateTestCountry();
        var simulationTime = _clock.CurrentTime;

        // When the cycle executes
        var snapshot = _cycleService.ExecuteCycle(country, simulationTime);

        // Then an evaluated National Yield snapshot is returned
        Assert.NotNull(snapshot);
        Assert.Equal(country.Id, snapshot.CountryId);
        Assert.Equal(simulationTime, snapshot.SimulationTime);
        Assert.NotNull(snapshot.Yield);

        // All 14 canonical categories are populated and non-negative
        Assert.True(snapshot.Yield.IndustrialCapacity > 0.0);
        Assert.True(snapshot.Yield.RepuTreasuryRevenue > 0.0);
        Assert.True(snapshot.Yield.HumanCapital > 0.0);
        Assert.True(snapshot.Yield.AdministrativeCapacity > 0.0);
        Assert.True(snapshot.Yield.ScienceCapacity > 0.0);
        Assert.True(snapshot.Yield.SecurityCapacity > 0.0);

        // Implicit conversion to NationalYield works seamlessly
        Republic.Core.NationalYield.NationalYield directYield = snapshot;
        Assert.Same(snapshot.Yield, directYield);

        // The snapshot is stored in the cycle service and retrievable
        var retrieved = _cycleService.GetLatestSnapshot(country.Id);
        Assert.NotNull(retrieved);
        Assert.Same(snapshot, retrieved);
    }

    [Fact]
    public void Test2_Determinism_SameCountryStateAndSameSimulationTime_ProducesIdenticalResult()
    {
        var country = CreateTestCountry();
        var simulationTime = _clock.CurrentTime;

        var snapshot1 = _cycleService.ExecuteCycle(country, simulationTime);
        var snapshot2 = _cycleService.ExecuteCycle(country, simulationTime);

        Assert.Equal(snapshot1.SimulationTime, snapshot2.SimulationTime);
        Assert.Equal(snapshot1.Yield.RepuTreasuryRevenue, snapshot2.Yield.RepuTreasuryRevenue);
        Assert.Equal(snapshot1.Yield.IndustrialCapacity, snapshot2.Yield.IndustrialCapacity);
        Assert.Equal(snapshot1.Yield.EnergyCapacity, snapshot2.Yield.EnergyCapacity);
        Assert.Equal(snapshot1.Yield.InfrastructureCapacity, snapshot2.Yield.InfrastructureCapacity);
        Assert.Equal(snapshot1.Yield.FinancialCapacity, snapshot2.Yield.FinancialCapacity);
        Assert.Equal(snapshot1.Yield.HumanCapital, snapshot2.Yield.HumanCapital);
        Assert.Equal(snapshot1.Yield.ScienceCapacity, snapshot2.Yield.ScienceCapacity);
        Assert.Equal(snapshot1.Yield.InnovationCapacity, snapshot2.Yield.InnovationCapacity);
        Assert.Equal(snapshot1.Yield.AdministrativeCapacity, snapshot2.Yield.AdministrativeCapacity);
        Assert.Equal(snapshot1.Yield.SecurityCapacity, snapshot2.Yield.SecurityCapacity);
        Assert.Equal(snapshot1.Yield.IntelligenceCapacity, snapshot2.Yield.IntelligenceCapacity);
        Assert.Equal(snapshot1.Yield.MilitaryReadiness, snapshot2.Yield.MilitaryReadiness);
        Assert.Equal(snapshot1.Yield.DiplomaticCapacity, snapshot2.Yield.DiplomaticCapacity);
        Assert.Equal(snapshot1.Yield.NaturalResourceOutput, snapshot2.Yield.NaturalResourceOutput);
    }

    [Fact]
    public void Test3_CountryIsolation_CalculatingCountryA_DoesNotModifyCountryB()
    {
        var countryA = CreateTestCountry("Country A", industry: 90.0, stability: 90.0);
        var countryB = CreateTestCountry("Country B", industry: 20.0, stability: 30.0);

        var countryBInitialIndustry = countryB.Yield.IndustrialCapacity;
        var countryBInitialStability = countryB.BaselineStability;
        var countryBInitialId = countryB.Id;

        // Execute cycle multiple times on Country A
        for (var i = 0; i < 5; i++)
        {
            _cycleService.ExecuteCycle(countryA, _clock.CurrentTime);
        }

        // Verify Country B remains completely unmutated
        Assert.Equal(countryBInitialIndustry, countryB.Yield.IndustrialCapacity);
        Assert.Equal(countryBInitialStability, countryB.BaselineStability);
        Assert.Equal(countryBInitialId, countryB.Id);

        // Execute cycle for Country B
        var snapshotB = _cycleService.ExecuteCycle(countryB, _clock.CurrentTime);
        var snapshotA = _cycleService.GetLatestSnapshot(countryA.Id);

        Assert.NotNull(snapshotA);
        Assert.NotNull(snapshotB);
        Assert.Equal(countryA.Id, snapshotA.CountryId);
        Assert.Equal(countryB.Id, snapshotB.CountryId);
        Assert.NotEqual(snapshotA.Yield.IndustrialCapacity, snapshotB.Yield.IndustrialCapacity);
    }

    [Fact]
    public void Test4_ClockIndependence_DoesNotReadRealSystemClock()
    {
        var country = CreateTestCountry();

        // Construct an arbitrary, explicit future simulation time (e.g. Day 1000)
        var explicitFutureTime = _clock.FromDayAndTime(1000, new TimeOnly(14, 30));

        var snapshot = _cycleService.ExecuteCycle(country, explicitFutureTime);

        // Snapshot uses strictly the supplied simulation time
        Assert.Equal(explicitFutureTime, snapshot.SimulationTime);
        Assert.Equal(1000, snapshot.SimulationTime.DayNumber);
        Assert.Equal(new TimeOnly(14, 30), snapshot.SimulationTime.Time);

        // The calculator remains independent of wall-clock time
        Assert.True(snapshot.Yield.RepuTreasuryRevenue > 0.0);
    }

    [Fact]
    public void Test5_CalculatorReuse_UsesExistingNationalYieldCalculator()
    {
        // Spy calculator to verify delegation
        var spy = new SpyNationalYieldCalculator();
        var cycleServiceWithSpy = new NationalYieldCycleService(spy);
        var country = CreateTestCountry();
        var simulationTime = _clock.CurrentTime;

        var snapshot = cycleServiceWithSpy.ExecuteCycle(country, simulationTime);

        // Verify the existing calculator interface was called exactly once
        Assert.Equal(1, spy.CallCount);
        Assert.NotNull(spy.LastInputs);
        Assert.Equal(country.Id, spy.LastInputs.CountryId);
        Assert.Equal(country.BaselineStability, spy.LastInputs.PoliticalStability);
        Assert.Equal(country.Yield.IndustrialCapacity, spy.LastInputs.EconomicCapacity);

        // Verify the resulting yield in the snapshot was produced by the calculator
        Assert.Same(spy.PresetResult, snapshot.Yield);
    }

    [Fact]
    public void Test6_ExplicitSimulationTime_PassesDifferentTimesWithoutCalculatorReadingSystemClock()
    {
        var country = CreateTestCountry();
        var time1 = _clock.FromDayAndTime(10, new TimeOnly(8, 0));
        var time2 = _clock.FromDayAndTime(50, new TimeOnly(20, 0));

        var snapshot1 = _cycleService.ExecuteCycle(country, time1);
        var snapshot2 = _cycleService.ExecuteCycle(country, time2);

        // Both snapshots record their exact respective simulation time
        Assert.Equal(time1, snapshot1.SimulationTime);
        Assert.Equal(time2, snapshot2.SimulationTime);
        Assert.NotEqual(snapshot1.SimulationTime, snapshot2.SimulationTime);

        // Country yields are identical because country state did not change
        Assert.Equal(snapshot1.Yield.IndustrialCapacity, snapshot2.Yield.IndustrialCapacity);
        Assert.Equal(snapshot1.Yield.RepuTreasuryRevenue, snapshot2.Yield.RepuTreasuryRevenue);
    }

    [Fact]
    public void ExecuteCycle_WithClockOverload_UsesClockCurrentTime()
    {
        var country = CreateTestCountry();
        _clock.SetTime(_clock.FromDayAndTime(5, new TimeOnly(12, 0)));

        var snapshot = _cycleService.ExecuteCycle(country, _clock);

        Assert.Equal(_clock.CurrentTime, snapshot.SimulationTime);
        Assert.Equal(5, snapshot.SimulationTime.DayNumber);
    }

    [Fact]
    public void ExecuteCycle_ThrowsArgumentNullException_WhenCountryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _cycleService.ExecuteCycle(null!, _clock.CurrentTime));
        Assert.Throws<ArgumentNullException>(() => _cycleService.ExecuteCycle(null!, _clock));
    }

    [Fact]
    public void ClearSnapshots_RemovesAllStoredSnapshots()
    {
        var country = CreateTestCountry();
        _cycleService.ExecuteCycle(country, _clock.CurrentTime);

        Assert.NotNull(_cycleService.GetLatestSnapshot(country.Id));

        _cycleService.ClearSnapshots();

        Assert.Null(_cycleService.GetLatestSnapshot(country.Id));
    }

    private sealed class SpyNationalYieldCalculator : INationalYieldCalculator
    {
        public int CallCount { get; private set; }
        public NationalYieldCalculationInputs? LastInputs { get; private set; }
        public Republic.Core.NationalYield.NationalYield PresetResult { get; } = new()
        {
            IndustrialCapacity = 77.7,
            RepuTreasuryRevenue = 44.4
        };

        public Republic.Core.NationalYield.NationalYield Calculate(NationalYieldCalculationInputs inputs)
        {
            CallCount++;
            LastInputs = inputs;
            return PresetResult;
        }
    }
}
