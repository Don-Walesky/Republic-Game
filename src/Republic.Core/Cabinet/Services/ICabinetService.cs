namespace Republic.Core.Cabinet.Services;

using Republic.Core.Cabinet.Models;

/// <summary>
/// Service interface managing executive cabinet appointments, minister performance, advice generation, and intrigues.
/// </summary>
public interface ICabinetService
{
    IReadOnlyList<Minister> GetAllMinisters();
    Minister? GetAppointedMinister(CabinetPortfolio portfolio);
    Task<Minister> AppointMinisterAsync(Minister minister, CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task<bool> DismissMinisterAsync(CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task GenerateCabinetAdviceAsync(CancellationToken cancellationToken = default);
    int EvaluateMinisterIntrigues(ulong currentTick);
}
