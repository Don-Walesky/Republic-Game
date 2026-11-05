namespace Republic.Core.Economy.StockMarket.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing the domestic stock exchange and equities market index.
/// Synthesizes macroeconomic signals (inflation, stability, corruption, corporate taxation, and interest rates)
/// and industry sector sub-indices into an authoritative market valuation score and sentiment indicator.
/// </summary>
public sealed class StockMarketIndex
{
    [JsonPropertyName("index_name")]
    public string IndexName { get; set; } = "Republic Composite Index (RCI)";

    [JsonPropertyName("current_points")]
    public double CurrentPoints { get; set; } = 2500.0;

    [JsonPropertyName("previous_points")]
    public double PreviousPoints { get; set; } = 2465.0;

    [JsonPropertyName("change_percent")]
    public double ChangePercent =>
        PreviousPoints > 0.0 ? Math.Round(((CurrentPoints - PreviousPoints) / PreviousPoints) * 100.0, 2) : 0.0;

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "Bullish ↗️";

    [JsonPropertyName("sentiment")]
    public string Sentiment =>
        ChangePercent > 1.0 ? "Strong Growth" :
        (ChangePercent > 0.0 ? "Moderate Expansion" :
        (ChangePercent > -2.0 ? "Cautious Consolidation" : "Bearish Contraction"));

    [JsonPropertyName("primary_drivers")]
    public string PrimaryDrivers { get; set; } = "Domestic stability and corporate investment flows supporting baseline equity valuations.";

    [JsonPropertyName("sectors")]
    public List<StockMarketSector> Sectors { get; set; } = new();

    public StockMarketIndex()
    {
        InitializeDefaultSectors();
    }

    /// <summary>
    /// Populates baseline industry sectors and representative listed firms if empty.
    /// </summary>
    public void InitializeDefaultSectors()
    {
        if (Sectors.Count > 0) return;

        Sectors = new List<StockMarketSector>
        {
            new()
            {
                SectorName = "Technology & Telecommunications",
                Icon = "💻",
                CurrentPoints = 2650.0,
                PreviousPoints = 2600.0,
                Direction = "Bullish ↗️",
                KeyDrivers = "Cloud computing expansion and telecommunication infrastructure demand.",
                RepresentativeCompanies = new List<ListedCompany>
                {
                    new() { Name = "OmniNet Cloud Systems", Ticker = "ONET", MarketCapRepu = 450_000_000.0, SharePriceRepu = 45.2, ChangePercent = 1.8 },
                    new() { Name = "Apex Silicon Microelectronics", Ticker = "APEX", MarketCapRepu = 620_000_000.0, SharePriceRepu = 88.5, ChangePercent = 2.4 }
                }
            },
            new()
            {
                SectorName = "Industrial & Manufacturing",
                Icon = "🏭",
                CurrentPoints = 2480.0,
                PreviousPoints = 2450.0,
                Direction = "Bullish ↗️",
                KeyDrivers = "Automotive assembly lines and heavy metal smelting exports.",
                RepresentativeCompanies = new List<ListedCompany>
                {
                    new() { Name = "Veloce Motors International", Ticker = "VELO", MarketCapRepu = 380_000_000.0, SharePriceRepu = 32.4, ChangePercent = 0.9 },
                    new() { Name = "Titan Heavy Metallurgy", Ticker = "TITN", MarketCapRepu = 510_000_000.0, SharePriceRepu = 64.0, ChangePercent = 1.2 }
                }
            },
            new()
            {
                SectorName = "Banking & Financial Services",
                Icon = "🏛️",
                CurrentPoints = 2520.0,
                PreviousPoints = 2510.0,
                Direction = "Bullish ↗️",
                KeyDrivers = "Steady commercial lending spreads and sovereign bond trading.",
                RepresentativeCompanies = new List<ListedCompany>
                {
                    new() { Name = "Sovereign Trust Financial", Ticker = "STFC", MarketCapRepu = 720_000_000.0, SharePriceRepu = 112.0, ChangePercent = 1.5 },
                    new() { Name = "Republic Commercial Bank", Ticker = "RCBK", MarketCapRepu = 490_000_000.0, SharePriceRepu = 54.8, ChangePercent = 0.4 }
                }
            },
            new()
            {
                SectorName = "Healthcare & Consumer Staples",
                Icon = "💊",
                CurrentPoints = 2410.0,
                PreviousPoints = 2400.0,
                Direction = "Neutral ➡️",
                KeyDrivers = "Essential food packaging and pharmaceutical formulation demand.",
                RepresentativeCompanies = new List<ListedCompany>
                {
                    new() { Name = "Nestor-Valo Consumer Foods", Ticker = "NVCF", MarketCapRepu = 340_000_000.0, SharePriceRepu = 28.5, ChangePercent = 0.5 },
                    new() { Name = "Aegis BioPharma Laboratories", Ticker = "AEGS", MarketCapRepu = 410_000_000.0, SharePriceRepu = 76.2, ChangePercent = 1.1 }
                }
            },
            new()
            {
                SectorName = "Energy & Infrastructure",
                Icon = "⚡",
                CurrentPoints = 2540.0,
                PreviousPoints = 2500.0,
                Direction = "Bullish ↗️",
                KeyDrivers = "Utility-scale renewable grid investments and deepwater seaport throughput.",
                RepresentativeCompanies = new List<ListedCompany>
                {
                    new() { Name = "Solis-Aura Clean Energy", Ticker = "SOLI", MarketCapRepu = 580_000_000.0, SharePriceRepu = 48.0, ChangePercent = 1.9 },
                    new() { Name = "AeroCargo Transshipment Hub", Ticker = "ACHB", MarketCapRepu = 640_000_000.0, SharePriceRepu = 95.5, ChangePercent = 2.0 }
                }
            }
        };
    }

    /// <summary>
    /// Updates the stock market index and all sector sub-indices based on macroeconomic fundamentals and global enterprise participation.
    /// </summary>
    public double UpdateIndex(
        double baselineStability,
        double inflationRate,
        double corruptionIndex,
        double corporateTaxRate,
        double benchmarkInterestRate,
        double exchangeRateUsd = 1.25,
        double importTariffRate = 0.05,
        IReadOnlyCollection<string>? acceptedIndustries = null)
    {
        double basePoints = 2500.0;

        // Stability: higher political stability inspires foreign and institutional capital (+15 pts per pt above 75)
        double stabilityFactor = (baselineStability - 75.0) * 15.0;

        // Inflation: higher inflation erodes company profits and consumer purchasing power (-60 pts per 1% above 3.5%)
        double inflationFactor = -(inflationRate - 3.5) * 60.0;

        // Corruption: systemic corruption discourages transparent equity investment (-18 pts per 1% above 25%)
        double corruptionFactor = -(corruptionIndex - 25.0) * 18.0;

        // Corporate Tax: lower corporate tax leaves higher retained earnings (+800 pts for every 10% tax cut below 20%)
        double taxFactor = -(corporateTaxRate - 0.20) * 800.0;

        // Interest Rate: higher borrowing costs depress stock multiples (-45 pts per 1% above 4.5%)
        double interestFactor = -(benchmarkInterestRate - 4.5) * 45.0;

        double calculated = basePoints + stabilityFactor + inflationFactor + corruptionFactor + taxFactor + interestFactor;
        double clampedPoints = Math.Round(Math.Max(500.0, calculated), 1);

        PreviousPoints = CurrentPoints;
        CurrentPoints = clampedPoints;

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

        // Build primary composite drivers
        var driversList = new List<string>();
        if (baselineStability > 75.0) driversList.Add("Political stability premium");
        if (inflationRate > 5.0) driversList.Add($"Inflation drag ({inflationRate:0.1}%)");
        else if (inflationRate <= 3.5) driversList.Add("Price stability support");
        if (corporateTaxRate <= 0.20) driversList.Add("Competitive corporate tax");
        else driversList.Add("Corporate tax headwind");
        if (benchmarkInterestRate > 6.0) driversList.Add($"Tight monetary policy ({benchmarkInterestRate:0.1}%)");
        else if (benchmarkInterestRate < 4.5) driversList.Add("Accommodative benchmark rate");

        PrimaryDrivers = driversList.Count > 0 ? string.Join(" | ", driversList) : "Market trading in macroeconomic equilibrium.";

        // Update Sectors
        InitializeDefaultSectors();
        var activeIndustries = acceptedIndustries ?? Array.Empty<string>();

        foreach (var sector in Sectors)
        {
            sector.UpdateSector(
                CurrentPoints,
                baselineStability,
                inflationRate,
                corruptionIndex,
                corporateTaxRate,
                benchmarkInterestRate,
                exchangeRateUsd,
                importTariffRate,
                activeIndustries);
        }

        return CurrentPoints;
    }
}
