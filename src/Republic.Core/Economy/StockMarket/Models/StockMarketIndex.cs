namespace Republic.Core.Economy.StockMarket.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing the domestic stock exchange and equities market index.
/// Synthesizes macroeconomic signals (inflation, stability, corruption, corporate taxation, and interest rates)
/// into an authoritative market valuation score and sentiment indicator.
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

    /// <summary>
    /// Updates the stock market index based on macroeconomic fundamentals.
    /// </summary>
    public double UpdateIndex(
        double baselineStability,
        double inflationRate,
        double corruptionIndex,
        double corporateTaxRate,
        double benchmarkInterestRate)
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

        return CurrentPoints;
    }
}

