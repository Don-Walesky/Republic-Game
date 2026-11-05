namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing an Agro-Industrial Irrigation Mega-Dam and Food Security Reservoir.
/// Expands sovereign food and natural resource output (+6.0), clean hydroelectric power (+4.0 energy),
/// baseline national stability (+4.0%), and citizen approval (+3.0%).
/// Costs R 950,000, requires 8 six-hour boundaries (48 hours). Requires an appointed Minister of Resources or Infrastructure.
/// </summary>
public sealed class NationalIrrigationDamProject : DevelopmentProject
{
    public const double StandardCost = 950_000.0;
    public const int DurationBoundaries = 8;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(48);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.IrrigationDam;
    public override string Name => "Agro-Industrial Irrigation Mega-Dam";

    public NationalIrrigationDamProject()
    {
        Cost = StandardCost;
    }

    public NationalIrrigationDamProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.Yield.NaturalResourceOutput += 6.0;
        country.Yield.EnergyCapacity += 4.0;
        country.BaselineStability = Math.Clamp(country.BaselineStability + 4.0, 0.0, 100.0);
        country.HappinessRating = Math.Clamp(country.HappinessRating + 3.0, 0.0, 100.0);
    }
}

