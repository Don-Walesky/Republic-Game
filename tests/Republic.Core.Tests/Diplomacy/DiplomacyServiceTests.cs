namespace Republic.Core.Tests.Diplomacy;

using Republic.Core.Diplomacy.Models;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;
using Republic.Core.Workspace.Services;

public sealed class DiplomacyServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorkspaceManager _workspace;

    public DiplomacyServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
    }

    [Fact]
    public async Task ProposeAndAcceptTreaty_UpgradesStatusToAllied()
    {
        var service = new DiplomacyService(_eventBus, _workspace, _logger);

        var treaty = await service.ProposeTreatyAsync("Aethel", "Norse", TreatyType.MilitaryAlliance, "Northern Defense Pact");
        Assert.NotNull(treaty);
        Assert.False(treaty.IsActive);
        Assert.NotEmpty(_workspace.Email.GetInbox());

        var accepted = await service.AcceptTreatyAsync(treaty.Id);
        Assert.True(accepted);
        Assert.True(treaty.IsActive);

        var relation = service.GetRelation("Aethel", "Norse");
        Assert.Equal(DiplomaticStatus.Allied, relation.Status);
    }

    [Fact]
    public async Task BreakTreaty_SetsStatusToHostile_AndPenalizesReputation()
    {
        var service = new DiplomacyService(_eventBus, _workspace, _logger);
        var treaty = await service.ProposeTreatyAsync("Aethel", "Norse", TreatyType.TradeAgreement, "Pact of Iron");
        await service.AcceptTreatyAsync(treaty.Id);

        var broken = await service.BreakTreatyAsync(treaty.Id, "Aethel");

        Assert.True(broken);
        Assert.False(treaty.IsActive);

        var relation = service.GetRelation("Aethel", "Norse");
        Assert.Equal(DiplomaticStatus.Hostile, relation.Status);
        Assert.True(relation.ReputationScore < 50.0);
    }
}
