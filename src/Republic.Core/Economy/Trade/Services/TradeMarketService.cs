namespace Republic.Core.Economy.Trade.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Economy.Trade.Models;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing international commodity price fluctuations and trade contracts.
/// </summary>
public sealed class TradeMarketService : ITradeMarketService
{
    private readonly List<CommodityPrice> _marketPrices = new();
    private readonly List<TradeContract> _contracts = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public TradeMarketService(
        IWorldManager worldManager,
        IEventBus eventBus,
        IWorkspaceManager? workspaceManager = null,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;

        InitializePrices();
    }

    private void InitializePrices()
    {
        foreach (CommodityType type in Enum.GetValues(typeof(CommodityType)))
        {
            var basePrice = type switch
            {
                CommodityType.Oil => 85.0,
                CommodityType.Grain => 45.0,
                CommodityType.RareEarths => 250.0,
                CommodityType.Arms => 500.0,
                _ => 120.0
            };

            _marketPrices.Add(new CommodityPrice
            {
                Commodity = type,
                BasePricePerUnit = basePrice,
                CurrentMarketPrice = basePrice
            });
        }
    }

    public IReadOnlyList<CommodityPrice> GetMarketPrices()
    {
        lock (_lock)
        {
            return _marketPrices.ToList().AsReadOnly();
        }
    }

    public async Task<TradeContract> EstablishTradeContractAsync(string exporter, string importer, CommodityType commodity, double quantity, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exporter);
        ArgumentException.ThrowIfNullOrWhiteSpace(importer);

        CommodityPrice? priceObj;
        lock (_lock)
        {
            priceObj = _marketPrices.FirstOrDefault(p => p.Commodity == commodity);
        }

        var pricePerUnit = priceObj?.CurrentMarketPrice ?? 100.0;
        var contract = new TradeContract
        {
            ExporterCountryId = exporter,
            ImporterCountryId = importer,
            Commodity = commodity,
            QuantityUnits = quantity,
            PricePerUnit = pricePerUnit,
            IsActive = true
        };

        lock (_lock)
        {
            _contracts.Add(contract);
        }

        _logger?.LogInfo($"Trade Contract Established [{commodity}]: '{exporter}' -> '{importer}' (Val: ${contract.TotalQuarterlyValue:N0})");

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Ministry of Commerce",
            Subject = $"TRADE DEAL SIGNED: {commodity} Export Contract",
            Body = $"Bilateral trade agreement finalized. '{exporter}' exporting {quantity:N0} units of {commodity} to '{importer}'.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return contract;
    }

    public Task ProcessMarketTickAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        if (currentTick % 50 != 0)
        {
            return Task.CompletedTask;
        }

        lock (_lock)
        {
            foreach (var price in _marketPrices)
            {
                // Dynamic supply/demand price fluctuation
                var ratio = price.GlobalDemandLevel / Math.Max(1.0, price.GlobalSupplyLevel);
                price.CurrentMarketPrice = Math.Clamp(price.BasePricePerUnit * ratio, price.BasePricePerUnit * 0.5, price.BasePricePerUnit * 3.0);
            }
        }

        _logger?.LogDebug($"Market Tick processed at tick {currentTick}.");
        return Task.CompletedTask;
    }
}
