namespace Republic.Core.Configuration;

/// <summary>
/// Loads and persists the repository's typed configuration.
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Gets the current in-memory configuration.
    /// </summary>
    RepublicConfiguration Current { get; }

    /// <summary>
    /// Loads configuration from the supplied path.
    /// </summary>
    /// <param name="path">Absolute or relative JSON file path.</param>
    /// <returns>The loaded configuration.</returns>
    RepublicConfiguration Load(string path);

    /// <summary>
    /// Saves configuration to the supplied path.
    /// </summary>
    /// <param name="path">Absolute or relative JSON file path.</param>
    /// <param name="configuration">Configuration to persist.</param>
    void Save(string path, RepublicConfiguration configuration);

    /// <summary>
    /// Reloads the last successfully loaded configuration file.
    /// </summary>
    RepublicConfiguration Reload();
}
