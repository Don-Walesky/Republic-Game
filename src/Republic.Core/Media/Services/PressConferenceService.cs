namespace Republic.Core.Media.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Media.Models;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation managing executive press briefings, journalist questions, and media sentiment.
/// </summary>
public sealed class PressConferenceService : IPressConferenceService
{
    private readonly List<PressConferenceQuestion> _activeQuestions = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public PressConferenceService(
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

    public Task<PressConferenceQuestion> HostPressConferenceAsync(string topic, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        var q = new PressConferenceQuestion
        {
            JournalistName = "Sarah Jenkins",
            NewsOutlet = "National Tribune",
            Topic = topic,
            QuestionText = $"Mr. President, how does your administration justify current policy on {topic}?",
            Options = new List<PressResponseOption>
            {
                new PressResponseOption
                {
                    ResponseText = "We are taking bold, decisive executive action to protect our citizens.",
                    PublicApprovalDelta = 3.0,
                    MediaSentimentDelta = 2.0
                },
                new PressResponseOption
                {
                    ResponseText = "No comment at this time while negotiations continue.",
                    PublicApprovalDelta = -2.0,
                    MediaSentimentDelta = -4.0
                }
            }
        };

        lock (_lock)
        {
            _activeQuestions.Add(q);
        }

        _logger?.LogInfo($"Press Conference initiated on topic '{topic}'.");
        return Task.FromResult(q);
    }

    public Task<bool> AnswerQuestionAsync(string questionId, string optionId, CancellationToken cancellationToken = default)
    {
        PressConferenceQuestion? question;
        lock (_lock)
        {
            question = _activeQuestions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                return Task.FromResult(false);
            }

            _activeQuestions.Remove(question);
        }

        var option = question.Options.FirstOrDefault(o => o.Id == optionId) ?? question.Options[0];

        // Adjust demographic happiness
        var demo = _worldManager.Demographics.GetDemographics();
        _worldManager.Demographics.UpdateHappiness(Math.Clamp(demo.HappinessRating + option.PublicApprovalDelta, 0.0, 100.0));

        _logger?.LogInfo($"Press Question Answered: '{option.ResponseText}' (Happiness Delta: {option.PublicApprovalDelta:+#;-#;0})");

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = question.NewsOutlet,
            Headline = $"PRESS CONFERENCE BRIEFING: PRESIDENT RESPONDS ON {question.Topic.ToUpperInvariant()}",
            Summary = $"In response to press inquiries, executive statement issued: '{option.ResponseText}'",
            Category = "Press",
            ImpactRating = 4
        });

        return Task.FromResult(true);
    }
}
