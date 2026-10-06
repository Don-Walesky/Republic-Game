# Architecture Overview

The canonical technical architecture for **Republic**.

---

## 1. Core Architectural Identity

Republic is architected as a **persistent multiplayer political and nation-building simulation**.

### The Country as the Central Simulation Object
The core simulation models the **country itself**—its territory, population demographics, macroeconomic parameters, civic institutions, infrastructure, and State Capacity.

The **Presidential Office** serves as the executive command interface and command centre through which the player issues decrees, conducts diplomacy, manages cabinet ministers, and allocates capital in **REPU** (`R`).

### Foundational Multiplayer Architecture
Multiplayer is a foundational architectural property of Republic from the ground up:
- The world state, country entities, diplomatic relationships, bilateral trade agreements, and persistence models are designed for **concurrent multi-nation persistence**.
- Even while low-level client-server network transport is scheduled for delivery in Wave 9, all lower-layer domain services and data models are architected to support a persistent, shared geopolitical world.

---

## 2. Technical Baseline

Republic is built around a **headless, deterministic .NET 8.0 simulation core** completely decoupled from visual presentation layers.

### Core Domain Namespaces (`src/Republic.Core`)
- `Republic.Core.World`: Country entities, provincial administration, demographics, geography, and resources
- `Republic.Core.Time`: Real-world calendar alignment, West Africa Time (WAT / UTC+1) global clock, and Independence Day tracking
- `Republic.Core.Economy`: REPU monetary baseline, national budget, taxation, trade markets, and development projects
- `Republic.Core.Government`: Cabinet appointments, trade-offs (competence vs. loyalty), legislation, and State Capacity
- `Republic.Core.Events`: High-performance decoupled domain event bus
- `Republic.Core.Persistence`: State serialization, offline catch-up, and SHA256 integrity checksums
- `Republic.Core.Diplomacy` & `Republic.Core.AI`: Bilateral treaties, summits, trade accords, and rival nation simulation
- `Republic.Core.Military`: Armed branches, defense readiness (DEFCON), and deterrence
- `Republic.Core.Intelligence` & `Republic.Core.Media`: Classified intelligence dossiers, press briefings, and public sentiment
- `Republic.Core.Engine`: Deterministic tick loop, yield cycle coordination, and subsystem orchestration

### Host Namespace (`src/Republic.App` & `src/Republic.Cli`)
- Dependency injection container, application host bootstrapper, and interactive CLI verification shell.

### Presentation Layer Boundary (`unity/`)
- Reserved for future presentation shells (Executive Desk, 3D office, interactive map).
- The simulation core runs entirely headless and independently of any presentation engine.

---

## 3. Persistent Global Clock & Offline Continuity

- The simulation is anchored to **West Africa Time (WAT / UTC+1)**, starting from **Republic Day 0, 00:00 WAT**.
- **Continuous Offline Simulation**: *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*
- When an individual player disconnects, their country continues generating National Yield, accumulating REPU revenue, and progressing development projects. Upon reconnecting, the system generates an executive **National Briefing**.

---

## 4. Architectural Decision Records

See **[Decisions/ADR-0001-canonical-delivery-model.md](./Decisions/ADR-0001-canonical-delivery-model.md)** for authoritative delivery model governance.
