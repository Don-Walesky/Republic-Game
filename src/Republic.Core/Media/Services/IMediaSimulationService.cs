namespace Republic.Core.Media.Services;

using Republic.Core.Media.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface governing the procedural media ecosystem, report publishing, and executive media management.
/// </summary>
public interface IMediaSimulationService
{
    /// <summary>
    /// Ensures that the specified country has a diverse set of procedural media organisations.
    /// </summary>
    IReadOnlyList<MediaOutlet> EnsureCountryMediaOutlets(Country country);

    /// <summary>
    /// Generates and publishes news reports reflecting actual country events and conditions.
    /// Applies balanced public approval and stability impacts.
    /// </summary>
    IReadOnlyList<MediaReport> GenerateMediaReports(Country country, RepublicTime simulationTime, bool applySimulationImpacts = true);

    /// <summary>
    /// Enacts a presidential media-management action on a specific media report.
    /// Supported response types: "statement", "hush_money", "censorship".
    /// </summary>
    bool ApplyMediaResponse(Country country, string reportId, string responseType, out string outcomeMessage);
}
