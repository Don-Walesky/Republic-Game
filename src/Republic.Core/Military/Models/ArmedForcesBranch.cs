namespace Republic.Core.Military.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating an individual service branch of the national armed forces (Army, Navy, Air Force).
/// Tracks personnel strength, equipment/hardware inventory, operational readiness, and quarterly budget allocation.
/// </summary>
public sealed class ArmedForcesBranch
{
    [JsonPropertyName("branch")]
    public MilitaryBranch Branch { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("personnel_count")]
    public int PersonnelCount { get; set; }

    [JsonPropertyName("equipment_count")]
    public int EquipmentCount { get; set; }

    [JsonPropertyName("readiness_score")]
    public double ReadinessScore { get; set; } = 80.0; // 0 - 100%

    [JsonPropertyName("quarterly_funding")]
    public double QuarterlyFunding { get; set; }

    [JsonPropertyName("combat_strength")]
    public double CombatStrength => Math.Round(((PersonnelCount * 0.4) + (EquipmentCount * 25.0)) * (ReadinessScore / 100.0), 1);

    /// <summary>
    /// Recruits new personnel into this service branch.
    /// </summary>
    public void Recruit(int count)
    {
        if (count <= 0) return;
        PersonnelCount += count;
        // Recruitment slightly dilutes raw training readiness until trained up
        ReadinessScore = Math.Clamp(ReadinessScore + (count > 5000 ? -2.0 : 0.5), 20.0, 100.0);
    }

    /// <summary>
    /// Procures modern military equipment/hardware for this service branch.
    /// </summary>
    public void Procure(int units)
    {
        if (units <= 0) return;
        EquipmentCount += units;
        ReadinessScore = Math.Clamp(ReadinessScore + (units * 0.05), 20.0, 100.0);
    }

    /// <summary>
    /// Adjusts the branch funding allocation and recalculates operational readiness.
    /// </summary>
    public void SetFunding(double newFunding)
    {
        QuarterlyFunding = Math.Max(10_000_000.0, newFunding);
        // Generous funding raises readiness; austerity starves maintenance
        if (QuarterlyFunding >= 150_000_000.0)
        {
            ReadinessScore = Math.Clamp(ReadinessScore + 3.0, 20.0, 100.0);
        }
        else if (QuarterlyFunding < 50_000_000.0)
        {
            ReadinessScore = Math.Clamp(ReadinessScore - 4.0, 20.0, 100.0);
        }
    }
}

