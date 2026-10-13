namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalDevelopmentProjectsPlayableTests
{
    private readonly RepublicClock _clock;

    public NationalDevelopmentProjectsPlayableTests()
    {
        _clock = RepublicClock.CreateControlled();
    }

    private Country CreateTestCountry(
        string name = "Playable Republic",
        double startingBalance = 2_000_000.0,
        double energyCapacity = 50.0)
    {
        var countryId = name.ToLowerInvariant().Replace(" ", "-");
        var treasury = new RepuTreasury(startingBalance, countryId);
        var yield = new NationalYield
        {
            EnergyCapacity = energyCapacity,
            HumanCapital = 50.0,
            ScienceCapacity = 40.0
        };

        return Country.Found(
            name: name,
            foundingTime: _clock.CurrentTime,
            id: countryId,
            treasury: treasury,
            yield: yield);
    }

    [Fact]
    public void StartingProject_DeductsCostOnlyOnce_AndSetsExpectedMetadata()
    {
        var country = CreateTestCountry("Single Deduction Republic", startingBalance: 2_000_000.0);
        var startTime = _clock.CurrentTime;

        var project = country.StartPowerPlantProject(startTime);

        Assert.NotNull(project);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);
        Assert.Equal(NationalPowerPlantProject.StandardCost, project.Cost);
        Assert.Equal("National Power Plant", project.Name);
        Assert.Equal(DevelopmentProjectType.PowerPlant, project.ProjectType);
        Assert.Equal(startTime, project.StartedBoundary);
        Assert.Equal(startTime.Add(TimeSpan.FromHours(72)), project.FinishBoundary);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Same(project, country.ActivePowerPlantProject);

        // Balance remains 1,000,000 after inspecting progress and metadata
        Assert.Equal(1_000_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void InsufficientFunds_ProducesClearFailure_WithoutChangingBalance()
    {
        var country = CreateTestCountry("Underfunded Republic", startingBalance: 999_999.0);
        var startTime = _clock.CurrentTime;

        var project = country.StartPowerPlantProject(startTime);

        Assert.Null(project);
        Assert.Equal(999_999.0, country.Treasury.Balance);
        Assert.Null(country.ActivePowerPlantProject);
        Assert.Empty(country.Projects);
    }

    [Fact]
    public void StartingProject_WhenAnotherPowerPlantIsAlreadyActive_FailsWithoutChangingBalance()
    {
        var country = CreateTestCountry("Duplicate Check Republic", startingBalance: 3_000_000.0);
        var startTime = _clock.CurrentTime;

        var first = country.StartPowerPlantProject(startTime);
        Assert.NotNull(first);
        Assert.Equal(2_000_000.0, country.Treasury.Balance);

        // Attempting to start a second power plant while the first is active fails
        var second = country.StartPowerPlantProject(startTime);
        Assert.Null(second);
        Assert.Equal(2_000_000.0, country.Treasury.Balance);
        Assert.Single(country.Projects);
    }

    [Fact]
    public void Progress_ReflectsSharedSimulationClock()
    {
        var country = CreateTestCountry("Clock Republic", startingBalance: 1_500_000.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // At T0, progress is 0.0
        Assert.Equal(0.0, country.GetPowerPlantProgress(_clock.CurrentTime));

        // Advance clock by 36 hours (50%)
        _clock.AdvanceHours(36);
        Assert.Equal(0.5, country.GetPowerPlantProgress(_clock.CurrentTime));

        // Advance clock by another 36 hours (total 72h, 100%)
        _clock.AdvanceHours(36);
        Assert.Equal(1.0, country.GetPowerPlantProgress(_clock.CurrentTime));

        // Advance clock further (100h total, clamped at 1.0)
        _clock.AdvanceHours(28);
        Assert.Equal(1.0, country.GetPowerPlantProgress(_clock.CurrentTime));
    }

    [Fact]
    public void CompletionBeforeFinishTime_IsRejected_AndYieldRemainsUnchanged()
    {
        var country = CreateTestCountry("Premature Republic", startingBalance: 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // At 36 hours
        _clock.AdvanceHours(36);
        var completeAt36h = country.CompletePowerPlantProject(_clock.CurrentTime);
        Assert.False(completeAt36h);
        Assert.False(project.Completed);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);

        // At 71 hours (1 hour short)
        _clock.AdvanceHours(35);
        var completeAt71h = country.CompletePowerPlantProject(_clock.CurrentTime);
        Assert.False(completeAt71h);
        Assert.False(project.Completed);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void CompletionAtOrAfterFinishTime_AppliesEnergyCapacityIncreaseOnce_AndDoesNotDeductFunds()
    {
        var country = CreateTestCountry("Commission Republic", startingBalance: 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);
        Assert.Equal(500_000.0, country.Treasury.Balance);

        // Advance to 72 hours (finish boundary)
        _clock.AdvanceHours(72);
        var completed = country.CompletePowerPlantProject(_clock.CurrentTime);

        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.False(project.IsActive);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);

        // Funds are NOT deducted again on completion
        Assert.Equal(500_000.0, country.Treasury.Balance);

        // Subsequent completion calls are rejected (idempotent)
        var secondComplete = country.CompletePowerPlantProject(_clock.CurrentTime);
        Assert.False(secondComplete);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);
        Assert.Equal(500_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void ProgressReaching100Percent_DoesNotAutomaticallyCompleteProject_RequiresExplicitCompletion()
    {
        var country = CreateTestCountry("Explicit Action Republic", startingBalance: 1_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var project = country.StartPowerPlantProject(startTime);
        Assert.NotNull(project);

        // Advance past finish time (80 hours)
        _clock.AdvanceHours(80);

        // Progress is 100%, but project is NOT completed until player explicitly completes it
        var progress = country.GetPowerPlantProgress(_clock.CurrentTime);
        Assert.Equal(1.0, progress);
        Assert.False(project.Completed);
        Assert.True(project.IsActive);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);

        // Explicit completion request
        var completed = country.CompletePowerPlantProject(_clock.CurrentTime);
        Assert.True(completed);
        Assert.True(project.Completed);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);
    }

    [Fact]
    public void ExistingRoadProject_RemainsUnaffected_WhenPowerPlantIsActiveAndCompleted()
    {
        var country = CreateTestCountry("Multi Project Republic", startingBalance: 2_500_000.0, energyCapacity: 50.0);
        var startTime = _clock.CurrentTime;
        var initialInfra = country.StateCapacity.InfrastructureCondition;

        // Start Road (500k) and Power Plant (1,000k)
        var road = country.StartRoadProject(startTime);
        var plant = country.StartPowerPlantProject(startTime);

        Assert.NotNull(road);
        Assert.NotNull(plant);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);

        // Road finishes at 24 hours
        _clock.AdvanceHours(24);
        Assert.True(country.CompleteRoadProject(_clock.CurrentTime));
        Assert.True(road.Completed);
        Assert.False(plant.Completed);
        Assert.Equal((initialInfra ?? 0.0) + 0.1, country.StateCapacity.InfrastructureCondition!.Value, precision: 4);
        Assert.Equal(50.0, country.Yield.EnergyCapacity);

        // Power Plant finishes at 72 hours
        _clock.AdvanceHours(48);
        Assert.True(country.CompletePowerPlantProject(_clock.CurrentTime));
        Assert.True(plant.Completed);
        Assert.Equal(55.0, country.Yield.EnergyCapacity);
        Assert.Equal(1_000_000.0, country.Treasury.Balance);
    }
}
