namespace Republic.Core.Economy.Debt.Models;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Economy.CentralBank.Models;
using Republic.Core.Economy.Treasury;

/// <summary>
/// Domain model encapsulating sovereign borrowing, domestic debt instruments,
/// external multilateral/sovereign facilities, and debt service obligations.
/// </summary>
public sealed class NationalDebtRecord
{
    [JsonPropertyName("domestic_debt")]
    public double DomesticDebt { get; set; } = 150_000_000.0;

    [JsonPropertyName("external_debt")]
    public double ExternalDebt { get; set; } = 100_000_000.0;

    [JsonPropertyName("total_debt")]
    public double TotalDebt => DomesticDebt + ExternalDebt;

    [JsonPropertyName("domestic_interest_rate")]
    public double DomesticInterestRate { get; set; } = 0.05; // 5.0% annual coupon

    [JsonPropertyName("external_interest_rate")]
    public double ExternalInterestRate { get; set; } = 0.07; // 7.0% annual coupon

    [JsonPropertyName("quarterly_debt_service")]
    public double QuarterlyDebtService =>
        (DomesticDebt * (DomesticInterestRate / 4.0)) + (ExternalDebt * (ExternalInterestRate / 4.0));

    [JsonPropertyName("debt_to_gdp_ratio")]
    public double DebtToGdpRatio { get; set; } = 2.5;

    [JsonPropertyName("debt_service_to_revenue_ratio")]
    public double DebtServiceToRevenueRatio { get; set; } = 1.6;

    [JsonPropertyName("sustainability_status")]
    public string SustainabilityStatus { get; set; } = "Sustainable (Low Risk) 🟢";

    [JsonPropertyName("sustainability_summary")]
    public string SustainabilitySummary { get; set; } = "Debt stock is highly sustainable. Sovereign debt represents a modest fraction of GDP and quarterly service is well covered by tax revenues.";

    /// <summary>
    /// Evaluates sovereign debt sustainability based on national GDP and quarterly revenue.
    /// Returns the updated sustainability status string.
    /// </summary>
    public string EvaluateSustainability(double gdp, double quarterlyRevenue)
    {
        double effectiveGdp = gdp > 0.0 ? gdp : 10_000_000_000.0;
        double effectiveRev = quarterlyRevenue > 0.0 ? quarterlyRevenue : 200_000_000.0;

        DebtToGdpRatio = Math.Round((TotalDebt / effectiveGdp) * 100.0, 2);
        DebtServiceToRevenueRatio = Math.Round((QuarterlyDebtService / effectiveRev) * 100.0, 2);

        if (DebtToGdpRatio > 70.0 || DebtServiceToRevenueRatio > 30.0)
        {
            SustainabilityStatus = "Critical Debt Distress (High Risk) 🔴";
            SustainabilitySummary = $"High risk of sovereign default! Debt is {DebtToGdpRatio:0.1}% of GDP and service consumes {DebtServiceToRevenueRatio:0.1}% of revenue. Austerity or restructuring required.";
        }
        else if (DebtToGdpRatio > 40.0 || DebtServiceToRevenueRatio > 15.0)
        {
            SustainabilityStatus = "Moderate Pressure (Medium Risk) 🟡";
            SustainabilitySummary = $"Moderate fiscal pressure. Sovereign debt is {DebtToGdpRatio:0.1}% of GDP with service absorbing {DebtServiceToRevenueRatio:0.1}% of quarterly revenue.";
        }
        else
        {
            SustainabilityStatus = "Sustainable (Low Risk) 🟢";
            SustainabilitySummary = $"Sovereign borrowing is fully sustainable at {DebtToGdpRatio:0.1}% of GDP. Debt service requires only {DebtServiceToRevenueRatio:0.1}% of revenue.";
        }

        return SustainabilityStatus;
    }

    /// <summary>
    /// Takes on additional sovereign borrowing.
    /// Domestic loans directly inject liquid funds into the national Treasury.
    /// External loans bolster Central Bank Foreign Reserves and/or liquid Treasury.
    /// </summary>
    public bool TakeLoan(string loanType, double amount, RepuTreasury treasury, CountryCentralBank centralBank)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        ArgumentNullException.ThrowIfNull(centralBank);
        if (amount <= 0.0) return false;

        if (string.Equals(loanType, "domestic", StringComparison.OrdinalIgnoreCase))
        {
            DomesticDebt += amount;
            treasury.Deposit(amount);
            return true;
        }
        else if (string.Equals(loanType, "external", StringComparison.OrdinalIgnoreCase))
        {
            ExternalDebt += amount;
            // External loan delivers foreign exchange reserves directly to the Central Bank
            centralBank.DepositForeignReserves(amount);
            // And a portion (50%) can be monetized into liquid Treasury for domestic budget use
            treasury.Deposit(amount * 0.5);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Repays existing debt obligations, reducing future interest and principal liability.
    /// </summary>
    public bool RepayLoan(string loanType, double amount, RepuTreasury treasury, CountryCentralBank centralBank)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        ArgumentNullException.ThrowIfNull(centralBank);
        if (amount <= 0.0) return false;

        if (string.Equals(loanType, "domestic", StringComparison.OrdinalIgnoreCase))
        {
            double payable = Math.Min(amount, DomesticDebt);
            if (payable <= 0.0 || treasury.Balance < payable) return false;

            treasury.Withdraw(payable);
            DomesticDebt -= payable;
            return true;
        }
        else if (string.Equals(loanType, "external", StringComparison.OrdinalIgnoreCase))
        {
            double payable = Math.Min(amount, ExternalDebt);
            if (payable <= 0.0) return false;

            // Prioritize paying from foreign reserves if available, otherwise from liquid treasury
            if (centralBank.ForeignReserves >= payable)
            {
                centralBank.WithdrawForeignReserves(payable);
                ExternalDebt -= payable;
                return true;
            }
            else if (treasury.Balance >= payable)
            {
                treasury.Withdraw(payable);
                ExternalDebt -= payable;
                return true;
            }

            return false;
        }

        return false;
    }
}
