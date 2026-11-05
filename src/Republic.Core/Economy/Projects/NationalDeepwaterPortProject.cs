namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a Deepwater Seaport Hub development project.
/// Expands sovereign international maritime commerce, boosting trade balance (+R 600M) and GDP (+10B REPU).
/// Costs R 2,200,000, requires 14 six-hour boundaries (84 hours). Requires an appointed Minister of Infrastructure.
/// </summary>
public sealed class NationalDeepwaterPortProject : DevelopmentProject
{
    public const double StandardCost = 2_200_000.0;
    public const int DurationBoundaries = 14;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(84);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.DeepwaterPort;
    public override string Name => "Deepwater Seaport Hub";

    public NationalDeepwaterPortProject()
    {
        Cost = StandardCost;
    }

    public NationalDeepwaterPortProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.TradeBalance += 600_000_000.0;
        country.GrossDomesticProduct += 10_000_000_000.0;
        country.Yield.IndustrialCapacity += 4.0;
    }
}

