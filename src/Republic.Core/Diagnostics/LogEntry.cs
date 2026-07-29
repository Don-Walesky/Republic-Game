namespace Republic.Core.Diagnostics;

/// <summary>
/// Immutable log record emitted by <see cref="Logger"/>.
/// </summary>
/// <param name="Timestamp">UTC timestamp of the log event.</param>
/// <param name="Level">Severity level.</param>
/// <param name="Message">Human-readable message.</param>
/// <param name="Exception">Optional exception payload.</param>
public sealed record LogEntry(DateTimeOffset Timestamp, LogLevel Level, string Message, Exception? Exception = null);
