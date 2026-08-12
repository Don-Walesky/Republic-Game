namespace Republic.Core.Tests.Narrative;

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Narrative.Services;
using Republic.Core.World;

public class NarrativeBranchingTests
{
    [Fact]
    public async Task EvaluateNarrativeTriggers_Fires_Whistleblower_Leak_On_Tick_15()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var engine = new NarrativeEngine(worldManager, eventBus);

        await engine.EvaluateNarrativeTriggersAsync(15);
        var activeEvents = engine.GetActiveStoryEvents();

        Assert.Contains(activeEvents, e => e.Id == "story-whistleblower-leak");
    }

    [Fact]
    public async Task MakeStoryChoice_Resolves_Event_And_Applies_Effects()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var engine = new NarrativeEngine(worldManager, eventBus);

        await engine.EvaluateNarrativeTriggersAsync(25);
        var borderEvent = engine.GetActiveStoryEvents().First(e => e.Id == "story-border-skirmish");
        var choice = borderEvent.Choices.First();

        bool resolved = await engine.MakeStoryChoiceAsync(borderEvent.Id, choice.Id);

        Assert.True(resolved);
        Assert.DoesNotContain(engine.GetActiveStoryEvents(), e => e.Id == "story-border-skirmish");
        Assert.Contains(engine.GetResolvedStoryEvents(), e => e.Id == "story-border-skirmish");
    }
}
