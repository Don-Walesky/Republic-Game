namespace Republic.Unity.Bridge;

using Republic.Core.Decisions.Models;

/// <summary>
/// Bridge interface exposing executive decision prompts and decree updates to Unity UI views.
/// </summary>
public interface IDecisionPresenter
{
    void OnDecisionPrompted(DecisionContext decision);
    void OnDecisionExecuted(string decisionId, DecisionOption chosenOption);
    void OnDecreeEnacted(string decreeId, string title);
}
