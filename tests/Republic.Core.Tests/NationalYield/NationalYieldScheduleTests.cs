namespace Republic.Core.Tests.NationalYield;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalYieldScheduleTests
{
    private readonly RepublicClock _clock;
    private readonly NationalYieldSchedule _schedule;
    private readonly NationalYieldCycleService _cycleService;

    public NationalYieldScheduleTests()
    {
        _clock = RepublicClock.CreateControlled();
        _schedule = new NationalYieldSchedule();
        _cycleService = new NationalYieldCycleService(schedule: _schedule);
    }

    private Country CreateTestCountry(
        string name = "Arcadia",
        double stability = 80.0,
        double industry = 50.0,
        double treasury = 0.0,
        RepublicTime? foundingTime = null)
    {
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = industry,
            InfrastructureCapacity = 50.0,
            HumanCapital = 50.0,
            AdministrativeCapacity = 50.0,
            SecurityCapacity = 50.0,
            ScienceCapacity = 50.0
        };

        var time = foundingTime ?? _clock.CurrentTime;
        var country = Country.Found(
            name: name,
            foundingTime: time,
            baselineStability: stability,
            yield: yield,
            treasury: new RepuTreasury(treasury),
            stateCapacity: StateCapacityInputs.All(1.0));

        return country;
    }

    [Fact]
    public void Schedule_GetNextBoundary_AlignsToNextSixHourWatInterval()
    {
        // Aligned to 00:00, 06:00, 12:00, and 18:00 WAT
        var day0Midnight = _clock.FromDayAndTime(0, new TimeOnly(0, 0));
        var day0Morning = _clock.FromDayAndTime(0, new TimeOnly(2, 30));
        var day0At0600 = _clock.FromDayAndTime(0, new TimeOnly(6, 0));
        var day0At0601 = _clock.FromDayAndTime(0, new TimeOnly(6, 1));
        var day0At1200 = _clock.FromDayAndTime(0, new TimeOnly(12, 0));
        var day0At1800 = _clock.FromDayAndTime(0, new TimeOnly(18, 0));
        var day0At2359 = _clock.FromDayAndTime(0, new TimeOnly(23, 59, 59));

        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(6, 0)), _schedule.GetNextBoundary(day0Midnight));
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(6, 0)), _schedule.GetNextBoundary(day0Morning));
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(12, 0)), _schedule.GetNextBoundary(day0At0600));
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(12, 0)), _schedule.GetNextBoundary(day0At0601));
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(18, 0)), _schedule.GetNextBoundary(day0At1200));
        Assert.Equal(_clock.FromDayAndTime(1, new TimeOnly(0, 0)), _schedule.GetNextBoundary(day0At1800));
        Assert.Equal(_clock.FromDayAndTime(1, new TimeOnly(0, 0)), _schedule.GetNextBoundary(day0At2359));
    }

    [Fact]
    public void Test1_NoBoundaryDue_MeansNoDeposit()
    {
        // Given a country founded at Day 0, 00:00 WAT with a treasury balance of R100
        var country = CreateTestCountry("Test Nation", treasury: 100.0);
        var initialBalance = country.Treasury.Balance;

        // When simulation time is at 04:00 (has not reached the 06:00 boundary)
        var simTime = _clock.FromDayAndTime(0, new TimeOnly(4, 0));
        var dueBoundaries = _cycleService.GetDueBoundaries(country, simTime);
        var executedSnapshots = _cycleService.ExecuteDueCycles(country, simTime);

        // Then no boundaries are due, no cycles execute, and no deposit is made
        Assert.Empty(dueBoundaries);
        Assert.Empty(executedSnapshots);
        Assert.Equal(initialBalance, country.Treasury.Balance);
        Assert.Null(country.LastCreditedBoundary);
    }

    [Fact]
    public void Test2_OneDepositAt0600_AndSecondCallAt0601DoesNotDepositAgain()
    {
        // Given a country founded at Day 0, 00:00 WAT with R0 treasury
        var country = CreateTestCountry("Test Nation", treasury: 0.0);

        // When simulation time reaches exactly 06:00 WAT
        var time0600 = _clock.FromDayAndTime(0, new TimeOnly(6, 0));
        var snapshotsAt0600 = _cycleService.ExecuteDueCycles(country, time0600);

        // Then exactly one boundary (06:00) is credited
        Assert.Single(snapshotsAt0600);
        Assert.Equal(time0600, snapshotsAt0600[0].SimulationTime);
        Assert.Equal(time0600, country.LastCreditedBoundary);

        var balanceAfter0600 = country.Treasury.Balance;
        Assert.True(balanceAfter0600 > 0.0);
        Assert.Single(country.Treasury.AppliedSnapshotIds);

        // When a second call is made at 06:01 WAT
        var time0601 = _clock.FromDayAndTime(0, new TimeOnly(6, 1));
        var dueBoundariesAt0601 = _cycleService.GetDueBoundaries(country, time0601);
        var snapshotsAt0601 = _cycleService.ExecuteDueCycles(country, time0601);

        // Then no boundaries are due, no new cycles execute, and treasury does NOT deposit again
        Assert.Empty(dueBoundariesAt0601);
        Assert.Empty(snapshotsAt0601);
        Assert.Equal(balanceAfter0600, country.Treasury.Balance);
        Assert.Equal(time0600, country.LastCreditedBoundary);
        Assert.Single(country.Treasury.AppliedSnapshotIds);
    }

    [Fact]
    public void Test3_From0000To1800_ThreeBoundariesCreditAndFourthDoesNot()
    {
        // Given a country founded at Day 0, 00:00 WAT
        var country = CreateTestCountry("Test Nation", treasury: 0.0);

        // When simulation time advances from 00:00 to 18:00 WAT
        var time1800 = _clock.FromDayAndTime(0, new TimeOnly(18, 0));
        var dueBoundaries = _cycleService.GetDueBoundaries(country, time1800);
        var executedSnapshots = _cycleService.ExecuteDueCycles(country, time1800);

        // Then exactly three boundaries credit: 06:00, 12:00, and 18:00
        Assert.Equal(3, dueBoundaries.Count);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(6, 0)), dueBoundaries[0]);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(12, 0)), dueBoundaries[1]);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(18, 0)), dueBoundaries[2]);

        Assert.Equal(3, executedSnapshots.Count);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(18, 0)), country.LastCreditedBoundary);

        // The fourth boundary (Day 1, 00:00 WAT) is NOT credited
        Assert.DoesNotContain(executedSnapshots, s => s.SimulationTime == _clock.FromDayAndTime(1, new TimeOnly(0, 0)));
        Assert.Equal(3, country.Treasury.AppliedSnapshotIds.Count);

        // Treasury balance equals the sum of the 3 credited boundaries
        var expectedBalance = executedSnapshots[0].Yield.RepuTreasuryRevenue +
                              executedSnapshots[1].Yield.RepuTreasuryRevenue +
                              executedSnapshots[2].Yield.RepuTreasuryRevenue;
        Assert.Equal(expectedBalance, country.Treasury.Balance, precision: 4);
    }

    [Fact]
    public void Test4_CountryBIsUnchangedWhenCountryAAdvances()
    {
        // Given two distinct countries A and B sharing the simulation clock
        var countryA = CreateTestCountry("Country A", treasury: 100.0);
        var countryB = CreateTestCountry("Country B", treasury: 250.0);

        var initialBalanceB = countryB.Treasury.Balance;
        Assert.Null(countryA.LastCreditedBoundary);
        Assert.Null(countryB.LastCreditedBoundary);

        // When simulation time reaches 12:00 WAT and due cycles are executed ONLY for Country A
        var time1200 = _clock.FromDayAndTime(0, new TimeOnly(12, 0));
        var snapshotsA = _cycleService.ExecuteDueCycles(countryA, time1200);

        // Then Country A receives two cycles (06:00 and 12:00) and advances
        Assert.Equal(2, snapshotsA.Count);
        Assert.Equal(time1200, countryA.LastCreditedBoundary);
        Assert.True(countryA.Treasury.Balance > 100.0);

        // Country B is strictly unchanged
        Assert.Equal(initialBalanceB, countryB.Treasury.Balance);
        Assert.Null(countryB.LastCreditedBoundary);
        Assert.Empty(countryB.Treasury.AppliedSnapshotIds);

        // When Country B later executes due cycles at 12:00 WAT
        var snapshotsB = _cycleService.ExecuteDueCycles(countryB, time1200);

        // Then Country B independently receives its credits
        Assert.Equal(2, snapshotsB.Count);
        Assert.Equal(time1200, countryB.LastCreditedBoundary);
        Assert.True(countryB.Treasury.Balance > initialBalanceB);
    }

    [Fact]
    public void Test5_EightBoundaryCapIsHonored()
    {
        // Given a country starting at Day 0, 00:00 WAT
        var country = CreateTestCountry("Long Offline Nation", treasury: 0.0);

        // When simulation time advances far into the future (Day 5, 00:00 WAT = 20 boundaries due)
        var farFuture = _clock.FromDayAndTime(5, new TimeOnly(0, 0));
        var dueBoundaries = _cycleService.GetDueBoundaries(country, farFuture);
        var snapshots = _cycleService.ExecuteDueCycles(country, farFuture);

        // Then exactly 8 boundaries are returned and executed (honoring the cap of 8)
        Assert.Equal(8, dueBoundaries.Count);
        Assert.Equal(8, snapshots.Count);
        Assert.Equal(8, country.Treasury.AppliedSnapshotIds.Count);

        // The 8th boundary corresponds to Day 2, 00:00 WAT
        // (Boundaries: Day0 06:00, 12:00, 18:00, Day1 00:00, 06:00, 12:00, 18:00, Day2 00:00)
        var expectedLastBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        Assert.Equal(expectedLastBoundary, country.LastCreditedBoundary);

        // A second execution call can process the next chunk of due boundaries up to 8
        var secondBatch = _cycleService.ExecuteDueCycles(country, farFuture);
        Assert.Equal(8, secondBatch.Count);
        Assert.Equal(16, country.Treasury.AppliedSnapshotIds.Count);
    }

    [Fact]
    public void ReExecutingExactSameBoundary_DoesNotDoubleCreditTreasury()
    {
        // Given a country whose 06:00 boundary was already credited
        var country = CreateTestCountry("Idempotency Nation", treasury: 0.0);
        var time0600 = _clock.FromDayAndTime(0, new TimeOnly(6, 0));

        var snapshots = _cycleService.ExecuteDueCycles(country, time0600);
        Assert.Single(snapshots);
        var balance = country.Treasury.Balance;

        // Even if an external caller directly calls ExecuteCycle with the exact same 06:00 boundary time
        var manualSnapshot = _cycleService.ExecuteCycle(country, time0600);

        // The snapshot ID is deterministic (countryId:Day0:060000.0000000) and treasury prevents double-credit
        Assert.Equal(snapshots[0].Id, manualSnapshot.Id);
        Assert.Equal(balance, country.Treasury.Balance);
    }

    [Fact]
    public void FoundingDayScaling_AppliesOnlyOnFoundingDayBoundaries()
    {
        // Founding day is Day 0. Boundaries on Day 0 receive x10 founding-day multiplier.
        // Boundaries on Day 1 receive normal x1 multiplier without any redundant scaling in scheduler.
        var country = CreateTestCountry("Founding Buff Nation", treasury: 0.0);

        var timeDay1Morning = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var snapshots = _cycleService.ExecuteDueCycles(country, timeDay1Morning);

        // Boundaries: Day0 06:00, 12:00, 18:00, Day1 00:00, Day1 06:00 (5 total)
        Assert.Equal(5, snapshots.Count);

        // Snapshots on Day 0 have x10 multiplier applied
        Assert.True(country.IsInFoundingDay(snapshots[0].SimulationTime));
        Assert.True(country.IsInFoundingDay(snapshots[1].SimulationTime));
        Assert.True(country.IsInFoundingDay(snapshots[2].SimulationTime));

        // Snapshots on Day 1 do NOT have founding day status
        Assert.False(country.IsInFoundingDay(snapshots[3].SimulationTime));
        Assert.False(country.IsInFoundingDay(snapshots[4].SimulationTime));

        // Day 0 revenue is 10x greater than Day 1 revenue for the same underlying state
        var day0Revenue = snapshots[0].Yield.RepuTreasuryRevenue;
        var day1Revenue = snapshots[4].Yield.RepuTreasuryRevenue;
        Assert.Equal(day0Revenue, day1Revenue * 10.0, precision: 4);
    }
}
