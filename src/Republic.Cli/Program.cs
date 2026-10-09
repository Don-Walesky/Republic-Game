namespace Republic.Cli;

using System;
using System.Linq;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Economy.Treasury;
using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Elections.Services;
using Republic.Core.Government;
using Republic.Core.Government.Models;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Legislature.Models;
using Republic.Core.Legislature.Services;
using Republic.Core.Military.Models;
using Republic.Core.NationalYield;
using Republic.Core.Scenarios.Services;
using Republic.Core.Time;
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
        var bootstrapperService = new ScenarioBootstrapper(app.WorldManager, app.WorkspaceManager, app.CabinetService, app.LegislatureService);
        await bootstrapperService.BootstrapScenarioAsync("arcadia-day1");

        // Process any startup offline gap specified in CLI arguments
        ApplyStartupGapIfSpecified(app, args);

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
            Console.WriteLine(" [7] Advance Time (1 Tick / 10 Ticks)");
            Console.WriteLine(" [8] Save / Quick-Load Session");
            Console.WriteLine(" [9] Conduct Military & Defense Command Operations");
            Console.WriteLine(" [10] Administer Regional Provinces & Investment");
            Console.WriteLine(" [11] Enact Presidential Executive Decree");
            Console.WriteLine(" [12] Conduct Presidential Press Conference");
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
        var treasury = playerCountry?.Treasury ?? new RepuTreasury(econ.TreasuryBalance);

        var schedule = new NationalYieldSchedule();
        var currentRepuTime = app.TimeSystem.CurrentRepublicTime;
        var nextBoundary = playerCountry != null
            ? schedule.GetNextDueBoundary(playerCountry, currentRepuTime)
            : schedule.GetNextBoundary(currentRepuTime);

        if (briefing != null && briefing.BoundariesCredited > 0)
        {
            PrintPresidentialBriefing(briefing);
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==============================================================");
        Console.WriteLine("          REPUBLIC OF ARCADIA - PRESIDENTIAL DESK            ");
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        Console.WriteLine($" Tick: {tick} | Republic Time: {app.TimeSystem.CurrentRepublicTime} ({app.TimeSystem.CurrentSimulatedDateTime:yyyy-MM-dd HH:mm:ss} UTC)");
        var profile = playerCountry?.Profile ?? CountryProfile.CreateArcadia(currentRepuTime);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" Profile: {profile.OfficialName} | Capital: {profile.Capital} | Region: {profile.Region} | Pop: {profile.Population:N0} | Form: {profile.GovernmentForm} | Resource: {profile.PrimaryResource}");
        Console.ResetColor();

        if (playerCountry != null)
        {
            playerCountry.GrossDomesticProduct = econ.GrossDomesticProduct;
            playerCountry.HappinessRating = demo.HappinessRating;
        }

        var countryForDashboard = playerCountry ?? Country.Found("Republic of Arcadia", currentRepuTime);
        var dashboard = countryForDashboard.GetDashboard(currentRepuTime);

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                    NATIONAL DASHBOARD                        ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        foreach (var line in dashboard.Lines)
        {
            Console.WriteLine($" {line}");
        }
        Console.WriteLine("--------------------------------------------------------------");

        var strength = countryForDashboard.GetNationalStrength();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                     NATIONAL STRENGTH                        ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        foreach (var line in strength.Lines)
        {
            Console.WriteLine($" {line}");
        }
        Console.WriteLine("--------------------------------------------------------------");

        var consequenceText = countryForDashboard.LatestTreasuryConsequence?.ToString() ?? "No consequence";
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("                    SYSTEMIC CONSEQUENCES                     ");
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
        Console.WriteLine($" Consequence: {consequenceText}");
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
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country") ?? app.WorldManager.Countries.GetAllCountries().FirstOrDefault();
        if (playerCountry != null)
        {
            var cycleService = new NationalYieldCycleService();
            cycleService.ExecuteDueCycles(playerCountry, app.TimeSystem.CurrentRepublicTime);
        }
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
