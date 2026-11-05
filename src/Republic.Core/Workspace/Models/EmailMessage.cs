namespace Republic.Core.Workspace.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an in-game communication, executive dispatch, legislative intelligence memo,
/// or diplomatic cable received or sent in the sovereign workspace.
/// </summary>
public sealed class EmailMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Sender { get; init; } = string.Empty;
    public string SenderTitle { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime DateSent { get; init; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public string Folder { get; set; } = "Inbox";
    public string Channel { get; init; } = "Executive"; // Legislative, Diplomatic, Executive, Cabinet, Security, Media
    public string Urgency { get; init; } = "Routine"; // Routine, Priority, Confidential, Urgent
    public bool ActionRequired { get; init; }
    public string ActionType { get; init; } = string.Empty; // e.g. "view_chamber", "review_bilateral_deal", "view_bill"
    public string RelatedEntityId { get; init; } = string.Empty; // e.g. proposal_id, lawmaker_id, bill_id
    public string AvatarSymbol { get; init; } = "✉️";
    public List<string> Options { get; init; } = new();
}
