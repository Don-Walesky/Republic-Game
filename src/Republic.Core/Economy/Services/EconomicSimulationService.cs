namespace Republic.Core.Economy.Services;

using System;

/// <summary>
/// State data representing key macroeconomic indices.
/// </summary>
public sealed record EconomicStateSummary(
    double GdpGrowthRate,
    double InflationRate,
    double UnemploymentRate,
    double TariffRevenue,
    double NationalDebt);

/// <summary>
/// Service calculating dynamic macroeconomic indicators, tariff revenue, and labor market allocation.
/// </summary>
public sealed class EconomicSimulationService
{
    public EconomicStateSummary CalculateNextEconomicCycle(
        double currentTreasury,
        double taxRatePercent,
        double tariffRatePercent,
        double tradeVolume,
        double infrastructureInvestment)
    {
        // 1. Tariff Revenue = Trade Volume * (Tariff Rate / 100)
        double tariffRevenue = tradeVolume * (tariffRatePercent / 100.0);

        // 2. Inflation Rate calculation
        // High tax dampens inflation; low tax and heavy infrastructure spending increases demand
        double baseInflation = 2.5;
        double demandPull = (infrastructureInvestment / 1_000_000.0) * 0.1;
        double taxDampening = (taxRatePercent - 20.0) * 0.05;
        double inflationRate = Math.Max(0.1, baseInflation + demandPull - taxDampening);

        // 3. GDP Growth Rate calculation
        // Optimal tax rate is ~25%; higher taxes suppress growth
        double taxPenalty = taxRatePercent > 30.0 ? (taxRatePercent - 30.0) * 0.15 : 0.0;
        double infraBonus = Math.Min(3.0, (infrastructureInvestment / 5_000_000.0) * 1.2);
        double gdpGrowthRate = Math.Max(-5.0, 3.0 + infraBonus - taxPenalty);

        // 4. Unemployment Rate calculation
        // Strong GDP growth reduces unemployment
        double baseUnemployment = 5.0;
        double unemploymentRate = Math.Clamp(baseUnemployment - (gdpGrowthRate * 0.4), 1.5, 25.0);

        // 5. National Debt
        double nationalDebt = currentTreasury < 0 ? Math.Abs(currentTreasury) : 0.0;

        return new EconomicStateSummary(
            Math.Round(gdpGrowthRate, 2),
            Math.Round(inflationRate, 2),
            Math.Round(unemploymentRate, 2),
            Math.Round(tariffRevenue, 2),
            Math.Round(nationalDebt, 2));
    }
}
