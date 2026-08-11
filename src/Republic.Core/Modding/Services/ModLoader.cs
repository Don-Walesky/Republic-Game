namespace Republic.Core.Modding.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Republic.Core.Diagnostics;
using Republic.Core.Modding.Models;
using Republic.Core.Scenarios.Models;

/// <summary>
/// Service scanning, loading, and managing custom mods and scenario packages from the local mods directory.
/// </summary>
public sealed class ModLoader
{
    private readonly string _modsDirectory;
    private readonly CustomScenarioImporter _importer;
    private readonly ILogger? _logger;
    private readonly List<ModManifest> _loadedMods = new();
    private readonly List<ScenarioPreset> _customScenarios = new();

    public ModLoader(string modsDirectory, CustomScenarioImporter importer, ILogger? logger = null)
    {
        _modsDirectory = modsDirectory ?? throw new ArgumentNullException(nameof(modsDirectory));
        _importer = importer ?? throw new ArgumentNullException(nameof(importer));
        _logger = logger;
    }

    public IReadOnlyList<ModManifest> LoadedMods => _loadedMods.AsReadOnly();
    public IReadOnlyList<ScenarioPreset> CustomScenarios => _customScenarios.AsReadOnly();

    public void RegisterCustomScenario(ScenarioPreset scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (!_customScenarios.Any(s => s.Id == scenario.Id))
        {
            _customScenarios.Add(scenario);
            _logger?.LogInfo($"[ModLoader] Custom scenario registered: '{scenario.Name}' ({scenario.Id})");
        }
    }

    public int ScanAndLoadMods()
    {
        if (!Directory.Exists(_modsDirectory))
        {
            Directory.CreateDirectory(_modsDirectory);
            return 0;
        }

        var loadedCount = 0;
        var scenarioFiles = Directory.GetFiles(_modsDirectory, "*.json", SearchOption.AllDirectories);

        foreach (var file in scenarioFiles)
        {
            try
            {
                string json = File.ReadAllText(file);
                var scenario = _importer.ImportScenarioFromJson(json);
                RegisterCustomScenario(scenario);
                loadedCount++;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[ModLoader] Failed to load mod file '{file}': {ex.Message}");
            }
        }

        return loadedCount;
    }
}
