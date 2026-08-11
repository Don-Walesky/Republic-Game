namespace Republic.Core.Tests.Tutorial;

using Republic.Core.Tutorial.Services;
using Xunit;

public sealed class TutorialEngineTests
{
    [Fact]
    public void GetCurrentStep_ReturnsFirstStepOnInitialization()
    {
        var engine = new TutorialEngine();

        var step = engine.GetCurrentStep();

        Assert.NotNull(step);
        Assert.Equal(0, step.StepIndex);
        Assert.Equal("Executive Suite", step.Title);
        Assert.False(engine.IsTutorialFinished);
    }

    [Fact]
    public void AdvanceStep_ProgressesThroughAllTutorialSteps()
    {
        var engine = new TutorialEngine();

        Assert.True(engine.AdvanceStep()); // to 1
        Assert.True(engine.AdvanceStep()); // to 2
        Assert.True(engine.AdvanceStep()); // to 3
        Assert.True(engine.AdvanceStep()); // to 4 (finished)

        Assert.True(engine.IsTutorialFinished);
        Assert.Null(engine.GetCurrentStep());
    }
}
