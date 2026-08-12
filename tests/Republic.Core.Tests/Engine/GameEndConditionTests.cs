namespace Republic.Core.Tests.Engine;

using System;
using Xunit;
using Republic.Core.Engine;
using Republic.Core.Military.Models;
using Republic.Core.World.Models;

public class GameEndConditionTests
{
    private static MilitaryReadinessReport CreateDefaultMilitaryReport() => new MilitaryReadinessReport
    {
        Defcon = DefconLevel.Defcon5_Peace,
        CompositeReadinessScore = 80.0,
        LogisticsSupplyEfficiency = 75.0,
        UnitTrainingIndex = 75.0
    };

    [Fact]
    public void Ongoing_Game_Returns_Ongoing_Status()
    {
        var evaluator = new GameEndConditionEvaluator();
        var demographics = new Demographics { HappinessRating = 75.0 };
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(50_000_000, demographics, militaryReport, 100);

        Assert.Equal(GameEndStatus.Ongoing, result.Status);
    }

    [Fact]
    public void Insolvency_Triggers_Insolvent_Status()
    {
        var settings = GameDifficultySettings.FromPreset(DifficultyPreset.Standard);
        var evaluator = new GameEndConditionEvaluator(settings);
        var demographics = new Demographics { HappinessRating = 75.0 };
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(-10_000_000, demographics, militaryReport, 100);

        Assert.Equal(GameEndStatus.Insolvent, result.Status);
    }

    [Fact]
    public void Low_Happiness_Triggers_Impeached_Status()
    {
        var settings = GameDifficultySettings.FromPreset(DifficultyPreset.Standard);
        var evaluator = new GameEndConditionEvaluator(settings);
        var demographics = new Demographics { HappinessRating = 10.0 }; // Below 25.0 threshold
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(1_000_000, demographics, militaryReport, 100);

        Assert.Equal(GameEndStatus.Impeached, result.Status);
    }

    [Theory]
    [InlineData(DifficultyPreset.Relaxed, 95.0, GameEndStatus.RevolutionaryUprising)]
    [InlineData(DifficultyPreset.Nightmare, 70.0, GameEndStatus.RevolutionaryUprising)]
    public void High_Unrest_Triggers_RevolutionaryUprising_Per_Difficulty(DifficultyPreset preset, double unrest, GameEndStatus expectedStatus)
    {
        var settings = GameDifficultySettings.FromPreset(preset);
        var evaluator = new GameEndConditionEvaluator(settings);
        var demographics = new Demographics { HappinessRating = 60.0 };
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(1_000_000, demographics, militaryReport, 100, civilUnrestLevel: unrest);

        Assert.Equal(expectedStatus, result.Status);
    }

    [Fact]
    public void Low_Geopolitical_Standing_Triggers_Subjugation()
    {
        var settings = GameDifficultySettings.FromPreset(DifficultyPreset.Standard);
        var evaluator = new GameEndConditionEvaluator(settings);
        var demographics = new Demographics { HappinessRating = 60.0 };
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(1_000_000, demographics, militaryReport, 100, civilUnrestLevel: 10.0, geopoliticalStanding: 2.0);

        Assert.Equal(GameEndStatus.GeopoliticalSubjugation, result.Status);
    }

    [Fact]
    public void Term_End_With_High_Happiness_Triggers_ReelectedVictory()
    {
        var settings = new GameDifficultySettings { MandateMaxTicks = 500 };
        var evaluator = new GameEndConditionEvaluator(settings);
        var demographics = new Demographics { HappinessRating = 85.0 };
        var militaryReport = CreateDefaultMilitaryReport();

        var result = evaluator.EvaluateGameState(500_000, demographics, militaryReport, 500);

        Assert.Equal(GameEndStatus.ReelectedVictory, result.Status);
    }
}
