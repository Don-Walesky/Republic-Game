namespace Republic.Core.Economy.Projects;

using System;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing the commissioning of a State Investigative Bureau (SIB).
/// Centralizes federal intelligence, counter-espionage surveillance, domestic threat audits,
/// and classified operational concealment.
/// Costs R 2,200,000, requires 8 six-hour boundaries (48 hours).
/// </summary>
public sealed class NationalInvestigativeBureauProject : DevelopmentProject
{
    public const double StandardCost = 2_200_000.0;
    public const int DurationBoundaries = 8;
    public static readonly TimeSpan Duration = TimeSpan.FromHours(48);

    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.InvestigativeBureau;
    public override string Name => "State Investigative Bureau (SIB)";

    public NationalInvestigativeBureauProject()
    {
        Cost = StandardCost;
    }

    public NationalInvestigativeBureauProject(string countryId, RepublicTime startedBoundary, double cost = StandardCost) : this()
    {
        CountryId = countryId;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(Duration);
        Cost = cost;
    }

    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.InvestigativeBureau.Commission();
        country.Yield.IntelligenceCapacity += 10.0;
        country.BaselineStability = Math.Clamp(country.BaselineStability + 5.0, 0.0, 100.0);
        if (country.StateCapacity != null && country.StateCapacity.CorruptionControl.HasValue)
        {
            country.StateCapacity.CorruptionControl = Math.Clamp(country.StateCapacity.CorruptionControl.Value + 0.10, 0.1, 1.0);
        }
    }
}

