namespace Republic.Core.Tests.AI;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.AI.Models;
using Republic.Core.AI.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;

public class RivalAIEscalationTests
{
    [Fact]
    public async Task ProcessAITick_Executes_Actions_On_Interval_Ticks()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        await worldManager.CreateAsync("AI Test World");

        var diplomacy = new DiplomacyService(worldManager, eventBus);
        var warfare = new InterPlayerWarfareService(worldManager, eventBus);
        var intelligence = new IntelligenceService(worldManager, eventBus);

        var service = new RivalAIService(worldManager, diplomacy, warfare, intelligence, eventBus);
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
