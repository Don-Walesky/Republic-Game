namespace Republic.Core.Tasks.Models;

/// <summary>
/// Represents an asynchronous long-running task (e.g. building construction, policy enactment).
/// </summary>
public sealed class ScheduledTask
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public TaskCategory Category { get; init; } = TaskCategory.Construction;
    public string TargetEntityId { get; init; } = string.Empty;
    public ulong StartTick { get; init; }
    public ulong TotalRequiredTicks { get; init; } = 100;
    public ulong ElapsedTicks { get; set; }
    public double ProgressPercentage => TotalRequiredTicks == 0 ? 100.0 : Math.Min(100.0, (double)ElapsedTicks / TotalRequiredTicks * 100.0);
    public TaskStatus Status { get; set; } = TaskStatus.InProgress;
    public DateTime StartSimulatedDate { get; init; } = DateTime.UtcNow;
    public DateTime EstimatedCompletionDate { get; set; } = DateTime.UtcNow;
}
