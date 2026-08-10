namespace Republic.App;

using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Engine;
using Republic.Core.Tasks.Models;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Top-level application facade used by Program.cs.
/// </summary>
public sealed class RepublicApplication
{
    private readonly RepublicConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly RepublicEngine _engine;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ITaskQueueManager _taskQueueManager;
    private readonly ITimeSystem _timeSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicApplication"/> class.
    /// </summary>
    public RepublicApplication(
        RepublicConfiguration configuration,
        ILogger logger,
        RepublicEngine engine,
        IWorkspaceManager workspaceManager,
        ITaskQueueManager taskQueueManager,
        ITimeSystem timeSystem)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _workspaceManager = workspaceManager ?? throw new ArgumentNullException(nameof(workspaceManager));
        _taskQueueManager = taskQueueManager ?? throw new ArgumentNullException(nameof(taskQueueManager));
        _timeSystem = timeSystem ?? throw new ArgumentNullException(nameof(timeSystem));
    }

    /// <summary>
    /// Executes the repository's bootstrap, workspace, and task pipeline path.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Republic bootstrap starting.");
        await _engine.InitializeAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInfo($"Simulation Epoch Date: {_timeSystem.CurrentSimulatedDateTime:yyyy-MM-dd HH:mm:ss} UTC");

        // Executive Workspace simulation pass
        _workspaceManager.UpdateRoomState(roomName: "Executive Office", lightingMode: "Day", audioZone: "DeskAmbience");

        _workspaceManager.Visitors.RegisterVisitor(new Visitor
        {
            Name = "Minister Alexander Vance",
            Title = "Minister of Finance",
            Faction = "Reform Coalition",
            Purpose = "Emergency Budget Briefing"
        });

        _workspaceManager.Phone.ReceiveCall(new PhoneCall
        {
            CallerName = "Ambassador Elena Rostova",
            Organization = "Foreign Relations Ministry",
            Urgency = CallUrgency.High,
            Subject = "Border Tariff Negotiations"
        });

        _workspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "chief.of.staff@republic.gov",
            Recipient = "executive@republic.gov",
            Subject = "Daily Executive Intelligence Summary",
            Body = "Legislative quorum confirmed for 14:00 session.",
            ActionRequired = true
        });

        _workspaceManager.News.PublishArticle(new NewsArticle
        {
            Source = "Republic National Press",
            Headline = "Quarterly GDP Growth Exceeds Estimates",
            Summary = "Economic indicators show robust expansion across industrial sectors.",
            Category = "Economy",
            ImpactRating = 4
        });

        // Queue a long-running construction action (Build Central National University)
        var constructionTask = _taskQueueManager.QueueTask(
            title: "Build Central National University",
            category: TaskCategory.Construction,
            targetEntityId: "UNI_CAPITAL_01",
            durationTicks: 10,
            timeSystem: _timeSystem);

        await _engine.RunAsync(
            _configuration.Engine.StartupFrameCount,
            TimeSpan.FromMilliseconds(_configuration.Engine.FrameDeltaMilliseconds),
            cancellationToken).ConfigureAwait(false);

        // Process tasks for remaining ticks
        await _taskQueueManager.ProcessTickAsync(_timeSystem.CurrentTick, cancellationToken).ConfigureAwait(false);

        var state = _workspaceManager.GetCurrentState();
        _logger.LogInfo($"Republic Executive Workspace active: {state.Visitors.Count} Visitor(s), {state.PhoneCalls.Count} Call(s), {state.Emails.Count} Email(s), {state.NewsArticles.Count} News item(s).");
        _logger.LogInfo($"Scheduled Task Status: '{constructionTask.Title}' -> {constructionTask.Status} ({constructionTask.ProgressPercentage:0.0}%)");
        _logger.LogInfo("Republic bootstrap completed.");
    }
}
