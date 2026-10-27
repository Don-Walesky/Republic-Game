namespace Republic.Core.Government.Officials.Models;

using Republic.Core.Military.Models;

/// <summary>
/// Domain model encapsulating comprehensive national security leadership evaluation,
/// combining military readiness, DEFCON alert posture, domestic order, and external threat monitoring.
/// </summary>
public sealed class SecurityLeadershipAssessment
{
    public DefconLevel Defcon { get; set; } = DefconLevel.Defcon5_Peace;
    public double CompositeReadiness { get; set; } = 85.0;
    public double DomesticThreatIndex { get; set; } = 20.0;
    public double ExternalThreatIndex { get; set; } = 15.0;
    public string DefenseMinisterName { get; set; } = "Vacant";
    public string AssessmentSummary { get; set; } = "Sovereign frontiers secure. Peacetime readiness maintained.";
    public List<SecurityThreatReport> Threats { get; set; } = new();
}
