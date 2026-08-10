namespace Republic.Core.Workspace.Events;

using Republic.Core.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Event emitted when a visitor arrives at the workspace waiting room.
/// </summary>
public sealed record VisitorArrivedEvent(Visitor Visitor, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a visitor departs from the workspace.
/// </summary>
public sealed record VisitorDepartedEvent(string VisitorId, string Reason, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when an incoming phone call is received.
/// </summary>
public sealed record PhoneCallReceivedEvent(PhoneCall Call, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a phone call completes or is rejected.
/// </summary>
public sealed record PhoneCallEndedEvent(string CallId, bool Answered, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when an email is received.
/// </summary>
public sealed record EmailReceivedEvent(EmailMessage Email, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when an email is marked as read.
/// </summary>
public sealed record EmailReadEvent(string EmailId, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a news article is published.
/// </summary>
public sealed record NewsArticlePublishedEvent(NewsArticle Article, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a calendar appointment is scheduled.
/// </summary>
public sealed record AppointmentScheduledEvent(CalendarAppointment Appointment, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Event emitted when a calendar appointment begins.
/// </summary>
public sealed record AppointmentStartedEvent(string AppointmentId, DateTimeOffset OccurredAt) : IGameEvent;
