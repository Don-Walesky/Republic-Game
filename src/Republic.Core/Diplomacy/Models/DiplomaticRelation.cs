namespace Republic.Core.Diplomacy.Models;

/// <summary>
/// Domain model tracking bilateral diplomatic standing and active treaties between two nations.
/// </summary>
public sealed class DiplomaticRelation
{
    public string CountryIdA { get; init; } = string.Empty;
    public string CountryIdB { get; init; } = string.Empty;
    public DiplomaticStatus Status { get; set; } = DiplomaticStatus.Neutral;
    public double ReputationScore { get; set; } = 50.0;
    public List<DiplomaticTreaty> ActiveTreaties { get; set; } = new();
}
