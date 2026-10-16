namespace Republic.Core.AI;

using Republic.Core.Client;
using Republic.Core.NationalYield;
using Republic.Core.Persistence.Services;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Autonomous AI government controlling sovereign domestic taxation and governance decisions
/// during National Yield cycles and offline progression.
/// </summary>
public sealed class AIGovernment
{
    /// <summary>
    /// Gets the unique country identifier governed by this AI.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a note describing the most recent policy decision made by this AI government.
    /// </summary>
    public string LastDecisionNote { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the simulation time at which the last decision was enacted.
    /// </summary>
    public RepublicTime? LastDecisionTime { get; set; }

    /// <summary>
    /// Gets or sets the income tax rate chosen in the last decision.
    /// </summary>
    public double LastTaxRate { get; set; } = 0.25;

    /// <summary>
    /// Evaluates domestic conditions and applies an authoritative taxation decision via
    /// <see cref="PresidentialDeskBridge.ApplyTaxDecision(Country, RepublicTime, double, bool, ulong, string?, string?, ISaveGameManager?, string?, string?)"/>.
    /// Records a short decision note and updates the country's <see cref="Country.LastAIDecision"/>.
    /// </summary>
    public PresidentialActionResult ProcessYieldCycle(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        ISaveGameManager? saveGameManager = null,
        string? stateFilePath = null,
        string? saveFilePath = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        country.IsAIControlled = true;

        var currentRate = country.TaxPolicy.IncomeTaxRate;
        var approval = country.HappinessRating;
        var treasury = country.Treasury.Balance;

        // Choose a simple tax-rate adjustment within existing rules (0.0 to 0.70)
        double proposedRate;
        string reason;

        if (approval < 55.0 && currentRate > 0.15)
        {
            proposedRate = Math.Max(0.15, Math.Round((currentRate - 0.01) * 1000.0) / 1000.0);
            reason = "Lowered income tax to stimulate public approval";
        }
        else if (treasury < 10_000_000.0 && currentRate < 0.35)
        {
            proposedRate = Math.Min(0.35, Math.Round((currentRate + 0.01) * 1000.0) / 1000.0);
            reason = "Raised income tax to reinforce sovereign treasury reserves";
        }
        else if (approval > 75.0 && currentRate < 0.30)
        {
            proposedRate = Math.Min(0.30, Math.Round((currentRate + 0.01) * 1000.0) / 1000.0);
            reason = "Raised income tax leveraging high public approval";
        }
        else if (currentRate > 0.30)
        {
            proposedRate = Math.Max(0.20, Math.Round((currentRate - 0.01) * 1000.0) / 1000.0);
            reason = "Lowered income tax to ease citizen economic burden";
        }
        else
        {
            // Maintain or gentle alternation around baseline 25%
            proposedRate = currentRate <= 0.22
                ? Math.Min(0.26, Math.Round((currentRate + 0.01) * 1000.0) / 1000.0)
                : Math.Max(0.20, Math.Round((currentRate - 0.01) * 1000.0) / 1000.0);
            reason = "Adjusted income tax for balanced fiscal stability";
        }

        var targetStatePath = stateFilePath;
        if (targetStatePath == null && country.IsAIControlled)
        {
            targetStatePath = Path.Combine(Path.GetTempPath(), $"republic_ai_{country.Id}_state.json");
        }

        // Apply via existing authoritative decision path used by the player
        var result = PresidentialDeskBridge.ApplyTaxDecision(
            country,
            simulationTime,
            proposedRate,
            confirmed: true,
            simulationTick: simulationTick,
            stateFilePath: targetStatePath,
            saveFilePath: saveFilePath,
            saveGameManager: saveGameManager);

        var timeStr = $"Day {simulationTime.DayNumber}, {simulationTime.Time:HH:mm} WAT";
        LastDecisionNote = $"{country.Name} set tax to {proposedRate * 100:0.#}% ({timeStr}) - {reason}.";
        LastDecisionTime = simulationTime;
        LastTaxRate = proposedRate;
        country.LastAIDecision = LastDecisionNote;

        return result;
    }

    /// <summary>
    /// Resumes and catches up all due National Yield cycles and AI tax decisions up to the specified simulation time.
    /// </summary>
    public IReadOnlyList<PresidentialActionResult> ProcessDueCycles(
        Country country,
        RepublicTime simulationTime,
        INationalYieldCycleService? cycleService = null,
        ulong simulationTick = 0,
        ISaveGameManager? saveGameManager = null,
        string? stateFilePath = null,
        string? saveFilePath = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        country.IsAIControlled = true;

        var activeCycleService = cycleService ?? new NationalYieldCycleService();
        var dueBoundaries = activeCycleService.GetDueBoundaries(country, simulationTime);
        if (dueBoundaries.Count == 0)
        {
            return Array.Empty<PresidentialActionResult>();
        }

        var results = new List<PresidentialActionResult>(dueBoundaries.Count);
        foreach (var boundary in dueBoundaries)
        {
            // 1. Run the National Yield cycle at boundary
            activeCycleService.ExecuteCycle(country, boundary);
            country.LastCreditedBoundary = boundary;

            // 2. Run the AI taxation decision at boundary
            var actionResult = ProcessYieldCycle(
                country,
                boundary,
                simulationTick,
                saveGameManager: saveGameManager,
                stateFilePath: stateFilePath,
                saveFilePath: saveFilePath);
            results.Add(actionResult);
        }

        return results;
    }
}
