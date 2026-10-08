namespace Republic.Core.Cabinet.Services;

using Republic.Core.Cabinet.Models;
using Republic.Core.World.Models;

/// <summary>
/// Service interface managing executive cabinet appointments, minister performance, advice generation, and intrigues.
/// Connects ministerial appointments to sovereign State Capacity.
/// </summary>
public interface ICabinetService
{
    IReadOnlyList<Minister> GetAllMinisters();
    Minister? GetAppointedMinister(CabinetPortfolio portfolio);
    Minister? GetAppointedMinister(Country country, CabinetPortfolio portfolio);
    Task<Minister> AppointMinisterAsync(Minister minister, CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task<Minister> AppointMinisterAsync(Country country, Minister minister, CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task<bool> DismissMinisterAsync(CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task<bool> DismissMinisterAsync(Country country, CabinetPortfolio portfolio, CancellationToken cancellationToken = default);
    Task GenerateCabinetAdviceAsync(CancellationToken cancellationToken = default);
    int EvaluateMinisterIntrigues(ulong currentTick);
}
