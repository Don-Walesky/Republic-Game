namespace Republic.Core.Government.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Government.Events;
using Republic.Core.Government.Models;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation conducting constitutional conventions and governance overhauls.
/// </summary>
public sealed class GovernmentReformService : IGovernmentReformService
{
    private GovernmentType _currentSystem = GovernmentType.PresidentialRepublic;
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public GovernmentReformService(
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

    private readonly List<ConstitutionalAmendment> _amendments = new();

    public GovernmentType GetCurrentGovernmentSystem()
    {
        lock (_lock)
        {
            return _currentSystem;
        }
    }

    public Task<ConstitutionalAmendment> ProposeConstitutionalAmendmentAsync(ConstitutionalAmendment amendment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(amendment);

        lock (_lock)
        {
            amendment.Status = ConstitutionalAmendmentStatus.Proposed;
            _amendments.Add(amendment);
        }

        _logger?.LogInfo($"CONSTITUTIONAL AMENDMENT PROPOSED: '{amendment.Title}' (Required Supermajority: {amendment.SupermajorityRatioRequired * 100:F0}%).");

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Assembly Parliamentary Journal",
            Headline = $"AMENDMENT PROPOSED: {amendment.Title.ToUpperInvariant()}",
            Summary = amendment.Description,
            Category = "Constitution",
            ImpactRating = 3
        });

        return Task.FromResult(amendment);
    }

    public async Task<bool> VoteOnConstitutionalAmendmentAsync(string amendmentId, int votesInFavor, int totalVotesCast, CancellationToken cancellationToken = default)
    {
        ConstitutionalAmendment? amendment;
        lock (_lock)
        {
            amendment = _amendments.FirstOrDefault(a => a.Id == amendmentId);
            if (amendment == null || amendment.Status == ConstitutionalAmendmentStatus.Enacted)
            {
                return false;
            }

            amendment.VotesInFavor = votesInFavor;
            amendment.TotalVotesCast = totalVotesCast;
        }

        double approvalRatio = totalVotesCast > 0 ? (double)votesInFavor / totalVotesCast : 0.0;
        bool passed = approvalRatio >= amendment.SupermajorityRatioRequired;

        if (passed)
        {
            lock (_lock)
            {
                amendment.Status = ConstitutionalAmendmentStatus.Enacted;
                amendment.EnactedAt = DateTimeOffset.UtcNow;
            }

            _worldManager.PoliticalCulture.AmendConstitution(amendment.Title, _currentSystem.ToString(), new[] { amendment.TargetLawCategory });
            _logger?.LogInfo($"CONSTITUTIONAL AMENDMENT PASSED & ENACTED: '{amendment.Title}' ({approvalRatio * 100:F1}% in favor >= {amendment.SupermajorityRatioRequired * 100:F0}% threshold).");

            await _eventBus.PublishAsync(new ConstitutionalAmendmentVotedEvent(amendment, true, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            await _eventBus.PublishAsync(new ConstitutionalAmendmentEnactedEvent(amendment, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

            _workspaceManager?.News.PublishArticle(new NewsArticle
            {
                Source = "Constitutional Court Gazette",
                Headline = $"CONSTITUTIONAL AMENDMENT RATIFIED: {amendment.Title.ToUpperInvariant()}",
                Summary = $"With {approvalRatio * 100:F1}% of assembly votes, '{amendment.Title}' has been ratified into supreme law.",
                Category = "Constitution",
                ImpactRating = 5
            });
        }
        else
        {
            lock (_lock)
            {
                amendment.Status = ConstitutionalAmendmentStatus.Rejected;
            }

            _logger?.LogInfo($"CONSTITUTIONAL AMENDMENT REJECTED: '{amendment.Title}' ({approvalRatio * 100:F1}% in favor < {amendment.SupermajorityRatioRequired * 100:F0}% threshold).");
            await _eventBus.PublishAsync(new ConstitutionalAmendmentVotedEvent(amendment, false, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        }

        return passed;
    }

    public IReadOnlyList<ConstitutionalAmendment> GetConstitutionalAmendments()
    {
        lock (_lock)
        {
            return _amendments.ToList().AsReadOnly();
        }
    }

    public async Task<bool> EnactConstitutionalReformAsync(ConstitutionalReform reform, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reform);

        GovernmentType oldSystem;
        lock (_lock)
        {
            oldSystem = _currentSystem;
            _currentSystem = reform.TargetSystem;
        }

        // Apply constitutional amendments to World Political Culture
        _worldManager.PoliticalCulture.AmendConstitution(reform.Title, reform.TargetSystem.ToString(), new[] { "Executive Order Power", "Judicial Review" });

        _logger?.LogInfo($"CONSTITUTIONAL REFORM ENACTED: '{reform.Title}' transformed system from [{oldSystem}] to [{reform.TargetSystem}].");

        await _eventBus.PublishAsync(new ConstitutionalReformEnactedEvent(reform, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        await _eventBus.PublishAsync(new GovernmentSystemTransformedEvent(oldSystem, reform.TargetSystem, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Constitutional Assembly Gazette",
            Headline = $"HISTORIC REFORM: CONSTITUTION AMENDED TO {reform.TargetSystem.ToString().ToUpperInvariant()}",
            Summary = $"The national constitutional convention has formally ratified '{reform.Title}'.",
            Category = "Constitution",
            ImpactRating = 5
        });

        return true;
    }
}
