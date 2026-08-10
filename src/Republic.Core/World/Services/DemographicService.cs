namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for population growth, literacy, employment, and satisfaction.
/// </summary>
public sealed class DemographicService : IDemographicService
{
    private readonly Demographics _demographics = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;

    public DemographicService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public Demographics GetDemographics()
    {
        return _demographics;
    }

    public void UpdatePopulation(long newTotal)
    {
        _demographics.TotalPopulation = Math.Max(0, newTotal);
        _logger?.LogInfo($"Demographics population updated to {_demographics.TotalPopulation:N0}");
        _eventBus.PublishAsync(new DemographicUpdatedEvent(_demographics, DateTimeOffset.UtcNow));
    }

    public void UpdateHappiness(double newRating)
    {
        _demographics.HappinessRating = Math.Clamp(newRating, 0.0, 100.0);
        _logger?.LogInfo($"Demographics happiness updated to {_demographics.HappinessRating:0.0}%");
        _eventBus.PublishAsync(new DemographicUpdatedEvent(_demographics, DateTimeOffset.UtcNow));
    }

    public void AdvanceDemographicsTick()
    {
        // Baseline organic growth tick update
        var netGrowth = (long)(_demographics.TotalPopulation * (_demographics.GrowthRate / 365.0));
        _demographics.TotalPopulation += netGrowth;
    }
}
