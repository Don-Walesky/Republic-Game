namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Central manager interface for the Executive Workspace.
/// </summary>
public interface IWorkspaceManager
{
    IVisitorService Visitors { get; }
    IPhoneService Phone { get; }
    IEmailService Email { get; }
    INewsService News { get; }
    ICalendarService Calendar { get; }
    WorkspaceState GetCurrentState();
    void UpdateRoomState(string? roomName = null, string? lightingMode = null, string? audioZone = null);
    void ProcessTimeTick(long totalTicks);
}
