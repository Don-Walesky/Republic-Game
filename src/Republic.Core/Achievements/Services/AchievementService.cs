namespace Republic.Core.Achievements.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Achievements.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Events;

/// <summary>
/// Event emitted when a game achievement is unlocked.
/// </summary>
public sealed record AchievementUnlockedEvent(AchievementDefinition Achievement, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Service managing game achievement definitions, progress evaluation, and unlocks.
/// </summary>
public sealed class AchievementService
{
    private readonly List<AchievementDefinition> _achievements = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public AchievementService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
        RegisterDefaultAchievements();
    }

    private void RegisterDefaultAchievements()
    {
        RegisterAchievement(new AchievementDefinition { Id = "peace-broker", Title = "Peace Broker", Description = "Sign 3 international peace or trade treaties.", Category = "Diplomacy" });
        RegisterAchievement(new AchievementDefinition { Id = "economic-miracle", Title = "Economic Miracle", Description = "Achieve a national treasury balance exceeding $50,000,000.", Category = "Economy" });
        RegisterAchievement(new AchievementDefinition { Id = "iron-dictator", Title = "Iron Dictator", Description = "Maintain DEFCON 1 maximum military readiness while keeping stability above 80%.", Category = "Military" });
        RegisterAchievement(new AchievementDefinition { Id = "crisis-master", Title = "Crisis Master", Description = "Successfully resolve 5 critical national decision contexts.", Category = "Executive" });
    }

    public void RegisterAchievement(AchievementDefinition achievement)
    {
        ArgumentNullException.ThrowIfNull(achievement);
        lock (_lock)
        {
            if (!_achievements.Any(a => a.Id == achievement.Id))
            {
                _achievements.Add(achievement);
            }
        }
    }

    public IReadOnlyList<AchievementDefinition> GetAchievements()
    {
        lock (_lock)
        {
            return _achievements.ToList().AsReadOnly();
        }
    }

    public bool UnlockAchievement(string achievementId)
    {
        AchievementDefinition? ach;
        lock (_lock)
        {
            ach = _achievements.FirstOrDefault(a => a.Id == achievementId);
            if (ach == null || ach.IsUnlocked) return false;

            ach.IsUnlocked = true;
            ach.UnlockedAt = DateTimeOffset.UtcNow;
        }

        _logger?.LogInfo($"[ACHIEVEMENT UNLOCKED] '{ach.Title}' - {ach.Description}");
        _eventBus.PublishAsync(new AchievementUnlockedEvent(ach, DateTimeOffset.UtcNow));
        return true;
    }
}
