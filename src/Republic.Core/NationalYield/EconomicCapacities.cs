namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model encapsulating the economic and productive capacities of a sovereign nation.
/// </summary>
public sealed class EconomicCapacities
{
    /// <summary>
    /// Gets or sets sovereign monetary revenue from tax collections, state enterprise returns, and sovereign assets in REPU (R).
    /// </summary>
    public double RepuTreasuryRevenue { get; set; }

    /// <summary>
    /// Gets or sets manufacturing base, heavy industry, construction throughput, and fabrication capability.
    /// </summary>
    public double IndustrialCapacity { get; set; }

    /// <summary>
    /// Gets or sets power grid output, fuel generation, energy reserves, and electrical distribution stability.
    /// </summary>
    public double EnergyCapacity { get; set; }

    /// <summary>
    /// Gets or sets physical transport networks, logistics corridors, deepwater ports, rail, and telecommunications throughput.
    /// </summary>
    public double InfrastructureCapacity { get; set; }

    /// <summary>
    /// Gets or sets sovereign financial sector depth, domestic banking capitalization, credit rating, and capital liquidity.
    /// </summary>
    public double FinancialCapacity { get; set; }

    /// <summary>
    /// Creates a deep clone of the current economic capacities.
    /// </summary>
    public EconomicCapacities Clone() => new()
    {
        RepuTreasuryRevenue = RepuTreasuryRevenue,
        IndustrialCapacity = IndustrialCapacity,
        EnergyCapacity = EnergyCapacity,
        InfrastructureCapacity = InfrastructureCapacity,
        FinancialCapacity = FinancialCapacity
    };
}
