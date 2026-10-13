namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class BilateralDiplomacyTests
{
    private static RepublicTime CreateTestTime(int day = 0, int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(day, new TimeOnly(hour, 0));
    }

    private static (Country Player, Country Norse, Country Aethel, List<Country> World) SetupTestWorld(RepublicTime time)
    {
        var player = Country.Found("Republic of Arcadia", time, id: "player-country");
        var norse = Country.Found("Federation of Norse", time, id: "norse-country");
        var aethel = Country.Found("Aethel Kingdom", time, id: "aethel-country");
        var world = new List<Country> { player, norse, aethel };
        return (player, norse, aethel, world);
    }

    [Fact]
    public void SelectTarget_ValidTarget_CanBeSelected()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, world) = SetupTestWorld(time);

        // In 1-based index: index 2 is Federation of Norse
        var (cancelled, selected, error) = BilateralDiplomacyHandler.SelectTarget(2, world, player);

        Assert.False(cancelled);
        Assert.Null(error);
        Assert.NotNull(selected);
        Assert.Equal("norse-country", selected!.Id);
        Assert.Equal("Federation of Norse", selected.Name);
    }

    [Fact]
    public void SelectTarget_PlayerOwnCountry_CannotBeSelected()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        // In 1-based index: index 1 is player country
        var (cancelled, selected, error) = BilateralDiplomacyHandler.SelectTarget(1, world, player);

        Assert.False(cancelled);
        Assert.Null(selected);
        Assert.NotNull(error);
        Assert.Contains("Self-relations are prohibited", error);
    }

    [Fact]
    public void SelectTarget_Zero_IsCancelled()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        var (cancelled, selected, error) = BilateralDiplomacyHandler.SelectTarget(0, world, player);

        Assert.True(cancelled);
        Assert.Null(selected);
    }

    [Fact]
    public void SelectTarget_OutOfBounds_ReturnsError()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        var (cancelled, selected, error) = BilateralDiplomacyHandler.SelectTarget(99, world, player);

        Assert.False(cancelled);
        Assert.Null(selected);
        Assert.NotNull(error);
        Assert.Contains("Invalid selection number", error);
    }

    [Fact]
    public void OpenRelation_NonexistentTarget_IsRejected()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        var outcome = BilateralDiplomacyHandler.OpenRelation(player, "nonexistent-id", world, time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.TargetNotFound, outcome.Result);
        Assert.Contains("does not exist in the world registry", outcome.Message);
        Assert.Null(outcome.Relation);
        Assert.Empty(player.Relations);
    }

    [Fact]
    public void OpenRelation_PlayerOwnCountry_IsRejected()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        var outcome = BilateralDiplomacyHandler.OpenRelation(player, player.Id, world, time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.SelfRelationProhibited, outcome.Result);
        Assert.Contains("Self-relations are prohibited", outcome.Message);
        Assert.Null(outcome.Relation);
        Assert.Empty(player.Relations);
    }

    [Fact]
    public void OpenRelation_SuccessfulAction_RecordsRelationAtCurrentSimulationTime()
    {
        var time = CreateTestTime(day: 2, hour: 12);
        var (player, norse, _, world) = SetupTestWorld(time);

        var outcome = BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, time);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.Success, outcome.Result);
        Assert.NotNull(outcome.Relation);
        Assert.Equal("player-country", outcome.Relation!.FromCountryId);
        Assert.Equal("norse-country", outcome.Relation.ToCountryId);
        Assert.Equal(DiplomaticRelationStatus.Unrecognized, outcome.Relation.Status);
        Assert.Equal(time, outcome.Relation.OpenedBoundary);

        // Stored on player country
        Assert.Same(outcome.Relation, player.Relation);
        Assert.Single(player.Relations);
    }

    [Fact]
    public void OpenRelation_DuplicateRelation_IsRejectedWithoutChangingExistingRelation()
    {
        var time1 = CreateTestTime(day: 0, hour: 6);
        var (player, norse, _, world) = SetupTestWorld(time1);

        // First action succeeds
        var firstOutcome = BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, time1);
        Assert.True(firstOutcome.IsSuccess);
        var initialRelation = firstOutcome.Relation;
        Assert.NotNull(initialRelation);

        // Duplicate action at a later boundary
        var time2 = CreateTestTime(day: 1, hour: 18);
        var secondOutcome = BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, time2);

        Assert.False(secondOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AlreadyExists, secondOutcome.Result);
        Assert.Contains("already exists", secondOutcome.Message);
        Assert.Same(initialRelation, secondOutcome.Relation);

        // State remains unchanged: 1 relation with initial timestamp
        Assert.Single(player.Relations);
        Assert.Same(initialRelation, player.Relation);
        Assert.Equal(time1, player.Relation!.OpenedBoundary);
    }

    [Fact]
    public void OpenRelation_InvalidOrWhitespaceInput_DoesNotChangeWorld()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, world) = SetupTestWorld(time);

        var outcomeEmpty = BilateralDiplomacyHandler.OpenRelation(player, "", world, time);
        var outcomeNull = BilateralDiplomacyHandler.OpenRelation(player, null, world, time);
        var outcomeWhitespace = BilateralDiplomacyHandler.OpenRelation(player, "   ", world, time);

        Assert.False(outcomeEmpty.IsSuccess);
        Assert.False(outcomeNull.IsSuccess);
        Assert.False(outcomeWhitespace.IsSuccess);
        Assert.Equal(DiplomaticActionResult.InvalidInput, outcomeEmpty.Result);

        // All countries completely unchanged
        Assert.Empty(player.Relations);
        Assert.Empty(norse.Relations);
        Assert.Empty(aethel.Relations);
    }

    [Fact]
    public void OpenRelation_PreservesTargetAndThirdPartyIsolation()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, world) = SetupTestWorld(time);

        var outcome = BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, time);
        Assert.True(outcome.IsSuccess);

        // Target (Norse) does NOT silently get a reverse relation from B to A
        Assert.Null(norse.Relation);
        Assert.Empty(norse.Relations);

        // Third party (Aethel Kingdom) remains completely untouched
        Assert.Null(aethel.Relation);
        Assert.Empty(aethel.Relations);
    }

    [Fact]
    public void GetEligibleTargets_FiltersOutPlayerCountry()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, world) = SetupTestWorld(time);

        var eligible = BilateralDiplomacyHandler.GetEligibleTargets(player, world);

        Assert.Equal(2, eligible.Count);
        Assert.DoesNotContain(player, eligible);
        Assert.Contains(norse, eligible);
        Assert.Contains(aethel, eligible);
    }

    [Fact]
    public void IsEligibleTarget_IdentifiesForeignCountriesCorrectly()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        Assert.False(BilateralDiplomacyHandler.IsEligibleTarget(player, player));
        Assert.False(BilateralDiplomacyHandler.IsEligibleTarget(player, null));
        Assert.False(BilateralDiplomacyHandler.IsEligibleTarget(null!, norse));
        Assert.True(BilateralDiplomacyHandler.IsEligibleTarget(player, norse));
    }

    [Fact]
    public void RecognizeRelation_ThroughHandler_UpdatesStoredStanding()
    {
        var openTime = CreateTestTime(day: 0, hour: 6);
        var recTime = CreateTestTime(day: 1, hour: 18);
        var (player, norse, _, world) = SetupTestWorld(openTime);

        // Cannot recognize unopened relation
        var fail = BilateralDiplomacyHandler.RecognizeRelation(player, norse.Id, world, recTime);
        Assert.False(fail.IsSuccess);
        Assert.Equal(DiplomaticActionResult.RelationNotFound, fail.Result);

        // Open first
        var open = BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, openTime);
        Assert.True(open.IsSuccess);

        // Recognize
        var rec = BilateralDiplomacyHandler.RecognizeRelation(player, norse.Id, world, recTime);
        Assert.True(rec.IsSuccess);
        Assert.NotNull(rec.Relation);
        Assert.True(rec.Relation!.IsRecognized);
        Assert.Equal(recTime, rec.Relation.RecognizedBoundary);

        // Country reflects recognition
        Assert.True(player.Relation!.IsRecognized);
    }

    [Fact]
    public void ProposeAndRespondAgreement_ThroughHandler_FullLifecycle()
    {
        var t1 = CreateTestTime(day: 0, hour: 6);
        var t2 = CreateTestTime(day: 1, hour: 12);
        var (player, norse, _, world) = SetupTestWorld(t1);

        // Open relation
        BilateralDiplomacyHandler.OpenRelation(player, norse.Id, world, t1);

        // Propose trade agreement
        var prop = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Trade, world, t1);
        Assert.True(prop.IsSuccess);
        Assert.NotNull(prop.Agreement);
        Assert.True(prop.Agreement!.IsPending);

        // Norse accepts
        var accept = BilateralDiplomacyHandler.RespondToAgreement(norse, prop.Agreement.Id, accept: true, world, t2);
        Assert.True(accept.IsSuccess);
        Assert.NotNull(accept.Agreement);
        Assert.True(accept.Agreement!.IsActive);
        Assert.Equal(t2, accept.Agreement.DecidedBoundary);

        // Stored on both
        Assert.True(player.HasActiveAgreement(norse.Id, DiplomaticAgreementType.Trade));
        Assert.True(norse.HasActiveAgreement(player.Id, DiplomaticAgreementType.Trade));
    }
}
