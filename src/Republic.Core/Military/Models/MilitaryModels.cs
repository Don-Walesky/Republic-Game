namespace Republic.Core.Military.Models;

using System;
using System.Collections.Generic;
using Republic.Core.Government;

/// <summary>
/// Defines the DEFCON alert state level of the republic.
/// </summary>
public enum DefconLevel
{
    Defcon5_Peace = 5,
    Defcon4_HeightenedWatch = 4,
    Defcon3_AirForceStandby = 3,
    Defcon2_ArmedForcesMobilized = 2,
    Defcon1_MaximumReadiness = 1
}

/// <summary>
/// Represents the distinct branches of the Republic's armed forces.
/// </summary>
public enum MilitaryBranch
{
    Army,
    Navy,
    AirForce,
    CyberCorps
}

/// <summary>
/// Status and readiness metrics for an individual military branch.
/// </summary>
public sealed class MilitaryBranchState
{
    public MilitaryBranch Branch { get; set; }
    public int PersonnelCount { get; set; }
    public int EquipmentCount { get; set; }
    public decimal BudgetAllocation { get; set; }
    public double ReadinessScore { get; set; }
}

/// <summary>
/// Comprehensive state and readiness summary of the republic's military.
/// </summary>
public sealed class MilitaryReadinessReport
{
    public DefconLevel Defcon { get; set; } = DefconLevel.Defcon5_Peace;
    public int TotalPersonnel { get; set; }
    public int TotalEquipment { get; set; }
    public decimal TotalDefenseBudget { get; set; }
    public double CompositeReadinessScore { get; set; }
    public List<MilitaryBranchState> BranchBreakdown { get; set; } = new();
    public List<MilitaryAction> RecentOperations { get; set; } = new();
}

/// <summary>
/// Result outcome of executing a strategic military operation or directive.
/// </summary>
public sealed class MilitaryDirectiveResult
{
    public bool Success { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int CasualtiesSustained { get; set; }
    public int TargetCasualties { get; set; }
    public decimal CostIncurred { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
