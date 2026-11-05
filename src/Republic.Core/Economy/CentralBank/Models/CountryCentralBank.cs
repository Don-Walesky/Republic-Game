namespace Republic.Core.Economy.CentralBank.Models;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;

/// <summary>
/// Domain model representing a sovereign nation's independent Central Bank.
/// Governs benchmark interest rates, monetary policy stances, foreign exchange reserves,
/// and national currency regimes.
/// </summary>
public sealed class CountryCentralBank
{
    public const double ForexUnlockThreshold = 150_000_000.0; // 150 Million REPU

    [JsonPropertyName("governor_name")]
    public string GovernorName { get; set; } = "Dr. Marcus Thorne";

    [JsonPropertyName("benchmark_interest_rate")]
    public double BenchmarkInterestRate { get; set; } = 4.5; // Baseline 4.5%

    [JsonPropertyName("foreign_reserves")]
    public double ForeignReserves { get; set; } = 45_000_000.0; // Starting 45M REPU reserves

    [JsonPropertyName("forex_unlocked")]
    public bool ForexUnlocked { get; set; }

    [JsonPropertyName("currency_regime")]
    public string CurrencyRegime { get; set; } = "Sovereign Managed Float";

    [JsonPropertyName("exchange_rate_usd")]
    public double ExchangeRateToUSD { get; set; } = 1.25; // 1 USD = 1.25 REPU

    [JsonPropertyName("total_injections")]
    public double TotalInjections { get; set; }

    [JsonPropertyName("total_sterilizations")]
    public double TotalSterilizations { get; set; }

    [JsonPropertyName("total_interventions")]
    public int TotalInterventions { get; set; }

    /// <summary>
    /// Gets the export competitiveness multiplier derived from the prevailing exchange rate.
    /// A weaker currency (higher REPU per USD) boosts foreign export demand.
    /// </summary>
    [JsonPropertyName("export_competitiveness_multiplier")]
    public double ExportCompetitivenessMultiplier =>
        Math.Round(Math.Clamp(1.0 + ((ExchangeRateToUSD - 1.25) * 0.50), 0.60, 1.80), 3);

    /// <summary>
    /// Gets the import cost inflation multiplier derived from the exchange rate.
    /// A weaker currency increases the cost of imported raw materials and consumer goods.
    /// </summary>
    [JsonPropertyName("import_cost_multiplier")]
    public double ImportCostMultiplier =>
        Math.Round(Math.Clamp(1.0 + ((ExchangeRateToUSD - 1.25) * 0.35), 0.70, 1.60), 3);

    /// <summary>
    /// Sets the central bank benchmark interest rate, clamped between 0.5% and 25.0%.
    /// </summary>
    public double SetInterestRate(double newRate)
    {
        BenchmarkInterestRate = Math.Clamp(Math.Round(newRate, 2), 0.5, 25.0);
        return BenchmarkInterestRate;
    }

    /// <summary>
    /// Adjusts the interest rate by delta (e.g. +0.5% hike or -0.5% cut).
    /// </summary>
    public double AdjustInterestRate(double delta)
    {
        return SetInterestRate(BenchmarkInterestRate + delta);
    }

    /// <summary>
    /// Central bank intervenes in the foreign exchange market to defend/strengthen the sovereign currency.
    /// Sells foreign reserves to absorb domestic currency, appreciating the exchange rate (lower REPU per USD)
    /// and cooling imported inflation pressure.
    /// </summary>
    public bool InterveneDefendCurrency(double reserveAmount)
    {
        if (reserveAmount <= 0.0 || ForeignReserves < reserveAmount) return false;

        ForeignReserves -= reserveAmount;
        double appreciationDelta = (reserveAmount / 100_000_000.0) * 0.05;
        ExchangeRateToUSD = Math.Max(0.75, Math.Round(ExchangeRateToUSD - appreciationDelta, 3));
        TotalInterventions++;
        return true;
    }

    /// <summary>
    /// Central bank intervenes in the foreign exchange market to boost export competitiveness.
    /// Sells domestic liquidity to purchase and accumulate foreign exchange reserves,
    /// allowing the exchange rate to slightly devalue/depreciate to make national exports cheaper abroad.
    /// </summary>
    public bool InterveneBoostExports(double reserveAmount, RepuTreasury? treasury = null)
    {
        if (reserveAmount <= 0.0) return false;

        if (treasury != null && treasury.Balance >= reserveAmount)
        {
            treasury.Withdraw(reserveAmount);
        }

        ForeignReserves += reserveAmount;
        double depreciationDelta = (reserveAmount / 100_000_000.0) * 0.05;
        ExchangeRateToUSD = Math.Min(2.50, Math.Round(ExchangeRateToUSD + depreciationDelta, 3));
        TotalInterventions++;
        CheckForexUnlock();
        return true;
    }

    /// <summary>
    /// Deposits foreign exchange reserves (e.g. from trade surplus, external loans, or commodity export receipts).
    /// Returns true if this deposit crossed the 150M REPU milestone and unlocked Forex.
    /// </summary>
    public bool DepositForeignReserves(double amount)
    {
        if (amount <= 0.0) return false;
        bool wasUnlocked = ForexUnlocked;
        ForeignReserves += amount;
        bool newlyUnlocked = CheckForexUnlock();
        return !wasUnlocked && newlyUnlocked;
    }

    /// <summary>
    /// Withdraws foreign exchange reserves (e.g. for import clearance or external debt repayments).
    /// </summary>
    public bool WithdrawForeignReserves(double amount)
    {
        if (amount <= 0.0 || ForeignReserves < amount) return false;
        ForeignReserves -= amount;
        return true;
    }

    /// <summary>
    /// Checks and persists whether foreign exchange reserves satisfy the 150 Million REPU unlock requirement.
    /// </summary>
    public bool CheckForexUnlock()
    {
        if (ForeignReserves >= ForexUnlockThreshold)
        {
            ForexUnlocked = true;
        }
        return ForexUnlocked;
    }

    /// <summary>
    /// Injects foreign reserves into the domestic Treasury liquidity to alleviate sovereign cash crunches.
    /// </summary>
    public bool InjectReservesToTreasury(RepuTreasury treasury, double amount)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        if (amount <= 0.0 || ForeignReserves < amount) return false;

        ForeignReserves -= amount;
        treasury.Deposit(amount);
        TotalInjections += amount;
        return true;
    }

    /// <summary>
    /// Sterilizes excess domestic Treasury cash by converting it into foreign reserves.
    /// </summary>
    public bool SterilizeTreasuryToReserves(RepuTreasury treasury, double amount)
    {
        ArgumentNullException.ThrowIfNull(treasury);
        if (amount <= 0.0 || treasury.Balance < amount) return false;

        treasury.Withdraw(amount);
        ForeignReserves += amount;
        TotalSterilizations += amount;
        CheckForexUnlock();
        return true;
    }
}
