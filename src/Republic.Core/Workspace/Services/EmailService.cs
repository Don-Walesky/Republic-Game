namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Service implementing email reception, read tracking, and folder organization.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly List<EmailMessage> _emails = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public EmailService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ReceiveEmail(EmailMessage email)
    {
        ArgumentNullException.ThrowIfNull(email);
        lock (_lock)
        {
            _emails.Add(email);
        }

        _logger.LogInfo($"Received email from {email.Sender}: '{email.Subject}'");
        _eventBus.PublishAsync(new EmailReceivedEvent(email, DateTimeOffset.UtcNow));
    }

    public bool MarkAsRead(string emailId)
    {
        lock (_lock)
        {
            var email = _emails.FirstOrDefault(e => e.Id == emailId);
            if (email == null || email.IsRead)
            {
                return false;
            }

            email.IsRead = true;
            _logger.LogInfo($"Marked email '{email.Subject}' as read.");
            _eventBus.PublishAsync(new EmailReadEvent(email.Id, DateTimeOffset.UtcNow));
            return true;
        }
    }

    public bool RespondToEmail(string emailId, string selectedOptionId)
    {
        lock (_lock)
        {
            var email = _emails.FirstOrDefault(e => e.Id == emailId);
            if (email == null)
            {
                return false;
            }

            email.IsRead = true;
            _logger.LogInfo($"Responded to email '{email.Subject}' with option '{selectedOptionId}'.");
            return true;
        }
    }

    public bool MoveToFolder(string emailId, string folder)
    {
        lock (_lock)
        {
            var email = _emails.FirstOrDefault(e => e.Id == emailId);
            if (email == null)
            {
                return false;
            }

            email.Folder = folder;
            _logger.LogInfo($"Moved email '{email.Subject}' to folder '{folder}'.");
            return true;
        }
    }

    public IReadOnlyList<EmailMessage> GetInbox()
    {
        return GetFolder("Inbox");
    }

    public IReadOnlyList<EmailMessage> GetFolder(string folder)
    {
        lock (_lock)
        {
            return _emails.Where(e => e.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }
    }

    public int GetUnreadCount()
    {
        lock (_lock)
        {
            return _emails.Count(e => !e.IsRead && e.Folder.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
        }
    }
}
