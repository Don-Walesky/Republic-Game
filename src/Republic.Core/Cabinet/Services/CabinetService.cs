namespace Republic.Core.Cabinet.Services;

using Republic.Core.Cabinet.Events;
using Republic.Core.Cabinet.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing the executive cabinet, portfolio assignments, passive domain bonuses, and plots.
/// </summary>
public sealed class CabinetService : ICabinetService
{
    private readonly List<Minister> _ministers = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public CabinetService(
        IWorldManager worldManager,
        IEventBus eventBus,
        IWorkspaceManager? workspaceManager = null,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;
    }

    public IReadOnlyList<Minister> GetAllMinisters()
    {
        lock (_lock)
        {
            return _ministers.ToList().AsReadOnly();
        }
    }

    public Minister? GetAppointedMinister(CabinetPortfolio portfolio)
    {
        lock (_lock)
        {
            return _ministers.FirstOrDefault(m => m.IsAppointed && m.Portfolio == portfolio);
        }
    }

    public async Task<Minister> AppointMinisterAsync(Minister minister, CabinetPortfolio portfolio, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(minister);

        lock (_lock)
        {
            // Dismiss existing minister in portfolio if any
            var existing = _ministers.FirstOrDefault(m => m.IsAppointed && m.Portfolio == portfolio);
            if (existing != null)
            {
                existing.IsAppointed = false;
            }

            minister.Portfolio = portfolio;
            minister.IsAppointed = true;

            if (!_ministers.Contains(minister))
            {
                _ministers.Add(minister);
            }
        }

        _logger?.LogInfo($"Cabinet appointment: '{minister.Name}' appointed to Portfolio [{portfolio}].");
        await _eventBus.PublishAsync(new MinisterAppointedEvent(minister, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Cabinet Secretariat",
            Subject = $"OFFICIAL CONFIRMATION: {minister.Name} Appointed as Minister of {portfolio}",
            Body = $"Executive decree confirmed. Minister {minister.Name} has assumed leadership of the Ministry of {portfolio}.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return minister;
    }

    public async Task<bool> DismissMinisterAsync(CabinetPortfolio portfolio, CancellationToken cancellationToken = default)
    {
        Minister? minister;
        lock (_lock)
        {
            minister = _ministers.FirstOrDefault(m => m.IsAppointed && m.Portfolio == portfolio);
            if (minister == null)
            {
                return false;
            }

            minister.IsAppointed = false;
        }

        _logger?.LogWarning($"Cabinet dismissal: Minister '{minister.Name}' dismissed from Portfolio [{portfolio}].");
        await _eventBus.PublishAsync(new MinisterDismissedEvent(minister.Id, portfolio, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task GenerateCabinetAdviceAsync(CancellationToken cancellationToken = default)
    {
        List<Minister> appointed;
        lock (_lock)
        {
            appointed = _ministers.Where(m => m.IsAppointed).ToList();
        }

        foreach (var minister in appointed)
        {
            var adviceSummary = minister.Portfolio switch
            {
                CabinetPortfolio.Finance => $"Fiscal outlook: Competence rating ({minister.CompetenceRating:0}%) suggests optimizing tax revenue pipelines.",
                CabinetPortfolio.Defense => $"National Security: Border readiness at operational efficiency based on defense readiness.",
                CabinetPortfolio.ForeignAffairs => $"Diplomatic Corp: International reputation benefit active.",
                _ => $"Ministry of {minister.Portfolio} operational baseline maintained."
            };

            await _eventBus.PublishAsync(new CabinetAdviceOfferedEvent(minister.Name, minister.Portfolio, adviceSummary, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        }
    }

    public int EvaluateMinisterIntrigues(ulong currentTick)
    {
        List<Minister> disloyal;
        lock (_lock)
        {
            disloyal = _ministers.Where(m => m.IsAppointed && m.LoyaltyRating < 40.0).ToList();
        }

        var uncoveredIntrigues = 0;
        foreach (var minister in disloyal)
        {
            uncoveredIntrigues++;
            _logger?.LogWarning($"CABINET INTRIGUE: Minister '{minister.Name}' (Loyalty: {minister.LoyaltyRating:0}%) uncovered leaking cabinet secrets!");

            _eventBus.PublishAsync(new MinisterIntrigueUncoveredEvent(minister.Id, minister.Name, "DocumentLeak", DateTimeOffset.UtcNow));

            _workspaceManager?.News.PublishArticle(new NewsArticle
            {
                Source = "Capital Sentinel",
                Headline = $"WHISTLEBLOWER SCOOP: CABINET LEAKS UNCOVERED IN MINISTRY OF {minister.Portfolio.ToString().ToUpperInvariant()}",
                Summary = $"Confidential memorandum leaked to press detailing internal executive disputes.",
                Category = "Politics",
                ImpactRating = 4
            });
        }

        return uncoveredIntrigues;
    }
}
