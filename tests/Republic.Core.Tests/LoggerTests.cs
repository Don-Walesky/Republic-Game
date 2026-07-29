namespace Republic.Core.Tests;

using Republic.Core.Diagnostics;

public sealed class LoggerTests
{
    [Fact]
    public void LogWarning_WritesToFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"republic-log-{Guid.NewGuid():N}.log");
        var logger = new Logger(
            new LoggingConfiguration { MinimumLevel = LogLevel.Debug, FileEnabled = true, ConsoleEnabled = false, FilePath = path },
            new ILogSink[] { new FileLogSink(path) });

        logger.LogWarning("hello world");

        var contents = File.ReadAllText(path);
        Assert.Contains("hello world", contents);
        Assert.Contains("Warning", contents);
    }

    [Fact]
    public void MinimumLevel_FiltersBelowThreshold()
    {
        var sink = new InMemorySink();
        var logger = new Logger(
            new LoggingConfiguration { MinimumLevel = LogLevel.Warning, ConsoleEnabled = false, FileEnabled = false },
            new ILogSink[] { sink });

        logger.LogInfo("ignored");
        logger.LogError("captured");

        Assert.Single(sink.Entries);
        Assert.Equal("captured", sink.Entries[0].Message);
    }

    [Fact]
    public void LogPerformance_UsesWarningForSlowOperation()
    {
        var sink = new InMemorySink();
        var logger = new Logger(
            new LoggingConfiguration { MinimumLevel = LogLevel.Debug, ConsoleEnabled = false, FileEnabled = false, SlowOperationThresholdMilliseconds = 10 },
            new ILogSink[] { sink });

        logger.LogPerformance("Tick", 11);

        Assert.Single(sink.Entries);
        Assert.Equal(LogLevel.Warning, sink.Entries[0].Level);
    }

    private sealed class InMemorySink : ILogSink
    {
        public List<LogEntry> Entries { get; } = new();

        public void Write(LogEntry entry, bool structured) => Entries.Add(entry);
    }
}
