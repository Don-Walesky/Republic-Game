namespace Republic.Core.Tests.Economy;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Economy.Budget.Events;
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
/// Phase 13 test suite: Reliable Simulation-Time Fiscal Cycles.
/// Validates:
/// - Fiscal quarter periods depend strictly on Republic simulation time (RepublicTime), not call frequency.
/// - Exactly one fiscal settlement per country per eligible simulation quarter.
/// - Idempotent processing within the same quarter (zero duplicate debits or credits).
/// - Multi-quarter time advancement processes all due quarters in chronological sequence.
/// - First eligible quarter follows country founding day (pre-founding quarters are never charged).
/// - Event and report timestamps use authoritative simulation WatTimestamp instead of wall-clock UtcNow.
/// - Integration with PresidentialWorkspaceHandler.AdvanceTime and AdvanceTimeAsync.
/// - JSON save/load persistence of fiscal quarter state across simulation time.
/// - Sovereign country isolation, non-negative treasury constraint, and non-finite value protection.
/// </summary>
public sealed class SimulationTimeFiscalCycleTests
{
    private readonly RepublicClock _clock;
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;
    private readonly WorldManager _world;
    private readonly WorkspaceManager _workspace;
    private readonly BudgetService _budgetService;

    public SimulationTimeFiscalCycleTests()
    {
        _clock = RepublicClock.CreateControlled();
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
        _world = new WorldManager(_eventBus, _logger, _clock);
        _world.CreateAsync("Fiscal Simulation World").GetAwaiter().GetResult();

        _workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);

        _budgetService = new BudgetService(_world, _eventBus, _workspace, _logger, _clock);
    }

    private Country CreateCountry(
        string name = "Republic of Walesky",
        double initialTreasury = 10_000_000_000.0,
        double gdp = 400_000_000_000.0,
        double tradeBalance = 2_000_000_000.0,
        double incomeTax = 0.25,
        double corpTax = 0.20,
        double tariff = 0.05,
        RepublicTime? foundingTime = null)
    {
        var countryId = name.ToLowerInvariant().Replace(" ", "-");
        var time = foundingTime ?? _clock.CurrentTime;
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
    public void FiscalSchedule_CalculatesQuarterIndexAndStartBoundaryCorrectly()
    {
        var launchEpoch = _clock.CurrentTime.LaunchEpochWat;

        // Day 0..89 is Quarter 1
        var day0 = RepublicTime.FromDayAndTime(0, TimeOnly.MinValue, launchEpoch);
        var day45 = RepublicTime.FromDayAndTime(45, new TimeOnly(12, 0), launchEpoch);
        var day89 = RepublicTime.FromDayAndTime(89, new TimeOnly(23, 59), launchEpoch);
        Assert.Equal(1, FiscalSchedule.GetQuarterIndex(day0));
        Assert.Equal(1, FiscalSchedule.GetQuarterIndex(day45));
        Assert.Equal(1, FiscalSchedule.GetQuarterIndex(day89));

        // Day 90..179 is Quarter 2
        var day90 = RepublicTime.FromDayAndTime(90, TimeOnly.MinValue, launchEpoch);
        var day179 = RepublicTime.FromDayAndTime(179, new TimeOnly(23, 59), launchEpoch);
        Assert.Equal(2, FiscalSchedule.GetQuarterIndex(day90));
        Assert.Equal(2, FiscalSchedule.GetQuarterIndex(day179));

        // Day 180 is Quarter 3
        var day180 = RepublicTime.FromDayAndTime(180, TimeOnly.MinValue, launchEpoch);
        Assert.Equal(3, FiscalSchedule.GetQuarterIndex(day180));

        // Day 270 is Quarter 4
        var day270 = RepublicTime.FromDayAndTime(270, TimeOnly.MinValue, launchEpoch);
        Assert.Equal(4, FiscalSchedule.GetQuarterIndex(day270));

        // Verify start boundaries
        var q1Start = FiscalSchedule.GetQuarterStartTime(1, launchEpoch);
        Assert.Equal(0, q1Start.DayNumber);
        Assert.Equal(TimeOnly.MinValue, q1Start.Time);

        var q2Start = FiscalSchedule.GetQuarterStartTime(2, launchEpoch);
        Assert.Equal(90, q2Start.DayNumber);
        Assert.Equal(TimeOnly.MinValue, q2Start.Time);

        var q3Start = FiscalSchedule.GetQuarterStartTime(3, launchEpoch);
        Assert.Equal(180, q3Start.DayNumber);

        var q4Start = FiscalSchedule.GetQuarterStartTime(4, launchEpoch);
        Assert.Equal(270, q4Start.DayNumber);

        // Invalid quarter index throws ArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => FiscalSchedule.GetQuarterStartTime(0, launchEpoch));
    }

    [Fact]
    public async Task FirstEligibleQuarter_RespectsCountryFoundingDay_PreFoundingQuartersNeverCharged()
    {
        var launchEpoch = _clock.CurrentTime.LaunchEpochWat;

        // Country A founded at Day 0 -> first eligible is Quarter 1
        var countryA = CreateCountry("Country Alpha", foundingTime: _clock.CurrentTime);
        Assert.Equal(1, FiscalSchedule.GetFirstEligibleQuarter(countryA));

        // Country B founded at Day 100 (Quarter 2) -> first eligible is Quarter 2
        var day100 = RepublicTime.FromDayAndTime(100, TimeOnly.MinValue, launchEpoch);
        var countryB = CreateCountry("Country Beta", foundingTime: day100, initialTreasury: 5_000_000_000.0);
        Assert.Equal(2, FiscalSchedule.GetFirstEligibleQuarter(countryB));

        // At Day 50 (Quarter 1), Country B is pre-founding
        var day50 = RepublicTime.FromDayAndTime(50, TimeOnly.MinValue, launchEpoch);
        var dueQAtDay50 = FiscalSchedule.GetDueQuarters(countryB, day50);
        Assert.Empty(dueQAtDay50);

        // Attempting to process Country B at Day 50 must not charge or settle anything
        var preReport = await _budgetService.ProcessQuarterlyReportAsync(countryB, day50);
        Assert.NotNull(preReport);
        Assert.Equal(5_000_000_000.0, countryB.Treasury.Balance);
        Assert.Equal(0, countryB.LastProcessedFiscalQuarter);

        // At Day 100 (Quarter 2), only Quarter 2 is due for Country B (Quarter 1 was never charged)
        var dueQAtDay100 = FiscalSchedule.GetDueQuarters(countryB, day100);
        Assert.Single(dueQAtDay100);
        Assert.Equal(2, dueQAtDay100[0]);

        var reportQ2 = await _budgetService.ProcessQuarterlyReportAsync(countryB, day100);
        Assert.Equal(2, reportQ2.QuarterIndex);
        Assert.Equal(2, countryB.LastProcessedFiscalQuarter);
        Assert.True(countryB.Treasury.Balance > 5_000_000_000.0);
        Assert.False(countryB.Treasury.HasAppliedSnapshot($"fiscal-quarter-{countryB.Id}-Q1"));
    }

    [Fact]
    public async Task FiscalQuarter_IdempotentWithinSameQuarter_RepeatedCallsDoNotDuplicateSettlements()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);
        var time = _clock.CurrentTime; // Day 0

        // 1. First execution in Quarter 1
        var report1 = await _budgetService.ProcessQuarterlyReportAsync(country, time);
        var balanceAfterFirst = country.Treasury.Balance;
        Assert.True(balanceAfterFirst > initialTreasury);
        Assert.Equal(1, country.LastProcessedFiscalQuarter);
        Assert.Equal(1, report1.QuarterIndex);

        // 2. Multiple repeated calls during Day 0, Day 15, Day 45, Day 89
        var launchEpoch = time.LaunchEpochWat;
        var timesInQ1 = new[]
        {
            time,
            RepublicTime.FromDayAndTime(15, new TimeOnly(10, 0), launchEpoch),
            RepublicTime.FromDayAndTime(45, new TimeOnly(14, 30), launchEpoch),
            RepublicTime.FromDayAndTime(89, new TimeOnly(23, 59), launchEpoch)
        };

        foreach (var t in timesInQ1)
        {
            var due = FiscalSchedule.GetDueQuarters(country, t);
            Assert.Empty(due);

            var report = await _budgetService.ProcessQuarterlyReportAsync(country, t);
            Assert.Same(report1, report);
            Assert.Equal(balanceAfterFirst, country.Treasury.Balance);
            Assert.Equal(1, country.LastProcessedFiscalQuarter);
        }
    }

    [Fact]
    public async Task SimulationTimeAdvance_CrossesQuarterBoundary_SettlesNextQuarterExactlyOnce()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);

        // Day 0: Settle Q1
        var reportQ1 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        var balanceAfterQ1 = country.Treasury.Balance;
        Assert.Equal(1, country.LastProcessedFiscalQuarter);

        // Advance simulation clock by 90 days to Day 90 (Quarter 2)
        _clock.AdvanceDays(90);
        Assert.Equal(90, _clock.CurrentTime.DayNumber);
        Assert.Equal(2, FiscalSchedule.GetQuarterIndex(_clock.CurrentTime));

        // Due quarters at Day 90: exactly [2]
        var due = FiscalSchedule.GetDueQuarters(country, _clock.CurrentTime);
        Assert.Single(due);
        Assert.Equal(2, due[0]);

        // Settle Q2
        var reportQ2 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        Assert.Equal(2, reportQ2.QuarterIndex);
        Assert.Equal(2, country.LastProcessedFiscalQuarter);

        var expectedBalanceAfterQ2 = balanceAfterQ1 + reportQ2.NetSurplusOrDeficit;
        Assert.Equal(expectedBalanceAfterQ2, country.Treasury.Balance);

        // Calling again at Day 95 does not re-settle Q2
        _clock.AdvanceDays(5);
        var reportQ2Repeat = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        Assert.Same(reportQ2, reportQ2Repeat);
        Assert.Equal(expectedBalanceAfterQ2, country.Treasury.Balance);
        Assert.Equal(2, country.LastProcessedFiscalQuarter);
    }

    [Fact]
    public async Task MultiQuarterSimulationAdvance_ProcessesAllDueQuartersInSequence()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);

        // Settle Q1 at Day 0
        var reportQ1 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        var balanceAfterQ1 = country.Treasury.Balance;
        Assert.Equal(1, country.LastProcessedFiscalQuarter);

        // Advance simulation clock by 270 days to Day 270 (Quarter 4)
        _clock.AdvanceDays(270);
        Assert.Equal(270, _clock.CurrentTime.DayNumber);
        Assert.Equal(4, FiscalSchedule.GetQuarterIndex(_clock.CurrentTime));

        // Due quarters must be [2, 3, 4]
        var dueQuarters = FiscalSchedule.GetDueQuarters(country, _clock.CurrentTime);
        Assert.Equal(new[] { 2, 3, 4 }, dueQuarters);

        // Process all due quarters in sequence
        var reports = await _budgetService.ProcessDueQuartersAsync(country, _clock.CurrentTime);
        Assert.Equal(3, reports.Count);
        Assert.Equal(2, reports[0].QuarterIndex);
        Assert.Equal(3, reports[1].QuarterIndex);
        Assert.Equal(4, reports[2].QuarterIndex);

        // Authoritative country state reflects Quarter 4
        Assert.Equal(4, country.LastProcessedFiscalQuarter);
        Assert.Same(reports[2], country.LatestFiscalReport);

        // Total treasury equals Q1 + Q2 + Q3 + Q4 net surpluses
        var expectedBalance = balanceAfterQ1 + reports[0].NetSurplusOrDeficit + reports[1].NetSurplusOrDeficit + reports[2].NetSurplusOrDeficit;
        Assert.Equal(expectedBalance, country.Treasury.Balance);

        // Immediate subsequent call returns 0 due quarters
        var secondBatch = await _budgetService.ProcessDueQuartersAsync(country, _clock.CurrentTime);
        Assert.Empty(secondBatch);
        Assert.Equal(expectedBalance, country.Treasury.Balance);
    }

    [Fact]
    public async Task PresidentialWorkspaceHandler_AdvanceTime_AutomaticallySettlesDueFiscalQuarters()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);

        var timeSystem = new TimeSystem(new TimeSystemConfiguration(), _eventBus, _logger, _clock);

        // First settle Q1 at Day 0
        var reportQ1 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        Assert.Equal(1, country.LastProcessedFiscalQuarter);

        // Advance simulation time by 180 days (crosses Q2 and Q3 boundaries)
        var summary = PresidentialWorkspaceHandler.AdvanceTime(
            _world,
            timeSystem,
            TimeSpan.FromDays(180),
            budgetService: _budgetService);

        Assert.Equal(180, _clock.CurrentTime.DayNumber);
        Assert.Equal(2, summary.FiscalQuartersProcessed);
        Assert.Equal(3, country.LastProcessedFiscalQuarter);
        Assert.NotNull(country.LatestFiscalReport);
        Assert.Equal(3, country.LatestFiscalReport.QuarterIndex);
    }

    [Fact]
    public async Task PresidentialWorkspaceHandler_AdvanceTimeAsync_ProcessesDueQuartersAsynchronously()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);

        var timeSystem = new TimeSystem(new TimeSystemConfiguration(), _eventBus, _logger, _clock);

        // Advance simulation time by 90 days from Day 0 (crosses Q1 boundary at Day 0, then enters Q2 at Day 90)
        var summary = await PresidentialWorkspaceHandler.AdvanceTimeAsync(
            _world,
            timeSystem,
            TimeSpan.FromDays(90),
            budgetService: _budgetService);

        Assert.Equal(90, _clock.CurrentTime.DayNumber);
        Assert.Equal(2, summary.FiscalQuartersProcessed); // Q1 and Q2 processed
        Assert.Equal(2, country.LastProcessedFiscalQuarter);
        Assert.Equal(2, country.LatestFiscalReport!.QuarterIndex);
    }

    [Fact]
    public async Task EventOccurredAt_UsesSimulationWatTimestamp_NotWallClockUtcNow()
    {
        var country = CreateCountry();
        QuarterlyFiscalReportGeneratedEvent? capturedEvent = null;

        using var subscription = _eventBus.Subscribe<QuarterlyFiscalReportGeneratedEvent>((evt, _) =>
        {
            capturedEvent = evt;
            return ValueTask.CompletedTask;
        });

        // Day 0
        var simTime = _clock.CurrentTime;
        var report = await _budgetService.ProcessQuarterlyReportAsync(country, simTime);
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.NotNull(capturedEvent);
        Assert.Equal(report.QuarterIndex, capturedEvent.Report.QuarterIndex);
        Assert.Equal(simTime.WatTimestamp, capturedEvent.OccurredAt);
        Assert.Equal(RepublicTime.WatOffset, capturedEvent.OccurredAt.Offset);
    }

    [Fact]
    public async Task SaveLoadPersistence_PreservesSimulationTimeFiscalSchedule()
    {
        var country = CreateCountry(initialTreasury: 10_000_000_000.0);

        // Process Q1 and Q2
        await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        _clock.AdvanceDays(90);
        await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        Assert.Equal(2, country.LastProcessedFiscalQuarter);

        // Serialize to JSON and deserialize
        var json = JsonSerializer.Serialize(country);
        var reloaded = JsonSerializer.Deserialize<Country>(json);

        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded.LastProcessedFiscalQuarter);
        Assert.NotNull(reloaded.LatestFiscalReport);
        Assert.Equal(2, reloaded.LatestFiscalReport.QuarterIndex);
        Assert.Equal(country.Treasury.Balance, reloaded.Treasury.Balance);

        // Re-evaluating reloaded country at Day 120 (within Q2) does not duplicate
        var day120 = RepublicTime.FromDayAndTime(120, TimeOnly.MinValue, _clock.CurrentTime.LaunchEpochWat);
        var due = FiscalSchedule.GetDueQuarters(reloaded, day120);
        Assert.Empty(due);
    }

    [Fact]
    public async Task SovereignIsolation_ForeignCountryNotSettled()
    {
        var countryA = CreateCountry("Republic of Alpha", initialTreasury: 5_000_000_000.0);
        var countryB = CreateCountry("Republic of Beta", initialTreasury: 8_000_000_000.0);

        // Settle Q1 for Country A only
        var reportA = await _budgetService.ProcessQuarterlyReportAsync(countryA, _clock.CurrentTime);

        Assert.Equal(1, countryA.LastProcessedFiscalQuarter);
        Assert.True(countryA.Treasury.Balance > 5_000_000_000.0);

        // Country B is completely unmutated
        Assert.Equal(0, countryB.LastProcessedFiscalQuarter);
        Assert.Null(countryB.LatestFiscalReport);
        Assert.Equal(8_000_000_000.0, countryB.Treasury.Balance);
    }

    [Fact]
    public async Task FiscalSurplusAndDeficit_MaintainsNonNegativeTreasuryConstraint()
    {
        // Country with small treasury and 0% taxes -> running deficit of R2.1B
        var country = CreateCountry(
            name: "Deficit Nation",
            initialTreasury: 300_000_000.0,
            incomeTax: 0.0,
            corpTax: 0.0,
            tariff: 0.0);

        var report = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // Deficit consumed all available funds, balance is exactly 0 without negative balance
        Assert.Equal(0.0, country.Treasury.Balance);
        Assert.Equal(0.0, report.UpdatedTreasuryBalance);
        Assert.Equal("R0", country.Treasury.FormattedBalance);
        Assert.True(report.NetSurplusOrDeficit < 0.0);
    }

    [Fact]
    public async Task InvalidAndNonFiniteInputs_ThrowArgumentException_PreservingState()
    {
        var initialTreasury = 10_000_000_000.0;
        var country = CreateCountry(initialTreasury: initialTreasury);

        // Non-finite GDP
        country.GrossDomesticProduct = double.NaN;
        await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime));
        Assert.Equal(initialTreasury, country.Treasury.Balance);

        country.GrossDomesticProduct = 400_000_000_000.0;

        // Non-finite Tax Rate
        country.TaxPolicy.IncomeTaxRate = double.PositiveInfinity;
        await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime));
        Assert.Equal(initialTreasury, country.Treasury.Balance);
    }

    [Fact]
    public async Task Phase11_TaxPolicyChange_AppliesToNextSimulationQuarter()
    {
        var country = CreateCountry(incomeTax: 0.20);

        // Q1 settled at 20%
        var reportQ1 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);
        var gdp = country.GrossDomesticProduct;
        var expectedIncomeQ1 = gdp * 0.15 * 0.20;
        Assert.Equal(expectedIncomeQ1, reportQ1.IncomeTaxRevenue);

        // Executive changes tax to 30% via Phase 11 PresidentialWorkspaceHandler
        var newPolicy = new TaxPolicy { IncomeTaxRate = 0.30, CorporateTaxRate = 0.20, ImportTariffRate = 0.05 };
        var actionOutcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, newPolicy, _clock.CurrentTime);
        Assert.True(actionOutcome.IsSuccess);

        // Advance to Quarter 2
        _clock.AdvanceDays(90);
        var reportQ2 = await _budgetService.ProcessQuarterlyReportAsync(country, _clock.CurrentTime);

        // Q2 revenue evaluated with the new 30% rate
        var expectedIncomeQ2 = gdp * 0.15 * 0.30;
        Assert.Equal(expectedIncomeQ2, reportQ2.IncomeTaxRevenue);
        Assert.Equal(2, reportQ2.QuarterIndex);
    }
}
