namespace Republic.Core.Tests.Politics;

using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class CountryElectionTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void RulingSupport40_OppositionSupport55_MakesOppositionNewRulingParty()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary, id: "player-country");

        var ruling = country.FoundParty("Civic Union", boundary, support: 40.0);
        var opposition = country.FoundParty("National Vanguard", boundary);
        Assert.NotNull(ruling);
        Assert.NotNull(opposition);

        // Set opposition support to 55.0
        country.OppositionParty = opposition with { Support = 55.0 };

        var result = country.HoldElection(boundary);

        Assert.NotNull(result);
        Assert.Equal("National Vanguard", result!.WinnerPartyName);
        Assert.Equal("National Vanguard", result.Winner);
        Assert.Equal("Civic Union", result.LoserPartyName);
        Assert.Equal("Civic Union", result.Loser);
        Assert.Equal(55.0, result.WinnerSupport);
        Assert.Equal(40.0, result.LoserSupport);
        Assert.Equal(boundary, result.Boundary);
        Assert.Equal(boundary, result.HeldBoundary);

        // Opposition is now the new ruling party
        Assert.Equal("National Vanguard", country.RulingParty!.Name);
        Assert.Equal(PoliticalPartyRole.Ruling, country.RulingParty.Role);
        Assert.True(country.RulingParty.IsRuling);
        Assert.False(country.RulingParty.IsOpposition);
        Assert.Equal(55.0, country.RulingParty.Support);

        // Former ruling party is now the opposition
        Assert.Equal("Civic Union", country.OppositionParty!.Name);
        Assert.Equal(PoliticalPartyRole.Opposition, country.OppositionParty.Role);
        Assert.True(country.OppositionParty.IsOpposition);
        Assert.False(country.OppositionParty.IsRuling);
        Assert.Equal(40.0, country.OppositionParty.Support);

        Assert.Same(result, country.LastElection);
        Assert.Same(result, country.Election);
        Assert.Single(country.Elections);

        // Implicit boolean evaluates to true
        bool completed = result;
        Assert.True(completed);
    }

    [Fact]
    public void RulingSupport50_OppositionSupport50_KeepsRulingParty()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var ruling = country.FoundParty("Civic Union", boundary, support: 50.0);
        var opposition = country.FoundParty("National Vanguard", boundary);
        Assert.NotNull(ruling);
        Assert.NotNull(opposition);

        country.OppositionParty = opposition with { Support = 50.0 };

        var result = country.HoldElection(boundary);

        Assert.NotNull(result);
        Assert.Equal("Civic Union", result!.WinnerPartyName);
        Assert.Equal("National Vanguard", result.LoserPartyName);
        Assert.Equal(50.0, result.WinnerSupport);
        Assert.Equal(50.0, result.LoserSupport);

        // Ruling party keeps power on a tie
        Assert.Equal("Civic Union", country.RulingParty!.Name);
        Assert.Equal(PoliticalPartyRole.Ruling, country.RulingParty.Role);
        Assert.Equal("National Vanguard", country.OppositionParty!.Name);
        Assert.Equal(PoliticalPartyRole.Opposition, country.OppositionParty.Role);
    }

    [Fact]
    public void HoldingElection_WithoutOpposition_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var ruling = country.FoundParty("Civic Union", boundary);
        Assert.NotNull(ruling);
        Assert.Null(country.OppositionParty);

        var result = country.HoldElection(boundary);

        Assert.Null(result);
        Assert.Null(country.LastElection);
        Assert.Empty(country.Elections);
        Assert.Equal("Civic Union", country.RulingParty!.Name);
    }

    [Fact]
    public void HoldingElection_WithoutRulingParty_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        Assert.Null(country.RulingParty);
        Assert.Null(country.OppositionParty);

        var result = country.HoldElection(boundary);

        Assert.Null(result);
        Assert.Null(country.LastElection);
        Assert.Empty(country.Elections);
    }

    [Fact]
    public void SecondCallAtSameBoundary_DoesNotSwapAgain()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var ruling = country.FoundParty("Civic Union", boundary, support: 40.0);
        var opposition = country.FoundParty("National Vanguard", boundary);
        Assert.NotNull(ruling);
        Assert.NotNull(opposition);

        country.OppositionParty = opposition with { Support = 55.0 };

        // First election: National Vanguard (55) defeats Civic Union (40)
        var firstElection = country.HoldElection(boundary);
        Assert.NotNull(firstElection);
        Assert.Equal("National Vanguard", country.RulingParty!.Name);
        Assert.Equal("Civic Union", country.OppositionParty!.Name);

        // Second election at the exact same boundary: must do nothing and not swap again
        var secondElection = country.HoldElection(boundary);
        Assert.Same(firstElection, secondElection);

        // Ruling party is STILL National Vanguard; does not swap back
        Assert.Equal("National Vanguard", country.RulingParty!.Name);
        Assert.Equal("Civic Union", country.OppositionParty!.Name);
        Assert.Single(country.Elections);
    }

    [Fact]
    public void SubsequentElection_AtLaterBoundary_CanSwapRolesAgain()
    {
        var boundary1 = CreateTestTime(6);
        var boundary2 = CreateTestTime(12);
        var country = Country.Found("Republic of Arcadia", boundary1);

        country.FoundParty("Civic Union", boundary1, support: 40.0);
        var opposition = country.FoundParty("National Vanguard", boundary1);
        country.OppositionParty = opposition! with { Support = 55.0 };

        // Election 1 at boundary 1: National Vanguard wins
        country.HoldElection(boundary1);
        Assert.Equal("National Vanguard", country.RulingParty!.Name);

        // Later boundary: Civic Union support rises to 65 while Vanguard drops to 30
        country.RulingParty = country.RulingParty! with { Support = 30.0 };
        country.OppositionParty = country.OppositionParty! with { Support = 65.0 };

        // Election 2 at boundary 2: Civic Union wins back ruling mandate
        var secondElection = country.HoldElection(boundary2);
        Assert.NotNull(secondElection);
        Assert.Equal("Civic Union", secondElection!.WinnerPartyName);
        Assert.Equal("Civic Union", country.RulingParty!.Name);
        Assert.Equal("National Vanguard", country.OppositionParty!.Name);
        Assert.Equal(2, country.Elections.Count);
    }

    [Fact]
    public void CountryIsolation_CountryB_IsUnchanged_WhenCountryAHoldsElection()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        // Setup Country A
        countryA.FoundParty("Arcadia Ruling", boundary, support: 30.0);
        var oppA = countryA.FoundParty("Arcadia Opposition", boundary);
        countryA.OppositionParty = oppA! with { Support = 70.0 };

        // Setup Country B
        countryB.FoundParty("Norse Ruling", boundary, support: 60.0);
        var oppB = countryB.FoundParty("Norse Opposition", boundary);
        countryB.OppositionParty = oppB! with { Support = 40.0 };

        // Country A holds election
        var electionA = countryA.HoldElection(boundary);
        Assert.NotNull(electionA);
        Assert.Equal("Arcadia Opposition", countryA.RulingParty!.Name);

        // Country B is completely untouched
        Assert.Null(countryB.LastElection);
        Assert.Empty(countryB.Elections);
        Assert.Equal("Norse Ruling", countryB.RulingParty!.Name);
        Assert.Equal("Norse Opposition", countryB.OppositionParty!.Name);
        Assert.Equal(60.0, countryB.RulingParty.Support);
        Assert.Equal(40.0, countryB.OppositionParty.Support);
    }

    [Fact]
    public void HoldElection_UsingClock_UsesSimulationClockWithoutCallingDateTimeNow()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Republic of Arcadia", clock.CurrentTime);

        country.FoundParty("Civic Union", clock, support: 45.0);
        var opp = country.FoundParty("National Vanguard", clock);
        country.OppositionParty = opp! with { Support = 55.0 };

        var election = country.HoldElection(clock);

        Assert.NotNull(election);
        Assert.Equal(clock.CurrentTime, election!.Boundary);
        Assert.Equal("National Vanguard", country.RulingParty!.Name);
    }

    [Fact]
    public void Deconstruct_ReturnsAllElectionComponents()
    {
        var boundary = CreateTestTime(6);
        var election = new CountryElection("c1", boundary, "Civic Union", "National Vanguard", 60.0, 40.0);

        var (countryId, b, winner, loser, winSupport, loseSupport) = election;

        Assert.Equal("c1", countryId);
        Assert.Equal(boundary, b);
        Assert.Equal("Civic Union", winner);
        Assert.Equal("National Vanguard", loser);
        Assert.Equal(60.0, winSupport);
        Assert.Equal(40.0, loseSupport);
    }

    [Fact]
    public void ToString_FormatsWinnerLoserAndPercentagesCorrectly()
    {
        var boundary = CreateTestTime(6);
        var election = new CountryElection("c1", boundary, "Civic Union", "National Vanguard", 60.0, 40.0);

        var text = election.ToString();
        Assert.Contains("Civic Union won at", text);
        Assert.Contains("Civic Union: 60%", text);
        Assert.Contains("National Vanguard: 40%", text);
    }

    [Fact]
    public void CountryFound_SupportsOptionalElection()
    {
        var boundary = CreateTestTime(6);
        var historicalElection = new CountryElection("arcadia", boundary, "Civic Union", "National Vanguard", 55.0, 45.0);
        var country = Country.Found("Republic of Arcadia", boundary, id: "arcadia", election: historicalElection);

        Assert.Same(historicalElection, country.LastElection);
        Assert.Single(country.Elections);
    }
}
