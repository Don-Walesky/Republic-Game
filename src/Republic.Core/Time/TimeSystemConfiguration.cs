namespace Republic.Core.Time;

/// <summary>
/// Configures deterministic simulation time.
/// </summary>
public sealed class TimeSystemConfiguration
{
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
}
