namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.World.Services;
using Xunit;

public sealed class DomesticResourceProductionTests
{
    private static RepublicTime CreateBoundaryTime(int day = 0, int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(day, new TimeOnly(hour, 0));
    }

    private static (Country Arcadia, Country Norse, Country Lys) SetupTestCountries(RepublicTime time)
    {
        var arcadia = Country.Found("Republic of Arcadia", time, id: "country-arcadia");
        var norse = Country.Found("Federation of Norse", time, id: "country-norse");
        var lys = Country.Found("Commonwealth of Lys", time, id: "country-lys");

        return (arcadia, norse, lys);
    }

    [Fact]
    public void Produce_ValidCycleBoundary_IncreasesCorrectCountryInventory()
    {
        var time = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, _, _) = SetupTestCountries(time);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);

        var initialIron = arcadia.Inventory.GetQuantity("Iron");
        Assert.Equal(0.0, initialIron);

        var service = new ResourceProductionService();
        var outcome = service.Produce(arcadia, ironNode, time);

        Assert.True(outcome.Success);
        Assert.False(outcome.AlreadyProduced);
        Assert.Equal(50.0, outcome.QuantityProduced);
        Assert.Equal("Iron", outcome.ResourceType);
        Assert.Equal(time, outcome.Boundary);
        Assert.Equal(50.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(950.0, ironNode.Abundance);
        Assert.Equal(time, arcadia.LastResourceProductionBoundary);
    }

    [Fact]
    public void Produce_RepeatingSameCycle_DoesNotDuplicateProduction()
    {
        var time = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, _, _) = SetupTestCountries(time);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);

        var service = new ResourceProductionService();

        // First execution succeeds
        var first = service.Produce(arcadia, ironNode, time);
        Assert.True(first.Success);
        Assert.Equal(50.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(950.0, ironNode.Abundance);

        // Repeated execution with identical boundary must be rejected as duplicate
        var second = service.Produce(arcadia, ironNode, time);
        Assert.False(second.Success);
        Assert.True(second.AlreadyProduced);
        Assert.Equal(0.0, second.QuantityProduced);

        // Country method also returns failure
        var third = arcadia.ProduceResource(ironNode, time);
        Assert.False(third.Success);
        Assert.True(third.AlreadyProduced);

        // Inventories and abundance must remain unchanged from first execution
        Assert.Equal(50.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(950.0, ironNode.Abundance);
    }

    [Fact]
    public void Produce_AdvancingToNewValidCycle_PermitsNextProduction()
    {
        var cycle1 = CreateBoundaryTime(day: 1, hour: 6);
        var cycle2 = CreateBoundaryTime(day: 1, hour: 12);
        var cycle3 = CreateBoundaryTime(day: 1, hour: 18);
        var (arcadia, _, _) = SetupTestCountries(cycle1);

        var timberNode = new ResourceNode
        {
            Id = "node-arcadia-timber",
            ResourceType = "Timber",
            Abundance = 5000.0,
            ExtractionRate = 100.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(timberNode);

        var service = new ResourceProductionService();

        // Cycle 1: 06:00
        var out1 = service.Produce(arcadia, timberNode, cycle1);
        Assert.True(out1.Success);
        Assert.Equal(100.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(4900.0, timberNode.Abundance);

        // Cycle 2: 12:00
        var out2 = service.Produce(arcadia, timberNode, cycle2);
        Assert.True(out2.Success);
        Assert.Equal(200.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(4800.0, timberNode.Abundance);

        // Cycle 3: 18:00
        var out3 = service.Produce(arcadia, timberNode, cycle3);
        Assert.True(out3.Success);
        Assert.Equal(300.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(4700.0, timberNode.Abundance);
    }

    [Fact]
    public void Produce_MissingOrInactiveResourceNode_DoesNotProduceGoods()
    {
        var time = CreateBoundaryTime(day: 2, hour: 0);
        var (arcadia, _, _) = SetupTestCountries(time);

        var service = new ResourceProductionService();

        // 1. Missing node (null)
        var nullOutcome = service.Produce(arcadia, null!, time);
        Assert.False(nullOutcome.Success);
        Assert.Equal(0.0, nullOutcome.QuantityProduced);
        Assert.Contains("missing", nullOutcome.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Inactive node
        var inactiveNode = new ResourceNode
        {
            Id = "node-inactive-bauxite",
            ResourceType = "Bauxite",
            Abundance = 2000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = false
        };
        arcadia.AssociateResourceNode(inactiveNode);

        var inactiveOutcome = service.Produce(arcadia, inactiveNode, time);
        Assert.False(inactiveOutcome.Success);
        Assert.Equal(0.0, inactiveOutcome.QuantityProduced);
        Assert.Contains("inactive", inactiveOutcome.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Bauxite"));
        Assert.Equal(2000.0, inactiveNode.Abundance);
    }

    [Fact]
    public void Produce_UnrelatedCountryInventory_RemainsCompletelyUnchanged()
    {
        var time = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, norse, lys) = SetupTestCountries(time);

        // Seed initial stockpiles
        arcadia.Inventory.Add("Iron", 100.0);
        norse.Inventory.Add("Timber", 800.0);
        lys.Inventory.Add("Gold", 500.0);

        var initialNorseTimber = norse.Inventory.GetQuantity("Timber");
        var initialNorseIron = norse.Inventory.GetQuantity("Iron");
        var initialLysGold = lys.Inventory.GetQuantity("Gold");
        var initialLysIron = lys.Inventory.GetQuantity("Iron");

        var arcadiaIronNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 1000.0,
            ExtractionRate = 75.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(arcadiaIronNode);

        // Arcadia produces Iron
        var service = new ResourceProductionService();
        var outcome = service.Produce(arcadia, arcadiaIronNode, time);
        Assert.True(outcome.Success);
        Assert.Equal(175.0, arcadia.Inventory.GetQuantity("Iron"));

        // Norse and Lys must be 100% unchanged
        Assert.Equal(initialNorseTimber, norse.Inventory.GetQuantity("Timber"));
        Assert.Equal(initialNorseIron, norse.Inventory.GetQuantity("Iron"));
        Assert.Equal(initialLysGold, lys.Inventory.GetQuantity("Gold"));
        Assert.Equal(initialLysIron, lys.Inventory.GetQuantity("Iron"));
    }

    [Fact]
    public void Produce_NodeBelongingToAnotherCountry_IsRejected()
    {
        var time = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, norse, _) = SetupTestCountries(time);

        var norseNode = new ResourceNode
        {
            Id = "node-norse-copper",
            ResourceType = "Copper",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = norse.Id,
            IsActive = true
        };
        norse.AssociateResourceNode(norseNode);

        // Arcadia attempts to produce from Norse's copper node
        var service = new ResourceProductionService();
        var outcome = service.Produce(arcadia, norseNode, time);

        Assert.False(outcome.Success);
        Assert.Equal(0.0, outcome.QuantityProduced);
        Assert.Contains("not associated", outcome.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Copper"));
        Assert.Equal(1000.0, norseNode.Abundance);
    }

    [Fact]
    public void Produce_NonBoundaryTime_IsRejected()
    {
        var nonBoundary = CreateBoundaryTime(day: 1, hour: 7); // 07:00 is not a 6-hour boundary (0, 6, 12, 18)
        var (arcadia, _, _) = SetupTestCountries(nonBoundary);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);

        var service = new ResourceProductionService();
        var outcome = service.Produce(arcadia, ironNode, nonBoundary);

        Assert.False(outcome.Success);
        Assert.Equal(0.0, outcome.QuantityProduced);
        Assert.Contains("does not fall on an authoritative six-hour", outcome.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Iron"));
    }

    [Fact]
    public void Produce_DepletedNode_ProducesOnlyAvailableAbundanceThenStops()
    {
        var cycle1 = CreateBoundaryTime(day: 1, hour: 6);
        var cycle2 = CreateBoundaryTime(day: 1, hour: 12);
        var (arcadia, _, _) = SetupTestCountries(cycle1);

        // Node with only 30 units of non-renewable reserve, but extraction rate is 50
        var limitedNode = new ResourceNode
        {
            Id = "node-limited-petroleum",
            ResourceType = "Petroleum",
            Abundance = 30.0,
            ExtractionRate = 50.0,
            IsRenewable = false,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(limitedNode);

        var service = new ResourceProductionService();

        // Cycle 1 extracts the remaining 30 units
        var out1 = service.Produce(arcadia, limitedNode, cycle1);
        Assert.True(out1.Success);
        Assert.Equal(30.0, out1.QuantityProduced);
        Assert.Equal(30.0, arcadia.Inventory.GetQuantity("Petroleum"));
        Assert.Equal(0.0, limitedNode.Abundance);

        // Cycle 2: node is now completely depleted
        var out2 = service.Produce(arcadia, limitedNode, cycle2);
        Assert.False(out2.Success);
        Assert.Equal(0.0, out2.QuantityProduced);
        Assert.Contains("depleted", out2.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(30.0, arcadia.Inventory.GetQuantity("Petroleum"));
    }

    [Fact]
    public void Produce_RenewableNode_DoesNotDepleteAbundance()
    {
        var cycle1 = CreateBoundaryTime(day: 1, hour: 6);
        var cycle2 = CreateBoundaryTime(day: 1, hour: 12);
        var (arcadia, _, _) = SetupTestCountries(cycle1);

        var hydroNode = new ResourceNode
        {
            Id = "node-hydro-timber",
            ResourceType = "Timber",
            Abundance = 500.0,
            ExtractionRate = 50.0,
            IsRenewable = true,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(hydroNode);

        var service = new ResourceProductionService();

        var out1 = service.Produce(arcadia, hydroNode, cycle1);
        Assert.True(out1.Success);
        Assert.Equal(50.0, out1.QuantityProduced);
        Assert.Equal(500.0, hydroNode.Abundance); // Undepleted!

        var out2 = service.Produce(arcadia, hydroNode, cycle2);
        Assert.True(out2.Success);
        Assert.Equal(50.0, out2.QuantityProduced);
        Assert.Equal(500.0, hydroNode.Abundance); // Still undepleted!
        Assert.Equal(100.0, arcadia.Inventory.GetQuantity("Timber"));
    }

    [Fact]
    public void ProduceDueCycles_ExecutesAllDueBoundariesUpToCurrentTime()
    {
        var launchTime = CreateBoundaryTime(day: 0, hour: 0);
        var currentTime = CreateBoundaryTime(day: 0, hour: 18); // 3 due boundaries: 06:00, 12:00, 18:00
        var (arcadia, _, _) = SetupTestCountries(launchTime);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 10000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);

        var service = new ResourceProductionService();
        var results = service.ProduceDueCycles(arcadia, ironNode, currentTime);

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal(150.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(currentTime, arcadia.LastResourceProductionBoundary);
    }

    // --- Regression & Validation Tests: Rejection of Non-Finite Numbers (NaN and Infinity) ---

    [Fact]
    public void BilateralTradeTerms_NonFiniteQuantityOrPrice_ThrowsArgumentOutOfRangeException()
    {
        // NaN quantity
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", double.NaN, 100_000.0));

        // Positive Infinity quantity
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", double.PositiveInfinity, 100_000.0));

        // Negative Infinity quantity
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", double.NegativeInfinity, 100_000.0));

        // NaN price
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", 500.0, double.NaN));

        // Positive Infinity price
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", 500.0, double.PositiveInfinity));

        // Negative Infinity price
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BilateralTradeTerms("country-a", "country-b", "Iron", 500.0, double.NegativeInfinity));
    }

    [Fact]
    public void ResourceInventory_NonFiniteQuantities_AreRejectedSafely()
    {
        var inventory = new ResourceInventory();

        // Add NaN throws
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add("Iron", double.NaN));

        // Add PositiveInfinity throws
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add("Iron", double.PositiveInfinity));

        // Add NegativeInfinity throws
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add("Iron", double.NegativeInfinity));

        // Remove NaN throws
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Remove("Iron", double.NaN));

        // Remove PositiveInfinity throws
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Remove("Iron", double.PositiveInfinity));

        // TryRemove with NaN or Infinity returns false
        Assert.False(inventory.TryRemove("Iron", double.NaN));
        Assert.False(inventory.TryRemove("Iron", double.PositiveInfinity));
        Assert.False(inventory.TryRemove("Iron", double.NegativeInfinity));

        // HasSufficient with NaN or Infinity returns false
        Assert.False(inventory.HasSufficient("Iron", double.NaN));
        Assert.False(inventory.HasSufficient("Iron", double.PositiveInfinity));

        // Stockpile remains 0
        Assert.Equal(0.0, inventory.GetQuantity("Iron"));
    }

    [Fact]
    public void RepuTreasury_NonFiniteAmounts_AreRejectedSafely()
    {
        var treasury = new RepuTreasury(500_000.0, "country-test");

        // Deposit NaN / Infinity throws
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Deposit(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Deposit(double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Deposit(double.NegativeInfinity));

        // Withdraw NaN / Infinity throws
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Withdraw(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Withdraw(double.PositiveInfinity));

        // TryWithdraw with NaN / Infinity returns false without modifying balance
        Assert.False(treasury.TryWithdraw(double.NaN));
        Assert.False(treasury.TryWithdraw(double.PositiveInfinity));
        Assert.False(treasury.TryWithdraw(double.NegativeInfinity));

        // Balance remains unchanged
        Assert.Equal(500_000.0, treasury.Balance);

        // Constructor rejecting NaN / Infinity
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepuTreasury(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepuTreasury(double.PositiveInfinity));
    }

    [Fact]
    public void ResourceNode_NonFiniteRatesOrAbundance_AreRejectedSafely()
    {
        var node = new ResourceNode();

        // Non-finite Abundance throws
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Abundance = double.NaN);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Abundance = double.PositiveInfinity);

        // Non-finite ExtractionRate throws
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ExtractionRate = double.NaN);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ExtractionRate = double.PositiveInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ExtractionRate = 0.0);
    }

    // --- Phase 9.1 Regression Tests: Multi-Node Reliability & Strict Ownership Enforcement ---

    [Fact]
    public void ProduceDueCycles_MultipleNodesOwnedBySameCountry_AllProduceDuringSameCycle()
    {
        var launchTime = CreateBoundaryTime(day: 0, hour: 0);
        var cycleTime = CreateBoundaryTime(day: 0, hour: 6);
        var (arcadia, _, _) = SetupTestCountries(launchTime);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron-1",
            ResourceType = "Iron",
            Abundance = 5000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        var timberNode = new ResourceNode
        {
            Id = "node-arcadia-timber-1",
            ResourceType = "Timber",
            Abundance = 3000.0,
            ExtractionRate = 75.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);
        arcadia.AssociateResourceNode(timberNode);

        var service = new ResourceProductionService();
        var results = service.ProduceDueCycles(arcadia, cycleTime);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal(50.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(75.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(cycleTime, ironNode.LastProductionBoundary);
        Assert.Equal(cycleTime, timberNode.LastProductionBoundary);
        Assert.Equal(4950.0, ironNode.Abundance);
        Assert.Equal(2925.0, timberNode.Abundance);
    }

    [Fact]
    public void ProduceDueCycles_RepeatedCatchUpCalls_DoNotDuplicateProductionForAnyNode()
    {
        var launchTime = CreateBoundaryTime(day: 0, hour: 0);
        var currentTime = CreateBoundaryTime(day: 0, hour: 18); // 3 due boundaries: 06:00, 12:00, 18:00
        var (arcadia, _, _) = SetupTestCountries(launchTime);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 10000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        var timberNode = new ResourceNode
        {
            Id = "node-arcadia-timber",
            ResourceType = "Timber",
            Abundance = 10000.0,
            ExtractionRate = 60.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        arcadia.AssociateResourceNode(ironNode);
        arcadia.AssociateResourceNode(timberNode);

        var service = new ResourceProductionService();

        // First catch-up run: both nodes catch up 3 cycles
        var firstResults = service.ProduceDueCycles(arcadia, currentTime);
        Assert.Equal(6, firstResults.Count); // 3 cycles * 2 nodes
        Assert.All(firstResults, r => Assert.True(r.Success));
        Assert.Equal(150.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(180.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(9850.0, ironNode.Abundance);
        Assert.Equal(9820.0, timberNode.Abundance);

        // Immediate repeated catch-up run: no new production
        var secondResults = service.ProduceDueCycles(arcadia, currentTime);
        Assert.Empty(secondResults);
        Assert.Equal(150.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(180.0, arcadia.Inventory.GetQuantity("Timber"));
        Assert.Equal(9850.0, ironNode.Abundance);
        Assert.Equal(9820.0, timberNode.Abundance);

        // Individual node repeated calls also yield no production
        var individualIron = service.ProduceDueCycles(arcadia, ironNode, currentTime);
        Assert.Empty(individualIron);
        var individualTimber = service.ProduceDueCycles(arcadia, timberNode, currentTime);
        Assert.Empty(individualTimber);

        Assert.Equal(150.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(180.0, arcadia.Inventory.GetQuantity("Timber"));
    }

    [Fact]
    public void ProduceDueCycles_MultipleMissedCycles_CaughtUpCorrectlyAcrossAllEligibleNodes()
    {
        var launchTime = CreateBoundaryTime(day: 0, hour: 0);
        var currentTime = CreateBoundaryTime(day: 1, hour: 0); // 4 due boundaries: 06:00, 12:00, 18:00, Day 1 00:00
        var (arcadia, _, _) = SetupTestCountries(launchTime);

        var ironNode = new ResourceNode
        {
            Id = "node-arcadia-iron",
            ResourceType = "Iron",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        var timberNode = new ResourceNode
        {
            Id = "node-arcadia-timber",
            ResourceType = "Timber",
            Abundance = 2000.0,
            ExtractionRate = 100.0,
            CountryId = arcadia.Id,
            IsActive = true
        };
        var inactiveBauxiteNode = new ResourceNode
        {
            Id = "node-arcadia-bauxite",
            ResourceType = "Bauxite",
            Abundance = 1000.0,
            ExtractionRate = 25.0,
            CountryId = arcadia.Id,
            IsActive = false // Inactive node must not produce or block other nodes
        };

        arcadia.AssociateResourceNode(ironNode);
        arcadia.AssociateResourceNode(timberNode);
        arcadia.AssociateResourceNode(inactiveBauxiteNode);

        var service = new ResourceProductionService();
        var results = service.ProduceDueCycles(arcadia, currentTime);

        // 4 boundaries * 2 active nodes = 8 results
        Assert.Equal(8, results.Count);
        Assert.All(results, r => Assert.True(r.Success));

        Assert.Equal(200.0, arcadia.Inventory.GetQuantity("Iron")); // 4 * 50
        Assert.Equal(400.0, arcadia.Inventory.GetQuantity("Timber")); // 4 * 100
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Bauxite")); // Inactive node produced 0

        Assert.Equal(800.0, ironNode.Abundance);
        Assert.Equal(1600.0, timberNode.Abundance);
        Assert.Equal(1000.0, inactiveBauxiteNode.Abundance);

        Assert.Equal(currentTime, ironNode.LastProductionBoundary);
        Assert.Equal(currentTime, timberNode.LastProductionBoundary);
        Assert.Null(inactiveBauxiteNode.LastProductionBoundary);
    }

    [Fact]
    public void AssociateAndProduce_ForeignOwnedNode_IsStrictlyRejected()
    {
        var launchTime = CreateBoundaryTime(day: 1, hour: 0);
        var boundaryTime = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, norse, _) = SetupTestCountries(launchTime);

        var norseNode = new ResourceNode
        {
            Id = "node-norse-gold",
            ResourceType = "Gold",
            Abundance = 1000.0,
            ExtractionRate = 25.0,
            CountryId = norse.Id,
            IsActive = true
        };
        var norseAssociated = norse.AssociateResourceNode(norseNode);
        Assert.True(norseAssociated);

        // 1. Arcadia cannot associate Norse's node
        var arcadiaAssociated = arcadia.AssociateResourceNode(norseNode);
        Assert.False(arcadiaAssociated);
        Assert.DoesNotContain(norseNode, arcadia.ResourceNodes);
        Assert.Equal(norse.Id, norseNode.CountryId); // Ownership unchanged

        // 2. Arcadia cannot extract Norse's node even if passed to Produce
        var service = new ResourceProductionService();
        var produceResult = service.Produce(arcadia, norseNode, boundaryTime);
        Assert.False(produceResult.Success);
        Assert.Contains("owned by country", produceResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not associated", produceResult.Message, StringComparison.OrdinalIgnoreCase);

        // 3. Arcadia cannot extract Norse's node via ProduceDueCycles
        var dueResults = service.ProduceDueCycles(arcadia, norseNode, boundaryTime);
        Assert.Single(dueResults);
        Assert.False(dueResults[0].Success);
        Assert.Contains("owned by country", dueResults[0].Message, StringComparison.OrdinalIgnoreCase);

        // 4. Manual insertion into ResourceNodes list must still be rejected by Produce and ProduceDueCycles
        arcadia.ResourceNodes.Add(norseNode);
        var manualProduceResult = service.Produce(arcadia, norseNode, boundaryTime);
        Assert.False(manualProduceResult.Success);
        Assert.Contains("owned by country", manualProduceResult.Message, StringComparison.OrdinalIgnoreCase);

        var countryDueResults = service.ProduceDueCycles(arcadia, boundaryTime);
        Assert.Empty(countryDueResults); // Excluded from country's due cycle execution
    }

    [Fact]
    public void Produce_FailedOwnershipCheck_LeavesBothCountriesAndNodeCompletelyUnchanged()
    {
        var boundaryTime = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, norse, _) = SetupTestCountries(boundaryTime);

        // Seed initial stockpiles
        arcadia.Inventory.Add("Gold", 50.0);
        norse.Inventory.Add("Gold", 200.0);

        var initialArcadiaGold = arcadia.Inventory.GetQuantity("Gold");
        var initialNorseGold = norse.Inventory.GetQuantity("Gold");

        var norseGoldNode = new ResourceNode
        {
            Id = "node-norse-gold-mine",
            ResourceType = "Gold",
            Abundance = 5000.0,
            ExtractionRate = 100.0,
            CountryId = norse.Id,
            IsActive = true
        };
        norse.AssociateResourceNode(norseGoldNode);

        var service = new ResourceProductionService();

        // Arcadia attempts unauthorized extraction
        var failedResult = service.Produce(arcadia, norseGoldNode, boundaryTime);
        Assert.False(failedResult.Success);
        Assert.Equal(0.0, failedResult.QuantityProduced);

        // Arcadia's stockpile must be unchanged
        Assert.Equal(initialArcadiaGold, arcadia.Inventory.GetQuantity("Gold"));

        // Norse's stockpile must be unchanged
        Assert.Equal(initialNorseGold, norse.Inventory.GetQuantity("Gold"));

        // Node's abundance, ownership, and boundary must be unchanged
        Assert.Equal(5000.0, norseGoldNode.Abundance);
        Assert.Equal(norse.Id, norseGoldNode.CountryId);
        Assert.Null(norseGoldNode.LastProductionBoundary);
    }

    [Fact]
    public void ProduceDueCycles_DepletedOrInactiveNode_DoesNotPreventOtherEligibleNodesFromProducing()
    {
        var launchTime = CreateBoundaryTime(day: 0, hour: 0);
        var currentTime = CreateBoundaryTime(day: 0, hour: 12); // 2 boundaries: 06:00, 12:00
        var (arcadia, _, _) = SetupTestCountries(launchTime);

        // Node 1: Depleted petroleum (abundance 0)
        var depletedPetroleum = new ResourceNode
        {
            Id = "node-arcadia-depleted-oil",
            ResourceType = "Petroleum",
            Abundance = 0.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true,
            IsRenewable = false
        };

        // Node 2: Inactive copper node
        var inactiveCopper = new ResourceNode
        {
            Id = "node-arcadia-inactive-copper",
            ResourceType = "Copper",
            Abundance = 1000.0,
            ExtractionRate = 40.0,
            CountryId = arcadia.Id,
            IsActive = false
        };

        // Node 3: Active, healthy Iron node
        var healthyIron = new ResourceNode
        {
            Id = "node-arcadia-healthy-iron",
            ResourceType = "Iron",
            Abundance = 2000.0,
            ExtractionRate = 50.0,
            CountryId = arcadia.Id,
            IsActive = true
        };

        arcadia.AssociateResourceNode(depletedPetroleum);
        arcadia.AssociateResourceNode(inactiveCopper);
        arcadia.AssociateResourceNode(healthyIron);

        var service = new ResourceProductionService();
        var results = service.ProduceDueCycles(arcadia, currentTime);

        // Healthy iron node must produce both cycles
        Assert.Equal(100.0, arcadia.Inventory.GetQuantity("Iron"));
        Assert.Equal(1900.0, healthyIron.Abundance);
        Assert.Equal(currentTime, healthyIron.LastProductionBoundary);

        // Depleted petroleum and inactive copper must not have produced goods
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Petroleum"));
        Assert.Equal(0.0, arcadia.Inventory.GetQuantity("Copper"));
        Assert.Equal(0.0, depletedPetroleum.Abundance);
        Assert.Equal(1000.0, inactiveCopper.Abundance);
    }

    [Fact]
    public void AssociateResourceNode_UnassignedNode_SuccessfullyClaimsAndExtracts()
    {
        var boundaryTime = CreateBoundaryTime(day: 1, hour: 6);
        var (arcadia, _, _) = SetupTestCountries(boundaryTime);

        var neutralNode = new ResourceNode
        {
            Id = "node-neutral-clay",
            ResourceType = "Clay",
            Abundance = 1000.0,
            ExtractionRate = 50.0,
            CountryId = string.Empty, // Unassigned
            IsActive = true
        };

        var associated = arcadia.AssociateResourceNode(neutralNode);
        Assert.True(associated);
        Assert.Equal(arcadia.Id, neutralNode.CountryId);
        Assert.Contains(neutralNode, arcadia.ResourceNodes);

        var service = new ResourceProductionService();
        var result = service.Produce(arcadia, neutralNode, boundaryTime);
        Assert.True(result.Success);
        Assert.Equal(50.0, arcadia.Inventory.GetQuantity("Clay"));
    }
}
