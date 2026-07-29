namespace Republic.Core.Time;

/// <summary>
/// Serializable time system snapshot.
/// </summary>
public sealed class TimeSystemState
{
    /// <summary>
    /// Gets or sets the current tick count.
    /// </summary>
    public ulong CurrentTick { get; set; }

    /// <summary>
    /// Gets or sets the accumulated real time not yet converted into ticks.
    /// </summary>
    public double AccumulatedSeconds { get; set; }

    /// <summary>
    /// Gets or sets the current time scale.
    /// </summary>
    public double TimeScale { get; set; } = 1d;

    /// <summary>
    /// Gets or sets a value indicating whether the system is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets the current calendar date.
    /// </summary>
    public GameDate CurrentDate { get; set; } = new(1, 1, 1);
}
