namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Republic.Core.World.Services;

/// <summary>
/// Status outcome classifications for executive presidential workspace actions.
/// </summary>
public enum ExecutiveActionResult
{
    Success,
    Cancelled,
    InsufficientFunds,
    AlreadyActive,
    InvalidCountry,
    PrerequisitesNotMet,
    InvalidInput
}

/// <summary>
/// Domain outcome record returned from an executive workspace action.
/// </summary>
public sealed record ExecutiveActionOutcome(
    ExecutiveActionResult Result,
    string Message,
    DevelopmentProject? Project = null,
    CitizenFeedbackRecord? CitizenFeedback = null,
    FiscalQuarterReport? FiscalReport = null)
{
    /// <summary>
    /// Indicates whether the executive action was successfully executed.
    /// </summary>
    public bool IsSuccess => Result == ExecutiveActionResult.Success;
}

/// <summary>
/// Domain summary record returned from advancing simulation time in the executive workspace.
/// </summary>
public sealed record TimeAdvancementSummary(
    RepublicTime OldTime,
    RepublicTime NewTime,
    TimeSpan ElapsedDuration,
    int YieldCyclesProcessed,
    int ResourceCyclesProcessed,
    int ProjectsCompletedCount,
    IReadOnlyList<DevelopmentProject> CompletedProjects,
    int FiscalQuartersProcessed = 0);

/// <summary>
/// Domain handler coordinating executive workspace actions and simulation time advancement.
/// Enforces sovereign isolation, authoritative clock advancement, single-cost deduction, and once-only project completion.
/// </summary>
public static class PresidentialWorkspaceHandler
{
    /// <summary>
    /// Initiates a National Power Plant development project for the specified sovereign country.
    /// Validates fund availability and explicit executive confirmation before deducting treasury or mutating state.
    /// </summary>
    public static ExecutiveActionOutcome InitiatePowerPlantProject(Country country, RepublicTime simulationTime, bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot initiate development project: invalid or missing sovereign country.");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Project initiation cancelled by executive order. Zero funds deducted.");
        }

        if (country.ActivePowerPlantProject != null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.AlreadyActive,
                $"A National Power Plant project is already under active construction (Started: {country.ActivePowerPlantProject.StartedBoundary}, Scheduled completion: {country.ActivePowerPlantProject.FinishBoundary}).");
        }

        if (country.Treasury.Balance < NationalPowerPlantProject.StandardCost)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InsufficientFunds,
                $"Insufficient sovereign funds. Required: {RepuTreasury.Format(NationalPowerPlantProject.StandardCost)}, Available: {country.Treasury.FormattedBalance}.");
        }

        var project = country.StartPowerPlantProject(simulationTime);
        if (project == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Unable to start National Power Plant project. Verify sovereign treasury and country prerequisites.");
        }

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            $"National Power Plant construction commenced. Deducted {RepuTreasury.Format(project.Cost)} from sovereign treasury. Scheduled completion: {project.FinishBoundary}.",
            project);
    }

    /// <summary>
    /// Initiates a National Road Program development project for the specified sovereign country.
    /// Validates fund availability and explicit executive confirmation before deducting treasury or mutating state.
    /// </summary>
    public static ExecutiveActionOutcome InitiateRoadProject(Country country, RepublicTime simulationTime, bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot initiate development project: invalid or missing sovereign country.");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Project initiation cancelled by executive order. Zero funds deducted.");
        }

        if (country.Projects.OfType<NationalRoadProject>().Any(p => p.IsActive))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.AlreadyActive,
                "A National Road Program is already under active construction.");
        }

        if (country.Treasury.Balance < NationalRoadProject.StandardCost)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InsufficientFunds,
                $"Insufficient sovereign funds. Required: {RepuTreasury.Format(NationalRoadProject.StandardCost)}, Available: {country.Treasury.FormattedBalance}.");
        }

        var project = country.StartRoadProject(simulationTime);
        if (project == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Unable to start National Road Program. Verify sovereign treasury and country prerequisites.");
        }

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            $"National Road Program construction commenced. Deducted {RepuTreasury.Format(project.Cost)} from sovereign treasury. Scheduled completion: {project.FinishBoundary}.",
            project);
    }

    /// <summary>
    /// Initiates a National University development project for the specified sovereign country.
    /// Validates fund availability and explicit executive confirmation before deducting treasury or mutating state.
    /// Costs R750,000 and requires 8 six-hour boundaries (48 hours).
    /// </summary>
    public static ExecutiveActionOutcome InitiateUniversityProject(Country country, RepublicTime simulationTime, bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot initiate development project: invalid or missing sovereign country.");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Project initiation cancelled by executive order. Zero funds deducted.");
        }

        var eduMinister = country.GetMinister(CabinetPortfolio.Education);
        if (eduMinister == null || !eduMinister.IsAppointed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Cannot initiate National University project: requires an appointed Minister of Education.");
        }

        if (country.Projects.OfType<NationalUniversityProject>().Any(p => p.IsActive))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.AlreadyActive,
                "A National University is already under active construction.");
        }

        if (country.Treasury.Balance < NationalUniversityProject.StandardCost)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InsufficientFunds,
                $"Insufficient sovereign funds. Required: {RepuTreasury.Format(NationalUniversityProject.StandardCost)}, Available: {country.Treasury.FormattedBalance}.");
        }

        var project = country.StartUniversityProject(simulationTime);
        if (project == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Unable to start National University. Verify sovereign treasury and country prerequisites.");
        }

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            $"National University construction commenced. Deducted {RepuTreasury.Format(project.Cost)} from sovereign treasury. Scheduled completion: {project.FinishBoundary}.",
            project);
    }

    /// <summary>
    /// Initiates a National Refinery development project for the specified sovereign country.
    /// Validates fund availability and requires an appointed Minister of Oil & Gas.
    /// Costs R1,200,000 and requires 10 six-hour boundaries (60 hours).
    /// </summary>
    public static ExecutiveActionOutcome InitiateRefineryProject(Country country, RepublicTime simulationTime, bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot initiate development project: invalid or missing sovereign country.");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Project initiation cancelled by executive order. Zero funds deducted.");
        }

        var oilMinister = country.GetMinister(CabinetPortfolio.OilAndGas);
        if (oilMinister == null || !oilMinister.IsAppointed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Cannot initiate National Refinery project: requires an appointed Minister of Oil & Gas.");
        }

        if (country.Projects.OfType<NationalRefineryProject>().Any(p => p.IsActive))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.AlreadyActive,
                "A National Refinery is already under active construction.");
        }

        if (country.Treasury.Balance < NationalRefineryProject.StandardCost)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InsufficientFunds,
                $"Insufficient sovereign funds. Required: {RepuTreasury.Format(NationalRefineryProject.StandardCost)}, Available: {country.Treasury.FormattedBalance}.");
        }

        var project = country.StartRefineryProject(simulationTime);
        if (project == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Unable to start National Refinery. Verify sovereign treasury and country prerequisites.");
        }

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            $"National Refinery construction commenced. Deducted {RepuTreasury.Format(project.Cost)} from sovereign treasury. Scheduled completion: {project.FinishBoundary}.",
            project);
    }

    /// <summary>
    /// Initiates a National Mine development project for the specified sovereign country.
    /// Validates fund availability and requires an appointed Minister of Resources.
    /// Costs R800,000 and requires 8 six-hour boundaries (48 hours).
    /// </summary>
    public static ExecutiveActionOutcome InitiateMineProject(Country country, RepublicTime simulationTime, bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot initiate development project: invalid or missing sovereign country.");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Project initiation cancelled by executive order. Zero funds deducted.");
        }

        var resMinister = country.GetMinister(CabinetPortfolio.Resources);
        if (resMinister == null || !resMinister.IsAppointed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Cannot initiate National Mine project: requires an appointed Minister of Resources.");
        }

        if (country.Projects.OfType<NationalMineProject>().Any(p => p.IsActive))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.AlreadyActive,
                "A National Mine is already under active construction.");
        }

        if (country.Treasury.Balance < NationalMineProject.StandardCost)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InsufficientFunds,
                $"Insufficient sovereign funds. Required: {RepuTreasury.Format(NationalMineProject.StandardCost)}, Available: {country.Treasury.FormattedBalance}.");
        }

        var project = country.StartMineProject(simulationTime);
        if (project == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.PrerequisitesNotMet,
                "Unable to start National Mine. Verify sovereign treasury and country prerequisites.");
        }

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            $"National Mine construction commenced. Deducted {RepuTreasury.Format(project.Cost)} from sovereign treasury. Scheduled completion: {project.FinishBoundary}.",
            project);
    }

    /// <summary>
    /// Adjusts the national taxation policy for the specified sovereign country.
    /// Validates inputs, requires explicit presidential confirmation, and evaluates citizen welfare and approval consequences.
    /// </summary>
    public static ExecutiveActionOutcome AdjustTaxPolicy(
        Country country,
        TaxPolicy newPolicy,
        RepublicTime simulationTime,
        bool confirmed = true)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot adjust taxation policy: invalid or missing sovereign country.");
        }

        if (newPolicy == null || !double.IsFinite(newPolicy.IncomeTaxRate) || !double.IsFinite(newPolicy.CorporateTaxRate) || !double.IsFinite(newPolicy.ImportTariffRate))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidInput,
                "Taxation policy rejected: rates must be finite, valid numbers.");
        }

        if (newPolicy.IncomeTaxRate < 0.0 || newPolicy.IncomeTaxRate > 0.70 ||
            newPolicy.CorporateTaxRate < 0.0 || newPolicy.CorporateTaxRate > 0.60 ||
            newPolicy.ImportTariffRate < 0.0 || newPolicy.ImportTariffRate > 0.50)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidInput,
                "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%, Corporate: 0-60%, Tariff: 0-50%).");
        }

        if (!confirmed)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.Cancelled,
                "Taxation policy adjustment cancelled by executive order. Existing tax rates and citizen approval preserved unchanged.");
        }

        var feedback = country.ApplyTaxPolicy(newPolicy, simulationTime);

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            feedback.Description,
            CitizenFeedback: feedback);
    }

    /// <summary>
    /// Processes a quarterly fiscal settlement for the specified sovereign country, calculating applicable tax revenues,
    /// deducting ministry expenditures, depositing surplus or deducting deficit from the sovereign treasury exactly once,
    /// and generating a domain fiscal quarter report.
    /// </summary>
    public static async Task<ExecutiveActionOutcome> ProcessFiscalQuarterAsync(
        Country country,
        IBudgetService budgetService,
        RepublicTime simulationTime,
        CancellationToken cancellationToken = default)
    {
        if (country == null || string.IsNullOrWhiteSpace(country.Id))
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidCountry,
                "Cannot process fiscal quarter: invalid or missing sovereign country.");
        }

        if (budgetService == null)
        {
            return new ExecutiveActionOutcome(
                ExecutiveActionResult.InvalidInput,
                "Cannot process fiscal quarter: missing budget service.");
        }

        var report = await budgetService.ProcessQuarterlyReportAsync(country, simulationTime, cancellationToken).ConfigureAwait(false);

        var summary = $"Fiscal Quarter {report.QuarterIndex} settlement processed. " +
                      $"Revenue: {report.FormattedTotalTaxRevenue}, Expenditure: {report.FormattedTotalExpenditures}, " +
                      $"Net: {report.FormattedNetSurplusOrDeficit}. Sovereign Treasury: {country.Treasury.FormattedBalance}.";

        return new ExecutiveActionOutcome(
            ExecutiveActionResult.Success,
            summary,
            FiscalReport: report);
    }

    /// <summary>
    /// Advances authoritative Republic simulation time by the specified duration, processing all due yield cycles,
    /// domestic resource production cycles, capital development project completions, and fiscal quarters across all registered countries.
    /// </summary>
    public static TimeAdvancementSummary AdvanceTime(
        IWorldManager worldManager,
        ITimeSystem timeSystem,
        TimeSpan duration,
        INationalYieldCycleService? yieldService = null,
        IResourceProductionService? resourceService = null,
        IBudgetService? budgetService = null)
    {
        ArgumentNullException.ThrowIfNull(worldManager);
        ArgumentNullException.ThrowIfNull(timeSystem);

        var oldTime = timeSystem.CurrentRepublicTime;

        if (duration > TimeSpan.Zero)
        {
            if (timeSystem.Clock is IControlledRepublicClock controlledClock)
            {
                controlledClock.Advance(duration);
            }
        }

        var newTime = timeSystem.CurrentRepublicTime;
        var activeYieldService = yieldService ?? new NationalYieldCycleService();
        var activeResourceService = resourceService ?? new ResourceProductionService();

        var yieldCyclesTotal = 0;
        var resourceCyclesTotal = 0;
        var fiscalQuartersTotal = 0;
        var completedProjects = new List<DevelopmentProject>();

        foreach (var country in worldManager.Countries.GetAllCountries())
        {
            // 1. Process due six-hour National Yield cycles
            var yieldSnapshots = activeYieldService.ExecuteDueCycles(country, newTime);
            yieldCyclesTotal += yieldSnapshots.Count;

            // 2. Process due domestic resource production cycles
            var resourceResults = activeResourceService.ProduceDueCycles(country, newTime);
            resourceCyclesTotal += resourceResults.Count(r => r.Success);

            // 3. Process completed development projects
            foreach (var proj in country.Projects.Where(p => p.IsActive).ToList())
            {
                if (proj.CanComplete(newTime) && country.CompleteProject(proj, newTime))
                {
                    completedProjects.Add(proj);
                }
            }

            // 4. Process due simulation-time fiscal cycles
            if (budgetService != null)
            {
                var fiscalReports = budgetService.ProcessDueQuartersAsync(country, newTime).GetAwaiter().GetResult();
                fiscalQuartersTotal += fiscalReports.Count;
            }
        }

        return new TimeAdvancementSummary(
            oldTime,
            newTime,
            duration,
            yieldCyclesTotal,
            resourceCyclesTotal,
            completedProjects.Count,
            completedProjects.AsReadOnly(),
            fiscalQuartersTotal);
    }

    /// <summary>
    /// Asynchronously advances authoritative Republic simulation time by the specified duration, processing all due yield cycles,
    /// domestic resource production cycles, capital development project completions, and fiscal quarters across all registered countries.
    /// </summary>
    public static async Task<TimeAdvancementSummary> AdvanceTimeAsync(
        IWorldManager worldManager,
        ITimeSystem timeSystem,
        TimeSpan duration,
        INationalYieldCycleService? yieldService = null,
        IResourceProductionService? resourceService = null,
        IBudgetService? budgetService = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worldManager);
        ArgumentNullException.ThrowIfNull(timeSystem);

        var oldTime = timeSystem.CurrentRepublicTime;

        if (duration > TimeSpan.Zero)
        {
            if (timeSystem.Clock is IControlledRepublicClock controlledClock)
            {
                controlledClock.Advance(duration);
            }
        }

        var newTime = timeSystem.CurrentRepublicTime;
        var activeYieldService = yieldService ?? new NationalYieldCycleService();
        var activeResourceService = resourceService ?? new ResourceProductionService();

        var yieldCyclesTotal = 0;
        var resourceCyclesTotal = 0;
        var fiscalQuartersTotal = 0;
        var completedProjects = new List<DevelopmentProject>();

        foreach (var country in worldManager.Countries.GetAllCountries())
        {
            // 1. Process due six-hour National Yield cycles
            var yieldSnapshots = activeYieldService.ExecuteDueCycles(country, newTime);
            yieldCyclesTotal += yieldSnapshots.Count;

            // 2. Process due domestic resource production cycles
            var resourceResults = activeResourceService.ProduceDueCycles(country, newTime);
            resourceCyclesTotal += resourceResults.Count(r => r.Success);

            // 3. Process completed development projects
            foreach (var proj in country.Projects.Where(p => p.IsActive).ToList())
            {
                if (proj.CanComplete(newTime) && country.CompleteProject(proj, newTime))
                {
                    completedProjects.Add(proj);
                }
            }

            // 4. Process due simulation-time fiscal cycles
            if (budgetService != null)
            {
                var fiscalReports = await budgetService.ProcessDueQuartersAsync(country, newTime, cancellationToken).ConfigureAwait(false);
                fiscalQuartersTotal += fiscalReports.Count;
            }
        }

        return new TimeAdvancementSummary(
            oldTime,
            newTime,
            duration,
            yieldCyclesTotal,
            resourceCyclesTotal,
            completedProjects.Count,
            completedProjects.AsReadOnly(),
            fiscalQuartersTotal);
    }
}
