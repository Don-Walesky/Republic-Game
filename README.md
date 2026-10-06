# Republic — Persistent Political & Nation-Building Simulation

**Republic** is a persistent multiplayer political and nation-building simulation powered by a deterministic .NET 8.0 core and an executive office presentation layer.

In Republic, every player governs their own procedurally generated country in a shared, living geopolitical world.

---

## The Sovereign Objective

> **BUILD, STRENGTHEN, PROTECT AND SUSTAIN THEIR COUNTRY.**

### The Country as the Central Simulation Object
The **country itself**—its territory, population, economy, civic institutions, infrastructure, and defense—is the central object of the simulation.

The **Presidential Office** serves as the player's command interface and executive command centre. Players experience the burden and power of leadership as heads of government, steering the nation from the executive desk, but the entire country is what lives, produces, evolves, and endures.

---

## Foundational Multiplayer Reality

Multiplayer is a foundational property of Republic from day one. Sovereign player-governed nations exist concurrently in the same persistent world and interact through:
- **Diplomacy & Treaties**: Non-aggression treaties, defense pacts, trade corridor access, and summits
- **Bilateral Trade**: Resource contracts, export/import quotas, and market access denominated in **REPU** (`R`)
- **Foreign Investment**: Cross-border capital flows and joint infrastructure development
- **Alliances & Blocs**: Multi-nation security pacts and regional voting coalitions
- **Intelligence**: Covert reconnaissance, espionage dossiers, and counter-intelligence
- **Economic Competition**: Tariff disputes, currency competition, and trade influence
- **Military Pressure & Conflict**: Border deterrence, force deployments, and strategic conflict
- **International Negotiations**: Multilateral accords, sanctions, and global diplomacy

The world continues to exist, evolve, and change even when an individual player is offline.

---

## Canonical Core Game Loop

```
Govern country
   ↓
Develop country
   ↓
Make political / economic / security decisions
   ↓
Citizens, businesses, and institutions respond
   ↓
National capacity changes
   ↓
Interact with other countries
   ↓
Trade / negotiate / invest / ally / compete / conflict
   ↓
World produces consequences
   ↓
Country changes
   ↓
Respond and adapt
   ↓
Continue building the country
```

> **Guiding Principle**: Every important action should ultimately affect the strength, stability, prosperity, security, or political survival of the country.

---

## Core Simulation Systems

### 1. Official Currency — REPU
The official currency of Republic is **REPU**, with the currency symbol **`R`**:
- `R45M` = 45,000,000 REPU
- `R450M` = 450,000,000 REPU
- `R1.2B` = 1,200,000,000 REPU
- REPU is the sole money of the game. It is not a secondary token or conversion commodity.

### 2. National Yield & State Capacity
- *"National Yield is not free stuff given to the player. It is the measurable output and accumulating capacity of a functioning state."*
- Calculated from the actual condition of the country across 14 capacity categories (Treasury revenue, science, industry, energy, infrastructure, human capital, military readiness, intelligence, etc.).
- **State Capacity** determines how efficiently institutions and decisions convert into productive national outcomes.

### 3. Food Economy & Upstream Science
- Food is **never** a direct yield deposit. The government creates the enabling environment (irrigation, roads, research, credit, storage), and citizens and private businesses produce food.
- Science functions as an **upstream national capability** that elevates downstream sectors including commercial innovation, healthcare, energy efficiency, agriculture, and defense.

### 4. Cabinet Appointments & Real Trade-Offs
- High-stakes appointments balancing technical competence, political loyalty, and integrity.
- **Rule: Every important decision has pros and cons.**

### 5. Real-World Time & Global Simulation Clock
- Anchored to the **real-world Gregorian calendar** on **West Africa Time (WAT / UTC+1)**.
- Official launch: **Republic Day 0, 00:00 WAT**, advancing continuously on a shared global simulation clock.
- **Independence Day**: Each country's founding date, time in WAT, and starting state are permanently recorded.
- **Founding Period**: Newly founded countries receive a temporary **10x National Yield Multiplier** and momentum buffs that taper off over time.
- **National Age**: Accumulated age reflects institutional maturity, but *age does not automatically equal strength*.

### 6. Continuous Offline Persistence
- *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*
- The simulation runs continuously while offline. When returning, the Presidential Office provides a concise **National Briefing** of all events and revenues accumulated.

---

## Canonical 10-Wave Delivery Roadmap

| Wave | Name | Core Focus |
|---|---|---|
| **0** | **Foundation** | Deterministic .NET 8.0 core, global WAT simulation clock, event bus, persistence, multi-country world shell |
| **1** | **Executive Workspace** | Presidential office command interface, executive desk suite, national briefing on login |
| **2** | **World Simulation** | Country as central simulation object, procedural generation, Independence Day, REPU currency, National Yield engine, State Capacity |
| **3** | **Campaign** | Factions, democratic elections, public sentiment, and constitutional mandates |
| **4** | **Government** | Cabinet appointments with competence vs. loyalty trade-offs, ministerial portfolios, and executive decrees |
| **5** | **Economy** | REPU national budget, food enabling environment investments, upstream science, and development projects |
| **6** | **Legislature** | Parliamentary chambers, legislative bills, and constitutional amendments |
| **7** | **Military** | Armed service branches, DEFCON readiness, border deterrence, and strategic directives |
| **8** | **Diplomacy** | Sovereign inter-country bilateral trade, treaties, alliances, and intelligence operations |
| **9** | **Multiplayer Networking** | Live network transport, client-server synchronization, and real-time inter-player summits |

---

## Quickstart & Local Verification

### Running the Interactive CLI Console
```bash
dotnet run --project src/Republic.Cli/Republic.Cli.csproj
```

### Running Automated Test Suites
```bash
dotnet test Republic.sln
```

---

## Project Structure

* `src/Republic.Core` — Deterministic game engine, domain models, services, and persistence.
* `src/Republic.App` — Application bootstrapper host and dependency injection container.
* `src/Republic.Cli` — Terminal dashboard, directive menus, and console game loop.
* `tests/Republic.Core.Tests` — Unit and integration test suites (160+ passing tests).
* `unity/` — Presentation shell boundary for executive desk visualization.
* `Docs/` — Canonical product, design, and technical specifications.
