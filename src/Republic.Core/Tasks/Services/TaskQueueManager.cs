namespace Republic.Core.Tasks.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Tasks.Events;
using Republic.Core.Tasks.Models;
using Republic.Core.Time;

/// <summary>
/// Pipeline manager executing duration countdowns, tick progress updates, and completion dispatches.
/// </summary>
public sealed class TaskQueueManager : ITaskQueueManager
{
    private readonly List<ScheduledTask> _tasks = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public TaskQueueManager(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledTask QueueTask(string title, TaskCategory category, string targetEntityId, ulong durationTicks, ITimeSystem timeSystem)
    {
        ArgumentNullException.ThrowIfNull(timeSystem);
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        var currentTick = timeSystem.CurrentTick;
        var startSimulatedDate = timeSystem.CurrentSimulatedDateTime;
        var estimatedCompletionDate = startSimulatedDate.AddSeconds(durationTicks * timeSystem.DeltaTime);

        var task = new ScheduledTask
        {
            Title = title,
            Category = category,
            TargetEntityId = targetEntityId,
            StartTick = currentTick,
            TotalRequiredTicks = Math.Max(1, durationTicks),
            ElapsedTicks = 0,
            Status = TaskStatus.InProgress,
            StartSimulatedDate = startSimulatedDate,
            EstimatedCompletionDate = estimatedCompletionDate
        };

        lock (_lock)
        {
            _tasks.Add(task);
        }

        _logger.LogInfo($"Task queued [{category}] '{title}' - Duration: {durationTicks} ticks (Est. Completion: {estimatedCompletionDate:yyyy-MM-dd HH:mm})");
        _eventBus.PublishAsync(new TaskQueuedEvent(task, DateTimeOffset.UtcNow));
        return task;
    }

    public bool PauseTask(string taskId)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null || task.Status != TaskStatus.InProgress)
            {
                return false;
            }

            task.Status = TaskStatus.Paused;
            _logger.LogInfo($"Task paused: '{task.Title}'");
            return true;
        }
    }

    public bool ResumeTask(string taskId)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null || task.Status != TaskStatus.Paused)
            {
                return false;
            }

            task.Status = TaskStatus.InProgress;
            _logger.LogInfo($"Task resumed: '{task.Title}'");
            return true;
        }
    }

    public bool CancelTask(string taskId)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null || task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
            {
                return false;
            }

            task.Status = TaskStatus.Cancelled;
            _logger.LogInfo($"Task cancelled: '{task.Title}'");
            _eventBus.PublishAsync(new TaskCancelledEvent(task.Id, "Cancelled by user", DateTimeOffset.UtcNow));
            return true;
        }
    }

    public ScheduledTask? GetTask(string taskId)
    {
        lock (_lock)
        {
            return _tasks.FirstOrDefault(t => t.Id == taskId);
        }
    }

    public IReadOnlyList<ScheduledTask> GetActiveTasks()
    {
        lock (_lock)
        {
            return _tasks.Where(t => t.Status == TaskStatus.InProgress || t.Status == TaskStatus.Paused).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<ScheduledTask> GetCompletedTasks()
    {
        lock (_lock)
        {
            return _tasks.Where(t => t.Status == TaskStatus.Completed).ToList().AsReadOnly();
        }
    }

    public async Task<int> ProcessTickAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        List<ScheduledTask> activeCopy;
        lock (_lock)
        {
            activeCopy = _tasks.Where(t => t.Status == TaskStatus.InProgress).ToList();
        }

        var processedCount = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var task in activeCopy)
        {
            task.ElapsedTicks++;
            processedCount++;

            if (task.ElapsedTicks >= task.TotalRequiredTicks)
            {
                task.Status = TaskStatus.Completed;
                _logger.LogInfo($"Task completed! [{task.Category}] '{task.Title}' (Target Entity: '{task.TargetEntityId}')");
                await _eventBus.PublishAsync(new TaskCompletedEvent(task, now), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _eventBus.PublishAsync(new TaskProgressUpdatedEvent(task, task.ProgressPercentage, now), cancellationToken).ConfigureAwait(false);
            }
        }

        return processedCount;
    }
}
