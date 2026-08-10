namespace Republic.Core.Tests.Media;

using Republic.Core.Events;
using Republic.Core.Media.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class PressConferenceServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public PressConferenceServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Media Test World").GetAwaiter().GetResult();

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
    public async Task HostAndAnswerPressConference_UpdatesDemographics_AndPublishesNews()
    {
        var service = new PressConferenceService(_world, _eventBus, _workspace, _logger);
        var q = await service.HostPressConferenceAsync("Economic Reform");

        Assert.NotNull(q);
        Assert.NotEmpty(q.Options);

        var answered = await service.AnswerQuestionAsync(q.Id, q.Options[0].Id);

        Assert.True(answered);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }
}
