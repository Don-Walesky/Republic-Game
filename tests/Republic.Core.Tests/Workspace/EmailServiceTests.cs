namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class EmailServiceTests
{
    [Fact]
    public async Task ReceiveEmail_IncrementsUnreadCountAndPublishesEvent()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var emailService = new EmailService(bus, logger);

        EmailReceivedEvent? receivedEvent = null;
        bus.Subscribe<EmailReceivedEvent>((e, _) =>
        {
            receivedEvent = e;
            return ValueTask.CompletedTask;
        });

        var email = new EmailMessage
        {
            Sender = "treasury@republic.gov",
            Subject = "Weekly Revenue Report",
            Body = "Tax collection up by 3.2%."
        };

        emailService.ReceiveEmail(email);
        await bus.ProcessQueuedEventsAsync();

        Assert.Equal(1, emailService.GetUnreadCount());
        Assert.NotNull(receivedEvent);
        Assert.Equal("Weekly Revenue Report", receivedEvent.Email.Subject);
    }

    [Fact]
    public void MarkAsRead_DecrementsUnreadCount()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var emailService = new EmailService(bus, logger);

        var email = new EmailMessage { Sender = "press@republic.gov", Subject = "Briefing Notes" };
        emailService.ReceiveEmail(email);

        var marked = emailService.MarkAsRead(email.Id);

        Assert.True(marked);
        Assert.True(email.IsRead);
        Assert.Equal(0, emailService.GetUnreadCount());
    }
}
