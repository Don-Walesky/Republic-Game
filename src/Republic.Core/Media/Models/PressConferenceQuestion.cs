namespace Republic.Core.Media.Models;

/// <summary>
/// Domain model representing a journalist's question during an executive press conference.
/// </summary>
public sealed class PressConferenceQuestion
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string JournalistName { get; init; } = string.Empty;
    public string NewsOutlet { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string QuestionText { get; init; } = string.Empty;
    public List<PressResponseOption> Options { get; init; } = new();
}

public sealed class PressResponseOption
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string ResponseText { get; init; } = string.Empty;
    public double PublicApprovalDelta { get; init; }
    public double MediaSentimentDelta { get; init; }
}
