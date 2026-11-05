namespace Republic.Core.Military.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating the sovereign military AI and algorithmic threat detection suite.
/// Provides deep-threat situational analysis, combat simulation forecasting, and heightened foreign spy detection.
/// </summary>
public sealed class MilitaryAiRecord
{
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("ai_system_name")]
    public string AiSystemName { get; set; } = "Aegis-9 Sovereign Defense Neural Grid";

    [JsonPropertyName("analysis_level")]
    public int AnalysisLevel { get; set; } = 1; // Tier 1: Heuristic, Tier 2: Neural Predictive, Tier 3: Autonomous Threat Intercept

    [JsonPropertyName("computing_funding_level")]
    public double ComputingFundingLevel { get; set; } = 25_000_000.0; // R / Qtr

    [JsonPropertyName("threat_detection_bonus")]
    public double ThreatDetectionBonus => IsActive
        ? Math.Clamp(Math.Round(12.0 + (AnalysisLevel * 6.5) + (ComputingFundingLevel / 5_000_000.0), 1), 5.0, 45.0)
        : 0.0;

    [JsonPropertyName("predictive_threat_summaries")]
    public List<string> PredictiveThreatSummaries { get; set; } = new()
    {
        "Frontier radar feeds indicate peacetime regional electronic emissions.",
        "Algorithm confidence: Zero immediate ballistic or armored mobilization signatures detected.",
        "Counter-espionage neural models active; monitoring unverified encrypted network handshakes."
    };

    [JsonPropertyName("status_summary")]
    public string StatusSummary => IsActive
        ? $"Online 🤖 [Tier {AnalysisLevel}]: {AiSystemName} providing +{ThreatDetectionBonus:0.0}% tactical detection bonus across land, air, and cyber corridors."
        : "Offline ⚠️: Algorithmic threat assessment disabled due to zero computing allocation.";

    /// <summary>
    /// Upgrades the military AI analysis tier with sovereign high-performance computing resources.
    /// </summary>
    public void UpgradeAnalysisTier(int newTier)
    {
        AnalysisLevel = Math.Clamp(newTier, 1, 3);
        if (AnalysisLevel >= 2 && !PredictiveThreatSummaries.Contains("Neural predictive threat vector scanning online."))
        {
            PredictiveThreatSummaries.Add("Neural predictive threat vector scanning online.");
        }
        if (AnalysisLevel >= 3 && !PredictiveThreatSummaries.Contains("Autonomous satellite signal interception and orbital threat tracking active."))
        {
            PredictiveThreatSummaries.Add("Autonomous satellite signal interception and orbital threat tracking active.");
        }
    }

    /// <summary>
    /// Sets quarterly high-performance computing funding for Military AI.
    /// </summary>
    public void SetFunding(double quarterlyFunding)
    {
        ComputingFundingLevel = Math.Max(5_000_000.0, quarterlyFunding);
        if (ComputingFundingLevel >= 50_000_000.0 && AnalysisLevel < 3)
        {
            UpgradeAnalysisTier(AnalysisLevel + 1);
        }
    }
}

