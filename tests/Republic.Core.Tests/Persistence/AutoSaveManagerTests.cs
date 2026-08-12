namespace Republic.Core.Tests.Persistence;

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Services;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public class AutoSaveManagerTests
{
    [Fact]
    public async Task ProcessTickAsync_Triggers_AutoSave_On_Configured_Tick_Interval()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "RepublicAutoSaveTests_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var logger = new TestLogger();
            var eventBus = new EventBus(new EventBusOptions(), logger);
            var timeSystem = new TimeSystem(new TimeConfiguration());
            var taskQueue = new TaskQueueManager(eventBus, logger);
            var serializer = new JsonStateSerializer();
            var config = new PersistenceConfiguration { SaveDirectory = tempDir };
            var store = new FileSaveStore(serializer, config);

            var world = new WorldManager(eventBus);
            var workspace = new WorkspaceManager(
                new VisitorService(eventBus, logger),
                new PhoneService(eventBus, logger),
                new EmailService(eventBus, logger),
                new NewsService(eventBus, logger),
                new CalendarService(eventBus, logger),
                eventBus,
                logger);

            var saveManager = new SaveGameManager(store, config, world, workspace, taskQueue, timeSystem, null, logger);
            var autoSaveManager = new AutoSaveManager(saveManager, eventBus, logger, autoSaveIntervalTicks: 50);

            // Tick 49 -> No autosave
            bool saved49 = await autoSaveManager.ProcessTickAsync(49);
            Assert.False(saved49);

            // Tick 50 -> Autosave triggered
            bool saved50 = await autoSaveManager.ProcessTickAsync(50);
            Assert.True(saved50);

            string expectedPath = Path.Combine(tempDir, "autosave.sav");
            Assert.True(File.Exists(expectedPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
