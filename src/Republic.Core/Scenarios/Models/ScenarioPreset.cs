namespace Republic.Core.Scenarios.Models;

/// <summary>
/// Data contract representing a preset starting scenario configuration.
/// </summary>
public sealed class ScenarioPreset
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PlayerCountryName { get; init; } = "Republic of Arcadia";
    public double StartingTreasury { get; init; } = 12_500_000_000.0;
    public double StartingStability { get; init; } = 80.0;
    public double StartingHappiness { get; init; } = 75.0;
    public List<string> NeighboringCountries { get; init; } = new();
    public List<string> PrimaryResourceNodes { get; init; } = new();
}
