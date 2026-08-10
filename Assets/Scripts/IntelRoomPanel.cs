using System;
using UnityEngine;
using UnityEngine.UI;

public class IntelRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Intelligence & Security";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 16,
            new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.89f));

        var stats = UiHelper.Label(root, string.Empty, 16,
            new Vector2(0.04f, 0.64f), new Vector2(0.96f, 0.77f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.06f, 0.11f), new Vector2(0.94f, 0.63f), spacing: 10f);

        UiHelper.ActionButton(col.transform, "Surveillance Op: Lys ($80K)", () =>
        {
            try { Office.Bridge.RunIntelligenceOp("Surveillance", "Lys", 80000m); Office.SyncFromBridge("Surveillance op launched vs Lys."); RefreshStats(stats); Feedback("Surveillance complete — insecurity reduced if successful."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Infiltrate Rival Network ($120K)", () =>
        {
            try { Office.Bridge.RunIntelligenceOp("Infiltration", "Rival State", 120000m); Office.SyncFromBridge("Infiltration op launched."); RefreshStats(stats); Feedback("Agent deployed — success depends on military strength."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Counter-Espionage Drive ($100K)", () =>
        {
            try { Office.Bridge.RunIntelligenceOp("CounterEspionage", "Domestic", 100000m); Office.SyncFromBridge("Counter-espionage operation active."); RefreshStats(stats); Feedback("Domestic threat reduced."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Military ops
        UiHelper.ActionButton(col.transform, "Airstrike on Verdan (2000 troops)", () =>
        {
            try { var r = (dynamic)Office.Bridge.LaunchMilitaryOperation("Verdan", "Airstrike", 2000); Office.SyncFromBridge($"Airstrike on Verdan: {(object)r.Succeeded}."); RefreshStats(stats); Feedback($"Op outcome: {(bool)r.Succeeded}. Troops: -{2000 / 5}"); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Cyber Attack on Orion ($60K)", () =>
        {
            try { var r = (dynamic)Office.Bridge.LaunchMilitaryOperation("Orion", "CyberAttack", 0); Office.SyncFromBridge("Cyber operation launched vs Orion."); RefreshStats(stats); Feedback("Cyber op complete."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Simulate Incoming Attack from Lys (5000)", () =>
        {
            try { Office.Bridge.ReceiveMilitaryAttack("Lys", 5000); Office.SyncFromBridge("Lys launched military action."); RefreshStats(stats); Feedback("Defense results logged — check insecurity index."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Disaster response
        UiHelper.ActionButton(col.transform, "Respond to Flood ($400K)", () =>
        {
            try { Office.Bridge.RespondToDisaster("Flood", 400000m); Office.SyncFromBridge("Flood relief deployed."); RefreshStats(stats); Feedback("Flood response active. Effectiveness depends on budget."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Respond to Pandemic ($600K)", () =>
        {
            try { Office.Bridge.RespondToDisaster("Pandemic", 600000m); Office.SyncFromBridge("Pandemic emergency response underway."); RefreshStats(stats); Feedback("Pandemic response launched."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Judicial
        UiHelper.ActionButton(col.transform, "Appoint Justice (Senate Score 75)", () =>
        {
            try { Office.Bridge.AppointJudge("Justice Amara Cole", 75); Office.SyncFromBridge("Justice Amara Cole appointed."); RefreshStats(stats); Feedback("Judge confirmed — affects future law rulings."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Rule on Latest Bill", () =>
        {
            try
            {
                if (Office.State.LastBillId == Guid.Empty) { Feedback("No bill pending judicial review."); return; }
                Office.Bridge.RuleOnLaw(Office.State.LastBillId);
                Office.SyncFromBridge($"Judiciary ruled on: {Office.State.LastBillTitle}");
                RefreshStats(stats);
                Feedback($"Ruling: {Office.State.LastBillStatus}");
            }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Minister initiative response
        UiHelper.ActionButton(col.transform, "Approve Pending Minister Initiative", () =>
        {
            try
            {
                if (Office.State.PendingInitiativeId == Guid.Empty) { Feedback("No minister initiative pending."); return; }
                Office.Bridge.ApproveMinisterInitiative(Office.State.PendingInitiativeId, true);
                Office.SyncFromBridge($"Initiative approved: {Office.State.PendingInitiativeTitle}");
                RefreshStats(stats);
                Feedback("Initiative approved — GDP impact applied.");
            }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Reject Pending Minister Initiative", () =>
        {
            try
            {
                if (Office.State.PendingInitiativeId == Guid.Empty) { Feedback("No minister initiative pending."); return; }
                Office.Bridge.ApproveMinisterInitiative(Office.State.PendingInitiativeId, false);
                Office.SyncFromBridge("Minister initiative rejected.");
                RefreshStats(stats);
                Feedback("Initiative rejected. Minister relations may suffer.");
            }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Legislator demand
        UiHelper.ActionButton(col.transform, "Grant Legislator Demand", () =>
        {
            try
            {
                if (Office.State.PendingDemandId == Guid.Empty) { Feedback("No legislator demand pending."); return; }
                Office.Bridge.GrantLegislatorDemand(Office.State.PendingDemandId, true);
                Office.SyncFromBridge($"Granted demand: {Office.State.PendingDemandDesc}");
                RefreshStats(stats);
                Feedback("Demand granted — loyalty rises.");
            }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Reject Legislator Demand", () =>
        {
            try
            {
                if (Office.State.PendingDemandId == Guid.Empty) { Feedback("No legislator demand pending."); return; }
                Office.Bridge.GrantLegislatorDemand(Office.State.PendingDemandId, false);
                Office.SyncFromBridge($"Rejected demand: {Office.State.PendingDemandDesc}");
                RefreshStats(stats);
                Feedback("Demand rejected — loyalty drops.");
            }
            catch (Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Run intelligence operations, respond to disasters, appoint judges, resolve minister initiatives, and respond to legislator demands. Opposition grows when you fail — act decisively.";

    private void RefreshStats(Text label)
    {
        label.text = $"Insecurity: {Office.State.InsecurityIndex:0.0}  |  Unrest: {Office.State.CivilUnrestIndex:0.0}" +
                     $"  |  Loyalty: {Office.State.LegislatorLoyalty:0.0}%  |  Opposition: {Office.State.OppositionStrength:0.0}" +
                     $"\nPending demand: {(string.IsNullOrEmpty(Office.State.PendingDemandDesc) ? "None" : Office.State.PendingDemandDesc)}" +
                     $"\nPending initiative: {(string.IsNullOrEmpty(Office.State.PendingInitiativeTitle) ? "None" : Office.State.PendingInitiativeTitle)}";
    }
}
