namespace Republic.Core.Events;

/// <summary>
/// Marker interface for deterministic simulation events.
/// </summary>
public interface IGameEvent
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
