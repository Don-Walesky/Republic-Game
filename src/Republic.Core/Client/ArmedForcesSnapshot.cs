namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable client data contract capturing individual military branch metrics (Army, Navy, Air Force).
/// </summary>
public sealed record ArmedForcesBranchSnapshot
{
    [JsonPropertyName("branch")]
    public string Branch { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("personnel_count")]
    public int PersonnelCount { get; init; }

    [JsonPropertyName("equipment_count")]
    public int EquipmentCount { get; init; }

    [JsonPropertyName("readiness_score")]
    public double ReadinessScore { get; init; }

    [JsonPropertyName("quarterly_funding")]
    public double QuarterlyFunding { get; init; }

    [JsonPropertyName("combat_strength")]
    public double CombatStrength { get; init; }
}

/// <summary>
/// Serializable client data contract capturing the entire armed forces posture across Army, Navy, and Air Force.
/// </summary>
public sealed record ArmedForcesSnapshot
{
    [JsonPropertyName("army")]
    public ArmedForcesBranchSnapshot Army { get; init; } = new();

    [JsonPropertyName("navy")]
    public ArmedForcesBranchSnapshot Navy { get; init; } = new();

    [JsonPropertyName("air_force")]
    public ArmedForcesBranchSnapshot AirForce { get; init; } = new();

    [JsonPropertyName("total_personnel")]
    public int TotalPersonnel { get; init; }

    [JsonPropertyName("total_equipment")]
    public int TotalEquipment { get; init; }

    [JsonPropertyName("total_budget")]
    public double TotalBudget { get; init; }

    [JsonPropertyName("composite_readiness")]
    public double CompositeReadiness { get; init; }

    [JsonPropertyName("total_combat_strength")]
    public double TotalCombatStrength { get; init; }
}

