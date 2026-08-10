namespace Republic.Core.Crises.Models;

/// <summary>
/// Domain model tracking an active or historical crisis event.
/// </summary>
public sealed class CrisisRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CrisisCategory Category { get; init; }
    public CrisisSeverity Severity { get; init; } = CrisisSeverity.Moderate;
    public string TargetEntityId { get; init; } = string.Empty;
    public ulong TriggeredAtTick { get; init; }
    public bool IsResolved { get; set; }
    public string? AttackerCountryId { get; init; }
}
