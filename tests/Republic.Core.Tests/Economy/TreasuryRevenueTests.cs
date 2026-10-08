namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class TreasuryRevenueTests
{
    private readonly TreasuryRevenueService _revenueService = new();

    private static Country CreateTestCountry(string name = "Republic of Testing", double initialTreasury = 0.0)
    {
        var clock = RepublicClock.CreateControlled();
        var countryId = Guid.NewGuid().ToString("N");
        return Country.Found(
            name: name,
            foundingTime: clock.CurrentTime,
            id: countryId,
            treasury: new RepuTreasury(initialTreasury, countryId),
            stateCapacity: StateCapacityInputs.All(1.0));
    }

    [Fact]
    public void Test1_InitialTreasury_HasValidSmallestSensibleDefaultStartingBalance()
    {
        // 1. Standalone treasury defaults to 0.0 REPU
        var standaloneTreasury = new RepuTreasury();
        Assert.Equal(0.0, standaloneTreasury.Balance);
        Assert.Equal("R0", standaloneTreasury.FormattedBalance);
        Assert.Equal("R0", standaloneTreasury.ToString());

        // 2. Newly founded country defaults to 0.0 REPU
        var country = CreateTestCountry();
        Assert.NotNull(country.Treasury);
        Assert.Equal(0.0, country.Treasury.Balance);
        Assert.Equal(country.Id, country.Treasury.CountryId);
    }

    [Fact]
    public void Test2_RevenueIncreasesTreasury_ByExactAmount()
    {
        var treasury = new RepuTreasury(100.0, "c1");
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = 45_000_000.0
        };

        var result = _revenueService.ApplyRevenue(treasury, yield);

        Assert.Same(treasury, result);
        Assert.Equal(45_000_100.0, treasury.Balance);
        Assert.Equal("R45M", RepuTreasury.Format(45_000_000.0));
    }

    [Fact]
    public void Test3_ZeroRevenue_DoesNotChangeTreasury()
    {
        var treasury = new RepuTreasury(500.0, "c1");
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = 0.0
        };

        _revenueService.ApplyRevenue(treasury, yield);

        Assert.Equal(500.0, treasury.Balance);
    }

    [Fact]
    public void Test4_CountryIsolation_ApplyingRevenueToCountryA_DoesNotChangeCountryBTreasury()
    {
        var countryA = CreateTestCountry("Country A", initialTreasury: 50.0);
        var countryB = CreateTestCountry("Country B", initialTreasury: 200.0);

        var yieldA = new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = 1500.0
        };

        _revenueService.ApplyRevenue(countryA, yieldA);

        // Country A receives revenue
        Assert.Equal(1550.0, countryA.Treasury.Balance);

        // Country B's treasury is strictly isolated and unchanged
        Assert.Equal(200.0, countryB.Treasury.Balance);
    }

    [Fact]
    public void Test5_NoModificationOfNationalYield_YieldRemainsUnmodified()
    {
        var treasury = new RepuTreasury(0.0, "c1");
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = 75.0,
            IndustrialCapacity = 80.0,
            EnergyCapacity = 70.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 90.0,
            ScienceCapacity = 85.0
        };

        // Snapshot original values
        var originalRevenue = yield.RepuTreasuryRevenue;
        var originalIndustry = yield.IndustrialCapacity;
        var originalHuman = yield.HumanCapital;

        _revenueService.ApplyRevenue(treasury, yield);

        // Assert yield object was completely unmutated
        Assert.Equal(originalRevenue, yield.RepuTreasuryRevenue);
        Assert.Equal(originalIndustry, yield.IndustrialCapacity);
        Assert.Equal(originalHuman, yield.HumanCapital);
    }

    [Fact]
    public void Test6_NoDuplicateCalculation_ConsumesExistingResultDirectly()
    {
        // Supply an arbitrary REPU revenue value that does not come from or match calculator formulas
        var arbitraryRevenue = 123_456.789;
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            RepuTreasuryRevenue = arbitraryRevenue,
            IndustrialCapacity = 0.0 // Economic base is 0, yet revenue is explicitly supplied
        };

        var treasury = new RepuTreasury(0.0, "c1");
        _revenueService.ApplyRevenue(treasury, yield);

        // The service directly applied the pre-evaluated revenue without recalculation
        Assert.Equal(arbitraryRevenue, treasury.Balance);
    }

    [Fact]
    public void Test7_InvalidSpending_FailsSafelyWithoutDebt()
    {
        var treasury = new RepuTreasury(50.0, "c1");

        // 1. TryWithdraw returns false and does not mutate balance
        var trySuccess = treasury.TryWithdraw(100.0);
        Assert.False(trySuccess);
        Assert.Equal(50.0, treasury.Balance);

        // 2. Withdraw throws InvalidOperationException and does not mutate balance
        var ex = Assert.Throws<InvalidOperationException>(() => treasury.Withdraw(100.0));
        Assert.Contains("Insufficient treasury funds", ex.Message);
        Assert.Equal(50.0, treasury.Balance);

        // 3. Valid spend succeeds
        treasury.Withdraw(20.0);
        Assert.Equal(30.0, treasury.Balance);

        var validTry = treasury.TryWithdraw(30.0);
        Assert.True(validTry);
        Assert.Equal(0.0, treasury.Balance);
    }

    [Theory]
    [InlineData(0.0, "R0")]
    [InlineData(500.0, "R500")]
    [InlineData(1_000.0, "R1K")]
    [InlineData(45_000_000.0, "R45M")]
    [InlineData(450_000_000.0, "R450M")]
    [InlineData(1_200_000_000.0, "R1.2B")]
    public void RepuCurrencyFormatting_MatchesExpectedTerminology(double amount, string expected)
    {
        Assert.Equal(expected, RepuTreasury.Format(amount));
    }

    [Fact]
    public void SnapshotOverload_AppliesRevenueCorrectly()
    {
        var clock = RepublicClock.CreateControlled();
        var snapshot = new NationalYieldSnapshot(
            "c1",
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 250.0 });

        var treasury = new RepuTreasury(10.0, "c1");
        _revenueService.ApplyRevenue(treasury, snapshot);

        Assert.Equal(260.0, treasury.Balance);
    }

    [Fact]
    public void DuplicateSnapshotApplication_DoesNotDoubleCreditTreasury()
    {
        var clock = RepublicClock.CreateControlled();
        var snapshot = new NationalYieldSnapshot(
            "c1",
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 250.0 });

        var treasury = new RepuTreasury(10.0, "c1");

        // First application deposits revenue
        _revenueService.ApplyRevenue(treasury, snapshot);
        Assert.Equal(260.0, treasury.Balance);

        // Second application of the exact same snapshot is ignored
        _revenueService.ApplyRevenue(treasury, snapshot);
        Assert.Equal(260.0, treasury.Balance);
    }

    [Fact]
    public void NegativeAmounts_ThrowArgumentOutOfRangeException()
    {
        var treasury = new RepuTreasury();

        Assert.Throws<ArgumentOutOfRangeException>(() => new RepuTreasury(-10.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Deposit(-5.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Withdraw(-5.0));
    }

    [Fact]
    public void FirstCredit_IncreasesBalance_BySnapshotRepuTreasuryRevenue()
    {
        var clock = RepublicClock.CreateControlled();
        var country = CreateTestCountry("First Credit Nation", initialTreasury: 100.0);
        var snapshot = new NationalYieldSnapshot(
            country.Id,
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 450.0 });

        var treasury = _revenueService.ApplyRevenue(country, snapshot);

        Assert.Equal(550.0, treasury.Balance);
        Assert.Equal(550.0, country.Treasury.Balance);
    }

    [Fact]
    public void SecondCredit_OfSameSnapshot_DoesNotChangeBalance()
    {
        var clock = RepublicClock.CreateControlled();
        var country = CreateTestCountry("Idempotent Credit Nation", initialTreasury: 50.0);
        var snapshot = new NationalYieldSnapshot(
            country.Id,
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 200.0 });

        _revenueService.ApplyRevenue(country, snapshot);
        Assert.Equal(250.0, country.Treasury.Balance);

        // Second credit with identical snapshot identity returns existing balance and does not deposit again
        var secondResult = _revenueService.ApplyRevenue(country, snapshot);
        Assert.Equal(250.0, secondResult.Balance);
        Assert.Equal(250.0, country.Treasury.Balance);
    }

    [Fact]
    public void DifferentSimulationTime_CanCreditAgain()
    {
        var clock = RepublicClock.CreateControlled();
        var country = CreateTestCountry("MultiTime Nation", initialTreasury: 0.0);

        var time1 = clock.FromDayAndTime(1, new TimeOnly(6, 0));
        var time2 = clock.FromDayAndTime(1, new TimeOnly(12, 0));

        var snapshot1 = new NationalYieldSnapshot(country.Id, time1, new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 100.0 });
        var snapshot2 = new NationalYieldSnapshot(country.Id, time2, new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 150.0 });

        _revenueService.ApplyRevenue(country, snapshot1);
        Assert.Equal(100.0, country.Treasury.Balance);

        _revenueService.ApplyRevenue(country, snapshot2);
        Assert.Equal(250.0, country.Treasury.Balance);
    }

    [Fact]
    public void FoundingDayCredit_IsGreaterThan_NonFoundingDayCredit_ForEqualInputs()
    {
        var clock = RepublicClock.CreateControlled();
        var foundingTime = clock.FromDayAndTime(5, new TimeOnly(10, 0));
        var nonFoundingTime = clock.FromDayAndTime(6, new TimeOnly(10, 0));

        var foundingCountry = Country.Found("Founding Nation", foundingTime, baselineStability: 75.0,
            yield: new Republic.Core.NationalYield.NationalYield
            {
                IndustrialCapacity = 60.0,
                InfrastructureCapacity = 60.0,
                AdministrativeCapacity = 70.0
            });

        var nonFoundingCountry = Country.Found("Non-Founding Nation", foundingTime, baselineStability: 75.0,
            yield: new Republic.Core.NationalYield.NationalYield
            {
                IndustrialCapacity = 60.0,
                InfrastructureCapacity = 60.0,
                AdministrativeCapacity = 70.0
            });

        var cycleService = new NationalYieldCycleService();

        // Founding day cycle execution applies x10 scaled yield to treasury
        cycleService.ExecuteCycle(foundingCountry, foundingTime);
        var foundingBalance = foundingCountry.Treasury.Balance;

        // Non-founding day cycle execution applies 1x scaled yield to treasury
        cycleService.ExecuteCycle(nonFoundingCountry, nonFoundingTime);
        var nonFoundingBalance = nonFoundingCountry.Treasury.Balance;

        Assert.True(foundingBalance > nonFoundingBalance);
        Assert.Equal(nonFoundingBalance * 10.0, foundingBalance);
    }

    [Fact]
    public void CountryIsolation_CreditingCountryA_DoesNotChangeCountryB()
    {
        var clock = RepublicClock.CreateControlled();
        var countryA = CreateTestCountry("Country A", initialTreasury: 100.0);
        var countryB = CreateTestCountry("Country B", initialTreasury: 200.0);

        var snapshotA = new NationalYieldSnapshot(
            countryA.Id,
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 50.0 });

        _revenueService.ApplyRevenue(countryA, snapshotA);

        Assert.Equal(150.0, countryA.Treasury.Balance);
        Assert.Equal(200.0, countryB.Treasury.Balance);
        Assert.True(countryA.Treasury.HasAppliedSnapshot(snapshotA.Id));
        Assert.False(countryB.Treasury.HasAppliedSnapshot(snapshotA.Id));
    }

    [Fact]
    public void NegativeOrZeroRevenue_DoesNotThrow_AndDoesNotDecreaseBalance()
    {
        var clock = RepublicClock.CreateControlled();
        var country = CreateTestCountry("Zero Revenue Nation", initialTreasury: 300.0);

        // Zero revenue snapshot
        var zeroSnapshot = new NationalYieldSnapshot(
            country.Id,
            clock.FromDayAndTime(1, new TimeOnly(6, 0)),
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 0.0 });

        _revenueService.ApplyRevenue(country, zeroSnapshot);
        Assert.Equal(300.0, country.Treasury.Balance);

        // Negative revenue snapshot (defensive verification)
        var negativeSnapshot = new NationalYieldSnapshot(
            country.Id,
            clock.FromDayAndTime(1, new TimeOnly(12, 0)),
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = -50.0 });

        _revenueService.ApplyRevenue(country, negativeSnapshot);
        Assert.Equal(300.0, country.Treasury.Balance);
    }

    [Fact]
    public void ApplyingRevenue_DoesNotMutateYieldSnapshot()
    {
        var clock = RepublicClock.CreateControlled();
        var snapshot = new NationalYieldSnapshot(
            "c1",
            clock.CurrentTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 250.0, IndustrialCapacity = 80.0 });

        var treasury = new RepuTreasury(50.0, "c1");
        _revenueService.ApplyRevenue(treasury, snapshot);

        Assert.Equal(250.0, snapshot.Yield.RepuTreasuryRevenue);
        Assert.Equal(80.0, snapshot.Yield.IndustrialCapacity);
    }
}
