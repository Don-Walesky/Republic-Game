namespace Republic.Core.Tasks.Events;

using Republic.Core.Events;
using Republic.Core.Tasks.Models;

/// <summary>
/// Event emitted when a task is queued into the simulation pipeline.
/// </summary>
public sealed record TaskQueuedEvent(ScheduledTask Task, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a task advances its progress percentage.
/// </summary>
public sealed record TaskProgressUpdatedEvent(ScheduledTask Task, double ProgressPercentage, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a task reaches 100% completion.
/// </summary>
public sealed record TaskCompletedEvent(ScheduledTask Task, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a task is cancelled.
/// </summary>
public sealed record TaskCancelledEvent(string TaskId, string Reason, DateTimeOffset OccurredAt) : IGameEvent;
