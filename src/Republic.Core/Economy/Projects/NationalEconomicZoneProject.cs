namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a Special Economic & Industrial Export Zone project.
/// Attracts private capital, accelerates manufacturing output (+5.0 Industrial Capacity), and boosts GDP (+15B REPU).
/// Costs R 1,600,000, requires 10 six-hour boundaries (60 hours). Requires an appointed Minister of Finance.
/// </summary>
public sealed class NationalEconomicZoneProject : DevelopmentProject
{
    public const double StandardCost = 1_600_000.0;
    public const int DurationBoundaries = 10;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(60);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.SpecialEconomicZone;
    public override string Name => "Special Economic Zone";

    public NationalEconomicZoneProject()
    {
        Cost = StandardCost;
    }

    public NationalEconomicZoneProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.Yield.IndustrialCapacity += 5.0;
        country.GrossDomesticProduct += 15_000_000_000.0;
        country.TradeBalance += 400_000_000.0;
    }
}

