namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing national currency and monetary baseline.
/// </summary>
public sealed class Currency
{
    public string Code { get; init; } = "RPC";
    public string Symbol { get; init; } = "R$";
    public string Name { get; init; } = "Republic Credit";
    public double ExchangeRateToUSD { get; set; } = 1.0;
    public double InflationRate { get; set; } = 0.025;
    public double CentralBankReserve { get; set; } = 50_000_000_000.0;
}
