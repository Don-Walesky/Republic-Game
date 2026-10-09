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
    public void FirstParty_IsRuling_AndSecondParty_IsOpposition()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary, id: "player-country");

        var ruling = country.FoundParty("Civic Union", boundary);

        Assert.NotNull(ruling);
        Assert.Equal("Civic Union", ruling.Name);
        Assert.Equal(10.0, ruling.Support);
        Assert.Equal(PoliticalPartyRole.Ruling, ruling.Role);
        Assert.True(ruling.IsRuling);
        Assert.False(ruling.IsOpposition);
        Assert.Same(ruling, country.RulingParty);
        Assert.Same(ruling, country.Party);
        Assert.Same(ruling, country.PoliticalParty);
        Assert.Null(country.OppositionParty);

        // Adjust ruling support to verify it stays where it is when opposition is founded
        var updatedRuling = ruling with { Support = 25.0 };
        country.RulingParty = updatedRuling;

        var opposition = country.FoundParty("National Vanguard", boundary);

        Assert.NotNull(opposition);
        Assert.Equal("National Vanguard", opposition.Name);
        Assert.Equal(10.0, opposition.Support);
        Assert.Equal(PoliticalPartyRole.Opposition, opposition.Role);
        Assert.True(opposition.IsOpposition);
        Assert.False(opposition.IsRuling);
        Assert.Same(opposition, country.OppositionParty);

        // Ruling support stays where it is; opposition starts at 10
        Assert.Equal(25.0, country.RulingParty!.Support);
        Assert.Equal(10.0, country.OppositionParty!.Support);
        Assert.Equal(2, country.Parties.Count);
        Assert.Same(country.RulingParty, country.Parties[0]);
        Assert.Same(country.OppositionParty, country.Parties[1]);
    }

    [Theory]
    [InlineData("Civic Union")]
    [InlineData("civic union")]
    [InlineData("CIVIC UNION")]
    [InlineData("  Civic Union  ")]
    [InlineData("  civic union  ")]
    public void DuplicateName_IsRejected(string duplicateName)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var first = country.FoundParty("Civic Union", boundary);
        Assert.NotNull(first);

        var second = country.FoundParty(duplicateName, boundary);

        Assert.Null(second);
        Assert.Null(country.OppositionParty);
        Assert.Equal("Civic Union", country.RulingParty?.Name);
        Assert.Single(country.Parties);
    }

    [Fact]
    public void ThirdParty_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var first = country.FoundParty("Civic Union", boundary);
        var second = country.FoundParty("National Vanguard", boundary);

        Assert.NotNull(first);
        Assert.NotNull(second);

        var third = country.FoundParty("Democratic Coalition", boundary);

        Assert.Null(third);
        Assert.Equal("Civic Union", country.RulingParty?.Name);
        Assert.Equal("National Vanguard", country.OppositionParty?.Name);
        Assert.Equal(2, country.Parties.Count);
    }

    [Fact]
    public void CountryIsolation_CountryB_DoesNotReceiveCountryAParties()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var partyA1 = countryA.FoundParty("Civic Union", boundary);
        var partyA2 = countryA.FoundParty("National Vanguard", boundary);

        Assert.NotNull(partyA1);
        Assert.NotNull(partyA2);

        // Country B is completely isolated
        Assert.Null(countryB.RulingParty);
        Assert.Null(countryB.OppositionParty);
        Assert.Empty(countryB.Parties);

        // Country B can independently found its own ruling and opposition parties
        var partyB1 = countryB.FoundParty("Norse Progress", boundary);
        var partyB2 = countryB.FoundParty("Nordic Alliance", boundary);

        Assert.NotNull(partyB1);
        Assert.NotNull(partyB2);
        Assert.Equal("Norse Progress", countryB.RulingParty?.Name);
        Assert.Equal("Nordic Alliance", countryB.OppositionParty?.Name);
        Assert.Equal("country-b", countryB.RulingParty?.CountryId);
        Assert.Equal("country-b", countryB.OppositionParty?.CountryId);

        // Country A remains unaffected
        Assert.Equal("Civic Union", countryA.RulingParty?.Name);
        Assert.Equal("National Vanguard", countryA.OppositionParty?.Name);
        Assert.Equal("country-a", countryA.RulingParty?.CountryId);
        Assert.Equal("country-a", countryA.OppositionParty?.CountryId);
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
        Assert.Null(country.RulingParty);
        Assert.Null(country.OppositionParty);
        Assert.Null(country.Party);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyNameForOpposition_IsRejected(string? invalidName)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var ruling = country.FoundParty("Civic Union", boundary);
        Assert.NotNull(ruling);

        var opposition = country.FoundParty(invalidName, boundary);

        Assert.Null(opposition);
        Assert.Null(country.OppositionParty);
        Assert.Equal("Civic Union", country.RulingParty?.Name);
    }

    [Fact]
    public void Founding_WithWhitespaceName_TrimsName()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var ruling = country.FoundParty("   Civic Union   ", boundary);
        var opposition = country.FoundParty("   National Vanguard   ", boundary);

        Assert.NotNull(ruling);
        Assert.NotNull(opposition);
        Assert.Equal("Civic Union", ruling.Name);
        Assert.Equal("National Vanguard", opposition.Name);
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
    public void ClockOverload_UsesClockBoundary_WithoutCallingDateTimeNow()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Republic of Arcadia", clock.CurrentTime);

        var ruling = country.FoundParty("Civic Union", clock);
        var opposition = country.FoundParty("National Vanguard", clock);

        Assert.NotNull(ruling);
        Assert.NotNull(opposition);
        Assert.Equal(clock.CurrentTime, ruling.FoundedBoundary);
        Assert.Equal(clock.CurrentTime, opposition.FoundedBoundary);
        Assert.Equal(country.Id, ruling.CountryId);
        Assert.Equal(country.Id, opposition.CountryId);
    }

    [Fact]
    public void ToString_FormatsNameAndSupportCorrectly()
    {
        var boundary = CreateTestTime(6);
        var party = new PoliticalParty("Civic Union", "country-1", boundary, 10.0, PoliticalPartyRole.Ruling);

        Assert.Equal("Civic Union (Support: 10%)", party.ToString());
    }

    [Fact]
    public void Deconstruct_ReturnsAllComponents()
    {
        var boundary = CreateTestTime(6);
        var party = new PoliticalParty("Civic Union", "country-1", boundary, 10.0, PoliticalPartyRole.Ruling);

        var (name, countryId, foundedBoundary, support, role) = party;

        Assert.Equal("Civic Union", name);
        Assert.Equal("country-1", countryId);
        Assert.Equal(boundary, foundedBoundary);
        Assert.Equal(10.0, support);
        Assert.Equal(PoliticalPartyRole.Ruling, role);
    }

    [Fact]
    public void LegacyDeconstruct_FourComponents_WorksCorrectly()
    {
        var boundary = CreateTestTime(6);
        var party = new PoliticalParty("Civic Union", "country-1", boundary, 10.0, PoliticalPartyRole.Ruling);

        var (name, countryId, foundedBoundary, support) = party;

        Assert.Equal("Civic Union", name);
        Assert.Equal("country-1", countryId);
        Assert.Equal(boundary, foundedBoundary);
        Assert.Equal(10.0, support);
    }
}
