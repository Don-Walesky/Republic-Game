namespace Republic.Core.Legislature.Models;

/// <summary>
/// Domain model summarizing a parliamentary vote outcome.
/// </summary>
public sealed class VoteResult
{
    public string BillId { get; init; } = string.Empty;
    public int TotalVotes { get; init; }
    public int AyesCount { get; init; }
    public int NaysCount { get; init; }
    public int AbstentionsCount { get; init; }
    public bool Passed { get; init; }
}
