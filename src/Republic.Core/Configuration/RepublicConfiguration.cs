namespace Republic.Core.Configuration;

using Republic.Core.Diagnostics;
using Republic.Core.Time;

/// <summary>
/// Root configuration object for the Wave 0 simulation foundation.
/// </summary>
public sealed class RepublicConfiguration
{
    /// <summary>
    /// Gets or sets engine execution settings.
    /// </summary>
    public EngineConfiguration Engine { get; set; } = new();

    /// <summary>
    /// Gets or sets logging settings.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new();

    /// <summary>
    /// Gets or sets time system settings.
    /// </summary>
    public TimeSystemConfiguration Time { get; set; } = new();

    /// <summary>
    /// Gets or sets persistence settings.
    /// </summary>
    public PersistenceConfiguration Persistence { get; set; } = new();
}

/// <summary>
/// Controls bootstrap and engine execution defaults.
/// </summary>
public sealed class EngineConfiguration
{
    /// <summary>
    /// Gets or sets the number of simulation frames the bootstrap run executes.
    /// </summary>
    public int StartupFrameCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the default real delta time in milliseconds for the bootstrap run.
    /// </summary>
    public int FrameDeltaMilliseconds { get; set; } = 100;

    /// <summary>
    /// Gets or sets the default world name.
    /// </summary>
    public string WorldName { get; set; } = "Republic Sandbox";
}

/// <summary>
/// Controls persistence behavior for serialized state.
/// </summary>
public sealed class PersistenceConfiguration
{
    /// <summary>
    /// Gets or sets the repository-relative save directory.
    /// </summary>
    public string SaveDirectory { get; set; } = "saves";

    /// <summary>
    /// Gets or sets the current serialized format version.
    /// </summary>
    public int FormatVersion { get; set; } = 1;
}
