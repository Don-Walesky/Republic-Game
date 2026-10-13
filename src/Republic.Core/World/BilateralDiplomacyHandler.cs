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
    Cancelled
}

/// <summary>
/// Domain outcome record returned from an interactive bilateral diplomatic action.
/// </summary>
public sealed record DiplomaticInteractionOutcome(
    DiplomaticActionResult Result,
    string Message,
    DiplomaticRelation? Relation = null)
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
}
