namespace Republic.App;

using Republic.Core.AI.Services;
using Republic.Core.Cabinet.Services;
using Republic.Core.Configuration;
using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Elections.Services;
using Republic.Core.Engine;
using Republic.Core.Intelligence.Services;
using Republic.Core.Legislature.Services;
using Republic.Core.Persistence.Services;
using Republic.Core.Tasks.Models;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Top-level application facade providing access to all runtime simulation services.
/// </summary>
public sealed class RepublicApplication
{
    public RepublicConfiguration Configuration { get; }
    public ILogger Logger { get; }
    public RepublicEngine Engine { get; }
    public IWorldManager WorldManager { get; }
    public IWorkspaceManager WorkspaceManager { get; }
    public ITaskQueueManager TaskQueueManager { get; }
    public ITimeSystem TimeSystem { get; }
    public IDecisionEngine DecisionEngine { get; }
    public ICrisisTriggerEngine CrisisTriggerEngine { get; }
    public IInterPlayerWarfareService WarfareService { get; }
    public IDiplomacyService DiplomacyService { get; }
    public ICabinetService CabinetService { get; }
    public IIntelligenceService IntelligenceService { get; }
    public ILegislatureService LegislatureService { get; }
    public IBudgetService BudgetService { get; }
    public IElectionService ElectionService { get; }
    public IRivalAIService RivalAIService { get; }
    public ISaveGameManager SaveGameManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicApplication"/> class.
    /// </summary>
    public RepublicApplication(
        RepublicConfiguration configuration,
        ILogger logger,
        RepublicEngine engine,
        IWorldManager worldManager,
        IWorkspaceManager workspaceManager,
        ITaskQueueManager taskQueueManager,
        ITimeSystem timeSystem,
        IDecisionEngine decisionEngine,
        ICrisisTriggerEngine crisisTriggerEngine,
        IInterPlayerWarfareService warfareService,
        IDiplomacyService diplomacyService,
        ICabinetService cabinetService,
        IIntelligenceService intelligenceService,
        ILegislatureService legislatureService,
        IBudgetService budgetService,
        IElectionService electionService,
        IRivalAIService rivalAIService,
        ISaveGameManager saveGameManager)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        WorldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        WorkspaceManager = workspaceManager ?? throw new ArgumentNullException(nameof(workspaceManager));
        TaskQueueManager = taskQueueManager ?? throw new ArgumentNullException(nameof(taskQueueManager));
        TimeSystem = timeSystem ?? throw new ArgumentNullException(nameof(timeSystem));
        DecisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
        CrisisTriggerEngine = crisisTriggerEngine ?? throw new ArgumentNullException(nameof(crisisTriggerEngine));
        WarfareService = warfareService ?? throw new ArgumentNullException(nameof(warfareService));
        DiplomacyService = diplomacyService ?? throw new ArgumentNullException(nameof(diplomacyService));
        CabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        IntelligenceService = intelligenceService ?? throw new ArgumentNullException(nameof(intelligenceService));
        LegislatureService = legislatureService ?? throw new ArgumentNullException(nameof(legislatureService));
        BudgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
        ElectionService = electionService ?? throw new ArgumentNullException(nameof(electionService));
        RivalAIService = rivalAIService ?? throw new ArgumentNullException(nameof(rivalAIService));
        SaveGameManager = saveGameManager ?? throw new ArgumentNullException(nameof(saveGameManager));
    }

    /// <summary>
    /// Executes the repository's bootstrap, workspace, and task pipeline path.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInfo("Republic bootstrap starting.");
        await Engine.InitializeAsync(cancellationToken).ConfigureAwait(false);

        Logger.LogInfo($"Simulation Epoch Date: {TimeSystem.CurrentSimulatedDateTime:yyyy-MM-dd HH:mm:ss} UTC");

        WorkspaceManager.UpdateRoomState(roomName: "Executive Office", lightingMode: "Day", audioZone: "DeskAmbience");

        WorkspaceManager.Visitors.RegisterVisitor(new Visitor
        {
            Name = "Minister Alexander Vance",
            Title = "Minister of Finance",
            Faction = "Reform Coalition",
            Purpose = "Emergency Budget Briefing"
        });

        WorkspaceManager.Phone.ReceiveCall(new PhoneCall
        {
            CallerName = "Ambassador Elena Rostova",
            Organization = "Foreign Relations Ministry",
            Urgency = CallUrgency.High,
            Subject = "Border Tariff Negotiations"
        });

        WorkspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "chief.of.staff@republic.gov",
            Recipient = "executive@republic.gov",
            Subject = "Daily Executive Intelligence Summary",
            Body = "Legislative quorum confirmed for 14:00 session.",
            ActionRequired = true
        });

        WorkspaceManager.News.PublishArticle(new NewsArticle
        {
            Source = "Republic National Press",
            Headline = "Quarterly GDP Growth Exceeds Estimates",
            Summary = "Economic indicators show robust expansion across industrial sectors.",
            Category = "Economy",
            ImpactRating = 4
        });

        var constructionTask = TaskQueueManager.QueueTask(
            title: "Build Central National University",
            category: TaskCategory.Construction,
            targetEntityId: "UNI_CAPITAL_01",
            durationTicks: 10,
            timeSystem: TimeSystem);

        await Engine.RunAsync(
            Configuration.Engine.StartupFrameCount,
            TimeSpan.FromMilliseconds(Configuration.Engine.FrameDeltaMilliseconds),
            cancellationToken).ConfigureAwait(false);

        await TaskQueueManager.ProcessTickAsync(TimeSystem.CurrentTick, cancellationToken).ConfigureAwait(false);

        var state = WorkspaceManager.GetCurrentState();
        Logger.LogInfo($"Republic Executive Workspace active: {state.Visitors.Count} Visitor(s), {state.PhoneCalls.Count} Call(s), {state.Emails.Count} Email(s), {state.NewsArticles.Count} News item(s).");
        Logger.LogInfo($"Scheduled Task Status: '{constructionTask.Title}' -> {constructionTask.Status} ({constructionTask.ProgressPercentage:0.0}%)");
        Logger.LogInfo("Republic bootstrap completed.");
    }
}
