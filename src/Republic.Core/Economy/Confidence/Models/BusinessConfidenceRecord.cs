namespace Republic.Core.Economy.Confidence.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating the national Business Confidence Index (BCI).
/// Measures private enterprise optimism, corporate investment appetites, and market sentiment
/// driven by taxation, inflation, institutional corruption, political stability, monetary policy,
/// and global enterprise participation.
/// </summary>
public sealed class BusinessConfidenceRecord
{
    [JsonPropertyName("score")]
    public double Score { get; set; } = 65.0;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Moderate Growth 🟡";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "Commercial sentiment remains constructive. Corporate balance sheets supported by manageable inflation and steady domestic stability.";

    [JsonPropertyName("tax_sentiment")]
    public double TaxSentiment { get; set; }

    [JsonPropertyName("inflation_sentiment")]
    public double InflationSentiment { get; set; }

    [JsonPropertyName("stability_sentiment")]
    public double StabilitySentiment { get; set; }

    [JsonPropertyName("corruption_sentiment")]
    public double CorruptionSentiment { get; set; }

    [JsonPropertyName("monetary_sentiment")]
    public double MonetarySentiment { get; set; }

    [JsonPropertyName("fdi_sentiment")]
    public double FdiSentiment { get; set; }

    /// <summary>
    /// Recalculates the Business Confidence Index (0-100) based on prevailing macroeconomic and institutional variables.
    /// </summary>
    public double UpdateConfidence(
        double baselineStability,
        double inflationRate,
        double corruptionIndex,
        double corporateTaxRate,
        double benchmarkInterestRate,
        int acceptedBusinessesCount,
        double importTariffRate = 0.05)
    {
        double baseScore = 65.0;

        // 1. Political Stability: predictability and rule of law anchor long-term capital deployment
        StabilitySentiment = Math.Round((baselineStability - 70.0) * 0.40, 1);

        // 2. Inflation: price stability preserves profit margins, whereas high inflation triggers cost spikes
        InflationSentiment = inflationRate <= 4.0
            ? Math.Round((4.0 - inflationRate) * 1.5, 1)
            : Math.Round(-(inflationRate - 4.0) * 2.5, 1);

        // 3. Corruption: regulatory friction, graft, and bureaucratic extortion deter honest enterprise
        CorruptionSentiment = Math.Round(-(corruptionIndex - 20.0) * 0.45, 1);

        // 4. Corporate Taxation: competitive corporate tax rates encourage retained earnings and capital reinvestment
        TaxSentiment = Math.Round(-(corporateTaxRate - 0.20) * 120.0, 1);

        // 5. Monetary Stance: benchmark rates dictate credit costs for working capital and corporate expansion
        MonetarySentiment = Math.Round(-(benchmarkInterestRate - 4.5) * 2.0, 1);

        // 6. Global Enterprise Ecosystem: each authorized multinational deepens domestic supply chains and supplier contracts
        FdiSentiment = Math.Round(acceptedBusinessesCount * 4.5, 1);

        // 7. Tariff friction: high tariffs protect select incumbents but impose supply bottlenecks on manufacturers
        double tariffFriction = importTariffRate > 0.15
            ? Math.Round(-(importTariffRate - 0.15) * 30.0, 1)
            : 0.0;

        double raw = baseScore + StabilitySentiment + InflationSentiment + CorruptionSentiment + TaxSentiment + MonetarySentiment + FdiSentiment + tariffFriction;
        Score = Math.Clamp(Math.Round(raw, 1), 5.0, 100.0);

        if (Score >= 75.0)
        {
            Status = "Strong Expansion 🟢";
            Summary = $"Business confidence is booming at {Score:0.0} pts. Robust institutional stability and capital inflows are accelerating private sector hiring and investment.";
        }
        else if (Score >= 55.0)
        {
            Status = "Moderate Growth 🟡";
            Summary = $"Business confidence is stable at {Score:0.0} pts. Commercial conditions are favorable, though enterprises remain watchful of price and interest rate trajectories.";
        }
        else if (Score >= 40.0)
        {
            Status = "Cautious Stagnation 🟠";
            Summary = $"Business confidence softened to {Score:0.0} pts. Corporate executives are deferring major capital outlays due to margin pressures and regulatory friction.";
        }
        else
        {
            Status = "Severe Contraction 🔴";
            Summary = $"Business confidence plunged to {Score:0.0} pts. Elevated macro distress, high inflation, and institutional risks threaten widespread commercial contraction.";
        }

        return Score;
    }
}

