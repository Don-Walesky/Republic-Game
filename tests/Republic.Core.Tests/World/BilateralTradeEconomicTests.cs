namespace Republic.Core.Tests.World;

using System;
using System.Linq;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class BilateralTradeEconomicTests
{
    private static RepublicTime CreateTestTime(int day = 0, int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(day, new TimeOnly(hour, 0));
    }

    private static (Country Player, Country Norse, Country Solaria, Country ThirdParty) SetupTestWorld(RepublicTime time)
    {
        var player = Country.Found(
            "Republic of Arcadia",
            time,
            id: "country-arcadia",
            treasury: new RepuTreasury(1_000_000.0, "country-arcadia"));
        player.Inventory.Add("Iron", 2000.0);
        player.Inventory.Add("Petroleum", 1000.0);

        var norse = Country.Found(
            "Federation of Norse",
            time,
            id: "country-norse",
            treasury: new RepuTreasury(800_000.0, "country-norse"));
        norse.Inventory.Add("Timber", 1500.0);
        norse.Inventory.Add("Copper", 800.0);

        var solaria = Country.Found(
            "Republic of Solaria",
            time,
            id: "country-solaria",
            treasury: new RepuTreasury(500_000.0, "country-solaria"));
        solaria.Inventory.Add("Bauxite", 1200.0);

        var thirdParty = Country.Found(
            "Commonwealth of Lys",
            time,
            id: "country-lys",
            treasury: new RepuTreasury(2_000_000.0, "country-lys"));
        thirdParty.Inventory.Add("Gold", 500.0);

        return (player, norse, solaria, thirdParty);
    }

    [Fact]
    public void ProposeTradeDeal_ValidTerms_CreatesPendingAgreementWithTermsAndSimulationTime()
    {
        var proposeTime = CreateTestTime(day: 2, hour: 10);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);

        // Player (buyer) proposes to buy 300 Timber from Norse (seller) for R150,000
        var terms = new BilateralTradeTerms(
            sellerCountryId: norse.Id,
            buyerCountryId: player.Id,
            resourceType: "Timber",
            quantity: 300.0,
            price: 150_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, proposeTime);

        Assert.NotNull(deal);
        Assert.Equal(DiplomaticAgreementType.Trade, deal!.AgreementType);
        Assert.Equal(DiplomaticAgreementStatus.Pending, deal.Status);
        Assert.True(deal.IsPending);
        Assert.False(deal.IsCompleted);
        Assert.Equal(proposeTime, deal.ProposedBoundary);
        Assert.Null(deal.DecidedBoundary);
        Assert.NotNull(deal.Terms);
        Assert.Equal("Timber", deal.Terms!.ResourceType);
        Assert.Equal(300.0, deal.Terms.Quantity);
        Assert.Equal(150_000.0, deal.Terms.Price);
        Assert.Equal(norse.Id, deal.Terms.SellerCountryId);
        Assert.Equal(player.Id, deal.Terms.BuyerCountryId);

        // Present on both countries
        Assert.Contains(player.Agreements, a => a.Id == deal.Id);
        Assert.Contains(norse.Agreements, a => a.Id == deal.Id);
    }

    [Fact]
    public void ProposeTradeDeal_InvalidTerms_NegativeOrZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", quantity: 0.0, price: 100_000.0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", quantity: -50.0, price: 100_000.0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", quantity: 100.0, price: 0.0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", quantity: 100.0, price: -500.0));
    }

    [Fact]
    public void ProposeTradeDeal_InvalidTerms_EmptyResourceOrSelfTrade_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "   ", quantity: 100.0, price: 100_000.0));

        Assert.Throws<ArgumentException>(() =>
            new BilateralTradeTerms("country-a", "country-a", "Iron", quantity: 100.0, price: 100_000.0));
    }

    [Fact]
    public void ProposeTradeDeal_TargetNotFound_ReturnsTargetNotFoundOutcome()
    {
        var time = CreateTestTime();
        var (player, _, _, _) = SetupTestWorld(time);
        var terms = new BilateralTradeTerms("country-missing", player.Id, "Iron", 100.0, 50_000.0);

        var outcome = BilateralDiplomacyHandler.ProposeTradeDeal(
            player,
            "country-missing",
            terms,
            new[] { player },
            time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.TargetNotFound, outcome.Result);
    }

    [Fact]
    public void ProposeTradeDeal_RelationNotFound_ReturnsRelationNotFoundOutcome()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);
        var terms = new BilateralTradeTerms(norse.Id, player.Id, "Timber", 100.0, 50_000.0);

        var outcome = BilateralDiplomacyHandler.ProposeTradeDeal(
            player,
            norse.Id,
            terms,
            new[] { player, norse },
            time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.RelationNotFound, outcome.Result);
    }

    [Fact]
    public void ProposeTradeDeal_DuplicatePendingDeal_Rejected()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);
        var terms1 = new BilateralTradeTerms(norse.Id, player.Id, "Timber", 200.0, 100_000.0);
        var first = player.ProposeTradeDeal(norse, terms1, time);
        Assert.NotNull(first);

        // Proposing another deal while first is still pending must be rejected
        var terms2 = new BilateralTradeTerms(player.Id, norse.Id, "Iron", 100.0, 80_000.0);
        var duplicate = player.ProposeTradeDeal(norse, terms2, time);
        Assert.Null(duplicate);

        var outcome = BilateralDiplomacyHandler.ProposeTradeDeal(
            player,
            norse.Id,
            terms2,
            new[] { player, norse },
            time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AlreadyExists, outcome.Result);
    }

    [Fact]
    public void AcceptTradeDeal_ValidFundsAndGoods_ExecutesAtomicTransferAndTransitionsToCompleted()
    {
        var proposeTime = CreateTestTime(day: 1, hour: 8);
        var acceptTime = CreateTestTime(day: 1, hour: 14);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);

        // Initial positions:
        // Player: Treasury = R1,000,000, Timber = 0
        // Norse:  Treasury = R800,000,   Timber = 1,500
        Assert.Equal(1_000_000.0, player.Treasury.Balance);
        Assert.Equal(0.0, player.Inventory.GetQuantity("Timber"));
        Assert.Equal(800_000.0, norse.Treasury.Balance);
        Assert.Equal(1500.0, norse.Inventory.GetQuantity("Timber"));

        var terms = new BilateralTradeTerms(
            sellerCountryId: norse.Id,
            buyerCountryId: player.Id,
            resourceType: "Timber",
            quantity: 500.0,
            price: 250_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, proposeTime);
        Assert.NotNull(deal);

        // Norse accepts the trade deal
        var completed = norse.AcceptAgreement(deal!.Id, acceptTime, otherCountry: player);

        Assert.NotNull(completed);
        Assert.Equal(DiplomaticAgreementStatus.Completed, completed!.Status);
        Assert.True(completed.IsCompleted);
        Assert.True(completed.IsAccepted);
        Assert.False(completed.IsPending);
        Assert.Equal(proposeTime, completed.ProposedBoundary);
        Assert.Equal(acceptTime, completed.DecidedBoundary);

        // Verify exact economic movement:
        // Player (buyer): R1,000,000 - R250,000 = R750,000; Timber: 0 + 500 = 500
        Assert.Equal(750_000.0, player.Treasury.Balance);
        Assert.Equal(500.0, player.Inventory.GetQuantity("Timber"));

        // Norse (seller): R800,000 + R250,000 = R1,050,000; Timber: 1,500 - 500 = 1,000
        Assert.Equal(1_050_000.0, norse.Treasury.Balance);
        Assert.Equal(1000.0, norse.Inventory.GetQuantity("Timber"));

        // Both countries have the updated completed record
        Assert.Equal(DiplomaticAgreementStatus.Completed, player.GetAgreement(deal.Id)!.Status);
        Assert.Equal(DiplomaticAgreementStatus.Completed, norse.GetAgreement(deal.Id)!.Status);
    }

    [Fact]
    public void RejectTradeDeal_TransitionsStatusToRejected_LeavesBalancesAndInventoriesUnchanged()
    {
        var proposeTime = CreateTestTime(day: 3, hour: 6);
        var rejectTime = CreateTestTime(day: 3, hour: 12);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);

        var terms = new BilateralTradeTerms(
            sellerCountryId: norse.Id,
            buyerCountryId: player.Id,
            resourceType: "Timber",
            quantity: 400.0,
            price: 200_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, proposeTime);
        Assert.NotNull(deal);

        var initialPlayerBalance = player.Treasury.Balance;
        var initialPlayerTimber = player.Inventory.GetQuantity("Timber");
        var initialNorseBalance = norse.Treasury.Balance;
        var initialNorseTimber = norse.Inventory.GetQuantity("Timber");

        // Norse rejects
        var rejected = norse.RejectAgreement(deal!.Id, rejectTime, otherCountry: player);

        Assert.NotNull(rejected);
        Assert.Equal(DiplomaticAgreementStatus.Rejected, rejected!.Status);
        Assert.True(rejected.IsRejected);
        Assert.False(rejected.IsAccepted);
        Assert.Equal(rejectTime, rejected.DecidedBoundary);

        // Economic positions unchanged
        Assert.Equal(initialPlayerBalance, player.Treasury.Balance);
        Assert.Equal(initialPlayerTimber, player.Inventory.GetQuantity("Timber"));
        Assert.Equal(initialNorseBalance, norse.Treasury.Balance);
        Assert.Equal(initialNorseTimber, norse.Inventory.GetQuantity("Timber"));
    }

    [Fact]
    public void AcceptTradeDeal_InsufficientBuyerFunds_RejectsTransactionWithoutStateChanges()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);

        // Norse wants to buy 1,000 Iron for R2,000,000 from Player.
        // But Norse only has R800,000!
        var terms = new BilateralTradeTerms(
            sellerCountryId: player.Id,
            buyerCountryId: norse.Id,
            resourceType: "Iron",
            quantity: 1000.0,
            price: 2_000_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, time);
        Assert.NotNull(deal);

        var initialPlayerBalance = player.Treasury.Balance;
        var initialPlayerIron = player.Inventory.GetQuantity("Iron");
        var initialNorseBalance = norse.Treasury.Balance;
        var initialNorseIron = norse.Inventory.GetQuantity("Iron");

        // Attempting to accept via domain returns null
        var domainResult = norse.AcceptAgreement(deal!.Id, time, otherCountry: player);
        Assert.Null(domainResult);

        // Attempting to accept via handler returns InsufficientFunds
        var outcome = BilateralDiplomacyHandler.RespondToAgreement(
            norse,
            deal.Id,
            accept: true,
            new[] { player, norse },
            time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.InsufficientFunds, outcome.Result);

        // Zero changes to balances or inventory
        Assert.Equal(initialPlayerBalance, player.Treasury.Balance);
        Assert.Equal(initialPlayerIron, player.Inventory.GetQuantity("Iron"));
        Assert.Equal(initialNorseBalance, norse.Treasury.Balance);
        Assert.Equal(initialNorseIron, norse.Inventory.GetQuantity("Iron"));
    }

    [Fact]
    public void AcceptTradeDeal_InsufficientSellerGoods_RejectsTransactionWithoutStateChanges()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);

        // Player wants to buy 5,000 Copper from Norse.
        // But Norse only has 800 Copper!
        var terms = new BilateralTradeTerms(
            sellerCountryId: norse.Id,
            buyerCountryId: player.Id,
            resourceType: "Copper",
            quantity: 5000.0,
            price: 100_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, time);
        Assert.NotNull(deal);

        var initialPlayerBalance = player.Treasury.Balance;
        var initialPlayerCopper = player.Inventory.GetQuantity("Copper");
        var initialNorseBalance = norse.Treasury.Balance;
        var initialNorseCopper = norse.Inventory.GetQuantity("Copper");

        // Domain method returns null
        var domainResult = norse.AcceptAgreement(deal!.Id, time, otherCountry: player);
        Assert.Null(domainResult);

        // Handler returns InsufficientGoods
        var outcome = BilateralDiplomacyHandler.RespondToAgreement(
            norse,
            deal.Id,
            accept: true,
            new[] { player, norse },
            time);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.InsufficientGoods, outcome.Result);

        // Balances and inventories unchanged
        Assert.Equal(initialPlayerBalance, player.Treasury.Balance);
        Assert.Equal(initialPlayerCopper, player.Inventory.GetQuantity("Copper"));
        Assert.Equal(initialNorseBalance, norse.Treasury.Balance);
        Assert.Equal(initialNorseCopper, norse.Inventory.GetQuantity("Copper"));
    }

    [Fact]
    public void DuplicateExecutionProtection_AcceptingAlreadyCompletedDeal_DoesNotTransferAgain()
    {
        var proposeTime = CreateTestTime(day: 1, hour: 0);
        var acceptTime = CreateTestTime(day: 1, hour: 4);
        var (player, norse, _, _) = SetupTestWorld(proposeTime);

        player.OpenRelation(norse.Id, proposeTime);

        var terms = new BilateralTradeTerms(
            sellerCountryId: player.Id,
            buyerCountryId: norse.Id,
            resourceType: "Iron",
            quantity: 200.0,
            price: 100_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, proposeTime);
        Assert.NotNull(deal);

        // First acceptance executes the trade
        var firstAccept = norse.AcceptAgreement(deal!.Id, acceptTime, otherCountry: player);
        Assert.NotNull(firstAccept);
        Assert.Equal(DiplomaticAgreementStatus.Completed, firstAccept!.Status);

        var balanceAfterFirst = player.Treasury.Balance;
        var ironAfterFirst = player.Inventory.GetQuantity("Iron");
        var norseBalanceAfterFirst = norse.Treasury.Balance;
        var norseIronAfterFirst = norse.Inventory.GetQuantity("Iron");

        // Second call on already completed deal
        var secondAccept = norse.AcceptAgreement(deal.Id, acceptTime.Add(TimeSpan.FromHours(2)), otherCountry: player);
        Assert.NotNull(secondAccept);
        Assert.Equal(DiplomaticAgreementStatus.Completed, secondAccept!.Status);

        // Balances and inventories must be identical to after first execution (no double-charge!)
        Assert.Equal(balanceAfterFirst, player.Treasury.Balance);
        Assert.Equal(ironAfterFirst, player.Inventory.GetQuantity("Iron"));
        Assert.Equal(norseBalanceAfterFirst, norse.Treasury.Balance);
        Assert.Equal(norseIronAfterFirst, norse.Inventory.GetQuantity("Iron"));

        // Handler call also rejects re-processing
        var handlerOutcome = BilateralDiplomacyHandler.RespondToAgreement(
            norse,
            deal.Id,
            accept: true,
            new[] { player, norse },
            acceptTime.Add(TimeSpan.FromHours(3)));

        Assert.False(handlerOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.AlreadyDecided, handlerOutcome.Result);
        Assert.Equal(balanceAfterFirst, player.Treasury.Balance);
    }

    [Fact]
    public void CountryIsolation_UninvolvedThirdCountry_RemainsCompletelyUnchanged()
    {
        var time = CreateTestTime(day: 2);
        var (player, norse, _, lys) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);

        var initialLysBalance = lys.Treasury.Balance;
        var initialLysGold = lys.Inventory.GetQuantity("Gold");
        var initialLysAgreements = lys.Agreements.Count;
        var initialLysRelations = lys.Relations.Count;

        var terms = new BilateralTradeTerms(
            sellerCountryId: player.Id,
            buyerCountryId: norse.Id,
            resourceType: "Iron",
            quantity: 500.0,
            price: 300_000.0);

        var deal = player.ProposeTradeDeal(norse, terms, time);
        norse.AcceptAgreement(deal!.Id, time.Add(TimeSpan.FromHours(1)), otherCountry: player);

        // Lys was entirely uninvolved and must be 100% untouched
        Assert.Equal(initialLysBalance, lys.Treasury.Balance);
        Assert.Equal(initialLysGold, lys.Inventory.GetQuantity("Gold"));
        Assert.Equal(initialLysAgreements, lys.Agreements.Count);
        Assert.Equal(initialLysRelations, lys.Relations.Count);
    }

    [Fact]
    public void BilateralDiplomacyHandler_ProposeAndAcceptTradeDeal_SucceedsWithClearOutcome()
    {
        var time = CreateTestTime();
        var (player, norse, _, _) = SetupTestWorld(time);

        player.OpenRelation(norse.Id, time);

        var terms = new BilateralTradeTerms(
            sellerCountryId: norse.Id,
            buyerCountryId: player.Id,
            resourceType: "Timber",
            quantity: 150.0,
            price: 75_000.0);

        var propOutcome = BilateralDiplomacyHandler.ProposeTradeDeal(
            player,
            norse.Id,
            terms,
            new[] { player, norse },
            time);

        Assert.True(propOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.Success, propOutcome.Result);
        Assert.NotNull(propOutcome.Agreement);

        var acceptOutcome = BilateralDiplomacyHandler.RespondToAgreement(
            norse,
            propOutcome.Agreement!.Id,
            accept: true,
            new[] { player, norse },
            time.Add(TimeSpan.FromHours(2)));

        Assert.True(acceptOutcome.IsSuccess);
        Assert.Equal(DiplomaticActionResult.Success, acceptOutcome.Result);
        Assert.Contains("successfully executed", acceptOutcome.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(DiplomaticAgreementStatus.Completed, acceptOutcome.Agreement!.Status);

        Assert.Equal(925_000.0, player.Treasury.Balance);
        Assert.Equal(150.0, player.Inventory.GetQuantity("Timber"));
        Assert.Equal(875_000.0, norse.Treasury.Balance);
        Assert.Equal(1350.0, norse.Inventory.GetQuantity("Timber"));
    }

    [Fact]
    public void ResourceInventory_DirectOperations_EnforceRulesAndStockpiles()
    {
        var inv = new ResourceInventory();
        Assert.Equal(0.0, inv.GetQuantity("Iron"));
        Assert.False(inv.HasSufficient("Iron", 10.0));

        inv.Add("Iron", 500.0);
        Assert.Equal(500.0, inv.GetQuantity("Iron"));
        Assert.True(inv.HasSufficient("Iron", 500.0));
        Assert.False(inv.HasSufficient("Iron", 501.0));

        inv.Remove("Iron", 200.0);
        Assert.Equal(300.0, inv.GetQuantity("Iron"));

        // Over-withdrawal throws InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => inv.Remove("Iron", 500.0));

        // TryRemove returns false without mutating
        Assert.False(inv.TryRemove("Iron", 500.0));
        Assert.Equal(300.0, inv.GetQuantity("Iron"));

        Assert.True(inv.TryRemove("Iron", 100.0));
        Assert.Equal(200.0, inv.GetQuantity("Iron"));

        // Negative values throw
        Assert.Throws<ArgumentOutOfRangeException>(() => inv.Add("Iron", -10.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => inv.Remove("Iron", -10.0));

        // Clone creates deep independent copy
        var clone = inv.Clone();
        clone.Add("Iron", 100.0);
        Assert.Equal(300.0, clone.GetQuantity("Iron"));
        Assert.Equal(200.0, inv.GetQuantity("Iron"));
    }
}
