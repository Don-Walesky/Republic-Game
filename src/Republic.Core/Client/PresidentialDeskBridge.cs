namespace Republic.Core.Client;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Persistence.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Bridge service coordinating the live Republic simulation state connection to the Godot presidential desk client.
/// Provides serialization, file export, path resolution, authoritative state saving, and persistent decision execution.
/// </summary>
public static class PresidentialDeskBridge
{
    /// <summary>
    /// Default filename used for the presidential desk simulation state JSON file.
    /// </summary>
    public const string DefaultFileName = "presidential_desk_state.json";

    /// <summary>
    /// Default filename used for the authoritative persistent simulation state save file.
    /// </summary>
    public const string DefaultSaveFileName = "presidential_desk_state.sav";

    /// <summary>
    /// Environment variable allowing custom override of the exported state file path.
    /// </summary>
    public const string StateFileEnvironmentVariable = "REPUBLIC_STATE_FILE";

    /// <summary>
    /// Environment variable allowing custom override of the persistent save file path.
    /// </summary>
    public const string SaveFileEnvironmentVariable = "REPUBLIC_SAVE_FILE";

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
    /// Resolves the default destination path for the authoritative persistent simulation save file.
    /// </summary>
    public static string ResolveDefaultSaveFilePath()
    {
        var envPath = Environment.GetEnvironmentVariable(SaveFileEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return Path.GetFullPath(envPath);
        }

        var defaultStatePath = ResolveDefaultStateFilePath();
        var companion = Path.ChangeExtension(defaultStatePath, ".sav");
        if (File.Exists(companion))
        {
            return companion;
        }

        // Search upward from current working directory for saves/session.sav or saves/quicksave.sav
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null)
        {
            var session = Path.Combine(currentDir.FullName, "saves", "session.sav");
            if (File.Exists(session))
            {
                return session;
            }
            var quick = Path.Combine(currentDir.FullName, "saves", "quicksave.sav");
            if (File.Exists(quick))
            {
                return quick;
            }
            currentDir = currentDir.Parent;
        }

        return companion;
    }

    /// <summary>
    /// Saves the authoritative state of a sovereign country to disk using the repository's established SaveEnvelope persistence format.
    /// Preserves full country identity, founding moment, treasury, tax policy, approval, stability, inventory, and projects.
    /// </summary>
    public static string SaveAuthoritativeState(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string saveName = "session")
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var path = targetPath ?? ResolveDefaultSaveFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var world = new WorldState
        {
            WorldId = Guid.NewGuid(),
            Name = "Republic Sandbox",
            CreatedAt = simulationTime.UtcTimestamp,
            CurrentTick = simulationTick,
            Countries = new List<Country> { country }
        };

        var fullState = new FullGameState
        {
            SaveName = saveName,
            CurrentTick = simulationTick,
            SaveTimestamp = simulationTime.UtcTimestamp,
            World = world
        };

        var envelope = new SaveEnvelope<FullGameState>
        {
            FormatVersion = 1,
            SavedAt = simulationTime.UtcTimestamp,
            State = fullState
        };

        var serializer = new JsonStateSerializer();
        var payload = serializer.Serialize(envelope);
        File.WriteAllText(path, payload);

        return path;
    }

    /// <summary>
    /// Loads an authoritative SaveEnvelope from disk, returning null if missing or invalid.
    /// </summary>
    public static SaveEnvelope<FullGameState>? LoadAuthoritativeState(string? filePath = null)
    {
        var path = filePath ?? ResolveDefaultSaveFilePath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var payload = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            var serializer = new JsonStateSerializer();
            return serializer.Deserialize<FullGameState>(payload);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Exports the live simulation state for a sovereign country to a JSON state file,
    /// also persisting the authoritative simulation state to a companion save file if needed.
    /// </summary>
    public static string ExportState(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string? saveFilePath = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var path = targetPath ?? ResolveDefaultStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var companionSavePath = saveFilePath ?? Path.ChangeExtension(path, ".sav");
        try
        {
            SaveAuthoritativeState(country, simulationTime, simulationTick, companionSavePath);
        }
        catch { /* best-effort companion save */ }

        var snapshot = PresidentialDeskSnapshot.Create(country, simulationTime, simulationTick, companionSavePath);
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
        string? targetPath = null,
        string? saveFilePath = null)
    {
        exportedPath = null;
        if (country == null)
        {
            return false;
        }

        try
        {
            exportedPath = ExportState(country, simulationTime, simulationTick, targetPath, saveFilePath);
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
        string? targetPath = null,
        string? saveFilePath = null)
    {
        return TryExportState(country, simulationTime, out _, simulationTick, targetPath, saveFilePath);
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
    /// operating on the persistent authoritative simulation state, and exporting a structured action result.
    /// Missing or corrupted state files are rejected cleanly without creating default sample countries.
    /// </summary>
    public static PresidentialActionResult ApplyTaxDecision(
        double proposedIncomeTaxRate,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();



        // 2. Resolve the authoritative save file path
        string targetSavePath;
        if (!string.IsNullOrWhiteSpace(saveFilePath))
        {
            targetSavePath = saveFilePath;
        }
        else if (targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase))
        {
            targetSavePath = targetStatePath;
        }
        else
        {
            // targetStatePath is a JSON file (could be a SaveEnvelope or a PresidentialDeskSnapshot)
            if (!File.Exists(targetStatePath))
            {
                // Check if companion save exists
                var companion = Path.ChangeExtension(targetStatePath, ".sav");
                if (File.Exists(companion))
                {
                    targetSavePath = companion;
                }
                else
                {
                    var missingResult = new PresidentialActionResult
                    {
                        Success = false,
                        Result = "StateMissingOrCorrupt",
                        Message = $"Cannot apply presidential taxation decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                    };
                    TryWriteResultFile(targetStatePath, missingResult);
                    return missingResult;
                }
            }
            else
            {
                // File exists. Check if it's corrupt JSON or what kind of JSON it is.
                string rawJson;
                try
                {
                    rawJson = File.ReadAllText(targetStatePath);
                }
                catch (Exception ex)
                {
                    var ioResult = new PresidentialActionResult
                    {
                        Success = false,
                        Result = "StateMissingOrCorrupt",
                        Message = $"Cannot apply presidential taxation decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                    };
                    TryWriteResultFile(targetStatePath, ioResult);
                    return ioResult;
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    var emptyResult = new PresidentialActionResult
                    {
                        Success = false,
                        Result = "StateMissingOrCorrupt",
                        Message = $"Cannot apply presidential taxation decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                    };
                    TryWriteResultFile(targetStatePath, emptyResult);
                    return emptyResult;
                }

                // Test if rawJson is valid JSON
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(rawJson);
                }
                catch (JsonException)
                {
                    var corruptResult = new PresidentialActionResult
                    {
                        Success = false,
                        Result = "StateMissingOrCorrupt",
                        Message = $"Cannot apply presidential taxation decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                    };
                    TryWriteResultFile(targetStatePath, corruptResult);
                    return corruptResult;
                }

                using (doc)
                {
                    // Check if the JSON is directly a SaveEnvelope (has "formatVersion" and "state" with "world")
                    if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                    {
                        targetSavePath = targetStatePath;
                    }
                    else
                    {
                        // It is a dashboard snapshot (PresidentialDeskSnapshot)
                        // Check if it has save_file_path property
                        string? snapshotSavePath = null;
                        if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                        {
                            snapshotSavePath = savePathEl.GetString();
                        }

                        if (!string.IsNullOrWhiteSpace(snapshotSavePath) && File.Exists(snapshotSavePath))
                        {
                            targetSavePath = snapshotSavePath;
                        }
                        else
                        {
                            var companion = Path.ChangeExtension(targetStatePath, ".sav");
                            if (File.Exists(companion))
                            {
                                targetSavePath = companion;
                            }
                            else
                            {
                                // Dashboard snapshot exists, but the authoritative saved game does NOT exist!
                                var noSaveResult = new PresidentialActionResult
                                {
                                    Success = false,
                                    Result = "StateMissingOrCorrupt",
                                    Message = $"Cannot apply presidential taxation decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. Dashboard snapshots cannot substitute for complete saved games. No changes enacted."
                                };
                                TryWriteResultFile(targetStatePath, noSaveResult);
                                return noSaveResult;
                            }
                        }
                    }
                }
            }
        }

        // 3. Load the authoritative SaveEnvelope<FullGameState>
        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply presidential taxation decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, missingSaveResult);
            return missingSaveResult;
        }

        SaveEnvelope<FullGameState>? envelope;
        try
        {
            var savePayload = File.ReadAllText(targetSavePath);
            var serializer = new JsonStateSerializer();
            envelope = serializer.Deserialize<FullGameState>(savePayload);
        }
        catch (Exception ex)
        {
            var corruptSaveResult = new PresidentialActionResult
            {
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply presidential taxation decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply presidential taxation decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        var country = envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
                      ?? envelope.State.World.Countries.FirstOrDefault()!;

        // 4. Derive Republic simulation time
        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        var prevRate = country.TaxPolicy.IncomeTaxRate;
        var prevApproval = country.HappinessRating;
        var prevStability = country.BaselineStability;

        // 4. Validate inputs (non-finite rates and constitutional bounds)
        if (!double.IsFinite(proposedIncomeTaxRate))
        {
            var invalidResult = new PresidentialActionResult
            {
                Success = false,
                Result = "InvalidInput",
                Message = "Taxation policy rejected: rates must be finite, valid numbers.",
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
            TryWriteResultFile(targetStatePath, invalidResult);
            return invalidResult;
        }

        if (proposedIncomeTaxRate < 0.0 || proposedIncomeTaxRate > 0.70)
        {
            var boundsResult = new PresidentialActionResult
            {
                Success = false,
                Result = "InvalidInput",
                Message = "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%).",
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
            TryWriteResultFile(targetStatePath, boundsResult);
            return boundsResult;
        }

        // 5. Handle cancellation
        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                Success = false,
                Result = "Cancelled",
                Message = "Taxation policy adjustment cancelled by executive order. Existing tax rates and citizen approval preserved unchanged.",
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        // 6. Enact decision via PresidentialWorkspaceHandler
        var proposedPolicy = new TaxPolicy
        {
            IncomeTaxRate = proposedIncomeTaxRate,
            CorporateTaxRate = country.TaxPolicy.CorporateTaxRate,
            ImportTariffRate = country.TaxPolicy.ImportTariffRate
        };

        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, proposedPolicy, simTime, confirmed: true);
        if (!outcome.IsSuccess)
        {
            var failResult = new PresidentialActionResult
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
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        // 7. Persist authoritative save file
        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            File.WriteAllText(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            // Revert in-memory mutations
            country.TaxPolicy.IncomeTaxRate = prevRate;
            country.HappinessRating = prevApproval;
            country.BaselineStability = prevStability;

            var persistFailResult = new PresidentialActionResult
            {
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Changes not enacted.",
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        // 8. Export updated dashboard snapshot for Godot
        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath);
            var snapshotJson = Serialize(snapshot);
            File.WriteAllText(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            Success = true,
            Result = "Success",
            Message = outcome.Message,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            PreviousIncomeTaxRate = prevRate,
            ApprovalRating = country.HappinessRating,
            PreviousApprovalRating = prevApproval,
            StabilityRating = country.BaselineStability,
            PreviousStabilityRating = prevStability
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Applies a presidential taxation policy decision directly to the provided country instance,
    /// evaluating constitutional bounds, input validity, and Phase 11 citizen approval rules.
    /// Can also persist through an ISaveGameManager or direct save file path.
    /// </summary>
    public static PresidentialActionResult ApplyTaxDecision(
        Country country,
        RepublicTime simulationTime,
        double proposedIncomeTaxRate,
        bool confirmed = true,
        ulong simulationTick = 0,
        string? stateFilePath = null,
        string? saveFilePath = null,
        ISaveGameManager? saveGameManager = null,
        string? saveSlot = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var targetPath = stateFilePath ?? ResolveDefaultStateFilePath();

        // 1. Validate inputs (non-finite rates and constitutional bounds)
        if (!double.IsFinite(proposedIncomeTaxRate))
        {
            var invalidResult = new PresidentialActionResult
            {
                Success = false,
                Result = "InvalidInput",
                Message = "Taxation policy rejected: rates must be finite, valid numbers."
            };
            TryWriteResultFile(targetPath, invalidResult);
            return invalidResult;
        }

        if (proposedIncomeTaxRate < 0.0 || proposedIncomeTaxRate > 0.70)
        {
            var boundsResult = new PresidentialActionResult
            {
                Success = false,
                Result = "InvalidInput",
                Message = "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%)."
            };
            TryWriteResultFile(targetPath, boundsResult);
            return boundsResult;
        }

        var prevRate = country.TaxPolicy.IncomeTaxRate;
        var prevApproval = country.HappinessRating;
        var prevStability = country.BaselineStability;

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                Success = false,
                Result = "Cancelled",
                Message = "Taxation policy adjustment cancelled by executive order. Existing tax rates and citizen approval preserved unchanged.",
                IncomeTaxRate = prevRate,
                PreviousIncomeTaxRate = prevRate,
                ApprovalRating = prevApproval,
                PreviousApprovalRating = prevApproval,
                StabilityRating = prevStability,
                PreviousStabilityRating = prevStability
            };
            TryWriteResultFile(targetPath, cancelResult);
            return cancelResult;
        }

        var proposedPolicy = new TaxPolicy
        {
            IncomeTaxRate = proposedIncomeTaxRate,
            CorporateTaxRate = country.TaxPolicy.CorporateTaxRate,
            ImportTariffRate = country.TaxPolicy.ImportTariffRate
        };

        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, proposedPolicy, simulationTime, confirmed: true);
        if (!outcome.IsSuccess)
        {
            var failResult = new PresidentialActionResult
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
            TryWriteResultFile(targetPath, failResult);
            return failResult;
        }

        // Handle persistence
        if (saveGameManager != null)
        {
            try
            {
                saveGameManager.SaveGameAsync(saveSlot ?? "session").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                country.TaxPolicy.IncomeTaxRate = prevRate;
                country.HappinessRating = prevApproval;
                country.BaselineStability = prevStability;

                var persistFail = new PresidentialActionResult
                {
                    Success = false,
                    Result = "PersistenceFailure",
                    Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Changes not enacted.",
                    IncomeTaxRate = prevRate,
                    PreviousIncomeTaxRate = prevRate,
                    ApprovalRating = prevApproval,
                    PreviousApprovalRating = prevApproval,
                    StabilityRating = prevStability,
                    PreviousStabilityRating = prevStability
                };
                TryWriteResultFile(targetPath, persistFail);
                return persistFail;
            }
        }
        else if (!string.IsNullOrWhiteSpace(saveFilePath))
        {
            try
            {
                SaveAuthoritativeState(country, simulationTime, simulationTick + 1, saveFilePath);
            }
            catch (Exception ex)
            {
                country.TaxPolicy.IncomeTaxRate = prevRate;
                country.HappinessRating = prevApproval;
                country.BaselineStability = prevStability;

                var persistFail = new PresidentialActionResult
                {
                    Success = false,
                    Result = "PersistenceFailure",
                    Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Changes not enacted.",
                    IncomeTaxRate = prevRate,
                    PreviousIncomeTaxRate = prevRate,
                    ApprovalRating = prevApproval,
                    PreviousApprovalRating = prevApproval,
                    StabilityRating = prevStability,
                    PreviousStabilityRating = prevStability
                };
                TryWriteResultFile(targetPath, persistFail);
                return persistFail;
            }
        }

        var nextTick = simulationTick + 1;
        ExportState(country, simulationTime, nextTick, targetPath, saveFilePath);

        var successResult = new PresidentialActionResult
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

        TryWriteResultFile(targetPath, successResult);
        return successResult;
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

    private static void TryWriteResultFile(string stateOrSavePath, PresidentialActionResult result)
    {
        try
        {
            var dir = Path.GetDirectoryName(stateOrSavePath);
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.GetDirectoryName(ResolveDefaultStateFilePath());
            }

            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                var resultPath = Path.Combine(dir, "presidential_desk_result.json");
                var resultJson = JsonSerializer.Serialize(result, SerializerOptions);
                File.WriteAllText(resultPath, resultJson);
            }
        }
        catch { /* best-effort write */ }
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
