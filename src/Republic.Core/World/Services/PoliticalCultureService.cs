namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for political factions, ideology spectrum, and constitutional framework.
/// </summary>
public sealed class PoliticalCultureService : IPoliticalCultureService
{
    private readonly List<Faction> _factions = new();
    private readonly Constitution _constitution = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public PoliticalCultureService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public Faction RegisterFaction(Faction faction)
    {
        ArgumentNullException.ThrowIfNull(faction);
        lock (_lock)
        {
            _factions.Add(faction);
        }

        _logger?.LogInfo($"Faction registered: '{faction.Name}' ({faction.Ideology} - Influence: {faction.InfluencePercentage:0.0}%)");
        return faction;
    }

    public Faction? GetFaction(string factionId)
    {
        lock (_lock)
        {
            return _factions.FirstOrDefault(f => f.Id == factionId);
        }
    }

    public IReadOnlyList<Faction> GetFactions()
    {
        lock (_lock)
        {
            return _factions.ToList().AsReadOnly();
        }
    }

    public bool UpdateApproval(string factionId, double newApproval)
    {
        lock (_lock)
        {
            var faction = _factions.FirstOrDefault(f => f.Id == factionId);
            if (faction == null)
            {
                return false;
            }

            faction.ApprovalRating = Math.Clamp(newApproval, 0.0, 100.0);
            _logger?.LogInfo($"Faction '{faction.Name}' approval updated to {faction.ApprovalRating:0.0}%");
            _eventBus.PublishAsync(new FactionApprovalChangedEvent(faction.Id, faction.ApprovalRating, DateTimeOffset.UtcNow));
            return true;
        }
    }

    public Constitution GetConstitution() => _constitution;

    public bool AmendConstitution(string name, string governmentSystem, IEnumerable<string>? enactedRights = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(governmentSystem);

        lock (_lock)
        {
            _constitution.Name = name;
            _constitution.GovernmentSystem = governmentSystem;
            if (enactedRights != null)
            {
                _constitution.EnactedRights = enactedRights.ToList();
            }

            _logger?.LogInfo($"Constitution amended: '{name}' ({governmentSystem})");
            return true;
        }
    }
}
