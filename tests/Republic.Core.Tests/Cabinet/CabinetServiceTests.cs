namespace Republic.Core.Tests.Cabinet;

using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class CabinetServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public CabinetServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Cabinet Test World").GetAwaiter().GetResult();

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
    public async Task AppointMinister_SetsAppointed_AndSendsEmail()
    {
        var service = new CabinetService(_world, _eventBus, _workspace, _logger);
        var minister = new Minister { Name = "General Vance", CompetenceRating = 90.0, LoyaltyRating = 85.0 };

        var appointed = await service.AppointMinisterAsync(minister, CabinetPortfolio.Defense);

        Assert.True(appointed.IsAppointed);
        Assert.Equal(CabinetPortfolio.Defense, appointed.Portfolio);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }

    [Fact]
    public async Task EvaluateMinisterIntrigues_DetectsDisloyalMinisters()
    {
        var service = new CabinetService(_world, _eventBus, _workspace, _logger);
        var disloyal = new Minister { Name = "Rogue Minister", LoyaltyRating = 20.0 };
        await service.AppointMinisterAsync(disloyal, CabinetPortfolio.Finance);

        var intrigues = service.EvaluateMinisterIntrigues(100);

        Assert.Equal(1, intrigues);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }
}
