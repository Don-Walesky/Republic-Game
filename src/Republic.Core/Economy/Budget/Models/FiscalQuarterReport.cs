namespace Republic.Core.Economy.Budget.Models;

using Republic.Core.Economy.Treasury;
using Republic.Core.Time;

/// <summary>
/// Domain model summarizing quarterly tax revenue and ministry expenditure outcomes.
/// </summary>
public sealed class FiscalQuarterReport
{
    public string CountryId { get; init; } = string.Empty;
    public int QuarterIndex { get; init; }
    public double IncomeTaxRevenue { get; init; }
    public double CorporateTaxRevenue { get; init; }
    public double TariffRevenue { get; init; }
    public double TotalTaxRevenue { get; init; }
    public double TotalExpenditures { get; init; }
    public double NetSurplusOrDeficit => TotalTaxRevenue - TotalExpenditures;
    public double UpdatedTreasuryBalance { get; init; }
    public RepublicTime? Timestamp { get; init; }

    public string FormattedTotalTaxRevenue => RepuTreasury.Format(TotalTaxRevenue);
    public string FormattedTotalExpenditures => RepuTreasury.Format(TotalExpenditures);
    public string FormattedNetSurplusOrDeficit => RepuTreasury.Format(NetSurplusOrDeficit);
    public string FormattedUpdatedTreasuryBalance => RepuTreasury.Format(UpdatedTreasuryBalance);
}

