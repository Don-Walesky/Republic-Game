namespace Republic.Core.Tests.Economy;

using System;
using System.IO;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.CentralBank.Models;
using Republic.Core.Economy.Debt.Models;
using Republic.Core.Economy.StockMarket.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Xunit;

public sealed class EconomicCycleTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _stateFilePath;
    private readonly string _saveFilePath;
    private readonly string _requestFilePath;

    public EconomicCycleTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "Republic_EconTests_" + Guid.NewGuid().ToString("N"));
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
            Name = "Economic Test World"
        };

        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry(12345, startingTreasury: 100_000_000.0);
        country.GrossDomesticProduct = 10_000_000_000.0;
        country.CentralBank.ForeignReserves = 45_000_000.0;
        country.Debt.DomesticDebt = 50_000_000.0;
        country.Debt.ExternalDebt = 30_000_000.0;

        world.Countries.Add(country);
        return (world, country);
    }

    private void SaveTestState(WorldState world, Country country)
    {
        var clock = RepublicClock.CreateControlled();
        PresidentialDeskBridge.SaveAuthoritativeState(world, clock.CurrentTime, 1, _saveFilePath);
        var snapshot = PresidentialDeskSnapshot.Create(world, country, clock.CurrentTime, 1, _saveFilePath);
        File.WriteAllText(_stateFilePath, PresidentialDeskBridge.Serialize(snapshot));
    }

    [Fact]
    public void TaxHike_CoolsInflation_IncreasesRevenue_ImpactsCitizenFeedback()
    {
        var (world, country) = CreateStandardTestCountry();
        var clock = RepublicClock.CreateControlled();

        country.UpdateEconomicCycle(clock.CurrentTime);
        double initialInflation = country.InflationRate;

        // Apply a tax hike from 25% to 32%
        var newPolicy = new TaxPolicy
        {
            IncomeTaxRate = 0.32,
            CorporateTaxRate = country.TaxPolicy.CorporateTaxRate,
            ImportTariffRate = country.TaxPolicy.ImportTariffRate
        };
        var feedback = country.ApplyTaxPolicy(newPolicy, clock.CurrentTime);

        Assert.Equal(0.32, country.TaxPolicy.IncomeTaxRate);
        Assert.True(country.InflationRate < initialInflation, $"Expected inflation to decrease from {initialInflation}, got {country.InflationRate}");
        Assert.Contains("Tax hike enacted", feedback.Description);
        Assert.Contains("Inflation pressure reduced", feedback.Description);
    }

    [Fact]
    public void TaxCut_StimulatesInflationPressure()
    {
        var (world, country) = CreateStandardTestCountry();
        var clock = RepublicClock.CreateControlled();

        country.UpdateEconomicCycle(clock.CurrentTime);
        double initialInflation = country.InflationRate;

        // Apply a tax cut from 25% to 18%
        var newPolicy = new TaxPolicy
        {
            IncomeTaxRate = 0.18,
            CorporateTaxRate = country.TaxPolicy.CorporateTaxRate,
            ImportTariffRate = country.TaxPolicy.ImportTariffRate
        };
        var feedback = country.ApplyTaxPolicy(newPolicy, clock.CurrentTime);

        Assert.Equal(0.18, country.TaxPolicy.IncomeTaxRate);
        Assert.True(country.InflationRate > initialInflation, $"Expected inflation pressure to rise above 3.5, got {country.InflationRate}");
        Assert.Contains("Tax relief enacted", feedback.Description);
        Assert.Contains("Inflation pressure adjusted", feedback.Description);
    }

    [Fact]
    public void InflationCostMultiplier_ScalesProjectCosts_AboveThreePercent()
    {
        var (world, country) = CreateStandardTestCountry();

        country.InflationRate = 3.0;
        Assert.Equal(1.0, country.GetInflationCostMultiplier());

        country.InflationRate = 2.5;
        Assert.Equal(1.0, country.GetInflationCostMultiplier());

        country.InflationRate = 8.0;
        // 1.0 + (8.0 - 3.0)/100.0 = 1.05
        Assert.Equal(1.05, Math.Round(country.GetInflationCostMultiplier(), 2));

        country.InflationRate = 15.0;
        // 1.0 + (15.0 - 3.0)/100.0 = 1.12
        Assert.Equal(1.12, Math.Round(country.GetInflationCostMultiplier(), 2));
    }

    [Fact]
    public void CentralBank_InterestRateHike_QuellsInflation_RateCut_Stimulates()
    {
        var (world, country) = CreateStandardTestCountry();
        var clock = RepublicClock.CreateControlled();

        double baseRate = country.CentralBank.BenchmarkInterestRate; // 4.5%
        country.UpdateEconomicCycle(clock.CurrentTime);
        double baselineInflation = country.InflationRate;

        // Hike rate by 2.0%
        country.CentralBank.AdjustInterestRate(2.0);
        Assert.Equal(baseRate + 2.0, country.CentralBank.BenchmarkInterestRate);

        country.UpdateEconomicCycle(clock.CurrentTime);
        Assert.True(country.InflationRate < baselineInflation, "Rate hike should lower inflation pressure.");

        // Cut rate by 4.0%
        country.CentralBank.AdjustInterestRate(-4.0);
        country.UpdateEconomicCycle(clock.CurrentTime);
        Assert.True(country.InflationRate > baselineInflation, "Rate cut should increase inflation pressure.");
    }

    [Fact]
    public void CentralBank_ReserveInjectionAndSterilization_TransfersTreasuryLiquidity()
    {
        var (world, country) = CreateStandardTestCountry();
        country.CentralBank.ForeignReserves = 50_000_000.0;
        country.Treasury.Withdraw(country.Treasury.Balance); country.Treasury.Deposit(100_000_000.0);

        // Reserve injection: 20M from reserves to treasury
        bool injected = country.CentralBank.InjectReservesToTreasury(country.Treasury, 20_000_000.0);
        Assert.True(injected);
        Assert.Equal(30_000_000.0, country.CentralBank.ForeignReserves);
        Assert.Equal(120_000_000.0, country.Treasury.Balance);

        // Reserve sterilization: 30M from treasury to reserves
        bool sterilized = country.CentralBank.SterilizeTreasuryToReserves(country.Treasury, 30_000_000.0);
        Assert.True(sterilized);
        Assert.Equal(60_000_000.0, country.CentralBank.ForeignReserves);
        Assert.Equal(90_000_000.0, country.Treasury.Balance);

        // Reject if insufficient funds
        bool failedInject = country.CentralBank.InjectReservesToTreasury(country.Treasury, 100_000_000.0);
        Assert.False(failedInject);
    }

    [Fact]
    public void Loans_DomesticAndExternal_BorrowingAndRepayment()
    {
        var (world, country) = CreateStandardTestCountry();
        country.Treasury.Withdraw(country.Treasury.Balance); country.Treasury.Deposit(50_000_000.0);
        country.CentralBank.ForeignReserves = 40_000_000.0;
        country.Debt.DomesticDebt = 50_000_000.0;
        country.Debt.ExternalDebt = 30_000_000.0;

        // 1. Take domestic loan of 20M -> Treasury +20M, DomesticDebt +20M
        bool domesticTaken = country.Debt.TakeLoan("domestic", 20_000_000.0, country.Treasury, country.CentralBank);
        Assert.True(domesticTaken);
        Assert.Equal(70_000_000.0, country.Treasury.Balance);
        Assert.Equal(70_000_000.0, country.Debt.DomesticDebt);

        // 2. Take external loan of 50M -> ForeignReserves +50M, Treasury +25M (monetized), ExternalDebt +50M
        bool externalTaken = country.Debt.TakeLoan("external", 50_000_000.0, country.Treasury, country.CentralBank);
        Assert.True(externalTaken);
        Assert.Equal(90_000_000.0, country.CentralBank.ForeignReserves);
        Assert.Equal(95_000_000.0, country.Treasury.Balance);
        Assert.Equal(80_000_000.0, country.Debt.ExternalDebt);
        Assert.Equal(150_000_000.0, country.Debt.TotalDebt);

        // 3. Repay domestic loan 10M -> Treasury -10M, DomesticDebt -10M
        bool domesticRepaid = country.Debt.RepayLoan("domestic", 10_000_000.0, country.Treasury, country.CentralBank);
        Assert.True(domesticRepaid);
        Assert.Equal(85_000_000.0, country.Treasury.Balance);
        Assert.Equal(60_000_000.0, country.Debt.DomesticDebt);

        // 4. Repay external loan 20M -> ForeignReserves -20M, ExternalDebt -20M
        bool externalRepaid = country.Debt.RepayLoan("external", 20_000_000.0, country.Treasury, country.CentralBank);
        Assert.True(externalRepaid);
        Assert.Equal(70_000_000.0, country.CentralBank.ForeignReserves);
        Assert.Equal(60_000_000.0, country.Debt.ExternalDebt);
    }

    [Fact]
    public void ForexUnlock_TriggeredAt150MillionREPU_EnablesCurrencyManagement()
    {
        var (world, country) = CreateStandardTestCountry();
        var clock = RepublicClock.CreateControlled();

        country.CentralBank.ForeignReserves = 140_000_000.0;
        country.CentralBank.ForexUnlocked = false;

        country.UpdateEconomicCycle(clock.CurrentTime);
        Assert.False(country.CentralBank.ForexUnlocked);

        // Deposit 15M to cross 150M threshold
        bool newlyUnlocked = country.CentralBank.DepositForeignReserves(15_000_000.0);
        Assert.True(newlyUnlocked);
        Assert.True(country.CentralBank.ForexUnlocked);
        Assert.Equal(155_000_000.0, country.CentralBank.ForeignReserves);

        country.UpdateEconomicCycle(clock.CurrentTime);
        Assert.True(country.CentralBank.ForexUnlocked);
    }

    [Fact]
    public void StockMarketIndex_ReactsToStability_Inflation_Corruption()
    {
        var index = new StockMarketIndex();

        // 1. Favorable environment: High stability (85), low inflation (3.0), low corruption (15), low rate (3.5)
        index.UpdateIndex(baselineStability: 85.0, inflationRate: 3.0, corruptionIndex: 15.0, corporateTaxRate: 0.20, benchmarkInterestRate: 3.5);
        Assert.True(index.CurrentPoints > 2500.0);
        Assert.Equal("Bullish ↗️", index.Direction);

        double bullishPoints = index.CurrentPoints;

        // 2. Toxic shock: Low stability (25), high inflation (18.0), rampant corruption (80), punitive rate (12.0)
        index.UpdateIndex(baselineStability: 25.0, inflationRate: 18.0, corruptionIndex: 80.0, corporateTaxRate: 0.35, benchmarkInterestRate: 12.0);
        Assert.True(index.CurrentPoints < bullishPoints);
        Assert.Equal("Bearish ↘️", index.Direction);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyAdjustBudgetDecision_PersistsAndReturnsResult()
    {
        var (world, country) = CreateStandardTestCountry();
        SaveTestState(world, country);

        var result = PresidentialDeskBridge.ApplyAdjustBudgetDecision(
            defenseAllocation: 600_000_000.0,
            healthcareAllocation: 500_000_000.0,
            educationAllocation: 450_000_000.0,
            infrastructureAllocation: 700_000_000.0,
            scienceAllocation: 350_000_000.0,
            confirmed: true,
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestId: "req-budget-1");

        Assert.True(result.Success);
        Assert.Contains("National ministry budget rebalanced", result.Message);

        var updatedSnapshot = PresidentialDeskBridge.Deserialize(File.ReadAllText(_stateFilePath));
        Assert.NotNull(updatedSnapshot);
        Assert.Equal(600_000_000.0, updatedSnapshot.BudgetDefenseAllocation);
        Assert.Equal(500_000_000.0, updatedSnapshot.BudgetHealthcareAllocation);
        Assert.Equal(450_000_000.0, updatedSnapshot.BudgetEducationAllocation);
        Assert.Equal(700_000_000.0, updatedSnapshot.BudgetInfrastructureAllocation);
        Assert.Equal(350_000_000.0, updatedSnapshot.BudgetScienceAllocation);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyLoanDecision_ExternalLoan_UnlocksForexAtThreshold()
    {
        var (world, country) = CreateStandardTestCountry();
        country.CentralBank.ForeignReserves = 120_000_000.0;
        SaveTestState(world, country);

        // Take external loan of 50M -> foreign reserves reach 170M, unlocking Forex!
        var result = PresidentialDeskBridge.ApplyLoanDecision(
            loanType: "external",
            amount: 50_000_000.0,
            isRepayment: false,
            confirmed: true,
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestId: "req-loan-ext");

        Assert.True(result.Success);
        Assert.Contains("Sovereign External loan contracted", result.Message);

        var updatedSnapshot = PresidentialDeskBridge.Deserialize(File.ReadAllText(_stateFilePath));
        Assert.NotNull(updatedSnapshot);
        Assert.True(updatedSnapshot.ForexUnlocked);
        Assert.True(updatedSnapshot.ForeignReserves >= 150_000_000.0);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyCentralBankDecision_RateHike_UpdatesSnapshot()
    {
        var (world, country) = CreateStandardTestCountry();
        SaveTestState(world, country);

        var result = PresidentialDeskBridge.ApplyCentralBankDecision(
            operation: "rate_hike",
            value: 1.0,
            confirmed: true,
            stateFilePath: _stateFilePath,
            saveFilePath: _saveFilePath,
            requestId: "req-cb-hike");

        Assert.True(result.Success);
        Assert.Contains("Central Bank raised benchmark interest rate", result.Message);

        var updatedSnapshot = PresidentialDeskBridge.Deserialize(File.ReadAllText(_stateFilePath));
        Assert.NotNull(updatedSnapshot);
        Assert.Equal(5.5, updatedSnapshot.CentralBankInterestRate);
    }

    [Fact]
    public void PresidentialDeskBridge_ProcessPendingDecisionRequest_HandlesBudgetAndLoanRequests()
    {
        var (world, country) = CreateStandardTestCountry();
        SaveTestState(world, country);

        // 1. Process Budget Request
        var budgetRequest = new
        {
            request_id = "req-pipe-budget",
            action = "adjust_budget",
            defense = 550_000_000.0,
            healthcare = 450_000_000.0,
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(budgetRequest));

        var budgetResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(_stateFilePath, _saveFilePath, _requestFilePath);
        Assert.NotNull(budgetResult);
        Assert.True(budgetResult.Success);
        Assert.False(File.Exists(_requestFilePath), "Request file should be cleaned up after processing.");

        // 2. Process Domestic Loan Request
        var loanRequest = new
        {
            request_id = "req-pipe-loan",
            action = "take_loan",
            loan_type = "domestic",
            amount = 30_000_000.0,
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(loanRequest));

        var loanResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(_stateFilePath, _saveFilePath, _requestFilePath);
        Assert.NotNull(loanResult);
        Assert.True(loanResult.Success);
        Assert.False(File.Exists(_requestFilePath));

        // 3. Process Central Bank Rate Cut Request
        var cbRequest = new
        {
            request_id = "req-pipe-cb",
            action = "central_bank",
            operation = "rate_cut",
            value = 0.5,
            confirmed = true
        };
        File.WriteAllText(_requestFilePath, JsonSerializer.Serialize(cbRequest));

        var cbResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(_stateFilePath, _saveFilePath, _requestFilePath);
        Assert.NotNull(cbResult);
        Assert.True(cbResult.Success);
        Assert.False(File.Exists(_requestFilePath));
    }
}
