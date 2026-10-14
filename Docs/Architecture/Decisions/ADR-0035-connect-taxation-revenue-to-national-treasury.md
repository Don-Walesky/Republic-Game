# ADR-0035: Connect Taxation Revenue to the National Treasury

## Status
Accepted

## Context
Phase 11 ([ADR-0034](ADR-0034-citizen-welfare-and-approval-feedback.md)) implemented domestic taxation policy decisions and their citizen welfare, public approval, and societal stability consequences. However, the economic loop was incomplete: while tax rates could be enacted, tax revenue and ministry expenditure outcomes were not deposited into or deducted from the sovereign country's treasury (`Country.Treasury` / `RepuTreasury`).

Prior to Phase 12:
- `BudgetService.ProcessQuarterlyReportAsync(ulong currentTick)` operated exclusively against legacy global indicators on `_worldManager.Economic`, depositing surplus or deducting deficit from `_indicators.TreasuryBalance` in `$`.
- Individual sovereign countries (`Country.cs`) maintained their own liquid REPU treasury (`Country.Treasury`), Gross Domestic Product, and enacted tax rates, but had no quarterly fiscal processing mechanism.
- The global economic indicators and sovereign nation treasuries diverged, leaving the Presidential Desk unable to reflect fiscal tax outcomes.
- There was no mechanism ensuring once-and-only-once execution of quarterly fiscal transactions on sovereign country treasuries, nor protection against invalid/non-finite financial figures corrupting treasury state.

## Decision

1. **Reuse Established Fiscal Formulas**:
   No new multipliers, arbitrary bonuses, or parallel taxation engines are introduced. We reuse the exact established domain rules:
   - **Personal Income Tax Revenue**: `GrossDomesticProduct * 0.15 * IncomeTaxRate`
   - **Corporate Tax Revenue**: `GrossDomesticProduct * 0.10 * CorporateTaxRate`
   - **Import Tariff Revenue**: `Math.Max(0.0, TradeBalance) * ImportTariffRate`
   - **Total Quarterly Tax Revenue**: `IncomeTaxRevenue + CorporateTaxRevenue + TariffRevenue`
   - **Total Quarterly Expenditures**: Sum of ministry appropriations (`MinistryBudget.TotalQuarterlyExpenditures`)
   - **Net Surplus or Deficit**: `TotalTaxRevenue - TotalExpenditures`

2. **Domain Model Extensions**:
   - Enhanced [`FiscalQuarterReport`](../../../src/Republic.Core/Economy/Budget/Models/FiscalQuarterReport.cs):
     - Added `CountryId`, `IncomeTaxRevenue`, `CorporateTaxRevenue`, `TariffRevenue`, `Timestamp` (`RepublicTime?`).
     - Added standard REPU formatted string helpers: `FormattedTotalTaxRevenue`, `FormattedTotalExpenditures`, `FormattedNetSurplusOrDeficit`, `FormattedUpdatedTreasuryBalance`.
     - Preserved existing properties (`QuarterIndex`, `TotalTaxRevenue`, `TotalExpenditures`, `NetSurplusOrDeficit`, `UpdatedTreasuryBalance`).
   - Enhanced [`Country`](../../../src/Republic.Core/World/Models/Country.cs):
     - Added `MinistryBudget MinistryBudget { get; set; } = new();` (allocations: Defense 500M, Health 400M, Education 350M, Infra 600M, Science 250M).
     - Added `double TradeBalance { get; set; } = 2_100_000_000.0;`.
     - Added `FiscalQuarterReport? LatestFiscalReport { get; set; }`.
     - Added `int LastProcessedFiscalQuarter { get; set; }`.
     - Added `ApplyFiscalQuarter(FiscalQuarterReport report)`:
       - Validates finite numbers; rejects non-finite (NaN, Infinity).
       - Enforces deduplication via `Treasury.HasAppliedSnapshot($"fiscal-quarter-{Id}-Q{report.QuarterIndex}")` and `LastProcessedFiscalQuarter`.
       - If surplus: deposits via `Treasury.Deposit(net)`.
       - If deficit: withdraws available liquid funds without causing negative overdraft (`Treasury.Withdraw(...)`).
       - Records transaction identifier in `Treasury.RecordSnapshotApplied(...)`.
       - Updates `LatestFiscalReport` and `LastProcessedFiscalQuarter`.
     - Updated `Country.Found` factory method with optional parameters preserving full backward compatibility.

3. **Country-Aware Fiscal Settlement in BudgetService**:
   - Enhanced [`IBudgetService`](../../../src/Republic.Core/Economy/Budget/Services/IBudgetService.cs) and [`BudgetService`](../../../src/Republic.Core/Economy/Budget/Services/BudgetService.cs):
     - Added `Task<FiscalQuarterReport> ProcessQuarterlyReportAsync(Country country, RepublicTime simulationTime, CancellationToken cancellationToken = default);`.
     - Reads authoritative `country.TaxPolicy`, `country.GrossDomesticProduct`, `country.TradeBalance`, and `country.MinistryBudget`.
     - Validates inputs safely (positive finite GDP, finite rates within constitutional limits, non-negative finite expenditures).
     - Generates the evaluated `FiscalQuarterReport` for the next quarter index (`country.LastProcessedFiscalQuarter + 1`).
     - Executes `country.ApplyFiscalQuarter(evaluatedReport)`, updating the sovereign treasury balance.
     - Synchronizes `_worldManager.Economic` indicators if the country is the player country, preventing indicator divergence.
     - Publishes `QuarterlyFiscalReportGeneratedEvent` and delivers a financial summary email to the Presidential inbox.
     - Preserves the legacy `ProcessQuarterlyReportAsync(ulong currentTick)` method for tests without registered countries.

4. **Executive Workspace Coordination**:
   - Enhanced [`PresidentialWorkspaceHandler`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs):
     - Added `FiscalQuarterReport? FiscalReport` to `ExecutiveActionOutcome`.
     - Implemented `ProcessFiscalQuarterAsync(Country country, IBudgetService budgetService, RepublicTime simulationTime)` providing a single executive entry point for quarterly fiscal settlements.

5. **Presidential Desk & Read Model Reflection**:
   - Enhanced [`NationalDashboard`](../../../src/Republic.Core/World/Models/NationalDashboard.cs):
     - Added `Fiscal` overview line (e.g. `Q1 | Rev: R25.9B | Exp: R2.1B | Net: R23.8B`) and `LatestFiscalReport` property.
     - Included `Fiscal: {Fiscal}` in read-only `Lines`.
     - `Treasury` line automatically reflects the updated balance from `Country.Treasury.FormattedBalance`.
   - Updated [`Program.cs`](../../../src/Republic.Cli/Program.cs):
     - Displayed `Fiscal Quarter Status` in `RenderDashboard`.
     - Updated option `[4]` (`ManageBudgetAsync`) to display projected quarterly tax revenues, ministry expenditures, and net surplus/deficit alongside citizen welfare sentiment.

6. **Economic Integrity & Security**:
   - **Isolated Treasuries**: Processing Country A does not mutate Country B's treasury, fiscal report, or quarter index.
   - **Deduplication**: Re-running quarterly processing for an already processed quarter returns the cached report without re-crediting or re-debiting funds.
   - **Non-Negative Constraints**: Liquid REPU balance cannot fall below zero. In deficit conditions where expenditures exceed treasury reserves, available funds are consumed safely without negative overdraft or crash.
   - **Non-Finite Safety**: NaN or infinite numbers are rejected prior to any mutation.
   - **Simulation Clock Authority**: All fiscal timestamps are governed by `RepublicTime`, with no wall-clock timing dependencies.
   - **Persistence**: `SaveGameManager` JSON serialization preserves `Treasury`, `MinistryBudget`, `TradeBalance`, `LatestFiscalReport`, and `LastProcessedFiscalQuarter`.

## Consequences
- The taxation-to-treasury economic loop is complete: tax rates set by domestic policy govern revenue generation, ministry budgets determine expenditures, and the resulting surplus or deficit settles into the authoritative sovereign treasury.
- Public approval rules from Phase 11 and fiscal treasury changes from Phase 12 operate in full harmony: tax hikes produce more revenue but sacrifice approval; tax cuts boost citizen approval but reduce revenue.
- Comprehensive test suite: 635 tests pass (624 baseline + 11 new Phase 12 tests) with zero regressions across National Yield, resource production, diplomacy, projects, and persistence.
