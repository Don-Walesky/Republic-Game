namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing national population demographics.
/// </summary>
public sealed class Demographics
{
    public long TotalPopulation { get; set; } = 10_000_000;
    public double GrowthRate { get; set; } = 0.012;
    public double HappinessRating { get; set; } = 68.5;
    public double LiteracyRate { get; set; } = 94.0;
    public double EmploymentRate { get; set; } = 92.5;
    public double UrbanPercentage { get; set; } = 65.0;
}
