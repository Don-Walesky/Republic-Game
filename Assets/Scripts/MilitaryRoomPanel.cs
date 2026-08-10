using UnityEngine;
using UnityEngine.UI;

public class MilitaryRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Military Command Room";

    protected override void BuildContent(Transform root)
    {
        var desc = UiHelper.Label(root, RoomDesc(), 17,
            new Vector2(0.04f, 0.73f), new Vector2(0.96f, 0.88f));

        var stats = UiHelper.Label(root, string.Empty, 18,
            new Vector2(0.04f, 0.57f), new Vector2(0.96f, 0.72f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.55f), spacing: 12f);

        UiHelper.ActionButton(col.transform, "Recruit 500 Personnel ($1,250)", () =>
        {
            try { Office.Bridge.RecruitPersonnel(500, 1250m); Office.SyncFromBridge("Recruited 500 personnel."); RefreshStats(stats); Feedback("500 new recruits enlisted."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Purchase 100 Weapons ($15,000)", () =>
        {
            try { Office.Bridge.PurchaseWeapons(100, 15000m); Office.SyncFromBridge("Procured 100 weapons."); RefreshStats(stats); Feedback("100 weapons added to arsenal."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });

        UiHelper.ActionButton(col.transform, "Launch Employment Program ($125,000)", () =>
        {
            try { Office.Bridge.CreateEmploymentProgram("Military Infrastructure", 300, 125000m); Office.SyncFromBridge("Military employment program launched."); RefreshStats(stats); Feedback("Military employment program active."); }
            catch (System.Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Meet your service chiefs. Direct recruitment, procurement, and strategic deployments from this command room.";

    private void RefreshStats(Text label)
    {
        label.text = $"Personnel: {Office.State.Personnel:N0}   |   Weapons: {Office.State.Weapons:N0}" +
                     $"   |   Insecurity Index: {Office.State.InsecurityIndex:0.0}";
    }
}
