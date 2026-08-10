namespace Republic.Core.Narrative.Events;

using Republic.Core.Events;
using Republic.Core.Narrative.Models;

public sealed record StoryEventTriggeredEvent(StoryEvent Event, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record StoryChoiceMadeEvent(StoryEvent Event, StoryChoice Choice, DateTimeOffset OccurredAt) : IGameEvent;
