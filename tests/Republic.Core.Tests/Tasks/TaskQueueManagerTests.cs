namespace Republic.Core.Tests.Tasks;

using Republic.Core.Events;
using Republic.Core.Tasks.Events;
using Republic.Core.Tasks.Models;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Xunit;

public sealed class TaskQueueManagerTests
{
    [Fact]
    public async Task QueueTask_PublishesTaskQueuedEvent()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var timeSystem = new TimeSystem(new TimeSystemConfiguration(), bus, logger);
        var taskManager = new TaskQueueManager(bus, logger);

        TaskQueuedEvent? queuedEvent = null;
        bus.Subscribe<TaskQueuedEvent>((e, _) =>
        {
            queuedEvent = e;
            return ValueTask.CompletedTask;
        });

        var task = taskManager.QueueTask(
            title: "Build National University",
            category: TaskCategory.Construction,
            targetEntityId: "BUILDING_UNI_01",
            durationTicks: 100,
            timeSystem: timeSystem);

        await bus.ProcessQueuedEventsAsync();

        Assert.NotNull(queuedEvent);
        Assert.Equal("Build National University", queuedEvent.Task.Title);
        Assert.Equal(TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public async Task ProcessTickAsync_AdvancesProgressAndCompletesTask()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var timeSystem = new TimeSystem(new TimeSystemConfiguration(), bus, logger);
        var taskManager = new TaskQueueManager(bus, logger);

        TaskCompletedEvent? completedEvent = null;
        bus.Subscribe<TaskCompletedEvent>((e, _) =>
        {
            completedEvent = e;
            return ValueTask.CompletedTask;
        });

        var task = taskManager.QueueTask("Pass Tax Reform", TaskCategory.PolicyLegislation, "POLICY_TAX_01", 2, timeSystem);

        await taskManager.ProcessTickAsync(1);
        Assert.Equal(50.0, task.ProgressPercentage);
        Assert.Equal(TaskStatus.InProgress, task.Status);

        await taskManager.ProcessTickAsync(2);
        await bus.ProcessQueuedEventsAsync();

        Assert.Equal(100.0, task.ProgressPercentage);
        Assert.Equal(TaskStatus.Completed, task.Status);
        Assert.NotNull(completedEvent);
        Assert.Equal("Pass Tax Reform", completedEvent.Task.Title);
    }

    [Fact]
    public async Task PauseAndCancel_UpdatesTaskStatus()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var timeSystem = new TimeSystem(new TimeSystemConfiguration(), bus, logger);
        var taskManager = new TaskQueueManager(bus, logger);

        TaskCancelledEvent? cancelledEvent = null;
        bus.Subscribe<TaskCancelledEvent>((e, _) =>
        {
            cancelledEvent = e;
            return ValueTask.CompletedTask;
        });

        var task = taskManager.QueueTask("Deploy 1st Division", TaskCategory.MilitaryDeployment, "UNIT_MIL_01", 50, timeSystem);

        var paused = taskManager.PauseTask(task.Id);
        Assert.True(paused);
        Assert.Equal(TaskStatus.Paused, task.Status);

        var resumed = taskManager.ResumeTask(task.Id);
        Assert.True(resumed);
        Assert.Equal(TaskStatus.InProgress, task.Status);

        var cancelled = taskManager.CancelTask(task.Id);
        await bus.ProcessQueuedEventsAsync();

        Assert.True(cancelled);
        Assert.Equal(TaskStatus.Cancelled, task.Status);
        Assert.NotNull(cancelledEvent);
        Assert.Equal(task.Id, cancelledEvent.TaskId);
    }
}
