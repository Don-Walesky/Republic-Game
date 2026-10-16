namespace Republic.Core.Tests.AI;

using Republic.Core.AI;
using Republic.Core.Configuration;
using Republic.Core.Economy.Treasury;
using Republic.Core.Events;
using Republic.Core.NationalYield;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Services;
using Republic.Core.Politics;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class AIGovernmentTests : IDisposable
{
    private readonly string _testSaveDir;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly TaskQueueManager _taskQueue;
    private readonly TimeSystem _time;
    private readonly SaveGameManager _saveManager;

    public AIGovernmentTests()
    {
        _testSaveDir = Path.Combine(Path.GetTempPath(), "RepublicAITest_" + Guid.NewGuid().ToString("N"));
        var config = new PersistenceConfiguration { SaveDirectory = _testSaveDir };
        var serializer = new JsonStateSerializer();
        var store = new FileSaveStore(config, serializer);

        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _time = new TimeSystem(new TimeSystemConfiguration(), _eventBus, _logger);
        _world = new WorldManager(_eventBus, _logger, _time.Clock);
        _world.CreateAsync("AI Governance Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _taskQueue = new TaskQueueManager(_eventBus, _logger);
        _saveManager = new SaveGameManager(store, config, _world, _workspace, _taskQueue, _time, narrativeEngine: null, logger: _logger);
    }

    [Fact]
    public async Task AICountry_AppliesTaxDecisionDuringYieldCycle_AndSurvivesSaveAndLoad()
    {
        // 1. Arrange: Setup Player and AI countries with rich domain state
        var epoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.FromHours(1));
        var t0 = RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0), epoch);
        var t6 = RepublicTime.FromDayAndTime(0, new TimeOnly(6, 0), epoch);

        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = 60.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 60.0,
            ScienceCapacity = 60.0,
            AdministrativeCapacity = 60.0,
            SecurityCapacity = 60.0
        };

        var playerCountry = Country.Found("Republic of Arcadia", t0, id: "player-arcadia", treasury: new RepuTreasury(50_000_000.0, "player-arcadia"), yield: yield, stateCapacity: StateCapacityInputs.All(1.0));
        var aiCountry = Country.Found("Republic of Valoria", t0, id: "ai-valoria", treasury: new RepuTreasury(5_000_000.0, "ai-valoria"), yield: yield, stateCapacity: StateCapacityInputs.All(1.0));
        aiCountry.IsAIControlled = true;

        // Establish parties
        aiCountry.FoundParty("Progressive Unity", t0, support: 60.0);
        aiCountry.FoundParty("Traditional Alliance", t0, support: 40.0);

        // Propose and pass a bill
        var bill = aiCountry.ProposeBill("Infrastructure Stimulus Bill");
        Assert.NotNull(bill);
        var passedBill = aiCountry.VoteBill(bill, t0);
        Assert.NotNull(passedBill);

        // Hold an election
        var election = aiCountry.HoldElection(t0);
        Assert.NotNull(election);

        // Open and recognize diplomatic relations
        playerCountry.OpenRelation(aiCountry.Id, t0);
        playerCountry.RecognizeRelation(aiCountry.Id, t0);
        aiCountry.OpenRelation(playerCountry.Id, t0);
        aiCountry.RecognizeRelation(playerCountry.Id, t0);

        _world.Countries.RegisterCountry(playerCountry);
        _world.Countries.RegisterCountry(aiCountry);

        // Register diplomatic message and trade proposal on world
        _world.Current.Messages.Add(new DiplomaticMessage("ai-valoria", "player-arcadia", "Greetings from Valoria", "00:00 WAT"));
        _world.Current.TradeProposals.Add(new TradeProposal("ai-valoria", "player-arcadia", 1_000_000.0, "Currency", "Pending") { Id = "trade-1" });

        // 2. Act: AI government processes a yield cycle at 06:00 WAT
        var aiGov = new AIGovernment { CountryId = aiCountry.Id };
        var cycleService = new NationalYieldCycleService();

        // Run yield cycle (deposits revenue to treasury)
        var yieldSnapshot = cycleService.ExecuteCycle(aiCountry, t6);
        aiCountry.LastCreditedBoundary = t6;
        Assert.NotNull(yieldSnapshot);

        // Run AI taxation decision
        var initialRate = aiCountry.TaxPolicy.IncomeTaxRate;
        var tempSave = Path.Combine(_testSaveDir, "temp_t1.sav");
        var tempState = Path.Combine(_testSaveDir, "temp_t1.json");
        var decisionResult = aiGov.ProcessYieldCycle(aiCountry, t6, simulationTick: 100, stateFilePath: tempState, saveFilePath: tempSave);

        // Assert domestic AI policy action took effect
        Assert.True(decisionResult.Success);
        Assert.NotEqual(initialRate, aiCountry.TaxPolicy.IncomeTaxRate);
        Assert.NotNull(aiCountry.LastAIDecision);
        Assert.Contains("Republic of Valoria set tax to", aiCountry.LastAIDecision);

        var preSaveTreasury = aiCountry.Treasury.Balance;
        var preSaveTaxRate = aiCountry.TaxPolicy.IncomeTaxRate;
        var preSaveApproval = aiCountry.HappinessRating;
        var preSaveAiDecision = aiCountry.LastAIDecision;

        // 3. Act: Save full game state to disk
        var slotName = "phase4_ai_offline_test";
        var savePath = await _saveManager.SaveGameAsync(slotName);
        Assert.True(File.Exists(savePath));

        // 4. Act: Load full game state from disk
        var loadedState = await _saveManager.LoadGameAsync(slotName);
        Assert.NotNull(loadedState);

        var restoredAiCountry = _world.Countries.GetCountry("ai-valoria");
        Assert.NotNull(restoredAiCountry);

        // 5. Assert: AI country state survived save and load completely
        Assert.True(restoredAiCountry.IsAIControlled);
        Assert.Equal(preSaveTaxRate, restoredAiCountry.TaxPolicy.IncomeTaxRate);
        Assert.Equal(preSaveTreasury, restoredAiCountry.Treasury.Balance);
        Assert.Equal(preSaveApproval, restoredAiCountry.HappinessRating);
        Assert.Equal(preSaveAiDecision, restoredAiCountry.LastAIDecision);

        // Assert yield state survived
        Assert.NotNull(restoredAiCountry.LatestYieldSnapshot);
        Assert.Equal(t6, restoredAiCountry.LastCreditedBoundary);

        // Assert parties survived
        Assert.NotNull(restoredAiCountry.RulingParty);
        Assert.NotNull(restoredAiCountry.OppositionParty);
        Assert.Equal("Progressive Unity", restoredAiCountry.RulingParty.Name);
        Assert.Equal("Traditional Alliance", restoredAiCountry.OppositionParty.Name);

        // Assert bills survived
        Assert.Single(restoredAiCountry.Bills);
        Assert.Equal("Infrastructure Stimulus Bill", restoredAiCountry.Bills[0].Title);
        Assert.Equal(CountryBillStatus.Passed, restoredAiCountry.Bills[0].Status);

        // Assert elections survived
        Assert.Single(restoredAiCountry.Elections);
        Assert.Equal("Progressive Unity", restoredAiCountry.LastElection?.WinnerPartyName);

        // Assert diplomatic relations survived
        Assert.Single(restoredAiCountry.Relations);
        Assert.Equal(DiplomaticRelationStatus.Recognized, restoredAiCountry.Relations[0].Status);

        // Assert world messages and trade proposals survived
        Assert.Single(loadedState.World.Messages);
        Assert.Equal("Greetings from Valoria", loadedState.World.Messages[0].Text);
        Assert.Single(loadedState.World.TradeProposals);
        Assert.Equal(1_000_000.0, loadedState.World.TradeProposals[0].Amount);
    }

    [Fact]
    public void AICountry_ProcessDueCycles_CatchesUpOfflineBoundariesUsingGlobalClock()
    {
        // Arrange
        var epoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.FromHours(1));
        var t0 = RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0), epoch);
        var t18 = RepublicTime.FromDayAndTime(0, new TimeOnly(18, 0), epoch);

        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = 60.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 60.0,
            ScienceCapacity = 60.0,
            AdministrativeCapacity = 60.0,
            SecurityCapacity = 60.0
        };

        var tempSave = Path.Combine(_testSaveDir, "temp_catchup.sav");
        var tempState = Path.Combine(_testSaveDir, "temp_catchup.json");

        var aiCountry = Country.Found(
            "Republic of Valoria",
            t0,
            id: "ai-valoria-catchup",
            treasury: new RepuTreasury(10_000_000.0, "ai-valoria-catchup"),
            yield: yield,
            stateCapacity: StateCapacityInputs.All(1.0));
        var aiGov = new AIGovernment { CountryId = aiCountry.Id };

        // Act: Offline progression catches up 3 boundaries (06:00, 12:00, 18:00)
        var results = aiGov.ProcessDueCycles(aiCountry, t18, stateFilePath: tempState, saveFilePath: tempSave);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal(t18, aiCountry.LastCreditedBoundary);
        Assert.NotNull(aiCountry.LastAIDecision);
        Assert.True(aiCountry.Treasury.Balance > 10_000_000.0);
    }

    [Fact]
    public async Task AICountry_CatchesUpMissedCycles_SurvivesSaveAndReloadAcrossNewSession()
    {
        // 1. Arrange: Setup initial state at t0
        var epoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.FromHours(1));
        var t0 = RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0), epoch);
        var t18 = RepublicTime.FromDayAndTime(0, new TimeOnly(18, 0), epoch);

        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = 60.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 60.0,
            ScienceCapacity = 60.0,
            AdministrativeCapacity = 60.0,
            SecurityCapacity = 60.0
        };

        var aiCountry = Country.Found("Republic of Valoria", t0, id: "ai-valoria-multi", treasury: new RepuTreasury(5_000_000.0, "ai-valoria-multi"), yield: yield, stateCapacity: StateCapacityInputs.All(1.0));
        aiCountry.IsAIControlled = true;
        _world.Countries.RegisterCountry(aiCountry);

        // 2. Act: Process multiple missed cycles (06:00, 12:00, 18:00)
        var aiGov = new AIGovernment { CountryId = aiCountry.Id };
        var tempSave = Path.Combine(_testSaveDir, "temp_multi.sav");
        var tempState = Path.Combine(_testSaveDir, "temp_multi.json");
        var results = aiGov.ProcessDueCycles(aiCountry, t18, stateFilePath: tempState, saveFilePath: tempSave);

        Assert.Equal(3, results.Count);
        Assert.Equal(t18, aiCountry.LastCreditedBoundary);

        var preSaveTreasury = aiCountry.Treasury.Balance;
        var preSaveTaxRate = aiCountry.TaxPolicy.IncomeTaxRate;
        var preSaveDecision = aiCountry.LastAIDecision;

        // 3. Act: Save world
        var slotName = "ai_multi_cycle_save";
        var savePath = await _saveManager.SaveGameAsync(slotName);
        Assert.True(File.Exists(savePath));

        // 4. Act: Load into fresh WorldManager
        var loaded = await _saveManager.LoadGameAsync(slotName);
        Assert.NotNull(loaded);

        var restored = _world.Countries.GetCountry("ai-valoria-multi");
        Assert.NotNull(restored);

        // 5. Assert: State matches across missed cycles
        Assert.True(restored.IsAIControlled);
        Assert.Equal(t18, restored.LastCreditedBoundary);
        Assert.Equal(preSaveTreasury, restored.Treasury.Balance);
        Assert.Equal(preSaveTaxRate, restored.TaxPolicy.IncomeTaxRate);
        Assert.Equal(preSaveDecision, restored.LastAIDecision);

        // 6. Assert: Next cycle at 18:00 produces no extra due cycles since already up to date
        var noDue = aiGov.ProcessDueCycles(restored, t18);
        Assert.Empty(noDue);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSaveDir))
        {
            try { Directory.Delete(_testSaveDir, true); } catch { }
        }
    }
}

