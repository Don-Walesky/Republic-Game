namespace Republic.Core.Tests.Crises;

using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Services;

public sealed class InterPlayerWarfareTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly DecisionEngine _decisionEngine;
    private readonly InterPlayerWarfareService _warfareService;

    public InterPlayerWarfareTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Warfare Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _decisionEngine = new DecisionEngine(_world, _eventBus, _logger);
        _warfareService = new InterPlayerWarfareService(_world, _decisionEngine, _workspace, _eventBus, _logger);
    }

    [Fact]
    public async Task LaunchTradeEmbargo_DrainsTradeBalance_AndPromptsDecision()
    {
        var initialTrade = _world.Economic.GetIndicators().TradeBalance;

        var crisis = await _warfareService.LaunchTradeEmbargoAsync("RivalNationA", "PlayerCountry");

        Assert.NotNull(crisis);
        Assert.Equal(initialTrade - 500_000_000.0, _world.Economic.GetIndicators().TradeBalance);
        Assert.NotEmpty(_decisionEngine.GetPendingDecisions());
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }

    [Fact]
    public async Task LaunchCyberAttack_DeductsTreasury_AndTriggersAlert()
    {
        var initialTreasury = _world.Economic.GetIndicators().TreasuryBalance;

        var crisis = await _warfareService.LaunchCyberAttackAsync("AttackerNation", "PlayerCountry", "Banking");

        Assert.NotNull(crisis);
        Assert.Equal(initialTreasury - 100_000_000.0, _world.Economic.GetIndicators().TreasuryBalance);
        Assert.NotEmpty(_decisionEngine.GetPendingDecisions());
    }

    [Fact]
    public async Task FundSubversion_IncreasesFactionApproval_AndDropsStability()
    {
        var country = _world.Countries.RegisterCountry(new Country { Id = "target-c", Name = "Targetia", BaselineStability = 80.0 });
        var faction = _world.PoliticalCulture.RegisterFaction(new Faction { Id = "opp-f", Name = "Opposition", ApprovalRating = 30.0 });

        var crisis = await _warfareService.FundSubversionAsync("RivalState", "target-c", "opp-f", 50_000_000);

        Assert.NotNull(crisis);
        Assert.Equal(72.0, country.BaselineStability);
        Assert.Equal(45.0, faction.ApprovalRating);
    }

    [Fact]
    public async Task DeployBorderSkirmish_PublishesNews_AndPromptsDefenseDecision()
    {
        var crisis = await _warfareService.DeployBorderSkirmishAsync("AggressorNation", "RegionNorth");

        Assert.NotNull(crisis);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
        Assert.NotEmpty(_decisionEngine.GetPendingDecisions());
    }
}
