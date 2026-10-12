namespace Republic.Core.Economy.Projects;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a timed National Power Plant development project.
/// Concrete implementation of the generic <see cref="DevelopmentProject"/> architecture.
/// Costs R1,000,000, takes 12 six-hour cycle boundaries (72 hours on the Republic clock),
/// and elevates national Energy Capacity on completion.
/// </summary>
public sealed class NationalPowerPlantProject : DevelopmentProject
{
    /// <summary>
    /// Fixed cost in liquid REPU to initiate the National Power Plant (R1,000,000).
    /// </summary>
    public const double StandardCost = 1_000_000.0;

    /// <summary>
    /// Number of six-hour cycle boundaries required to complete construction (12 boundaries / 72 hours).
    /// </summary>
    public const int DurationBoundaries = 12;

    /// <summary>
    /// Total duration on the global Republic simulation clock (72 hours).
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(72);

    /// <summary>
    /// Increase applied to the sovereign nation's Energy Capacity upon completion (+5.0).
    /// </summary>
    public const double EnergyCapacityBonus = 5.0;

    /// <summary>
    /// Alias for <see cref="EnergyCapacityBonus"/>.
    /// </summary>
    public const double EnergyBonus = EnergyCapacityBonus;

    /// <inheritdoc />
    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.PowerPlant;

    /// <inheritdoc />
    public override string Name => "National Power Plant";

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalPowerPlantProject"/> class with default standard cost.
    /// Parameterless constructor required for polymorphic JSON deserialization.
    /// </summary>
    public NationalPowerPlantProject()
    {
        Cost = StandardCost;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalPowerPlantProject"/> class with explicit ownership and timing boundaries.
    /// </summary>
    public NationalPowerPlantProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
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

        country.Yield.EnergyCapacity = Math.Max(0.0, country.Yield.EnergyCapacity + EnergyCapacityBonus);
    }
}
