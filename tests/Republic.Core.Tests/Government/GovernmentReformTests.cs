namespace Republic.Core.Tests.Government;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Government.Models;
using Republic.Core.Government.Services;
using Republic.Core.World;

public class GovernmentReformTests
{
    [Fact]
    public async Task VoteOnConstitutionalAmendment_Enacts_When_Supermajority_Ratio_Met()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new GovernmentReformService(worldManager, eventBus);

        var amendment = new ConstitutionalAmendment
        {
            Title = "Executive Term Limit Reform",
            Description = "Enforces two-term presidency limit",
            SupermajorityRatioRequired = 0.66
        };

        await service.ProposeConstitutionalAmendmentAsync(amendment);

        // 70 / 100 = 0.70 >= 0.66 -> Passed
        bool passed = await service.VoteOnConstitutionalAmendmentAsync(amendment.Id, 70, 100);

        Assert.True(passed);
        Assert.Equal(ConstitutionalAmendmentStatus.Enacted, amendment.Status);
        Assert.NotNull(amendment.EnactedAt);
    }

    [Fact]
    public async Task VoteOnConstitutionalAmendment_Rejects_When_Supermajority_Not_Met()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var worldManager = new WorldManager(eventBus);
        var service = new GovernmentReformService(worldManager, eventBus);

        var amendment = new ConstitutionalAmendment
        {
            Title = "Judicial Veto Amendment",
            Description = "Grants Supreme Court binding veto over assembly decrees",
            SupermajorityRatioRequired = 0.66
        };

        await service.ProposeConstitutionalAmendmentAsync(amendment);

        // 60 / 100 = 0.60 < 0.66 -> Rejected
        bool passed = await service.VoteOnConstitutionalAmendmentAsync(amendment.Id, 60, 100);

        Assert.False(passed);
        Assert.Equal(ConstitutionalAmendmentStatus.Rejected, amendment.Status);
    }
}
