# Wave 00: Foundation

**The skeleton that everything else hangs on.**

**Duration**: 2-3 weeks

**Status**: 🟡 In Progress

**Milestone Gate**: "The skeleton works"

---

## Objective

Build the core infrastructure. Nothing visible. Only architecture.

By the end of Wave 0:
- Project builds without warnings
- Time engine runs
- Event bus works
- Simulation can step forward
- Save system saves data
- Everything is ready for Wave 1

---

## Systems to Build

### 1. Project Bootstrap & Build Pipeline
- Unity project structure
- Build system setup
- Dependency management
- Asset organization

### 2. Logging System
- Debug logging
- Error reporting
- Performance monitoring
- Log file writing

### 3. Configuration Manager
- Loading game config
- Game settings
- World generation parameters
- Difficulty settings

### 4. Save System (Framework)
- Save slot management
- Save data structure
- Load/save operations
- Version management

### 5. Event Bus
- Publish/subscribe system
- Decoupled communication
- Event routing
- Listener management

### 6. Time Engine
- 60 tick/second simulation
- Frame-rate independent stepping
- Time scaling (fast-forward, pause)
- Turn-based integration

### 7. Simulation Core
- Deterministic stepping
- Serialization for multiplayer
- Replay system support
- State rollback

### 8. Dependency Injection
- Service container
- Dependency resolution
- Scope management
- Constructor injection

### 9. Game Loop
- Update → Render cycle
- Frame timing
- Input handling
- Scene transitions

### 10. Folder Structure
- Assets organization
- Code organization
- Documentation structure
- Test structure

---

## Deliverables

By end of Wave 0, we have:
✅ Clean, buildable Unity project
✅ All core systems functional
✅ No compiler warnings
✅ Basic logging working
✅ Time engine stepping at 60 Hz
✅ Event system routing messages
✅ Save/load framework in place
✅ Dependency injection ready
✅ Ready to build Wave 1

---

## Technical Specs

**See**: `Technical Specs.md` (in this directory)

---

## Integration Points

Wave 0 systems are used by:
- **Wave 1**: Office interactions use event bus and time engine
- **Wave 2**: Simulation uses time engine and save system
- **Wave 3-9**: All systems use Wave 0 as foundation

---

## Success Criteria

✅ Project builds without errors or warnings
✅ Time engine runs at 60 Hz
✅ Event bus successfully routes 100 events/frame
✅ Save system serializes data correctly
✅ Dependency injection resolves all dependencies
✅ Logging captures all errors
✅ No memory leaks
✅ Ready to accept Wave 1 systems

---

## GitHub Epics

- Epic: Project Bootstrap
- Epic: Time Engine
- Epic: Event Bus
- Epic: Save System
- Epic: Dependency Injection
- Epic: Logging & Monitoring

---

## Related Documents

- **[TDD](../../TDD/README.md)** - Technical specifications
- **[Development Bible](../README.md)** - Master roadmap
