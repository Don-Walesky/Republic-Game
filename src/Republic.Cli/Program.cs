namespace Republic.Cli;

using System;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Elections.Services;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Legislature.Models;
using Republic.Core.Legislature.Services;
using Republic.Core.Scenarios.Services;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "REPUBLIC - Executive Sovereign Desktop";

        var bootstrapper = new ApplicationBootstrapper();
        var app = bootstrapper.Bootstrap();

        // Obtain services from application
        var bootstrapperService = new ScenarioBootstrapper(app.WorldManager, app.WorkspaceManager, app.CabinetService, app.LegislatureService);
        await bootstrapperService.BootstrapScenarioAsync("arcadia-day1");

        var running = true;
        while (running)
        {
            RenderDashboard(app);

            Console.WriteLine("==============================================================");
            Console.WriteLine("                EXECUTIVE DIRECTIVE MENU                      ");
            Console.WriteLine("==============================================================");
            Console.WriteLine(" [1] Read Executive Inbox & Emails");
            Console.WriteLine(" [2] Evaluate Pending Decisions & Crises");
            Console.WriteLine(" [3] Review Cabinet & Appoint Ministers");
            Console.WriteLine(" [4] Adjust Taxation & Ministry Budget");
            Console.WriteLine(" [5] Launch Covert Intelligence Operation");
            Console.WriteLine(" [6] Conduct Parliamentary Bill Vote");
            Console.WriteLine(" [7] Advance Time (1 Tick / 10 Ticks)");
            Console.WriteLine(" [8] Save / Quick-Load Session");
            Console.WriteLine(" [0] Exit Application");
            Console.WriteLine("==============================================================");
            Console.Write(" Select Option > ");

            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    ReadInbox(app);
                    break;
                case "2":
                    await EvaluateDecisionsAsync(app);
                    break;
                case "3":
                    await ManageCabinetAsync(app);
                    break;
                case "4":
                    await ManageBudgetAsync(app);
                    break;
                case "5":
                    await LaunchIntelAsync(app);
                    break;
                case "6":
                    await ManageLegislatureAsync(app);
                    break;
                case "7":
                    await AdvanceTimeAsync(app);
                    break;
                case "8":
                    await SaveLoadAsync(app);
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option selection.");
                    break;
            }
        }

        Console.WriteLine("Executive session terminated.");
    }

    private static void RenderDashboard(RepublicApplication app)
    {
        Console.Clear();
        var econ = app.WorldManager.Economic.GetIndicators();
        var demo = app.WorldManager.Demographics.GetDemographics();
        var tick = app.TimeSystem.CurrentTick;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==============================================================");
        Console.WriteLine("          REPUBLIC OF ARCADIA - PRESIDENTIAL DESK            ");
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        Console.WriteLine($" Tick: {tick} | Date: {app.TimeSystem.CurrentSimulatedDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($" Treasury: ${econ.TreasuryBalance:N0} | GDP: ${econ.GrossDomesticProduct:N0}");
        Console.WriteLine($" Inflation: {econ.InflationRate * 100:0.0}% | Trade Balance: ${econ.TradeBalance:N0}");
        Console.WriteLine($" Demographics: Population ({demo.TotalPopulation:N0}) | Happiness ({demo.HappinessRating:0.0}%)");
        Console.WriteLine("--------------------------------------------------------------");

        var emails = app.WorkspaceManager.Email.GetInbox();
        var news = app.WorkspaceManager.News.GetNewsFeed();
        var decisions = app.DecisionEngine.GetPendingDecisions();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($" [UNREAD EMAILS]: {emails.Count} | [NEWS TICKER]: {(news.Count > 0 ? news[^1].Headline : "No headlines")}");
        if (decisions.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" [CRITICAL CRISES PENDING]: {decisions.Count} Decision Context(s) Require Action!");
        }
        Console.ResetColor();
        Console.WriteLine("--------------------------------------------------------------");
    }

    private static void ReadInbox(RepublicApplication app)
    {
        var inbox = app.WorkspaceManager.Email.GetInbox();
        Console.WriteLine("=== EXECUTIVE INBOX ===");
        if (inbox.Count == 0)
        {
            Console.WriteLine("No messages in inbox.");
            return;
        }

        for (var i = 0; i < inbox.Count; i++)
        {
            var email = inbox[i];
            Console.WriteLine($" [{i + 1}] FROM: {email.Sender} | SUBJECT: {email.Subject}");
            Console.WriteLine($"     \"{email.Body}\"");
        }
        Console.WriteLine("\nPress Enter to return to main menu...");
        Console.ReadLine();
    }

    private static async Task EvaluateDecisionsAsync(RepublicApplication app)
    {
        var decisions = app.DecisionEngine.GetPendingDecisions();
        Console.WriteLine("=== PENDING EXECUTIVE DECISIONS ===");
        if (decisions.Count == 0)
        {
            Console.WriteLine("No pending decisions at this time.");
            Console.ReadLine();
            return;
        }

        var decision = decisions[0];
        Console.WriteLine($" DECISION: {decision.Title} [{decision.Category}]");
        Console.WriteLine($" DESCRIPTION: {decision.Description}\n");

        for (var i = 0; i < decision.Options.Count; i++)
        {
            var opt = decision.Options[i];
            Console.WriteLine($"  [{i + 1}] {opt.Label} - Treasury Cost: ${opt.TreasuryCost:N0}");
            Console.WriteLine($"      Description: {opt.Description}");
        }

        Console.Write("\n Select Policy Option Number > ");
        var choice = Console.ReadLine()?.Trim();
        if (int.TryParse(choice, out var idx) && idx >= 1 && idx <= decision.Options.Count)
        {
            var selectedOpt = decision.Options[idx - 1];
            var success = await app.DecisionEngine.ExecuteDecisionAsync(decision.Id, selectedOpt.Id);
            Console.WriteLine(success ? "Policy enacted successfully!" : "Failed to enact policy.");
        }
        Console.ReadLine();
    }

    private static async Task ManageCabinetAsync(RepublicApplication app)
    {
        Console.WriteLine("=== EXECUTIVE CABINET ===");
        var ministers = app.CabinetService.GetAllMinisters();
        foreach (var m in ministers)
        {
            Console.WriteLine($" - [{m.Portfolio}] {m.Name} | Competence: {m.CompetenceRating:0}% | Loyalty: {m.LoyaltyRating:0}%");
        }

        Console.WriteLine("\nAppoint new Minister of Foreign Affairs?");
        Console.Write("Enter Minister Name (or press Enter to skip) > ");
        var name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            await app.CabinetService.AppointMinisterAsync(new Minister { Name = name, CompetenceRating = 85.0, LoyaltyRating = 90.0 }, CabinetPortfolio.ForeignAffairs);
            Console.WriteLine($"Appointed {name} as Minister of Foreign Affairs!");
        }
    }

    private static async Task ManageBudgetAsync(RepublicApplication app)
    {
        Console.WriteLine("=== TAXATION & BUDGET POLICY ===");
        var currentTax = app.BudgetService.GetTaxPolicy();
        Console.WriteLine($" Current Income Tax: {currentTax.IncomeTaxRate * 100:0}% | Corp Tax: {currentTax.CorporateTaxRate * 100:0}%");

        Console.Write("Enter new Income Tax Rate % (e.g. 28) > ");
        var input = Console.ReadLine()?.Trim();
        if (double.TryParse(input, out var rate))
        {
            await app.BudgetService.UpdateTaxPolicyAsync(new TaxPolicy { IncomeTaxRate = rate / 100.0, CorporateTaxRate = currentTax.CorporateTaxRate });
            Console.WriteLine("Tax policy updated!");
        }
    }

    private static async Task LaunchIntelAsync(RepublicApplication app)
    {
        Console.WriteLine("=== COVERT INTELLIGENCE AGENCY ===");
        Console.Write("Enter Target Country Name > ");
        var target = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(target))
        {
            await app.IntelligenceService.InfiltrateTargetAsync(target, 3);
            var op = await app.IntelligenceService.LaunchOperationAsync(CovertOperationType.IndustrialSabotage, target, $"Operation {target} Strike");
            Console.WriteLine($"Operation launched! Outcome Completed: {op.IsCompleted}, Exposed: {op.IsExposed}");
        }
        Console.ReadLine();
    }

    private static async Task ManageLegislatureAsync(RepublicApplication app)
    {
        Console.WriteLine("=== PARLIAMENTARY ASSEMBLY ===");
        Console.Write("Enter Bill Title to Introduce > ");
        var title = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
        {
            var bill = await app.LegislatureService.IntroduceBillAsync(title, "Executive sponsored legislative reform.");
            var res = await app.LegislatureService.VoteOnBillAsync(bill.Id);
            Console.WriteLine($"Vote result: Passed = {res.Passed} ({res.AyesCount}/{res.TotalVotes} Ayes)");
        }
        Console.ReadLine();
    }

    private static async Task AdvanceTimeAsync(RepublicApplication app)
    {
        Console.WriteLine("Advancing time by 10 simulation frames...");
        await app.Engine.RunAsync(10, TimeSpan.FromSeconds(0.1));
        Console.WriteLine($"Advanced to Tick {app.TimeSystem.CurrentTick}. Press Enter to continue...");
        Console.ReadLine();
    }

    private static async Task SaveLoadAsync(RepublicApplication app)
    {
        Console.WriteLine("=== SAVE / LOAD SESSION ===");
        Console.WriteLine(" [1] Save Session");
        Console.WriteLine(" [2] Load Quicksave");
        Console.Write("Choice > ");
        var choice = Console.ReadLine()?.Trim();
        if (choice == "1")
        {
            var file = await app.SaveGameManager.SaveGameAsync("Quicksave");
            Console.WriteLine($"Session saved to '{file}'!");
        }
        else if (choice == "2")
        {
            var state = await app.SaveGameManager.LoadGameAsync("Quicksave");
            Console.WriteLine($"Session loaded! Current tick: {state.CurrentTick}");
        }
        Console.ReadLine();
    }
}
