namespace Republic.Core.Time;

/// <summary>
/// Authoritative simulation clock for the Republic world.
/// Exposes current simulation time anchored to West Africa Time (WAT / UTC+1)
/// starting from Republic Day 0, 00:00 WAT.
/// </summary>
public interface IRepublicClock
{
    /// <summary>
    /// Gets the authoritative Republic launch epoch (Day 0, 00:00 WAT).
    /// </summary>
    RepublicTime LaunchEpoch { get; }

    /// <summary>
    /// Gets the current authoritative Republic simulation time.
    /// </summary>
    RepublicTime CurrentTime { get; }

    /// <summary>
    /// Gets a value indicating whether this clock is in controlled (deterministic/test) mode.
    /// </summary>
    bool IsControlled { get; }

    /// <summary>
    /// Converts a UTC or offset-aware timestamp into a RepublicTime instance.
    /// </summary>
    RepublicTime ToRepublicTime(DateTimeOffset timestamp);

    /// <summary>
    /// Converts an elapsed duration since launch into a RepublicTime instance.
    /// </summary>
    RepublicTime FromElapsed(TimeSpan elapsed);

    /// <summary>
    /// Creates a RepublicTime instance for a specific day number and time of day in WAT.
    /// </summary>
    RepublicTime FromDayAndTime(long dayNumber, TimeOnly timeOfDay);
}
