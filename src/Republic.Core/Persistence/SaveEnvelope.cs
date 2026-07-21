namespace Republic.Core.Persistence;

/// <summary>
/// Metadata wrapper for persisted state.
/// </summary>
public sealed class SaveEnvelope<TState>
{
    /// <summary>
    /// Gets or sets the persisted state kind.
    /// </summary>
    public string Kind { get; set; } = typeof(TState).Name;

    /// <summary>
    /// Gets or sets the save format version.
    /// </summary>
    public int FormatVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the UTC save timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the saved payload.
    /// </summary>
    public TState? State { get; set; }
}
