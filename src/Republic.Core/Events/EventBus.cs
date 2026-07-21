namespace Republic.Core.Events;

using Republic.Core.Diagnostics;

/// <summary>
/// In-memory event queue and dispatcher.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();
    private readonly Queue<IGameEvent> _queue = new();
    private readonly EventBusOptions _options;
    private readonly ILogger _logger;
    private readonly object _syncRoot = new();
    private long _publishedCount;
    private long _processedCount;
    private long _droppedCount;
    private long _failedHandlerCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class.
    /// </summary>
    public EventBus(EventBusOptions options, ILogger logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int PendingCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _queue.Count;
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(GameEventHandler<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_syncRoot)
        {
            var eventType = typeof(TEvent);
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscriptions[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        return new Subscription<TEvent>(this, handler);
    }

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(GameEventHandler<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_syncRoot)
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                return false;
            }

            return handlers.Remove(handler);
        }
    }

    /// <inheritdoc />
    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_queue.Count >= _options.MaxQueueSize)
            {
                _droppedCount++;
                _logger.LogWarning($"Event queue overflow. Dropping {@event.GetType().Name}.");
                return ValueTask.CompletedTask;
            }

            _queue.Enqueue(@event);
            _publishedCount++;
        }

        _logger.LogDebug($"Queued event {@event.GetType().Name}.");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> ProcessQueuedEventsAsync(CancellationToken cancellationToken = default)
    {
        var processed = 0;

        while (true)
        {
            IGameEvent? nextEvent;
            List<Delegate>? handlers;

            lock (_syncRoot)
            {
                if (_queue.Count == 0)
                {
                    return processed;
                }

                nextEvent = _queue.Dequeue();
                _subscriptions.TryGetValue(nextEvent.GetType(), out handlers);
            }

            if (handlers is not null && handlers.Count > 0)
            {
                var tasks = handlers.Select(handler => InvokeHandlerAsync(handler, nextEvent, cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            processed++;
            _processedCount++;
        }
    }

    /// <inheritdoc />
    public int GetSubscriberCount<TEvent>() where TEvent : IGameEvent
    {
        lock (_syncRoot)
        {
            return _subscriptions.TryGetValue(typeof(TEvent), out var handlers) ? handlers.Count : 0;
        }
    }

    /// <inheritdoc />
    public EventBusMetrics GetMetrics() => new(_publishedCount, _processedCount, _droppedCount, _failedHandlerCount);

    private async Task InvokeHandlerAsync(Delegate handler, IGameEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var task = (ValueTask)handler.DynamicInvoke(@event, cancellationToken)!;
            await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _failedHandlerCount++;
            _logger.LogError($"Event handler failed for {@event.GetType().Name}.", exception);
        }
    }

    private sealed class Subscription<TEvent> : IDisposable where TEvent : IGameEvent
    {
        private readonly EventBus _eventBus;
        private readonly GameEventHandler<TEvent> _handler;
        private bool _disposed;

        public Subscription(EventBus eventBus, GameEventHandler<TEvent> handler)
        {
            _eventBus = eventBus;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.Unsubscribe(_handler);
            _disposed = true;
        }
    }
}
