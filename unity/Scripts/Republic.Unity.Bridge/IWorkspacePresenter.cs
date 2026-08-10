namespace Republic.Unity.Bridge;

using Republic.Core.Workspace.Models;

/// <summary>
/// Bridge interface enabling future Unity UI views (Desk, Phone, Email, Visitor room, Calendar, News Ticker)
/// to receive deterministic updates from the headless C# Workspace simulation core without coupling core logic to Unity.
/// </summary>
public interface IWorkspacePresenter
{
    void OnWorkspaceStateUpdated(WorkspaceState state);
    void OnVisitorArrived(Visitor visitor);
    void OnPhoneRinging(PhoneCall call);
    void OnEmailReceived(EmailMessage email);
    void OnNewsPublished(NewsArticle article);
    void OnAppointmentReminded(CalendarAppointment appointment);
}
