namespace Republic.Core.Economy.Treasury;

using Republic.Core.NationalYield;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for applying evaluated National Yield REPU revenue to sovereign treasuries.
/// Strictly consumes existing calculation outputs without recalculating or modifying yield data.
/// </summary>
public sealed class TreasuryRevenueService : ITreasuryRevenueService
{
    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYield yield)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        ArgumentNullException.ThrowIfNull(yield);

        var revenue = yield.RepuTreasuryRevenue;
        if (revenue > 0.0)
        {
            treasury.Deposit(revenue);
        }

        return treasury;
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(RepuTreasury treasury, NationalYieldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return ApplyRevenue(treasury, snapshot.Yield);
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(Country country, NationalYield yield)
    {
        ArgumentNullException.ThrowIfNull(country);
        return ApplyRevenue(country.Treasury, yield);
    }

    /// <inheritdoc />
    public RepuTreasury ApplyRevenue(Country country, NationalYieldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(snapshot);
        return ApplyRevenue(country.Treasury, snapshot.Yield);
    }
}
