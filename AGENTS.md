# AGENTS.md — Republic Game

This file provides guidance for AI agents (GitHub Copilot, coding agents, etc.) working in this repository.

---

## Project Overview

**Republic** is a persistent, multiplayer geopolitical strategy and democratic governance simulation game built in Unity 6 with C#. Players campaign for office, govern procedurally generated nations, manage economies, negotiate diplomacy, and compete for geopolitical dominance through governance rather than conquest.

- **Engine**: Unity 6
- **Language**: C#
- **Runtime**: .NET 6.0+
- **Status**: Pre-Production — Wave 0 (Foundation) in progress

---

## Repository Structure

```
Republic-Game/
├── Assets/                    # Unity project assets
│   ├── _Project/              # Game-specific assets (scripts, scenes, prefabs, etc.)
│   ├── ThirdParty/            # Third-party plugins
│   ├── Art/                   # Art assets
│   ├── Audio/                 # Audio assets
│   └── Tests/                 # Unity test assets
├── Docs/                      # Design and technical documentation
│   ├── Architecture/          # System architecture decisions
│   ├── Features/              # Feature specifications per Production Wave
│   ├── Gameplay Philosophy/   # Design manifesto and principles
│   ├── Roadmap/               # Wave-based development timeline
│   ├── TDD/                   # Technical Design Documents
│   ├── GDD/                   # Game Design Documents
│   └── UX/                    # Player experience and UI design
├── .github/                   # GitHub/studio operational guides
│   ├── REPUBLIC_STUDIO.md     # Overview of how Republic is developed
│   ├── PRODUCTION_WAVES.md    # The 10-wave development strategy
│   └── REPUBLIC_STUDIO_SETUP.md  # GitHub setup and workflow guide
├── PRODUCT_BACKLOG.md         # Living feature backlog organized by Epics
├── SPRINT_1.md                # Sprint 1 plan and tasks
└── AGENTS.md                  # This file
```

---

## Architecture

Republic uses a layered, event-driven architecture. All C# code is organized into these layers:

```
Source/
├── Core/           # Layer 1: Simulation Engine (Time, EventBus, WorldManager, SaveSystem)
├── Gameplay/       # Layer 2: Gameplay Systems (Country, Government, Economy, Population)
├── Communication/  # Layer 3: Event/query/notification systems
├── UI/             # Layer 4: Player Interface
├── AI/             # Layer 5: AI & Autonomous Agents
└── Networking/     # Layer 6: Multiplayer & State Synchronization
```

### Key Design Patterns
- **Event-Driven**: Systems communicate via an `EventBus` (publish/subscribe). Never create direct dependencies between systems where an event would suffice.
- **Entity-Component System**: Flexible entity composition; avoid monolithic classes.
- **State Machines**: Countries and key entities use explicit state machines with defined transitions.
- **Dependency Injection**: Constructor injection throughout; avoid service locators.

### Technology Stack
- **Testing**: NUnit / xUnit — minimum 90% unit test coverage for all critical systems
- **Persistence**: JSON + binary formats
- **Networking**: Netcode for GameObjects (or custom, TBD)
- **UI**: Unity UI / ImGui Pro

---

## Development Workflow

### Production Waves

All work is organized into 10 sequential Production Waves. **Do not implement Wave N+1 systems while Wave N is incomplete.**

| Wave | Name | Milestone Gate |
|------|------|----------------|
| 0 | Foundation | "The skeleton works" |
| 1 | Executive Workspace | "The player has an office" |
| 2 | World Simulation | "The world lives without a player" |
| 3 | Campaign | "You can run for office" |
| 4 | Government | "You have the power to govern" |
| 5 | Economy | "Your economy responds to your rule" |
| 6 | Legislature | "You must work with parliament" |
| 7 | Military | "You control military power" |
| 8 | Diplomacy | "You can shape international politics" |
| 9 | Multiplayer | "Politics becomes personal" |

Wave 0 is currently **In Progress**.

### Branch Naming

```
feature/epic-XX-feature-name
feature/wave-N-system-name
```

### Commit Messages

Reference the related issue or Wave in commit messages, e.g.:

```
feat(EventBus): implement publish/subscribe system [Wave-0]
fix(TimeSystem): correct fixed tick rate calculation
```

### Pull Requests

- Link PRs to the corresponding GitHub Issue
- PRs require code review and CI passing before merge
- All CI checks must pass (no compiler warnings)

---

## Coding Conventions

- **Language**: C# — follow standard C# naming conventions (PascalCase for types/methods, camelCase for fields/locals)
- **Namespaces**: Follow the layer structure, e.g. `Republic.Core.EventBus`, `Republic.Gameplay.Country`
- **No compiler warnings**: All code must build cleanly
- **Unit tests required**: >90% coverage for all Core and Gameplay systems
- **XML doc comments**: Public APIs must have `/// <summary>` documentation

### EventBus Usage Pattern

```csharp
// Define an event
public class GameStartedEvent : IEvent { }

// Subscribe
eventBus.Subscribe<GameStartedEvent>(e => Debug.Log("Game started!"));

// Publish
eventBus.Publish(new GameStartedEvent());
```

### Logging Pattern

```csharp
logger.LogInfo("Game initialized");
logger.LogDebug($"Frame time: {deltaTime}ms");
logger.LogError("Failed to load resource", exception);
```

---

## The Republic Test

Before implementing any feature, verify it answers **yes** to most of:

1. Does it strengthen the executive leadership experience?
2. Does it create meaningful strategic choices?
3. Does it affect multiple systems?
4. Does it fit within the office-centric design philosophy?
5. Does it increase replayability?
6. Does it create opportunities for emergent storytelling?
7. Does it reinforce the player's role as Head of Government?

If a feature fails most of these questions — redesign or omit it.

---

## Key Documents for Agents

| Document | Purpose |
|----------|---------|
| `.github/REPUBLIC_STUDIO.md` | Complete overview of how Republic is developed |
| `.github/PRODUCTION_WAVES.md` | Full wave strategy with objectives and milestones |
| `Docs/Architecture/README.md` | System architecture and layer descriptions |
| `Docs/Features/README.md` | Feature specifications by wave |
| `Docs/Roadmap/README.md` | Timeline and current wave status |
| `PRODUCT_BACKLOG.md` | Living backlog organized by Epic |
| `SPRINT_1.md` | Current sprint tasks and definition of done |

---

## Definition of Done

Every task must satisfy:

- [ ] All sub-tasks completed
- [ ] Code follows C# style conventions
- [ ] Unit tests written (>90% coverage for critical components)
- [ ] Code reviewed and approved
- [ ] Documentation updated
- [ ] Builds successfully (CI/CD passes, no compiler warnings)
- [ ] Changes merged to main branch

---

*Republic is built in Production Waves. GitHub is the studio. Build each Wave completely before starting the next.*
