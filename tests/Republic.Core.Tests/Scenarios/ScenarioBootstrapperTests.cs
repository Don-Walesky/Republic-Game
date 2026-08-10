namespace Republic.Core.Tests.Scenarios;

using Republic.Core.Cabinet.Services;
using Republic.Core.Events;
using Republic.Core.Legislature.Services;
using Republic.Core.Scenarios.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class ScenarioBootstrapperTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly CabinetService _cabinet;
    private readonly LegislatureService _legislature;

    public ScenarioBootstrapperTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
        _cabinet = new CabinetService(_world, _eventBus, _workspace, _logger);
        _legislature = new LegislatureService(_eventBus, _workspace, _logger);
    }

    [Fact]
    public async Task BootstrapScenario_InitializesWorld_Cabinet_AndWorkspace()
    {
        var bootstrapper = new ScenarioBootstrapper(_world, _workspace, _cabinet, _legislature, _logger);
        var presets = bootstrapper.GetAvailablePresets();

        Assert.NotEmpty(presets);

        var scenario = await bootstrapper.BootstrapScenarioAsync(presets[0].Id);

        Assert.NotNull(scenario);
        Assert.Equal("Republic of Arcadia - Day 1", scenario.Name);
        Assert.NotEmpty(_world.Countries.GetAllCountries());
        Assert.NotEmpty(_cabinet.GetAllMinisters());
        Assert.NotEmpty(_legislature.GetParties());
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
