namespace Republic.Core.Media.Models;

/// <summary>
/// Domain model representing an accredited press corps journalist.
/// </summary>
public sealed class Journalist
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string NewsOutlet { get; init; } = string.Empty;
    public double BiasRating { get; set; } = 0.0; // -1.0 hostile to +1.0 supportive
}
