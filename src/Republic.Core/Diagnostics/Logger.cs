namespace Republic.Core.Diagnostics;

using System.Text.Json;

/// <summary>
/// Minimal multi-sink logger for the simulation core.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Gets the current minimum level.
    /// </summary>
    LogLevel MinimumLevel { get; }

    /// <summary>
    /// Updates the current minimum level.
    /// </summary>
    /// <param name="level">New minimum level.</param>
    void SetMinimumLevel(LogLevel level);

    /// <summary>
    /// Writes a debug message.
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Writes an informational message.
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Writes an error message.
    /// </summary>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Writes a fatal message.
    /// </summary>
    void LogFatal(string message, Exception? exception = null);

    /// <summary>
    /// Logs an operation's duration.
    /// </summary>
    void LogPerformance(string operation, long elapsedMilliseconds, long? thresholdMilliseconds = null);
}

/// <summary>
/// Default <see cref="ILogger"/> implementation.
/// </summary>
public sealed class Logger : ILogger
{
    private readonly IReadOnlyCollection<ILogSink> _sinks;
    private readonly LoggingConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="configuration">Logger configuration.</param>
    /// <param name="sinks">Destinations to write to.</param>
    public Logger(LoggingConfiguration configuration, IEnumerable<ILogSink> sinks)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _sinks = sinks?.ToArray() ?? throw new ArgumentNullException(nameof(sinks));
    }

    /// <inheritdoc />
    public LogLevel MinimumLevel => _configuration.MinimumLevel;

    /// <inheritdoc />
    public void SetMinimumLevel(LogLevel level) => _configuration.MinimumLevel = level;

    /// <inheritdoc />
    public void LogDebug(string message) => Log(LogLevel.Debug, message);

    /// <inheritdoc />
    public void LogInfo(string message) => Log(LogLevel.Info, message);

    /// <inheritdoc />
    public void LogWarning(string message) => Log(LogLevel.Warning, message);

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);

    /// <inheritdoc />
    public void LogFatal(string message, Exception? exception = null) => Log(LogLevel.Fatal, message, exception);

    /// <inheritdoc />
    public void LogPerformance(string operation, long elapsedMilliseconds, long? thresholdMilliseconds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        var threshold = thresholdMilliseconds ?? _configuration.SlowOperationThresholdMilliseconds;
        var message = $"{operation} completed in {elapsedMilliseconds}ms (threshold {threshold}ms).";

        if (elapsedMilliseconds > threshold)
        {
            LogWarning(message);
            return;
        }

        LogDebug(message);
    }

    private void Log(LogLevel level, string message, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (level < _configuration.MinimumLevel)
        {
            return;
        }

        var entry = new LogEntry(DateTimeOffset.UtcNow, level, message, exception);

        foreach (var sink in _sinks)
        {
            sink.Write(entry, _configuration.StructuredJson);
        }
    }
}

internal static class LogFormatter
{
    public static string Format(LogEntry entry, bool structured)
    {
        return structured
            ? JsonSerializer.Serialize(new
            {
                timestamp = entry.Timestamp,
                level = entry.Level.ToString(),
                message = entry.Message,
                exception = entry.Exception?.ToString(),
            })
            : $"[{entry.Timestamp:O}] [{entry.Level}] {entry.Message}{FormatException(entry.Exception)}";
    }

    private static string FormatException(Exception? exception) => exception is null ? string.Empty : $"{Environment.NewLine}{exception}";
}
