namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing macro-economic metrics.
/// </summary>
public sealed class EconomicIndicator
{
    public double GrossDomesticProduct { get; set; } = 450_000_000_000.0;
    public double TreasuryBalance { get; set; } = 12_500_000_000.0;
    public double TradeBalance { get; set; } = 2_100_000_000.0;
    public double ProductionOutput { get; set; } = 100.0;
    public double InflationRate { get; set; } = 0.025;
}
