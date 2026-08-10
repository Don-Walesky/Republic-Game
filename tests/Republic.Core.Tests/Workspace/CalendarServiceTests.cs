namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class CalendarServiceTests
{
    [Fact]
    public void ScheduleAppointment_DetectsConflictsCorrectly()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var calendarService = new CalendarService(bus, logger);

        var startTime = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
        calendarService.ScheduleAppointment(new CalendarAppointment
        {
            Title = "Cabinet Strategy Meeting",
            ScheduledDate = startTime,
            Duration = TimeSpan.FromHours(1)
        });

        Assert.True(calendarService.HasConflict(startTime.AddMinutes(30), TimeSpan.FromMinutes(30)));
        Assert.False(calendarService.HasConflict(startTime.AddHours(2), TimeSpan.FromMinutes(30)));
    }
}
