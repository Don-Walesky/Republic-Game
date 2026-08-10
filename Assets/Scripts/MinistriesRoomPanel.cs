using UnityEngine;
using UnityEngine.UI;

public class MinistriesRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Ministerial Offices";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 17,
            new Vector2(0.04f, 0.73f), new Vector2(0.96f, 0.88f));

        var stats = UiHelper.Label(root, string.Empty, 18,
            new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.72f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.56f), spacing: 12f);

        UiHelper.ActionButton(col.transform, "Senate Review: Trade Minister (75 pts)", () =>
        {
            try { Office.Bridge.ReviewAndAssignCabinetMember("Ariana Vale", "Trade", 75); Office.SyncFromBridge("Ariana Vale confirmed as Trade Minister."); RefreshStats(stats); Feedback("Senate confirmed: Ariana Vale — Trade."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Issue Task: Expand Trade Corridors ($180,000)", () =>
        {
            try { Office.Bridge.IssueMinisterialTask("Ariana Vale", "Expand trade corridors", 180000m); Office.SyncFromBridge("Task issued to Ariana Vale."); RefreshStats(stats); Feedback("Task issued; execution cost negotiated down."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Senate Review: Defense Minister (80 pts)", () =>
        {
            try { Office.Bridge.ReviewAndAssignCabinetMember("Jonas Reed", "Defense", 80); Office.SyncFromBridge("Jonas Reed confirmed as Defense Minister."); RefreshStats(stats); Feedback("Senate confirmed: Jonas Reed — Defense."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Issue Task: Modernize Defense Infrastructure ($200,000)", () =>
        {
            try { Office.Bridge.IssueMinisterialTask("Jonas Reed", "Modernize defense infrastructure", 200000m); Office.SyncFromBridge("Task issued to Jonas Reed."); RefreshStats(stats); Feedback("Task issued; execution cost negotiated."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Approve Trade Through Ministry ($50,000)", () =>
        {
            try { Office.Bridge.ExecuteTradeThroughMinister("Ariana Vale", "Lys", 50000m, 1.10m); Office.SyncFromBridge("Trade executed through Ministry of Trade."); RefreshStats(stats); Feedback("Trade with Lys approved."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Enter each minister's office to assign tasks, negotiate execution costs, and approve ministerial policy actions. Appointments require senate confirmation.";

    private void RefreshStats(Text label)
    {
        label.text = $"Cabinet: {Office.State.CabinetCount} ministers   |   Tasks issued: {Office.State.MinisterialTaskCount}" +
                     $"   |   Approval: {Office.State.ApprovalRating:0.0}%";
    }
}
