namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a timed National High-Speed Rail Corridor project.
/// Connects economic regions, cutting transit times and boosting domestic trade and civil service mobility.
/// Costs R 1,800,000, requires 12 six-hour boundaries (72 hours), and elevates Infrastructure condition (+0.08)
/// and baseline stability (+3.0%). Requires an appointed Minister of Infrastructure.
/// </summary>
public sealed class NationalHighSpeedRailProject : DevelopmentProject
{
    public const double StandardCost = 1_800_000.0;
    public const int DurationBoundaries = 12;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(72);
    public const double StabilityBonus = 3.0;

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.HighSpeedRail;
    public override string Name => "High-Speed Rail Corridor";

    public NationalHighSpeedRailProject()
    {
        Cost = StandardCost;
    }

    public NationalHighSpeedRailProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.BaselineStability = Math.Clamp(country.BaselineStability + StabilityBonus, 0.0, 100.0);
        if (country.StateCapacity != null)
        {
            var cur = country.StateCapacity.InfrastructureCondition ?? 0.3;
            country.StateCapacity.InfrastructureCondition = Math.Clamp(cur + 0.08, 0.05, 1.0);
        }
        country.Yield.InfrastructureCapacity += 8.0;
        country.TradeBalance += 350_000_000.0;
    }
}
