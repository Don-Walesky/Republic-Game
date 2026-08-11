namespace Republic.Core.Tests.Bridge;

using Republic.Core.Decisions.Models;
using Republic.Core.Military.Models;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Models;
using Republic.Unity.Bridge;
using Xunit;

public sealed class RepublicUnityBridgeTests
{
    [Fact]
    public void OnEconomicIndicatorsUpdated_ForwardsEventToSubscribers()
    {
        var bridge = new RepublicUnityBridge();
        EconomicIndicator? receivedIndicators = null;

        bridge.EconomicIndicatorsUpdated += indicators => receivedIndicators = indicators;

        var sample = new EconomicIndicator
        {
            TreasuryBalance = 1500000.0,
            GrossDomesticProduct = 50000000.0
        };

        bridge.OnEconomicIndicatorsUpdated(sample);

        Assert.NotNull(receivedIndicators);
        Assert.Equal(1500000.0, receivedIndicators.TreasuryBalance);
        Assert.Equal(50000000.0, receivedIndicators.GrossDomesticProduct);
    }

    [Fact]
    public void OnMilitaryReadinessReportUpdated_ForwardsEventToSubscribers()
    {
        var bridge = new RepublicUnityBridge();
        MilitaryReadinessReport? receivedReport = null;

        bridge.MilitaryReadinessReportUpdated += report => receivedReport = report;

        var sampleReport = new MilitaryReadinessReport
        {
            Defcon = DefconLevel.Defcon3_AirForceStandby,
            TotalPersonnel = 31000,
            CompositeReadinessScore = 88.5
        };

        bridge.OnMilitaryReadinessReportUpdated(sampleReport);

        Assert.NotNull(receivedReport);
        Assert.Equal(DefconLevel.Defcon3_AirForceStandby, receivedReport.Defcon);
        Assert.Equal(31000, receivedReport.TotalPersonnel);
        Assert.Equal(88.5, receivedReport.CompositeReadinessScore);
    }

    [Fact]
    public void OnDefconLevelChanged_ForwardsPreviousAndNewLevel()
    {
        var bridge = new RepublicUnityBridge();
        DefconLevel prev = DefconLevel.Defcon5_Peace;
        DefconLevel next = DefconLevel.Defcon5_Peace;

        bridge.DefconLevelChanged += (p, n) =>
        {
            prev = p;
            next = n;
        };

        bridge.OnDefconLevelChanged(DefconLevel.Defcon5_Peace, DefconLevel.Defcon1_MaximumReadiness);

        Assert.Equal(DefconLevel.Defcon5_Peace, prev);
        Assert.Equal(DefconLevel.Defcon1_MaximumReadiness, next);
    }

    [Fact]
    public void OnMilitaryDirectiveExecuted_ForwardsResultDetails()
    {
        var bridge = new RepublicUnityBridge();
        MilitaryDirectiveResult? result = null;

        bridge.MilitaryDirectiveExecuted += res => result = res;

        var sampleResult = new MilitaryDirectiveResult
        {
            Success = true,
            OperationType = "CyberAttack",
            TargetCountry = "Valoria",
            CasualtiesSustained = 0
        };

        bridge.OnMilitaryDirectiveExecuted(sampleResult);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("CyberAttack", result.OperationType);
        Assert.Equal("Valoria", result.TargetCountry);
    }

    [Fact]
    public void OnDecisionPrompted_ForwardsDecisionContext()
    {
        var bridge = new RepublicUnityBridge();
        DecisionContext? prompted = null;

        bridge.DecisionPrompted += ctx => prompted = ctx;

        var context = new DecisionContext
        {
            Id = "dec-001",
            Title = "Economic Stimulus Bill",
            Category = "Economy"
        };

        bridge.OnDecisionPrompted(context);

        Assert.NotNull(prompted);
        Assert.Equal("dec-001", prompted.Id);
        Assert.Equal("Economic Stimulus Bill", prompted.Title);
    }
}
