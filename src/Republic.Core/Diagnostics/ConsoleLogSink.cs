namespace Republic.Core.Diagnostics;

/// <summary>
/// Writes log entries to standard output.
/// </summary>
public sealed class ConsoleLogSink : ILogSink
{
    /// <inheritdoc />
    public void Write(LogEntry entry, bool structured)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Console.WriteLine(LogFormatter.Format(entry, structured));
    }
}
