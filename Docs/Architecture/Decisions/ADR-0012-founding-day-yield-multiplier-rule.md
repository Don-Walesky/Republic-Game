# ADR-0012: Founding-Day National Yield Multiplier (x10) Domain Rule

## Status
Accepted

## Context
Across Steps 3, 4A–4I, Republic established country founding moments ([ADR-0003](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0003-country-identity-and-founding-state.md)), the 14-category National Yield model ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)), the isolated calculation engine ([ADR-0007](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0007-isolated-national-yield-calculation-engine.md)), cycle execution ([ADR-0010](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0010-national-yield-cycle-execution.md)), and REPU treasury revenue application ([ADR-0011](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0011-repu-treasury-and-yield-revenue.md)).

Step 4J-A introduces the first part of Step 4J: the domain-level rule governing the founding-day National Yield multiplier ($\times 10$).

## Decision

1. **Founding Day Simulation Calendar Boundary**:
   - The founding day is based strictly on the country's actual **Independence Day in the Republic simulation calendar** (West Africa Time / UTC+1), **not** a sliding 24-hour window from creation.
   - A country is in its founding day if and only if:
     $$\text{simulationTime} \ge \text{country.FoundingTime} \quad \land \quad \text{simulationTime.DayNumber} == \text{country.FoundingRepublicDay}$$
   - When the simulation clock crosses 00:00:00 WAT into the next calendar day (`DayNumber > FoundingRepublicDay`), the founding day ends regardless of elapsed real-world time.

2. **Applied to Calculated Output Without Calculator Modification**:
   - The $\times 10$ multiplier scales the evaluated [`NationalYield`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYield.cs) output across all 14 canonical categories.
   - It does **not** modify or compromise the pure, stateless calculation formulas inside [`NationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculator.cs).
   - The input yield result remains unmutated; a new scaled instance is allocated via `yield.Multiply(10.0)`.

3. **No Direct Treasury Modification**:
   - The rule strictly produces scaled National Yield output.
   - It does **not** grant arbitrary free REPU balance and does **not** directly credit or mutate [`RepuTreasury`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Economy/Treasury/RepuTreasury.cs).

4. **Domain Rule Encapsulation**:
   - Encapsulated via [`IFoundingDayYieldRule`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/IFoundingDayYieldRule.cs) and [`FoundingDayYieldRule`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/FoundingDayYieldRule.cs) in `Republic.Core.NationalYield`.
   - Allows future simulation pipelines and cycle services to query or apply the multiplier cleanly without duplicating rule logic.

5. **Country Isolation**:
   - Founding day identification is strictly sovereign to the individual country. Country A experiencing its founding day has zero effect on Country B.

6. **Intentional Scope Boundaries**:
   - **No Yield Cycle Scheduling**: Not yet wired to automatic background timers or recurring cycles.
   - **No Offline Progression**: Deferrals preserved for subsequent phases.
   - **No Additional Buffs**: Other temporary founding-day modifiers remain deferred.

## Consequences
- The simulation now possesses a pure domain rule identifying Independence Day in the simulation calendar and scaling evaluated National Yield output by $\times 10$.
