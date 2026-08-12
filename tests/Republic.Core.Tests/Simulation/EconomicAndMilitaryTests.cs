namespace Republic.Core.Tests.Simulation;

using System.Collections.Generic;
using Xunit;
using Republic.Core.Economy.Services;
using Republic.Core.Military.Models;

public class EconomicAndMilitaryTests
{
    [Fact]
    public void CalculateNextEconomicCycle_Computes_Tariff_Revenue_Correctly()
    {
        var service = new EconomicSimulationService();
        double tradeVolume = 10_000_000.0;
        double tariffRate = 5.0; // 5% tariff

        var summary = service.CalculateNextEconomicCycle(5_000_000, 20.0, tariffRate, tradeVolume, 1_000_000);

        Assert.Equal(500_000.0, summary.TariffRevenue);
    }

    [Fact]
    public void High_Infrastructure_Investment_Boosts_GDP_Growth()
    {
        var service = new EconomicSimulationService();

        var baseline = service.CalculateNextEconomicCycle(5_000_000, 20.0, 5.0, 10_000_000, 0);
        var highInvestment = service.CalculateNextEconomicCycle(5_000_000, 20.0, 5.0, 10_000_000, 10_000_000);

        Assert.True(highInvestment.GdpGrowthRate > baseline.GdpGrowthRate);
    }

    [Fact]
    public void CalculateCompositeReadiness_Factors_Logistics_And_Training()
    {
        var report = new MilitaryReadinessReport
        {
            LogisticsSupplyEfficiency = 90.0,
            UnitTrainingIndex = 80.0,
            BranchBreakdown = new List<MilitaryBranchState>
            {
                new MilitaryBranchState { Branch = MilitaryBranch.Army, ReadinessScore = 70.0 },
                new MilitaryBranchState { Branch = MilitaryBranch.AirForce, ReadinessScore = 90.0 }
            }
        };

        // Branch avg = (70 + 90) / 2 = 80.0
        // Composite = (80 * 0.5) + (90 * 0.3) + (80 * 0.2) = 40 + 27 + 16 = 83.0
        double readiness = report.CalculateCompositeReadiness();

        Assert.Equal(83.0, readiness);
    }
}
