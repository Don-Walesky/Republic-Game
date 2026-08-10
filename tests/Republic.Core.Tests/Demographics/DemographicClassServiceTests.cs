namespace Republic.Core.Tests.Demographics;

using Republic.Core.Demographics.Classes.Models;
using Republic.Core.Demographics.Classes.Services;
using Republic.Core.Events;
using Republic.Core.World;

public sealed class DemographicClassServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;

    public DemographicClassServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Demo Class World").GetAwaiter().GetResult();
    }

    [Fact]
    public void AdjustClassApproval_UpdatesTargetRatingAndWeightedSum()
    {
        var service = new DemographicClassService(_world, _eventBus, _logger);
        var initialWeighted = service.GetWeightedOverallApproval();

        service.AdjustClassApproval(DemographicClass.WorkingClass, -20.0);

        var newWeighted = service.GetWeightedOverallApproval();
        Assert.True(newWeighted < initialWeighted);
    }
}
