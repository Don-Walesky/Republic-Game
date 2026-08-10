namespace Republic.Core.Tests.Bridge;

using Republic.Core.Decisions.Models;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Models;
using Republic.Unity.Bridge;

public sealed class UnityBridgeTests
{
    [Fact]
    public void WorkspaceStateUpdated_FiresSubscribedUnityDelegate()
    {
        var bridge = new RepublicUnityBridge();
        WorkspaceState? receivedState = null;

        bridge.WorkspaceStateUpdated += state => receivedState = state;
        bridge.OnWorkspaceStateUpdated(new WorkspaceState());

        Assert.NotNull(receivedState);
    }

    [Fact]
    public void DecisionPrompted_FiresSubscribedUnityDelegate()
    {
        var bridge = new RepublicUnityBridge();
        DecisionContext? receivedDecision = null;

        bridge.DecisionPrompted += decision => receivedDecision = decision;
        bridge.OnDecisionPrompted(new DecisionContext { Title = "Unity Test Decision" });

        Assert.NotNull(receivedDecision);
        Assert.Equal("Unity Test Decision", receivedDecision.Title);
    }

    [Fact]
    public void CrisisTriggered_FiresSubscribedUnityDelegate()
    {
        var bridge = new RepublicUnityBridge();
        string? crisis = null;

        bridge.CrisisTriggered += (title, cat, sev) => crisis = title;
        bridge.OnCrisisTriggered("Severe Drought", "NaturalDisaster", "Severe");

        Assert.Equal("Severe Drought", crisis);
    }
}
