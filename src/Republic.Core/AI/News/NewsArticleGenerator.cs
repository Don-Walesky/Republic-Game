namespace Republic.Core.AI.News;

using System;
using Republic.Core.Workspace.Models;

/// <summary>
/// Generator dynamically composing news press articles from presidential directives and state metrics.
/// </summary>
public sealed class NewsArticleGenerator
{
    public NewsArticle GenerateArticleFromDecision(string decisionTitle, string chosenOptionLabel, double publicHappiness)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(chosenOptionLabel);

        string tone = publicHappiness >= 60.0 ? "PRAISE" : "CRITICISM";
        string headline = $"EXECUTIVE DIRECTIVE: PRESIDENT SIGNS '{decisionTitle.ToUpper()}'";
        string summary = $"In a major policy move, the administration enacted '{chosenOptionLabel}' regarding '{decisionTitle}'. Public response indicates mixed sentiment ({publicHappiness:0.0}% approval).";

        return new NewsArticle
        {
            Source = "The National Chronicle",
            Headline = headline,
            Summary = summary,
            Category = "Politics",
            ImpactRating = publicHappiness < 40.0 ? 5 : 3
        };
    }

    public NewsArticle GenerateArticleFromMilitaryDirective(string targetCountry, string operationType, bool success)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountry);

        string status = success ? "SUCCESSFUL" : "COMPROMISED";
        string headline = $"MILITARY ACTION: OPERATION AGAINST {targetCountry.ToUpper()} {status}";
        string summary = $"High Command reported the conclusion of military operation '{operationType}' in the {targetCountry} sector with {status.ToLower()} outcome.";

        return new NewsArticle
        {
            Source = "Global Defense Dispatch",
            Headline = headline,
            Summary = summary,
            Category = "Military",
            ImpactRating = 5
        };
    }
}
