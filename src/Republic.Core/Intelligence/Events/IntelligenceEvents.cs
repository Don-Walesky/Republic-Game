namespace Republic.Core.Intelligence.Events;

using Republic.Core.Events;
using Republic.Core.Intelligence.Models;

public sealed record SpyNetworkEstablishedEvent(SpyNetwork Network, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record OperationLaunchedEvent(CovertOperation Operation, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record OperationSucceededEvent(CovertOperation Operation, string OutcomeSummary, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record OperationExposedEvent(CovertOperation Operation, string IncidentSummary, DateTimeOffset OccurredAt) : IGameEvent;
