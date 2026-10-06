namespace Republic.Core.Time;

/// <summary>
/// Immutable value representation of Republic simulation time,
/// strictly aligned with the Gregorian calendar on West Africa Time (WAT / UTC+1).
/// </summary>
public readonly struct RepublicTime : IEquatable<RepublicTime>, IComparable<RepublicTime>
{
    /// <summary>
    /// Fixed offset for West Africa Time (WAT): UTC+1 year-round with no daylight saving time.
    /// </summary>
    public static readonly TimeSpan WatOffset = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicTime"/> struct.
    /// </summary>
    public RepublicTime(DateTimeOffset watTimestamp, DateTimeOffset launchEpochWat)
    {
        WatTimestamp = watTimestamp.ToOffset(WatOffset);
        LaunchEpochWat = launchEpochWat.ToOffset(WatOffset);
        Date = DateOnly.FromDateTime(WatTimestamp.DateTime);
        Time = TimeOnly.FromDateTime(WatTimestamp.DateTime);

        var epochDate = DateOnly.FromDateTime(LaunchEpochWat.DateTime);
        DayNumber = Date.DayNumber - epochDate.DayNumber;
        ElapsedDuration = WatTimestamp - LaunchEpochWat;
    }

    /// <summary>
    /// Gets the authoritative timestamp in West Africa Time (WAT / UTC+1).
    /// </summary>
    public DateTimeOffset WatTimestamp { get; }

    /// <summary>
    /// Gets the authoritative launch epoch timestamp (Day 0, 00:00 WAT) against which this time is anchored.
    /// </summary>
    public DateTimeOffset LaunchEpochWat { get; }

    /// <summary>
    /// Gets the Gregorian calendar date in WAT.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// Gets the time of day in WAT.
    /// </summary>
    public TimeOnly Time { get; }

    /// <summary>
    /// Gets the simulation day number measured from launch (0 for Day 0, 1 for Day 1, etc.).
    /// </summary>
    public long DayNumber { get; }

    /// <summary>
    /// Gets the total elapsed duration since Republic Day 0, 00:00 WAT.
    /// </summary>
    public TimeSpan ElapsedDuration { get; }

    /// <summary>
    /// Gets the hour of the day in WAT (0 to 23).
    /// </summary>
    public int Hour => Time.Hour;

    /// <summary>
    /// Gets the minute of the hour in WAT (0 to 59).
    /// </summary>
    public int Minute => Time.Minute;

    /// <summary>
    /// Gets the second of the minute in WAT (0 to 59).
    /// </summary>
    public int Second => Time.Second;

    /// <summary>
    /// Gets the UTC equivalent of this simulation timestamp.
    /// </summary>
    public DateTimeOffset UtcTimestamp => WatTimestamp.ToUniversalTime();

    /// <summary>
    /// Gets a value indicating whether this timestamp falls on Republic Day 0.
    /// </summary>
    public bool IsLaunchDay => DayNumber == 0;

    /// <summary>
    /// Creates a <see cref="RepublicTime"/> from an arbitrary timestamp (UTC or offset-aware)
    /// anchored to the given launch epoch.
    /// </summary>
    public static RepublicTime FromTimestamp(DateTimeOffset timestamp, DateTimeOffset launchEpochWat) =>
        new(timestamp, launchEpochWat);

    /// <summary>
    /// Creates a <see cref="RepublicTime"/> from an elapsed duration since the launch epoch.
    /// </summary>
    public static RepublicTime FromElapsed(TimeSpan elapsed, DateTimeOffset launchEpochWat) =>
        new(launchEpochWat.ToOffset(WatOffset) + elapsed, launchEpochWat);

    /// <summary>
    /// Creates a <see cref="RepublicTime"/> from a specific simulation day number and time of day in WAT.
    /// </summary>
    public static RepublicTime FromDayAndTime(long dayNumber, TimeOnly timeOfDay, DateTimeOffset launchEpochWat)
    {
        var epochWat = launchEpochWat.ToOffset(WatOffset);
        var epochDate = DateOnly.FromDateTime(epochWat.DateTime);
        var targetDate = epochDate.AddDays((int)dayNumber);
        var targetWat = new DateTimeOffset(
            targetDate.Year,
            targetDate.Month,
            targetDate.Day,
            timeOfDay.Hour,
            timeOfDay.Minute,
            timeOfDay.Second,
            timeOfDay.Millisecond,
            WatOffset);

        return new RepublicTime(targetWat, launchEpochWat);
    }

    /// <summary>
    /// Adds a duration to this simulation time.
    /// </summary>
    public RepublicTime Add(TimeSpan duration) =>
        new(WatTimestamp + duration, LaunchEpochWat);

    /// <summary>
    /// Subtracts a duration from this simulation time.
    /// </summary>
    public RepublicTime Subtract(TimeSpan duration) =>
        new(WatTimestamp - duration, LaunchEpochWat);

    /// <summary>
    /// Calculates the duration between two simulation times.
    /// </summary>
    public TimeSpan Subtract(RepublicTime other) =>
        WatTimestamp - other.WatTimestamp;

    /// <inheritdoc />
    public bool Equals(RepublicTime other) =>
        WatTimestamp.Equals(other.WatTimestamp) && LaunchEpochWat.Equals(other.LaunchEpochWat);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is RepublicTime other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(WatTimestamp, LaunchEpochWat);

    /// <inheritdoc />
    public int CompareTo(RepublicTime other) =>
        WatTimestamp.CompareTo(other.WatTimestamp);

    /// <inheritdoc />
    public override string ToString() =>
        $"Day {DayNumber}, {Time:HH\\:mm} WAT ({Date:yyyy-MM-dd})";

    /// <summary>
    /// Formats the time in a detailed format including seconds and timezone offset.
    /// </summary>
    public string ToDetailedString() =>
        $"{WatTimestamp:yyyy-MM-dd HH\\:mm\\:ss} WAT (Day {DayNumber}, +{ElapsedDuration.TotalHours:0.##}h elapsed)";

    public static RepublicTime operator +(RepublicTime time, TimeSpan duration) => time.Add(duration);

    public static RepublicTime operator -(RepublicTime time, TimeSpan duration) => time.Subtract(duration);

    public static TimeSpan operator -(RepublicTime left, RepublicTime right) => left.Subtract(right);

    public static bool operator ==(RepublicTime left, RepublicTime right) => left.Equals(right);

    public static bool operator !=(RepublicTime left, RepublicTime right) => !left.Equals(right);

    public static bool operator <(RepublicTime left, RepublicTime right) => left.CompareTo(right) < 0;

    public static bool operator <=(RepublicTime left, RepublicTime right) => left.CompareTo(right) <= 0;

    public static bool operator >(RepublicTime left, RepublicTime right) => left.CompareTo(right) > 0;

    public static bool operator >=(RepublicTime left, RepublicTime right) => left.CompareTo(right) >= 0;
}
