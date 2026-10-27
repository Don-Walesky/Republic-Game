namespace Republic.Core.Media.Models;

/// <summary>
/// Sentiment framing applied to a media report.
/// </summary>
public enum MediaReportSentiment
{
    /// <summary>
    /// Favorable coverage highlighting achievements, prosperity, or effective governance.
    /// </summary>
    Positive = 0,

    /// <summary>
    /// Factual, measured reporting without strong advocacy or condemnation.
    /// </summary>
    Neutral = 1,

    /// <summary>
    /// Critical reporting highlighting failures, bottlenecks, deficits, or unaddressed grievances.
    /// </summary>
    Critical = 2,

    /// <summary>
    /// Inflammatory or alarmist framing amplifying civic unrest or government incompetence.
    /// </summary>
    Sensationalized = 3
}

/// <summary>
/// Executive response status recorded for an individual published media report.
/// </summary>
public enum MediaResponseStatus
{
    /// <summary>
    /// The administration has taken no formal action regarding this report.
    /// </summary>
    Unanswered = 0,

    /// <summary>
    /// The administration issued an official clarifying statement or held a press conference.
    /// </summary>
    StatementIssued = 1,

    /// <summary>
    /// The administration attempted a covert payoff, but the outcome is pending or resolved.
    /// </summary>
    HushMoneyAttempted = 2,

    /// <summary>
    /// The outlet accepted covert hush money and quietly suppressed the story.
    /// </summary>
    HushMoneyAccepted = 3,

    /// <summary>
    /// The outlet refused or leaked the hush money bribe, creating an explosive scandal.
    /// </summary>
    HushMoneyExposed = 4,

    /// <summary>
    /// The administration imposed legal censorship or emergency injunctions on the report.
    /// </summary>
    Censored = 5
}

/// <summary>
/// Domain model representing a published or suppressed news report in Republic.
/// Connected directly to actual simulation events and country indicators.
/// </summary>
public sealed class MediaReport
{
    /// <summary>
    /// Gets the unique identifier of the report.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the publishing media outlet.
    /// </summary>
    public string OutletId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the publishing outlet.
    /// </summary>
    public string OutletName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the editorial leaning of the publishing outlet.
    /// </summary>
    public MediaEditorialLeaning Leaning { get; init; }

    /// <summary>
    /// Gets the primary headline of the story.
    /// </summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>
    /// Gets the journalistic article summary or lead paragraph.
    /// </summary>
    public string ArticleSummary { get; init; } = string.Empty;

    /// <summary>
    /// Gets the topical category (e.g. Economic, Governance, Infrastructure, Security, Scandal, Investment).
    /// </summary>
    public string Category { get; init; } = "Governance";

    /// <summary>
    /// Gets the sentiment framing of the report.
    /// </summary>
    public MediaReportSentiment Sentiment { get; init; }

    /// <summary>
    /// Gets the underlying simulation event or state trigger that prompted this article.
    /// </summary>
    public string EventSource { get; init; } = string.Empty;

    /// <summary>
    /// Gets the outlet's credibility rating at the time of publishing.
    /// </summary>
    public double OutletCredibility { get; init; }

    /// <summary>
    /// Gets the outlet's audience reach at the time of publishing.
    /// </summary>
    public double OutletReach { get; init; }

    /// <summary>
    /// Gets the simulation day on which this article appeared.
    /// </summary>
    public long PublishedDay { get; init; }

    /// <summary>
    /// Gets or sets the calculated impact on presidential approval rating.
    /// </summary>
    public double PublicApprovalImpact { get; set; }

    /// <summary>
    /// Gets or sets the calculated impact on national stability rating.
    /// </summary>
    public double StabilityImpact { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether distribution was suppressed through bribes or censorship.
    /// </summary>
    public bool IsSuppressed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this report is circulating via underground samizdat networks.
    /// </summary>
    public bool IsUndergroundLeak { get; set; }

    /// <summary>
    /// Gets or sets the executive response status recorded for this story.
    /// </summary>
    public MediaResponseStatus ResponseStatus { get; set; } = MediaResponseStatus.Unanswered;
}
