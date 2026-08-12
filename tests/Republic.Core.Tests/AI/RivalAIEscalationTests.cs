namespace Republic.Core.Tests.AI;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.AI.Models;
using Republic.Core.AI.Services;
using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Services;

public class RivalAIEscalationTests
{
    [Fact]
    public async Task ProcessAITick_Executes_Actions_On_Interval_Ticks()
    {
        var logger = new TestLogger();
        var eventBus = new EventBus(new EventBusOptions(), logger);
        var worldManager = new WorldManager(eventBus, logger);
        await worldManager.CreateAsync("AI Test World");

        worldManager.Countries.RegisterCountry(new Country
        {
            Id = "player-country",
            Name = "Republic Capital Region",
            GovernmentType = "Presidential Republic",
            BaselineStability = 75.0
        });

        var workspace = new WorkspaceManager(
            new VisitorService(eventBus, logger),
            new PhoneService(eventBus, logger),
            new EmailService(eventBus, logger),
            new NewsService(eventBus, logger),
            new CalendarService(eventBus, logger),
            eventBus,
            logger);

        var decisionEngine = new DecisionEngine(worldManager, eventBus, logger);
        var diplomacy = new DiplomacyService(eventBus, workspace, logger);
        var warfare = new InterPlayerWarfareService(worldManager, decisionEngine, workspace, eventBus, logger);
        var intelligence = new IntelligenceService(worldManager, eventBus, workspace, logger);

        var service = new RivalAIService(worldManager, diplomacy, warfare, intelligence, eventBus, logger);
        service.RegisterRivalBot(new RivalAIBot
        {
            CountryId = "rival-alpha",
            Name = "Valorian Hegemony",
            Behavior = RivalAIBehavior.Opportunistic
        });

        // Tick 99 -> 0 actions
        int actions99 = await service.ProcessAITickAsync(99);
        Assert.Equal(0, actions99);

        // Tick 100 -> AI tick runs
        int actions100 = await service.ProcessAITickAsync(100);
        Assert.True(actions100 >= 1);
    }
}
