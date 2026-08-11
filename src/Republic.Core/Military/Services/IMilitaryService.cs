namespace Republic.Core.Military.Services;

using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Government;
using Republic.Core.Military.Models;

/// <summary>
/// Service interface for managing defense readiness, armed forces branches, DEFCON alert states, and military operations.
/// </summary>
public interface IMilitaryService
{
    /// <summary>
    /// Computes and returns the overall military readiness report across all branches.
    /// </summary>
    MilitaryReadinessReport GetReadinessReport(GovernmentState state);

    /// <summary>
    /// Changes the republic's active DEFCON threat level and triggers readiness adjustments.
    /// </summary>
    Task<DefconLevel> SetDefconLevelAsync(GovernmentState state, DefconLevel level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recruits personnel into a specific military branch using government funds.
    /// </summary>
    Task<bool> RecruitBranchPersonnelAsync(GovernmentState state, MilitaryBranch branch, int count, decimal costPerUnit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Procures equipment and weapons for a specific military branch using government funds.
    /// </summary>
    Task<bool> ProcureBranchEquipmentAsync(GovernmentState state, MilitaryBranch branch, int units, decimal costPerUnit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a strategic military directive or attack against a foreign target nation.
    /// </summary>
    Task<MilitaryDirectiveResult> ExecuteDirectiveAsync(GovernmentState state, string targetCountry, MilitaryOpType opType, int troopsCommitted, CancellationToken cancellationToken = default);
}
