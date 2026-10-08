namespace Republic.Core.Tests.Economy;

using System.Text.Json;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalUniversityProjectTests
{
    private readonly RepublicClock _clock;

    public NationalUniversityProjectTests()
    {
        _clock = RepublicClock.CreateControlled();
    }

    private Country CreateTestCountry(
        string name,
        double startingBalance,
        double humanCapital = 50.0,
        double scienceCapacity = 40.0,
        string? id = null)
    {
        var countryId = id ?? name.ToLowerInvariant().Replace(" ", "-");
        var treasury = new RepuTreasury(startingBalance, countryId);
        var yield = new NationalYield
        {
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
    public void Test1_UniversityStartsSuccessfully_WhenTreasuryCanPay()
    {
        // Given a sovereign country with R1,000,000 in treasury
        var country = CreateTestCountry("Scholarly Republic", 1_000_000.0);
        var startTime = _clock.CurrentTime;

        // When starting the National University
        var project = country.StartUniversityProject(startTime);

        // Then project starts successfully and records all metadata
        Assert.NotNull(project);
        Assert.Same(project, country.UniversityProject);
        Assert.Equal(country.Id, project.CountryId);
        Assert.Equal(DevelopmentProjectType.University, project.ProjectType);
        Assert.Equal("National University", project.Name);
        Assert.Equal(NationalUniversityProject.StandardCost, project.Cost);
        Assert.Equal(750_000.0, project.Cost);
        Assert.False(project.Completed);
        Assert.False(project.IsCompleted);
        Assert.True(project.IsActive);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime, project.StartedTime);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(48)), project.FinishBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(48)), project.FinishTime);
        Assert.Contains(project, country.Projects);
        Assert.Same(project, country.ActiveProject);
    }

    [Fact]
    public void Test2_TreasuryIsReducedByExactly750k()
    {
        // Given a sovereign country with R1,000,000 in treasury
        var country = CreateTestCountry("Funding Republic", 1_000_000.0);
        var startTime = _clock.CurrentTime;

        // When starting the National University
        var project = country.StartUniversityProject(startTime);

        // Then treasury is reduced by exactly R750,000
        Assert.NotNull(project);
        Assert.Equal(250_000.0, country.Treasury.Balance);

        // And an underfunded country cannot start and balance remains unchanged
        var poorCountry = CreateTestCountry("Struggling Republic", 700_000.0);
        var failedProject = poorCountry.StartUniversityProject(startTime);

        Assert.Null(failedProject);
        Assert.Equal(700_000.0, poorCountry.Treasury.Balance);
    }

    [Fact]
    public void Test3_UniversityBelongsToCorrectCountryId()
    {
        // Given a country with a specific ID
        var country = CreateTestCountry("Isolated Republic", 1_000_000.0, id: "sovereign-alpha");
        var startTime = _clock.CurrentTime;

        // When university starts
        var project = country.StartUniversityProject(startTime);

        // Then project strictly belongs to that country's ID
        Assert.NotNull(project);
        Assert.Equal("sovereign-alpha", project.CountryId);
        Assert.Equal(country.Id, project.CountryId);
    }

    [Fact]
    public void Test4_UniversityCannotBeStartedByAnotherCountry()
    {
        // Given Country A and Country B
        var countryA = CreateTestCountry("Country Alpha", 1_000_000.0, id: "country-a");
        var countryB = CreateTestCountry("Country Beta", 1_000_000.0, id: "country-b");
        var startTime = _clock.CurrentTime;

        // When a project explicitly configured for Country B is attempted to be started by Country A
        var foreignProject = new NationalUniversityProject(countryB.Id, startTime);
        var started = countryA.StartProject(foreignProject);

        // Then Country A rejects starting the project
        Assert.False(started);
        Assert.Equal(1_000_000.0, countryA.Treasury.Balance);
        Assert.Empty(countryA.Projects);

        // Nor can Country A complete Country B's project
        countryB.StartProject(foreignProject);
        var completeAttempt = countryA.CompleteProject(foreignProject, startTime.Add(TimeSpan.FromHours(48)));
        Assert.False(completeAttempt);
        Assert.False(foreignProject.Completed);
    }

    [Fact]
    public void Test5_ProgressIsZeroAtStart()
    {
        // Given a started university
        var country = CreateTestCountry("Academia Republic", 1_000_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        // At start time, progress is 0.0
        Assert.Equal(0.0, project.GetProgress(startTime));
        Assert.Equal(0.0, country.GetUniversityProgress(startTime));

        // Prior to start time, progress is also 0.0
        var beforeStart = startTime.Add(TimeSpan.FromHours(-1));
        Assert.Equal(0.0, project.GetProgress(beforeStart));
    }

    [Fact]
    public void Test6_ProgressIsHalfwayAt24Hours()
    {
        // Given a university project with 48 hours duration
        var country = CreateTestCountry("Midterm Republic", 1_000_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        // At 24 hours (50% of 48h)
        var halfwayTime = startTime.Add(TimeSpan.FromHours(24));
        Assert.Equal(0.5, project.GetProgress(halfwayTime));
        Assert.Equal(0.5, country.GetUniversityProgress(halfwayTime));

        // At 12 hours (25% of 48h)
        var quarterTime = startTime.Add(TimeSpan.FromHours(12));
        Assert.Equal(0.25, project.GetProgress(quarterTime));

        // At 36 hours (75% of 48h)
        var threeQuarterTime = startTime.Add(TimeSpan.FromHours(36));
        Assert.Equal(0.75, project.GetProgress(threeQuarterTime));
    }

    [Fact]
    public void Test7_ProgressIsOneAtFinishBoundary()
    {
        // Given a university project finishing at 48 hours
        var country = CreateTestCountry("Graduate Republic", 1_000_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // At finish boundary, progress is exactly 1.0
        Assert.Equal(1.0, project.GetProgress(finishTime));
        Assert.Equal(1.0, country.GetUniversityProgress(finishTime));

        // Beyond finish boundary, progress remains clamped to 1.0
        var pastFinishTime = startTime.Add(TimeSpan.FromHours(72));
        Assert.Equal(1.0, project.GetProgress(pastFinishTime));
    }

    [Fact]
    public void Test8_ProgressDoesNotAutomaticallyCompleteProject()
    {
        // Given a university project evaluated after finish boundary
        var country = CreateTestCountry("Pending Republic", 1_000_000.0, humanCapital: 50.0, scienceCapacity: 40.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // When evaluating progress
        var progress = project.GetProgress(finishTime);

        // Then progress is 1.0 but the project remains incomplete until explicitly completed
        Assert.Equal(1.0, progress);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Equal(50.0, country.Yield.HumanCapital);
        Assert.Equal(40.0, country.Yield.ScienceCapacity);
    }

    [Fact]
    public void Test9_CompletionAppliesIntendedHumanCapitalEffect()
    {
        // Given a country with baseline Human Capital 50.0
        var country = CreateTestCountry("Humanity Republic", 1_000_000.0, humanCapital: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // When completed at finish boundary
        var completed = country.CompleteUniversityProject(finishTime);

        // Then completion succeeds and increases Human Capital by +5.0
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.Equal(55.0, country.Yield.HumanCapital);
    }

    [Fact]
    public void Test10_CompletionAppliesIntendedScienceCapacityEffect()
    {
        // Given a country with baseline Science Capacity 40.0
        var country = CreateTestCountry("Research Republic", 1_000_000.0, scienceCapacity: 40.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // When completed at finish boundary
        var completed = country.CompleteUniversityProject(finishTime);

        // Then completion succeeds and increases Science / Research Capacity by +5.0
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.Equal(45.0, country.Yield.ScienceCapacity);
    }

    [Fact]
    public void Test11_CompletionIsIdempotent()
    {
        // Given a country and completed university project
        var country = CreateTestCountry("Idempotent Republic", 1_000_000.0, humanCapital: 50.0, scienceCapacity: 40.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // First completion succeeds
        var first = country.CompleteUniversityProject(finishTime);
        Assert.True(first);
        Assert.Equal(55.0, country.Yield.HumanCapital);
        Assert.Equal(45.0, country.Yield.ScienceCapacity);

        // Subsequent completions return false and leave yields unchanged
        var second = country.CompleteUniversityProject(finishTime);
        Assert.False(second);

        var directCall = project.Complete(country, finishTime);
        Assert.False(directCall);

        var genericCall = country.CompleteProject(project, finishTime);
        Assert.False(genericCall);

        Assert.Equal(55.0, country.Yield.HumanCapital);
        Assert.Equal(45.0, country.Yield.ScienceCapacity);
    }

    [Fact]
    public void Test12_ActiveUniversitySurvivesJsonSerialization()
    {
        // Given a country with an active university halfway through construction
        var country = CreateTestCountry("Persistent Republic", 1_000_000.0, humanCapital: 52.0, scienceCapacity: 43.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartUniversityProject(startTime);
        Assert.NotNull(project);

        var halfwayTime = startTime.Add(TimeSpan.FromHours(24));
        Assert.Equal(0.5, project.GetProgress(halfwayTime));

        // When serializing to JSON and restoring
        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);

        // Then all metadata and polymorphic types are preserved
        Assert.NotNull(restored);
        Assert.Single(restored.Projects);

        var restoredProject = Assert.IsType<NationalUniversityProject>(restored.Projects[0]);
        Assert.Equal(DevelopmentProjectType.University, restoredProject.ProjectType);
        Assert.Equal("National University", restoredProject.Name);
        Assert.Equal(country.Id, restoredProject.CountryId);
        Assert.Equal(750_000.0, restoredProject.Cost);
        Assert.Equal(startTime, restoredProject.StartedBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(48)), restoredProject.FinishBoundary);
        Assert.False(restoredProject.Completed);
        Assert.True(restoredProject.IsActive);

        // And progress calculation continues to function accurately
        Assert.Equal(0.5, restoredProject.GetProgress(halfwayTime));
        Assert.Equal(0.5, restored.GetUniversityProgress(halfwayTime));
    }

    [Fact]
    public void Test13_RestoredUniversityCanCompleteCorrectly()
    {
        // Given a serialized country with an in-progress university
        var country = CreateTestCountry("Completing Republic", 1_000_000.0, humanCapital: 60.0, scienceCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        country.StartUniversityProject(startTime);

        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);
        Assert.NotNull(restored);

        var finishTime = startTime.Add(TimeSpan.FromHours(48));

        // When completing the restored university at finish boundary
        var completed = restored.CompleteUniversityProject(finishTime);

        // Then completion succeeds and effects are applied to restored country
        Assert.True(completed);
        Assert.NotNull(restored.UniversityProject);
        Assert.True(restored.UniversityProject.Completed);
        Assert.Equal(65.0, restored.Yield.HumanCapital);
        Assert.Equal(55.0, restored.Yield.ScienceCapacity);

        // And completion remains idempotent on restored instance
        Assert.False(restored.CompleteUniversityProject(finishTime));
        Assert.Equal(65.0, restored.Yield.HumanCapital);
        Assert.Equal(55.0, restored.Yield.ScienceCapacity);
    }

    [Fact]
    public void Test14_RoadAndUniversityProjectsCanCoexistPolymorphically()
    {
        // Given a country with enough treasury for both Road (500k) and University (750k)
        var country = CreateTestCountry("MultiProject Republic", 2_000_000.0, humanCapital: 50.0, scienceCapacity: 40.0);
        var startTime = _clock.CurrentTime;

        // When starting both projects
        var road = country.StartRoadProject(startTime);
        var university = country.StartUniversityProject(startTime);

        Assert.NotNull(road);
        Assert.NotNull(university);
        Assert.Equal(2, country.Projects.Count);
        Assert.Equal(750_000.0, country.Treasury.Balance); // 2M - 500k - 750k = 750k

        // Road finishes at 24h
        var roadFinish = startTime.Add(TimeSpan.FromHours(24));
        Assert.Equal(1.0, road.GetProgress(roadFinish));
        Assert.Equal(0.5, university.GetProgress(roadFinish));

        Assert.True(country.CompleteRoadProject(roadFinish));
        Assert.True(road.Completed);
        Assert.False(university.Completed);

        // Serialize and restore both projects
        var json = JsonSerializer.Serialize(country);
        var restored = JsonSerializer.Deserialize<Country>(json);
        Assert.NotNull(restored);
        Assert.Equal(2, restored.Projects.Count);

        var restoredRoad = Assert.IsType<NationalRoadProject>(restored.RoadProject);
        var restoredUni = Assert.IsType<NationalUniversityProject>(restored.UniversityProject);

        Assert.True(restoredRoad.Completed);
        Assert.False(restoredUni.Completed);

        // University finishes at 48h
        var uniFinish = startTime.Add(TimeSpan.FromHours(48));
        Assert.True(restored.CompleteUniversityProject(uniFinish));
        Assert.True(restoredUni.Completed);
        Assert.Equal(55.0, restored.Yield.HumanCapital);
        Assert.Equal(45.0, restored.Yield.ScienceCapacity);
    }
}
