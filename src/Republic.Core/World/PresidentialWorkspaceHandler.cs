namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Linq;
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
    PrerequisitesNotMet
}

/// <summary>
/// Domain outcome record returned from an executive workspace action.
/// </summary>
public sealed record ExecutiveActionOutcome(
    ExecutiveActionResult Result,
    string Message,
    DevelopmentProject? Project = null)
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
    IReadOnlyList<DevelopmentProject> CompletedProjects);

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
    /// Advances authoritative Republic simulation time by the specified duration, processing all due yield cycles,
    /// domestic resource production cycles, and capital development project completions across all registered countries.
    /// </summary>
    public static TimeAdvancementSummary AdvanceTime(
        IWorldManager worldManager,
        ITimeSystem timeSystem,
        TimeSpan duration,
        INationalYieldCycleService? yieldService = null,
        IResourceProductionService? resourceService = null)
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
        }

        return new TimeAdvancementSummary(
            oldTime,
            newTime,
            duration,
            yieldCyclesTotal,
            resourceCyclesTotal,
            completedProjects.Count,
            completedProjects.AsReadOnly());
    }
}
