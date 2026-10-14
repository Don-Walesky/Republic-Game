namespace Republic.Core.Economy.Budget.Services;

using Republic.Core.Economy.Budget.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface managing tax rates, ministry appropriations, and quarterly fiscal reports.
/// </summary>
public interface IBudgetService
{
    TaxPolicy GetTaxPolicy();
    MinistryBudget GetMinistryBudget();
    Task UpdateTaxPolicyAsync(TaxPolicy policy, CancellationToken cancellationToken = default);
    Task UpdateMinistryBudgetAsync(MinistryBudget budget, CancellationToken cancellationToken = default);
    Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(ulong currentTick, CancellationToken cancellationToken = default);
    Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(Country country, RepublicTime simulationTime, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FiscalQuarterReport>> ProcessDueQuartersAsync(Country country, RepublicTime simulationTime, CancellationToken cancellationToken = default);
}

