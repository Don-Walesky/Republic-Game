namespace Republic.Core.Tests.AI;

using Republic.Core.AI.Models;
using Republic.Core.AI.Services;
using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Services;

public sealed class RivalAIServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly DecisionEngine _decisionEngine;
    private readonly InterPlayerWarfareService _warfareService;
    private readonly DiplomacyService _diplomacyService;
    private readonly IntelligenceService _intelligenceService;

    public RivalAIServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("AI Test World").GetAwaiter().GetResult();

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
        _diplomacyService = new DiplomacyService(_eventBus, _workspace, _logger);
        _intelligenceService = new IntelligenceService(_world, _eventBus, _workspace, _logger);
    }

    [Fact]
    public async Task DiplomaticAI_ProposesTradeAgreement_OnAITick()
    {
        var service = new RivalAIService(_world, _diplomacyService, _warfareService, _intelligenceService, _eventBus, _logger);
        _world.Countries.RegisterCountry(new Country { Id = "player-country", Name = "Player Nation", BaselineStability = 80.0 });
        _world.Countries.RegisterCountry(new Country { Id = "ai-country", Name = "AI Ally", BaselineStability = 85.0 });

        service.RegisterRivalBot(new RivalAIBot { CountryId = "ai-country", Name = "AI Ally", Behavior = RivalAIBehavior.Diplomatic });

        var actions = await service.ProcessAITickAsync(100);

        Assert.Equal(1, actions);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
