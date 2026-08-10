namespace Republic.Core.Tests.Crises;

using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class CrisisTriggerEngineTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly DecisionEngine _decisionEngine;

    public CrisisTriggerEngineTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Crisis World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _decisionEngine = new DecisionEngine(_world, _eventBus, _logger);
    }

    [Fact]
    public void EvaluateSimulationMetrics_TriggersFiscalCrisis_WhenTreasuryNegative()
    {
        var engine = new CrisisTriggerEngine(_world, _decisionEngine, _workspace, _eventBus, _logger);
        var currentBalance = _world.Economic.GetIndicators().TreasuryBalance;
        _world.Economic.WithdrawTreasury(currentBalance); // Drain treasury to 0

        var count = engine.EvaluateSimulationMetrics(1);

        Assert.True(count > 0);
        Assert.NotEmpty(_decisionEngine.GetPendingDecisions());
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }

    [Fact]
    public void EvaluateSimulationMetrics_TriggersCivilUnrest_WhenHappinessLow()
    {
        var engine = new CrisisTriggerEngine(_world, _decisionEngine, _workspace, _eventBus, _logger);
        _world.Demographics.UpdateHappiness(20.0);

        var count = engine.EvaluateSimulationMetrics(2);

        Assert.True(count > 0);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }
}
