namespace Republic.Core.Tests;

using Republic.Core.Events;

public sealed class EventBusTests
{
    [Fact]
    public async Task PublishAsync_DeliversToMultipleSubscribers()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var calls = 0;
        bus.Subscribe<PingEvent>((_, _) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });
        bus.Subscribe<PingEvent>((_, _) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });

        await bus.PublishAsync(new PingEvent(DateTimeOffset.UtcNow));
        var processed = await bus.ProcessQueuedEventsAsync();

        Assert.Equal(1, processed);
        Assert.Equal(2, calls);
        Assert.Equal(2, bus.GetSubscriberCount<PingEvent>());
    }

    [Fact]
    public async Task PublishAsync_DropsEventsWhenQueueOverflows()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions { MaxQueueSize = 1 }, logger);

        await bus.PublishAsync(new PingEvent(DateTimeOffset.UtcNow));
        await bus.PublishAsync(new PingEvent(DateTimeOffset.UtcNow));

        var metrics = bus.GetMetrics();
        Assert.Equal(1, metrics.DroppedCount);
        Assert.Contains(logger.Entries, entry => entry.Level == Republic.Core.Diagnostics.LogLevel.Warning);
    }

    [Fact]
    public async Task ProcessQueuedEventsAsync_LogsHandlerFailuresAndContinues()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var invoked = false;
        bus.Subscribe<PingEvent>((_, _) => throw new InvalidOperationException("boom"));
        bus.Subscribe<PingEvent>((_, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        await bus.PublishAsync(new PingEvent(DateTimeOffset.UtcNow));
        await bus.ProcessQueuedEventsAsync();

        Assert.True(invoked);
        Assert.Contains(logger.Entries, entry => entry.Level == Republic.Core.Diagnostics.LogLevel.Error);
    }

    [Fact]
    public async Task Unsubscribe_RemovesHandler()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        GameEventHandler<PingEvent> handler = (_, _) => ValueTask.CompletedTask;
        bus.Subscribe(handler).Dispose();

        await bus.PublishAsync(new PingEvent(DateTimeOffset.UtcNow));
        await bus.ProcessQueuedEventsAsync();

        Assert.Equal(0, bus.GetSubscriberCount<PingEvent>());
    }

    public sealed record PingEvent(DateTimeOffset OccurredAt) : IGameEvent;
}
