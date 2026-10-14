namespace Republic.Core.Tests.Economy;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Economy.Treasury;
using Republic.Core.Events;
using Republic.Core.Scenarios.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Services;
using Xunit;

/// <summary>
/// Phase 12 test suite: Connect Taxation Revenue to the National Treasury.
/// Validates the complete fiscal loop:
/// - Domestic tax policy establishes tax rates
/// - Fiscal system calculates revenue using established rules
/// - Ministry expenditure rules are applied
/// - Fiscal report accurately describes revenue, expenditure, and surplus/deficit
/// - Sovereign treasury is updated exactly once (depositing surplus or deducting deficit)
/// - Idempotency and duplicate transaction prevention
/// - Non-finite / invalid value rejection safely preserving treasury
/// - Sovereign country isolation
/// - Presidential desk / NationalDashboard reflection
/// - Save/load persistence
/// - Harmonious integration with Phase 11 citizen welfare & approval rules
/// </summary>
public sealed class FiscalTaxationTreasuryTests
{
    private readonly RepublicClock _clock;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly BudgetService _budgetService;

    public FiscalTaxationTreasuryTests()
    {
        _clock = RepublicClock.CreateControlled();
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Fiscal Test World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _budgetService = new BudgetService(_world, _eventBus, _workspace, _logger);
    }

    private Country CreateTestCountry(
        string name = "Republic of Arcadia",
        double initialTreasury = 10_000_000_000.0,
        double gdp = 450_000_000_000.0,
        double tradeBalance = 2_100_000_000.0,
        double incomeTax = 0.25,
        double corpTax = 0.20,
        double tariff = 0.05)
    {
        var countryId = name.ToLowerInvariant().Replace(" ", "-");
        var time = _clock.CurrentTime;
        var treasury = new RepuTreasury(initialTreasury, countryId);

        var country = Country.Found(
            name,
            time,
            id: countryId,
            treasury: treasury,
            grossDomesticProduct: gdp,
            tradeBalance: tradeBalance);

        country.TaxPolicy.IncomeTaxRate = incomeTax;
        country.TaxPolicy.CorporateTaxRate = corpTax;
        country.TaxPolicy.ImportTariffRate = tariff;

        _world.Countries.RegisterCountry(country);
        return country;
    }

    [Fact]
    public async Task TaxPolicy_DirectlyDeterminesQuarterlyTaxRevenue()
    {
        // 1. Arrange: GDP = 450B, TradeBalance = 2.1B
        // Tax rates: Income: 30%, Corporate: 25%, Tariff: 10%
        var country = CreateTestCountry(
            initialTreasury: 5_000_000_000.0,
            gdp: 450_000_000_000.0,
            tradeBalance: 2_100_000_000.0,
            incomeTax: 0.30,
            corpTax: 0.25,
            tariff: 0.10);

        // Expected calculation:
        // Income tax rev = 450B * 0.15 * 0.30 = 20,250,000,000
        // Corp tax rev   = 450B * 0.10 * 0.25 = 11,250,000,000
        // Tariff rev     = 2.1B * 0.10        =    210,000,000
        // Total tax rev  = 20.25B + 11.25B + 0.21B = 31,710,000,000
        var expectedIncomeTax = 20_250_000_000.0;
        var expectedCorpTax = 11_250_000_000.0;
        var expectedTariff = 210_000_000.0;
        var expectedTotalTax = 31_710_000_000.0;

        // 2. Act
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 3. Assert
        Assert.NotNull(report);
        Assert.Equal(1, report.QuarterIndex);
        Assert.Equal(country.Id, report.CountryId);
        Assert.Equal(expectedIncomeTax, report.IncomeTaxRevenue);
        Assert.Equal(expectedCorpTax, report.CorporateTaxRevenue);
        Assert.Equal(expectedTariff, report.TariffRevenue);
        Assert.Equal(expectedTotalTax, report.TotalTaxRevenue);
    }

    [Fact]
    public async Task FiscalSurplus_DepositsNetOutcomeDirectlyIntoSovereignTreasury()
    {
        // 1. Arrange: Initial balance R10B
        // Default policy (25% income, 20% corp, 5% tariff) =>
        // Rev: (450B * 0.15 * 0.25) + (450B * 0.10 * 0.20) + (2.1B * 0.05)
        // Rev: 16.875B + 9.0B + 0.105B = 25,980,000,000
        // Default expenditure: 500M + 400M + 350M + 600M + 250M = 2,100,000,000
        // Net surplus = 25,980,000,000 - 2,100,000,000 = 23,880,000,000
        var initialBalance = 10_000_000_000.0;
        var country = CreateTestCountry(initialTreasury: initialBalance);

        // 2. Act
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 3. Assert
        var expectedSurplus = report.TotalTaxRevenue - report.TotalExpenditures;
        Assert.True(expectedSurplus > 0, "Fiscal quarter should produce a surplus.");
        Assert.Equal(expectedSurplus, report.NetSurplusOrDeficit);

        var expectedBalance = initialBalance + expectedSurplus;
        Assert.Equal(expectedBalance, country.Treasury.Balance);
        Assert.Equal(expectedBalance, report.UpdatedTreasuryBalance);
        Assert.Equal(RepuTreasury.Format(expectedBalance), country.Treasury.FormattedBalance);
        Assert.Same(report, country.LatestFiscalReport);
        Assert.Equal(1, country.LastProcessedFiscalQuarter);
    }

    [Fact]
    public async Task FiscalDeficit_WithdrawsNetDeficitFromSovereignTreasury()
    {
        // 1. Arrange: Initial balance R10B
        // Tax rates set to 0.0 => Zero tax revenue
        // Ministry expenditures = R2.1B
        // Net deficit = -2,100,000,000
        var initialBalance = 10_000_000_000.0;
        var country = CreateTestCountry(
            initialTreasury: initialBalance,
            incomeTax: 0.0,
            corpTax: 0.0,
            tariff: 0.0);

        // 2. Act
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 3. Assert
        Assert.Equal(0.0, report.TotalTaxRevenue);
        Assert.Equal(2_100_000_000.0, report.TotalExpenditures);
        Assert.Equal(-2_100_000_000.0, report.NetSurplusOrDeficit);

        var expectedBalance = initialBalance - 2_100_000_000.0;
        Assert.Equal(expectedBalance, country.Treasury.Balance);
        Assert.Equal(expectedBalance, report.UpdatedTreasuryBalance);
        Assert.Equal(7_900_000_000.0, country.Treasury.Balance);
    }

    [Fact]
    public async Task FiscalDeficit_WithInsufficientFunds_ConsumesAvailableFundsWithoutNegativeOverdraft()
    {
        // 1. Arrange: Only R500M in treasury, but deficit is R2.1B
        var initialBalance = 500_000_000.0;
        var country = CreateTestCountry(
            initialTreasury: initialBalance,
            incomeTax: 0.0,
            corpTax: 0.0,
            tariff: 0.0);

        // 2. Act
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 3. Assert: All liquid funds safely consumed; balance reaches exactly 0.0 without crash or negative balance
        Assert.Equal(0.0, country.Treasury.Balance);
        Assert.Equal(0.0, report.UpdatedTreasuryBalance);
        Assert.Equal("R0", country.Treasury.FormattedBalance);
    }

    [Fact]
    public async Task FiscalQuarter_AppliedExactlyOnce_RepeatedExecutionDoesNotDuplicateTransactions()
    {
        // 1. Arrange: Initial balance R10B
        var country = CreateTestCountry(initialTreasury: 10_000_000_000.0);

        // 2. Act: First quarterly processing
        var report1 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        var balanceAfterFirst = country.Treasury.Balance;
        Assert.True(balanceAfterFirst > 10_000_000_000.0);

        // 3. Act: Re-applying the same report directly
        var reportReapplied = country.ApplyFiscalQuarter(report1);
        Assert.Equal(balanceAfterFirst, country.Treasury.Balance);
        Assert.Same(report1, reportReapplied);

        // 4. Act: Attempting to re-process with same quarter state via service
        var report2 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        Assert.Equal(balanceAfterFirst, country.Treasury.Balance);
        Assert.Equal(1, country.LastProcessedFiscalQuarter);
        Assert.Same(report1, report2);

        var transactionKey = $"fiscal-quarter-{country.Id}-Q1";
        Assert.True(country.Treasury.HasAppliedSnapshot(transactionKey));
        Assert.Equal(1, country.LastProcessedFiscalQuarter);
    }

    [Fact]
    public async Task InvalidFinancialValues_DoNotCorruptTreasuryState()
    {
        // 1. Arrange: Valid starting balance R5B
        var initialBalance = 5_000_000_000.0;
        var country = CreateTestCountry(initialTreasury: initialBalance);

        // 2. Act & Assert: NaN / Infinity in report
        var nanReport = new FiscalQuarterReport
        {
            QuarterIndex = 1,
            TotalTaxRevenue = double.NaN,
            TotalExpenditures = 1000.0
        };
        Assert.Throws<ArgumentException>(() => country.ApplyFiscalQuarter(nanReport));
        Assert.Equal(initialBalance, country.Treasury.Balance);

        var infReport = new FiscalQuarterReport
        {
            QuarterIndex = 1,
            TotalTaxRevenue = double.PositiveInfinity,
            TotalExpenditures = 1000.0
        };
        Assert.Throws<ArgumentException>(() => country.ApplyFiscalQuarter(infReport));
        Assert.Equal(initialBalance, country.Treasury.Balance);

        // 3. Act & Assert: Negative GDP in country processing
        country.GrossDomesticProduct = -100.0;
        await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime));
        Assert.Equal(initialBalance, country.Treasury.Balance);
    }


    [Fact]
    public async Task CountryIsolation_ProcessingCountryA_DoesNotMutateCountryB()
    {
        // 1. Arrange: Two sovereign countries
        var countryA = CreateTestCountry("Republic of Alpha", initialTreasury: 5_000_000_000.0);
        var countryB = CreateTestCountry("Republic of Beta", initialTreasury: 12_000_000_000.0);

        var initialBalanceB = countryB.Treasury.Balance;

        // 2. Act: Process fiscal quarter for Country A only
        var reportA = await _budgetService.ProcessQuarterlyReportAsync(countryA, _clock.CurrentTime);

        // 3. Assert: Country A was credited
        Assert.True(countryA.Treasury.Balance > 5_000_000_000.0);
        Assert.NotNull(countryA.LatestFiscalReport);
        Assert.Equal(1, countryA.LastProcessedFiscalQuarter);

        // Country B is completely untouched
        Assert.Equal(initialBalanceB, countryB.Treasury.Balance);
        Assert.Null(countryB.LatestFiscalReport);
        Assert.Equal(0, countryB.LastProcessedFiscalQuarter);
        Assert.False(countryB.Treasury.HasAppliedSnapshot($"fiscal-quarter-{countryB.Id}-Q1"));
    }

    [Fact]
    public async Task PresidentialDesk_NationalDashboard_ReflectsUpdatedTreasuryAndFiscalMetrics()
    {
        // 1. Arrange
        var country = CreateTestCountry("Republic of Arcadia", initialTreasury: 10_000_000_000.0);
        var dashboardBefore = country.GetDashboard(_clock.CurrentTime);
        Assert.Equal("R10B", dashboardBefore.Treasury);
        Assert.Equal("Not established", dashboardBefore.Fiscal);

        // 2. Act: Process quarterly fiscal settlement
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 3. Assert: Dashboard immediately reflects the resulting sovereign balance and fiscal line
        var dashboardAfter = country.GetDashboard(_clock.CurrentTime);
        Assert.Equal(country.Treasury.FormattedBalance, dashboardAfter.Treasury);
        Assert.Equal(country.Treasury.Balance, dashboardAfter.TreasuryBalance);
        Assert.NotNull(dashboardAfter.LatestFiscalReport);
        Assert.Contains($"Q{report.QuarterIndex}", dashboardAfter.Fiscal);
        Assert.Contains(report.FormattedTotalTaxRevenue, dashboardAfter.Fiscal);
        Assert.Contains(report.FormattedNetSurplusOrDeficit, dashboardAfter.Fiscal);
        Assert.Contains($"Fiscal: {dashboardAfter.Fiscal}", dashboardAfter.Lines);
        Assert.Contains($"Treasury: {dashboardAfter.Treasury}", dashboardAfter.Lines);
    }

    [Fact]
    public async Task PresidentialWorkspaceHandler_ProcessFiscalQuarterAsync_CoordinatesFiscalSettlement()
    {
        // 1. Arrange
        var country = CreateTestCountry("Republic of Arcadia", initialTreasury: 10_000_000_000.0);

        // 2. Act
        var outcome = await PresidentialWorkspaceHandler.ProcessFiscalQuarterAsync(
            country,
            _budgetService,
            _clock.CurrentTime);

        // 3. Assert
        Assert.True(outcome.IsSuccess);
        Assert.NotNull(outcome.FiscalReport);
        Assert.Equal(1, outcome.FiscalReport.QuarterIndex);
        Assert.Contains("Fiscal Quarter 1 settlement processed", outcome.Message);
        Assert.Equal(country.Treasury.FormattedBalance, outcome.FiscalReport.FormattedUpdatedTreasuryBalance);
    }

    [Fact]
    public async Task Phase11_TaxDecision_And_Phase12_FiscalTreasury_WorkHarmoniously()
    {
        // 1. Arrange
        var country = CreateTestCountry(
            name: "Republic of Arcadia",
            initialTreasury: 10_000_000_000.0,
            gdp: 450_000_000_000.0,
            incomeTax: 0.25);

        var time = _clock.CurrentTime;

        // 2. Act: Executive enacts a tax hike (25% -> 35%) via Phase 11 PresidentialWorkspaceHandler
        var newPolicy = new TaxPolicy { IncomeTaxRate = 0.35, CorporateTaxRate = 0.20, ImportTariffRate = 0.05 };
        var taxOutcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, newPolicy, time, confirmed: true);

        // Phase 11 consequence: Approval drops by 5.0% (+10% tax * -0.5 points/%)
        Assert.True(taxOutcome.IsSuccess);
        Assert.Equal(63.5, country.HappinessRating);
        Assert.Equal(-5.0, taxOutcome.CitizenFeedback!.ApprovalDelta);

        // 3. Act: Process quarterly fiscal settlement with the newly enacted tax rate
        var fiscalOutcome = await PresidentialWorkspaceHandler.ProcessFiscalQuarterAsync(country, _budgetService, time);

        // 4. Assert: Revenue calculated using the enacted 35% income tax rate!
        // Expected income tax rev: 450B * 0.15 * 0.35 = 23,625,000,000
        var expectedIncomeTax = 23_625_000_000.0;
        Assert.True(fiscalOutcome.IsSuccess);
        Assert.Equal(expectedIncomeTax, fiscalOutcome.FiscalReport!.IncomeTaxRevenue);

        // Treasury reflects the resulting surplus
        var expectedSurplus = fiscalOutcome.FiscalReport.TotalTaxRevenue - fiscalOutcome.FiscalReport.TotalExpenditures;
        Assert.Equal(10_000_000_000.0 + expectedSurplus, country.Treasury.Balance);

        // Approval is preserved exactly from Phase 11
        Assert.Equal(63.5, country.HappinessRating);
    }

    [Fact]
    public async Task SaveLoadPersistence_PreservesFiscalReportAndQuarterIndex()
    {
        // 1. Arrange: Country with processed fiscal quarter
        var country = CreateTestCountry(initialTreasury: 10_000_000_000.0);
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // 2. Act: Serialize and Deserialize via JSON
        var json = JsonSerializer.Serialize(country);
        var deserialized = JsonSerializer.Deserialize<Country>(json);

        // 3. Assert: All fiscal state accurately preserved
        Assert.NotNull(deserialized);
        Assert.Equal(country.Treasury.Balance, deserialized.Treasury.Balance);
        Assert.Equal(1, deserialized.LastProcessedFiscalQuarter);
        Assert.NotNull(deserialized.LatestFiscalReport);
        Assert.Equal(report.QuarterIndex, deserialized.LatestFiscalReport.QuarterIndex);
        Assert.Equal(report.TotalTaxRevenue, deserialized.LatestFiscalReport.TotalTaxRevenue);
        Assert.Equal(report.TotalExpenditures, deserialized.LatestFiscalReport.TotalExpenditures);
        Assert.Equal(report.NetSurplusOrDeficit, deserialized.LatestFiscalReport.NetSurplusOrDeficit);
        Assert.Equal(report.UpdatedTreasuryBalance, deserialized.LatestFiscalReport.UpdatedTreasuryBalance);
        Assert.Equal(country.TradeBalance, deserialized.TradeBalance);
        Assert.Equal(country.MinistryBudget.TotalQuarterlyExpenditures, deserialized.MinistryBudget.TotalQuarterlyExpenditures);
    }
}
