namespace Republic.Core.Tests.AI;

using Republic.Core.AI.News;
using Xunit;

public sealed class NewsArticleGeneratorTests
{
    [Fact]
    public void GenerateArticleFromDecision_ComposesHeadlineAndSummary()
    {
        var generator = new NewsArticleGenerator();

        var article = generator.GenerateArticleFromDecision("Infrastructure Reform", "Approve $50M Grant", 75.0);

        Assert.NotNull(article);
        Assert.Contains("INFRASTRUCTURE REFORM", article.Headline);
        Assert.Contains("Approve $50M Grant", article.Summary);
        Assert.Equal("Politics", article.Category);
    }

    [Fact]
    public void GenerateArticleFromMilitaryDirective_ComposesMilitaryDispatch()
    {
        var generator = new NewsArticleGenerator();

        var article = generator.GenerateArticleFromMilitaryDirective("Valoria", "CyberAttack", true);

        Assert.NotNull(article);
        Assert.Contains("VALORIA", article.Headline);
        Assert.Equal("Military", article.Category);
    }
}
