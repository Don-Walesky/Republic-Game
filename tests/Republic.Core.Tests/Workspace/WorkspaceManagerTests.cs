namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class WorkspaceManagerTests
{
    [Fact]
    public void WorkspaceManager_AggregatesAllWorkspaceChannels()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);

        var visitorService = new VisitorService(bus, logger);
        var phoneService = new PhoneService(bus, logger);
        var emailService = new EmailService(bus, logger);
        var newsService = new NewsService(bus, logger);
        var calendarService = new CalendarService(bus, logger);

        var manager = new WorkspaceManager(
            visitorService,
            phoneService,
            emailService,
            newsService,
            calendarService,
            bus,
            logger);

        visitorService.RegisterVisitor(new Visitor { Name = "Ambassador Lin" });
        phoneService.ReceiveCall(new PhoneCall { CallerName = "General Vance" });
        emailService.ReceiveEmail(new EmailMessage { Subject = "Briefing Memo" });
        newsService.PublishArticle(new NewsArticle { Headline = "Economic Report" });

        var state = manager.GetCurrentState();

        Assert.Single(state.Visitors);
        Assert.Single(state.PhoneCalls);
        Assert.Single(state.Emails);
        Assert.Single(state.NewsArticles);
        Assert.Equal("Executive Office", state.RoomState.ActiveRoomName);
        Assert.Equal("Day", state.RoomState.LightingMode);
    }

    [Fact]
    public void UpdateRoomState_ChangesRoomEnvironmentProperties()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);

        var manager = new WorkspaceManager(
            new VisitorService(bus, logger),
            new PhoneService(bus, logger),
            new EmailService(bus, logger),
            new NewsService(bus, logger),
            new CalendarService(bus, logger),
            bus,
            logger);

        manager.UpdateRoomState(roomName: "War Room", lightingMode: "Night", audioZone: "TacticalAmbience");
        var state = manager.GetCurrentState();

        Assert.Equal("War Room", state.RoomState.ActiveRoomName);
        Assert.Equal("Night", state.RoomState.LightingMode);
        Assert.Equal("TacticalAmbience", state.RoomState.AmbientAudioZone);
    }
}
