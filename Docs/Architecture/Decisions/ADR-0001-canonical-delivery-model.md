# ADR-0001: Canonical Delivery Model

## Status
Accepted (Amended to reflect Revised Republic Design v2.0)

## Context
The repository historically contained two conflicting planning models:
- a wave-oriented player-experience roadmap
- a layered technical model presented as an alternative roadmap

Additionally, early planning artifacts sometimes relegated multiplayer to a late-stage add-on (Phase 5 / Wave 9) and conflated the executive office with the entire simulation.

## Decision
1. **10-Wave Delivery Model**: Republic uses the **10-wave player-experience roadmap** as the canonical delivery framework.
2. **Foundational Multiplayer Identity**: Multiplayer is a foundational architectural property from Wave 0 onwards. The game is conceived, modeled, and architected as a persistent multiplayer simulation where every player governs their own procedurally generated country in a shared living world. Wave 9 delivers live network transport and client-server synchronization, but all preceding domain layers (world simulation, economy, military, diplomacy) are architected for persistent multi-nation coexistence from the start.
3. **The Country as the Central Simulation Object**: The core simulation engine models the **country itself** (territory, population, economy, institutions, state capacity). The Presidential Office is the player's executive command interface and command centre.
4. **Headless Deterministic .NET Core**: Wave 0 is implemented as a headless, deterministic .NET 8.0 simulation core decoupled from visual presentation engines. Presentation shells adapt to the core rather than owning it.
5. **Real-World Global Simulation Clock**: The simulation timeline follows real-world calendar progression anchored to **West Africa Time (WAT / UTC+1)** starting at Republic Day 0, 00:00 WAT, supporting continuous offline persistence.

## Consequences
- Roadmap, feature ordering, and milestone gates follow the 10-wave model.
- Simulation logic remains fully testable via automated .NET test suites without presentation dependencies.
- Data structures and domain models assume multi-country persistence and shared global time from day one.
- Legacy planning documents serve as historical context only; canonical design documents in `Docs/` govern active development.
