namespace Republic.Unity.Bridge;

using Republic.Core.Military.Models;

/// <summary>
/// Bridge interface exposing military readiness reports, DEFCON alerts, and directive outcomes to Unity UI views.
/// </summary>
public interface IMilitaryPresenter
{
    void OnMilitaryReadinessReportUpdated(MilitaryReadinessReport report);
    void OnDefconLevelChanged(DefconLevel previousLevel, DefconLevel newLevel);
    void OnMilitaryDirectiveExecuted(MilitaryDirectiveResult result);
}
