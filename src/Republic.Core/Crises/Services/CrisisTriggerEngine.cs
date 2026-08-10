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
/// Service implementation monitoring simulation indicators and broadcasting emergent crises.
/// </summary>
public sealed class CrisisTriggerEngine : ICrisisTriggerEngine
{
    private readonly IWorldManager _worldManager;
    private readonly IDecisionEngine _decisionEngine;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private ulong _lastEvaluationTick;

    public CrisisTriggerEngine(
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

    public int EvaluateSimulationMetrics(ulong currentTick)
    {
        if (currentTick == _lastEvaluationTick)
        {
            return 0;
        }

        _lastEvaluationTick = currentTick;
        var triggeredCrises = 0;

        var econ = _worldManager.Economic.GetIndicators();
        var demo = _worldManager.Demographics.GetDemographics();

        // 1. Fiscal Crisis Check
        if (econ.TreasuryBalance <= 0 || econ.InflationRate >= 0.15)
        {
            triggeredCrises++;
            _logger?.LogWarning($"CRISIS TRIGGERED: Fiscal Crisis detected at tick {currentTick} (Treasury: {econ.TreasuryBalance:C0})");
            _eventBus.PublishAsync(new FiscalCrisisEvent(econ.TreasuryBalance, econ.InflationRate, DateTimeOffset.UtcNow));

            _workspaceManager.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Finance Ministry",
                Subject = "URGENT: Impending Sovereign Default",
                Body = "Mr. President, national treasury reserves have dropped below critical operating thresholds.",
                Folder = "Inbox",
                ActionRequired = true,
            });

            _decisionEngine.RegisterDecision(new DecisionContext
            {
                Title = "Emergency Fiscal Stabilization",
                Description = "Select an immediate strategy to restore national solvency.",
                Category = "Economy",
                IsUrgent = true,
                Options = new List<DecisionOption>
                {
                    new DecisionOption
                    {
                        Label = "Emergency Reserve Drawdown",
                        Description = "Inject $100M into national accounts.",
                        Effects = new List<PolicyEffect>
                        {
                            new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 100_000_000 },
                            new PolicyEffect { TargetMetric = "Happiness", DeltaValue = -2.0 }
                        }
                    },
                    new DecisionOption
                    {
                        Label = "Austerity Spending Cuts",
                        Description = "Reduce national expenditures drastically.",
                        Effects = new List<PolicyEffect>
                        {
                            new PolicyEffect { TargetMetric = "Happiness", DeltaValue = -8.0 },
                            new PolicyEffect { TargetMetric = "Approval", DeltaValue = -12.0 }
                        }
                    }
                }
            });
        }

        // 2. Civil Unrest Check
        if (demo.HappinessRating < 35.0)
        {
            triggeredCrises++;
            _logger?.LogWarning($"CRISIS TRIGGERED: Civil Unrest detected at tick {currentTick} (Happiness: {demo.HappinessRating:0.0}%)");
            _eventBus.PublishAsync(new CivilUnrestEvent("Nationwide", 0, demo.HappinessRating, DateTimeOffset.UtcNow));

            _workspaceManager.News.PublishArticle(new NewsArticle
            {
                Source = "National Gazette",
                Headline = "MASS PROTESTS ERUPT ACROSS CAPITAL CITIES",
                Summary = "Demonstrators demand immediate executive action amidst surging public dissatisfaction.",
                Category = "Politics",
                ImpactRating = 5
            });

            _decisionEngine.RegisterDecision(new DecisionContext
            {
                Title = "Nationwide Civil Unrest",
                Description = "Widespread demonstrations threaten public order.",
                Category = "Domestic Security",
                IsUrgent = true,
                Options = new List<DecisionOption>
                {
                    new DecisionOption
                    {
                        Label = "Enact Civil Reform Package",
                        Description = "Grant immediate concessions and public aid.",
                        TreasuryCost = 50_000_000,
                        Effects = new List<PolicyEffect>
                        {
                            new PolicyEffect { TargetMetric = "Happiness", DeltaValue = 15.0 }
                        }
                    }
                }
            });
        }

        // 3. Coup Threat Check (Military faction approval < 25%)
        var militaryFaction = _worldManager.PoliticalCulture.GetFactions().FirstOrDefault(f => f.Name.Contains("Military", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Armed", StringComparison.OrdinalIgnoreCase));
        if (militaryFaction != null && militaryFaction.ApprovalRating < 25.0)
        {
            triggeredCrises++;
            _logger?.LogWarning($"CRISIS TRIGGERED: Coup Threat detected at tick {currentTick} (Military Approval: {militaryFaction.ApprovalRating:0.0}%)");
            _eventBus.PublishAsync(new CoupThreatTriggeredEvent("Nationwide", militaryFaction.ApprovalRating, DateTimeOffset.UtcNow));

            _workspaceManager.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Defense High Command",
                Subject = "ULTIMATUM: Military Council Demand",
                Body = "Mr. President, general staff officers demand an immediate expansion of the defense budget.",
                Folder = "Inbox",
                ActionRequired = true,
            });

            _decisionEngine.RegisterDecision(new DecisionContext
            {
                Title = "Military Coup Ultimatum",
                Description = "High command officers threaten to seize control of government facilities.",
                Category = "National Defense",
                IsUrgent = true,
                Options = new List<DecisionOption>
                {
                    new DecisionOption
                    {
                        Label = "Increase Defense Appropriations",
                        Description = "Approve $200M defense allocation.",
                        TreasuryCost = 200_000_000,
                        Effects = new List<PolicyEffect>
                        {
                            new PolicyEffect { TargetMetric = "Approval", TargetId = militaryFaction.Id, DeltaValue = 25.0 }
                        }
                    }
                }
            });
        }

        // 4. Armed Insurgency Check (Country baseline stability < 25%)
        var lowStabilityCountry = _worldManager.Countries.GetAllCountries().FirstOrDefault(c => c.BaselineStability < 25.0);
        if (lowStabilityCountry != null)
        {
            triggeredCrises++;
            _logger?.LogWarning($"CRISIS TRIGGERED: Armed Insurgency in '{lowStabilityCountry.Name}' (Stability: {lowStabilityCountry.BaselineStability:0.0})");
            _eventBus.PublishAsync(new InsurgencyBeganEvent(lowStabilityCountry.Id, CrisisSeverity.Catastrophic, DateTimeOffset.UtcNow));

            _workspaceManager.News.PublishArticle(new NewsArticle
            {
                Source = "Defense Network",
                Headline = "ARMED REBEL MILITIA SEIZES REGIONAL CAPITALS",
                Summary = "Insurgent forces have established control over provincial government buildings.",
                Category = "Security",
                ImpactRating = 5
            });
        }

        // 5. Environmental & Natural Disaster Check (Every 1000 ticks)
        if (currentTick > 0 && currentTick % 1000 == 0)
        {
            triggeredCrises++;
            _logger?.LogWarning($"CRISIS TRIGGERED: Natural Disaster at tick {currentTick}");
            _eventBus.PublishAsync(new NaturalDisasterOccurredEvent("Severe Earthquake", "Region-1", CrisisSeverity.Severe, DateTimeOffset.UtcNow));

            _workspaceManager.News.PublishArticle(new NewsArticle
            {
                Source = "Global Disaster Network",
                Headline = "MAJOR EARTHQUAKE STRIKES PROVINCIAL SECTOR",
                Summary = "Emergency response teams deployed amidst infrastructure damage and power outages.",
                Category = "Environment",
                ImpactRating = 4
            });

            _decisionEngine.RegisterDecision(new DecisionContext
            {
                Title = "Disaster Relief Allocation",
                Description = "A major natural disaster requires urgent emergency funding.",
                Category = "Disaster Relief",
                IsUrgent = true,
                Options = new List<DecisionOption>
                {
                    new DecisionOption
                    {
                        Label = "Deploy Federal Disaster Aid",
                        Description = "Allocate $75M in emergency reconstruction aid.",
                        TreasuryCost = 75_000_000,
                        Effects = new List<PolicyEffect>
                        {
                            new PolicyEffect { TargetMetric = "Happiness", DeltaValue = 8.0 }
                        }
                    }
                }
            });
        }

        return triggeredCrises;
    }
}
