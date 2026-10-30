namespace Republic.Core.Persistence.Models;

using System;
using System.Collections.Generic;
using Republic.Core.Identity.Models;
using Republic.Core.Narrative.Models;
using Republic.Core.Tasks.Models;
using Republic.Core.World;
using Republic.Core.Workspace.Models;

/// <summary>
/// Domain model encapsulating the complete snapshot of a game session.
/// </summary>
public sealed class FullGameState
{
    public string SaveName { get; set; } = "Autosave";
    public ulong CurrentTick { get; set; }
    public DateTimeOffset SaveTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public WorldState World { get; set; } = new();
    public WorkspaceState Workspace { get; set; } = new();
    public NarrativeSnapshot Narrative { get; set; } = new();
    public List<ScheduledTask> ActiveTasks { get; set; } = new();
    public List<ScheduledTask> CompletedTasks { get; set; } = new();
    public PlayerAccount? Account { get; set; }
    public OnboardingStep OnboardingStep { get; set; } = OnboardingStep.NotStarted;
}
