namespace Republic.Core.Events;

/// <summary>
/// Delegate signature for asynchronous event handlers.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <param name="event">Published event instance.</param>
/// <param name="cancellationToken">Cancellation token.</param>
public delegate ValueTask GameEventHandler<in TEvent>(TEvent @event, CancellationToken cancellationToken)
    where TEvent : IGameEvent;

/// <summary>
/// Queued event bus used by the simulation engine.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Gets the number of queued events awaiting processing.
    /// </summary>
    int PendingCount { get; }

    /// <summary>
    /// Registers a handler for the supplied event type.
    /// </summary>
    IDisposable Subscribe<TEvent>(GameEventHandler<TEvent> handler) where TEvent : IGameEvent;

    /// <summary>
    /// Removes a handler for the supplied event type.
    /// </summary>
    bool Unsubscribe<TEvent>(GameEventHandler<TEvent> handler) where TEvent : IGameEvent;

    /// <summary>
    /// Queues an event for later processing.
    /// </summary>
    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IGameEvent;

    /// <summary>
    /// Processes all queued events.
    /// </summary>
    Task<int> ProcessQueuedEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the subscriber count for the supplied event type.
    /// </summary>
    int GetSubscriberCount<TEvent>() where TEvent : IGameEvent;

    /// <summary>
    /// Gets cumulative metrics for the event bus.
    /// </summary>
    EventBusMetrics GetMetrics();
}
