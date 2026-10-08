namespace Republic.Core.Tests.World;

using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalDashboardTests
{
    private static RepublicTime CreateTestTime()
    {
        var clock = RepublicClock.CreateControlled();
        return clock.CurrentTime;
    }

    [Fact]
    public void Country_WithNoYieldSnapshot_ShowsR0Yield()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        country.LatestYieldSnapshot = null;

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal("R0", dashboard.Yield);
        Assert.Equal(0.0, dashboard.YieldRevenue);
        Assert.Contains("Yield: R0", dashboard.Lines);
        Assert.Contains("Yield: R0", dashboard.ToString());
    }

    [Fact]
    public void Country_WithYieldSnapshot_ShowsFormattedYield()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        var yield = new NationalYield();
        yield.Economic.RepuTreasuryRevenue = 500_000.0;
        country.LatestYieldSnapshot = new NationalYieldSnapshot(country.Id, time, yield);

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal("R500K", dashboard.Yield);
        Assert.Equal(500_000.0, dashboard.YieldRevenue);
        Assert.Contains("Yield: R500K", dashboard.Lines);
    }

    [Fact]
    public void Infrastructure_MatchesCountryInfrastructureInput()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        country.StateCapacity.InfrastructureCondition = 0.75;

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal(0.75, dashboard.InfrastructureCondition);
        Assert.Equal("75%", dashboard.Infrastructure);
        Assert.Contains("Infrastructure: 75%", dashboard.Lines);
    }

    [Fact]
    public void Infrastructure_DefaultCondition_MatchesStateCapacityDefault()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        // Default is 0.6
        var dashboard = new NationalDashboard(country, time);

        Assert.Equal(0.6, dashboard.InfrastructureCondition);
        Assert.Equal("60%", dashboard.Infrastructure);
        Assert.Contains("Infrastructure: 60%", dashboard.Lines);
    }

    [Fact]
    public void DiplomacyScienceMilitary_AreNotEstablished()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal("Not established", dashboard.Diplomacy);
        Assert.Equal("Not established", dashboard.Science);
        Assert.Equal("Not established", dashboard.Military);
        Assert.Contains("Diplomacy: Not established", dashboard.Lines);
        Assert.Contains("Science: Not established", dashboard.Lines);
        Assert.Contains("Military: Not established", dashboard.Lines);
    }

    [Fact]
    public void BuildingDashboard_DoesNotChangeTreasuryBalance()
    {
        var time = CreateTestTime();
        var treasury = new RepuTreasury(12_345_678.0);
        var country = Country.Found("Republic of Test", time, treasury: treasury);

        var balanceBefore = country.Treasury.Balance;
        var dashboard = country.GetDashboard(time);

        Assert.Equal(balanceBefore, country.Treasury.Balance);
        Assert.Equal(balanceBefore, dashboard.TreasuryBalance);
        Assert.Equal(country.Treasury.FormattedBalance, dashboard.Treasury);
    }

    [Fact]
    public void CountryIsolation_BuildingDashboardForCountryA_DoesNotAffectCountryB()
    {
        var time = CreateTestTime();
        var countryA = Country.Found("Republic of Alpha", time, treasury: new RepuTreasury(5_000_000.0));
        var countryB = Country.Found("Republic of Beta", time, treasury: new RepuTreasury(10_000_000.0));
        countryB.StateCapacity.InfrastructureCondition = 0.85;

        var initialTreasuryB = countryB.Treasury.Balance;
        var initialInfraB = countryB.StateCapacity.InfrastructureCondition;
        var initialProjectsCountB = countryB.Projects.Count;

        var dashboardA = countryA.GetDashboard(time);

        Assert.Equal(initialTreasuryB, countryB.Treasury.Balance);
        Assert.Equal(initialInfraB, countryB.StateCapacity.InfrastructureCondition);
        Assert.Equal(initialProjectsCountB, countryB.Projects.Count);
        Assert.NotEqual(dashboardA.CountryId, countryB.Id);
    }

    [Fact]
    public void Dashboard_ApprovalAndStability_FormatAsPercentages()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, baselineStability: 85.0);
        country.HappinessRating = 78.5;

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal("78.5%", dashboard.Approval);
        Assert.Equal(78.5, dashboard.ApprovalRating);
        Assert.Equal("85%", dashboard.Stability);
        Assert.Equal(85.0, dashboard.BaselineStability);
        Assert.Contains("Approval: 78.5%", dashboard.Lines);
        Assert.Contains("Stability: 85%", dashboard.Lines);
    }

    [Fact]
    public void Dashboard_Economy_IncludesTreasuryAndGdp()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(15_000_000_000.0));
        country.GrossDomesticProduct = 450_000_000_000.0;

        var dashboard = new NationalDashboard(country, time);

        Assert.Equal("R15B", dashboard.Treasury);
        Assert.Equal("R450,000,000,000", dashboard.Gdp);
        Assert.Contains(dashboard.Treasury, dashboard.Economy);
        Assert.Contains(dashboard.Gdp, dashboard.Economy);
        Assert.Contains($"Economy: {dashboard.Economy}", dashboard.Lines);
    }
}
