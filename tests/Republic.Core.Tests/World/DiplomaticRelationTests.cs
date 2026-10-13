namespace Republic.Core.Tests.World;

using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class DiplomaticRelationTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void OpeningRelation_FromAToB_StoresUnrecognizedAndBoundary()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var relation = countryA.OpenRelation(countryB.Id, boundary);

        Assert.NotNull(relation);
        Assert.Equal("country-a", relation!.FromCountryId);
        Assert.Equal("country-b", relation.ToCountryId);
        Assert.Equal(DiplomaticRelationStatus.Unrecognized, relation.Status);
        Assert.True(relation.IsUnrecognized);
        Assert.False(relation.IsRecognized);
        Assert.Equal(boundary, relation.OpenedBoundary);
        Assert.Equal(boundary, relation.Boundary);

        // Stored on source country A
        Assert.Same(relation, countryA.Relation);
        Assert.Same(relation, countryA.DiplomaticRelation);
        Assert.Same(relation, countryA.LastRelation);
        Assert.Single(countryA.Relations);
        Assert.Same(relation, countryA.Relations[0]);
        Assert.True(countryA.HasRelation("country-b"));
        Assert.Same(relation, countryA.GetRelation("country-b"));

        // Implicit boolean evaluates to true
        bool isValid = relation;
        Assert.True(isValid);
    }

    [Fact]
    public void OpeningRelation_SecondOpenOfSamePair_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        // First open succeeds
        var firstRelation = countryA.OpenRelation(countryB.Id, boundary);
        Assert.NotNull(firstRelation);

        // Second open of the exact same ordered pair (country-a, country-b) is rejected
        var laterBoundary = CreateTestTime(12);
        var secondRelation = countryA.OpenRelation(countryB.Id, laterBoundary);

        Assert.Null(secondRelation);
        Assert.Single(countryA.Relations);
        Assert.Same(firstRelation, countryA.Relation);
    }

    [Fact]
    public void OpeningRelation_SelfRelation_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");

        // Self-relation by string id
        var relationById = countryA.OpenRelation(countryA.Id, boundary);
        Assert.Null(relationById);
        Assert.Null(countryA.Relation);
        Assert.Empty(countryA.Relations);

        // Self-relation by country instance
        var relationByObj = countryA.OpenRelation(countryA, boundary);
        Assert.Null(relationByObj);
        Assert.Null(countryA.Relation);
        Assert.Empty(countryA.Relations);
    }

    [Fact]
    public void CountryC_IsUnchanged_WhenCountryAOpensRelationWithCountryB()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");
        var countryC = Country.Found("Kingdom of Solaria", boundary, id: "country-c");

        var relation = countryA.OpenRelation(countryB.Id, boundary);
        Assert.NotNull(relation);

        // Country C is completely untouched
        Assert.Null(countryC.Relation);
        Assert.Null(countryC.DiplomaticRelation);
        Assert.Empty(countryC.Relations);
        Assert.Empty(countryC.DiplomaticRelations);
        Assert.False(countryC.HasRelation("country-a"));
        Assert.False(countryC.HasRelation("country-b"));
        Assert.Null(countryC.GetRelation("country-a"));
        Assert.Null(countryC.GetRelation("country-b"));

        // Country B is also untouched (relations are stored on source country)
        Assert.Null(countryB.Relation);
        Assert.Empty(countryB.Relations);
    }

    [Fact]
    public void ReversePair_IsADifferentRelation_InThisSlice()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        // Country A opens relation to Country B
        var relationAB = countryA.OpenRelation(countryB.Id, boundary);
        Assert.NotNull(relationAB);

        // Country B can independently open relation to Country A
        var relationBA = countryB.OpenRelation(countryA.Id, boundary);
        Assert.NotNull(relationBA);

        Assert.Equal("country-a", relationAB!.FromCountryId);
        Assert.Equal("country-b", relationAB.ToCountryId);

        Assert.Equal("country-b", relationBA!.FromCountryId);
        Assert.Equal("country-a", relationBA.ToCountryId);

        Assert.NotEqual(relationAB, relationBA);
        Assert.Same(relationAB, countryA.Relation);
        Assert.Same(relationBA, countryB.Relation);
    }

    [Fact]
    public void OpenRelation_UsingClock_UsesSimulationClockWithoutDateTimeNow()
    {
        var clock = RepublicClock.CreateControlled();
        var countryA = Country.Found("Republic of Arcadia", clock.CurrentTime, id: "country-a");
        var countryB = Country.Found("Federation of Norse", clock.CurrentTime, id: "country-b");

        var relation = countryA.OpenRelation(countryB, clock);

        Assert.NotNull(relation);
        Assert.Equal(clock.CurrentTime, relation!.OpenedBoundary);
        Assert.Equal("country-a", relation.FromCountryId);
        Assert.Equal("country-b", relation.ToCountryId);
    }

    [Fact]
    public void OpenRelation_UsingCountryObject_WorksCorrectly()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var relation = countryA.OpenRelation(countryB, boundary);

        Assert.NotNull(relation);
        Assert.Equal("country-a", relation!.FromCountryId);
        Assert.Equal("country-b", relation.ToCountryId);
    }

    [Fact]
    public void OpenRelation_WithNullOrEmptyTarget_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");

        Assert.Null(countryA.OpenRelation((string)null!, boundary));
        Assert.Null(countryA.OpenRelation("   ", boundary));
        Assert.Null(countryA.OpenRelation((Country)null!, boundary));
        Assert.Empty(countryA.Relations);
    }

    [Fact]
    public void Deconstruct_ReturnsAllComponents()
    {
        var boundary = CreateTestTime(6);
        var relation = new DiplomaticRelation("c-1", "c-2", DiplomaticRelationStatus.Recognized, boundary);

        var (fromId, toId, status, openedAt) = relation;

        Assert.Equal("c-1", fromId);
        Assert.Equal("c-2", toId);
        Assert.Equal(DiplomaticRelationStatus.Recognized, status);
        Assert.Equal(boundary, openedAt);
    }

    [Fact]
    public void ToString_FormatsRelationCorrectly()
    {
        var boundary = CreateTestTime(6);
        var relation = new DiplomaticRelation("arcadia", "norse", DiplomaticRelationStatus.Unrecognized, boundary);

        var text = relation.ToString();
        Assert.Contains("arcadia -> norse", text);
        Assert.Contains("Unrecognized", text);
    }

    [Fact]
    public void CountryFound_SupportsOptionalRelation()
    {
        var boundary = CreateTestTime(6);
        var historicalRelation = new DiplomaticRelation("arcadia", "norse", DiplomaticRelationStatus.Unrecognized, boundary);
        var country = Country.Found("Republic of Arcadia", boundary, id: "arcadia", relation: historicalRelation);

        Assert.Same(historicalRelation, country.Relation);
        Assert.Single(country.Relations);
    }
}
