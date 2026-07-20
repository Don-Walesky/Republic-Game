# Technical Design Document - Architecture

**Version**: 2.0  
**Status**: Active  
**Last Updated**: 2026-07-20

---

## Architectural Overview

Republic's architecture is built in **five distinct layers**, each serving a specific purpose in the player interaction model. This layered architecture ensures **clean separation of concerns** and allows each layer to evolve independently.

---

## The Five Layers

### Layer 1: Simulation Engine (Foundation)

**Responsibility**: Pure simulation without player agency or presentation

**What it does**:
- Runs deterministic simulation ticks
- Calculates economic production, population growth, military logistics
- Applies environmental and systemic effects
- Generates autonomous AI decisions
- Updates world state based on rules

**Key Components**:
- Time System (deterministic tick rate)
- Entity management (countries, regions, populations)
- Physics/economic calculations
- Random number generation (seeded for replays)

**Characteristics**:
- No UI dependencies
- Completely deterministic (same input = same output)
- Can run offline/headless
- Runs at fixed time step (60 ticks/second)

**Example**: *The economy produces resources. Population grows. Military units advance. This happens whether the player is watching or not.*

---

### Layer 2: Gameplay Systems (Rules & Events)

**Responsibility**: Transform simulation state into meaningful game rules and events

**What it does**:
- Interprets simulation results as game rules
- Generates game events from simulation state changes
- Manages victory/defeat conditions
- Enforces turn structures (if applicable)
- Coordinates cross-system interactions

**Key Components**:
- Event bus (system-to-system communication)
- Rule engine (game rules derived from simulation)
- Victory/defeat conditions
- Turn/phase management
- System integration coordinator

**Characteristics**:
- Stateless (derives everything from simulation)
- Event-driven (responds to simulation changes)
- Extensible (new rules added without changing simulation)
- No UI dependencies (yet)

**Example**: *When population satisfaction drops below 50%, the rule engine triggers an "Unrest" event. This event can then be communicated to the player and affect their decisions.*

---

### Layer 3: Communication Layer (Information Flow)

**Responsibility**: Translate simulation and game state into player-understandable information

**What it does**:
- Analyzes game state and extracts relevant information
- Generates player-facing messages and alerts
- Creates reports and summaries
- Formats data for UI consumption
- Manages information privacy/fog of war

**Key Components**:
- State analyzer (what information is relevant?)
- Message generator (how to communicate it?)
- Report system (aggregated information)
- Information broker (privacy/visibility rules)
- Analytics engine (metrics for decision-making)

**Characteristics**:
- Derives from lower layers (doesn't change them)
- Generates player-facing output
- Implements "fog of war" (what player can know)
- Provides multiple data views (detailed, summary, trending)

**Example**: *The communication layer sees that a neighboring country's military is mobilizing. It generates an alert: "Nation X has moved 5,000 troops to border." It also generates a report showing military readiness trends. But it only shows information based on player's intelligence capabilities.*

---

### Layer 4: Decision Space Layer (Agency)

**Responsibility**: Define what decisions are available to the player and their constraints

**What it does**:
- Determines which actions are available
- Validates player decisions against rules
- Queues player decisions for execution
- Manages action costs (resources, time, political capital)
- Provides decision information (outcomes, side effects)

**Key Components**:
- Action registry (all possible player actions)
- Action validator (is this action legal?)
- Action queue (pending player decisions)
- Cost calculator (what does this action cost?)
- Consequence predictor (what will happen?)

**Characteristics**:
- Defines player agency
- Enforces constraints
- Transparent (player sees costs and consequences)
- Modular (new actions can be added)

**Example**: *Player wants to lower taxes. Decision Space Layer checks: Is the government able to do this? What will happen to treasury? What will happen to population happiness? What are the military implications? It presents all this information so the player can make an informed decision. Then it queues the decision for execution next tick.*

---

### Layer 5: Presentation Layer (Player Interface)

**Responsibility**: Display game state and receive player input

**What it does**:
- Renders map, UI, and graphics
- Displays reports and information
- Receives player input (clicks, commands)
- Manages camera, scrolling, filtering
- Provides player feedback (animations, sounds, notifications)

**Key Components**:
- Map renderer
- UI framework
- Input handler
- Camera system
- Animation system
- Audio system

**Characteristics**:
- Purely presentational (doesn't affect simulation)
- Can be replaced without breaking game
- Multiple UI modes (tactical, strategic, diplomatic)
- Responsive to player input

**Example**: *Player sees the map. They see a button "Lower Taxes". They click it. The presentation layer sends that command to the Decision Space Layer. The Decision Space Layer validates it and queues it. Eventually, the Gameplay Systems layer executes it in the simulation. The Simulation Engine updates the world. The Communication Layer generates new information. The Presentation Layer displays the results.*

---

## Data Flow Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                        │
│              (UI, Graphics, Player Input)                   │
│                                                              │
│  Player clicks button → Input Queue → Actions              │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              DECISION SPACE LAYER                           │
│         (Agency, Choices, Constraints)                     │
│                                                              │
│  Validate Action → Calculate Costs → Queue Decision         │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│             COMMUNICATION LAYER                             │
│       (Information, Reporting, Analysis)                    │
│                                                              │
│  Extract Relevant Info → Generate Reports → Cache Data      │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│            GAMEPLAY SYSTEMS LAYER                           │
│       (Rules, Events, Integration)                          │
│                                                              │
│  Apply Decisions → Generate Events → Enforce Rules          │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│           SIMULATION ENGINE LAYER                           │
│      (Deterministic World Physics)                          │
│                                                              │
│  Execute Tick → Calculate Production → Update State         │
│  Generate Events → Create Autonomous Decisions             │
└─────────────────────────────────────────────────────────────┘
                     │
                     └──► World State Changes ──┐
                                                │
                         ┌──────────────────────┘
                         │
        ┌────────────────▼───────────────────┐
        │   Communication Layer reads new    │
        │   state and generates new info     │
        │   for next presentation cycle      │
        └────────────────────────────────────┘
```

---

## Key Principles

### 1. Layered Independence
Each layer should be independently testable and deployable. Changes to the Presentation Layer shouldn't require changes to the Simulation Engine.

### 2. Information Flows Down
- Upper layers depend on lower layers
- Lower layers don't depend on upper layers
- Simulation Engine is pure (no framework/UI dependencies)

### 3. Control Flows Down
Player input flows down through layers:
- Presentation (I want to do X)
- Decision Space (Is X valid? What does it cost?)
- Gameplay Systems (Execute X through the rules)
- Simulation Engine (X's effects on world)

### 4. State Flows Up
World state flows up through layers:
- Simulation Engine (here's what happened)
- Gameplay Systems (here's what it means)
- Communication Layer (here's how to understand it)
- Presentation (here's how to see it)

### 5. No Circular Dependencies
No layer should depend on a layer above it. This prevents circular dependencies and makes testing possible.

---

## Benefits of This Architecture

### For Development
- Each layer can be developed independently
- Clear interfaces between layers
- Easy to test (mock lower layers)
- New features can be added to any layer

### For Gameplay
- Clean separation between mechanics and presentation
- Easy to change how players interact without changing simulation
- Multiple UI modes (tactical view, strategic view, reporting view)
- Easy to implement fog of war, hidden information, etc.

### For Modding/Extensibility
- Modders can add features at any layer
- Clear extension points
- Can't accidentally break lower layers

### For Performance
- Simulation runs at fixed rate (60 ticks/sec)
- Presentation updates at variable rate (60 FPS)
- Communication layer caches information
- Each layer can be optimized independently

---

## System Responsibilities By Layer

### Simulation Engine
- Time system (ticks)
- Entity management
- Resource production
- Population dynamics
- Military logistics
- Environmental effects
- Autonomous AI decisions
- Random events

### Gameplay Systems
- Rule enforcement
- Victory/defeat conditions
- Event aggregation
- System integration
- Replay system
- Save/load system

### Communication Layer
- World state analysis
- Report generation
- Message formatting
- Information privacy (fog of war)
- Analytics and metrics
- Data caching

### Decision Space Layer
- Action registry and validation
- Cost calculation
- Consequence prediction
- Action queuing
- Constraint checking

### Presentation Layer
- Map rendering
- UI rendering
- Player input handling
- Camera/viewport management
- Animation and effects
- Audio

---

## Sprint 1 Implementation Focus

**Sprint 1 focuses on Layers 1-3 foundations:**

- **Simulation Engine**: Time System, basic entity management, event system
- **Gameplay Systems**: Event bus, system integration points
- **Communication Layer**: Logging system (precursor to information layer)

**Layers 4-5 come in later sprints** once the foundation is solid.

---

## This Architecture Guides All Future Decisions

- **Feature Design**: Where does this feature belong? Which layer(s)?
- **UI Design**: Is this presentation or decision-space?
- **System Integration**: Does this layer respect the architecture?
- **Testing**: Can we test each layer independently?
- **Performance**: Which layer is the bottleneck?

---

*Technical Design Document - Architecture v2.0*
*The guiding principle for all Republic technical decisions*
