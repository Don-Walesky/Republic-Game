namespace Republic.Core.Persistence.Services;

using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Persistence.Models;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation coordinating full state serialization, file saving, and slot loading.
/// </summary>
public sealed class SaveGameManager : ISaveGameManager
{
    private readonly FileSaveStore _store;
    private readonly PersistenceConfiguration _config;
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ITaskQueueManager _taskQueueManager;
    private readonly ITimeSystem _timeSystem;
    private readonly ILogger? _logger;

    public SaveGameManager(
        FileSaveStore store,
        PersistenceConfiguration config,
        IWorldManager worldManager,
        IWorkspaceManager workspaceManager,
        ITaskQueueManager taskQueueManager,
        ITimeSystem timeSystem,
        ILogger? logger = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _workspaceManager = workspaceManager ?? throw new ArgumentNullException(nameof(workspaceManager));
        _taskQueueManager = taskQueueManager ?? throw new ArgumentNullException(nameof(taskQueueManager));
        _timeSystem = timeSystem ?? throw new ArgumentNullException(nameof(timeSystem));
        _logger = logger;
    }

    public async Task<string> SaveGameAsync(string slotName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);

        var fullState = new FullGameState
        {
            SaveName = slotName,
            CurrentTick = _timeSystem.CurrentTick,
            SaveTimestamp = DateTimeOffset.UtcNow,
            World = _worldManager.Snapshot(),
            Workspace = _workspaceManager.GetCurrentState(),
            ActiveTasks = _taskQueueManager.GetActiveTasks().ToList(),
            CompletedTasks = _taskQueueManager.GetCompletedTasks().ToList(),
        };

        var envelope = new SaveEnvelope<FullGameState>
        {
            FormatVersion = 1,
            SavedAt = fullState.SaveTimestamp,
            State = fullState
        };

        var fileName = $"{slotName.ToLowerInvariant().Replace(" ", "_")}.sav";
        var path = await _store.SaveAsync(fileName, envelope, cancellationToken).ConfigureAwait(false);
        _logger?.LogInfo($"Game saved to slot '{slotName}' at path: {path}");
        return path;
    }

    public async Task<FullGameState> LoadGameAsync(string slotName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);

        var fileName = $"{slotName.ToLowerInvariant().Replace(" ", "_")}.sav";
        var directory = Path.GetFullPath(_config.SaveDirectory);
        var fullPath = Path.Combine(directory, fileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Save file for slot '{slotName}' not found.", fullPath);
        }

        var envelope = await _store.LoadAsync<FullGameState>(fullPath, cancellationToken).ConfigureAwait(false);
        if (envelope.State == null)
        {
            throw new InvalidDataException($"Save file '{slotName}' is corrupted or empty.");
        }

        var state = envelope.State;
        _worldManager.Restore(state.World);
        _logger?.LogInfo($"Game loaded successfully from slot '{slotName}' (Saved tick: {state.CurrentTick})");
        return state;
    }

    public IReadOnlyList<string> ListSaveSlots()
    {
        var directory = Path.GetFullPath(_config.SaveDirectory);
        if (!Directory.Exists(directory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(directory, "*.sav")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != null)
            .Select(name => name!)
            .ToList()
            .AsReadOnly();
    }

    public bool DeleteSaveSlot(string slotName)
    {
        var fileName = $"{slotName.ToLowerInvariant().Replace(" ", "_")}.sav";
        var directory = Path.GetFullPath(_config.SaveDirectory);
        var fullPath = Path.Combine(directory, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger?.LogInfo($"Save slot '{slotName}' deleted.");
            return true;
        }

        return false;
    }
}
