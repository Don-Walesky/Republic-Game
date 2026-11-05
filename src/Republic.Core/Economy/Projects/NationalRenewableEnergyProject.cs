namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a Clean Solar & Wind Mega-Grid development project.
/// Elevates national Energy Capacity (+6.0) and public approval (+3.0%) while reducing carbon footprint.
/// Costs R 1,400,000, requires 10 six-hour boundaries (60 hours). Requires an appointed Minister of Oil & Gas or Infrastructure.
/// </summary>
public sealed class NationalRenewableEnergyProject : DevelopmentProject
{
    public const double StandardCost = 1_400_000.0;
    public const int DurationBoundaries = 10;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(60);
    public const double EnergyCapacityBonus = 6.0;

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.RenewableEnergy;
    public override string Name => "Clean Renewable Energy Grid";

    public NationalRenewableEnergyProject()
    {
        Cost = StandardCost;
    }

    public NationalRenewableEnergyProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.Yield.EnergyCapacity += EnergyCapacityBonus;
        country.HappinessRating = Math.Clamp(country.HappinessRating + 3.0, 0.0, 100.0);
    }
}

