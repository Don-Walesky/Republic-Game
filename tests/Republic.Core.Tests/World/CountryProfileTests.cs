namespace Republic.Core.Tests.World;

using System;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;
using Xunit;

public sealed class CountryProfileTests
{
    private readonly CountryProfileFactory _factory = new();

    [Fact]
    public void Test1_ArcadiaDefaults_AreStable_AndNamePassesRepublicRule()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(1);
        var foundingTime = clock.CurrentTime;

        var arcadia = Country.Found(
            name: "Republic of Arcadia",
            foundingTime: foundingTime,
            id: "player-country",
            capitalCity: "Grand Arcadia");

        var profile = arcadia.Profile;

        Assert.NotNull(profile);
        Assert.Equal("Republic of Arcadia", profile.OfficialName);
        Assert.True(CountryNameRule.Default.IsValid(profile.OfficialName));
        Assert.Equal("Grand Arcadia", profile.Capital);
        Assert.Equal(CountryRegion.Central, profile.Region);
        Assert.Equal(10_000_000, profile.Population);
        Assert.Equal("English", profile.DominantLanguage);
        Assert.Equal(GovernmentForm.Federal, profile.GovernmentForm);
        Assert.Equal("Constitution of the Republic of Arcadia", profile.ConstitutionName);
        Assert.Equal("Iron", profile.PrimaryResource);
        Assert.False(string.IsNullOrWhiteSpace(profile.PoliticalCulture));
        Assert.Equal(foundingTime, profile.FoundingTime);

        // Accessing multiple times produces identical, stable defaults
        Assert.Same(profile, arcadia.Profile);
    }

    [Fact]
    public void Test2_Arcadia_WithoutRepublicInName_DefaultsOfficialNameToRepublicOfArcadia()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(2);
        clock.AdvanceHours(6);
        var foundingTime = clock.CurrentTime;

        var country = Country.Found(
            name: "Arcadia",
            foundingTime: foundingTime,
            id: "arcadia-legacy");

        var profile = country.Profile;
        Assert.Equal("Republic of Arcadia", profile.OfficialName);
        Assert.True(CountryNameRule.Default.IsValid(profile.OfficialName));
    }

    [Fact]
    public void Test3_TheSameSeed_ProducesTheSameProfile()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(10);
        clock.AdvanceHours(12);
        var foundingTime = clock.CurrentTime;

        var profile1 = _factory.CreateProfile(42, foundingTime);
        var profile2 = _factory.CreateProfile(42, foundingTime);

        Assert.Equal(profile1, profile2);
        Assert.Equal(profile1.OfficialName, profile2.OfficialName);
        Assert.Equal(profile1.Capital, profile2.Capital);
        Assert.Equal(profile1.Region, profile2.Region);
        Assert.Equal(profile1.Population, profile2.Population);
        Assert.Equal(profile1.DominantLanguage, profile2.DominantLanguage);
        Assert.Equal(profile1.GovernmentForm, profile2.GovernmentForm);
        Assert.Equal(profile1.ConstitutionName, profile2.ConstitutionName);
        Assert.Equal(profile1.PrimaryResource, profile2.PrimaryResource);
        Assert.Equal(profile1.PoliticalCulture, profile2.PoliticalCulture);
        Assert.Equal(profile1.FoundingTime, profile2.FoundingTime);
    }

    [Fact]
    public void Test4_TwoDifferentSeeds_CanDiffer()
    {
        var profileA = _factory.CreateProfile(101);
        var profileB = _factory.CreateProfile(999);

        // Two different seeds should produce different attributes
        Assert.NotEqual(profileA, profileB);
    }

    [Fact]
    public void Test5_Profile_WhoseNameIsWalesky_IsRejected()
    {
        // "Walesky" lacks the word "Republic" and must be rejected by CountryProfile
        var ex = Assert.Throws<ArgumentException>(() => new CountryProfile(officialName: "Walesky"));
        Assert.Contains("Republic", ex.Message);
    }

    [Theory]
    [InlineData("Republic of Walesky")]
    [InlineData("Kalakuta Republic")]
    [InlineData("  republic of walesky  ")]
    public void Test6_Profile_ValidRepublicNames_AreAccepted(string validName)
    {
        var profile = new CountryProfile(officialName: validName);
        Assert.True(CountryNameRule.Default.IsValid(profile.OfficialName));
        Assert.Equal(validName.Trim(), profile.OfficialName);
    }

    [Fact]
    public void Test7_NegativePopulation_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountryProfile(population: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountryProfile(population: -50_000));
    }

    [Fact]
    public void Test8_ReadingProfile_DoesNotChangeTreasuryBalance()
    {
        var clock = RepublicClock.CreateControlled();
        var initialTreasury = 15_000_000.0;
        var country = Country.Found(
            name: "Republic of Solaria",
            foundingTime: clock.CurrentTime,
            treasury: new RepuTreasury(initialTreasury, "solaria-1"));

        Assert.Equal(initialTreasury, country.Treasury.Balance);

        // Read profile multiple times
        var p1 = country.Profile;
        var p2 = country.Profile;

        Assert.NotNull(p1);
        Assert.NotNull(p2);
        Assert.Equal(initialTreasury, country.Treasury.Balance);
    }

    [Fact]
    public void Test9_ReadingCountryAProfile_DoesNotChangeCountryB()
    {
        var clock = RepublicClock.CreateControlled();
        var countryA = _factory.CreateCountry(100, clock.CurrentTime, "country-a", startingTreasury: 500_000.0);
        var countryB = _factory.CreateCountry(200, clock.CurrentTime, "country-b", startingTreasury: 750_000.0);

        var countryBInitialSnapshot = countryB.Profile;
        var countryBInitialTreasury = countryB.Treasury.Balance;

        // Read Country A's profile
        _ = countryA.Profile;

        // Assert Country B remains completely unchanged
        Assert.Equal(countryBInitialSnapshot, countryB.Profile);
        Assert.Equal(countryBInitialTreasury, countryB.Treasury.Balance);
    }

    [Fact]
    public void Test10_MissingText_DefaultsToUnspecified()
    {
        var profile = new CountryProfile(
            officialName: null,
            capital: null,
            dominantLanguage: null,
            constitutionName: null,
            primaryResource: null,
            politicalCulture: null);

        Assert.Equal(CountryProfile.DefaultUnspecified, profile.OfficialName);
        Assert.Equal(CountryProfile.DefaultUnspecified, profile.Capital);
        Assert.Equal(CountryProfile.DefaultUnspecified, profile.DominantLanguage);
        Assert.Equal(CountryProfile.DefaultUnspecified, profile.ConstitutionName);
        Assert.Equal(CountryProfile.DefaultUnspecified, profile.PrimaryResource);
        Assert.Equal(CountryProfile.DefaultUnspecified, profile.PoliticalCulture);
    }

    [Fact]
    public void Test11_FactoryGeneratedCountries_AlwaysPassCountryNameRule()
    {
        for (int seed = 1; seed <= 50; seed++)
        {
            var profile = _factory.CreateProfile(seed);
            Assert.True(
                CountryNameRule.Default.IsValid(profile.OfficialName),
                $"Seed {seed} generated name '{profile.OfficialName}' which failed CountryNameRule.");
        }
    }
}
