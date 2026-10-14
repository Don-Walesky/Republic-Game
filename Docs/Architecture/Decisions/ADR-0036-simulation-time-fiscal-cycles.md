# ADR-0036: Reliable Simulation-Time Fiscal Cycles

## Status
Accepted

## Context
Phase 12 ([ADR-0035](ADR-0035-connect-taxation-revenue-to-national-treasury.md)) connected country-level taxation policy and ministry expenditures to sovereign treasuries (`Country.Treasury` / `RepuTreasury`).

However, repository audit identified a critical reliability defect:
- In Phase 12, quarter indexing was naive: `targetQuarter = country.LastProcessedFiscalQuarter + 1`. This meant quarter selection was driven by call frequency rather than Republic's authoritative simulation clock (`RepublicTime`). Calling the service multiple times on the same simulation day erroneously incremented the quarter number.
- There was no domain scheduling abstraction to map simulation days to fiscal quarters or calculate due quarter boundaries across time advancement.
- If simulation time advanced across multiple quarters (e.g. 180 or 270 days), only a single quarter would be evaluated on an ad-hoc call, skipping intermediate quarters.
- Pre-founding quarters for newly established countries were not formally prevented from being settled.
- Event publication and report generation utilized wall-clock timestamps (`DateTimeOffset.UtcNow`) instead of the authoritative `WatTimestamp` (+01:00) from `RepublicTime`.
- Legacy `currentTick` fallbacks relied on unanchored arbitrary dates.

## Decision

1. **Authoritative Simulation-Time Fiscal Schedule (`FiscalSchedule.cs`)**:
   We introduced [`FiscalSchedule`](../../../src/Republic.Core/Economy/Budget/Services/FiscalSchedule.cs) to govern fiscal cycle boundaries anchored strictly to West Africa Time (WAT / UTC+1):
   - **Constant Quarter Length**: 90 simulation days per quarter (`DaysPerQuarter = 90`).
     - Quarter 1: Days 0..89
     - Quarter 2: Days 90..179
     - Quarter 3: Days 180..269
     - Quarter 4: Days 270..359
   - **Deterministic Quarter Indexing**:
     `GetQuarterIndex(RepublicTime time) => (int)(time.DayNumber / 90) + 1;`
   - **First Eligible Quarter Rule**:
     `GetFirstEligibleQuarter(Country country) => country.IsFounded ? GetQuarterIndex(country.FoundingTime) : 1;`
     Pre-founding quarters are never eligible for taxation or expenditure settlement. A nation founded on Day 100 begins eligibility in Quarter 2; Quarter 1 is never charged.
   - **Authoritative Quarter Boundaries**:
     `GetQuarterStartTime(int quarterIndex, DateTimeOffset launchEpochWat)` returns the exact `RepublicTime` boundary at 00:00:00 WAT on Day $(Q - 1) \times 90$.
   - **Due Quarters Resolution**:
     `GetDueQuarters(Country country, RepublicTime simulationTime)` calculates all unsettled quarters between $\max(\text{firstEligible}, \text{LastProcessedFiscalQuarter} + 1)$ and $\text{currentQuarter}$. If none are due, it returns an empty sequence.

2. **Idempotent Quarter Processing & Sequential Catch-Up**:
   - Enhanced [`IBudgetService`](../../../src/Republic.Core/Economy/Budget/Services/IBudgetService.cs) and [`BudgetService`](../../../src/Republic.Core/Economy/Budget/Services/BudgetService.cs):
     - Added `Task<IReadOnlyList<FiscalQuarterReport>> ProcessDueQuartersAsync(Country country, RepublicTime simulationTime, CancellationToken cancellationToken = default);`.
     - When multiple quarters are due (e.g. simulation time advances across 180 days), all due quarters are processed in strict chronological sequence ($Q2, Q3, \dots$).
     - Each due quarter is timestamped with its authoritative boundary time, evaluated against current policy and budgets, and settled into the sovereign treasury.
     - Within the same quarter, repeated invocations are strictly idempotent: `ProcessQuarterlyReportAsync` detects that no quarters are due and returns `country.LatestFiscalReport` directly with zero treasury mutations, zero duplicate events, and zero duplicate notifications.

3. **Authoritative Simulation Timestamping**:
   - Replaced all usages of wall-clock `DateTimeOffset.UtcNow` in `BudgetService`:
     - `QuarterlyFiscalReportGeneratedEvent` publishes with `quarterTime.WatTimestamp` (fixed UTC+1 offset).
     - `TaxPolicyUpdatedEvent` and `MinistryBudgetUpdatedEvent` publish with the active simulation clock `WatTimestamp`.
     - Legacy `ProcessQuarterlyReportAsync(ulong currentTick)` derives simulation time from `_clock.CurrentTime` or `_worldManager.Countries.Clock.CurrentTime`.

4. **Integration with Simulation Time Advancement**:
   - Enhanced [`PresidentialWorkspaceHandler`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs):
     - Updated `AdvanceTime` to accept optional `IBudgetService? budgetService = null`.
     - Added `AdvanceTimeAsync` for asynchronous time advancement.
     - While advancing simulation time across days or weeks, all four simulation cycles are processed across all registered sovereign countries in order:
       1. Six-hour National Yield cycles
       2. Six-hour domestic resource production cycles
       3. Capital development project completions
       4. Due simulation-time fiscal quarter settlements
     - Added `int FiscalQuartersProcessed` to [`TimeAdvancementSummary`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs), preserving backward compatibility.

5. **Deduplication Hardening**:
   - Updated `Country.ApplyFiscalQuarter`: deduplication check enforces `Treasury.HasAppliedSnapshot(transactionKey) || LastProcessedFiscalQuarter >= report.QuarterIndex`, guaranteeing that re-evaluating older or reloaded reports can never re-debit or re-credit liquid treasury balances.

## Consequences
- Fiscal settlements are strictly anchored to simulation time; repeated calls during the same simulation day or quarter produce zero financial side-effects.
- Offline and step-by-step time advancement smoothly and sequentially catches up all due fiscal quarters without skipping cycles or duplicating revenue.
- Pre-founding quarters are preserved as uncharged; foreign countries remain strictly isolated.
- Full test suite passes: **648 passed, 0 failed, 0 skipped** (635 baseline + 13 new Phase 13 tests).
