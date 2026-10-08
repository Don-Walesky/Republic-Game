namespace Republic.Core.Economy.Projects;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a timed National University development project.
/// Concrete implementation of the generic <see cref="DevelopmentProject"/> architecture.
/// Costs R750,000, takes 8 six-hour cycle boundaries (48 hours on the Republic clock),
/// and elevates national Human Capital and Science / Research Capacity on completion.
/// </summary>
public sealed class NationalUniversityProject : DevelopmentProject
{
    /// <summary>
    /// Fixed cost in liquid REPU to initiate the National University (R750,000).
    /// </summary>
    public const double StandardCost = 750_000.0;

    /// <summary>
    /// Number of six-hour cycle boundaries required to complete construction (8 boundaries / 48 hours).
    /// </summary>
    public const int DurationBoundaries = 8;

    /// <summary>
    /// Total duration on the global Republic simulation clock (48 hours).
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(48);

    /// <summary>
    /// Increase applied to the sovereign nation's Human Capital upon completion (+5.0).
    /// </summary>
    public const double HumanCapitalBonus = 5.0;

    /// <summary>
    /// Increase applied to the sovereign nation's Science / Research Capacity upon completion (+5.0).
    /// </summary>
    public const double ScienceCapacityBonus = 5.0;

    /// <inheritdoc />
    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.University;

    /// <inheritdoc />
    public override string Name => "National University";

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalUniversityProject"/> class with default standard cost.
    /// Parameterless constructor required for polymorphic JSON deserialization.
    /// </summary>
    public NationalUniversityProject()
    {
        Cost = StandardCost;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalUniversityProject"/> class with explicit ownership and timing boundaries.
    /// </summary>
    public NationalUniversityProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
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

        country.Yield.HumanCapital = Math.Max(0.0, country.Yield.HumanCapital + HumanCapitalBonus);
        country.Yield.ScienceCapacity = Math.Max(0.0, country.Yield.ScienceCapacity + ScienceCapacityBonus);
    }
}
