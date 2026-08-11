namespace Republic.Core.Narrative.Services;

using Republic.Core.Narrative.Models;

/// <summary>
/// Service interface managing branching narrative storylines and narrative choices.
/// </summary>
public interface INarrativeEngine
{
    IReadOnlyList<StoryEvent> GetActiveStoryEvents();
    IReadOnlyList<StoryEvent> GetResolvedStoryEvents();
    Task EvaluateNarrativeTriggersAsync(ulong currentTick, CancellationToken cancellationToken = default);
    Task<bool> MakeStoryChoiceAsync(string storyEventId, string choiceId, CancellationToken cancellationToken = default);
    NarrativeSnapshot GetNarrativeState();
    void RestoreNarrativeState(NarrativeSnapshot snapshot);
}
