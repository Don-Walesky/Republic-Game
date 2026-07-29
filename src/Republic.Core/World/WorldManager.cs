namespace Republic.Core.World;

using Republic.Core.Events;

/// <summary>
/// Minimal world shell for Wave 0 integration and save tests.
/// </summary>
public sealed class WorldManager : IWorldManager
{
    private readonly IEventBus _eventBus;
    private readonly Dictionary<Guid, WorldEntity> _entities = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldManager"/> class.
    /// </summary>
    public WorldManager(IEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Current = new WorldState();
    }

    /// <inheritdoc />
    public WorldState Current { get; private set; }

    /// <inheritdoc />
    public async Task<WorldState> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Current = new WorldState
        {
            WorldId = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            CurrentTick = 0,
            Entities = new List<WorldEntity>(),
        };

        _entities.Clear();
        await _eventBus.PublishAsync(new WorldCreatedEvent(Current.WorldId, Current.Name, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return Snapshot();
    }

    /// <inheritdoc />
    public async Task<WorldEntity> RegisterEntityAsync(string kind, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        EnsureWorldExists();
        var entity = new WorldEntity(Guid.NewGuid(), kind, name);
        _entities[entity.Id] = entity;
        Current.Entities = _entities.Values.ToList();
        await _eventBus.PublishAsync(new WorldEntityRegisteredEvent(Current.WorldId, entity, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public bool RemoveEntity(Guid entityId)
    {
        EnsureWorldExists();

        var removed = _entities.Remove(entityId);
        if (removed)
        {
            Current.Entities = _entities.Values.ToList();
            _ = _eventBus.PublishAsync(new WorldEntityRemovedEvent(Current.WorldId, entityId, DateTimeOffset.UtcNow));
        }

        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<WorldEntity> GetEntities() => _entities.Values.ToArray();

    /// <inheritdoc />
    public void AdvanceTo(ulong tick)
    {
        EnsureWorldExists();
        Current.CurrentTick = tick;
    }

    /// <inheritdoc />
    public WorldState Snapshot() => new()
    {
        WorldId = Current.WorldId,
        Name = Current.Name,
        CreatedAt = Current.CreatedAt,
        CurrentTick = Current.CurrentTick,
        Entities = _entities.Values.ToList(),
    };

    /// <inheritdoc />
    public void Restore(WorldState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Current = new WorldState
        {
            WorldId = state.WorldId,
            Name = state.Name,
            CreatedAt = state.CreatedAt,
            CurrentTick = state.CurrentTick,
            Entities = state.Entities.Select(entity => entity with { }).ToList(),
        };

        _entities.Clear();
        foreach (var entity in Current.Entities)
        {
            _entities[entity.Id] = entity;
        }
    }

    private void EnsureWorldExists()
    {
        if (Current.WorldId == Guid.Empty)
        {
            throw new InvalidOperationException("A world must be created before it can be used.");
        }
    }
}
