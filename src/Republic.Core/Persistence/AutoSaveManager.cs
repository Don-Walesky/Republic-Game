namespace Republic.Core.Persistence;

using System;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Persistence.Services;

/// <summary>
/// Event emitted when an automated game save completes.
/// </summary>
public sealed record AutoSaveCompletedEvent(string SlotName, string FilePath, string Checksum, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Service managing tick-interval auto-saves and event-driven save triggers.
/// </summary>
public sealed class AutoSaveManager
{
    private readonly ISaveGameManager _saveGameManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly ulong _autoSaveIntervalTicks;

    public AutoSaveManager(ISaveGameManager saveGameManager, IEventBus eventBus, ILogger? logger = null, ulong autoSaveIntervalTicks = 100)
    {
        _saveGameManager = saveGameManager ?? throw new ArgumentNullException(nameof(saveGameManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
        _autoSaveIntervalTicks = Math.Max(10, autoSaveIntervalTicks);
    }

    public async Task<bool> ProcessTickAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        if (currentTick > 0 && currentTick % _autoSaveIntervalTicks == 0)
        {
            return await TriggerAutoSaveAsync("TickInterval", cancellationToken).ConfigureAwait(false);
        }
        return false;
    }

    public async Task<bool> TriggerAutoSaveAsync(string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            string filePath = await _saveGameManager.AutoSaveAsync(cancellationToken).ConfigureAwait(false);
            string content = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false) : string.Empty;
            string checksum = SaveChecksumValidator.CalculateChecksum(content);

            _logger?.LogInfo($"[AutoSave] Session auto-saved cleanly ({reason}) -> '{filePath}' [Checksum: {checksum[..8]}]");
            await _eventBus.PublishAsync(new AutoSaveCompletedEvent("autosave", filePath, checksum, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[AutoSave] Auto-save failed: {ex.Message}");
            return false;
        }
    }
}
