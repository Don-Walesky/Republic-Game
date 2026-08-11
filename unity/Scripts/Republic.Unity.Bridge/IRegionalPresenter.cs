namespace Republic.Unity.Bridge;

using System.Collections.Generic;
using Republic.Core.World.Models;

/// <summary>
/// Bridge interface exposing provincial terrain, stability ratings, and regional infrastructure events to Unity map views.
/// </summary>
public interface IRegionalPresenter
{
    void OnProvincialListUpdated(IReadOnlyList<ProvinceState> provinces);
    void OnProvinceStabilityChanged(string provinceId, string provinceName, double newStability);
    void OnRegionalInfrastructureBuilt(string provinceId, string provinceName, double newInfrastructureIndex);
    void OnRebellionRiskElevated(string provinceId, string provinceName, double riskLevel);
}
