namespace Republic.Core.Tests.Economy;

using System.Text.Json;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalPowerPlantProjectTests
{
    private readonly RepublicClock _clock;

    public NationalPowerPlantProjectTests()
    {
        _clock = RepublicClock.CreateControlled();
    }

    private Country CreateTestCountry(
        string name,
        double startingBalance,
        double energyCapacity = 50.0,
        double humanCapital = 50.0,
        double scienceCapacity = 40.0,
        string? id = null)
    {
        var countryId = id ?? name.ToLowerInvariant().Replace(" ", "-");
        var treasury = new RepuTreasury(startingBalance, countryId);
        var yield = new NationalYield
        {
            EnergyCapacity = energyCapacity,
            HumanCapital = humanCapital,
            ScienceCapacity = scienceCapacity
        };

        return Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            id: countryId,
            treasury: treasury,
            yield: yield);
    }

    [Fact]
    public void Test1_PowerPlantStartsSuccessfully_WhenTreasuryCanPay()
    {
        // Given a sovereign country with R1,500,000 in treasury
        var country = CreateTestCountry("Energetic Republic", 1_500_000.0);
        var startTime = _clock.CurrentTime;

        // When starting the National Power Plant
        var project = country.StartPowerPlantProject(startTime);

        // Then project starts successfully and records all metadata
        Assert.NotNull(project);
        Assert.Same(project, country.PowerPlantProject);
        Assert.Same(project, country.ActivePowerPlantProject);
        Assert.Equal(country.Id, project.CountryId);
        Assert.Equal(DevelopmentProjectType.PowerPlant, project.ProjectType);
        Assert.Equal("National Power Plant", project.Name);
        Assert.Equal(NationalPowerPlantProject.StandardCost, project.Cost);
        Assert.Equal(1_000_000.0, project.Cost);
        Assert.False(project.Completed);
        Assert.False(project.IsCompleted);
        Assert.True(project.IsActive);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime, project.StartedTime);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), project.FinishBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), project.FinishTime);
        Assert.Contains(project, country.Projects);
        Assert.Same(project, country.ActiveProject);
    }

    [Fact]
    public void Test2_TreasuryIsReducedByExactly1Million()
    {
        // Given a sovereign country with R1,500,000 in treasury
        var country = CreateTestCountry("Funding Republic", 1_500_000.0);
        var startTime = _clock.CurrentTime;

        // When starting the National Power Plant
        var project = country.StartPowerPlantProject(startTime);

        // Then treasury is reduced by exactly R1,000,000
        Assert.NotNull(project);
        Assert.Equal(500_000.0, country.Treasury.Balance);

        // And an underfunded country cannot start and balance remains unchanged
        var poorCountry = CreateTestCountry("Struggling Republic", 999_999.0);
        var failedProject = poorCountry.StartPowerPlantProject(startTime);

        Assert.Null(failedProject);
        Assert.Equal(999_999.0, poorCountry.Treasury.Balance);
    }

    [Fact]
    public void Test3_PowerPlantBelongsToCorrectCountryId()
    {
        // Given a country with a specific ID
        var country = CreateTestCountry("Isolated Republic", 1_500_000.0, id: "sovereign-power");
        var startTime = _clock.CurrentTime;

        // When power plant starts
        var project = country.StartPowerPlantProject(startTime);

        // Then project strictly belongs to that country's ID
        Assert.NotNull(project);
        Assert.Equal("sovereign-power", project.CountryId);
        Assert.Equal(country.Id, project.CountryId);
    }

    [Fact]
    public void Test4_PowerPlantCannotBeStartedOrCompletedByAnotherCountry()
    {
        // Given Country A and Country B
        var countryA = CreateTestCountry("Country Alpha", 1_500_000.0, id: "country-a");
        var countryB = CreateTestCountry("Country Beta", 1_500_000.0, id: "country-b");
        var startTime = _clock.CurrentTime;

        // When a project explicitly configured for Country B is attempted to be started by Country A
        var foreignProject = new NationalPowerPlantProject(countryB.Id, startTime);
        var started = countryA.StartProject(foreignProject);

        // Then Country A rejects starting the project
        Assert.False(started);
        Assert.Equal(1_500_000.0, countryA.Treasury.Balance);
        Assert.Empty(countryA.Projects);

        // Nor can Country A complete Country B's project
        countryB.StartProject(foreignProject);
        var completeAttempt = countryA.CompleteProject(foreignProject, startTime.Add(TimeSpan.FromHours(72)));
        Assert.False(completeAttempt);
        Assert.False(foreignProject.Completed);

        var directCallAttempt = foreignProject.Complete(countryA, startTime.Add(TimeSpan.FromHours(72)));
        Assert.False(directCallAttempt);
        Assert.False(foreignProject.Completed);
    }

    [Fact]
    public void Test5_ProgressIsZeroAtStart()
    {
        // Given a started power plant
        var country = CreateTestCountry("Grid Republic", 1_500_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // At start time, progress is 0.0
        Assert.Equal(0.0, project.GetProgress(startTime));
        Assert.Equal(0.0, country.GetPowerPlantProgress(startTime));

        // Prior to start time, progress is also 0.0
        var beforeStart = startTime.Add(TimeSpan.FromHours(-1));
        Assert.Equal(0.0, project.GetProgress(beforeStart));
    }

    [Fact]
    public void Test6_ProgressIsHalfwayAt36Hours()
    {
        // Given a power plant project with 72 hours duration
        var country = CreateTestCountry("Progress Republic", 1_500_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // At 18 hours (25% of 72h)
        var quarterTime = startTime.Add(TimeSpan.FromHours(18));
        Assert.Equal(0.25, project.GetProgress(quarterTime));

        // At 36 hours (50% of 72h)
        var halfwayTime = startTime.Add(TimeSpan.FromHours(36));
        Assert.Equal(0.5, project.GetProgress(halfwayTime));
        Assert.Equal(0.5, country.GetPowerPlantProgress(halfwayTime));

        // At 54 hours (75% of 72h)
        var threeQuarterTime = startTime.Add(TimeSpan.FromHours(54));
        Assert.Equal(0.75, project.GetProgress(threeQuarterTime));
    }

    [Fact]
    public void Test7_ProgressIsOneAtFinishBoundary()
    {
        // Given a power plant project finishing at 72 hours
        var country = CreateTestCountry("Complete Grid Republic", 1_500_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(72));

        // At finish boundary, progress is exactly 1.0
        Assert.Equal(1.0, project.GetProgress(finishTime));
        Assert.Equal(1.0, country.GetPowerPlantProgress(finishTime));

        // Beyond finish boundary, progress remains clamped to 1.0
        var pastFinishTime = startTime.Add(TimeSpan.FromHours(100));
        Assert.Equal(1.0, project.GetProgress(pastFinishTime));
    }

    [Fact]
    public void Test8_ProgressDoesNotAutomaticallyCompleteProject()
    {
        // Given a power plant project evaluated after finish boundary
        var country = CreateTestCountry("Pending Power Republic", 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(72));

        // When evaluating progress
        var progress = project.GetProgress(finishTime);

        // Then progress is 1.0 but the project remains incomplete until explicitly completed
        Assert.Equal(1.0, progress);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test9_CannotCompleteBeforeFinishBoundary()
    {
        // Given a started power plant project
        var country = CreateTestCountry("Early Republic", 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // Attempting completion halfway through fails
        var halfwayTime = startTime.Add(TimeSpan.FromHours(36));
        Assert.False(country.CompletePowerPlantProject(halfwayTime));
        Assert.False(project.Completed);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);

        // Attempting completion 1 hour before finish boundary fails
        var almostTime = startTime.Add(TimeSpan.FromHours(71));
        Assert.False(country.CompletePowerPlantProject(almostTime));
        Assert.False(project.Completed);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test10_CompletionAppliesIntendedEnergyEffect()
    {
        // Given a country with baseline Energy Capacity 50.0
        var country = CreateTestCountry("Watt Republic", 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(72));

        // When completed at finish boundary
        var completed = country.CompletePowerPlantProject(finishTime);

        // Then completion succeeds and increases Energy Capacity by +5.0
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test11_CompletionIsIdempotent()
    {
        // Given a country and completed power plant project
        var country = CreateTestCountry("Idempotent Power Republic", 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(72));

        // First completion succeeds
        var first = country.CompletePowerPlantProject(finishTime);
        Assert.True(first);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);

        // Subsequent completions return false and leave yields unchanged
        var second = country.CompletePowerPlantProject(finishTime);
        Assert.False(second);

        var directCall = project.Complete(country, finishTime);
        Assert.False(directCall);

        var genericCall = country.CompleteProject(project, finishTime);
        Assert.False(genericCall);

        Assert.Equal(55.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test12_ActivePowerPlantSurvivesJsonSerialization()
    {
        // Given a country with an active power plant halfway through construction
        var country = CreateTestCountry("Persistent Power Republic", 1_500_000.0, energyCapacity: 52.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        var halfwayTime = startTime.Add(TimeSpan.FromHours(36));
        Assert.Equal(0.5, project.GetProgress(halfwayTime));

        // When serializing to JSON and restoring
        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);

        // Then all metadata and polymorphic types are preserved
        Assert.NotNull(restored);
        Assert.Single(restored.Projects);

        var restoredProject = Assert.IsType<NationalPowerPlantProject>(restored.Projects[0]);
        Assert.Equal(DevelopmentProjectType.PowerPlant, restoredProject.ProjectType);
        Assert.Equal("National Power Plant", restoredProject.Name);
        Assert.Equal(country.Id, restoredProject.CountryId);
        Assert.Equal(1_000_000.0, restoredProject.Cost);
        Assert.Equal(startTime, restoredProject.StartedBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), restoredProject.FinishBoundary);
        Assert.False(restoredProject.Completed);
        Assert.True(restoredProject.IsActive);

        // And progress calculation continues to function accurately
        Assert.Equal(0.5, restoredProject.GetProgress(halfwayTime));
        Assert.Equal(0.5, restored.GetPowerPlantProgress(halfwayTime));
    }

    [Fact]
    public void Test13_RestoredPowerPlantCanCompleteCorrectly()
    {
        // Given a serialized country with an in-progress power plant
        var country = CreateTestCountry("Completing Power Republic", 1_500_000.0, energyCapacity: 60.0);
        var startTime = _clock.CurrentTime;
        country.StartPowerPlantProject(startTime);

        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);
        Assert.NotNull(restored);

        var finishTime = startTime.Add(TimeSpan.FromHours(72));

        // When completing the restored power plant at finish boundary
        var completed = restored.CompletePowerPlantProject(finishTime);

        // Then completion succeeds and effects are applied to restored country
        Assert.True(completed);
        Assert.NotNull(restored.PowerPlantProject);
        Assert.True(restored.PowerPlantProject.Completed);
        Assert.Equal(65.0, restored.Yield.EnergyCapacity);

        // And completion remains idempotent on restored instance
        Assert.False(restored.CompletePowerPlantProject(finishTime));
        Assert.Equal(65.0, restored.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test14_RoadUniversityAndPowerPlantCanCoexistPolymorphically()
    {
        // Given a country with enough treasury for Road (500k), University (750k), and PowerPlant (1,000k)
        var country = CreateTestCountry("TriProject Republic", 3_000_000.0, energyCapacity: 50.0, humanCapital: 50.0, scienceCapacity: 40.0);
        var startTime = _clock.CurrentTime;

        // When starting all three projects
        var road = country.StartRoadProject(startTime);
        var university = country.StartUniversityProject(startTime);
        var powerPlant = country.StartPowerPlantProject(startTime);

        Assert.NotNull(road);
        Assert.NotNull(university);
        Assert.NotNull(powerPlant);
        Assert.Equal(3, country.Projects.Count);
        // 3M - 500k - 750k - 1000k = 750k
        Assert.Equal(750_000.0, country.Treasury.Balance);

        // Road finishes at 24h
        var roadFinish = startTime.Add(TimeSpan.FromHours(24));
        Assert.Equal(1.0, road.GetProgress(roadFinish));
        Assert.Equal(0.5, university.GetProgress(roadFinish));
        Assert.Equal(24.0 / 72.0, powerPlant.GetProgress(roadFinish), precision: 5);

        Assert.True(country.CompleteRoadProject(roadFinish));
        Assert.True(road.Completed);
        Assert.False(university.Completed);
        Assert.False(powerPlant.Completed);

        // University finishes at 48h
        var uniFinish = startTime.Add(TimeSpan.FromHours(48));
        Assert.Equal(1.0, university.GetProgress(uniFinish));
        Assert.Equal(48.0 / 72.0, powerPlant.GetProgress(uniFinish), precision: 5);

        Assert.True(country.CompleteUniversityProject(uniFinish));
        Assert.True(university.Completed);
        Assert.False(powerPlant.Completed);
        Assert.Equal(55.0, country.Yield.HumanCapital);
        Assert.Equal(45.0, country.Yield.ScienceCapacity);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);

        // Serialize and restore all 3 projects
        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);
        Assert.NotNull(restored);
        Assert.Equal(3, restored.Projects.Count);

        var restoredRoad = Assert.IsType<NationalRoadProject>(restored.RoadProject);
        var restoredUni = Assert.IsType<NationalUniversityProject>(restored.UniversityProject);
        var restoredPlant = Assert.IsType<NationalPowerPlantProject>(restored.PowerPlantProject);

        Assert.True(restoredRoad.Completed);
        Assert.True(restoredUni.Completed);
        Assert.False(restoredPlant.Completed);

        // Power Plant finishes at 72h
        var plantFinish = startTime.Add(TimeSpan.FromHours(72));
        Assert.True(restored.CompletePowerPlantProject(plantFinish));
        Assert.True(restoredPlant.Completed);
        Assert.Equal(55.0, restored.Yield.HumanCapital);
        Assert.Equal(45.0, restored.Yield.ScienceCapacity);
        Assert.Equal(55.0, restored.Yield.EnergyCapacity);
    }

    [Fact]
    public void Test15_CannotStartDuplicateActivePowerPlant()
    {
        // Given a country with an active power plant
        var country = CreateTestCountry("Single Plant Republic", 3_000_000.0);
        var startTime = _clock.CurrentTime;

        var first = country.StartPowerPlantProject(startTime);
        Assert.NotNull(first);
        Assert.Equal(2_000_000.0, country.Treasury.Balance);

        // Attempting to start a second power plant while first is active fails
        var second = country.StartPowerPlantProject(startTime);
        Assert.Null(second);
        Assert.Equal(2_000_000.0, country.Treasury.Balance);
        Assert.Single(country.Projects);
    }
}
