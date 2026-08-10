using UnityEngine;
using UnityEngine.UI;

public class LegislatureRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Legislative Chamber";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 17,
            new Vector2(0.04f, 0.73f), new Vector2(0.96f, 0.88f));

        var stats = UiHelper.Label(root, string.Empty, 18,
            new Vector2(0.04f, 0.59f), new Vector2(0.96f, 0.72f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.57f), spacing: 12f);

        UiHelper.ActionButton(col.transform, "Sponsor Education Reform Bill ($250,000)", () =>
        {
            try
            {
                var bill = (dynamic)Office.Bridge.SponsorBill("National Education Reform", "Sera Holt", 250000m);
                Office.SyncFromBridge($"Bill '{bill.Title}' sponsored.");
                RefreshStats(stats);
                Feedback($"Bill drafted — status: {bill.Status}");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Sponsor Infrastructure Development Bill ($400,000)", () =>
        {
            try
            {
                var bill = (dynamic)Office.Bridge.SponsorBill("National Infrastructure Plan", "Marcus Obi", 400000m);
                Office.SyncFromBridge($"Bill '{bill.Title}' sponsored.");
                RefreshStats(stats);
                Feedback($"Bill drafted — status: {bill.Status}");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Negotiate Latest Bill Down ($180,000, Score 85)", () =>
        {
            try
            {
                var snap = (dynamic)Office.Bridge.GetSnapshot();
                if (snap.LastBillId == System.Guid.Empty) { Feedback("No bill to negotiate."); return; }
                Office.Bridge.NegotiateBill(snap.LastBillId, 180000m, 85);
                Office.SyncFromBridge("Bill terms negotiated with the majority.");
                RefreshStats(stats);
                Feedback($"Bill negotiated — new status: {Office.State.LastBillStatus}");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Meet Loyalists — Employment Jobs Bill ($300,000)", () =>
        {
            try
            {
                var bill = (dynamic)Office.Bridge.SponsorBill("Employment & Jobs Act", "Majority Leader", 300000m);
                Office.SyncFromBridge("Employment bill discussed with loyalists.");
                RefreshStats(stats);
                Feedback("Loyalists onboard — bill ready for floor.");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Convene loyalists, discuss bills and motions, and negotiate legislative terms before sending proposals to the assembly. Your majority leader routes bills to the floor when your party holds the majority.";

    private void RefreshStats(Text label)
    {
        label.text = $"Bills: {Office.State.BillCount}   |   Last: {Office.State.LastBillTitle}   |   Status: {Office.State.LastBillStatus}" +
                     $"   |   Approval: {Office.State.ApprovalRating:0.0}%";
    }
}
