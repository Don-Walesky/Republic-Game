namespace Republic.Core.Economy.Debt.Models;

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

