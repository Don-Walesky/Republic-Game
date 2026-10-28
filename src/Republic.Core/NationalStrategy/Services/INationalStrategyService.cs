namespace Republic.Core.NationalStrategy.Services;

using System.Collections.Generic;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service contract managing national strategic directions, adoption friction, sector alignment weighting,
/// and empirical progress evaluations.
/// </summary>
public interface INationalStrategyService
{
    /// <summary>
    /// Minimum simulation days required before the administration can change national strategic direction again.
    /// </summary>
    const int DefaultCooldownDays = 30;

    /// <summary>
    /// Default transition friction duration in days.
    /// </summary>
    const int DefaultTransitionFrictionDays = 14;

    /// <summary>
    /// Default treasury cost (in REPU) required for administrative retooling and policy realignment.
    /// </summary>
    const double DefaultTransitionCost = 25_000_000.0;

    /// <summary>
    /// Ensures that the specified country has an authoritative national strategy profile.
    /// </summary>
    NationalStrategyProfile EnsureStrategyProfile(Country country, RepublicTime? simulationTime = null);

    /// <summary>
    /// Sets or changes the country's national strategic direction, applying authoritative treasury costs,
    /// political friction, and adoption timelines.
    /// </summary>
    (bool Success, string Message, NationalStrategyProfile Profile) SetStrategicDirection(
        Country country,
        NationalStrategicDirection newDirection,
        RepublicTime simulationTime,
        bool bypassFriction = false);

    /// <summary>
    /// Evaluates the strategic alignment multiplier for a given economic sector or policy domain (0.8 to 1.5).
    /// </summary>
    double GetAlignmentMultiplier(Country country, string sectorOrCategory);

    /// <summary>
    /// Evaluates empirical progress toward the active strategic goals based on actual economic,
    /// budgetary, and infrastructural metrics.
    /// </summary>
    double UpdateStrategicProgress(Country country, RepublicTime simulationTime);
}
