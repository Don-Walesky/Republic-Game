namespace Republic.Core.Elections.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Elections.Events;
using Republic.Core.Elections.Models;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation conducting democratic campaigns, polling updates, and election night results.
/// </summary>
public sealed class ElectionService : IElectionService
{
    private readonly PollingData _pollingData = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();
    private bool _isCampaignActive;

    public ElectionService(
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

    public bool IsCampaignActive
    {
        get
        {
            lock (_lock) return _isCampaignActive;
        }
    }

    public PollingData GetCurrentPollingData()
    {
        lock (_lock)
        {
            var demo = _worldManager.Demographics.GetDemographics();
            _pollingData.IncumbentApprovalPercentage = Math.Clamp(demo.HappinessRating + 5.0, 10.0, 90.0);
            _pollingData.OppositionApprovalPercentage = Math.Clamp(100.0 - _pollingData.IncumbentApprovalPercentage - 8.0, 10.0, 80.0);
            _pollingData.UndecidedVotersPercentage = 100.0 - _pollingData.IncumbentApprovalPercentage - _pollingData.OppositionApprovalPercentage;

            return new PollingData
            {
                IncumbentApprovalPercentage = _pollingData.IncumbentApprovalPercentage,
                OppositionApprovalPercentage = _pollingData.OppositionApprovalPercentage,
                UndecidedVotersPercentage = _pollingData.UndecidedVotersPercentage
            };
        }
    }

    public async Task StartCampaignSeasonAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _isCampaignActive = true;
        }

        _logger?.LogInfo("Campaign Season officially declared!");
        await _eventBus.PublishAsync(new CampaignSeasonBeganEvent(DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Electoral Commission",
            Headline = "CAMPAIGN SEASON OFFICIALLY BEGINS",
            Summary = "Candidates enter national debates ahead of upcoming general presidential election.",
            Category = "Elections",
            ImpactRating = 5
        });
    }

    public async Task<ElectionResult> ConductElectionAsync(string incumbentName, string candidateOppositionName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(incumbentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateOppositionName);

        PollingData polling;
        lock (_lock)
        {
            polling = GetCurrentPollingData();
            _isCampaignActive = false;
        }

        var totalElectorate = (long)(_worldManager.Demographics.GetDemographics().TotalPopulation * 0.65);
        var turnoutPct = 72.5;
        var actualVoters = (long)(totalElectorate * (turnoutPct / 100.0));

        var incumbentVotes = (long)(actualVoters * (polling.IncumbentApprovalPercentage / 100.0));
        var oppositionVotes = actualVoters - incumbentVotes;

        var isReelected = incumbentVotes >= oppositionVotes;
        var winner = isReelected ? incumbentName : candidateOppositionName;

        var result = new ElectionResult
        {
            IncumbentVotes = incumbentVotes,
            OppositionVotes = oppositionVotes,
            TotalTurnoutPercentage = turnoutPct,
            IsIncumbentReelected = isReelected,
            WinnerName = winner
        };

        _logger?.LogInfo($"ELECTION NIGHT: Winner: '{winner}' (Incumbent Reelected: {isReelected})");
        await _eventBus.PublishAsync(new ElectionConductedEvent(result, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        await _eventBus.PublishAsync(new PresidentialTransitionEvent(winner, isReelected, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Elections Network",
            Headline = $"ELECTION NIGHT VICTORY: {winner.ToUpperInvariant()} ELECTED PRESIDENT!",
            Summary = $"Voter turnout reached {turnoutPct:0.0}%. {winner} has secured a decisive electoral mandate.",
            Category = "Elections",
            ImpactRating = 5
        });

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Electoral Commission",
            Subject = $"OFFICIAL CERTIFICATION: Election Night Results",
            Body = $"The national electoral result is certified. Winner: {winner}. Incumbent votes: {incumbentVotes:N0}, Opposition votes: {oppositionVotes:N0}.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return result;
    }
}
