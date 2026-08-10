namespace Republic.Core.Tests.Intelligence;

using Republic.Core.Events;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;
using Republic.Core.Workspace.Services;

public sealed class IntelligenceServiceTests
{
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;

    public IntelligenceServiceTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Intel Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
    }

    [Fact]
    public async Task InfiltrateTarget_IncreasesInfiltrationLevel()
    {
        var service = new IntelligenceService(_world, _eventBus, _workspace, _logger);
        var network = await service.InfiltrateTargetAsync("RivalState", 3);

        Assert.Equal(55.0, network.InfiltrationLevel);
        Assert.Equal(6, network.AssignedAgentsCount);
    }

    [Fact]
    public async Task LaunchOperation_HighInfiltration_SucceedsCleanly()
    {
        var service = new IntelligenceService(_world, _eventBus, _workspace, _logger);
        await service.InfiltrateTargetAsync("TargetNation", 3); // 55% infiltration

        var op = await service.LaunchOperationAsync(CovertOperationType.IndustrialSabotage, "TargetNation", "Operation Blackout");

        Assert.True(op.IsCompleted);
        Assert.False(op.IsExposed);
        Assert.NotEmpty(_workspace.Email.GetInbox());
    }
}
