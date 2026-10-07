namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain rule interface governing the founding-day National Yield multiplier (x10).
/// Evaluates whether a country's calculation falls within its Independence Day in the Republic simulation calendar
/// and applies the x10 multiplier to the calculated National Yield output.
/// </summary>
public interface IFoundingDayYieldRule
{
    /// <summary>
    /// The multiplier applied to National Yield output on a country's founding day (x10).
    /// </summary>
    public const double Multiplier = 10.0;

    /// <summary>
    /// The neutral standard multiplier applied to National Yield output on ordinary non-founding days (x1).
    /// </summary>
    public const double StandardMultiplier = 1.0;

    /// <summary>
    /// Determines whether the specified simulation time falls within the country's founding day (Independence Day in WAT).
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="simulationTime">The simulation timestamp to evaluate.</param>
    /// <returns><c>true</c> if within the country's founding day; otherwise, <c>false</c>.</returns>
    bool IsFoundingDay(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Determines whether the country is currently within its founding day relative to the authoritative Republic clock.
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns><c>true</c> if within the country's founding day; otherwise, <c>false</c>.</returns>
    bool IsFoundingDay(Country country, IRepublicClock clock);

    /// <summary>
    /// Gets the effective yield multiplier for the country at the specified simulation time (10.0 on founding day, 1.0 otherwise).
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="simulationTime">The simulation timestamp to evaluate.</param>
    /// <returns>10.0 if on founding day; otherwise, 1.0.</returns>
    double GetMultiplier(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Applies the founding-day multiplier rule to a calculated <see cref="NationalYield"/> result.
    /// Returns a newly allocated, independent <see cref="NationalYield"/> scaled by x10 if on founding day,
    /// or an unchanged deep clone otherwise. The original <paramref name="yield"/> is never mutated.
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="yield">The calculated National Yield output to evaluate.</param>
    /// <param name="simulationTime">The explicit Republic simulation timestamp.</param>
    /// <returns>An evaluated National Yield result reflecting the founding-day multiplier rule.</returns>
    NationalYield Apply(Country country, NationalYield yield, RepublicTime simulationTime);

    /// <summary>
    /// Applies the founding-day multiplier rule to a calculated <see cref="NationalYield"/> result relative to the authoritative Republic clock.
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="yield">The calculated National Yield output to evaluate.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>An evaluated National Yield result reflecting the founding-day multiplier rule.</returns>
    NationalYield Apply(Country country, NationalYield yield, IRepublicClock clock);

    /// <summary>
    /// Applies the founding-day multiplier rule to a completed <see cref="NationalYieldSnapshot"/>.
    /// Returns a new snapshot with the yield scaled by x10 if on founding day, or an unchanged clone otherwise.
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="snapshot">The completed yield snapshot to evaluate.</param>
    /// <returns>A new <see cref="NationalYieldSnapshot"/> reflecting the founding-day multiplier rule.</returns>
    NationalYieldSnapshot Apply(Country country, NationalYieldSnapshot snapshot);
}

/// <summary>
/// Alias interface for <see cref="IFoundingDayYieldRule"/>.
/// </summary>
public interface IFoundingYieldMultiplierRule : IFoundingDayYieldRule
{
}
