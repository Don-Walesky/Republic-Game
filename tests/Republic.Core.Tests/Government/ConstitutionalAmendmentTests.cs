namespace Republic.Core.Tests.Government;

using Republic.Core.Events;
using Republic.Core.Government.Events;
using Republic.Core.Government.Models;
using Republic.Core.Government.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class ConstitutionalAmendmentTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly GovernmentReformService _reformService;

    public ConstitutionalAmendmentTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Government Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _reformService = new GovernmentReformService(_world, _eventBus, _workspace, _logger);
    }

    [Fact]
    public async Task ProposeConstitutionalAmendment_AddsAmendmentToRegistry()
    {
        var amendment = new ConstitutionalAmendment
        {
            Title = "Bill of Digital Rights",
            Description = "Establishes constitutional protections for digital privacy.",
            TargetLawCategory = "CivilLiberties",
            SupermajorityRatioRequired = 0.66
        };

        var proposed = await _reformService.ProposeConstitutionalAmendmentAsync(amendment);

        Assert.Equal(ConstitutionalAmendmentStatus.Proposed, proposed.Status);
        Assert.Single(_reformService.GetConstitutionalAmendments());
        Assert.NotEmpty(_workspace.News.GetNewsFeed());
    }

    [Fact]
    public async Task VoteOnConstitutionalAmendment_WithSupermajority_EnactsAmendmentAndPublishesEvent()
    {
        var amendment = new ConstitutionalAmendment
        {
            Title = "Judicial Independence Charter",
            Description = "Requires two-thirds Senate approval for supreme court appointments.",
            SupermajorityRatioRequired = 0.66
        };
        await _reformService.ProposeConstitutionalAmendmentAsync(amendment);

        ConstitutionalAmendmentEnactedEvent? enactedEvent = null;
        _eventBus.Subscribe<ConstitutionalAmendmentEnactedEvent>((evt, ct) =>
        {
            enactedEvent = evt;
            return ValueTask.CompletedTask;
        });

        // 70 votes out of 100 = 70% >= 66% threshold
        bool passed = await _reformService.VoteOnConstitutionalAmendmentAsync(amendment.Id, 70, 100);
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.True(passed);
        Assert.Equal(ConstitutionalAmendmentStatus.Enacted, amendment.Status);
        Assert.NotNull(enactedEvent);
        Assert.Equal("Judicial Independence Charter", enactedEvent.Amendment.Title);
    }

    [Fact]
    public async Task VoteOnConstitutionalAmendment_FailingThreshold_RejectsAmendment()
    {
        var amendment = new ConstitutionalAmendment
        {
            Title = "Executive Term Limit Expansion",
            SupermajorityRatioRequired = 0.66
        };
        await _reformService.ProposeConstitutionalAmendmentAsync(amendment);

        // 50 votes out of 100 = 50% < 66% threshold
        bool passed = await _reformService.VoteOnConstitutionalAmendmentAsync(amendment.Id, 50, 100);

        Assert.False(passed);
        Assert.Equal(ConstitutionalAmendmentStatus.Rejected, amendment.Status);
    }
}
