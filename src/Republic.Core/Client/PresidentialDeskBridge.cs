namespace Republic.Core.Client;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Bridge service coordinating the live Republic simulation state connection to the Godot presidential desk client.
/// Provides serialization, file export, path resolution, and state reloading.
/// </summary>
public static class PresidentialDeskBridge
{
    /// <summary>
    /// Default filename used for the presidential desk simulation state JSON file.
    /// </summary>
    public const string DefaultFileName = "presidential_desk_state.json";

    /// <summary>
    /// Environment variable allowing custom override of the exported state file path.
    /// </summary>
    public const string StateFileEnvironmentVariable = "REPUBLIC_STATE_FILE";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a <see cref="PresidentialDeskSnapshot"/> to an indented JSON string.
    /// </summary>
    public static string Serialize(PresidentialDeskSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return JsonSerializer.Serialize(snapshot, SerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PresidentialDeskSnapshot"/>, returning null if invalid.
    /// </summary>
    public static PresidentialDeskSnapshot? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PresidentialDeskSnapshot>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves the default destination path for the Godot client state file.
    /// Searches upward from the working directory to locate the client/godot folder.
    /// </summary>
    public static string ResolveDefaultStateFilePath()
    {
        var envPath = Environment.GetEnvironmentVariable(StateFileEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return Path.GetFullPath(envPath);
        }

        // Search upward from current working directory
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null)
        {
            var candidate = Path.Combine(currentDir.FullName, "client", "godot");
            if (Directory.Exists(candidate))
            {
                return Path.Combine(candidate, DefaultFileName);
            }

            currentDir = currentDir.Parent;
        }

        // Fallback to AppContext base directory search
        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDir != null)
        {
            var candidate = Path.Combine(baseDir.FullName, "client", "godot");
            if (Directory.Exists(candidate))
            {
                return Path.Combine(candidate, DefaultFileName);
            }

            baseDir = baseDir.Parent;
        }

        // Default relative fallback
        return Path.GetFullPath(Path.Combine("client", "godot", DefaultFileName));
    }

    /// <summary>
    /// Exports the live simulation state for a sovereign country to a JSON state file.
    /// </summary>
    public static string ExportState(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var snapshot = PresidentialDeskSnapshot.Create(country, simulationTime, simulationTick);
        var path = targetPath ?? ResolveDefaultStateFilePath();

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = Serialize(snapshot);
        File.WriteAllText(path, json);

        return path;
    }

    /// <summary>
    /// Attempts to export the live simulation state to disk, catching any IO errors safely.
    /// </summary>
    public static bool TryExportState(
        Country country,
        RepublicTime simulationTime,
        out string? exportedPath,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        exportedPath = null;
        if (country == null)
        {
            return false;
        }

        try
        {
            exportedPath = ExportState(country, simulationTime, simulationTick, targetPath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to export the live simulation state to disk, ignoring the output path.
    /// </summary>
    public static bool TryExportState(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        return TryExportState(country, simulationTime, out _, simulationTick, targetPath);
    }

    /// <summary>
    /// Loads and deserializes a <see cref="PresidentialDeskSnapshot"/> from the specified or resolved file path.
    /// Returns null if the file does not exist or contains invalid data.
    /// </summary>
    public static PresidentialDeskSnapshot? LoadState(string? filePath = null)
    {
        var path = filePath ?? ResolveDefaultStateFilePath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            return Deserialize(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Applies an interactive presidential taxation decision using the authoritative Republic simulation rules,
    /// persists the resulting state if confirmed and valid, and exports a structured action result.
    /// </summary>
    public static PresidentialActionResult ApplyTaxDecision(
        double proposedIncomeTaxRate,
        bool confirmed = true,
        string? stateFilePath = null)
    {
        var targetPath = stateFilePath ?? ResolveDefaultStateFilePath();
        var snapshot = LoadState(targetPath);

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var defaultTime = new RepublicTime(launchEpoch, launchEpoch);

        Country country;
        RepublicTime simTime;
        ulong tick = 0;

        if (snapshot != null)
        {
            var countryId = !string.IsNullOrWhiteSpace(snapshot.CountryId) ? snapshot.CountryId : "player-country";
            var countryName = !string.IsNullOrWhiteSpace(snapshot.CountryName) && snapshot.CountryName != "[No State Loaded]"
                ? snapshot.CountryName
                : "Republic of Arcadia";

            var day = Math.Max(0, snapshot.SimulationDay);
            var timestamp = launchEpoch.AddDays(day);
            simTime = new RepublicTime(timestamp, launchEpoch);

            var treasury = new RepuTreasury(Math.Max(0.0, snapshot.TreasuryBalance), countryId);
            country = Country.Found(countryName, simTime, countryId, treasury: treasury);

            country.HappinessRating = Math.Clamp(snapshot.ApprovalRating, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(snapshot.StabilityRating, 0.0, 100.0);

            var existingIncome = snapshot.IncomeTaxRate > 0.0 ? snapshot.IncomeTaxRate : 0.25;
            var existingCorp = snapshot.CorporateTaxRate > 0.0 ? snapshot.CorporateTaxRate : 0.20;
            var existingTariff = snapshot.ImportTariffRate > 0.0 ? snapshot.ImportTariffRate : 0.10;

            country.TaxPolicy.IncomeTaxRate = existingIncome;
            country.TaxPolicy.CorporateTaxRate = existingCorp;
            country.TaxPolicy.ImportTariffRate = existingTariff;

            if (!string.IsNullOrWhiteSpace(snapshot.RulingParty) && snapshot.RulingParty != "None" && snapshot.RulingParty != "[No State Loaded]")
            {
                country.FoundParty(snapshot.RulingParty, simTime);
            }

            tick = snapshot.SimulationTick;
        }
        else
        {
            simTime = defaultTime;
            country = Country.Found("Republic of Arcadia", simTime, "player-country");
        }

        return ApplyTaxDecision(country, simTime, proposedIncomeTaxRate, confirmed, tick, targetPath);
    }

    /// <summary>
    /// Applies a presidential taxation policy decision directly to the provided country instance,
    /// evaluating constitutional bounds, input validity, and Phase 11 citizen approval rules.
    /// </summary>
    public static PresidentialActionResult ApplyTaxDecision(
        Country country,
        RepublicTime simulationTime,
        double proposedIncomeTaxRate,
        bool confirmed = true,
        ulong simulationTick = 0,
        string? stateFilePath = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var prevRate = country.TaxPolicy.IncomeTaxRate;
        var prevApproval = country.HappinessRating;
        var prevStability = country.BaselineStability;

        var proposedPolicy = new TaxPolicy
        {
            IncomeTaxRate = proposedIncomeTaxRate,
            CorporateTaxRate = country.TaxPolicy.CorporateTaxRate,
            ImportTariffRate = country.TaxPolicy.ImportTariffRate
        };

        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, proposedPolicy, simulationTime, confirmed);

        PresidentialActionResult actionResult;
        var targetPath = stateFilePath ?? ResolveDefaultStateFilePath();

        if (outcome.IsSuccess)
        {
            var nextTick = simulationTick + 1;
            ExportState(country, simulationTime, nextTick, targetPath);

            actionResult = new PresidentialActionResult
            {
                Success = true,
                Result = outcome.Result.ToString(),
                Message = outcome.Message,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = country.HappinessRating,
                PreviousApprovalRating = prevApproval,
                StabilityRating = country.BaselineStability,
                PreviousStabilityRating = prevStability
            };
        }
        else
        {
            actionResult = new PresidentialActionResult
            {
                Success = false,
                Result = outcome.Result.ToString(),
                Message = outcome.Message,
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
        }

        try
        {
            var dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir))
            {
                var resultPath = Path.Combine(dir, "presidential_desk_result.json");
                var resultJson = JsonSerializer.Serialize(actionResult, SerializerOptions);
                File.WriteAllText(resultPath, resultJson);
            }
        }
        catch { /* best-effort write */ }

        return actionResult;
    }

    /// <summary>
    /// Calculates the expected citizen approval consequence according to Phase 11 rules
    /// (-0.5% approval per +1% tax increase, +0.5% approval per -1% tax decrease, and stability warnings).
    /// </summary>
    public static string EstimateTaxImpact(double currentRate, double proposedRate)
    {
        var deltaRate = proposedRate - currentRate;
        var approvalDelta = -deltaRate * 50.0;
        var stabilityWarning = string.Empty;

        if (currentRate <= 0.50 && proposedRate > 0.50)
        {
            stabilityWarning = " Warning: Stability penalty -2.0% (Rate exceeds 50% limit).";
        }
        else if (currentRate > 0.50 && proposedRate <= 0.50)
        {
            stabilityWarning = " Benefit: +2.0% stability restored (Rate eased to 50% or below).";
        }

        if (Math.Abs(deltaRate) < 0.0001)
        {
            return "No change in income tax rate. Public approval and stability will remain unchanged.";
        }

        var sign = approvalDelta > 0 ? "+" : string.Empty;
        var dir = deltaRate > 0 ? "hike" : "cut";
        return $"Tax {dir} of {(deltaRate >= 0 ? "+" : "")}{deltaRate * 100:0.#}%: Expected approval effect is {sign}{approvalDelta:0.#}%.{stabilityWarning}";
    }
}

/// <summary>
/// Result contract representing the outcome of an interactive presidential decision submitted from Godot or CLI.
/// </summary>
public sealed record PresidentialActionResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("income_tax_rate")]
    public double IncomeTaxRate { get; init; }

    [JsonPropertyName("previous_income_tax_rate")]
    public double PreviousIncomeTaxRate { get; init; }

    [JsonPropertyName("approval_rating")]
    public double ApprovalRating { get; init; }

    [JsonPropertyName("previous_approval_rating")]
    public double PreviousApprovalRating { get; init; }

    [JsonPropertyName("stability_rating")]
    public double StabilityRating { get; init; }

    [JsonPropertyName("previous_stability_rating")]
    public double PreviousStabilityRating { get; init; }
}
