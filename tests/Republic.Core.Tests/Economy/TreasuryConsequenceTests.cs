namespace Republic.Core.Tests.Economy;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class TreasuryConsequenceTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void LowTreasury_R50MillionAtBoundary_DropsApprovalBy2()
    {
        var boundary = CreateTestTime(6);
        var initialApproval = 68.5;
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(50_000_000.0), happinessRating: initialApproval);

        var consequence = TreasuryConsequence.Apply(country, boundary);

        Assert.NotNull(consequence);
        Assert.Equal(initialApproval - 2.0, country.ApprovalRating);
        Assert.Equal(2.0, consequence.ApprovalDrop);
        Assert.Equal(initialApproval, consequence.PreviousApproval);
        Assert.Equal(66.5, consequence.CurrentApproval);
        Assert.Equal(country.LatestTreasuryConsequence, consequence);
        Assert.Contains("Approval -2%", consequence.Description);
    }

    [Fact]
    public void SameBoundary_AppliedTwice_DropsApprovalBy2Not4()
    {
        var boundary = CreateTestTime(6);
        var initialApproval = 68.5;
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(50_000_000.0), happinessRating: initialApproval);

        var firstCall = TreasuryConsequence.Apply(country, boundary);
        var secondCall = TreasuryConsequence.Apply(country, boundary);

        Assert.NotNull(firstCall);
        Assert.Null(secondCall);
        Assert.Equal(initialApproval - 2.0, country.ApprovalRating);
        Assert.Equal(66.5, country.ApprovalRating);
    }

    [Fact]
    public void ThresholdOrHigher_R100Million_DropsApprovalBy0()
    {
        var boundary = CreateTestTime(6);
        var initialApproval = 68.5;
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(100_000_000.0), happinessRating: initialApproval);

        var consequence = TreasuryConsequence.Apply(country, boundary);

        Assert.Null(consequence);
        Assert.Equal(initialApproval, country.ApprovalRating);
        Assert.Null(country.LatestTreasuryConsequence);

        // Same boundary called again also changes nothing
        var secondCall = TreasuryConsequence.Apply(country, boundary);
        Assert.Null(secondCall);
        Assert.Equal(initialApproval, country.ApprovalRating);
    }

    [Fact]
    public void ThresholdOrHigher_AboveR100Million_DropsApprovalBy0()
    {
        var boundary = CreateTestTime(6);
        var initialApproval = 75.0;
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(500_000_000.0), happinessRating: initialApproval);

        var consequence = TreasuryConsequence.Apply(country, boundary);

        Assert.Null(consequence);
        Assert.Equal(initialApproval, country.ApprovalRating);
    }

    [Fact]
    public void LowApproval_DoesNotFallBelowZero()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(10_000_000.0), happinessRating: 1.0);

        var consequence = TreasuryConsequence.Apply(country, boundary);

        Assert.NotNull(consequence);
        Assert.Equal(0.0, country.ApprovalRating);
        Assert.Equal(1.0, consequence.ApprovalDrop);
        Assert.Equal(1.0, consequence.PreviousApproval);
        Assert.Equal(0.0, consequence.CurrentApproval);

        // If already at 0.0, drops by 0 and stays clamped at 0
        var boundary2 = CreateTestTime(12);
        var consequence2 = TreasuryConsequence.Apply(country, boundary2);
        Assert.NotNull(consequence2);
        Assert.Equal(0.0, country.ApprovalRating);
        Assert.Equal(0.0, consequence2.ApprovalDrop);
    }

    [Fact]
    public void CountryIsolation_RunningForCountryA_LeavesCountryBUnchanged()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Alpha", boundary, treasury: new RepuTreasury(30_000_000.0), happinessRating: 60.0);
        var countryB = Country.Found("Republic of Beta", boundary, treasury: new RepuTreasury(50_000_000.0), happinessRating: 80.0);

        var initialTreasuryB = countryB.Treasury.Balance;
        var initialApprovalB = countryB.ApprovalRating;
        var initialConsequencesB = countryB.AppliedConsequenceBoundaryIds.Count;

        var consequenceA = TreasuryConsequence.Apply(countryA, boundary);

        Assert.NotNull(consequenceA);
        Assert.Equal(58.0, countryA.ApprovalRating);

        // Country B is completely unaffected
        Assert.Equal(initialApprovalB, countryB.ApprovalRating);
        Assert.Equal(initialTreasuryB, countryB.Treasury.Balance);
        Assert.Equal(initialConsequencesB, countryB.AppliedConsequenceBoundaryIds.Count);
        Assert.Null(countryB.LatestTreasuryConsequence);
    }

    [Fact]
    public void DueCyclePath_AppliesConsequenceAfterYieldCredit()
    {
        var clock = RepublicClock.CreateControlled();
        var foundingTime = clock.FromDayAndTime(0, new TimeOnly(0, 0));
        var time0600 = clock.FromDayAndTime(0, new TimeOnly(6, 0));

        var country = Country.Found("Republic of Arcadia", foundingTime, treasury: new RepuTreasury(50_000_000.0), happinessRating: 70.0);
        var cycleService = new NationalYieldCycleService();

        var snapshots = cycleService.ExecuteDueCycles(country, time0600);

        Assert.Single(snapshots);
        // Approval dropped by 2 because balance after yield remains below R100M
        Assert.Equal(68.0, country.ApprovalRating);
        Assert.NotNull(country.LatestTreasuryConsequence);

        // Executing due cycles again at the same boundary does not drop approval again
        var secondPass = cycleService.ExecuteDueCycles(country, time0600);
        Assert.Empty(secondPass);
        Assert.Equal(68.0, country.ApprovalRating);
    }

    [Fact]
    public void DueCyclePath_YieldCreditCrossingThreshold_PreventsApprovalDrop()
    {
        var clock = RepublicClock.CreateControlled();
        var foundingTime = clock.FromDayAndTime(0, new TimeOnly(0, 0));
        var time0600 = clock.FromDayAndTime(0, new TimeOnly(6, 0));

        // Start with R99,999,999; yield credit will deposit > R1 so balance crosses R100M
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = 50.0,
            InfrastructureCapacity = 50.0,
            HumanCapital = 50.0,
            AdministrativeCapacity = 50.0,
            SecurityCapacity = 50.0,
            ScienceCapacity = 50.0
        };
        var country = Country.Found("Republic of Arcadia", foundingTime, yield: yield, treasury: new RepuTreasury(99_999_999.0), happinessRating: 70.0);

        var cycleService = new NationalYieldCycleService();

        var snapshots = cycleService.ExecuteDueCycles(country, time0600);

        Assert.Single(snapshots);
        Assert.True(country.Treasury.Balance >= 100_000_000.0);
        // Since balance after yield credit is >= R100M, approval does NOT drop
        Assert.Equal(70.0, country.ApprovalRating);
        Assert.Null(country.LatestTreasuryConsequence);
    }

    [Fact]
    public void Service_DelegatesCorrectly()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Test", boundary, treasury: new RepuTreasury(20_000_000.0), happinessRating: 50.0);

        ITreasuryConsequenceService service = new TreasuryConsequenceService();
        var consequence = service.Apply(country, boundary);

        Assert.NotNull(consequence);
        Assert.Equal(48.0, country.ApprovalRating);
    }
}
