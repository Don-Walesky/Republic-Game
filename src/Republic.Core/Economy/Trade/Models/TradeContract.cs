namespace Republic.Core.Economy.Trade.Models;

/// <summary>
/// Domain model representing a binding bilateral commercial trade contract.
/// </summary>
public sealed class TradeContract
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string ExporterCountryId { get; init; } = string.Empty;
    public string ImporterCountryId { get; init; } = string.Empty;
    public CommodityType Commodity { get; init; }
    public double QuantityUnits { get; init; } = 100.0;
    public double PricePerUnit { get; init; } = 100.0;
    public double TotalQuarterlyValue => QuantityUnits * PricePerUnit;
    public bool IsActive { get; set; } = true;
}
