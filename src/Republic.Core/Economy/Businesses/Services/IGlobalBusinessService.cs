namespace Republic.Core.Economy.Businesses.Services;

using Republic.Core.Economy.Businesses.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service coordinating the timed approach of influential global businesses and industrialists,
/// ministerial proposal presentation, presidential acceptance/decline decisions, and macroeconomic realization.
/// </summary>
public interface IGlobalBusinessService
{
    /// <summary>
    /// Gets all predefined First Wave and Second Wave global business proposals tailored for the specified country.
    /// </summary>
    IReadOnlyList<GlobalBusinessProposal> GetPredefinedProposals(Country country);

    /// <summary>
    /// Evaluates simulation time and advances the arrival of scheduled global business delegations.
    /// Wave 1 approaches at Day 30, 60, 90, 120, 150 of the presidency.
    /// Wave 2 approaches at Day 30, 60, 90, 120, 150 after national currency management creation.
    /// </summary>
    IReadOnlyList<GlobalBusinessProposal> UpdateBusinessApproaches(Country country, RepublicTime simTime, bool forceUnlockAll = false);

    /// <summary>
    /// Accepts an inbound global enterprise proposal, authorizing operations and realizing ongoing economic dividends.
    /// </summary>
    bool AcceptProposal(Country country, string businessId, RepublicTime simTime, out string outcomeMessage);

    /// <summary>
    /// Declines an inbound global enterprise proposal, turning away the delegation without ongoing economic benefits.
    /// </summary>
    bool DeclineProposal(Country country, string businessId, RepublicTime simTime, out string outcomeMessage);
}

