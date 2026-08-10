namespace Republic.Core.Workspace.Models;

/// <summary>
/// Represents a scheduled calendar appointment for the executive.
/// </summary>
public sealed class CalendarAppointment
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = "Executive Desk";
    public DateTime ScheduledDate { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; } = TimeSpan.FromMinutes(30);
    public List<string> Attendees { get; init; } = new();
    public int Priority { get; init; } = 1;
    public bool IsCompleted { get; set; }
}
