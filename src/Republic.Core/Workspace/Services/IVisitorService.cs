namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Service interface for visitor handling in the executive office.
/// </summary>
public interface IVisitorService
{
    void RegisterVisitor(Visitor visitor);
    Visitor? GetNextVisitor();
    bool StartMeeting(string visitorId);
    bool DecideVisitorAudience(string visitorId, bool granted);
    bool DismissVisitor(string visitorId);
    IReadOnlyList<Visitor> GetActiveVisitors();
}
