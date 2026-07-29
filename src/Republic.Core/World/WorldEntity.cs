namespace Republic.Core.World;

/// <summary>
/// Minimal entity record registered with the world manager shell.
/// </summary>
/// <param name="Id">Stable entity identifier.</param>
/// <param name="Kind">Entity kind.</param>
/// <param name="Name">Display name.</param>
public sealed record WorldEntity(Guid Id, string Kind, string Name);
