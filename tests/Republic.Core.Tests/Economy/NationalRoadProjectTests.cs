namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalRoadProjectTests
{
    private readonly RepublicClock _clock;
    private readonly OfflineYieldCatchUpService _catchUpService;

    public NationalRoadProjectTests()
    {
        _clock = RepublicClock.CreateControlled();
        _catchUpService = new OfflineYieldCatchUpService();
    }

    private Country CreateTestCountry(string name, double startingBalance, double infrastructureCondition = 0.6, string? id = null)
    {
        var capacity = new StateCapacityInputs
        {
            GovernmentEffectiveness = 0.8,
            AdministrativeCompetence = 0.8,
            InstitutionQuality = 0.8,
            CorruptionControl = 0.8,
            Stability = 0.8,
            InfrastructureCondition = infrastructureCondition,
            CivilServiceQuality = 0.8
        };

        var countryId = id ?? name.ToLowerInvariant().Replace(" ", "-");
        var treasury = new RepuTreasury(startingBalance, countryId);
        return Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            id: countryId,
            treasury: treasury,
            stateCapacity: capacity);
    }

    [Fact]
    public void Test1_ProjectStartsSuccessfully_WhenTreasuryCanPay()
    {
        // Given a sovereign country with R1,000,000 in treasury
        var startingBalance = 1_000_000.0;
        var country = CreateTestCountry("Funded Republic", startingBalance);
        var startTime = _clock.CurrentTime;

        // When starting the National Road Program
        var project = country.StartRoadProject(startTime);

        // Then project starts successfully and records all metadata
        Assert.NotNull(project);
        Assert.Same(project, country.RoadProject);
        Assert.Equal(country.Id, project.CountryId);
        Assert.Equal(DevelopmentProjectType.Road, project.ProjectType);
        Assert.Equal("National Road Program", project.Name);
        Assert.Equal(500_000.0, project.Cost);
        Assert.False(project.Completed);
        Assert.False(project.IsCompleted);
        Assert.True(project.IsActive);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime, project.StartedTime);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(24)), project.FinishBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(24)), project.FinishTime);
        Assert.Contains(project, country.Projects);
        Assert.Same(project, country.ActiveProject);
    }

    [Fact]
    public void Test2_TreasuryIsChargedExactlyOnce()
    {
        // Given a sovereign country with R1,000,000 in treasury
        var startingBalance = 1_000_000.0;
        var country = CreateTestCountry("Treasury Republic", startingBalance);
        var startTime = _clock.CurrentTime;

        // When starting the National Road Program
        var project = country.StartRoadProject(startTime);

        // Then treasury is charged exactly R500,000 once
        Assert.NotNull(project);
        Assert.Equal(500_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void Test3_ProjectCannotStart_WithInsufficientFunds()
    {
        // Given a sovereign country with R400,000 (less than required R500,000)
        var startingBalance = 400_000.0;
        var country = CreateTestCountry("Underfunded Republic", startingBalance);
        var startTime = _clock.CurrentTime;

        // When attempting to start the National Road Program
        var project = country.StartRoadProject(startTime);

        // Then project does not start and balance remains strictly unchanged
        Assert.Null(project);
        Assert.Null(country.RoadProject);
        Assert.Null(country.ActiveRoadProject);
        Assert.Empty(country.Projects);
        Assert.Equal(startingBalance, country.Treasury.Balance);
    }

    [Fact]
    public void Test4_ProjectCannotBeStartedTwice_WhileActive()
    {
        // Given a country that already has an active road program under construction
        var country = CreateTestCountry("Active Republic", 2_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var firstProject = country.StartRoadProject(startBoundary);
        Assert.NotNull(firstProject);
        Assert.Equal(1_500_000.0, country.Treasury.Balance);

        // When attempting to start a second road program while the first is active
        var secondProject = country.StartRoadProject(startBoundary.Add(TimeSpan.FromHours(6)));

        // Then second start fails, balance is NOT debited again, and first project remains active
        Assert.Null(secondProject);
        Assert.Equal(1_500_000.0, country.Treasury.Balance);
        Assert.Same(firstProject, country.RoadProject);
        Assert.Single(country.Projects);
    }

    [Fact]
    public void Test5_ProjectRemainsIncomplete_BeforeCompletionBoundary()
    {
        // Given a country with baseline infrastructure condition 0.6 that starts a road at Day 1, 00:00
        var country = CreateTestCountry("Construction Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // At boundaries before finish (Day 1 06:00, 12:00, 18:00, or 23:59)
        var boundary1 = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var boundary2 = _clock.FromDayAndTime(1, new TimeOnly(12, 0));
        var boundary3 = _clock.FromDayAndTime(1, new TimeOnly(18, 0));
        var justBefore = _clock.FromDayAndTime(1, new TimeOnly(23, 59));

        Assert.False(project.CanComplete(boundary1));
        Assert.False(project.CanComplete(boundary2));
        Assert.False(project.CanComplete(boundary3));
        Assert.False(project.CanComplete(justBefore));

        Assert.False(country.CompleteRoadProject(boundary1));
        Assert.False(country.CompleteRoadProject(boundary2));
        Assert.False(country.CompleteRoadProject(boundary3));
        Assert.False(country.CompleteRoadProject(justBefore));

        // Then infrastructure condition is unchanged and project remains incomplete
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Equal(0.6, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test6_ProjectCompletesExactly_AtCompletionBoundary()
    {
        // Given a country with infrastructure condition 0.6 that starts a road at Day 1, 00:00
        var country = CreateTestCountry("Complete Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When reaching the exact finish boundary (Day 2, 00:00: exactly 4 boundaries / 24 hours later)
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        Assert.True(project.CanComplete(finishBoundary));
        var completed = country.CompleteRoadProject(finishBoundary);

        // Then infrastructure condition rises by 0.1 to 0.7, project is completed, and State Capacity is recalculated
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.True(project.IsCompleted);
        Assert.False(project.IsActive);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);
        Assert.Equal(0.7, country.StateCapacityScore);
    }

    [Fact]
    public void Test7_ProjectCompletes_WhenCheckedAfterCompletionBoundary()
    {
        // Given a country starting a road at Day 1, 00:00 (finish is Day 2, 00:00)
        var country = CreateTestCountry("Late Check Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When checked 8 hours after the finish boundary (Day 2, 08:00)
        var afterFinish = _clock.FromDayAndTime(2, new TimeOnly(8, 0));
        Assert.True(project.CanComplete(afterFinish));
        var completed = country.CompleteRoadProject(afterFinish);

        // Then project completes successfully and applies effect
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test8_CompletionEffect_IsAppliedExactlyOnce_Idempotent()
    {
        // Given a country where a road project reaches completion
        var country = CreateTestCountry("Idempotent Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));

        // First completion call succeeds and raises condition from 0.6 to 0.7
        var firstResult = country.CompleteRoadProject(finishBoundary);
        Assert.True(firstResult);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);

        // Second completion call must be completely idempotent (returns false, does not raise again)
        var secondResult = country.CompleteRoadProject(finishBoundary);
        Assert.False(secondResult);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);

        // Third completion call much later also does nothing
        var laterTime = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var thirdResult = country.CompleteRoadProject(laterTime);
        Assert.False(thirdResult);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test9_CountryIsolation_Preserved()
    {
        // Given two isolated sovereign nations
        var countryA = CreateTestCountry("Country A", 1_000_000.0, infrastructureCondition: 0.5, id: "country-a");
        var countryB = CreateTestCountry("Country B", 1_000_000.0, infrastructureCondition: 0.5, id: "country-b");
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));

        // When Country A starts a road program
        var projectA = countryA.StartRoadProject(startBoundary);
        Assert.NotNull(projectA);
        Assert.Equal("country-a", projectA.CountryId);

        // Country B cannot complete Country A's project
        var finishBoundary = startBoundary.Add(TimeSpan.FromHours(24));
        var foreignComplete = countryB.CompleteProject(projectA, finishBoundary);
        Assert.False(foreignComplete);
        Assert.False(projectA.Completed);
        Assert.Equal(0.5, countryB.StateCapacity.InfrastructureCondition);

        // Country A completes its own project
        var ownComplete = countryA.CompleteRoadProject(finishBoundary);
        Assert.True(ownComplete);

        // Country A paid and benefited
        Assert.Equal(500_000.0, countryA.Treasury.Balance);
        Assert.Equal(0.6, countryA.StateCapacity.InfrastructureCondition);
        Assert.True(projectA.Completed);

        // Country B paid nothing and received zero benefit
        Assert.Equal(1_000_000.0, countryB.Treasury.Balance);
        Assert.Equal(0.5, countryB.StateCapacity.InfrastructureCondition);
        Assert.Null(countryB.RoadProject);
    }

    [Fact]
    public void Test10_OfflineCatchUp_CanCompleteProject()
    {
        // Given a country with an active road program starting at Day 0, 00:00 (finishes Day 1, 00:00)
        var country = CreateTestCountry("Offline Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(0, new TimeOnly(0, 0));
        country.StartRoadProject(startBoundary);
        Assert.False(country.RoadProject!.Completed);
        Assert.Equal(0.6, country.StateCapacity.InfrastructureCondition);

        // When offline catch-up is executed at Day 1, 06:00 (past finish boundary)
        var returnTime = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var briefing = _catchUpService.CatchUp(country, returnTime);

        // Then road project is marked completed, infrastructure condition rose by 0.1, and briefing reports completion
        Assert.True(country.RoadProject.Completed);
        Assert.Equal(0.7, country.StateCapacity.InfrastructureCondition);
        Assert.True(briefing.RoadProgramCompleted);
    }

    [Fact]
    public void Test11_Completion_ClampsInfrastructureConditionAtOne()
    {
        // Given a country with high infrastructure condition (0.95)
        var country = CreateTestCountry("Modern Republic", 1_000_000.0, infrastructureCondition: 0.95);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        country.StartRoadProject(startBoundary);

        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        country.CompleteRoadProject(finishBoundary);

        // 0.95 + 0.1 = 1.05 -> clamped strictly at 1.0
        Assert.Equal(1.0, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test12_GenericProjectFoundation_SupportsAbstraction()
    {
        // Given a country and generic DevelopmentProject contract
        var country = CreateTestCountry("Generic Republic", 1_000_000.0, infrastructureCondition: 0.5);
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));

        IDevelopmentProject project = new NationalRoadProject
        {
            CountryId = country.Id,
            StartedBoundary = startTime,
            FinishBoundary = startTime.Add(TimeSpan.FromHours(24))
        };

        // Starts via generic country method
        var started = country.StartProject((DevelopmentProject)project);
        Assert.True(started);
        Assert.Equal(500_000.0, country.Treasury.Balance);
        Assert.True(project.IsActive);
        Assert.False(project.Completed);

        // Completes via generic country method
        var completed = country.CompleteProject((DevelopmentProject)project, project.FinishBoundary);
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.False(project.IsActive);
        Assert.Equal(0.6, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test13_Project_WithMissingOrInvalidCountryId_CannotStartOrComplete()
    {
        // Given a country with funds and projects with missing or invalid CountryId
        var country = CreateTestCountry("Ownership Republic", 1_000_000.0);
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var finishTime = startTime.Add(TimeSpan.FromHours(24));

        var projectWithEmptyCountryId = new NationalRoadProject
        {
            CountryId = string.Empty,
            StartedBoundary = startTime,
            FinishBoundary = finishTime
        };

        var projectWithWhitespaceCountryId = new NationalRoadProject
        {
            CountryId = "   ",
            StartedBoundary = startTime,
            FinishBoundary = finishTime
        };

        // When attempting to start projects with missing/invalid CountryId
        var emptyStartResult = country.StartProject(projectWithEmptyCountryId);
        var whitespaceStartResult = country.StartProject(projectWithWhitespaceCountryId);

        // Then starts fail, treasury balance is unaffected, and projects collection remains empty
        Assert.False(emptyStartResult);
        Assert.False(whitespaceStartResult);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);
        Assert.Empty(country.Projects);

        // And completion is also refused
        Assert.False(projectWithEmptyCountryId.CanComplete(finishTime));
        Assert.False(projectWithEmptyCountryId.Complete(country, finishTime));
        Assert.False(country.CompleteProject(projectWithEmptyCountryId, finishTime));
    }

    [Fact]
    public void Test14_CountryA_CannotStart_CountryBProject()
    {
        // Given two sovereign nations and a project created for Country B
        var countryA = CreateTestCountry("Country A", 1_000_000.0, id: "country-a");
        var countryB = CreateTestCountry("Country B", 1_000_000.0, id: "country-b");
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));

        var projectB = new NationalRoadProject
        {
            CountryId = countryB.Id,
            StartedBoundary = startTime,
            FinishBoundary = startTime.Add(TimeSpan.FromHours(24))
        };

        // When Country A attempts to start Country B's project
        var startResult = countryA.StartProject(projectB);

        // Then start is rejected, Country A is not charged, and project is not in Country A
        Assert.False(startResult);
        Assert.Equal(1_000_000.0, countryA.Treasury.Balance);
        Assert.Empty(countryA.Projects);
        Assert.Null(countryA.ActiveProject);
    }

    [Fact]
    public void Test15_CountryA_CannotComplete_CountryBProject()
    {
        // Given Country B starting its own road project
        var countryA = CreateTestCountry("Country A", 1_000_000.0, infrastructureCondition: 0.5, id: "country-a");
        var countryB = CreateTestCountry("Country B", 1_000_000.0, infrastructureCondition: 0.5, id: "country-b");
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var finishTime = startTime.Add(TimeSpan.FromHours(24));

        var projectB = countryB.StartRoadProject(startTime);
        Assert.NotNull(projectB);

        // When Country A attempts to complete Country B's project at the finish boundary
        var foreignCompleteViaCountry = countryA.CompleteProject(projectB, finishTime);
        var foreignCompleteViaProject = projectB.Complete(countryA, finishTime);

        // Then both completion attempts fail and project remains incomplete
        Assert.False(foreignCompleteViaCountry);
        Assert.False(foreignCompleteViaProject);
        Assert.False(projectB.Completed);
        Assert.Equal(0.5, countryA.StateCapacity.InfrastructureCondition);
        Assert.Equal(0.5, countryB.StateCapacity.InfrastructureCondition);

        // When Country B completes its own project
        var ownComplete = countryB.CompleteProject(projectB, finishTime);
        Assert.True(ownComplete);
        Assert.True(projectB.Completed);
        Assert.Equal(0.6, countryB.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test16_NoDuplicateProjectRecord_BetweenProjectsCollectionAndRoadProperty()
    {
        // Given a country starting a National Road Program
        var country = CreateTestCountry("No Duplicates Republic", 1_000_000.0);
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));

        var project = country.StartRoadProject(startTime);
        Assert.NotNull(project);

        // Then Projects contains exactly one item
        Assert.Single(country.Projects);

        // And RoadProject references the exact same in-memory instance
        Assert.Same(project, country.Projects[0]);
        Assert.Same(project, country.RoadProject);
        Assert.Same(project, country.ActiveProject);
        Assert.Same(project, country.ActiveRoadProject);

        // And setting RoadProject updates the authoritative Projects collection without duplication
        var updatedProject = new NationalRoadProject
        {
            Id = project.Id,
            CountryId = country.Id,
            StartedBoundary = startTime,
            FinishBoundary = startTime.Add(TimeSpan.FromHours(24))
        };
        country.RoadProject = updatedProject;

        Assert.Single(country.Projects);
        Assert.Same(updatedProject, country.Projects[0]);
        Assert.Same(updatedProject, country.RoadProject);
    }

    [Fact]
    public void Test17_Project_RemainsAssociatedWithCountry_AfterSerializationAndRestoration()
    {
        // Given a country with an active road project
        var originalCountry = CreateTestCountry("Persist Republic", 1_000_000.0, infrastructureCondition: 0.5, id: "persist-country-1");
        var startTime = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        var originalProject = originalCountry.StartRoadProject(startTime);
        Assert.NotNull(originalProject);

        // When serialized to JSON and deserialized back
        var json = System.Text.Json.JsonSerializer.Serialize(originalCountry);
        var restoredCountry = System.Text.Json.JsonSerializer.Deserialize<Country>(json);

        // Then restored country has project correctly associated with its country ID
        Assert.NotNull(restoredCountry);
        Assert.Equal(originalCountry.Id, restoredCountry.Id);
        Assert.Single(restoredCountry.Projects);

        var restoredProject = restoredCountry.Projects[0];
        Assert.Equal(restoredCountry.Id, restoredProject.CountryId);
        Assert.Equal(originalCountry.Id, restoredProject.CountryId);
        Assert.Same(restoredProject, restoredCountry.RoadProject);
    }

    [Fact]
    public void Test18_ActiveRoadProject_SurvivesSerializationAndRestoration()
    {
        // Given a country with an active road program
        var country = CreateTestCountry("Survive Republic", 1_000_000.0, infrastructureCondition: 0.5, id: "survive-1");
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var project = country.StartRoadProject(startTime);
        Assert.NotNull(project);

        // When serialized through SaveEnvelope and JsonStateSerializer
        var serializer = new Republic.Core.Persistence.JsonStateSerializer();
        var worldState = new Republic.Core.World.WorldState
        {
            WorldId = Guid.NewGuid(),
            Name = "Test World",
            Countries = new List<Country> { country }
        };
        var envelope = new Republic.Core.Persistence.SaveEnvelope<Republic.Core.World.WorldState>
        {
            FormatVersion = 1,
            State = worldState
        };

        var json = serializer.Serialize(envelope);
        var loadedEnvelope = serializer.Deserialize<Republic.Core.World.WorldState>(json);
        Assert.NotNull(loadedEnvelope.State);
        var loadedCountry = loadedEnvelope.State.Countries.Single();

        // Then active road project survives with all fields intact
        Assert.Single(loadedCountry.Projects);
        var loadedProject = Assert.IsType<NationalRoadProject>(loadedCountry.Projects[0]);

        Assert.Equal(DevelopmentProjectType.Road, loadedProject.ProjectType);
        Assert.Equal("National Road Program", loadedProject.Name);
        Assert.Equal(500_000.0, loadedProject.Cost);
        Assert.Equal(project.StartedBoundary, loadedProject.StartedBoundary);
        Assert.Equal(project.FinishBoundary, loadedProject.FinishBoundary);
        Assert.False(loadedProject.Completed);
        Assert.True(loadedProject.IsActive);
        Assert.Equal(country.Id, loadedProject.CountryId);
    }

    [Fact]
    public void Test19_RestoredRoadProject_CanStillComplete_AtFinishBoundary()
    {
        // Given an active road project serialized while under construction
        var country = CreateTestCountry("Restored Complete Republic", 1_000_000.0, infrastructureCondition: 0.6, id: "complete-restored");
        var startTime = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        country.StartRoadProject(startTime);

        var json = System.Text.Json.JsonSerializer.Serialize(country);
        var restored = System.Text.Json.JsonSerializer.Deserialize<Country>(json);
        Assert.NotNull(restored);
        Assert.NotNull(restored.RoadProject);

        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        var beforeFinish = _clock.FromDayAndTime(1, new TimeOnly(18, 0));

        // When checked before finish boundary, remains incomplete
        Assert.False(restored.CheckRoadProjectCompletion(beforeFinish));
        Assert.Equal(0.6, restored.StateCapacity.InfrastructureCondition);
        Assert.False(restored.RoadProject.Completed);

        // When checked at finish boundary, completes and raises infrastructure condition
        var completed = restored.CheckRoadProjectCompletion(finishBoundary);
        Assert.True(completed);
        Assert.True(restored.RoadProject.Completed);
        Assert.False(restored.RoadProject.IsActive);
        Assert.Equal(0.7, restored.StateCapacity.InfrastructureCondition);
        Assert.Equal(0.7, restored.StateCapacityScore);

        // Subsequent check is idempotent
        Assert.False(restored.CheckRoadProjectCompletion(finishBoundary));
        Assert.Equal(0.7, restored.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void Test20_Progress_IsZero_BeforeStartBoundary()
    {
        // Given a road project starting at Day 1, 00:00
        var country = CreateTestCountry("Before Start Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When evaluated before the start boundary (Day 0, 12:00 or Day 0, 23:59)
        var wayBefore = _clock.FromDayAndTime(0, new TimeOnly(12, 0));
        var justBefore = _clock.FromDayAndTime(0, new TimeOnly(23, 59));

        // Then progress is exactly 0.0
        Assert.Equal(0.0, project.GetProgress(wayBefore));
        Assert.Equal(0.0, project.GetProgress(justBefore));
        Assert.Equal(0.0, country.GetRoadProgress(wayBefore));
        Assert.Equal(0.0, country.GetActiveProjectProgress(wayBefore));
    }

    [Fact]
    public void Test21_Progress_IsZero_ExactlyAtStartBoundary()
    {
        // Given a road project starting at Day 1, 00:00
        var country = CreateTestCountry("Start Boundary Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When evaluated exactly at the start boundary
        var progress = project.GetProgress(startBoundary);

        // Then progress is exactly 0.0
        Assert.Equal(0.0, progress);
        Assert.Equal(0.0, country.GetRoadProgress(startBoundary));
    }

    [Fact]
    public void Test22_Progress_IsCorrect_HalfwayThroughConstruction()
    {
        // Given a road project lasting 24 hours from Day 1, 00:00 to Day 2, 00:00
        var country = CreateTestCountry("Halfway Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When evaluated exactly halfway (Day 1, 12:00: 12 hours elapsed out of 24)
        var halfway = _clock.FromDayAndTime(1, new TimeOnly(12, 0));
        var progress = project.GetProgress(halfway);

        // Then progress is exactly 0.5
        Assert.Equal(0.5, progress, precision: 6);
        Assert.Equal(0.5, country.GetRoadProgress(halfway)!.Value, precision: 6);
    }

    [Fact]
    public void Test23_Progress_IsCorrect_AtIntermediatePoints()
    {
        // Given a road project lasting 24 hours
        var country = CreateTestCountry("Intermediate Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // 6 hours elapsed (1 boundary) = 0.25 progress
        var quarter = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        Assert.Equal(0.25, project.GetProgress(quarter), precision: 6);

        // 18 hours elapsed (3 boundaries) = 0.75 progress
        var threeQuarters = _clock.FromDayAndTime(1, new TimeOnly(18, 0));
        Assert.Equal(0.75, project.GetProgress(threeQuarters), precision: 6);

        // 8 hours elapsed = 8/24 = 1/3 progress
        var eightHours = _clock.FromDayAndTime(1, new TimeOnly(8, 0));
        Assert.Equal(8.0 / 24.0, project.GetProgress(eightHours), precision: 6);
    }

    [Fact]
    public void Test24_Progress_IsOne_ExactlyAtFinishBoundary()
    {
        // Given a road project finishing at Day 2, 00:00
        var country = CreateTestCountry("Finish Boundary Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When evaluated exactly at finish boundary
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        var progress = project.GetProgress(finishBoundary);

        // Then progress is exactly 1.0
        Assert.Equal(1.0, progress);
        Assert.Equal(1.0, country.GetRoadProgress(finishBoundary));
    }

    [Fact]
    public void Test25_Progress_RemainsOne_AfterFinishBoundary()
    {
        // Given a road project finishing at Day 2, 00:00
        var country = CreateTestCountry("After Finish Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When evaluated after the finish boundary
        var fourHoursLate = _clock.FromDayAndTime(2, new TimeOnly(4, 0));
        var twoDaysLate = _clock.FromDayAndTime(4, new TimeOnly(0, 0));

        // Then progress remains strictly 1.0
        Assert.Equal(1.0, project.GetProgress(fourHoursLate));
        Assert.Equal(1.0, project.GetProgress(twoDaysLate));
    }

    [Fact]
    public void Test26_CompletedProjects_ReportOne()
    {
        // Given a completed project
        var country = CreateTestCountry("Completed Report Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When project completes at finish boundary
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
        country.CompleteRoadProject(finishBoundary);
        Assert.True(project.Completed);

        // Then it reports 1.0 at any evaluation time (even times before or at finish)
        var earlyTime = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        Assert.Equal(1.0, project.GetProgress(earlyTime));
        Assert.Equal(1.0, project.GetProgress(finishBoundary));
        Assert.Equal(1.0, country.GetRoadProgress(earlyTime));
    }

    [Fact]
    public void Test27_Progress_IsNeverBelowZero_OrAboveOne()
    {
        // Given a project with defined boundaries
        var country = CreateTestCountry("Clamp Republic", 1_000_000.0);
        var startBoundary = _clock.FromDayAndTime(5, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // Days before start
        var farPast = _clock.FromDayAndTime(0, new TimeOnly(0, 0));
        Assert.True(project.GetProgress(farPast) >= 0.0);
        Assert.Equal(0.0, project.GetProgress(farPast));

        // Days after finish
        var farFuture = _clock.FromDayAndTime(50, new TimeOnly(0, 0));
        Assert.True(project.GetProgress(farFuture) <= 1.0);
        Assert.Equal(1.0, project.GetProgress(farFuture));
    }

    [Fact]
    public void Test28_ProgressCalculation_DoesNotMutateProject()
    {
        // Given an active road project and baseline country state
        var country = CreateTestCountry("No Mutation Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        var balanceAfterStart = country.Treasury.Balance;
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));

        // When GetProgress is called at and past the finish boundary
        var progressAtFinish = project.GetProgress(finishBoundary);
        var progressPastFinish = project.GetProgress(_clock.FromDayAndTime(3, new TimeOnly(0, 0)));

        // Then progress reports 1.0 but project is NOT automatically completed
        Assert.Equal(1.0, progressAtFinish);
        Assert.Equal(1.0, progressPastFinish);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);

        // And country state is completely unmutated
        Assert.Equal(balanceAfterStart, country.Treasury.Balance);
        Assert.Equal(0.6, country.StateCapacity.InfrastructureCondition);
    }
}
