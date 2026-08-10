namespace Republic.Core.Intelligence.Models;

/// <summary>
/// Domain model representing a clandestine operation launched against a rival power.
/// </summary>
public sealed class CovertOperation
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public CovertOperationType Type { get; init; }
    public string TargetCountryId { get; init; } = string.Empty;
    public double RiskRating { get; init; } = 0.35; // 0.0 to 1.0
    public double FinancialCost { get; init; } = 50_000_000.0;
    public bool IsCompleted { get; set; }
    public bool IsExposed { get; set; }
}
