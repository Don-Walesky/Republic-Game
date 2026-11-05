namespace Republic.Core.Tests.Communication;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Communication.Services;
using Republic.Core.Legislature.Models;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class InGameCommunicationTests
{
    private static Country CreateTestCountry(string name = "Arcadia")
    {
        var country = new Country
        {
            Id = $"id_{name.ToLowerInvariant()}",
            Name = name,
            Legislature = new ParliamentaryChamber()
        };
        country.Legislature.InitializeDefaultLawmakers();
        return country;
    }

    [Fact]
    public void DispatchLegislatorDefectionIntel_GeneratesConfidentialInsiderMemo_ForDistrictGrant()
    {
        var country = CreateTestCountry();
        var gameState = new FullGameState();
        var lawmaker = country.Legislature!.Lawmakers.First(l => l.Id == "lawmaker_thorne");

        var memo = InGameCommunicationService.DispatchLegislatorDefectionIntel(
            country, lawmaker, "district_grant", gameState);

        Assert.NotNull(memo);
        Assert.Equal("Hon. Marcus Thorne", memo.Sender);
        Assert.Equal("Legislative", memo.Channel);
        Assert.Equal("Confidential", memo.Urgency);
        Assert.Equal("view_chamber", memo.ActionType);
        Assert.Equal("lawmaker_thorne", memo.RelatedEntityId);
        Assert.Contains("capital development grant", memo.Body);
        Assert.Contains("INSIDE INTELLIGENCE", memo.Body);
        Assert.Single(gameState.Workspace.Emails);
        Assert.Equal(memo.Id, gameState.Workspace.Emails[0].Id);
    }

    [Fact]
    public void DispatchLegislatorDefectionIntel_GeneratesConfidentialInsiderMemo_ForCommitteeChair()
    {
        var country = CreateTestCountry();
        var gameState = new FullGameState();
        var lawmaker = country.Legislature!.Lawmakers.First(l => l.Id == "lawmaker_sterling");

        var memo = InGameCommunicationService.DispatchLegislatorDefectionIntel(
            country, lawmaker, "committee_chair", gameState);

        Assert.NotNull(memo);
        Assert.Equal("Hon. Clara Sterling", memo.Sender);
        Assert.Equal("Legislative", memo.Channel);
        Assert.Contains("gavel", memo.Body);
        Assert.Contains("Committee Chairmanship", memo.SenderTitle);
        Assert.Contains("Standing Committee", memo.Body);
        Assert.Single(gameState.Workspace.Emails);
    }

    [Fact]
    public void DispatchChamberStatusAlert_ReportsWorkingMajority_WhenSeatsReachFiftyOne()
    {
        var country = CreateTestCountry();
        var gameState = new FullGameState();
        var chamber = country.Legislature!;

        // Sway two lawmakers to cross threshold (48 + 2 + 2 = 52)
        chamber.RulingPartySeats = 52;
        chamber.OppositionSeats = 38;

        var alert = InGameCommunicationService.DispatchChamberStatusAlert(country, chamber, gameState);

        Assert.NotNull(alert);
        Assert.Equal("Legislative", alert.Channel);
        Assert.Contains("WORKING MAJORITY", alert.Subject);
        Assert.Contains("52/100 seats", alert.Body);
        Assert.Equal("Priority", alert.Urgency);
    }

    [Fact]
    public void DispatchLegislatorBillReview_GeneratesReadingReport_ForProposedBill()
    {
        var country = CreateTestCountry();
        var gameState = new FullGameState();
        var bill = new CountryBill("Clean Energy Grid Act", country.Id);

        var report = InGameCommunicationService.DispatchLegislatorBillReview(country, bill, "proposed", gameState);

        Assert.NotNull(report);
        Assert.Equal("Legislative", report.Channel);
        Assert.Contains("Clean Energy Grid Act", report.Subject);
        Assert.Contains("Order Paper", report.Body);
        Assert.True(report.ActionRequired);
        Assert.Equal("view_chamber", report.ActionType);
    }

    [Fact]
    public void DispatchBilateralProposalCable_GeneratesPriorityDiplomaticCable_AndWorldStateMessage()
    {
        var countryA = CreateTestCountry("Arcadia");
        var countryB = CreateTestCountry("Valoria");
        var gameState = new FullGameState { World = new WorldState() };

        var terms = new FlexibleTradeTerms(
            proposerCountryId: countryB.Id,
            targetCountryId: countryA.Id,
            offeredResourceType: "Iron",
            offeredQuantity: 5000,
            requestedResourceType: "Oil",
            requestedQuantity: 2000,
            currencyPayment: 250_000,
            isRecurring: false,
            totalDeliveryCycles: 1,
            createdAt: default,
            id: "prop_valoria_99");

        var cable = InGameCommunicationService.DispatchBilateralProposalCable(
            countryA, countryB, terms, gameState);

        Assert.NotNull(cable);
        Assert.Equal("Diplomatic", cable.Channel);
        Assert.Equal("Priority", cable.Urgency);
        Assert.True(cable.ActionRequired);
        Assert.Equal("review_bilateral_deal", cable.ActionType);
        Assert.Equal("prop_valoria_99", cable.RelatedEntityId);
        Assert.Contains("diplomatic greetings", cable.Body, StringComparison.OrdinalIgnoreCase);

        // Verify WorldState multiplayer message tracking
        Assert.Single(gameState.World.Messages);
        var dipMsg = gameState.World.Messages[0];
        Assert.Equal("Valoria", dipMsg.From);
        Assert.Equal("Arcadia", dipMsg.To);
        Assert.Equal("BilateralOffer", dipMsg.MessageType);
        Assert.Equal("prop_valoria_99", dipMsg.OfferedDealId);
    }

    [Fact]
    public void DispatchBilateralResolutionCable_GeneratesRatificationNotice_OnAccept()
    {
        var countryA = CreateTestCountry("Arcadia");
        var countryB = CreateTestCountry("Maridia");
        var gameState = new FullGameState();

        var terms = new FlexibleTradeTerms(
            proposerCountryId: countryB.Id,
            targetCountryId: countryA.Id,
            offeredResourceType: "Food",
            offeredQuantity: 10000,
            requestedResourceType: "REPU",
            requestedQuantity: 0,
            currencyPayment: 500_000,
            isRecurring: false,
            totalDeliveryCycles: 1,
            createdAt: default,
            id: "deal_77");

        var notice = InGameCommunicationService.DispatchBilateralResolutionCable(
            countryA, countryB, terms, "accepted", gameState);

        Assert.NotNull(notice);
        Assert.Equal("Diplomatic", notice.Channel);
        Assert.Contains("RATIFIED", notice.Subject);
        Assert.Contains("customs", notice.Body);
        Assert.False(notice.ActionRequired);
    }

    [Fact]
    public void EnsureDefaultCommunications_PopulatesLegislativeAndDiplomaticOpeners()
    {
        var country = CreateTestCountry("Arcadia");
        var gameState = new FullGameState();

        InGameCommunicationService.EnsureDefaultCommunications(country, gameState);

        Assert.True(gameState.Workspace.Emails.Count >= 3);
        Assert.Contains(gameState.Workspace.Emails, e => e.Channel == "Legislative");
        Assert.Contains(gameState.Workspace.Emails, e => e.Channel == "Diplomatic");
        Assert.Contains(gameState.Workspace.Emails, e => e.Channel == "Executive");
    }

    [Fact]
    public void ApplyMarkEmailRead_MarksEmailRead_AndUpdatesUnreadCountInSnapshot()
    {
        var country = CreateTestCountry("Arcadia");
        var tempSave = Path.Combine(Path.GetTempPath(), $"rep_comm_test_{Guid.NewGuid():N}.sav");
        var tempJson = Path.ChangeExtension(tempSave, ".json");

        try
        {
            var simTime = RepublicClock.CreateControlled().CurrentTime;
            PresidentialDeskBridge.SaveAuthoritativeState(country, simTime, 1, tempSave);

            // Read the save envelope and add an unread email
            var envelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSave);
            Assert.NotNull(envelope);
            envelope!.State!.Workspace ??= new();
            envelope.State.Workspace!.Emails ??= new();

            var testEmail = new Republic.Core.Workspace.Models.EmailMessage
            {
                Id = "test_mail_unread_01",
                Sender = "Hon. Marcus Thorne",
                Subject = "Urgent Whip Count",
                Body = "Floor numbers are volatile.",
                Channel = "Legislative",
                IsRead = false
            };
            envelope.State.Workspace.Emails.Add(testEmail);

            var serializer = new JsonStateSerializer();
            AtomicFileWriter.WriteAllTextSafely(tempSave, serializer.Serialize(envelope));

            // Export baseline snapshot
            var snapBefore = PresidentialDeskSnapshot.Create(country, simTime, 1, tempSave);
            Assert.True(snapBefore.UnreadEmailCount >= 1);
            Assert.True(snapBefore.UnreadLegislativeCount >= 1);

            // Mark email as read via bridge
            var result = PresidentialDeskBridge.ApplyMarkEmailRead(
                "test_mail_unread_01", tempJson, tempSave, "req_mark_01");

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);

            // Verify state
            var envelopeAfter = PresidentialDeskBridge.LoadAuthoritativeState(tempSave);
            Assert.NotNull(envelopeAfter);
            Assert.NotNull(envelopeAfter!.State);
            Assert.NotNull(envelopeAfter.State!.Workspace);
            Assert.NotNull(envelopeAfter.State.Workspace!.Emails);
            var emailAfter = envelopeAfter.State.Workspace.Emails!.First(e => e.Id == "test_mail_unread_01");
            Assert.True(emailAfter.IsRead);

            var snapAfter = PresidentialDeskSnapshot.Create(country, simTime, 1, tempSave);
            var snapEmail = snapAfter.Emails.First(e => e.Id == "test_mail_unread_01");
            Assert.True(snapEmail.IsRead);
        }
        finally
        {
            if (File.Exists(tempSave)) File.Delete(tempSave);
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }

    [Fact]
    public void ApplyMarkEmailRead_All_MarksAllEmailsRead()
    {
        var country = CreateTestCountry("Arcadia");
        var tempSave = Path.Combine(Path.GetTempPath(), $"rep_comm_test_all_{Guid.NewGuid():N}.sav");
        var tempJson = Path.ChangeExtension(tempSave, ".json");

        try
        {
            var simTime = RepublicClock.CreateControlled().CurrentTime;
            PresidentialDeskBridge.SaveAuthoritativeState(country, simTime, 1, tempSave);

            var envelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSave);
            Assert.NotNull(envelope);
            envelope!.State!.Workspace ??= new();
            envelope.State.Workspace!.Emails ??= new();

            envelope.State.Workspace.Emails.Add(new Republic.Core.Workspace.Models.EmailMessage
            {
                Id = "mail_a",
                Sender = "Sender A",
                Subject = "Subj A",
                Body = "Body A",
                IsRead = false
            });
            envelope.State.Workspace.Emails.Add(new Republic.Core.Workspace.Models.EmailMessage
            {
                Id = "mail_b",
                Sender = "Sender B",
                Subject = "Subj B",
                Body = "Body B",
                IsRead = false
            });

            var serializer = new JsonStateSerializer();
            AtomicFileWriter.WriteAllTextSafely(tempSave, serializer.Serialize(envelope));

            var result = PresidentialDeskBridge.ApplyMarkEmailRead("all", tempJson, tempSave, "req_mark_all");
            Assert.True(result.Success);

            var envelopeAfter = PresidentialDeskBridge.LoadAuthoritativeState(tempSave);
            Assert.NotNull(envelopeAfter);
            Assert.All(envelopeAfter!.State!.Workspace!.Emails, e => Assert.True(e.IsRead));
        }
        finally
        {
            if (File.Exists(tempSave)) File.Delete(tempSave);
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }
}
