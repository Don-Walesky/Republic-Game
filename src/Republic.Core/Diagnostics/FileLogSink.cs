namespace Republic.Core.Diagnostics;

/// <summary>
/// Writes log entries to a file on disk.
/// </summary>
public sealed class FileLogSink : ILogSink
{
    private readonly string _path;
    private static readonly object _syncRoot = new();

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
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using var stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(LogFormatter.Format(entry, structured));
            }
            catch (IOException)
            {
                // Fallback: Concurrent process holding file lock should not crash simulation
            }
            catch (UnauthorizedAccessException)
            {
                // Fallback: Permission restriction should not crash simulation
            }
        }
    }
}
