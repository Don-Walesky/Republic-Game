namespace Republic.Core.Military.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing the national armed forces, comprising the Army, Navy, and Air Force.
/// Coordinates inter-branch resource distribution, composite combat readiness, and overall defense posture.
/// </summary>
public sealed class ArmedForcesRecord
{
    [JsonPropertyName("army")]
    public ArmedForcesBranch Army { get; set; } = new()
    {
        Branch = MilitaryBranch.Army,
        Name = "National Army (Ground Forces)",
        PersonnelCount = 45000,
        EquipmentCount = 520,
        ReadinessScore = 82.0,
        QuarterlyFunding = 200_000_000.0
    };

    [JsonPropertyName("navy")]
    public ArmedForcesBranch Navy { get; set; } = new()
    {
        Branch = MilitaryBranch.Navy,
        Name = "Naval Fleet & Coastal Defense",
        PersonnelCount = 14000,
        EquipmentCount = 48,
        ReadinessScore = 78.0,
        QuarterlyFunding = 150_000_000.0
    };

    [JsonPropertyName("air_force")]
    public ArmedForcesBranch AirForce { get; set; } = new()
    {
        Branch = MilitaryBranch.AirForce,
        Name = "Air Force & Air Defense Command",
        PersonnelCount = 16000,
        EquipmentCount = 135,
        ReadinessScore = 85.0,
        QuarterlyFunding = 150_000_000.0
    };

    [JsonPropertyName("total_personnel")]
    public int TotalPersonnel => Army.PersonnelCount + Navy.PersonnelCount + AirForce.PersonnelCount;

    [JsonPropertyName("total_equipment")]
    public int TotalEquipment => Army.EquipmentCount + Navy.EquipmentCount + AirForce.EquipmentCount;

    [JsonPropertyName("total_budget")]
    public double TotalBudget => Army.QuarterlyFunding + Navy.QuarterlyFunding + AirForce.QuarterlyFunding;

    [JsonPropertyName("composite_readiness")]
    public double CompositeReadiness => Math.Round((Army.ReadinessScore * 0.40) + (Navy.ReadinessScore * 0.30) + (AirForce.ReadinessScore * 0.30), 1);

    [JsonPropertyName("total_combat_strength")]
    public double TotalCombatStrength => Math.Round(Army.CombatStrength + Navy.CombatStrength + AirForce.CombatStrength, 1);

    /// <summary>
    /// Gets all branches as a readable collection.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<ArmedForcesBranch> Branches => new[] { Army, Navy, AirForce };

    /// <summary>
    /// Allocates the military budget across Army, Navy, and Air Force.
    /// </summary>
    public void AllocateBudget(double armyFunding, double navyFunding, double airForceFunding)
    {
        Army.SetFunding(armyFunding);
        Navy.SetFunding(navyFunding);
        AirForce.SetFunding(airForceFunding);
    }
}

