namespace Republic.Core.Workspace.Models;

/// <summary>
/// Aggregate state snapshot for the Executive Workspace.
/// </summary>
public sealed class WorkspaceState
{
    public OfficeRoomState RoomState { get; set; } = new();
    public List<Visitor> Visitors { get; set; } = new();
    public List<PhoneCall> PhoneCalls { get; set; } = new();
    public List<EmailMessage> Emails { get; set; } = new();
    public List<NewsArticle> NewsArticles { get; set; } = new();
    public List<CalendarAppointment> Appointments { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
