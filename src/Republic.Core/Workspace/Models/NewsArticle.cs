namespace Republic.Core.Workspace.Models;

/// <summary>
/// Represents a news article published to the executive workspace news feed.
/// </summary>
public sealed class NewsArticle
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Source { get; init; } = string.Empty;
    public string Headline { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Category { get; init; } = "Domestic";
    public DateTime DatePublished { get; init; } = DateTime.UtcNow;
    public int ImpactRating { get; init; } = 1;
}
