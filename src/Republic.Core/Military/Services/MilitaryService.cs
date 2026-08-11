namespace Republic.Core.Military.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Government;
using Republic.Core.Military.Events;
using Republic.Core.Military.Models;

/// <summary>
/// Core implementation of the <see cref="IMilitaryService"/> managing DEFCON readiness and military operations.
/// </summary>
public sealed class MilitaryService : IMilitaryService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private DefconLevel _currentDefcon = DefconLevel.Defcon5_Peace;

    private readonly Dictionary<MilitaryBranch, MilitaryBranchState> _branches = new()
    {
        [MilitaryBranch.Army] = new MilitaryBranchState { Branch = MilitaryBranch.Army, PersonnelCount = 15000, EquipmentCount = 450, BudgetAllocation = 500000m, ReadinessScore = 80.0 },
        [MilitaryBranch.Navy] = new MilitaryBranchState { Branch = MilitaryBranch.Navy, PersonnelCount = 6000, EquipmentCount = 35, BudgetAllocation = 350000m, ReadinessScore = 75.0 },
        [MilitaryBranch.AirForce] = new MilitaryBranchState { Branch = MilitaryBranch.AirForce, PersonnelCount = 8000, EquipmentCount = 120, BudgetAllocation = 450000m, ReadinessScore = 85.0 },
        [MilitaryBranch.CyberCorps] = new MilitaryBranchState { Branch = MilitaryBranch.CyberCorps, PersonnelCount = 2000, EquipmentCount = 800, BudgetAllocation = 200000m, ReadinessScore = 90.0 },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="MilitaryService"/> class.
    /// </summary>
    public MilitaryService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    /// <inheritdoc />
    public MilitaryReadinessReport GetReadinessReport(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Sync total personnel and weapons with state
        int totalPersonnel = state.Military.Personnel > 0 ? state.Military.Personnel : _branches.Values.Sum(b => b.PersonnelCount);
        int totalEquipment = state.Military.WeaponsInventory > 0 ? state.Military.WeaponsInventory : _branches.Values.Sum(b => b.EquipmentCount);
        decimal totalBudget = _branches.Values.Sum(b => b.BudgetAllocation);

        // DEFCON multiplier increases readiness urgency
        double defconMultiplier = (6 - (int)_currentDefcon) * 0.05 + 0.9;
        double compositeReadiness = Math.Min(100.0, _branches.Values.Average(b => b.ReadinessScore) * defconMultiplier);

        return new MilitaryReadinessReport
        {
            Defcon = _currentDefcon,
            TotalPersonnel = totalPersonnel,
            TotalEquipment = totalEquipment,
            TotalDefenseBudget = totalBudget,
            CompositeReadinessScore = Math.Round(compositeReadiness, 1),
            BranchBreakdown = _branches.Values.Select(b => new MilitaryBranchState
            {
                Branch = b.Branch,
                PersonnelCount = b.PersonnelCount,
                EquipmentCount = b.EquipmentCount,
                BudgetAllocation = b.BudgetAllocation,
                ReadinessScore = Math.Min(100.0, Math.Round(b.ReadinessScore * defconMultiplier, 1))
            }).ToList(),
            RecentOperations = state.Military.OperationHistory.ToList()
        };
    }

    /// <inheritdoc />
    public async Task<DefconLevel> SetDefconLevelAsync(GovernmentState state, DefconLevel level, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var prevLevel = _currentDefcon;
        _currentDefcon = level;

        _logger?.LogInfo($"Military DEFCON level changed from {prevLevel} to {level}");

        await _eventBus.PublishAsync(new DefconLevelChangedEvent(prevLevel, level, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return _currentDefcon;
    }

    /// <inheritdoc />
    public async Task<bool> RecruitBranchPersonnelAsync(GovernmentState state, MilitaryBranch branch, int count, decimal costPerUnit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (count <= 0 || costPerUnit < 0) return false;

        decimal totalCost = count * costPerUnit;
        if (state.TreasuryBalance < totalCost)
        {
            _logger?.LogWarning($"Insufficient treasury ({state.TreasuryBalance:C}) to recruit {count} personnel for {branch} (cost: {totalCost:C})");
            return false;
        }

        state.TreasuryBalance -= totalCost;
        state.Military.Personnel += count;

        if (_branches.TryGetValue(branch, out var branchState))
        {
            branchState.PersonnelCount += count;
            branchState.ReadinessScore = Math.Min(100.0, branchState.ReadinessScore + (count / 500.0));
        }

        _logger?.LogInfo($"Recruited {count} personnel for {branch} at total cost of {totalCost:C}");
        await _eventBus.PublishAsync(new TroopsRecruitedEvent(branch, count, totalCost, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ProcureBranchEquipmentAsync(GovernmentState state, MilitaryBranch branch, int units, decimal costPerUnit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (units <= 0 || costPerUnit < 0) return false;

        decimal totalCost = units * costPerUnit;
        if (state.TreasuryBalance < totalCost)
        {
            _logger?.LogWarning($"Insufficient treasury ({state.TreasuryBalance:C}) to procure {units} equipment for {branch} (cost: {totalCost:C})");
            return false;
        }

        state.TreasuryBalance -= totalCost;
        state.Military.WeaponsInventory += units;

        if (_branches.TryGetValue(branch, out var branchState))
        {
            branchState.EquipmentCount += units;
            branchState.ReadinessScore = Math.Min(100.0, branchState.ReadinessScore + (units / 50.0));
        }

        _logger?.LogInfo($"Procured {units} equipment for {branch} at total cost of {totalCost:C}");
        await _eventBus.PublishAsync(new EquipmentProcuredEvent(branch, units, totalCost, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<MilitaryDirectiveResult> ExecuteDirectiveAsync(GovernmentState state, string targetCountry, MilitaryOpType opType, int troopsCommitted, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountry);

        int availablePersonnel = state.Military.Personnel > 0 ? state.Military.Personnel : _branches.Values.Sum(b => b.PersonnelCount);

        if (troopsCommitted <= 0 || troopsCommitted > availablePersonnel)
        {
            return new MilitaryDirectiveResult
            {
                Success = false,
                OperationType = opType.ToString(),
                TargetCountry = targetCountry,
                Message = $"Troop commitment ({troopsCommitted}) exceeds available personnel ({availablePersonnel}).",
                CasualtiesSustained = 0,
                CostIncurred = 0m
            };
        }

        decimal operationCost = troopsCommitted * 100m;
        if (state.TreasuryBalance < operationCost)
        {
            return new MilitaryDirectiveResult
            {
                Success = false,
                OperationType = opType.ToString(),
                TargetCountry = targetCountry,
                Message = $"Insufficient treasury balance ({state.TreasuryBalance:C}) for operation deployment cost ({operationCost:C}).",
                CasualtiesSustained = 0,
                CostIncurred = 0m
            };
        }

        // Deduct operational deployment cost
        state.TreasuryBalance -= operationCost;

        // Calculate success odds based on troop strength, equipment, DEFCON boost
        double defconBonus = (6 - (int)_currentDefcon) * 0.08;
        double baseStrength = (double)troopsCommitted / 10000.0 + (double)state.Military.WeaponsInventory / 2500.0;
        double successProbability = Math.Min(0.95, baseStrength + defconBonus);

        bool success = successProbability >= 0.45;
        int casualties = success ? (int)(troopsCommitted * 0.03) : (int)(troopsCommitted * 0.12);
        int enemyCasualties = success ? (int)(troopsCommitted * 0.18) : (int)(troopsCommitted * 0.05);

        // Deduct casualties from state and branches
        state.Military.Personnel = Math.Max(0, state.Military.Personnel - casualties);
        if (_branches.TryGetValue(MilitaryBranch.Army, out var army))
        {
            army.PersonnelCount = Math.Max(0, army.PersonnelCount - casualties);
        }

        var action = new MilitaryAction
        {
            AttackerCountry = state.CountryName,
            TargetCountry = targetCountry,
            OpType = opType,
            TroopsCommitted = troopsCommitted,
            Succeeded = success,
            Timestamp = DateTimeOffset.UtcNow
        };

        state.Military.OperationHistory.Add(action);

        var result = new MilitaryDirectiveResult
        {
            Success = success,
            OperationType = opType.ToString(),
            TargetCountry = targetCountry,
            Message = success
                ? $"Directive {opType} against {targetCountry} accomplished objective with high tactical outcome."
                : $"Directive {opType} against {targetCountry} met heavy resistance; tactical retreat ordered.",
            CasualtiesSustained = casualties,
            TargetCasualties = enemyCasualties,
            CostIncurred = operationCost,
            Timestamp = DateTimeOffset.UtcNow
        };

        _logger?.LogInfo($"Executed military directive {opType} targeting {targetCountry}. Result: Success={success}, Casualties={casualties}");

        await _eventBus.PublishAsync(new MilitaryDirectiveExecutedEvent(result, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        return result;
    }
}
