namespace Republic.Core.Time;

/// <summary>
/// Authoritative implementation of the global Republic simulation clock,
/// anchored to West Africa Time (WAT / UTC+1) starting from Republic Day 0, 00:00 WAT.
/// </summary>
public sealed class RepublicClock : IControlledRepublicClock, IRepublicClock
{
    private readonly RepublicClockConfiguration _configuration;
    private readonly TimeProvider? _realTimeProvider;
    private bool _isControlled;
    private RepublicTime _controlledTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicClock"/> class.
    /// By default, operates in controlled/deterministic mode initialized to Republic Day 0, 00:00 WAT.
    /// If a <paramref name="realTimeProvider"/> is supplied, the clock derives current time from wall-clock progression.
    /// </summary>
    public RepublicClock(RepublicClockConfiguration configuration, TimeProvider? realTimeProvider = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _realTimeProvider = realTimeProvider;
        _isControlled = realTimeProvider == null;

        LaunchEpoch = RepublicTime.FromTimestamp(_configuration.LaunchEpochWat, _configuration.LaunchEpochWat);
        _controlledTime = LaunchEpoch;
    }

    /// <inheritdoc />
    public RepublicTime LaunchEpoch { get; }

    /// <inheritdoc />
    public RepublicTime CurrentTime
    {
        get
        {
            if (_isControlled || _realTimeProvider == null)
            {
                return _controlledTime;
            }

            var nowUtc = _realTimeProvider.GetUtcNow();
            return ToRepublicTime(nowUtc);
        }
    }

    /// <inheritdoc />
    public bool IsControlled => _isControlled;

    /// <summary>
    /// Creates a deterministic, controlled simulation clock starting at Republic Day 0, 00:00 WAT.
    /// </summary>
    public static RepublicClock CreateControlled(
        RepublicClockConfiguration? configuration = null,
        RepublicTime? initialTime = null)
    {
        var config = configuration ?? new RepublicClockConfiguration();
        var clock = new RepublicClock(config);
        if (initialTime.HasValue)
        {
            clock.SetTime(initialTime.Value);
        }

        return clock;
    }

    /// <summary>
    /// Creates a real-time clock tracking authoritative wall-clock time relative to the launch epoch.
    /// </summary>
    public static RepublicClock CreateRealTime(
        RepublicClockConfiguration configuration,
        TimeProvider? timeProvider = null) =>
        new(configuration, timeProvider ?? TimeProvider.System);

    /// <inheritdoc />
    public RepublicTime ToRepublicTime(DateTimeOffset timestamp) =>
        RepublicTime.FromTimestamp(timestamp, _configuration.LaunchEpochWat);

    /// <inheritdoc />
    public RepublicTime FromElapsed(TimeSpan elapsed) =>
        RepublicTime.FromElapsed(elapsed, _configuration.LaunchEpochWat);

    /// <inheritdoc />
    public RepublicTime FromDayAndTime(long dayNumber, TimeOnly timeOfDay) =>
        RepublicTime.FromDayAndTime(dayNumber, timeOfDay, _configuration.LaunchEpochWat);

    /// <inheritdoc />
    public void SetTime(RepublicTime time)
    {
        _isControlled = true;
        _controlledTime = time;
    }

    /// <inheritdoc />
    public void SetElapsed(TimeSpan elapsed)
    {
        SetTime(FromElapsed(elapsed));
    }

    /// <inheritdoc />
    public void Advance(TimeSpan delta)
    {
        _isControlled = true;
        _controlledTime = _controlledTime.Add(delta);
    }

    /// <inheritdoc />
    public void AdvanceDays(int days) => Advance(TimeSpan.FromDays(days));

    /// <inheritdoc />
    public void AdvanceHours(int hours) => Advance(TimeSpan.FromHours(hours));

    /// <inheritdoc />
    public void AdvanceMinutes(int minutes) => Advance(TimeSpan.FromMinutes(minutes));

    /// <inheritdoc />
    public void AdvanceSeconds(int seconds) => Advance(TimeSpan.FromSeconds(seconds));

    /// <inheritdoc />
    public void Reset()
    {
        SetTime(LaunchEpoch);
    }
}
