# ADR-0021: Comparable National Strength Score

## Status
Accepted

## Context
Following the establishment of the sovereign country profile ([ADR-0019](ADR-0019-readable-country-profile.md)) and the consolidated read-only national dashboard ([ADR-0020](ADR-0020-read-only-national-dashboard.md)), Phase 5 item 5.3 introduces a normalized, comparable **National Strength** scoring model.

Simulation observers and executive players require an objective, normalized basis to compare state capacity and resilience across sovereign nations. Rather than creating new isolated simulations or ungrounded scores, the model synthesizes six core dimensions into normalized values from 0.0 to 1.0, paired with an aggregate total score.

## Decision

1. **NationalStrength Value Object**:
   - Implemented as [`NationalStrength`](../../../src/Republic.Core/World/Models/NationalStrength.cs).
   - Evaluated directly from a sovereign [`Country`](../../../src/Republic.Core/World/Models/Country.cs) instance.
   - Computes six normalized scores (`0.0` to `1.0`), plus a composite total:
     - **Economic**: Liquid treasury balance from [`RepuTreasury.Balance`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) divided by R1,000,000,000 (`1B`), clamped at 1.0. R0 yields 0.0, and balances of R1B or greater yield 1.0.
     - **Institutional**: The sovereign country's evaluated [`StateCapacityScore`](../../../src/Republic.Core/World/Models/Country.cs) from [`StateCapacityEvaluator`](../../../src/Republic.Core/NationalYield/StateCapacityEvaluator.cs).
     - **Infrastructure**: The institutional condition input from [`StateCapacityInputs.InfrastructureCondition`](../../../src/Republic.Core/NationalYield/StateCapacityInputs.cs).
     - **Human Capital**: The temporary institution-building founding buff factor [`FoundingBuffSnapshot.InstitutionBuilding`](../../../src/Republic.Core/NationalYield/FoundingBuffSnapshot.cs) if greater than 0.0; otherwise falls back to the civil service quality input [`StateCapacityInputs.CivilServiceQuality`](../../../src/Republic.Core/NationalYield/StateCapacityInputs.cs).
     - **Diplomatic**: Strictly `0.0` (0%). Diplomacy remains unestablished in this phase.
     - **Military**: Strictly `0.0` (0%). The military system remains unestablished in this phase.
     - **Total**: The unweighted arithmetic average of the six individual scores: `(Economic + Institutional + Infrastructure + HumanCapital + Diplomatic + Military) / 6.0`.

2. **Strict Purity and Isolation**:
   - Reading or evaluating strength is strictly side-effect-free: treasury balances, project queues, ministerial appointments, and yield snapshots are unmutated.
   - Scoring Country A has zero side-effects on Country B.
   - Temporal independence: never calls `DateTime.Now`. Grounded strictly in simulation state.

3. **CLI Presentation**:
   - Rendered as formatted percentages directly beneath the National Dashboard block in [`Program.cs`](../../../src/Republic.Cli/Program.cs).
   - Preserves all existing dashboard, profile, and Republic clock lines.
   - Does not introduce new executive directive menu numbers.

4. **Scope Boundaries**:
   - Strictly confined to Phase 5 item 5.3 (Comparable National Strength Score).
   - Does not implement 5.4 consequences, elections, diplomacy, science, or military systems.

## Consequences
- Provides a clean, objective metric for comparing national power and administrative readiness.
- Reuses authoritative state capacity and treasury state without introducing duplicative simulation layers.
- Formats clearly for both programmatic consumption and CLI user experience.
