namespace Republic.Core.Time;

using Republic.Core.Diagnostics;
using Republic.Core.Events;

/// <summary>
/// Default deterministic time system.
/// </summary>
public sealed class TimeSystem : ITimeSystem
{
    private readonly TimeSystemConfiguration _configuration;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly GameCalendar _calendar;
    private double _accumulatedSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSystem"/> class.
    /// </summary>
    public TimeSystem(TimeSystemConfiguration configuration, IEventBus eventBus, ILogger logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _calendar = new GameCalendar(configuration.DaysPerMonth, configuration.MonthsPerYear);
    }

    /// <inheritdoc />
    public bool IsPaused { get; private set; }

    /// <inheritdoc />
    public double TimeScale { get; private set; } = 1d;

    /// <inheritdoc />
    public ulong CurrentTick { get; private set; }

    /// <inheritdoc />
    public double DeltaTime => 1d / _configuration.TickRate;

    /// <inheritdoc />
    public TimeSpan ElapsedTime => TimeSpan.FromSeconds(CurrentTick * DeltaTime);

    /// <inheritdoc />
    public GameDate CurrentDate => _calendar.CurrentDate;

    /// <inheritdoc />
    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        IsPaused = true;
        _logger.LogDebug("Time system paused.");
        return _eventBus.PublishAsync(new SimulationPausedEvent(DateTimeOffset.UtcNow), cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        IsPaused = false;
        _logger.LogDebug("Time system resumed.");
        return _eventBus.PublishAsync(new SimulationResumedEvent(DateTimeOffset.UtcNow), cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask SetTimeScaleAsync(double scale, CancellationToken cancellationToken = default)
    {
        TimeScale = Math.Max(0d, scale);
        _logger.LogDebug($"Time scale set to {TimeScale:0.###}x.");
        return _eventBus.PublishAsync(new TimeScaleChangedEvent(TimeScale, DateTimeOffset.UtcNow), cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask TickAsync(double realDeltaSeconds, CancellationToken cancellationToken = default)
    {
        if (realDeltaSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(realDeltaSeconds), "Delta seconds cannot be negative.");
        }

        if (IsPaused || TimeScale == 0d)
        {
            return;
        }

        _accumulatedSeconds += realDeltaSeconds * TimeScale;

        while (_accumulatedSeconds >= DeltaTime)
        {
            _accumulatedSeconds -= DeltaTime;
            CurrentTick++;
            var now = DateTimeOffset.UtcNow;
            await _eventBus.PublishAsync(new SimulationTickEvent(CurrentTick, DeltaTime, CurrentDate, now), cancellationToken).ConfigureAwait(false);

            if (CurrentTick % (ulong)_configuration.TickRate == 0)
            {
                await _eventBus.PublishAsync(new SimulationSecondEvent(CurrentTick, CurrentTick / (ulong)_configuration.TickRate, now), cancellationToken).ConfigureAwait(false);
            }

            if (CurrentTick % (ulong)_configuration.TicksPerDay == 0)
            {
                var advanceResult = _calendar.AdvanceDay();
                await _eventBus.PublishAsync(new SimulationDayEvent(advanceResult.Date, CurrentTick, now), cancellationToken).ConfigureAwait(false);

                if (advanceResult.MonthChanged)
                {
                    await _eventBus.PublishAsync(new SimulationMonthEvent(advanceResult.Date, CurrentTick, now), cancellationToken).ConfigureAwait(false);
                }

                if (advanceResult.YearChanged)
                {
                    await _eventBus.PublishAsync(new SimulationYearEvent(advanceResult.Date, CurrentTick, now), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    /// <inheritdoc />
    public TimeSystemState Serialize() => new()
    {
        CurrentTick = CurrentTick,
        AccumulatedSeconds = _accumulatedSeconds,
        TimeScale = TimeScale,
        IsPaused = IsPaused,
        CurrentDate = CurrentDate,
    };

    /// <inheritdoc />
    public void Deserialize(TimeSystemState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        CurrentTick = state.CurrentTick;
        _accumulatedSeconds = state.AccumulatedSeconds;
        TimeScale = Math.Max(0d, state.TimeScale);
        IsPaused = state.IsPaused;
        _calendar.Restore(state.CurrentDate);
        _logger.LogInfo($"Restored time system to tick {CurrentTick} ({CurrentDate.Year}/{CurrentDate.Month}/{CurrentDate.Day}).");
    }
}
