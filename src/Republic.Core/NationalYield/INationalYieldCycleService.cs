namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for executing sovereign National Yield cycles at a specific Republic simulation time.
/// Obtains country calculation inputs and evaluates yield via <see cref="INationalYieldCalculator"/> without mutating the clock or country.
/// </summary>
public interface INationalYieldCycleService
{
    /// <summary>
    /// Executes a yield cycle for the specified country at the given simulation time.
    /// </summary>
    /// <param name="country">The sovereign country whose yield is to be calculated.</param>
    /// <param name="simulationTime">The explicit Republic simulation time context.</param>
    /// <returns>An immutable <see cref="NationalYieldSnapshot"/> capturing the evaluated yield at the specified simulation time.</returns>
    NationalYieldSnapshot ExecuteCycle(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Executes a yield cycle for the specified country at the current time of the authoritative Republic clock.
    /// </summary>
    /// <param name="country">The sovereign country whose yield is to be calculated.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>An immutable <see cref="NationalYieldSnapshot"/> capturing the evaluated yield at the clock's simulation time.</returns>
    NationalYieldSnapshot ExecuteCycle(Country country, IRepublicClock clock);

    /// <summary>
    /// Retrieves the most recent National Yield snapshot calculated for the specified country, if any.
    /// </summary>
    /// <param name="countryId">The country identifier.</param>
    /// <returns>The latest <see cref="NationalYieldSnapshot"/>, or <c>null</c> if no cycle has been executed for the country.</returns>
    NationalYieldSnapshot? GetLatestSnapshot(string countryId);

    /// <summary>
    /// Clears any cached snapshots stored in this service instance.
    /// </summary>
    void ClearSnapshots();
}
