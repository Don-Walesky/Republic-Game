namespace Republic.Core.Events;

/// <summary>
/// Event bus counters for diagnostics and tests.
/// </summary>
public sealed record EventBusMetrics(long PublishedCount, long ProcessedCount, long DroppedCount, long FailedHandlerCount);
