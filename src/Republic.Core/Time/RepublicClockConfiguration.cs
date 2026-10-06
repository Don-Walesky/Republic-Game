namespace Republic.Core.Time;

/// <summary>
/// Configuration for the authoritative Republic simulation clock and launch epoch.
/// </summary>
public sealed class RepublicClockConfiguration
{
    /// <summary>
    /// Gets or sets the Gregorian calendar date of Republic Day 0 in West Africa Time (WAT / UTC+1).
    /// Defaults to the repository's pre-existing configuration placeholder date (2027-02-10).
    /// An official public launch date will be designated prior to live production deployment.
    /// </summary>
    public DateOnly LaunchDate { get; set; } = new(2027, 2, 10);

    /// <summary>
    /// Computes the authoritative launch epoch: Republic Day 0, 00:00 WAT (+01:00).
    /// </summary>
    public DateTimeOffset LaunchEpochWat => new(
        LaunchDate.Year,
        LaunchDate.Month,
        LaunchDate.Day,
        0, 0, 0,
        RepublicTime.WatOffset);
}
