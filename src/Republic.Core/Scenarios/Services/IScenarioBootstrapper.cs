namespace Republic.Core.Scenarios.Services;

using Republic.Core.Scenarios.Models;

/// <summary>
/// Service interface providing available game presets and bootstrapping scenario world states.
/// </summary>
public interface IScenarioBootstrapper
{
    IReadOnlyList<ScenarioPreset> GetAvailablePresets();
    Task<ScenarioPreset> BootstrapScenarioAsync(string presetId, CancellationToken cancellationToken = default);
}
