namespace Republic.Core.Tests;

using Republic.Core.Configuration;
using Republic.Core.Persistence;

public sealed class PersistenceTests
{
    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsEnvelope()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"republic-saves-{Guid.NewGuid():N}");
        var store = new FileSaveStore(new PersistenceConfiguration { SaveDirectory = directory, FormatVersion = 1 }, new JsonStateSerializer());
        var envelope = new SaveEnvelope<SampleState>
        {
            FormatVersion = 1,
            State = new SampleState { Name = "Republic" },
        };

        var path = await store.SaveAsync("state.json", envelope);
        var loaded = await store.LoadAsync<SampleState>(path);

        Assert.Equal("Republic", loaded.State?.Name);
        Assert.Equal(1, loaded.FormatVersion);
    }

    public sealed class SampleState
    {
        public string Name { get; set; } = string.Empty;
    }
}
