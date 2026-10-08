namespace Republic.Core.Economy.Treasury;

using Republic.Core.NationalYield;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for applying evaluated National Yield REPU revenue to sovereign treasuries.
/// Strictly consumes existing calculation outputs without recalculating or modifying yield data.
/// Scales revenue at credit time by sovereign State Capacity.
/// </summary>
public sealed class TreasuryRevenueService : ITreasuryRevenueService
{
    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYield yield, double stateCapacityScore = 1.0)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        ArgumentNullException.ThrowIfNull(yield);

        var clampedScore = Math.Clamp(stateCapacityScore, 0.0, 1.0);
        var unscaledRevenue = yield.RepuTreasuryRevenue;
        if (unscaledRevenue > 0.0 && clampedScore > 0.0)
        {
            var scaledRevenue = unscaledRevenue * clampedScore;
            treasury.Deposit(scaledRevenue);
        }

        return treasury;
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYieldSnapshot snapshot, double stateCapacityScore = 1.0)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        ArgumentNullException.ThrowIfNull(snapshot);

        // Prevent duplicate revenue application for the same completed snapshot/cycle
        if (!string.IsNullOrWhiteSpace(snapshot.Id))
        {
            if (treasury.HasAppliedSnapshot(snapshot.Id))
            {
                return treasury;
            }

            treasury.RecordSnapshotApplied(snapshot.Id);
        }

        var clampedScore = Math.Clamp(stateCapacityScore, 0.0, 1.0);
        var unscaledRevenue = snapshot.Yield.RepuTreasuryRevenue;
        if (unscaledRevenue > 0.0 && clampedScore > 0.0)
        {
            var scaledRevenue = unscaledRevenue * clampedScore;
            treasury.Deposit(scaledRevenue);
        }

        return treasury;
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(Country country, NationalYield yield)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(yield);
        return ApplyRevenue(country.Treasury, yield, country.StateCapacityScore);
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(Country country, NationalYieldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(snapshot);
        return ApplyRevenue(country.Treasury, snapshot, country.StateCapacityScore);
    }
}
