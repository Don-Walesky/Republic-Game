namespace Republic.Core.Tests.Application;

using Republic.App;
using Republic.Core.AI.Services;

public sealed class ApplicationBootstrapperCompilationTests
{
    [Fact]
    public void Bootstrapper_CanResolveApplicationWithRivalAIServiceRegistration()
    {
        var application = new ApplicationBootstrapper().Bootstrap();

        Assert.NotNull(application);
        Assert.NotNull(application.ElectionService);
        Assert.True(typeof(IRivalAIService).IsAssignableFrom(typeof(RivalAIService)));
    }
}
