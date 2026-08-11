namespace Republic.Core.Narrative.Models;

/// <summary>
/// Domain model representing a narrative story event.
/// </summary>
public sealed class StoryEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string NarrativeText { get; init; } = string.Empty;
    public string Category { get; init; } = "Political";
    public bool IsTriggered { get; set; }
    public bool IsResolved { get; set; }
    public ulong PrerequisiteTick { get; init; }
    public string? PrerequisiteMetric { get; init; }
    public double MinMetricValue { get; init; }
    public List<StoryChoice> Choices { get; init; } = new();
}
