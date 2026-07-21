namespace Republic.Core.Tests;

using Republic.Core.Events;
using Republic.Core.Time;

public sealed class TimeSystemTests
{
    [Fact]
    public async Task TickAsync_AdvancesAtFixedRate()
    {
        var system = CreateSystem();

        await system.TickAsync(0.5d);

        Assert.Equal<ulong>(5, system.CurrentTick);
        Assert.Equal(TimeSpan.FromSeconds(0.5d), system.ElapsedTime);
    }

    [Fact]
    public async Task PauseAsync_StopsAdvancement()
    {
        var system = CreateSystem();
        await system.PauseAsync();

        await system.TickAsync(1d);

        Assert.Equal<ulong>(0, system.CurrentTick);
        Assert.True(system.IsPaused);
    }

    [Fact]
    public async Task SetTimeScaleAsync_ClampsNegativeValues()
    {
        var system = CreateSystem();

        await system.SetTimeScaleAsync(-2d);
        await system.TickAsync(1d);

        Assert.Equal(0d, system.TimeScale);
        Assert.Equal<ulong>(0, system.CurrentTick);
    }

    [Fact]
    public async Task TickAsync_EmitsCalendarEvents()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var days = 0;
        var months = 0;
        bus.Subscribe<SimulationDayEvent>((_, _) =>
        {
            days++;
            return ValueTask.CompletedTask;
        });
        bus.Subscribe<SimulationMonthEvent>((_, _) =>
        {
            months++;
            return ValueTask.CompletedTask;
        });

        var system = new TimeSystem(new TimeSystemConfiguration { TickRate = 10, TicksPerDay = 5, DaysPerMonth = 2, MonthsPerYear = 12 }, bus, logger);
        await system.TickAsync(1d);
        await bus.ProcessQueuedEventsAsync();

        Assert.Equal(2, days);
        Assert.Equal(1, months);
        Assert.Equal(new GameDate(1, 2, 1), system.CurrentDate);
    }

    [Fact]
    public async Task SerializeAndDeserialize_RoundTripState()
    {
        var system = CreateSystem();
        await system.SetTimeScaleAsync(2d);
        await system.TickAsync(0.4d);
        var state = system.Serialize();

        var restored = CreateSystem();
        restored.Deserialize(state);

        Assert.Equal(system.CurrentTick, restored.CurrentTick);
        Assert.Equal(system.CurrentDate, restored.CurrentDate);
        Assert.Equal(system.TimeScale, restored.TimeScale);
    }

    [Fact]
    public async Task ResumeAsync_AllowsAdvancementAfterPause()
    {
        var system = CreateSystem();
        await system.PauseAsync();
        await system.ResumeAsync();

        await system.TickAsync(0.2d);

        Assert.False(system.IsPaused);
        Assert.Equal<ulong>(2, system.CurrentTick);
    }

    [Fact]
    public async Task TickAsync_ThrowsForNegativeDelta()
    {
        var system = CreateSystem();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => system.TickAsync(-0.1d).AsTask());
    }

    private static TimeSystem CreateSystem()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        return new TimeSystem(new TimeSystemConfiguration { TickRate = 10, TicksPerDay = 5, DaysPerMonth = 2, MonthsPerYear = 2 }, bus, logger);
    }
}
