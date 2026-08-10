namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Service interface for national and international news feeds.
/// </summary>
public interface INewsService
{
    void PublishArticle(NewsArticle article);
    IReadOnlyList<NewsArticle> GetNewsFeed(string? category = null);
    IReadOnlyList<NewsArticle> GetHighImpactNews(int minimumRating);
}
