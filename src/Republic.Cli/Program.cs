namespace Republic.Cli;

using System;
using System.Linq;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Economy.Projects;
using Republic.Core.Elections.Services;
using Republic.Core.Government;
using Republic.Core.Government.Models;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Legislature.Models;
using Republic.Core.Legislature.Services;
using Republic.Core.Military.Models;
using Republic.Core.NationalYield;
using Republic.Core.Politics;
using Republic.Core.Scenarios.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "REPUBLIC - Executive Sovereign Desktop";

        var bootstrapper = new ApplicationBootstrapper();
        var app = bootstrapper.Bootstrap();

        // Obtain services from application
        var bootstrapperService = new ScenarioBootstrapper(app.WorldManager, app.WorkspaceManager, app.CabinetService, app.LegislatureService, null, null, app.Clock);
        await bootstrapperService.BootstrapScenarioAsync("arcadia-day1");

        // Process any startup offline gap specified in CLI arguments
        ApplyStartupGapIfSpecified(app, args);
        ApplyStartupPartyIfSpecified(app, args);

        // Check if player country has due boundaries on startup and brief the president
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        OfflineYieldBriefing? startupBriefing = null;
        if (playerCountry != null)
        {
            var catchUpService = new OfflineYieldCatchUpService();
            if (catchUpService.Schedule.CountDueBoundaries(playerCountry, app.TimeSystem.CurrentRepublicTime) > 0)
            {
                startupBriefing = catchUpService.CatchUp(playerCountry, app.TimeSystem.CurrentRepublicTime);
            }
        }

        // Handle CLI presidential taxation decision request if specified
        if (args != null && (args.Any(a => a.StartsWith("--apply-tax=") || a.StartsWith("--tax-rate=")) || args.Contains("--cancel-tax")))
        {
            var rateArg = args.FirstOrDefault(a => a.StartsWith("--apply-tax=") || a.StartsWith("--tax-rate="));
            var isConfirmed = !args.Contains("--cancel-tax") && !args.Contains("--confirmed=false");

            double rate = 0.25;
            if (rateArg != null)
            {
                var valStr = rateArg.Split('=', 2)[1].Trim();
                if (double.TryParse(valStr, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    rate = parsed > 1.0 ? parsed / 100.0 : parsed;
                }
            }

            string? customStatePath = null;
            var stateFileArg = args.FirstOrDefault(a => a.StartsWith("--state-file="));
            if (stateFileArg != null)
            {
                customStatePath = stateFileArg.Split('=', 2)[1].Trim();
            }

            var result = PresidentialDeskBridge.ApplyTaxDecision(rate, confirmed: isConfirmed, stateFilePath: customStatePath);
            Console.WriteLine($"[TAX DECISION]: Result={result.Result}, Success={result.Success}, TaxRate={result.IncomeTaxRate * 100:0.#}%, Approval={result.ApprovalRating:0.#}%, Message=\"{result.Message}\"");
            return;
        }

        // Handle CLI export request if specified
        if (args != null && (args.Contains("--export-godot") || args.Contains("--export-state")))
        {
            var exportCountry = playerCountry ?? Country.Found("Republic of Arcadia", app.TimeSystem.CurrentRepublicTime);
            if (PresidentialDeskBridge.TryExportState(exportCountry, app.TimeSystem.CurrentRepublicTime, out var outPath, app.TimeSystem.CurrentTick))
            {
                Console.WriteLine($"[GODOT BRIDGE]: Exported live simulation state to: {outPath}");
            }
            return;
        }

        // Export initial live state for the Godot presidential desk
        if (playerCountry != null)
        {
            PresidentialDeskBridge.TryExportState(playerCountry, app.TimeSystem.CurrentRepublicTime, app.TimeSystem.CurrentTick);
        }

        var running = true;
        while (running)
        {
            RenderDashboard(app, startupBriefing);
            startupBriefing = null;

            Console.WriteLine("==============================================================");
            Console.WriteLine("                EXECUTIVE DIRECTIVE MENU                      ");
            Console.WriteLine("==============================================================");
            Console.WriteLine(" [1] Read Executive Inbox & Emails");
            Console.WriteLine(" [2] Evaluate Pending Decisions & Crises");
            Console.WriteLine(" [3] Review Cabinet & Appoint Ministers");
            Console.WriteLine(" [4] Adjust Taxation & Ministry Budget");
            Console.WriteLine(" [5] Launch Covert Intelligence Operation");
            Console.WriteLine(" [6] Conduct Parliamentary Bill Vote");
            Console.WriteLine(" [7] Advance Republic Simulation Time");
            Console.WriteLine(" [8] Save / Quick-Load Session");
            Console.WriteLine(" [9] Conduct Military & Defense Command Operations");
            Console.WriteLine(" [10] Administer Regional Provinces & Investment");
            Console.WriteLine(" [11] Enact Presidential Executive Decree");
            Console.WriteLine(" [12] Conduct Presidential Press Conference");
            Console.WriteLine(" [13] Oversee National Development Projects");
            Console.WriteLine(" [14] Diplomacy & International Relations");
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
                case "9":
                    await ManageMilitaryAsync(app);
                    break;
                case "10":
                    await ManageProvincesAsync(app);
                    break;
                case "11":
                    await EnactDecreeAsync(app);
                    break;
                case "12":
                    await HoldPressConferenceAsync(app);
                    break;
                case "13":
                    await ManageDevelopmentProjectsAsync(app);
                    break;
                case "14":
                    await ManageDiplomacyAsync(app);
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

    private static void RenderDashboard(RepublicApplication app, OfflineYieldBriefing? briefing = null)
    {
        try { Console.Clear(); } catch { /* ignore in non-interactive console */ }
        var econ = app.WorldManager.Economic.GetIndicators();
        var demo = app.WorldManager.Demographics.GetDemographics();
        var tick = app.TimeSystem.CurrentTick;

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        var schedule = new NationalYieldSchedule();
        var currentRepuTime = app.TimeSystem.CurrentRepublicTime;
        var nextBoundary = playerCountry != null
            ? schedule.GetNextDueBoundary(playerCountry, currentRepuTime)
            : schedule.GetNextBoundary(currentRepuTime);

        if (briefing != null && (briefing.BoundariesCredited > 0 || briefing.RoadProgramCompleted || briefing.ProjectsCompletedCount > 0))
        {
            PrintPresidentialBriefing(briefing);
        }

        if (playerCountry != null)
        {
            playerCountry.GrossDomesticProduct = econ.GrossDomesticProduct;
            demo.HappinessRating = playerCountry.HappinessRating;
        }

        var countryForDashboard = playerCountry ?? Country.Found("Republic of Arcadia", currentRepuTime);
        var profile = countryForDashboard.Profile ?? CountryProfile.CreateArcadia(currentRepuTime);
        var dashboard = countryForDashboard.GetDashboard(currentRepuTime);

        // Keep Godot presidential desk in sync with authoritative dashboard state
        PresidentialDeskBridge.TryExportState(countryForDashboard, currentRepuTime, tick);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==============================================================");
        Console.WriteLine("          REPUBLIC OF ARCADIA - PRESIDENTIAL DESK            ");
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        // 1. National Identity & Authoritative Simulation Clock
        var foundingStr = countryForDashboard.IsFounded
            ? $"Day {countryForDashboard.FoundingRepublicDay}, {countryForDashboard.IndependenceDay:d MMMM yyyy} WAT"
            : "Not established";
        Console.WriteLine($" Nation: {countryForDashboard.Name} | Founding Date: {foundingStr}");
        Console.WriteLine($" Sim Time: Day {currentRepuTime.DayNumber}, {currentRepuTime.Date:d MMMM yyyy} {currentRepuTime.Time:HH:mm} WAT ({currentRepuTime.UtcTimestamp:yyyy-MM-dd HH:mm:ss} UTC) | Tick: {tick}");
        Console.WriteLine($" Next 6h Boundary: Day {nextBoundary.DayNumber}, {nextBoundary.Time:HH:mm} WAT (in {Math.Max(0.0, (nextBoundary - currentRepuTime).TotalHours):F1}h)");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" Profile: {profile.OfficialName} | Capital: {profile.Capital} | Region: {profile.Region} | Pop: {profile.Population:N0} | Form: {profile.GovernmentForm}");
        Console.ResetColor();

        // 2. Sovereign Treasury & Domestic Stockpiles
        var activeNodesDesc = countryForDashboard.ResourceNodes.Count > 0
            ? string.Join(", ", countryForDashboard.ResourceNodes.Select(n => $"{n.ResourceType} ({(n.IsActive ? "Active" : "Inactive")})"))
            : "None";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($" Treasury Balance: {countryForDashboard.Treasury.FormattedBalance} (R{countryForDashboard.Treasury.Balance:N0} REPU) | GDP: R{countryForDashboard.GrossDomesticProduct:N0}");
        Console.WriteLine($" Resource Stockpiles: {countryForDashboard.Inventory.FormattedStockpiles} | Domestic Nodes: {activeNodesDesc}");
        Console.ResetColor();

        // 3. National Capacity & Yield Overview
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                  NATIONAL CAPACITY & YIELD                   ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        Console.WriteLine($" Yield Revenue: {dashboard.Yield} (per 6h cycle) | Infrastructure Input: {dashboard.Infrastructure}");
        Console.WriteLine($" Energy Capacity: {countryForDashboard.Yield.EnergyCapacity:F1} | Infrastructure Capacity: {countryForDashboard.Yield.InfrastructureCapacity:F1}");
        Console.WriteLine($" Human Capital: {countryForDashboard.Yield.HumanCapital:F1} | Science & Research: {countryForDashboard.Yield.ScienceCapacity:F1}");
        Console.WriteLine($" State Capacity Score: {countryForDashboard.StateCapacityScore:P0} | Public Approval: {dashboard.Approval} | Stability: {dashboard.Stability}");
        Console.WriteLine($" Taxation: {dashboard.Taxation} | Citizen Sentiment: {dashboard.CitizenSentiment}");
        Console.WriteLine($" Fiscal Quarter Status: {dashboard.Fiscal}");
        Console.WriteLine("--------------------------------------------------------------");

        // 4. Capital Development Projects
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                 CAPITAL DEVELOPMENT PROJECTS                 ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        if (countryForDashboard.Projects.Count == 0)
        {
            Console.WriteLine(" (No capital development projects initiated yet. Select option [13] to begin)");
        }
        else
        {
            foreach (var p in countryForDashboard.Projects)
            {
                if (p.IsActive)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" [UNDER CONSTRUCTION] {p.Name} | Progress: {p.GetProgress(currentRepuTime):P1} | Started: Day {p.StartedBoundary.DayNumber}, {p.StartedBoundary.Time:HH:mm} WAT | Finishes: Day {p.FinishBoundary.DayNumber}, {p.FinishBoundary.Time:HH:mm} WAT");
                    Console.ResetColor();
                }
                else if (p.Completed)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" [COMPLETED] {p.Name} | Commissioned Day {p.FinishBoundary.DayNumber}, {p.FinishBoundary.Time:HH:mm} WAT | Benefit Active (+5.0 Energy Capacity)");
                    Console.ResetColor();
                }
            }
        }
        Console.WriteLine("--------------------------------------------------------------");

        // 5. Systemic & Political State
        var consequenceText = countryForDashboard.LatestTreasuryConsequence?.ToString() ?? "No consequence";
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                   SYSTEMIC & POLITICAL STATE                 ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        Console.WriteLine($" Treasury Consequence: {consequenceText}");
        if (countryForDashboard.RulingParty == null && countryForDashboard.OppositionParty == null)
        {
            Console.WriteLine(" Party Status: No registered political parties");
        }
        else
        {
            Console.WriteLine($" Ruling Party: {countryForDashboard.RulingParty?.ToString() ?? "None"} | Opposition: {countryForDashboard.OppositionParty?.ToString() ?? "None"}");
        }
        Console.WriteLine($" Parliamentary Bill: {countryForDashboard.Bill?.ToString() ?? "No active bill"} | Interest Group: {countryForDashboard.InterestGroup?.ToString() ?? "None"}");
        Console.WriteLine($" Diplomatic Status: {dashboard.Diplomacy} | Bilateral Agreements: {(countryForDashboard.ActiveAgreements.Count > 0 ? string.Join("; ", countryForDashboard.ActiveAgreements) : "None")}");
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
        Console.WriteLine("==============================================================");
        Console.WriteLine("              EXECUTIVE CABINET & MINISTRIES                  ");
        Console.WriteLine("==============================================================");

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();

        Console.WriteLine("Current Ministerial Appointments:");
        var finance = playerCountry?.GetMinister(CabinetPortfolio.Finance) ?? app.CabinetService.GetAppointedMinister(CabinetPortfolio.Finance);
        var interior = playerCountry?.GetMinister(CabinetPortfolio.Interior) ?? app.CabinetService.GetAppointedMinister(CabinetPortfolio.Interior);
        var infra = playerCountry?.GetMinister(CabinetPortfolio.Infrastructure) ?? app.CabinetService.GetAppointedMinister(CabinetPortfolio.Infrastructure);

        Console.WriteLine($" - [Finance]        {(finance != null ? $"{finance.Name} | Competence: {finance.Competence:P0} | Integrity: {finance.Integrity:P0}" : "VACANT (Input: 30%)")}");
        Console.WriteLine($" - [Interior]       {(interior != null ? $"{interior.Name} | Competence: {interior.Competence:P0} | Integrity: {interior.Integrity:P0}" : "VACANT (Input: 30%)")}");
        Console.WriteLine($" - [Infrastructure] {(infra != null ? $"{infra.Name} | Competence: {infra.Competence:P0} | Experience: {infra.Experience:P0}" : "VACANT (Input: 30%)")}");

        var ministers = app.CabinetService.GetAllMinisters();
        foreach (var m in ministers.Where(m => m.Portfolio != CabinetPortfolio.Finance && m.Portfolio != CabinetPortfolio.Interior && m.Portfolio != CabinetPortfolio.Infrastructure))
        {
            Console.WriteLine($" - [{m.Portfolio}] {m.Name} | Competence: {m.Competence:P0} | Loyalty: {m.Loyalty:P0}");
        }

        var currentCapacity = playerCountry?.EvaluateStateCapacity();
        Console.WriteLine($"\nCurrent State Capacity: {currentCapacity?.FormattedScore ?? "N/A"} (Bottleneck: {currentCapacity?.BottleneckFactor ?? "N/A"})");
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("Select Portfolio to Appoint:");
        Console.WriteLine(" [1] Finance (Governs Civil Service Quality)");
        Console.WriteLine(" [2] Interior (Governs Corruption Control)");
        Console.WriteLine(" [3] Infrastructure (Governs Infrastructure Condition)");
        Console.WriteLine(" [0] Return to Main Menu");
        Console.Write("Select Portfolio (1-3, 0 to cancel) > ");

        var choice = Console.ReadLine()?.Trim();
        if (choice == "0" || string.IsNullOrWhiteSpace(choice))
        {
            return;
        }

        CabinetPortfolio targetPortfolio;
        string portfolioName;
        switch (choice)
        {
            case "1":
                targetPortfolio = CabinetPortfolio.Finance;
                portfolioName = "Finance";
                break;
            case "2":
                targetPortfolio = CabinetPortfolio.Interior;
                portfolioName = "Interior";
                break;
            case "3":
                targetPortfolio = CabinetPortfolio.Infrastructure;
                portfolioName = "Infrastructure";
                break;
            default:
                Console.WriteLine("Invalid selection.");
                return;
        }

        Console.Write($"Enter Minister Name for {portfolioName} (default candidate if empty) > ");
        var nameInput = Console.ReadLine()?.Trim();
        var ministerName = !string.IsNullOrWhiteSpace(nameInput)
            ? nameInput
            : targetPortfolio switch
            {
                CabinetPortfolio.Finance => "Dr. Elena Rostova",
                CabinetPortfolio.Interior => "Hon. Marcus Holloway",
                _ => "Eng. Victoria Sterling"
            };

        Console.Write("Enter Competence (0.0 to 1.0, default 0.85) > ");
        var compStr = Console.ReadLine()?.Trim();
        var competence = double.TryParse(compStr, out var cVal) ? Math.Clamp(cVal, 0.0, 1.0) : 0.85;

        double integrity = 0.80;
        double experience = 0.80;

        if (targetPortfolio == CabinetPortfolio.Finance || targetPortfolio == CabinetPortfolio.Interior)
        {
            Console.Write("Enter Integrity (0.0 to 1.0, default 0.85) > ");
            var intStr = Console.ReadLine()?.Trim();
            integrity = double.TryParse(intStr, out var iVal) ? Math.Clamp(iVal, 0.0, 1.0) : 0.85;
        }

        if (targetPortfolio == CabinetPortfolio.Infrastructure)
        {
            Console.Write("Enter Experience (0.0 to 1.0, default 0.85) > ");
            var expStr = Console.ReadLine()?.Trim();
            experience = double.TryParse(expStr, out var eVal) ? Math.Clamp(eVal, 0.0, 1.0) : 0.85;
        }

        var newMinister = new Minister
        {
            Name = ministerName,
            Competence = competence,
            Integrity = integrity,
            Experience = experience,
            Loyalty = 0.80,
            PoliticalConnections = 0.60
        };

        if (playerCountry != null)
        {
            await app.CabinetService.AppointMinisterAsync(playerCountry, newMinister, targetPortfolio).ConfigureAwait(false);
        }
        else
        {
            await app.CabinetService.AppointMinisterAsync(newMinister, targetPortfolio).ConfigureAwait(false);
        }

        var newCapacity = playerCountry?.EvaluateStateCapacity();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[APPOINTMENT CONFIRMED]: {newMinister.Name} appointed as Minister of {portfolioName}!");
        Console.WriteLine($"New State Capacity: {newCapacity?.FormattedScore} (Bottleneck: {newCapacity?.BottleneckFactor})");
        Console.ResetColor();
        Console.WriteLine("\nPress Enter to return to main menu...");
        Console.ReadLine();
    }

    public static async Task ManageBudgetAsync(
        RepublicApplication app,
        Func<string?>? readLine = null,
        Action<string>? write = null,
        Action<string>? writeLine = null)
    {
        var read = readLine ?? Console.ReadLine;
        var print = write ?? Console.Write;
        var printLine = writeLine ?? Console.WriteLine;

        printLine("==============================================================");
        printLine("           TAXATION POLICY & CITIZEN WELFARE DESK             ");
        printLine("==============================================================");

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        if (playerCountry == null)
        {
            printLine("No sovereign player country available.");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        var currentTax = playerCountry.TaxPolicy;
        var incomeRate = currentTax.IncomeTaxRate;
        var corpRate = currentTax.CorporateTaxRate;

        printLine($" Sovereign Nation: {playerCountry.Name} | Simulation Time: Day {currentTime.DayNumber}, {currentTime.Time:HH:mm} WAT");
        printLine($" Sovereign Treasury: {playerCountry.Treasury.FormattedBalance} | GDP: R{playerCountry.GrossDomesticProduct:N0}");
        printLine($" Current Public Approval: {playerCountry.HappinessRating:0.#}% | Baseline Stability: {playerCountry.BaselineStability:0.#}%");
        printLine("--------------------------------------------------------------");
        printLine($" Current Policy: Personal Income Tax: {incomeRate * 100:0.#}% | Corporate Tax: {corpRate * 100:0.#}%");
        printLine(" Standard Baseline: Personal Income Tax: 25% | Corporate Tax: 20%");
        printLine("--------------------------------------------------------------");
        printLine(" Citizen Welfare & Public Approval Feedback Rules:");
        printLine("  * Tax Hike (> current): Reduces household disposable income; approval decreases by 0.5% per +1% tax increase.");
        printLine("  * Tax Relief (< current): Expands household disposable income; approval increases by 0.5% per -1% tax cut.");
        printLine("  * Extreme Taxation (> 50%): Induces societal friction and reduces national stability by 2.0%.");
        printLine("  * Constitutional Limits: Personal Income Tax must be between 0% and 70%.");
        printLine("--------------------------------------------------------------");

        if (playerCountry.LatestCitizenFeedback != null)
        {
            printLine($" Latest Citizen Feedback: {playerCountry.LatestCitizenFeedback.Description}");
            printLine("--------------------------------------------------------------");
        }

        print(" Enter new Personal Income Tax Rate % (0 - 70) [or 'C' to Cancel] > ");
        var input = read()?.Trim();
        if (string.IsNullOrWhiteSpace(input) || input.Equals("C", StringComparison.OrdinalIgnoreCase))
        {
            printLine("\nTax policy review closed. No changes enacted.");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        if (!double.TryParse(input, out var enteredPercent) || !double.IsFinite(enteredPercent) || enteredPercent < 0.0 || enteredPercent > 70.0)
        {
            printLine("\n[INVALID INPUT]: Tax rate must be a valid percentage between 0% and 70%. Zero changes enacted.");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        var newIncomeRate = Math.Round(enteredPercent / 100.0, 4);
        var deltaRate = newIncomeRate - incomeRate;
        var estimatedApprovalDelta = -deltaRate * 50.0;
        var estimatedNewApproval = Math.Clamp(playerCountry.HappinessRating + estimatedApprovalDelta, 0.0, 100.0);

        var gdp = playerCountry.GrossDomesticProduct;
        var estIncomeTax = gdp * 0.15 * newIncomeRate;
        var estCorpTax = gdp * 0.10 * corpRate;
        var estTariff = Math.Max(0.0, playerCountry.TradeBalance) * currentTax.ImportTariffRate;
        var estTotalTax = estIncomeTax + estCorpTax + estTariff;
        var quarterlyExp = playerCountry.MinistryBudget.TotalQuarterlyExpenditures;
        var estNetOutcome = estTotalTax - quarterlyExp;

        printLine("");
        printLine(" POLICY IMPACT PROJECTION:");
        printLine($"  - Proposed Income Tax: {enteredPercent:0.#}% (Change: {(deltaRate >= 0 ? "+" : "")}{deltaRate * 100:0.#}%)");
        printLine($"  - Projected Public Approval: {estimatedNewApproval:0.#}% (Delta: {(estimatedApprovalDelta >= 0 ? "+" : "")}{estimatedApprovalDelta:0.#}%)");
        printLine($"  - Projected Quarterly Tax Revenue: {RepuTreasury.Format(estTotalTax)} (Net: {(estNetOutcome >= 0 ? "+" : "")}{RepuTreasury.Format(estNetOutcome)})");
        if (incomeRate <= 0.50 && newIncomeRate > 0.50)
        {
            printLine("  - WARNING: Income tax exceeds 50% threshold. Societal stress will reduce national stability by -2.0%!");
        }
        else if (incomeRate > 0.50 && newIncomeRate <= 0.50)
        {
            printLine("  - BENEFIT: Easing rate to 50% or below will restore +2.0% national stability!");
        }

        print("\n Do you confirm enacting this executive taxation policy? [Y/N] > ");
        var confirm = read()?.Trim();
        var isConfirmed = string.Equals(confirm, "Y", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(confirm, "YES", StringComparison.OrdinalIgnoreCase);

        var newPolicy = new TaxPolicy
        {
            IncomeTaxRate = newIncomeRate,
            CorporateTaxRate = corpRate,
            ImportTariffRate = currentTax.ImportTariffRate
        };

        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(playerCountry, newPolicy, currentTime, isConfirmed);

        if (outcome.IsSuccess)
        {
            await app.BudgetService.UpdateTaxPolicyAsync(newPolicy);
            var demo = app.WorldManager.Demographics.GetDemographics();
            demo.HappinessRating = playerCountry.HappinessRating;

            // Sync updated approval, stability, and tax policy to Godot desk
            PresidentialDeskBridge.TryExportState(playerCountry, currentTime, app.TimeSystem.CurrentTick);

            printLine("");
            printLine("==============================================================");
            printLine("            EXECUTIVE TAXATION ENACTMENT REPORT               ");
            printLine("==============================================================");
            printLine($" [POLICY ENACTED]: Personal Income Tax set to {newIncomeRate * 100:0.#}%.");
            printLine($" Citizen Reaction: {outcome.Message}");
            printLine(" Updated Sovereign State:");
            printLine($"  * Public Approval Rating: {playerCountry.HappinessRating:0.#}% (Previous: {outcome.CitizenFeedback?.PreviousApproval:0.#}%)");
            printLine($"  * Baseline Stability:     {playerCountry.BaselineStability:0.#}% (Previous: {outcome.CitizenFeedback?.PreviousStability:0.#}%)");
            printLine("==============================================================");
        }
        else if (outcome.Result == ExecutiveActionResult.Cancelled)
        {
            printLine("");
            printLine($" [ACTION CANCELLED]: {outcome.Message}");
        }
        else
        {
            printLine("");
            printLine($" [ACTION REJECTED]: {outcome.Message}");
        }

        printLine("\nPress Enter to continue...");
        read();
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

    public static async Task AdvanceTimeAsync(
        RepublicApplication app,
        Func<string?>? readLine = null,
        Action<string>? write = null,
        Action<string>? writeLine = null)
    {
        var read = readLine ?? Console.ReadLine;
        var print = write ?? Console.Write;
        var printLine = writeLine ?? Console.WriteLine;

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        printLine("==============================================================");
        printLine("                 EXECUTIVE TIME ADVANCEMENT                   ");
        printLine("==============================================================");
        printLine($" Current Simulation Time: Day {currentTime.DayNumber}, {currentTime.Time:HH:mm} WAT ({currentTime})");
        if (playerCountry != null)
        {
            printLine($" Sovereign Treasury: {playerCountry.Treasury.FormattedBalance} | Energy Capacity: {playerCountry.Yield.EnergyCapacity:F1}");
            if (playerCountry.ActiveProject != null)
            {
                var active = playerCountry.ActiveProject;
                printLine($" Active Project: {active.Name} | Progress: {active.GetProgress(currentTime):P1} | Finishes: Day {active.FinishBoundary.DayNumber}, {active.FinishBoundary.Time:HH:mm} WAT");
            }
            else
            {
                printLine(" Active Projects: None");
            }
        }
        printLine("--------------------------------------------------------------");
        printLine(" Select Advancement Duration:");
        printLine("  [1] Advance 1 Boundary (6 Hours / 1 standard cycle)");
        printLine("  [2] Advance 4 Boundaries (24 Hours / 1 Republic day)");
        printLine("  [3] Advance 12 Boundaries (72 Hours / Full Power Plant construction duration)");
        printLine("  [4] Advance 10 Engine Frames (micro-tick)");
        printLine("  [5] Advance Custom Hours");
        printLine("  [0] Return to Presidential Desk");
        print("\nSelect Option > ");

        var input = read()?.Trim();
        if (input == "0" || string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        var beforeStockpiles = playerCountry?.Inventory.FormattedStockpiles ?? "None";
        var beforeTreasury = playerCountry?.Treasury.Balance ?? 0.0;
        var beforeEnergy = playerCountry?.Yield.EnergyCapacity ?? 0.0;

        double hoursToAdvance = 0.0;
        if (input == "1") hoursToAdvance = 6.0;
        else if (input == "2") hoursToAdvance = 24.0;
        else if (input == "3") hoursToAdvance = 72.0;
        else if (input == "5")
        {
            print("Enter hours to advance > ");
            if (!double.TryParse(read()?.Trim(), out hoursToAdvance) || hoursToAdvance <= 0.0)
            {
                printLine("Invalid duration specified. Aborting.");
                printLine("\nPress Enter to return to main menu...");
                read();
                return;
            }
        }

        if (hoursToAdvance > 0.0)
        {
            if (app.Clock is IControlledRepublicClock controlledClock)
            {
                controlledClock.Advance(TimeSpan.FromHours(hoursToAdvance));
            }
        }
        else
        {
            // Option 4: engine frames
            await app.Engine.RunAsync(10, TimeSpan.FromSeconds(0.1));
        }

        var newTime = app.TimeSystem.CurrentRepublicTime;

        if (playerCountry != null)
        {
            // 1. Process 6-hour National Yield cycles
            var cycleService = new NationalYieldCycleService();
            var snapshots = cycleService.ExecuteDueCycles(playerCountry, newTime);

            // 2. Process domestic resource production cycles
            var prodService = new Republic.Core.World.Services.ResourceProductionService();
            var prodResults = prodService.ProduceDueCycles(playerCountry, newTime);

            // 3. Process capital development projects completion
            var completedCount = playerCountry.CheckProjectsCompletion(newTime);

            var afterTreasury = playerCountry.Treasury.Balance;
            var revenueDeposited = Math.Max(0.0, afterTreasury - beforeTreasury);

            printLine("");
            printLine("==============================================================");
            printLine("              SIMULATION ADVANCEMENT REPORT                   ");
            printLine("==============================================================");
            printLine($" Advanced To: Day {newTime.DayNumber}, {newTime.Time:HH:mm} WAT ({newTime}) | Tick: {app.TimeSystem.CurrentTick}");

            if (snapshots.Count > 0)
            {
                printLine($" - National Yield: {snapshots.Count} cycle boundary/boundaries credited. Deposited {RepuTreasury.Format(revenueDeposited)} REPU (Balance: {playerCountry.Treasury.FormattedBalance}).");
            }

            if (prodResults.Count > 0)
            {
                printLine(" - Domestic Resource Extraction:");
                foreach (var r in prodResults)
                {
                    printLine($"    * [Cycle {r.Boundary}]: {r.Message}");
                }
                printLine($"   Current Stockpiles: {playerCountry.Inventory.FormattedStockpiles}");
            }

            if (completedCount > 0)
            {
                printLine($" - Capital Development: {completedCount} project(s) completed and commissioned!");
                var activePlant = playerCountry.PowerPlantProject;
                if (activePlant is { Completed: true })
                {
                    printLine($"   National Power Plant is now fully operational! Energy Capacity elevated from {beforeEnergy:F1} to {playerCountry.Yield.EnergyCapacity:F1} (+{NationalPowerPlantProject.EnergyCapacityBonus:F1}).");
                    printLine($"   Recalculated State Capacity Score: {playerCountry.StateCapacityScore:P0}.");
                }
            }
            else if (playerCountry.ActivePowerPlantProject != null)
            {
                var p = playerCountry.ActivePowerPlantProject;
                printLine($" - Capital Development: {p.Name} is at {p.GetProgress(newTime):P1} construction progress (Expected Completion: Day {p.FinishBoundary.DayNumber}, {p.FinishBoundary.Time:HH:mm} WAT).");
            }
            printLine("==============================================================");

            // Sync updated simulation time, yield, and treasury balance to Godot desk
            PresidentialDeskBridge.TryExportState(playerCountry, newTime, app.TimeSystem.CurrentTick);
        }

        printLine("\nPress Enter to continue...");
        read();
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

    private static async Task ManageMilitaryAsync(RepublicApplication app)
    {
        Console.WriteLine("=== MILITARY & ARMED FORCES COMMAND ===");

        var state = new GovernmentState
        {
            CountryName = "Arcadia",
            TreasuryBalance = 2_500_000m
        };

        var report = app.MilitaryService.GetReadinessReport(state);

        Console.WriteLine($" Active Alert Level: {report.Defcon}");
        Console.WriteLine($" Total Personnel: {report.TotalPersonnel:N0} | Ordnance/Weapons: {report.TotalEquipment:N0}");
        Console.WriteLine($" Composite Readiness Score: {report.CompositeReadinessScore:0.0}%");
        Console.WriteLine(" Branch Breakdown:");
        foreach (var branch in report.BranchBreakdown)
        {
            Console.WriteLine($"  - {branch.Branch,-10}: Personnel: {branch.PersonnelCount,6:N0} | Equipment: {branch.EquipmentCount,4} | Readiness: {branch.ReadinessScore:0.0}%");
        }

        Console.WriteLine();
        Console.WriteLine(" [1] Set DEFCON Alert Level");
        Console.WriteLine(" [2] Recruit Branch Personnel");
        Console.WriteLine(" [3] Procure Branch Equipment");
        Console.WriteLine(" [4] Execute Strategic Military Directive");
        Console.Write(" Choice > ");

        var choice = Console.ReadLine()?.Trim();
        if (choice == "1")
        {
            Console.Write("Enter DEFCON level (1=Max Readiness, 5=Peace) > ");
            if (int.TryParse(Console.ReadLine()?.Trim(), out var lvl) && lvl >= 1 && lvl <= 5)
            {
                var newDefcon = (DefconLevel)lvl;
                await app.MilitaryService.SetDefconLevelAsync(state, newDefcon);
                Console.WriteLine($"DEFCON Level updated to: {newDefcon}");
            }
        }
        else if (choice == "2")
        {
            Console.Write("Branch (0=Army, 1=Navy, 2=AirForce, 3=CyberCorps) > ");
            if (Enum.TryParse<MilitaryBranch>(Console.ReadLine()?.Trim(), out var branch))
            {
                Console.Write("Recruit Count > ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out var count))
                {
                    bool ok = await app.MilitaryService.RecruitBranchPersonnelAsync(state, branch, count, 150m);
                    Console.WriteLine(ok ? $"Recruited {count} personnel for {branch}!" : "Recruitment failed due to insufficient treasury.");
                }
            }
        }
        else if (choice == "3")
        {
            Console.Write("Branch (0=Army, 1=Navy, 2=AirForce, 3=CyberCorps) > ");
            if (Enum.TryParse<MilitaryBranch>(Console.ReadLine()?.Trim(), out var branch))
            {
                Console.Write("Equipment Units > ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out var units))
                {
                    bool ok = await app.MilitaryService.ProcureBranchEquipmentAsync(state, branch, units, 500m);
                    Console.WriteLine(ok ? $"Procured {units} units of equipment for {branch}!" : "Procurement failed due to insufficient treasury.");
                }
            }
        }
        else if (choice == "4")
        {
            Console.Write("Target Country Name > ");
            var target = Console.ReadLine()?.Trim() ?? "Valoria";
            Console.Write("Operation Type (Invasion, Airstrike, CyberAttack, Blockade, PeacekeepingMission) > ");
            if (Enum.TryParse<MilitaryOpType>(Console.ReadLine()?.Trim(), true, out var opType))
            {
                Console.Write("Troops Committed > ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out var troops))
                {
                    var res = await app.MilitaryService.ExecuteDirectiveAsync(state, target, opType, troops);
                    Console.WriteLine($"Directive Outcome: {(res.Success ? "SUCCESS" : "FAILED")}");
                    Console.WriteLine($" {res.Message}");
                    Console.WriteLine($" Sustained Casualties: {res.CasualtiesSustained} | Target Casualties: {res.TargetCasualties}");
                }
            }
        }
        Console.ReadLine();
    }

    private static async Task ManageProvincesAsync(RepublicApplication app)
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine("      REGIONAL PROVINCES & INFRASTRUCTURE INVESTMENT          ");
        Console.WriteLine("==============================================================");

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        if (playerCountry != null)
        {
            // Check if active road project has reached or passed its finish boundary
            if (playerCountry.RoadProject is { Completed: false } activeRoad && currentTime >= activeRoad.FinishBoundary)
            {
                playerCountry.CompleteRoadProject(currentTime);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[DEVELOPMENT NOTICE]: National Road Program completed!");
                Console.WriteLine($"New Infrastructure Condition: {playerCountry.StateCapacity.InfrastructureCondition:P0} | State Capacity: {playerCountry.StateCapacityScore:P0}");
                Console.ResetColor();
            }

            Console.WriteLine($"Nation: {playerCountry.Name} | Liquid Treasury: {playerCountry.Treasury.FormattedBalance}");
            Console.WriteLine($"Infrastructure Condition: {playerCountry.StateCapacity.InfrastructureCondition:P0} | State Capacity: {playerCountry.StateCapacityScore:P0}");

            if (playerCountry.RoadProject != null)
            {
                if (playerCountry.RoadProject.Completed)
                {
                    Console.WriteLine("Road Program Status: COMPLETED (+10% Infrastructure Condition Applied)");
                }
                else
                {
                    Console.WriteLine($"Road Program Status: UNDER CONSTRUCTION (Started: {playerCountry.RoadProject.StartedBoundary}, Finishes: {playerCountry.RoadProject.FinishBoundary})");
                }
            }
            Console.WriteLine("--------------------------------------------------------------");
        }

        Console.WriteLine("Options:");
        Console.WriteLine(" [1] Start National Road Program (Cost: R500,000 | 24h / 4 boundaries | +0.1 Infra Condition)");
        Console.WriteLine(" [2] Invest in Specific Regional Province");
        Console.WriteLine(" [0] Return to Main Menu");
        Console.Write("\nSelect Option (1, 2, 0) > ");
        var selection = Console.ReadLine()?.Trim().ToUpperInvariant();

        if (selection == "1" || selection == "R" || selection == "ROAD")
        {
            if (playerCountry == null)
            {
                Console.WriteLine("No player country available.");
                Console.ReadLine();
                return;
            }

            if (playerCountry.RoadProject is { Completed: false })
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nA National Road Program is already active and under construction!");
                Console.WriteLine($"Finishes at: {playerCountry.RoadProject.FinishBoundary}");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            if (playerCountry.Treasury.Balance < 500_000.0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nInsufficient funds in sovereign treasury! Required: R500,000, Available: {playerCountry.Treasury.FormattedBalance}.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            var project = playerCountry.StartRoadProject(currentTime);
            if (project != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[CONSTRUCTION COMMENCED]: National Road Program initiated successfully!");
                Console.WriteLine($"Treasury Balance: {playerCountry.Treasury.FormattedBalance}");
                Console.WriteLine($"Infrastructure Condition: {playerCountry.StateCapacity.InfrastructureCondition:P0}");
                Console.WriteLine($"Target Finish Boundary: {project.FinishBoundary} (4 six-hour cycles / 24 hours)");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nFailed to start National Road Program.");
            }

            Console.WriteLine("\nPress Enter to return...");
            Console.ReadLine();
            return;
        }

        if (selection == "0" || string.IsNullOrWhiteSpace(selection))
        {
            return;
        }

        Console.WriteLine("\n=== REGIONAL PROVINCE ADMINISTRATION ===");
        var provinces = app.GeographyService.GetAllProvinces();
        if (provinces.Count == 0)
        {
            Console.WriteLine("No provinces registered.");
            Console.ReadLine();
            return;
        }

        for (var i = 0; i < provinces.Count; i++)
        {
            var p = provinces[i];
            Console.WriteLine($" [{i + 1}] {p.Name} | Terrain: {p.Terrain} | Pop: {p.Population:N0} | Infra: {p.InfrastructureIndex:0.0} | Stability: {p.LocalStability:0.0}%");
        }

        Console.Write("\nSelect Province Number to Invest > ");
        if (int.TryParse(Console.ReadLine()?.Trim(), out var idx) && idx >= 1 && idx <= provinces.Count)
        {
            var target = provinces[idx - 1];
            Console.Write("Enter Infrastructure Investment Amount ($) > ");
            if (decimal.TryParse(Console.ReadLine()?.Trim(), out var amount) && amount > 0m)
            {
                bool success = await app.GeographyService.InvestInRegionalInfrastructureAsync(target.Id, amount);
                Console.WriteLine(success ? $"Invested {amount:C} in '{target.Name}'! New Infra Index: {target.InfrastructureIndex:0.0}" : "Investment failed.");
            }
        }
        Console.ReadLine();
    }

    private static async Task EnactDecreeAsync(RepublicApplication app)
    {
        Console.WriteLine("=== PRESIDENTIAL EXECUTIVE DECREE ===");
        Console.Write("Enter Decree Title > ");
        var title = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
        {
            Console.Write("Enter Justification Summary > ");
            var justification = Console.ReadLine()?.Trim() ?? "National Interest";
            var order = new ExecutiveOrder
            {
                Title = title,
                Description = justification,
                IssuedAt = DateTimeOffset.UtcNow
            };
            Console.WriteLine($"[DECREE ENACTED] '{order.Title}' issued cleanly under executive authority.");
        }
        Console.ReadLine();
    }

    private static async Task HoldPressConferenceAsync(RepublicApplication app)
    {
        Console.WriteLine("=== PRESIDENTIAL PRESS CONFERENCE ===");
        Console.Write("Enter Press Conference Topic > ");
        var topic = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(topic))
        {
            var question = await app.PressConferenceService.HostPressConferenceAsync(topic);
            Console.WriteLine($"[PRESS BRIEFING QUESTION]:\n Journalist: {question.JournalistName} ({question.NewsOutlet})\n Prompt: {question.QuestionText}");
        }
        Console.ReadLine();
    }

    public static async Task ManageDevelopmentProjectsAsync(
        RepublicApplication app,
        Func<string?>? readLine = null,
        Action<string>? write = null,
        Action<string>? writeLine = null)
    {
        var read = readLine ?? Console.ReadLine;
        var print = write ?? Console.Write;
        var printLine = writeLine ?? Console.WriteLine;

        printLine("==============================================================");
        printLine("               NATIONAL DEVELOPMENT PROJECTS                  ");
        printLine("==============================================================");

        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        if (playerCountry == null)
        {
            printLine("No sovereign player country available.");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        printLine($"Nation: {playerCountry.Name} | Sovereign Treasury: R{playerCountry.Treasury.Balance:N0} ({playerCountry.Treasury.FormattedBalance})");
        printLine($"Energy Capacity: {playerCountry.Yield.EnergyCapacity:F1} | State Capacity Score: {playerCountry.StateCapacityScore:P0}");
        printLine($"Current Simulation Time: {currentTime} (WAT)");
        printLine("--------------------------------------------------------------");

        printLine("All National Development Projects:");
        if (playerCountry.Projects.Count == 0)
        {
            printLine(" (No development projects have been initiated yet)");
        }
        else
        {
            for (var i = 0; i < playerCountry.Projects.Count; i++)
            {
                var p = playerCountry.Projects[i];
                var status = p.Completed ? "COMPLETED" : $"UNDER CONSTRUCTION ({p.GetProgress(currentTime):P1})";
                printLine($" [{i + 1}] {p.Name} | Cost: R{p.Cost:N0} | Status: {status} | Started: {p.StartedBoundary} | Finishes: {p.FinishBoundary}");
            }
        }
        printLine("--------------------------------------------------------------");

        printLine("National Power Plant Specification:");
        printLine(" - Scope: Major municipal grid infrastructure providing base-load energy.");
        printLine($" - Cost: R{NationalPowerPlantProject.StandardCost:N0} (withdrawn once from sovereign treasury)");
        printLine(" - Construction Duration: 72 hours (12 six-hour simulation boundaries)");
        printLine($" - National Yield Bonus: +{NationalPowerPlantProject.EnergyCapacityBonus:F1} Energy Capacity upon completion");

        var powerPlant = playerCountry.PowerPlantProject;
        var activePlant = playerCountry.ActivePowerPlantProject;

        if (activePlant != null)
        {
            var canComplete = currentTime >= activePlant.FinishBoundary;
            var readyStatus = canComplete ? "READY TO COMPLETE / COMMISSION" : "IN PROGRESS";
            printLine($" - Status: UNDER CONSTRUCTION ({readyStatus})");
            printLine($" - Start Time: {activePlant.StartedBoundary}");
            printLine($" - Expected Completion Time: {activePlant.FinishBoundary}");
            printLine($" - Current Progress: {activePlant.GetProgress(currentTime):P1}");
        }
        else if (powerPlant is { Completed: true })
        {
            printLine(" - Status: COMPLETED (Operational)");
            printLine($" - Start Time: {powerPlant.StartedBoundary}");
            printLine($" - Expected Completion Time: {powerPlant.FinishBoundary}");
            printLine(" - Current Progress: 100.0%");
        }
        else
        {
            printLine(" - Status: NOT STARTED");
            printLine(" - Start Time: N/A");
            printLine(" - Expected Completion Time: N/A");
            printLine(" - Current Progress: 0.0%");
        }
        printLine("--------------------------------------------------------------");

        printLine("Options:");
        printLine($" [1] Start National Power Plant (Cost: R{NationalPowerPlantProject.StandardCost:N0} | 72h / 12 boundaries | +{NationalPowerPlantProject.EnergyCapacityBonus:F1} Energy)");
        printLine(" [2] Complete National Power Plant (Commission Facility & Elevate Energy Capacity)");
        printLine(" [0] Return to Executive Directive Menu");
        print("\nSelect Option (1, 2, 0) > ");

        var selection = read()?.Trim().ToUpperInvariant();

        if (selection == "1" || selection == "P" || selection == "POWER")
        {
            if (activePlant != null)
            {
                printLine("\n[ACTION REJECTED]: A National Power Plant project is already active and under construction!");
                printLine($"Started: {activePlant.StartedBoundary} | Expected Finish: {activePlant.FinishBoundary} | Current Progress: {activePlant.GetProgress(currentTime):P1}.");
            }
            else if (playerCountry.Treasury.Balance < NationalPowerPlantProject.StandardCost)
            {
                printLine("\n[ACTION REJECTED]: Insufficient funds in sovereign treasury!");
                printLine($"Required: R{NationalPowerPlantProject.StandardCost:N0} REPU | Available: {playerCountry.Treasury.FormattedBalance} (R{playerCountry.Treasury.Balance:N0} REPU).");
                printLine("The national development project cannot be initiated without adequate liquid capital.");
            }
            else
            {
                printLine("\n--- EXECUTIVE CONFIRMATION REQUIRED ---");
                printLine("Project: National Power Plant");
                printLine($"Cost: R{NationalPowerPlantProject.StandardCost:N0} REPU (will be deducted immediately from sovereign treasury)");
                printLine($"Treasury Available: {playerCountry.Treasury.FormattedBalance} (R{playerCountry.Treasury.Balance:N0} REPU)");
                printLine($"Treasury Remaining After Start: R{(playerCountry.Treasury.Balance - NationalPowerPlantProject.StandardCost):N0} REPU");
                printLine("Construction Duration: 72 hours (12 six-hour simulation boundaries)");
                printLine($"Intended National Benefit: +{NationalPowerPlantProject.EnergyCapacityBonus:F1} Energy Capacity upon completion");
                print("\nInitiate project and deduct R1,000,000 REPU from sovereign treasury? (Y/N) > ");

                var confirm = read()?.Trim().ToUpperInvariant();
                if (confirm == "Y" || confirm == "YES")
                {
                    var newProject = playerCountry.StartPowerPlantProject(currentTime);
                    if (newProject != null)
                    {
                        printLine("\n[CONSTRUCTION COMMENCED]: National Power Plant initiated successfully!");
                        printLine($"Treasury Deducted: R{NationalPowerPlantProject.StandardCost:N0} REPU | Remaining Balance: {playerCountry.Treasury.FormattedBalance} (R{playerCountry.Treasury.Balance:N0} REPU)");
                        printLine($"Started: Day {newProject.StartedBoundary.DayNumber}, {newProject.StartedBoundary.Time:HH:mm} WAT | Expected Completion: Day {newProject.FinishBoundary.DayNumber}, {newProject.FinishBoundary.Time:HH:mm} WAT (72 hours on Republic clock)");
                        printLine($"Current Progress: {newProject.GetProgress(currentTime):P1}");
                    }
                    else
                    {
                        printLine("\n[ACTION REJECTED]: Failed to initiate National Power Plant project.");
                    }
                }
                else
                {
                    printLine("\n[ACTION CANCELLED]: Project initiation cancelled by executive decision. Zero funds were deducted.");
                }
            }

            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        if (selection == "2" || selection == "C" || selection == "COMPLETE")
        {
            if (activePlant == null)
            {
                if (powerPlant is { Completed: true })
                {
                    printLine("\n[NOTICE]: The National Power Plant is already completed and fully operational (+5.0 Energy Capacity active).");
                }
                else
                {
                    printLine("\n[NOTICE]: No National Power Plant is currently under construction. Initiate one first with option [1].");
                }
            }
            else if (currentTime < activePlant.FinishBoundary)
            {
                printLine("\n[COMPLETION REJECTED]: Construction has not yet finished!");
                printLine($"Current Simulation Time: {currentTime} | Expected Completion Time: {activePlant.FinishBoundary}");
                printLine($"Current Progress: {activePlant.GetProgress(currentTime):P1}. Construction requires 72 hours on the Republic simulation clock before commissioning.");
            }
            else
            {
                var success = playerCountry.CompletePowerPlantProject(currentTime);
                if (success)
                {
                    printLine("\n[PROJECT COMPLETED]: National Power Plant successfully commissioned!");
                    printLine($"Energy Capacity Bonus Applied: +{NationalPowerPlantProject.EnergyCapacityBonus:F1}");
                    printLine($"New National Energy Capacity: {playerCountry.Yield.EnergyCapacity:F1}");
                    printLine($"Recalculated State Capacity Score: {playerCountry.StateCapacityScore:P0}");
                }
                else
                {
                    printLine("\n[ACTION REJECTED]: Failed to complete National Power Plant project.");
                }
            }

            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        await Task.CompletedTask;
    }

    public static async Task ManageDiplomacyAsync(
        RepublicApplication app,
        Func<string?>? readLine = null,
        Action<string>? write = null,
        Action<string>? writeLine = null)
    {
        var read = readLine ?? Console.ReadLine;
        var print = write ?? Console.Write;
        var printLine = writeLine ?? Console.WriteLine;

        printLine("==============================================================");
        printLine("             DIPLOMACY & INTERNATIONAL RELATIONS              ");
        printLine("==============================================================");

        var allCountries = app.WorldManager.Countries.GetAllCountries();
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? allCountries.FirstOrDefault();
        var currentTime = app.TimeSystem.CurrentRepublicTime;

        if (playerCountry == null)
        {
            printLine("No sovereign player country available.");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        printLine($"Home Nation: {playerCountry.Name} (ID: {playerCountry.Id})");
        printLine($"Current Simulation Time: {currentTime} (WAT)");
        printLine("--------------------------------------------------------------");
        printLine("Available Sovereign Nations in World:");

        if (allCountries.Count == 0)
        {
            printLine(" (No countries registered in the world)");
            printLine("\nPress Enter to return to main menu...");
            read();
            return;
        }

        for (var i = 0; i < allCountries.Count; i++)
        {
            var country = allCountries[i];
            var isPlayer = string.Equals(country.Id, playerCountry.Id, StringComparison.OrdinalIgnoreCase);
            var tag = isPlayer ? " [YOUR NATION - CANNOT TARGET]" : "";
            var existingRel = playerCountry.GetRelation(country.Id);
            var relStatus = existingRel != null
                ? $" | Relation: {existingRel.Status}{(existingRel.IsRecognized && existingRel.RecognizedBoundary.HasValue ? $" (at {existingRel.RecognizedBoundary.Value})" : "")}"
                : " | Relation: None";
            var trade = playerCountry.GetAgreementWith(country.Id, DiplomaticAgreementType.Trade);
            var tradeStatus = trade != null ? $" | Trade: {trade.Status}" : "";
            var alliance = playerCountry.GetAgreementWith(country.Id, DiplomaticAgreementType.Alliance);
            var allianceStatus = alliance != null ? $" | Alliance: {alliance.Status}" : "";
            printLine($" [{i + 1}] {country.Name} (ID: {country.Id}){tag}{relStatus}{tradeStatus}{allianceStatus}");
        }

        var pendingProposals = playerCountry.PendingAgreements.ToList();
        if (pendingProposals.Count > 0)
        {
            printLine("--------------------------------------------------------------");
            printLine($"PENDING DIPLOMATIC PROPOSALS ({pendingProposals.Count} pending):");
            for (var p = 0; p < pendingProposals.Count; p++)
            {
                var prop = pendingProposals[p];
                var otherId = string.Equals(prop.SourceCountryId, playerCountry.Id, StringComparison.OrdinalIgnoreCase)
                    ? prop.TargetCountryId
                    : prop.SourceCountryId;
                var otherName = allCountries.FirstOrDefault(c => c.Id == otherId)?.Name ?? otherId;
                printLine($"  * {prop.AgreementType} Agreement with {otherName} (proposed at {prop.ProposedBoundary})");
            }
        }

        printLine("--------------------------------------------------------------");
        printLine("Options:");
        printLine($" [1-{allCountries.Count}] Select Country to Inspect & Manage Diplomacy");
        if (pendingProposals.Count > 0)
        {
            printLine(" [P] Review & Respond to Pending Proposals");
        }
        printLine(" [0] Return to Executive Directive Menu");
        printLine("--------------------------------------------------------------");
        print($"Select Option (1-{allCountries.Count}, 0 to cancel) > ");

        var input = read()?.Trim();
        if (string.IsNullOrWhiteSpace(input) || input == "0")
        {
            printLine("Diplomatic review concluded. Returning to executive menu.");
            return;
        }

        if (string.Equals(input, "P", StringComparison.OrdinalIgnoreCase))
        {
            await ResolvePendingProposalsAsync(playerCountry, allCountries, currentTime, read, print, printLine);
            return;
        }

        if (!int.TryParse(input, out var choiceIdx))
        {
            printLine("Invalid input: Selection must be a valid number. Returning to executive menu.");
            return;
        }

        var (cancelled, selectedCountry, error) = BilateralDiplomacyHandler.SelectTarget(choiceIdx, allCountries, playerCountry);
        if (cancelled)
        {
            printLine("Diplomatic review concluded. Returning to executive menu.");
            return;
        }

        if (selectedCountry == null)
        {
            printLine($"\n[REJECTED]: {error}");
            printLine("Returning to executive menu.");
            return;
        }

        await InspectAndManageCountryAsync(playerCountry, selectedCountry, allCountries, currentTime, read, print, printLine);
    }

    private static async Task InspectAndManageCountryAsync(
        Country playerCountry,
        Country selectedCountry,
        IReadOnlyList<Country> allCountries,
        RepublicTime currentTime,
        Func<string?> read,
        Action<string> print,
        Action<string> printLine)
    {
        printLine("==============================================================");
        printLine($"DIPLOMATIC DOSSIER: {selectedCountry.Name} (ID: {selectedCountry.Id})");
        printLine("==============================================================");
        printLine($" Capital: {selectedCountry.CapitalCity} | Government: {selectedCountry.GovernmentType}");
        printLine($" Baseline Stability: {selectedCountry.BaselineStability:0.#}% | GDP: R{selectedCountry.GrossDomesticProduct:N0}");
        printLine($" Treasury: {selectedCountry.Treasury.FormattedBalance} | Physical Stockpiles: {selectedCountry.Inventory.FormattedStockpiles}");
        printLine($" Player Treasury: {playerCountry.Treasury.FormattedBalance} | Player Stockpiles: {playerCountry.Inventory.FormattedStockpiles}");

        var currentRelation = playerCountry.GetRelation(selectedCountry.Id);
        if (currentRelation != null)
        {
            var recInfo = currentRelation.IsRecognized
                ? $"Formal recognition recorded at {currentRelation.RecognizedBoundary ?? currentRelation.OpenedBoundary}."
                : "Relation opened but unrecognized.";
            printLine($" Relation: {currentRelation.Status} (opened at {currentRelation.OpenedBoundary}). {recInfo}");
        }
        else
        {
            printLine(" Relation: None (No diplomatic relation established)");
        }

        var tradeAgreement = playerCountry.GetAgreementWith(selectedCountry.Id, DiplomaticAgreementType.Trade);
        var tradeDetails = tradeAgreement != null
            ? $"{tradeAgreement.Status}{(tradeAgreement.Terms != null ? $" [Terms: {tradeAgreement.Terms}]" : "")} (proposed at {tradeAgreement.ProposedBoundary}{(tradeAgreement.DecidedBoundary.HasValue ? $", decided at {tradeAgreement.DecidedBoundary.Value}" : "")})"
            : "None";
        printLine($" Trade Agreement: {tradeDetails}");

        var alliance = playerCountry.GetAgreementWith(selectedCountry.Id, DiplomaticAgreementType.Alliance);
        printLine($" Bilateral Alliance: {(alliance != null ? $"{alliance.Status} (proposed at {alliance.ProposedBoundary}{(alliance.DecidedBoundary.HasValue ? $", decided at {alliance.DecidedBoundary.Value}" : "")})" : "None")}");

        var pendingWithTarget = playerCountry.Agreements
            .FirstOrDefault(a => a.Involves(selectedCountry.Id) && a.IsPending);

        printLine("--------------------------------------------------------------");
        printLine("Available Diplomatic Actions:");
        if (currentRelation == null)
        {
            printLine($" [1] Open Diplomatic Relation with {selectedCountry.Name}");
        }
        else if (!currentRelation.IsRecognized)
        {
            printLine($" [2] Formally Recognize Diplomatic Relation with {selectedCountry.Name}");
        }

        if (currentRelation != null && (tradeAgreement == null || tradeAgreement.IsRejected || tradeAgreement.IsCompleted))
        {
            printLine($" [3] Propose Bilateral Trade Deal to {selectedCountry.Name}");
        }

        if (currentRelation != null && currentRelation.IsRecognized && (alliance == null || alliance.IsRejected))
        {
            printLine($" [4] Propose Bilateral Alliance to {selectedCountry.Name}");
        }

        if (pendingWithTarget != null)
        {
            printLine($" [5] Resolve Pending {pendingWithTarget.AgreementType} Proposal with {selectedCountry.Name}");
        }

        printLine(" [0] Cancel / Return to Executive Menu");
        print("Select Action > ");

        var actionChoice = read()?.Trim();
        if (actionChoice == "0" || string.IsNullOrWhiteSpace(actionChoice))
        {
            printLine("Action cancelled. Returning to executive menu.");
            return;
        }

        if (actionChoice == "1" && currentRelation == null)
        {
            var outcome = BilateralDiplomacyHandler.OpenRelation(playerCountry, selectedCountry.Id, allCountries, currentTime);
            printLine(outcome.IsSuccess ? $"\n[DIPLOMATIC SUCCESS]: {outcome.Message}" : $"\n[DIPLOMATIC REJECTION]: {outcome.Message}");
        }
        else if (actionChoice == "2" || (actionChoice == "1" && currentRelation != null && !currentRelation.IsRecognized))
        {
            var outcome = BilateralDiplomacyHandler.RecognizeRelation(playerCountry, selectedCountry.Id, allCountries, currentTime);
            printLine(outcome.IsSuccess ? $"\n[DIPLOMATIC SUCCESS]: {outcome.Message}" : $"\n[DIPLOMATIC REJECTION]: {outcome.Message}");
        }
        else if (actionChoice == "3")
        {
            printLine("\n--- PROPOSE BILATERAL TRADE DEAL ---");
            printLine($"Player Reserves: {playerCountry.Treasury.FormattedBalance} | Stockpiles: {playerCountry.Inventory.FormattedStockpiles}");
            printLine($"{selectedCountry.Name} Reserves: {selectedCountry.Treasury.FormattedBalance} | Stockpiles: {selectedCountry.Inventory.FormattedStockpiles}");
            printLine("Select Trade Direction:");
            printLine($" [1] Export: {playerCountry.Name} sells goods to {selectedCountry.Name} for REPU");
            printLine($" [2] Import: {playerCountry.Name} buys goods from {selectedCountry.Name} with REPU");
            printLine(" [0] Cancel");
            print("Direction (1=Export, 2=Import, 0=Cancel) > ");
            var dirInput = read()?.Trim();
            if (dirInput != "1" && dirInput != "2")
            {
                printLine("Trade proposal cancelled.");
                return;
            }

            var playerIsSeller = dirInput == "1";
            var seller = playerIsSeller ? playerCountry : selectedCountry;
            var buyer = playerIsSeller ? selectedCountry : playerCountry;

            print("Resource Commodity Type (e.g. Iron, Timber, Petroleum, Copper, Bauxite) > ");
            var resInput = read()?.Trim();
            if (string.IsNullOrWhiteSpace(resInput))
            {
                printLine("Resource type cannot be empty. Proposal cancelled.");
                return;
            }

            print("Unit Quantity to Trade > ");
            if (!double.TryParse(read()?.Trim(), out var qty) || qty <= 0.0)
            {
                printLine("Quantity must be a positive number. Proposal cancelled.");
                return;
            }

            print("Total Agreed Price in REPU > ");
            if (!double.TryParse(read()?.Trim(), out var price) || price <= 0.0)
            {
                printLine("Price must be a positive number. Proposal cancelled.");
                return;
            }

            try
            {
                var terms = new BilateralTradeTerms(seller.Id, buyer.Id, resInput, qty, price);
                var outcome = BilateralDiplomacyHandler.ProposeTradeDeal(playerCountry, selectedCountry.Id, terms, allCountries, currentTime);
                printLine(outcome.IsSuccess ? $"\n[TRADE PROPOSAL SUBMITTED]: {outcome.Message}" : $"\n[PROPOSAL REJECTED]: {outcome.Message}");
            }
            catch (Exception ex)
            {
                printLine($"\n[PROPOSAL REJECTED]: {ex.Message}");
            }
        }
        else if (actionChoice == "4")
        {
            var outcome = BilateralDiplomacyHandler.ProposeAgreement(playerCountry, selectedCountry.Id, DiplomaticAgreementType.Alliance, allCountries, currentTime);
            printLine(outcome.IsSuccess ? $"\n[PROPOSAL SUBMITTED]: {outcome.Message}" : $"\n[PROPOSAL REJECTED]: {outcome.Message}");
        }
        else if (actionChoice == "5" && pendingWithTarget != null)
        {
            if (pendingWithTarget.Terms != null)
            {
                printLine($"\nPending Bilateral Trade Deal with {selectedCountry.Name}:");
                printLine($"  Commodity: {pendingWithTarget.Terms.Quantity:N0} units of {pendingWithTarget.Terms.ResourceType}");
                printLine($"  Agreed Price: {pendingWithTarget.Terms.FormattedPrice}");
                var s = allCountries.FirstOrDefault(c => c.Id == pendingWithTarget.Terms.SellerCountryId);
                var b = allCountries.FirstOrDefault(c => c.Id == pendingWithTarget.Terms.BuyerCountryId);
                printLine($"  Seller (delivering goods): {s?.Name ?? pendingWithTarget.Terms.SellerCountryId} (Has: {s?.Inventory.GetQuantity(pendingWithTarget.Terms.ResourceType):N0} units)");
                printLine($"  Buyer (paying REPU): {b?.Name ?? pendingWithTarget.Terms.BuyerCountryId} (Has: {b?.Treasury.FormattedBalance})");
            }
            else
            {
                printLine($"\nPending {pendingWithTarget.AgreementType} proposal with {selectedCountry.Name}:");
            }

            printLine(" [1] Accept Proposal (Execute Trade/Agreement)");
            printLine(" [2] Reject Proposal");
            printLine(" [0] Cancel");
            print("Decision (1=Accept, 2=Reject, 0=Cancel) > ");
            var decInput = read()?.Trim();
            if (decInput == "1")
            {
                var outcome = BilateralDiplomacyHandler.RespondToAgreement(playerCountry, pendingWithTarget.Id, accept: true, allCountries, currentTime);
                printLine(outcome.IsSuccess ? $"\n[SUCCESS]: {outcome.Message}" : $"\n[REJECTION]: {outcome.Message}");
                if (outcome.IsSuccess && pendingWithTarget.Terms != null)
                {
                    printLine($"  {playerCountry.Name} New Position -> Treasury: {playerCountry.Treasury.FormattedBalance} | Inventory: {playerCountry.Inventory.FormattedStockpiles}");
                    printLine($"  {selectedCountry.Name} New Position -> Treasury: {selectedCountry.Treasury.FormattedBalance} | Inventory: {selectedCountry.Inventory.FormattedStockpiles}");
                }
            }
            else if (decInput == "2")
            {
                var outcome = BilateralDiplomacyHandler.RespondToAgreement(playerCountry, pendingWithTarget.Id, accept: false, allCountries, currentTime);
                printLine(outcome.IsSuccess ? $"\n[PROPOSAL REJECTED]: {outcome.Message}" : $"\n[REJECTION]: {outcome.Message}");
            }
            else
            {
                printLine("Decision cancelled.");
            }
        }
        else
        {
            printLine("Invalid option or action not eligible at this time.");
        }

        await Task.CompletedTask;
    }

    private static async Task ResolvePendingProposalsAsync(
        Country playerCountry,
        IReadOnlyList<Country> allCountries,
        RepublicTime currentTime,
        Func<string?> read,
        Action<string> print,
        Action<string> printLine)
    {
        var pending = playerCountry.PendingAgreements.ToList();
        if (pending.Count == 0)
        {
            printLine("No pending proposals requiring action.");
            return;
        }

        printLine("==============================================================");
        printLine("             PENDING DIPLOMATIC PROPOSALS                     ");
        printLine("==============================================================");
        for (var i = 0; i < pending.Count; i++)
        {
            var p = pending[i];
            var otherId = string.Equals(p.SourceCountryId, playerCountry.Id, StringComparison.OrdinalIgnoreCase)
                ? p.TargetCountryId
                : p.SourceCountryId;
            var otherName = allCountries.FirstOrDefault(c => c.Id == otherId)?.Name ?? otherId;
            var direction = string.Equals(p.SourceCountryId, playerCountry.Id, StringComparison.OrdinalIgnoreCase)
                ? $"Proposed by {playerCountry.Name} to {otherName}"
                : $"Proposed by {otherName} to {playerCountry.Name}";
            var termsInfo = p.Terms != null ? $" | Terms: {p.Terms}" : "";
            printLine($" [{i + 1}] {p.AgreementType} Agreement | {direction}{termsInfo} | Boundary: {p.ProposedBoundary}");
        }
        printLine(" [0] Cancel / Return");
        print($"Select Proposal to Resolve (1-{pending.Count}, 0 to cancel) > ");

        var input = read()?.Trim();
        if (string.IsNullOrWhiteSpace(input) || input == "0" || !int.TryParse(input, out var idx) || idx < 1 || idx > pending.Count)
        {
            printLine("Cancelled.");
            return;
        }

        var selected = pending[idx - 1];
        if (selected.Terms != null)
        {
            printLine($"\nSelected: Bilateral Trade Deal (ID: {selected.Id})");
            printLine($"  Commodity: {selected.Terms.Quantity:N0} units of {selected.Terms.ResourceType}");
            printLine($"  Agreed Price: {selected.Terms.FormattedPrice}");
            var seller = allCountries.FirstOrDefault(c => c.Id == selected.Terms.SellerCountryId);
            var buyer = allCountries.FirstOrDefault(c => c.Id == selected.Terms.BuyerCountryId);
            printLine($"  Seller (delivering goods): {seller?.Name ?? selected.Terms.SellerCountryId} (Available: {seller?.Inventory.GetQuantity(selected.Terms.ResourceType):N0} units)");
            printLine($"  Buyer (paying REPU): {buyer?.Name ?? selected.Terms.BuyerCountryId} (Available: {buyer?.Treasury.FormattedBalance})");
        }
        else
        {
            printLine($"\nSelected: {selected.AgreementType} Agreement (ID: {selected.Id})");
        }

        printLine(" [1] Accept Proposal (Execute Trade/Agreement)");
        printLine(" [2] Reject Proposal");
        printLine(" [0] Cancel");
        print("Decision (1=Accept, 2=Reject, 0=Cancel) > ");

        var decInput = read()?.Trim();
        if (decInput == "1")
        {
            var outcome = BilateralDiplomacyHandler.RespondToAgreement(playerCountry, selected.Id, accept: true, allCountries, currentTime);
            printLine(outcome.IsSuccess ? $"\n[SUCCESS]: {outcome.Message}" : $"\n[REJECTION]: {outcome.Message}");
            if (outcome.IsSuccess && selected.Terms != null)
            {
                var s = allCountries.FirstOrDefault(c => c.Id == selected.Terms.SellerCountryId);
                var b = allCountries.FirstOrDefault(c => c.Id == selected.Terms.BuyerCountryId);
                if (s != null && b != null)
                {
                    printLine("\n--- UPDATED ECONOMIC POSITIONS ---");
                    printLine($"  {s.Name} (Seller) -> Treasury: {s.Treasury.FormattedBalance} | {selected.Terms.ResourceType}: {s.Inventory.GetQuantity(selected.Terms.ResourceType):N0} units");
                    printLine($"  {b.Name} (Buyer)  -> Treasury: {b.Treasury.FormattedBalance} | {selected.Terms.ResourceType}: {b.Inventory.GetQuantity(selected.Terms.ResourceType):N0} units");
                }
            }
        }
        else if (decInput == "2")
        {
            var outcome = BilateralDiplomacyHandler.RespondToAgreement(playerCountry, selected.Id, accept: false, allCountries, currentTime);
            printLine(outcome.IsSuccess ? $"\n[PROPOSAL REJECTED]: {outcome.Message}" : $"\n[REJECTION]: {outcome.Message}");
        }
        else
        {
            printLine("Decision cancelled.");
        }

        await Task.CompletedTask;
    }

    private static void PrintPresidentialBriefing(OfflineYieldBriefing briefing)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("==============================================================");
        Console.WriteLine("        PRESIDENTIAL BRIEFING: OFFLINE YIELD RECOVERY         ");
        Console.WriteLine("==============================================================");
        Console.ResetColor();
        Console.WriteLine(" Welcome back, Mr. President.");
        Console.WriteLine($" Country ID: {briefing.CountryId}");
        Console.WriteLine($" Offline Period: {briefing.FromTime} -> {briefing.ToTime}");
        Console.WriteLine($" - Production Boundaries Credited: {briefing.BoundariesCredited}");
        Console.WriteLine($" - Sovereign REPU Deposited: {briefing.FormattedRepuDeposited}");
        if (briefing.RemainingBoundaries > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" - Remaining Uncredited Boundaries: {briefing.RemainingBoundaries} (cap of 8 reached)");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(" - Remaining Due Boundaries: 0");
        }
        Console.WriteLine($" - Next Due WAT Boundary: Day {briefing.NextDueBoundary.DayNumber}, {briefing.NextDueBoundary.Time:HH\\:mm} WAT");
        if (briefing.ActiveFoundingBuffs != null)
        {
            Console.WriteLine($" - Active Founding Buffs: Admin: {briefing.ActiveFoundingBuffs.AdministrativeEfficiency:P0}, Innovation: {briefing.ActiveFoundingBuffs.InnovationDrive:P0}, Dev: {briefing.ActiveFoundingBuffs.DevelopmentInitiative:P0}, Investor: {briefing.ActiveFoundingBuffs.InvestorConfidence:P0}, Diplo: {briefing.ActiveFoundingBuffs.DiplomaticRecognitionMomentum:P0}, Inst: {briefing.ActiveFoundingBuffs.InstitutionBuilding:P0}, Unity: {briefing.ActiveFoundingBuffs.TemporaryNationalUnity:P0}");
        }
        if (briefing.RoadProgramCompleted)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" - Development: National Road Program completed! Infrastructure condition increased by +10%.");
            Console.ResetColor();
        }
        if (briefing.ProjectsCompletedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" - Development: {briefing.ProjectsCompletedCount} capital development project(s) completed and commissioned!");
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("==============================================================");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void ApplyStartupGapIfSpecified(RepublicApplication app, string[] args)
    {
        if (args == null || args.Length == 0) return;
        if (app.Clock is not IControlledRepublicClock controlledClock) return;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if ((arg.Equals("--gap-hours", StringComparison.OrdinalIgnoreCase)
                 || arg.Equals("--gap", StringComparison.OrdinalIgnoreCase)
                 || arg.Equals("--advance-hours", StringComparison.OrdinalIgnoreCase))
                && i + 1 < args.Length)
            {
                if (double.TryParse(args[i + 1], out var hours) && hours > 0)
                {
                    controlledClock.Advance(TimeSpan.FromHours(hours));
                    return;
                }
            }
            else if (arg.StartsWith("--gap-hours=", StringComparison.OrdinalIgnoreCase)
                     || arg.StartsWith("--gap=", StringComparison.OrdinalIgnoreCase)
                     || arg.StartsWith("--advance-hours=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Split('=')[1];
                if (double.TryParse(val, out var hours) && hours > 0)
                {
                    controlledClock.Advance(TimeSpan.FromHours(hours));
                    return;
                }
            }
            else if (double.TryParse(arg, out var directHours) && directHours > 0)
            {
                controlledClock.Advance(TimeSpan.FromHours(directHours));
                return;
            }
        }
    }

    private static void ApplyStartupPartyIfSpecified(RepublicApplication app, string[] args)
    {
        if (args == null || args.Length == 0) return;
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        if (playerCountry == null) return;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if ((arg.Equals("--party", StringComparison.OrdinalIgnoreCase) || arg.Equals("--ruling", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                playerCountry.FoundParty(args[i + 1], app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.StartsWith("--party=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--party=".Length);
                playerCountry.FoundParty(val, app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.StartsWith("--ruling=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--ruling=".Length);
                playerCountry.FoundParty(val, app.TimeSystem.CurrentRepublicTime);
            }
            else if ((arg.Equals("--opposition", StringComparison.OrdinalIgnoreCase) || arg.Equals("--party2", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                playerCountry.FoundParty(args[i + 1], app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.StartsWith("--opposition=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--opposition=".Length);
                playerCountry.FoundParty(val, app.TimeSystem.CurrentRepublicTime);
            }
            else if ((arg.Equals("--interest-group", StringComparison.OrdinalIgnoreCase) || arg.Equals("--interestgroup", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                playerCountry.FoundInterestGroup(args[i + 1], InterestGroupFocus.Labor);
            }
            else if (arg.StartsWith("--interest-group=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--interest-group=".Length);
                playerCountry.FoundInterestGroup(val, InterestGroupFocus.Labor);
            }
            else if (arg.StartsWith("--interestgroup=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--interestgroup=".Length);
                playerCountry.FoundInterestGroup(val, InterestGroupFocus.Labor);
            }
            else if (arg.Equals("--bill", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                playerCountry.ProposeBill(args[i + 1]);
            }
            else if (arg.StartsWith("--bill=", StringComparison.OrdinalIgnoreCase))
            {
                var val = arg.Substring("--bill=".Length);
                playerCountry.ProposeBill(val);
            }
            else if (arg.Equals("--vote-bill", StringComparison.OrdinalIgnoreCase))
            {
                playerCountry.VoteBill(app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.Equals("--hold-election", StringComparison.OrdinalIgnoreCase) || arg.Equals("--election", StringComparison.OrdinalIgnoreCase))
            {
                playerCountry.HoldElection(app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.Equals("--relation", StringComparison.OrdinalIgnoreCase) || arg.Equals("--open-relation", StringComparison.OrdinalIgnoreCase))
            {
                var otherCountry = app.WorldManager.Countries.GetAllCountries().FirstOrDefault(c => c.Id != playerCountry.Id);
                if (otherCountry != null)
                {
                    playerCountry.OpenRelation(otherCountry.Id, app.TimeSystem.CurrentRepublicTime);
                }
            }
            else if (arg.StartsWith("--relation=", StringComparison.OrdinalIgnoreCase))
            {
                var targetId = arg.Substring("--relation=".Length);
                playerCountry.OpenRelation(targetId, app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.Equals("--recognize", StringComparison.OrdinalIgnoreCase) || arg.Equals("--recognize-relation", StringComparison.OrdinalIgnoreCase))
            {
                var other = app.WorldManager.Countries.GetAllCountries().FirstOrDefault(c => c.Id != playerCountry.Id && playerCountry.HasRelation(c.Id));
                if (other != null)
                {
                    playerCountry.RecognizeRelation(other.Id, app.TimeSystem.CurrentRepublicTime);
                }
            }
            else if (arg.StartsWith("--recognize=", StringComparison.OrdinalIgnoreCase))
            {
                var targetId = arg.Substring("--recognize=".Length);
                playerCountry.RecognizeRelation(targetId, app.TimeSystem.CurrentRepublicTime);
            }
            else if (arg.Equals("--trade", StringComparison.OrdinalIgnoreCase) || arg.Equals("--trade-agreement", StringComparison.OrdinalIgnoreCase))
            {
                var other = app.WorldManager.Countries.GetAllCountries().FirstOrDefault(c => c.Id != playerCountry.Id && playerCountry.HasRelation(c.Id));
                if (other != null)
                {
                    playerCountry.ProposeTradeAgreement(other, app.TimeSystem.CurrentRepublicTime);
                }
            }
            else if (arg.Equals("--alliance", StringComparison.OrdinalIgnoreCase))
            {
                var other = app.WorldManager.Countries.GetAllCountries().FirstOrDefault(c => c.Id != playerCountry.Id && playerCountry.HasRelation(c.Id));
                if (other != null)
                {
                    playerCountry.ProposeAlliance(other, app.TimeSystem.CurrentRepublicTime);
                }
            }
        }
    }

    /// <summary>
    /// Prompts the user for a sovereign country name containing the word 'Republic'.
    /// Prints the rejection message and re-prompts until a valid name is provided.
    /// Does not add a new menu number.
    /// </summary>
    public static string PromptCountryName(Func<string?>? readLine = null, Action<string>? write = null, Action<string>? writeLine = null)
    {
        var read = readLine ?? Console.ReadLine;
        var print = write ?? Console.Write;
        var printLine = writeLine ?? Console.WriteLine;

        while (true)
        {
            print("Enter Sovereign Country Name > ");
            var input = read()?.Trim();
            if (CountryNameRule.Default.IsValid(input))
            {
                return CountryNameRule.Default.Validate(input);
            }

            printLine($"Invalid country name. Every player country name must contain the whole word '{CountryNameRule.RequiredKeyword}'.");
        }
    }
}
