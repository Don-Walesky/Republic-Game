namespace Republic.Unity.Bridge;

using Republic.Core.Intelligence.Models;

/// <summary>
/// Bridge interface exposing intelligence ops, covert infiltration, and counter-espionage alerts to Unity views.
/// </summary>
public interface IIntelligencePresenter
{
    void OnIntelligenceInfiltrated(string targetCountryId, int spyLevel);
    void OnCovertOperationCompleted(string operationName, bool success, string details);
    void OnThreatLevelEscalated(string regionOrCountry, double threatScore);
}
