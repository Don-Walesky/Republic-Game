namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for population demographics and public sentiment.
/// </summary>
public interface IDemographicService
{
    Demographics GetDemographics();
    void UpdatePopulation(long newTotal);
    void UpdateHappiness(double newRating);
    void AdvanceDemographicsTick();
}
