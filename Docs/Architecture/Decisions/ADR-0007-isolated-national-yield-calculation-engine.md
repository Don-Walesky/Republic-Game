# ADR-0007: Isolated National Yield Calculation Engine

## Status
Accepted

## Context
Following the specification established in [ADR-0006](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0006-national-yield-calculation-rules.md), Step 4D creates the first operational, isolated National Yield calculation component: [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs) and [`NationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculator.cs).

## Decision

1. **Pure Component Contract**:
   - `NationalYield Calculate(NationalYieldCalculationInputs inputs)`: Pure function.
   - Zero mutation of the input parameter, [`Country`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/Models/Country.cs), or [`WorldState`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/WorldState.cs).
   - Zero global mutable state, zero I/O, zero database access, zero networking.
   - Zero random number generation (`System.Random` prohibited) and zero system time dependencies.

2. **Minimal Deterministic Baseline Model**:
   - Implements the 6-stage pipeline from ADR-0006:
     1. *Input Snapshotting & Resolution*: Stable ordinal sorting of modifiers by `Id`.
     2. *Base Potential Evaluation*: Derives structural potential from available input factors.
     3. *Cross-Category Upstream Relationships*: Propagates transparent upstream catalysts (Science $\rightarrow$ Innovation, Energy/Infrastructure $\rightarrow$ Industry).
     4. *State Capacity / Governance Conversion*: Evaluates institutional efficiency from Government Effectiveness, Political Stability, and Cabinet leadership.
     5. *National & Category Modifiers*: Evaluates category-targeted and universal modifiers additively with non-negativity clamping ($\ge 0.0$).
     6. *Final Output Resolution*: Returns an independent, newly allocated [`NationalYield`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYield.cs) containing all 14 canonical categories.

3. **Current Limitations & Future Calibration**:
   - **First Operational Engine**: This is the minimal foundational calculation engine, not the final game-balanced economic model.
   - **Baseline Placeholders**: Categories awaiting richer domain models (e.g. detailed mineral extraction, military command structures, diplomatic treaty nets) use transparent baselines derived from existing inputs.
   - **No Yield Cycles or REPU Accumulation**: Time-based cycles, treasury accumulation, and deposits into player balances remain strictly outside this component.
   - **No Founding $\times 10$ Bonus**: Lifecycle multipliers belong to future lifecycle steps.
   - **Food Exclusion Preserved**: Food is strictly an output of the wider civilian economy and is not calculated or deposited by this engine.

## Consequences
- The simulation now possesses a working, pure calculation service that converts national inputs into evaluated capacities.
- Future steps can introduce richer domain models, yield cycles, and calibration curves without altering the isolated calculation contract.
