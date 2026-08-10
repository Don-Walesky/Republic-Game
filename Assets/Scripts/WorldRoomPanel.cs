using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "World Affairs Chamber";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, "Manage alliances, treaty-constrained incidents, FX power, and offline-governance risk in a multiplayer world.", 16,
            new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.89f));

        var stats = UiHelper.Label(root, string.Empty, 17,
            new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.75f));
        RefreshStats(stats);

        var actions = UiHelper.Column(root, "WorldActions",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.58f), spacing: 10f);

        UiHelper.ActionButton(actions.transform, "Recalculate FX Market", () =>
        {
            try
            {
                Office.Bridge.RecalculateForexMarket();
                Office.SyncFromBridge("Global FX market recalculated.");
                RefreshStats(stats);
                Feedback("Currency strength recalibrated to world power.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Form Economic Alliance", () =>
        {
            try
            {
                Office.Bridge.FormAlliance("Regional Growth Bloc", "Economic", new[] { "Arcadia", "Nation-1", "Nation-2" });
                Office.SyncFromBridge("Formed Regional Growth Bloc alliance.");
                RefreshStats(stats);
                Feedback("Alliance formed with no-war clause.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Form Security Alliance", () =>
        {
            try
            {
                Office.Bridge.FormAlliance("Continental Shield", "Security", new[] { "Arcadia", "Nation-3", "Nation-4" });
                Office.SyncFromBridge("Formed Continental Shield alliance.");
                RefreshStats(stats);
                Feedback("Security pact signed.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Record Limited Military Incident", () =>
        {
            try
            {
                Office.Bridge.RecordMilitaryIncident("Arcadia", "Nation-1", "Cross-border strike", "Medium");
                Office.SyncFromBridge("Logged a limited military incident under treaty rules.");
                RefreshStats(stats);
                Feedback("Incident recorded; no-war rule enforced.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Simulate 6 Offline Turns", () =>
        {
            try
            {
                Office.Bridge.UpdatePlayerPresence(6, 5);
                Office.SyncFromBridge("Extended absence triggered governance continuity protocol.");
                RefreshStats(stats);
                Feedback(Office.State.IsGovernmentToppled
                    ? "Government toppled. AI caretaker in place."
                    : "Government remained stable.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Run Midterm Election", () =>
        {
            try
            {
                Office.Bridge.RunMidtermElection();
                Office.SyncFromBridge("Midterm election completed.");
                RefreshStats(stats);
                Feedback("Seats updated based on public momentum.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Defection Crisis: Concede Patronage", () =>
        {
            try
            {
                Office.Bridge.ResolveDefectionCrisis("ConcedePatronage");
                Office.SyncFromBridge("Conceded patronage during defection crisis.");
                RefreshStats(stats);
                Feedback("Loyalty restored at fiscal cost.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Defection Crisis: Let Them Go", () =>
        {
            try
            {
                Office.Bridge.ResolveDefectionCrisis("LetThemGo");
                Office.SyncFromBridge("Allowed defection wave to proceed.");
                RefreshStats(stats);
                Feedback("Seat losses applied.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });
    }

    private void RefreshStats(Text label)
    {
        label.text =
            $"Neighbors: {Office.State.NeighborCountries}  |  Treaties: {Office.State.ActiveTreaties}  |  Toppled: {Office.State.IsGovernmentToppled}" +
            $"\nCaretaker: {(string.IsNullOrWhiteSpace(Office.State.CaretakerPresident) ? "None" : Office.State.CaretakerPresident)}" +
            $"  |  Strongest neighbor: {Office.State.StrongestNeighbor} ({Office.State.StrongestNeighborCurrency})" +
            $"\nLegislature seats: Senate={Office.State.SenateSeatsHeld}, House={Office.State.HouseSeatsHeld}";
    }
}
