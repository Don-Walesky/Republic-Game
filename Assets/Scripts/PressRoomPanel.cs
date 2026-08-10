using UnityEngine;
using UnityEngine.UI;

public class PressRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Press Secretary Office";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 17,
            new Vector2(0.04f, 0.73f), new Vector2(0.96f, 0.88f));

        var stats = UiHelper.Label(root, string.Empty, 18,
            new Vector2(0.04f, 0.59f), new Vector2(0.96f, 0.72f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.57f), spacing: 12f);

        UiHelper.ActionButton(col.transform, "Brief: Economic Recovery Plan (Boost +6)", () =>
        {
            try { Office.Bridge.HoldPressBriefing("Economic recovery plan announced", 6); Office.SyncFromBridge("Press briefed on economic recovery."); RefreshStats(stats); Feedback("Approval boosted by economic messaging."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Brief: National Security Address (Boost +8)", () =>
        {
            try { Office.Bridge.HoldPressBriefing("National security address", 8); Office.SyncFromBridge("Security address delivered to national press."); RefreshStats(stats); Feedback("Public confidence in security improved."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Brief: Education & Jobs Initiative (Boost +5)", () =>
        {
            try { Office.Bridge.HoldPressBriefing("Education and jobs initiative unveiled", 5); Office.SyncFromBridge("Education initiative announced to press."); RefreshStats(stats); Feedback("Education and HDI narrative strengthened."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Brief: Infrastructure Milestone (Boost +7)", () =>
        {
            try { Office.Bridge.HoldPressBriefing("Infrastructure milestone celebrated", 7); Office.SyncFromBridge("Infrastructure milestone press conference held."); RefreshStats(stats); Feedback("Infrastructure message boosted immigration and approval."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Your press secretary shapes public perception. Each briefing targets a theme — economy, security, education, or infrastructure — and improves approval ratings while drawing skilled immigrants and boosting development indices.";

    private void RefreshStats(Text label)
    {
        label.text = $"Approval: {Office.State.ApprovalRating:0.0}%   |   Population: {Office.State.Population:N0}" +
                     $"   |   HDI: {Office.State.Hdi:0.000}   |   Infrastructure: {Office.State.InfrastructureIndex:0.0}";
    }
}
