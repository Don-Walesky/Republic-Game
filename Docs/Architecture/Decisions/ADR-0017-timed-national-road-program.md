# ADR-0017: Timed National Road Program Development Project

## Status
Accepted

## Context
Following the establishment of State Capacity as the conversion rate for sovereign revenue ([ADR-0015](ADR-0015-state-capacity-conversion-rate.md)) and ministerial appointments steering institutional capacity ([ADR-0016](ADR-0016-cabinet-appointments-state-capacity.md)), Phase 4 item 4P introduces the sovereign state's first timed capital development project: the **National Road Program**.

Prior to this decision, infrastructure condition could only be steered through ministerial appointments or static baselines. Real-world state capacity expands through concrete, capital-intensive infrastructure investments that take elapsed time to construct and directly elevate national operational capability.

## Decision

1. **Dedicated Project Record**:
   - The road program is represented by the immutable domain model [`NationalRoadProject`](../../../src/Republic.Core/Economy/Projects/NationalRoadProject.cs).
   - Stored directly on the sovereign country model ([`Country.RoadProject`](../../../src/Republic.Core/World/Models/Country.cs)).
   - Records five core properties:
     - `Id`: Unique project identifier.
     - `StartedBoundary`: Republic simulation timestamp / boundary at which construction begins.
     - `FinishBoundary`: Republic simulation timestamp / boundary at which construction completes.
     - `Cost`: Fixed capital expenditure in liquid REPU (R500,000).
     - `Completed`: Boolean flag indicating completion.

2. **Capital Cost and Treasury Discipline**:
   - Fixed cost of **R500,000**, withdrawn once from that country's sovereign [`RepuTreasury`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) when construction starts.
   - If the treasury has insufficient funds (< R500,000), initiation is rejected and the treasury balance remains strictly unchanged (no deficit spending or negative balances).

3. **Deterministic Timed Duration**:
   - Construction takes **4 six-hour cycle boundaries** (exactly **24 hours**) on the global Republic clock from the start boundary.
   - Purity rule: Progress is tracked strictly using [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs); `DateTime.Now` is strictly prohibited.
   - Finish boundary is calculated deterministically as `StartedBoundary.Add(TimeSpan.FromHours(24))`.

4. **Single Active Project Rule**:
   - A nation cannot start a second road program while one is currently active (`Completed == false`).
   - Starting while an active project exists fails safely with zero financial penalty.

5. **Completion and State Capacity Recalculation**:
   - Before the finish boundary is reached, infrastructure condition remains strictly unchanged.
   - At or after the finish boundary, completion raises that country's `InfrastructureCondition` by **0.1**, clamped strictly at **1.0**.
   - Completion immediately triggers a recalculation of sovereign State Capacity via `EvaluateStateCapacity()`.
   - Core State Capacity minimum bottleneck rules and cabinet formulas are preserved completely intact.

6. **Offline Catch-Up Integration**:
   - When offline catch-up is executed via [`OfflineYieldCatchUpService`](../../../src/Republic.Core/NationalYield/OfflineYieldCatchUpService.cs), any active road project whose finish boundary has passed is completed automatically.
   - Reported through [`OfflineYieldBriefing.RoadProgramCompleted`](../../../src/Republic.Core/NationalYield/OfflineYieldBriefing.cs).
   - The catch-up engine does not invent or process other project types.

7. **Sovereign Isolation**:
   - Expenditures and infrastructure benefits are strictly confined to the initiating sovereign country.
   - Country B does not pay for, advance, or benefit from Country A's road program.

8. **CLI Wiring**:
   - The existing provincial-investment directive option `[10]` in [`Program.cs`](../../../src/Republic.Cli/Program.cs) is wired to allow the executive player to initiate the National Road Program.
   - Prints the remaining sovereign R treasury balance and the updated infrastructure condition.
   - Preserves the existing top-level menu hierarchy without adding new menu numbers.

9. **Scope Boundary**:
   - This slice implements only the National Road Program and closes Phase 4.
   - Does not implement a generic project catalog, infrastructure maintenance decay, elections, diplomacy, or networking.

## Consequences
- Sovereign nations now possess an active, capital-expenditure mechanism to elevate infrastructure condition and relieve State Capacity bottlenecks.
- Treasury liquidity must be balanced between reserves and capital investment.
- All timing conforms to the pure, deterministic Republic simulation clock.
- Phase 4 is formally completed.
