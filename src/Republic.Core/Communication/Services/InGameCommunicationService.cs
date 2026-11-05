namespace Republic.Core.Communication.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Politics;
using Republic.Core.Legislature.Models;
using Republic.Core.Persistence.Models;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Models;

/// <summary>
/// Authoritative communication service that orchestrates in-game communications across
/// institutional channels: Legislative arm intelligence, bilateral diplomatic cables,
/// executive cabinet memoranda, and intelligence alerts.
/// </summary>
public static class InGameCommunicationService
{
    /// <summary>
    /// Dispatches an insider intelligence report from a legislator who has courted or defected across the aisle.
    /// Provides the player with confidential legislative floor dynamics, opposition morale, and caucus expectations.
    /// </summary>
    public static EmailMessage DispatchLegislatorDefectionIntel(
        Country country,
        Legislator lawmaker,
        string incentiveType,
        FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(lawmaker);

        var chamber = country.Legislature ?? new ParliamentaryChamber();
        int rulingSeats = chamber.RulingPartySeats;
        string majorityText = rulingSeats >= 51
            ? $"The Ruling Coalition now commands a working majority of {rulingSeats}/100 seats."
            : $"The administration currently commands {rulingSeats}/100 seats (Hung Parliament — 51 required for assured statutory passage).";

        string bodyText;
        string incentiveSummary;

        if (incentiveType.Contains("grant", StringComparison.OrdinalIgnoreCase) ||
            incentiveType.Contains("technocrat", StringComparison.OrdinalIgnoreCase))
        {
            incentiveSummary = "Constituency Development Grant (Technocratic)";
            bodyText = $"Mr./Madam President,\n\n" +
                $"The R 250,000 capital development grant for {lawmaker.Constituency} has been confirmed by our local municipal assembly. " +
                $"Breaking ground on vital civil works has solidified my local standing and neutralized hardline opposition rhetoric in my district.\n\n" +
                $"As agreed, my {lawmaker.SeatWeight} parliamentary seat(s) have officially crossed the aisle to join your Ruling Coalition. " +
                $"{majorityText}\n\n" +
                $"INSIDE INTELLIGENCE: The opposition floor managers are in disarray over my defection. " +
                $"They are whispering about holding backroom meetings to stall upcoming statutory bills in committee. " +
                $"If you intend to propose national infrastructure or tax legislation, strike while their caucus discipline is fractured.\n\n" +
                $"Respectfully submitted,\n" +
                $"{lawmaker.Name}\n" +
                $"Representative for {lawmaker.Constituency} (National Assembly)";
        }
        else if (incentiveType.Contains("chair", StringComparison.OrdinalIgnoreCase) ||
                 incentiveType.Contains("committee", StringComparison.OrdinalIgnoreCase))
        {
            incentiveSummary = "Committee Chairmanship (Institutional Power)";
            bodyText = $"Mr./Madam President,\n\n" +
                $"I have officially assumed the gavel of the Parliamentary Standing Committee. " +
                $"From this institutional vantage point, I will ensure executive statutory proposals receive expedited calendar priority " +
                $"and favorable roll-call scheduling.\n\n" +
                $"My {lawmaker.SeatWeight} seat(s) are firmly registered with the administration's parliamentary coalition. " +
                $"{majorityText}\n\n" +
                $"INSIDE INTELLIGENCE: Several backbench lawmakers from the opposition are envious of this committee appointment. " +
                $"Keep an eye on independent representatives like Clara Sterling or Julian Mercer—they are watching closely to see if the executive " +
                $"delivers on committee autonomy before committing their own votes on the next big fiscal bill.\n\n" +
                $"In institutional solidarity,\n" +
                $"{lawmaker.Name}\n" +
                $"Chair, Parliamentary Standing Committee • Member for {lawmaker.Constituency}";
        }
        else
        {
            incentiveSummary = "Executive Patronage (Machine Politics)";
            bodyText = $"Confidential — For the President's Eyes Only,\n\n" +
                $"The political resources have reached my campaign war chest. My local precinct captains have been mobilized, " +
                $"and my {lawmaker.SeatWeight} seat(s) will vote in lockstep with the administration on all confidence motions and statutory bills.\n\n" +
                $"{majorityText}\n\n" +
                $"INSIDE INTELLIGENCE: The parliamentary press gallery is sniffing around rumors of backroom patronage. " +
                $"I advise keeping state auditor reports focused elsewhere. So long as the media remains managed, " +
                $"you can count on my floor votes when the roll is called.\n\n" +
                $"Fraternally,\n" +
                $"{lawmaker.Name}\n" +
                $"Representative for {lawmaker.Constituency}";
        }

        var message = new EmailMessage
        {
            Id = $"mail_leg_{lawmaker.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = lawmaker.Name,
            SenderTitle = $"Lawmaker for {lawmaker.Constituency} • {incentiveSummary}",
            Recipient = "Presidential Desk",
            Subject = $"🏛️ LEGISLATIVE DISPATCH: Floor Crossing & Intelligence ({lawmaker.Name})",
            Body = bodyText,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Legislative",
            Urgency = "Confidential",
            ActionRequired = false,
            ActionType = "view_chamber",
            RelatedEntityId = lawmaker.Id,
            AvatarSymbol = "🏛️"
        };

        StoreMessageInGameState(gameState, message);
        return message;
    }

    /// <summary>
    /// Dispatches a legislative committee report on an introduced or pending statutory bill.
    /// </summary>
    public static EmailMessage DispatchLegislatorBillReview(
        Country country,
        CountryBill bill,
        string stage,
        FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(bill);

        var chamber = country.Legislature ?? new ParliamentaryChamber();
        string subject;
        string body;
        string urgency = "Priority";

        if (stage.Equals("proposed", StringComparison.OrdinalIgnoreCase))
        {
            subject = $"🏛️ LEGISLATIVE REPORT: First Reading of '{bill.Title}'";
            body = $"Mr./Madam President,\n\n" +
                $"Your executive bill '{bill.Title}' has been assigned to the Assembly Order Paper. " +
                $"Preliminary floor whipping indicates {chamber.RulingPartySeats} baseline Ruling Coalition votes.\n\n" +
                $"• Category: {bill.Category}\n" +
                $"• Positive Expected Impact: {bill.PositiveConsequence}\n" +
                $"• Negative Friction / Scrutiny: {bill.NegativeConsequence}\n\n" +
                $"INSIDE INTELLIGENCE: Opposition floor leaders are planning amendments. " +
                (chamber.HasWorkingMajority
                    ? "Because our coalition commands a majority, we can force a straight vote without conceding major amendments."
                    : "Because the chamber is currently a Hung Parliament, passage hinges on courting unaligned lawmakers or offering committee concessions.") +
                $"\n\nParliamentary Clerk's Office\nNational Assembly of {country.Name}";
        }
        else if (stage.Equals("passed", StringComparison.OrdinalIgnoreCase))
        {
            urgency = "Routine";
            subject = $"🏛️ STATUTORY ENACTMENT: Bill '{bill.Title}' Passed!";
            body = $"Mr./Madam President,\n\n" +
                $"The roll call has concluded on '{bill.Title}'. The bill received {bill.AyesCount} Ayes against {bill.NaysCount} Noes " +
                $"and has been formally transmitted for executive gazetting.\n\n" +
                $"The statutory reforms are now sovereign law.\n\nSpeaker of the House\nNational Assembly";
        }
        else
        {
            urgency = "Urgent";
            subject = $"🏛️ CONSTITUTIONAL NOTICE: Executive Veto of '{bill.Title}'";
            body = $"Mr./Madam President,\n\n" +
                $"The Assembly has recorded your executive veto of '{bill.Title}'.\n\n" +
                $"INSIDE INTELLIGENCE: Opposition leaders are canvassing for a two-thirds override (67 votes). " +
                $"Party whips advise maintaining close communication with coalition members to ensure our voting bloc holds the veto sustain threshold.\n\n" +
                $"Office of the Majority Whip\nNational Assembly";
        }

        var message = new EmailMessage
        {
            Id = $"mail_bill_{bill.Title}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = "Speaker of the House",
            SenderTitle = "Speaker's Chambers • National Assembly",
            Recipient = "Presidential Desk",
            Subject = subject,
            Body = body,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Legislative",
            Urgency = urgency,
            ActionRequired = stage.Equals("proposed", StringComparison.OrdinalIgnoreCase),
            ActionType = "view_chamber",
            RelatedEntityId = bill.Title,
            AvatarSymbol = "🏛️"
        };

        StoreMessageInGameState(gameState, message);
        return message;
    }

    /// <summary>
    /// Dispatches an official foreign diplomatic cable when another sovereign nation or player initiates a bilateral trade deal.
    /// </summary>
    public static EmailMessage DispatchBilateralProposalCable(
        Country recipientCountry,
        Country proposerCountry,
        FlexibleTradeTerms proposal,
        FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(recipientCountry);
        ArgumentNullException.ThrowIfNull(proposerCountry);
        ArgumentNullException.ThrowIfNull(proposal);

        string termsSummary = proposal.FormatSummary();
        string bodyText = $"To the Sovereign Executive of the Republic of {recipientCountry.Name},\n\n" +
            $"The Sovereign Government of '{proposerCountry.Name}' extends fraternal diplomatic greetings and formally submits " +
            $"the following bilateral economic terms for your presidential review:\n\n" +
            $"PROPOSED BILATERAL ACCORD:\n" +
            $"{termsSummary}\n\n" +
            $"STRATEGIC IMPERATIVE:\n" +
            $"Our two sovereign nations share mutual interests in regional economic liquidity and supply chain resilience. " +
            $"This bilateral exchange will fortify our respective industrial reserves while securing predictable commercial flows across our borders.\n\n" +
            $"DIPLOMATIC OPTIONS:\n" +
            $"1. Ratify & Accept: Dispatches instructions to your central bank and customs ministry to enact the deal immediately.\n" +
            $"2. Propose Counter-Offer: Adjust offered commodity volumes or currency terms to match your domestic economic plan.\n" +
            $"3. Decline: Cordially decline the accord without diplomatic penalty.\n\n" +
            $"With highest sovereign esteem,\n" +
            $"Ministry of Foreign Affairs & International Trade\n" +
            $"Sovereign State of {proposerCountry.Name}";

        var email = new EmailMessage
        {
            Id = $"mail_dip_{proposal.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = $"Foreign Ministry of {proposerCountry.Name}",
            SenderTitle = $"Diplomatic Mission to {recipientCountry.Name} • Sovereign Peer",
            Recipient = "Presidential Desk",
            Subject = $"🌐 BILATERAL TRADE PROPOSAL: {termsSummary}",
            Body = bodyText,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Diplomatic",
            Urgency = "Priority",
            ActionRequired = true,
            ActionType = "review_bilateral_deal",
            RelatedEntityId = proposal.Id,
            AvatarSymbol = "🌐"
        };

        StoreMessageInGameState(gameState, email);

        // Also record a multiplayer DiplomaticMessage in WorldState
        if (gameState?.World != null)
        {
            gameState.World.Messages ??= new();
            gameState.World.Messages.Add(new DiplomaticMessage
            {
                Id = $"dip_msg_{proposal.Id}",
                From = proposerCountry.Name,
                To = recipientCountry.Name,
                Subject = $"Bilateral Proposal: {termsSummary}",
                Text = bodyText,
                MessageType = "BilateralOffer",
                OfferedDealId = proposal.Id,
                Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Urgency = "Priority",
                IsRead = false
            });
        }

        return email;
    }

    /// <summary>
    /// Dispatches a diplomatic resolution cable confirming agreement ratification, rejection, or counter-proposals.
    /// </summary>
    public static EmailMessage DispatchBilateralResolutionCable(
        Country recipientCountry,
        Country actorCountry,
        FlexibleTradeTerms proposal,
        string outcome,
        FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(recipientCountry);
        ArgumentNullException.ThrowIfNull(actorCountry);
        ArgumentNullException.ThrowIfNull(proposal);

        string subject;
        string body;
        string urgency = "Routine";

        if (outcome.Equals("accepted", StringComparison.OrdinalIgnoreCase) ||
            outcome.Equals("ratified", StringComparison.OrdinalIgnoreCase))
        {
            subject = $"🌐 DIPLOMATIC ACCORD RATIFIED: Agreement with {actorCountry.Name} Enacted";
            body = $"Excellency,\n\n" +
                $"We celebrate the bilateral ratification of our trade accord:\n" +
                $"{proposal.FormatSummary()}\n\n" +
                $"Commodities have cleared border customs and state accounts have been authoritatively settled. " +
                $"This pact strengthens the sovereign friendship and economic prosperity of our two peoples.\n\n" +
                $"High Commissioner for Bilateral Commerce\nGovernment of {actorCountry.Name}";
        }
        else if (outcome.Equals("countered", StringComparison.OrdinalIgnoreCase))
        {
            urgency = "Priority";
            subject = $"🌐 DIPLOMATIC COUNTER-OFFER: Revised Terms from {actorCountry.Name}";
            body = $"Excellency,\n\n" +
                $"Our economic council has reviewed the prior trade proposal and drafted a counter-offer:\n" +
                $"{proposal.FormatSummary()}\n\n" +
                $"Please inspect the revised terms in your Foreign Affairs dossier.\n\n" +
                $"Department of Foreign Trade\nGovernment of {actorCountry.Name}";
        }
        else
        {
            subject = $"🌐 DIPLOMATIC NOTICE: Trade Proposal Declined by {actorCountry.Name}";
            body = $"Excellency,\n\n" +
                $"Due to domestic commodity constraints, our government is currently unable to conclude the proposed bilateral accord:\n" +
                $"{proposal.FormatSummary()}\n\n" +
                $"We look forward to future economic opportunities as market conditions develop.\n\n" +
                $"Foreign Ministry\nGovernment of {actorCountry.Name}";
        }

        var email = new EmailMessage
        {
            Id = $"mail_dip_res_{proposal.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = $"Foreign Ministry ({actorCountry.Name})",
            SenderTitle = $"Bilateral Mission of {actorCountry.Name}",
            Recipient = "Presidential Desk",
            Subject = subject,
            Body = body,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Diplomatic",
            Urgency = urgency,
            ActionRequired = outcome.Equals("countered", StringComparison.OrdinalIgnoreCase),
            ActionType = outcome.Equals("countered", StringComparison.OrdinalIgnoreCase) ? "review_bilateral_deal" : string.Empty,
            RelatedEntityId = proposal.Id,
            AvatarSymbol = "🌐"
        };

        StoreMessageInGameState(gameState, email);
        return email;
    }

    /// <summary>
    /// Dispatches an advisory when chamber majority status shifts (e.g. crossing the 51-seat threshold).
    /// </summary>
    public static EmailMessage DispatchChamberStatusAlert(
        Country country,
        ParliamentaryChamber chamber,
        FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(chamber);

        string subject;
        string body;
        string urgency;

        if (chamber.HasWorkingMajority)
        {
            urgency = "Priority";
            subject = $"🏛️ HISTORIC WORKING MAJORITY: 51+ Seats Secured in National Assembly!";
            body = $"Mr./Madam President,\n\n" +
                $"Through strategic cross-aisle courtship, the President's Ruling Coalition has reached {chamber.RulingPartySeats}/100 seats, " +
                $"officially ending the Hung Parliament and securing a STABLE WORKING MAJORITY in the High Chamber!\n\n" +
                $"• Ruling Coalition: {chamber.RulingPartySeats} Seats\n" +
                $"• Opposition: {chamber.OppositionSeats} Seats\n" +
                $"• Independents: {chamber.IndependentSeats} Seats\n\n" +
                $"LEGISLATIVE ADVANTAGE: Executive statutory bills and developmental charters will no longer face automatic gridlock. " +
                $"Maintain strong ties with defected lawmakers to protect party discipline.\n\n" +
                $"Majority Leader\nNational Assembly of {country.Name}";
        }
        else
        {
            urgency = "Routine";
            subject = $"🏛️ PARLIAMENTARY AUDIT: Hung Parliament Balance ({chamber.RulingPartySeats}/100 Seats)";
            body = $"Mr./Madam President,\n\n" +
                $"The National Assembly current seat count stands at {chamber.RulingPartySeats} for the Ruling Coalition, " +
                $"{chamber.OppositionSeats} Opposition, and {chamber.IndependentSeats} Independents.\n\n" +
                $"To guarantee passage of capital budgets and statutory bills, party whips recommend continuing targeted courtship of " +
                $"reform-minded Opposition or Independent lawmakers.\n\n" +
                $"Parliamentary Secretariat\nNational Assembly";
        }

        var email = new EmailMessage
        {
            Id = $"mail_maj_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = "Parliamentary Secretariat",
            SenderTitle = "Office of the Clerk & Majority Whips",
            Recipient = "Presidential Desk",
            Subject = subject,
            Body = body,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Legislative",
            Urgency = urgency,
            ActionRequired = false,
            ActionType = "view_chamber",
            AvatarSymbol = "🏛️"
        };

        StoreMessageInGameState(gameState, email);
        return email;
    }

    /// <summary>
    /// Seeds comprehensive default communications for a new or freshly loaded presidential session,
    /// providing rich initial immersion across legislative, bilateral, and executive channels.
    /// </summary>
    public static void EnsureDefaultCommunications(Country country, FullGameState? gameState)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (gameState?.Workspace == null)
        {
            return;
        }

        gameState.Workspace.Emails ??= new List<EmailMessage>();

        // If emails already exist and have at least one legislative or diplomatic message, keep intact
        if (gameState.Workspace.Emails.Any(e => e.Channel is "Legislative" or "Diplomatic"))
        {
            return;
        }

        var chamber = country.Legislature ?? new ParliamentaryChamber();
        chamber.InitializeDefaultLawmakers();

        // 1. Legislative Chamber Opening Briefing
        gameState.Workspace.Emails.Add(new EmailMessage
        {
            Id = "mail_leg_intro_01",
            Sender = "Hon. Marcus Thorne",
            SenderTitle = "Opposition Whip & Senior Parliamentarian",
            Recipient = "Presidential Desk",
            Subject = "🏛️ LEGISLATIVE WHISPER: Opening Assembly Mood & Floor Numbers",
            Body = $"Mr./Madam President,\n\n" +
                $"As the National Assembly convenes for this governance cycle, our caucus numbers stand at 48 Ruling Coalition seats, " +
                $"42 Opposition, and 10 Independents. Under our constitutional standing rules, 51 votes are required to enact executive bills.\n\n" +
                $"My colleagues in the Industrial Rustbelt are feeling the pinch of economic transitions. " +
                $"If your administration is willing to invest in constituency development or offer committee oversight roles, " +
                $"several of our members are prepared to cross the floor and deliver a reliable working majority.\n\n" +
                $"We await your signals from the Executive Office.\n\n" +
                $"Hon. Marcus Thorne\n" +
                $"Representative, 4th Industrial District",
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Legislative",
            Urgency = "Confidential",
            ActionRequired = true,
            ActionType = "view_chamber",
            RelatedEntityId = "lawmaker_thorne",
            AvatarSymbol = "🏛️"
        });

        // 2. Foreign Bilateral Trade Opening Cable
        gameState.Workspace.Emails.Add(new EmailMessage
        {
            Id = "mail_dip_intro_01",
            Sender = "Foreign Ministry of the Maridian Union",
            SenderTitle = "Ambassador Plenipotentiary",
            Recipient = "Presidential Desk",
            Subject = "🌐 BILATERAL OVERVIEW: Maridian Commercial & Resource Pact Proposed",
            Body = $"To the Sovereign President of {country.Name},\n\n" +
                $"The Maridian Union conveys its congratulations on your executive mandate. " +
                $"Our trade delegation has prepared a bilateral trade framework: our heavy manufacturing sector requires reliable " +
                $"crude oil and agricultural deliveries, in exchange for discounted industrial machinery and direct currency settlements.\n\n" +
                $"Our diplomatic mission in your capital is prepared to conclude formal terms whenever you review the bilateral proposal.\n\n" +
                $"In diplomatic fraternity,\n" +
                $"Ambassador Elena Vane\n" +
                $"Maridian Diplomatic Mission",
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Diplomatic",
            Urgency = "Priority",
            ActionRequired = true,
            ActionType = "review_bilateral_deal",
            RelatedEntityId = "prop_maridia_01",
            AvatarSymbol = "🌐"
        });

        // 3. Chief of Staff Executive Memo
        gameState.Workspace.Emails.Add(new EmailMessage
        {
            Id = "mail_cos_intro_01",
            Sender = "Chief of Staff",
            SenderTitle = "Executive Office of the President",
            Recipient = "Presidential Desk",
            Subject = "📋 EXECUTIVE MEMO: Daily Readiness & Priority Briefing",
            Body = $"Mr./Madam President,\n\n" +
                $"All ministries have completed their cycle audits. Treasury liquidity is stable at {country.Treasury.FormattedBalance}, " +
                $"with public approval holding at {country.HappinessRating:0.0}%. " +
                $"Pending items on your desk include Cabinet portfolio confirmations, legislative bills, and bilateral diplomatic overtures.\n\n" +
                $"Office of the Chief of Staff",
            DateSent = DateTime.UtcNow,
            IsRead = true,
            Folder = "Inbox",
            Channel = "Executive",
            Urgency = "Routine",
            ActionRequired = false,
            AvatarSymbol = "💼"
        });
    }

    /// <summary>
    /// Dispatches a high-priority sovereign economic cable announcing the historic milestone of crossing 150 Million REPU in foreign exchange reserves.
    /// Unlocks National Currency Management and Forex Participation.
    /// </summary>
    public static EmailMessage DispatchForexUnlockCable(FullGameState gameState, Country country)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(country);

        var centralBank = country.CentralBank;
        var subject = "🌐 HISTORIC ECONOMIC MILESTONE: Foreign Reserves Cross R 150M — Currency Management & Forex Authorized";
        var body = $"Mr./Madam President,\n\n" +
            $"The Central Bank of {country.Name} officially certifies that our sovereign foreign exchange reserves have crossed R 150,000,000 REPU.\n\n" +
            $"Reaching this critical threshold provides our republic with profound macroeconomic resilience:\n" +
            $"• National Currency Management unlocked: Sovereign float or pegged exchange rate mechanisms authorized.\n" +
            $"• Basic Forex Participation authorized: Bilateral reserve allocations and foreign currency market interventions enabled.\n" +
            $"• Sovereign Credit Standing elevated across global capital markets.\n\n" +
            $"We submit our readiness to execute your foreign exchange directives.\n\n" +
            $"{centralBank.GovernorName}\nGovernor, Central Bank of {country.Name}";

        var email = new EmailMessage
        {
            Id = $"mail_forex_unlock_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Sender = $"Central Bank ({centralBank.GovernorName})",
            SenderTitle = "Central Bank Governor",
            Recipient = "Presidential Desk",
            Subject = subject,
            Body = body,
            DateSent = DateTime.UtcNow,
            IsRead = false,
            Folder = "Inbox",
            Channel = "Executive",
            Urgency = "Priority",
            ActionRequired = false,
            ActionType = "view_forex",
            RelatedEntityId = "forex_desk",
            AvatarSymbol = "🌐"
        };

        StoreMessageInGameState(gameState, email);
        return email;
    }

    private static void StoreMessageInGameState(FullGameState? gameState, EmailMessage message)
    {
        if (gameState?.Workspace == null)
        {
            return;
        }

        gameState.Workspace.Emails ??= new List<EmailMessage>();

        // Remove any message with duplicate ID
        gameState.Workspace.Emails.RemoveAll(e => e.Id == message.Id);

        // Prepend new messages so newest appear at top
        gameState.Workspace.Emails.Insert(0, message);
    }
}
