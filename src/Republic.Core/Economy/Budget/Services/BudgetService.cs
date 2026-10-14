namespace Republic.Core.Economy.Budget.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Economy.Budget.Events;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Implementation managing fiscal taxation policy, ministry expenditures, and treasury quarterly reports.
/// </summary>
public sealed class BudgetService : IBudgetService
{
    private readonly TaxPolicy _taxPolicy = new();
    private readonly MinistryBudget _ministryBudget = new();
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly IRepublicClock? _clock;
    private readonly object _lock = new();
    private int _quarterCounter;

    public BudgetService(
        IWorldManager worldManager,
        IEventBus eventBus,
        IWorkspaceManager? workspaceManager = null,
        ILogger? logger = null,
        IRepublicClock? clock = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;
        _clock = clock ?? _worldManager.Countries?.Clock;
    }

    public TaxPolicy GetTaxPolicy()
    {
        lock (_lock)
        {
            return new TaxPolicy
            {
                IncomeTaxRate = _taxPolicy.IncomeTaxRate,
                CorporateTaxRate = _taxPolicy.CorporateTaxRate,
                ImportTariffRate = _taxPolicy.ImportTariffRate
            };
        }
    }

    public MinistryBudget GetMinistryBudget()
    {
        lock (_lock)
        {
            return new MinistryBudget
            {
                DefenseAllocation = _ministryBudget.DefenseAllocation,
                HealthcareAllocation = _ministryBudget.HealthcareAllocation,
                EducationAllocation = _ministryBudget.EducationAllocation,
                InfrastructureAllocation = _ministryBudget.InfrastructureAllocation,
                ScienceAllocation = _ministryBudget.ScienceAllocation
            };
        }
    }

    public async Task UpdateTaxPolicyAsync(TaxPolicy policy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);

        lock (_lock)
        {
            _taxPolicy.IncomeTaxRate = Math.Clamp(policy.IncomeTaxRate, 0.0, 0.70);
            _taxPolicy.CorporateTaxRate = Math.Clamp(policy.CorporateTaxRate, 0.0, 0.60);
            _taxPolicy.ImportTariffRate = Math.Clamp(policy.ImportTariffRate, 0.0, 0.50);
        }

        _logger?.LogInfo($"Tax Policy Updated: Income ({_taxPolicy.IncomeTaxRate * 100:0}%), Corp ({_taxPolicy.CorporateTaxRate * 100:0}%)");
        var timestamp = _clock?.CurrentTime.WatTimestamp ?? DateTimeOffset.UtcNow;
        await _eventBus.PublishAsync(new TaxPolicyUpdatedEvent(_taxPolicy, timestamp), cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMinistryBudgetAsync(MinistryBudget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);

        lock (_lock)
        {
            _ministryBudget.DefenseAllocation = Math.Max(0, budget.DefenseAllocation);
            _ministryBudget.HealthcareAllocation = Math.Max(0, budget.HealthcareAllocation);
            _ministryBudget.EducationAllocation = Math.Max(0, budget.EducationAllocation);
            _ministryBudget.InfrastructureAllocation = Math.Max(0, budget.InfrastructureAllocation);
            _ministryBudget.ScienceAllocation = Math.Max(0, budget.ScienceAllocation);
        }

        _logger?.LogInfo($"Ministry Budget Updated: Total Quarterly Expenditure (${_ministryBudget.TotalQuarterlyExpenditures:N0})");
        var timestamp = _clock?.CurrentTime.WatTimestamp ?? DateTimeOffset.UtcNow;
        await _eventBus.PublishAsync(new MinistryBudgetUpdatedEvent(_ministryBudget, timestamp), cancellationToken).ConfigureAwait(false);
    }

    public async Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        var playerCountry = _worldManager.Countries.GetCountry("player-country") ?? _worldManager.Countries.GetAllCountries().FirstOrDefault();
        var simulationTime = _clock?.CurrentTime ?? _worldManager.Countries.Clock.CurrentTime;
        if (playerCountry != null)
        {
            return await ProcessQuarterlyReportAsync(playerCountry, simulationTime, cancellationToken).ConfigureAwait(false);
        }

        TaxPolicy policy;
        MinistryBudget budget;
        lock (_lock)
        {
            _quarterCounter++;
            policy = GetTaxPolicy();
            budget = GetMinistryBudget();
        }

        var indicators = _worldManager.Economic.GetIndicators();
        var gdp = indicators.GrossDomesticProduct;

        // Calculate Quarterly Revenues
        var incomeTaxRev = gdp * 0.15 * policy.IncomeTaxRate;
        var corpTaxRev = gdp * 0.10 * policy.CorporateTaxRate;
        var tariffRev = Math.Max(0, indicators.TradeBalance) * policy.ImportTariffRate;
        var totalTaxRevenue = incomeTaxRev + corpTaxRev + tariffRev;

        var totalExpenditures = budget.TotalQuarterlyExpenditures;
        var netSurplusDeficit = totalTaxRevenue - totalExpenditures;

        // Update Treasury Balance
        if (netSurplusDeficit >= 0)
        {
            _worldManager.Economic.DepositTreasury(netSurplusDeficit);
        }
        else
        {
            _worldManager.Economic.WithdrawTreasury(Math.Abs(netSurplusDeficit));
        }

        var updatedTreasury = _worldManager.Economic.GetIndicators().TreasuryBalance;

        var report = new FiscalQuarterReport
        {
            QuarterIndex = _quarterCounter,
            IncomeTaxRevenue = incomeTaxRev,
            CorporateTaxRevenue = corpTaxRev,
            TariffRevenue = tariffRev,
            TotalTaxRevenue = totalTaxRevenue,
            TotalExpenditures = totalExpenditures,
            UpdatedTreasuryBalance = updatedTreasury,
            Timestamp = simulationTime
        };

        _logger?.LogInfo($"Q{_quarterCounter} Fiscal Report: Rev (${totalTaxRevenue:N0}) - Exp (${totalExpenditures:N0}) = Net (${netSurplusDeficit:N0})");
        await _eventBus.PublishAsync(new QuarterlyFiscalReportGeneratedEvent(report, simulationTime.WatTimestamp), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Ministry of Finance",
            Subject = $"FISCAL REPORT: Q{_quarterCounter} Financial Summary",
            Body = $"Quarter {_quarterCounter} report generated. Revenue: ${totalTaxRevenue:N0}, Expenditure: ${totalExpenditures:N0}. Treasury Balance: ${updatedTreasury:N0}.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return report;
    }

    public async Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(
        Country country,
        RepublicTime simulationTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(country);

        var reports = await ProcessDueQuartersAsync(country, simulationTime, cancellationToken).ConfigureAwait(false);
        if (reports.Count > 0)
        {
            return reports[^1];
        }

        if (country.LatestFiscalReport != null)
        {
            return country.LatestFiscalReport;
        }

        return new FiscalQuarterReport
        {
            CountryId = country.Id,
            QuarterIndex = FiscalSchedule.GetQuarterIndex(simulationTime),
            IncomeTaxRevenue = 0.0,
            CorporateTaxRevenue = 0.0,
            TariffRevenue = 0.0,
            TotalTaxRevenue = 0.0,
            TotalExpenditures = 0.0,
            UpdatedTreasuryBalance = country.Treasury.Balance,
            Timestamp = simulationTime
        };
    }

    public async Task<IReadOnlyList<FiscalQuarterReport>> ProcessDueQuartersAsync(
        Country country,
        RepublicTime simulationTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (!double.IsFinite(country.GrossDomesticProduct) || country.GrossDomesticProduct <= 0.0)
        {
            throw new ArgumentException("Country GDP must be a positive finite amount.", nameof(country));
        }

        var policy = country.TaxPolicy ?? GetTaxPolicy();
        if (!double.IsFinite(policy.IncomeTaxRate) || !double.IsFinite(policy.CorporateTaxRate) || !double.IsFinite(policy.ImportTariffRate))
        {
            throw new ArgumentException("Tax policy rates must be finite numbers.", nameof(country));
        }

        if (policy.IncomeTaxRate < 0.0 || policy.IncomeTaxRate > 0.70 ||
            policy.CorporateTaxRate < 0.0 || policy.CorporateTaxRate > 0.60 ||
            policy.ImportTariffRate < 0.0 || policy.ImportTariffRate > 0.50)
        {
            throw new ArgumentException("Tax policy rates exceed constitutional limits.", nameof(country));
        }

        var budget = country.MinistryBudget ?? GetMinistryBudget();
        if (!double.IsFinite(budget.TotalQuarterlyExpenditures) || budget.TotalQuarterlyExpenditures < 0.0)
        {
            throw new ArgumentException("Ministry expenditures must be finite and non-negative.", nameof(country));
        }

        var dueQuarters = FiscalSchedule.GetDueQuarters(country, simulationTime);
        if (dueQuarters.Count == 0)
        {
            return Array.Empty<FiscalQuarterReport>();
        }

        var gdp = country.GrossDomesticProduct;
        var incomeTaxRev = gdp * 0.15 * policy.IncomeTaxRate;
        var corpTaxRev = gdp * 0.10 * policy.CorporateTaxRate;
        var tradeBalance = double.IsFinite(country.TradeBalance) ? country.TradeBalance : 0.0;
        var tariffRev = Math.Max(0.0, tradeBalance) * policy.ImportTariffRate;
        var totalTaxRevenue = incomeTaxRev + corpTaxRev + tariffRev;
        var totalExpenditures = budget.TotalQuarterlyExpenditures;

        var currentQuarter = FiscalSchedule.GetQuarterIndex(simulationTime);
        var reports = new List<FiscalQuarterReport>(dueQuarters.Count);

        foreach (var quarterIndex in dueQuarters)
        {
            var quarterTime = (quarterIndex == currentQuarter)
                ? simulationTime
                : FiscalSchedule.GetQuarterStartTime(quarterIndex, simulationTime.LaunchEpochWat);

            var evaluatedReport = new FiscalQuarterReport
            {
                CountryId = country.Id,
                QuarterIndex = quarterIndex,
                IncomeTaxRevenue = incomeTaxRev,
                CorporateTaxRevenue = corpTaxRev,
                TariffRevenue = tariffRev,
                TotalTaxRevenue = totalTaxRevenue,
                TotalExpenditures = totalExpenditures,
                UpdatedTreasuryBalance = country.Treasury.Balance,
                Timestamp = quarterTime
            };

            var finalReport = country.ApplyFiscalQuarter(evaluatedReport);

            // Keep world manager economic indicators synchronized if this is the player country
            var playerCountry = _worldManager.Countries.GetCountry("player-country") ?? _worldManager.Countries.GetAllCountries().FirstOrDefault();
            if (playerCountry != null && playerCountry.Id == country.Id)
            {
                var ind = _worldManager.Economic.GetIndicators();
                ind.TreasuryBalance = country.Treasury.Balance;
                ind.GrossDomesticProduct = country.GrossDomesticProduct;
            }

            _logger?.LogInfo($"Q{finalReport.QuarterIndex} Fiscal Report for {country.Name}: Rev ({finalReport.FormattedTotalTaxRevenue}) - Exp ({finalReport.FormattedTotalExpenditures}) = Net ({finalReport.FormattedNetSurplusOrDeficit}). Treasury: {country.Treasury.FormattedBalance}");
            await _eventBus.PublishAsync(new QuarterlyFiscalReportGeneratedEvent(finalReport, quarterTime.WatTimestamp), cancellationToken).ConfigureAwait(false);

            _workspaceManager?.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Ministry of Finance",
                Subject = $"FISCAL REPORT: Q{finalReport.QuarterIndex} Financial Summary - {country.Name}",
                Body = $"Quarter {finalReport.QuarterIndex} report generated for {country.Name}. Revenue: {finalReport.FormattedTotalTaxRevenue}, Expenditure: {finalReport.FormattedTotalExpenditures}. Net: {finalReport.FormattedNetSurplusOrDeficit}. Treasury Balance: {country.Treasury.FormattedBalance}.",
                Folder = "Inbox",
                ActionRequired = false
            });

            reports.Add(finalReport);
        }

        return reports.AsReadOnly();
    }
}

