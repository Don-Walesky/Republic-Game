namespace Republic.Core.Legislature.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing the 100-seat National Assembly / High Chamber of Parliament.
/// Tracks seat breakdown between President's Ruling Coalition, Opposition, and Independents.
/// Provides political-technocratic courtship mechanisms for floor-crossing and defection.
/// </summary>
public sealed class ParliamentaryChamber
{
    public const int DefaultTotalSeats = 100;
    public const int BaselineRulingSeats = 48;
    public const int BaselineOppositionSeats = 42;
    public const int BaselineIndependentSeats = 10;

    public int TotalSeats { get; set; } = DefaultTotalSeats;
    public int RulingPartySeats { get; set; } = BaselineRulingSeats;
    public int OppositionSeats { get; set; } = BaselineOppositionSeats;
    public int IndependentSeats { get; set; } = BaselineIndependentSeats;

    public List<Legislator> Lawmakers { get; set; } = new();

    public ParliamentaryChamber()
    {
        InitializeDefaultLawmakers();
    }

    public void InitializeDefaultLawmakers()
    {
        if (Lawmakers != null && Lawmakers.Count > 0)
        {
            return;
        }

        Lawmakers = new List<Legislator>
        {
            new Legislator
            {
                Id = "lawmaker_thorne",
                Name = "Hon. Marcus Thorne",
                Constituency = "Industrial Rustbelt District",
                OriginalParty = "Opposition",
                CurrentParty = "Opposition",
                SeatWeight = 2,
                Stance = "Fiscal Conservative & Heavy Industry Advocate",
                LoyaltyToOriginal = 65.0
            },
            new Legislator
            {
                Id = "lawmaker_sterling",
                Name = "Hon. Clara Sterling",
                Constituency = "Capital Metropolitan District",
                OriginalParty = "Independent",
                CurrentParty = "Independent",
                SeatWeight = 1,
                Stance = "Centrist Technocrat & Healthcare Reformer",
                LoyaltyToOriginal = 45.0
            },
            new Legislator
            {
                Id = "lawmaker_mercer",
                Name = "Hon. Julian Mercer",
                Constituency = "Agrarian Heartland & River Basin",
                OriginalParty = "Opposition",
                CurrentParty = "Opposition",
                SeatWeight = 2,
                Stance = "Moderate Agrarian & Water Infrastructure Defender",
                LoyaltyToOriginal = 55.0
            },
            new Legislator
            {
                Id = "lawmaker_diallo",
                Name = "Hon. Amara Diallo",
                Constituency = "University Corridor & Tech Hub",
                OriginalParty = "Independent",
                CurrentParty = "Independent",
                SeatWeight = 1,
                Stance = "Green Energy & AI Research Pioneer",
                LoyaltyToOriginal = 40.0
            },
            new Legislator
            {
                Id = "lawmaker_briggs",
                Name = "Hon. Donald Briggs",
                Constituency = "Coastal Seaport & Maritime District",
                OriginalParty = "Opposition",
                CurrentParty = "Opposition",
                SeatWeight = 2,
                Stance = "Nationalist Hardliner & Old Guard Machine Politician",
                LoyaltyToOriginal = 75.0
            }
        };
    }

    public string ChamberStatus =>
        RulingPartySeats >= 67 ? "Supermajority" :
        RulingPartySeats >= 51 ? "Stable Majority" :
        "Hung Parliament / Minority";

    public bool HasWorkingMajority => RulingPartySeats >= 51;

    /// <summary>
    /// Courts a target legislator to defect to the President's ruling party.
    /// Balances technocratic development incentives against raw political patronage and corruption.
    /// </summary>
    public (bool Success, string Message) CourtLawmaker(string legislatorId, string incentiveType, Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        var lawmaker = Lawmakers.FirstOrDefault(l =>
            string.Equals(l.Id, legislatorId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(l.Name, legislatorId, StringComparison.OrdinalIgnoreCase));

        if (lawmaker == null)
        {
            return (false, $"Lawmaker '{legislatorId}' not found in parliamentary assembly roster.");
        }

        if (lawmaker.HasDefectedToRulingParty)
        {
            return (false, $"{lawmaker.Name} has already crossed the aisle and sits with the President's party.");
        }

        var normIncentive = incentiveType?.Trim().ToLowerInvariant() ?? "district_grant";

        if (normIncentive is "district_grant" or "grant" or "technocrat")
        {
            const double cost = 250_000.0;
            if (country.Treasury.Balance < cost)
            {
                return (false, $"Insufficient funds for District Development Grant (Requires R 250,000, Available: {country.Treasury.FormattedBalance}).");
            }

            country.Treasury.TryWithdraw(cost);
            country.AdjustCorruption(-1.0, $"Transparent constituency development grant in {lawmaker.Constituency}");
            country.BaselineStability = Math.Clamp(country.BaselineStability + 1.0, 0.0, 100.0);
            country.HappinessRating = Math.Clamp(country.HappinessRating + 1.5, 0.0, 100.0);
            lawmaker.DefectionIncentive = "District Development Grant (Technocratic)";
        }
        else if (normIncentive is "committee_chair" or "chair" or "committee")
        {
            // Committee chairing causes minor internal ruling party friction
            country.HappinessRating = Math.Clamp(country.HappinessRating - 1.0, 0.0, 100.0);
            lawmaker.DefectionIncentive = "Committee Chairmanship (Institutional Power)";
        }
        else if (normIncentive is "executive_patronage" or "patronage" or "war_chest")
        {
            const double cost = 150_000.0;
            if (country.Treasury.Balance < cost)
            {
                return (false, $"Insufficient funds for Executive Campaign Patronage (Requires R 150,000, Available: {country.Treasury.FormattedBalance}).");
            }

            country.Treasury.TryWithdraw(cost);
            country.AdjustCorruption(2.5, $"Discretionary executive political patronage to sway lawmaker {lawmaker.Name}");
            lawmaker.DefectionIncentive = "Executive Patronage (Machine Politics)";
        }
        else
        {
            return (false, $"Unrecognized courtship incentive '{incentiveType}'.");
        }

        // Apply seat defection
        lawmaker.HasDefectedToRulingParty = true;
        lawmaker.CurrentParty = "Ruling";

        RulingPartySeats += lawmaker.SeatWeight;
        if (string.Equals(lawmaker.OriginalParty, "Opposition", StringComparison.OrdinalIgnoreCase))
        {
            OppositionSeats = Math.Max(0, OppositionSeats - lawmaker.SeatWeight);
        }
        else
        {
            IndependentSeats = Math.Max(0, IndependentSeats - lawmaker.SeatWeight);
        }

        string majorityNote = HasWorkingMajority ? " [GOVERNMENT NOW HOLDS WORKING MAJORITY]" : " [MINORITY GOVERNMENT STILL ACTIVE]";
        return (true, $"{lawmaker.Name} has crossed the floor and officially joined the President's Ruling Coalition (+{lawmaker.SeatWeight} seat(s)). Chamber status: {RulingPartySeats}/100 seats{majorityNote}.");
    }
}

