using System;
using UnityEngine;
using UnityEngine.UI;

public class EconomyRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Economy & Industry Hub";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, RoomDesc(), 16,
            new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.89f));

        var stats = UiHelper.Label(root, string.Empty, 16,
            new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.77f));
        RefreshStats(stats);

        var col = UiHelper.Column(root, "Actions",
            new Vector2(0.04f, 0.11f), new Vector2(0.96f, 0.61f), spacing: 9f);

        // Industries
        UiHelper.ActionButton(col.transform, "Invest in Oil Industry ($2M)", () =>
        {
            try { Office.Bridge.AddIndustry("National Oil Corp", "Oil", 2000000m); Office.SyncFromBridge("Oil industry funded."); RefreshStats(stats); Feedback("Oil industry active — +GDP each turn."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Invest in Technology Sector ($1.5M)", () =>
        {
            try { Office.Bridge.AddIndustry("TechHub Initiative", "Technology", 1500000m); Office.SyncFromBridge("Technology sector funded."); RefreshStats(stats); Feedback("Tech sector online — boosts education and employment."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Invest in Agriculture ($800K)", () =>
        {
            try { Office.Bridge.AddIndustry("National Agriculture Fund", "Agriculture", 800000m); Office.SyncFromBridge("Agriculture sector funded."); RefreshStats(stats); Feedback("Agriculture sector active."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Natural resources
        UiHelper.ActionButton(col.transform, "Discover Oil Reserve", () =>
        {
            try { Office.Bridge.DiscoverResource("Northern Oil Field", "Oil"); Office.SyncFromBridge("Oil reserve discovered."); RefreshStats(stats); Feedback("Oil reserve found — extract to earn revenue."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Begin Oil Extraction ($500K)", () =>
        {
            try { Office.Bridge.BeginResourceExtraction("Northern Oil Field", 500000m); Office.SyncFromBridge("Oil extraction commenced."); RefreshStats(stats); Feedback("Extraction live — resource income added each turn."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Infrastructure
        UiHelper.ActionButton(col.transform, "Build University ($600K)", () =>
        {
            try { Office.Bridge.BuildInfrastructure("University", 600000m); Office.SyncFromBridge("University constructed."); RefreshStats(stats); Feedback("University built — education, HDI, and employment improve."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Build Road Network ($400K)", () =>
        {
            try { Office.Bridge.BuildInfrastructure("RoadNetwork", 400000m); Office.SyncFromBridge("Road network expanded."); RefreshStats(stats); Feedback("Roads built — GDP and infrastructure rise."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Build Power Grid ($700K)", () =>
        {
            try { Office.Bridge.BuildInfrastructure("PowerGrid", 700000m); Office.SyncFromBridge("Power grid expanded."); RefreshStats(stats); Feedback("Grid online — GDP and security improve."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Build Airport ($1M)", () =>
        {
            try { Office.Bridge.BuildInfrastructure("Airport", 1000000m); Office.SyncFromBridge("Airport constructed."); RefreshStats(stats); Feedback("Airport active — trade volume and GDP surge."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Loans
        UiHelper.ActionButton(col.transform, "IMF Loan $5M @ 6% (20 turns)", () =>
        {
            try { var l = (dynamic)Office.Bridge.TakeLoan("Imf", 5000000m, 0.06m, 20); Office.SyncFromBridge($"IMF loan taken. Owed: ${(object)l.TotalOwed}"); RefreshStats(stats); Feedback("IMF loan received — approval -4. Repay per turn."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "World Bank Loan $3M @ 4% (15 turns)", () =>
        {
            try { Office.Bridge.TakeLoan("WorldBank", 3000000m, 0.04m, 15); Office.SyncFromBridge("World Bank loan taken."); RefreshStats(stats); Feedback("World Bank loan received."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "CBN Loan $1M @ 9% (10 turns)", () =>
        {
            try { Office.Bridge.TakeLoan("Cbn", 1000000m, 0.09m, 10); Office.SyncFromBridge("CBN loan taken."); RefreshStats(stats); Feedback("CBN loan received — higher rate, faster access."); }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Repay Active Loan ($100K)", () =>
        {
            try
            {
                if (Office.State.ActiveLoanId == Guid.Empty) { Feedback("No active loan."); return; }
                Office.Bridge.RepayLoan(Office.State.ActiveLoanId, 100000m);
                Office.SyncFromBridge("Loan repayment made ($100K).");
                RefreshStats(stats);
                Feedback($"Repaid $100K on {Office.State.ActiveLoanLender} loan. Remaining: ${Office.State.ActiveLoanOwed:N0}");
            }
            catch (Exception e) { Feedback(e.Message); }
        });
        UiHelper.ActionButton(col.transform, "Renegotiate Loan (+2% rate, +5 turns)", () =>
        {
            try
            {
                if (Office.State.ActiveLoanId == Guid.Empty) { Feedback("No active loan."); return; }
                Office.Bridge.RenegotiateLoan(Office.State.ActiveLoanId, 0.02m, 5);
                Office.SyncFromBridge("Loan renegotiated.");
                RefreshStats(stats);
                Feedback("Loan extended — higher total cost but more time.");
            }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Bilateral trade
        UiHelper.ActionButton(col.transform, "Bilateral Trade: Lys ($2M export / $1M import)", () =>
        {
            try { Office.Bridge.NegotiateBilateralTrade("Lys", 2000000m, 1000000m); Office.SyncFromBridge("Bilateral trade with Lys signed."); RefreshStats(stats); Feedback("Trade with Lys active — net +$1M GDP."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Corruption
        UiHelper.ActionButton(col.transform, "Anti-Corruption Drive ($200K)", () =>
        {
            try { Office.Bridge.InvestigateCorruption(200000m); Office.SyncFromBridge("Anti-corruption investigation launched."); RefreshStats(stats); Feedback("Corruption reduced — insecurity drops, approval rises."); }
            catch (Exception e) { Feedback(e.Message); }
        });

        // Protests
        UiHelper.ActionButton(col.transform, "Address Civil Unrest ($150K)", () =>
        {
            try { Office.Bridge.AddressProtest("Wage increase concession", 150000m); Office.SyncFromBridge("Protest addressed — concession made."); RefreshStats(stats); Feedback("Unrest reduced. Approval improves."); }
            catch (Exception e) { Feedback(e.Message); }
        });
    }

    private static string RoomDesc() =>
        "Invest in industries, extract natural resources, build infrastructure, take or repay loans, and manage bilateral trade and civil unrest. Each decision ripples across all governance indices.";

    private void RefreshStats(Text label)
    {
        label.text = $"Industries: {Office.State.IndustryCount}  |  Turn income: ${Office.State.IndustryTurnIncome:N0}" +
                     $"  |  Resources: {Office.State.ResourceCount}  |  Loans: {Office.State.ActiveLoansCount}" +
                     $"  |  Debt: ${Office.State.ExternalDebt:N0}  |  Trades: {Office.State.BilateralTradesCount}" +
                     $"\nCorruption: {Office.State.CorruptionLevel:0.0}  |  Unrest: {Office.State.CivilUnrestIndex:0.0}" +
                     $"  |  Universities: {Office.State.TertiaryInstitutions}  |  Edu: {Office.State.EducationIndex:0.0}";
    }
}
