namespace Republic.Core.Decisions.Services;

using Republic.Core.Decisions.Events;
using Republic.Core.Decisions.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World;

/// <summary>
/// Service implementation managing decision space, choice evaluation, and simulation metric updates.
/// </summary>
public sealed class DecisionEngine : IDecisionEngine
{
    private readonly List<DecisionContext> _pendingDecisions = new();
    private readonly IWorldManager _worldManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public DecisionEngine(IWorldManager worldManager, IEventBus eventBus, ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public void RegisterDecision(DecisionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        lock (_lock)
        {
            _pendingDecisions.Add(context);
        }

        _logger?.LogInfo($"Decision prompted [{context.Category}]: '{context.Title}' ({context.Options.Count} options)");
        _eventBus.PublishAsync(new DecisionPromptedEvent(context, DateTimeOffset.UtcNow));
    }

    public DecisionContext? GetDecision(string decisionId)
    {
        lock (_lock)
        {
            return _pendingDecisions.FirstOrDefault(d => d.Id == decisionId);
        }
    }

    public IReadOnlyList<DecisionContext> GetPendingDecisions()
    {
        lock (_lock)
        {
            return _pendingDecisions.ToList().AsReadOnly();
        }
    }

    public async Task<bool> ExecuteDecisionAsync(string decisionId, string optionId, CancellationToken cancellationToken = default)
    {
        DecisionContext? context;
        lock (_lock)
        {
            context = _pendingDecisions.FirstOrDefault(d => d.Id == decisionId);
            if (context == null)
            {
                return false;
            }

            _pendingDecisions.Remove(context);
        }

        var option = context.Options.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
        {
            return false;
        }

        // Apply treasury cost if specified
        if (option.TreasuryCost > 0)
        {
            _worldManager.Economic.WithdrawTreasury(option.TreasuryCost);
        }

        // Apply policy effects
        await ApplyPolicyEffectsAsync(option.Effects).ConfigureAwait(false);

        _logger?.LogInfo($"Executed decision '{context.Title}' - Chosen option: '{option.Label}'");
        await _eventBus.PublishAsync(new DecisionExecutedEvent(context.Id, option, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task DirectEnactPolicyAsync(string title, List<PolicyEffect> effects, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(effects);

        await ApplyPolicyEffectsAsync(effects).ConfigureAwait(false);
        _logger?.LogInfo($"Direct policy decree enacted: '{title}' ({effects.Count} effects applied)");
        await _eventBus.PublishAsync(new DecreeEnactedEvent(Guid.NewGuid().ToString("N"), title, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
    }

    private Task ApplyPolicyEffectsAsync(List<PolicyEffect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.TargetMetric.ToLowerInvariant())
            {
                case "stability":
                    if (!string.IsNullOrWhiteSpace(effect.TargetId))
                    {
                        _worldManager.Countries.UpdateStability(effect.TargetId, effect.DeltaValue);
                    }
                    break;

                case "treasury":
                    if (effect.DeltaValue > 0)
                    {
                        _worldManager.Economic.DepositTreasury(effect.DeltaValue);
                    }
                    else
                    {
                        _worldManager.Economic.WithdrawTreasury(Math.Abs(effect.DeltaValue));
                    }
                    break;

                case "happiness":
                    var currentHappiness = _worldManager.Demographics.GetDemographics().HappinessRating;
                    _worldManager.Demographics.UpdateHappiness(currentHappiness + effect.DeltaValue);
                    break;

                case "population":
                    var currentPop = _worldManager.Demographics.GetDemographics().TotalPopulation;
                    _worldManager.Demographics.UpdatePopulation((long)(currentPop + effect.DeltaValue));
                    break;

                case "approval":
                    if (!string.IsNullOrWhiteSpace(effect.TargetId))
                    {
                        var faction = _worldManager.PoliticalCulture.GetFaction(effect.TargetId);
                        if (faction != null)
                        {
                            _worldManager.PoliticalCulture.UpdateApproval(effect.TargetId, faction.ApprovalRating + effect.DeltaValue);
                        }
                    }
                    else
                    {
                        foreach (var faction in _worldManager.PoliticalCulture.GetFactions())
                        {
                            _worldManager.PoliticalCulture.UpdateApproval(faction.Id, faction.ApprovalRating + effect.DeltaValue);
                        }
                    }
                    break;
            }
        }

        return Task.CompletedTask;
    }
}
