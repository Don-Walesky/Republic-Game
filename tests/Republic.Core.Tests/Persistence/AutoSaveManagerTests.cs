namespace Republic.Core.Tests.Persistence;

using System;
using System.IO;
using System.Threading.Tasks;
using Republic.Core.Configuration;
using Republic.Core.Events;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Services;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class AutoSaveManagerTests : IDisposable
{
    private readonly string _testSaveDir;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly TaskQueueManager _taskQueue;
    private readonly TimeSystem _time;
    private readonly SaveGameManager _saveManager;

    public AutoSaveManagerTests()
    {
        _testSaveDir = Path.Combine(Path.GetTempPath(), "RepublicAutoSaveTest_" + Guid.NewGuid().ToString("N"));
        var config = new PersistenceConfiguration { SaveDirectory = _testSaveDir };
        var serializer = new JsonStateSerializer();
        var store = new FileSaveStore(config, serializer);

        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _time = new TimeSystem(new TimeSystemConfiguration(), _eventBus, _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("AutoSave Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _taskQueue = new TaskQueueManager(_eventBus, _logger);

        _saveManager = new SaveGameManager(store, config, _world, _workspace, _taskQueue, _time, narrativeEngine: null, logger: _logger);
    }

    [Fact]
    public void CalculateChecksum_GeneratesConsistentHash()
    {
        string text = "{\"SaveSlot\":\"test\",\"CurrentTick\":42}";
        string hash1 = SaveChecksumValidator.CalculateChecksum(text);
        string hash2 = SaveChecksumValidator.CalculateChecksum(text);

        Assert.Equal(hash1, hash2);
        Assert.True(SaveChecksumValidator.VerifyChecksum(text, hash1));
    }

    [Fact]
    public void VerifyChecksum_ReturnsFalse_WhenContentIsTampered()
    {
        string original = "{\"Treasury\":1000}";
        string tampered = "{\"Treasury\":9999999}";
        string hash = SaveChecksumValidator.CalculateChecksum(original);

        Assert.False(SaveChecksumValidator.VerifyChecksum(tampered, hash));
    }

    [Fact]
    public async Task ProcessTickAsync_TriggersSave_OnInterval()
    {
        var autoSaveManager = new AutoSaveManager(_saveManager, _eventBus, _logger, 50);

        bool saved = await autoSaveManager.ProcessTickAsync(100);
        Assert.True(saved);
        Assert.Contains("autosave", _saveManager.ListSaveSlots(), StringComparer.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSaveDir))
        {
            try { Directory.Delete(_testSaveDir, true); } catch { }
        }
    }
}
