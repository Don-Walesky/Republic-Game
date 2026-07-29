namespace Republic.Core.Tests;

using Republic.Core.Diagnostics;

internal sealed class TestLogger : ILogger
{
    public List<LogEntry> Entries { get; } = new();

    public LogLevel MinimumLevel => LogLevel.Debug;

    public void SetMinimumLevel(LogLevel level)
    {
    }

    public void LogDebug(string message) => Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Debug, message));

    public void LogInfo(string message) => Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Info, message));

    public void LogWarning(string message) => Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Warning, message));

    public void LogError(string message, Exception? exception = null) => Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Error, message, exception));

    public void LogFatal(string message, Exception? exception = null) => Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Fatal, message, exception));

    public void LogPerformance(string operation, long elapsedMilliseconds, long? thresholdMilliseconds = null) =>
        Entries.Add(new LogEntry(DateTimeOffset.UtcNow, LogLevel.Debug, $"{operation}:{elapsedMilliseconds}:{thresholdMilliseconds}"));
}
