namespace Republic.Core.Time;

using Republic.Core.Events;

/// <summary>
/// Emitted every simulation tick.
/// </summary>
public sealed record SimulationTickEvent(ulong TickNumber, double DeltaTime, GameDate CurrentDate, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when an in-simulation second elapses.
/// </summary>
public sealed record SimulationSecondEvent(ulong TickNumber, ulong ElapsedSeconds, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the calendar advances by a day.
/// </summary>
public sealed record SimulationDayEvent(GameDate CurrentDate, ulong TickNumber, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the calendar advances by a month.
/// </summary>
public sealed record SimulationMonthEvent(GameDate CurrentDate, ulong TickNumber, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the calendar advances by a year.
/// </summary>
public sealed record SimulationYearEvent(GameDate CurrentDate, ulong TickNumber, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the time system is paused.
/// </summary>
public sealed record SimulationPausedEvent(DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the time system resumes.
/// </summary>
public sealed record SimulationResumedEvent(DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when the time scale changes.
/// </summary>
public sealed record TimeScaleChangedEvent(double TimeScale, DateTimeOffset OccurredAt) : IGameEvent;
