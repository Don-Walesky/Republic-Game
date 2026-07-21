namespace Republic.Core.Diagnostics;

/// <summary>
/// Configures the built-in logger.
/// </summary>
public sealed class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets the minimum emitted log level.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool ConsoleEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether file logging is enabled.
    /// </summary>
    public bool FileEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether structured JSON output is enabled.
    /// </summary>
    public bool StructuredJson { get; set; }

    /// <summary>
    /// Gets or sets the repository-relative log file path.
    /// </summary>
    public string FilePath { get; set; } = "logs/republic.log";

    /// <summary>
    /// Gets or sets the threshold used by performance log messages.
    /// </summary>
    public long SlowOperationThresholdMilliseconds { get; set; } = 16;
}
