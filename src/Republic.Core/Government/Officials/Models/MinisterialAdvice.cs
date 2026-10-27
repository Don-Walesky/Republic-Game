namespace Republic.Core.Government.Officials.Models;

using Republic.Core.Cabinet.Models;

/// <summary>
/// Domain model encapsulating portfolio-specific advice provided by an executive cabinet minister.
/// Reflected and weighted by the minister's competence, loyalty, integrity, and domain responsibilities.
/// </summary>
public sealed class MinisterialAdvice
{
    public CabinetPortfolio Portfolio { get; init; }
    public string MinisterName { get; init; } = "Vacant";
    public bool IsAppointed { get; init; }
    public string Headline { get; init; } = string.Empty;
    public string DetailedAdvice { get; init; } = string.Empty;
    public string RiskLevel { get; init; } = "Low";
    public double ConfidenceRating { get; init; } = 80.0;
    public double CompetenceRating { get; init; } = 50.0;
    public double LoyaltyRating { get; init; } = 50.0;
    public double IntegrityRating { get; init; } = 50.0;
    public string RecommendedAction { get; init; } = string.Empty;
    public double ActionCost { get; init; }
    public bool IsSelfServing { get; init; }
    public string WarningNotes { get; init; } = string.Empty;
}
