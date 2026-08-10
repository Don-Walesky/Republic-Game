namespace Republic.Core.Workspace.Models;

/// <summary>
/// Represents an email message received or sent in the executive workspace.
/// </summary>
public sealed class EmailMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Sender { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime DateSent { get; init; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public string Folder { get; set; } = "Inbox";
    public bool ActionRequired { get; init; }
}
