# Republic Documentation Hub

This directory is the authoritative, canonical documentation hub for **Republic**.

---

## Canonical Design Summary

Republic is a **persistent multiplayer political and nation-building simulation**. Every player governs their own procedurally generated country with the primary objective:

> **BUILD, STRENGTHEN, PROTECT AND SUSTAIN THEIR COUNTRY.**

- **Central Simulation Object**: The **country itself** is the central object of the simulation. The **Presidential Office** serves as the executive command interface and command centre.
- **Foundational Multiplayer**: Multiplayer is part of Republic's identity from the beginning. Player-governed nations interact through diplomacy, bilateral trade, treaties, investment, alliances, intelligence, economic competition, military pressure, and war.
- **Official Currency**: **REPU** (`R`), the sole money of the game (e.g., `R45M`, `R450M`, `R1.2B`).
- **National Yield & State Capacity**: National Yield is the measurable output and accumulating capacity of a functioning state (not free handouts). Outcomes depend heavily on State Capacity and governance.
- **Food & Science**: Food is produced by citizens and businesses within a government-enabled environment (no magic food deposits). Science acts as an upstream national capability upgrading all sectors.
- **Cabinet Appointments & Trade-offs**: Real consequences and trade-offs (e.g., competence vs. loyalty). Every important decision has pros and cons.
- **Simulation Time**: Real-world calendar date alignment on **West Africa Time (WAT / UTC+1)**, starting from **Republic Day 0, 00:00 WAT** on a shared global simulation clock.
- **Founding & National Age**: Recorded Independence Day timestamp, temporary 10x founding boost, and National Age where *age does not automatically equal strength*.
- **Offline Persistence**: Continuous simulation while offline. *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."* Players receive a concise National Briefing upon return.

---

## Authoritative Canonical Documents

1. **[Roadmap/README.md](./Roadmap/README.md)** — Authoritative 10-wave player-experience roadmap.
2. **[Features/README.md](./Features/README.md)** — Canonical wave-by-wave feature mapping and acceptance standards.
3. **[Architecture/README.md](./Architecture/README.md)** — Technical structure, headless simulation core, and host interfaces.
4. **[Architecture/Decisions/ADR-0001-canonical-delivery-model.md](./Architecture/Decisions/ADR-0001-canonical-delivery-model.md)** — Delivery model decision record.
5. **[Gameplay Philosophy/Design Principles.md](./Gameplay%20Philosophy/Design%20Principles.md)** — Canonical v2.0 design principles and the Republic Test.
6. **[Gameplay Philosophy/MANIFESTO.md](./Gameplay%20Philosophy/MANIFESTO.md)** — The sovereign nation-building manifesto.
7. **[Core_Gameplay_Philosophy.md](./Core_Gameplay_Philosophy.md)** — Core gameplay systems and systemic interaction loops.
8. **[Features/Foundation/Time_System.md](./Features/Foundation/Time_System.md)** — Technical specification for real-world date alignment and the WAT global simulation clock.
9. **[GDD/README.md](./GDD/README.md)** — Comprehensive Game Design Document index.

---

## Documentation Policy

- **Authoritative Hierarchy**: The documents listed above constitute the canonical source of truth for design, architecture, and feature delivery.
- **Legacy Planning**: Older sprint sheets and legacy backlog matrices serve as historical implementation context and must yield to these canonical documents.
- **Multiplayer-First Integrity**: All specifications must reflect that Republic is a persistent multiplayer simulation by identity from day one, even if low-level network transport is staged across roadmap waves.
- **No Fictional Calendars or Flat Stat Buffs**: Adhere strictly to real-world WAT date alignment and deep systemic trade-offs.
