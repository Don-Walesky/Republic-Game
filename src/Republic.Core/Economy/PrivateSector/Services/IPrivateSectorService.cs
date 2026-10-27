namespace Republic.Core.Economy.PrivateSector.Services;

using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative service interface for managing the Republic private sector, corporate investors,
/// inbound investment proposals, and presidential investment decisions.
/// </summary>
public interface IPrivateSectorService
{
    /// <summary>
    /// Ensures standard commercial corporate investors exist for the sovereign country.
    /// </summary>
    IReadOnlyList<PrivateInvestor> EnsureCountryPrivateInvestors(Country country);

    /// <summary>
    /// Generates plausible commercial investment proposals tailored to country conditions.
    /// </summary>
    IReadOnlyList<InvestmentProposal> GenerateInvestmentProposals(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Processes an executive presidential decision on a private-sector investment proposal (accept, reject, or negotiate).
    /// </summary>
    bool ApplyInvestmentDecision(Country country, string proposalId, string decision, out string outcomeMessage);

    /// <summary>
    /// Checks all active private-sector investment projects and updates proposal/reporting statuses upon completion.
    /// </summary>
    int CheckInvestmentCompletions(Country country, RepublicTime simulationTime);
}
