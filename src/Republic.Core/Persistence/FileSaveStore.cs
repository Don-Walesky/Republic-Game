namespace Republic.Core.Persistence;

using Republic.Core.Configuration;

/// <summary>
/// Saves JSON state envelopes to disk.
/// </summary>
public sealed class FileSaveStore
{
    private readonly PersistenceConfiguration _configuration;
    private readonly IStateSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSaveStore"/> class.
    /// </summary>
    public FileSaveStore(PersistenceConfiguration configuration, IStateSerializer serializer)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Saves the supplied envelope under the configured save directory.
    /// </summary>
    public async Task<string> SaveAsync<TState>(string fileName, SaveEnvelope<TState> envelope, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(envelope);

        var directory = Path.GetFullPath(_configuration.SaveDirectory);
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, fileName);
        var payload = _serializer.Serialize(envelope);
        await File.WriteAllTextAsync(fullPath, payload, cancellationToken).ConfigureAwait(false);
        return fullPath;
    }

    /// <summary>
    /// Loads a save envelope from disk.
    /// </summary>
    public async Task<SaveEnvelope<TState>> LoadAsync<TState>(string fullPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        var payload = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
        return _serializer.Deserialize<TState>(payload);
    }
}
