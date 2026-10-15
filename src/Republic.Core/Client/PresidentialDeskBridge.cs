namespace Republic.Core.Client;

using System;
using System.IO;
using System.Text.Json;
using Republic.Core.Time;
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
}
