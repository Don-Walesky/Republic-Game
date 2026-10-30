namespace Republic.Core.Client;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Republic.Core.Identity.Models;

using Republic.Core.Identity.Services;

using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.PrivateSector.Services;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Government.Officials.Services;
using Republic.Core.Government.Advisers.Models;
using Republic.Core.Government.Advisers.Services;
using Republic.Core.Government;
using Republic.Core.Government.Campaign.Services;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Media.Services;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.NationalStrategy.Services;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Persistence.Services;
using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;
using Republic.Core.World.Services;

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

            if (country.IsFounded && fullState.OnboardingStep < OnboardingStep.CountryCreated)
            {
                fullState.OnboardingStep = OnboardingStep.CountryCreated;
                if (fullState.Account != null)
                {
                    fullState.Account.CurrentStep = OnboardingStep.CountryCreated;
                }
            }

            if (fullState.Account != null && string.IsNullOrEmpty(fullState.Account.OwnedCountryId))
            {
                fullState.Account.OwnedCountryId = country.Id;
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
                World = world,
                OnboardingStep = country.IsFounded ? OnboardingStep.CountryCreated : OnboardingStep.NotStarted
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
    /// Uses the authoritative CountryProfileFactory to generate rich procedural geography, resources, and institutions.
    /// Rejects duplicate founding if a sovereign nation has already been founded for this session.
    /// Enforces email verification if a registered account is present.
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

        var nameIdentityService = new PlayerIdentityService();
        var (isNameValid, nameValidationMsg) = nameIdentityService.ValidateCountryName(countryName);
        if (!isNameValid)
        {
            var invalidNameResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidName",
                Message = nameValidationMsg,
                CountryName = countryName ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, invalidNameResult);
            return invalidNameResult;
        }

        SaveEnvelope<FullGameState>? existingEnvelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                existingEnvelope = LoadAuthoritativeState(targetSavePath);
            }
            catch
            {
                // corrupted or unreadable save will be handled or recreated
            }
        }

        // Email Verification Precondition: If an account exists, email must be verified before country founding
        if (existingEnvelope?.State?.Account != null && !existingEnvelope.State.Account.PersonalIdentity.IsEmailVerified)
        {
            var unverifiedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "EmailNotVerified",
                Message = "Email verification must be completed before a player can proceed to country creation.",
                CountryName = countryName.Trim(),
                OnboardingStep = OnboardingStep.AccountRegistered.ToString(),
                IsRegistered = true,
                IsEmailVerified = false,
                PlayerDisplayName = existingEnvelope.State.Account.GetPublicIdentity()
            };
            TryWriteResultFile(targetStatePath, unverifiedResult);
            return unverifiedResult;
        }

        // Duplicate Country Creation Protection: Prevent multiple sovereign nations from being created in one session
        var existingCountry = existingEnvelope?.State?.World?.Countries?.FirstOrDefault(c =>
            c.Id == "player-country" || c.IsFounded || string.Equals(c.Name, countryName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existingCountry != null && (existingEnvelope?.State?.OnboardingStep >= OnboardingStep.CountryCreated || existingCountry.IsFounded))
        {
            var duplicateResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "DuplicateCountryCreation",
                Message = $"A sovereign nation ('{existingCountry.Name}') has already been founded for this session. Duplicate country creation is prohibited.",
                CountryName = existingCountry.Name,
                OnboardingStep = (existingEnvelope?.State?.OnboardingStep ?? OnboardingStep.CountryCreated).ToString(),
                IsRegistered = existingEnvelope?.State?.Account != null,
                IsEmailVerified = existingEnvelope?.State?.Account?.PersonalIdentity.IsEmailVerified ?? true,
                PlayerDisplayName = existingEnvelope?.State?.Account?.GetPublicIdentity() ?? string.Empty
            };
            TryWriteResultFile(targetStatePath, duplicateResult);
            return duplicateResult;
        }

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var simTime = new RepublicTime(launchEpoch, launchEpoch);
        var seed = Math.Abs(HashCode.Combine(countryName.Trim().ToUpperInvariant(), 1337));

        // Procedural generation using authoritative CountryProfileFactory with standardized initial metrics
        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry(
            officialName: countryName.Trim(),
            seed: seed,
            foundingTime: simTime,
            id: "player-country",
            startingTreasury: CountryProfileFactory.StandardStartingTreasury,
            startingPopulation: CountryProfileFactory.StandardStartingPopulation);

        country.HappinessRating = CountryProfileFactory.StandardStartingApproval;
        country.BaselineStability = CountryProfileFactory.StandardStartingStability;
        if (country.StateCapacity != null)
        {
            country.StateCapacity.GovernmentEffectiveness = CountryProfileFactory.StandardStartingStateCapacity;
            country.StateCapacity.AdministrativeCompetence = CountryProfileFactory.StandardStartingStateCapacity;
            country.StateCapacity.CivilServiceQuality = CountryProfileFactory.StandardStartingStateCapacity;
            country.StateCapacity.CorruptionControl = CountryProfileFactory.StandardStartingStateCapacity;
        }
        country.TaxPolicy.IncomeTaxRate = CountryProfileFactory.StandardStartingIncomeTaxRate;
        country.TaxPolicy.CorporateTaxRate = CountryProfileFactory.StandardStartingCorporateTaxRate;
        country.TaxPolicy.ImportTariffRate = CountryProfileFactory.StandardStartingImportTariffRate;

        // Establish inaugural democratic mandate: Ruling party and Opposition party
        country.FoundParty("Democratic Union", simTime, support: 55.0);
        country.FoundParty("National Civic Front", simTime);
        if (country.OppositionParty != null)
        {
            country.OppositionParty = country.OppositionParty with { Support = 45.0 };
        }
        country.HoldElection(simTime);

        if (existingEnvelope?.State != null)
        {
            if (existingEnvelope.State.Account != null)
            {
                existingEnvelope.State.Account.CurrentStep = OnboardingStep.CountryCreated;
                existingEnvelope.State.Account.OwnedCountryId = country.Id;
            }
            existingEnvelope.State.OnboardingStep = OnboardingStep.CountryCreated;
        }

        SaveAuthoritativeState(country, simTime, 0, targetSavePath);
        ExportState(country, simTime, 0, targetStatePath, targetSavePath, latestDecisionResult: $"Successfully founded {country.Name}. Executive desk ready.");

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Successfully founded {country.Name}. Executive desk ready.",
            CountryName = country.Name,
            CapitalCity = country.CapitalCity,
            Population = country.Profile?.Population ?? CountryProfileFactory.StandardStartingPopulation,
            PrimaryResource = country.Profile?.PrimaryResource ?? string.Empty,
            ConstitutionName = country.Profile?.ConstitutionName ?? string.Empty,
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
            IsReelected = true,
            OnboardingStep = OnboardingStep.CountryCreated.ToString(),
            IsRegistered = existingEnvelope?.State?.Account != null,
            IsEmailVerified = existingEnvelope?.State?.Account?.PersonalIdentity.IsEmailVerified ?? true,
            PlayerDisplayName = existingEnvelope?.State?.Account?.GetPublicIdentity() ?? string.Empty
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Registers a new player account with private personal identity, age verification, and verification token.
    /// </summary>
    public static PresidentialActionResult RegisterAccountDecision(
        string firstName,
        string lastName,
        string email,
        string dateOfBirthString,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        DateOnly? referenceDate = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        if (!DateOnly.TryParse(dateOfBirthString, out var dob))
        {
            var invalidDobResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidDateOfBirth",
                Message = "Please provide a valid date of birth in YYYY-MM-DD format."
            };
            TryWriteResultFile(targetStatePath, invalidDobResult);
            return invalidDobResult;
        }

        var identityService = new PlayerIdentityService();
        var (isValid, validationMessage) = identityService.ValidateRegistration(firstName, lastName, email, dob, referenceDate);
        if (!isValid)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "ValidationFailed",
                Message = validationMessage
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch
            {
                // envelope will be created fresh if corrupted or empty
            }
        }

        var cleanEmail = email.Trim().ToLowerInvariant();
        if (envelope?.State?.Account != null &&
            string.Equals(envelope.State.Account.PersonalIdentity.EmailAddress, cleanEmail, StringComparison.OrdinalIgnoreCase))
        {
            var dupResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "DuplicateAccount",
                Message = $"An account associated with '{cleanEmail}' already exists. Please verify your email or continue onboarding.",
                IsRegistered = true,
                IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
                VerificationToken = envelope.State.Account.PersonalIdentity.VerificationToken ?? string.Empty,
                VerificationEmailSubject = envelope.State.Account.PersonalIdentity.PendingVerificationEmail?.Subject ?? string.Empty,
                VerificationEmailBody = envelope.State.Account.PersonalIdentity.PendingVerificationEmail?.Body ?? string.Empty,
                VerificationEmailSender = envelope.State.Account.PersonalIdentity.PendingVerificationEmail?.Sender ?? string.Empty,
                VerificationEmailRecipient = envelope.State.Account.PersonalIdentity.PendingVerificationEmail?.RecipientEmail ?? cleanEmail,
                VerificationButtonText = envelope.State.Account.PersonalIdentity.PendingVerificationEmail?.ButtonText ?? "Verify Email"
            };
            TryWriteResultFile(targetStatePath, dupResult);
            return dupResult;
        }

        var account = identityService.RegisterAccount(firstName, lastName, email, dob, referenceDate);
        var pendingEmail = account.PersonalIdentity.PendingVerificationEmail;

        if (envelope?.State != null)
        {
            envelope.State.Account = account;
            envelope.State.OnboardingStep = OnboardingStep.AccountRegistered;
            envelope.SavedAt = DateTimeOffset.UtcNow;
            var serializer = new JsonStateSerializer();
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));
        }
        else
        {
            var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
            var fullState = new FullGameState
            {
                SaveName = "session",
                SaveTimestamp = launchEpoch,
                Account = account,
                OnboardingStep = OnboardingStep.AccountRegistered
            };
            envelope = new SaveEnvelope<FullGameState>
            {
                FormatVersion = 1,
                SavedAt = launchEpoch,
                State = fullState
            };
            var serializer = new JsonStateSerializer();
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));
        }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Account for {account.DisplayName} registered successfully. A verification code has been dispatched to {account.PersonalIdentity.EmailAddress}.",
            VerificationToken = account.PersonalIdentity.VerificationToken ?? string.Empty,
            VerificationEmailSubject = pendingEmail?.Subject ?? string.Empty,
            VerificationEmailBody = pendingEmail?.Body ?? string.Empty,
            VerificationEmailSender = pendingEmail?.Sender ?? string.Empty,
            VerificationEmailRecipient = pendingEmail?.RecipientEmail ?? account.PersonalIdentity.EmailAddress,
            VerificationButtonText = pendingEmail?.ButtonText ?? "Verify Email",
            OnboardingStep = OnboardingStep.AccountRegistered.ToString(),
            IsRegistered = true,
            IsEmailVerified = false,
            PlayerDisplayName = account.GetPublicIdentity()
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Verifies the player's registered email using the supplied token.
    /// </summary>
    public static PresidentialActionResult VerifyEmailDecision(
        string token,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch
            {
                // handle below
            }
        }

        if (envelope?.State?.Account == null)
        {
            var noAccountResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AccountNotFound",
                Message = "Cannot verify email: No registered player account exists in current save session."
            };
            TryWriteResultFile(targetStatePath, noAccountResult);
            return noAccountResult;
        }

        var identityService = new PlayerIdentityService();
        var (success, message) = identityService.VerifyEmail(envelope.State.Account, token);
        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidVerificationToken",
                Message = message,
                IsRegistered = true,
                IsEmailVerified = false,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        envelope.State.OnboardingStep = OnboardingStep.EmailVerified;
        envelope.SavedAt = DateTimeOffset.UtcNow;
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = message,
            IsRegistered = true,
            IsEmailVerified = true,
            OnboardingStep = OnboardingStep.EmailVerified.ToString(),
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Dispatches a fresh verification email token to the registered account with rate-limit protection.
    /// </summary>
    public static PresidentialActionResult ResendVerificationEmailDecision(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch { }
        }

        if (envelope?.State?.Account == null)
        {
            var noAcc = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AccountNotFound",
                Message = "Cannot resend verification email: No registered account found in current session."
            };
            TryWriteResultFile(targetStatePath, noAcc);
            return noAcc;
        }

        var identityService = new PlayerIdentityService();
        var (success, msg, email) = identityService.ResendVerificationEmail(envelope.State.Account);
        if (!success)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "ResendCooldown",
                Message = msg,
                IsRegistered = true,
                IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        envelope.SavedAt = DateTimeOffset.UtcNow;
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var okResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = msg,
            VerificationToken = envelope.State.Account.PersonalIdentity.VerificationToken ?? string.Empty,
            VerificationEmailSubject = email?.Subject ?? string.Empty,
            VerificationEmailBody = email?.Body ?? string.Empty,
            VerificationEmailSender = email?.Sender ?? string.Empty,
            VerificationEmailRecipient = email?.RecipientEmail ?? string.Empty,
            VerificationButtonText = email?.ButtonText ?? "Verify Email",
            OnboardingStep = envelope.State.OnboardingStep.ToString(),
            IsRegistered = true,
            IsEmailVerified = false,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
        };
        TryWriteResultFile(targetStatePath, okResult);
        return okResult;
    }

    /// <summary>
    /// Reads and returns the current onboarding state of the session without mutating state.
    /// </summary>
    public static PresidentialActionResult GetOnboardingStatusDecision(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch
            {
                // envelope remains null
            }
        }

        var account = envelope?.State?.Account;
        var step = envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted;
        var country = envelope?.State?.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Current onboarding stage: {step}",
            OnboardingStep = step.ToString(),
            IsRegistered = account != null,
            IsEmailVerified = account?.PersonalIdentity?.IsEmailVerified ?? false,
            PlayerDisplayName = account?.GetPublicIdentity() ?? string.Empty,
            CountryName = country?.Name ?? string.Empty,
            SelectedLeadershipArchetypes = account?.SelectedLeadershipArchetypes?.Select(a => a.ToString()).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>(),
            LeadershipDoctrineTitle = account?.LeadershipDoctrineTitle ?? string.Empty,
            SelectedDevelopmentPlans = account?.SelectedDevelopmentPlans?.Select(p => p.ToString()).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>(),
            SelectedDevelopmentPlanNames = account?.SelectedDevelopmentPlans?.Select(p => NationalDevelopmentPlanRegistry.Get(p).DisplayName).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>()
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    

    /// <summary>
    /// Confirms the player's inaugural presidential advisory council,
    /// advancing the onboarding journey to AdvisersIntroduced.
    /// </summary>
    public static PresidentialActionResult IntroduceAdvisersDecision(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.DevelopmentPlansSelected)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must select your national development plans before inducting your advisory council.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to modify another player's sovereign advisory council.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country != null)
        {
            AdviserService.SynchronizeAdvisers(country);
        }

        envelope.State.OnboardingStep = OnboardingStep.AdvisersIntroduced;
        envelope.State.Account.CurrentStep = OnboardingStep.AdvisersIntroduced;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        if (country != null)
        {
            AdviserService.SynchronizeAdvisers(country);
            var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
            ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: "Inaugural presidential advisory council inducted.");
        }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = "Inaugural presidential advisory council inducted. Principal Strategic Guide and Press Secretary active.",
            OnboardingStep = OnboardingStep.AdvisersIntroduced.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country?.Name ?? string.Empty,
            GeneralAdviser = country?.GeneralAdviser,
            PressSecretary = country?.PressSecretary,
            SpecialAdvisers = country?.SpecialAdvisers != null ? country.SpecialAdvisers.ToList().AsReadOnly() : SpecialAdviserSlot.CreateDefaultRoster().AsReadOnly(),
            SovereignGems = country?.SovereignGems ?? 500
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Unlocks a locked Special Adviser position using Sovereign Gems or designated buffs.
    /// </summary>
    public static PresidentialActionResult UnlockSpecialAdviserDecision(
        int slotNumber,
        string? unlockMethod = null,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.CountryCreated)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "A sovereign country must be established before unlocking Special Adviser positions.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to modify another player's sovereign advisers.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country == null)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "CountryNotFound",
                Message = "Sovereign nation state not found in authoritative save.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        var (unlockSuccess, unlockMsg) = AdviserService.UnlockSpecialAdviser(country, slotNumber, unlockMethod ?? "gems");
        if (!unlockSuccess)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "UnlockFailed",
                Message = unlockMsg,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                GeneralAdviser = country.GeneralAdviser,
                PressSecretary = country.PressSecretary,
                SpecialAdvisers = country.SpecialAdvisers.ToList().AsReadOnly(),
                SovereignGems = country.SovereignGems
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        envelope.SavedAt = DateTimeOffset.UtcNow;
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
        ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: unlockMsg);

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = unlockMsg,
            OnboardingStep = envelope.State.OnboardingStep.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country.Name,
            GeneralAdviser = country.GeneralAdviser,
            PressSecretary = country.PressSecretary,
            SpecialAdvisers = country.SpecialAdvisers.ToList().AsReadOnly(),
            SovereignGems = country.SovereignGems
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    

    /// <summary>
    /// Transitions player to Campaign Headquarters, initializing the R 5,000,000 purse and campaign decisions.
    /// </summary>
    public static PresidentialActionResult EnterCampaignHeadquartersDecision(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.AdvisersIntroduced)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must induct your presidential advisers before entering Campaign Headquarters.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to modify another player's campaign headquarters.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country != null)
        {
            CampaignService.InitializeCampaign(country);
        }

        envelope.State.OnboardingStep = OnboardingStep.CampaignHeadquarters;
        envelope.State.Account.CurrentStep = OnboardingStep.CampaignHeadquarters;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        if (country != null)
        {
            var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
            ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: "Campaign Headquarters established with R 5,000,000 REPU purse.");
        }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = "Campaign Headquarters established with R 5,000,000 REPU purse.",
            OnboardingStep = OnboardingStep.CampaignHeadquarters.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country?.Name ?? string.Empty,
            CampaignPurse = country?.CampaignOffice?.Purse ?? 5_000_000m,
            CampaignPurseFormatted = $"R {country?.CampaignOffice?.Purse ?? 5_000_000m:N0} REPU",
            CampaignPolling = country?.CampaignOffice?.Polling ?? 35.0m,
            AccountantGeneral = country?.CampaignOffice?.AccountantGeneral,
            AvailableCampaignDecisions = country?.CampaignOffice?.AvailableDecisions != null ? country.CampaignOffice.AvailableDecisions.ToList().AsReadOnly() : Array.Empty<CampaignDecision>(),
            CompletedCampaignDecisions = country?.CampaignOffice?.CompletedDecisions != null ? country.CampaignOffice.CompletedDecisions.ToList().AsReadOnly() : Array.Empty<CampaignDecision>(),
            GeneralAdviser = country?.GeneralAdviser,
            PressSecretary = country?.PressSecretary
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Executes a campaign decision with balanced 70:30 consequences, deducting from the campaign purse.
    /// </summary>
    public static PresidentialActionResult ExecuteCampaignDecisionAction(
        string decisionId,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.AdvisersIntroduced)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must enter Campaign Headquarters before making campaign decisions.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to execute decisions for another player's campaign.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country == null)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "CountryNotFound",
                Message = "Sovereign nation state not found in authoritative save.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        var (execSuccess, execMsg, executedDec) = CampaignService.ExecuteDecision(country, decisionId);
        if (!execSuccess)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "CampaignDecisionFailed",
                Message = execMsg,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                CampaignPurse = country.CampaignOffice?.Purse ?? 0m,
                CampaignPurseFormatted = $"R {country.CampaignOffice?.Purse ?? 0m:N0} REPU",
                CampaignPolling = country.CampaignOffice?.Polling ?? 0m,
                AccountantGeneral = country.CampaignOffice?.AccountantGeneral
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        envelope.State.OnboardingStep = OnboardingStep.CampaignHeadquarters;
        envelope.State.Account.CurrentStep = OnboardingStep.CampaignHeadquarters;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
        ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: execMsg);

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = execMsg,
            OnboardingStep = OnboardingStep.CampaignHeadquarters.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country.Name,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            CampaignPurse = country.CampaignOffice.Purse,
            CampaignPurseFormatted = $"R {country.CampaignOffice.Purse:N0} REPU",
            CampaignPolling = country.CampaignOffice.Polling,
            AccountantGeneral = country.CampaignOffice.AccountantGeneral,
            AvailableCampaignDecisions = country.CampaignOffice.AvailableDecisions.ToList().AsReadOnly(),
            CompletedCampaignDecisions = country.CampaignOffice.CompletedDecisions.ToList().AsReadOnly(),
            GeneralAdviser = country.GeneralAdviser,
            PressSecretary = country.PressSecretary
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Resolves national election night. If tally >= 50%, the player wins the presidential election!
    /// </summary>
    public static PresidentialActionResult ResolveElectionDecisionAction(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.AdvisersIntroduced)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must complete campaign activities before contesting national election night.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to contest election night for another player's sovereign nation.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country == null)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "CountryNotFound",
                Message = "Sovereign nation state not found in authoritative save.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        var (won, electionSummary, tally) = CampaignService.ResolveElectionNight(country);

        if (won)
        {
            envelope.State.OnboardingStep = OnboardingStep.InOffice;
            envelope.State.Account.CurrentStep = OnboardingStep.InOffice;
        }

        envelope.SavedAt = DateTimeOffset.UtcNow;
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
        ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: electionSummary);

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = won ? "Victory" : "Defeat",
            Message = electionSummary,
            OnboardingStep = envelope.State.OnboardingStep.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country.Name,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            CampaignPurse = country.CampaignOffice.Purse,
            CampaignPurseFormatted = $"R {country.CampaignOffice.Purse:N0} REPU",
            CampaignPolling = country.CampaignOffice.Polling,
            ElectionResolved = true,
            ElectionWon = won,
            ElectionTally = tally,
            ElectionSummary = electionSummary,
            AccountantGeneral = country.CampaignOffice.AccountantGeneral,
            AvailableCampaignDecisions = country.CampaignOffice.AvailableDecisions.ToList().AsReadOnly(),
            CompletedCampaignDecisions = country.CampaignOffice.CompletedDecisions.ToList().AsReadOnly()
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Assumes office in the Presidential Executive Office after winning election night.
    /// </summary>
    public static PresidentialActionResult AssumeOfficeDecisionAction(
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "Authoritative state or player account missing.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country == null)
        {
            var noCountryResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "CountryNotFound",
                Message = "Sovereign nation state not found in authoritative save.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, noCountryResult);
            return noCountryResult;
        }

        envelope.State.OnboardingStep = OnboardingStep.InOffice;
        envelope.State.Account.CurrentStep = OnboardingStep.InOffice;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
        ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: "Inaugural Presidential Oath of Office administered. Executive Desk operational.");

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = "Inaugural Presidential Oath of Office administered. Executive Desk operational.",
            OnboardingStep = OnboardingStep.InOffice.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country.Name,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Resumes a game session from an authoritative save file, projecting the sovereign state
    /// <summary>
    /// Validates and records the player's choice of exactly two leadership archetypes,
    /// ratifying their presidential executive doctrine and advancing onboarding to LeadershipSelected.
    /// </summary>
    public static PresidentialActionResult SelectLeadershipDecision(
        IReadOnlyList<string>? archetypes,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.CountryCreated)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must register, verify your email, and found your sovereign country before selecting leadership archetypes.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to modify another player's sovereign leadership identity.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var identityService = new PlayerIdentityService();
        var (isValid, validationMessage, parsedArchetypes, doctrineTitle) = identityService.ValidateLeadershipSelection(archetypes);
        if (!isValid)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "ValidationFailed",
                Message = validationMessage,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                IsRegistered = true,
                IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
                PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        identityService.ApplyLeadershipSelection(envelope.State.Account, parsedArchetypes, doctrineTitle);
        envelope.State.OnboardingStep = OnboardingStep.LeadershipSelected;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country != null)
        {
            country.LeadershipTypes = parsedArchetypes.ToList();
            country.LeadershipDoctrine = doctrineTitle;

            country.Opportunities.Clear();
            foreach (var arch in parsedArchetypes)
            {
                var def = LeadershipArchetypeRegistry.Get(arch);
                country.Opportunities.Add(new StrategicOpportunity
                {
                    Id = $"opp_{def.Key}",
                    Title = $"{def.DisplayName} Opportunity: {def.Advantage.Split('(')[0].Trim()}",
                    Description = $"Strategic presidential opportunity emerging from {def.DisplayName} leadership: {def.Advantage} Challenge: {def.Challenge}",
                    Source = def.DisplayName,
                    AdvantageSummary = def.Advantage,
                    IsActive = true
                });
            }

            if (country.Profile != null)
            {
                country.Profile = country.Profile with
                {
                    LeadershipTypes = country.LeadershipTypes.Select(t => t.ToString()).ToList(),
                    LeadershipDoctrine = country.LeadershipDoctrine
                };
            }

            // Apply executive doctrine stat bonuses to live country
            foreach (var arch in parsedArchetypes)
            {
                switch (arch)
                {
                    case LeadershipArchetype.Technocrat:
                        if (country.StateCapacity != null)
                        {
                            var csq = country.StateCapacity.CivilServiceQuality ?? 0.6;
                            var ge = country.StateCapacity.GovernmentEffectiveness ?? 0.6;
                            var ac = country.StateCapacity.AdministrativeCompetence ?? 0.6;
                            country.StateCapacity.CivilServiceQuality = Math.Min(1.0, csq * 1.15);
                            country.StateCapacity.GovernmentEffectiveness = Math.Min(1.0, ge * 1.15);
                            country.StateCapacity.AdministrativeCompetence = Math.Min(1.0, ac * 1.10);
                        }
                        break;
                    case LeadershipArchetype.PeoplesChampion:
                        country.HappinessRating = Math.Min(100.0, country.HappinessRating + 10.0);
                        break;
                    case LeadershipArchetype.Visionary:
                        var treasuryBonus = country.Treasury.Balance * 0.10;
                        if (treasuryBonus > 0)
                        {
                            country.Treasury.Deposit(treasuryBonus);
                        }
                        break;
                    case LeadershipArchetype.Strategist:
                        country.BaselineStability = Math.Min(100.0, country.BaselineStability + 10.0);
                        break;
                    case LeadershipArchetype.Diplomat:
                        break;
                }
            }
        }

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        if (country != null)
        {
            var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
            ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: $"Executive doctrine ratified: '{doctrineTitle}'.");
        }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Executive doctrine ratified: '{doctrineTitle}'. Leadership archetypes established.",
            OnboardingStep = OnboardingStep.LeadershipSelected.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country?.Name ?? string.Empty,
            SelectedLeadershipArchetypes = parsedArchetypes.Select(a => a.ToString()).ToList(),
            LeadershipDoctrineTitle = doctrineTitle,
            StrategicOpportunities = country?.Opportunities != null ? country.Opportunities.ToList().AsReadOnly() : Array.Empty<StrategicOpportunity>(),
            GeneralAdviser = country?.GeneralAdviser,
            PressSecretary = country?.PressSecretary,
            SpecialAdvisers = country?.SpecialAdvisers != null ? country.SpecialAdvisers.ToList().AsReadOnly() : SpecialAdviserSlot.CreateDefaultRoster().AsReadOnly(),
            SovereignGems = country?.SovereignGems ?? 500
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
    }

    /// <summary>
    /// Validates and records the player's choice of exactly four national development priorities,
    /// establishing their inaugural legislative focus and advancing onboarding to DevelopmentPlansSelected.
    /// </summary>
    public static PresidentialActionResult SelectDevelopmentPlansDecision(
        IReadOnlyList<string>? plans,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null,
        string? requestingAccountId = null,
        string? targetCountryId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var targetSavePath = saveFilePath ?? Path.ChangeExtension(targetStatePath, ".sav");

        SaveEnvelope<FullGameState>? envelope = null;
        if (File.Exists(targetSavePath))
        {
            try
            {
                envelope = LoadAuthoritativeState(targetSavePath);
            }
            catch (Exception ex)
            {
                var corruptResult = new PresidentialActionResult
                {
                    RequestId = requestId ?? string.Empty,
                    Success = false,
                    Result = "SaveCorrupted",
                    Message = $"Authoritative save file is corrupted: {ex.Message}"
                };
                TryWriteResultFile(targetStatePath, corruptResult);
                return corruptResult;
            }
        }

        if (envelope?.State?.Account == null || envelope.State.OnboardingStep < OnboardingStep.LeadershipSelected)
        {
            var invalidPrecondition = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InvalidOnboardingStep",
                Message = "You must select your leadership identity before establishing national development priorities.",
                OnboardingStep = (envelope?.State?.OnboardingStep ?? OnboardingStep.NotStarted).ToString()
            };
            TryWriteResultFile(targetStatePath, invalidPrecondition);
            return invalidPrecondition;
        }

        if (!string.IsNullOrWhiteSpace(requestingAccountId) &&
            !string.Equals(requestingAccountId, envelope.State.Account.AccountId, StringComparison.OrdinalIgnoreCase))
        {
            var unauthorizedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not have permission to modify another player's sovereign development agenda.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, unauthorizedResult);
            return unauthorizedResult;
        }

        if (!string.IsNullOrWhiteSpace(targetCountryId) &&
            !string.IsNullOrWhiteSpace(envelope.State.Account.OwnedCountryId) &&
            !string.Equals(targetCountryId, envelope.State.Account.OwnedCountryId, StringComparison.OrdinalIgnoreCase))
        {
            var forbiddenResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Unauthorized",
                Message = "You do not own this sovereign country.",
                OnboardingStep = envelope.State.OnboardingStep.ToString()
            };
            TryWriteResultFile(targetStatePath, forbiddenResult);
            return forbiddenResult;
        }

        var identityService = new PlayerIdentityService();
        var (isValid, validationMessage, parsedPriorities) = identityService.ValidateDevelopmentPlansSelection(plans);
        if (!isValid)
        {
            var failResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "ValidationFailed",
                Message = validationMessage,
                OnboardingStep = envelope.State.OnboardingStep.ToString(),
                IsRegistered = true,
                IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
                PlayerDisplayName = envelope.State.Account.GetPublicIdentity()
            };
            TryWriteResultFile(targetStatePath, failResult);
            return failResult;
        }

        identityService.ApplyDevelopmentPlansSelection(envelope.State.Account, parsedPriorities);
        envelope.State.OnboardingStep = OnboardingStep.DevelopmentPlansSelected;
        envelope.SavedAt = DateTimeOffset.UtcNow;

        var planNames = parsedPriorities.Select(p => NationalDevelopmentPlanRegistry.Get(p).DisplayName).ToList();
        var planSectors = parsedPriorities.Select(p => NationalDevelopmentPlanRegistry.Get(p).Sector).Distinct().ToList();

        var country = envelope.State.World?.Countries?.FirstOrDefault(c => c.Id == "player-country" || c.IsFounded);
        if (country != null)
        {
            country.DevelopmentPlans = parsedPriorities.ToList();
            country.StrategyProfile ??= new NationalStrategyProfile();
            country.StrategyProfile.PrioritySectors = planSectors;
            country.StrategyProfile.FocusStatement = $"Inaugural development focus: {string.Join(", ", planNames)}";

            country.StartingObjectives.Clear();
            foreach (var priority in parsedPriorities)
            {
                var def = NationalDevelopmentPlanRegistry.Get(priority);
                country.StartingObjectives.Add(new NationalObjective
                {
                    Id = $"obj_{def.Key}",
                    Title = $"Implement {def.DisplayName}",
                    Description = $"{def.Focus} Expected benefit: {def.Benefit} Challenge: {def.Challenge}",
                    Category = def.Sector,
                    TargetValue = 100.0,
                    CurrentValue = 15.0,
                    ProgressPercentage = 15.0,
                    IsCompleted = false,
                    RewardSummary = string.Join("; ", def.KeyBenefits)
                });
            }

            if (country.Profile != null)
            {
                country.Profile = country.Profile with
                {
                    LeadershipTypes = country.LeadershipTypes.Select(t => t.ToString()).ToList(),
                    LeadershipDoctrine = country.LeadershipDoctrine,
                    DevelopmentPlans = country.DevelopmentPlans.Select(p => p.ToString()).ToList()
                };
            }
        }

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(targetSavePath, serializer.Serialize(envelope));

        if (country != null)
        {
            var simTime = new RepublicTime(envelope.SavedAt, envelope.SavedAt);
            ExportState(country, simTime, envelope.State.CurrentTick, targetStatePath, targetSavePath, latestDecisionResult: $"National development agenda ratified: {string.Join(", ", planNames)}.");
        }

        var successResult = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Inaugural national development agenda ratified with 4 priorities: {string.Join(", ", planNames)}.",
            OnboardingStep = OnboardingStep.DevelopmentPlansSelected.ToString(),
            IsRegistered = true,
            IsEmailVerified = envelope.State.Account.PersonalIdentity.IsEmailVerified,
            PlayerDisplayName = envelope.State.Account.GetPublicIdentity(),
            CountryName = country?.Name ?? string.Empty,
            SelectedLeadershipArchetypes = envelope.State.Account.SelectedLeadershipArchetypes.Select(a => a.ToString()).ToList(),
            LeadershipDoctrineTitle = envelope.State.Account.LeadershipDoctrineTitle,
            SelectedDevelopmentPlans = parsedPriorities.Select(p => p.ToString()).ToList(),
            SelectedDevelopmentPlanNames = planNames,
            StartingObjectives = country?.StartingObjectives != null ? country.StartingObjectives.ToList().AsReadOnly() : Array.Empty<NationalObjective>(),
            StrategicOpportunities = country?.Opportunities != null ? country.Opportunities.ToList().AsReadOnly() : Array.Empty<StrategicOpportunity>()
        };

        TryWriteResultFile(targetStatePath, successResult);
        return successResult;
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
        else if (string.Equals(projectType, "Refinery", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(projectType, "National Refinery", StringComparison.OrdinalIgnoreCase))
        {
            outcome = PresidentialWorkspaceHandler.InitiateRefineryProject(country, simTime, confirmed: true);
        }
        else if (string.Equals(projectType, "Mine", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(projectType, "National Mine", StringComparison.OrdinalIgnoreCase))
        {
            outcome = PresidentialWorkspaceHandler.InitiateMineProject(country, simTime, confirmed: true);
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

    /// <summary>
    /// Executes an authoritative presidential trade proposal, offering commodities or currency to a target nation.
    /// Performs atomic pre-execution validation of country solvency and commodity availability.
    /// </summary>
    public static PresidentialActionResult ApplyBilateralTradeProposal(
        string targetCountryId,
        string? offeredResourceType,
        double offeredQuantity,
        string? requestedResourceType,
        double requestedQuantity,
        double currencyPayment = 0.0,
        bool isRecurring = false,
        int totalDeliveryCycles = 1,
        string? notes = null,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "propose bilateral trade");

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
                Message = "Bilateral trade proposal cancelled by executive order. No proposal submitted.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            var invalidPartnerResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = "Bilateral trade rejected: Target partner country must be specified.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, invalidPartnerResult);
            return invalidPartnerResult;
        }

        var world = envelope?.State?.World;
        Country? targetCountry = null;
        if (world?.Countries != null)
        {
            targetCountry = world.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, targetCountryId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, targetCountryId, StringComparison.OrdinalIgnoreCase));
        }

        if (targetCountry == null)
        {
            // Seed peer country into world if not already present
            var factory = new CountryProfileFactory();
            int seed = Math.Abs(targetCountryId.GetHashCode()) % 1000 + 100;
            targetCountry = factory.CreateCountry(seed, simTime);
            if (world?.Countries != null && !world.Countries.Any(c => c.Id == targetCountry.Id))
            {
                world.Countries.Add(targetCountry);
            }
        }

        var terms = new FlexibleTradeTerms
        {
            Id = $"trade_{Guid.NewGuid():N}"[..16],
            ProposerCountryId = country!.Id,
            TargetCountryId = targetCountry.Id,
            OfferedResourceType = offeredResourceType,
            OfferedQuantity = offeredQuantity,
            RequestedResourceType = requestedResourceType,
            RequestedQuantity = requestedQuantity,
            CurrencyPayment = currencyPayment,
            IsRecurring = isRecurring,
            TotalDeliveryCycles = isRecurring ? Math.Max(1, totalDeliveryCycles) : 1,
            RemainingDeliveryCycles = isRecurring ? Math.Max(1, totalDeliveryCycles) : 1,
            RevisionNumber = 1,
            LastModifiedByCountryId = country.Id,
            ProposerApproved = true,
            TargetApproved = false,
            Status = FlexibleTradeStatus.Pending,
            CreatedAt = simTime,
            LastModifiedAt = simTime,
            TermsNotes = notes ?? string.Empty,
            StatusMessage = $"Outbound trade proposal sent to {targetCountry.Name}."
        };

        var tradeService = new BilateralTradeService();
        var (success, outcomeMessage, proposal) = tradeService.CreateProposal(
            country!,
            targetCountry,
            terms,
            simTime);

        if (!success || proposal == null)
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
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, outcomeMessage);
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
    /// Submits a bilateral counter-offer to an existing trade proposal, revising terms and invalidating prior approvals.
    /// </summary>
    public static PresidentialActionResult ApplyTradeCounterProposal(
        string proposalId,
        string? newOfferedResourceType,
        double? newOfferedQuantity,
        string? newRequestedResourceType,
        double? newRequestedQuantity,
        double? newCurrencyPayment,
        string? notes = null,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "counter trade proposal");

        if (error != null)
        {
            return error;
        }

        var world = envelope?.State?.World;

        if (!confirmed)
        {
            var cancelResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Cancelled",
                Message = "Counter-proposal review cancelled. Existing proposal terms remain unchanged.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var proposal = country!.GetFlexibleTradeProposal(proposalId);
        if (proposal == null)
        {
            var notFoundResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = $"Cannot counter trade proposal: Proposal '{proposalId}' was not found in diplomatic archives.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, notFoundResult);
            return notFoundResult;
        }

        var counterTerms = proposal.CreateCounterOffer(
            country!.Id,
            newOfferedResourceType,
            newOfferedQuantity,
            newRequestedResourceType,
            newRequestedQuantity,
            newCurrencyPayment,
            simTime,
            notes);

        var tradeService = new BilateralTradeService();
        var (success, outcomeMessage, counter) = tradeService.CounterProposal(
            country!,
            proposalId,
            counterTerms,
            simTime);

        if (!success || counter == null)
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
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, outcomeMessage);
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
    /// Authoritatively accepts and executes an agreed bilateral trade proposal as a single atomic transaction.
    /// Debits and credits both countries simultaneously with complete rollback on any shortfall.
    /// </summary>
    public static PresidentialActionResult ApplyAcceptBilateralTrade(
        string proposalId,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "accept bilateral trade proposal");

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
                Message = "Trade proposal acceptance cancelled by executive order. Agreement remains unexecuted.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var proposal = country!.GetFlexibleTradeProposal(proposalId);
        if (proposal == null)
        {
            var notFoundResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = $"Cannot accept trade agreement: Proposal '{proposalId}' not found in active diplomatic archives.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, notFoundResult);
            return notFoundResult;
        }

        var partnerId = string.Equals(proposal.ProposerCountryId, country.Id, StringComparison.OrdinalIgnoreCase)
            ? proposal.TargetCountryId
            : proposal.ProposerCountryId;

        var world = envelope?.State?.World;
        Country? partnerCountry = null;
        if (world?.Countries != null)
        {
            partnerCountry = world.Countries.FirstOrDefault(c =>
                string.Equals(c.Id, partnerId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, partnerId, StringComparison.OrdinalIgnoreCase));
        }

        if (partnerCountry == null)
        {
            var factory = new CountryProfileFactory();
            int seed = Math.Abs(partnerId.GetHashCode()) % 1000 + 100;
            partnerCountry = factory.CreateCountry(seed, simTime);
            if (world?.Countries != null && !world.Countries.Any(c => c.Id == partnerCountry.Id))
            {
                world.Countries.Add(partnerCountry);
            }
        }

        var allCountries = world?.Countries ?? new List<Country> { country! };
        var tradeService = new BilateralTradeService();
        var (success, outcomeMessage, executed) = tradeService.AcceptProposal(
            country!,
            proposalId,
            simTime,
            allCountries);

        if (!success || executed == null)
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
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, outcomeMessage);
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
    /// Rejects or cancels an active or pending bilateral trade proposal.
    /// </summary>
    public static PresidentialActionResult ApplyRejectBilateralTrade(
        string proposalId,
        string? reason = null,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "reject bilateral trade proposal");

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
                Message = "Trade proposal rejection cancelled. Proposal remains in current state.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var proposal = country!.GetFlexibleTradeProposal(proposalId);
        if (proposal == null)
        {
            var notFoundResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = $"Cannot reject trade proposal: Proposal '{proposalId}' not found in active diplomatic archives.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, notFoundResult);
            return notFoundResult;
        }

        var tradeService = new BilateralTradeService();
        var (success, outcomeMessage, rejected) = tradeService.RejectProposal(
            country!,
            proposalId,
            simTime);

        if (!success || rejected == null)
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

        var world = envelope?.State?.World;
        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, outcomeMessage);
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
    /// Opens an initial diplomatic mission or relation with a target country.
    /// </summary>
    public static PresidentialActionResult ApplyOpenDiplomaticRelation(
        string targetCountryId,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "open diplomatic relation");

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
                Message = "Diplomatic mission dispatch cancelled by executive order.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var rel = country!.OpenRelation(targetCountryId, simTime, DiplomaticRelationStatus.Unrecognized);
        var outcomeMessage = $"Diplomatic mission established with {targetCountryId}. Initial relation status: Unrecognized.";

        var world = envelope?.State?.World;
        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
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
    /// Formally grants sovereign diplomatic recognition to another sovereign nation.
    /// </summary>
    public static PresidentialActionResult ApplyRecognizeDiplomaticRelation(
        string targetCountryId,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "recognize diplomatic relation");

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
                Message = "Sovereign diplomatic recognition cancelled by executive order.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var rel = country!.RecognizeRelation(targetCountryId, simTime);
        var outcomeMessage = $"Sovereign diplomatic recognition granted to {targetCountryId}. Bilateral ties elevated to Recognized.";

        var world = envelope?.State?.World;
        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        PresidentialDeskSnapshot? snapshot = null;
        try
        {
            snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
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
    /// Applies an executive decision to adopt or transition to a new national strategic direction.
    /// Incurs R 25M retooling expense, political transition friction (-5% stability, -4% approval), and enforces cooldown.
    /// </summary>
    public static PresidentialActionResult ApplyStrategicDirectionDecision(
        string strategicDirectionName,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "set national strategic direction");

        if (error != null)
        {
            return error;
        }

        if (!Enum.TryParse<NationalStrategicDirection>(strategicDirectionName, true, out var direction))
        {
            var invalidDirResult = CreateInvalidInputResult(
                targetStatePath,
                $"Invalid national strategic direction '{strategicDirectionName}'. Valid options: Industrialisation, ScienceAndTechnology, EnergyIndependence, HumanDevelopment, InfrastructureDevelopment, RegionalInfluence, BalancedGrowth.",
                requestId);
            TryWriteResultFile(targetStatePath, invalidDirResult);
            return invalidDirResult;
        }

        var strategyService = new NationalStrategyService();

        if (!confirmed)
        {
            var quoteResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AwaitingConfirmation",
                Message = $"Propose adopting {direction.GetDisplayName()}: R 25,000,000 administrative retooling expense, 14-day transition friction (-5% stability, -4% approval), and 30-day change cooldown.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, quoteResult);
            return quoteResult;
        }

        var (success, outcomeMessage, profile) = strategyService.SetStrategicDirection(country!, direction, simTime);

        var world = envelope?.State?.World;
        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = success,
            Result = success ? "Success" : "Failed",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Applies an executive directive to launch or execute a covert foreign espionage or domestic reconnaissance operation.
    /// Validates military level unlocking thresholds (L15 for basic, L20 for advanced) and prerequisites.
    /// </summary>
    public static PresidentialActionResult ApplyLaunchIntelligenceOperation(
        string operationTypeName,
        string targetCountryId,
        string? title = null,
        bool confirmed = true,
        bool resolveImmediately = false,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "launch intelligence operation");

        if (error != null)
        {
            return error;
        }

        if (!Enum.TryParse<IntelligenceOperationType>(operationTypeName, true, out var opType))
        {
            var invalidTypeResult = CreateInvalidInputResult(
                targetStatePath,
                $"Invalid intelligence operation type '{operationTypeName}'.",
                requestId);
            TryWriteResultFile(targetStatePath, invalidTypeResult);
            return invalidTypeResult;
        }

        var world = envelope?.State?.World;
        var targetCountry = world?.Countries?.FirstOrDefault(c => string.Equals(c.Id, targetCountryId, StringComparison.OrdinalIgnoreCase));

        if (targetCountry == null && opType >= IntelligenceOperationType.StrategicIntelligence)
        {
            var invalidTargetResult = CreateInvalidInputResult(
                targetStatePath,
                $"Foreign espionage target nation '{targetCountryId}' was not found in world state.",
                requestId);
            TryWriteResultFile(targetStatePath, invalidTargetResult);
            return invalidTargetResult;
        }

        var intelService = new IntelligenceProgressionService();
        var (isUnlocked, unlockReason, reqLevel) = intelService.CheckOperationPrerequisites(country!, opType);

        if (!isUnlocked)
        {
            var lockedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PrerequisitesNotMet",
                Message = $"Intelligence operation locked: {unlockReason}",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, lockedResult);
            return lockedResult;
        }

        if (!confirmed)
        {
            var quoteResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AwaitingConfirmation",
                Message = $"Ready to launch {opType} targeting {targetCountry?.Name ?? targetCountryId}. Prerequisites verified at Military Level {country!.MilitaryLevel}.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, quoteResult);
            return quoteResult;
        }

        var effectiveTarget = targetCountry ?? country!;
        var opTitle = !string.IsNullOrWhiteSpace(title) ? title : $"{opType} against {effectiveTarget.Name}";
        var (launchSuccess, outcomeMessage, op) = intelService.LaunchOperation(country!, effectiveTarget, opType, opTitle, simTime);

        if (op != null && resolveImmediately)
        {
            var (_, resMsg, _) = intelService.ResolveOperation(op, country!, effectiveTarget, simTime);
            outcomeMessage = resMsg;
        }

        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMessage);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = launchSuccess,
            Result = launchSuccess ? "Success" : "Failed",
            Message = outcomeMessage,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Applies an executive directive to execute a domestic counterintelligence sweep.
    /// Uncovers foreign espionage threats and heightens security posture.
    /// </summary>
    public static PresidentialActionResult ApplyCounterintelligenceSweep(
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "conduct counterintelligence sweep");

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
                Message = "Domestic counterintelligence sweep stood down by presidential instruction.",
                CountryName = country!.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, cancelResult);
            return cancelResult;
        }

        var intelService = new IntelligenceProgressionService();
        var outcome = intelService.ConductCounterintelligenceSweep(country!, simTime);

        var world = envelope?.State?.World;
        try
        {
            if (world != null)
            {
                SaveAuthoritativeState(world, simTime, tick, targetSavePath);
            }
            else
            {
                SaveAuthoritativeState(country!, simTime, tick, targetSavePath);
            }
        }
        catch { /* best-effort save */ }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcome.Message)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcome.Message);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = outcome.Success,
            Result = outcome.Success ? "Success" : "Failed",
            Message = outcome.Message,
            CountryName = country!.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Checks for and processes any pending presidential decision request written to disk by the Godot client.
    /// If presidential_desk_request.json exists, reads the proposed rate and confirmed flag,
    /// applies the decision via <see cref="ApplyTaxDecision(double, bool, string?, string?)"/>,
    /// ensures the updated state file is exported, deletes the request file, and returns the action result.
    /// Returns null if no request file exists.
    /// </summary>
    /// <summary>
    /// Executes a hostile military directive or attack against a foreign target nation.
    /// Deducts deployment costs, calculates combat outcome, casualties, and systemic repercussions.
    /// </summary>
    public static PresidentialActionResult ApplyMilitaryOperation(
        string targetCountryName,
        string operationType = "Airstrike",
        int troopsCommitted = 1000,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "execute military operation");

        if (error != null)
        {
            return error;
        }

        if (string.IsNullOrWhiteSpace(targetCountryName))
        {
            var noTarget = CreateInvalidInputResult(targetStatePath, "Military operation requires a valid foreign target country.", requestId);
            TryWriteResultFile(targetStatePath, noTarget);
            return noTarget;
        }

        if (string.Equals(targetCountryName, country!.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(targetCountryName, country.Id, StringComparison.OrdinalIgnoreCase))
        {
            var selfAttack = CreateInvalidInputResult(targetStatePath, "Cannot launch hostile military operations against your own sovereign territory.", requestId);
            TryWriteResultFile(targetStatePath, selfAttack);
            return selfAttack;
        }

        if (troopsCommitted <= 0)
        {
            var invalidTroops = CreateInvalidInputResult(targetStatePath, "Troop commitment must be greater than zero.", requestId);
            TryWriteResultFile(targetStatePath, invalidTroops);
            return invalidTroops;
        }

        int availablePersonnel = country.MilitaryLevel * 2500;
        if (troopsCommitted > availablePersonnel)
        {
            var exceedResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "PrerequisitesNotMet",
                Message = $"Troop commitment ({troopsCommitted:N0}) exceeds available standing armed forces personnel ({availablePersonnel:N0} at Military Level {country.MilitaryLevel}).",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, exceedResult);
            return exceedResult;
        }

        decimal operationCost = troopsCommitted * 100m;
        if (country.Treasury.Balance < (double)operationCost)
        {
            var brokeResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "InsufficientFunds",
                Message = $"Insufficient treasury reserves for military deployment. Required: {RepuTreasury.Format((double)operationCost)}, Available: {country.Treasury.FormattedBalance}.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, brokeResult);
            return brokeResult;
        }

        if (!confirmed)
        {
            var quoteResult = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AwaitingConfirmation",
                Message = $"Ready to launch {operationType} targeting {targetCountryName}. Deployment Cost: {RepuTreasury.Format((double)operationCost)}. Confirmation required.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, quoteResult);
            return quoteResult;
        }

        // Deduct treasury deployment cost
        country.Treasury.TryWithdraw((double)operationCost);

        // Calculate simulation combat outcome
        var defMinister = country.GetMinister(CabinetPortfolio.Defense);
        double ministerBonus = (defMinister != null && defMinister.IsAppointed) ? (defMinister.Competence * 0.15) : 0.0;
        double baseOdds = Math.Min(0.85, 0.40 + (country.MilitaryLevel * 0.015) + ministerBonus);

        int seed = Math.Abs(HashCode.Combine(country.Id, targetCountryName, operationType, (int)tick));
        bool success = (seed % 100) < (int)(baseOdds * 100);

        int casualties = success ? (int)(troopsCommitted * 0.04) : (int)(troopsCommitted * 0.15);
        int enemyCasualties = success ? (int)(troopsCommitted * 0.20) : (int)(troopsCommitted * 0.06);

        string outcomeMsg;
        if (success)
        {
            country.HappinessRating = Math.Clamp(country.HappinessRating + 3.0, 0.0, 100.0);
            country.GrossDomesticProduct += 1_500_000.0;
            outcomeMsg = $"Operation {operationType} against {targetCountryName} succeeded! Strategic objectives secured. Target casualties: {enemyCasualties:N0}. Friendly losses: {casualties:N0}. Cost: {RepuTreasury.Format((double)operationCost)}.";
        }
        else
        {
            country.HappinessRating = Math.Clamp(country.HappinessRating - 5.0, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability - 3.0, 0.0, 100.0);
            outcomeMsg = $"Operation {operationType} against {targetCountryName} encountered fierce defensive resistance. Tactical retreat ordered. Friendly losses: {casualties:N0}. Target casualties: {enemyCasualties:N0}. Cost: {RepuTreasury.Format((double)operationCost)}.";
        }

        // Persist authoritative save file
        if (envelope?.State != null && !string.IsNullOrWhiteSpace(targetSavePath))
        {
            envelope.State.CurrentTick = tick + 1;
            envelope.SavedAt = DateTimeOffset.UtcNow;
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var world = envelope?.State?.World;
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMsg)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath, latestDecisionResult: outcomeMsg);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = success,
            Result = success ? "Success" : "TacticalDefeat",
            Message = outcomeMsg,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Proposes a new legislative bill with 50:50 consequence balance to Parliament.
    /// </summary>
    public static PresidentialActionResult ApplyProposeBillDecision(
        string title,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "propose legislative bill");

        if (error != null)
        {
            return error;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            var noTitle = CreateInvalidInputResult(targetStatePath, "Cannot propose bill: Bill title cannot be empty.", requestId);
            TryWriteResultFile(targetStatePath, noTitle);
            return noTitle;
        }

        if (country!.PendingBill != null)
        {
            var alreadyPending = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "AlreadyActive",
                Message = $"A legislative bill ('{country.PendingBill.Title}') is already pending a parliamentary vote.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, alreadyPending);
            return alreadyPending;
        }

        var bill = country.ProposeBill(title);
        if (bill == null)
        {
            var fail = CreateInvalidInputResult(targetStatePath, "Failed to submit bill to parliament.", requestId);
            TryWriteResultFile(targetStatePath, fail);
            return fail;
        }

        if (envelope?.State != null && !string.IsNullOrWhiteSpace(targetSavePath))
        {
            envelope.State.CurrentTick = tick + 1;
            envelope.SavedAt = DateTimeOffset.UtcNow;
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var world = envelope?.State?.World;
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = true,
            Result = "Success",
            Message = $"Bill '{bill.Title}' submitted to Parliament. Category: {bill.Category}. Trade-off: {bill.PositiveConsequence} vs {bill.NegativeConsequence}.",
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Conducts a roll-call vote on a pending bill using the deterministic anti-save-scumming difficulty engine.
    /// </summary>
    public static PresidentialActionResult ApplyVoteBillDecision(
        string? title = null,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "vote legislative bill");

        if (error != null)
        {
            return error;
        }

        var targetBill = string.IsNullOrWhiteSpace(title) ? country!.PendingBill : country!.Bills.FirstOrDefault(b => b.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
        if (targetBill == null)
        {
            var noBill = CreateInvalidInputResult(targetStatePath, "No pending legislative bill found to vote upon.", requestId);
            TryWriteResultFile(targetStatePath, noBill);
            return noBill;
        }

        var voted = country!.VoteBill(targetBill, simTime);
        if (voted == null || voted.Status == CountryBillStatus.Pending)
        {
            var voteFail = new PresidentialActionResult
            {
                RequestId = requestId ?? string.Empty,
                Success = false,
                Result = "Failed",
                Message = "Parliamentary vote failed to conduct. Ensure an active ruling party is established.",
                CountryName = country.Name,
                IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
                ApprovalRating = country.HappinessRating,
                StabilityRating = country.BaselineStability,
                TreasuryBalance = country.Treasury.Balance
            };
            TryWriteResultFile(targetStatePath, voteFail);
            return voteFail;
        }

        if (envelope?.State != null && !string.IsNullOrWhiteSpace(targetSavePath))
        {
            envelope.State.CurrentTick = tick + 1;
            envelope.SavedAt = DateTimeOffset.UtcNow;
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var world = envelope?.State?.World;
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = voted.IsPassed,
            Result = voted.IsPassed ? "Passed" : "Defeated",
            Message = voted.IsPassed
                ? $"Bill '{voted.Title}' PASSED Parliament ({voted.AyesCount}/{voted.TotalVotes} Ayes). Enacted consequences applied: {voted.PositiveConsequence} | {voted.NegativeConsequence}."
                : $"Bill '{voted.Title}' DEFEATED in Parliament ({voted.AyesCount}/{voted.TotalVotes} Ayes, required {voted.RequiredMajorityPercentage}%).",
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

    /// <summary>
    /// Exercises executive presidential veto on an enacted bill.
    /// </summary>
    public static PresidentialActionResult ApplyVetoBillDecision(
        string title,
        bool confirmed = true,
        string? stateFilePath = null,
        string? saveFilePath = null,
        string? requestId = null)
    {
        var targetStatePath = stateFilePath ?? ResolveDefaultStateFilePath();
        var (envelope, country, simTime, tick, targetSavePath, error) = ResolveAuthoritativeCountry(
            targetStatePath, saveFilePath, requestId, "veto legislative bill");

        if (error != null)
        {
            return error;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            var noTitle = CreateInvalidInputResult(targetStatePath, "Veto requires a specific enacted bill title.", requestId);
            TryWriteResultFile(targetStatePath, noTitle);
            return noTitle;
        }

        bool vetoSucceeded = country!.VetoBill(title, out var msg);

        if (envelope?.State != null && !string.IsNullOrWhiteSpace(targetSavePath))
        {
            envelope.State.CurrentTick = tick + 1;
            envelope.SavedAt = DateTimeOffset.UtcNow;
            var serializer = new JsonStateSerializer();
            var updatedSavePayload = serializer.Serialize(envelope);
            AtomicFileWriter.WriteAllTextSafely(targetSavePath, updatedSavePayload);
        }

        var exportPath = targetStatePath.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(targetStatePath, ".json")
            : targetStatePath;

        try
        {
            var world = envelope?.State?.World;
            var snapshot = world != null
                ? PresidentialDeskSnapshot.Create(world, country!, simTime, tick + 1, targetSavePath)
                : PresidentialDeskSnapshot.Create(country!, simTime, tick + 1, targetSavePath);
            var snapshotJson = Serialize(snapshot);
            AtomicFileWriter.WriteAllTextSafely(exportPath, snapshotJson);
        }
        catch { }

        var result = new PresidentialActionResult
        {
            RequestId = requestId ?? string.Empty,
            Success = vetoSucceeded,
            Result = vetoSucceeded ? "VetoSustained" : "VetoOverridden",
            Message = msg,
            CountryName = country.Name,
            IncomeTaxRate = country.TaxPolicy.IncomeTaxRate,
            ApprovalRating = country.HappinessRating,
            StabilityRating = country.BaselineStability,
            TreasuryBalance = country.Treasury.Balance
        };

        TryWriteResultFile(targetStatePath, result);
        return result;
    }

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
                    if (normalizedAction is "register_account" or "register" or "account_registration")

                    {

                        string firstName = string.Empty;

                        if (root.TryGetProperty("first_name", out var fnEl) && fnEl.ValueKind == JsonValueKind.String)

                        {

                            firstName = fnEl.GetString() ?? string.Empty;

                        }



                        string lastName = string.Empty;

                        if (root.TryGetProperty("last_name", out var lnEl) && lnEl.ValueKind == JsonValueKind.String)

                        {

                            lastName = lnEl.GetString() ?? string.Empty;

                        }



                        string email = string.Empty;

                        if (root.TryGetProperty("email", out var emEl) && emEl.ValueKind == JsonValueKind.String)

                        {

                            email = emEl.GetString() ?? string.Empty;

                        }

                        else if (root.TryGetProperty("email_address", out var emaEl) && emaEl.ValueKind == JsonValueKind.String)

                        {

                            email = emaEl.GetString() ?? string.Empty;

                        }



                        string dob = string.Empty;

                        if (root.TryGetProperty("date_of_birth", out var dobEl) && dobEl.ValueKind == JsonValueKind.String)

                        {

                            dob = dobEl.GetString() ?? string.Empty;

                        }

                        else if (root.TryGetProperty("dob", out var dEl) && dEl.ValueKind == JsonValueKind.String)

                        {

                            dob = dEl.GetString() ?? string.Empty;

                        }



                        try

                        {

                            return RegisterAccountDecision(firstName, lastName, email, dob, targetStatePath, saveFilePath, requestId);

                        }

                        finally

                        {

                            TryDeleteFile(targetRequestPath);

                        }

                    }



                    if (normalizedAction is "verify_email" or "verify_token" or "email_verification")

                    {

                        string token = string.Empty;

                        if (root.TryGetProperty("token", out var tokEl) && tokEl.ValueKind == JsonValueKind.String)

                        {

                            token = tokEl.GetString() ?? string.Empty;

                        }

                        else if (root.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String)

                        {

                            token = codeEl.GetString() ?? string.Empty;

                        }

                        else if (root.TryGetProperty("verification_code", out var vcEl) && vcEl.ValueKind == JsonValueKind.String)

                        {

                            token = vcEl.GetString() ?? string.Empty;

                        }



                        try

                        {

                            return VerifyEmailDecision(token, targetStatePath, saveFilePath, requestId);

                        }

                        finally

                        {

                            TryDeleteFile(targetRequestPath);

                        }

                    }



                                        if (normalizedAction is "resend_verification_email" or "resend_email" or "resend_token")
                    {
                        try
                        {
                            return ResendVerificationEmailDecision(targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "onboarding_status" or "get_onboarding_status")

                    {

                        try

                        {

                            return GetOnboardingStatusDecision(targetStatePath, saveFilePath, requestId);

                        }

                        finally

                        {

                            TryDeleteFile(targetRequestPath);

                        }

                    }



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

                    if (normalizedAction is "select_leadership" or "select_leadership_identity" or "leadership_selection")
                    {
                        var archetypesList = new List<string>();

                        if (root.TryGetProperty("archetypes", out var archEl) && archEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in archEl.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                {
                                    archetypesList.Add(item.GetString()!.Trim());
                                }
                            }
                        }
                        else if (root.TryGetProperty("selected_archetypes", out var saEl) && saEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in saEl.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                {
                                    archetypesList.Add(item.GetString()!.Trim());
                                }
                            }
                        }

                        if (root.TryGetProperty("archetype1", out var a1El) && a1El.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(a1El.GetString()))
                        {
                            archetypesList.Add(a1El.GetString()!.Trim());
                        }

                        if (root.TryGetProperty("archetype2", out var a2El) && a2El.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(a2El.GetString()))
                        {
                            archetypesList.Add(a2El.GetString()!.Trim());
                        }

                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return SelectLeadershipDecision(archetypesList, targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "enter_campaign_hq" or "enter_campaign" or "campaign_hq")
                    {
                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return EnterCampaignHeadquartersDecision(targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "execute_campaign_decision" or "campaign_decision")
                    {
                        string decId = "";
                        if (root.TryGetProperty("decision_id", out var dEl) && dEl.ValueKind == JsonValueKind.String)
                        {
                            decId = dEl.GetString()!;
                        }
                        else if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        {
                            decId = idEl.GetString()!;
                        }

                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return ExecuteCampaignDecisionAction(decId, targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "resolve_campaign_election" or "resolve_election" or "election_night")
                    {
                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return ResolveElectionDecisionAction(targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "assume_office" or "in_office")
                    {
                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return AssumeOfficeDecisionAction(targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "introduce_advisers" or "confirm_advisers" or "advisers_introduced")
                    {
                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return IntroduceAdvisersDecision(targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "unlock_special_adviser" or "unlock_adviser")
                    {
                        int slotNum = 2;
                        if (root.TryGetProperty("slot_number", out var snEl) && snEl.TryGetInt32(out var parsedSlot))
                        {
                            slotNum = parsedSlot;
                        }
                        else if (root.TryGetProperty("slot", out var sEl) && sEl.TryGetInt32(out var parsedSlot2))
                        {
                            slotNum = parsedSlot2;
                        }

                        string method = "gems";
                        if (root.TryGetProperty("unlock_method", out var mEl) && mEl.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(mEl.GetString()))
                        {
                            method = mEl.GetString()!.Trim();
                        }
                        else if (root.TryGetProperty("method", out var mEl2) && mEl2.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(mEl2.GetString()))
                        {
                            method = mEl2.GetString()!.Trim();
                        }

                        string? reqAccId = root.TryGetProperty("account_id", out var accEl) ? accEl.GetString() : null;
                        string? reqCid = root.TryGetProperty("country_id", out var cidEl) ? cidEl.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid) && root.TryGetProperty("target_country_id", out var tcidEl))
                        {
                            reqCid = tcidEl.GetString();
                        }

                        try
                        {
                            return UnlockSpecialAdviserDecision(slotNum, method, targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId, targetCountryId: reqCid);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "select_development_plans" or "select_development_plan" or "development_plans_selection" or "development_agenda")
                    {
                        var plansList = new List<string>();

                        if (root.TryGetProperty("plans", out var plansEl) && plansEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in plansEl.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                {
                                    plansList.Add(item.GetString()!.Trim());
                                }
                            }
                        }
                        else if (root.TryGetProperty("development_plans", out var dpEl) && dpEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in dpEl.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                {
                                    plansList.Add(item.GetString()!.Trim());
                                }
                            }
                        }
                        else if (root.TryGetProperty("selected_plans", out var spEl) && spEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in spEl.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                {
                                    plansList.Add(item.GetString()!.Trim());
                                }
                            }
                        }

                        for (int i = 1; i <= 4; i++)
                        {
                            if (root.TryGetProperty($"plan{i}", out var pEl) && pEl.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pEl.GetString()))
                            {
                                plansList.Add(pEl.GetString()!.Trim());
                            }
                        }

                        string? reqAccId2 = root.TryGetProperty("account_id", out var accEl2) ? accEl2.GetString() : null;
                        string? reqCid2 = root.TryGetProperty("country_id", out var cidEl2) ? cidEl2.GetString() : null;
                        if (string.IsNullOrWhiteSpace(reqCid2) && root.TryGetProperty("target_country_id", out var tcidEl2))
                        {
                            reqCid2 = tcidEl2.GetString();
                        }

                        try
                        {
                            return SelectDevelopmentPlansDecision(plansList, targetStatePath, saveFilePath, requestId, requestingAccountId: reqAccId2, targetCountryId: reqCid2);
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

                    if (normalizedAction is "diplomatic_response" or "diplomacy_response")
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

                        bool dipConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var dipConfEl) && (dipConfEl.ValueKind == JsonValueKind.True || dipConfEl.ValueKind == JsonValueKind.False))
                        {
                            dipConfirmed = dipConfEl.GetBoolean();
                        }

                        string? reason = null;
                        if (root.TryGetProperty("reason", out var reasonEl) && reasonEl.ValueKind == JsonValueKind.String)
                        {
                            reason = reasonEl.GetString();
                        }

                        try
                        {
                            if (string.Equals(decision, "reject", StringComparison.OrdinalIgnoreCase))
                            {
                                return ApplyRejectBilateralTrade(proposalId, reason, dipConfirmed, targetStatePath, saveFilePath, requestId);
                            }
                            else if (string.Equals(decision, "counter", StringComparison.OrdinalIgnoreCase))
                            {
                                string? newOffered = root.TryGetProperty("offered_resource", out var oEl) ? oEl.GetString() : null;
                                double? newOfferedQty = root.TryGetProperty("offered_quantity", out var oqEl) && oqEl.TryGetDouble(out var oq) ? oq : null;
                                string? newRequested = root.TryGetProperty("requested_resource", out var rEl) ? rEl.GetString() : null;
                                double? newRequestedQty = root.TryGetProperty("requested_quantity", out var rqEl) && rqEl.TryGetDouble(out var rq) ? rq : null;
                                double? newCurr = root.TryGetProperty("currency_payment", out var cpEl) && cpEl.TryGetDouble(out var cp) ? cp : null;
                                string? notes = root.TryGetProperty("notes", out var nEl) ? nEl.GetString() : null;

                                return ApplyTradeCounterProposal(proposalId, newOffered, newOfferedQty, newRequested, newRequestedQty, newCurr, notes, dipConfirmed, targetStatePath, saveFilePath, requestId);
                            }
                            else
                            {
                                return ApplyAcceptBilateralTrade(proposalId, dipConfirmed, targetStatePath, saveFilePath, requestId);
                            }
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "propose_bilateral_trade" or "propose_trade" or "create_trade_proposal")
                    {
                        string targetCountryId = string.Empty;
                        if (root.TryGetProperty("target_country_id", out var tcIdEl) && tcIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = tcIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("partner_id", out var pIdEl) && pIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = pIdEl.GetString() ?? string.Empty;
                        }

                        string? offeredResource = root.TryGetProperty("offered_resource", out var oEl) ? oEl.GetString() :
                            (root.TryGetProperty("offered_resource_type", out var ortEl) ? ortEl.GetString() : null);

                        double offeredQty = 0.0;
                        if (root.TryGetProperty("offered_quantity", out var oqEl) && oqEl.TryGetDouble(out var oqVal))
                        {
                            offeredQty = oqVal;
                        }

                        string? requestedResource = root.TryGetProperty("requested_resource", out var rEl) ? rEl.GetString() :
                            (root.TryGetProperty("requested_resource_type", out var rrtEl) ? rrtEl.GetString() : null);

                        double requestedQty = 0.0;
                        if (root.TryGetProperty("requested_quantity", out var rqEl) && rqEl.TryGetDouble(out var rqVal))
                        {
                            requestedQty = rqVal;
                        }

                        double currencyPayment = 0.0;
                        if (root.TryGetProperty("currency_payment", out var cpEl) && cpEl.TryGetDouble(out var cpVal))
                        {
                            currencyPayment = cpVal;
                        }
                        else if (root.TryGetProperty("cash_payment", out var cashEl) && cashEl.TryGetDouble(out var cashVal))
                        {
                            currencyPayment = cashVal;
                        }

                        bool isRecurring = false;
                        if (root.TryGetProperty("is_recurring", out var irEl) && (irEl.ValueKind == JsonValueKind.True || irEl.ValueKind == JsonValueKind.False))
                        {
                            isRecurring = irEl.GetBoolean();
                        }

                        int totalCycles = 1;
                        if (root.TryGetProperty("total_cycles", out var tcEl) && tcEl.TryGetInt32(out var tcVal))
                        {
                            totalCycles = tcVal;
                        }
                        else if (root.TryGetProperty("cycles", out var cEl) && cEl.TryGetInt32(out var cVal))
                        {
                            totalCycles = cVal;
                        }

                        string? notes = root.TryGetProperty("notes", out var nEl) ? nEl.GetString() : null;

                        bool tradeConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var tConfEl) && (tConfEl.ValueKind == JsonValueKind.True || tConfEl.ValueKind == JsonValueKind.False))
                        {
                            tradeConfirmed = tConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyBilateralTradeProposal(
                                targetCountryId,
                                offeredResource,
                                offeredQty,
                                requestedResource,
                                requestedQty,
                                currencyPayment,
                                isRecurring,
                                totalCycles,
                                notes,
                                tradeConfirmed,
                                targetStatePath,
                                saveFilePath,
                                requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "counter_trade_proposal" or "counter_trade" or "trade_counter")
                    {
                        string proposalId = string.Empty;
                        if (root.TryGetProperty("proposal_id", out var pIdEl) && pIdEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = pIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = idEl.GetString() ?? string.Empty;
                        }

                        string? newOffered = root.TryGetProperty("offered_resource", out var oEl) ? oEl.GetString() : null;
                        double? newOfferedQty = root.TryGetProperty("offered_quantity", out var oqEl) && oqEl.TryGetDouble(out var oq) ? oq : null;
                        string? newRequested = root.TryGetProperty("requested_resource", out var rEl) ? rEl.GetString() : null;
                        double? newRequestedQty = root.TryGetProperty("requested_quantity", out var rqEl) && rqEl.TryGetDouble(out var rq) ? rq : null;
                        double? newCurr = root.TryGetProperty("currency_payment", out var cpEl) && cpEl.TryGetDouble(out var cp) ? cp : null;
                        string? notes = root.TryGetProperty("notes", out var nEl) ? nEl.GetString() : null;

                        bool counterConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var cConfEl) && (cConfEl.ValueKind == JsonValueKind.True || cConfEl.ValueKind == JsonValueKind.False))
                        {
                            counterConfirmed = cConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyTradeCounterProposal(
                                proposalId,
                                newOffered,
                                newOfferedQty,
                                newRequested,
                                newRequestedQty,
                                newCurr,
                                notes,
                                counterConfirmed,
                                targetStatePath,
                                saveFilePath,
                                requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "accept_bilateral_trade" or "accept_trade")
                    {
                        string proposalId = string.Empty;
                        if (root.TryGetProperty("proposal_id", out var pIdEl) && pIdEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = pIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = idEl.GetString() ?? string.Empty;
                        }

                        bool acceptConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var aConfEl) && (aConfEl.ValueKind == JsonValueKind.True || aConfEl.ValueKind == JsonValueKind.False))
                        {
                            acceptConfirmed = aConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyAcceptBilateralTrade(proposalId, acceptConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "reject_bilateral_trade" or "reject_trade")
                    {
                        string proposalId = string.Empty;
                        if (root.TryGetProperty("proposal_id", out var pIdEl) && pIdEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = pIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        {
                            proposalId = idEl.GetString() ?? string.Empty;
                        }

                        string? reason = root.TryGetProperty("reason", out var rEl) ? rEl.GetString() : null;
                        bool rejectConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var rConfEl) && (rConfEl.ValueKind == JsonValueKind.True || rConfEl.ValueKind == JsonValueKind.False))
                        {
                            rejectConfirmed = rConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyRejectBilateralTrade(proposalId, reason, rejectConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "open_diplomatic_relation" or "open_relation")
                    {
                        string targetCountryId = string.Empty;
                        if (root.TryGetProperty("target_country_id", out var tcIdEl) && tcIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = tcIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("country_id", out var cIdEl) && cIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = cIdEl.GetString() ?? string.Empty;
                        }

                        bool openConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var oConfEl) && (oConfEl.ValueKind == JsonValueKind.True || oConfEl.ValueKind == JsonValueKind.False))
                        {
                            openConfirmed = oConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyOpenDiplomaticRelation(targetCountryId, openConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "recognize_diplomatic_relation" or "recognize_relation")
                    {
                        string targetCountryId = string.Empty;
                        if (root.TryGetProperty("target_country_id", out var tcIdEl) && tcIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = tcIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("country_id", out var cIdEl) && cIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = cIdEl.GetString() ?? string.Empty;
                        }

                        bool recConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var rConfEl) && (rConfEl.ValueKind == JsonValueKind.True || rConfEl.ValueKind == JsonValueKind.False))
                        {
                            recConfirmed = rConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyRecognizeDiplomaticRelation(targetCountryId, recConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "set_strategic_direction" or "change_strategic_direction" or "set_strategy" or "change_strategy")
                    {
                        string direction = string.Empty;
                        if (root.TryGetProperty("direction", out var dirEl) && dirEl.ValueKind == JsonValueKind.String)
                        {
                            direction = dirEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("strategic_direction", out var sDirEl) && sDirEl.ValueKind == JsonValueKind.String)
                        {
                            direction = sDirEl.GetString() ?? string.Empty;
                        }

                        bool dirConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var dConfEl) && (dConfEl.ValueKind == JsonValueKind.True || dConfEl.ValueKind == JsonValueKind.False))
                        {
                            dirConfirmed = dConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyStrategicDirectionDecision(direction, dirConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "launch_intelligence_operation" or "launch_intelligence_op" or "spy_operation" or "launch_spy_op")
                    {
                        string opType = string.Empty;
                        if (root.TryGetProperty("operation_type", out var otEl) && otEl.ValueKind == JsonValueKind.String)
                        {
                            opType = otEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("type", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            opType = tEl.GetString() ?? string.Empty;
                        }

                        string targetCountryId = string.Empty;
                        if (root.TryGetProperty("target_country_id", out var tcIdEl) && tcIdEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = tcIdEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("target", out var tgEl) && tgEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryId = tgEl.GetString() ?? string.Empty;
                        }

                        string? title = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;

                        bool intelConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var iConfEl) && (iConfEl.ValueKind == JsonValueKind.True || iConfEl.ValueKind == JsonValueKind.False))
                        {
                            intelConfirmed = iConfEl.GetBoolean();
                        }

                        bool resolveImmediately = false;
                        if (root.TryGetProperty("resolve_immediately", out var resImmEl) && (resImmEl.ValueKind == JsonValueKind.True || resImmEl.ValueKind == JsonValueKind.False))
                        {
                            resolveImmediately = resImmEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyLaunchIntelligenceOperation(opType, targetCountryId, title, intelConfirmed, resolveImmediately, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "counterintelligence_sweep" or "conduct_counterintelligence_sweep" or "security_sweep")
                    {
                        bool sweepConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var sConfEl) && (sConfEl.ValueKind == JsonValueKind.True || sConfEl.ValueKind == JsonValueKind.False))
                        {
                            sweepConfirmed = sConfEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyCounterintelligenceSweep(sweepConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "military_attack" or "military_directive" or "military_operation" or "attack")
                    {
                        string targetCountryName = string.Empty;
                        if (root.TryGetProperty("target_country", out var tcEl) && tcEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryName = tcEl.GetString() ?? string.Empty;
                        }
                        else if (root.TryGetProperty("target", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            targetCountryName = tEl.GetString() ?? string.Empty;
                        }

                        string opType = "Airstrike";
                        if (root.TryGetProperty("operation_type", out var otEl) && otEl.ValueKind == JsonValueKind.String)
                        {
                            opType = otEl.GetString() ?? "Airstrike";
                        }
                        else if (root.TryGetProperty("op_type", out var opEl) && opEl.ValueKind == JsonValueKind.String)
                        {
                            opType = opEl.GetString() ?? "Airstrike";
                        }

                        int troops = 1000;
                        if (root.TryGetProperty("troops_committed", out var trEl) && trEl.ValueKind == JsonValueKind.Number)
                        {
                            troops = trEl.GetInt32();
                        }
                        else if (root.TryGetProperty("troops", out var tNumEl) && tNumEl.ValueKind == JsonValueKind.Number)
                        {
                            troops = tNumEl.GetInt32();
                        }

                        bool milConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var mcEl) && (mcEl.ValueKind == JsonValueKind.True || mcEl.ValueKind == JsonValueKind.False))
                        {
                            milConfirmed = mcEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyMilitaryOperation(targetCountryName, opType, troops, milConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "propose_bill" or "legislature_propose" or "bill_propose")
                    {
                        string title = string.Empty;
                        if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                        {
                            title = titleEl.GetString() ?? string.Empty;
                        }

                        bool bConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var bcEl) && (bcEl.ValueKind == JsonValueKind.True || bcEl.ValueKind == JsonValueKind.False))
                        {
                            bConfirmed = bcEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyProposeBillDecision(title, bConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "vote_bill" or "legislature_vote" or "bill_vote")
                    {
                        string? title = null;
                        if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                        {
                            title = titleEl.GetString();
                        }

                        bool bConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var bcEl) && (bcEl.ValueKind == JsonValueKind.True || bcEl.ValueKind == JsonValueKind.False))
                        {
                            bConfirmed = bcEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyVoteBillDecision(title, bConfirmed, targetStatePath, saveFilePath, requestId);
                        }
                        finally
                        {
                            TryDeleteFile(targetRequestPath);
                        }
                    }

                    if (normalizedAction is "veto_bill" or "legislature_veto" or "bill_veto")
                    {
                        string title = string.Empty;
                        if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                        {
                            title = titleEl.GetString() ?? string.Empty;
                        }

                        bool bConfirmed = true;
                        if (root.TryGetProperty("confirmed", out var bcEl) && (bcEl.ValueKind == JsonValueKind.True || bcEl.ValueKind == JsonValueKind.False))
                        {
                            bConfirmed = bcEl.GetBoolean();
                        }

                        try
                        {
                            return ApplyVetoBillDecision(title, bConfirmed, targetStatePath, saveFilePath, requestId);
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

    [JsonPropertyName("onboarding_step")]
    public string OnboardingStep { get; init; } = string.Empty;

    [JsonPropertyName("verification_token")]
    public string VerificationToken { get; init; } = string.Empty;

    [JsonPropertyName("is_registered")]
    public bool IsRegistered { get; init; }

    [JsonPropertyName("is_email_verified")]
    public bool IsEmailVerified { get; init; }

    [JsonPropertyName("player_display_name")]
    public string PlayerDisplayName { get; init; } = string.Empty;

    [JsonPropertyName("verification_email_subject")]
    public string VerificationEmailSubject { get; init; } = string.Empty;

    [JsonPropertyName("verification_email_body")]
    public string VerificationEmailBody { get; init; } = string.Empty;

    [JsonPropertyName("verification_email_sender")]
    public string VerificationEmailSender { get; init; } = string.Empty;

    [JsonPropertyName("verification_email_recipient")]
    public string VerificationEmailRecipient { get; init; } = string.Empty;

    [JsonPropertyName("verification_button_text")]
    public string VerificationButtonText { get; init; } = "Verify Email";

    [JsonPropertyName("capital_city")]
    public string CapitalCity { get; init; } = string.Empty;

    [JsonPropertyName("population")]
    public long Population { get; init; }

    [JsonPropertyName("primary_resource")]
    public string PrimaryResource { get; init; } = string.Empty;

    [JsonPropertyName("constitution_name")]
    public string ConstitutionName { get; init; } = string.Empty;

    [JsonPropertyName("selected_leadership_archetypes")]
    public IReadOnlyList<string> SelectedLeadershipArchetypes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("leadership_doctrine_title")]
    public string LeadershipDoctrineTitle { get; init; } = string.Empty;

    [JsonPropertyName("selected_development_plans")]
    public IReadOnlyList<string> SelectedDevelopmentPlans { get; init; } = Array.Empty<string>();

    [JsonPropertyName("selected_development_plan_names")]
    public IReadOnlyList<string> SelectedDevelopmentPlanNames { get; init; } = Array.Empty<string>();

    [JsonPropertyName("starting_objectives")]
    public IReadOnlyList<NationalObjective> StartingObjectives { get; init; } = Array.Empty<NationalObjective>();

    [JsonPropertyName("strategic_opportunities")]
    public IReadOnlyList<StrategicOpportunity> StrategicOpportunities { get; init; } = Array.Empty<StrategicOpportunity>();

    [JsonPropertyName("general_adviser")]
    public GeneralAdviser? GeneralAdviser { get; init; }

    [JsonPropertyName("press_secretary")]
    public PresidentialPressSecretary? PressSecretary { get; init; }

    [JsonPropertyName("special_advisers")]
    public IReadOnlyList<SpecialAdviserSlot> SpecialAdvisers { get; init; } = Array.Empty<SpecialAdviserSlot>();

    [JsonPropertyName("sovereign_gems")]
    public int SovereignGems { get; init; }

    [JsonPropertyName("campaign_purse")]
    public decimal CampaignPurse { get; init; } = 5_000_000m;

    [JsonPropertyName("campaign_purse_formatted")]
    public string CampaignPurseFormatted { get; init; } = "R 5,000,000 REPU";

    [JsonPropertyName("campaign_polling")]
    public decimal CampaignPolling { get; init; } = 35.0m;

    [JsonPropertyName("campaign_accountant_general")]
    public CampaignAccountantGeneral? AccountantGeneral { get; init; }

    [JsonPropertyName("available_campaign_decisions")]
    public IReadOnlyList<CampaignDecision> AvailableCampaignDecisions { get; init; } = Array.Empty<CampaignDecision>();

    [JsonPropertyName("completed_campaign_decisions")]
    public IReadOnlyList<CampaignDecision> CompletedCampaignDecisions { get; init; } = Array.Empty<CampaignDecision>();

    [JsonPropertyName("election_resolved")]
    public bool ElectionResolved { get; init; }

    [JsonPropertyName("election_won")]
    public bool ElectionWon { get; init; }

    [JsonPropertyName("election_tally")]
    public decimal ElectionTally { get; init; }

    [JsonPropertyName("election_summary")]
    public string ElectionSummary { get; init; } = string.Empty;
}
