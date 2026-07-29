namespace Republic.Core.Tests;

using Republic.Core.Events;
using Republic.Core.World;

public sealed class WorldManagerTests
{
    [Fact]
    public async Task CreateAsync_InitializesWorldState()
    {
        var world = new WorldManager(new EventBus(new EventBusOptions(), new TestLogger()));

        var state = await world.CreateAsync("Test World");

        Assert.NotEqual(Guid.Empty, state.WorldId);
        Assert.Equal("Test World", state.Name);
    }

    [Fact]
    public async Task RegisterEntityAsync_AddsEntityToSnapshot()
    {
        var world = new WorldManager(new EventBus(new EventBusOptions(), new TestLogger()));
        await world.CreateAsync("Test World");

        var entity = await world.RegisterEntityAsync("Country", "Arcadia");

        Assert.Contains(world.GetEntities(), item => item.Id == entity.Id && item.Kind == "Country");
    }

    [Fact]
    public async Task RemoveEntity_RemovesExistingEntity()
    {
        var world = new WorldManager(new EventBus(new EventBusOptions(), new TestLogger()));
        await world.CreateAsync("Test World");
        var entity = await world.RegisterEntityAsync("Country", "Arcadia");

        var removed = world.RemoveEntity(entity.Id);

        Assert.True(removed);
        Assert.Empty(world.GetEntities());
    }

    [Fact]
    public async Task SnapshotAndRestore_RoundTripEntitiesAndTick()
    {
        var world = new WorldManager(new EventBus(new EventBusOptions(), new TestLogger()));
        await world.CreateAsync("Test World");
        await world.RegisterEntityAsync("Country", "Arcadia");
        world.AdvanceTo(42);
        var snapshot = world.Snapshot();
        var restored = new WorldManager(new EventBus(new EventBusOptions(), new TestLogger()));

        restored.Restore(snapshot);

        Assert.Equal<ulong>(42, restored.Current.CurrentTick);
        Assert.Single(restored.GetEntities());
    }
}
