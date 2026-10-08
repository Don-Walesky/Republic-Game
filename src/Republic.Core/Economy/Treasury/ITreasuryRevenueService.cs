namespace Republic.Core.Economy.Treasury;

using Republic.Core.NationalYield;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for applying completed National Yield revenue to sovereign country treasuries.
/// Bridges flow (RepuTreasuryRevenue) to stock (Treasury Balance) in a strictly controlled, deterministic manner.
/// Scales revenue at credit time by sovereign State Capacity.
/// </summary>
public interface ITreasuryRevenueService
{
    /// <summary>
    /// Applies the REPU Treasury Revenue from a completed National Yield calculation result to the specified treasury,
    /// scaled by the specified sovereign State Capacity score (0.0 to 1.0, defaulting to 1.0).
    /// </summary>
    /// <param name="treasury">The country's treasury balance to credit.</param>
    /// <param name="yield">The completed National Yield calculation result.</param>
    /// <param name="stateCapacityScore">The sovereign State Capacity score (0.0 to 1.0) scaling credit.</param>
    /// <returns>The resulting treasury state.</returns>
    RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYield yield, double stateCapacityScore = 1.0);

    /// <summary>
    /// Applies the REPU Treasury Revenue from a completed National Yield snapshot to the specified treasury,
    /// scaled by the specified sovereign State Capacity score (0.0 to 1.0, defaulting to 1.0).
    /// </summary>
    /// <param name="treasury">The country's treasury balance to credit.</param>
    /// <param name="snapshot">The completed National Yield snapshot.</param>
    /// <param name="stateCapacityScore">The sovereign State Capacity score (0.0 to 1.0) scaling credit.</param>
    /// <returns>The resulting treasury state.</returns>
    RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYieldSnapshot snapshot, double stateCapacityScore = 1.0);

    /// <summary>
    /// Applies the REPU Treasury Revenue from a completed National Yield calculation result to the country's treasury,
    /// scaled at credit time by the country's sovereign State Capacity score.
    /// </summary>
    /// <param name="country">The sovereign country whose treasury will receive the revenue.</param>
    /// <param name="yield">The completed National Yield calculation result.</param>
    /// <returns>The country's resulting treasury state.</returns>
    RepuTreasury ApplyRevenue(Country country, NationalYield yield);

    /// <summary>
    /// Applies the REPU Treasury Revenue from a completed National Yield snapshot to the country's treasury,
    /// scaled at credit time by the country's sovereign State Capacity score.
    /// </summary>
    /// <param name="country">The sovereign country whose treasury will receive the revenue.</param>
    /// <param name="snapshot">The completed National Yield snapshot.</param>
    /// <returns>The country's resulting treasury state.</returns>
    RepuTreasury ApplyRevenue(Country country, NationalYieldSnapshot snapshot);
}
