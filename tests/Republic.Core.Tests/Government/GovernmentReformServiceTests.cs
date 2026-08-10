namespace Republic.Core.Tests.Government;

using Republic.Core.Events;
using Republic.Core.Government.Models;
using Republic.Core.Government.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class GovernmentReformServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public GovernmentReformServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Government Test World").GetAwaiter().GetResult();

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
    public async Task EnactConstitutionalReform_TransformsSystem_AndPublishesNews()
    {
        var service = new GovernmentReformService(_world, _eventBus, _workspace, _logger);
        Assert.Equal(GovernmentType.PresidentialRepublic, service.GetCurrentGovernmentSystem());

        var reform = new ConstitutionalReform
        {
            Title = "Parliamentary Sovereignty Amendment",
            TargetSystem = GovernmentType.ParliamentaryRepublic
        };

        var success = await service.EnactConstitutionalReformAsync(reform);

        Assert.True(success);
        Assert.Equal(GovernmentType.ParliamentaryRepublic, service.GetCurrentGovernmentSystem());
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }
}
