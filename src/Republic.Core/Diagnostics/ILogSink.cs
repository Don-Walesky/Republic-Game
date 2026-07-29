namespace Republic.Core.Diagnostics;

/// <summary>
/// Defines a destination for log entries.
/// </summary>
public interface ILogSink
{
    /// <summary>
    /// Writes the supplied entry.
    /// </summary>
    /// <param name="entry">Entry to write.</param>
    /// <param name="structured">When true, emit structured JSON.</param>
    void Write(LogEntry entry, bool structured);
}
