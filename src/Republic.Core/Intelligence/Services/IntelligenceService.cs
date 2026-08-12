namespace Republic.Core.Intelligence.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Intelligence.Events;
using Republic.Core.Intelligence.Models;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing foreign spy networks, covert operations, and counter-intelligence.
/// </summary>
public sealed class IntelligenceService : IIntelligenceService
{
    private readonly List<SpyNetwork> _networks = new();
    private readonly List<CovertOperation> _operations = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public IntelligenceService(
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

    public SpyNetwork GetOrCreateNetwork(string targetCountryId)
    {
        lock (_lock)
        {
            var network = _networks.FirstOrDefault(n => n.TargetCountryId == targetCountryId);
            if (network == null)
            {
                network = new SpyNetwork { TargetCountryId = targetCountryId };
                _networks.Add(network);
            }
            return network;
        }
    }

    public async Task<SpyNetwork> InfiltrateTargetAsync(string targetCountryId, int additionalAgents = 2, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountryId);

        SpyNetwork network;
        lock (_lock)
        {
            network = GetOrCreateNetwork(targetCountryId);
            network.AssignedAgentsCount += additionalAgents;
            network.InfiltrationLevel = Math.Min(100.0, network.InfiltrationLevel + (additionalAgents * 10.0));
        }

        _logger?.LogInfo($"Intelligence network expanded in '{targetCountryId}' (Infiltration: {network.InfiltrationLevel:0}%)");
        await _eventBus.PublishAsync(new SpyNetworkEstablishedEvent(network, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return network;
    }

    public async Task<CovertOperation> LaunchOperationAsync(CovertOperationType type, string targetCountryId, string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var op = new CovertOperation
        {
            Title = title,
            Type = type,
            TargetCountryId = targetCountryId,
            FinancialCost = 50_000_000.0,
            RiskRating = 0.3,
            IsCompleted = false,
            IsExposed = false
        };

        // Withdraw treasury cost
        _worldManager.Economic.WithdrawTreasury(op.FinancialCost);

        lock (_lock)
        {
            _operations.Add(op);
        }

        _logger?.LogInfo($"Covert Operation launched [{type}]: '{title}' targeting '{targetCountryId}'");
        await _eventBus.PublishAsync(new OperationLaunchedEvent(op, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        // Execute operation outcome based on infiltration
        var network = GetOrCreateNetwork(targetCountryId);
        if (network.InfiltrationLevel >= 40.0)
        {
            op.IsCompleted = true;
            _logger?.LogInfo($"Covert Operation SUCCESS: '{title}' completed cleanly.");
            await _eventBus.PublishAsync(new OperationSucceededEvent(op, "Objective achieved without detection.", DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

            _workspaceManager?.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Director of Central Intelligence",
                Subject = $"CLASSIFIED: Operation '{title}' Successful",
                Body = $"Agents in '{targetCountryId}' report complete success for operation {type}.",
                Folder = "Inbox",
                ActionRequired = false
            });
        }
        else
        {
            op.IsExposed = true;
            _logger?.LogWarning($"Covert Operation EXPOSED: '{title}' compromised in '{targetCountryId}'!");
            await _eventBus.PublishAsync(new OperationExposedEvent(op, "Agent captured in target territory.", DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

            _workspaceManager?.News.PublishArticle(new NewsArticle
            {
                Source = "Global Intelligence Monitor",
                Headline = $"DIPLOMATIC INCIDENT: FOREIGN SPY CELL UNCOVERED IN '{targetCountryId.ToUpperInvariant()}'",
                Summary = "Hostile intelligence agents apprehended carrying sensitive state clearance.",
                Category = "Espionage",
                ImpactRating = 5
            });
        }

        return op;
    }

    public IReadOnlyList<CovertOperation> GetActiveOperations()
    {
        lock (_lock)
        {
            return _operations.Where(o => !o.IsCompleted && !o.IsExposed).ToList().AsReadOnly();
        }
    }

    public Task<bool> ConductCounterEspionageSweepAsync(CancellationToken cancellationToken = default)
    {
        _worldManager.Economic.WithdrawTreasury(15_000_000.0);
        _logger?.LogInfo("COUNTER-ESPIONAGE SWEEP: National wiretap and signals intelligence sweep executed.");

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Ministry of Security Gazette",
            Headline = "NATIONAL SECURITY BULLETIN: COUNTER-INTELLIGENCE SURVEILLANCE EXPANDED",
            Summary = "Domestic wiretap sweeps and counter-surveillance protocols heightened across state ministries.",
            Category = "Security",
            ImpactRating = 3
        });

        return Task.FromResult(true);
    }
}
