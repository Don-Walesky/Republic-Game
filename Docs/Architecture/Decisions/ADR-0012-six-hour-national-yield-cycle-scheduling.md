# ADR-0012: Six-Hour National Yield Cycle Scheduling on the Republic Clock

## Status
Accepted

## Context
Following the establishment of the National Yield domain model ([ADR-0004](ADR-0004-national-yield-domain-model.md)), the isolated calculation engine ([ADR-0007](ADR-0007-isolated-national-yield-calculation-engine.md)), cycle execution ([ADR-0010](ADR-0010-national-yield-cycle-execution.md)), and REPU treasury revenue crediting ([ADR-0011](ADR-0011-repu-treasury-and-yield-revenue.md)), Step 4L establishes the production cycle schedule anchored to the authoritative global Republic clock.

## Decision

1. **Strict 6-Hour Boundary Alignment on Global WAT Clock**:
   - Production cycles operate on six-hour intervals strictly aligned to 00:00, 06:00, 12:00, and 18:00 West Africa Time (WAT / UTC+1).
   - Time calculations use [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs) and [`IRepublicClock`](../../../src/Republic.Core/Time/IRepublicClock.cs) exclusively; real-world system calls like `DateTime.Now` remain prohibited.

2. **Cycle Scheduling Engine (`INationalYieldSchedule`)**:
   - Scheduling logic is encapsulated in [`INationalYieldSchedule`](../../../src/Republic.Core/NationalYield/INationalYieldSchedule.cs) and [`NationalYieldSchedule`](../../../src/Republic.Core/NationalYield/NationalYieldSchedule.cs).
   - Provides pure boundary calculation (`GetNextBoundary`, `GetDueBoundaries`, `GetNextDueBoundary`) without side effects, answering which boundaries are due so downstream systems (such as 4K offline briefings) can inspect them without mutating state.

3. **Per-Country Due Time and State Isolation**:
   - The simulation clock is shared globally, but country state is completely isolated.
   - Each sovereign country independently tracks its last credited boundary via [`Country.LastCreditedBoundary`](../../../src/Republic.Core/World/Models/Country.cs).
   - When Country A advances its simulation timeline and receives yield credits, Country B's treasury, snapshots, and schedule state remain strictly untouched.

4. **Once-Only Boundary Crediting and Idempotency**:
   - Cycle revenue may be credited only when simulation time has reached or passed that country's next boundary.
   - Handled via `ExecuteDueCycles` in [`NationalYieldCycleService`](../../../src/Republic.Core/NationalYield/NationalYieldCycleService.cs).
   - The credited snapshot identifier is deterministic (`$"{country.Id}:Day{boundary.DayNumber}:{boundary.Time:HHmmss.fffffff}"`), tying each credit to the country ID and the specific boundary timestamp.
   - The existing once-only credit mechanism in [`RepuTreasury`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) and [`TreasuryRevenueService`](../../../src/Republic.Core/Economy/Treasury/TreasuryRevenueService.cs) prevents double-crediting if the same boundary is executed more than once.
   - Executing at 06:01 when 06:00 was already credited evaluates 0 due boundaries and creates no additional credit.

5. **Founding-Day Rule Preservation**:
   - Founding-day scaling (x10) remains exclusively encapsulated inside [`IFoundingDayYieldRule`](../../../src/Republic.Core/NationalYield/IFoundingDayYieldRule.cs).
   - The scheduler and cycle executor do not re-multiply or apply additional scaling.

6. **8-Boundary Evaluation Cap**:
   - Any single execution or schedule query is capped at a maximum of 8 due boundaries (48 hours of simulation time).
   - Subsequent calls can process remaining intervals if simulation time extends further.

7. **CLI Dashboard Integration**:
   - The executive desk in [`Program.cs`](../../../src/Republic.Cli/Program.cs) surfaces the next due WAT boundary alongside the current REPU balance (e.g., `Treasury: R15B | Next WAT Boundary: Day 0, 06:00 WAT | GDP: ...`).
   - No menu options or directives were added.

8. **Strict Scope Boundaries**:
   - Does not implement 4K offline briefing, 4M additional founding buffs, State Capacity, cabinet effects, development projects, or networking.

## Consequences
- Sovereign nations now progress along deterministic 6-hour production cycles synchronized with the global Republic clock.
- Complete state isolation between countries is preserved.
- Full idempotency and once-only crediting protect treasury integrity across repeated or staggered evaluations.
