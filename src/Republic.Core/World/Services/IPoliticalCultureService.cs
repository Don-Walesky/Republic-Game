namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for political factions, ideology distribution, and constitution.
/// </summary>
public interface IPoliticalCultureService
{
    Faction RegisterFaction(Faction faction);
    IReadOnlyList<Faction> GetFactions();
    bool UpdateApproval(string factionId, double newApproval);
    Constitution GetConstitution();
}
