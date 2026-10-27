namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Republic.Core.World.Services;
using Republic.Core.Client;

public class BilateralTradeServiceTests
{
    private (Country Proposer, Country Target, RepublicTime Time) CreateTestCountries()
    {
        var factory = new CountryProfileFactory();
        var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var time = new RepublicTime(epoch.AddDays(10), epoch);
        var proposer = factory.CreateCountry(1001, time, "valoria_sovereign", 10_000_000.0);
        var target = factory.CreateCountry(1002, time, "astoria_republic", 10_000_000.0);

        // Ensure baseline stockpiles for test isolation
        proposer.AddResource("Iron", 1000.0);
        proposer.AddResource("Timber", 1000.0);
        target.AddResource("Petroleum", 1000.0);
        target.AddResource("Bauxite", 1000.0);

        return (proposer, target, time);
    }

    [Fact]
    public void DiplomaticCountryProfile_ClassificationRules_RevealsExactForSelf_AndEstimatesForForeign()
    {
        var (proposer, target, time) = CreateTestCountries();

        // 1. Self profile inspection: All facts must be Confirmed
        var selfProfile = DiplomaticCountryProfile.Create(viewingCountry: proposer, targetCountry: proposer, currentTime: time);
        Assert.True(selfProfile.TreasuryIsConfirmed);
        Assert.True(selfProfile.ApprovalIsConfirmed);
        Assert.True(selfProfile.StabilityIsConfirmed);
        Assert.Contains("Confirmed", selfProfile.TreasuryDisplay);

        // 2. Foreign profile inspection without authorized alliance: Values must be Estimated with classified flags
        var foreignProfile = DiplomaticCountryProfile.Create(viewingCountry: proposer, targetCountry: target, currentTime: time);
        Assert.False(foreignProfile.TreasuryIsConfirmed);
        Assert.False(foreignProfile.ApprovalIsConfirmed);
        Assert.False(foreignProfile.StabilityIsConfirmed);
        Assert.Contains("Estimated", foreignProfile.TreasuryDisplay);
        Assert.Contains("Classified", foreignProfile.TreasuryDisplay);
    }

    [Fact]
    public void CreateProposal_SpotBarter_ValidatesAndStoresProposal()
    {
        var (proposer, target, time) = CreateTestCountries();
        var service = new BilateralTradeService();

        var terms = new FlexibleTradeTerms(
            proposerCountryId: proposer.Id,
            targetCountryId: target.Id,
            offeredResourceType: "Iron",
            offeredQuantity: 250.0,
            requestedResourceType: "Petroleum",
            requestedQuantity: 100.0,
            currencyPayment: 0.0,
            isRecurring: false,
            totalDeliveryCycles: 1,
            createdAt: time);

        var (success, message, proposal) = service.CreateProposal(proposer, target, terms, time);

        Assert.True(success, message);
        Assert.NotNull(proposal);
        Assert.Equal(FlexibleTradeStatus.Pending, proposal.Status);
        Assert.True(proposal.ProposerApproved);
        Assert.False(proposal.TargetApproved);
        Assert.Equal(1, proposal.RevisionNumber);
        Assert.Equal(proposer.Id, proposal.LastModifiedByCountryId);

        // Verify stored in both countries
        Assert.NotNull(proposer.GetFlexibleTradeProposal(proposal.Id));
        Assert.NotNull(target.GetFlexibleTradeProposal(proposal.Id));
    }

    [Fact]
    public void CounterProposal_InvalidatesPriorApprovals_AdvancesRevisionNumber_ResetsAcceptanceAuthority()
    {
        var (proposer, target, time) = CreateTestCountries();
        var service = new BilateralTradeService();

        var initialTerms = new FlexibleTradeTerms(
            proposerCountryId: proposer.Id,
            targetCountryId: target.Id,
            offeredResourceType: "Iron",
            offeredQuantity: 200.0,
            requestedResourceType: "Petroleum",
            requestedQuantity: 100.0,
            currencyPayment: 0.0,
            createdAt: time);

        var (_, _, initialProposal) = service.CreateProposal(proposer, target, initialTerms, time);
        Assert.NotNull(initialProposal);

        // Target counters: Asks for 300 Iron and offers 100 Petroleum + 15,000 REPU
        var counterTerms = initialProposal.CreateCounterOffer(
            counteringCountryId: target.Id,
            revisedOfferedResource: "Iron",
            revisedOfferedQty: 300.0,
            revisedRequestedResource: "Petroleum",
            revisedRequestedQty: 100.0,
            revisedCurrencyPayment: -15000.0,
            currentTime: time.Add(TimeSpan.FromDays(1)),
            revisedNotes: "Astoria requires 300 Iron given current strategic bauxite demand.");

        var (counterSuccess, counterMessage, counteredProposal) = service.CounterProposal(
            actor: target,
            proposalId: initialProposal.Id,
            counterTerms: counterTerms,
            currentTime: time.Add(TimeSpan.FromDays(1)));

        Assert.True(counterSuccess, counterMessage);
        Assert.NotNull(counteredProposal);
        Assert.Equal(FlexibleTradeStatus.Countered, counteredProposal.Status);
        Assert.Equal(2, counteredProposal.RevisionNumber);
        Assert.Equal(target.Id, counteredProposal.LastModifiedByCountryId);

        // CRITICAL: Countering invalidates proposer's prior approval!
        Assert.True(counteredProposal.TargetApproved);
        Assert.False(counteredProposal.ProposerApproved);

        // Target cannot accept its own counter-offer without foreign review
        Assert.False(counteredProposal.CanBeAcceptedBy(target.Id));
        // Proposer CAN accept the counter-offer
        Assert.True(counteredProposal.CanBeAcceptedBy(proposer.Id));
    }

    [Fact]
    public void PreExecutionValidation_AbortsTransactionWhenStockpileInsufficient()
    {
        var (proposer, target, time) = CreateTestCountries();
        var service = new BilateralTradeService();

        // Proposer requests 500,000 Petroleum which exceeds target's 1000 stockpile
        var terms = new FlexibleTradeTerms(
            proposerCountryId: proposer.Id,
            targetCountryId: target.Id,
            offeredResourceType: "Iron",
            offeredQuantity: 100.0,
            requestedResourceType: "Petroleum",
            requestedQuantity: 500_000.0,
            createdAt: time);

        var (_, _, proposal) = service.CreateProposal(proposer, target, terms, time);
        Assert.NotNull(proposal);

        // Target attempts to accept
        var (acceptSuccess, acceptMessage, _) = service.AcceptProposal(
            actor: target,
            proposalId: proposal.Id,
            currentTime: time.Add(TimeSpan.FromDays(1)),
            worldCountries: new[] { proposer, target });

        Assert.False(acceptSuccess);
        Assert.Contains("insufficient", acceptMessage, StringComparison.OrdinalIgnoreCase);

        double targetInitialPetroleum = target.GetResourceQuantity("Petroleum");
        double proposerInitialIron = proposer.GetResourceQuantity("Iron");

        // Stockpiles must be completely untouched
        Assert.Equal(proposerInitialIron, proposer.GetResourceQuantity("Iron"));
        Assert.Equal(targetInitialPetroleum, target.GetResourceQuantity("Petroleum"));
    }

    [Fact]
    public void AtomicExecution_BarterAndCashBalancing_TransfersResourcesAndImprovesRelations()
    {
        var (proposer, target, time) = CreateTestCountries();
        var service = new BilateralTradeService();

        double proposerInitialTreasury = proposer.Treasury.Balance;
        double targetInitialTreasury = target.Treasury.Balance;
        double proposerInitialIron = proposer.GetResourceQuantity("Iron");
        double targetInitialIron = target.GetResourceQuantity("Iron");
        double proposerInitialPetroleum = proposer.GetResourceQuantity("Petroleum");
        double targetInitialPetroleum = target.GetResourceQuantity("Petroleum");

        // Proposer gives 200 Iron + 50,000 REPU for 100 Petroleum from Target
        var terms = new FlexibleTradeTerms(
            proposerCountryId: proposer.Id,
            targetCountryId: target.Id,
            offeredResourceType: "Iron",
            offeredQuantity: 200.0,
            requestedResourceType: "Petroleum",
            requestedQuantity: 100.0,
            currencyPayment: 50_000.0,
            createdAt: time);

        var (_, _, proposal) = service.CreateProposal(proposer, target, terms, time);
        Assert.NotNull(proposal);

        var (success, message, executed) = service.AcceptProposal(
            actor: target,
            proposalId: proposal.Id,
            currentTime: time.Add(TimeSpan.FromDays(1)),
            worldCountries: new[] { proposer, target });

        Assert.True(success, message);
        Assert.NotNull(executed);
        Assert.Equal(FlexibleTradeStatus.Completed, executed.Status);

        // Verify Commodities transferred atomically
        Assert.Equal(proposerInitialIron - 200.0, proposer.GetResourceQuantity("Iron"));
        Assert.Equal(targetInitialIron + 200.0, target.GetResourceQuantity("Iron"));
        Assert.Equal(proposerInitialPetroleum + 100.0, proposer.GetResourceQuantity("Petroleum"));
        Assert.Equal(targetInitialPetroleum - 100.0, target.GetResourceQuantity("Petroleum"));

        // Verify Currencies transferred atomically
        Assert.Equal(proposerInitialTreasury - 50_000.0, proposer.Treasury.Balance);
        Assert.Equal(targetInitialTreasury + 50_000.0, target.Treasury.Balance);

        // Verify Diplomatic Relation established / recognized
        var relation = proposer.GetRelation(target.Id);
        Assert.NotNull(relation);
        Assert.Equal(DiplomaticRelationStatus.Recognized, relation.Status);
    }

    [Fact]
    public void RecurringDeliveryContract_ExecutesCycles_AndFlagsDisputeOnDefault()
    {
        var (proposer, target, time) = CreateTestCountries();
        var service = new BilateralTradeService();

        // 3-cycle recurring contract: Proposer delivers 100 Timber per cycle for 25,000 REPU
        var terms = new FlexibleTradeTerms(
            proposerCountryId: proposer.Id,
            targetCountryId: target.Id,
            offeredResourceType: "Timber",
            offeredQuantity: 100.0,
            requestedResourceType: null,
            requestedQuantity: 0.0,
            currencyPayment: -25_000.0, // Target pays Proposer
            isRecurring: true,
            totalDeliveryCycles: 3,
            createdAt: time);

        var (_, _, proposal) = service.CreateProposal(proposer, target, terms, time);
        Assert.NotNull(proposal);

        // Target ratifies
        var (acceptSuccess, _, activeContract) = service.AcceptProposal(
            actor: target,
            proposalId: proposal.Id,
            currentTime: time,
            worldCountries: new[] { proposer, target });

        Assert.True(acceptSuccess);
        Assert.NotNull(activeContract);
        Assert.Equal(FlexibleTradeStatus.ActiveContract, activeContract.Status);
        Assert.Equal(2, activeContract.RemainingDeliveryCycles);

        // Cycle 2: Target empties treasury so payment fails
        target.Treasury.Withdraw(target.Treasury.Balance);

        var cycleResults = service.ProcessRecurringContracts(
            country: target,
            currentTime: time.Add(TimeSpan.FromDays(1)),
            worldCountries: new[] { proposer, target });

        Assert.NotEmpty(cycleResults);
        var contractOutcome = cycleResults.First(r => r.ContractId == activeContract.Id);
        Assert.False(contractOutcome.Success);
        Assert.Contains("defaulted", contractOutcome.Message, StringComparison.OrdinalIgnoreCase);

        var disputed = proposer.GetFlexibleTradeProposal(activeContract.Id);
        Assert.NotNull(disputed);
        Assert.Equal(FlexibleTradeStatus.Disputed, disputed.Status);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyBilateralTradeProposal_AndRejectAction()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Trade_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var statePath = Path.Combine(tempDir, "presidential_desk_state.json");
        var savePath = Path.Combine(tempDir, "presidential_desk_state.sav");

        try
        {
            var factory = new CountryProfileFactory();
            var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
            var time = new RepublicTime(epoch.AddDays(10), epoch);
            var proposer = factory.CreateCountry(1001, time, "valoria_sovereign", 10_000_000.0);
            var target = factory.CreateCountry(1002, time, "astoria_republic", 10_000_000.0);
            proposer.AddResource("Iron", 1000.0);
            target.AddResource("Petroleum", 1000.0);
            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Test World",
                Countries = new List<Country> { proposer, target }
            };

            PresidentialDeskBridge.ExportState(world, proposer, time, 1, statePath, savePath);

            // 1. Propose via bridge method
            var result = PresidentialDeskBridge.ApplyBilateralTradeProposal(
                targetCountryId: target.Id,
                offeredResourceType: "Iron",
                offeredQuantity: 100.0,
                requestedResourceType: "Petroleum",
                requestedQuantity: 50.0,
                currencyPayment: 0.0,
                isRecurring: false,
                totalDeliveryCycles: 1,
                notes: "Strategic materials trade",
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(result.Success, result.Message);
            Assert.Contains("Bilateral trade proposal", result.Message);

            var snapshotJson = File.ReadAllText(statePath);
            var snapshot = PresidentialDeskBridge.Deserialize(snapshotJson);
            Assert.NotNull(snapshot);
            Assert.NotEmpty(snapshot.DiplomaticProposals);

            var storedProp = snapshot.DiplomaticProposals.FirstOrDefault(p => p.TargetCountryId == target.Id);
            Assert.NotNull(storedProp);

            // 2. Reject proposal via bridge method
            var rejectResult = PresidentialDeskBridge.ApplyRejectBilateralTrade(
                proposalId: storedProp.Id,
                reason: "Tariff adjustments pending",
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(rejectResult.Success, rejectResult.Message);

            var updatedJson = File.ReadAllText(statePath);
            var updatedSnapshot = PresidentialDeskBridge.Deserialize(updatedJson);
            Assert.NotNull(updatedSnapshot);
            var updatedProp = updatedSnapshot.DiplomaticProposals.FirstOrDefault(p => p.Id == storedProp.Id);
            Assert.NotNull(updatedProp);
            Assert.Equal("Rejected", updatedProp.Status);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
