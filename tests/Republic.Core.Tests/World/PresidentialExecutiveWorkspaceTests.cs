namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Scenarios.Services;
using Xunit;

/// <summary>
/// Phase 10 verification test suite: First Playable Presidential Executive Workspace.
/// Validates the complete presidential decision loop: reviewing the country's condition,
/// initiating the National Power Plant project, confirmation/cancellation ergonomics,
/// single one-time treasury deduction, simulation-time progression, deterministic completion,
/// one-time effect application, idempotency, foreign country isolation, and persistence.
/// </summary>
public sealed class PresidentialExecutiveWorkspaceTests
{
    private readonly RepublicClock _clock;

    public PresidentialExecutiveWorkspaceTests()
    {
        _clock = RepublicClock.CreateControlled();
    }

    private Country CreateTestCountry(
        string name = "Republic of Arcadia",
        double startingBalance = 5_000_000.0,
        double energyCapacity = 50.0,
        string? id = null)
    {
        var countryId = id ?? name.ToLowerInvariant().Replace(" ", "-");
        var treasury = new RepuTreasury(startingBalance, countryId);
        var yield = new NationalYield
        {
            EnergyCapacity = energyCapacity,
            IndustrialCapacity = 60.0,
            AdministrativeCapacity = 60.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 60.0,
            ScienceCapacity = 60.0
        };

        var country = Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            id: countryId,
            treasury: treasury,
            yield: yield);

        return country;
    }

    private RepublicApplication CreateTestApplication(double playerTreasury = 5_000_000.0)
    {
        var bootstrapper = new ApplicationBootstrapper();
        var app = bootstrapper.Bootstrap();
        var scenarioService = new ScenarioBootstrapper(
            app.WorldManager,
            app.WorkspaceManager,
            app.CabinetService,
            app.LegislatureService,
            null,
            null,
            app.Clock);

        scenarioService.BootstrapScenarioAsync("arcadia-day1").GetAwaiter().GetResult();

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country");
        if (playerCountry != null)
        {
            if (playerCountry.Treasury.Balance < playerTreasury)
            {
                playerCountry.Treasury.Deposit(playerTreasury - playerCountry.Treasury.Balance);
            }
            else if (playerCountry.Treasury.Balance > playerTreasury)
            {
                playerCountry.Treasury.Withdraw(playerCountry.Treasury.Balance - playerTreasury);
            }
            playerCountry.Yield.EnergyCapacity = 50.0;
        }

        return app;
    }

    [Fact]
    public void Test1_ValidProjectStart_DeductsCostExactlyOnce_AndRecordsMetadata()
    {
        // Given a sovereign nation with R5,000,000 liquid treasury
        var country = CreateTestCountry(startingBalance: 5_000_000.0);
        var startTime = _clock.CurrentTime;

        // When initiating the National Power Plant
        var project = country.StartPowerPlantProject(startTime);

        // Then project is successfully created with correct parameters
        Assert.NotNull(project);
        Assert.Equal("National Power Plant", project.Name);
        Assert.Equal(DevelopmentProjectType.PowerPlant, project.ProjectType);
        Assert.Equal(NationalPowerPlantProject.StandardCost, project.Cost);
        Assert.Equal(1_000_000.0, project.Cost);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), project.FinishBoundary);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Same(project, country.ActivePowerPlantProject);

        // And exactly R1,000,000 is deducted from the sovereign treasury
        Assert.Equal(4_000_000.0, country.Treasury.Balance);
        Assert.Equal("R4M", country.Treasury.FormattedBalance);
    }

    [Fact]
    public void Test2_InsufficientFunds_RejectedWithoutPartialChanges()
    {
        // Given an underfunded sovereign nation with R500,000 (< R1,000,000)
        var country = CreateTestCountry(startingBalance: 500_000.0);
        var startTime = _clock.CurrentTime;

        // When attempting to initiate the National Power Plant
        var project = country.StartPowerPlantProject(startTime);

        // Then initiation is rejected
        Assert.Null(project);
        Assert.Null(country.ActivePowerPlantProject);
        Assert.Empty(country.Projects);

        // And treasury balance remains completely unchanged
        Assert.Equal(500_000.0, country.Treasury.Balance);
        Assert.Equal("R500K", country.Treasury.FormattedBalance);
    }

    [Fact]
    public void Test3_InitiatePowerPlantProject_CancellationBeforeConfirmation_LeavesStateUnchanged()
    {
        // Given an executive workspace session with player country
        var app = CreateTestApplication(playerTreasury: 3_000_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var initialBalance = playerCountry.Treasury.Balance;
        var startTime = app.TimeSystem.CurrentRepublicTime;

        // When the president decides to cancel before confirmation (confirmed: false)
        var outcome = PresidentialWorkspaceHandler.InitiatePowerPlantProject(playerCountry, startTime, confirmed: false);

        // Then action is cancelled with zero deductions and zero project state changes
        Assert.False(outcome.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Cancelled, outcome.Result);
        Assert.Contains("cancelled", outcome.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(initialBalance, playerCountry.Treasury.Balance);
        Assert.Null(playerCountry.ActivePowerPlantProject);
        Assert.Empty(playerCountry.Projects);
    }

    [Fact]
    public void Test4_InitiatePowerPlantProject_Confirmation_StartsProjectAndDeductsTreasury()
    {
        // Given an executive workspace session with R3,000,000
        var app = CreateTestApplication(playerTreasury: 3_000_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var startTime = app.TimeSystem.CurrentRepublicTime;

        // When the president confirms initiation (confirmed: true)
        var outcome = PresidentialWorkspaceHandler.InitiatePowerPlantProject(playerCountry, startTime, confirmed: true);

        // Then construction commences successfully
        Assert.True(outcome.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Success, outcome.Result);
        Assert.NotNull(outcome.Project);
        Assert.Equal(2_000_000.0, playerCountry.Treasury.Balance);
        Assert.NotNull(playerCountry.ActivePowerPlantProject);
        Assert.Single(playerCountry.Projects);
    }

    [Fact]
    public void Test5_Project_RemainsInProgress_BeforeCompletionBoundary()
    {
        // Given an active power plant started at Day 0, 00:00 WAT
        var country = CreateTestCountry(startingBalance: 2_000_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // When simulation time advances by 36 hours (halfway)
        _clock.AdvanceHours(36);
        var currentTime = _clock.CurrentTime;

        // Then progress is 50%, project is not completed, and cannot complete prematurely
        Assert.Equal(0.5, project.GetProgress(currentTime));
        Assert.False(project.CanComplete(currentTime));
        Assert.False(project.Completed);
        Assert.True(project.IsActive);

        // And attempting to complete returns 0 with no yield change
        var completedCount = country.CheckProjectsCompletion(currentTime);
        Assert.Equal(0, completedCount);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test6_Project_CompletesAtFinishBoundary_AndAppliesBenefitExactlyOnce()
    {
        // Given an active power plant started at Day 0, 00:00 WAT
        var country = CreateTestCountry(startingBalance: 2_000_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);

        // When simulation time advances to exactly 72 hours (finish boundary)
        _clock.AdvanceHours(72);
        var finishTime = _clock.CurrentTime;

        Assert.True(project.CanComplete(finishTime));

        // When completion check runs
        var completedCount = country.CheckProjectsCompletion(finishTime);

        // Then project completes and elevates Energy Capacity by +5.0
        Assert.Equal(1, completedCount);
        Assert.True(project.Completed);
        Assert.False(project.IsActive);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);

        // And funds are NOT deducted again on completion
        Assert.Equal(1_000_000.0, country.Treasury.Balance);

        // And repeated check produces 0 completions and does not duplicate benefit
        var secondCheck = country.CheckProjectsCompletion(finishTime);
        Assert.Equal(0, secondCheck);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void Test7_AdvanceTime_AdvancesSimulationClockAndCompletesProjectDeterministically()
    {
        // Given an application session with active power plant
        var app = CreateTestApplication(playerTreasury: 3_000_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var startTime = app.TimeSystem.CurrentRepublicTime;

        var startOutcome = PresidentialWorkspaceHandler.InitiatePowerPlantProject(playerCountry, startTime, confirmed: true);
        Assert.True(startOutcome.IsSuccess);
        Assert.NotNull(playerCountry.ActivePowerPlantProject);
        Assert.Equal(2_000_000.0, playerCountry.Treasury.Balance);

        // When simulation time is advanced by 72 hours (12 boundaries)
        var summary = PresidentialWorkspaceHandler.AdvanceTime(app.WorldManager, app.TimeSystem, TimeSpan.FromHours(72));

        // Then time advanced by 72 hours
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), summary.NewTime);
        Assert.Equal(app.TimeSystem.CurrentRepublicTime, summary.NewTime);

        // And project completed with report
        Assert.True(summary.ProjectsCompletedCount >= 1);
        Assert.Contains(summary.CompletedProjects, p => p.ProjectType == DevelopmentProjectType.PowerPlant);
        Assert.True(playerCountry.PowerPlantProject!.Completed);
        Assert.Null(playerCountry.ActivePowerPlantProject);
        Assert.Equal(55.0, playerCountry.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test8_RepeatedTimeAdvancement_DoesNotDuplicateCostOrBenefit()
    {
        // Given an application session where power plant has already completed
        var app = CreateTestApplication(playerTreasury: 3_000_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var startTime = app.TimeSystem.CurrentRepublicTime;

        PresidentialWorkspaceHandler.InitiatePowerPlantProject(playerCountry, startTime, confirmed: true);

        // Advance 72 hours to complete construction
        var summary1 = PresidentialWorkspaceHandler.AdvanceTime(app.WorldManager, app.TimeSystem, TimeSpan.FromHours(72));
        Assert.True(playerCountry.PowerPlantProject!.Completed);
        var energyAfterFirst = playerCountry.Yield.EnergyCapacity;
        Assert.Equal(55.0, energyAfterFirst);

        // When advancing time AGAIN by another 24 hours
        var summary2 = PresidentialWorkspaceHandler.AdvanceTime(app.WorldManager, app.TimeSystem, TimeSpan.FromHours(24));

        // Then Energy Capacity is NOT increased a second time
        Assert.Equal(energyAfterFirst, playerCountry.Yield.EnergyCapacity);

        // And no projects completed in the second advance
        Assert.Equal(0, summary2.ProjectsCompletedCount);

        // And Power Plant remains completed exactly once
        Assert.Single(playerCountry.Projects);
        Assert.True(playerCountry.PowerPlantProject.Completed);
    }

    [Fact]
    public void Test9_ForeignCountryIsolation_TreasuryAndCapacitiesUntouched()
    {
        // Given two sovereign countries: Arcadia and Norse Federation
        var arcadia = CreateTestCountry("Republic of Arcadia", startingBalance: 2_000_000.0, energyCapacity: 50.0, id: "arcadia");
        var norse = CreateTestCountry("Federation of Norse", startingBalance: 4_000_000.0, energyCapacity: 40.0, id: "norse");
        var startTime = _clock.CurrentTime;

        // When Arcadia starts and completes a power plant
        var arcadiaPlant = arcadia.StartPowerPlantProject(startTime);
        Assert.NotNull(arcadiaPlant);

        // Then Norse treasury and projects are unaffected
        Assert.Equal(1_000_000.0, arcadia.Treasury.Balance);
        Assert.Equal(4_000_000.0, norse.Treasury.Balance);
        Assert.Empty(norse.Projects);
        Assert.Null(norse.ActivePowerPlantProject);

        // Advance to 72h and complete for Arcadia
        _clock.AdvanceHours(72);
        arcadia.CheckProjectsCompletion(_clock.CurrentTime);

        // Arcadia received +5.0 Energy Capacity
        Assert.Equal(55.0, arcadia.Yield.EnergyCapacity);

        // Norse remains completely untouched
        Assert.Equal(40.0, norse.Yield.EnergyCapacity);
        Assert.Equal(4_000_000.0, norse.Treasury.Balance);
        Assert.Empty(norse.Projects);

        // Norse cannot complete Arcadia's project
        var invalidComplete = norse.CompleteProject(arcadiaPlant, _clock.CurrentTime);
        Assert.False(invalidComplete);
        Assert.Equal(40.0, norse.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test10_OfflineCatchUp_CompletesProjectDuringAbsence()
    {
        // Given an active power plant starting at Day 0, 00:00 WAT (finishes Day 3, 00:00 WAT)
        var country = CreateTestCountry("Offline Nation", startingBalance: 2_000_000.0, energyCapacity: 50.0);
        var startBoundary = _clock.FromDayAndTime(0, new TimeOnly(0, 0));
        var project = country.StartPowerPlantProject(startBoundary);
        Assert.NotNull(project);
        Assert.False(project.Completed);

        // When the president returns at Day 3, 06:00 WAT (past the finish boundary)
        var returnTime = _clock.FromDayAndTime(3, new TimeOnly(6, 0));
        var catchUpService = new OfflineYieldCatchUpService();
        var briefing = catchUpService.CatchUp(country, returnTime);

        // Then the power plant completes and is commissioned
        Assert.True(project.Completed);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);

        // And the executive briefing reports the completed project
        Assert.True(briefing.ProjectsCompletedCount >= 1);
    }

    [Fact]
    public async Task Test11_Persistence_ActiveAndCompletedProjectsSurviveSaveAndLoad()
    {
        // Given an application with an active project at 50% construction
        var app = CreateTestApplication(playerTreasury: 4_000_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var startTime = app.TimeSystem.CurrentRepublicTime;

        var project = playerCountry.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // Advance 36 hours (50%)
        if (app.Clock is IControlledRepublicClock controlledClock)
        {
            controlledClock.Advance(TimeSpan.FromHours(36));
        }

        Assert.Equal(0.5, project.GetProgress(app.TimeSystem.CurrentRepublicTime));

        // When saving the session to quicksave slot
        var slotName = "phase10_workspace_test";
        await app.SaveGameManager.SaveGameAsync(slotName);

        // And loading the session back
        var loadedState = await app.SaveGameManager.LoadGameAsync(slotName);
        Assert.NotNull(loadedState);

        var restoredCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        Assert.NotNull(restoredCountry);
        Assert.Single(restoredCountry.Projects);

        var restoredProject = Assert.IsType<NationalPowerPlantProject>(restoredCountry.Projects[0]);
        Assert.Equal(project.Id, restoredProject.Id);
        Assert.Equal(project.Cost, restoredProject.Cost);
        Assert.Equal(project.StartedBoundary, restoredProject.StartedBoundary);
        Assert.Equal(project.FinishBoundary, restoredProject.FinishBoundary);
        Assert.False(restoredProject.Completed);
        Assert.True(restoredProject.IsActive);

        // Advance remaining 36 hours to reach completion
        if (app.Clock is IControlledRepublicClock controlledClock2)
        {
            controlledClock2.Advance(TimeSpan.FromHours(36));
        }

        var completed = restoredCountry.CompletePowerPlantProject(app.TimeSystem.CurrentRepublicTime);
        Assert.True(completed);
        Assert.True(restoredProject.Completed);
    }

    [Fact]
    public void Test12_PresidentialDashboard_ReflectsAuthenticDomainState()
    {
        // Given a player country with treasury, founding date, stockpiles, and state capacities
        var app = CreateTestApplication(playerTreasury: 4_500_000.0);
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country")!;
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        // Ensure domestic stockpiles exist
        playerCountry.Inventory.Add("Iron", 1250.0);
        playerCountry.Inventory.Add("Petroleum", 800.0);

        // When evaluating the sovereign national dashboard
        var dashboard = playerCountry.GetDashboard(currentTime);

        // Then dashboard accurately reflects sovereign domain values
        Assert.Equal("Republic of Arcadia", dashboard.CountryName);
        Assert.Equal("player-country", dashboard.CountryId);
        Assert.Equal(4_500_000.0, dashboard.TreasuryBalance);
        Assert.Equal("R4.5M", dashboard.Treasury);
        Assert.Equal(3250.0, playerCountry.Inventory.GetQuantity("Iron"));
        Assert.Equal(1800.0, playerCountry.Inventory.GetQuantity("Petroleum"));

        // And unestablished fields clearly distinguish unavailable from zero
        Assert.Equal(NationalDashboard.NotEstablishedText, dashboard.Science);
        Assert.Equal(NationalDashboard.NotEstablishedText, dashboard.Military);
        Assert.Contains("Science: Not established", dashboard.Lines);
        Assert.Contains("Military: Not established", dashboard.Lines);
    }
}
