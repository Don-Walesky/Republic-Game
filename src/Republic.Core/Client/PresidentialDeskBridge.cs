namespace Republic.Core.Client;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.PrivateSector.Services;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Government.Officials.Services;
using Republic.Core.Media.Services;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Persistence.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;

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
    /// Default filename used for presidential desk decision request JSON files from the Godot client.
    /// </summary>
    public const string DefaultRequestFileName = "presidential_desk_request.json";

    /// <summary>
    /// Default filename used for structured presidential action result JSON files.
    /// </summary>
    public const string DefaultResultFileName = "presidential_desk_result.json";

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

    private static readonly object RequestProcessingLock = new();

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
    /// Resolves the default destination path for the presidential desk decision request JSON file.
    /// Places it adjacent to the resolved state file.
    /// </summary>
    public static string ResolveDefaultRequestFilePath(string? stateFilePath = null)
    {
        var targetState = stateFilePath ?? ResolveDefaultStateFilePath();
        var fullTargetState = Path.GetFullPath(targetState);
        var dir = Path.GetDirectoryName(fullTargetState);
        if (!string.IsNullOrEmpty(dir))
        {
            return Path.Combine(dir, DefaultRequestFileName);
        }

        return Path.GetFullPath(Path.Combine("client", "godot", DefaultRequestFileName));
    }

    /// <summary>
    /// Resolves the default destination path for the structured presidential action result JSON file.
    /// Places it adjacent to the resolved state file.
    /// </summary>
    public static string ResolveDefaultResultFilePath(string? stateFilePath = null)
    {
        var targetState = stateFilePath ?? ResolveDefaultStateFilePath();
        var fullTargetState = Path.GetFullPath(targetState);
        var dir = Path.GetDirectoryName(fullTargetState);
        if (!string.IsNullOrEmpty(dir))
        {
            return Path.Combine(dir, DefaultResultFileName);
        }

        return Path.GetFullPath(Path.Combine("client", "godot", DefaultResultFileName));
    }

    /// <summary>
    /// Saves the authoritative state of a sovereign country to disk using the repository's established SaveEnvelope persistence format.
    /// Preserves full country identity, founding moment, treasury, tax policy, approval, stability, inventory, and projects.
    /// If the target save file already exists, preserves all unrelated countries, world identity, entities, workspace, narrative, and tasks.
    /// Rejects corrupted or invalid existing save files with <see cref="InvalidDataException"/> without overwriting.
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

        SaveEnvelope<FullGameState>? existingEnvelope = null;
        if (File.Exists(path))
        {
            var rawText = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(rawText))
            {
                throw new InvalidDataException(
                    $"Cannot update authoritative save at '{path}': Existing save file is empty. Overwriting with incomplete state is prohibited.");
            }

            var serializer = new JsonStateSerializer();
            try
            {
                existingEnvelope = serializer.Deserialize<FullGameState>(rawText);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"Cannot update authoritative save at '{path}': Existing save file is corrupted or contains invalid data ({ex.Message}). Overwriting with incomplete state is prohibited.", ex);
            }

            if (existingEnvelope?.State?.World == null)
            {
                throw new InvalidDataException(
                    $"Cannot update authoritative save at '{path}': Existing save file does not contain a valid world state. Overwriting with incomplete state is prohibited.");
            }
        }

        SaveEnvelope<FullGameState> envelope;
        if (existingEnvelope?.State != null)
        {
            envelope = existingEnvelope;
            var fullState = envelope.State;
            var world = fullState.World;

            world.CurrentTick = simulationTick;
            world.Countries ??= new List<Country>();

            var existingIndex = world.Countries.FindIndex(c =>
                (!string.IsNullOrWhiteSpace(country.Id) && string.Equals(c.Id, country.Id, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(country.Name) && string.Equals(c.Name, country.Name, StringComparison.OrdinalIgnoreCase)));

            if (existingIndex >= 0)
            {
                world.Countries[existingIndex] = country;
            }
            else
            {
                world.Countries.Add(country);
            }

            fullState.CurrentTick = simulationTick;
            fullState.SaveTimestamp = simulationTime.UtcTimestamp;
            if (!string.IsNullOrWhiteSpace(saveName) && saveName != "session")
            {
                fullState.SaveName = saveName;
            }
            envelope.SavedAt = simulationTime.UtcTimestamp;
        }
        else
        {
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

            envelope = new SaveEnvelope<FullGameState>
            {
                FormatVersion = 1,
                SavedAt = simulationTime.UtcTimestamp,
                State = fullState
            };
        }

        var saveSerializer = new JsonStateSerializer();
        var payload = saveSerializer.Serialize(envelope);
        AtomicFileWriter.WriteAllTextSafely(path, payload);

        return path;
    }

    /// <summary>
    /// Saves the authoritative state of a complete game world to disk using the repository's established SaveEnvelope persistence format.
    /// Preserves all sovereign countries, world identity, entities, and systemic states.
    /// </summary>
    public static string SaveAuthoritativeState(
        WorldState world,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string saveName = "session")
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var path = targetPath ?? ResolveDefaultSaveFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        SaveEnvelope<FullGameState>? existingEnvelope = null;
        if (File.Exists(path))
        {
            var rawText = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(rawText))
            {
                throw new InvalidDataException(
                    $"Cannot update authoritative save at '{path}': Existing save file is empty. Overwriting with incomplete state is prohibited.");
            }

            var serializer = new JsonStateSerializer();
            try
            {
                existingEnvelope = serializer.Deserialize<FullGameState>(rawText);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"Cannot update authoritative save at '{path}': Existing save file is corrupted or contains invalid data ({ex.Message}). Overwriting with incomplete state is prohibited.", ex);
            }
        }

        SaveEnvelope<FullGameState> envelope;
        if (existingEnvelope?.State != null)
        {
            envelope = existingEnvelope;
            envelope.State.World = world;
            envelope.State.CurrentTick = simulationTick;
            envelope.State.SaveTimestamp = simulationTime.UtcTimestamp;
            if (!string.IsNullOrWhiteSpace(saveName) && saveName != "session")
            {
                envelope.State.SaveName = saveName;
            }
            envelope.SavedAt = simulationTime.UtcTimestamp;
        }
        else
        {
            var fullState = new FullGameState
            {
                SaveName = saveName,
                CurrentTick = simulationTick,
                SaveTimestamp = simulationTime.UtcTimestamp,
                World = world
            };

            envelope = new SaveEnvelope<FullGameState>
            {
                FormatVersion = 1,
                SavedAt = simulationTime.UtcTimestamp,
                State = fullState
            };
        }

        var saveSerializer = new JsonStateSerializer();
        var payload = saveSerializer.Serialize(envelope);
        AtomicFileWriter.WriteAllTextSafely(path, payload);

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
    /// Exports the live simulation state for a sovereign country to a JSON state file (display snapshot),
    /// while preserving any existing complete game save at the companion save path.
    /// Never overwrites a complete save with a reduced single-country representation.
    /// </summary>
    public static string ExportState(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string? saveFilePath = null,
        string? latestDecisionResult = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var path = targetPath ?? ResolveDefaultStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var companionSavePath = saveFilePath ?? Path.ChangeExtension(path, ".sav");

        // Authoritative save must succeed before writing the dashboard snapshot.
        // If saving fails (e.g. disk error, permissions, corruption), propagate the failure
        // and do not write a misleading snapshot that suggests authoritative state was saved.
        SaveAuthoritativeState(country, simulationTime, simulationTick, companionSavePath);

        var snapshot = PresidentialDeskSnapshot.Create(country, simulationTime, simulationTick, companionSavePath, latestDecisionResult: latestDecisionResult);
        var json = Serialize(snapshot);
        AtomicFileWriter.WriteAllTextSafely(path, json);

        return path;
    }

    /// <summary>
    /// Exports the live simulation state for a sovereign country within a complete game world to a JSON state file,
    /// also persisting the complete world state to the companion save file.
    /// Preserves all sovereign nations, world identity, entities, and systemic state.
    /// Throws an exception if saving the authoritative state fails, never reporting false success.
    /// </summary>
    public static string ExportState(
        WorldState world,
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string? saveFilePath = null,
        string? latestDecisionResult = null)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(country);

        var path = targetPath ?? ResolveDefaultStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var companionSavePath = saveFilePath ?? Path.ChangeExtension(path, ".sav");

        // Authoritative world save must succeed before writing the display snapshot.
        SaveAuthoritativeState(world, simulationTime, simulationTick, companionSavePath);

        var snapshot = PresidentialDeskSnapshot.Create(world, country, simulationTime, simulationTick, companionSavePath, latestDecisionResult: latestDecisionResult);
        var json = Serialize(snapshot);
        AtomicFileWriter.WriteAllTextSafely(path, json);

        return path;
    }

    /// <summary>
    /// Exports a read-only display snapshot for the Godot presidential desk without saving or updating
    /// the authoritative game save file. Explicitly indicates read-only status in the snapshot metadata.
    /// </summary>
    public static string ExportReadOnlyDashboard(
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        var path = targetPath ?? ResolveDefaultStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = PresidentialDeskSnapshot.Create(
            country,
            simulationTime,
            simulationTick,
            saveFilePath: null,
            latestDecisionResult: null,
            statusMessage: "Read-only presidential dashboard (authoritative state not synchronized)");

        var json = Serialize(snapshot);
        AtomicFileWriter.WriteAllTextSafely(path, json);

        return path;
    }

    /// <summary>
    /// Attempts to export a read-only display snapshot, catching any IO errors safely.
    /// </summary>
    public static bool TryExportReadOnlyDashboard(
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
            exportedPath = ExportReadOnlyDashboard(country, simulationTime, simulationTick, targetPath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Exports a read-only display snapshot for the Godot presidential desk for a country in a world
    /// without saving or updating the authoritative game save file. Explicitly indicates read-only status in the snapshot metadata.
    /// </summary>
    public static string ExportReadOnlyDashboard(
        WorldState world,
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(country);

        var path = targetPath ?? ResolveDefaultStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = PresidentialDeskSnapshot.Create(
            country,
            simulationTime,
            simulationTick,
            saveFilePath: null,
            latestDecisionResult: null,
            statusMessage: "Read-only presidential dashboard (authoritative state not synchronized)");

        var json = Serialize(snapshot);
        AtomicFileWriter.WriteAllTextSafely(path, json);

        return path;
    }

    /// <summary>
    /// Attempts to export a read-only display snapshot for a country in a world, catching any IO errors safely.
    /// </summary>
    public static bool TryExportReadOnlyDashboard(
        WorldState world,
        Country country,
        RepublicTime simulationTime,
        out string? exportedPath,
        ulong simulationTick = 0,
        string? targetPath = null)
    {
        exportedPath = null;
        if (world == null || country == null)
        {
            return false;
        }

        try
        {
            exportedPath = ExportReadOnlyDashboard(world, country, simulationTime, simulationTick, targetPath);
            return true;
        }
        catch
        {
            return false;
        }
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
    /// Attempts to export the live simulation state for a country within a complete world, catching any IO errors safely.
    /// </summary>
    public static bool TryExportState(
        WorldState world,
        Country country,
        RepublicTime simulationTime,
        out string? exportedPath,
        ulong simulationTick = 0,
        string? targetPath = null,
        string? saveFilePath = null)
    {
        exportedPath = null;
        if (world == null || country == null)
        {
            return false;
        }

        try
        {
            exportedPath = ExportState(world, country, simulationTime, simulationTick, targetPath, saveFilePath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to export the live simulation state for a country within a complete world, ignoring the output path.
    /// </summary>
    public static bool TryExportState(
        WorldState world,
        Country country,
        RepublicTime simulationTime,
        ulong simulationTick = 0,
        string? targetPath = null,
        string? saveFilePath = null)
    {
        return TryExportState(world, country, simulationTime, out _, simulationTick, targetPath, saveFilePath);
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
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

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
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply presidential taxation decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

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
                    RequestId = requestId ?? string.Empty,
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
                    RequestId = requestId ?? string.Empty,
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
                    RequestId = requestId ?? string.Empty,
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

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
                    }

                    if (!string.IsNullOrWhiteSpace(snapshotSavePath))
                    {
                        if (File.Exists(snapshotSavePath))
                        {
                            targetSavePath = snapshotSavePath;
                        }
                        else
                        {
                            // Authoritative save file referenced by dashboard snapshot does not exist
                            var missingReferencedSave = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply presidential taxation decision: Authoritative persistent simulation save file '{snapshotSavePath}' referenced by dashboard snapshot '{targetStatePath}' does not exist. Dashboard snapshots cannot substitute for complete saved games. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, missingReferencedSave);
                            return missingReferencedSave;
                        }
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
                                RequestId = requestId ?? string.Empty,
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

        // 3. Load the authoritative SaveEnvelope<FullGameState>
        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply presidential taxation decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));

            if (country == null)
            {
                var notFoundResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply presidential taxation decision: Sovereign country '{snapshotCountryId}' referenced by snapshot does not exist in authoritative save '{targetSavePath}'. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, notFoundResult);
                return notFoundResult;
            }
        }
        else
        {
            country = envelope.State.World.Countries.FirstOrDefault(c => string.Equals(c.Id, "player-country", StringComparison.OrdinalIgnoreCase))
                      ?? envelope.State.World.Countries.FirstOrDefault()!;
        }

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
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
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

            var exportCancelPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
                ? Path.ChangeExtension(targetStatePath, ".json")
                : targetStatePath;

            try
            {
                var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick, targetSavePath, cancelResult.Message);
                var snapshotJson = Serialize(snapshot);
                AtomicFileWriter.WriteAllTextSafely(exportCancelPath, snapshotJson);
            }
            catch { /* best-effort export */ }

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
                RequestId = requestId ?? string.Empty,
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
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            // Revert in-memory mutations
            country.TaxPolicy.IncomeTaxRate = prevRate;
            country.HappinessRating = prevApproval;
            country.BaselineStability = prevStability;

            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
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
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, outcome.Message);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcome.Message,
            CountryName = country.Name,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
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
        string? saveSlot = null,
        string? requestId = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var targetPath = stateFilePath ?? ResolveDefaultStateFilePath();

        // 1. Validate inputs (non-finite rates and constitutional bounds)
        if (!double.IsFinite(proposedIncomeTaxRate))
        {
            var invalidResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
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
                RequestId = requestId ?? string.Empty,
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

            try
            {
                var snapshot = PresidentialDeskSnapshot.Create(country, simulationTime, simulationTick, saveFilePath, cancelResult.Message);
                var snapshotJson = Serialize(snapshot);
                AtomicFileWriter.WriteAllTextSafely(targetPath, snapshotJson);
            }
            catch { /* best-effort export */ }

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
                RequestId = requestId ?? string.Empty,
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
                    RequestId = requestId ?? string.Empty,
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
        else
        {
            var effectiveSavePath = !string.IsNullOrWhiteSpace(saveFilePath)
                ? saveFilePath
                : Path.ChangeExtension(targetPath, ".sav");

            try
            {
                SaveAuthoritativeState(country, simulationTime, simulationTick + 1, effectiveSavePath);
            }
            catch (Exception ex)
            {
                country.TaxPolicy.IncomeTaxRate = prevRate;
                country.HappinessRating = prevApproval;
                country.BaselineStability = prevStability;

                var persistFail = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
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
            RequestId = requestId ?? string.Empty,
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

    /// <summary>
    /// Creates and founds a sovereign country, initializes its treasury, stability, and tax policy,
    /// saves authoritative state, and exports the state file for the Godot presidential desk.
    /// Rejects names that do not contain the whole word 'Republic'.
    /// </summary>
    public static PresidentialActionResult CreateOrLoadCountry(
        string countryName,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        if (string.IsNullOrWhiteSpace(countryName) || !CountryNameRule.Default.IsValid(countryName))
        {
            var invalidNameResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidName",
                Message = "Country name must contain the whole word 'Republic'.",
                CountryName = countryName ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, invalidNameResult);
            return invalidNameResult;
        }

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var simTime = new RepublicTime(launchEpoch, launchEpoch);
        var treasury = new RepuTreasury(10_000_000_000.0, "player-country");
        var country = Country.Found(
            name: countryName.Trim(),
            foundingTime: simTime,
            id: "player-country",
            capitalCity: "Capital City",
            treasury: treasury);

        country.HappinessRating = 70.0;
        country.BaselineStability = 75.0;
        country.TaxPolicy.IncomeTaxRate = 0.25;
        country.TaxPolicy.CorporateTaxRate = 0.20;
        country.TaxPolicy.ImportTariffRate = 0.10;

        // Establish inaugural democratic mandate: Ruling party and Opposition party
        country.FoundParty("Democratic Union", simTime, support: 55.0);
        country.FoundParty("National Civic Front", simTime);
        if (country.OppositionParty != null)
        {
            country.OppositionParty = country.OppositionParty with { Support = 45.0 };
        }
        country.HoldElection(simTime);

        SaveAuthoritativeState(country, simTime, 0, targetSavePath);
        ExportState(country, simTime, 0, targetStatePath, targetSavePath, latestDecisionResult: $"Successfully founded {country.Name}. Executive desk ready.");

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Successfully founded {country.Name}. Executive desk ready.",
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            PreviousIncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            PreviousApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            PreviousStabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "National Civic Front",
            ElectionResult = country.LastElection?.ToString() ?? string.Empty,
            IsReelected = true
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Resumes a game session from an authoritative save file, projecting the sovereign state
    /// into the presidential desk state file for live gameplay.
    /// </summary>
    public static PresidentialActionResult ResumeFromSave(
        string? saveFilePath = null,
        string? stateFilePath = null,
        string? requestId = null)
    {
        var targetSavePath = saveFilePath ?? ResolveDefaultSaveFilePath();
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();

        if (!File.Exists(targetSavePath))
        {
            var missingResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot resume from save: Save file '{targetSavePath}' does not exist."
            };
            TryWriteResultFile(targetStatePath, missingResult);
            return missingResult;
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
            var corruptResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot resume from save: Save file '{targetSavePath}' is corrupted ({ex.Message})."
            };
            TryWriteResultFile(targetStatePath, corruptResult);
            return corruptResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var emptyResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot resume from save: Save file '{targetSavePath}' contains no valid countries."
            };
            TryWriteResultFile(targetStatePath, emptyResult);
            return emptyResult;
        }

        var country = envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);

        ExportState(envelope.State.World, country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: $"Resumed game session from {Path.GetFileName(targetSavePath)}.");

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Successfully resumed game for {country.Name}.",
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "National Civic Front",
            ElectionResult = country.LastElection?.ToString() ?? string.Empty,
            IsReelected = true
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Applies an executive capital development project decision (e.g. National Road Program)
    /// to the sovereign nation, deducting funds from treasury and scheduling construction.
    /// </summary>
    public static PresidentialActionResult ApplyProjectDecision(
        string projectType = "Road",
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

        // Resolve save file path
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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply project decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply project decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return ioResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply project decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return emptyResult;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply project decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply project decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return noSaveResult;
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply project decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply project decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply project decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);

        var tick = envelope.State.CurrentTick;

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Project initiation cancelled by executive order. Zero funds deducted.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None"
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        ExecutiveActionOutcome outcome;
        if (string.Equals(projectType, "PowerPlant", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(projectType, "National Power Plant", StringComparison.OrdinalIgnoreCase))
        {
            outcome = PresidentialWorkspaceHandler.InitiatePowerPlantProject(country, simTime, confirmed: true);
        }
        else if (string.Equals(projectType, "University", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(projectType, "National University", StringComparison.OrdinalIgnoreCase))
        {
            outcome = PresidentialWorkspaceHandler.InitiateUniversityProject(country, simTime, confirmed: true);
        }
        else
        {
            // Default to Road
            outcome = PresidentialWorkspaceHandler.InitiateRoadProject(country, simTime, confirmed: true);
        }

        if (!outcome.IsSuccess)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = outcome.Result.ToString(),
                Message = outcome.Message,
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None"
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        // Persist authoritative save file
        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Project not recorded.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        // Export updated dashboard snapshot for Godot
        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, latestDecisionResult: outcome.Message);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcome.Message,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = outcome.Project?.Name ?? country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None"
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Enacts an executive campaign rally directive, allocating funds from the national treasury
    /// to mobilize voters, increase ruling party support, and boost citizen approval.
    /// </summary>
    public static PresidentialActionResult ApplyCampaignRallyDecision(
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply campaign rally decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply campaign rally decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return ioResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply campaign rally decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return emptyResult;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply campaign rally decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply campaign rally decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return noSaveResult;
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply campaign rally decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply campaign rally decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply campaign rally decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        // Ensure political parties are initialized
        if (country.RulingParty == null)
        {
            country.FoundParty("Democratic Union", simTime, support: 55.0);
        }
        if (country.OppositionParty == null)
        {
            country.FoundParty("National Civic Front", simTime);
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 45.0 };
            }
        }

        const double RallyCost = 250_000.0;

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Campaign rally cancelled by executive order. Zero funds deducted.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
                RulingParty = country.RulingParty?.Name ?? "Democratic Union",
                OppositionParty = country.OppositionParty?.Name ?? "None",
                ElectionResult = country.LastElection?.ToString() ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        if (country.Treasury.Balance < RallyCost)
        {
            var noFundsResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InsufficientFunds",
                Message = $"Insufficient sovereign treasury funds. Required: {RepuTreasury.Format(RallyCost)}, Available: {country.Treasury.FormattedBalance}.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
                RulingParty = country.RulingParty?.Name ?? "Democratic Union",
                OppositionParty = country.OppositionParty?.Name ?? "None",
                ElectionResult = country.LastElection?.ToString() ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, noFundsResult);
            return noFundsResult;
        }

        country.Treasury.Withdraw(RallyCost);

        var newRulingSupport = Math.Clamp((country.RulingParty?.Support ?? 55.0) + 4.0, 0.0, 95.0);
        var newOppositionSupport = Math.Clamp((country.OppositionParty?.Support ?? 45.0) - 2.0, 5.0, 95.0);
        if (country.RulingParty != null)
        {
            country.RulingParty = country.RulingParty with { Support = newRulingSupport };
        }
        if (country.OppositionParty != null)
        {
            country.OppositionParty = country.OppositionParty with { Support = newOppositionSupport };
        }
        country.HappinessRating = Math.Clamp(country.HappinessRating + 1.5, 0.0, 100.0);

        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            country.Treasury.Deposit(RallyCost);
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Campaign rally not recorded.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        var message = $"Campaign rally successfully conducted across provinces. Deducted {RepuTreasury.Format(RallyCost)}. Ruling party support surged to {newRulingSupport:0.#}% (+4.0%), public approval at {country.HappinessRating:0.#}%.";

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, latestDecisionResult: message);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = message,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            ElectionResult = country.LastElection?.ToString() ?? string.Empty,
            IsReelected = true
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Enacts an executive town hall or public citizen address directive.
    /// Costs R50,000 from the national treasury, boosting citizen approval (+1.0%),
    /// baseline stability (+1.5%), and ruling party polling support (+1.5%).
    /// </summary>
    public static PresidentialActionResult ApplyTownHallDecision(
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        const double TownHallCost = 50_000.0;
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply town hall decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply town hall decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return ioResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply town hall decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return emptyResult;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply town hall decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply town hall decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return noSaveResult;
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply town hall decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply town hall decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply town hall decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        if (country.RulingParty == null)
        {
            country.FoundParty("Democratic Union", simTime, support: 55.0);
        }
        if (country.OppositionParty == null)
        {
            country.FoundParty("National Civic Front", simTime);
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 45.0 };
            }
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Town hall assembly cancelled by presidential directive. Zero treasury funds deducted.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
                RulingParty = country.RulingParty?.Name ?? "Democratic Union",
                OppositionParty = country.OppositionParty?.Name ?? "None"
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        if (country.Treasury.Balance < TownHallCost)
        {
            var fundsResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InsufficientFunds",
                Message = $"Cannot conduct citizen town hall: Insufficient sovereign treasury funds. Required: {RepuTreasury.Format(TownHallCost)}, Available: {country.Treasury.FormattedBalance}.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, fundsResult);
            return fundsResult;
        }

        country.Treasury.Withdraw(TownHallCost);
        var newRulingSupport = Math.Clamp((country.RulingParty?.Support ?? 55.0) + 1.5, 5.0, 95.0);
        var newOppositionSupport = Math.Clamp((country.OppositionParty?.Support ?? 45.0) - 1.5, 5.0, 95.0);
        if (country.RulingParty != null) country.RulingParty = country.RulingParty with { Support = newRulingSupport };
        if (country.OppositionParty != null) country.OppositionParty = country.OppositionParty with { Support = newOppositionSupport };

        country.HappinessRating = Math.Clamp(country.HappinessRating + 1.0, 0.0, 100.0);
        country.BaselineStability = Math.Clamp(country.BaselineStability + 1.5, 0.0, 100.0);

        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            country.Treasury.Deposit(TownHallCost);
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Town hall not recorded.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        var message = $"Citizen Town Hall conducted successfully. Deducted {RepuTreasury.Format(TownHallCost)}. Public approval rose to {country.HappinessRating:0.#}%, stability to {country.BaselineStability:0.#}%, and ruling support to {newRulingSupport:0.#}%.";

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, latestDecisionResult: message);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = message,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            ElectionResult = country.LastElection?.ToString() ?? string.Empty,
            IsReelected = true
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Enacts an emergency executive crisis decree to resolve sovereign fiscal insolvency or civil unrest.
    /// Decree options:
    /// - "civic_relief": Allocates R100,000 to public services, lifting approval (+4.0%) and stability (+3.0%).
    /// - "austerity": Injects R500,000 from spending cuts, but reduces approval (-4.0%) and stability (-2.0%).
    /// </summary>
    public static PresidentialActionResult ApplyCrisisResponseDecision(
        string decreeType,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply crisis decree: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply crisis decree: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return ioResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply crisis decree: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return emptyResult;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply crisis decree: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply crisis decree: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return noSaveResult;
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply crisis decree: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply crisis decree: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply crisis decree: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Emergency decree cancelled by presidential directive. No fiscal changes enacted.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None"
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        string outcomeMessage;
        var normalizedDecree = decreeType?.Trim().ToLowerInvariant() ?? "austerity";

        if (normalizedDecree is "civic_relief" or "relief")
        {
            const double ReliefCost = 100_000.0;
            if (country.Treasury.Balance < ReliefCost)
            {
                var fundsResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "InsufficientFunds",
                    Message = $"Cannot enact Civic Relief Package: Insufficient treasury reserves. Required: {RepuTreasury.Format(ReliefCost)}, Available: {country.Treasury.FormattedBalance}.",
                    CountryName = country.Name,
                    IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                    ApprovalRating = country.HappinessRating,
                    StabilityRating = country.BaselineStability,
                    TreasuryBalance = country.Treasury.Balance
                };
                TryWriteResultFile(targetStatePath, fundsResult);
                return fundsResult;
            }

            country.Treasury.Withdraw(ReliefCost);
            country.HappinessRating = Math.Clamp(country.HappinessRating + 4.0, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability + 3.0, 0.0, 100.0);
            outcomeMessage = $"Civic Relief Package enacted: Allocated {RepuTreasury.Format(ReliefCost)} to citizen emergency services. Approval lifted to {country.HappinessRating:0.#}%, stability restored to {country.BaselineStability:0.#}%.";
        }
        else
        {
            // Default: Emergency Austerity
            const double Influx = 500_000.0;
            country.Treasury.Deposit(Influx);
            country.HappinessRating = Math.Clamp(country.HappinessRating - 4.0, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability - 2.0, 0.0, 100.0);
            outcomeMessage = $"Emergency Austerity Decree enacted: Reallocated {RepuTreasury.Format(Influx)} to liquid operating reserves from ministerial reductions. Public approval adjusted to {country.HappinessRating:0.#}%, stability at {country.BaselineStability:0.#}%.";
        }

        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Crisis decree not recorded.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            ElectionResult = country.LastElection?.ToString() ?? string.Empty,
            IsReelected = true
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Enacts a democratic national election decision, holding an election between the ruling party
    /// and opposition party, factoring in citizen approval and stability, certifying the result,
    /// and either renewing the executive mandate or enacting a peaceful transition of power.
    /// </summary>
    public static PresidentialActionResult ApplyElectionDecision(
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        string? snapshotCountryId = null;

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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply election decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return missingResult;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply election decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return ioResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply election decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return emptyResult;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot apply election decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot apply election decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return noSaveResult;
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply election decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply election decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply election decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        if (country.RulingParty == null)
        {
            country.FoundParty("Democratic Union", simTime, support: 55.0);
        }
        if (country.OppositionParty == null)
        {
            country.FoundParty("National Civic Front", simTime);
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 45.0 };
            }
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "National election call cancelled by executive order. Current administration mandate remains intact.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
                RulingParty = country.RulingParty?.Name ?? "Democratic Union",
                OppositionParty = country.OppositionParty?.Name ?? "None",
                ElectionResult = country.LastElection?.ToString() ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        // Dynamically link public approval to voter sentiment
        var approvalDelta = (country.HappinessRating - 50.0) * 0.2;
        var effectiveRulingSupport = Math.Clamp((country.RulingParty?.Support ?? 55.0) + approvalDelta, 5.0, 95.0);
        var effectiveOppSupport = Math.Clamp((country.OppositionParty?.Support ?? 45.0) - approvalDelta, 5.0, 95.0);
        if (country.RulingParty != null)
        {
            country.RulingParty = country.RulingParty with { Support = effectiveRulingSupport };
        }
        if (country.OppositionParty != null)
        {
            country.OppositionParty = country.OppositionParty with { Support = effectiveOppSupport };
        }

        var electionBoundary = (country.LastElection != null && country.LastElection.Boundary == simTime)
            ? simTime.Add(TimeSpan.FromDays(30))
            : simTime;

        var previousRulingName = country.RulingParty?.Name ?? string.Empty;
        var election = country.HoldElection(electionBoundary);
        if (election == null)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PrerequisitesNotMet",
                Message = "Unable to hold national election. Both ruling and opposition parties must be established.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        bool isReelected = string.Equals(election.WinnerPartyName, previousRulingName, StringComparison.OrdinalIgnoreCase);
        string outcomeMessage;

        if (isReelected)
        {
            country.HappinessRating = Math.Clamp(country.HappinessRating + 2.5, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability + 2.0, 0.0, 100.0);
            outcomeMessage = $"Electoral Victory: {election.WinnerPartyName} re-elected with {election.WinnerSupport:0.#}% of the national vote! Executive mandate renewed.";
        }
        else
        {
            country.BaselineStability = Math.Clamp(country.BaselineStability - 2.0, 0.0, 100.0);
            country.VacatePortfolio(CabinetPortfolio.Finance);
            country.VacatePortfolio(CabinetPortfolio.Interior);
            country.VacatePortfolio(CabinetPortfolio.Infrastructure);
            country.RecalculateCabinetStateCapacity();
            outcomeMessage = $"Democratic Transition: {election.WinnerPartyName} won the national election with {election.WinnerSupport:0.#}% of the vote ({election.LoserPartyName}: {election.LoserSupport:0.#}%). Opposition assumes the ruling executive mandate. Ministerial portfolios vacated for incoming administration.";
        }

        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Election not certified.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = PresidentialDeskSnapshot.Create(country, electionBoundary, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            ElectionResult = election.ToString(),
            IsReelected = isReelected
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Executes an authoritative executive cabinet appointment decision (Technocrat vs Party Loyalist),
    /// recalculating ministerial state capacities and applying political polling consequences.
    /// Persists changes to the authoritative .sav file and updates the presidential desk state file.
    /// </summary>
    public static PresidentialActionResult ApplyCabinetAppointmentDecision(
        string portfolioName,
        string candidateType,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? ResolveDefaultSaveFilePath();
        string? snapshotCountryId = null;

        if (!File.Exists(targetStatePath))
        {
            var missingResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, missingResult);
            return missingResult;
        }

        string rawJson;
        try
        {
            rawJson = File.ReadAllText(targetStatePath);
        }
        catch (Exception ex)
        {
            var ioResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, ioResult);
            return ioResult;
        }

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            var emptyResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Simulation state file '{targetStatePath}' is empty. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, emptyResult);
            return emptyResult;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(rawJson);
        }
        catch (JsonException)
        {
            var corruptResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptResult);
            return corruptResult;
        }

        using (doc)
        {
            if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
            {
                targetSavePath = targetStatePath;
            }
            else
            {
                string? snapshotSavePath = null;
                if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                {
                    snapshotSavePath = savePathEl.GetString();
                }

                if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                {
                    snapshotCountryId = cIdEl.GetString();
                }
                if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                {
                    snapshotCountryId = cNameEl.GetString();
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
                        var noSaveResult = new PresidentialActionResult
                        {
                            RequestId = requestId ?? string.Empty,
                            Success = false,
                            Result = "StateMissingOrCorrupt",
                            Message = $"Cannot apply cabinet appointment decision: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                        };
                        TryWriteResultFile(targetStatePath, noSaveResult);
                        return noSaveResult;
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return corruptSaveResult;
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot apply cabinet appointment decision: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        if (country.RulingParty == null)
        {
            country.FoundParty("Democratic Union", simTime, support: 55.0);
        }
        if (country.OppositionParty == null)
        {
            country.FoundParty("National Civic Front", simTime);
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 45.0 };
            }
        }

        var portfolio = CabinetPortfolio.Finance;
        if (portfolioName != null)
        {
            if (portfolioName.Contains("Interior", StringComparison.OrdinalIgnoreCase))
            {
                portfolio = CabinetPortfolio.Interior;
            }
            else if (portfolioName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase) || portfolioName.Contains("Infra", StringComparison.OrdinalIgnoreCase))
            {
                portfolio = CabinetPortfolio.Infrastructure;
            }
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = $"Cabinet appointment order for {portfolio} cancelled by executive order. Existing ministerial portfolio and political standing preserved.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance,
                ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
                RulingParty = country.RulingParty?.Name ?? "Democratic Union",
                OppositionParty = country.OppositionParty?.Name ?? "None",
                Portfolio = portfolio.ToString(),
                CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
                CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        string outcomeMessage;
        Minister? appointed = null;
        var normalizedType = candidateType?.Trim().ToLowerInvariant() ?? "technocrat";

        if (normalizedType is "technocrat" or "expert")
        {
            var ministerName = portfolio switch
            {
                CabinetPortfolio.Finance => "Dr. Elena Rostova",
                CabinetPortfolio.Interior => "Dr. Alistair Finch",
                _ => "Eng. Marcus Bennett"
            };

            appointed = new Minister
            {
                Name = ministerName,
                Portfolio = portfolio,
                Competence = 0.90,
                Integrity = 0.90,
                Loyalty = 0.50,
                Experience = 0.85,
                PoliticalConnections = 0.40
            };

            country.AppointMinister(appointed, portfolio);

            var effRuling = Math.Clamp((country.RulingParty?.Support ?? 55.0) - 3.0, 5.0, 95.0);
            var effOpp = Math.Clamp((country.OppositionParty?.Support ?? 45.0) + 3.0, 5.0, 95.0);
            if (country.RulingParty != null) country.RulingParty = country.RulingParty with { Support = effRuling };
            if (country.OppositionParty != null) country.OppositionParty = country.OppositionParty with { Support = effOpp };

            country.HappinessRating = Math.Clamp(country.HappinessRating + 1.5, 0.0, 100.0);
            outcomeMessage = $"Technocrat Appointed: {appointed.Name} commissioned as Minister of {portfolio}. Administrative competence elevated to 90%, but party loyalists are discontented (-3.0% polling support).";
        }
        else if (normalizedType is "loyalist" or "party_loyalist")
        {
            var ministerName = portfolio switch
            {
                CabinetPortfolio.Finance => "Senator Donald Vance",
                CabinetPortfolio.Interior => "Party Whip Thomas Sterling",
                _ => "Alderman Gregory Cross"
            };

            appointed = new Minister
            {
                Name = ministerName,
                Portfolio = portfolio,
                Competence = 0.45,
                Integrity = 0.45,
                Loyalty = 0.95,
                Experience = 0.50,
                PoliticalConnections = 0.90
            };

            country.AppointMinister(appointed, portfolio);

            var effRuling = Math.Clamp((country.RulingParty?.Support ?? 55.0) + 3.0, 5.0, 95.0);
            var effOpp = Math.Clamp((country.OppositionParty?.Support ?? 45.0) - 3.0, 5.0, 95.0);
            if (country.RulingParty != null) country.RulingParty = country.RulingParty with { Support = effRuling };
            if (country.OppositionParty != null) country.OppositionParty = country.OppositionParty with { Support = effOpp };

            country.HappinessRating = Math.Clamp(country.HappinessRating - 1.5, 0.0, 100.0);
            outcomeMessage = $"Party Loyalist Appointed: {appointed.Name} commissioned as Minister of {portfolio}. Party base rallied (+3.0% polling support), but administrative competence is compromised (45%).";
        }
        else if (normalizedType is "dismiss" or "vacate")
        {
            country.VacatePortfolio(portfolio);
            outcomeMessage = $"Portfolio Vacated: Minister of {portfolio} dismissed. Administrative capacity reverted to emergency baseline (30%).";
        }
        else
        {
            var invalidTypeResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidInput",
                Message = $"Cabinet appointment rejected: Unknown candidate type '{candidateType}'. Must be 'technocrat', 'loyalist', or 'dismiss'.",
                CountryName = country.Name
            };
            TryWriteResultFile(targetStatePath, invalidTypeResult);
            return invalidTypeResult;
        }

        envelope.State.CurrentTick = tick + 1;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        try
        {
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }
        catch (Exception ex)
        {
            var persistFailResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PersistenceFailure",
                Message = $"Persistence error: Failed to save authoritative simulation state ({ex.Message}). Cabinet appointment not certified.",
                CountryName = country.Name
            };
            TryWriteResultFile(targetStatePath, persistFailResult);
            return persistFailResult;
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = PresidentialDeskSnapshot.Create(country, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            Portfolio = portfolio.ToString(),
            AppointedMinister = appointed?.Name ?? "Vacant",
            CabinetSummary = snapshot?.CabinetSummary ?? string.Empty,
            CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
            CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    private static (SaveEnvelope<FullGameState>? Envelope, Country? Country, RepublicTime SimTime, ulong Tick, string TargetSavePath, PresidentialActionResult? Error) ResolveAuthoritativeCountry(
        string targetStatePath,
        string? saveFilePath,
        string? requestId,
        string operationName)
    {
        string? snapshotCountryId = null;
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
            if (!File.Exists(targetStatePath))
            {
                var missingResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot {operationName}: Authoritative simulation state file '{targetStatePath}' does not exist. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, missingResult);
                return (null, null, default, 0, string.Empty, missingResult);
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetStatePath);
            }
            catch (Exception ex)
            {
                var ioResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot {operationName}: Unable to read simulation state file '{targetStatePath}' ({ex.Message}). No changes enacted."
                };
                TryWriteResultFile(targetStatePath, ioResult);
                return (null, null, default, 0, string.Empty, ioResult);
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                var emptyResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot {operationName}: Simulation state file '{targetStatePath}' is empty. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, emptyResult);
                return (null, null, default, 0, string.Empty, emptyResult);
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawJson);
            }
            catch (JsonException)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "StateMissingOrCorrupt",
                    Message = $"Cannot {operationName}: Simulation state file '{targetStatePath}' contains corrupted or invalid JSON. No changes enacted."
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return (null, null, default, 0, string.Empty, corruptResult);
            }

            using (doc)
            {
                if (doc.RootElement.TryGetProperty("state", out var stateEl) && stateEl.TryGetProperty("world", out _))
                {
                    targetSavePath = targetStatePath;
                }
                else
                {
                    string? snapshotSavePath = null;
                    if (doc.RootElement.TryGetProperty("save_file_path", out var savePathEl))
                    {
                        snapshotSavePath = savePathEl.GetString();
                    }

                    if (doc.RootElement.TryGetProperty("country_id", out var cIdEl))
                    {
                        snapshotCountryId = cIdEl.GetString();
                    }
                    if (string.IsNullOrWhiteSpace(snapshotCountryId) && doc.RootElement.TryGetProperty("country_name", out var cNameEl))
                    {
                        snapshotCountryId = cNameEl.GetString();
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
                            var noSaveResult = new PresidentialActionResult
                            {
                                RequestId = requestId ?? string.Empty,
                                Success = false,
                                Result = "StateMissingOrCorrupt",
                                Message = $"Cannot {operationName}: Authoritative persistent simulation save file could not be found for dashboard snapshot '{targetStatePath}'. No changes enacted."
                            };
                            TryWriteResultFile(targetStatePath, noSaveResult);
                            return (null, null, default, 0, string.Empty, noSaveResult);
                        }
                    }
                }
            }
        }

        if (!File.Exists(targetSavePath))
        {
            var missingSaveResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot {operationName}: Authoritative simulation save file '{targetSavePath}' does not exist. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, missingSaveResult);
            return (null, null, default, 0, string.Empty, missingSaveResult);
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
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot {operationName}: Authoritative simulation save file '{targetSavePath}' is corrupted or contains invalid data ({ex.Message}). No changes enacted."
            };
            TryWriteResultFile(targetStatePath, corruptSaveResult);
            return (null, null, default, 0, string.Empty, corruptSaveResult);
        }

        if (envelope?.State?.World?.Countries == null || envelope.State.World.Countries.Count == 0)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "StateMissingOrCorrupt",
                Message = $"Cannot {operationName}: Authoritative save file '{targetSavePath}' does not contain a valid sovereign country. No changes enacted."
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return (null, null, default, 0, string.Empty, noCountryResult);
        }

        Country? country = null;
        if (!string.IsNullOrWhiteSpace(snapshotCountryId))
        {
            country = envelope.State.World.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, snapshotCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, snapshotCountryId, StringComparison.OrdinalIgnoreCase));
        }

        country ??= envelope.State.World.Countries.FirstOrDefault(c => c.Id == "player-country")
            ?? envelope.State.World.Countries.First();

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var saveTime = envelope.State.SaveTimestamp != default ? envelope.State.SaveTimestamp : launchEpoch;
        var simTime = new RepublicTime(saveTime, launchEpoch);
        var tick = envelope.State.CurrentTick;

        return (envelope, country, simTime, tick, targetSavePath, null);
    }

    /// <summary>
    /// Evaluates and applies a presidential decision regarding a proactive Chief of Staff recommendation.
    /// Decisions: "accept", "reject", "defer".
    /// </summary>
    public static PresidentialActionResult ApplyChiefOfStaffDecision(
        string recommendationId,
        string decision,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "apply Chief of Staff decision");

        if (error != null)
        {
            return error;
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Chief of Staff recommendation review cancelled. No executive action taken.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var officialsService = new ActiveOfficialsService();
        bool success = officialsService.ApplyRecommendationDecision(country!, recommendationId, decision, out var outcomeMessage);

        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = outcomeMessage,
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        try
        {
            SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            CabinetSummary = snapshot?.CabinetSummary ?? string.Empty,
            CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
            CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Executes a presidential security directive in response to a national security threat report.
    /// Directives: "mobilize", "sweep", "deescalate", "set_defcon".
    /// </summary>
    public static PresidentialActionResult ApplySecurityDirective(
        string threatId,
        string directive,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "apply security directive");

        if (error != null)
        {
            return error;
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Security directive order cancelled. National security posture unchanged.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var officialsService = new ActiveOfficialsService();
        bool success = officialsService.ApplySecurityDirective(country!, threatId, directive, out var outcomeMessage);

        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = outcomeMessage,
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        try
        {
            SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            CabinetSummary = snapshot?.CabinetSummary ?? string.Empty,
            CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
            CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Executes an authoritative presidential media-management action (statement, hush_money, or censorship)
    /// in response to a published or leaked media report, updating authoritative state and exporting an updated snapshot.
    /// </summary>
    public static PresidentialActionResult ApplyMediaResponse(
        string reportId,
        string responseType,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "apply media management response");

        if (error != null)
        {
            return error;
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Media response action cancelled. No executive action taken.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var mediaService = new MediaSimulationService();
        bool success = mediaService.ApplyMediaResponse(country!, reportId, responseType, out var outcomeMessage);

        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = outcomeMessage,
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        try
        {
            SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            CabinetSummary = snapshot?.CabinetSummary ?? string.Empty,
            CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
            CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    /// <summary>
    /// Executes an authoritative presidential investment decision (accept, reject, or negotiate)
    /// on a private-sector investment proposal, updating authoritative state and exporting an updated snapshot.
    /// </summary>
    public static PresidentialActionResult ApplyInvestmentDecision(
        string proposalId,
        string decision,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "apply private sector investment decision");

        if (error != null)
        {
            return error;
        }

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Investment proposal review cancelled. No executive action taken.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var privateSectorService = new PrivateSectorService();
        bool success = privateSectorService.ApplyInvestmentDecision(country!, proposalId, decision, out var outcomeMessage);

        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = outcomeMessage,
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        try
        {
            SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { /* best-effort export */ }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance,
            ActiveProject = country.Projects.FirstOrDefault(p => p.IsActive)?.Name ?? "None",
            RulingParty = country.RulingParty?.Name ?? "Democratic Union",
            OppositionParty = country.OppositionParty?.Name ?? "None",
            CabinetSummary = snapshot?.CabinetSummary ?? string.Empty,
            CivilServiceQuality = country.StateCapacity?.CivilServiceQuality ?? 0.3,
            CorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3
        };

        TryWriteResultFile(exportPath, successResult);
        return successResult;
    }

    private static void TryWriteResultFile(string stateOrSavePath, PresidentialActionResult result)
    {
        try
        {
            var resultPath = ResolveDefaultResultFilePath(stateOrSavePath);
            var resultJson = JsonSerializer.Serialize(result, SerializerOptions);
            AtomicFileWriter.WriteAllTextSafely(resultPath, resultJson);
        }
        catch { /* best-effort write */ }
    }

    /// <summary>
    /// Checks for and processes any pending presidential decision request written to disk by the Godot client.
    /// If presidential_desk_request.json exists, reads the proposed rate and confirmed flag,
    /// applies the decision via <see cref="ApplyTaxDecision(double, bool, string?, string?)"/>,
    /// ensures the updated state file is exported, deletes the request file, and returns the action result.
    /// Returns null if no request file exists.
    /// </summary>
    public static PresidentialActionResult? ProcessPendingDecisionRequest(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestFilePath = null)
    {
        lock (RequestProcessingLock)
        {
            var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
            var targetRequestPath = requestFilePath ?? ResolveDefaultRequestFilePath(targetStatePath);

            if (!File.Exists(targetRequestPath))
            {
                return null;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(targetRequestPath);
            }
            catch (IOException)
            {
                // Transient file lock while client finishes writing; wait briefly and retry
                System.Threading.Thread.Sleep(50);
                try
                {
                    rawJson = File.ReadAllText(targetRequestPath);
                }
                catch
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                // Brief pause in case client just created the file handle but hasn't flushed bytes yet
                System.Threading.Thread.Sleep(50);
                try
                {
                    rawJson = File.ReadAllText(targetRequestPath);
                }
                catch { }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    var emptyResult = CreateInvalidInputResult(
                        targetStatePath,
                        "Decision request file was empty or contained no readable JSON content.");
                    TryWriteResultFile(targetStatePath, emptyResult);
                    TryDeleteFile(targetRequestPath);
                    return emptyResult;
                }
            }

            string? requestId = null;
            double proposedRate;
            bool confirmed;

            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    var invalidObjectResult = CreateInvalidInputResult(
                        targetStatePath,
                        "Decision request must be a JSON object.",
                        requestId);
                    TryWriteResultFile(targetStatePath, invalidObjectResult);
                    TryDeleteFile(targetRequestPath);
                    return invalidObjectResult;
                }

                if (root.TryGetProperty("request_id", out var reqIdEl) && reqIdEl.ValueKind == JsonValueKind.String)
                {
                    requestId = reqIdEl.GetString();
                }

                // Check for explicit action / request_type
                string? action = null;
                if (root.TryGetProperty("action", out var actionEl) && actionEl.ValueKind == JsonValueKind.String)
                {
                    action = actionEl.GetString();
                }
                else if (root.TryGetProperty("request_type", out var reqTypeEl) && reqTypeEl.ValueKind == JsonValueKind.String)
                {
                    action = reqTypeEl.GetString();
                }

                if (!string.IsNullOrWhiteSpace(action))
                {
                    var normalizedAction = action.Trim().ToLowerInvariant();
                    if (normalizedAction is "create_country" or "found_country")
                    {
                        string countryName = string.Empty;
                        if (root.TryGetProperty("country_name", out var cNameEl) && cNameEl.ValueKind == JsonValueKind.String)
                        {
                            countryName = cNameEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                        {
                            countryName = nameEl.GetString() ?? string.Empty;
                        }

                        try
                        {
                            return CreateOrLoadCountry(countryName, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "load_save" or "resume_save")
                    {
                        string? specifiedSave = null;
                        if (root.TryGetProperty("save_file", out var sFileEl) && sFileEl.ValueKind == JsonValueKind.String)
                        {
                            specifiedSave = sFileEl.GetString();
                        }
                        else if (root.TryGetProperty("path", out var pEl) && pEl.ValueKind == JsonValueKind.String)
                        {
                            specifiedSave = pEl.GetString();
                        }

                        try
                        {
                            return ResumeFromSave(specifiedSave ?? saveFilePath, targetStatePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "project" or "start_project")
                    {
                        string projectType = "Road";
                        if (root.TryGetProperty("project_type", out var pTypeEl) && pTypeEl.ValueKind == JsonValueKind.String)
                        {
                            projectType = pTypeEl.GetString() ?? "Road";
                        }
                        else if (root.TryGetProperty("type", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            projectType = tEl.GetString() ?? "Road";
                        }

                        bool projectConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var projConfEl) && (projConfEl.ValueKind == JsonValueKind.True || projConfEl.ValueKind == JsonValueKind.False))
                        {
                            projectConfirmed = projConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyProjectDecision(projectType, projectConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "campaign_rally" or "rally" or "campaign")
                    {
                        bool rallyConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var rConfEl) && (rConfEl.ValueKind == JsonValueKind.True || rConfEl.ValueKind == JsonValueKind.False))
                        {
                            rallyConfirmed = rConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyCampaignRallyDecision(rallyConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "contest_election" or "hold_election" or "election")
                    {
                        bool electConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var eConfEl) && (eConfEl.ValueKind == JsonValueKind.True || eConfEl.ValueKind == JsonValueKind.False))
                        {
                            electConfirmed = eConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyElectionDecision(electConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "appoint_minister" or "cabinet" or "cabinet_appointment")
                    {
                        string portfolio = "Finance";
                        if (root.TryGetProperty("portfolio", out var pEl) && pEl.ValueKind == JsonValueKind.String)
                        {
                            portfolio = pEl.GetString() ?? "Finance";
                        }

                        string candidateType = "technocrat";
                        if (root.TryGetProperty("candidate_type", out var cTypeEl) && cTypeEl.ValueKind == JsonValueKind.String)
                        {
                            candidateType = cTypeEl.GetString() ?? "technocrat";
                        }
                        else if (root.TryGetProperty("type", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            candidateType = tEl.GetString() ?? "technocrat";
                        }

                        bool cabinetConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var cabConfEl) && (cabConfEl.ValueKind == JsonValueKind.True || cabConfEl.ValueKind == JsonValueKind.False))
                        {
                            cabinetConfirmed = cabConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyCabinetAppointmentDecision(portfolio, candidateType, cabinetConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "town_hall" or "public_address" or "townhall")
                    {
                        bool townHallConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var thConfEl) && (thConfEl.ValueKind == JsonValueKind.True || thConfEl.ValueKind == JsonValueKind.False))
                        {
                            townHallConfirmed = thConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyTownHallDecision(townHallConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "crisis_response" or "emergency_decree" or "crisis")
                    {
                        string decreeType = "austerity";
                        if (root.TryGetProperty("decree_type", out var dTypeEl) && dTypeEl.ValueKind == JsonValueKind.String)
                        {
                            decreeType = dTypeEl.GetString() ?? "austerity";
                        }
                        else if (root.TryGetProperty("type", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            decreeType = tEl.GetString() ?? "austerity";
                        }

                        bool crisisConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var cConfEl) && (cConfEl.ValueKind == JsonValueKind.True || cConfEl.ValueKind == JsonValueKind.False))
                        {
                            crisisConfirmed = cConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyCrisisResponseDecision(decreeType, crisisConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "cos_recommendation" or "chief_of_staff_decision" or "recommendation_decision")
                    {
                        string recId = string.Empty;
                        if (root.TryGetProperty("recommendation_id", out var rIdEl) && rIdEl.ValueKind == JsonValueKind.String)
                        {
                            recId = rIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        {
                            recId = idEl.GetString() ?? string.Empty;
                        }

                        string decision = "accept";
                        if (root.TryGetProperty("decision", out var decEl) && decEl.ValueKind == JsonValueKind.String)
                        {
                            decision = decEl.GetString() ?? "accept";
                        }
                        else if (root.TryGetProperty("action_type", out var atEl) && atEl.ValueKind == JsonValueKind.String)
                        {
                            decision = atEl.GetString() ?? "accept";
                        }

                        bool cosConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var cosConfEl) && (cosConfEl.ValueKind == JsonValueKind.True || cosConfEl.ValueKind == JsonValueKind.False))
                        {
                            cosConfirmed = cosConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyChiefOfStaffDecision(recId, decision, cosConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "security_directive" or "security_response" or "threat_response")
                    {
                        string threatId = string.Empty;
                        if (root.TryGetProperty("threat_id", out var tIdEl) && tIdEl.ValueKind == JsonValueKind.String)
                        {
                            threatId = tIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var tidEl) && tidEl.ValueKind == JsonValueKind.String)
                        {
                            threatId = tidEl.GetString() ?? string.Empty;
                        }

                        string directive = "mobilize";
                        if (root.TryGetProperty("directive", out var dirEl) && dirEl.ValueKind == JsonValueKind.String)
                        {
                            directive = dirEl.GetString() ?? "mobilize";
                        }
                        else if (root.TryGetProperty("response", out var respEl) && respEl.ValueKind == JsonValueKind.String)
                        {
                            directive = respEl.GetString() ?? "mobilize";
                        }

                        bool secConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var secConfEl) && (secConfEl.ValueKind == JsonValueKind.True || secConfEl.ValueKind == JsonValueKind.False))
                        {
                            secConfirmed = secConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplySecurityDirective(threatId, directive, secConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "media_response" or "media_action" or "respond_media")
                    {
                        string reportId = string.Empty;
                        if (root.TryGetProperty("report_id", out var repIdEl) && repIdEl.ValueKind == JsonValueKind.String)
                        {
                            reportId = repIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var ridEl) && ridEl.ValueKind == JsonValueKind.String)
                        {
                            reportId = ridEl.GetString() ?? string.Empty;
                        }

                        string responseType = "statement";
                        if (root.TryGetProperty("response_type", out var rTypeEl) && rTypeEl.ValueKind == JsonValueKind.String)
                        {
                            responseType = rTypeEl.GetString() ?? "statement";
                        }
                        else if (root.TryGetProperty("type", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            responseType = tEl.GetString() ?? "statement";
                        }
                        else if (root.TryGetProperty("decision", out var decEl) && decEl.ValueKind == JsonValueKind.String)
                        {
                            responseType = decEl.GetString() ?? "statement";
                        }

                        bool mediaConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var mConfEl) && (mConfEl.ValueKind == JsonValueKind.True || mConfEl.ValueKind == JsonValueKind.False))
                        {
                            mediaConfirmed = mConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyMediaResponse(reportId, responseType, mediaConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "investment_decision" or "investment_action" or "invest_decision")
                    {
                        string proposalId = string.Empty;
                        if (root.TryGetProperty("proposal_id", out var propIdEl) && propIdEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = propIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var pidEl) && pidEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = pidEl.GetString() ?? string.Empty;
                        }

                        string decision = "accept";
                        if (root.TryGetProperty("decision", out var decEl) && decEl.ValueKind == JsonValueKind.String)
                        {
                            decision = decEl.GetString() ?? "accept";
                        }
                        else if (root.TryGetProperty("action_type", out var atEl) && atEl.ValueKind == JsonValueKind.String)
                        {
                            decision = atEl.GetString() ?? "accept";
                        }

                        bool investConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var iConfEl) && (iConfEl.ValueKind == JsonValueKind.True || iConfEl.ValueKind == JsonValueKind.False))
                        {
                            investConfirmed = iConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyInvestmentDecision(proposalId, decision, investConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }
                }

                // 1. Validate rate field presence and format for taxation decisions
                bool isPercentField = root.TryGetProperty("proposed_rate_percent", out var propRateEl);
                bool hasRateField = isPercentField ||
                    root.TryGetProperty("proposed_income_tax_rate", out propRateEl) ||
                    root.TryGetProperty("proposed_rate", out propRateEl) ||
                    root.TryGetProperty("rate", out propRateEl) ||
                    root.TryGetProperty("income_tax_rate", out propRateEl);

                if (!hasRateField)
                {
                    var missingRateResult = CreateInvalidInputResult(
                        targetStatePath,
                        "Decision request is missing the required proposed tax rate field (e.g. 'proposed_income_tax_rate').",
                        requestId);
                    TryWriteResultFile(targetStatePath, missingRateResult);
                    TryDeleteFile(targetRequestPath);
                    return missingRateResult;
                }

                if (propRateEl.ValueKind == JsonValueKind.Number)
                {
                    if (!propRateEl.TryGetDouble(out var rateVal) || !double.IsFinite(rateVal))
                    {
                        var nonFiniteResult = CreateInvalidInputResult(
                            targetStatePath,
                            "Taxation policy rejected: rates must be finite, valid numbers.",
                            requestId);
                        TryWriteResultFile(targetStatePath, nonFiniteResult);
                        TryDeleteFile(targetRequestPath);
                        return nonFiniteResult;
                    }

                    if (isPercentField)
                    {
                        proposedRate = rateVal / 100.0;
                    }
                    else if (rateVal >= 2.0)
                    {
                        proposedRate = rateVal / 100.0;
                    }
                    else
                    {
                        proposedRate = rateVal;
                    }
                }
                else if (propRateEl.ValueKind == JsonValueKind.String)
                {
                    var strVal = propRateEl.GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(strVal))
                    {
                        var emptyStrResult = CreateInvalidInputResult(
                            targetStatePath,
                            "Taxation policy rejected: proposed tax rate string is empty.",
                            requestId);
                        TryWriteResultFile(targetStatePath, emptyStrResult);
                        TryDeleteFile(targetRequestPath);
                        return emptyStrResult;
                    }

                    // Check for strings representing NaN or Infinity explicitly
                    if (string.Equals(strVal, "NaN", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(strVal, "Infinity", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(strVal, "+Infinity", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(strVal, "-Infinity", StringComparison.OrdinalIgnoreCase))
                    {
                        var nonFiniteStrResult = CreateInvalidInputResult(
                            targetStatePath,
                            "Taxation policy rejected: rates must be finite, valid numbers.",
                            requestId);
                        TryWriteResultFile(targetStatePath, nonFiniteStrResult);
                        TryDeleteFile(targetRequestPath);
                        return nonFiniteStrResult;
                    }

                    bool hasPercentSign = strVal.EndsWith('%');
                    var numPart = hasPercentSign ? strVal[..^1].Trim() : strVal;

                    if (!double.TryParse(numPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedRate) ||
                        !double.IsFinite(parsedRate))
                    {
                        var parseFailResult = CreateInvalidInputResult(
                            targetStatePath,
                            $"Taxation policy rejected: '{strVal}' is not a valid numeric tax rate.",
                            requestId);
                        TryWriteResultFile(targetStatePath, parseFailResult);
                        TryDeleteFile(targetRequestPath);
                        return parseFailResult;
                    }

                    if (hasPercentSign || isPercentField)
                    {
                        proposedRate = parsedRate / 100.0;
                    }
                    else if (parsedRate >= 2.0)
                    {
                        proposedRate = parsedRate / 100.0;
                    }
                    else
                    {
                        proposedRate = parsedRate;
                    }
                }
                else
                {
                    // Invalid type (e.g. boolean, array, object, null)
                    var invalidTypeResult = CreateInvalidInputResult(
                        targetStatePath,
                        $"Taxation policy rejected: tax rate field has invalid JSON type '{propRateEl.ValueKind}'.",
                        requestId);
                    TryWriteResultFile(targetStatePath, invalidTypeResult);
                    TryDeleteFile(targetRequestPath);
                    return invalidTypeResult;
                }

                // Range validation (0.0 to 0.70)
                if (proposedRate < 0.0 || proposedRate > 0.70)
                {
                    var rangeResult = CreateInvalidInputResult(
                        targetStatePath,
                        "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%).",
                        requestId);
                    TryWriteResultFile(targetStatePath, rangeResult);
                    TryDeleteFile(targetRequestPath);
                    return rangeResult;
                }

                // 2. Validate confirmation field presence and format
                bool hasConfField =
                    root.TryGetProperty("confirmed", out var confEl) ||
                    root.TryGetProperty("is_confirmed", out confEl);

                if (!hasConfField)
                {
                    var missingConfResult = CreateInvalidInputResult(
                        targetStatePath,
                        "Decision request is missing the required confirmation field ('confirmed' or 'is_confirmed').",
                        requestId);
                    TryWriteResultFile(targetStatePath, missingConfResult);
                    TryDeleteFile(targetRequestPath);
                    return missingConfResult;
                }

                if (confEl.ValueKind != JsonValueKind.True && confEl.ValueKind != JsonValueKind.False)
                {
                    var nonBoolConfResult = CreateInvalidInputResult(
                        targetStatePath,
                        $"Decision request confirmation field must be a valid Boolean (true/false), but found '{confEl.ValueKind}'.",
                        requestId);
                    TryWriteResultFile(targetStatePath, nonBoolConfResult);
                    TryDeleteFile(targetRequestPath);
                    return nonBoolConfResult;
                }

                confirmed = confEl.GetBoolean();
            }
            catch (JsonException)
            {
                // Corrupt request JSON
                var corruptResult = CreateInvalidInputResult(
                    targetStatePath,
                    "Decision request file contained corrupt or invalid JSON.",
                    requestId);
                TryWriteResultFile(targetStatePath, corruptResult);
                TryDeleteFile(targetRequestPath);
                return corruptResult;
            }

            try
            {
                // Apply decision using existing ApplyTaxDecision logic
                return ApplyTaxDecision(proposedRate, confirmed: confirmed, stateFilePath: targetStatePath, saveFilePath: saveFilePath, requestId: requestId);
            }
            finally
            {
                // Ensure request file is always deleted once processed so it cannot be executed twice
                TryDeleteFile(targetRequestPath);
            }
        }
    }

    private static PresidentialActionResult CreateInvalidInputResult(string targetStatePath, string message, string? requestId = null)
    {
        double currentRate = 0.0;
        double currentApproval = 0.0;
        double currentStability = 0.0;

        try
        {
            var snapshot = LoadState(targetStatePath);
            if (snapshot != null)
            {
                currentRate = snapshot.IncomeTaxRate;
                currentApproval = snapshot.ApprovalRating;
                currentStability = snapshot.StabilityRating;
            }
        }
        catch { /* best effort */ }

        return new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = false,
            Result = "InvalidInput",
            Message = message,
            IncomeTaxRate = currentRate,
            PreviousIncomeTaxRate = currentRate,
            ApprovalRating = currentApproval,
            PreviousApprovalRating = currentApproval,
            StabilityRating = currentStability,
            PreviousStabilityRating = currentStability
        };
    }

    /// <summary>
    /// Creates a file system watcher on the directory of the state file to automatically detect
    /// and process incoming presidential decision requests from the Godot client in real time.
    /// </summary>
    public static IDisposable CreateRequestWatcher(
        string? stateFilePath = null,
        string? saveFilePath = null,
        Action<PresidentialActionResult>? onDecisionProcessed = null)
    {
        var targetState = stateFilePath ?? ResolveDefaultStateFilePath();
        var dir = Path.GetDirectoryName(targetState);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
        {
            dir = Path.GetDirectoryName(ResolveDefaultStateFilePath());
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return new EmptyDisposable();
            }
        }

        var watcher = new FileSystemWatcher(dir)
        {
            Filter = DefaultRequestFileName,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        var syncLock = new object();

        void HandleRequest()
        {
            lock (syncLock)
            {
                System.Threading.Thread.Sleep(80);
                var result = ProcessPendingDecisionRequest(targetState, saveFilePath);
                if (result != null)
                {
                    onDecisionProcessed?.Invoke(result);
                }
            }
        }

        watcher.Created += (_, _) => HandleRequest();
        watcher.Changed += (_, _) => HandleRequest();

        // Periodic polling fallback (1s) to ensure requests are never dropped across platforms/filesystems
        var timer = new System.Threading.Timer(_ =>
        {
            var reqPath = ResolveDefaultRequestFilePath(targetState);
            if (File.Exists(reqPath))
            {
                HandleRequest();
            }
        }, null, 1000, 1000);

        // Process any request file already pending on startup
        HandleRequest();

        return new CompositeWatcher(watcher, timer);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch { /* best-effort deletion */ }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class CompositeWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly System.Threading.Timer _timer;
        private bool _disposed;

        public CompositeWatcher(FileSystemWatcher watcher, System.Threading.Timer timer)
        {
            _watcher = watcher;
            _timer = timer;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _timer.Dispose();
            }
        }
    }
}

/// <summary>
/// Result contract representing the outcome of an interactive presidential decision submitted from Godot or CLI.
/// </summary>
public sealed record PresidentialActionResult
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; init; } = string.Empty;

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

    [JsonPropertyName("country_name")]
    public string CountryName { get; init; } = string.Empty;

    [JsonPropertyName("active_project")]
    public string ActiveProject { get; init; } = string.Empty;

    [JsonPropertyName("treasury_balance")]
    public double TreasuryBalance { get; init; }

    [JsonPropertyName("ruling_party")]
    public string RulingParty { get; init; } = string.Empty;

    [JsonPropertyName("opposition_party")]
    public string OppositionParty { get; init; } = string.Empty;

    [JsonPropertyName("election_result")]
    public string ElectionResult { get; init; } = string.Empty;

    [JsonPropertyName("is_reelected")]
    public bool IsReelected { get; init; }

    [JsonPropertyName("cabinet_summary")]
    public string CabinetSummary { get; init; } = string.Empty;

    [JsonPropertyName("appointed_minister")]
    public string AppointedMinister { get; init; } = string.Empty;

    [JsonPropertyName("portfolio")]
    public string Portfolio { get; init; } = string.Empty;

    [JsonPropertyName("civil_service_quality")]
    public double CivilServiceQuality { get; init; }

    [JsonPropertyName("corruption_control")]
    public double CorruptionControl { get; init; }
}
