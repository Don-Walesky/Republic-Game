namespace Republic.Core.Tests;

using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Engine;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World;

public sealed class CalendarAndEngineTests
{
    [Fact]
    public void AdvanceDay_WrapsMonthAndYear()
    {
        var calendar = new GameCalendar(1, 1);

        var result = calendar.AdvanceDay();

        Assert.True(result.DayChanged);
        Assert.True(result.MonthChanged);
        Assert.True(result.YearChanged);
        Assert.Equal(new GameDate(1, 1, 2), result.Date);
    }

    [Fact]
    public void Restore_SetsCurrentDate()
    {
        var calendar = new GameCalendar(30, 12);
        var date = new GameDate(5, 6, 7);

        calendar.Restore(date);

        Assert.Equal(date, calendar.CurrentDate);
    }

    [Fact]
    public async Task Engine_RunAsync_AdvancesWorldTick()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var world = new WorldManager(bus);
        var time = new TimeSystem(new TimeSystemConfiguration { TickRate = 10, TicksPerDay = 50 }, bus, logger);
        var engine = new RepublicEngine(
            new EngineConfiguration { StartupFrameCount = 2, FrameDeltaMilliseconds = 100, WorldName = "Engine Test" },
            logger,
            time,
            bus,
            world);

        await engine.InitializeAsync();
        await engine.RunAsync(2, TimeSpan.FromMilliseconds(100));

        Assert.Equal<ulong>(2, world.Current.CurrentTick);
        Assert.Equal("Engine Test", world.Current.Name);
    }

    [Fact]
    public async Task Engine_RunAsync_RequiresInitialization()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var engine = new RepublicEngine(
            new EngineConfiguration(),
            logger,
            new TimeSystem(new TimeSystemConfiguration(), bus, logger),
            bus,
            new WorldManager(bus));

        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RunAsync(1, TimeSpan.FromMilliseconds(16)));
    }

    [Fact]
    public void ConsoleLogSink_WritesToConsole()
    {
        var sink = new ConsoleLogSink();
        using var writer = new StringWriter();
        var original = Console.Out;
        Console.SetOut(writer);

        try
        {
            sink.Write(new LogEntry(DateTimeOffset.UnixEpoch, LogLevel.Info, "console test"), false);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Contains("console test", writer.ToString());
    }

    [Fact]
    public void TimeEventRecords_ExposePayloadData()
    {
        var tickEvent = new SimulationTickEvent(1, 0.1d, new GameDate(1, 1, 1), DateTimeOffset.UnixEpoch);
        var secondEvent = new SimulationSecondEvent(60, 1, DateTimeOffset.UnixEpoch);
        var dayEvent = new SimulationDayEvent(new GameDate(2, 1, 1), 120, DateTimeOffset.UnixEpoch);
        var monthEvent = new SimulationMonthEvent(new GameDate(1, 2, 1), 240, DateTimeOffset.UnixEpoch);
        var yearEvent = new SimulationYearEvent(new GameDate(1, 1, 2), 480, DateTimeOffset.UnixEpoch);
        var pausedEvent = new SimulationPausedEvent(DateTimeOffset.UnixEpoch);
        var resumedEvent = new SimulationResumedEvent(DateTimeOffset.UnixEpoch);
        var scaleEvent = new TimeScaleChangedEvent(2d, DateTimeOffset.UnixEpoch);

        Assert.Equal(1UL, tickEvent.TickNumber);
        Assert.Equal(1UL, secondEvent.ElapsedSeconds);
        Assert.Equal(2, dayEvent.CurrentDate.Day);
        Assert.Equal(2, monthEvent.CurrentDate.Month);
        Assert.Equal(2, yearEvent.CurrentDate.Year);
        Assert.Equal(DateTimeOffset.UnixEpoch, pausedEvent.OccurredAt);
        Assert.Equal(DateTimeOffset.UnixEpoch, resumedEvent.OccurredAt);
        Assert.Equal(2d, scaleEvent.TimeScale);
    }
}
