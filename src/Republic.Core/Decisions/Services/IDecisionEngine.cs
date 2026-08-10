namespace Republic.Core.Decisions.Services;

using Republic.Core.Decisions.Models;

/// <summary>
/// Service interface evaluating crises, executive options, and applying policy effects.
/// </summary>
public interface IDecisionEngine
{
    void RegisterDecision(DecisionContext context);
    DecisionContext? GetDecision(string decisionId);
    IReadOnlyList<DecisionContext> GetPendingDecisions();
    Task<bool> ExecuteDecisionAsync(string decisionId, string optionId, CancellationToken cancellationToken = default);
    Task DirectEnactPolicyAsync(string title, List<PolicyEffect> effects, CancellationToken cancellationToken = default);
}
