namespace Republic.Core.Crises.Services;

using Republic.Core.Crises.Models;

/// <summary>
/// Service interface for initiating and handling inter-player economic, cyber, and military warfare actions.
/// </summary>
public interface IInterPlayerWarfareService
{
    Task<CrisisRecord> LaunchTradeEmbargoAsync(string attackerId, string targetId, CancellationToken cancellationToken = default);
    Task<CrisisRecord> LaunchCyberAttackAsync(string attackerId, string targetId, string targetSector, CancellationToken cancellationToken = default);
    Task<CrisisRecord> FundSubversionAsync(string attackerId, string targetId, string targetFactionId, double fundingAmount, CancellationToken cancellationToken = default);
    Task<CrisisRecord> DeployBorderSkirmishAsync(string attackerId, string targetRegionId, CancellationToken cancellationToken = default);
}
