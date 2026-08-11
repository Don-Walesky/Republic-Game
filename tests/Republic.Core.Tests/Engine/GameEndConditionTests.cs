namespace Republic.Core.Tests.Engine;

using Republic.Core.Engine;
using Republic.Core.Military.Models;
using Republic.Core.World.Models;
using Xunit;

public sealed class GameEndConditionTests
{
    [Fact]
    public void EvaluateGameState_TriggersInsolvent_WhenTreasuryDeficitTooHigh()
    {
        var evaluator = new GameEndConditionEvaluator();
        var demographics = new Demographics { HappinessRating = 80.0 };
        var military = new MilitaryReadinessReport { CompositeReadinessScore = 75.0 };

        var result = evaluator.EvaluateGameState(-10_000_000.0, demographics, military, 100);

        Assert.Equal(GameEndStatus.Insolvent, result.Status);
    }

    [Fact]
    public void EvaluateGameState_TriggersImpeachment_WhenHappinessBelowThreshold()
    {
        var evaluator = new GameEndConditionEvaluator(GameDifficultySettings.FromPreset(DifficultyPreset.Standard));
        var demographics = new Demographics { HappinessRating = 15.0 };
        var military = new MilitaryReadinessReport { CompositeReadinessScore = 70.0 };

        var result = evaluator.EvaluateGameState(1_000_000.0, demographics, military, 100);

        Assert.Equal(GameEndStatus.Impeached, result.Status);
    }

    [Fact]
    public void EvaluateGameState_TriggersReelectedVictory_WhenMandateCompletedSuccessfully()
    {
        var evaluator = new GameEndConditionEvaluator(new GameDifficultySettings { MandateMaxTicks = 500 });
        var demographics = new Demographics { HappinessRating = 75.0 };
        var military = new MilitaryReadinessReport { CompositeReadinessScore = 90.0 };

        var result = evaluator.EvaluateGameState(5_000_000.0, demographics, military, 500);

        Assert.Equal(GameEndStatus.ReelectedVictory, result.Status);
    }
}
