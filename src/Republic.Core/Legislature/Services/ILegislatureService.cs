namespace Republic.Core.Legislature.Services;

using Republic.Core.Legislature.Models;

/// <summary>
/// Service interface managing parliamentary parties, bill introductions, floor voting, and executive vetoes.
/// </summary>
public interface ILegislatureService
{
    IReadOnlyList<PoliticalParty> GetParties();
    void RegisterParty(PoliticalParty party);
    Task<Bill> IntroduceBillAsync(string title, string description, double requiredMajority = 50.0, CancellationToken cancellationToken = default);
    Task<VoteResult> VoteOnBillAsync(string billId, CancellationToken cancellationToken = default);
    Task<bool> ExerciseExecutiveVetoAsync(string billId, CancellationToken cancellationToken = default);
}
