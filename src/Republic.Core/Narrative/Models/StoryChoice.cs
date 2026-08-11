namespace Republic.Core.Narrative.Models;

using Republic.Core.Decisions.Models;

/// <summary>
/// Choice option within a narrative story event.
/// </summary>
public sealed class StoryChoice
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Text { get; init; } = string.Empty;
    public string OutcomeDescription { get; init; } = string.Empty;
    public List<PolicyEffect> Effects { get; init; } = new();
    public string? FollowUpEventId { get; init; }
}
