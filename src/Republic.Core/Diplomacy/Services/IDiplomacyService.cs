namespace Republic.Core.Diplomacy.Services;

using Republic.Core.Diplomacy.Models;

/// <summary>
/// Service interface managing bilateral treaties, alliances, reputation, and summits.
/// </summary>
public interface IDiplomacyService
{
    DiplomaticRelation GetRelation(string countryA, string countryB);
    Task<DiplomaticTreaty> ProposeTreatyAsync(string sourceCountry, string targetCountry, TreatyType type, string title, CancellationToken cancellationToken = default);
    Task<bool> AcceptTreatyAsync(string treatyId, CancellationToken cancellationToken = default);
    Task<bool> BreakTreatyAsync(string treatyId, string violatorCountry, CancellationToken cancellationToken = default);
    void AdjustReputation(string countryA, string countryB, double delta);
    IReadOnlyList<DiplomaticTreaty> GetActiveTreatiesForCountry(string countryId);
}
