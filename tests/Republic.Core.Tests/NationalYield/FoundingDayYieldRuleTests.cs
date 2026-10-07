namespace Republic.Core.Tests.NationalYield;

using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class FoundingDayYieldRuleTests
{
    private readonly RepublicClock _clock;
    private readonly FoundingDayYieldRule _rule;

    public FoundingDayYieldRuleTests()
    {
        _clock = RepublicClock.CreateControlled();
        _rule = new FoundingDayYieldRule();
    }

    private static Country CreateCountryAt(RepublicTime foundingTime, string name = "Republic of Testing")
    {
        return Country.Found(
            name: name,
            foundingTime: foundingTime,
            baselineStability: 75.0);
    }

    private static Republic.Core.NationalYield.NationalYield CreateSampleYield(double baseValue = 50.0)
    {
        return new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = baseValue,
            IndustrialCapacity = baseValue,
            EnergyCapacity = baseValue,
            InfrastructureCapacity = baseValue,
            FinancialCapacity = baseValue,
            HumanCapital = baseValue,
            ScienceCapacity = baseValue,
            InnovationCapacity = baseValue,
            AdministrativeCapacity = baseValue,
            SecurityCapacity = baseValue,
            IntelligenceCapacity = baseValue,
            MilitaryReadiness = baseValue,
            DiplomaticCapacity = baseValue,
            NaturalResourceOutput = baseValue
        };
    }

    [Fact]
    public void Test1_NewlyFoundedCountry_WithinFirstRepublicDay_IsIdentifiedAsInFoundingDay()
    {
        // Country founded on Day 5 at 10:00 WAT
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = CreateCountryAt(foundingTime);

        // 1. Exact moment of founding (Day 5, 10:00 WAT)
        Assert.True(_rule.IsFoundingDay(country, foundingTime));
        Assert.True(country.IsInFoundingDay(foundingTime));
        Assert.Equal(10.0, _rule.GetMultiplier(country, foundingTime));

        // 2. Later during the same day (Day 5, 18:30 WAT)
        var afternoonTime = _clock.FromDayAndTime(5, new TimeOnly(18, 30));
        Assert.True(_rule.IsFoundingDay(country, afternoonTime));
        Assert.True(country.IsInFoundingDay(afternoonTime));
        Assert.Equal(10.0, _rule.GetMultiplier(country, afternoonTime));

        // 3. Final minute of the founding day (Day 5, 23:59:59 WAT)
        var endOfDayTime = _clock.FromDayAndTime(5, new TimeOnly(23, 59, 59));
        Assert.True(_rule.IsFoundingDay(country, endOfDayTime));
        Assert.True(country.IsInFoundingDay(endOfDayTime));
        Assert.Equal(10.0, _rule.GetMultiplier(country, endOfDayTime));
    }

    [Fact]
    public void Test2_CountryAfterFirstRepublicDay_IsNotInFoundingDay()
    {
        // Country founded on Day 5 at 10:00 WAT
        var foundingTime = _clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var country = CreateCountryAt(foundingTime);

        // 1. Day 6 at 00:00 WAT (exact start of next calendar day)
        var nextDayStart = _clock.FromDayAndTime(6, new TimeOnly(0, 0));
        Assert.False(_rule.IsFoundingDay(country, nextDayStart));
        Assert.False(country.IsInFoundingDay(nextDayStart));
        Assert.Equal(1.0, _rule.GetMultiplier(country, nextDayStart));

        // 2. Day 6 afternoon
        var nextDayNoon = _clock.FromDayAndTime(6, new TimeOnly(12, 0));
        Assert.False(_rule.IsFoundingDay(country, nextDayNoon));
        Assert.False(country.IsInFoundingDay(nextDayNoon));
        Assert.Equal(1.0, _rule.GetMultiplier(country, nextDayNoon));

        // 3. Far in the future (Day 42)
        var farFuture = _clock.FromDayAndTime(42, new TimeOnly(12, 0));
        Assert.False(_rule.IsFoundingDay(country, farFuture));
        Assert.False(country.IsInFoundingDay(farFuture));
        Assert.Equal(1.0, _rule.GetMultiplier(country, farFuture));
    }

    [Fact]
    public void Test3_FoundingDayYieldResult_ReceivesX10Multiplier_AcrossAll14Categories()
    {
        var foundingTime = _clock.FromDayAndTime(2, new TimeOnly(8, 0));
        var country = CreateCountryAt(foundingTime);
        var sampleYield = CreateSampleYield(25.0);

        var evalTime = _clock.FromDayAndTime(2, new TimeOnly(14, 0));
        var scaledYield = _rule.Apply(country, sampleYield, evalTime);

        // All 14 canonical categories must receive the exact x10 multiplier (25.0 * 10 = 250.0)
        Assert.Equal(250.0, scaledYield.RepuTreasuryRevenue);
        Assert.Equal(250.0, scaledYield.IndustrialCapacity);
        Assert.Equal(250.0, scaledYield.EnergyCapacity);
        Assert.Equal(250.0, scaledYield.InfrastructureCapacity);
        Assert.Equal(250.0, scaledYield.FinancialCapacity);
        Assert.Equal(250.0, scaledYield.HumanCapital);
        Assert.Equal(250.0, scaledYield.ScienceCapacity);
        Assert.Equal(250.0, scaledYield.InnovationCapacity);
        Assert.Equal(250.0, scaledYield.AdministrativeCapacity);
        Assert.Equal(250.0, scaledYield.SecurityCapacity);
        Assert.Equal(250.0, scaledYield.IntelligenceCapacity);
        Assert.Equal(250.0, scaledYield.MilitaryReadiness);
        Assert.Equal(250.0, scaledYield.DiplomaticCapacity);
        Assert.Equal(250.0, scaledYield.NaturalResourceOutput);

        // Test with snapshot overload
        var snapshot = new NationalYieldSnapshot(country.Id, evalTime, sampleYield);
        var scaledSnapshot = _rule.Apply(country, snapshot);
        Assert.Equal(evalTime, scaledSnapshot.SimulationTime);
        Assert.Equal(country.Id, scaledSnapshot.CountryId);
        Assert.Equal(250.0, scaledSnapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(250.0, scaledSnapshot.Yield.IndustrialCapacity);
    }

    [Fact]
    public void Test4_NonFoundingDayYieldResult_RemainsUnchanged()
    {
        var foundingTime = _clock.FromDayAndTime(2, new TimeOnly(8, 0));
        var country = CreateCountryAt(foundingTime);
        var sampleYield = CreateSampleYield(50.0);

        // Simulation time is on Day 3 (after founding day)
        var postFoundingTime = _clock.FromDayAndTime(3, new TimeOnly(12, 0));
        var resultYield = _rule.Apply(country, sampleYield, postFoundingTime);

        // Yield values remain unchanged (1x multiplier)
        Assert.Equal(50.0, resultYield.RepuTreasuryRevenue);
        Assert.Equal(50.0, resultYield.IndustrialCapacity);
        Assert.Equal(50.0, resultYield.HumanCapital);
        Assert.Equal(50.0, resultYield.AdministrativeCapacity);
    }

    [Fact]
    public void Test5_OriginalUnmodifiedYieldCalculation_RemainsUnchanged()
    {
        var foundingTime = _clock.FromDayAndTime(1, new TimeOnly(10, 0));
        var country = CreateCountryAt(foundingTime);
        var originalYield = CreateSampleYield(30.0);

        var resultYield = _rule.Apply(country, originalYield, foundingTime);

        // The returned yield is scaled by x10
        Assert.Equal(300.0, resultYield.RepuTreasuryRevenue);

        // The original yield instance is NOT mutated in place
        Assert.NotSame(originalYield, resultYield);
        Assert.Equal(30.0, originalYield.RepuTreasuryRevenue);
        Assert.Equal(30.0, originalYield.IndustrialCapacity);
    }

    [Fact]
    public void Test6_ExactWATDayBoundary_IsRespected_NotSimply24HoursAfterLogin()
    {
        // Country founded late in the day: Day 10 at 23:45 WAT
        var lateFoundingTime = _clock.FromDayAndTime(10, new TimeOnly(23, 45));
        var country = CreateCountryAt(lateFoundingTime);

        // 1. Just 10 minutes later, still on Day 10 (Day 10, 23:55 WAT) -> IN FOUNDING DAY
        var sameDayTime = _clock.FromDayAndTime(10, new TimeOnly(23, 55));
        Assert.True(_rule.IsFoundingDay(country, sameDayTime));
        Assert.Equal(10.0, _rule.GetMultiplier(country, sameDayTime));

        // 2. Only 20 minutes after founding in elapsed time, but the WAT calendar day has rolled over to Day 11 (00:05 WAT)!
        // If it were a crude 24-hour window, it would still be active.
        // Because it is based on the country's actual Independence Day in the Republic calendar (WAT boundary), it is NOT in founding day!
        var nextDayTime = _clock.FromDayAndTime(11, new TimeOnly(0, 5));
        Assert.False(_rule.IsFoundingDay(country, nextDayTime));
        Assert.Equal(1.0, _rule.GetMultiplier(country, nextDayTime));

        // 3. Time before founding (Day 10, 22:00 WAT) -> NOT in founding day (nation not proclaimed yet)
        var beforeFoundingTime = _clock.FromDayAndTime(10, new TimeOnly(22, 0));
        Assert.False(_rule.IsFoundingDay(country, beforeFoundingTime));
        Assert.Equal(1.0, _rule.GetMultiplier(country, beforeFoundingTime));
    }

    [Fact]
    public void Test7_DifferentCountries_RemainIsolated()
    {
        // Country A founded on Day 10
        var countryA = CreateCountryAt(_clock.FromDayAndTime(10, new TimeOnly(8, 0)), "Country A");

        // Country B founded on Day 2
        var countryB = CreateCountryAt(_clock.FromDayAndTime(2, new TimeOnly(8, 0)), "Country B");

        var simulationTime = _clock.FromDayAndTime(10, new TimeOnly(12, 0));
        var baseYield = CreateSampleYield(40.0);

        // Country A is in its founding day -> x10 multiplier (400.0)
        Assert.True(_rule.IsFoundingDay(countryA, simulationTime));
        Assert.Equal(10.0, _rule.GetMultiplier(countryA, simulationTime));
        var yieldA = _rule.Apply(countryA, baseYield, simulationTime);
        Assert.Equal(400.0, yieldA.IndustrialCapacity);

        // Country B is on Day 10 (its 9th day, not founding day) -> 1x multiplier (40.0)
        Assert.False(_rule.IsFoundingDay(countryB, simulationTime));
        Assert.Equal(1.0, _rule.GetMultiplier(countryB, simulationTime));
        var yieldB = _rule.Apply(countryB, baseYield, simulationTime);
        Assert.Equal(40.0, yieldB.IndustrialCapacity);
    }

    [Fact]
    public void RuleApplication_DoesNotModifyTreasuryBalanceOrCreateFreeREPU()
    {
        var foundingTime = _clock.FromDayAndTime(0, new TimeOnly(0, 0));
        var country = CreateCountryAt(foundingTime);
        var initialBalance = country.Treasury.Balance; // R0

        var yield = CreateSampleYield(100.0);
        var scaled = _rule.Apply(country, yield, foundingTime);

        // Yield output is scaled to 1000.0
        Assert.Equal(1000.0, scaled.RepuTreasuryRevenue);

        // Treasury balance remains strictly unmodified (R0)
        Assert.Equal(initialBalance, country.Treasury.Balance);
    }

    [Fact]
    public void ClockOverload_EvaluatesAgainstClockCurrentTime()
    {
        var foundingTime = _clock.FromDayAndTime(3, new TimeOnly(12, 0));
        var country = CreateCountryAt(foundingTime);

        _clock.SetTime(foundingTime);
        Assert.True(_rule.IsFoundingDay(country, _clock));

        var yield = CreateSampleYield(20.0);
        var scaled = _rule.Apply(country, yield, _clock);
        Assert.Equal(200.0, scaled.RepuTreasuryRevenue);
    }
}
