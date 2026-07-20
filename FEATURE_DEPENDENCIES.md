# Republic Game - Feature Dependencies

A comprehensive dependency map showing feature relationships and implementation order.

**Status**: In Development | **Last Updated**: 2026-07-20

---

## Dependency Graph Legend

```
Feature A
  ↓
Feature B (depends on A)
  ↓
Feature C (depends on B)
```

Dependencies are directional: features lower in the chain must be implemented first.

---

## EPIC-01: Foundation Dependencies

### 1.1 Project Bootstrap
```
Project Bootstrap
  ├─ (No dependencies)
  └─ Foundation for all other features
```
**Status**: Must be first  
**Blocks**: All other features

---

### 1.2 Logging
```
Logging
  ↓
Project Bootstrap
```
**Status**: Early infrastructure  
**Blocks**: Debugging tools, all systems

---

### 1.3 Event Bus
```
Event Bus
  ↓
Project Bootstrap
  ↓
Logging (optional - for debug events)
```
**Status**: Core communication layer  
**Blocks**: World Manager, Country Entity, all simulation systems

---

### 1.4 Time Simulation
```
Time Simulation
  ↓
Project Bootstrap
  ↓
Event Bus (emits time events)
```
**Status**: Core simulation engine  
**Blocks**: Save System, all time-dependent features

---

### 1.5 Save System
```
Save System
  ↓
Project Bootstrap
  ↓
Time Simulation (needs to save time state)
  ↓
Event Bus (listens for save events)
```
**Status**: Persistence foundation  
**Blocks**: World Manager, Country Entity, all saveable entities

---

### 1.6 World Manager
```
World Manager
  ↓
Project Bootstrap
  ↓
Event Bus (publishes entity lifecycle events)
  ↓
Save System (persists world state)
```
**Status**: Entity container  
**Blocks**: Country Entity, Geography, all world entities

---

### 1.7 Country Entity
```
Country Entity
  ↓
Project Bootstrap
  ↓
Event Bus (publishes country events)
  ↓
Save System (serializes country state)
  ↓
Time Simulation (time-dependent properties)
  ↓
World Manager (contained by world)
```
**Status**: Core game entity  
**Blocks**: Constitution, Political Culture, all country-specific features

---

### 1.8 Configuration
```
Configuration
  ↓
Project Bootstrap
  ↓
(Optional dependency on all systems for config values)
```
**Status**: Can be done in parallel with other foundation work  
**Blocks**: Balance tuning, feature constants

---

## EPIC-01 Foundation Build Order

```
1. Project Bootstrap
   ↓
2. Logging (parallel)
3. Event Bus (can start after Project Bootstrap)
   ↓
4. Time Simulation
5. Configuration (parallel)
   ↓
6. Save System (depends on Time Simulation)
   ↓
7. World Manager (depends on Save System, Event Bus)
   ↓
8. Country Entity (depends on World Manager, Save System, Time Simulation)
```

**Estimated Timeline**: 2-3 weeks for all foundation features

---

## EPIC-02: World Simulation Dependencies

### 2.1 Country Generation
```
Country Generation
  ↓
Project Bootstrap
  ↓
Country Entity
  ↓
Configuration (generation parameters)
  ↓
Event Bus (generation events)
```
**Status**: Depends on Foundation complete  
**Blocks**: Game initialization, world loading

---

### 2.2 Geography
```
Geography
  ↓
Project Bootstrap
  ↓
World Manager
  ↓
Event Bus (terrain change events)
  ↓
Save System (persistent map data)
```
**Status**: Can start with World Manager  
**Blocks**: Population, Resources (location-based features)

---

### 2.3 Population
```
Population
  ↓
Country Entity
  ↓
Geography (population in regions)
  ↓
Time Simulation (growth calculations per tick)
  ↓
Event Bus (population events)
  ↓
Save System (serialize population data)
```
**Status**: Depends on Country Entity and Geography  
**Blocks**: Resources consumption, Culture spread

---

### 2.4 Resources
```
Resources
  ↓
Country Entity
  ↓
Geography (location-based resources)
  ↓
Population (resource consumers)
  ↓
Time Simulation (production per tick)
  ↓
Event Bus (resource events)
  ↓
Configuration (resource definitions)
```
**Status**: Depends on Country, Geography, Population  
**Blocks**: Trade, Economics, Taxation

---

### 2.5 Currency
```
Currency
  ↓
Country Entity
  ↓
Time Simulation (inflation per tick)
  ↓
Configuration (currency parameters)
  ↓
Event Bus (transaction events)
  ↓
Save System (treasury state)
```
**Status**: Can start early in EPIC-02  
**Blocks**: Trade, Taxation, Banking

---

### 2.6 Constitution
```
Constitution
  ↓
Country Entity
  ↓
Configuration (government types, rules)
  ↓
Event Bus (law change events)
  ↓
Save System (constitutional state)
```
**Status**: Parallel to Population/Resources  
**Blocks**: Politics, Elections, Internal Conflicts

---

### 2.7 Political Culture
```
Political Culture
  ↓
Country Entity
  ↓
Population (cultural spread through population)
  ↓
Constitution (government affects culture)
  ↓
Time Simulation (cultural drift per tick)
  ↓
Event Bus (culture change events)
```
**Status**: Later in EPIC-02 (depends on Population, Constitution)  
**Blocks**: Factions, Revolutions, Political AI

---

## EPIC-02 World Simulation Build Order

```
Foundation (EPIC-01) Complete
  ↓
2.1 Country Generation
2.2 Geography
2.5 Currency (parallel)
   ↓
2.3 Population (depends on Geography)
2.6 Constitution (parallel)
   ↓
2.4 Resources (depends on Geography, Population)
   ↓
2.7 Political Culture (depends on Population, Constitution)
```

**Estimated Timeline**: 3-4 weeks after foundation complete

---

## Critical Dependencies Summary

### Must Complete FIRST (Foundation Prerequisites)
1. **Project Bootstrap** — Everything depends on this
2. **Event Bus** — Core communication; needed by most systems
3. **Time Simulation** — Enables time-dependent mechanics
4. **Save System** — Enables persistence; blocks entity implementations
5. **World Manager** — Entity container; blocks all world entities

### Foundation Must Complete Before EPIC-02
- All of EPIC-01 features must be stable before starting EPIC-02

### EPIC-02 Build Phases

**Phase A (Early)**
- Country Generation
- Geography
- Currency

**Phase B (Mid)**
- Population (depends on Geography)
- Constitution (independent)

**Phase C (Late)**
- Resources (depends on Geography, Population)
- Political Culture (depends on Population, Constitution)

---

## EPIC-03: Economy Dependencies

```
Economy Features
  ↓
All of EPIC-02 (Foundation + World Simulation)
  ↓
Specifically requires:
  - Currency (from EPIC-02)
  - Resources (from EPIC-02)
  - Population (from EPIC-02)
  - Country Entity (from EPIC-01)
```

**Key Dependencies**:
- 3.1 Production Systems → Resources, Population
- 3.2 Trade Networks → Currency, Resources, Country Entity
- 3.3 Markets & Prices → Currency, Resources, Production
- 3.4 Labour & Employment → Population, Constitution
- 3.5 Taxation & Revenue → Currency, Country Entity
- 3.6 Banking & Credit → Currency, Country Entity
- 3.7 Guilds & Organizations → Country Entity, Constitution

---

## EPIC-04: Politics & Governance Dependencies

```
Politics Features
  ↓
All of EPIC-02 (Foundation + World Simulation)
  ↓
Specifically requires:
  - Constitution (from EPIC-02)
  - Political Culture (from EPIC-02)
  - Population (from EPIC-02)
  - Country Entity (from EPIC-01)
```

**Key Dependencies**:
- 4.1 Factions & Parties → Political Culture, Population
- 4.2 Diplomacy → Country Entity, Constitution
- 4.3 Elections & Succession → Constitution, Political Culture
- 4.4 Laws & Decrees → Constitution, Country Entity
- 4.5 Internal Conflicts → Political Culture, Factions
- 4.6 Revolutions & Coups → Political Culture, Internal Conflicts

---

## EPIC-05: Military Dependencies

```
Military Features
  ↓
All of EPIC-02 (Foundation + World Simulation)
  ↓
EPIC-03 Economy (for supply/logistics)
  ↓
Specifically requires:
  - Country Entity (from EPIC-01)
  - Geography (from EPIC-02)
  - Population (for troops)
  - Resources (for weapons, supplies)
```

**Key Dependencies**:
- 5.1 Units & Forces → Country Entity, Geography, Population
- 5.2 Warfare & Combat → Units, Geography
- 5.3 Military Strategy AI → AI Architecture, Units
- 5.4 Naval Systems → Units, Geography
- 5.5 Logistics & Supply → Resources, Production

---

## EPIC-06: AI & Decision Making Dependencies

```
AI Features
  ↓
All previous EPICs (1-5)
  ↓
Specifically requires:
  - All world simulation systems (EPIC-02)
  - All economy systems (EPIC-03)
  - All politics systems (EPIC-04)
  - All military systems (EPIC-05)
  - Event Bus (for decision triggers)
```

**Key Dependencies**:
- 6.1 AI Architecture → Event Bus, Country Entity
- 6.2 Economic AI → Economy features, AI Architecture
- 6.3 Political AI → Politics features, AI Architecture
- 6.4 Military AI → Military features, AI Architecture
- 6.5 Diplomatic AI → Politics features, AI Architecture

---

## EPIC-07: Networking & Multiplayer Dependencies

```
Networking Features
  ↓
All previous EPICs stable (1-6)
  ↓
Specifically requires:
  - Save System (for state serialization)
  - Event Bus (for distributed events)
  - Time Simulation (synchronized ticks)
  - All world entities (Countries, etc)
```

**Key Dependencies**:
- 7.1 Network Protocol → Save System, Event Bus
- 7.2 Server Architecture → Project Bootstrap, Network Protocol
- 7.3 State Synchronization → Save System, Time Simulation
- 7.4 Player Sessions → Server Architecture
- 7.5 Turn-based Synchronization → Time Simulation, State Sync

---

## EPIC-08: UI & Visualization Dependencies

```
UI Features
  ↓
All previous EPICs (1-7)
  ↓
Can start development in parallel with other systems
  ↓
Specifically requires:
  - All world data (EPIC-02)
  - Game state (Save System)
  - UI Framework (EPIC-08.2)
```

**Key Dependencies**:
- 8.1 Map Visualization → Geography, Event Bus
- 8.2 UI Framework → Project Bootstrap
- 8.3 Country Management UI → Country Entity, Constitution
- 8.4 Reports & Analytics → All systems

---

## Overall Build Sequence

```
PHASE 1: Foundation (EPIC-01)
├─ 1.1 Project Bootstrap
├─ 1.2 Logging
├─ 1.3 Event Bus
├─ 1.4 Time Simulation
├─ 1.5 Save System
├─ 1.6 World Manager
├─ 1.7 Country Entity
└─ 1.8 Configuration
   Estimated: 2-3 weeks

   ↓

PHASE 2: World Simulation (EPIC-02)
├─ 2.1 Country Generation
├─ 2.2 Geography
├─ 2.5 Currency
├─ 2.3 Population
├─ 2.6 Constitution
├─ 2.4 Resources
└─ 2.7 Political Culture
   Estimated: 3-4 weeks

   ↓

PHASE 3: Economy (EPIC-03)
├─ 3.1 Production Systems
├─ 3.2 Trade Networks
├─ 3.3 Markets & Prices
├─ 3.4 Labour & Employment
├─ 3.5 Taxation & Revenue
├─ 3.6 Banking & Credit
└─ 3.7 Guilds & Organizations
   Estimated: 4-5 weeks

   ↓

PHASE 4: Politics (EPIC-04)
├─ 4.1 Factions & Parties
├─ 4.2 Diplomacy
├─ 4.3 Elections & Succession
├─ 4.4 Laws & Decrees
├─ 4.5 Internal Conflicts
└─ 4.6 Revolutions & Coups
   Estimated: 4-5 weeks

   ↓

PHASE 5: Military (EPIC-05)
├─ 5.1 Units & Forces
├─ 5.2 Warfare & Combat
├─ 5.3 Military Strategy AI
├─ 5.4 Naval Systems
└─ 5.5 Logistics & Supply
   Estimated: 4-5 weeks

   ↓

PHASE 6: AI & Decision Making (EPIC-06)
├─ 6.1 AI Architecture
├─ 6.2 Economic AI
├─ 6.3 Political AI
├─ 6.4 Military AI
└─ 6.5 Diplomatic AI
   Estimated: 4-6 weeks

   ↓

PHASE 7: Networking (EPIC-07)
├─ 7.1 Network Protocol
├─ 7.2 Server Architecture
├─ 7.3 State Synchronization
├─ 7.4 Player Sessions
└─ 7.5 Turn-based Synchronization
   Estimated: 4-6 weeks

   ↓

PHASE 8: UI & Visualization (EPIC-08)
├─ 8.1 Map Visualization
├─ 8.2 UI Framework
├─ 8.3 Country Management UI
└─ 8.4 Reports & Analytics
   Estimated: 4-6 weeks

Total Estimated Duration: 6-8 months for MVP with all features
```

---

## Parallel Work Opportunities

While waiting for dependencies, these features can be worked on in parallel:

**During EPIC-01**:
- Configuration can be built alongside other systems
- Logging can be built early
- Documentation can start immediately

**During EPIC-02**:
- Geography and Country Generation can proceed in parallel
- Constitution can be built independently

**During EPIC-03 & beyond**:
- UI framework (EPIC-08.2) can start early
- Basic AI Architecture (EPIC-06.1) can be designed

---

## Dependency Breaking Strategy

If certain features are blocking progress:

1. **Create stub implementations** to satisfy dependencies
2. **Use dependency injection** to swap real implementations later
3. **Mock external systems** during development
4. **Iterate on dependencies** without blocking dependent features

Example:
```
Need Country Entity quickly?
├─ Create minimal Country stub
├─ Later enhance with full properties
├─ Other systems can depend on stub immediately
└─ Upgrade Country Entity once complete
```

---

*This dependency map guides implementation order and helps identify blockers early.*
