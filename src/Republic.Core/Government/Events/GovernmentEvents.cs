namespace Republic.Core.Government.Events;

using Republic.Core.Events;
using Republic.Core.Government.Models;

public sealed record ConstitutionalReformEnactedEvent(ConstitutionalReform Reform, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record GovernmentSystemTransformedEvent(GovernmentType OldSystem, GovernmentType NewSystem, DateTimeOffset OccurredAt) : IGameEvent;
