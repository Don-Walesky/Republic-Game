namespace Republic.Core.Tests.Persistence;

using Republic.Core.Configuration;
using Republic.Core.Events;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Services;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class SaveGameManagerTests : IDisposable
{
    private readonly string _testSaveDir;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly TaskQueueManager _taskQueue;
    private readonly TimeSystem _time;
    private readonly SaveGameManager _saveManager;

    public SaveGameManagerTests()
    {
        _testSaveDir = Path.Combine(Path.GetTempPath(), "RepublicSaveTest_" + Guid.NewGuid().ToString("N"));
        var config = new PersistenceConfiguration { SaveDirectory = _testSaveDir };
        var serializer = new JsonStateSerializer();
        var store = new FileSaveStore(config, serializer);

        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _time = new TimeSystem(new TimeSystemConfiguration(), _eventBus, _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Save Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _taskQueue = new TaskQueueManager(_eventBus, _logger);

        _saveManager = new SaveGameManager(store, config, _world, _workspace, _taskQueue, _time, _logger);
    }

    [Fact]
    public async Task SaveAndLoadGame_RoundTripsState()
    {
        await _world.RegisterEntityAsync("Region", "North Province");

        var path = await _saveManager.SaveGameAsync("Slot_1");
        Assert.True(File.Exists(path));

        var loadedState = await _saveManager.LoadGameAsync("Slot_1");

        Assert.NotNull(loadedState);
        Assert.Equal("Save Test World", loadedState.World.Name);
        Assert.Single(loadedState.World.Entities);
        Assert.Contains(_saveManager.ListSaveSlots(), slot => slot.Equals("slot_1", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DeleteSaveSlot_RemovesFile()
    {
        await _saveManager.SaveGameAsync("TempSlot");

        var deleted = _saveManager.DeleteSaveSlot("TempSlot");

        Assert.True(deleted);
        Assert.DoesNotContain(_saveManager.ListSaveSlots(), slot => slot.Equals("tempslot", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSaveDir))
        {
            try { Directory.Delete(_testSaveDir, true); } catch { }
        }
    }
}
