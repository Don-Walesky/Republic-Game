namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Service implementing news publishing, category filtering, and impact scoring.
/// </summary>
public sealed class NewsService : INewsService
{
    private readonly List<NewsArticle> _articles = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public NewsService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void PublishArticle(NewsArticle article)
    {
        ArgumentNullException.ThrowIfNull(article);
        lock (_lock)
        {
            _articles.Add(article);
        }

        _logger.LogInfo($"News published [{article.Source}] '{article.Headline}' (Impact: {article.ImpactRating})");
        _eventBus.PublishAsync(new NewsArticlePublishedEvent(article, DateTimeOffset.UtcNow));
    }

    public IReadOnlyList<NewsArticle> GetNewsFeed(string? category = null)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return _articles.OrderByDescending(a => a.DatePublished).ToList().AsReadOnly();
            }

            return _articles
                .Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.DatePublished)
                .ToList()
                .AsReadOnly();
        }
    }

    public IReadOnlyList<NewsArticle> GetHighImpactNews(int minimumRating)
    {
        lock (_lock)
        {
            return _articles
                .Where(a => a.ImpactRating >= minimumRating)
                .OrderByDescending(a => a.ImpactRating)
                .ThenByDescending(a => a.DatePublished)
                .ToList()
                .AsReadOnly();
        }
    }
}
