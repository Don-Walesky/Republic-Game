namespace Republic.Core.Time;

/// <summary>
/// Configures deterministic simulation time.
/// </summary>
public sealed class TimeSystemConfiguration
{
    private DateTime _epochStartDate = new DateTime(2027, 2, 10, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets or sets the configuration for the authoritative global Republic simulation clock.
    /// </summary>
    public RepublicClockConfiguration Clock { get; set; } = new();

    /// <summary>
    /// Gets or sets the simulation tick rate.
    /// </summary>
    public int TickRate { get; set; } = 60;

    /// <summary>
    /// Gets or sets the days per month for the simulation calendar.
    /// </summary>
    public int DaysPerMonth { get; set; } = 30;

    /// <summary>
    /// Gets or sets the months per year for the simulation calendar.
    /// </summary>
    public int MonthsPerYear { get; set; } = 12;

    /// <summary>
    /// Gets or sets the number of ticks required to advance one in-game day.
    /// </summary>
    public int TicksPerDay { get; set; } = 600;

    /// <summary>
    /// Gets or sets the real-world epoch start date for the simulation.
    /// Preserved for backward compatibility with tick-based legacy engine systems.
    /// </summary>
    public DateTime EpochStartDate
    {
        get => _epochStartDate;
        set
        {
            _epochStartDate = value;
            Clock.LaunchDate = DateOnly.FromDateTime(value);
        }
    }
}
