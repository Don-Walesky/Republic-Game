namespace Republic.Core.Tests.Politics;

using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class PoliticalPartyTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void Founding_CivicUnion_StoresNameAndSupport10()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary, id: "player-country");

        var party = country.FoundParty("Civic Union", boundary);

        Assert.NotNull(party);
        Assert.Equal("Civic Union", party.Name);
        Assert.Equal(10.0, party.Support);
        Assert.Equal("player-country", party.CountryId);
        Assert.Equal(boundary, party.FoundedBoundary);
        Assert.Same(party, country.Party);
        Assert.Same(party, country.PoliticalParty);
    }

    [Fact]
    public void Founding_WithWhitespaceName_TrimsName()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var party = country.FoundParty("   Civic Union   ", boundary);

        Assert.NotNull(party);
        Assert.Equal("Civic Union", party.Name);
        Assert.Equal(10.0, party.Support);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyName_IsRejected_AndDoesNotCreateParty(string? invalidName)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var party = country.FoundParty(invalidName, boundary);

        Assert.Null(party);
        Assert.Null(country.Party);
        Assert.Null(country.PoliticalParty);
    }

    [Fact]
    public void PoliticalPartyConstructor_WithEmptyName_ThrowsArgumentException()
    {
        var boundary = CreateTestTime(6);
        Assert.Throws<ArgumentException>(() => new PoliticalParty("", "c1", boundary));
        Assert.Throws<ArgumentException>(() => new PoliticalParty("   ", "c1", boundary));
        Assert.Throws<ArgumentNullException>(() => new PoliticalParty(null!, "c1", boundary));
    }

    [Fact]
    public void SecondFounding_OnSameCountry_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var firstParty = country.FoundParty("Civic Union", boundary);
        Assert.NotNull(firstParty);
        Assert.Equal("Civic Union", country.Party?.Name);

        var secondParty = country.FoundParty("National Vanguard", boundary);

        Assert.Null(secondParty);
        Assert.Same(firstParty, country.Party);
        Assert.Equal("Civic Union", country.Party?.Name);
        Assert.Equal(10.0, country.Party?.Support);
    }

    [Fact]
    public void CountryIsolation_CountryB_DoesNotReceiveCountryAParty()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var partyA = countryA.FoundParty("Civic Union", boundary);

        Assert.NotNull(partyA);
        Assert.Equal("Civic Union", countryA.Party?.Name);
        Assert.Equal("country-a", countryA.Party?.CountryId);

        // Country B is completely isolated and does not receive Country A's party
        Assert.Null(countryB.Party);
        Assert.Null(countryB.PoliticalParty);

        // Country B can independently found its own party
        var partyB = countryB.FoundParty("Norse Progress", boundary);

        Assert.NotNull(partyB);
        Assert.Equal("Norse Progress", countryB.Party?.Name);
        Assert.Equal("country-b", countryB.Party?.CountryId);
        Assert.NotSame(countryA.Party, countryB.Party);
        Assert.Equal("Civic Union", countryA.Party?.Name);
    }

    [Fact]
    public void ClockOverload_UsesClockBoundary_WithoutCallingDateTimeNow()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Republic of Arcadia", clock.CurrentTime);

        var party = country.FoundParty("Civic Union", clock);

        Assert.NotNull(party);
        Assert.Equal(clock.CurrentTime, party.FoundedBoundary);
        Assert.Equal(country.Id, party.CountryId);
    }

    [Fact]
    public void ToString_FormatsNameAndSupportCorrectly()
    {
        var boundary = CreateTestTime(6);
        var party = new PoliticalParty("Civic Union", "country-1", boundary, 10.0);

        Assert.Equal("Civic Union (Support: 10%)", party.ToString());
    }

    [Fact]
    public void Deconstruct_ReturnsAllComponents()
    {
        var boundary = CreateTestTime(6);
        var party = new PoliticalParty("Civic Union", "country-1", boundary, 10.0);

        var (name, countryId, foundedBoundary, support) = party;

        Assert.Equal("Civic Union", name);
        Assert.Equal("country-1", countryId);
        Assert.Equal(boundary, foundedBoundary);
        Assert.Equal(10.0, support);
    }
}
