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
    bool DismissVisitor(string visitorId);
    IReadOnlyList<Visitor> GetActiveVisitors();
}
