namespace Republic.Core.Modding.Services;

using System;
using System.Text.Json;
using Republic.Core.Scenarios.Models;

/// <summary>
/// Service parsing and validating custom scenario JSON strings into runnable ScenarioPreset instances.
/// </summary>
public sealed class CustomScenarioImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ScenarioPreset ImportScenarioFromJson(string jsonContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);

        try
        {
            var preset = JsonSerializer.Deserialize<ScenarioPreset>(jsonContent, Options);
            if (preset == null || string.IsNullOrWhiteSpace(preset.Id) || string.IsNullOrWhiteSpace(preset.Name))
            {
                throw new InvalidOperationException("Custom scenario JSON is missing required fields ('id', 'name').");
            }
            return preset;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse custom scenario JSON: {ex.Message}", ex);
        }
    }

    public string ExportScenarioToJson(ScenarioPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        return JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
    }
}
