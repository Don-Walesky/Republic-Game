namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a National Teaching Hospital & Healthcare Network project.
/// Elevates sovereign citizen health welfare, increasing public approval (+5.0%) and baseline stability (+3.0%).
/// Costs R 1,100,000, requires 8 six-hour boundaries (48 hours). Requires an appointed Minister of Health.
/// </summary>
public sealed class NationalHospitalProject : DevelopmentProject
{
    public const double StandardCost = 1_100_000.0;
    public const int DurationBoundaries = 8;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(48);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.TeachingHospital;
    public override string Name => "National Teaching Hospital Network";

    public NationalHospitalProject()
    {
        Cost = StandardCost;
    }

    public NationalHospitalProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.HappinessRating = Math.Clamp(country.HappinessRating + 5.0, 0.0, 100.0);
        country.BaselineStability = Math.Clamp(country.BaselineStability + 3.0, 0.0, 100.0);
    }
}

