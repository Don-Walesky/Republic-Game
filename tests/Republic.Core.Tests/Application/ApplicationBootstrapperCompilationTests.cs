namespace Republic.Core.Tests.Application;

using Republic.App;
using Republic.Core.AI.Services;
using Republic.Core.Elections.Services;

public sealed class ApplicationBootstrapperCompilationTests
{
    [Fact]
    public void Bootstrapper_CanResolveApplicationWithRivalAIServiceRegistration()
    {
        var application = new ApplicationBootstrapper().Bootstrap();

        Assert.NotNull(application);
        Assert.NotNull(application.ElectionService);
        Assert.IsType<RivalAIService>(application.RivalAIService);
        Assert.Same(application.Clock, ((ElectionService)application.ElectionService).Clock);
    }
}
