namespace Republic.Core.World.Services;

using System.Collections.Generic;
using Republic.Core.Time;
using Republic.Core.Workspace.Services;
using Republic.Core.World.Models;

/// <summary>
/// Service contract governing flexible bilateral trade proposals, multi-round negotiations,
/// atomic execution with complete rollback, and recurring contract delivery enforcement.
/// </summary>
public interface IBilateralTradeService
{
    /// <summary>
    /// Creates and sends an outbound flexible bilateral trade proposal to a foreign sovereign nation.
    /// Delivers persistent notification through email and updates country proposal ledgers.
    /// </summary>
    (bool Success, string Message, FlexibleTradeTerms? Proposal) CreateProposal(
        Country proposer,
        Country target,
        FlexibleTradeTerms terms,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null);

    /// <summary>
    /// Revises an existing trade proposal with counter-terms.
    /// Invalidates previous approvals and requires the other party's explicit acceptance.
    /// </summary>
    (bool Success, string Message, FlexibleTradeTerms? Proposal) CounterProposal(
        Country actor,
        string proposalId,
        FlexibleTradeTerms counterTerms,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null);

    /// <summary>
    /// Validates and atomically executes an agreed trade deal or recurring contract cycle.
    /// Guarantees all-or-nothing execution with clean rollback on any shortfall.
    /// </summary>
    (bool Success, string Message, FlexibleTradeTerms? ExecutedDeal) AcceptProposal(
        Country actor,
        string proposalId,
        RepublicTime currentTime,
        IEnumerable<Country> worldCountries,
        IWorkspaceManager? workspaceManager = null);

    /// <summary>
    /// Declines or cancels a pending trade proposal without transferring any funds or goods.
    /// </summary>
    (bool Success, string Message, FlexibleTradeTerms? RejectedDeal) RejectProposal(
        Country actor,
        string proposalId,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null);

    /// <summary>
    /// Processes outstanding recurring delivery contracts due on the current simulation time boundary.
    /// </summary>
    IReadOnlyList<(string ContractId, bool Success, string Message)> ProcessRecurringContracts(
        Country country,
        RepublicTime currentTime,
        IEnumerable<Country> worldCountries,
        IWorkspaceManager? workspaceManager = null);

    /// <summary>
    /// Retrieves all active, pending, or historical flexible trade proposals involving the specified country.
    /// </summary>
    IReadOnlyList<FlexibleTradeTerms> GetProposalsForCountry(Country country);

    /// <summary>
    /// Retrieves currently active recurring trade contracts for the specified country.
    /// </summary>
    IReadOnlyList<FlexibleTradeTerms> GetActiveContractsForCountry(Country country);
}
