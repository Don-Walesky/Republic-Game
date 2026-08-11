namespace Republic.Core.Government.Services;

using Republic.Core.Government.Models;

/// <summary>
/// Service interface managing constitutional conventions, government system overhauls, and civil liberties.
/// </summary>
public interface IGovernmentReformService
{
    GovernmentType GetCurrentGovernmentSystem();
    Task<bool> EnactConstitutionalReformAsync(ConstitutionalReform reform, CancellationToken cancellationToken = default);
    Task<ConstitutionalAmendment> ProposeConstitutionalAmendmentAsync(ConstitutionalAmendment amendment, CancellationToken cancellationToken = default);
    Task<bool> VoteOnConstitutionalAmendmentAsync(string amendmentId, int votesInFavor, int totalVotesCast, CancellationToken cancellationToken = default);
    IReadOnlyList<ConstitutionalAmendment> GetConstitutionalAmendments();
}
