namespace Republic.Core.Tests.Scenarios;

using System.Threading.Tasks;
using Republic.Core.Cabinet.Services;
using Republic.Core.Events;
using Republic.Core.Legislature.Services;
using Republic.Core.Scenarios.Models;
using Republic.Core.Scenarios.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class ScenarioPresetsTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly CabinetService _cabinet;
    private readonly LegislatureService _legislature;

    public ScenarioPresetsTests()
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
    public async Task BootstrapScenario_InitializesAllPresetsCleanly()
    {
        var bootstrapper = new ScenarioBootstrapper(_world, _workspace, _cabinet, _legislature, _logger);
        var presets = bootstrapper.GetAvailablePresets();

        Assert.Equal(4, presets.Count);

        foreach (var id in ScenarioPresets.AllPresetIds)
        {
            var preset = await bootstrapper.BootstrapScenarioAsync(id);
            Assert.NotNull(preset);
            Assert.Equal(id, preset.Id);
        }
    }
}
