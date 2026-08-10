namespace Republic.Core.AI.Services;

using Republic.Core.AI.Models;

/// <summary>
/// Service interface managing autonomous rival sovereign nation AI behavior and decision cycles.
/// </summary>
public interface IRivalAIService
{
    void RegisterRivalBot(RivalAIBot bot);
    IReadOnlyList<RivalAIBot> GetRivalBots();
    Task<int> ProcessAITickAsync(ulong currentTick, CancellationToken cancellationToken = default);
}
