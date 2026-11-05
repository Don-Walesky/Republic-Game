namespace Republic.Core.Tests.Politics;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Legislature.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class LegislatureChamberTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void ParliamentaryChamber_Defaults_To_100Seats_And_HungParliament()
    {
        var chamber = new ParliamentaryChamber();
        Assert.Equal(100, chamber.TotalSeats);
        Assert.Equal(48, chamber.RulingPartySeats);
        Assert.Equal(42, chamber.OppositionSeats);
        Assert.Equal(10, chamber.IndependentSeats);
        Assert.False(chamber.HasWorkingMajority);
        Assert.Equal("Hung Parliament / Minority", chamber.ChamberStatus);
        Assert.Equal(5, chamber.Lawmakers.Count);
    }

    [Fact]
    public void Courting_Lawmaker_With_DistrictGrant_DeductsFunds_LowersCorruption_AndFlipsSeat()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(1_000_000.0);
        var initialTreasury = country.Treasury.Balance;
        var initialCorruption = country.CorruptionIndex;

        var chamber = country.Legislature;
        var (success, msg) = chamber.CourtLawmaker("lawmaker_sterling", "district_grant", country);

        Assert.True(success);
        Assert.Contains("Hon. Clara Sterling has crossed the floor", msg);
        Assert.Equal(initialTreasury - 250_000.0, country.Treasury.Balance);
        Assert.Equal(initialCorruption - 1.0, country.CorruptionIndex);
        Assert.Equal(49, chamber.RulingPartySeats); // 48 + 1
        Assert.Equal(9, chamber.IndependentSeats); // 10 - 1

        var sterling = chamber.Lawmakers.First(l => l.Id == "lawmaker_sterling");
        Assert.True(sterling.HasDefectedToRulingParty);
        Assert.Equal("Ruling", sterling.CurrentParty);
    }

    [Fact]
    public void Courting_Lawmaker_With_Patronage_RaisesCorruption_AndAchievesMajority()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(2_000_000.0);
        var initialCorruption = country.CorruptionIndex;

        var chamber = country.Legislature;

        // Court Clara Sterling (+1 seat -> 49)
        chamber.CourtLawmaker("lawmaker_sterling", "committee_chair", country);
        Assert.Equal(49, chamber.RulingPartySeats);
        Assert.False(chamber.HasWorkingMajority);

        // Court Marcus Thorne with patronage (+2 seats -> 51 => Stable Majority!)
        var (success, msg) = chamber.CourtLawmaker("lawmaker_thorne", "executive_patronage", country);
        Assert.True(success);
        Assert.True(chamber.RulingPartySeats >= 51);
        Assert.True(chamber.HasWorkingMajority);
        Assert.Equal("Stable Majority", chamber.ChamberStatus);
        Assert.Equal(initialCorruption + 2.5, country.CorruptionIndex);
        Assert.Contains("WORKING MAJORITY", msg);
    }

    [Fact]
    public void Courting_AlreadyDefected_Lawmaker_ReturnsFalse()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(1_000_000.0);

        var chamber = country.Legislature;
        var (first, _) = chamber.CourtLawmaker("lawmaker_diallo", "district_grant", country);
        Assert.True(first);

        var (second, secondMsg) = chamber.CourtLawmaker("lawmaker_diallo", "committee_chair", country);
        Assert.False(second);
        Assert.Contains("already crossed the aisle", secondMsg);
    }
}

