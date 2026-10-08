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

    private Country CreateTestCountry(string name, double startingBalance, double infrastructureCondition = 0.6)
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

        var treasury = new RepuTreasury(startingBalance, name);
        return Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            treasury: treasury,
            stateCapacity: capacity);
    }

    [Fact]
    public void InsufficientFunds_DoesNotStartProject_AndDoesNotChangeBalance()
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
        Assert.Equal(startingBalance, country.Treasury.Balance);
    }

    [Fact]
    public void PaidStart_WithdrawsExactly500000()
    {
        // Given a sovereign country with R1,000,000 in treasury
        var startingBalance = 1_000_000.0;
        var country = CreateTestCountry("Funded Republic", startingBalance);
        var startTime = _clock.CurrentTime;

        // When starting the National Road Program
        var project = country.StartRoadProject(startTime);

        // Then exactly R500,000 is withdrawn once
        Assert.NotNull(project);
        Assert.Same(project, country.RoadProject);
        Assert.Equal(startingBalance - 500_000.0, country.Treasury.Balance);
        Assert.Equal(500_000.0, country.Treasury.Balance);
        Assert.Equal(500_000.0, project.Cost);
        Assert.False(project.Completed);
        Assert.False(project.IsCompleted);
        Assert.True(project.IsActive);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(24)), project.FinishBoundary);
    }

    [Fact]
    public void BeforeFinishBoundary_InfrastructureConditionIsUnchanged()
    {
        // Given a country with baseline infrastructure condition 0.6 that starts a road at Day 1, 00:00
        var country = CreateTestCountry("Construction Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // At boundaries before finish (e.g. Day 1 06:00, 12:00, 18:00, or 23:59)
        var boundary1 = _clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var boundary2 = _clock.FromDayAndTime(1, new TimeOnly(12, 0));
        var boundary3 = _clock.FromDayAndTime(1, new TimeOnly(18, 0));
        var justBefore = _clock.FromDayAndTime(1, new TimeOnly(23, 59));

        Assert.False(country.CompleteRoadProject(boundary1));
        Assert.False(country.CompleteRoadProject(boundary2));
        Assert.False(country.CompleteRoadProject(boundary3));
        Assert.False(country.CompleteRoadProject(justBefore));

        // Then infrastructure condition is unchanged and project remains incomplete
        Assert.False(project.Completed);
        Assert.Equal(0.6, country.StateCapacity.InfrastructureCondition);
    }

    [Fact]
    public void AtFinishBoundary_InfrastructureConditionRisesByPoint1_AndProjectIsCompleted()
    {
        // Given a country with infrastructure condition 0.6 that starts a road at Day 1, 00:00
        var country = CreateTestCountry("Complete Republic", 1_000_000.0, infrastructureCondition: 0.6);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));
        var project = country.StartRoadProject(startBoundary);
        Assert.NotNull(project);

        // When reaching the exact finish boundary (Day 2, 00:00: exactly 4 boundaries / 24 hours later)
        var finishBoundary = _clock.FromDayAndTime(2, new TimeOnly(0, 0));
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
    public void Completion_ClampsInfrastructureConditionAtOne()
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
    public void SecondStartWhileActive_Fails()
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
    }

    [Fact]
    public void CountryIsolation_CountryBDoesNotPayForOrBenefitFromCountryARoad()
    {
        // Given two isolated sovereign nations
        var countryA = CreateTestCountry("Country A", 1_000_000.0, infrastructureCondition: 0.5);
        var countryB = CreateTestCountry("Country B", 1_000_000.0, infrastructureCondition: 0.5);
        var startBoundary = _clock.FromDayAndTime(1, new TimeOnly(0, 0));

        // When Country A starts and completes a road program
        var projectA = countryA.StartRoadProject(startBoundary);
        Assert.NotNull(projectA);
        var finishBoundary = startBoundary.Add(TimeSpan.FromHours(24));
        countryA.CompleteRoadProject(finishBoundary);

        // Country A paid and benefited
        Assert.Equal(500_000.0, countryA.Treasury.Balance);
        Assert.Equal(0.6, countryA.StateCapacity.InfrastructureCondition);
        Assert.True(countryA.RoadProject?.Completed);

        // Country B paid nothing and received zero benefit
        Assert.Equal(1_000_000.0, countryB.Treasury.Balance);
        Assert.Equal(0.5, countryB.StateCapacity.InfrastructureCondition);
        Assert.Null(countryB.RoadProject);
    }

    [Fact]
    public void OfflineCatchUp_CompletesRoad_WhenPastFinishBoundary()
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
}
