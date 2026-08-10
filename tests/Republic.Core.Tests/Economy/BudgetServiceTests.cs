namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class BudgetServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public BudgetServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Budget Test World").GetAwaiter().GetResult();

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
    public async Task ProcessQuarterlyReport_CalculatesTaxRevenues_AndUpdatesTreasury()
    {
        var service = new BudgetService(_world, _eventBus, _workspace, _logger);
        await service.UpdateTaxPolicyAsync(new TaxPolicy { IncomeTaxRate = 0.30, CorporateTaxRate = 0.25 });

        var initialTreasury = _world.Economic.GetIndicators().TreasuryBalance;
        var report = await service.ProcessQuarterlyReportAsync(100);

        Assert.NotNull(report);
        Assert.Equal(1, report.QuarterIndex);
        Assert.True(report.TotalTaxRevenue > 0);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
