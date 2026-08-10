namespace Republic.Core.Tests.Legislature;

using Republic.Core.Events;
using Republic.Core.Legislature.Models;
using Republic.Core.Legislature.Services;
using Republic.Core.Workspace.Services;

public sealed class LegislatureServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorkspaceManager _workspace;

    public LegislatureServiceTests()
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
    public async Task IntroduceBill_AndVote_PassesWithCoalitionMajority()
    {
        var service = new LegislatureService(_eventBus, _workspace, _logger);
        service.RegisterParty(new PoliticalParty { Name = "Reform Movement", SeatCount = 60, IsGovernmentCoalition = true });
        service.RegisterParty(new PoliticalParty { Name = "Traditionalists", SeatCount = 40, IsGovernmentCoalition = false });

        var bill = await service.IntroduceBillAsync("National Infrastructure Reform", "Allocates federal grants to regional transport.");
        var result = await service.VoteOnBillAsync(bill.Id);

        Assert.True(result.Passed);
        Assert.True(result.AyesCount > 50);
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }

    [Fact]
    public async Task ExerciseExecutiveVeto_OverridesPassedBill()
    {
        var service = new LegislatureService(_eventBus, _workspace, _logger);
        service.RegisterParty(new PoliticalParty { Name = "Governing Party", SeatCount = 70, IsGovernmentCoalition = true });

        var bill = await service.IntroduceBillAsync("Tax Reform Act", "Lowers corporate tax rate.");
        await service.VoteOnBillAsync(bill.Id);

        var vetoed = await service.ExerciseExecutiveVetoAsync(bill.Id);

        Assert.True(vetoed);
        Assert.True(bill.IsVetoed);
        Assert.False(bill.IsPassed);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
