namespace Republic.Core.Economy.StockMarket.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing a representative publicly traded enterprise listed on the national exchange.
/// </summary>
public sealed record ListedCompany
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("ticker")]
    public string Ticker { get; init; } = string.Empty;

    [JsonPropertyName("market_cap_repu")]
    public double MarketCapRepu { get; init; }

    [JsonPropertyName("share_price_repu")]
    public double SharePriceRepu { get; init; }

    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; init; }
}

/// <summary>
/// Domain model representing an industry sector sub-index on the sovereign stock exchange.
/// </summary>
public sealed class StockMarketSector
{
    [JsonPropertyName("sector_name")]
    public string SectorName { get; set; } = "Industrial & Manufacturing";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "🏭";

    [JsonPropertyName("current_points")]
    public double CurrentPoints { get; set; } = 2500.0;

    [JsonPropertyName("previous_points")]
    public double PreviousPoints { get; set; } = 2480.0;

    [JsonPropertyName("change_percent")]
    public double ChangePercent =>
        PreviousPoints > 0.0 ? Math.Round(((CurrentPoints - PreviousPoints) / PreviousPoints) * 100.0, 2) : 0.0;

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "Bullish ↗️";

    [JsonPropertyName("key_drivers")]
    public string KeyDrivers { get; set; } = "Broad economic momentum";

    [JsonPropertyName("representative_companies")]
    public List<ListedCompany> RepresentativeCompanies { get; set; } = new();

    /// <summary>
    /// Recalculates the sector sub-index based on sector-specific macro sensitivity.
    /// </summary>
    public double UpdateSector(
        double compositeBasePoints,
        double baselineStability,
        double inflationRate,
        double corruptionIndex,
        double corporateTaxRate,
        double benchmarkInterestRate,
        double exchangeRateUsd,
        double importTariffRate,
        IReadOnlyCollection<string> acceptedIndustries)
    {
        double sectorMultiplier = 1.0;
        var drivers = new List<string>();

        switch (SectorName)
        {
            case "Technology & Telecommunications":
                // Highly sensitive to interest rates (discount rate) and corporate taxation
                if (benchmarkInterestRate > 6.0)
                {
                    sectorMultiplier -= 0.08;
                    drivers.Add("Higher debt costs compressing tech multiples");
                }
                else if (benchmarkInterestRate < 4.0)
                {
                    sectorMultiplier += 0.06;
                    drivers.Add("Accommodative credit lifting growth valuations");
                }

                if (corporateTaxRate <= 0.20)
                {
                    sectorMultiplier += 0.05;
                    drivers.Add("Competitive corporate tax aiding reinvestment");
                }

                if (acceptedIndustries.Any(i => i.Contains("Tech", StringComparison.OrdinalIgnoreCase) || i.Contains("Semi", StringComparison.OrdinalIgnoreCase)))
                {
                    sectorMultiplier += 0.12;
                    drivers.Add("Global tech & semiconductor foundry commitments");
                }
                break;

            case "Industrial & Manufacturing":
                // Benefits from export competitiveness (weaker currency) and protective tariffs, hurt by inflation input costs
                if (exchangeRateUsd > 1.25)
                {
                    sectorMultiplier += 0.07;
                    drivers.Add("Export price advantage from competitive currency");
                }

                if (importTariffRate >= 0.10)
                {
                    sectorMultiplier += 0.05;
                    drivers.Add("Tariff protections shielding domestic output");
                }

                if (inflationRate > 6.0)
                {
                    sectorMultiplier -= 0.09;
                    drivers.Add("Elevated raw material & energy input inflation");
                }

                if (acceptedIndustries.Any(i => i.Contains("Auto", StringComparison.OrdinalIgnoreCase) || i.Contains("Heavy", StringComparison.OrdinalIgnoreCase)))
                {
                    sectorMultiplier += 0.10;
                    drivers.Add("Inbound automotive & metallurgy plant investments");
                }
                break;

            case "Banking & Financial Services":
                // Benefits from moderate benchmark yield spread; hurt by default risk from high inflation or corruption
                if (benchmarkInterestRate is >= 3.5 and <= 7.0)
                {
                    sectorMultiplier += 0.06;
                    drivers.Add("Healthy net interest margins across commercial lenders");
                }
                else if (benchmarkInterestRate > 8.0)
                {
                    sectorMultiplier -= 0.07;
                    drivers.Add("Elevated borrower debt service and default worries");
                }

                if (corruptionIndex > 35.0)
                {
                    sectorMultiplier -= 0.10;
                    drivers.Add("Institutional governance risk dampening bank multiples");
                }

                if (acceptedIndustries.Any(i => i.Contains("Finance", StringComparison.OrdinalIgnoreCase)))
                {
                    sectorMultiplier += 0.10;
                    drivers.Add("Regional forex & securities clearinghouse expansion");
                }
                break;

            case "Healthcare & Consumer Staples":
                // Defensive sector: stable cash flows, but squeezed if consumer purchasing power falls steeply
                if (inflationRate <= 4.0)
                {
                    sectorMultiplier += 0.05;
                    drivers.Add("Resilient household consumption and staple retail sales");
                }
                else
                {
                    sectorMultiplier -= 0.06;
                    drivers.Add("Consumer purchasing power erosion from price inflation");
                }

                if (acceptedIndustries.Any(i => i.Contains("FMCG", StringComparison.OrdinalIgnoreCase) || i.Contains("Pharma", StringComparison.OrdinalIgnoreCase)))
                {
                    sectorMultiplier += 0.10;
                    drivers.Add("Domestic bottling and pharmaceutical laboratory scale");
                }
                break;

            case "Energy & Infrastructure":
                // Sensitive to political stability and sovereign infrastructure commitments
                if (baselineStability > 75.0)
                {
                    sectorMultiplier += 0.06;
                    drivers.Add("Political stability securing long-cycle infrastructure concessions");
                }

                if (acceptedIndustries.Any(i => i.Contains("Energy", StringComparison.OrdinalIgnoreCase) || i.Contains("Logistics", StringComparison.OrdinalIgnoreCase)))
                {
                    sectorMultiplier += 0.11;
                    drivers.Add("Renewable energy grid & intercontinental logistics hub active");
                }
                break;

            default:
                drivers.Add("Tracking general macroeconomic fundamentals");
                break;
        }

        double rawPoints = compositeBasePoints * sectorMultiplier;
        double clamped = Math.Round(Math.Max(300.0, rawPoints), 1);

        PreviousPoints = CurrentPoints;
        CurrentPoints = clamped;

        if (CurrentPoints > PreviousPoints + 0.1)
        {
            Direction = "Bullish ↗️";
        }
        else if (CurrentPoints < PreviousPoints - 0.1)
        {
            Direction = "Bearish ↘️";
        }
        else
        {
            Direction = "Neutral ➡️";
        }

        KeyDrivers = drivers.Count > 0 ? string.Join(" | ", drivers) : "Operating near baseline sector equilibrium.";
        return CurrentPoints;
    }
}

