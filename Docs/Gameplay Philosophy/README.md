# Gameplay Philosophy

The canonical design philosophy guiding all architecture, gameplay systems, and development decisions in Republic.

---

## Authoritative Documents

### 📖 [The Republic Manifesto](./MANIFESTO.md)
The foundational ethos of Republic:
> **"Republic is not a game about ruling people. It is a persistent multiplayer simulation about carrying the responsibility of statecraft, building a nation, and sustaining a sovereign country in a living world."**

### 📋 [Design Principles](./Design%20Principles.md)
Comprehensive v2.0 design specifications covering:
- **Game Identity**: Persistent multiplayer political and nation-building simulation where every player governs their own procedurally generated country.
- **The Country as Central Object**: The country itself is the central simulation object; the presidential office is the executive command interface.
- **Primary Objective**: Build, strengthen, protect, and sustain their country.
- **Core Game Loop**: The 11-step national governance cycle.
- **Official Currency**: REPU (`R`), the sole money of the game.
- **National Yield & State Capacity**: Measurable output and institutional capacity, not arbitrary handouts.
- **The Food Economy**: Enabling environment investments vs. private agricultural production (no magic food deposits).
- **Upstream Science**: Research capacity upgrading all downstream sectors.
- **Political Appointments**: High-stakes trade-offs between technical competence, political loyalty, and integrity.
- **Pros and Cons**: Mandatory trade-offs in every major decision.
- **Development Projects**: Concrete capacity creation with REPU costs, build durations, and continuous progress independent of yield cycles.
- **Simulation Time**: Real-world calendar alignment, West Africa Time (WAT / UTC+1) global simulation clock, and persistent offline continuity.
- **Founding & National Age**: Independence Day timestamps, temporary 10x founding boost, and age vs. strength principles.

---

## The Republic Test

Before approving or implementing any mechanic, feature, or subsystem, verify:

1. **Does it center the country as the primary simulation object while preserving the presidential office as the executive command interface?**
2. **Does it respect the persistent, shared multiplayer reality of the world?**
3. **Does it enforce meaningful trade-offs with explicit pros and cons rather than obvious "best stats"?**
4. **Does it treat National Yield as earned state output rather than free handouts?**
5. **Does it align with the continuous real-world timeline (WAT / UTC+1) and persistent offline progression?**
6. **Does it prioritize executive leadership and state capacity over micromanagement?**
7. **Does it generate emergent geopolitical, economic, and institutional storytelling?**

If a feature fails these criteria, it must be redesigned or omitted.

---

## Core Design Pillars

Republic is founded upon five core pillars:

### 1. Sovereign Agency & Meaningful Trade-offs
Players shape their nation through high-impact executive choices. Every policy, appointment, and development project entails unavoidable trade-offs (e.g., technocratic competence vs. political loyalty; immediate consumption vs. capital investment). There are no painless solutions.

### 2. State Capacity & Earned National Yield
National output is not a free periodic gift. National Yield represents the measurable productive and institutional capacity of a functioning state. Two nations with identical geography achieve vastly different outcomes depending on their governance, civil service effectiveness, and State Capacity.

### 3. Emergent Economics & Enabling Governance
The state creates the enabling environment (infrastructure, credit, irrigation, research), while citizens and businesses generate actual production (e.g., food, consumer goods, commercial technology). Systems interact dynamically to produce emergent national wealth or shortages.

### 4. Real-World Persistence & Global Simulation Clock
Republic operates on a unified, real-world calendar advancing continuously on West Africa Time (WAT / UTC+1). Time never stops when a player logs off. Revenue, development projects, institutional dynamics, and international events proceed uninterrupted in the player's absence.

### 5. Foundational Multiplayer Reality
Republic is a multiplayer world by identity from the very beginning. Sovereign player nations coexist, trade, negotiate, ally, compete, spy, and clash in an evolving global order. Even while network transport is staged across development waves, all core models and systems are architected for persistent multi-nation interaction.

---

## Quick Navigation

- **Feature Specifications** → [Features](../Features/README.md)
- **Technical Architecture** → [Architecture](../Architecture/README.md)
- **Authoritative Roadmap** → [Roadmap](../Roadmap/README.md)
- **User Experience & Command Center** → [UX](../UX/README.md)
