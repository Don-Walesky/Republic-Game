namespace Republic.Core.Tests.Narrative;

using Republic.Core.Events;
using Republic.Core.Narrative.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class NarrativeEngineTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public NarrativeEngineTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Narrative Test World").GetAwaiter().GetResult();

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
    public async Task EvaluateNarrativeTriggers_FiresStoryEvent_AndSendsEmail()
    {
        var engine = new NarrativeEngine(_world, _eventBus, _workspace, _logger);
        await engine.EvaluateNarrativeTriggersAsync(10);

        var active = engine.GetActiveStoryEvents();

        Assert.NotEmpty(active);
        Assert.Equal("Offshore Energy Reserve Uncovered", active[0].Title);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }

    [Fact]
    public async Task MakeStoryChoice_WithFollowUpEvent_TriggersFollowUpEvent()
    {
        var engine = new NarrativeEngine(_world, _eventBus, _workspace, _logger);
        await engine.EvaluateNarrativeTriggersAsync(10);

        var initialEvent = engine.GetActiveStoryEvents()[0];
        var nationalizeChoice = initialEvent.Choices[0]; // Has FollowUpEventId = "story-assembly-debate"

        await engine.MakeStoryChoiceAsync(initialEvent.Id, nationalizeChoice.Id);

        var activeAfterChoice = engine.GetActiveStoryEvents();
        Assert.Single(activeAfterChoice);
        Assert.Equal("National Assembly Emergency Debate", activeAfterChoice[0].Title);
        Assert.Single(engine.GetResolvedStoryEvents());
    }

    [Fact]
    public async Task GetNarrativeState_AndRestoreNarrativeState_RoundTripsState()
    {
        var engine = new NarrativeEngine(_world, _eventBus, _workspace, _logger);
        await engine.EvaluateNarrativeTriggersAsync(10);

        var snapshot = engine.GetNarrativeState();

        var newEngine = new NarrativeEngine(_world, _eventBus, _workspace, _logger);
        Assert.Empty(newEngine.GetActiveStoryEvents());

        newEngine.RestoreNarrativeState(snapshot);
        Assert.Single(newEngine.GetActiveStoryEvents());
        Assert.Equal("Offshore Energy Reserve Uncovered", newEngine.GetActiveStoryEvents()[0].Title);
    }
}
