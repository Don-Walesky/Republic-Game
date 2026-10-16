namespace Republic.Core.Client;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Serializable data contract representing the live Republic simulation state displayed in the Godot presidential desk.
/// Encapsulates national identity, treasury balance, public approval, stability, simulation clock, and political status.
/// </summary>
public sealed record PresidentialDeskSnapshot
{
    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    [JsonPropertyName("country_name")]
    public string CountryName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the numerical sovereign treasury balance in REPU.
    /// </summary>
    [JsonPropertyName("treasury_balance")]
    public double TreasuryBalance { get; init; }

    /// <summary>
    /// Gets the formatted sovereign treasury string in standard REPU notation.
    /// </summary>
    [JsonPropertyName("treasury")]
    public string Treasury { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign public approval rating percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("approval_rating")]
    public double ApprovalRating { get; init; }

    /// <summary>
    /// Gets the formatted public approval percentage string (e.g. "78%").
    /// </summary>
    [JsonPropertyName("approval")]
    public string Approval { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign baseline stability rating percentage (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("stability_rating")]
    public double StabilityRating { get; init; }

    /// <summary>
    /// Gets the formatted stability percentage string (e.g. "85%").
    /// </summary>
    [JsonPropertyName("stability")]
    public string Stability { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authoritative Republic simulation day number.
    /// </summary>
    [JsonPropertyName("simulation_day")]
    public long SimulationDay { get; init; }

    /// <summary>
    /// Gets the concise simulation time string (e.g. "Day 1, 00:00 WAT").
    /// </summary>
    [JsonPropertyName("simulation_time")]
    public string SimulationTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed simulation timestamp string (e.g. "Day 1, 1 January 2030 00:00 WAT").
    /// </summary>
    [JsonPropertyName("simulation_time_detailed")]
    public string SimulationTimeDetailed { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the sovereign ruling political party, or "None" if unestablished.
    /// </summary>
    [JsonPropertyName("ruling_party")]
    public string RulingParty { get; init; } = "None";

    /// <summary>
    /// Gets the status or description of the most recent national democratic election.
    /// </summary>
    [JsonPropertyName("election")]
    public string Election { get; init; } = "No election";

    /// <summary>
    /// Gets the live connection status message.
    /// </summary>
    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; } = "Connected to live Republic simulation state";

    /// <summary>
    /// Gets the UTC timestamp at which this state snapshot was exported.
    /// </summary>
    [JsonPropertyName("exported_at_utc")]
    public string ExportedAtUtc { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique sovereign country identifier.
    /// </summary>
    [JsonPropertyName("country_id")]
    public string CountryId { get; init; } = "player-country";

    /// <summary>
    /// Gets the personal income tax rate as a decimal (e.g. 0.25 for 25%).
    /// </summary>
    [JsonPropertyName("income_tax_rate")]
    public double IncomeTaxRate { get; init; } = 0.25;

    /// <summary>
    /// Gets the formatted personal income tax rate percentage string (e.g. "25%").
    /// </summary>
    [JsonPropertyName("income_tax_rate_formatted")]
    public string IncomeTaxRateFormatted { get; init; } = "25%";

    /// <summary>
    /// Gets the corporate tax rate as a decimal (e.g. 0.20 for 20%).
    /// </summary>
    [JsonPropertyName("corporate_tax_rate")]
    public double CorporateTaxRate { get; init; } = 0.20;

    /// <summary>
    /// Gets the import tariff rate as a decimal (e.g. 0.10 for 10%).
    /// </summary>
    [JsonPropertyName("import_tariff_rate")]
    public double ImportTariffRate { get; init; } = 0.10;

    /// <summary>
    /// Gets the explanatory outcome message or citizen feedback from the latest presidential decision.
    /// </summary>
    [JsonPropertyName("latest_decision_result")]
    public string LatestDecisionResult { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name and status of the current active national development project, or "None" if no project is active.
    /// </summary>
    [JsonPropertyName("active_project")]
    public string ActiveProject { get; init; } = "None";

    /// <summary>
    /// Gets the simulation engine tick at which this state snapshot was exported.
    /// </summary>
    [JsonPropertyName("simulation_tick")]
    public ulong SimulationTick { get; init; }

    /// <summary>
    /// Gets the path to the authoritative persistent save file from which this snapshot was projected.
    /// </summary>
    [JsonPropertyName("save_file_path")]
    public string SaveFilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive summary of the latest decision made by an autonomous AI government.
    /// </summary>
    [JsonPropertyName("last_ai_decision")]
    public string LastAiDecision { get; init; } = "[No AI decisions recorded]";

    /// <summary>
    /// Factory helper to build a <see cref="PresidentialDeskSnapshot"/> from live simulation state.
    /// Reuses existing core models and services without duplicating domain rules.
    /// </summary>
    public static PresidentialDeskSnapshot Create(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? saveFilePath = null,
        string? latestDecisionResult = null,
        string? statusMessage = null,
        string? lastAiDecision = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var dashboard = country.GetDashboard(simulationTime);

        var rulingPartyName = country.RulingParty?.Name;
        if (string.IsNullOrWhiteSpace(rulingPartyName))
        {
            rulingPartyName = "None";
        }

        string electionText;
        if (country.LastElection != null)
        {
            var election = country.LastElection;
            electionText = $"Day {election.Boundary.DayNumber} ({election.WinnerPartyName} victory, {election.WinnerSupport:0.#}% vs {election.LoserSupport:0.#}%)";
        }
        else
        {
            electionText = "No election";
        }

        var conciseTime = $"Day {simulationTime.DayNumber}, {simulationTime.Time:HH:mm} WAT";
        var detailedTime = $"Day {simulationTime.DayNumber}, {simulationTime.Date:d MMMM yyyy} {simulationTime.Time:HH:mm} WAT";

        var formattedTreasury = $"R {country.Treasury.Balance:N0}";

        var taxPolicy = country.TaxPolicy ?? new Republic.Core.Economy.Budget.Models.TaxPolicy();
        var formattedTaxRate = $"{taxPolicy.IncomeTaxRate * 100:0.#}%";
        var decisionResult = latestDecisionResult ?? country.LatestCitizenFeedback?.Description ?? string.Empty;
        var resolvedAiDecision = !string.IsNullOrWhiteSpace(lastAiDecision)
            ? lastAiDecision
            : (country.LastAIDecision ?? "[No AI decisions recorded]");

        var activeProj = country.Projects?.FirstOrDefault(p => p.IsActive);
        string activeProjectText = activeProj != null
            ? $"{activeProj.Name} (Finishes: {activeProj.FinishBoundary})"
            : "None";

        return new PresidentialDeskSnapshot
        {
            CountryName = country.Name,
            CountryId = country.Id,
            TreasuryBalance = country.Treasury.Balance,
            Treasury = formattedTreasury,
            ApprovalRating = dashboard.ApprovalRating,
            Approval = dashboard.Approval,
            StabilityRating = dashboard.BaselineStability,
            Stability = dashboard.Stability,
            IncomeTaxRate = taxPolicy.IncomeTaxRate,
            IncomeTaxRateFormatted = formattedTaxRate,
            CorporateTaxRate = taxPolicy.CorporateTaxRate,
            ImportTariffRate = taxPolicy.ImportTariffRate,
            LatestDecisionResult = decisionResult,
            ActiveProject = activeProjectText,
            SimulationDay = simulationTime.DayNumber,
            SimulationTime = conciseTime,
            SimulationTimeDetailed = detailedTime,
            RulingParty = rulingPartyName,
            Election = electionText,
            StatusMessage = !string.IsNullOrWhiteSpace(statusMessage) ? statusMessage : "Connected to live Republic simulation state",
            ExportedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            SimulationTick = simulationTick,
            SaveFilePath = saveFilePath ?? string.Empty,
            LastAiDecision = resolvedAiDecision
        };
    }

    /// <summary>
    /// Factory helper to build a <see cref="PresidentialDeskSnapshot"/> from a world state and player country,
    /// resolving autonomous AI government decisions across registered nations.
    /// </summary>
    public static PresidentialDeskSnapshot Create(
        Republic.Core.World.WorldState world,
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? saveFilePath = null,
        string? latestDecisionResult = null,
        string? statusMessage = null,
        string? lastAiDecision = null)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(country);

        string? resolvedAiDecision = lastAiDecision;
        if (string.IsNullOrWhiteSpace(resolvedAiDecision))
        {
            var otherCountry = world.Countries?.FirstOrDefault(c => c.Id != country.Id && !string.IsNullOrWhiteSpace(c.LastAIDecision));
            resolvedAiDecision = otherCountry?.LastAIDecision;
        }

        return Create(country, simulationTime, simulationTick, saveFilePath, latestDecisionResult, statusMessage, resolvedAiDecision);
    }
}

