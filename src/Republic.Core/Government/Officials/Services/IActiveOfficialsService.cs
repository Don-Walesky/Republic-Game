namespace Republic.Core.Government.Officials.Services;

using Republic.Core.Cabinet.Models;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface managing proactive government officials, the Chief of Staff recommendation engine,
/// distinct ministerial portfolio contributions, and security leadership threat reporting.
/// </summary>
public interface IActiveOfficialsService
{
    /// <summary>
    /// Evaluates current sovereign conditions and updates proactive recommendations from the Chief of Staff.
    /// Tracks accepted, rejected, and unresolved recommendations across simulation cycles.
    /// </summary>
    List<ChiefOfStaffRecommendation> EvaluateChiefOfStaffRecommendations(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Generates distinct, portfolio-specific advice for each of the six executive ministries,
    /// weighted by minister competence, loyalty, and integrity ratings.
    /// </summary>
    List<MinisterialAdvice> GenerateMinisterialAdvice(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Generates a comprehensive national security leadership assessment,
    /// distinguishing confirmed intelligence from uncertain signals and recommending proportionate responses.
    /// </summary>
    SecurityLeadershipAssessment EvaluateSecurityLeadership(Country country, RepublicTime simulationTime);

    /// <summary>
    /// Applies a player's decision to accept, reject, or defer a Chief of Staff recommendation.
    /// </summary>
    bool ApplyRecommendationDecision(Country country, string recommendationId, string decision, out string outcomeMessage);

    /// <summary>
    /// Applies a presidential directive in response to a reported security threat.
    /// </summary>
    bool ApplySecurityDirective(Country country, string threatId, string directive, out string outcomeMessage);
}
