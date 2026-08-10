namespace Republic.Core.Workspace.Models;

/// <summary>
/// Urgency level of a phone call.
/// </summary>
public enum CallUrgency
{
    Low,
    Medium,
    High,
    Emergency
}

/// <summary>
/// Represents a phone call to the executive workspace.
/// </summary>
public sealed class PhoneCall
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string CallerName { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
    public CallUrgency Urgency { get; init; } = CallUrgency.Medium;
    public string Subject { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public DateTime TimeReceived { get; init; } = DateTime.UtcNow;
    public bool IsAnswered { get; set; }
    public bool IsActive { get; set; } = true;
}
