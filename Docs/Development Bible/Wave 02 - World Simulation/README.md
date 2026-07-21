# Wave 02: World Simulation

**A living world that exists without the player.**

**Duration**: 4-6 weeks

**Status**: 🔴 Not Started

**Depends On**: Wave 0 (Foundation) + Wave 1 (Office to view data)

**Milestone Gate**: "The world lives without a player"

---

## Objective

Build the simulation engine. Countries exist. Populations grow. Resources flow. Currencies fluctuate. Everything happens without the player's input.

By the end of Wave 2:
- Countries exist with full attributes
- Simulation runs deterministically
- Population changes over time
- Resources are produced and consumed
- Trade flows between countries
- Player sees this through office reports
- Ready for Wave 3 (player campaigns to lead)

---

## Systems to Build

### 1. World Manager
- World entity lifecycle
- Country spawning
- Time progression
- Serialization for saves

### 2. Country Entity
- Country data model
- Attributes (population, wealth, etc)
- Resources & inventory
- Government type
- Constitutional rules
- Political culture

### 3. Geography System
- Procedural terrain generation
- Resource distribution
- Climate zones
- Regional division
- Trade route generation

### 4. Population System
- Demographics tracking
- Population growth
- Migration between regions
- Population needs (food, etc)
- Happiness tracking

### 5. Resources System
- Resource types
- Supply/demand
- Production rates
- Consumption rates
- Stockpile management

### 6. Currency System
- Currency per country
- Exchange rates
- Inflation/deflation
- Central bank operations
- Wealth distribution

### 7. Government Types
- Democracy
- Dictatorship
- Monarchy
- Other government types
- Government attributes

### 8. Constitution System
- Constitutional rules
- Power limitations
- Succession rules
- Constitutional amendments
- Enforcement

### 9. Political Culture
- National values
- Cultural traits
- Traditions
- Education level
- Cultural identity

### 10. Economic Foundation
- Production chains
- Basic trade
- Market prices
- Economic growth
- Economic crises

---

## Deliverables

By end of Wave 2, we have:
✅ Procedurally generated world
✅ Multiple countries with unique attributes
✅ Simulation running deterministically
✅ Population changing over time
✅ Resources flowing
✅ Trade working
✅ Office reports showing world data
✅ Save/load preserves simulation state
✅ Ready for player campaigns (Wave 3)

---

## Technical Specs

**See**: `Technical Specs.md` (in this directory)

---

## Integration Points

Wave 2 integrates with:
- **Wave 0**: Uses time engine, event bus, save system
- **Wave 1**: Office displays reports from simulation
- **Wave 3**: Player campaigns within this world
- **Wave 4**: Player governs this world
- **Wave 5+**: All consequences ripple from Wave 2

---

## Success Criteria

✅ World generates consistently
✅ Simulation runs at stable speed
✅ Population trends realistic
✅ Resources flow realistically
✅ Trade working between countries
✅ No simulation crashes
✅ Deterministic (same seed = same results)
✅ Office reports accurate
✅ Save/load works correctly
✅ Ready for Wave 3

---

## GitHub Epics

- Epic: World Manager
- Epic: Country Entity
- Epic: Geography System
- Epic: Population System
- Epic: Resources System
- Epic: Economy Foundation
- Epic: Government Types
- Epic: Constitution System

---

## Related Documents

- **[ADR-004: Procedural Countries](../../Architecture%20Decision%20Records/ADR-004%20Procedural%20Countries.md)**
- **[Development Bible](../README.md)** - Master roadmap
