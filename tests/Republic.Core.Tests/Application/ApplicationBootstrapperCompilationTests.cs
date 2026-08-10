namespace Republic.Core.Tests.Application;

using Republic.App;
using Republic.Core.AI.Services;

public sealed class ApplicationBootstrapperCompilationTests
{
    [Fact]
    public void BootstrapperAndRivalAIServiceTypes_AreResolvableTogether()
    {
        Assert.NotNull(typeof(ApplicationBootstrapper));
        Assert.True(typeof(IRivalAIService).IsAssignableFrom(typeof(RivalAIService)));
    }
}
