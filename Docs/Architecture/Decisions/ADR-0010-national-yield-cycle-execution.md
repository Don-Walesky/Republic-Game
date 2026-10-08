# ADR-0010: National Yield Cycle Execution

## Status
Accepted

## Context
Following the establishment of the National Yield domain model ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)), the calculation input contract ([ADR-0005](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0005-national-yield-calculation-inputs.md)), the isolated calculation engine ([ADR-0007](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0007-isolated-national-yield-calculation-engine.md)), and the numerical calibration framework ([ADR-0009](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0009-national-yield-numerical-calibration-framework.md)), Step 4H introduces the execution mechanism for evaluating sovereign National Yield at a specific point in simulation time.

## Decision

1. **Separation of Calculation, Domain Rules, and Execution**:
   - The [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs) remains a pure, stateless calculation function. It does not know about simulation time or sovereign identity.
   - The [`IFoundingDayYieldRule`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/IFoundingDayYieldRule.cs) remains the single source of truth for the founding-day $\times 10$ decision.
   - The [`INationalYieldCycleService`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCycleService.cs) is the execution mechanism: at simulation time $T$, it accepts a [`Country`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/Models/Country.cs), derives its current in-memory [`NationalYieldCalculationInputs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculationInputs.cs), delegates pure evaluation to [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs), passes the completed result through [`IFoundingDayYieldRule`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/IFoundingDayYieldRule.cs), and produces an independent [`NationalYieldSnapshot`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldSnapshot.cs).

2. **Snapshot Integrity and Defensive Copying**:
   - [`NationalYieldSnapshot`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldSnapshot.cs) ensures that callers cannot mutate the service's stored cache by modifying a returned snapshot or its [`NationalYield`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYield.cs) object.
   - The service stores an independent snapshot instance, and all retrieval methods return defensive clones.
   - All 14 canonical National Yield categories are fully preserved across defensive copies.

3. **Simulation-Time Semantics (Current State Evaluation)**:
   - Cycle execution accepts an explicit [`RepublicTime`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Time/RepublicTime.cs) or [`IRepublicClock`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Time/IRepublicClock.cs).
   - The supplied simulation timestamp identifies the time recorded for the calculation.
   - Crucially, the cycle calculates from the country's *current in-memory state* at execution time; it does **not** reconstruct historical country state, replay past events, or predict future states based on the timestamp.
   - The service and calculator never query `DateTime.Now`, `DateTime.UtcNow`, or the physical system clock.

4. **Country Isolation**:
   - Cycle execution evaluates one country at a time without reading or mutating any other country's state.
   - Snapshots are stored per country identifier in the service instance, avoiding global mutable state or cross-country cache pollution.

5. **Intentional Scope Boundaries**:
   - **No Yield Accumulation**: Yield calculation is strictly kept distinct from yield accumulation; liquid treasury balances, REPU wallets, founding multipliers, offline rewards, and scheduled intervals belong to subsequent economic phases.
   - **No Historical State Replay**: No historical state storage or backward time-travel reconstruction is introduced.

## Consequences
- The simulation can now calculate and snapshot any country's current national yield at any point in simulation time deterministically, safely, and reproducibly without risking cached snapshot corruption.
