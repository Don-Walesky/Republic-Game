namespace Republic.Core.Legislature.Events;

using Republic.Core.Events;
using Republic.Core.Legislature.Models;

public sealed record BillIntroducedEvent(Bill Bill, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record ParliamentaryVoteConductedEvent(Bill Bill, VoteResult Result, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record BillEnactedEvent(Bill Bill, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record ExecutiveVetoExercisedEvent(Bill Bill, DateTimeOffset OccurredAt) : IGameEvent;
