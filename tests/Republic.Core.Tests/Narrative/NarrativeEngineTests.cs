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
    public async Task MakeStoryChoice_AppliesEffects_AndResolvesEvent()
    {
        var engine = new NarrativeEngine(_world, _eventBus, _workspace, _logger);
        await engine.EvaluateNarrativeTriggersAsync(10);

        var active = engine.GetActiveStoryEvents()[0];
        var choice = active.Choices[0];

        var resolved = await engine.MakeStoryChoiceAsync(active.Id, choice.Id);

        Assert.True(resolved);
        Assert.Empty(engine.GetActiveStoryEvents());
    }
}
