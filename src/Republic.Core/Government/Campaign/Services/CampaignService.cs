namespace Republic.Core.Government.Campaign.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Government;
using Republic.Core.Identity.Models;
using Republic.Core.Politics;
using Republic.Core.World.Models;

/// <summary>
/// Service coordinating the presidential campaign headquarters, 70:30 consequence distribution,
/// campaign purse audit, stakeholder meetings, and election night resolution.
/// </summary>
public static class CampaignService
{
    /// <summary>
    /// Initializes or synchronizes the campaign framework for the player's sovereign nation,
    /// establishing the R 5,000,000 REPU purse and canonical 70:30 consequence decisions.
    /// </summary>
    public static void InitializeCampaign(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.CampaignOffice ??= new CampaignOfficeState();

        if (country.CampaignOffice.AvailableDecisions == null || country.CampaignOffice.AvailableDecisions.Count == 0)
        {
            country.CampaignOffice.AvailableDecisions = CampaignDecision.CreateCanonicalDecisions();
        }

        country.CampaignOffice.AccountantGeneral ??= new CampaignAccountantGeneral();
        if (country.CampaignOffice.Purse <= 0m && country.CampaignOffice.CompletedDecisions.Count == 0)
        {
            country.CampaignOffice.Purse = 5_000_000m;
            country.CampaignOffice.AccountantGeneral.CurrentPurse = 5_000_000m;
        }

        if (country.CampaignOffice.Polling <= 0m)
        {
            country.CampaignOffice.Polling = 35.0m;
        }

        var doctrine = !string.IsNullOrWhiteSpace(country.LeadershipDoctrine)
            ? country.LeadershipDoctrine
            : (country.LeadershipTypes.Count > 0
                ? string.Join(" & ", country.LeadershipTypes.Select(t => t.ToString()))
                : "Balanced Governance");

        // Tailor Campaign Advice from General Adviser, Press Secretary, and Accountant General
        if (country.GeneralAdviser != null)
        {
            if (string.IsNullOrWhiteSpace(country.GeneralAdviser.StrategicGuidance))
            {
                country.GeneralAdviser.StrategicGuidance =
                    $"Campaign Strategy ({doctrine}): To cross the 50.0% electoral threshold, we must build a broad coalition across capital and labor while pacing expenditures from our R 5,000,000 purse.";
            }
            else if (!country.GeneralAdviser.StrategicGuidance.Contains("electoral threshold"))
            {
                country.GeneralAdviser.StrategicGuidance +=
                    $" Campaign Strategy: To cross the 50.0% electoral threshold, we must build a broad coalition across capital and labor while pacing expenditures from our R 5,000,000 purse.";
            }
        }

        if (country.PressSecretary != null)
        {
            if (string.IsNullOrWhiteSpace(country.PressSecretary.BriefingHeadline))
            {
                country.PressSecretary.BriefingHeadline = $"{doctrine} Campaign Outlines National Reform Platform";
            }
            if (string.IsNullOrWhiteSpace(country.PressSecretary.BriefingSummary))
            {
                country.PressSecretary.BriefingSummary =
                    "Press Secretary Elena Vance confirms high campaign rally turnout and active engagement across all provincial districts.";
            }
        }

        if (country.CampaignOffice.AccountantGeneral != null)
        {
            country.CampaignOffice.AccountantGeneral.CurrentPurse = country.CampaignOffice.Purse;
            country.CampaignOffice.AccountantGeneral.FinancialAdvice =
                country.CampaignOffice.Purse < 1_000_000m
                    ? "Warning: Campaign purse has dipped below R 1,000,000. Prioritize low-cost grassroots events."
                    : "Campaign purse is solvent. All expenditures fully audited and compliant with national election commission limits.";
        }
    }

    /// <summary>
    /// Executes a player-approved campaign decision with balanced positive and negative consequences,
    /// deducting costs from the campaign purse and updating polling and national metrics.
    /// </summary>
    public static (bool Success, string Message, CampaignDecision? Decision) ExecuteDecision(Country country, string decisionId)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (string.IsNullOrWhiteSpace(decisionId))
        {
            return (false, "Campaign decision ID cannot be empty.", null);
        }

        country.CampaignOffice ??= new CampaignOfficeState();
        country.CampaignOffice.AvailableDecisions ??= CampaignDecision.CreateCanonicalDecisions();
        country.CampaignOffice.CompletedDecisions ??= new List<CampaignDecision>();

        var decision = country.CampaignOffice.AvailableDecisions.FirstOrDefault(d => string.Equals(d.Id, decisionId, StringComparison.OrdinalIgnoreCase));
        if (decision == null)
        {
            var completed = country.CampaignOffice.CompletedDecisions.FirstOrDefault(d => string.Equals(d.Id, decisionId, StringComparison.OrdinalIgnoreCase));
            if (completed != null)
            {
                return (false, $"Decision '{completed.Title}' has already been executed.", completed);
            }

            return (false, $"Campaign decision '{decisionId}' not found.", null);
        }

        if (country.CampaignOffice.Purse < decision.CostRepu)
        {
            return (false, $"Insufficient Campaign Purse funds. Required: R {decision.CostRepu:N0} REPU, Available: R {country.CampaignOffice.Purse:N0} REPU.", decision);
        }

        // Apply financial consequences to campaign purse
        country.CampaignOffice.Purse -= decision.CostRepu;
        if (decision.FundingImpact > 0m)
        {
            country.CampaignOffice.Purse += decision.FundingImpact;
        }

        // Apply political and social consequences
        country.CampaignOffice.Polling = Math.Clamp(country.CampaignOffice.Polling + decision.PollingImpact, 0m, 100m);
        country.HappinessRating = Math.Clamp(country.HappinessRating + (double)decision.ApprovalImpact, 0.0, 100.0);
        country.BaselineStability = Math.Clamp(country.BaselineStability + (double)decision.StabilityImpact, 0.0, 100.0);

        // Update Campaign Accountant General audit record
        country.CampaignOffice.AccountantGeneral ??= new CampaignAccountantGeneral();
        country.CampaignOffice.AccountantGeneral.CurrentPurse = country.CampaignOffice.Purse;
        country.CampaignOffice.AccountantGeneral.TotalExpenditures += decision.CostRepu;
        country.CampaignOffice.AccountantGeneral.FinancialAdvice =
            country.CampaignOffice.Purse < 1_000_000m
                ? "Warning: Campaign purse has dipped below R 1,000,000. Prioritize low-cost grassroots events."
                : "Campaign purse is solvent. All expenditures fully audited and compliant with national election commission limits.";

        decision.Executed = true;
        decision.OutcomeExplanation =
            $"Executed '{decision.Title}'. " +
            $"Positive consequence: {decision.ForeseeablePositiveConsequence} | " +
            $"Negative consequence: {decision.ForeseeableNegativeConsequence} | " +
            $"Purse Impact: -R {decision.CostRepu:N0}" + (decision.FundingImpact > 0m ? $" (+R {decision.FundingImpact:N0})" : "") +
            $" | New Polling: {country.CampaignOffice.Polling:0.#}%.";

        country.CampaignOffice.AvailableDecisions.Remove(decision);
        country.CampaignOffice.CompletedDecisions.Add(decision);

        return (true, decision.OutcomeExplanation, decision);
    }

    /// <summary>
    /// Resolves national election night based on accumulated polling support and remaining campaign purse momentum.
    /// A combined tally of >= 50.0% yields presidential victory and democratic mandate.
    /// </summary>
    public static (bool Won, string Message, decimal FinalTally) ResolveElectionNight(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.CampaignOffice ??= new CampaignOfficeState();

        // Financial reserves provide a modest election-day turnout mobilization bonus (max +10%)
        var fundingBonus = Math.Min(10.0m, country.CampaignOffice.Purse / 500_000.0m);
        var finalTally = Math.Clamp(country.CampaignOffice.Polling + fundingBonus, 0.0m, 100.0m);
        var won = finalTally >= 50.0m;

        country.CampaignOffice.ElectionResolved = true;
        country.CampaignOffice.ElectionWon = won;
        country.CampaignOffice.ElectionTally = finalTally;

        if (won)
        {
            country.CampaignOffice.ElectionSummary =
                $"VICTORY: Won presidential election with {finalTally:0.#}% of the popular vote! Democratic mandate secured.";

            if (country.RulingParty != null)
            {
                country.RulingParty = country.RulingParty with { Support = (double)finalTally };
            }
            else
            {
                country.RulingParty = new PoliticalParty { Name = "Democratic Union", Support = (double)finalTally };
            }

            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = Math.Max(0.0, 100.0 - (double)finalTally) };
            }

            country.HappinessRating = Math.Clamp(country.HappinessRating + 5.0, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability + 5.0, 0.0, 100.0);
        }
        else
        {
            country.CampaignOffice.ElectionSummary =
                $"DEFEAT: Won {finalTally:0.#}% of the popular vote, failing to cross the 50.0% constitutional majority required to assume office.";
        }

        return (won, country.CampaignOffice.ElectionSummary, finalTally);
    }
}
