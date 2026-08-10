namespace Republic.Core.Demographics.Classes.Services;

using Republic.Core.Demographics.Classes.Models;

/// <summary>
/// Service interface managing socio-economic class approval ratings and weighted public sentiment.
/// </summary>
public interface IDemographicClassService
{
    IReadOnlyList<ClassApproval> GetClassApprovals();
    void AdjustClassApproval(DemographicClass classType, double delta);
    double GetWeightedOverallApproval();
}
