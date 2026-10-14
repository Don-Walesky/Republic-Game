namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Status outcome classifications for bilateral diplomatic interactions.
/// </summary>
public enum DiplomaticActionResult
{
    Success,
    SelfRelationProhibited,
    TargetNotFound,
    AlreadyExists,
    InvalidInput,
    Cancelled,
    RelationNotFound,
    PrerequisitesNotMet,
    AgreementNotFound,
    AlreadyDecided,
    InsufficientFunds,
    InsufficientGoods
}

/// <summary>
/// Domain outcome record returned from an interactive bilateral diplomatic action.
/// </summary>
public sealed record DiplomaticInteractionOutcome(
    DiplomaticActionResult Result,
    string Message,
    DiplomaticRelation? Relation = null,
    DiplomaticAgreement? Agreement = null)
{
    /// <summary>
    /// Indicates whether the diplomatic interaction was successfully executed.
    /// </summary>
    public bool IsSuccess => Result == DiplomaticActionResult.Success;
}

/// <summary>
/// Domain handler coordinating interactive bilateral diplomacy between sovereign nations.
/// Enforces registry verification, self-relation prohibitions, and deterministic time recording.
/// </summary>
public static class BilateralDiplomacyHandler
{
    /// <summary>
    /// Filters the available countries to retrieve only eligible foreign nations.
    /// </summary>
    public static IReadOnlyList<Country> GetEligibleTargets(Country playerCountry, IEnumerable<Country> allCountries)
    {
        ArgumentNullException.ThrowIfNull(playerCountry);
        ArgumentNullException.ThrowIfNull(allCountries);

        return allCountries
            .Where(c => !string.Equals(c.Id, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified candidate country is an eligible diplomatic target for the player country.
    /// </summary>
    public static bool IsEligibleTarget(Country playerCountry, Country? targetCountry)
    {
        if (playerCountry == null || targetCountry == null)
        {
            return false;
        }

        return !string.Equals(playerCountry.Id, targetCountry.Id, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates and resolves a 1-based index selection from the list of registered countries.
    /// Prevents selection of the player's own nation.
    /// </summary>
    public static (bool Cancelled, Country? SelectedCountry, string? ErrorMessage) SelectTarget(
        int selectionIndex,
        IReadOnlyList<Country> availableCountries,
        Country playerCountry)
    {
        ArgumentNullException.ThrowIfNull(availableCountries);
        ArgumentNullException.ThrowIfNull(playerCountry);

        if (selectionIndex == 0)
        {
            return (true, null, "Diplomatic selection cancelled.");
        }

        if (selectionIndex < 1 || selectionIndex > availableCountries.Count)
        {
            return (false, null, $"Invalid selection number '{selectionIndex}'. Please enter a valid country number (1-{availableCountries.Count}).");
        }

        var candidate = availableCountries[selectionIndex - 1];

        if (string.Equals(candidate.Id, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
        {
            return (false, null, $"Cannot establish diplomatic relations with your own nation ('{playerCountry.Name}'). Self-relations are prohibited.");
        }

        return (false, candidate, null);
    }

    /// <summary>
    /// Executes the opening of a bilateral diplomatic relation from the player country to a target country.
    /// Confirms the target exists in the world registry, prevents self-relations and duplicates,
    /// and anchors the relation strictly to the simulation clock.
    /// </summary>
    public static DiplomaticInteractionOutcome OpenRelation(
        Country playerCountry,
        string? targetCountryId,
        IEnumerable<Country> worldCountries,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(playerCountry);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Target country identifier cannot be null or empty.");
        }

        var trimmedTargetId = targetCountryId.Trim();

        // 1. Prevent self-relations
        if (string.Equals(trimmedTargetId, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.SelfRelationProhibited,
                $"Cannot establish diplomatic relations with your own nation ('{playerCountry.Name}'). Self-relations are prohibited.");
        }

        // 2. Confirm target country actually exists in the world
        var targetCountry = worldCountries.FirstOrDefault(c => string.Equals(c.Id, trimmedTargetId, StringComparison.OrdinalIgnoreCase));
        if (targetCountry == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.TargetNotFound,
                $"Target country with ID '{trimmedTargetId}' does not exist in the world registry.");
        }

        // 3. Prevent duplicate relations in the same direction
        if (playerCountry.HasRelation(trimmedTargetId))
        {
            var existing = playerCountry.GetRelation(trimmedTargetId);
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.AlreadyExists,
                $"A diplomatic relation from '{playerCountry.Name}' to '{targetCountry.Name}' already exists (Status: {existing?.Status}, Opened: {existing?.OpenedBoundary}).",
                existing);
        }

        // 4. Open relation using existing Country.OpenRelation domain logic
        var newRelation = playerCountry.OpenRelation(targetCountry.Id, currentTime);
        if (newRelation != null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Successfully opened diplomatic relation from '{playerCountry.Name}' to '{targetCountry.Name}'.",
                newRelation);
        }

        // Fallback for domain rejection
        return new DiplomaticInteractionOutcome(
            DiplomaticActionResult.AlreadyExists,
            $"Diplomatic relation request toward '{targetCountry.Name}' was rejected by domain policy.",
            playerCountry.GetRelation(trimmedTargetId));
    }

    /// <summary>
    /// Executes the formal diplomatic recognition of an existing relation opened from the player country toward the target country.
    /// Confirms target exists, prevents self-action, verifies existing relation exists, and updates stored relation state.
    /// </summary>
    public static DiplomaticInteractionOutcome RecognizeRelation(
        Country playerCountry,
        string? targetCountryId,
        IEnumerable<Country> worldCountries,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(playerCountry);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Target country identifier cannot be null or empty.");
        }

        var trimmedTargetId = targetCountryId.Trim();

        if (string.Equals(trimmedTargetId, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.SelfRelationProhibited,
                $"Cannot recognize diplomatic relations with your own nation ('{playerCountry.Name}'). Self-relations are prohibited.");
        }

        var targetCountry = worldCountries.FirstOrDefault(c => string.Equals(c.Id, trimmedTargetId, StringComparison.OrdinalIgnoreCase));
        if (targetCountry == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.TargetNotFound,
                $"Target country with ID '{trimmedTargetId}' does not exist in the world registry.");
        }

        var existing = playerCountry.GetRelation(trimmedTargetId);
        if (existing == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.RelationNotFound,
                $"Cannot recognize diplomatic relation with '{targetCountry.Name}': No relation has been opened yet. Open a relation first.");
        }

        if (existing.IsRecognized)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Diplomatic relation from '{playerCountry.Name}' to '{targetCountry.Name}' is already formally recognized.",
                existing);
        }

        var recognized = playerCountry.RecognizeRelation(trimmedTargetId, currentTime);
        if (recognized != null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Formally recognized diplomatic relation from '{playerCountry.Name}' to '{targetCountry.Name}'.",
                recognized);
        }

        return new DiplomaticInteractionOutcome(
            DiplomaticActionResult.InvalidInput,
            $"Recognition request toward '{targetCountry.Name}' was rejected by domain policy.");
    }

    /// <summary>
    /// Proposes a bilateral diplomatic agreement (Trade or Alliance) from the player country to a target country.
    /// Requires an open relation (and for alliances, formal recognition). Prevents duplicates and records simulation time.
    /// </summary>
    public static DiplomaticInteractionOutcome ProposeAgreement(
        Country playerCountry,
        string? targetCountryId,
        DiplomaticAgreementType type,
        IEnumerable<Country> worldCountries,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(playerCountry);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Target country identifier cannot be null or empty.");
        }

        var trimmedTargetId = targetCountryId.Trim();

        if (string.Equals(trimmedTargetId, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.SelfRelationProhibited,
                $"Cannot conclude diplomatic agreements with your own nation ('{playerCountry.Name}').");
        }

        var targetCountry = worldCountries.FirstOrDefault(c => string.Equals(c.Id, trimmedTargetId, StringComparison.OrdinalIgnoreCase));
        if (targetCountry == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.TargetNotFound,
                $"Target country with ID '{trimmedTargetId}' does not exist in the world registry.");
        }

        var relation = playerCountry.GetRelation(trimmedTargetId);
        if (relation == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.RelationNotFound,
                $"Cannot propose {type.ToString().ToLowerInvariant()} agreement to '{targetCountry.Name}': No diplomatic relation exists. Open a relation first.");
        }

        if (type == DiplomaticAgreementType.Alliance && !relation.IsRecognized)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.PrerequisitesNotMet,
                $"Cannot propose a bilateral alliance to '{targetCountry.Name}': Diplomatic relation must be formally recognized before an alliance can be concluded.");
        }

        var existingAgreement = playerCountry.GetAgreementWith(trimmedTargetId, type);
        if (existingAgreement != null && (existingAgreement.IsActive || existingAgreement.IsPending))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.AlreadyExists,
                $"An active or pending {type.ToString().ToLowerInvariant()} agreement already exists between '{playerCountry.Name}' and '{targetCountry.Name}' (Status: {existingAgreement.Status}).",
                Agreement: existingAgreement);
        }

        var agreement = playerCountry.ProposeAgreement(targetCountry, type, currentTime);
        if (agreement != null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Successfully proposed bilateral {type.ToString().ToLowerInvariant()} agreement to '{targetCountry.Name}'.",
                Agreement: agreement);
        }

        return new DiplomaticInteractionOutcome(
            DiplomaticActionResult.InvalidInput,
            $"Bilateral {type.ToString().ToLowerInvariant()} proposal to '{targetCountry.Name}' was rejected by domain policy.");
    }

    /// <summary>
    /// Proposes a bilateral trade deal transaction with explicit economic terms from the player country to a target country.
    /// Requires an open relation, validates explicit terms, prevents duplicate pending deals, and records simulation time.
    /// </summary>
    public static DiplomaticInteractionOutcome ProposeTradeDeal(
        Country playerCountry,
        string? targetCountryId,
        BilateralTradeTerms terms,
        IEnumerable<Country> worldCountries,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(playerCountry);
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Target country identifier cannot be null or empty.");
        }

        var trimmedTargetId = targetCountryId.Trim();

        if (string.Equals(trimmedTargetId, playerCountry.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.SelfRelationProhibited,
                $"Cannot execute trade deals with your own nation ('{playerCountry.Name}').");
        }

        var targetCountry = worldCountries.FirstOrDefault(c => string.Equals(c.Id, trimmedTargetId, StringComparison.OrdinalIgnoreCase));
        if (targetCountry == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.TargetNotFound,
                $"Target country with ID '{trimmedTargetId}' does not exist in the world registry.");
        }

        var relation = playerCountry.GetRelation(trimmedTargetId);
        if (relation == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.RelationNotFound,
                $"Cannot propose trade deal to '{targetCountry.Name}': No diplomatic relation exists. Open a relation first.");
        }

        if (!terms.Involves(playerCountry.Id) || !terms.Involves(trimmedTargetId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Trade deal terms must specifically involve the player country and the target country.");
        }

        var existingPending = playerCountry.Agreements
            .FirstOrDefault(a => a.AgreementType == DiplomaticAgreementType.Trade && a.Involves(trimmedTargetId) && a.IsPending);
        if (existingPending != null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.AlreadyExists,
                $"A pending trade proposal already exists between '{playerCountry.Name}' and '{targetCountry.Name}'. Resolve it first before proposing another deal.",
                Agreement: existingPending);
        }

        var deal = playerCountry.ProposeTradeDeal(targetCountry, terms, currentTime);
        if (deal != null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Successfully proposed bilateral trade deal to '{targetCountry.Name}': {terms}.",
                Agreement: deal);
        }

        return new DiplomaticInteractionOutcome(
            DiplomaticActionResult.InvalidInput,
            $"Bilateral trade deal proposal to '{targetCountry.Name}' was rejected by domain policy.");
    }

    /// <summary>
    /// Executes a deterministic decision (Accept or Reject) on a pending bilateral diplomatic agreement or trade deal.
    /// When accepting a trade deal with explicit economic terms, validates buyer funds and seller goods,
    /// executes the atomic transfer of REPU and commodities, updates state on both countries, and records the decision boundary.
    /// </summary>
    public static DiplomaticInteractionOutcome RespondToAgreement(
        Country respondingCountry,
        string? agreementId,
        bool accept,
        IEnumerable<Country> worldCountries,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(respondingCountry);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(agreementId))
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.InvalidInput,
                "Agreement identifier cannot be null or empty.");
        }

        var agreement = respondingCountry.GetAgreement(agreementId.Trim());
        if (agreement == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.AgreementNotFound,
                $"Agreement with ID '{agreementId}' was not found on '{respondingCountry.Name}'.");
        }

        if (agreement.Status != DiplomaticAgreementStatus.Pending)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.AlreadyDecided,
                $"Agreement '{agreement.AgreementType}' has already been decided (Status: {agreement.Status}, Decided: {agreement.DecidedBoundary}).",
                Agreement: agreement);
        }

        // Find the other country involved in this agreement
        var otherCountryId = string.Equals(agreement.SourceCountryId, respondingCountry.Id, StringComparison.OrdinalIgnoreCase)
            ? agreement.TargetCountryId
            : agreement.SourceCountryId;

        var otherCountry = worldCountries.FirstOrDefault(c => string.Equals(c.Id, otherCountryId, StringComparison.OrdinalIgnoreCase));
        if (otherCountry == null)
        {
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.TargetNotFound,
                $"Partner country with ID '{otherCountryId}' does not exist in the world registry.");
        }

        // Validation for trade deals with explicit terms when accepting
        if (agreement.Terms != null && accept)
        {
            var terms = agreement.Terms;
            var buyer = string.Equals(respondingCountry.Id, terms.BuyerCountryId, StringComparison.OrdinalIgnoreCase)
                ? respondingCountry
                : (string.Equals(otherCountry.Id, terms.BuyerCountryId, StringComparison.OrdinalIgnoreCase) ? otherCountry : null);

            var seller = string.Equals(respondingCountry.Id, terms.SellerCountryId, StringComparison.OrdinalIgnoreCase)
                ? respondingCountry
                : (string.Equals(otherCountry.Id, terms.SellerCountryId, StringComparison.OrdinalIgnoreCase) ? otherCountry : null);

            if (buyer == null || seller == null)
            {
                return new DiplomaticInteractionOutcome(
                    DiplomaticActionResult.InvalidInput,
                    "Trade deal terms reference countries that do not match the agreement parties.");
            }

            if (buyer.Treasury.Balance < terms.Price)
            {
                return new DiplomaticInteractionOutcome(
                    DiplomaticActionResult.InsufficientFunds,
                    $"Trade deal execution rejected: Buyer '{buyer.Name}' has insufficient REPU. Required: {terms.FormattedPrice}, Available: {buyer.Treasury.FormattedBalance}.",
                    Agreement: agreement);
            }

            if (!seller.Inventory.HasSufficient(terms.ResourceType, terms.Quantity))
            {
                return new DiplomaticInteractionOutcome(
                    DiplomaticActionResult.InsufficientGoods,
                    $"Trade deal execution rejected: Seller '{seller.Name}' has insufficient {terms.ResourceType}. Required: {terms.Quantity:N0} units, Available: {seller.Inventory.GetQuantity(terms.ResourceType):N0} units.",
                    Agreement: agreement);
            }
        }

        var decided = respondingCountry.RespondToAgreement(agreement.Id, accept, currentTime, otherCountry);
        if (decided != null)
        {
            if (decided.Terms != null && accept)
            {
                return new DiplomaticInteractionOutcome(
                    DiplomaticActionResult.Success,
                    $"Bilateral trade deal successfully executed at {currentTime}: Transferred {decided.Terms.Quantity:N0} units of {decided.Terms.ResourceType} for {decided.Terms.FormattedPrice}.",
                    Agreement: decided);
            }

            if (decided.Terms != null && !accept)
            {
                return new DiplomaticInteractionOutcome(
                    DiplomaticActionResult.Success,
                    $"Bilateral trade deal proposal was rejected at {currentTime}. No funds or goods were transferred.",
                    Agreement: decided);
            }

            var actionText = accept ? "accepted" : "rejected";
            return new DiplomaticInteractionOutcome(
                DiplomaticActionResult.Success,
                $"Bilateral {decided.AgreementType.ToString().ToLowerInvariant()} agreement proposal was {actionText} at {currentTime}.",
                Agreement: decided);
        }

        return new DiplomaticInteractionOutcome(
            DiplomaticActionResult.InvalidInput,
            "Failed to resolve agreement proposal.");
    }
}
