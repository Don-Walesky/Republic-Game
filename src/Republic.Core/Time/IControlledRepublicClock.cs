namespace Republic.Core.Time;

/// <summary>
/// Provides controlled advancement capabilities for deterministic simulation,
/// offline catch-up, and automated testing.
/// </summary>
public interface IControlledRepublicClock : IRepublicClock
{
    /// <summary>
    /// Sets the authoritative simulation time to a specific RepublicTime.
    /// </summary>
    void SetTime(RepublicTime time);

    /// <summary>
    /// Sets the authoritative simulation time by specifying duration elapsed from launch.
    /// </summary>
    void SetElapsed(TimeSpan elapsed);

    /// <summary>
    /// Advances the simulation clock by the specified duration.
    /// </summary>
    void Advance(TimeSpan delta);

    /// <summary>
    /// Advances the simulation clock by a number of days.
    /// </summary>
    void AdvanceDays(int days);

    /// <summary>
    /// Advances the simulation clock by a number of hours.
    /// </summary>
    void AdvanceHours(int hours);

    /// <summary>
    /// Advances the simulation clock by a number of minutes.
    /// </summary>
    void AdvanceMinutes(int minutes);

    /// <summary>
    /// Advances the simulation clock by a number of seconds.
    /// </summary>
    void AdvanceSeconds(int seconds);

    /// <summary>
    /// Resets the simulation clock back to Day 0, 00:00 WAT.
    /// </summary>
    void Reset();
}
