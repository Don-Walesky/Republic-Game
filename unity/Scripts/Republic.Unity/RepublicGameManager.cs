namespace Republic.Unity;

using System;
using System.Threading.Tasks;
using UnityEngine;
using Republic.App;
using Republic.Core.Configuration;
using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Models;
using Republic.Core.Decisions.Services;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Engine;
using Republic.Core.Events;
using Republic.Core.Persistence.Models;
using Republic.Core.Persistence.Services;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;
using Republic.Unity.Bridge;

/// <summary>
/// Main Unity MonoBehaviour entry point initializing the C# simulation engine and managing the main game loop.
/// </summary>
public sealed class RepublicGameManager : MonoBehaviour
{
    public static RepublicGameManager Instance { get; private set; } = null!;

    [Header("Engine Settings")]
    [SerializeField] private string worldName = "Republic of Arcadia";
    [SerializeField] private float frameRateTarget = 60f;

    public RepublicApplication Application { get; private set; } = null!;
    public RepublicEngine Engine { get; private set; } = null!;
    public IWorldManager WorldManager { get; private set; } = null!;
    public IWorkspaceManager WorkspaceManager { get; private set; } = null!;
    public ITaskQueueManager TaskQueueManager { get; private set; } = null!;
    public IDecisionEngine DecisionEngine { get; private set; } = null!;
    public ICrisisTriggerEngine CrisisTriggerEngine { get; private set; } = null!;
    public IInterPlayerWarfareService WarfareService { get; private set; } = null!;
    public IDiplomacyService DiplomacyService { get; private set; } = null!;
    public ISaveGameManager SaveGameManager { get; private set; } = null!;
    public ITimeSystem TimeSystem { get; private set; } = null!;
    public RepublicUnityBridge UnityBridge { get; private set; } = new();

    private bool _isInitialized;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeCoreEngineAsync();
    }

    private async Task InitializeCoreEngineAsync()
    {
        var bootstrapper = new ApplicationBootstrapper();
        Application = bootstrapper.Bootstrap();

        Engine = Application.Engine;
        WorldManager = Application.WorldManager;
        WorkspaceManager = Application.WorkspaceManager;
        TaskQueueManager = Application.TaskQueueManager;
        DecisionEngine = Application.DecisionEngine;
        TimeSystem = Application.TimeSystem;
        SaveGameManager = Application.SaveGameManager;

        await Engine.InitializeAsync();
        _isInitialized = true;
        Debug.Log("[Republic] Unity Game Manager initialized successfully.");
    }

    private async void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        // Advance 1 frame tick deterministically using real frame delta
        await Engine.RunAsync(1, TimeSpan.FromSeconds(Time.deltaTime));

        // Dispatch updated states to Unity UI Presenter Bridge
        UnityBridge.OnWorldStateUpdated(WorldManager.Current);
        UnityBridge.OnWorkspaceStateUpdated(WorkspaceManager.GetCurrentState());
        UnityBridge.OnEconomicIndicatorsUpdated(WorldManager.Economic.GetIndicators());
    }

    public async Task<bool> ExecuteDecisionAsync(string decisionId, string optionId)
    {
        return await DecisionEngine.ExecuteDecisionAsync(decisionId, optionId);
    }

    public async Task<string> QuickSaveGameAsync(string slotName = "Quicksave")
    {
        return await SaveGameManager.SaveGameAsync(slotName);
    }

    public async Task<FullGameState> QuickLoadGameAsync(string slotName = "Quicksave")
    {
        return await SaveGameManager.LoadGameAsync(slotName);
    }
}
