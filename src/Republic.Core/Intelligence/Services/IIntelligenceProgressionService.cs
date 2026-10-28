namespace Republic.Core.Intelligence.Services;

using System.Collections.Generic;
using Republic.Core.Intelligence.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service contract managing military level-gated espionage progression, operation lifecycle,
/// uncertain outcome resolution, counterintelligence defenses, and diplomatic fallout.
/// </summary>
public interface IIntelligenceProgressionService
{
    /// <summary>
    /// Gets the configurable progression thresholds.
    /// </summary>
    IntelligenceProgressionConfig Config { get; set; }

    /// <summary>
    /// Evaluates whether the country meets all military level, readiness, and equipment prerequisites
    /// to conduct the specified intelligence operation type.
    /// </summary>
    (bool IsUnlocked, string Reason, int RequiredLevel) CheckOperationPrerequisites(
        Country initiator,
        IntelligenceOperationType operationType);

    /// <summary>
    /// Plans and launches an authoritative intelligence operation targeting a peer foreign power.
    /// Deducts operational treasury costs and enforces level thresholds and country isolation.
    /// </summary>
    (bool Success, string Message, IntelligenceOperation? Operation) LaunchOperation(
        Country initiator,
        Country target,
        IntelligenceOperationType operationType,
        string title,
        RepublicTime simulationTime,
        double? customCost = null);

    /// <summary>
    /// Resolves an in-progress or scheduled operation, calculating uncertain outcome probability
    /// against target counterintelligence, generating intelligence dossiers, and applying diplomatic fallout if detected.
    /// </summary>
    (bool Success, string Message, IntelligenceOperation Operation) ResolveOperation(
        IntelligenceOperation operation,
        Country initiator,
        Country target,
        RepublicTime simulationTime,
        double? forcedSuccessRoll = null,
        double? forcedDetectionRoll = null);

    /// <summary>
    /// Conducts a domestic counterintelligence sweep, heightening domestic defenses and detecting hostile foreign assets.
    /// </summary>
    (bool Success, string Message, int ThreatsNeutralized) ConductCounterintelligenceSweep(
        Country country,
        RepublicTime simulationTime,
        IEnumerable<Country>? allWorldCountries = null);

    /// <summary>
    /// Evaluates and degrades the freshness of all acquired foreign intelligence dossiers over time.
    /// </summary>
    void DegradeIntelligenceFreshness(Country country, RepublicTime simulationTime);
}
