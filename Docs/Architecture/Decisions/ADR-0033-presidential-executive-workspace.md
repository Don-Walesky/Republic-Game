# ADR-0033: First Playable Presidential Executive Workspace

## Status
Accepted

## Context
Following the completion of Phase 9 ([ADR-0032](ADR-0032-domestic-resource-production.md)), sovereign countries can produce physical goods domestically and engage in bilateral trade. However, the player lacked an integrated, authoritative presidential decision-making loop where they could enter the presidential office, understand the genuine condition of their nation, make a meaningful capital development decision, spend liquid state resources, advance simulation time, and observe the resulting state changes reflected on the country.

Phase 10 requires:
1. An improved, readable Presidential Desk dashboard displaying authentic domain state: country identity, founding moment, WAT simulation timestamp, next scheduled 6-hour cycle boundary, REPU treasury formatted with the `R` symbol, National Yield and state capacity indicators, domestic resource stockpiles, active/completed capital development projects and progress, and systemic political/stability concerns.
2. Complete transparency: distinguishing unestablished capacities from zero without inventing arbitrary statistics.
3. One fully playable national development decision reusing the existing canonical [`NationalPowerPlantProject`](../../../src/Republic.Core/Economy/Projects/NationalPowerPlantProject.cs) (Cost: R1,000,000, Duration: 72 hours / 12 cycle boundaries, Benefit: +5.0 Energy Capacity).
4. Ergonomic presidential confirmation prompts (`[Y/N]`) where cancelling leaves sovereign state and treasury completely untouched.
5. Authoritative simulation-time progression: project duration must depend strictly on the Republic simulation clock (not real-time delays or wall clocks).
6. Meaningful time advancement: advancing time must process due National Yield cycles, domestic resource extraction cycles, and project completions deterministically without duplicate deductions or duplicate benefits.
7. Offline catch-up: capital projects completing during simulated absences must be automatically commissioned and reported in the executive briefing.
8. Sovereign isolation: foreign nations' treasuries, inventories, and capacities must remain untouched.
9. Persistence: project state must survive serialization through the save/load manager.

## Decision

1. **Presidential Desk Dashboard Enhancements**:
   - Overhauled `RenderDashboard` in [`Program.cs`](../../../src/Republic.Cli/Program.cs) to present an authentic executive overview:
     - **Sovereign Identity & Age**: official name, founding timestamp, and historical age in Republic days.
     - **Authoritative Clock**: current simulation day, time in West Africa Time (WAT, UTC+1), tick count, and next scheduled 6-hour cycle boundary (00:00, 06:00, 12:00, 18:00 WAT).
     - **Sovereign Treasury**: liquid balance formatted with standard REPU notation (e.g. `R4.5M`, `R1,000,000`).
     - **National Yield & State Capacities**: Latest evaluated yield revenue, current State Capacity score, and itemized capacities (Energy, Industrial, Administrative, Infrastructure, Human Capital, Science, Security).
     - **Domestic Stockpiles**: physical commodity inventory (Iron, Petroleum, Timber, etc.).
     - **Capital Development Projects**: active projects with % progress and finish boundary, and completed projects.
     - **Political & Stability Health**: public approval rating, national stability, ruling/opposition party breakdown, pending parliamentary bills, and active crises.
   - Non-established capacities display `"Not established"` via [`NationalDashboard`](../../../src/Republic.Core/World/Models/NationalDashboard.cs) rather than fake values.

2. **Capital Development Decision Ergonomics**:
   - Reused [`NationalPowerPlantProject`](../../../src/Republic.Core/Economy/Projects/NationalPowerPlantProject.cs) without alteration to preserve existing causal balance:
     - Fixed Cost: R1,000,000 REPU.
     - Construction Duration: 72 hours (12 six-hour boundaries).
     - Completion Benefit: +5.0 Energy Capacity, triggering state capacity recalculation.
   - Provided full project specification review in the CLI development workspace (`[13] Oversee National Development Projects`).
   - Implemented two-stage executive confirmation:
     - If the player rejects or cancels at the prompt (`N`), zero funds are deducted and no project is created.
     - If liquid funds are below R1,000,000, the action is rejected with diagnostic feedback.
     - If confirmed (`Y`), R1,000,000 is withdrawn once and the project commences.

3. **Domain Presidential Workspace Handler**:
   - Created [`PresidentialWorkspaceHandler`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs) in `Republic.Core.World`:
     - `InitiatePowerPlantProject(Country country, RepublicTime simulationTime, bool confirmed = true)` validates fund availability, active project conflicts, and confirmation, returning an [`ExecutiveActionOutcome`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs).
     - `AdvanceTime(IWorldManager worldManager, ITimeSystem timeSystem, TimeSpan duration, ...)` advances the authoritative clock, processes due National Yield cycles, processes due domestic resource production cycles, checks project completions across all sovereign nations, and returns a [`TimeAdvancementSummary`](../../../src/Republic.Core/World/PresidentialWorkspaceHandler.cs).

4. **Time Advancement Menu & Execution**:
   - Upgraded menu option `[7]` to `Advance Republic Simulation Time` with preset options:
     - `[1]` Advance 1 Boundary (6 Hours / 1 standard cycle)
     - `[2]` Advance 4 Boundaries (24 Hours / 1 Republic day)
     - `[3]` Advance 12 Boundaries (72 Hours / Full Power Plant construction duration)
     - `[4]` Advance 10 Engine Frames (micro-tick)
     - `[5]` Advance Custom Hours
   - Displays a comprehensive post-advancement report itemizing National Yield revenue deposited, domestic resource extraction per node, and capital projects commissioned.

5. **Offline Catch-Up Integration**:
   - Updated [`OfflineYieldBriefing`](../../../src/Republic.Core/NationalYield/OfflineYieldBriefing.cs) with `ProjectsCompletedCount`.
   - Updated [`OfflineYieldCatchUpService`](../../../src/Republic.Core/NationalYield/OfflineYieldCatchUpService.cs) to call `country.CheckProjectsCompletion(returnTime)` and record completed projects in the startup briefing.

6. **File Logging Concurrency Hardening**:
   - Updated [`FileLogSink`](../../../src/Republic.Core/Diagnostics/FileLogSink.cs) to synchronize file writes across all instances using a `static readonly object _syncRoot`, preventing concurrent test and service runs on Windows from throwing `IOException`.

7. **Persistence**:
   - Capital development projects are registered in `Country.Projects` and serialized polymorphicly through [`SaveGameManager`](../../../src/Republic.Core/Persistence/Services/SaveGameManager.cs). Tests verify active and completed projects survive save and load cycles intact.

## Consequences

- The player now possesses an authentic, fully functional Presidential Executive Workspace where they can review the real state of the nation, make an executive investment decision, advance time through the authoritative simulation clock, and observe genuine causal results.
- All operations are idempotent; repeated time advancement or completion checks never duplicate treasury deductions or yield benefits.
- Foreign countries remain strictly isolated and unaffected by domestic project decisions.
- Full test suite passed: 613 tests executed with 0 failures and 0 regressions.
