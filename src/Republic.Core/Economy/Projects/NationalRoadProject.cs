namespace Republic.Core.Economy.Projects;

using Republic.Core.Time;

/// <summary>
/// Domain model representing a timed National Road Program development project.
/// Encapsulates the unique identifier, started boundary, finish boundary, fixed cost, and completion state.
/// </summary>
public sealed class NationalRoadProject
{
    /// <summary>
    /// Fixed cost in liquid REPU to initiate the National Road Program (R500,000).
    /// </summary>
    public const double StandardCost = 500_000.0;

    /// <summary>
    /// Number of six-hour cycle boundaries required to complete construction (4 boundaries / 24 hours).
    /// </summary>
    public const int DurationBoundaries = 4;

    /// <summary>
    /// Total duration on the global Republic simulation clock (24 hours).
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(24);

    /// <summary>
    /// Increase applied to the sovereign nation's infrastructure condition upon completion (+0.1, clamped at 1.0).
    /// </summary>
    public const double InfrastructureBonus = 0.1;

    /// <summary>
    /// Gets the unique identifier of the road project.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp / boundary at which construction started.
    /// </summary>
    public RepublicTime StartedBoundary { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp / boundary at which construction finishes
    /// (strictly 4 six-hour boundaries / 24 hours from <see cref="StartedBoundary"/>).
    /// </summary>
    public RepublicTime FinishBoundary { get; init; }

    /// <summary>
    /// Gets the liquid REPU cost withdrawn at project initiation (defaults to R500,000).
    /// </summary>
    public double Cost { get; init; } = StandardCost;

    /// <summary>
    /// Gets or sets a value indicating whether construction of the road program has completed.
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether construction of the road program has completed.
    /// Synchronized with <see cref="Completed"/>.
    /// </summary>
    public bool IsCompleted
    {
        get => Completed;
        set => Completed = value;
    }

    /// <summary>
    /// Gets a value indicating whether the project is currently in-progress and active.
    /// </summary>
    public bool IsActive => !Completed;
}
