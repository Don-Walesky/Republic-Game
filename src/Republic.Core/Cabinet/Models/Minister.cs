namespace Republic.Core.Cabinet.Models;

/// <summary>
/// Domain model representing an appointed executive cabinet minister.
/// </summary>
public sealed class Minister
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public CabinetPortfolio Portfolio { get; set; }
    public double LoyaltyRating { get; set; } = 75.0; // 0.0 to 100.0
    public double CompetenceRating { get; set; } = 80.0; // 0.0 to 100.0
    public string FactionAlignmentId { get; init; } = string.Empty;
    public bool IsAppointed { get; set; }
    public double MonthlySalary { get; set; } = 150_000.0;
}
