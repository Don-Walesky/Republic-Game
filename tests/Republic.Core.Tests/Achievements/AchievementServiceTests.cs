namespace Republic.Core.Tests.Achievements;

using System;
using System.Threading.Tasks;
using Republic.Core.Achievements.Services;
using Republic.Core.Events;
using Xunit;

public sealed class AchievementServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;

    public AchievementServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
    }

    [Fact]
    public void DefaultAchievements_AreRegisteredOnInitialization()
    {
        var service = new AchievementService(_eventBus, _logger);
        var list = service.GetAchievements();

        Assert.True(list.Count >= 4);
        Assert.Contains(list, a => a.Id == "peace-broker");
        Assert.Contains(list, a => a.Id == "economic-miracle");
    }

    [Fact]
    public async Task UnlockAchievement_SetsUnlockedAndPublishesEvent()
    {
        var service = new AchievementService(_eventBus, _logger);
        bool eventFired = false;

        _eventBus.Subscribe<AchievementUnlockedEvent>((e, _) =>
        {
            if (e.Achievement.Id == "peace-broker") eventFired = true;
            return ValueTask.CompletedTask;
        });

        bool unlocked = service.UnlockAchievement("peace-broker");
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.True(unlocked);
        Assert.True(eventFired);
    }
}
