namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Service implementing calendar scheduling, conflict detection, and event management.
/// </summary>
public sealed class CalendarService : ICalendarService
{
    private readonly List<CalendarAppointment> _appointments = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public CalendarService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ScheduleAppointment(CalendarAppointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        lock (_lock)
        {
            _appointments.Add(appointment);
        }

        _logger.LogInfo($"Appointment scheduled: '{appointment.Title}' at {appointment.ScheduledDate:yyyy-MM-dd HH:mm}");
        _eventBus.PublishAsync(new AppointmentScheduledEvent(appointment, DateTimeOffset.UtcNow));
    }

    public bool CompleteAppointment(string appointmentId)
    {
        lock (_lock)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment == null || appointment.IsCompleted)
            {
                return false;
            }

            appointment.IsCompleted = true;
            _logger.LogInfo($"Completed appointment: '{appointment.Title}'");
            return true;
        }
    }

    public IReadOnlyList<CalendarAppointment> GetUpcomingAppointments(DateTime fromDate)
    {
        lock (_lock)
        {
            return _appointments
                .Where(a => !a.IsCompleted && a.ScheduledDate >= fromDate)
                .OrderBy(a => a.ScheduledDate)
                .ToList()
                .AsReadOnly();
        }
    }

    public bool HasConflict(DateTime startTime, TimeSpan duration)
    {
        var endTime = startTime + duration;
        lock (_lock)
        {
            return _appointments.Any(a =>
                !a.IsCompleted &&
                a.ScheduledDate < endTime &&
                (a.ScheduledDate + a.Duration) > startTime);
        }
    }
}
