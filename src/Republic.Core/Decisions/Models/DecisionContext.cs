namespace Republic.Core.Decisions.Models;

/// <summary>
/// Domain model representing a decision scenario presented to the executive.
/// </summary>
public sealed class DecisionContext
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Executive";
    public bool IsUrgent { get; set; }
    public ulong ExpirationTick { get; set; }
    public List<DecisionOption> Options { get; set; } = new();
}
