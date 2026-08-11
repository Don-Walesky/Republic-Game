namespace Republic.Core.Persistence.Services;

using Republic.Core.Persistence.Models;

/// <summary>
/// Service interface managing session saves, loads, quick saves, and slot enumeration.
/// </summary>
public interface ISaveGameManager
{
    Task<string> SaveGameAsync(string slotName, CancellationToken cancellationToken = default);
    Task<FullGameState> LoadGameAsync(string slotName, CancellationToken cancellationToken = default);
    Task<string> QuickSaveAsync(CancellationToken cancellationToken = default);
    Task<FullGameState> QuickLoadAsync(CancellationToken cancellationToken = default);
    Task<string> AutoSaveAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<string> ListSaveSlots();
    bool DeleteSaveSlot(string slotName);
}
