namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a Sovereign AI & High-Performance Supercomputing Data Center.
/// Accelerates national scientific research (+6.0 science), commercial tech innovation (+8.0),
/// administrative intelligence (+5.0 admin), and cybersecurity/counter-espionage (+6.0 intelligence).
/// Costs R 1,900,000, requires 10 six-hour boundaries (60 hours). Requires an appointed Minister of Science/Education.
/// </summary>
public sealed class NationalSupercomputerProject : DevelopmentProject
{
    public const double StandardCost = 1_900_000.0;
    public const int DurationBoundaries = 10;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(60);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.SupercomputingCenter;
    public override string Name => "Sovereign AI & Supercomputing Center";

    public NationalSupercomputerProject()
    {
        Cost = StandardCost;
    }

    public NationalSupercomputerProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.Yield.ScienceCapacity += 6.0;
        country.Yield.InnovationCapacity += 8.0;
        country.Yield.AdministrativeCapacity += 5.0;
        country.Yield.IntelligenceCapacity += 6.0;
    }
}

