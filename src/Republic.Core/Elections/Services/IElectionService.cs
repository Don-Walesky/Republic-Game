namespace Republic.Core.Elections.Services;

using Republic.Core.Elections.Models;

/// <summary>
/// Service interface managing election polling, campaign seasons, and democratic vote transitions.
/// </summary>
public interface IElectionService
{
    bool IsCampaignActive { get; }
    PollingData GetCurrentPollingData();
    Task StartCampaignSeasonAsync(CancellationToken cancellationToken = default);
    Task<PollingData> SimulateWeeklyPollingShiftsAsync(CancellationToken cancellationToken = default);
    Task<ElectionResult> ConductElectionAsync(string incumbentName, string candidateOppositionName, CancellationToken cancellationToken = default);
}
