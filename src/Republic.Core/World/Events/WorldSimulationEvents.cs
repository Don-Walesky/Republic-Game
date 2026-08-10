namespace Republic.Core.World.Events;

using Republic.Core.Events;
using Republic.Core.World.Models;

/// <summary>
/// Event emitted when a new country is initialized in the world.
/// </summary>
public sealed record CountryCreatedEvent(Country Country, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when resources are extracted from a resource node.
/// </summary>
public sealed record ResourceExtractedEvent(string NodeId, double ExtractedAmount, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when demographic metrics are updated.
/// </summary>
public sealed record DemographicUpdatedEvent(Demographics Demographics, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when macroeconomic indicators shift.
/// </summary>
public sealed record EconomyUpdatedEvent(EconomicIndicator Indicators, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a political faction's approval rating changes.
/// </summary>
public sealed record FactionApprovalChangedEvent(string FactionId, double NewApproval, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a country's baseline stability changes.
/// </summary>
public sealed record StabilityChangedEvent(string CountryId, double NewStability, DateTimeOffset OccurredAt) : IGameEvent;
