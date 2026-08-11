namespace Republic.Core.Achievements.Models;

using System;

/// <summary>
/// Domain model describing an unlockable game achievement.
/// </summary>
public sealed class AchievementDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsUnlocked { get; set; } = false;
    public DateTimeOffset? UnlockedAt { get; set; } = null;
}
