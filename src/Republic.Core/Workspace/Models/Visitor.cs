namespace Republic.Core.Workspace.Models;

/// <summary>
/// Status of a visitor to the executive office.
/// </summary>
public enum VisitorStatus
{
    Waiting,
    InMeeting,
    Departed,
    Dismissed
}

/// <summary>
/// Represents a visitor waiting or meeting in the executive workspace.
/// </summary>
public sealed class Visitor
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Faction { get; init; } = string.Empty;
    public DateTime ArrivalTime { get; init; } = DateTime.UtcNow;
    public int PatienceSeconds { get; init; } = 300;
    public string Purpose { get; init; } = string.Empty;
    public VisitorStatus Status { get; set; } = VisitorStatus.Waiting;
}
