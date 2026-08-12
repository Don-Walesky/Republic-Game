namespace Republic.Core.Tests.Elections;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diagnostics;
using Republic.Core.Elections.Services;
using Republic.Core.Events;
using Republic.Core.World;

public class ParliamentaryElectionTests
{
    [Fact]
    public async Task SimulateWeeklyPollingShifts_Updates_Incumbent_Approval_During_Campaign()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new ElectionService(worldManager, eventBus);

        await service.StartCampaignSeasonAsync();
        Assert.True(service.IsCampaignActive);

        var pollingBefore = service.GetCurrentPollingData();
        var pollingAfter = await service.SimulateWeeklyPollingShiftsAsync();

        Assert.NotEqual(pollingBefore.IncumbentApprovalPercentage, pollingAfter.IncumbentApprovalPercentage);
    }

    [Fact]
    public async Task ConductElection_Produces_Valid_ElectionResult()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new ElectionService(worldManager, eventBus);

        await service.StartCampaignSeasonAsync();
        var result = await service.ConductElectionAsync("President Vance", "Senator Sterling");

        Assert.NotNull(result);
        Assert.False(service.IsCampaignActive);
        Assert.True(result.TotalTurnoutPercentage > 0.0);
    }
}
