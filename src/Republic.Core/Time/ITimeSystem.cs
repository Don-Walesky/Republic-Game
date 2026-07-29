namespace Republic.Core.Time;

/// <summary>
/// Advances deterministic simulation time.
/// </summary>
public interface ITimeSystem
{
    /// <summary>
    /// Gets a value indicating whether the simulation is paused.
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Gets the active time scale.
    /// </summary>
    double TimeScale { get; }

    /// <summary>
    /// Gets the current tick count.
    /// </summary>
    ulong CurrentTick { get; }

    /// <summary>
    /// Gets the fixed delta time for one simulation tick.
    /// </summary>
    double DeltaTime { get; }

    /// <summary>
    /// Gets the elapsed simulated time.
    /// </summary>
    TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Gets the current simulation date.
    /// </summary>
    GameDate CurrentDate { get; }

    /// <summary>
    /// Pauses simulation advancement.
    /// </summary>
    ValueTask PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes simulation advancement.
    /// </summary>
    ValueTask ResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the time scale.
    /// </summary>
    ValueTask SetTimeScaleAsync(double scale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances time using real elapsed seconds.
    /// </summary>
    ValueTask TickAsync(double realDeltaSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes the current time state.
    /// </summary>
    TimeSystemState Serialize();

    /// <summary>
    /// Restores the current time state.
    /// </summary>
    void Deserialize(TimeSystemState state);
}
