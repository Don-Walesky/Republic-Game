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
/// campaign purse audit, competitive AI opponents, interest group endorsements, and election night resolution.
/// </summary>
public static class CampaignService
{
    /// <summary>
    /// Initializes or synchronizes the campaign framework for the player's sovereign nation,
    /// establishing the R 5,000,000 REPU purse, competitive AI opponents, interest groups, and canonical decisions.
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

        if (country.CampaignOffice.AiOpponents == null || country.CampaignOffice.AiOpponents.Count == 0)
        {
            country.CampaignOffice.AiOpponents = CampaignOfficeState.CreateCanonicalOpponents();
        }

        if (country.CampaignOffice.InterestGroups == null || country.CampaignOffice.InterestGroups.Count == 0)
        {
            country.CampaignOffice.InterestGroups = CampaignOfficeState.CreateCanonicalInterestGroups();
        }

        if (country.CampaignOffice.CampaignNews == null || country.CampaignOffice.CampaignNews.Count == 0)
        {
            country.CampaignOffice.CampaignNews = CampaignOfficeState.CreateInitialNews();
        }

        if (country.CampaignOffice.UndecidedPolling <= 0m)
        {
            country.CampaignOffice.UndecidedPolling = 10.0m;
        }

        NormalizeRacePolling(country);

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
                    $"Campaign Strategy ({doctrine}): To cross the 50.0% electoral threshold, build coalition support across capital, labor, and civil groups while pacing expenditures from our R 5,000,000 purse.";
            }
            else if (!country.GeneralAdviser.StrategicGuidance.Contains("electoral threshold"))
            {
                country.GeneralAdviser.StrategicGuidance +=
                    $" Campaign Strategy: To cross the 50.0% electoral threshold, build coalition support across capital, labor, and civil groups while pacing expenditures from our R 5,000,000 purse.";
            }
        }

        if (country.PressSecretary != null)
        {
            if (string.IsNullOrWhiteSpace(country.PressSecretary.BriefingHeadline))
            {
                country.PressSecretary.BriefingHeadline = $"{doctrine} Campaign Contests Three-Way National Race";
            }
            if (string.IsNullOrWhiteSpace(country.PressSecretary.BriefingSummary))
            {
                country.PressSecretary.BriefingSummary =
                    "Press Secretary Elena Vance confirms rising poll numbers across provincial districts, noting active interest-group negotiations.";
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
    /// deducting costs from the campaign purse, updating interest-group reputation, and prompting AI opponent counter-moves.
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

        // Apply interest-group reputation shifts
        ApplyDecisionInterestGroupShifts(country, decision);
        EvaluateInterestGroupEndorsements(country);

        // Simulate AI opponent turns and reactive counter-moves
        SimulateOpponentTurns(country, decision);
        NormalizeRacePolling(country);

        decision.Executed = true;
        decision.OutcomeExplanation =
            $"Executed '{decision.Title}'. " +
            $"Positive consequence: {decision.ForeseeablePositiveConsequence} | " +
            $"Negative consequence: {decision.ForeseeableNegativeConsequence} | " +
            $"Purse Impact: -R {decision.CostRepu:N0}" + (decision.FundingImpact > 0m ? $" (+R {decision.FundingImpact:N0})" : "") +
            $" | New Polling: {country.CampaignOffice.Polling:0.#}%.";

        country.CampaignOffice.AvailableDecisions.Remove(decision);
        country.CampaignOffice.CompletedDecisions.Add(decision);

        // Add news article describing the event
        AddNews(country, $"CAMPAIGN ACTION: {decision.Title} Unveiled",
            $"The presidential contender enacted '{decision.Title}', shifting national polling and drawing immediate reactions across the political spectrum.",
            "Campaign Press Corps", decision.IsNetPositive ? "Positive" : "Warning");

        return (true, decision.OutcomeExplanation, decision);
    }

    /// <summary>
    /// Courts a key civic or commercial interest group through bilateral negotiations, pledges, and campaign expenditures.
    /// </summary>
    public static (bool Success, string Message, CampaignInterestGroup? Group) CourtInterestGroup(Country country, string groupId, decimal expenditureRepu, string pledge)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.CampaignOffice ??= new CampaignOfficeState();
        country.CampaignOffice.InterestGroups ??= CampaignOfficeState.CreateCanonicalInterestGroups();

        var normalizedTarget = groupId.Replace("group_", "").Replace("ig_", "");
        var group = country.CampaignOffice.InterestGroups.FirstOrDefault(g => 
            string.Equals(g.Id, groupId, StringComparison.OrdinalIgnoreCase) ||
            g.Id.EndsWith(normalizedTarget, StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            return (false, $"Interest group '{groupId}' not found.", null);
        }

        if (country.CampaignOffice.Purse < expenditureRepu)
        {
            return (false, $"Insufficient Campaign Purse. Required: R {expenditureRepu:N0} REPU, Available: R {country.CampaignOffice.Purse:N0} REPU.", group);
        }

        country.CampaignOffice.Purse -= expenditureRepu;
        country.CampaignOffice.AccountantGeneral ??= new CampaignAccountantGeneral();
        country.CampaignOffice.AccountantGeneral.TotalExpenditures += expenditureRepu;
        country.CampaignOffice.AccountantGeneral.CurrentPurse = country.CampaignOffice.Purse;

        decimal repGain = expenditureRepu >= 250_000m ? 25m : 18m;
        group.PlayerReputation = Math.Clamp(group.PlayerReputation + repGain, 0m, 100m);
        country.CampaignOffice.Polling = Math.Clamp(country.CampaignOffice.Polling + 2.0m, 0m, 100m);

        string pledgeText = string.IsNullOrWhiteSpace(pledge) ? "National Partnership Charter" : pledge;
        string msg = $"Successfully courted {group.Name} with pledge '{pledgeText}'. Player reputation raised to {group.PlayerReputation:0.#}%.";

        AddNews(country, $"FACTION PACT: Contender Meets with {group.Name}",
            $"Following bilateral talks, the contender committed: '{pledgeText}'. {group.Name} leadership praised the dialogue.",
            "Sovereign News Wire", "Positive");

        EvaluateInterestGroupEndorsements(country);
        SimulateOpponentTurns(country, null, groupId);
        NormalizeRacePolling(country);

        return (true, msg, group);
    }

    /// <summary>
    /// Adjusts interest group relations based on the decision category and commitments.
    /// </summary>
    private static void ApplyDecisionInterestGroupShifts(Country country, CampaignDecision decision)
    {
        if (country.CampaignOffice?.InterestGroups == null || country.CampaignOffice.InterestGroups.Count == 0) return;

        var groups = country.CampaignOffice.InterestGroups;
        var industrialists = groups.FirstOrDefault(g => g.Id == "ig_industrialists");
        var media = groups.FirstOrDefault(g => g.Id == "ig_media");
        var unions = groups.FirstOrDefault(g => g.Id == "ig_unions");
        var professionals = groups.FirstOrDefault(g => g.Id == "ig_professionals");
        var influencers = groups.FirstOrDefault(g => g.Id == "ig_influencers");

        switch (decision.Id)
        {
            case "dec_meet_business":
                if (industrialists != null) industrialists.PlayerReputation = Math.Clamp(industrialists.PlayerReputation + 25m, 0m, 100m);
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation - 5m, 0m, 100m);
                break;
            case "dec_meet_labor":
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation + 25m, 0m, 100m);
                if (industrialists != null) industrialists.PlayerReputation = Math.Clamp(industrialists.PlayerReputation - 5m, 0m, 100m);
                break;
            case "dec_media_blitz":
                if (media != null) media.PlayerReputation = Math.Clamp(media.PlayerReputation + 20m, 0m, 100m);
                if (influencers != null) influencers.PlayerReputation = Math.Clamp(influencers.PlayerReputation + 10m, 0m, 100m);
                break;
            case "dec_regional_tour":
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation + 10m, 0m, 100m);
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation + 10m, 0m, 100m);
                break;
            case "dec_grassroots_townhall":
                if (influencers != null) influencers.PlayerReputation = Math.Clamp(influencers.PlayerReputation + 15m, 0m, 100m);
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation + 10m, 0m, 100m);
                break;
            case "dec_reject_controversial_deal":
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation + 20m, 0m, 100m);
                if (media != null) media.PlayerReputation = Math.Clamp(media.PlayerReputation + 10m, 0m, 100m);
                if (industrialists != null) industrialists.PlayerReputation = Math.Clamp(industrialists.PlayerReputation - 5m, 0m, 100m);
                break;
            case "dec_public_commitment":
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation + 15m, 0m, 100m);
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation + 10m, 0m, 100m);
                break;
            case "dec_expensive_gala":
                if (industrialists != null) industrialists.PlayerReputation = Math.Clamp(industrialists.PlayerReputation + 15m, 0m, 100m);
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation - 15m, 0m, 100m);
                if (influencers != null) influencers.PlayerReputation = Math.Clamp(influencers.PlayerReputation - 10m, 0m, 100m);
                break;
            case "dec_accept_sponsorship":
                if (industrialists != null) industrialists.PlayerReputation = Math.Clamp(industrialists.PlayerReputation + 25m, 0m, 100m);
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation - 20m, 0m, 100m);
                if (unions != null) unions.PlayerReputation = Math.Clamp(unions.PlayerReputation - 15m, 0m, 100m);
                if (media != null) media.PlayerReputation = Math.Clamp(media.PlayerReputation - 15m, 0m, 100m);
                break;
            case "dec_aggressive_attack_ads":
                if (media != null) media.PlayerReputation = Math.Clamp(media.PlayerReputation - 10m, 0m, 100m);
                if (professionals != null) professionals.PlayerReputation = Math.Clamp(professionals.PlayerReputation - 15m, 0m, 100m);
                break;
        }
    }

    /// <summary>
    /// Evaluates interest-group reputation thresholds for endorsement flips or revocations.
    /// </summary>
    public static void EvaluateInterestGroupEndorsements(Country country)
    {
        if (country.CampaignOffice?.InterestGroups == null) return;

        foreach (var group in country.CampaignOffice.InterestGroups)
        {
            if (group.PlayerReputation >= 65m)
            {
                if (group.EndorsementStatus != "EndorsingPlayer")
                {
                    group.EndorsementStatus = "EndorsingPlayer";
                    group.EndorsedCandidate = "Player";

                    if (group.Id == "ig_industrialists")
                    {
                        AddNews(country, $"FINANCIAL BACKING: {group.Name} Endorses Contender",
                            $"Following policy agreements, {group.Name} has formally endorsed the contender, unlocking corporate PAC networks and commercial voter support.",
                            "Commercial Bureau", "Positive");
                    }
                    else
                    {
                        AddNews(country, $"MAJOR ENDORSEMENT: {group.Name} Officially Backs Contender!",
                            $"In a decisive boost to campaign momentum, {group.Name} announced an official endorsement, citing trust and shared vision.",
                            "National Wire", "Positive");
                    }
                }
            }
            else if (group.PlayerReputation < 35m && group.EndorsementStatus == "EndorsingPlayer")
            {
                group.EndorsementStatus = "Neutral";
                group.EndorsedCandidate = string.Empty;
                AddNews(country, $"ENDORSEMENT RETRACTED: {group.Name} Pulls Support",
                    $"Citing recent platform decisions and policy drift, {group.Name} announced the immediate revocation of its presidential endorsement.",
                    "Sovereign Press", "Warning");
            }
        }
    }

    /// <summary>
    /// Simulates reactive counter-moves by AI campaign opponents in response to player activities.
    /// </summary>
    public static void SimulateOpponentTurns(Country country, CampaignDecision? playerDecision, string? courtedGroupId = null)
    {
        if (country.CampaignOffice?.AiOpponents == null || country.CampaignOffice.AiOpponents.Count == 0) return;

        var opponents = country.CampaignOffice.AiOpponents;
        var thorne = opponents.FirstOrDefault(o => o.Id == "opp_marcus_thorne");
        var drake = opponents.FirstOrDefault(o => o.Id == "opp_cassian_drake");

        int turnIndex = country.CampaignOffice.CompletedDecisions.Count;

        // 1. Senator Marcus Thorne (Establishment Stalwart)
        if (thorne != null)
        {
            if (turnIndex % 2 == 0)
            {
                thorne.RecentAction = "Marcus Thorne booked R 450,000 in broadcast television ads touting economic stability.";
                thorne.FundsRepu = Math.Max(0m, thorne.FundsRepu - 450_000m);
                thorne.Polling += 1.5m;
                thorne.Momentum = 1.5m;
                AddNews(country, "Thorne Dominates Prime-Time Airwaves with Stability Ads",
                    "Senator Marcus Thorne launched a nationwide media buy targeting suburban swing districts with messages of fiscal continuity.",
                    "Elections Watch", "Neutral");
            }
            else
            {
                thorne.RecentAction = "Thorne secured endorsements from municipal mayors and commercial leaders.";
                thorne.FundsRepu += 500_000m;
                thorne.Polling += 1.2m;
                thorne.Momentum = 1.2m;
                AddNews(country, "Municipal Mayors Rally Behind Marcus Thorne",
                    "A coalition of provincial mayors endorsed Marcus Thorne, warning against unproven contenders.",
                    "Provincial Herald", "Neutral");
            }
        }

        // 2. Cassian Drake (Populist Challenger)
        if (drake != null)
        {
            if (turnIndex % 2 == 0)
            {
                drake.RecentAction = "Cassian Drake held a massive 30,000-person anti-establishment rally in the western district.";
                drake.FundsRepu = Math.Max(0m, drake.FundsRepu - 250_000m);
                drake.Polling += 2.0m;
                drake.Momentum = 2.0m;
                AddNews(country, "Drake Packs Stadium in Fiery Anti-Establishment Rally",
                    "Thousands turned out in Western Province as Cassian Drake railed against backroom party politics and corporate donors.",
                    "The People's Tribune", "Neutral");
            }
            else
            {
                drake.RecentAction = "Drake released a viral digital dossier questioning establishment candidate integrity.";
                drake.Polling += 1.0m;
                drake.Momentum = 1.0m;
                AddNews(country, "Drake Campaign Attacks Rival Pledges in Viral Video",
                    "A rapid-fire digital critique produced by Drake's team went viral across streaming platforms, capturing youth voter attention.",
                    "Digital Vanguard", "Warning");
            }
        }
    }

    /// <summary>
    /// Smoothly normalizes multi-candidate polling and undecided percentages so they always total 100%.
    /// </summary>
    public static void NormalizeRacePolling(Country country)
    {
        if (country.CampaignOffice == null) return;

        var office = country.CampaignOffice;
        decimal player = office.Polling;
        decimal thorne = office.AiOpponents.FirstOrDefault(o => o.Id == "opp_marcus_thorne")?.Polling ?? 30m;
        decimal drake = office.AiOpponents.FirstOrDefault(o => o.Id == "opp_cassian_drake")?.Polling ?? 20m;

        decimal sumCandidates = player + thorne + drake;
        decimal undecided = Math.Clamp(100.0m - sumCandidates, 3.0m, 25.0m);

        if (sumCandidates + undecided > 100m)
        {
            decimal overflow = (sumCandidates + undecided) - 100m;
            if (undecided > 4m)
            {
                decimal shaveUndecided = Math.Min(overflow * 0.5m, undecided - 4m);
                undecided -= shaveUndecided;
                overflow -= shaveUndecided;
            }

            var thorneObj = office.AiOpponents.FirstOrDefault(o => o.Id == "opp_marcus_thorne");
            if (thorneObj != null && thorneObj.Polling > 15m)
            {
                decimal shaveThorne = Math.Min(overflow * 0.5m, thorneObj.Polling - 15m);
                thorneObj.Polling -= shaveThorne;
                overflow -= shaveThorne;
            }

            var drakeObj = office.AiOpponents.FirstOrDefault(o => o.Id == "opp_cassian_drake");
            if (drakeObj != null && drakeObj.Polling > 12m)
            {
                drakeObj.Polling -= Math.Min(overflow, drakeObj.Polling - 12m);
            }
        }

        office.UndecidedPolling = Math.Round(undecided, 1);
    }

    private static void AddNews(Country country, string headline, string summary, string source, string tone)
    {
        if (country.CampaignOffice?.CampaignNews == null) return;

        country.CampaignOffice.CampaignNews.Insert(0, new CampaignNewsItem
        {
            Headline = headline,
            Summary = summary,
            Source = source,
            Tone = tone,
            Timestamp = $"Turn {country.CampaignOffice.CompletedDecisions.Count + 1}"
        });

        if (country.CampaignOffice.CampaignNews.Count > 15)
        {
            country.CampaignOffice.CampaignNews.RemoveAt(country.CampaignOffice.CampaignNews.Count - 1);
        }
    }

    /// <summary>
    /// Resolves national election night based on accumulated polling support, interest-group turnout bonuses,
    /// and remaining campaign purse mobilization reserves.
    /// A combined tally of >= 50.0% yields presidential victory and democratic mandate.
    /// </summary>
    public static (bool Won, string Message, decimal FinalTally) ResolveElectionNight(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.CampaignOffice ??= new CampaignOfficeState();
        var office = country.CampaignOffice;

        // 1. Calculate Interest-Group Turnout Bonus
        decimal endorsementBonus = 0m;
        var playerEndorsements = office.InterestGroups?.Where(g => g.EndorsementStatus == "EndorsingPlayer").ToList() ?? new List<CampaignInterestGroup>();
        foreach (var grp in playerEndorsements)
        {
            endorsementBonus += grp.TurnoutBonus;
        }

        // 2. Financial reserves GOTV mobilization bonus (max +10%)
        var fundingBonus = Math.Min(10.0m, office.Purse / 500_000.0m);

        // 3. Final player popular vote tally
        var finalTally = Math.Clamp(office.Polling + fundingBonus + (endorsementBonus * 0.5m), 0.0m, 100.0m);
        var won = finalTally >= 50.0m;

        // 4. Opponent vote tallies
        var thorne = office.AiOpponents?.FirstOrDefault(o => o.Id == "opp_marcus_thorne");
        var drake = office.AiOpponents?.FirstOrDefault(o => o.Id == "opp_cassian_drake");

        decimal thorneShare = Math.Round(Math.Clamp(100.0m - finalTally, 0m, 100m) * 0.60m, 1);
        decimal drakeShare = Math.Round(Math.Clamp(100.0m - finalTally - thorneShare, 0m, 100m), 1);

        office.ElectionBreakdown = new Dictionary<string, decimal>
        {
            ["Player"] = finalTally,
            [thorne?.Name ?? "Senator Marcus Thorne"] = thorneShare,
            [drake?.Name ?? "Cassian Drake"] = drakeShare
        };

        office.ElectionResolved = true;
        office.ElectionWon = won;
        office.ElectionTally = finalTally;

        string endorsedNames = playerEndorsements.Count > 0
            ? string.Join(", ", playerEndorsements.Select(g => g.Name))
            : "Grassroots Voters Only";

        if (won)
        {
            office.ElectionSummary =
                $"VICTORY: Won presidential election with {finalTally:0.#}% of the popular vote! Democratic mandate secured. " +
                $"(Opponents: Marcus Thorne {thorneShare:0.#}%, Cassian Drake {drakeShare:0.#}%). " +
                $"Key Coalition Endorsements: {endorsedNames}.";

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
                country.OppositionParty = country.OppositionParty with { Support = (double)thorneShare };
            }

            country.HappinessRating = Math.Clamp(country.HappinessRating + 5.0, 0.0, 100.0);
            country.BaselineStability = Math.Clamp(country.BaselineStability + 5.0, 0.0, 100.0);

            AddNews(country, $"ELECTION NIGHT VICTORY: Contender Elected President with {finalTally:0.#}%",
                office.ElectionSummary, "Elections Commission", "Positive");
        }
        else
        {
            office.ElectionSummary =
                $"DEFEAT: Won {finalTally:0.#}% of the popular vote, failing to cross the 50.0% constitutional majority required to assume office. " +
                $"(Marcus Thorne: {thorneShare:0.#}%, Cassian Drake: {drakeShare:0.#}%). " +
                $"Loss Factors: Insufficient coalition endorsements and voter turnout.";

            AddNews(country, $"ELECTION NIGHT SHORTFALL: Contender Falls Short with {finalTally:0.#}%",
                office.ElectionSummary, "Elections Commission", "Warning");
        }

        return (won, office.ElectionSummary, finalTally);
    }
}
