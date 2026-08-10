namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class VisitorServiceTests
{
    [Fact]
    public async Task RegisterVisitor_PublishesVisitorArrivedEvent()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var visitorService = new VisitorService(bus, logger);

        VisitorArrivedEvent? publishedEvent = null;
        bus.Subscribe<VisitorArrivedEvent>((e, _) =>
        {
            publishedEvent = e;
            return ValueTask.CompletedTask;
        });

        var visitor = new Visitor
        {
            Name = "Senator Jane Doe",
            Title = "Chair of Armed Services",
            Faction = "National Security Committee",
            Purpose = "Defense Appropriation"
        };

        visitorService.RegisterVisitor(visitor);
        await bus.ProcessQueuedEventsAsync();

        Assert.NotNull(publishedEvent);
        Assert.Equal("Senator Jane Doe", publishedEvent.Visitor.Name);
        Assert.Equal(VisitorStatus.Waiting, publishedEvent.Visitor.Status);
    }

    [Fact]
    public void StartMeeting_TransitionsVisitorStatusToInMeeting()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var visitorService = new VisitorService(bus, logger);

        var visitor = new Visitor
        {
            Name = "Governor Carl Vance",
            Title = "Regional Governor",
            Purpose = "Disaster Relief Funding"
        };

        visitorService.RegisterVisitor(visitor);
        var next = visitorService.GetNextVisitor();
        Assert.NotNull(next);

        var started = visitorService.StartMeeting(next.Id);

        Assert.True(started);
        Assert.Equal(VisitorStatus.InMeeting, next.Status);
    }

    [Fact]
    public async Task DismissVisitor_PublishesVisitorDepartedEvent()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var visitorService = new VisitorService(bus, logger);

        VisitorDepartedEvent? departedEvent = null;
        bus.Subscribe<VisitorDepartedEvent>((e, _) =>
        {
            departedEvent = e;
            return ValueTask.CompletedTask;
        });

        var visitor = new Visitor { Name = "Lobbyist John Smith" };
        visitorService.RegisterVisitor(visitor);

        var dismissed = visitorService.DismissVisitor(visitor.Id);
        await bus.ProcessQueuedEventsAsync();

        Assert.True(dismissed);
        Assert.NotNull(departedEvent);
        Assert.Equal(visitor.Id, departedEvent.VisitorId);
    }
}
