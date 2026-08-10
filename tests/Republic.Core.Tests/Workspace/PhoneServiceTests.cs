namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class PhoneServiceTests
{
    [Fact]
    public async Task ReceiveCall_PublishesPhoneCallReceivedEvent()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var phoneService = new PhoneService(bus, logger);

        PhoneCallReceivedEvent? receivedEvent = null;
        bus.Subscribe<PhoneCallReceivedEvent>((e, _) =>
        {
            receivedEvent = e;
            return ValueTask.CompletedTask;
        });

        var call = new PhoneCall
        {
            CallerName = "General Marcus Cole",
            Organization = "Joint Chiefs",
            Urgency = CallUrgency.Emergency,
            Subject = "Border Sector Readiness"
        };

        phoneService.ReceiveCall(call);
        await bus.ProcessQueuedEventsAsync();

        Assert.NotNull(receivedEvent);
        Assert.Equal("General Marcus Cole", receivedEvent.Call.CallerName);
    }

    [Fact]
    public async Task AnswerCall_MarksCallAsAnsweredAndEnded()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var phoneService = new PhoneService(bus, logger);

        PhoneCallEndedEvent? endedEvent = null;
        bus.Subscribe<PhoneCallEndedEvent>((e, _) =>
        {
            endedEvent = e;
            return ValueTask.CompletedTask;
        });

        var call = new PhoneCall { CallerName = "Diplomat Sarah Jenkins" };
        phoneService.ReceiveCall(call);

        var answered = phoneService.AnswerCall(call.Id);
        await bus.ProcessQueuedEventsAsync();

        Assert.True(answered);
        Assert.True(call.IsAnswered);
        Assert.False(call.IsActive);
        Assert.NotNull(endedEvent);
        Assert.True(endedEvent.Answered);
    }
}
