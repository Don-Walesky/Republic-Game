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
            Choices = new List<StoryChoice>
            {
                new StoryChoice
                {
                    Text = "Nationalize gas extraction under state energy corporation.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 500_000_000 },
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = 10.0 }
                    }
                },
                new StoryChoice
                {
                    Text = "Auction extraction rights to international conglomerates.",
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 1_200_000_000 },
                        new PolicyEffect { TargetMetric = "Approval", DeltaValue = -5.0 }
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

    public async Task EvaluateNarrativeTriggersAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        if (currentTick == 10)
        {
            StoryEvent? e;
            lock (_lock)
            {
                e = _events.FirstOrDefault(x => x.Id == "story-oil-discovery");
                if (e != null && !e.IsTriggered)
                {
                    e.IsTriggered = true;
                }
            }

            if (e != null)
            {
                _logger?.LogInfo($"NARRATIVE EVENT TRIGGERED: '{e.Title}'");
                await _eventBus.PublishAsync(new StoryEventTriggeredEvent(e, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

                _workspaceManager?.Email.ReceiveEmail(new EmailMessage
                {
                    Sender = "Ministry of Energy",
                    Subject = $"SPECIAL REPORT: {e.Title}",
                    Body = e.NarrativeText,
                    Folder = "Inbox",
                    ActionRequired = true
                });
            }
        }
    }

    public async Task<bool> MakeStoryChoiceAsync(string storyEventId, string choiceId, CancellationToken cancellationToken = default)
    {
        StoryEvent? storyEvent;
        lock (_lock)
        {
            storyEvent = _events.FirstOrDefault(e => e.Id == storyEventId);
            if (storyEvent == null || storyEvent.IsResolved)
            {
                return false;
            }

            storyEvent.IsResolved = true;
        }

        var choice = storyEvent.Choices.FirstOrDefault(c => c.Id == choiceId) ?? storyEvent.Choices[0];

        foreach (var effect in choice.Effects)
        {
            if (effect.TargetMetric == "Treasury")
            {
                _worldManager.Economic.DepositTreasury(effect.DeltaValue);
            }
        }

        _logger?.LogInfo($"NARRATIVE CHOICE MADE: '{choice.Text}' in '{storyEvent.Title}'");
        await _eventBus.PublishAsync(new StoryChoiceMadeEvent(storyEvent, choice, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }
}
