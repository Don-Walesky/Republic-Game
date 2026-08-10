namespace Republic.Core.Legislature.Models;

/// <summary>
/// Domain model representing a parliamentary political party holding seats in the assembly.
/// </summary>
public sealed class PoliticalParty
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public int SeatCount { get; set; } = 50;
    public string Ideology { get; init; } = "Centrist";
    public bool IsGovernmentCoalition { get; set; } = true;
}
