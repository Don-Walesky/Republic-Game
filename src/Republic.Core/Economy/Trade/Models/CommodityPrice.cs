namespace Republic.Core.Economy.Trade.Models;

/// <summary>
/// Domain model tracking real-time global commodity prices, supply, and demand levels.
/// </summary>
public sealed class CommodityPrice
{
    public CommodityType Commodity { get; init; }
    public double BasePricePerUnit { get; init; } = 100.0;
    public double CurrentMarketPrice { get; set; } = 100.0;
    public double GlobalSupplyLevel { get; set; } = 1000.0;
    public double GlobalDemandLevel { get; set; } = 1000.0;
}
