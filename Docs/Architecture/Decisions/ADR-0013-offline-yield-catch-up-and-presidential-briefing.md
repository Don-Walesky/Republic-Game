# ADR-0013: Offline National Yield Catch-Up and Presidential Briefing

## Status
Accepted

## Context
Following the establishment of the National Yield domain model ([ADR-0004](ADR-0004-national-yield-domain-model.md)), calculation engine ([ADR-0007](ADR-0007-isolated-national-yield-calculation-engine.md)), cycle execution ([ADR-0010](ADR-0010-national-yield-cycle-execution.md)), REPU treasury revenue ([ADR-0011](ADR-0011-repu-treasury-and-yield-revenue.md)), and the 6-hour production cycle schedule on the Republic clock ([ADR-0012](ADR-0012-six-hour-national-yield-cycle-scheduling.md)), Step 4K introduces offline progression recovery and executive briefing.

## Decision

1. **Offline Catch-Up Domain Mechanics**:
   - Evaluates missed production cycles between the sovereign nation's [`Country.LastCreditedBoundary`](../../../src/Republic.Core/World/Models/Country.cs) (or founding time/epoch) and the authoritative clock time upon return.
   - Progression uses the existing 6-hour West Africa Time schedule (00:00, 06:00, 12:00, 18:00 WAT).
   - Time calculations strictly utilize [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs); direct calls to system real-time APIs (`DateTime.Now`) are prohibited.
   - Project progression is not simulated or invented because development projects are not yet scheduled.

2. **8-Boundary Evaluation Cap & Remainder Tracking**:
   - A single login or catch-up execution processes at most 8 missed cycle boundaries (up to 48 hours of missed production).
   - If more boundaries were due beyond the cap, the service computes and reports the remaining count via [`OfflineYieldBriefing.RemainingBoundaries`](../../../src/Republic.Core/NationalYield/OfflineYieldBriefing.cs).

3. **Presidential Briefing Value Object (`OfflineYieldBriefing`)**:
   - Encapsulated as an immutable domain value object in [`OfflineYieldBriefing`](../../../src/Republic.Core/NationalYield/OfflineYieldBriefing.cs), decoupled from UI or console rendering.
   - Authoritative fields:
     - `CountryId`: Identifier of the sovereign country.
     - `FromTime`: Simulation timestamp at the start of the catch-up window.
     - `ToTime`: Simulation timestamp on return.
     - `BoundariesCredited`: Total boundaries credited in this execution (0 to 8).
     - `RepuDeposited`: Liquid REPU deposited into the treasury.
     - `RemainingBoundaries`: Number of due boundaries remaining uncredited.
     - `NextDueBoundary`: Next upcoming 6-hour production boundary.

4. **Treasury Difference Calculation & Durable State Idempotency**:
   - The catch-up service ([`OfflineYieldCatchUpService`](../../../src/Republic.Core/NationalYield/OfflineYieldCatchUpService.cs)) reads the sovereign treasury balance before calling [`NationalYieldCycleService.ExecuteDueCycles`](../../../src/Republic.Core/NationalYield/NationalYieldCycleService.cs), executes the cycles, reads the final balance, and determines deposited REPU from the exact positive difference.
   - Idempotency relies strictly on durable state in [`Country.LastCreditedBoundary`](../../../src/Republic.Core/World/Models/Country.cs) and [`RepuTreasury.AppliedSnapshotIds`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs).
   - It does not depend on transient in-memory caches in the cycle service. An immediate subsequent call deposits zero REPU and credits zero boundaries.

5. **Sovereign Country Isolation**:
   - Country states are sovereign and isolated. Country A catching up on missed yield has zero side-effects on Country B.

6. **CLI Startup Integration**:
   - When the Republic CLI initializes in [`Program.cs`](../../../src/Republic.Cli/Program.cs), if the player's sovereign nation has due boundaries (e.g. following an offline gap), the catch-up service executes and displays the executive briefing before presenting the executive desktop.

7. **Strict Scope Boundaries**:
   - Does not implement 4M founding buffs, State Capacity, cabinet effects, development projects, or networking.

## Consequences
- Presidents returning after absence receive deterministic yield recovery and a transparent executive briefing.
- Treasury balances remain safe against double-credit through durable snapshot tracking.
