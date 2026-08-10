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

    public GovernmentType GetCurrentGovernmentSystem()
    {
        lock (_lock)
        {
            return _currentSystem;
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
