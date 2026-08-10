namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Service interface for executive schedule and calendar management.
/// </summary>
public interface ICalendarService
{
    void ScheduleAppointment(CalendarAppointment appointment);
    bool CompleteAppointment(string appointmentId);
    IReadOnlyList<CalendarAppointment> GetUpcomingAppointments(DateTime fromDate);
    bool HasConflict(DateTime startTime, TimeSpan duration);
}
