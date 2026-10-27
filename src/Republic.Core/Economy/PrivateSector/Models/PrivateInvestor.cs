namespace Republic.Core.Economy.PrivateSector.Models;

/// <summary>
/// General risk tolerance archetype of a private enterprise or investor consortium.
/// </summary>
public enum InvestorRiskTolerance
{
    /// <summary>
    /// Prefers state co-funding, long development windows, and low macroeconomic volatility.
    /// </summary>
    Conservative = 0,

    /// <summary>
    /// Balanced appetite for commercial expansion with reasonable government partnership terms.
    /// </summary>
    Moderate = 1,

    /// <summary>
    /// Rapid capital deployment and high return expectation. Tolerant of political friction if yield is high.
    /// </summary>
    Aggressive = 2
}

/// <summary>
/// Domain model representing an AI-controlled corporate enterprise, investor, or industrial consortium.
/// </summary>
public sealed class PrivateInvestor
{
    /// <summary>
    /// Gets the unique identifier of the investor entity.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the corporate or enterprise name of the investor.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the primary industry sector of operations (e.g. Manufacturing, Energy, Agriculture, Technology).
    /// </summary>
    public string Sector { get; init; } = "Manufacturing";

    /// <summary>
    /// Gets the category of commercial entity (e.g. DomesticConglomerate, ForeignDirectInvestor, TechEntrepreneur).
    /// </summary>
    public string InvestorType { get; init; } = "DomesticConglomerate";

    /// <summary>
    /// Gets or sets the total liquid capital capacity available for co-investments in REPU.
    /// </summary>
    public double CapitalCapacity { get; set; } = 2_000_000.0;

    /// <summary>
    /// Gets the commercial risk tolerance of the investor.
    /// </summary>
    public InvestorRiskTolerance RiskTolerance { get; init; } = InvestorRiskTolerance.Moderate;

    /// <summary>
    /// Gets or sets the commercial relationship index with the sovereign government (-100.0 to +100.0).
    /// </summary>
    public double GovernmentRelationship { get; set; } = 10.0;
}
