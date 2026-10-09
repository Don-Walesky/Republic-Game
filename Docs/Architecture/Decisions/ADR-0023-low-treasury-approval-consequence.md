# ADR-0023: Low-Treasury Public Approval Consequence

## Status
Accepted

## Context
Following the implementation of the sovereign country profile ([ADR-0019](ADR-0019-readable-country-profile.md)), the read-only national dashboard ([ADR-0020](ADR-0020-read-only-national-dashboard.md)), and comparable national strength ([ADR-0021](ADR-0021-comparable-national-strength-score.md)), Phase 5 item 5.4 introduces Republic's first systemic economic consequence: the low-treasury approval consequence.

In sovereign governance, public confidence is tied to sovereign fiscal solvency. A national government operating with inadequate liquid reserves experiences a decline in citizen trust and political approval.

## Decision

1. **Rule Specification**:
   - At each six-hour production cycle boundary (00:00, 06:00, 12:00, 18:00 WAT on the Republic simulation clock), the sovereign country's treasury balance is evaluated.
   - If the treasury balance is strictly below **R100,000,000** (`100M`), public approval drops by **2.0 points**, clamped at a minimum of `0.0`.
   - The drop occurs exactly once per sovereign country per cycle boundary. A second evaluation call for the same boundary is idempotent and performs no action.
   - A treasury balance of **R100,000,000 or greater** changes nothing (0 points dropped).
   - Sovereign country isolation is strictly preserved: evaluating consequences for Country A induces zero mutations or side-effects on Country B.
   - Grounded strictly in Republic simulation clock boundaries; never calls `DateTime.Now` or `DateTime.UtcNow`.

2. **Integration into the Due-Cycle Path**:
   - Implemented via [`TreasuryConsequence`](../../../src/Republic.Core/Economy/Treasury/TreasuryConsequence.cs) and [`ITreasuryConsequenceService`](../../../src/Republic.Core/Economy/Treasury/ITreasuryConsequenceService.cs).
   - Executed inside the existing due-cycle path ([`NationalYieldCycleService.ExecuteDueCycles`](../../../src/Republic.Core/NationalYield/NationalYieldCycleService.cs)) immediately **after** the periodic yield revenue credit ([`TreasuryRevenueService.ApplyRevenue`](../../../src/Republic.Core/Economy/Treasury/TreasuryRevenueService.cs)).
   - Evaluates the final accumulated treasury balance inclusive of the freshly credited yield. If the incoming yield revenue raises the treasury balance to R100M or higher, the consequence penalty is avoided.

3. **Boundary Idempotency**:
   - Each sovereign [`Country`](../../../src/Republic.Core/World/Models/Country.cs) records processed cycle boundaries in `AppliedConsequenceBoundaryIds` using deterministic boundary identifiers (`{CountryId}:Day{DayNumber}:{Time:HHmmss}`).
   - Once a boundary identifier is recorded for a country, subsequent evaluations for that boundary return `null` and do not re-apply the penalty.

4. **CLI Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the National Strength block.
   - Displays the formatted latest consequence description, or `"No consequence"` if none has applied.

5. **Scope Boundaries**:
   - Strictly confined to Phase 5 item 5.4 (one treasury consequence).
   - Does not implement elections, political parties, diplomacy, science, or military systems.
   - Does not rewrite National Yield, State Capacity, the dashboard, or National Strength.

## Consequences
- Establishes Republic's first systemic feedback loop where treasury solvency directly impacts public sentiment.
- Maintains strict temporal determinism, idempotency, and country isolation.
- Seamlessly integrates into offline catch-up and scheduled cycle advancement without duplicate math.
