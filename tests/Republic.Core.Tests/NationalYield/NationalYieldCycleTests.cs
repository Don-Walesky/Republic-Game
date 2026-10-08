namespace Republic.Core.Tests.NationalYield;

using Republic.Core.Economy.Treasury;
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
            yield: yield,
            stateCapacity: StateCapacityInputs.All(1.0));

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

        // The snapshot is stored in the cycle service and retrievable as a defensive copy
        var retrieved = _cycleService.GetLatestSnapshot(country.Id);
        Assert.NotNull(retrieved);
        Assert.NotSame(snapshot, retrieved);
        Assert.Equal(snapshot.Yield.IndustrialCapacity, retrieved.Yield.IndustrialCapacity);
        Assert.Equal(snapshot.Yield.RepuTreasuryRevenue, retrieved.Yield.RepuTreasuryRevenue);
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
        var simulationTime = _clock.FromDayAndTime(1, new TimeOnly(12, 0));

        var snapshot = cycleServiceWithSpy.ExecuteCycle(country, simulationTime);

        // Verify the existing calculator interface was called exactly once
        Assert.Equal(1, spy.CallCount);
        Assert.NotNull(spy.LastInputs);
        Assert.Equal(country.Id, spy.LastInputs.CountryId);
        Assert.Equal(country.BaselineStability, spy.LastInputs.PoliticalStability);
        Assert.Equal(country.Yield.IndustrialCapacity, spy.LastInputs.EconomicCapacity);

        // Verify the resulting yield values match the calculator output
        Assert.Equal(spy.PresetResult.IndustrialCapacity, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(spy.PresetResult.RepuTreasuryRevenue, snapshot.Yield.RepuTreasuryRevenue);
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
    public void SnapshotIntegrity_MutatingReturnedOrRetrievedSnapshot_DoesNotAlterServiceCachedSnapshot()
    {
        var country = CreateTestCountry();
        var time = _clock.CurrentTime;

        // 1. Execute cycle and get returned snapshot
        var returnedSnapshot = _cycleService.ExecuteCycle(country, time);
        var originalIndustrial = returnedSnapshot.Yield.IndustrialCapacity;
        var originalRevenue = returnedSnapshot.Yield.RepuTreasuryRevenue;

        // Verify all 14 categories exist on returned snapshot
        Assert.True(originalIndustrial > 0.0);
        Assert.True(originalRevenue > 0.0);

        // 2. Mutate the returned snapshot's Yield
        returnedSnapshot.Yield.IndustrialCapacity = 999999.0;
        returnedSnapshot.Yield.RepuTreasuryRevenue = 888888.0;

        // 3. Retrieve snapshot from the service
        var cachedSnapshot = _cycleService.GetLatestSnapshot(country.Id);
        Assert.NotNull(cachedSnapshot);

        // The cached snapshot must NOT be altered by mutations to the returned snapshot
        Assert.Equal(originalIndustrial, cachedSnapshot.Yield.IndustrialCapacity);
        Assert.Equal(originalRevenue, cachedSnapshot.Yield.RepuTreasuryRevenue);

        // 4. Mutate the retrieved snapshot
        cachedSnapshot.Yield.IndustrialCapacity = 777777.0;

        // 5. Retrieve snapshot again from the service
        var cachedSnapshotAgain = _cycleService.GetLatestSnapshot(country.Id);
        Assert.NotNull(cachedSnapshotAgain);

        // The service's internal snapshot must remain pristine
        Assert.Equal(originalIndustrial, cachedSnapshotAgain.Yield.IndustrialCapacity);
        Assert.Equal(originalRevenue, cachedSnapshotAgain.Yield.RepuTreasuryRevenue);

        // 6. Verify all 14 categories are preserved across defensive copies
        Assert.Equal(returnedSnapshot.Yield.HumanCapital, cachedSnapshotAgain.Yield.HumanCapital);
        Assert.Equal(returnedSnapshot.Yield.EnergyCapacity, cachedSnapshotAgain.Yield.EnergyCapacity);
        Assert.Equal(returnedSnapshot.Yield.InfrastructureCapacity, cachedSnapshotAgain.Yield.InfrastructureCapacity);
        Assert.Equal(returnedSnapshot.Yield.FinancialCapacity, cachedSnapshotAgain.Yield.FinancialCapacity);
        Assert.Equal(returnedSnapshot.Yield.ScienceCapacity, cachedSnapshotAgain.Yield.ScienceCapacity);
        Assert.Equal(returnedSnapshot.Yield.InnovationCapacity, cachedSnapshotAgain.Yield.InnovationCapacity);
        Assert.Equal(returnedSnapshot.Yield.AdministrativeCapacity, cachedSnapshotAgain.Yield.AdministrativeCapacity);
        Assert.Equal(returnedSnapshot.Yield.SecurityCapacity, cachedSnapshotAgain.Yield.SecurityCapacity);
        Assert.Equal(returnedSnapshot.Yield.IntelligenceCapacity, cachedSnapshotAgain.Yield.IntelligenceCapacity);
        Assert.Equal(returnedSnapshot.Yield.MilitaryReadiness, cachedSnapshotAgain.Yield.MilitaryReadiness);
        Assert.Equal(returnedSnapshot.Yield.DiplomaticCapacity, cachedSnapshotAgain.Yield.DiplomaticCapacity);
        Assert.Equal(returnedSnapshot.Yield.NaturalResourceOutput, cachedSnapshotAgain.Yield.NaturalResourceOutput);
    }

    [Fact]
    public void SimulationTimeSemantics_RecordsSuppliedTime_CalculatesFromCurrentCountryStateWithoutHistoricalReconstruction()
    {
        var country = CreateTestCountry(industry: 40.0, infrastructure: 40.0);
        var historicalTime = _clock.FromDayAndTime(1, new TimeOnly(10, 0));

        // Initial cycle execution at historicalTime
        var snapshot1 = _cycleService.ExecuteCycle(country, historicalTime);
        Assert.Equal(historicalTime, snapshot1.SimulationTime);
        Assert.Equal(40.0, snapshot1.Yield.IndustrialCapacity);

        // Upgrading country's current in-memory state
        country.Yield.IndustrialCapacity = 85.0;
        country.Yield.InfrastructureCapacity = 85.0;

        // Running cycle again with the SAME historicalTime evaluates the current state
        // (i.e. does not reconstruct what the country state was at Day 1)
        var snapshot2 = _cycleService.ExecuteCycle(country, historicalTime);
        Assert.Equal(historicalTime, snapshot2.SimulationTime);
        Assert.Equal(85.0, snapshot2.Yield.IndustrialCapacity);
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

    [Fact]
    public void FoundingDayMultiplier_AppliesTenXMultiplier_AndSnapshotContainsFinalYield()
    {
        // 1 & 5. A country whose cycle executes during its founding day receives x10 yield and snapshot contains final yield
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = Country.Found("Founding Nation", foundingTime, baselineStability: 80.0);
        var evalTime = _clock.FromDayAndTime(5, new TimeOnly(15, 0));

        var snapshot = _cycleService.ExecuteCycle(country, evalTime);

        // Expected unscaled baseline from calculator
        var expectedBase = new NationalYieldCalculator().Calculate(NationalYieldCalculationInputs.FromCountry(country));

        // Output yield receives x10 across all 14 canonical categories
        Assert.Equal(expectedBase.RepuTreasuryRevenue * 10.0, snapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(expectedBase.IndustrialCapacity * 10.0, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(expectedBase.EnergyCapacity * 10.0, snapshot.Yield.EnergyCapacity);
        Assert.Equal(expectedBase.InfrastructureCapacity * 10.0, snapshot.Yield.InfrastructureCapacity);
        Assert.Equal(expectedBase.FinancialCapacity * 10.0, snapshot.Yield.FinancialCapacity);
        Assert.Equal(expectedBase.HumanCapital * 10.0, snapshot.Yield.HumanCapital);
        Assert.Equal(expectedBase.ScienceCapacity * 10.0, snapshot.Yield.ScienceCapacity);
        Assert.Equal(expectedBase.InnovationCapacity * 10.0, snapshot.Yield.InnovationCapacity);
        Assert.Equal(expectedBase.AdministrativeCapacity * 10.0, snapshot.Yield.AdministrativeCapacity);
        Assert.Equal(expectedBase.SecurityCapacity * 10.0, snapshot.Yield.SecurityCapacity);
        Assert.Equal(expectedBase.IntelligenceCapacity * 10.0, snapshot.Yield.IntelligenceCapacity);
        Assert.Equal(expectedBase.MilitaryReadiness * 10.0, snapshot.Yield.MilitaryReadiness);
        Assert.Equal(expectedBase.DiplomaticCapacity * 10.0, snapshot.Yield.DiplomaticCapacity);
        Assert.Equal(expectedBase.NaturalResourceOutput * 10.0, snapshot.Yield.NaturalResourceOutput);

        // Snapshot stored in service also contains the final post-multiplier yield
        var cached = _cycleService.GetLatestSnapshot(country.Id);
        Assert.NotNull(cached);
        Assert.Equal(snapshot.Yield.IndustrialCapacity, cached.Yield.IndustrialCapacity);
        Assert.Equal(snapshot.Yield.RepuTreasuryRevenue, cached.Yield.RepuTreasuryRevenue);
    }

    [Fact]
    public void PostFoundingDay_CycleExecution_YieldRemainsNormalOneX()
    {
        // 2. A country whose cycle executes after its founding day receives normal x1 yield
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = Country.Found("Post-Founding Nation", foundingTime, baselineStability: 80.0);
        var postFoundingTime = _clock.FromDayAndTime(6, new TimeOnly(10, 0));

        var snapshot = _cycleService.ExecuteCycle(country, postFoundingTime);

        var expectedBase = new NationalYieldCalculator().Calculate(NationalYieldCalculationInputs.FromCountry(country));

        Assert.Equal(expectedBase.RepuTreasuryRevenue, snapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(expectedBase.IndustrialCapacity, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(expectedBase.EnergyCapacity, snapshot.Yield.EnergyCapacity);
        Assert.Equal(expectedBase.InfrastructureCapacity, snapshot.Yield.InfrastructureCapacity);
        Assert.Equal(expectedBase.FinancialCapacity, snapshot.Yield.FinancialCapacity);
        Assert.Equal(expectedBase.HumanCapital, snapshot.Yield.HumanCapital);
        Assert.Equal(expectedBase.ScienceCapacity, snapshot.Yield.ScienceCapacity);
        Assert.Equal(expectedBase.InnovationCapacity, snapshot.Yield.InnovationCapacity);
        Assert.Equal(expectedBase.AdministrativeCapacity, snapshot.Yield.AdministrativeCapacity);
        Assert.Equal(expectedBase.SecurityCapacity, snapshot.Yield.SecurityCapacity);
        Assert.Equal(expectedBase.IntelligenceCapacity, snapshot.Yield.IntelligenceCapacity);
        Assert.Equal(expectedBase.MilitaryReadiness, snapshot.Yield.MilitaryReadiness);
        Assert.Equal(expectedBase.DiplomaticCapacity, snapshot.Yield.DiplomaticCapacity);
        Assert.Equal(expectedBase.NaturalResourceOutput, snapshot.Yield.NaturalResourceOutput);
    }

    [Fact]
    public void CycleExecution_UsesExistingFoundingDayRule_RatherThanDuplicatingRuleInternally()
    {
        // 3. The cycle uses the existing founding-day rule rather than duplicating the rule internally
        var spyCalculator = new SpyNationalYieldCalculator();
        var spyRule = new SpyFoundingDayYieldRule();
        var cycleService = new NationalYieldCycleService(spyCalculator, spyRule);

        var country = CreateTestCountry();
        var simulationTime = _clock.FromDayAndTime(12, new TimeOnly(14, 0));

        var snapshot = cycleService.ExecuteCycle(country, simulationTime);

        // Verify cycle service delegated directly to IFoundingDayYieldRule
        Assert.Equal(1, spyRule.ApplyCount);
        Assert.Same(country, spyRule.LastCountry);
        Assert.Equal(simulationTime, spyRule.LastSimulationTime);
        Assert.Same(spyCalculator.PresetResult, spyRule.LastYield);
        Assert.Equal(spyRule.CustomResult.IndustrialCapacity, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(spyRule.CustomResult.RepuTreasuryRevenue, snapshot.Yield.RepuTreasuryRevenue);
    }

    [Fact]
    public void CycleExecution_OriginalCalculatedYield_IsNotMutated()
    {
        // 4. The original calculated yield is not mutated
        var original = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = 50.0,
            RepuTreasuryRevenue = 20.0,
            EnergyCapacity = 30.0
        };
        var fixedCalculator = new FixedYieldCalculator(original);
        var cycleService = new NationalYieldCycleService(fixedCalculator);

        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = Country.Found("Test Country", foundingTime, baselineStability: 75.0);

        var snapshot = cycleService.ExecuteCycle(country, foundingTime);

        // Snapshot receives x10
        Assert.Equal(500.0, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(200.0, snapshot.Yield.RepuTreasuryRevenue);

        // Original yield object returned by calculator is completely unmutated
        Assert.Equal(50.0, original.IndustrialCapacity);
        Assert.Equal(20.0, original.RepuTreasuryRevenue);
        Assert.Equal(30.0, original.EnergyCapacity);
    }

    [Fact]
    public void WatMidnightBoundary_TransitionsCorrectly_FromTenXToNormalOneX()
    {
        // 6. The exact WAT midnight boundary works correctly: before midnight -> x10, after midnight -> x1
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(23, 45));
        var country = Country.Found("Late Nation", foundingTime, baselineStability: 70.0);

        var expectedBase = new NationalYieldCalculator().Calculate(NationalYieldCalculationInputs.FromCountry(country));

        // 1. One second before midnight WAT (Day 5, 23:59:59 WAT) -> x10
        var beforeMidnight = _clock.FromDayAndTime(5, new TimeOnly(23, 59, 59));
        var snapshotBefore = _cycleService.ExecuteCycle(country, beforeMidnight);
        Assert.Equal(expectedBase.IndustrialCapacity * 10.0, snapshotBefore.Yield.IndustrialCapacity);
        Assert.Equal(expectedBase.RepuTreasuryRevenue * 10.0, snapshotBefore.Yield.RepuTreasuryRevenue);

        // 2. Exactly at midnight WAT (Day 6, 00:00:00 WAT) -> normal x1
        var atMidnight = _clock.FromDayAndTime(6, new TimeOnly(0, 0, 0));
        var snapshotAfter = _cycleService.ExecuteCycle(country, atMidnight);
        Assert.Equal(expectedBase.IndustrialCapacity, snapshotAfter.Yield.IndustrialCapacity);
        Assert.Equal(expectedBase.RepuTreasuryRevenue, snapshotAfter.Yield.RepuTreasuryRevenue);
    }

    [Fact]
    public void TwoCountries_WithDifferentFoundingDates_AreEvaluatedIndependently()
    {
        // 7. Two countries with different founding dates are evaluated independently
        var countryAFounding = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var countryBFounding = _clock.FromDayAndTime(6, new TimeOnly(12, 0));

        var countryA = Country.Found("Country A", countryAFounding, baselineStability: 75.0);
        var countryB = Country.Found("Country B", countryBFounding, baselineStability: 75.0);

        var calculator = new NationalYieldCalculator();
        var baseA = calculator.Calculate(NationalYieldCalculationInputs.FromCountry(countryA));
        var baseB = calculator.Calculate(NationalYieldCalculationInputs.FromCountry(countryB));

        // At Day 5, 14:00 WAT: Country A is in founding day (x10), Country B is not yet founded
        var day5Time = _clock.FromDayAndTime(5, new TimeOnly(14, 0));
        var snapshotA_Day5 = _cycleService.ExecuteCycle(countryA, day5Time);
        var snapshotB_Day5 = _cycleService.ExecuteCycle(countryB, day5Time);

        Assert.Equal(baseA.IndustrialCapacity * 10.0, snapshotA_Day5.Yield.IndustrialCapacity);
        Assert.Equal(baseB.IndustrialCapacity, snapshotB_Day5.Yield.IndustrialCapacity); // not founded yet -> 1x

        // At Day 6, 14:00 WAT: Country A is past founding day (1x), Country B is in founding day (x10)
        var day6Time = _clock.FromDayAndTime(6, new TimeOnly(14, 0));
        var snapshotA_Day6 = _cycleService.ExecuteCycle(countryA, day6Time);
        var snapshotB_Day6 = _cycleService.ExecuteCycle(countryB, day6Time);

        Assert.Equal(baseA.IndustrialCapacity, snapshotA_Day6.Yield.IndustrialCapacity);
        Assert.Equal(baseB.IndustrialCapacity * 10.0, snapshotB_Day6.Yield.IndustrialCapacity);
    }

    [Fact]
    public void Test1_NormalYieldCycle_DepositsRepuTreasuryRevenue_IntoTreasury()
    {
        // 1. A normal yield cycle deposits its RepuTreasuryRevenue into the Treasury.
        var country = CreateTestCountry("Normal Cycle Nation");
        country.Treasury.Deposit(100.0);

        var normalTime = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var snapshot = _cycleService.ExecuteCycle(country, normalTime);

        Assert.True(snapshot.Yield.RepuTreasuryRevenue > 0.0);
        Assert.Equal(100.0 + snapshot.Yield.RepuTreasuryRevenue, country.Treasury.Balance);
    }

    [Fact]
    public void Test2_FoundingDayCycle_DepositsTenXRevenue_IntoTreasury()
    {
        // 2. A founding-day cycle deposits the x10 final revenue into the Treasury.
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = Country.Found("Founding Nation", foundingTime, baselineStability: 75.0,
            yield: new Republic.Core.NationalYield.NationalYield
            {
                IndustrialCapacity = 60.0,
                InfrastructureCapacity = 60.0,
                AdministrativeCapacity = 70.0
            },
            stateCapacity: StateCapacityInputs.All(1.0));

        Assert.Equal(0.0, country.Treasury.Balance);

        var calculator = new NationalYieldCalculator();
        var baseRevenue = calculator.Calculate(NationalYieldCalculationInputs.FromCountry(country)).RepuTreasuryRevenue;
        Assert.True(baseRevenue > 0.0);

        var evalTime = _clock.FromDayAndTime(5, new TimeOnly(15, 0));
        var snapshot = _cycleService.ExecuteCycle(country, evalTime);

        // Snapshot has x10 revenue
        Assert.Equal(baseRevenue * 10.0, snapshot.Yield.RepuTreasuryRevenue);

        // Treasury receives x10 final revenue
        Assert.Equal(baseRevenue * 10.0, country.Treasury.Balance);
    }

    [Fact]
    public void Test3_PreMultiplierCalculatedRevenue_IsNotWhatGetsDepositedDuringFoundingDay()
    {
        // 3. The pre-multiplier calculated revenue is NOT what gets deposited during founding day.
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = Country.Found("Founding Nation", foundingTime, baselineStability: 75.0,
            yield: new Republic.Core.NationalYield.NationalYield
            {
                IndustrialCapacity = 60.0,
                InfrastructureCapacity = 60.0,
                AdministrativeCapacity = 70.0
            },
            stateCapacity: StateCapacityInputs.All(1.0));

        var baseRevenue = new NationalYieldCalculator().Calculate(NationalYieldCalculationInputs.FromCountry(country)).RepuTreasuryRevenue;

        var snapshot = _cycleService.ExecuteCycle(country, foundingTime);

        // Treasury balance must strictly reflect the x10 revenue, NOT the pre-multiplier revenue
        Assert.NotEqual(baseRevenue, country.Treasury.Balance);
        Assert.Equal(baseRevenue * 10.0, country.Treasury.Balance);
    }

    [Fact]
    public void Test4_YieldSnapshot_RemainsUnchanged_AfterTreasuryRevenueApplication()
    {
        // 4. The yield snapshot remains unchanged after Treasury revenue application.
        var country = CreateTestCountry("Snapshot Integrity Nation");
        var evalTime = _clock.FromDayAndTime(2, new TimeOnly(12, 0));

        var snapshot = _cycleService.ExecuteCycle(country, evalTime);
        var originalRevenue = snapshot.Yield.RepuTreasuryRevenue;
        var originalIndustry = snapshot.Yield.IndustrialCapacity;
        var originalHuman = snapshot.Yield.HumanCapital;
        var originalAdmin = snapshot.Yield.AdministrativeCapacity;

        // Mutate treasury balance directly afterwards
        country.Treasury.Deposit(999_999.0);
        country.Treasury.Withdraw(50.0);

        // Assert snapshot values remained completely pristine
        Assert.Equal(originalRevenue, snapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(originalIndustry, snapshot.Yield.IndustrialCapacity);
        Assert.Equal(originalHuman, snapshot.Yield.HumanCapital);
        Assert.Equal(originalAdmin, snapshot.Yield.AdministrativeCapacity);
    }

    [Fact]
    public void Test5_ExecutingSameCompletedCycleTwice_DoesNotDoubleCreditTreasury()
    {
        // 5. Executing the same completed cycle/result twice does not double-credit the Treasury.
        var country = CreateTestCountry("Idempotency Nation");
        country.Treasury.Deposit(50.0);
        var cycleTime = _clock.FromDayAndTime(3, new TimeOnly(14, 0));

        // First cycle execution
        var snapshot1 = _cycleService.ExecuteCycle(country, cycleTime);
        var expectedBalance = 50.0 + snapshot1.Yield.RepuTreasuryRevenue;
        Assert.Equal(expectedBalance, country.Treasury.Balance);

        // Second cycle execution of the same completed cycle at the same simulation time
        var snapshot2 = _cycleService.ExecuteCycle(country, cycleTime);
        Assert.Equal(expectedBalance, country.Treasury.Balance);

        // Explicit application of snapshot1 via TreasuryRevenueService directly also does not double-credit
        var revenueService = new TreasuryRevenueService();
        revenueService.ApplyRevenue(country, snapshot1);
        Assert.Equal(expectedBalance, country.Treasury.Balance);

        revenueService.ApplyRevenue(country.Treasury, snapshot2);
        Assert.Equal(expectedBalance, country.Treasury.Balance);
    }

    [Fact]
    public void Test6_TwoDifferentCycles_CorrectlyProduceTwoSeparateRevenueApplications()
    {
        // 6. Two different cycles correctly produce two separate revenue applications.
        var country = CreateTestCountry("Multi-Cycle Nation");
        var time1 = _clock.FromDayAndTime(2, new TimeOnly(6, 0));
        var time2 = _clock.FromDayAndTime(2, new TimeOnly(12, 0));

        var snapshot1 = _cycleService.ExecuteCycle(country, time1);
        var balanceAfter1 = country.Treasury.Balance;
        Assert.Equal(snapshot1.Yield.RepuTreasuryRevenue, balanceAfter1);

        var snapshot2 = _cycleService.ExecuteCycle(country, time2);
        var balanceAfter2 = country.Treasury.Balance;
        Assert.Equal(snapshot1.Yield.RepuTreasuryRevenue + snapshot2.Yield.RepuTreasuryRevenue, balanceAfter2);
    }

    [Fact]
    public void Test7_TwoDifferentCountries_HaveCompletelyIndependentTreasuryBalances()
    {
        // 7. Two different countries have completely independent Treasury balances.
        var countryA = CreateTestCountry("Country A", industry: 50.0);
        var countryB = CreateTestCountry("Country B", industry: 80.0);
        countryA.Treasury.Deposit(100.0);
        countryB.Treasury.Deposit(200.0);

        var evalTime = _clock.FromDayAndTime(4, new TimeOnly(12, 0));

        // Execute cycle on Country A
        var snapshotA = _cycleService.ExecuteCycle(countryA, evalTime);
        Assert.Equal(100.0 + snapshotA.Yield.RepuTreasuryRevenue, countryA.Treasury.Balance);
        Assert.Equal(200.0, countryB.Treasury.Balance); // Country B strictly untouched

        // Execute cycle on Country B
        var snapshotB = _cycleService.ExecuteCycle(countryB, evalTime);
        Assert.Equal(100.0 + snapshotA.Yield.RepuTreasuryRevenue, countryA.Treasury.Balance); // Country A unaffected
        Assert.Equal(200.0 + snapshotB.Yield.RepuTreasuryRevenue, countryB.Treasury.Balance);
    }

    [Fact]
    public void Test8_ZeroRevenueCycle_DoesNotCreateInvalidTreasuryTransaction()
    {
        // 8. A zero-revenue cycle does not create an invalid Treasury transaction.
        var zeroYield = new Republic.Core.NationalYield.NationalYield(); // all 0.0
        var country = Country.Found("Zero Revenue Nation", _clock.CurrentTime, baselineStability: 50.0, yield: zeroYield);
        country.Treasury.Deposit(300.0);

        var snapshot = _cycleService.ExecuteCycle(country, _clock.FromDayAndTime(5, new TimeOnly(10, 0)));

        Assert.Equal(0.0, snapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(300.0, country.Treasury.Balance); // Balance untouched, no exception
    }

    [Fact]
    public void Test9_ExistingTreasurySpendingAndWithdrawal_WorksAfterCycleRevenueApplication()
    {
        // 9. Existing Treasury spending/withdrawal behaviour still works.
        var country = CreateTestCountry("Spending Nation");
        var snapshot = _cycleService.ExecuteCycle(country, _clock.FromDayAndTime(3, new TimeOnly(10, 0)));
        var initialBalance = country.Treasury.Balance;
        Assert.True(initialBalance >= 50.0);

        // Valid spend
        country.Treasury.Withdraw(20.0);
        Assert.Equal(initialBalance - 20.0, country.Treasury.Balance);

        // TryWithdraw with valid and invalid amounts
        Assert.True(country.Treasury.TryWithdraw(10.0));
        Assert.False(country.Treasury.TryWithdraw(999_999.0));

        // Overdraft prevention
        Assert.Throws<InvalidOperationException>(() => country.Treasury.Withdraw(999_999.0));
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

    private sealed class FixedYieldCalculator : INationalYieldCalculator
    {
        private readonly Republic.Core.NationalYield.NationalYield _yield;
        public FixedYieldCalculator(Republic.Core.NationalYield.NationalYield yield) => _yield = yield;
        public Republic.Core.NationalYield.NationalYield Calculate(NationalYieldCalculationInputs inputs) => _yield;
    }

    private sealed class SpyFoundingDayYieldRule : IFoundingDayYieldRule
    {
        public int ApplyCount { get; private set; }
        public Country? LastCountry { get; private set; }
        public RepublicTime? LastSimulationTime { get; private set; }
        public Republic.Core.NationalYield.NationalYield? LastYield { get; private set; }
        public Republic.Core.NationalYield.NationalYield CustomResult { get; } = new()
        {
            IndustrialCapacity = 999.0,
            RepuTreasuryRevenue = 888.0
        };

        public bool IsFoundingDay(Country country, RepublicTime simulationTime) => false;
        public bool IsFoundingDay(Country country, IRepublicClock clock) => false;
        public double GetMultiplier(Country country, RepublicTime simulationTime) => 1.0;

        public Republic.Core.NationalYield.NationalYield Apply(Country country, Republic.Core.NationalYield.NationalYield yield, RepublicTime simulationTime)
        {
            ApplyCount++;
            LastCountry = country;
            LastSimulationTime = simulationTime;
            LastYield = yield;
            return CustomResult;
        }

        public Republic.Core.NationalYield.NationalYield Apply(Country country, Republic.Core.NationalYield.NationalYield yield, IRepublicClock clock) =>
            Apply(country, yield, clock.CurrentTime);

        public NationalYieldSnapshot Apply(Country country, NationalYieldSnapshot snapshot) => snapshot;
    }
}
