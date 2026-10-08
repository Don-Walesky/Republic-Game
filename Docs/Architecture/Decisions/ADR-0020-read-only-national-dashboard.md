# ADR-0020: Read-Only National Dashboard

## Status
Accepted

## Context
Following the implementation of the readable country profile ([ADR-0019](ADR-0019-readable-country-profile.md)), Phase 5 item 5.2 introduces a unified, read-only **National Dashboard** for sovereign countries.

Previously, CLI status lines were fragmented across the terminal (treasury and GDP mixed with raw state capacity scores, founding buffs printed independently, demographics and inflation shown ad-hoc). Observers needed a consolidated, side-effect-free read model that projects authoritative sovereign state without mutating simulation parameters or inventing ungrounded scores.

## Decision

1. **Read-Only NationalDashboard Value Object**:
   - Implemented as [`NationalDashboard`](../../../src/Republic.Core/World/Models/NationalDashboard.cs).
   - Constructed directly from a sovereign [`Country`](../../../src/Republic.Core/World/Models/Country.cs) entity and the authoritative simulation timestamp [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs).
   - Features nine standardized diagnostic lines:
     - **Approval**: Displays the country's existing happiness rating as a formatted percentage (e.g., `68.5%`). Does not create or simulate a new approval metric.
     - **Economy**: Combines the treasury balance and GDP already present on the sovereign country.
     - **Stability**: Displays the country's existing baseline stability as a formatted percentage (e.g., `75%`).
     - **Treasury**: Displays the liquid treasury balance formatted in standard REPU notation via [`RepuTreasury.FormattedBalance`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) (e.g., `R15B`).
     - **Yield**: Displays the revenue from the latest completed [`NationalYieldSnapshot`](../../../src/Republic.Core/NationalYield/NationalYieldSnapshot.cs) (`RepuTreasuryRevenue`), formatted in standard REPU notation, or strictly `"R0"` if no yield snapshot exists.
     - **Infrastructure**: Displays the State Capacity infrastructure condition input from [`StateCapacityInputs.InfrastructureCondition`](../../../src/Republic.Core/NationalYield/StateCapacityInputs.cs) as a percentage (e.g., `60%`).
     - **Diplomacy**: Prints `"Not established"`. Does not invent or simulate foreign policy scores.
     - **Science**: Prints `"Not established"`. Does not invent or simulate research scores.
     - **Military**: Prints `"Not established"`. Does not invent or simulate defense scores.

2. **Strict Mutation & Purity Rules**:
   - Building or querying the dashboard is strictly side-effect-free: reading the dashboard does not alter treasury balance, development projects, ministerial appointments, or yield snapshots.
   - Preserves complete country isolation: querying Country A's dashboard induces zero changes to Country B.
   - Purity: Never calls `DateTime.Now`. Temporal reference points are grounded exclusively in the authoritative [`RepublicClock`](../../../src/Republic.Core/Time/RepublicClock.cs) and [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs).

3. **Consolidated CLI Presentation**:
   - Replaces scattered, disjointed CLI status lines with a single, coherent dashboard block positioned directly beneath the country profile line in [`Program.cs`](../../../src/Republic.Cli/Program.cs).
   - Preserves the country profile line and the authoritative simulation time line.
   - Does not add menu options or numbers to the presidential executive menu.

4. **Scope Constraints**:
   - Restricted strictly to Phase 5 item 5.2.
   - Does not implement 5.3 national strength, 5.4 consequences, elections, diplomacy, science, or military simulation engines.

## Consequences
- Players and tools observe a clear, authoritative summary of sovereign national health and capacity.
- Zero risk of side-effects or state pollution when reading national statistics.
- Prepares the presentation layer for future milestone evaluations without violating domain encapsulation.
