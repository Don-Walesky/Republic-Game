namespace Republic.Core.Tests.Time;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Time;
using Xunit;

public sealed class RepublicClockTests
{
    private static readonly RepublicClockConfiguration DefaultConfig = new();

    [Fact]
    public void LaunchEpoch_InitializesAtDayZero_ZeroHoursWat()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        var time = clock.CurrentTime;

        Assert.Equal(0, time.DayNumber);
        Assert.True(time.IsLaunchDay);
        Assert.Equal(TimeSpan.Zero, time.ElapsedDuration);
        Assert.Equal(0, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal(0, time.Second);
        Assert.Equal(new DateOnly(2027, 2, 10), time.Date);
        Assert.Equal(new TimeOnly(0, 0, 0), time.Time);
        Assert.Equal(TimeSpan.FromHours(1), time.WatTimestamp.Offset);
        Assert.Equal(new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1)), time.WatTimestamp);
        Assert.Equal(new DateTimeOffset(2027, 2, 9, 23, 0, 0, TimeSpan.Zero), time.UtcTimestamp);
    }

    [Fact]
    public void OneMinuteAfterLaunch_AdvancesAccurately()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        clock.AdvanceMinutes(1);

        var time = clock.CurrentTime;

        Assert.Equal(0, time.DayNumber);
        Assert.True(time.IsLaunchDay);
        Assert.Equal(TimeSpan.FromMinutes(1), time.ElapsedDuration);
        Assert.Equal(0, time.Hour);
        Assert.Equal(1, time.Minute);
        Assert.Equal(0, time.Second);
        Assert.Equal(new DateOnly(2027, 2, 10), time.Date);
        Assert.Equal(new TimeOnly(0, 1, 0), time.Time);
    }

    [Fact]
    public void OneHourAfterLaunch_AdvancesAccurately()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        clock.AdvanceHours(1);

        var time = clock.CurrentTime;

        Assert.Equal(0, time.DayNumber);
        Assert.True(time.IsLaunchDay);
        Assert.Equal(TimeSpan.FromHours(1), time.ElapsedDuration);
        Assert.Equal(1, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal(new DateOnly(2027, 2, 10), time.Date);
        Assert.Equal(new DateTimeOffset(2027, 2, 10, 1, 0, 0, TimeSpan.FromHours(1)), time.WatTimestamp);
        Assert.Equal(new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.Zero), time.UtcTimestamp);
    }

    [Fact]
    public void At2359_RemainsDayZero()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        clock.Advance(new TimeSpan(23, 59, 0));

        var time = clock.CurrentTime;

        Assert.Equal(0, time.DayNumber);
        Assert.True(time.IsLaunchDay);
        Assert.Equal(23, time.Hour);
        Assert.Equal(59, time.Minute);
        Assert.Equal(new DateOnly(2027, 2, 10), time.Date);
        Assert.Equal(new TimeOnly(23, 59, 0), time.Time);
        Assert.Equal(TimeSpan.FromMinutes(23 * 60 + 59), time.ElapsedDuration);
    }

    [Fact]
    public void MidnightTransition_From2359ToNextDay_TransitionsToDayOne()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        clock.Advance(new TimeSpan(23, 59, 0));

        // Advance 1 more minute across midnight
        clock.AdvanceMinutes(1);

        var time = clock.CurrentTime;

        Assert.Equal(1, time.DayNumber);
        Assert.False(time.IsLaunchDay);
        Assert.Equal(0, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal(new DateOnly(2027, 2, 11), time.Date);
        Assert.Equal(new TimeOnly(0, 0, 0), time.Time);
        Assert.Equal(TimeSpan.FromDays(1), time.ElapsedDuration);
        Assert.Equal(new DateTimeOffset(2027, 2, 11, 0, 0, 0, TimeSpan.FromHours(1)), time.WatTimestamp);
    }

    [Fact]
    public void MultipleElapsedDays_CalculatesDayNumberAndGregorianDateAccurately()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);

        // Advance 7 days (1 week)
        clock.AdvanceDays(7);
        var week1 = clock.CurrentTime;
        Assert.Equal(7, week1.DayNumber);
        Assert.Equal(new DateOnly(2027, 2, 17), week1.Date);
        Assert.Equal(TimeSpan.FromDays(7), week1.ElapsedDuration);

        // Advance further across month boundary (February 2027 has 28 days)
        // 2027-02-10 + 20 days = 2027-03-02 (18 days to end of Feb + 2 days)
        clock.Reset();
        clock.AdvanceDays(20);
        var marchDate = clock.CurrentTime;
        Assert.Equal(20, marchDate.DayNumber);
        Assert.Equal(new DateOnly(2027, 3, 2), marchDate.Date);

        // Advance 365 days (1 full non-leap year)
        clock.Reset();
        clock.AdvanceDays(365);
        var year1 = clock.CurrentTime;
        Assert.Equal(365, year1.DayNumber);
        Assert.Equal(new DateOnly(2028, 2, 10), year1.Date);

        // Leap year handling: 2028 is a leap year (February 29 exists)
        // 2027-02-10 + 384 days = 2028-02-29
        clock.Reset();
        clock.AdvanceDays(384);
        var leapDay = clock.CurrentTime;
        Assert.Equal(384, leapDay.DayNumber);
        Assert.Equal(new DateOnly(2028, 2, 29), leapDay.Date);
    }

    [Fact]
    public void WatRepresentation_StrictlyFixedAtUtcPlusOne_NoDstDistortion()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);

        // Advance to typical Northern Hemisphere DST switch dates (late March and late October 2027)
        // March 28, 2027: 46 days after Feb 10
        clock.AdvanceDays(46);
        var springDstDate = clock.CurrentTime;
        Assert.Equal(TimeSpan.FromHours(1), springDstDate.WatTimestamp.Offset);

        // October 31, 2027: 263 days after Feb 10
        clock.Reset();
        clock.AdvanceDays(263);
        var autumnDstDate = clock.CurrentTime;
        Assert.Equal(TimeSpan.FromHours(1), autumnDstDate.WatTimestamp.Offset);

        // Every 24-hour step advances exactly 1 calendar day, never 23h or 25h
        var prevDate = clock.CurrentTime.Date;
        clock.AdvanceHours(24);
        Assert.Equal(prevDate.AddDays(1), clock.CurrentTime.Date);
        Assert.Equal(TimeSpan.FromHours(1), clock.CurrentTime.WatTimestamp.Offset);
    }

    [Fact]
    public void HostMachineTimezoneIndependence_PreservesUniversalWATConversion()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);

        // Timestamps representing the same absolute instant expressed in different time zones:
        // 2027-02-10 12:00:00 WAT (+01:00) is 11:00:00 UTC (+00:00), 06:00:00 EST (-05:00), 20:00:00 JST (+09:00)
        var utcInstant = new DateTimeOffset(2027, 2, 10, 11, 0, 0, TimeSpan.Zero);
        var estInstant = new DateTimeOffset(2027, 2, 10, 6, 0, 0, TimeSpan.FromHours(-5));
        var jstInstant = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.FromHours(9));

        var timeFromUtc = clock.ToRepublicTime(utcInstant);
        var timeFromEst = clock.ToRepublicTime(estInstant);
        var timeFromJst = clock.ToRepublicTime(jstInstant);

        Assert.Equal(timeFromUtc, timeFromEst);
        Assert.Equal(timeFromUtc, timeFromJst);

        Assert.Equal(0, timeFromUtc.DayNumber);
        Assert.Equal(12, timeFromUtc.Hour);
        Assert.Equal(0, timeFromUtc.Minute);
        Assert.Equal(new DateOnly(2027, 2, 10), timeFromUtc.Date);
        Assert.Equal(TimeSpan.FromHours(1), timeFromUtc.WatTimestamp.Offset);
        Assert.Equal(TimeSpan.FromHours(12), timeFromUtc.ElapsedDuration);
    }

    [Fact]
    public void DeterministicControlledClock_SupportsControlledAdvancementAndReset()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        Assert.True(clock.IsControlled);

        clock.AdvanceDays(3);
        clock.AdvanceHours(5);
        clock.AdvanceMinutes(30);
        clock.AdvanceSeconds(15);

        var time = clock.CurrentTime;
        Assert.Equal(3, time.DayNumber);
        Assert.Equal(5, time.Hour);
        Assert.Equal(30, time.Minute);
        Assert.Equal(15, time.Second);
        Assert.Equal(new TimeSpan(3, 5, 30, 15), time.ElapsedDuration);

        // Reset returns to Day 0, 00:00 WAT
        clock.Reset();
        Assert.Equal(clock.LaunchEpoch, clock.CurrentTime);
    }

    [Fact]
    public void FromDayAndTime_ConstructsExactTargetSimulationTime()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        var targetTime = clock.FromDayAndTime(42, new TimeOnly(18, 45, 30));

        Assert.Equal(42, targetTime.DayNumber);
        Assert.Equal(18, targetTime.Hour);
        Assert.Equal(45, targetTime.Minute);
        Assert.Equal(30, targetTime.Second);
        Assert.Equal(new DateOnly(2027, 2, 10).AddDays(42), targetTime.Date);
        Assert.Equal(TimeSpan.FromHours(1), targetTime.WatTimestamp.Offset);
    }

    [Fact]
    public void ComparisonAndOperators_BehaveChronologicallyAndDeterministically()
    {
        var clock = RepublicClock.CreateControlled(DefaultConfig);
        var t0 = clock.CurrentTime;
        var t1 = t0 + TimeSpan.FromHours(6);
        var t2 = t1 + TimeSpan.FromHours(18);

        Assert.True(t0 < t1);
        Assert.True(t1 <= t2);
        Assert.True(t2 > t1);
        Assert.True(t2 >= t0);
        Assert.True(t0 == clock.LaunchEpoch);
        Assert.False(t0 != clock.LaunchEpoch);
        Assert.True(t0 != t1);

        Assert.Equal(TimeSpan.FromHours(6), t1 - t0);
        Assert.Equal(TimeSpan.FromHours(24), t2 - t0);
        Assert.Equal(t1, t2 - TimeSpan.FromHours(18));

        // Array sorting
        var list = new[] { t2, t0, t1 };
        Array.Sort(list);
        Assert.Equal(new[] { t0, t1, t2 }, list);
    }

    [Fact]
    public async Task TimeSystemIntegration_SynchronizesWithRepublicClock()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var config = new TimeSystemConfiguration
        {
            TickRate = 1,
            TicksPerDay = 86400,
            DaysPerMonth = 30,
            MonthsPerYear = 12
        };

        var timeSystem = new TimeSystem(config, bus, logger);

        // Initial state at Day 0, 00:00 WAT
        Assert.Equal(0, timeSystem.CurrentRepublicTime.DayNumber);
        Assert.True(timeSystem.CurrentRepublicTime.IsLaunchDay);
        Assert.Equal(new DateOnly(2027, 2, 10), timeSystem.CurrentRepublicTime.Date);
        Assert.Equal(new TimeOnly(0, 0, 0), timeSystem.CurrentRepublicTime.Time);
        Assert.Equal(timeSystem.CurrentRepublicTime, timeSystem.Clock.CurrentTime);

        // Advance by 60 seconds (60 ticks = 1 minute of simulated time)
        await timeSystem.TickAsync(60.0);

        Assert.Equal(60UL, timeSystem.CurrentTick);
        Assert.Equal(TimeSpan.FromMinutes(1), timeSystem.ElapsedTime);
        Assert.Equal(0, timeSystem.CurrentRepublicTime.DayNumber);
        Assert.Equal(1, timeSystem.CurrentRepublicTime.Minute);
        Assert.Equal(new TimeOnly(0, 1, 0), timeSystem.CurrentRepublicTime.Time);
        Assert.Equal(timeSystem.CurrentRepublicTime, timeSystem.Clock.CurrentTime);
    }

    [Fact]
    public void RealTimeMode_WithFakeTimeProvider_CalculatesWatAuthoritatively()
    {
        var fakeProvider = new ControllableTimeProvider(
            new DateTimeOffset(2027, 2, 10, 11, 0, 0, TimeSpan.Zero)); // 11:00 UTC = 12:00 WAT

        var clock = RepublicClock.CreateRealTime(DefaultConfig, fakeProvider);
        Assert.False(clock.IsControlled);

        var time = clock.CurrentTime;
        Assert.Equal(0, time.DayNumber);
        Assert.Equal(12, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal(new DateOnly(2027, 2, 10), time.Date);
        Assert.Equal(TimeSpan.FromHours(12), time.ElapsedDuration);

        // Advance wall clock by 2 hours (13:00 UTC = 14:00 WAT)
        fakeProvider.Advance(TimeSpan.FromHours(2));
        var advanced = clock.CurrentTime;
        Assert.Equal(14, advanced.Hour);
        Assert.Equal(TimeSpan.FromHours(14), advanced.ElapsedDuration);
    }

    private sealed class ControllableTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ControllableTimeProvider(DateTimeOffset initialUtcNow)
        {
            _utcNow = initialUtcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan delta) => _utcNow += delta;
    }
}
