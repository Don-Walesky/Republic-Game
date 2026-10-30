namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a timed National Refinery development project.
/// Concrete implementation of the generic <see cref="DevelopmentProject"/> architecture.
/// Costs R1,200,000, takes 10 six-hour cycle boundaries (60 hours on the Republic clock),
/// and elevates national Energy Capacity and Industrial Capacity on completion.
/// Initiating this project requires an appointed Minister of Oil &amp; Gas.
/// </summary>
public sealed class NationalRefineryProject : DevelopmentProject
{
    /// <summary>
    /// Fixed cost in liquid REPU to initiate the National Refinery (R1,200,000).
    /// </summary>
    public const double StandardCost = 1_200_000.0;

    /// <summary>
    /// Number of six-hour cycle boundaries required to complete construction (10 boundaries / 60 hours).
    /// </summary>
    public const int DurationBoundaries = 10;

    /// <summary>
    /// Total duration on the global Republic simulation clock (60 hours).
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(60);

    /// <summary>
    /// Increase applied to the sovereign nation's Energy Capacity upon completion (+5.0).
    /// </summary>
    public const double EnergyCapacityBonus = 5.0;

    /// <summary>
    /// Increase applied to the sovereign nation's Industrial Capacity upon completion (+3.0).
    /// </summary>
    public const double IndustrialCapacityBonus = 3.0;

    /// <inheritdoc />
    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.Refinery;

    /// <inheritdoc />
    public override string Name => "National Refinery";

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalRefineryProject"/> class with default standard cost.
    /// Parameterless constructor required for polymorphic JSON deserialization.
    /// </summary>
    public NationalRefineryProject()
    {
        Cost = StandardCost;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalRefineryProject"/> class with explicit ownership and timing boundaries.
    /// </summary>
    public NationalRefineryProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
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
        country.Yield.IndustrialCapacity = Math.Max(0.0, country.Yield.IndustrialCapacity + IndustrialCapacityBonus);
    }
}
