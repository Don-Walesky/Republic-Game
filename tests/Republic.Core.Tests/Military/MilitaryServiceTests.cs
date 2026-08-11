namespace Republic.Core.Tests.Military;

using System;
using System.Threading.Tasks;
using Republic.Core.Events;
using Republic.Core.Government;
using Republic.Core.Military.Events;
using Republic.Core.Military.Models;
using Republic.Core.Military.Services;
using Xunit;

public sealed class MilitaryServiceTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());

    [Fact]
    public void GetReadinessReport_ReturnsCorrectAggregatesAndBranchScores()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 1000000m
        };
        state.Military.Personnel = 25000;
        state.Military.WeaponsInventory = 500;

        var report = service.GetReadinessReport(state);

        Assert.NotNull(report);
        Assert.Equal(DefconLevel.Defcon5_Peace, report.Defcon);
        Assert.Equal(25000, report.TotalPersonnel);
        Assert.Equal(500, report.TotalEquipment);
        Assert.True(report.CompositeReadinessScore > 0);
        Assert.Equal(4, report.BranchBreakdown.Count);
    }

    [Fact]
    public async Task SetDefconLevelAsync_UpdatesAlertStateAndPublishesEvent()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState { CountryName = "Arcadia" };
        bool eventPublished = false;

        _eventBus.Subscribe<DefconLevelChangedEvent>((e, _) =>
        {
            if (e.NewLevel == DefconLevel.Defcon2_ArmedForcesMobilized)
            {
                eventPublished = true;
            }
            return ValueTask.CompletedTask;
        });

        var level = await service.SetDefconLevelAsync(state, DefconLevel.Defcon2_ArmedForcesMobilized);
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.Equal(DefconLevel.Defcon2_ArmedForcesMobilized, level);
        Assert.True(eventPublished);
    }

    [Fact]
    public async Task RecruitBranchPersonnelAsync_DeductsTreasuryAndIncreasesPersonnel()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 100000m
        };
        state.Military.Personnel = 1000;

        bool result = await service.RecruitBranchPersonnelAsync(state, MilitaryBranch.Army, 200, 100m);

        Assert.True(result);
        Assert.Equal(80000m, state.TreasuryBalance);
        Assert.Equal(1200, state.Military.Personnel);
    }

    [Fact]
    public async Task RecruitBranchPersonnelAsync_ReturnsFalse_WhenTreasuryInsufficient()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 500m
        };

        bool result = await service.RecruitBranchPersonnelAsync(state, MilitaryBranch.Army, 10, 100m);

        Assert.False(result);
        Assert.Equal(500m, state.TreasuryBalance);
    }

    [Fact]
    public async Task ProcureBranchEquipmentAsync_DeductsTreasuryAndIncreasesInventory()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 50000m
        };
        state.Military.WeaponsInventory = 50;

        bool result = await service.ProcureBranchEquipmentAsync(state, MilitaryBranch.AirForce, 20, 1000m);

        Assert.True(result);
        Assert.Equal(30000m, state.TreasuryBalance);
        Assert.Equal(70, state.Military.WeaponsInventory);
    }

    [Fact]
    public async Task ExecuteDirectiveAsync_Fails_WhenTroopCommitmentExceedsAvailable()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 1000000m
        };
        state.Military.Personnel = 500;

        var result = await service.ExecuteDirectiveAsync(state, "Valoria", MilitaryOpType.Invasion, 1000000);

        Assert.False(result.Success);
        Assert.Contains("exceeds available personnel", result.Message);
    }

    [Fact]
    public async Task ExecuteDirectiveAsync_ResolvesOperationAndDeductsCost()
    {
        var service = new MilitaryService(_eventBus);
        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 1000000m
        };
        state.Military.Personnel = 10000;
        state.Military.WeaponsInventory = 200;

        var result = await service.ExecuteDirectiveAsync(state, "Valoria", MilitaryOpType.Airstrike, 2000);

        Assert.Equal("Airstrike", result.OperationType);
        Assert.Equal("Valoria", result.TargetCountry);
        Assert.Equal(800000m, state.TreasuryBalance); // 1,000,000 - (2000 * 100)
        Assert.Single(state.Military.OperationHistory);
    }
}
