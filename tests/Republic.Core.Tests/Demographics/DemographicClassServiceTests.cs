namespace Republic.Core.Tests.Demographics;

using System.Linq;
using Xunit;
using Republic.Core.Demographics.Classes.Models;
using Republic.Core.Demographics.Classes.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.World.Services;

public class DemographicClassServiceTests
{
    [Fact]
    public void InitializeClasses_Registers_All_DemographicClasses()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new DemographicClassService(worldManager, eventBus);

        var approvals = service.GetClassApprovals();

        Assert.NotEmpty(approvals);
        Assert.Contains(approvals, a => a.ClassType == DemographicClass.WorkingClass);
        Assert.Contains(approvals, a => a.ClassType == DemographicClass.Oligarchs);
        Assert.Contains(approvals, a => a.ClassType == DemographicClass.MilitaryStaff);
    }

    [Fact]
    public void AdjustClassApproval_Clamps_And_Updates_Rating()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new DemographicClassService(worldManager, eventBus);

        service.AdjustClassApproval(DemographicClass.WorkingClass, -20.0);
        var target = service.GetClassApprovals().First(a => a.ClassType == DemographicClass.WorkingClass);

        Assert.Equal(55.0, target.ApprovalRating);
        Assert.True(target.RebellionRiskIndex > 0);
    }

    [Fact]
    public void WeightedOverallApproval_Calculates_Sum_Correctly()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new DemographicClassService(worldManager, eventBus);

        double overall = service.GetWeightedOverallApproval();

        Assert.True(overall > 0.0);
    }
}
