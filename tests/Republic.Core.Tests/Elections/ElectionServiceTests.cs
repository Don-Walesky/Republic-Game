namespace Republic.Core.Tests.Elections;

using Republic.Core.Elections.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class ElectionServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public ElectionServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Election Test World").GetAwaiter().GetResult();

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
    public async Task StartCampaign_AndConductElection_ReturnsWinnerAndSendsEmail()
    {
        var service = new ElectionService(_world, _eventBus, _workspace, _logger);
        await service.StartCampaignSeasonAsync();

        var result = await service.ConductElectionAsync("President Sterling", "Senator Vance");

        Assert.NotNull(result);
        Assert.True(result.IncumbentVotes > 0);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
