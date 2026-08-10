namespace Republic.Core.Tasks.Models;

/// <summary>
/// Execution status of a scheduled task.
/// </summary>
public enum TaskStatus
{
    Queued,
    InProgress,
    Paused,
    Completed,
    Cancelled
}
