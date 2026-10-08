namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Defines the scheduling contract for six-hour National Yield production cycles on the global Republic clock.
/// Cycles are strictly aligned to West Africa Time (WAT / UTC+1) boundaries at 00:00, 06:00, 12:00, and 18:00 WAT.
/// </summary>
public interface INationalYieldSchedule
{
    /// <summary>
    /// Gets the next six-hour cycle boundary strictly after the specified simulation time.
    /// </summary>
    /// <param name="time">The reference simulation timestamp.</param>
    /// <returns>The earliest six-hour cycle boundary strictly greater than <paramref name="time"/>.</returns>
    RepublicTime GetNextBoundary(RepublicTime time);

    /// <summary>
    /// Gets the next due six-hour cycle boundary for the specified country.
    /// Evaluates the country's <see cref="Country.LastCreditedBoundary"/>, or falls back to founding time or current simulation time.
    /// </summary>
    /// <param name="country">The sovereign country.</param>
    /// <param name="currentSimulationTime">The current Republic simulation time.</param>
    /// <returns>The next six-hour cycle boundary due for the country.</returns>
    RepublicTime GetNextDueBoundary(Country country, RepublicTime currentSimulationTime);

    /// <summary>
    /// Calculates the due six-hour production cycle boundaries on the global WAT clock
    /// strictly after <paramref name="lastCreditedBoundary"/> and up to <paramref name="currentSimulationTime"/>,
    /// capped at <paramref name="maxBoundaries"/> (and never exceeding 8).
    /// </summary>
    /// <param name="lastCreditedBoundary">The timestamp of the last credited cycle boundary, or null if none credited yet.</param>
    /// <param name="currentSimulationTime">The current Republic simulation time.</param>
    /// <param name="maxBoundaries">The maximum number of boundaries to return in a single call (default is 8, capped at 8).</param>
    /// <returns>A list of due boundary timestamps in chronological order.</returns>
    IReadOnlyList<RepublicTime> GetDueBoundaries(
        RepublicTime? lastCreditedBoundary,
        RepublicTime currentSimulationTime,
        int maxBoundaries = 8);

    /// <summary>
    /// Calculates the due six-hour production cycle boundaries for the specified sovereign country
    /// up to <paramref name="currentSimulationTime"/>, capped at <paramref name="maxBoundaries"/> (and never exceeding 8).
    /// </summary>
    /// <param name="country">The sovereign country.</param>
    /// <param name="currentSimulationTime">The current Republic simulation time.</param>
    /// <param name="maxBoundaries">The maximum number of boundaries to return in a single call (default is 8, capped at 8).</param>
    /// <returns>A list of due boundary timestamps in chronological order.</returns>
    IReadOnlyList<RepublicTime> GetDueBoundaries(
        Country country,
        RepublicTime currentSimulationTime,
        int maxBoundaries = 8);
}
