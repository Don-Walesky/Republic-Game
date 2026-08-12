namespace Republic.Core.Diplomacy.Services;

using System;

/// <summary>
/// Trade deal evaluation parameters and outcome calculations.
/// </summary>
public sealed record TradeDealEvaluation(
    bool IsFavorable,
    double ExpectedTariffSavings,
    double MutualGdpBoostPercent,
    double DiplomaticReputationBonus,
    string EvaluationSummary);

/// <summary>
/// Calculation engine evaluating international trade pact proposals, tariff balance, and diplomatic leverage.
/// </summary>
public sealed class DiplomaticTradeEngine
{
    public TradeDealEvaluation EvaluateTradeAgreement(
        double sourceGdp,
        double targetGdp,
        double proposedTariffDiscountPercent,
        double sourceReputation,
        double targetReputation)
    {
        // 1. Calculate Expected Tariff Savings based on combined economic weight
        double combinedGdp = sourceGdp + targetGdp;
        double expectedTariffSavings = (combinedGdp * 0.001) * (proposedTariffDiscountPercent / 100.0);

        // 2. Mutual GDP Boost Calculation
        double mutualGdpBoostPercent = Math.Min(4.5, (proposedTariffDiscountPercent * 0.1) + ((sourceReputation + targetReputation) / 200.0));

        // 3. Diplomatic Reputation Bonus
        double reputationBonus = Math.Min(15.0, 5.0 + (proposedTariffDiscountPercent * 0.2));

        // 4. Favorability threshold check
        bool isFavorable = sourceReputation >= 30.0 && targetReputation >= 30.0 && proposedTariffDiscountPercent >= 5.0;

        string summary = isFavorable
            ? $"Trade Agreement Highly Favorable: Yields ${expectedTariffSavings:N0} in tariff savings and a +{reputationBonus:0.0} diplomatic reputation boost."
            : "Trade Agreement Suboptimal: Low diplomatic standing or insufficient tariff concessions.";

        return new TradeDealEvaluation(
            isFavorable,
            Math.Round(expectedTariffSavings, 2),
            Math.Round(mutualGdpBoostPercent, 2),
            Math.Round(reputationBonus, 2),
            summary);
    }
}
