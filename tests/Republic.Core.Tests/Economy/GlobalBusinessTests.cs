namespace Republic.Core.Tests.Economy;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Businesses.Models;
using Republic.Core.Economy.Businesses.Services;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Xunit;

public sealed class GlobalBusinessTests : IDisposable
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

    public GlobalBusinessTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "Republic_GlobalBizTests_" + Guid.NewGuid().ToString("N"));
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
            Name = "Global Business Test World"
        };

        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry(98765, startingTreasury: 100_000_000.0);
        country.GrossDomesticProduct = 10_000_000_000.0;
        country.PresidencyStartDay = 1;
        country.CentralBank.ForeignReserves = 50_000_000.0;
        country.StockMarket.CurrentPoints = 1000.0;
        world.Countries = new List<Country> { country };

        return (world, country);
    }

    [Fact]
    public void FirstWave_ApproachesSequentially_Every30DaysOfPresidency()
    {
        var (_, country) = CreateStandardTestCountry();
        var service = new GlobalBusinessService();

        // Day 1: No business has reached its 30-day scheduled day
        var simTimeDay1 = CreateTime(1, 10);
        service.UpdateBusinessApproaches(country, simTimeDay1);
        Assert.Empty(country.GlobalBusinesses);

        // Day 30: 1st business approaches (FMCG: Nestor-Valo)
        var simTimeDay30 = CreateTime(30, 9);
        service.UpdateBusinessApproaches(country, simTimeDay30);
        Assert.Single(country.GlobalBusinesses);
        var biz1 = country.GlobalBusinesses[0];
        Assert.Equal("biz_wave1_fmcg", biz1.Id);
        Assert.Equal("FMCG", biz1.Industry);
        Assert.Equal(GlobalBusinessWave.FirstWave, biz1.Wave);
        Assert.Equal(GlobalBusinessDecisionStatus.Pending, biz1.Status);
        Assert.Contains("Nestor-Valo", biz1.BusinessName);

        // Day 60: 2nd business approaches (Tech: OmniNet Cloud Systems)
        var simTimeDay60 = CreateTime(60, 9);
        service.UpdateBusinessApproaches(country, simTimeDay60);
        Assert.Equal(2, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave1_tech", country.GlobalBusinesses[1].Id);
        Assert.Equal("Technology", country.GlobalBusinesses[1].Industry);

        // Day 90: 3rd business approaches (Automobile: Veloce Motors)
        var simTimeDay90 = CreateTime(90, 9);
        service.UpdateBusinessApproaches(country, simTimeDay90);
        Assert.Equal(3, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave1_auto", country.GlobalBusinesses[2].Id);
        Assert.Equal("Automobile", country.GlobalBusinesses[2].Industry);

        // Day 120: 4th business approaches (Energy: Solis-Aura)
        var simTimeDay120 = CreateTime(120, 9);
        service.UpdateBusinessApproaches(country, simTimeDay120);
        Assert.Equal(4, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave1_energy", country.GlobalBusinesses[3].Id);
        Assert.Equal("Energy", country.GlobalBusinesses[3].Industry);

        // Day 150: 5th business approaches (Pharma: BioGenix)
        var simTimeDay150 = CreateTime(150, 9);
        service.UpdateBusinessApproaches(country, simTimeDay150);
        Assert.Equal(5, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave1_pharma", country.GlobalBusinesses[4].Id);
        Assert.Equal("Pharmaceuticals", country.GlobalBusinesses[4].Industry);

        // Ensure all are Wave 1 and distinct
        Assert.All(country.GlobalBusinesses, b => Assert.Equal(GlobalBusinessWave.FirstWave, b.Wave));
        Assert.Equal(5, country.GlobalBusinesses.Select(b => b.Industry).Distinct().Count());
    }

    [Fact]
    public void SecondWave_RequiresNationalCurrency_AndApproachesSequentially_Every30Days()
    {
        var (_, country) = CreateStandardTestCountry();
        var service = new GlobalBusinessService();

        // At Day 160 without currency, Wave 2 businesses MUST NOT approach
        var simTimeDay160 = CreateTime(160, 10);
        service.UpdateBusinessApproaches(country, simTimeDay160);
        Assert.Equal(5, country.GlobalBusinesses.Count); // Only the 5 Wave 1 businesses

        // Player establishes national currency at Day 160
        country.CurrencyCreationDay = 160;
        country.CentralBank.ForexUnlocked = true;

        // Day 180 (20 days post-currency): No new Wave 2 business yet
        var simTimeDay180 = CreateTime(180, 10);
        service.UpdateBusinessApproaches(country, simTimeDay180);
        Assert.Equal(5, country.GlobalBusinesses.Count);

        // Day 190 (30 days post-currency): 1st Wave 2 business arrives (Retail: Apex Global)
        var simTimeDay190 = CreateTime(190, 10);
        service.UpdateBusinessApproaches(country, simTimeDay190);
        Assert.Equal(6, country.GlobalBusinesses.Count);
        var biz6 = country.GlobalBusinesses[5];
        Assert.Equal("biz_wave2_logistics", biz6.Id);
        Assert.Equal(GlobalBusinessWave.SecondWave, biz6.Wave);
        Assert.Equal("Retail & Logistics", biz6.Industry);

        // Day 220 (60 days post-currency): 2nd Wave 2 arrives (Semiconductors)
        var simTimeDay220 = CreateTime(220, 10);
        service.UpdateBusinessApproaches(country, simTimeDay220);
        Assert.Equal(7, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave2_semiconductors", country.GlobalBusinesses[6].Id);
        Assert.Equal("Semiconductors", country.GlobalBusinesses[6].Industry);

        // Day 250 (90 days post-currency): 3rd Wave 2 arrives (Finance)
        var simTimeDay250 = CreateTime(250, 10);
        service.UpdateBusinessApproaches(country, simTimeDay250);
        Assert.Equal(8, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave2_finance", country.GlobalBusinesses[7].Id);
        Assert.Equal("Finance", country.GlobalBusinesses[7].Industry);

        // Day 280 (120 days post-currency): 4th Wave 2 arrives (Aerospace)
        var simTimeDay280 = CreateTime(280, 10);
        service.UpdateBusinessApproaches(country, simTimeDay280);
        Assert.Equal(9, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave2_aerospace", country.GlobalBusinesses[8].Id);
        Assert.Equal("Aerospace & Defense", country.GlobalBusinesses[8].Industry);

        // Day 310 (150 days post-currency): 5th Wave 2 arrives (Heavy Industry)
        var simTimeDay310 = CreateTime(310, 10);
        service.UpdateBusinessApproaches(country, simTimeDay310);
        Assert.Equal(10, country.GlobalBusinesses.Count);
        Assert.Equal("biz_wave2_metallurgy", country.GlobalBusinesses[9].Id);
        Assert.Equal("Heavy Industry", country.GlobalBusinesses[9].Industry);

        // Verify grand total has 5 Wave 1 and 5 Wave 2
        Assert.Equal(5, country.GlobalBusinesses.Count(b => b.Wave == GlobalBusinessWave.FirstWave));
        Assert.Equal(5, country.GlobalBusinesses.Count(b => b.Wave == GlobalBusinessWave.SecondWave));
    }

    [Fact]
    public void AcceptProposal_GrantsCapital_Reserves_Jobs_QuarterlyTax_AndBoostsStockMarket()
    {
        var (_, country) = CreateStandardTestCountry();
        var service = new GlobalBusinessService();
        var simTime = CreateTime(35, 12);
        country.UpdateEconomicCycle(simTime);
        service.UpdateBusinessApproaches(country, simTime);

        var proposal = country.GlobalBusinesses.First(b => b.Id == "biz_wave1_fmcg");
        double initialTreasury = country.Treasury.Balance;
        double initialReserves = country.CentralBank.ForeignReserves;
        double initialStockIndex = country.StockMarket.CurrentPoints;
        double initialGdp = country.GrossDomesticProduct;

        bool accepted = service.AcceptProposal(country, proposal.Id, simTime, out var message);

        Assert.True(accepted);
        Assert.Equal(GlobalBusinessDecisionStatus.Accepted, proposal.Status);
        Assert.Equal(35, proposal.DecidedOnDay);
        Assert.Contains("Nestor-Valo Consumer Goods", message);

        // Direct economic benefits applied
        Assert.Equal(initialTreasury + proposal.CapitalInvestment, country.Treasury.Balance);
        Assert.Equal(initialReserves + proposal.ForeignReservesInflow, country.CentralBank.ForeignReserves);
        Assert.True(country.StockMarket.CurrentPoints > initialStockIndex);
        Assert.Equal(initialGdp + proposal.GdpContribution, country.GrossDomesticProduct);

        // Verify quarterly tax flows into Country economic cycle
        country.UpdateEconomicCycle(simTime);
        Assert.True(country.MinistryBudget.TotalQuarterlyExpenditures > 0);
    }

    [Fact]
    public void DeclineProposal_MarksDeclined_SlightMarketDip()
    {
        var (_, country) = CreateStandardTestCountry();
        var service = new GlobalBusinessService();
        var simTime = CreateTime(35, 12);
        country.UpdateEconomicCycle(simTime);
        service.UpdateBusinessApproaches(country, simTime);

        var proposal = country.GlobalBusinesses.First(b => b.Id == "biz_wave1_fmcg");
        double initialTreasury = country.Treasury.Balance;
        double initialStockIndex = country.StockMarket.CurrentPoints;

        bool declined = service.DeclineProposal(country, proposal.Id, simTime, out var message);

        Assert.True(declined);
        Assert.Equal(GlobalBusinessDecisionStatus.Declined, proposal.Status);
        Assert.Equal(35, proposal.DecidedOnDay);
        Assert.Contains("declined", message.ToLowerInvariant());

        // Treasury unaffected, stock market experiences slight confidence dip
        Assert.Equal(initialTreasury, country.Treasury.Balance);
        Assert.True(country.StockMarket.CurrentPoints < initialStockIndex);
    }

    [Fact]
    public void PresentingMinister_DynamicallyReflectsAppointedMinisters()
    {
        var (_, country) = CreateStandardTestCountry();
        
        // Appoint custom ministers
        country.Cabinet[CabinetPortfolio.Interior] = new Minister
        {
            Portfolio = CabinetPortfolio.Interior,
            Name = "Hon. Sarah Connor",
            Competence = 88,
            Loyalty = 92
        };

        var service = new GlobalBusinessService();
        var proposals = service.GetPredefinedProposals(country);
        var fmcg = proposals.First(p => p.Id == "biz_wave1_fmcg");

        Assert.Contains("Hon. Sarah Connor", fmcg.PresentingMinister);
    }

    [Fact]
    public void PresidentialDeskBridge_ProcessesPendingBusinessDecision()
    {
        var (world, country) = CreateStandardTestCountry();
        var simTime = CreateTime(32, 14);

        // Save initial state so Bridge can load it
        PresidentialDeskBridge.SaveAuthoritativeState(world, simTime, 500, _saveFilePath);
        var snapshot = PresidentialDeskSnapshot.Create(world, country, simTime, 500, _saveFilePath);
        File.WriteAllText(_stateFilePath, PresidentialDeskBridge.Serialize(snapshot));

        // Write an inbound decision request
        var requestPayload = new
        {
            request_id = "req_biz_001",
            action = "global_business_decision",
            business_id = "biz_wave1_fmcg",
            decision = "accept",
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(requestPayload));

        // Process request through bridge
        var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestFilePath: _requestFilePath);

        Assert.NotNull(result);
        Assert.True(result.Success, $"Failed with: Result='{result.Result}', Message='{result.Message}'");
        Assert.Equal("Success", result.Result);
        Assert.Contains("Nestor-Valo Consumer Goods", result.Message);
        Assert.False(File.Exists(_requestFilePath), "Request file should be consumed/deleted.");

        // Reload state snapshot and verify data contract
        var reloadedJson = File.ReadAllText(_stateFilePath);
        var updatedSnapshot = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(reloadedJson);
        Assert.NotNull(updatedSnapshot);
        Assert.Equal(1, updatedSnapshot.OperatingBusinessesCount);
        Assert.True(updatedSnapshot.TotalBusinessQuarterlyTax > 0);
        Assert.True(updatedSnapshot.TotalBusinessJobsCreated > 0);
        Assert.True(updatedSnapshot.TotalBusinessGdpBoost > 0);
        Assert.NotEmpty(updatedSnapshot.GlobalBusinesses);
        var acceptedBiz = updatedSnapshot.GlobalBusinesses.First(b => b.Id == "biz_wave1_fmcg");
        Assert.Equal("Accepted", acceptedBiz.Status);
    }
}
