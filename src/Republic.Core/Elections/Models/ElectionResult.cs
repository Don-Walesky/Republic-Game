namespace Republic.Core.Elections.Models;

/// <summary>
/// Domain model summarizing presidential election vote outcomes and winner transition.
/// </summary>
public sealed class ElectionResult
{
    public long IncumbentVotes { get; init; }
    public long OppositionVotes { get; init; }
    public double TotalTurnoutPercentage { get; init; }
    public bool IsIncumbentReelected { get; init; }
    public string WinnerName { get; init; } = string.Empty;
}
