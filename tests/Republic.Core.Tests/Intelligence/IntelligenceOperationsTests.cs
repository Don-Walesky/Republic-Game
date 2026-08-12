namespace Republic.Core.Tests.Intelligence;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;

public class IntelligenceOperationsTests
{
    [Fact]
    public async Task InfiltrateTarget_Increases_InfiltrationLevel()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new IntelligenceService(worldManager, eventBus);

        var network = await service.InfiltrateTargetAsync("country-alpha", 3);

        Assert.NotNull(network);
        Assert.Equal(55.0, network.InfiltrationLevel); // 25.0 default + 30.0 (3 * 10%)
        Assert.Equal(6, network.AssignedAgentsCount);   // 3 default + 3
    }

    [Fact]
    public async Task LaunchOperation_Succeeds_When_Infiltration_Threshold_Met()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        worldManager.Economic.DepositTreasury(100_000_000.0);
        var service = new IntelligenceService(worldManager, eventBus);

        // Raise infiltration level to >= 40% (25% default + 2 agents * 10% = 45%)
        await service.InfiltrateTargetAsync("country-beta", 2);

        var op = await service.LaunchOperationAsync(CovertOperationType.AssetExtraction, "country-beta", "Operation Blackout");

        Assert.True(op.IsCompleted);
        Assert.False(op.IsExposed);
    }

    [Fact]
    public async Task ConductCounterEspionageSweep_Deducts_Treasury_And_Logs()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new IntelligenceService(worldManager, eventBus);

        double initialTreasury = worldManager.Economic.GetIndicators().TreasuryBalance;
        bool success = await service.ConductCounterEspionageSweepAsync();

        Assert.True(success);
        Assert.Equal(initialTreasury - 15_000_000.0, worldManager.Economic.GetIndicators().TreasuryBalance);
    }
}
