namespace Republic.Core.Diplomacy.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Events;
using Republic.Core.Diplomacy.Models;
using Republic.Core.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing international relations, treaty proposals, and alliances.
/// </summary>
public sealed class DiplomacyService : IDiplomacyService
{
    private readonly List<DiplomaticRelation> _relations = new();
    private readonly List<DiplomaticTreaty> _treaties = new();
    private readonly IEventBus _eventBus;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public DiplomacyService(IEventBus eventBus, IWorkspaceManager? workspaceManager = null, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;
    }

    public DiplomaticRelation GetRelation(string countryA, string countryB)
    {
        lock (_lock)
        {
            var relation = _relations.FirstOrDefault(r =>
                (r.CountryIdA == countryA && r.CountryIdB == countryB) ||
                (r.CountryIdA == countryB && r.CountryIdB == countryA));

            if (relation == null)
            {
                relation = new DiplomaticRelation
                {
                    CountryIdA = countryA,
                    CountryIdB = countryB,
                    Status = DiplomaticStatus.Neutral,
                    ReputationScore = 50.0
                };
                _relations.Add(relation);
            }

            return relation;
        }
    }

    public async Task<DiplomaticTreaty> ProposeTreatyAsync(string sourceCountry, string targetCountry, TreatyType type, string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCountry);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountry);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var treaty = new DiplomaticTreaty
        {
            Title = title,
            Type = type,
            SignatoryCountryA = sourceCountry,
            SignatoryCountryB = targetCountry,
            IsActive = false,
        };

        lock (_lock)
        {
            _treaties.Add(treaty);
        }

        _logger?.LogInfo($"Treaty proposed [{type}]: '{title}' ({sourceCountry} -> {targetCountry})");
        await _eventBus.PublishAsync(new TreatyProposedEvent(treaty, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = $"Foreign Office ({sourceCountry})",
            Subject = $"DIPLOMATIC PROPOSAL: {title}",
            Body = $"Nations '{sourceCountry}' and '{targetCountry}' have initiated formal treaty discussions regarding a {type}.",
            Folder = "Inbox",
            ActionRequired = true
        });

        return treaty;
    }

    public async Task<bool> AcceptTreatyAsync(string treatyId, CancellationToken cancellationToken = default)
    {
        DiplomaticTreaty? treaty;
        lock (_lock)
        {
            treaty = _treaties.FirstOrDefault(t => t.Id == treatyId);
            if (treaty == null || treaty.IsActive)
            {
                return false;
            }

            treaty.IsActive = true;
            var relation = GetRelation(treaty.SignatoryCountryA, treaty.SignatoryCountryB);
            relation.ActiveTreaties.Add(treaty);

            // Upgrade diplomatic status based on treaty type
            if (treaty.Type == TreatyType.MilitaryAlliance || treaty.Type == TreatyType.MutualDefensePact)
            {
                relation.Status = DiplomaticStatus.Allied;
            }
            else if (treaty.Type == TreatyType.TradeAgreement || treaty.Type == TreatyType.NonAggressionPact)
            {
                relation.Status = DiplomaticStatus.Friendly;
            }
        }

        _logger?.LogInfo($"Treaty ratified: '{treaty.Title}' between '{treaty.SignatoryCountryA}' and '{treaty.SignatoryCountryB}'");
        await _eventBus.PublishAsync(new TreatySignedEvent(treaty, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> BreakTreatyAsync(string treatyId, string violatorCountry, CancellationToken cancellationToken = default)
    {
        DiplomaticTreaty? treaty;
        lock (_lock)
        {
            treaty = _treaties.FirstOrDefault(t => t.Id == treatyId);
            if (treaty == null || !treaty.IsActive)
            {
                return false;
            }

            treaty.IsActive = false;
            var relation = GetRelation(treaty.SignatoryCountryA, treaty.SignatoryCountryB);
            relation.ActiveTreaties.Remove(treaty);
            relation.Status = DiplomaticStatus.Hostile;
            relation.ReputationScore = Math.Max(0, relation.ReputationScore - 40.0);
        }

        _logger?.LogWarning($"TREATY VIOLATED: '{treaty.Title}' broken by '{violatorCountry}'");
        await _eventBus.PublishAsync(new TreatyViolatedEvent(treaty, violatorCountry, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    public void AdjustReputation(string countryA, string countryB, double delta)
    {
        lock (_lock)
        {
            var relation = GetRelation(countryA, countryB);
            relation.ReputationScore = Math.Clamp(relation.ReputationScore + delta, 0.0, 100.0);
            _logger?.LogInfo($"Reputation adjusted between '{countryA}' and '{countryB}': {relation.ReputationScore:0.0}");
        }
    }

    public IReadOnlyList<DiplomaticTreaty> GetActiveTreatiesForCountry(string countryId)
    {
        lock (_lock)
        {
            return _treaties.Where(t => t.IsActive && (t.SignatoryCountryA == countryId || t.SignatoryCountryB == countryId)).ToList().AsReadOnly();
        }
    }
}
