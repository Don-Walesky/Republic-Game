namespace Republic.Core.Narrative.Models;

/// <summary>
/// Serializable snapshot containing full narrative engine state for persistence.
/// </summary>
public sealed class NarrativeSnapshot
{
    public List<StoryEventSnapshot> Events { get; set; } = new();
}

public sealed class StoryEventSnapshot
{
    public string Id { get; set; } = string.Empty;
    public bool IsTriggered { get; set; }
    public bool IsResolved { get; set; }
}
