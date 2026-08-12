namespace Republic.Core.Intelligence.Services;

using Republic.Core.Intelligence.Models;

/// <summary>
/// Service interface managing national intelligence agency networks and covert operations.
/// </summary>
public interface IIntelligenceService
{
    SpyNetwork GetOrCreateNetwork(string targetCountryId);
    Task<SpyNetwork> InfiltrateTargetAsync(string targetCountryId, int additionalAgents = 2, CancellationToken cancellationToken = default);
    Task<CovertOperation> LaunchOperationAsync(CovertOperationType type, string targetCountryId, string title, CancellationToken cancellationToken = default);
    IReadOnlyList<CovertOperation> GetActiveOperations();
    Task<bool> ConductCounterEspionageSweepAsync(CancellationToken cancellationToken = default);
}
