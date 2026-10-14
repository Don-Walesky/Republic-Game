# ADR-0034: First Citizen Welfare and Public Approval Feedback Loop

## Status
Accepted

## Context
Following Phase 10 ([ADR-0033](ADR-0033-presidential-executive-workspace.md)), the Presidential Executive Workspace allows the player to review their nation, initiate capital infrastructure projects, and advance simulation time. However, domestic governance lacked a closed, transparent feedback loop where policy decisions directly produce tangible consequences on citizen welfare, public approval sentiment, and national stability.

In the Republic domain, taxation directly affects citizen households:
- Income taxation reduces disposable income available for private consumption, family welfare, and savings.
- Changes in taxation rate must produce direct, predictable public approval and citizen satisfaction responses without resorting to arbitrary random generators or hidden multipliers.
- Extreme taxation rates induce societal friction and unrest, threatening national baseline stability.
- The Presidential Desk and national dashboard must transparently reflect current taxation policy, public approval, and explanatory citizen sentiment feedback.
- The decision workflow must support projection, explicit presidential confirmation (`[Y/N]`), cancellation ergonomics, idempotency, foreign country isolation, and persistence.

## Decision

1. **Domain Feedback Rules (Smallest Defensible Rule Set)**:
   - **Baseline Taxation Standards**: Standard personal income tax is 25% (`0.25`), corporate tax is 20% (`0.20`), and import tariff is 5% (`0.05`).
   - **Constitutional Limits**: Personal income tax is constrained to [0.0%, 70.0%], corporate tax to [0.0%, 60.0%], and import tariffs to [0.0%, 50.0%]. Non-finite inputs (NaN, Infinity) or negative values are strictly rejected.
   - **Public Approval Response**:
     - Income tax adjustments directly modify citizen disposable income and satisfaction.
     - Tax hike (> previous rate): Public approval drops by `-0.5` points per `+0.01` (+1.0%) tax rate increase.
     - Tax relief (< previous rate): Public approval rises by `+0.5` points per `-0.01` (-1.0%) tax rate decrease.
     - Public approval is clamped between `0.0%` and `100.0%`.
   - **Extreme Taxation Stability Threshold**:
     - When personal income tax exceeds 50% (`0.50`), intense economic pressure and societal friction inflict a `-2.0%` penalty on `Country.BaselineStability`.
     - When income tax is repealed from `> 50%` back down to `<= 50%`, the `-2.0%` stability penalty is restored (`+2.0%`).
     - Adjustments entirely within extreme territory (e.g. 55% to 60%) do not duplicate the stability penalty.
   - **Idempotency**:
     - Enacting or re-evaluating the identical tax rate applies zero approval change (`0.0%`) and zero stability change (`0.0%`), maintaining citizen sentiment.

2. **Domain Model Extensions**:
   - Created [`CitizenFeedbackRecord`](../../../src/Republic.Core/Economy/Budget/Models/CitizenFeedbackRecord.cs):
     - Records previous and new tax rates (`PreviousIncomeTaxRate`, `NewIncomeTaxRate`, `PreviousCorporateTaxRate`, `NewCorporateTaxRate`).
     - Records tax rate delta, approval delta, before/after approval ratings, stability delta, before/after stability ratings, enactment timestamp, and human-readable explanation.
   - Enhanced [`Country`](../../../src/Republic.Core/World/Models/Country.cs):
     - Added `TaxPolicy TaxPolicy { get; set; }` defaulting to baseline rates.
     - Added `CitizenFeedbackRecord? LatestCitizenFeedback { get; set; }`.
     - Added `ApplyTaxPolicy(TaxPolicy newPolicy, RepublicTime enactedTime)` to execute domain rules and record feedback.
     - Updated `Country.Found` to support optional `TaxPolicy` and `LatestCitizenFeedback` parameters.

3. **Executive Action Coordination**:
   - Enhanced [`PresidentialWorkspaceHandler`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs):
     - Added `InvalidInput` to `ExecutiveActionResult`.
     - Extended `ExecutiveActionOutcome` to carry optional `CitizenFeedbackRecord`.
     - Implemented `AdjustTaxPolicy(Country country, TaxPolicy newPolicy, RepublicTime simulationTime, bool confirmed = true)`:
       - Validates country validity, finite values, and constitutional bounds.
       - If `confirmed == false`, returns `ExecutiveActionResult.Cancelled` with zero changes to tax rates, approval, or stability.
       - If confirmed, applies `country.ApplyTaxPolicy` and returns `ExecutiveActionResult.Success` with detailed outcome diagnostics.

4. **Dashboard & Read Model Transparency**:
   - Enhanced [`NationalDashboard`](../../../src/Republic.Core/World/Models/NationalDashboard.cs):
     - Added `Taxation` line (e.g. `Income: 25% | Corporate: 20%`).
     - Added `CitizenSentiment` description from `LatestCitizenFeedback` (or baseline neutral).
     - Added `Taxation` to read-only `Lines`.
   - Updated Presidential Desk in [`Program.cs`](../../../src/Republic.Cli/Program.cs):
     - Corrected prototype state sync: synchronized `demo.HappinessRating = playerCountry.HappinessRating` to keep sovereign country approval authoritative.
     - Added `Taxation` and `Citizen Sentiment` display in the National Capacity & Yield section.

5. **Interactive Executive Decision Ergonomics**:
   - Overhauled option `[4] Adjust Taxation & Ministry Budget` (`ManageBudgetAsync`):
     - Displays current tax rates, baseline reference, and transparent consequence rules.
     - Provides pre-decision impact projection showing proposed rate, rate delta, projected approval, approval delta, and stability warnings.
     - Prompts for explicit presidential confirmation (`[Y/N]`) or cancellation (`C`).
     - If confirmed, applies domain policy, updates the budget service, syncs demographics, and outputs an executive enactment report.
     - If cancelled or invalid, leaves state 100% untouched.

6. **Persistence & Sovereign Isolation**:
   - Tax policies, approval ratings, stability, and citizen feedback records serialize and deserialize cleanly through `SaveGameManager` JSON persistence.
   - Adjusting domestic taxation for one country has zero side-effects on foreign sovereign countries.

## Consequences

- Republic now possesses its first closed, authoritative citizen welfare and public approval feedback loop.
- The player experiences clear cause-and-effect: raising taxes sacrifices citizen approval to fund national ambitions; lowering taxes pleases citizens and expands disposable income at the expense of potential state revenue.
- Extreme taxation carries a tangible societal stability risk.
- Idempotency and cancellation safety are guaranteed across domain logic and user workflows.
- Full regression verification: 624 tests passed with 0 failures and 0 regressions.
