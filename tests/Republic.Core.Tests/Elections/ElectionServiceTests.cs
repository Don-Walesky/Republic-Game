namespace Republic.Core.Tests.Elections;

using Republic.Core.Diagnostics;
using Republic.Core.Elections.Events;
using Republic.Core.Elections.Services;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;
using Xunit;

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
        var service = new ElectionService(_world, _eventBus, _world.Countries.Clock, _workspace, _logger);
        await service.StartCampaignSeasonAsync();

        var result = await service.ConductElectionAsync("President Sterling", "Senator Vance");

        Assert.NotNull(result);
        Assert.True(result.IncumbentVotes > 0);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }

    [Fact]
    public void ElectionService_ThrowsArgumentNullException_WhenClockIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ElectionService(_world, _eventBus, null!));
    }

    [Fact]
    public void ElectionService_UsesSuppliedClock()
    {
        var clock = _world.Countries.Clock;
        var service = new ElectionService(_world, _eventBus, clock);

        Assert.Same(clock, service.Clock);
    }

    [Fact]
    public async Task StartCampaignSeasonAsync_PublishesCampaignSeasonBeganEvent_WithAuthoritativeSimulationTime()
    {
        CampaignSeasonBeganEvent? capturedEvent = null;
        _eventBus.Subscribe<CampaignSeasonBeganEvent>((evt, _) =>
        {
            capturedEvent = evt;
            return ValueTask.CompletedTask;
        });

        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(45);
        clock.AdvanceHours(14);

        var service = new ElectionService(_world, _eventBus, clock, _workspace, _logger);
        await service.StartCampaignSeasonAsync();
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.NotNull(capturedEvent);
        Assert.Equal(clock.CurrentTime.WatTimestamp, capturedEvent.OccurredAt);
        Assert.NotEqual(DateTimeOffset.UtcNow.Date, capturedEvent.OccurredAt.Date);
    }

    [Fact]
    public async Task ConductElectionAsync_PublishesElectionConductedAndPresidentialTransitionEvents_WithAuthoritativeSimulationTime()
    {
        ElectionConductedEvent? conductedEvent = null;
        PresidentialTransitionEvent? transitionEvent = null;

        _eventBus.Subscribe<ElectionConductedEvent>((evt, _) =>
        {
            conductedEvent = evt;
            return ValueTask.CompletedTask;
        });

        _eventBus.Subscribe<PresidentialTransitionEvent>((evt, _) =>
        {
            transitionEvent = evt;
            return ValueTask.CompletedTask;
        });

        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(120);
        clock.AdvanceHours(21);

        var service = new ElectionService(_world, _eventBus, clock, _workspace, _logger);
        await service.StartCampaignSeasonAsync();
        var result = await service.ConductElectionAsync("President Sterling", "Senator Vance");
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.NotNull(result);
        Assert.NotNull(conductedEvent);
        Assert.NotNull(transitionEvent);

        Assert.Equal(clock.CurrentTime.WatTimestamp, conductedEvent.OccurredAt);
        Assert.Equal(clock.CurrentTime.WatTimestamp, transitionEvent.OccurredAt);
        Assert.NotEqual(DateTimeOffset.UtcNow.Date, conductedEvent.OccurredAt.Date);
        Assert.NotEqual(DateTimeOffset.UtcNow.Date, transitionEvent.OccurredAt.Date);
    }
}
