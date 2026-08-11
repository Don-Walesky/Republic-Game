namespace Republic.Core.Engine;

using System;
using Republic.Core.Military.Models;
using Republic.Core.World.Models;

/// <summary>
/// Status outcome of campaign state evaluation.
/// </summary>
public enum GameEndStatus
{
    Ongoing,
    Impeached,
    MilitaryCoup,
    Insolvent,
    ReelectedVictory,
    DefeatedElections
}

/// <summary>
/// Result snapshot summarizing game end-state evaluation.
/// </summary>
public sealed record GameEndResult(GameEndStatus Status, string Explanation, DateTimeOffset EvaluatedAt);

/// <summary>
/// Service evaluating victory or defeat conditions per game frame.
/// </summary>
public sealed class GameEndConditionEvaluator
{
    private readonly GameDifficultySettings _settings;

    public GameEndConditionEvaluator(GameDifficultySettings? settings = null)
    {
        _settings = settings ?? new GameDifficultySettings();
    }

    public GameEndResult EvaluateGameState(
        double treasuryBalance,
        Demographics demographics,
        MilitaryReadinessReport militaryReport,
        ulong currentTick)
    {
        ArgumentNullException.ThrowIfNull(demographics);

        // 1. Economic Insolvency Defeat
        if (treasuryBalance < _settings.InsolvencyThreshold)
        {
            return new GameEndResult(
                GameEndStatus.Insolvent,
                $"National Debt Spiral: Treasury balance fell below critical limit (${_settings.InsolvencyThreshold:N0}). International lenders seized sovereign assets.",
                DateTimeOffset.UtcNow);
        }

        // 2. Impeachment Defeat
        if (demographics.HappinessRating < _settings.ImpeachmentRiskThreshold)
        {
            return new GameEndResult(
                GameEndStatus.Impeached,
                $"Parliamentary Impeachment: Public approval collapsed to {demographics.HappinessRating:0.0}%. You were impeached by a legislative supermajority.",
                DateTimeOffset.UtcNow);
        }

        // 3. Military Coup Defeat
        if (militaryReport != null && militaryReport.CompositeReadinessScore < _settings.MilitaryCoupThreshold && militaryReport.Defcon == DefconLevel.Defcon1_MaximumReadiness)
        {
            return new GameEndResult(
                GameEndStatus.MilitaryCoup,
                $"Armed Forces Junta Coup: Military readiness degraded to {militaryReport.CompositeReadinessScore:0.0}% during DEFCON 1. High command seized executive power.",
                DateTimeOffset.UtcNow);
        }

        // 4. Presidential Term Re-election Victory / Election Loss Milestone
        if (currentTick >= _settings.MandateMaxTicks)
        {
            if (demographics.HappinessRating >= 60.0 && treasuryBalance >= 0)
            {
                return new GameEndResult(
                    GameEndStatus.ReelectedVictory,
                    $"Landslide Election Victory: You concluded your presidential term with {demographics.HappinessRating:0.0}% approval and secured a historic mandate!",
                    DateTimeOffset.UtcNow);
            }
            else
            {
                return new GameEndResult(
                    GameEndStatus.DefeatedElections,
                    $"Electoral Defeat: Voters rejected your administration at the ballot box due to stagnating public welfare.",
                    DateTimeOffset.UtcNow);
            }
        }

        return new GameEndResult(GameEndStatus.Ongoing, "Presidential administration active.", DateTimeOffset.UtcNow);
    }
}
