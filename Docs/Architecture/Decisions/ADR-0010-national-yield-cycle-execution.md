# ADR-0010: National Yield Cycle Execution

## Status
Accepted

## Context
Following the establishment of the National Yield domain model ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)), the calculation input contract ([ADR-0005](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0005-national-yield-calculation-inputs.md)), the isolated calculation engine ([ADR-0007](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0007-isolated-national-yield-calculation-engine.md)), and the numerical calibration framework ([ADR-0009](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0009-national-yield-numerical-calibration-framework.md)), Step 4H introduces the execution mechanism for evaluating sovereign National Yield at a specific point in simulation time.

## Decision

1. **Separation of Calculation and Execution**:
   - The [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs) remains a pure, stateless calculation function. It does not know about simulation time or sovereign identity.
   - The [`INationalYieldCycleService`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCycleService.cs) is the execution mechanism: at simulation time $T$, it accepts a [`Country`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/Models/Country.cs), derives its current [`NationalYieldCalculationInputs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculationInputs.cs), delegates pure evaluation to [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs), and produces an immutable [`NationalYieldSnapshot`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldSnapshot.cs).

2. **Simulation Time Authority**:
   - Cycle execution accepts an explicit [`RepublicTime`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Time/RepublicTime.cs) or [`IRepublicClock`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Time/IRepublicClock.cs).
   - The service and calculator never query `DateTime.Now`, `DateTime.UtcNow`, or the physical system clock.

3. **Country Isolation and Stateless Snapshotting**:
   - Cycle execution evaluates one country at a time without reading or mutating any other country's state.
   - Snapshots are stored per country identifier in the service instance, avoiding global mutable state or cross-country cache pollution.

4. **Intentional Scope Boundaries**:
   - **No Yield Accumulation**: Yield calculation is strictly kept distinct from yield accumulation; liquid treasury balances, REPU wallets, founding multipliers, offline rewards, and scheduled intervals belong to subsequent economic phases.

## Consequences
- The simulation can now calculate and snapshot any country's current national yield at any point in simulation time deterministically and reproducibly.
