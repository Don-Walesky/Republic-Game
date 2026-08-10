using UnityEngine;
using UnityEngine.UI;

public class DiplomacyRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Diplomatic Desk";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 17,
            new Vector2(0.04f, 0.73f), new Vector2(0.96f, 0.88f));

        var stats = UiHelper.Label(root, string.Empty, 18,
            new Vector2(0.04f, 0.59f), new Vector2(0.96f, 0.72f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.57f), spacing: 12f);

        UiHelper.ActionButton(col.transform, "Accept Bailout from Lys (Oil Rights, $4M)", () =>
        {
            try
            {
                Office.Bridge.NegotiateForeignAid("Lys", "Bailout", 4000000m, 0m, 2500000m);
                Office.SyncFromBridge("Bailout accepted from Lys — oil rights secured in return.");
                RefreshStats(stats);
                Feedback("Lys bailout received. Oil rights negotiated.");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Issue Loan to Verdan (8% interest, $2M)", () =>
        {
            try
            {
                Office.Bridge.NegotiateForeignAid("Verdan", "Loan", 2000000m, 0.08m, 400000m);
                Office.SyncFromBridge("Loan issued to Verdan at 8% interest.");
                RefreshStats(stats);
                Feedback("Loan to Verdan active — returns accrue over term.");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Accept Development Grant from Orion ($1.5M)", () =>
        {
            try
            {
                Office.Bridge.NegotiateForeignAid("Orion", "Grant", 1500000m, 0m, 750000m);
                Office.SyncFromBridge("Development grant accepted from Orion.");
                RefreshStats(stats);
                Feedback("Orion grant received — infrastructure investment begins.");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Prepare for Next Election Cycle ($200,000)", () =>
        {
            try
            {
                Office.Bridge.PrepareForNextElection(200000m);
                Office.SyncFromBridge("Election campaign preparations underway.");
                RefreshStats(stats);
                Feedback("Campaign phase started — negotiations ramp up.");
            }
            catch (System.Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Foreign states approach you for bailouts, loans, and grants. You choose the terms and the return — natural resources, trade percentages, or economic stakes. All agreements influence your GDP, approval, and development indices.";

    private void RefreshStats(Text label)
    {
        label.text = $"Aid agreements: {Office.State.ForeignAidCount}   |   External debt: ${Office.State.ExternalDebt:N0}" +
                     $"   |   Treasury: ${Office.State.TreasuryBalance:N0}   |   GDP: ${Office.State.Gdp:N0}";
    }
}
