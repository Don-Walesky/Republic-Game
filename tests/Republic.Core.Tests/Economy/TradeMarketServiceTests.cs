namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Trade.Models;
using Republic.Core.Economy.Trade.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class TradeMarketServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public TradeMarketServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Trade Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
    }

    [Fact]
    public async Task EstablishTradeContract_CalculatesValue_AndSendsEmail()
    {
        var service = new TradeMarketService(_world, _eventBus, _workspace, _logger);
        var prices = service.GetMarketPrices();

        Assert.NotEmpty(prices);

        var contract = await service.EstablishTradeContractAsync("Arcadia", "Norse", CommodityType.Oil, 500.0);

        Assert.NotNull(contract);
        Assert.True(contract.TotalQuarterlyValue > 0);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
