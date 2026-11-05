namespace Republic.Core.Military.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating national border control, frontier monitoring, and customs integrity.
/// Directly suppresses contraband smuggling, unmonitored human ingress, and provides early detection of foreign intelligence operatives.
/// </summary>
public sealed class BorderSecurityRecord
{
    [JsonPropertyName("funding_level")]
    public double FundingLevel { get; set; } = 35_000_000.0; // R / Qtr

    [JsonPropertyName("patrol_strength")]
    public int PatrolStrength { get; set; } = 3500;

    [JsonPropertyName("surveillance_tech_level")]
    public double SurveillanceTechLevel { get; set; } = 65.0; // 0 - 100%

    [JsonPropertyName("contraband_interception_rate")]
    public double ContrabandInterceptionRate => Math.Clamp(Math.Round(30.0 + (FundingLevel / 2_000_000.0) + (SurveillanceTechLevel * 0.35), 1), 10.0, 95.0);

    [JsonPropertyName("infiltration_detection_chance")]
    public double InfiltrationDetectionChance => Math.Clamp(Math.Round(25.0 + (PatrolStrength / 100.0) + (SurveillanceTechLevel * 0.40), 1), 15.0, 95.0);

    [JsonPropertyName("effectiveness_score")]
    public double EffectivenessScore => Math.Round((ContrabandInterceptionRate * 0.50) + (InfiltrationDetectionChance * 0.50), 1);

    [JsonPropertyName("status_summary")]
    public string StatusSummary => EffectivenessScore >= 75.0
        ? "Frontiers Fortified 🟢: High-tech sensor grids and active patrols minimize contraband and intercept unauthorized crossings."
        : (EffectivenessScore >= 50.0
            ? "Standard Frontier Watch 🟡: Routine border checkpoints functional; minor contraband leakage detected in rural corridors."
            : "Porous Frontiers 🔴: Underfunded border posts report significant smuggling activities and unmonitored transit.");

    /// <summary>
    /// Funds and modernizes the national border security apparatus.
    /// </summary>
    public void FundBorderSecurity(double newQuarterlyFunding)
    {
        FundingLevel = Math.Max(10_000_000.0, newQuarterlyFunding);
        if (FundingLevel >= 60_000_000.0)
        {
            SurveillanceTechLevel = Math.Clamp(SurveillanceTechLevel + 5.0, 20.0, 100.0);
            PatrolStrength += 500;
        }
        else if (FundingLevel < 20_000_000.0)
        {
            SurveillanceTechLevel = Math.Clamp(SurveillanceTechLevel - 3.0, 20.0, 100.0);
        }
    }
}

