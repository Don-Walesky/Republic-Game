namespace Republic.Core.Tests.NationalYield;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class OfflineYieldCatchUpTests
{
    private readonly RepublicClock _clock;
    private readonly NationalYieldSchedule _schedule;
    private readonly NationalYieldCycleService _cycleService;
    private readonly OfflineYieldCatchUpService _catchUpService;

    public OfflineYieldCatchUpTests()
    {
        _clock = RepublicClock.CreateControlled();
        _schedule = new NationalYieldSchedule();
        _cycleService = new NationalYieldCycleService(schedule: _schedule);
        _catchUpService = new OfflineYieldCatchUpService(_cycleService, _schedule);
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
    public void Test1_TwelveHourGap_CreditsTwoBoundariesAndRepuMatchesTreasuryIncrease()
    {
        // Given a sovereign nation founded at Day 0, 00:00 WAT with starting treasury balance of R100
        var country = CreateTestCountry("Arcadia", treasury: 100.0);
        var initialBalance = country.Treasury.Balance;

        // When the president returns after a 12-hour offline gap (at Day 0, 12:00 WAT)
        var returnTime = _clock.FromDayAndTime(0, new TimeOnly(12, 0));
        var briefing = _catchUpService.CatchUp(country, returnTime);

        // Then exactly two 6-hour boundaries are credited (06:00 and 12:00)
        Assert.Equal(country.Id, briefing.CountryId);
        Assert.Equal(2, briefing.BoundariesCredited);
        Assert.Equal(0, briefing.RemainingBoundaries);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(0, 0)), briefing.FromTime);
        Assert.Equal(returnTime, briefing.ToTime);

        // The REPU deposited reported in the briefing matches the actual treasury increase
        var finalBalance = country.Treasury.Balance;
        var expectedIncrease = finalBalance - initialBalance;
        Assert.True(briefing.RepuDeposited > 0.0);
        Assert.Equal(expectedIncrease, briefing.RepuDeposited, precision: 4);

        // The next due boundary is correctly calculated as Day 0, 18:00 WAT
        var expectedNext = _clock.FromDayAndTime(0, new TimeOnly(18, 0));
        Assert.Equal(expectedNext, briefing.NextDueBoundary);
        Assert.Equal(returnTime, country.LastCreditedBoundary);

        // Formatting contains the REPU symbol "R"
        Assert.StartsWith("R", briefing.FormattedRepuDeposited);
        Assert.True(briefing.HasCredited);
    }

    [Fact]
    public void Test2_RunningCatchUpAgainImmediately_CreditsZeroAndDepositsNothing()
    {
        // Given a country that just caught up to Day 0, 12:00 WAT
        var country = CreateTestCountry("Arcadia", treasury: 50.0);
        var time1200 = _clock.FromDayAndTime(0, new TimeOnly(12, 0));
        var initialBriefing = _catchUpService.CatchUp(country, time1200);
        var balanceAfterFirstCatchUp = country.Treasury.Balance;
        Assert.Equal(2, initialBriefing.BoundariesCredited);

        // When running catch-up again immediately at Day 0, 12:00 WAT
        var immediateBriefing = _catchUpService.CatchUp(country, time1200);

        // Then zero boundaries are credited, zero REPU is deposited, and treasury is unchanged
        Assert.Equal(0, immediateBriefing.BoundariesCredited);
        Assert.Equal(0.0, immediateBriefing.RepuDeposited);
        Assert.Equal(0, immediateBriefing.RemainingBoundaries);
        Assert.Equal(balanceAfterFirstCatchUp, country.Treasury.Balance);
        Assert.False(immediateBriefing.HasCredited);
        Assert.Equal("R0", immediateBriefing.FormattedRepuDeposited);
        Assert.Equal(_clock.FromDayAndTime(0, new TimeOnly(18, 0)), immediateBriefing.NextDueBoundary);

        // Even with a completely new catch-up service instance (zero in-memory cache),
        // durable state in Country and RepuTreasury guarantees zero deposit
        var freshCatchUpService = new OfflineYieldCatchUpService();
        var freshBriefing = freshCatchUpService.CatchUp(country, time1200);
        Assert.Equal(0, freshBriefing.BoundariesCredited);
        Assert.Equal(0.0, freshBriefing.RepuDeposited);
        Assert.Equal(balanceAfterFirstCatchUp, country.Treasury.Balance);
    }

    [Fact]
    public void Test3_GapLargerThan48Hours_CreditsEightAndReportsRemainder()
    {
        // Given a country founded at Day 0, 00:00 WAT with R0 treasury
        var country = CreateTestCountry("Long Offline State", treasury: 0.0);

        // When the president returns after 72 hours (Day 3, 00:00 WAT), which corresponds to 12 due boundaries
        var returnTime = _clock.FromDayAndTime(3, new TimeOnly(0, 0));
        var briefing = _catchUpService.CatchUp(country, returnTime);

        // Then exactly 8 boundaries are credited (honoring the 8-boundary cap)
        Assert.Equal(8, briefing.BoundariesCredited);

        // The remaining 4 boundaries are explicitly reported in the briefing
        Assert.Equal(4, briefing.RemainingBoundaries);

        // The REPU deposited matches the treasury balance
        Assert.Equal(country.Treasury.Balance, briefing.RepuDeposited, precision: 4);

        // Country's last credited boundary is Day 2, 00:00 WAT (the 8th boundary)
        var eighthBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        Assert.Equal(eighthBoundary, country.LastCreditedBoundary);

        // The next due boundary is Day 2, 06:00 WAT (the 9th boundary)
        var ninthBoundary = _clock.FromDayAndTime(2, new TimeOnly(6, 0));
        Assert.Equal(ninthBoundary, briefing.NextDueBoundary);

        // A second catch-up execution processes the remaining 4 boundaries
        var secondBriefing = _catchUpService.CatchUp(country, returnTime);
        Assert.Equal(4, secondBriefing.BoundariesCredited);
        Assert.Equal(0, secondBriefing.RemainingBoundaries);
        Assert.Equal(returnTime, country.LastCreditedBoundary);
    }

    [Fact]
    public void Test4_CountryIsolation_CountryBDoesNotChangeWhenCountryACatchesUp()
    {
        // Given two distinct countries A and B sharing the global simulation clock
        var countryA = CreateTestCountry("Country A", treasury: 100.0);
        var countryB = CreateTestCountry("Country B", treasury: 500.0);

        var initialBalanceB = countryB.Treasury.Balance;
        Assert.Null(countryA.LastCreditedBoundary);
        Assert.Null(countryB.LastCreditedBoundary);

        // When Country A catches up after an 18-hour gap (3 boundaries)
        var returnTime = _clock.FromDayAndTime(0, new TimeOnly(18, 0));
        var briefingA = _catchUpService.CatchUp(countryA, returnTime);

        // Country A receives 3 boundaries and increased treasury
        Assert.Equal(3, briefingA.BoundariesCredited);
        Assert.True(countryA.Treasury.Balance > 100.0);
        Assert.Equal(returnTime, countryA.LastCreditedBoundary);

        // Country B is completely untouched
        Assert.Equal(initialBalanceB, countryB.Treasury.Balance);
        Assert.Null(countryB.LastCreditedBoundary);
        Assert.Empty(countryB.Treasury.AppliedSnapshotIds);

        // When Country B subsequently catches up, it executes independently
        var briefingB = _catchUpService.CatchUp(countryB, returnTime);
        Assert.Equal(3, briefingB.BoundariesCredited);
        Assert.True(countryB.Treasury.Balance > initialBalanceB);
        Assert.Equal(returnTime, countryB.LastCreditedBoundary);
    }

    [Fact]
    public void OfflineYieldBriefing_ValueObjectProperties_PreserveIntegrity()
    {
        var fromTime = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var toTime = _clock.FromDayAndTime(1, new TimeOnly(18, 0));
        var nextDue = _clock.FromDayAndTime(2, new TimeOnly(0, 0));

        var briefing = new OfflineYieldBriefing(
            countryId: "test-country",
            fromTime: fromTime,
            toTime: toTime,
            boundariesCredited: 2,
            repuDeposited: 45_000_000.0,
            remainingBoundaries: 0,
            nextDueBoundary: nextDue);

        Assert.Equal("test-country", briefing.CountryId);
        Assert.Equal(fromTime, briefing.FromTime);
        Assert.Equal(toTime, briefing.ToTime);
        Assert.Equal(2, briefing.BoundariesCredited);
        Assert.Equal(45_000_000.0, briefing.RepuDeposited);
        Assert.Equal("R45M", briefing.FormattedRepuDeposited);
        Assert.Equal(0, briefing.RemainingBoundaries);
        Assert.Equal(nextDue, briefing.NextDueBoundary);
        Assert.True(briefing.HasCredited);

        var text = briefing.ToString();
        Assert.Contains("test-country", text);
        Assert.Contains("R45M", text);
    }
}
