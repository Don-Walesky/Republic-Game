namespace Republic.Core.World;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Services;

/// <summary>
/// World manager coordinating entity registries and Wave 2 world simulation services.
/// </summary>
public sealed class WorldManager : IWorldManager
{
    private readonly IEventBus _eventBus;
    private readonly Dictionary<Guid, WorldEntity> _entities = new();

    public ICountryService Countries { get; }
    public IGeographyService Geography { get; }
    public IResourceService Resources { get; }
    public IDemographicService Demographics { get; }
    public IEconomicService Economic { get; }
    public IPoliticalCultureService PoliticalCulture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldManager"/> class with default services.
    /// </summary>
    public WorldManager(IEventBus eventBus, ILogger? logger = null)
        : this(
            eventBus,
            new CountryService(eventBus, logger),
            new GeographyService(logger),
            new ResourceService(eventBus, logger),
            new DemographicService(eventBus, logger),
            new EconomicService(eventBus, logger),
            new PoliticalCultureService(eventBus, logger))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldManager"/> class.
    /// </summary>
    public WorldManager(
        IEventBus eventBus,
        ICountryService countryService,
        IGeographyService geographyService,
        IResourceService resourceService,
        IDemographicService demographicService,
        IEconomicService economicService,
        IPoliticalCultureService politicalCultureService)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Countries = countryService ?? throw new ArgumentNullException(nameof(countryService));
        Geography = geographyService ?? throw new ArgumentNullException(nameof(geographyService));
        Resources = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        Demographics = demographicService ?? throw new ArgumentNullException(nameof(demographicService));
        Economic = economicService ?? throw new ArgumentNullException(nameof(economicService));
        PoliticalCulture = politicalCultureService ?? throw new ArgumentNullException(nameof(politicalCultureService));
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
        Demographics.AdvanceDemographicsTick();
        Economic.AdvanceEconomyTick();
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
