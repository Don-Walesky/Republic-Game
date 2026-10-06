# Features

The canonical feature mapping and technical delivery specifications for **Republic**.

---

## Canonical Feature Ordering by Production Wave

### Wave 0 — Foundation
- Solution scaffolding and CI automation
- Configuration loading and schema validation
- Multi-sink structured logging
- High-performance event bus
- Deterministic simulation clock abstractions aligned with real-world calendar progression on **West Africa Time (WAT / UTC+1)**
- Persistence framework (serialization, SHA256 checksum verification)
- Multi-country world manager shell built with foundational multiplayer concurrency

### Wave 1 — Executive Workspace
- Presidential Office as the player's executive command interface and command centre
- Executive desk interaction suite (phone, email, secure memoranda, visitor reception, calendar)
- Executive Decision Spaces (Presidential Office, Cabinet Chamber, Situation Room, Economic Council)
- Actionable **National Briefing** interface presented upon return from offline sessions

### Wave 2 — World Simulation
- **The Country as the Central Simulation Object**: territorial integrity, demographics, institutions, infrastructure, and state capacity
- Procedural country generation and world geography
- **Country Founding & Independence Day**: permanent timestamping (`Date`, `Time` in WAT, starting conditions, country age)
- **Independence Day Founding Period**: temporary **10x National Yield Multiplier** and initial founding momentum buffs
- **Official Currency (REPU / R)**: monetary domain baseline, sovereign reserves, and fiscal accounting
- **National Yield Engine**: calculating measurable productive output across 14 capacity categories (not free handouts)
- **State Capacity Framework**: institutional conversion efficiency driven by governance, civil service merit, and anti-corruption

### Wave 3 — Campaign & Legitimacy
- Political factions, societal demographics, and class alignments
- Democratic electoral campaigns, polling engines, and debates
- Public approval dynamics, civic legitimacy, and constitutional mandates

### Wave 4 — Government & Appointments
- **High-Stakes Political Appointments**: deliberate selection of ministers with genuine trade-offs (e.g., technical competence vs. political loyalty vs. integrity)
- Portfolio effectiveness and civil service execution
- Executive decrees, ministerial proposals, and crisis management
- Cabinet stability and factional realignment

### Wave 5 — Economy & Development
- National budget allocation and taxation policy denominated in REPU (`R`)
- **Emergent Food Economy**: public enabling environment investments (irrigation, feeder roads, agricultural research, storage, credit) driving private production (no magic food deposits)
- **Upstream Science & Research**: research capacity systematically elevating agricultural productivity, medical health, energy efficiency, and industrial margins
- **National Development Projects**: infrastructure, energy, transport, and education projects with REPU costs, real simulation build durations, and continuous progression independent of yield cycles

### Wave 6 — Legislature
- Parliamentary chambers, voting coalitions, and party discipline
- Legislative bill sponsorship, amendments, and debate
- Constitutional amendments and statutory reform

### Wave 7 — Military & Defense
- Armed service branches (Army, Navy, Air Force, Cyber Corps)
- DEFCON readiness levels and strategic deterrence postures
- Border deployments, military installations, and conflict escalation

### Wave 8 — Diplomacy & International Statecraft
- Foundational inter-country interactions between sovereign nations
- Bilateral trade contracts, import/export quotas, and tariff disputes
- Treaties, non-aggression pacts, and mutual defense alliances
- Covert intelligence operations, counter-espionage, and classified dossiers
- Geopolitical competition, diplomatic summits, and global negotiations

### Wave 9 — Multiplayer Networking
- Authoritative client-server network transport
- Real-time multi-client state synchronization across persistent worlds
- Live inter-player treaty execution, bilateral summit interaction, and multilateral council sessions

---

## Feature Quality Bar

Every feature in Republic must:
1. **Pass the Republic Test**: Reinforce the country as the central object of simulation and the presidential office as its command interface.
2. **Respect Foundational Multiplayer Identity**: Data structures, entity models, and game rules must assume concurrent, persistent multi-nation coexistence from the start.
3. **Embody Meaningful Trade-Offs**: Offer no cost-free silver bullets; every important action must have explicit pros and cons.
4. **Treat National Yield as Earned Capacity**: Reflect the measurable productive output of a functioning state, modulated by State Capacity.
5. **Align with the Continuous WAT Clock**: Respect real-world date progression in WAT (UTC+1) and persistent offline continuity.
6. **Expose Clear Events and State Boundaries**: Maintain headless, deterministic simulation logic decoupled from presentation layers.
7. **Include Comprehensive Test Coverage & Documentation**: Every feature must include automated unit/integration tests and complete specifications.
