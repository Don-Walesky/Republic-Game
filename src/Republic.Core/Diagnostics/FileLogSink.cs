namespace Republic.Core.Diagnostics;

/// <summary>
/// Writes log entries to a file on disk.
/// </summary>
public sealed class FileLogSink : ILogSink
{
    private readonly string _path;
    private readonly object _syncRoot = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLogSink"/> class.
    /// </summary>
    /// <param name="path">Target log file path.</param>
    public FileLogSink(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    /// <inheritdoc />
    public void Write(LogEntry entry, bool structured)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_syncRoot)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.AppendAllText(_path, LogFormatter.Format(entry, structured) + Environment.NewLine);
        }
    }
}
