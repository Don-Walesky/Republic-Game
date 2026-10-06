# ADR-0007: Isolated National Yield Calculation Engine

## Status
Accepted

## Context
Following the architectural specification established in [ADR-0006](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0006-national-yield-calculation-rules.md), Step 4D establishes the first operational, isolated National Yield calculation engine: [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs) and [`NationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculator.cs).

## Decision

1. **Pure Component Contract**:
   - `NationalYield Calculate(NationalYieldCalculationInputs inputs)`: Pure function.
   - Zero mutation of the input parameter, [`Country`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/Models/Country.cs), or [`WorldState`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/WorldState.cs).
   - Zero global mutable state, zero I/O, zero database access, zero networking.
   - Zero random number generation (`System.Random` prohibited) and zero system time dependencies.

2. **Minimal Neutral Baseline Implementation**:
   - Step 4D establishes the isolated calculation engine and a minimal neutral baseline implementation:
     - Direct mappings are used where a straightforward conceptual relationship exists:
       - `IndustrialCapacity` $\leftarrow$ `EconomicCapacity`
       - `InfrastructureCapacity` $\leftarrow$ `InfrastructureCondition`
       - `HumanCapital` $\leftarrow$ `HumanCapital`
       - `ScienceCapacity` $\leftarrow$ `ScienceTechnology`
       - `AdministrativeCapacity` $\leftarrow$ `GovernmentEffectiveness`
       - `SecurityCapacity` $\leftarrow$ `SecurityLevel`
     - Categories without direct input mappings currently use an explicit neutral baseline (`0.0`) pending dedicated domain models (e.g. Energy, Financial, Innovation, Intelligence, Military, Diplomatic, Natural Resource, and REPU Treasury Revenue).
     - Modifiers are evaluated additively as a temporary Step 4D baseline, with deterministic ordinal sorting by `Id` and non-negativity clamping ($\ge 0.0$).

3. **Preservation of the Six-Stage Pipeline**:
   - The engine maintains the 6 conceptual stages defined in ADR-0006:
     1. *Input Snapshotting & Resolution*: Stable ordinal sorting of modifiers.
     2. *Base Potential Evaluation*: Direct baseline mappings and neutral defaults.
     3. *Cross-Category Upstream Relationships*: Architectural boundary preserved; values pass through unchanged without premature numerical multipliers.
     4. *State Capacity / Governance Conversion*: Architectural boundary preserved; values pass through unchanged without premature coefficient formulas.
     5. *National & Category Modifiers*: Deterministic modifier application.
     6. *Final Output Resolution*: Returns an independent, newly allocated [`NationalYield`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYield.cs) containing all 14 canonical categories.

4. **Intentional Scope Boundaries & Deferrals**:
   - **No Premature Economic Balancing**: Final economic relationships, State Capacity mathematics, cross-category coefficients, REPU conversion rules, and balancing curves remain intentionally unresolved for subsequent design/calibration work.
   - **No Yield Cycles or REPU Accumulation**: Time-based cycles, treasury accumulation, and financial deposits into player accounts remain strictly outside this component.
   - **No Founding $\times 10$ Bonus**: Lifecycle multipliers belong to future lifecycle steps.
   - **Food Exclusion Preserved**: Food is strictly an output of the wider civilian economy and is not calculated or deposited by this engine.

## Consequences
- The simulation now possesses a working, pure calculation service that converts national inputs into evaluated capacities.
- Future steps can introduce richer domain models, yield cycles, and calibration curves without altering the isolated calculation contract.
