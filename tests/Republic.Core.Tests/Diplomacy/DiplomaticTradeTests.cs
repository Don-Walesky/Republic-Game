namespace Republic.Core.Tests.Diplomacy;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diplomacy.Models;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;

public class DiplomaticTradeTests
{
    [Fact]
    public void EvaluateTradeAgreement_Calculates_Tariff_Savings_And_GDP_Boost()
    {
        var engine = new DiplomaticTradeEngine();
        double sourceGdp = 500_000_000.0;
        double targetGdp = 300_000_000.0;

        var result = engine.EvaluateTradeAgreement(sourceGdp, targetGdp, 15.0, 75.0, 80.0);

        Assert.True(result.IsFavorable);
        Assert.True(result.ExpectedTariffSavings > 0);
        Assert.True(result.MutualGdpBoostPercent > 0);
    }

    [Fact]
    public async Task Propose_And_Accept_Trade_Treaty_Updates_Status_To_Friendly()
    {
        var eventBus = new EventBus(new EventBusOptions(), new TestLogger());
        var service = new DiplomacyService(eventBus);

        var treaty = await service.ProposeTreatyAsync("Arcadia", "Vanguard", TreatyType.TradeAgreement, "Arcadia-Vanguard Free Trade Pact");
        Assert.False(treaty.IsActive);

        bool ratified = await service.AcceptTreatyAsync(treaty.Id);
        Assert.True(ratified);
        Assert.True(treaty.IsActive);

        var relation = service.GetRelation("Arcadia", "Vanguard");
        Assert.Equal(DiplomaticStatus.Friendly, relation.Status);
    }
}
