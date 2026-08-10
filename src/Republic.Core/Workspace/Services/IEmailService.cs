namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Service interface for executive email inbox management.
/// </summary>
public interface IEmailService
{
    void ReceiveEmail(EmailMessage email);
    bool MarkAsRead(string emailId);
    bool MoveToFolder(string emailId, string folder);
    IReadOnlyList<EmailMessage> GetInbox();
    IReadOnlyList<EmailMessage> GetFolder(string folder);
    int GetUnreadCount();
}
