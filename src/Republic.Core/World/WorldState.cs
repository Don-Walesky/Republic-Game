namespace Republic.Core.World;

/// <summary>
/// Serializable snapshot of the current world shell.
/// </summary>
public sealed class WorldState
{
    /// <summary>
    /// Gets or sets the world identifier.
    /// </summary>
    public Guid WorldId { get; set; }

    /// <summary>
    /// Gets or sets the world name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC creation time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the current tick recorded by the world shell.
    /// </summary>
    public ulong CurrentTick { get; set; }

    /// <summary>
    /// Gets or sets the registered entity list.
    /// </summary>
    public List<WorldEntity> Entities { get; set; } = new();
}
