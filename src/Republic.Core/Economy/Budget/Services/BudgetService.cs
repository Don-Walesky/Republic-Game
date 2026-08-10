namespace Republic.Core.Economy.Budget.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Economy.Budget.Events;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Events;
using Republic.Core.World;
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
    private readonly object _lock = new();
    private int _quarterCounter;

    public BudgetService(
        IWorldManager worldManager,
        IEventBus eventBus,
        IWorkspaceManager? workspaceManager = null,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;
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
        await _eventBus.PublishAsync(new TaxPolicyUpdatedEvent(_taxPolicy, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
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
        await _eventBus.PublishAsync(new MinistryBudgetUpdatedEvent(_ministryBudget, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
    }

    public async Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
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
            TotalTaxRevenue = totalTaxRevenue,
            TotalExpenditures = totalExpenditures,
            UpdatedTreasuryBalance = updatedTreasury
        };

        _logger?.LogInfo($"Q{_quarterCounter} Fiscal Report: Rev (${totalTaxRevenue:N0}) - Exp (${totalExpenditures:N0}) = Net (${netSurplusDeficit:N0})");
        await _eventBus.PublishAsync(new QuarterlyFiscalReportGeneratedEvent(report, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

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
}
