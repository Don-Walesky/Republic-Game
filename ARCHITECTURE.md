# Architecture

## Canonical Delivery & Architectural Model

Republic is organized by **player-experience waves** and implemented through **headless simulation layers**.

- **Waves** answer **what the player experiences next**.
- **Layers** answer **where code belongs and how systems interact**.

Authoritative decision record:
- [Docs/Architecture/Decisions/ADR-0001-canonical-delivery-model.md](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0001-canonical-delivery-model.md)

---

## Foundational Architectural Principles

1. **The Country as the Central Simulation Object**:
   The core simulation models the **country itself** (territorial integrity, population demographics, macroeconomic production, civic institutions, infrastructure, and State Capacity). The Presidential Office serves as the player's executive command interface and command centre.

2. **Multiplayer by Foundation**:
   Republic is designed from day one as a persistent multiplayer political and nation-building simulation. Entity definitions, world manager registries, and persistence systems model concurrent multi-nation coexistence, bilateral trade, treaties, and intelligence, even while network transport is staged across delivery waves.

3. **Persistent Global Simulation Clock (WAT / UTC+1)**:
   The simulation aligns with real-world Gregorian calendar dates on **West Africa Time (WAT / UTC+1)**, starting from **Republic Day 0, 00:00 WAT**. National Yield cycles are globally synchronized to this timeline.

4. **Continuous Offline Persistence**:
   *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*  
   The simulation advances continuously when players are offline, generating an executive National Briefing upon reconnecting.

5. **Official Currency — REPU (`R`)**:
   Monetary accounting is strictly denominated in REPU (`R`).

6. **State Capacity & Earned National Yield**:
   National Yield is the measurable output and accumulating capacity of a functioning state (14 categories). Outcomes are mediated through State Capacity and governance efficiency.

---

## Runtime Layout

### Headless Core (`src/Republic.Core`)
- `Republic.Core.Configuration`: JSON configuration loading and schema validation
- `Republic.Core.Diagnostics`: Multi-sink structured logging and performance telemetry
- `Republic.Core.Events`: Decoupled domain event bus
- `Republic.Core.Time`: Real-world WAT clock, epoch calculation, and Independence Day tracking
- `Republic.Core.World`: Country entities, provincial state, demographics, and geography
- `Republic.Core.Economy`: REPU treasury, budget services, trade markets, and development projects
- `Republic.Core.Government`: Cabinet appointments, trade-offs, and State Capacity
- `Republic.Core.Diplomacy` & `Republic.Core.Military`: Bilateral treaties, summits, defense, and deterrence
- `Republic.Core.Persistence`: Serialization, offline catch-up, and SHA256 checksums
- `Republic.Core.Engine`: Deterministic tick loop and subsystem coordination

### Host Namespaces
- `src/Republic.App`: Dependency injection container and bootstrap host
- `src/Republic.Cli`: Interactive console verification dashboard and gameplay loop

### Presentation Boundary (`unity/`)
- Reserved for future presentation shells (Executive Desk, 3D office suite, interactive maps).
- No Unity runtime dependencies are required for building, running, or testing the simulation core.

---

## Design Rules

- Domain logic remains strictly outside presentation engines.
- Every system exposes explicit inputs, outputs, and events.
- Every new feature must preserve deterministic simulation.
- Every decision must embody meaningful trade-offs with explicit pros and cons.
- Every wave must ship as an automated, testable, or executable slice.
