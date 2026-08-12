namespace Republic.Core.Narrative.Services;

using Republic.Core.Decisions.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Narrative.Events;
using Republic.Core.Narrative.Models;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing narrative event libraries, branching choices, and storyline triggers.
/// </summary>
public sealed class NarrativeEngine : INarrativeEngine
{
    private readonly List<StoryEvent> _events = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public NarrativeEngine(
        IWorldManager worldManager,
        IEventBus eventBus,
        IWorkspaceManager? workspaceManager = null,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;

        SeedStorylineEvents();
    }

    private void SeedStorylineEvents()
    {
        _events.Add(new StoryEvent
        {
            Id = "story-oil-discovery",
            Title = "Offshore Energy Reserve Uncovered",
            NarrativeText = "Geological survey teams have confirmed a massive offshore natural gas field in sovereign waters.",
            Category = "Economy",
            PrerequisiteTick = 10,
            Choices = new List<StoryChoice>
            {
                new StoryChoice
                {
                    Text = "Nationalize gas extraction under state energy corporation.",
                    OutcomeDescription = "State revenues surge; private energy sector objects.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 500_000_000 },
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 10.0 }
                    },
                    FollowUpEventId = "story-assembly-debate"
                },
                new StoryChoice
                {
                    Text = "Auction extraction rights to international conglomerates.",
                    OutcomeDescription = "Immediate capital influx; public uproar over foreign control.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 1_200_000_000 },
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = -5.0 }
                    }
                }
            }
        });

        _events.Add(new StoryEvent
        {
            Id = "story-assembly-debate",
            Title = "National Assembly Emergency Debate",
            NarrativeText = "Lawmakers convene to pass environmental regulatory frameworks for offshore drilling.",
            Category = "Legislature",
            PrerequisiteTick = 0, // Triggered as follow-up
            Choices = new List<StoryChoice>
            {
                new StoryChoice
                {
                    Text = "Fast-track strict environmental protection standards.",
                    OutcomeDescription = "Protect maritime ecosystems; development delays expected.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 5.0 }
                    }
                },
                new StoryChoice
                {
                    Text = "Grant expedited drilling permits with tax incentives.",
                    OutcomeDescription = "Accelerated production schedule.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 150_000_000 }
                    }
                }
            }
        });

        _events.Add(new StoryEvent
        {
            Id = "story-whistleblower-leak",
            Title = "Intelligence Whistleblower Dossier Leaked",
            NarrativeText = "A high-ranking intelligence officer has leaked classified cables regarding unauthorized wiretapping.",
            Category = "Intelligence",
            PrerequisiteTick = 15,
            Choices = new List<StoryChoice>
            {
                new StoryChoice
                {
                    Text = "Order full public disclosure and independent commission inquiry.",
                    OutcomeDescription = "Restores public trust; destabilizes intelligence cabinet relations.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 15.0 }
                    }
                },
                new StoryChoice
                {
                    Text = "Invoke executive privilege and seal national security records.",
                    OutcomeDescription = "Protects intelligence operations; sparks parliamentary protest.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = -10.0 }
                    }
                }
            }
        });

        _events.Add(new StoryEvent
        {
            Id = "story-border-skirmish",
            Title = "Northern Border Skirmish Escalation",
            NarrativeText = "Unmarked foreign patrols exchanged fire with border border guards near Sector 4.",
            Category = "Military",
            PrerequisiteTick = 25,
            Choices = new List<StoryChoice>
            {
                new StoryChoice
                {
                    Text = "Deploy armored task force and elevate alert status to DEFCON 3.",
                    OutcomeDescription = "Deters further incursions; raises regional tension.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = -50_000_000 }
                    }
                },
                new StoryChoice
                {
                    Text = "Request immediate neutral diplomatic mediation via international summit.",
                    OutcomeDescription = "De-escalates border friction peacefully.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 5.0 }
                    }
                }
            }
        });
    }

    public IReadOnlyList<StoryEvent> GetActiveStoryEvents()
    {
        lock (_lock)
        {
            return _events.Where(e => e.IsTriggered && !e.IsResolved).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<StoryEvent> GetResolvedStoryEvents()
    {
        lock (_lock)
        {
            return _events.Where(e => e.IsResolved).ToList().AsReadOnly();
        }
    }

    public async Task EvaluateNarrativeTriggersAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        List<StoryEvent> newlyTriggered = new();
        lock (_lock)
        {
            foreach (var e in _events)
            {
                if (!e.IsTriggered && !e.IsResolved && e.PrerequisiteTick > 0 && currentTick >= e.PrerequisiteTick)
                {
                    e.IsTriggered = true;
                    newlyTriggered.Add(e);
                }
            }
        }

        foreach (var e in newlyTriggered)
        {
            _logger?.LogInfo($"NARRATIVE EVENT TRIGGERED: '{e.Title}'");
            await _eventBus.PublishAsync(new StoryEventTriggeredEvent(e, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

            _workspaceManager?.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Executive Secretariat",
                Subject = $"SPECIAL REPORT: {e.Title}",
                Body = e.NarrativeText,
                Folder = "Inbox",
                ActionRequired = true
            });
        }
    }

    public async Task<bool> MakeStoryChoiceAsync(string storyEventId, string choiceId, CancellationToken cancellationToken = default)
    {
        StoryEvent? storyEvent;
        StoryEvent? followUpEvent = null;

        lock (_lock)
        {
            storyEvent = _events.FirstOrDefault(e => e.Id == storyEventId);
            if (storyEvent == null || storyEvent.IsResolved)
            {
                return false;
            }

            storyEvent.IsResolved = true;
            var choice = storyEvent.Choices.FirstOrDefault(c => c.Id == choiceId) ?? storyEvent.Choices[0];

            if (!string.IsNullOrEmpty(choice.FollowUpEventId))
            {
                followUpEvent = _events.FirstOrDefault(e => e.Id == choice.FollowUpEventId);
                if (followUpEvent != null)
                {
                    followUpEvent.IsTriggered = true;
                }
            }
        }

        var chosenOption = storyEvent.Choices.FirstOrDefault(c => c.Id == choiceId) ?? storyEvent.Choices[0];

        foreach (var effect in chosenOption.Effects)
        {
            if (effect.TargetMetric == "Treasury")
            {
                _worldManager.Economic.DepositTreasury(effect.DeltaValue);
            }
        }

        _logger?.LogInfo($"NARRATIVE CHOICE MADE: '{chosenOption.Text}' in '{storyEvent.Title}'");
        await _eventBus.PublishAsync(new StoryChoiceMadeEvent(storyEvent, chosenOption, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        if (followUpEvent != null)
        {
            await _eventBus.PublishAsync(new StoryEventTriggeredEvent(followUpEvent, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            _workspaceManager?.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Executive Secretariat",
                Subject = $"FOLLOW-UP EVENT: {followUpEvent.Title}",
                Body = followUpEvent.NarrativeText,
                Folder = "Inbox",
                ActionRequired = true
            });
        }

        return true;
    }

    public NarrativeSnapshot GetNarrativeState()
    {
        lock (_lock)
        {
            return new NarrativeSnapshot
            {
                Events = _events.Select(e => new StoryEventSnapshot
                {
                    Id = e.Id,
                    IsTriggered = e.IsTriggered,
                    IsResolved = e.IsResolved
                }).ToList()
            };
        }
    }

    public void RestoreNarrativeState(NarrativeSnapshot snapshot)
    {
        if (snapshot == null || snapshot.Events == null)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var item in snapshot.Events)
            {
                var existing = _events.FirstOrDefault(e => e.Id == item.Id);
                if (existing != null)
                {
                    existing.IsTriggered = item.IsTriggered;
                    existing.IsResolved = item.IsResolved;
                }
            }
        }
    }
}
