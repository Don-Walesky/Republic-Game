namespace Republic.Core.Tests.Modding;

using System;
using System.IO;
using Republic.Core.Modding.Services;
using Republic.Core.Scenarios.Models;
using Xunit;

public sealed class ModLoaderTests : IDisposable
{
    private readonly string _testModsDir;

    public ModLoaderTests()
    {
        _testModsDir = Path.Combine(Path.GetTempPath(), "RepublicModTest_" + Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public void ImportAndExportScenario_RoundTripsState()
    {
        var importer = new CustomScenarioImporter();
        var original = new ScenarioPreset
        {
            Id = "custom-cyberpunk",
            Name = "Neo-Arcadia 2099",
            Description = "Corporate oligarchy and cyber warfare scenario.",
            StartingTreasury = 50_000_000_000.0,
            StartingStability = 40.0
        };

        string json = importer.ExportScenarioToJson(original);
        Assert.False(string.IsNullOrWhiteSpace(json));

        var imported = importer.ImportScenarioFromJson(json);
        Assert.Equal("custom-cyberpunk", imported.Id);
        Assert.Equal("Neo-Arcadia 2099", imported.Name);
        Assert.Equal(50_000_000_000.0, imported.StartingTreasury);
    }

    [Fact]
    public void ScanAndLoadMods_LoadsJsonFilesFromDirectory()
    {
        Directory.CreateDirectory(_testModsDir);
        var importer = new CustomScenarioImporter();
        var sample = new ScenarioPreset
        {
            Id = "test-mod-1",
            Name = "Test Campaign"
        };
        string filePath = Path.Combine(_testModsDir, "test_scenario.json");
        File.WriteAllText(filePath, importer.ExportScenarioToJson(sample));

        var loader = new ModLoader(_testModsDir, importer);
        int loaded = loader.ScanAndLoadMods();

        Assert.Equal(1, loaded);
        Assert.Single(loader.CustomScenarios);
        Assert.Equal("test-mod-1", loader.CustomScenarios[0].Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testModsDir))
        {
            try { Directory.Delete(_testModsDir, true); } catch { }
        }
    }
}
