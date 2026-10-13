namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class DiplomaticAgreementTests
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

    // --- Formal Recognition Tests ---

    [Fact]
    public void RecognizeRelation_ExistingRelation_BecomesRecognizedWithBoundary()
    {
        var openTime = CreateTestTime(day: 0, hour: 6);
        var recTime = CreateTestTime(day: 1, hour: 12);
        var (player, norse, _, _) = SetupTestWorld(openTime);

        // Open relation
        player.OpenRelation(norse.Id, openTime);
        Assert.True(player.Relation!.IsUnrecognized);

        // Formally recognize
        var recognized = player.RecognizeRelation(norse.Id, recTime);

        Assert.NotNull(recognized);
        Assert.Equal(DiplomaticRelationStatus.Recognized, recognized!.Status);
        Assert.True(recognized.IsRecognized);
        Assert.False(recognized.IsUnrecognized);
        Assert.Equal(openTime, recognized.OpenedBoundary);
        Assert.Equal(recTime, recognized.RecognizedBoundary);

        // Stored on country
        Assert.Same(recognized, player.Relation);
        Assert.Single(player.Relations);
    }

    [Fact]
    public void RecognizeRelation_WithoutPriorOpenRelation_IsRejected()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        // No relation was opened
        var result = player.RecognizeRelation(norse.Id, time);

        Assert.Null(result);
        Assert.Null(player.Relation);
        Assert.Empty(player.Relations);
    }

    [Fact]
    public void RecognizeRelation_RepeatedCall_IsIdempotentAndSafe()
    {
        var openTime = CreateTestTime(day: 0, hour: 6);
        var recTime1 = CreateTestTime(day: 1, hour: 12);
        var recTime2 = CreateTestTime(day: 2, hour: 18);
        var (player, norse, _, _) = SetupTestWorld(openTime);

        player.OpenRelation(norse.Id, openTime);
        var firstRec = player.RecognizeRelation(norse.Id, recTime1);
        Assert.NotNull(firstRec);

        // Repeated recognition at a later boundary
        var secondRec = player.RecognizeRelation(norse.Id, recTime2);

        Assert.NotNull(secondRec);
        Assert.Same(firstRec, secondRec);
        Assert.Single(player.Relations);
        Assert.Equal(recTime1, secondRec!.RecognizedBoundary); // Preserves original recognition boundary
    }

    [Fact]
    public void RecognizeRelation_SelfCountry_IsRejected()
    {
        var time = CreateTestTime();
        var (player, _, _, _) = SetupTestWorld(time);

        var result = player.RecognizeRelation(player.Id, time);
        Assert.Null(result);
    }

    [Fact]
    public void RecognizeRelation_Handler_ReturnsAppropriateOutcomes()
    {
        var time = CreateTestTime();
        var (player, norse, _, world) = SetupTestWorld(time);

        // Fails when not opened
        var failOutcome = BilateralDiplomacyHandler.RecognizeRelation(player, norse.Id, world, time);
        Assert.False(failOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.RelationNotFound, failOutcome.Result);

        // Open relation
        player.OpenRelation(norse.Id, time);

        // Succeeds
        var successOutcome = BilateralDiplomacyHandler.RecognizeRelation(player, norse.Id, world, time);
        Assert.True(successOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.Success, successOutcome.Result);
        Assert.NotNull(successOutcome.Relation);
        Assert.True(successOutcome.Relation!.IsRecognized);

        // Repeated handler call is idempotent
        var repeatedOutcome = BilateralDiplomacyHandler.RecognizeRelation(player, norse.Id, world, time);
        Assert.True(repeatedOutcome.IsSuccess);
        Assert.Contains("already formally recognized", repeatedOutcome.Message);
    }

    // --- Bilateral Trade Agreement Tests ---

    [Fact]
    public void ProposeTradeAgreement_WithExistingRelation_CreatesPendingAgreement()
    {
        var time = CreateTestTime(day: 0, hour: 6);
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);
        var agreement = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);

        Assert.NotNull(agreement);
        Assert.Equal("player-country", agreement!.SourceCountryId);
        Assert.Equal("norse-country", agreement.TargetCountryId);
        Assert.Equal(DiplomaticAgreementType.Trade, agreement.AgreementType);
        Assert.Equal(DiplomaticAgreementStatus.Pending, agreement.Status);
        Assert.True(agreement.IsPending);
        Assert.False(agreement.IsActive);
        Assert.Equal(time, agreement.ProposedBoundary);
        Assert.Null(agreement.DecidedBoundary);

        // Attached to both involved countries
        Assert.Contains(agreement, player.Agreements);
        Assert.Contains(agreement, norse.Agreements);
    }

    [Fact]
    public void ProposeTradeAgreement_WithoutOpenRelation_IsRejected()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        // No relation opened
        var agreement = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);

        Assert.Null(agreement);
        Assert.Empty(player.Agreements);
        Assert.Empty(norse.Agreements);
    }

    [Fact]
    public void AcceptTradeAgreement_TransitionsStatusToAcceptedWithDecidedBoundary()
    {
        var proposeTime = CreateTestTime(day: 0, hour: 6);
        var acceptTime = CreateTestTime(day: 1, hour: 12);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);
        var proposal = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, proposeTime);
        Assert.NotNull(proposal);

        // Norse accepts the proposal
        var accepted = norse.AcceptAgreement(proposal!.Id, acceptTime, otherCountry: player);

        Assert.NotNull(accepted);
        Assert.Equal(DiplomaticAgreementStatus.Accepted, accepted!.Status);
        Assert.True(accepted.IsAccepted);
        Assert.True(accepted.IsActive);
        Assert.False(accepted.IsPending);
        Assert.Equal(proposeTime, accepted.ProposedBoundary);
        Assert.Equal(acceptTime, accepted.DecidedBoundary);

        // Updated on both countries
        Assert.True(player.HasActiveAgreement(norse.Id, DiplomaticAgreementType.Trade));
        Assert.True(norse.HasActiveAgreement(player.Id, DiplomaticAgreementType.Trade));
    }

    [Fact]
    public void RejectTradeAgreement_TransitionsStatusToRejectedWithDecidedBoundary()
    {
        var proposeTime = CreateTestTime(day: 0, hour: 6);
        var rejectTime = CreateTestTime(day: 0, hour: 18);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);
        var proposal = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, proposeTime);
        Assert.NotNull(proposal);

        // Norse rejects the proposal
        var rejected = norse.RejectAgreement(proposal!.Id, rejectTime, otherCountry: player);

        Assert.NotNull(rejected);
        Assert.Equal(DiplomaticAgreementStatus.Rejected, rejected!.Status);
        Assert.True(rejected.IsRejected);
        Assert.False(rejected.IsActive);
        Assert.Equal(rejectTime, rejected.DecidedBoundary);

        // Both countries reflect rejection
        Assert.False(player.HasActiveAgreement(norse.Id, DiplomaticAgreementType.Trade));
        Assert.False(norse.HasActiveAgreement(player.Id, DiplomaticAgreementType.Trade));
    }

    [Fact]
    public void DuplicateActiveTradeAgreement_IsPrevented()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);
        var proposal = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);
        Assert.NotNull(proposal);

        // Attempting to propose a second trade agreement while one is pending is rejected
        var duplicatePending = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);
        Assert.Null(duplicatePending);

        // Accept the first
        norse.AcceptAgreement(proposal!.Id, time, otherCountry: player);

        // Attempting to propose another trade agreement while active is rejected
        var duplicateActive = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);
        Assert.Null(duplicateActive);
    }

    // --- Bilateral Alliance Tests ---

    [Fact]
    public void ProposeAlliance_RequiresRecognizedDiplomaticRelation()
    {
        var time = CreateTestTime();
        var (player, norse, _, world) = SetupTestWorld(time);

        // 1. No relation: rejected
        var noRelOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, time);
        Assert.False(noRelOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.RelationNotFound, noRelOutcome.Result);

        // 2. Open relation but Unrecognized: rejected
        player.OpenRelation(norse.Id, time);
        var unrecOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, time);
        Assert.False(unrecOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.PrerequisitesNotMet, unrecOutcome.Result);
        Assert.Contains("must be formally recognized", unrecOutcome.Message);

        // 3. Formally recognize: now alliance can be proposed
        player.RecognizeRelation(norse.Id, time);
        var successOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, time);
        Assert.True(successOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.Success, successOutcome.Result);
        Assert.NotNull(successOutcome.Agreement);
        Assert.Equal(DiplomaticAgreementType.Alliance, successOutcome.Agreement!.AgreementType);
        Assert.Equal(DiplomaticAgreementStatus.Pending, successOutcome.Agreement.Status);
    }

    [Fact]
    public void Alliance_AcceptanceAndRejection_RecordedAccurately()
    {
        var proposeTime = CreateTestTime(day: 2, hour: 0);
        var decisionTime = CreateTestTime(day: 2, hour: 12);
        var (player, norse, _, world) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);
        player.RecognizeRelation(norse.Id, proposeTime);

        // Propose alliance
        var outcome = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, proposeTime);
        Assert.True(outcome.IsSuccess);
        var allianceId = outcome.Agreement!.Id;

        // Norse accepts
        var acceptOutcome = BilateralDiplomacyHandler.RespondToAgreement(norse, allianceId, accept: true, world, decisionTime);
        Assert.True(acceptOutcome.IsSuccess);
        Assert.NotNull(acceptOutcome.Agreement);
        Assert.Equal(DiplomaticAgreementStatus.Accepted, acceptOutcome.Agreement!.Status);
        Assert.Equal(decisionTime, acceptOutcome.Agreement.DecidedBoundary);

        Assert.True(player.HasActiveAgreement(norse.Id, DiplomaticAgreementType.Alliance));
        Assert.True(norse.HasActiveAgreement(player.Id, DiplomaticAgreementType.Alliance));
    }

    [Fact]
    public void DuplicateAlliance_IsPrevented()
    {
        var time = CreateTestTime();
        var (player, norse, _, world) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);
        player.RecognizeRelation(norse.Id, time);

        var first = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, time);
        Assert.True(first.IsSuccess);

        // Duplicate proposal while pending
        var second = BilateralDiplomacyHandler.ProposeAgreement(player, norse.Id, DiplomaticAgreementType.Alliance, world, time);
        Assert.False(second.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AlreadyExists, second.Result);
    }

    // --- Sovereign Country Isolation Tests ---

    [Fact]
    public void SovereignIsolation_ThirdPartyRemainsUntouched()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, world) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);
        player.RecognizeRelation(norse.Id, time);
        player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);
        player.ProposeAgreement(norse, DiplomaticAgreementType.Alliance, time);

        // Aethel (Country C) is completely untouched
        Assert.Empty(aethel.Relations);
        Assert.Empty(aethel.Agreements);
        Assert.False(aethel.HasActiveAgreement(player.Id, DiplomaticAgreementType.Trade));
        Assert.False(aethel.HasActiveAgreement(norse.Id, DiplomaticAgreementType.Trade));
        Assert.False(aethel.HasActiveAgreement(player.Id, DiplomaticAgreementType.Alliance));
        Assert.Null(aethel.Agreement);
    }

    // --- Invalid and Nonexistent Targets ---

    [Fact]
    public void BilateralDiplomacyHandler_InvalidAndNonexistentTargets_AreRejected()
    {
        var time = CreateTestTime();
        var (player, _, _, world) = SetupTestWorld(time);

        // Self-target
        var selfOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, player.Id, DiplomaticAgreementType.Trade, world, time);
        Assert.False(selfOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.SelfRelationProhibited, selfOutcome.Result);

        // Nonexistent target
        var missingOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, "nonexistent-id", DiplomaticAgreementType.Trade, world, time);
        Assert.False(missingOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.TargetNotFound, missingOutcome.Result);

        // Null target
        var nullOutcome = BilateralDiplomacyHandler.ProposeAgreement(player, null, DiplomaticAgreementType.Trade, world, time);
        Assert.False(nullOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.InvalidInput, nullOutcome.Result);
    }

    // --- Clock Simulation Anchoring ---

    [Fact]
    public void Agreements_AnchorStrictlyToSimulationTime()
    {
        var clock = RepublicClock.CreateControlled();
        var t1 = clock.CurrentTime;
        var (player, norse, _, _) = SetupTestWorld(t1);

        player.OpenRelation(norse.Id, clock);
        player.RecognizeRelation(norse.Id, clock);

        var agreement = player.ProposeTradeAgreement(norse, clock);
        Assert.NotNull(agreement);
        Assert.Equal(t1, agreement!.ProposedBoundary);

        // Advance simulation clock by 18 hours (3 boundaries)
        clock.Advance(TimeSpan.FromHours(18));
        var t2 = clock.CurrentTime;

        var decided = norse.AcceptAgreement(agreement.Id, clock, otherCountry: player);
        Assert.NotNull(decided);
        Assert.Equal(t1, decided!.ProposedBoundary);
        Assert.Equal(t2, decided.DecidedBoundary);
        Assert.NotEqual(decided.ProposedBoundary, decided.DecidedBoundary);
    }

    // --- National Dashboard Integration ---

    [Fact]
    public void NationalDashboard_DisplaysRecognitionStatusWhenRelationsExist()
    {
        var time = CreateTestTime();
        var (player, norse, aethel, _) = SetupTestWorld(time);

        // No relations: Not established
        var dash0 = player.GetDashboard(time);
        Assert.Equal("Not established", dash0.Diplomacy);

        // 1 open relation (unrecognized)
        player.OpenRelation(norse.Id, time);
        var dash1 = player.GetDashboard(time);
        Assert.Contains("Unrecognized", dash1.Diplomacy);

        // Recognize relation
        player.RecognizeRelation(norse.Id, time);
        var dash2 = player.GetDashboard(time);
        Assert.Contains("Recognized", dash2.Diplomacy);
    }

    [Fact]
    public void RespondToAgreement_NonexistentOrAlreadyDecided_ReturnsError()
    {
        var time = CreateTestTime();
        var (player, norse, _, world) = SetupTestWorld(time);

        // Nonexistent
        var outcome = BilateralDiplomacyHandler.RespondToAgreement(player, "nonexistent-id", accept: true, world, time);
        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AgreementNotFound, outcome.Result);

        // Propose and accept
        player.OpenRelation(norse.Id, time);
        var agreement = player.ProposeAgreement(norse, DiplomaticAgreementType.Trade, time);
        Assert.NotNull(agreement);

        var firstAccept = BilateralDiplomacyHandler.RespondToAgreement(norse, agreement!.Id, accept: true, world, time);
        Assert.True(firstAccept.IsSuccess);

        // Attempting to decide again
        var secondAttempt = BilateralDiplomacyHandler.RespondToAgreement(norse, agreement.Id, accept: false, world, time);
        Assert.False(secondAttempt.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AlreadyDecided, secondAttempt.Result);
    }

    [Fact]
    public void DiplomaticAgreement_InvolvesAndDeconstruct_WorkCorrectly()
    {
        var time = CreateTestTime();
        var agreement = new DiplomaticAgreement("c1", "c2", DiplomaticAgreementType.Trade, DiplomaticAgreementStatus.Pending, time);

        Assert.True(agreement.Involves("c1"));
        Assert.True(agreement.Involves("c2"));
        Assert.False(agreement.Involves("c3"));
        Assert.True(agreement.Involves("c1", "c2"));
        Assert.True(agreement.Involves("c2", "c1"));
        Assert.False(agreement.Involves("c1", "c3"));

        var (source, target, type, status, proposed, decided) = agreement;
        Assert.Equal("c1", source);
        Assert.Equal("c2", target);
        Assert.Equal(DiplomaticAgreementType.Trade, type);
        Assert.Equal(DiplomaticAgreementStatus.Pending, status);
        Assert.Equal(time, proposed);
        Assert.Null(decided);

        var str = agreement.ToString();
        Assert.Contains("Trade Agreement", str);
        Assert.Contains("c1 <-> c2", str);
    }

    [Fact]
    public void CountryFound_SupportsAgreementsInitialization()
    {
        var time = CreateTestTime();
        var agreement = new DiplomaticAgreement("player", "target", DiplomaticAgreementType.Trade, DiplomaticAgreementStatus.Accepted, time);
        var country = Country.Found("Republic of Test", time, id: "player", agreement: agreement);

        Assert.NotNull(country.Agreement);
        Assert.Same(agreement, country.Agreement);
        Assert.Single(country.Agreements);
        Assert.Single(country.ActiveAgreements);
        Assert.NotNull(country.ActiveTradeAgreement);
    }
}
