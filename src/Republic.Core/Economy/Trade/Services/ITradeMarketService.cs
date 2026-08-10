namespace Republic.Core.Economy.Trade.Services;

using Republic.Core.Economy.Trade.Models;

/// <summary>
/// Service interface managing global commodity markets and trade contract execution.
/// </summary>
public interface ITradeMarketService
{
    IReadOnlyList<CommodityPrice> GetMarketPrices();
    Task<TradeContract> EstablishTradeContractAsync(string exporter, string importer, CommodityType commodity, double quantity, CancellationToken cancellationToken = default);
    Task ProcessMarketTickAsync(ulong currentTick, CancellationToken cancellationToken = default);
}
