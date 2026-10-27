namespace Republic.Core.Persistence;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides atomic-where-supported, safe file write operations that protect saves and state files
/// from interrupted writes, power loss, and partial corruption.
/// Serializes to a sibling temporary file in the same directory, flushes to disk, closes the stream,
/// and safely replaces the destination file using the safest platform-supported move/replace strategy.
/// </summary>
/// <remarks>
/// <para>
/// <b>Safety Strategy:</b>
/// 1. Content is written to a unique sibling temporary file in the same directory (guaranteeing the same filesystem volume).
/// 2. FileStream is opened with <see cref="FileOptions.WriteThrough"/> and explicitly flushed to physical disk via <c>Flush(flushToDisk: true)</c>.
/// 3. The temporary file stream is completely closed and disposed before moving.
/// 4. The destination file is replaced using <see cref="File.Move(string, string, bool)"/> with <c>overwrite: true</c>,
/// which maps to atomic <c>MoveFileExW</c> (<c>MOVEFILE_REPLACE_EXISTING</c>) on Windows (NTFS/ReFS) and atomic <c>rename(2)</c> on POSIX filesystems.
/// 5. If an exception occurs at any point during writing or flushing, the temporary file is deleted and the existing destination file remains completely intact.
/// </para>
/// <para>
/// <b>Platform Limitations:</b>
/// While this implementation provides robust protection against process crashes and sudden power loss on local journaling filesystems (NTFS, ext4, APFS, XFS),
/// no userland file-replacement mechanism is guaranteed to be 100% crash-proof across every storage topology. Specifically:
/// - <b>Network filesystems (NFS, SMB/CIFS):</b> Remote file servers may not provide atomic directory rename semantics or immediate sync cache flushing.
/// - <b>Legacy filesystems (FAT32, exFAT):</b> FAT filesystems do not have journaling; catastrophic system power loss during directory entry update can leave directory state inconsistent.
/// - <b>Active file locks:</b> On Windows, if another process holds an open file handle without <see cref="FileShare.Delete"/> on the destination file, replacing the destination will throw an <see cref="IOException"/>. In this event, the replacement is aborted, the temporary file is cleaned up, and the existing valid save file remains uncorrupted.
/// </para>
/// </remarks>
public static class AtomicFileWriter
{
    /// <summary>
    /// Writes text safely to the destination file.
    /// Writes to a temporary file in the same directory, flushes to disk, and replaces the destination.
    /// Cleans up the temporary file if an error occurs, preserving any existing destination file untouched.
    /// </summary>
    /// <param name="destinationPath">Target file path to write to.</param>
    /// <param name="content">Text content to safely write.</param>
    /// <param name="encoding">Text encoding to use (defaults to UTF-8 without BOM).</param>
    public static void WriteAllTextSafely(string destinationPath, string content, Encoding? encoding = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(content);

        var fullDestPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = Path.GetFileName(fullDestPath);
        var tempPath = Path.Combine(directory!, $"{fileName}.{Guid.NewGuid():N}.tmp");
        var enc = encoding ?? Encoding.UTF8;

        try
        {
            // 1. Write completely to a temporary sibling file in the same directory (same filesystem volume)
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, enc))
            {
                writer.Write(content);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            // 2. Safely replace destination file once temporary file is fully closed and flushed
            bool replaced = false;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    if (File.Exists(fullDestPath))
                    {
                        File.Move(tempPath, fullDestPath, overwrite: true);
                    }
                    else
                    {
                        File.Move(tempPath, fullDestPath);
                    }
                    replaced = true;
                    break;
                }
                catch (Exception ex) when (attempt < 9 && (ex is IOException or UnauthorizedAccessException))
                {
                    Thread.Sleep(50);
                }
            }

            if (!replaced)
            {
                File.Copy(tempPath, fullDestPath, overwrite: true);
                try
                {
                    File.Delete(tempPath);
                }
                catch { /* best-effort cleanup */ }
            }
        }
        catch
        {
            // 3. Clean up temporary file on failure without touching existing destination file
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch { /* best-effort cleanup */ }

            throw;
        }
    }

    /// <summary>
    /// Asynchronously writes text safely to the destination file.
    /// Writes to a temporary file in the same directory, flushes to disk, and replaces the destination.
    /// Cleans up the temporary file if an error occurs, preserving any existing destination file untouched.
    /// </summary>
    /// <param name="destinationPath">Target file path to write to.</param>
    /// <param name="content">Text content to safely write.</param>
    /// <param name="encoding">Text encoding to use (defaults to UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task WriteAllTextSafelyAsync(
        string destinationPath,
        string content,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(content);

        var fullDestPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = Path.GetFileName(fullDestPath);
        var tempPath = Path.Combine(directory!, $"{fileName}.{Guid.NewGuid():N}.tmp");
        var enc = encoding ?? Encoding.UTF8;

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough | FileOptions.Asynchronous))
            await using (var writer = new StreamWriter(stream, enc))
            {
                await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            bool replaced = false;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    if (File.Exists(fullDestPath))
                    {
                        File.Move(tempPath, fullDestPath, overwrite: true);
                    }
                    else
                    {
                        File.Move(tempPath, fullDestPath);
                    }
                    replaced = true;
                    break;
                }
                catch (Exception ex) when (attempt < 9 && (ex is IOException or UnauthorizedAccessException))
                {
                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                }
            }

            if (!replaced)
            {
                File.Copy(tempPath, fullDestPath, overwrite: true);
                try
                {
                    File.Delete(tempPath);
                }
                catch { /* best-effort cleanup */ }
            }
        }
        catch
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch { /* best-effort cleanup */ }

            throw;
        }
    }
}
