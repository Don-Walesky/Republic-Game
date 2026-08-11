namespace Republic.Core.Engine;

/// <summary>
/// Preset difficulty modes adjusting economic decay, AI aggression, and rebellion thresholds.
/// </summary>
public enum DifficultyPreset
{
    Relaxed,
    Standard,
    Realist,
    Nightmare
}

/// <summary>
/// Configuration parameters controlling game challenge multipliers and end-game thresholds.
/// </summary>
public sealed class GameDifficultySettings
{
    public DifficultyPreset Preset { get; set; } = DifficultyPreset.Standard;
    public double AiAggressionMultiplier { get; set; } = 1.0;
    public double EconomicDecayMultiplier { get; set; } = 1.0;
    public double ImpeachmentRiskThreshold { get; set; } = 25.0; // Public happiness below 25% triggers impeachment risk
    public double MilitaryCoupThreshold { get; set; } = 30.0;     // Composite military readiness below 30% or DEFCON 1 chaos
    public double InsolvencyThreshold { get; set; } = -5_000_000.0; // Negative treasury limit
    public ulong MandateMaxTicks { get; set; } = 1000;            // Re-election evaluation tick milestone

    public static GameDifficultySettings FromPreset(DifficultyPreset preset)
    {
        return preset switch
        {
            DifficultyPreset.Relaxed => new GameDifficultySettings
            {
                Preset = preset,
                AiAggressionMultiplier = 0.5,
                EconomicDecayMultiplier = 0.7,
                ImpeachmentRiskThreshold = 15.0,
                MilitaryCoupThreshold = 20.0
            },
            DifficultyPreset.Realist => new GameDifficultySettings
            {
                Preset = preset,
                AiAggressionMultiplier = 1.4,
                EconomicDecayMultiplier = 1.3,
                ImpeachmentRiskThreshold = 35.0,
                MilitaryCoupThreshold = 40.0
            },
            DifficultyPreset.Nightmare => new GameDifficultySettings
            {
                Preset = preset,
                AiAggressionMultiplier = 2.0,
                EconomicDecayMultiplier = 1.8,
                ImpeachmentRiskThreshold = 45.0,
                MilitaryCoupThreshold = 50.0
            },
            _ => new GameDifficultySettings()
        };
    }
}
