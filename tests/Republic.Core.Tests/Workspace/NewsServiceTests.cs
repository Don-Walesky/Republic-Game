namespace Republic.Core.Tests.Workspace;

using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class NewsServiceTests
{
    [Fact]
    public async Task PublishArticle_EmitsEventAndFiltersHighImpact()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var newsService = new NewsService(bus, logger);

        NewsArticlePublishedEvent? publishedEvent = null;
        bus.Subscribe<NewsArticlePublishedEvent>((e, _) =>
        {
            publishedEvent = e;
            return ValueTask.CompletedTask;
        });

        newsService.PublishArticle(new NewsArticle
        {
            Source = "Global News Network",
            Headline = "Minor Regional Trade Accord Signed",
            Category = "Foreign",
            ImpactRating = 2
        });

        newsService.PublishArticle(new NewsArticle
        {
            Source = "Republic Gazette",
            Headline = "Major Industrial Energy Shortage Declared",
            Category = "Domestic",
            ImpactRating = 5
        });

        await bus.ProcessQueuedEventsAsync();

        Assert.NotNull(publishedEvent);
        Assert.Equal(2, newsService.GetNewsFeed().Count);

        var highImpact = newsService.GetHighImpactNews(4);
        Assert.Single(highImpact);
        Assert.Equal("Major Industrial Energy Shortage Declared", highImpact[0].Headline);
    }
}
