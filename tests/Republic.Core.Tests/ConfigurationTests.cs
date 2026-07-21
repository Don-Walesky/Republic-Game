namespace Republic.Core.Tests;

using Republic.Core.Configuration;

public sealed class ConfigurationTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsConfiguration()
    {
        var manager = new JsonConfigurationManager();
        var path = Path.Combine(Path.GetTempPath(), $"republic-config-{Guid.NewGuid():N}.json");
        var configuration = new RepublicConfiguration
        {
            Engine = new EngineConfiguration { StartupFrameCount = 9, FrameDeltaMilliseconds = 42, WorldName = "Test" },
            Persistence = new PersistenceConfiguration { SaveDirectory = "temp", FormatVersion = 3 },
        };

        manager.Save(path, configuration);
        var loaded = manager.Load(path);

        Assert.Equal(9, loaded.Engine.StartupFrameCount);
        Assert.Equal(42, loaded.Engine.FrameDeltaMilliseconds);
        Assert.Equal("Test", loaded.Engine.WorldName);
        Assert.Equal(3, loaded.Persistence.FormatVersion);
    }

    [Fact]
    public void Reload_UsesLastLoadedPath()
    {
        var manager = new JsonConfigurationManager();
        var path = Path.Combine(Path.GetTempPath(), $"republic-config-{Guid.NewGuid():N}.json");
        manager.Save(path, new RepublicConfiguration { Engine = new EngineConfiguration { StartupFrameCount = 2 } });
        File.WriteAllText(path, """
            {"engine":{"startupFrameCount":7}}
            """);

        var loaded = manager.Reload();

        Assert.Equal(7, loaded.Engine.StartupFrameCount);
    }
}
