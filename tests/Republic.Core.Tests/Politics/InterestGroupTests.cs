namespace Republic.Core.Tests.Politics;

using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class InterestGroupTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void Founding_WithFocusLabor_StoresNameFocusAndInfluence15()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary, id: "player-country");

        var group = country.FoundInterestGroup("Trade Union", InterestGroupFocus.Labor);

        Assert.NotNull(group);
        Assert.Equal("Trade Union", group.Name);
        Assert.Equal(InterestGroupFocus.Labor, group.Focus);
        Assert.Equal(15.0, group.Influence);
        Assert.Equal("player-country", group.CountryId);
        Assert.Same(group, country.InterestGroup);
        Assert.Single(country.InterestGroups);
        Assert.Same(group, country.InterestGroups[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyName_IsRejected_AndDoesNotCreateGroup(string? invalidName)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var group = country.FoundInterestGroup(invalidName, InterestGroupFocus.Labor);

        Assert.Null(group);
        Assert.Null(country.InterestGroup);
        Assert.Empty(country.InterestGroups);
    }

    [Fact]
    public void SecondGroup_OnSameCountry_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var first = country.FoundInterestGroup("Trade Union", InterestGroupFocus.Labor);
        Assert.NotNull(first);

        var second = country.FoundInterestGroup("Chamber of Commerce", InterestGroupFocus.Business);

        Assert.Null(second);
        Assert.Same(first, country.InterestGroup);
        Assert.Equal("Trade Union", country.InterestGroup!.Name);
        Assert.Equal(InterestGroupFocus.Labor, country.InterestGroup!.Focus);
        Assert.Equal(15.0, country.InterestGroup!.Influence);
        Assert.Single(country.InterestGroups);
    }

    [Fact]
    public void CountryIsolation_CountryB_DoesNotReceiveCountryAGroup()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var groupA = countryA.FoundInterestGroup("Trade Union", InterestGroupFocus.Labor);

        Assert.NotNull(groupA);
        Assert.Null(countryB.InterestGroup);
        Assert.Empty(countryB.InterestGroups);

        // Country B can independently found its own interest group
        var groupB = countryB.FoundInterestGroup("Chamber of Commerce", InterestGroupFocus.Business);

        Assert.NotNull(groupB);
        Assert.Equal("Chamber of Commerce", countryB.InterestGroup?.Name);
        Assert.Equal(InterestGroupFocus.Business, countryB.InterestGroup?.Focus);
        Assert.Equal("country-b", countryB.InterestGroup?.CountryId);

        // Country A remains completely untouched
        Assert.Equal("Trade Union", countryA.InterestGroup?.Name);
        Assert.Equal(InterestGroupFocus.Labor, countryA.InterestGroup?.Focus);
        Assert.Equal("country-a", countryA.InterestGroup?.CountryId);
    }

    [Fact]
    public void Founding_WithWhitespaceName_TrimsName()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var group = country.FoundInterestGroup("   Trade Union   ", InterestGroupFocus.Labor);

        Assert.NotNull(group);
        Assert.Equal("Trade Union", group.Name);
    }

    [Fact]
    public void Constructor_WithEmptyOrWhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new InterestGroup("", "c1", InterestGroupFocus.Labor));
        Assert.Throws<ArgumentException>(() => new InterestGroup("   ", "c1", InterestGroupFocus.Labor));
        Assert.Throws<ArgumentNullException>(() => new InterestGroup(null!, "c1", InterestGroupFocus.Labor));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(100.1)]
    [InlineData(-50.0)]
    public void Constructor_WithInvalidInfluence_ThrowsArgumentOutOfRangeException(double invalidInfluence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InterestGroup("Trade Union", "c1", InterestGroupFocus.Labor, invalidInfluence));
    }

    [Theory]
    [InlineData(InterestGroupFocus.Labor)]
    [InlineData(InterestGroupFocus.Business)]
    [InlineData(InterestGroupFocus.Farmers)]
    [InlineData(InterestGroupFocus.Students)]
    [InlineData(InterestGroupFocus.CivilService)]
    public void AllFocusValues_CanBeFounded(InterestGroupFocus focus)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found($"Republic of {focus}", boundary);

        var group = country.FoundInterestGroup($"Alliance of {focus}", focus);

        Assert.NotNull(group);
        Assert.Equal(focus, group.Focus);
        Assert.Equal(15.0, group.Influence);
    }

    [Fact]
    public void ToString_FormatsNameFocusAndInfluenceCorrectly()
    {
        var group = new InterestGroup("Trade Union", "country-1", InterestGroupFocus.Labor, 15.0);

        Assert.Equal("Trade Union (Labor, Influence: 15%)", group.ToString());
    }

    [Fact]
    public void Deconstruct_ReturnsAllComponents()
    {
        var group = new InterestGroup("Trade Union", "country-1", InterestGroupFocus.Labor, 15.0);

        var (name, countryId, focus, influence) = group;

        Assert.Equal("Trade Union", name);
        Assert.Equal("country-1", countryId);
        Assert.Equal(InterestGroupFocus.Labor, focus);
        Assert.Equal(15.0, influence);
    }

    [Fact]
    public void LegacyDeconstruct_ThreeComponents_WorksCorrectly()
    {
        var group = new InterestGroup("Trade Union", "country-1", InterestGroupFocus.Labor, 15.0);

        var (name, focus, influence) = group;

        Assert.Equal("Trade Union", name);
        Assert.Equal(InterestGroupFocus.Labor, focus);
        Assert.Equal(15.0, influence);
    }

    [Fact]
    public void TryFoundInterestGroup_ReturnsTrueOnSuccess_AndFalseOnRejection()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var success1 = country.TryFoundInterestGroup("Trade Union", InterestGroupFocus.Labor, out var group1);
        Assert.True(success1);
        Assert.NotNull(group1);
        Assert.Equal("Trade Union", group1!.Name);

        // Second founding rejected
        var success2 = country.TryFoundInterestGroup("Business Council", InterestGroupFocus.Business, out var group2);
        Assert.False(success2);
        Assert.Null(group2);
        Assert.Same(group1, country.InterestGroup);
    }

    [Fact]
    public void CountryFound_FactoryMethod_SupportsOptionalInterestGroup()
    {
        var boundary = CreateTestTime(6);
        var initialGroup = new InterestGroup("Agrarian Union", "arcadia", InterestGroupFocus.Farmers, 20.0);
        var country = Country.Found("Republic of Arcadia", boundary, id: "arcadia", interestGroup: initialGroup);

        Assert.Same(initialGroup, country.InterestGroup);
        Assert.Single(country.InterestGroups);

        // Cannot found another group
        var second = country.FoundInterestGroup("Civil League", InterestGroupFocus.CivilService);
        Assert.Null(second);
        Assert.Same(initialGroup, country.InterestGroup);
    }
}
