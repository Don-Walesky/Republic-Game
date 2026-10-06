# Technical Design Document — Architecture

**Version**: 2.0 (Canonical Revised Republic Architecture)  
**Status**: Active  
**Last Updated**: 2026-10-06  

---

## 1. Architectural Vision & Identity

Republic's architecture is engineered to support a **persistent multiplayer political and nation-building simulation**.

### The Country as the Central Simulation Object
The core simulation models the **country itself** as the foundational entity:
- Territorial provinces and geography
- Demographic composition, living standards, and public sentiment
- Macroeconomic production, fiscal budgets, and sovereign reserves in **REPU** (`R`)
- **State Capacity** and institutional effectiveness
- **National Yield** capacity across 14 vital categories

The **Presidential Office** serves as the player's executive command interface and command centre. The player directs statecraft from the executive desk, but the entire country is what lives, evolves, and endures.

### Foundational Multiplayer Architecture
Multiplayer is a foundational architectural property of Republic from day one:
- The domain models, country entities, world state, persistence architecture, and event bus are designed for **concurrent multi-nation persistence**.
- Sovereign player-governed nations coexist and interact through bilateral trade, diplomacy, treaties, alliances, intelligence, and conflict.
- While direct client-server network transport and live multi-client synchronization are staged for delivery in Wave 9, all lower architectural layers assume a shared, persistent multiplayer world.

### Real-World Global Simulation Clock & Offline Continuity
- Republic operates on a shared global simulation clock aligned to the **real-world Gregorian calendar** in **West Africa Time (WAT / UTC+1)**, anchored to **Republic Day 0, 00:00 WAT**.
- **Continuous Offline Simulation**:  
  > *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*  
  The simulation never halts when an individual player logs off. The engine processes yield cycles, advances construction of development projects, executes institutional functions, and records world events, generating a concise **National Briefing** upon the player's return.

---

## 2. The Five Architectural Layers

Republic's technical architecture is structured in **five distinct layers**, ensuring clean separation of concerns, testability, and modularity:

```
┌─────────────────────────────────────────────────────────────┐
│                 LAYER 5: PRESENTATION LAYER                 │
│         (Presidential Office Suite, Desk UI, Audio)         │
└──────────────────────────────┬──────────────────────────────┘
                               │ Player Actions
┌──────────────────────────────▼──────────────────────────────┐
│              LAYER 4: DECISION SPACE LAYER                  │
│       (Executive Agency, Trade-Offs, Pros & Cons)           │
└──────────────────────────────┬──────────────────────────────┘
                               │ Validated Decrees & Projects
┌──────────────────────────────▼──────────────────────────────┐
│               LAYER 3: COMMUNICATION LAYER                  │
│        (Intelligence Dossiers, National Briefing)           │
└──────────────────────────────┬──────────────────────────────┘
                               │ State Insights & Alerts
┌──────────────────────────────▼──────────────────────────────┐
│              LAYER 2: GAMEPLAY SYSTEMS LAYER                │
│    (Cabinet Appointments, Treaties, Legislative Rules)      │
└──────────────────────────────┬──────────────────────────────┘
                               │ Systemic Rules & Events
┌──────────────────────────────▼──────────────────────────────┐
│              LAYER 1: SIMULATION ENGINE LAYER               │
│   (Country State, WAT Clock, National Yield, Persistence)   │
└─────────────────────────────────────────────────────────────┘
```

---

### Layer 1: Simulation Engine Layer (Foundation)

**Responsibility**: Pure, headless, deterministic simulation of the physical, economic, and institutional world.

**Key Subsystems**:
- **Persistent Global Simulation Clock**: Operates continuously on West Africa Time (WAT / UTC+1). Synchronizes global National Yield cycles across all nations.
- **Country & World Entities**: Models sovereign countries, provinces, demographics, resources, and geography.
- **National Yield Engine**: Calculates measurable state output across 14 categories based on actual country conditions:
  - REPU Treasury Revenue
  - Science / Research Capacity
  - Industrial Capacity
  - Energy Capacity
  - Infrastructure Capacity
  - Human Capital
  - Administrative Capacity
  - Military Readiness
  - Intelligence Capacity
  - Diplomatic Capacity
  - Innovation Capacity
  - Financial Capacity
  - Security Capacity
  - Natural Resource Output
- **State Capacity Engine**: Computes conversion efficiency based on government effectiveness, civil service competence, corruption, institutional strength, and legitimacy.
- **Emergent Food Economy**: Simulates private agricultural production fostered by government-enabled infrastructure (irrigation, roads, research, credit) rather than direct yield handouts.
- **Persistence & Checksum System**: Saves/loads state with SHA256 integrity checks; handles offline time catch-up calculations.

**Characteristics**:
- Completely independent of UI and game engines.
- Deterministic, high-performance C# domain logic.
- Capable of running headless in server, CLI, or test harness environments.

---

### Layer 2: Gameplay Systems Layer (Rules & Inter-Country Systems)

**Responsibility**: Enforce constitutional rules, inter-country relations, and systemic events.

**Key Subsystems**:
- **Cabinet & Appointments Engine**: Manages appointment of ministers with authentic trade-offs (e.g., technical competence vs. political loyalty vs. integrity). Vacant or poorly staffed portfolios degrade sector capacity and yield.
- **Development Project Manager**: Tracks infrastructure projects with REPU costs, build durations, and maintenance costs. Construction progresses continuously across real time, independent of scheduled National Yield cycles.
- **Diplomacy & Treaty System**: Models bilateral trade contracts, non-aggression pacts, defense alliances, and diplomatic summits between sovereign nations.
- **Military & Defense Systems**: DEFCON readiness, armed branch mobilization, border deterrence, and strategic directives.
- **Legislature & Constitutional System**: Parliamentary bills, coalition politics, and constitutional amendments.
- **Rival AI & Autonomy**: Simulates foreign nations with distinct strategic behavioral postures.

**Characteristics**:
- Stateless rule enforcement derived from Layer 1 entity states.
- Event-driven communication via decoupled `IEventBus`.

---

### Layer 3: Communication Layer (Information & Briefings)

**Responsibility**: Translate deep simulation dynamics into executive intelligence and actionable player briefings.

**Key Subsystems**:
- **National Briefing Engine**: Compiles a concise briefing of revenues collected, projects completed, diplomatic overtures, and urgent crises that unfolded while the player was offline.
- **Classified Intelligence Dossiers**: Summarizes covert surveillance and military intelligence filtered by national intelligence capacity (fog of war).
- **Executive Desk Channels**: Dispatches memoranda, phone calls, cabinet alerts, and official visitors to the presidential desk.
- **Media & Press System**: Generates news broadcasts and monitors public sentiment.

**Characteristics**:
- Derives from lower layers without altering simulation state directly.
- Caches and formats information for executive consumption.

---

### Layer 4: Decision Space Layer (Agency & Trade-offs)

**Responsibility**: Provide structured executive decision spaces where choices involve explicit trade-offs.

**Key Subsystems**:
- **Action Registry & Validator**: Verifies legal authority, constitutional mandates, and REPU treasury availability for proposed decrees.
- **Trade-Off Calculator (Pros & Cons)**: Evaluates and displays the unavoidable costs and benefits of any decision (e.g., economic growth vs. political stability; military spending vs. education; competence vs. loyalty).
- **Decree & Policy Queue**: Schedules executive orders and project commitments for deterministic execution in the simulation loop.

**Characteristics**:
- Enforces the core principle: **Every important decision has pros and cons.**
- Prevents simplistic "optimal stat" exploitation.

---

### Layer 5: Presentation Layer (Executive Command Interface)

**Responsibility**: Visual presentation and player interaction.

**Key Subsystems**:
- **Executive Suite Shell**: The presidential office environment acting as the player's command centre.
- **Interactive Desk Items**: Phone, computer, dossier binders, and calendar linked to executive channels.
- **Decision Space Canvases**: Dedicated views for the Cabinet Room, Situation Room, and National Map.
- **Audiovisuals & Lighting**: Office atmosphere, ambient audio, and responsive visual feedback.

**Characteristics**:
- Presentation-only; communicates with lower layers exclusively through the `RepublicUnityBridge` or host presenter interfaces.
- Can be adapted, swapped, or operated via CLI without altering game simulation rules.

---

## 3. Data Flow Architecture

```
[Player Interaction in Presidential Office]
                 ↓
      (Layer 5: Presentation)
                 ↓
      (Layer 4: Decision Space)  ──► Validates REPU Budget & Displays Pros/Cons
                 ↓
      (Layer 2: Gameplay Rules)  ──► Enforces Constitutional & Cabinet Rules
                 ↓
      (Layer 1: Simulation Engine) ──► Advances Continuous Real-Time Tick (WAT)
                 ↓                     Calculates National Yield & State Capacity
                 ↓                     Progresses Development Projects
                 ↓
[World State Updates & Yield Cycles]
                 ↓
      (Layer 3: Communication)   ──► Synthesizes Intelligence & National Briefing
                 ↓
      (Layer 5: Presentation)    ──► Updates Desk Telemetry & Alerts
```

---

## 4. Key Architectural Guarantees

1. **Deterministic Core**: The simulation logic in `src/Republic.Core` remains deterministic and testable through automated xUnit test suites without runtime presentation coupling.
2. **Multiplayer Integrity**: All entity schemas and persistence containers assume multi-country concurrency and a shared world timeline.
3. **Monetary Consistency**: All economic systems denominate currency exclusively in **REPU** (`R`).
4. **Continuous Simulation**: Game time strictly aligns with real-world dates on West Africa Time (WAT / UTC+1), running continuously during offline periods.
5. **Emergent Economy**: Food and consumer goods are produced by private citizens and businesses supported by state-funded enabling infrastructure, rather than through direct yield deposits.
