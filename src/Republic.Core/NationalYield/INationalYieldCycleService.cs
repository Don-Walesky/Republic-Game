namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for executing sovereign National Yield cycles at an explicit Republic simulation time.
/// Obtains country calculation inputs and evaluates yield via <see cref="INationalYieldCalculator"/> without mutating the clock or country.
/// Preserves snapshot integrity via defensive copying and operates on the country's current in-memory state.
/// </summary>
public interface INationalYieldCycleService
{
    /// <summary>
    /// Executes a yield cycle for the specified country at the given simulation time.
    /// Evaluates the country's current in-memory state, applies the founding-day multiplier rule (x10 on founding day),
    /// applies the resulting REPU Treasury Revenue to the country's treasury with duplicate protection,
    /// and records <paramref name="simulationTime"/> as the authoritative simulation timestamp.
    /// Does not reconstruct historical country state.
    /// </summary>
    /// <param name="country">The sovereign country whose current state is to be evaluated.</param>
    /// <param name="simulationTime">The explicit Republic simulation timestamp recorded for the calculation.</param>
    /// <returns>An independent <see cref="NationalYieldSnapshot"/> capturing the evaluated yield.</returns>
    NationalYieldSnapshot ExecuteCycle(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Executes a yield cycle for the specified country at the current time of the authoritative Republic clock.
    /// Evaluates the country's current in-memory state and records the clock's current time as the simulation timestamp.
    /// Does not reconstruct historical country state.
    /// </summary>
    /// <param name="country">The sovereign country whose current state is to be evaluated.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>An independent <see cref="NationalYieldSnapshot"/> capturing the evaluated yield.</returns>
    NationalYieldSnapshot ExecuteCycle(Country country, IRepublicClock clock);

    /// <summary>
    /// Retrieves a defensive copy of the most recent National Yield snapshot calculated for the specified country, if any.
    /// </summary>
    /// <param name="countryId">The country identifier.</param>
    /// <returns>A defensive copy of the latest <see cref="NationalYieldSnapshot"/>, or <c>null</c> if no cycle has been executed for the country.</returns>
    NationalYieldSnapshot? GetLatestSnapshot(string countryId);

    /// <summary>
    /// Gets the schedule engine used to evaluate six-hour cycle boundaries.
    /// </summary>
    INationalYieldSchedule Schedule { get; }

    /// <summary>
    /// Evaluates and credits all due six-hour production cycle boundaries for the specified country
    /// up to <paramref name="simulationTime"/>, capped at 8 boundaries per call.
    /// Credits each boundary through the existing once-only treasury path, updates the country's
    /// <see cref="Country.LastCreditedBoundary"/>, and returns the snapshots generated.
    /// </summary>
    /// <param name="country">The sovereign country whose due cycles are to be evaluated.</param>
    /// <param name="simulationTime">The current Republic simulation timestamp.</param>
    /// <returns>A read-only list of snapshots generated for the credited boundaries.</returns>
    IReadOnlyList<NationalYieldSnapshot> ExecuteDueCycles(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Evaluates and credits all due six-hour production cycle boundaries for the specified country
    /// up to the clock's current time, capped at 8 boundaries per call.
    /// Credits each boundary through the existing once-only treasury path, updates the country's
    /// <see cref="Country.LastCreditedBoundary"/>, and returns the snapshots generated.
    /// </summary>
    /// <param name="country">The sovereign country whose due cycles are to be evaluated.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>A read-only list of snapshots generated for the credited boundaries.</returns>
    IReadOnlyList<NationalYieldSnapshot> ExecuteDueCycles(Country country, IRepublicClock clock);

    /// <summary>
    /// Calculates the due six-hour production cycle boundaries for the specified country
    /// up to <paramref name="simulationTime"/> without executing them, capped at 8 boundaries per call.
    /// </summary>
    /// <param name="country">The sovereign country.</param>
    /// <param name="simulationTime">The current Republic simulation timestamp.</param>
    /// <returns>A read-only list of due boundary timestamps.</returns>
    IReadOnlyList<RepublicTime> GetDueBoundaries(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Calculates the due six-hour production cycle boundaries for the specified country
    /// up to the clock's current time without executing them, capped at 8 boundaries per call.
    /// </summary>
    /// <param name="country">The sovereign country.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>A read-only list of due boundary timestamps.</returns>
    IReadOnlyList<RepublicTime> GetDueBoundaries(Country country, IRepublicClock clock);

    /// <summary>
    /// Clears any cached snapshots stored in this service instance.
    /// </summary>
    void ClearSnapshots();
}
