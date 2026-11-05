namespace Republic.Core.Tests.Economy;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Economy.Businesses.Models;
using Republic.Core.Economy.CentralBank.Models;
using Republic.Core.Economy.Confidence.Models;
using Republic.Core.Economy.Debt.Models;
using Republic.Core.Economy.StockMarket.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Xunit;

public sealed class EconomicCyclePhase2Tests : IDisposable
{
    private static readonly DateTimeOffset LaunchEpoch = new(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));

    private static RepublicTime CreateTime(int days, int hours = 0, int minutes = 0)
    {
        var timestamp = LaunchEpoch.AddDays(days).AddHours(hours).AddMinutes(minutes);
        return new RepublicTime(timestamp, LaunchEpoch);
    }

    private readonly string _tempDirectory;
    private readonly string _stateFilePath;
    private readonly string _saveFilePath;
    private readonly string _requestFilePath;

    public EconomicCyclePhase2Tests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "Republic_EconPhase2Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _stateFilePath = Path.Combine(_tempDirectory, "presidential_desk_state.json");
        _saveFilePath = Path.Combine(_tempDirectory, "presidential_desk_state.sav");
        _requestFilePath = Path.Combine(_tempDirectory, "presidential_desk_request.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch { }
    }

    private (WorldState World, Country Country) CreateStandardTestCountry()
    {
        var world = new WorldState
        {
            WorldId = Guid.NewGuid(),
            Name = "Phase 2 Economic World"
        };

        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry(77889, startingTreasury: 120_000_000.0);
        country.GrossDomesticProduct = 10_000_000_000.0;
        country.CentralBank.ForeignReserves = 55_000_000.0;
        country.CentralBank.ExchangeRateToUSD = 1.25;
        country.Debt.DomesticDebt = 150_000_000.0;
        country.Debt.ExternalDebt = 100_000_000.0;
        country.TaxPolicy.ImportTariffRate = 0.05;
        world.Countries = new List<Country> { country };

        return (world, country);
    }

    [Fact]
    public void ExpandedStockMarket_InitializesSectors_AndReactsToMacroSignals()
    {
        var (_, country) = CreateStandardTestCountry();
        var simTime = CreateTime(10, 12);

        country.UpdateEconomicCycle(simTime);

        // 1. Five diverse industry sectors are present
        Assert.NotNull(country.StockMarket.Sectors);
        Assert.Equal(5, country.StockMarket.Sectors.Count);
        Assert.Contains(country.StockMarket.Sectors, s => s.SectorName == "Technology & Telecommunications");
        Assert.Contains(country.StockMarket.Sectors, s => s.SectorName == "Industrial & Manufacturing");
        Assert.Contains(country.StockMarket.Sectors, s => s.SectorName == "Banking & Financial Services");
        Assert.Contains(country.StockMarket.Sectors, s => s.SectorName == "Healthcare & Consumer Staples");
        Assert.Contains(country.StockMarket.Sectors, s => s.SectorName == "Energy & Infrastructure");

        // Verify representative listed firms exist
        var techSector = country.StockMarket.Sectors.First(s => s.SectorName == "Technology & Telecommunications");
        Assert.NotEmpty(techSector.RepresentativeCompanies);
        Assert.Contains(techSector.RepresentativeCompanies, c => c.Ticker == "ONET");

        double initialTechPoints = techSector.CurrentPoints;

        // 2. High interest rate shock depresses tech valuations
        country.CentralBank.BenchmarkInterestRate = 9.0;
        country.UpdateEconomicCycle(simTime);
        techSector = country.StockMarket.Sectors.First(s => s.SectorName == "Technology & Telecommunications");
        Assert.True(techSector.CurrentPoints < initialTechPoints);

        // 3. Accepted global tech business provides sector valuation boost
        country.GlobalBusinesses.Add(new GlobalBusinessProposal
        {
            Id = "biz_wave1_tech",
            BusinessName = "OmniNet Cloud Systems",
            Industry = "Technology",
            Status = GlobalBusinessDecisionStatus.Accepted,
            QuarterlyExportContribution = 15_000_000.0,
            QuarterlyTaxContribution = 5_000_000.0
        });

        country.UpdateEconomicCycle(simTime);
        var boostedTech = country.StockMarket.Sectors.First(s => s.SectorName == "Technology & Telecommunications");
        Assert.Contains("Global tech & semiconductor foundry commitments", boostedTech.KeyDrivers);
    }

    [Fact]
    public void BusinessConfidenceIndex_ReactsToMacroSignals_AndGlobalEnterprises()
    {
        var (_, country) = CreateStandardTestCountry();
        var simTime = CreateTime(15, 10);

        // 1. Initial healthy baseline: ~60-75 (Moderate Growth / Expansion)
        country.UpdateEconomicCycle(simTime);
        Assert.True(country.BusinessConfidence.Score >= 55.0);
        Assert.Contains("Growth", country.BusinessConfidence.Status);

        double baselineBci = country.BusinessConfidence.Score;

        // 2. Toxic shock: Rampant corruption and high inflation
        country.CorruptionIndex = 75.0;
        country.InflationRate = 18.0;
        country.UpdateEconomicCycle(simTime);

        Assert.True(country.BusinessConfidence.Score < baselineBci);
        Assert.True(country.BusinessConfidence.Score < 50.0);
        Assert.True(country.BusinessConfidence.Status.Contains("Contraction") || country.BusinessConfidence.Status.Contains("Stagnation"));

        // 3. Rebound with institutional cleanup and multiple authorized global businesses
        country.CorruptionIndex = 12.0;
        country.InflationRate = 2.8;
        country.BaselineStability = 88.0;
        country.GlobalBusinesses.Add(new GlobalBusinessProposal { Id = "b1", Status = GlobalBusinessDecisionStatus.Accepted, Industry = "FMCG" });
        country.GlobalBusinesses.Add(new GlobalBusinessProposal { Id = "b2", Status = GlobalBusinessDecisionStatus.Accepted, Industry = "Technology" });
        country.GlobalBusinesses.Add(new GlobalBusinessProposal { Id = "b3", Status = GlobalBusinessDecisionStatus.Accepted, Industry = "Energy" });

        country.UpdateEconomicCycle(simTime);
        Assert.True(country.BusinessConfidence.Score >= 75.0);
        Assert.Equal("Strong Expansion 🟢", country.BusinessConfidence.Status);
    }

    [Fact]
    public void TradeBalance_SurplusInflow_AndDeficitDrainOnReserves()
    {
        var (_, country) = CreateStandardTestCountry();
        var simTime = CreateTime(20, 14);

        // 1. Initial Trade balance calculation
        country.UpdateEconomicCycle(simTime);
        Assert.True(country.QuarterlyExports > 0.0);
        Assert.True(country.QuarterlyImports > 0.0);

        double initialReserves = country.CentralBank.ForeignReserves;

        // 2. High export volume generates trade surplus that inflows to Central Bank reserves
        country.GlobalBusinesses.Add(new GlobalBusinessProposal
        {
            Id = "mega_export_biz",
            Status = GlobalBusinessDecisionStatus.Accepted,
            QuarterlyExportContribution = 250_000_000.0
        });

        country.UpdateEconomicCycle(simTime);
        country.SettleQuarterlyTradeBalance();
        Assert.True(country.CentralBank.ForeignReserves > initialReserves);
    }

    [Fact]
    public void TradeTariff_AdjustsImports_TariffRevenue_AndInflation()
    {
        var (_, country) = CreateStandardTestCountry();
        var simTime = CreateTime(25, 12);

        country.UpdateEconomicCycle(simTime);
        double initialImports = country.QuarterlyImports;
        double initialInflation = country.InflationRate;

        // Apply a protective tariff hike from 5% to 25%
        country.ApplyTradeTariff(0.25, simTime);

        // High tariffs compress imports and increase domestic inflation pressure
        Assert.Equal(0.25, country.TaxPolicy.ImportTariffRate);
        Assert.True(country.QuarterlyImports < initialImports);
        Assert.True(country.InflationRate > initialInflation);
    }

    [Fact]
    public void DebtSustainability_EvaluatesRiskTiers_AndTriggersApprovalDragWhenCritical()
    {
        var (_, country) = CreateStandardTestCountry();
        var simTime = CreateTime(30, 9);

        // 1. Low baseline debt (Total 250M on 10B GDP = 2.5% Debt-to-GDP)
        country.UpdateEconomicCycle(simTime);
        Assert.Equal("Sustainable (Low Risk) 🟢", country.Debt.SustainabilityStatus);

        double initialApproval = country.HappinessRating;

        // 2. Massive debt surge (e.g. 8 Billion REPU debt)
        country.Debt.DomesticDebt = 5_000_000_000.0;
        country.Debt.ExternalDebt = 3_000_000_000.0;
        country.UpdateEconomicCycle(simTime);

        Assert.Equal("Critical Debt Distress (High Risk) 🔴", country.Debt.SustainabilityStatus);
        Assert.True(country.Debt.DebtToGdpRatio > 70.0);
        Assert.True(country.HappinessRating < initialApproval);
    }

    [Fact]
    public void ForexInterventions_DefendCurrency_AndBoostExports()
    {
        var (_, country) = CreateStandardTestCountry();
        country.CentralBank.ForeignReserves = 180_000_000.0; // Meets 150M threshold
        country.CentralBank.CheckForexUnlock();
        Assert.True(country.CentralBank.ForexUnlocked);

        double initialRate = country.CentralBank.ExchangeRateToUSD; // 1.25

        // 1. Defend currency (sell foreign reserves to strengthen REPU)
        bool defended = country.CentralBank.InterveneDefendCurrency(20_000_000.0);
        Assert.True(defended);
        Assert.Equal(160_000_000.0, country.CentralBank.ForeignReserves);
        Assert.True(country.CentralBank.ExchangeRateToUSD < initialRate); // Appreciated (e.g. 1.24)

        // 2. Boost exports (accumulate foreign reserves, slightly devaluing currency)
        double preBoostRate = country.CentralBank.ExchangeRateToUSD;
        bool boosted = country.CentralBank.InterveneBoostExports(30_000_000.0);
        Assert.True(boosted);
        Assert.Equal(190_000_000.0, country.CentralBank.ForeignReserves);
        Assert.True(country.CentralBank.ExchangeRateToUSD > preBoostRate);
        Assert.True(country.CentralBank.ExportCompetitivenessMultiplier > 1.0);
    }

    [Fact]
    public void PresidentialDeskBridge_ProcessesTariffAndForexDecisions()
    {
        var (world, country) = CreateStandardTestCountry();
        country.CentralBank.ForeignReserves = 160_000_000.0;
        country.CentralBank.CheckForexUnlock();
        var simTime = CreateTime(35, 11);

        PresidentialDeskBridge.SaveAuthoritativeState(world, simTime, 600, _saveFilePath);
        var snapshot = PresidentialDeskSnapshot.Create(world, country, simTime, 600, _saveFilePath);
        File.WriteAllText(_stateFilePath, PresidentialDeskBridge.Serialize(snapshot));

        // 1. Test Trade Tariff Decision
        var tariffRequest = new
        {
            request_id = "req_tariff_01",
            action = "trade_tariff",
            tariff_percent = 18.0,
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(tariffRequest));

        var tariffResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestFilePath: _requestFilePath);

        Assert.NotNull(tariffResult);
        Assert.True(tariffResult.Success);
        Assert.Contains("import tariff adjustment", tariffResult.Message);

        var updatedSnapshot = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(File.ReadAllText(_stateFilePath));
        Assert.NotNull(updatedSnapshot);
        Assert.Equal("18.0%", updatedSnapshot.ImportTariffRateFormatted);

        // 2. Test Forex Intervention Decision
        var forexRequest = new
        {
            request_id = "req_forex_int_01",
            action = "forex_intervention",
            intervention_type = "defend",
            amount = 15_000_000.0,
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(forexRequest));

        var forexResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestFilePath: _requestFilePath);

        Assert.NotNull(forexResult);
        Assert.True(forexResult.Success);
        Assert.Contains("Central Bank intervened in foreign exchange market", forexResult.Message);

        var finalSnapshot = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(File.ReadAllText(_stateFilePath));
        Assert.NotNull(finalSnapshot);
        Assert.True(finalSnapshot.ForexInterventionsCount >= 1);
        Assert.NotEmpty(finalSnapshot.StockMarketSectors);
        Assert.True(finalSnapshot.BusinessConfidenceScore > 0.0);
    }
}
