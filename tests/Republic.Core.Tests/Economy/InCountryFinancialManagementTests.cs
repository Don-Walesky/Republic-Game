namespace Republic.Core.Tests.Economy;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Debt.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class InCountryFinancialManagementTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _statePath;
    private readonly string _savePath;

    public InCountryFinancialManagementTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "Republic_FinMgmtTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _statePath = Path.Combine(_tempDir, "presidential_desk_state.json");
        _savePath = Path.Combine(_tempDir, "presidential_desk_state.sav");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch { /* best-effort cleanup */ }
    }

    private Country CreateStandardTestCountry()
    {
        var country = new Country
        {
            Id = "test-republic",
            Name = "Republic of Testia"
        };
        country.GrossDomesticProduct = 450_000_000_000.0;
        country.Treasury.Deposit(500_000_000.0);
        country.TaxPolicy.IncomeTaxRate = 0.25;
        country.TaxPolicy.CorporateTaxRate = 0.20;
        country.TaxPolicy.ImportTariffRate = 0.05;
        country.CentralBank.DepositForeignReserves(50_000_000.0);

        var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var simTime = new RepublicTime(launchEpoch, launchEpoch);
        country.UpdateEconomicCycle(simTime);

        // Export save and state file
        PresidentialDeskBridge.SaveAuthoritativeState(country, simTime, 1, _savePath);
        var snapshot = PresidentialDeskSnapshot.Create(country, simTime, 1, _savePath);
        File.WriteAllText(_statePath, PresidentialDeskBridge.Serialize(snapshot));

        return country;
    }

    [Fact]
    public void MinistryBudget_IncludesRecurrentOngoingGovernanceExpenses()
    {
        var budget = new MinistryBudget();

        Assert.Equal(85_000_000.0, budget.CivilServiceSalariesAllocation);
        Assert.Equal(140_000_000.0, budget.GovernanceOperationsAllocation);
        Assert.Equal(180_000_000.0, budget.InfrastructureMaintenanceAllocation);

        Assert.Equal(405_000_000.0, budget.TotalOngoingGovernanceExpenses);
        Assert.Equal(2_505_000_000.0, budget.TotalQuarterlyExpenditures);
    }

    [Fact]
    public void MinisterialAppointment_EnforcesTenMinuteCountdownAndOneAtATimeRestriction()
    {
        CreateStandardTestCountry();

        // 1. First appointment: Appoint Finance Minister (Technocrat)
        var result1 = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
            "Finance",
            "technocrat",
            confirmed: true,
            _statePath,
            _savePath,
            "req_app_1");

        Assert.True(result1.Success, result1.Message);
        Assert.Equal("Success", result1.Result);

        // Verify snapshot shows active appointment in progress with countdown timer
        var rawJson1 = File.ReadAllText(_statePath);
        var snap1 = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(rawJson1);
        Assert.NotNull(snap1);
        Assert.True(snap1.IsAppointmentInProgress);
        Assert.Equal("Finance", snap1.ActiveAppointmentPortfolio);
        Assert.Equal("Dr. Elena Rostova", snap1.ActiveAppointmentMinisterName);
        Assert.True(snap1.ActiveAppointmentTimeRemainingSeconds > 0);
        Assert.Contains("Confirmation Hearing in Progress", snap1.AppointmentStatusText);

        // 2. Second appointment attempted immediately while first is running: must be rejected!
        var result2 = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
            "Interior",
            "technocrat",
            confirmed: true,
            _statePath,
            _savePath,
            "req_app_2");

        Assert.False(result2.Success);
        Assert.Equal("AppointmentInProgress", result2.Result);
        Assert.Contains("currently underway", result2.Message);

        // 3. Complete the active appointment hearing
        var completeResult = PresidentialDeskBridge.ApplyCompleteAppointmentDecision(
            _statePath,
            _savePath,
            "req_app_complete");

        Assert.True(completeResult.Success);

        // 4. Now second appointment is permitted!
        var result3 = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
            "Interior",
            "technocrat",
            confirmed: true,
            _statePath,
            _savePath,
            "req_app_3");

        Assert.True(result3.Success, result3.Message);
        Assert.Equal("Success", result3.Result);

        var rawJson3 = File.ReadAllText(_statePath);
        var snap3 = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(rawJson3);
        Assert.NotNull(snap3);
        Assert.True(snap3.IsAppointmentInProgress);
        Assert.Equal("Interior", snap3.ActiveAppointmentPortfolio);
    }

    [Fact]
    public void RevenueActivityDecision_EnactsProgram_DeductsSetup_BolstersQuarterlyRevenue()
    {
        var country = CreateStandardTestCountry();
        double initialTreasury = country.Treasury.Balance;

        // 1. Enact Digital Tax Compliance Drive (Cost: 35M, Yield: +120M)
        var result = PresidentialDeskBridge.ApplyRevenueActivityDecision(
            "rev_digital_tax_audit",
            confirmed: true,
            _statePath,
            _savePath,
            "req_rev_1");

        Assert.True(result.Success, result.Message);
        Assert.Equal("Success", result.Result);

        var rawJson = File.ReadAllText(_statePath);
        var snap = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(rawJson);
        Assert.NotNull(snap);

        Assert.Contains("rev_digital_tax_audit", snap.EnactedRevenueActivities);
        Assert.Equal(120_000_000.0, snap.AdditionalQuarterlyRevenue);
        Assert.Equal("+R120M / Qtr", snap.AdditionalQuarterlyRevenueFormatted);
        Assert.Equal(initialTreasury - 35_000_000.0, snap.TreasuryBalance);

        // Verify available activities list flags it as enacted
        var act = snap.AvailableRevenueActivities.FirstOrDefault(a => a.Id == "rev_digital_tax_audit");
        Assert.NotNull(act);
        Assert.True(act.IsEnacted);

        // 2. Attempting to enact the same program again returns AlreadyEnacted
        var dupResult = PresidentialDeskBridge.ApplyRevenueActivityDecision(
            "rev_digital_tax_audit",
            confirmed: true,
            _statePath,
            _savePath,
            "req_rev_dup");

        Assert.False(dupResult.Success);
        Assert.Equal("AlreadyEnacted", dupResult.Result);
    }

    [Fact]
    public void FinanceMinister_AdvisoryRole_ProvidesAdvice_RecommendsLoan_AndAssistsRepaymentPlans()
    {
        CreateStandardTestCountry();

        // 1. Before appointment: Finance Minister advice indicates vacancy
        var preAppointJson = File.ReadAllText(_statePath);
        var preSnap = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(preAppointJson);
        Assert.NotNull(preSnap);
        Assert.False(preSnap.IsFinanceMinisterAppointed);
        Assert.Equal("Finance Portfolio Vacant", preSnap.FinanceMinisterAdviceHeadline);

        // 2. Appoint Finance Minister (Elena Rostova)
        var appointResult = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
            "Finance",
            "technocrat",
            confirmed: true,
            _statePath,
            _savePath,
            "req_fin_appoint");
        Assert.True(appointResult.Success);

        // 3. Inspect updated snapshot: Finance Minister advisory is active
        var postAppointJson = File.ReadAllText(_statePath);
        var postSnap = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(postAppointJson);
        Assert.NotNull(postSnap);
        Assert.True(postSnap.IsFinanceMinisterAppointed);
        Assert.Equal("Dr. Elena Rostova", postSnap.FinanceMinisterName);
        Assert.NotEmpty(postSnap.FinanceMinisterExpenditureAdvice);
        Assert.NotEmpty(postSnap.FinanceMinisterRevenueAdvice);
        Assert.NotEmpty(postSnap.FinanceMinisterLoanRecommendation);

        // Because reserves are at 50M (< 100M), recommendation should be External Multilateral Facility
        Assert.Equal("external", postSnap.FinanceMinisterRecommendedLoanType);
        Assert.Equal(75_000_000.0, postSnap.FinanceMinisterRecommendedLoanAmount);

        // 4. Update repayment plan to Accelerated Paydown
        var planResult = PresidentialDeskBridge.ApplyRepaymentPlanDecision(
            "Accelerated Debt Paydown",
            confirmed: true,
            _statePath,
            _savePath,
            "req_plan_1");
        Assert.True(planResult.Success);

        var planJson = File.ReadAllText(_statePath);
        var planSnap = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(planJson);
        Assert.NotNull(planSnap);
        Assert.Equal("Accelerated Debt Paydown", planSnap.CurrentDebtRepaymentPlan);
        Assert.Contains("Accelerated Paydown active", planSnap.FinanceMinisterRepaymentPlanAdvice);

        // 5. Take recommended loan
        var loanResult = PresidentialDeskBridge.ApplyRecommendedLoanDecision(
            confirmed: true,
            _statePath,
            _savePath,
            "req_loan_rec");
        Assert.True(loanResult.Success, loanResult.Message);

        var loanJson = File.ReadAllText(_statePath);
        var loanSnap = JsonSerializer.Deserialize<PresidentialDeskSnapshot>(loanJson);
        Assert.NotNull(loanSnap);
        Assert.True(loanSnap.NationalDebtExternal >= 175_000_000.0);
        Assert.True(loanSnap.ForeignReserves >= 125_000_000.0);
    }
}
