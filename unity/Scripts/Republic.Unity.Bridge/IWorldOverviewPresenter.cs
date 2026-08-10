namespace Republic.Unity.Bridge;

using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Bridge interface exposing World metrics, stability shifts, and macro-economics to Unity UI inspectors and map views.
/// </summary>
public interface IWorldOverviewPresenter
{
    void OnWorldStateUpdated(WorldState worldState);
    void OnCountryStabilityChanged(string countryId, double newStability);
    void OnEconomicIndicatorsUpdated(EconomicIndicator indicators);
    void OnCrisisTriggered(string crisisTitle, string category, string severity);
}
