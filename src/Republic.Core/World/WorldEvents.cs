namespace Republic.Core.World;

using Republic.Core.Events;

/// <summary>
/// Emitted when a world shell is created.
/// </summary>
public sealed record WorldCreatedEvent(Guid WorldId, string Name, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when an entity is registered.
/// </summary>
public sealed record WorldEntityRegisteredEvent(Guid WorldId, WorldEntity Entity, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when an entity is removed.
/// </summary>
public sealed record WorldEntityRemovedEvent(Guid WorldId, Guid EntityId, DateTimeOffset OccurredAt) : IGameEvent;
