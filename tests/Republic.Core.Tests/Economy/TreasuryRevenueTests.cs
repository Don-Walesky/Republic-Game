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
            treasury: new RepuTreasury(initialTreasury, countryId));
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
    public void NegativeAmounts_ThrowArgumentOutOfRangeException()
    {
        var treasury = new RepuTreasury();

        Assert.Throws<ArgumentOutOfRangeException>(() => new RepuTreasury(-10.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Deposit(-5.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => treasury.Withdraw(-5.0));
    }
}
