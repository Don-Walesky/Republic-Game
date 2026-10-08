# ADR-0015: State Capacity as the Conversion Rate from National Conditions into Results

## Status
Accepted

## Context
Following the establishment of National Yield cycle execution ([ADR-0010](ADR-0010-national-yield-cycle-execution.md)), REPU Treasury revenue application ([ADR-0011](ADR-0011-repu-treasury-and-yield-revenue.md)), the founding-day ×10 yield multiplier ([ADR-0012](ADR-0012-founding-day-yield-multiplier-rule.md)), 6-hour production scheduling ([ADR-0012](ADR-0012-six-hour-national-yield-cycle-scheduling.md)), offline catch-up briefing ([ADR-0013](ADR-0013-offline-yield-catch-up-and-presidential-briefing.md)), and temporary tapering founding buffs ([ADR-0014](ADR-0014-temporary-tapering-founding-buffs.md)), Step 4N introduces **State Capacity**.

In sovereign governance, raw national potential (industrial capital, natural resources, demographic size) does not automatically translate into realized revenue or executive achievements. The efficiency with which national conditions convert into tangible results is governed by the state's institutional competence, administrative reach, and integrity. Without adequate State Capacity, sovereign revenue is lost to friction, corruption, infrastructural decay, and bureaucratic bottlenecks.

## Decision

1. **Seven Institutional Inputs**:
   - State Capacity is computed solely from seven governance and institutional factors:
     1. `Government effectiveness`
     2. `Administrative competence`
     3. `Institution quality`
     4. `Corruption control` (where 1.0 represents a clean government and 0.0 represents a fully corrupt state)
     5. `Stability`
     6. `Infrastructure condition`
     7. `Civil service quality`
   - Each input is a normalized factor clamped between 0.0 and 1.0 ([`StateCapacityInputs`](../../../src/Republic.Core/NationalYield/StateCapacityInputs.cs)).

2. **Weakest Institution as Bottleneck (Minimum Rule)**:
   - State Capacity produces a single score from 0.0 to 1.0.
   - The score is strictly the mathematical **minimum** of the seven inputs:
     $$\text{StateCapacity} = \min(\text{GovEff}, \text{AdminComp}, \text{InstQual}, \text{CorrCtrl}, \text{Stability}, \text{InfraCond}, \text{CivilServ})$$
   - The weakest institution is the bottleneck of the state apparatus. Inputs are never averaged.
   - Any missing or null input evaluates strictly to 0.0, never 1.0.

3. **Country State Isolation**:
   - Institutional inputs are stored directly on the sovereign country model ([`Country.StateCapacity`](../../../src/Republic.Core/World/Models/Country.cs)).
   - Each sovereign nation maintains an independent, isolated [`StateCapacityInputs`](../../../src/Republic.Core/NationalYield/StateCapacityInputs.cs) instance.
   - Country B never uses, inherits, or influences Country A's State Capacity score.

4. **Deterministic Purity and Clock Independence**:
   - Evaluation ([`IStateCapacityEvaluator`](../../../src/Republic.Core/NationalYield/IStateCapacityEvaluator.cs) and [`StateCapacityEvaluator`](../../../src/Republic.Core/NationalYield/StateCapacityEvaluator.cs)) uses in-memory country state only.
   - Evaluator methods never invoke real-world time APIs (`DateTime.Now` is strictly prohibited).
   - Produces an immutable, standalone value snapshot ([`StateCapacitySnapshot`](../../../src/Republic.Core/NationalYield/StateCapacitySnapshot.cs)).

5. **Revenue Scaling at Credit Time**:
   - In this slice, State Capacity scales REPU revenue strictly at credit time inside [`TreasuryRevenueService`](../../../src/Republic.Core/Economy/Treasury/TreasuryRevenueService.cs).
   - A score of 0.5 deposits exactly half of that cycle's `RepuTreasuryRevenue`. A score of 1.0 deposits 100%. A score of 0.0 deposits nothing.
   - Applied once, after the founding-day ×10 rule, not before and after.
   - Scaling occurs at credit time and does not alter the underlying snapshot's recorded yield metrics.

6. **Snapshot Identity Preservation & Idempotency**:
   - Crediting relies on the existing snapshot identifier ([`NationalYieldSnapshot.Id`](../../../src/Republic.Core/NationalYield/NationalYieldSnapshot.cs)) and [`RepuTreasury.HasAppliedSnapshot`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) to ensure idempotency.
   - Revenue scaling must not create a new snapshot identity or alter the existing snapshot's identity.
   - Attempting to credit the same snapshot multiple times will still deposit exactly once.

7. **Default Baseline**:
   - Existing and newly founded countries default to 0.6 across all seven inputs (`DefaultTestFactor`), ensuring existing nations are not suddenly paralyzed at 0.0 capacity.

8. **CLI Dashboard Presentation**:
   - The CLI executive dashboard in [`Program.cs`](../../../src/Republic.Cli/Program.cs) presents the State Capacity score as a percentage beside the treasury R balance.
   - No interactive menu options or gameplay directives are added in this slice.

9. **Strict Scope Boundaries**:
   - Does not implement Phase 4O appointments, 4P development projects, diplomacy systems, or networking.
   - Does not change the founding-day ×10 rule or the seven temporary founding buffs.

## Consequences
- Sovereign revenue accrual realistically reflects the institutional maturity and governance integrity of the nation.
- Governance bottlenecks cannot be hidden through high capacity in unrelated sectors.
- Yield snapshots, treasury credit idempotency, and clock purity remain robustly preserved.
