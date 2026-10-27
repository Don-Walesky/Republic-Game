namespace Republic.Core.Government.Officials.Models;

using Republic.Core.Crises.Models;

/// <summary>
/// Intelligence confidence level reflecting the verification status of a reported threat.
/// </summary>
public enum IntelligenceConfidence
{
    UnverifiedRumor = 0,
    InterceptedSignal = 1,
    HighProbability = 2,
    Confirmed = 3
}

/// <summary>
/// Domain model representing a security or intelligence threat monitored by national security leadership.
/// </summary>
public sealed class SecurityThreatReport
{
    public string ThreatId { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "DomesticSecurity";
    public CrisisSeverity Severity { get; set; } = CrisisSeverity.Moderate;
    public IntelligenceConfidence Confidence { get; set; } = IntelligenceConfidence.HighProbability;
    public string SourceAgency { get; set; } = "National Intelligence Service";
    public string Status { get; set; } = "Active";
    public string Details { get; set; } = string.Empty;
    public string RecommendedResponse { get; set; } = string.Empty;
    public long ReportedDay { get; set; }
    public bool IsConfirmed => Confidence == IntelligenceConfidence.Confirmed;
}
