namespace Republic.Core.Events;

/// <summary>
/// Configures event bus queue behavior.
/// </summary>
public sealed class EventBusOptions
{
    /// <summary>
    /// Gets or sets the maximum queued event count before new events are dropped.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1024;
}
