namespace Republic.Core.Crises.Services;

using Republic.Core.Crises.Events;
using Republic.Core.Crises.Models;
using Republic.Core.Decisions.Models;
using Republic.Core.Decisions.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation facilitating inter-player offensive actions and geopolitical warfare.
/// </summary>
public sealed class InterPlayerWarfareService : IInterPlayerWarfareService
{
    private readonly IWorldManager _worldManager;
    private readonly IDecisionEngine _decisionEngine;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;

    public InterPlayerWarfareService(
        IWorldManager worldManager,
        IDecisionEngine decisionEngine,
        IWorkspaceManager workspaceManager,
        IEventBus eventBus,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _decisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
        _workspaceManager = workspaceManager ?? throw new ArgumentNullException(nameof(workspaceManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public async Task<CrisisRecord> LaunchTradeEmbargoAsync(string attackerId, string targetId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attackerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var crisis = new CrisisRecord
        {
            Title = "Foreign Trade Embargo",
            Description = $"Rival sovereign state '{attackerId}' has declared a total commercial embargo against your exports.",
            Category = CrisisCategory.GeopoliticalAggression,
            Severity = CrisisSeverity.Severe,
            TargetEntityId = targetId,
            AttackerCountryId = attackerId,
        };

        // Impact target's trade indicators
        _worldManager.Economic.GetIndicators().TradeBalance -= 500_000_000.0;
        _logger?.LogWarning($"WARFARE: Trade embargo declared by '{attackerId}' against '{targetId}'");

        _workspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Ministry of Commerce",
            Subject = "CRITICAL: Foreign Trade Embargo Declared",
            Body = $"Rival player '{attackerId}' has imposed a strict maritime and land embargo on our key exports.",
            Folder = "Inbox",
            ActionRequired = true,
        });

        _workspaceManager.News.PublishArticle(new NewsArticle
        {
            Source = "Global Diplomat",
            Headline = "RIVAL STATE DECLARES HOSTILE TRADE EMBARGO",
            Summary = "Commercial channels severed as international tensions escalate dramatically.",
            Category = "International",
            ImpactRating = 5,
        });

        _decisionEngine.RegisterDecision(new DecisionContext
        {
            Title = "Countering Hostile Trade Embargo",
            Description = "Select an executive strategy to mitigate the embargo's economic damage.",
            Category = "Foreign Affairs",
            IsUrgent = true,
            Options = new List<DecisionOption>
            {
                new DecisionOption
                {
                    Label = "Enact Retaliatory Tariff Package",
                    Description = "Reciprocate sanctions against rival player.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 5.0 },
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = -50_000_000 }
                    }
                },
                new DecisionOption
                {
                    Label = "Seek WTO Diplomatic Arbitration",
                    Description = "Appeal to international courts for sanction relief.",
                    TreasuryCost = 10_000_000,
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Stability", TargetId = targetId, DeltaValue = 5.0 }
                    }
                }
            }
        });

        await _eventBus.PublishAsync(new InterPlayerAttackLaunchedEvent(attackerId, targetId, "TradeEmbargo", CrisisSeverity.Severe, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return crisis;
    }

    public async Task<CrisisRecord> LaunchCyberAttackAsync(string attackerId, string targetId, string targetSector, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attackerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var crisis = new CrisisRecord
        {
            Title = "State-Sponsored Cyber Sabotage",
            Description = $"Advanced persistent threats linked to '{attackerId}' have breached critical {targetSector} networks.",
            Category = CrisisCategory.Infrastructure,
            Severity = CrisisSeverity.Moderate,
            TargetEntityId = targetId,
            AttackerCountryId = attackerId,
        };

        // Deduct emergency response costs
        _worldManager.Economic.WithdrawTreasury(100_000_000.0);
        _logger?.LogWarning($"WARFARE: Cyber attack targeted '{targetSector}' in '{targetId}' launched by '{attackerId}'");

        _workspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Cyber Defense Agency",
            Subject = "SECURITY ALERT: Critical Infrastructure Breach",
            Body = $"State-sponsored malware origin traced to '{attackerId}'. Operating systems in {targetSector} sector compromised.",
            Folder = "Inbox",
            ActionRequired = true,
        });

        _decisionEngine.RegisterDecision(new DecisionContext
        {
            Title = "Cyber Security Breach Containment",
            Description = "Authorize defense protocol to isolate infected national servers.",
            Category = "Cyber Security",
            IsUrgent = true,
            Options = new List<DecisionOption>
            {
                new DecisionOption
                {
                    Label = "Purge & Harden Grid Infrastructure",
                    Description = "Deploy cyber warfare division to neutralize intrusion.",
                    TreasuryCost = 25_000_000,
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Stability", TargetId = targetId, DeltaValue = 2.0 }
                    }
                }
            }
        });

        await _eventBus.PublishAsync(new InterPlayerAttackLaunchedEvent(attackerId, targetId, "CyberAttack", CrisisSeverity.Moderate, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return crisis;
    }

    public async Task<CrisisRecord> FundSubversionAsync(string attackerId, string targetId, string targetFactionId, double fundingAmount, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attackerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var crisis = new CrisisRecord
        {
            Title = "Covert Political Subversion",
            Description = $"Foreign intelligence agents from '{attackerId}' have funneled illicit funds to domestic opposition factions.",
            Category = CrisisCategory.Political,
            Severity = CrisisSeverity.Severe,
            TargetEntityId = targetId,
            AttackerCountryId = attackerId,
        };

        // Boost target faction influence and drop stability
        var faction = _worldManager.PoliticalCulture.GetFaction(targetFactionId);
        if (faction != null)
        {
            _worldManager.PoliticalCulture.UpdateApproval(targetFactionId, Math.Min(100.0, faction.ApprovalRating + 15.0));
        }

        _worldManager.Countries.UpdateStability(targetId, -8.0);
        _logger?.LogWarning($"WARFARE: Foreign subversion funded in '{targetId}' targeting faction '{targetFactionId}'");

        _workspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "National Intelligence Bureau",
            Subject = "INTELLIGENCE BRIEF: Illicit Foreign Money Trail",
            Body = $"Covert funding from '{attackerId}' detected transferring into domestic political action committees.",
            Folder = "Inbox",
            ActionRequired = true,
        });

        await _eventBus.PublishAsync(new InterPlayerAttackLaunchedEvent(attackerId, targetId, "CovertSubversion", CrisisSeverity.Severe, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return crisis;
    }

    public async Task<CrisisRecord> DeployBorderSkirmishAsync(string attackerId, string targetRegionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attackerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetRegionId);

        var crisis = new CrisisRecord
        {
            Title = "Militarized Border Incursion",
            Description = $"Armed forces from '{attackerId}' have crossed sovereign territory into region '{targetRegionId}'.",
            Category = CrisisCategory.Security,
            Severity = CrisisSeverity.Catastrophic,
            TargetEntityId = targetRegionId,
            AttackerCountryId = attackerId,
        };

        _logger?.LogWarning($"WARFARE: Border skirmish deployed by '{attackerId}' into region '{targetRegionId}'");

        _workspaceManager.News.PublishArticle(new NewsArticle
        {
            Source = "Frontline Dispatch",
            Headline = "BREAKING: ARMED INCURSION REPORTED ON BORDER SECTOR",
            Summary = "Heavy artillery exchanges reported as hostile troops cross demarcation lines.",
            Category = "Defense",
            ImpactRating = 5,
        });

        _decisionEngine.RegisterDecision(new DecisionContext
        {
            Title = "Militarized Border Defense",
            Description = "Foreign forces are advancing across the border region.",
            Category = "Defense",
            IsUrgent = true,
            Options = new List<DecisionOption>
            {
                new DecisionOption
                {
                    Label = "Deploy Armed Forces Strike Group",
                    Description = "Counter-attack invading forces to reclaim sovereign territory.",
                    TreasuryCost = 150_000_000,
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Stability", DeltaValue = 10.0 },
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 15.0 }
                    }
                }
            }
        });

        await _eventBus.PublishAsync(new InterPlayerAttackLaunchedEvent(attackerId, targetRegionId, "BorderSkirmish", CrisisSeverity.Catastrophic, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return crisis;
    }
}
