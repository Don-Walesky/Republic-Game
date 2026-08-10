namespace Republic.Core.World;

using Republic.Core.World.Services;

/// <summary>
/// Manages the current world shell, entity registry, and world simulation services.
/// </summary>
public interface IWorldManager
{
    /// <summary>
    /// Gets the current world state.
    /// </summary>
    WorldState Current { get; }

    ICountryService Countries { get; }
    IGeographyService Geography { get; }
    IResourceService Resources { get; }
    IDemographicService Demographics { get; }
    IEconomicService Economic { get; }
    IPoliticalCultureService PoliticalCulture { get; }

    /// <summary>
    /// Creates a new world shell.
    /// </summary>
    Task<WorldState> CreateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers an entity in the current world shell.
    /// </summary>
    Task<WorldEntity> RegisterEntityAsync(string kind, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity from the current world shell.
    /// </summary>
    bool RemoveEntity(Guid entityId);

    /// <summary>
    /// Returns the registered entities.
    /// </summary>
    IReadOnlyCollection<WorldEntity> GetEntities();

    /// <summary>
    /// Advances the world shell to the supplied tick.
    /// </summary>
    void AdvanceTo(ulong tick);

    /// <summary>
    /// Creates a deep snapshot of the current world state.
    /// </summary>
    WorldState Snapshot();

    /// <summary>
    /// Restores a previously captured snapshot.
    /// </summary>
    void Restore(WorldState state);
}
