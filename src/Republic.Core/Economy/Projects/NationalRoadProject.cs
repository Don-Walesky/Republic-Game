namespace Republic.Core.Economy.Projects;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a timed National Road Program development project.
/// First concrete implementation of the generic <see cref="DevelopmentProject"/> architecture.
/// Costs R500,000, takes 4 six-hour cycle boundaries (24 hours on the Republic clock),
/// and elevates national Infrastructure Condition by +0.1 (clamped at 1.0) on completion.
/// </summary>
public sealed class NationalRoadProject : DevelopmentProject
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
    /// Gets the categorical development project type (<see cref="DevelopmentProjectType.Road"/>).
    /// </summary>
    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.Road;

    /// <summary>
    /// Gets the display name of the National Road Program.
    /// </summary>
    public override string Name => "National Road Program";

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalRoadProject"/> class with default standard cost.
    /// Parameterless constructor required for polymorphic JSON deserialization.
    /// </summary>
    public NationalRoadProject()
    {
        Cost = StandardCost;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalRoadProject"/> class with explicit ownership and timing boundaries.
    /// </summary>
    public NationalRoadProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    /// <inheritdoc />
    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        var currentInfra = country.StateCapacity.InfrastructureCondition ?? 0.0;
        country.StateCapacity.InfrastructureCondition = Math.Clamp(
            currentInfra + InfrastructureBonus,
            0.0,
            1.0);
    }
}
