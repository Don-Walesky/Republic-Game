# Republic — Project Overview

Republic is a **persistent multiplayer political and nation-building simulation** created by **Don-Walesky**.

In Republic, every player governs their own procedurally generated country in an unbroken, evolving geopolitical world.

Repository: <https://github.com/Don-Walesky/Republic-Game>

---

## 1. Core Vision & Objective

The player's primary objective is clear and fundamental:

> **BUILD, STRENGTHEN, PROTECT AND SUSTAIN THEIR COUNTRY.**

### The Country as the Central Simulation Object
The **country itself**—its territory, population, economy, civic institutions, infrastructure, and defense—is the central object of the simulation.

The **Presidential Office** serves as the player's command interface and executive command centre. Players experience the burden and power of executive leadership, but the nation itself is what lives, develops, and endures.

---

## 2. Foundational Multiplayer Reality

Republic is a multiplayer simulation from its very foundation:
- Sovereign player-governed nations exist concurrently within the same persistent world.
- Players interact through **bilateral trade**, **diplomacy**, **treaties**, **cross-border investment**, **alliances**, **intelligence operations**, **economic competition**, **military deterrence**, and **open conflict**.
- The simulation continues to progress even when an individual player is offline.

---

## 3. The Core Game Loop

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

> **Core Principle**: Every important action should ultimately affect the strength, stability, prosperity, security, or political survival of the country.

---

## 4. Key Systems & Pillars

### Official Currency — REPU
- Denominated in **REPU** with symbol **`R`** (e.g., `R45M`, `R450M`, `R1.2B`).
- REPU is the sole money of the game. It is not a secondary token or conversion commodity.

### National Yield & State Capacity
- *"National Yield is not free stuff given to the player. It is the measurable output and accumulating capacity of a functioning state."*
- Calculated from actual country conditions across 14 capacity categories (Treasury revenue, science, industry, energy, infrastructure, human capital, military readiness, security, etc.).
- **State Capacity** determines how effectively resources and executive orders translate into tangible national strength.

### Emergent Food Economy & Upstream Science
- Food is **never** an arbitrary yield deposit. The government creates the enabling environment (irrigation, roads, research, credit, storage), and private citizens and enterprises produce food.
- Science functions as an upstream capability upgrading commercial technology, healthcare, energy efficiency, agriculture, and sovereign defense.

### Cabinet Appointments & Inherent Trade-Offs
- Cabinet appointments carry authentic consequences and trade-offs (e.g., technical competence vs. political loyalty).
- **Rule: Every important decision has pros and cons.**

### Real-World Calendar & Global Clock
- Synchronized with the real-world Gregorian calendar on **West Africa Time (WAT / UTC+1)**, starting from **Republic Day 0, 00:00 WAT** on a shared global simulation clock.
- When a nation is founded, its permanent **Independence Day** timestamp is recorded.
- Newly founded nations receive a temporary **Founding Period** with a **10x National Yield Multiplier** and initial momentum buffs that taper off over time.
- **National Age** accumulates continuously from founding, but *age does not automatically equal strength*.

### Continuous Offline Persistence
- *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*
- When returning from offline periods, the Presidential Office delivers a concise **National Briefing**.

---

## 5. Development Model

Republic follows a **10-wave player-experience roadmap**, built upon a headless, deterministic .NET 8.0 core with automated test verification.

For canonical specifications and design documents, see [Docs/README.md](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/README.md).
