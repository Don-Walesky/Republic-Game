namespace Republic.Core.Intelligence.Models;

/// <summary>
/// Domain model representing an active intelligence network operating in a foreign nation.
/// </summary>
public sealed class SpyNetwork
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string TargetCountryId { get; init; } = string.Empty;
    public double InfiltrationLevel { get; set; } = 25.0; // 0.0 to 100.0
    public int AssignedAgentsCount { get; set; } = 3;
    public bool IsActive { get; set; } = true;
}
