namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing a political interest group or party faction.
/// </summary>
public sealed class Faction
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string Ideology { get; init; } = "Centrist";
    public double InfluencePercentage { get; set; } = 25.0;
    public double ApprovalRating { get; set; } = 55.0;
    public double PolarizationLevel { get; set; } = 30.0;
}
