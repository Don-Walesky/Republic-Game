namespace Republic.Core.Tasks.Services;

using Republic.Core.Tasks.Models;
using Republic.Core.Time;

/// <summary>
/// Service interface managing time-based task queues, countdown progress, and completion events.
/// </summary>
public interface ITaskQueueManager
{
    ScheduledTask QueueTask(string title, TaskCategory category, string targetEntityId, ulong durationTicks, ITimeSystem timeSystem);
    bool PauseTask(string taskId);
    bool ResumeTask(string taskId);
    bool CancelTask(string taskId);
    IReadOnlyList<ScheduledTask> GetActiveTasks();
    IReadOnlyList<ScheduledTask> GetCompletedTasks();
    Task<int> ProcessTickAsync(ulong currentTick, CancellationToken cancellationToken = default);
}
