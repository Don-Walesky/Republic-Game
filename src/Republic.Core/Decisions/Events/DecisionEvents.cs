namespace Republic.Core.Decisions.Events;

using Republic.Core.Decisions.Models;
using Republic.Core.Events;

public sealed record DecisionPromptedEvent(DecisionContext Decision, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record DecisionExecutedEvent(string DecisionId, DecisionOption ChosenOption, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record DecreeEnactedEvent(string DecreeId, string Title, DateTimeOffset OccurredAt) : IGameEvent;
