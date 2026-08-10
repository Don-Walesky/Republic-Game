namespace Republic.Core.Economy.Budget.Models;

/// <summary>
/// Domain model summarizing quarterly tax revenue and ministry expenditure outcomes.
/// </summary>
public sealed class FiscalQuarterReport
{
    public int QuarterIndex { get; init; }
    public double TotalTaxRevenue { get; init; }
    public double TotalExpenditures { get; init; }
    public double NetSurplusOrDeficit => TotalTaxRevenue - TotalExpenditures;
    public double UpdatedTreasuryBalance { get; init; }
}
