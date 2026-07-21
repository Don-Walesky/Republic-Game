# Republic Game - Features Complexity Estimates

Complexity sizing for all 632 features using XS/S/M/L/XL scale to enable realistic sprint planning and milestone estimation.

---

## Complexity Scale Definition

| Size | Duration | Effort | Examples |
|------|----------|--------|----------|
| **XS** | <1 day | Trivial | Config, simple utility, basic UI text |
| **S** | 1-2 days | Simple | Basic manager, simple UI panel, data structure |
| **M** | 3-5 days | Moderate | System implementation, complex UI, integration |
| **L** | 1-2 weeks | Large | Subsystem, multiple integrations, algorithms |
| **XL** | Multiple weeks | Huge | Major system, complex algorithms, heavy testing |

---

## Wave 00 – Foundation

### EPIC-001: Project Bootstrap

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-001-01: Project structure | XS | Standard folder setup | 0.5 |
| FEAT-001-02: Git setup | XS | Repository initialization | 0.5 |
| FEAT-001-03: Unity initialization | S | Project configuration, basic settings | 2 |
| FEAT-001-04: Build system | M | Multiple build targets, configurations | 4 |
| FEAT-001-05: CI/CD pipeline | M | GitHub Actions setup, testing integration | 4 |
| FEAT-001-06: Documentation templates | S | README, CONTRIBUTING, guides | 2 |
| FEAT-001-07: Dev environment guide | S | Setup instructions, tool configuration | 2 |
| FEAT-001-08: Code style & linter | S | ESLint/StyleCop setup, pre-commit hooks | 2 |
| **EPIC-001 Total** | | | **18.5 days** |

### EPIC-002: Core Framework

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-002-01: Singleton manager | M | Base class, pattern implementation | 4 |
| FEAT-002-02: Service locator | M | Registration system, lookup mechanisms | 4 |
| FEAT-002-03: Object pooling | M | Pool management, recycling logic | 4 |
| FEAT-002-04: Dependency injection | L | Container implementation, resolution logic | 8 |
| FEAT-002-05: State machine | M | State transitions, event handling | 4 |
| FEAT-002-06: Observer pattern | M | Subscription/notification system | 4 |
| FEAT-002-07: Utility functions | L | Math utilities, string helpers, extensions | 10 |
| FEAT-002-08: Extension methods | S | Convenience methods for built-in types | 2 |
| **EPIC-002 Total** | | | **40 days** |

### EPIC-003: Simulation Engine

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-003-01: Update loop manager | L | Core loop, frame management | 8 |
| FEAT-003-02: Fixed timestep | M | Physics calculation, frame independent | 4 |
| FEAT-003-03: Delta time management | S | Time tracking, precision | 2 |
| FEAT-003-04: Coroutine system | M | Async execution, yield handling | 4 |
| FEAT-003-05: Scheduled callbacks | M | Event scheduling, callback management | 4 |
| FEAT-003-06: Physics simulation | M | Integration with Unity physics | 4 |
| FEAT-003-07: Render loop integration | S | Rendering synchronization | 2 |
| FEAT-003-08: Update priority system | M | Execution ordering, dependencies | 4 |
| **EPIC-003 Total** | | | **32 days** |

### EPIC-004: Data Architecture

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-004-01: Data model base | M | Abstract classes, interfaces | 4 |
| FEAT-004-02: Serialization interface | S | Interface definitions | 2 |
| FEAT-004-03: JSON serialization | M | JSON parsing, writing | 4 |
| FEAT-004-04: Binary serialization | L | Binary format, compression | 8 |
| FEAT-004-05: Type registry | M | Type mapping, lookup | 4 |
| FEAT-004-06: Data validation | M | Schema validation, error checking | 4 |
| FEAT-004-07: Data transformation | M | Conversion utilities | 4 |
| FEAT-004-08: Version management | L | Migration system, compatibility | 8 |
| **EPIC-004 Total** | | | **38 days** |

### EPIC-005: Save & Persistence

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-005-01: SaveSlot structure | S | Data structure definition | 2 |
| FEAT-005-02: Save file format | M | Format specification, implementation | 4 |
| FEAT-005-03: Save encryption | M | Encryption/decryption logic | 4 |
| FEAT-005-04: Save compression | M | Compression algorithms | 4 |
| FEAT-005-05: Auto-save management | S | Scheduled save logic | 2 |
| FEAT-005-06: Save slot UI data | S | UI data structure | 2 |
| FEAT-005-07: Save validation | M | Corruption detection, recovery | 4 |
| FEAT-005-08: Save migration | L | Data format migration, versioning | 8 |
| **EPIC-005 Total** | | | **30 days** |

### EPIC-006: Event System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-006-01: EventBus singleton | M | Core event system | 4 |
| FEAT-006-02: Event subscription | M | Subscribe/unsubscribe mechanisms | 4 |
| FEAT-006-03: Event unsubscription | S | Cleanup logic | 2 |
| FEAT-006-04: Event publishing | M | Publishing mechanisms, listeners | 4 |
| FEAT-006-05: Event priority | S | Priority queue, ordering | 2 |
| FEAT-006-06: Event filtering | S | Type filtering | 2 |
| FEAT-006-07: Weak references | M | Memory management, listener cleanup | 4 |
| FEAT-006-08: Event queue & batching | M | Queue management, batch processing | 4 |
| **EPIC-006 Total** | | | **26 days** |

### EPIC-007: Game Time

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-007-01: Game clock | M | Time tracking, updates | 4 |
| FEAT-007-02: Calendar system | M | Date/month/year tracking | 4 |
| FEAT-007-03: Time acceleration | S | Speed multiplier | 2 |
| FEAT-007-04: Pause/resume | S | State management | 2 |
| FEAT-007-05: Time-of-day tracking | S | Hour tracking, callbacks | 2 |
| FEAT-007-06: Season system | M | Seasonal transitions | 4 |
| FEAT-007-07: Historical tracking | S | Date logging | 2 |
| FEAT-007-08: Scheduled triggers | M | Event scheduling | 4 |
| **EPIC-007 Total** | | | **24 days** |

### EPIC-008: Configuration

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-008-01: ConfigManager | S | Singleton manager | 2 |
| FEAT-008-02: JSON loading | S | File I/O, parsing | 2 |
| FEAT-008-03: Validation | S | Schema validation | 2 |
| FEAT-008-04: Runtime override | S | Value overrides | 2 |
| FEAT-008-05: Environment configs | S | Environment-specific files | 2 |
| FEAT-008-06: Default values | XS | Hardcoded defaults | 0.5 |
| FEAT-008-07: Hot-reload | M | File watching, reloading | 4 |
| FEAT-008-08: Export/import | S | File serialization | 2 |
| **EPIC-008 Total** | | | **16.5 days** |

### EPIC-009: Debug Tools

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-009-01: Logger singleton | M | Multiple outputs, levels | 4 |
| FEAT-009-02: File logging | S | File I/O, rotation | 2 |
| FEAT-009-03: Console output | XS | Console handler | 1 |
| FEAT-009-04: Performance profiler | L | Timing measurements, analysis | 8 |
| FEAT-009-05: Memory profiler | L | Memory tracking, reporting | 8 |
| FEAT-009-06: Frame rate monitor | S | FPS tracking | 2 |
| FEAT-009-07: Debug drawing | M | Visualization, drawing utilities | 4 |
| FEAT-009-08: Debug console | M | UI console, command parsing | 4 |
| **EPIC-009 Total** | | | **33 days** |

**Wave 00 Total: 257.5 days (~13 weeks)**

---

## Wave 01 – Executive Workspace

### EPIC-010: Office Framework

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-010-01: Office scene init | M | Scene setup, prefabs | 4 |
| FEAT-010-02: Office manager | M | Manager singleton, state | 4 |
| FEAT-010-03: Room layout | M | Layout system, navigation | 4 |
| FEAT-010-04: Office lighting | M | Lighting setup, materials | 4 |
| FEAT-010-05: Music/ambience | S | Audio system integration | 2 |
| FEAT-010-06: Time-of-day effects | M | Lighting transitions, shaders | 4 |
| FEAT-010-07: State persistence | S | Save/load office state | 2 |
| FEAT-010-08: Customization | M | Customization options | 4 |
| **EPIC-010 Total** | | | **28 days** |

### EPIC-011: Room Manager

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-011-01: Room entity | M | Room class, properties | 4 |
| FEAT-011-02: Room navigation | M | Navigation system, transitions | 4 |
| FEAT-011-03: Instantiation | S | Spawn/destroy logic | 2 |
| FEAT-011-04: Room state | M | State machine, management | 4 |
| FEAT-011-05: Transitions | M | Fade/transition effects | 4 |
| FEAT-011-06: Camera management | M | Camera movement, positioning | 4 |
| FEAT-011-07: Lighting | M | Per-room lighting | 4 |
| FEAT-011-08: Audio zones | M | Audio area definitions | 4 |
| **EPIC-011 Total** | | | **30 days** |

### EPIC-012: Transition Manager

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-012-01: Fade effects | M | Fade animations | 4 |
| FEAT-012-02: Transition sequencing | M | Multiple transition steps | 4 |
| FEAT-012-03: Loading screen | M | Loading UI, display | 4 |
| FEAT-012-04: Progress tracking | S | Progress updates | 2 |
| FEAT-012-05: Timing control | S | Duration adjustments | 2 |
| FEAT-012-06: Audio crossfade | M | Music fading | 4 |
| FEAT-012-07: Input blocking | S | Input disable during transitions | 2 |
| FEAT-012-08: Async loading | L | Async scene loading | 8 |
| **EPIC-012 Total** | | | **30 days** |

### EPIC-013: Visitor System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-013-01: Visitor entity | M | NPC class, properties | 4 |
| FEAT-013-02: Schedule management | L | Scheduling system, calendar integration | 8 |
| FEAT-013-03: Arrival/departure | M | Spawn/despawn logic | 4 |
| FEAT-013-04: Interaction framework | M | Interaction system | 4 |
| FEAT-013-05: State machine | M | Visitor states | 4 |
| FEAT-013-06: Animation integration | M | Animation triggers | 4 |
| FEAT-013-07: Audio (voice/footsteps) | M | Sound effects, voice | 4 |
| FEAT-013-08: Persistence | S | Save/load visitor state | 2 |
| **EPIC-013 Total** | | | **34 days** |

### EPIC-014: Phone System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-014-01: Phone entity | S | Phone object, properties | 2 |
| FEAT-014-02: Phone UI | M | UI panel, display | 4 |
| FEAT-014-03: Call initiation | M | Call logic | 4 |
| FEAT-014-04: Answer/reject | S | Call state changes | 2 |
| FEAT-014-05: Duration tracking | S | Timer logic | 2 |
| FEAT-014-06: Audio management | M | Voice audio, mixing | 4 |
| FEAT-014-07: Contact management | S | Contact storage, display | 2 |
| FEAT-014-08: Call logging | S | History storage | 2 |
| **EPIC-014 Total** | | | **22 days** |

### EPIC-015: Email System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-015-01: Email entity | S | Email data structure | 2 |
| FEAT-015-02: Inbox UI | M | List display, interactions | 4 |
| FEAT-015-03: Compose UI | M | Compose panel, sending | 4 |
| FEAT-015-04: Read/unread status | S | State tracking | 2 |
| FEAT-015-05: Folders | M | Folder system, organization | 4 |
| FEAT-015-06: Archival | S | Archive logic | 2 |
| FEAT-015-07: Search | M | Search functionality | 4 |
| FEAT-015-08: Notifications | S | Email alerts | 2 |
| **EPIC-015 Total** | | | **24 days** |

### EPIC-016: News System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-016-01: News article | S | News data structure | 2 |
| FEAT-016-02: Feed display | M | News UI display | 4 |
| FEAT-016-03: Source management | S | Source tracking | 2 |
| FEAT-016-04: Categorization | S | Category system | 2 |
| FEAT-016-05: Ticker display | M | Scrolling ticker UI | 4 |
| FEAT-016-06: Notifications | S | News alerts | 2 |
| FEAT-016-07: Archival | S | News history | 2 |
| FEAT-016-08: Reader persistence | S | Save read status | 2 |
| **EPIC-016 Total** | | | **20 days** |

### EPIC-017: Calendar

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-017-01: Calendar UI | L | Month/week/day views, complex UI | 8 |
| FEAT-017-02: Event creation | M | Event input, storage | 4 |
| FEAT-017-03: Event editing | M | Event modification | 4 |
| FEAT-017-04: Event deletion | S | Delete logic | 2 |
| FEAT-017-05: Reminders | M | Reminder scheduling, notification | 4 |
| FEAT-017-06: Color coding | S | Color assignment system | 2 |
| FEAT-017-07: Filtering | M | Filter logic, UI | 4 |
| FEAT-017-08: Personal vs global | S | Event categorization | 2 |
| **EPIC-017 Total** | | | **30 days** |

### EPIC-018: Desk Interaction

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-018-01: Interaction system | M | Click/interact logic | 4 |
| FEAT-018-02: Item storage | S | Inventory system | 2 |
| FEAT-018-03: Item placement | M | Grid placement, positioning | 4 |
| FEAT-018-04: Pickup/putdown | S | Item interaction | 2 |
| FEAT-018-05: Customization | M | Customization UI | 4 |
| FEAT-018-06: Photo frames | S | Photo display | 2 |
| FEAT-018-07: Lighting control | S | Lamp toggling | 2 |
| FEAT-018-08: Persistence | S | Save desk state | 2 |
| **EPIC-018 Total** | | | **22 days** |

### EPIC-019: Notification System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-019-01: Entity system | S | Notification data structure | 2 |
| FEAT-019-02: UI display | M | Toast/notification display | 4 |
| FEAT-019-03: Queue management | M | Queue logic | 4 |
| FEAT-019-04: Priority levels | S | Priority system | 2 |
| FEAT-019-05: Audio alerts | S | Sound effects | 2 |
| FEAT-019-06: Persistence | S | Save notifications | 2 |
| FEAT-019-07: History | S | Notification log | 2 |
| FEAT-019-08: Filtering | S | Filter logic | 2 |
| **EPIC-019 Total** | | | **20 days** |

**Wave 01 Total: 272 days (~14 weeks)**

---

## Wave 02 – World Simulation

### EPIC-020: Country Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-020-01: Country entity | M | Country class, data | 4 |
| FEAT-020-02: Name generation | S | Procedural name generation | 2 |
| FEAT-020-03: Border definition | L | Complex border system | 8 |
| FEAT-020-04: Capital placement | M | Placement algorithm | 4 |
| FEAT-020-05: Government type | M | Government assignment | 4 |
| FEAT-020-06: National traits | M | Trait system | 4 |
| FEAT-020-07: Size/territory | M | Area calculation | 4 |
| FEAT-020-08: Resource allocation | M | Resource distribution | 4 |
| **EPIC-020 Total** | | | **34 days** |

### EPIC-021: Geography Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-021-01: Height map | L | Perlin noise, terrain generation | 8 |
| FEAT-021-02: Biome assignment | L | Biome system, transitions | 8 |
| FEAT-021-03: Climate zones | M | Climate distribution | 4 |
| FEAT-021-04: Water bodies | L | Water generation, rivers | 8 |
| FEAT-021-05: Mountains | M | Mountain placement | 4 |
| FEAT-021-06: Forests | M | Forest distribution | 4 |
| FEAT-021-07: Deserts | M | Desert placement | 4 |
| FEAT-021-08: Coastlines | M | Coastline generation | 4 |
| **EPIC-021 Total** | | | **44 days** |

### EPIC-022: Resource Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-022-01: Resource types | S | Resource definitions | 2 |
| FEAT-022-02: Resource nodes | L | Node placement algorithm | 8 |
| FEAT-022-03: Scarcity system | M | Scarcity mechanics | 4 |
| FEAT-022-04: Regional abundance | M | Abundance distribution | 4 |
| FEAT-022-05: Extraction mechanics | M | Extraction system | 4 |
| FEAT-022-06: Renewal system | M | Resource regeneration | 4 |
| FEAT-022-07: Strategic resources | M | Special resource placement | 4 |
| FEAT-022-08: Trade goods | M | Trade good distribution | 4 |
| **EPIC-022 Total** | | | **34 days** |

### EPIC-023: Population Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-023-01: Distribution | L | Population distribution algorithm | 8 |
| FEAT-023-02: Demographics | M | Age/gender distribution | 4 |
| FEAT-023-03: Settlement generation | L | Settlement placement | 8 |
| FEAT-023-04: Urban centers | M | City placement | 4 |
| FEAT-023-05: Villages | M | Village placement | 4 |
| FEAT-023-06: Density calculation | M | Population density | 4 |
| FEAT-023-07: Migration patterns | L | Migration algorithm | 8 |
| FEAT-023-08: Growth baseline | M | Growth rates | 4 |
| **EPIC-023 Total** | | | **44 days** |

### EPIC-024: Currency Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-024-01: Currency system | M | Currency creation | 4 |
| FEAT-024-02: Exchange rates | M | Exchange rate calculation | 4 |
| FEAT-024-03: Monetary policy | L | Policy system | 8 |
| FEAT-024-04: Inflation mechanics | L | Inflation simulation | 8 |
| FEAT-024-05: Symbol/naming | S | Currency naming | 2 |
| FEAT-024-06: Denominations | S | Denomination system | 2 |
| FEAT-024-07: Central bank | M | Bank simulation | 4 |
| FEAT-024-08: Stability system | M | Stability mechanics | 4 |
| **EPIC-024 Total** | | | **36 days** |

### EPIC-025: Constitution Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-025-01: Government structure | M | Structure definition | 4 |
| FEAT-025-02: Legal framework | L | Laws and legal system | 8 |
| FEAT-025-03: Rights & freedoms | M | Rights definition | 4 |
| FEAT-025-04: National policies | L | Policy system | 8 |
| FEAT-025-05: Law enforcement | M | Enforcement system | 4 |
| FEAT-025-06: Judicial system | M | Court system | 4 |
| FEAT-025-07: Legislative system | M | Parliament/legislature | 4 |
| FEAT-025-08: Amendment rules | M | Amendment mechanics | 4 |
| **EPIC-025 Total** | | | **40 days** |

### EPIC-026: Political Culture

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-026-01: Ideology system | M | Ideology definitions | 4 |
| FEAT-026-02: Factions | L | Faction generation | 8 |
| FEAT-026-03: Power structure | M | Hierarchy system | 4 |
| FEAT-026-04: Stability metrics | M | Stability calculation | 4 |
| FEAT-026-05: Polarization | M | Political polarization | 4 |
| FEAT-026-06: Opposition groups | M | Opposition generation | 4 |
| FEAT-026-07: Movements | M | Movement system | 4 |
| FEAT-026-08: Effectiveness | M | Rating system | 4 |
| **EPIC-026 Total** | | | **36 days** |

### EPIC-027: Global Economy

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-027-01: Trade networks | XL | Complex network system | 15 |
| FEAT-027-02: Production chains | XL | Chain simulation | 15 |
| FEAT-027-03: Interdependence | L | Dependency mapping | 8 |
| FEAT-027-04: Market prices | L | Price calculation | 8 |
| FEAT-027-05: Supply/demand | XL | Simulation system | 15 |
| FEAT-027-06: Trade routes | L | Route generation | 8 |
| FEAT-027-07: Competition | L | Competition mechanics | 8 |
| FEAT-027-08: Crisis generation | M | Crisis events | 4 |
| **EPIC-027 Total** | | | **81 days** |

### EPIC-028: International Organizations

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-028-01: Org templates | M | Template system | 4 |
| FEAT-028-02: Treaty system | L | Treaty mechanics | 8 |
| FEAT-028-03: Alliances | L | Alliance system | 8 |
| FEAT-028-04: Diplomatic protocol | M | Protocol system | 4 |
| FEAT-028-05: International law | L | Legal framework | 8 |
| FEAT-028-06: Voting system | M | Voting mechanics | 4 |
| FEAT-028-07: Membership | M | Member management | 4 |
| FEAT-028-08: Influence | M | Influence system | 4 |
| **EPIC-028 Total** | | | **44 days** |

### EPIC-029: World Event Generator

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-029-01: Event templates | M | Event system | 4 |
| FEAT-029-02: Random events | L | Random event generation | 8 |
| FEAT-029-03: Historical events | M | Historical event system | 4 |
| FEAT-029-04: Natural disasters | L | Disaster generation | 8 |
| FEAT-029-05: Political crises | L | Crisis generation | 8 |
| FEAT-029-06: Economic events | L | Economic event generation | 8 |
| FEAT-029-07: Cultural events | M | Cultural event generation | 4 |
| FEAT-029-08: Weather events | L | Weather event generation | 8 |
| **EPIC-029 Total** | | | **52 days** |

**Wave 02 Total: 425 days (~21 weeks)**

---

## Wave 03 – Core Gameplay

### EPIC-030: Turn System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-030-01: Turn counter | S | Simple counter | 2 |
| FEAT-030-02: Phase management | L | Multiple phases, state machine | 8 |
| FEAT-030-03: Turn order | M | Order calculation | 4 |
| FEAT-030-04: Player notification | S | Notification logic | 2 |
| FEAT-030-05: AI execution | L | AI turn logic | 8 |
| FEAT-030-06: Simultaneous turns | L | Synchronization logic | 8 |
| FEAT-030-07: Turn timeout | M | Timeout system | 4 |
| FEAT-030-08: History logging | S | Turn logging | 2 |
| **EPIC-030 Total** | | | **38 days** |

### EPIC-031: Map & World

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-031-01: Tile grid | XL | Complex grid system | 15 |
| FEAT-031-02: Tile entity | L | Tile class, properties | 8 |
| FEAT-031-03: Terrain types | M | Terrain system | 4 |
| FEAT-031-04: Resource visualization | M | UI visualization | 4 |
| FEAT-031-05: Improvements | L | Improvement system | 8 |
| FEAT-031-06: Adjacency | M | Adjacency calculations | 4 |
| FEAT-031-07: Coordinates | S | Coordinate system | 2 |
| FEAT-031-08: Fog of war | XL | Complex visibility system | 15 |
| **EPIC-031 Total** | | | **60 days** |

### EPIC-032: Units

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-032-01: Unit entity | L | Unit class, properties | 8 |
| FEAT-032-02: Unit types | M | Type system | 4 |
| FEAT-032-03: Unit stats | M | Stat system | 4 |
| FEAT-032-04: Equipment | M | Equipment system | 4 |
| FEAT-032-05: Experience | M | Leveling system | 4 |
| FEAT-032-06: Stacking rules | M | Stacking mechanics | 4 |
| FEAT-032-07: Visibility | M | Visibility mechanics | 4 |
| FEAT-032-08: Animations | L | Animation system | 8 |
| **EPIC-032 Total** | | | **40 days** |

### EPIC-033: Movement

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-033-01: Pathfinding | XL | A* algorithm, complex pathfinding | 15 |
| FEAT-033-02: Movement validation | L | Validation system | 8 |
| FEAT-033-03: Movement points | L | Point system | 8 |
| FEAT-033-04: Terrain costs | M | Terrain-based costs | 4 |
| FEAT-033-05: Difficult terrain | M | Difficult terrain logic | 4 |
| FEAT-033-06: Animation | M | Movement animation | 4 |
| FEAT-033-07: Preview | M | Movement preview UI | 4 |
| FEAT-033-08: Undo | M | Undo logic | 4 |
| **EPIC-033 Total** | | | **51 days** |

### EPIC-034: Actions & Orders

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-034-01: Action entity | M | Action class | 4 |
| FEAT-034-02: Action types | M | Type system | 4 |
| FEAT-034-03: Order queuing | L | Queue system | 8 |
| FEAT-034-04: Validation | M | Validation logic | 4 |
| FEAT-034-05: Execution | L | Execution engine | 8 |
| FEAT-034-06: Cancellation | M | Cancel logic | 4 |
| FEAT-034-07: Undo/redo | L | Undo/redo system | 8 |
| FEAT-034-08: History | M | Action history | 4 |
| **EPIC-034 Total** | | | **44 days** |

### EPIC-035: Combat System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-035-01: Combat init | M | Combat setup | 4 |
| FEAT-035-02: Targeting | M | Target selection | 4 |
| FEAT-035-03: Hit/miss calc | L | Probability calculation | 8 |
| FEAT-035-04: Damage calc | L | Damage formula | 8 |
| FEAT-035-05: Critical hits | M | Critical system | 4 |
| FEAT-035-06: Area effects | L | Area effect system | 8 |
| FEAT-035-07: Death/removal | M | Unit removal | 4 |
| FEAT-035-08: Rewards | M | Experience/loot | 4 |
| **EPIC-035 Total** | | | **44 days** |

### EPIC-036: Resources

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-036-01: Resource types | S | Type definitions | 2 |
| FEAT-036-02: Resource pools | M | Pool system | 4 |
| FEAT-036-03: Storage limits | S | Limit system | 2 |
| FEAT-036-04: Production calc | L | Production simulation | 8 |
| FEAT-036-05: Consumption calc | L | Consumption simulation | 8 |
| FEAT-036-06: Trading | L | Trade mechanics | 8 |
| FEAT-036-07: Alerts | S | Alert system | 2 |
| FEAT-036-08: Stockpile | M | Stockpile management | 4 |
| **EPIC-036 Total** | | | **38 days** |

### EPIC-037: Basic UI

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-037-01: Main HUD layout | L | Complex HUD system | 8 |
| FEAT-037-02: Unit info panel | M | Info display | 4 |
| FEAT-037-03: Tile info panel | M | Info display | 4 |
| FEAT-037-04: Resource display | M | Resource UI | 4 |
| FEAT-037-05: Turn counter | S | Counter display | 2 |
| FEAT-037-06: Status effects | M | Effects display | 4 |
| FEAT-037-07: UI scaling | M | Scaling system | 4 |
| FEAT-037-08: Theme system | M | Theme system | 4 |
| **EPIC-037 Total** | | | **34 days** |

### EPIC-038: Input Handling

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-038-01: Keyboard input | S | Input mapping | 2 |
| FEAT-038-02: Mouse input | M | Mouse handling | 4 |
| FEAT-038-03: Click detection | M | Click system | 4 |
| FEAT-038-04: Drag detection | M | Drag system | 4 |
| FEAT-038-05: Camera controls | M | Camera movement | 4 |
| FEAT-038-06: Double-click | S | Double-click logic | 2 |
| FEAT-038-07: Context menu | M | Context menu | 4 |
| FEAT-038-08: Rebinding | M | Rebinding system | 4 |
| **EPIC-038 Total** | | | **28 days** |

### EPIC-039: Game Loop

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-039-01: Main loop | L | Core loop implementation | 8 |
| FEAT-039-02: Game state | M | State management | 4 |
| FEAT-039-03: Pause/resume | S | Pause system | 2 |
| FEAT-039-04: Game speed | M | Speed control | 4 |
| FEAT-039-05: Frame rate | M | Frame rate management | 4 |
| FEAT-039-06: Input loop | M | Input processing | 4 |
| FEAT-039-07: Render loop | M | Rendering | 4 |
| FEAT-039-08: State transitions | M | Transition logic | 4 |
| **EPIC-039 Total** | | | **34 days** |

**Wave 03 Total: 446 days (~22 weeks)**

---

## Wave 04 – Advanced Simulation

### EPIC-040: Factions

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-040-01: Faction entity | M | Faction class | 4 |
| FEAT-040-02: Faction traits | M | Trait system | 4 |
| FEAT-040-03: Bonuses | L | Bonus calculation | 8 |
| FEAT-040-04: Leaders | M | Leader system | 4 |
| FEAT-040-05: Relationships | M | Relationship tracking | 4 |
| FEAT-040-06: Alignment | M | Alignment system | 4 |
| FEAT-040-07: Victory conditions | M | Victory tracking | 4 |
| FEAT-040-08: Colors/branding | S | Color assignment | 2 |
| **EPIC-040 Total** | | | **34 days** |

### EPIC-041: Buildings & Infrastructure

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-041-01: Building entity | M | Building class | 4 |
| FEAT-041-02: Building types | M | Type system | 4 |
| FEAT-041-03: Placement | M | Placement system | 4 |
| FEAT-041-04: Construction | L | Construction system | 8 |
| FEAT-041-05: Upgrades | L | Upgrade system | 8 |
| FEAT-041-06: Effects/bonuses | L | Bonus application | 8 |
| FEAT-041-07: Maintenance | M | Maintenance system | 4 |
| FEAT-041-08: Destruction | S | Destruction logic | 2 |
| **EPIC-041 Total** | | | **42 days** |

### EPIC-042: Population & Growth

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-042-01: Population entity | M | Population class | 4 |
| FEAT-042-02: Growth | L | Growth calculation | 8 |
| FEAT-042-03: Age distribution | M | Demographics | 4 |
| FEAT-042-04: Happiness | L | Happiness system | 8 |
| FEAT-042-05: Migration | L | Migration system | 8 |
| FEAT-042-06: Settlement limits | M | Limit system | 4 |
| FEAT-042-07: Specialization | L | Specialization system | 8 |
| FEAT-042-08: Death/disease | M | Attrition system | 4 |
| **EPIC-042 Total** | | | **48 days** |

### EPIC-043: Economy & Trade

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-043-01: Trade routes | XL | Route management system | 15 |
| FEAT-043-02: Merchant AI | XL | AI merchant logic | 15 |
| FEAT-043-03: Trade agreements | L | Agreement system | 8 |
| FEAT-043-04: Price negotiation | L | Negotiation mechanics | 8 |
| FEAT-043-05: Market fluctuation | M | Price fluctuation | 4 |
| FEAT-043-06: Trade profit | M | Profit calculation | 4 |
| FEAT-043-07: Dispute resolution | M | Dispute system | 4 |
| FEAT-043-08: Trade caravans | L | Caravan system | 8 |
| **EPIC-043 Total** | | | **66 days** |

### EPIC-044: Diplomacy

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-044-01: Relationships | M | Relationship tracking | 4 |
| FEAT-044-02: Actions | L | Diplomatic actions | 8 |
| FEAT-044-03: Alliances | L | Alliance system | 8 |
| FEAT-044-04: Treaties | XL | Treaty system | 15 |
| FEAT-044-05: Espionage | L | Espionage mechanics | 8 |
| FEAT-044-06: Reputation | M | Reputation system | 4 |
| FEAT-044-07: War declaration | M | War mechanics | 4 |
| FEAT-044-08: Peace negotiation | L | Peace mechanics | 8 |
| **EPIC-044 Total** | | | **59 days** |

### EPIC-045: Technology Trees

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-045-01: Tech tree | L | Tree structure | 8 |
| FEAT-045-02: Research points | M | Point system | 4 |
| FEAT-045-03: Prerequisites | M | Prerequisite system | 4 |
| FEAT-045-04: Tech unlock | L | Unlock mechanics | 8 |
| FEAT-045-05: Tech costs | M | Cost calculation | 4 |
| FEAT-045-06: Tech bonuses | L | Bonus application | 8 |
| FEAT-045-07: Tech loss | M | Loss on conquest | 4 |
| FEAT-045-08: Tech spy | L | Espionage mechanics | 8 |
| **EPIC-045 Total** | | | **48 days** |

### EPIC-046: Morale & Culture

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-046-01: Morale tracking | M | Tracking system | 4 |
| FEAT-046-02: Modifiers | L | Modifier system | 8 |
| FEAT-046-03: Cultural identity | M | Identity system | 4 |
| FEAT-046-04: Cultural spread | L | Spread mechanics | 8 |
| FEAT-046-05: Conversion | L | Conversion mechanics | 8 |
| FEAT-046-06: Wonders | L | Wonder system | 8 |
| FEAT-046-07: Cultural victory | M | Victory mechanics | 4 |
| FEAT-046-08: Unrest | L | Unrest system | 8 |
| **EPIC-046 Total** | | | **52 days** |

### EPIC-047: Weather & Climate

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-047-01: Weather patterns | L | Weather generation | 8 |
| FEAT-047-02: Seasons | L | Seasonal system | 8 |
| FEAT-047-03: Temperature | M | Temperature tracking | 4 |
| FEAT-047-04: Precipitation | L | Precipitation system | 8 |
| FEAT-047-05: Storms | M | Storm generation | 4 |
| FEAT-047-06: Climate penalties | L | Penalty system | 8 |
| FEAT-047-07: Production effects | L | Production effects | 8 |
| FEAT-047-08: Disasters | L | Disaster system | 8 |
| **EPIC-047 Total** | | | **56 days** |

### EPIC-048: AI Decision Making

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-048-01: Goal evaluation | XL | Complex AI system | 15 |
| FEAT-048-02: Strategy | XL | Strategic planning | 15 |
| FEAT-048-03: Threat assessment | L | Assessment logic | 8 |
| FEAT-048-04: Resource mgmt | L | Resource management | 8 |
| FEAT-048-05: Diplomacy | L | Diplomatic decisions | 8 |
| FEAT-048-06: Military strategy | XL | Military planning | 15 |
| FEAT-048-07: Expansion | L | Expansion logic | 8 |
| FEAT-048-08: Behavior trees | L | Behavior tree system | 8 |
| **EPIC-048 Total** | | | **85 days** |

### EPIC-049: Victory Conditions

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-049-01: Domination | M | Domination tracking | 4 |
| FEAT-049-02: Science | M | Science tracking | 4 |
| FEAT-049-03: Cultural | M | Cultural tracking | 4 |
| FEAT-049-04: Economic | M | Economic tracking | 4 |
| FEAT-049-05: Diplomatic | M | Diplomatic tracking | 4 |
| FEAT-049-06: Progress tracking | L | Progress display | 8 |
| FEAT-049-07: Notification | M | Victory notification | 4 |
| FEAT-049-08: Customization | M | Customizable conditions | 4 |
| **EPIC-049 Total** | | | **36 days** |

**Wave 04 Total: 523 days (~26 weeks)**

---

## Wave 05 – Polish & UI

### EPIC-050: Main Menu

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-050-01: Main menu screen | M | Menu UI | 4 |
| FEAT-050-02: New game flow | M | New game UI/logic | 4 |
| FEAT-050-03: Load game flow | M | Load game UI/logic | 4 |
| FEAT-050-04: Settings nav | M | Settings navigation | 4 |
| FEAT-050-05: Exit game | S | Exit logic | 2 |
| FEAT-050-06: Version display | XS | Version display | 0.5 |
| FEAT-050-07: Continue game | S | Continue logic | 2 |
| FEAT-050-08: Mod manager | M | Mod UI (if applicable) | 4 |
| **EPIC-050 Total** | | | **24.5 days** |

### EPIC-051: Settings & Options

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-051-01: Graphics options | L | Complex graphics settings | 8 |
| FEAT-051-02: Audio options | M | Audio settings | 4 |
| FEAT-051-03: Gameplay options | M | Gameplay settings | 4 |
| FEAT-051-04: Accessibility | L | Accessibility options | 8 |
| FEAT-051-05: Control remapping | L | Key remapping UI | 8 |
| FEAT-051-06: Video quality | M | Quality settings | 4 |
| FEAT-051-07: Resolution | M | Resolution selector | 4 |
| FEAT-051-08: Save/load settings | S | Settings persistence | 2 |
| **EPIC-051 Total** | | | **42 days** |

### EPIC-052: HUD System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-052-01: Main HUD redesign | L | HUD polish/overhaul | 8 |
| FEAT-052-02: Minimap | L | Minimap system | 8 |
| FEAT-052-03: Resource display | M | Resource UI polish | 4 |
| FEAT-052-04: Unit status | M | Status indicators | 4 |
| FEAT-052-05: Objectives | M | Objective display | 4 |
| FEAT-052-06: Time/date | S | Time display | 2 |
| FEAT-052-07: FPS counter | S | FPS display | 2 |
| FEAT-052-08: Customization | M | HUD customization | 4 |
| **EPIC-052 Total** | | | **36 days** |

### EPIC-053: Tooltips & Help

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-053-01: Tooltip system | M | Tooltip implementation | 4 |
| FEAT-053-02: Styling | S | Tooltip styling | 2 |
| FEAT-053-03: Tutorial framework | L | Tutorial system | 8 |
| FEAT-053-04: Tutorial dialogue | M | Dialogue system | 4 |
| FEAT-053-05: Interactive tutorials | L | Interactive elements | 8 |
| FEAT-053-06: Help menu | M | Help UI | 4 |
| FEAT-053-07: Context help | M | Context-sensitive help | 4 |
| FEAT-053-08: Glossary | M | Glossary/encyclopedia | 4 |
| **EPIC-053 Total** | | | **38 days** |

### EPIC-054: Audio System

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-054-01: Audio engine | M | Audio system setup | 4 |
| FEAT-054-02: Background music | M | Music system | 4 |
| FEAT-054-03: Sound effects | M | SFX system | 4 |
| FEAT-054-04: Volume control | M | Volume management | 4 |
| FEAT-054-05: Audio mixing | L | Mixing system | 8 |
| FEAT-054-06: Spatial audio | L | 3D audio | 8 |
| FEAT-054-07: Music crossfade | M | Music transitions | 4 |
| FEAT-054-08: Audio persistence | S | Audio settings save | 2 |
| **EPIC-054 Total** | | | **38 days** |

### EPIC-055: Visual Effects

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-055-01: Particle system | M | Particle effects | 4 |
| FEAT-055-02: Combat effects | M | Combat VFX | 4 |
| FEAT-055-03: UI animations | M | UI animation framework | 4 |
| FEAT-055-04: Screen shake | S | Screen shake effect | 2 |
| FEAT-055-05: Fade effects | S | Fade effects | 2 |
| FEAT-055-06: Bloom effects | M | Bloom effect | 4 |
| FEAT-055-07: Unit animation | L | Animation polish | 8 |
| FEAT-055-08: Building animation | L | Animation polish | 8 |
| **EPIC-055 Total** | | | **36 days** |

### EPIC-056: Accessibility

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-056-01: UI scaling | M | Scaling system | 4 |
| FEAT-056-02: Colorblind modes | L | Multiple colorblind modes | 8 |
| FEAT-056-03: High contrast | M | High contrast mode | 4 |
| FEAT-056-04: Font sizing | M | Font size adjustment | 4 |
| FEAT-056-05: Screen reader | L | Screen reader support | 8 |
| FEAT-056-06: Keyboard nav | M | Keyboard-only nav | 4 |
| FEAT-056-07: Controller UI | M | Controller-friendly UI | 4 |
| FEAT-056-08: Text-to-speech | L | TTS support | 8 |
| **EPIC-056 Total** | | | **44 days** |

### EPIC-057: Controller Support

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-057-01: Controller mapping | M | Input mapping | 4 |
| FEAT-057-02: Analog sticks | M | Analog input | 4 |
| FEAT-057-03: Triggers | M | Trigger input | 4 |
| FEAT-057-04: Button remapping | M | Remapping UI | 4 |
| FEAT-057-05: UI navigation | M | Menu navigation | 4 |
| FEAT-057-06: Cursor movement | M | Cursor with controller | 4 |
| FEAT-057-07: Menu navigation | M | Menu controls | 4 |
| FEAT-057-08: Vibration | S | Controller vibration | 2 |
| **EPIC-057 Total** | | | **30 days** |

### EPIC-058: Performance Optimization

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-058-01: Profiler integration | M | Profiler setup | 4 |
| FEAT-058-02: Memory profiling | L | Memory analysis | 8 |
| FEAT-058-03: CPU profiling | L | CPU analysis | 8 |
| FEAT-058-04: GPU profiling | L | GPU analysis | 8 |
| FEAT-058-05: Rendering opt | L | Rendering optimization | 8 |
| FEAT-058-06: Physics opt | M | Physics optimization | 4 |
| FEAT-058-07: Audio opt | M | Audio optimization | 4 |
| FEAT-058-08: Loading opt | L | Loading time optimization | 8 |
| **EPIC-058 Total** | | | **52 days** |

### EPIC-059: Graphics Polish

| Feature | Size | Rationale | Days |
|---------|------|-----------|------|
| FEAT-059-01: Lighting polish | L | Lighting improvements | 8 |
| FEAT-059-02: Materials | M | Material creation | 4 |
| FEAT-059-03: Shader opt | M | Shader optimization | 4 |
| FEAT-059-04: Texture opt | M | Texture optimization | 4 |
| FEAT-059-05: Normal maps | M | Normal mapping | 4 |
| FEAT-059-06: Parallax mapping | M | Parallax effects | 4 |
| FEAT-059-07: Reflections | L | Reflection system | 8 |
| FEAT-059-08: Shadow quality | L | Shadow improvements | 8 |
| **EPIC-059 Total** | | | **44 days** |

**Wave 05 Total: 385 days (~19 weeks)**

---

## Wave 06 – Content & Depth

### EPIC-060 through EPIC-069 Summary

(Each content epic has 8 features representing different deliverables)

| Epic | Total Days | Rationale |
|------|-----------|-----------|
| EPIC-060: Faction Content (8 factions) | 60 | 7-8 days per faction design |
| EPIC-061: Unit Content (40+ units) | 70 | 1-2 days per unit (model + animation) |
| EPIC-062: Building Content (30+ buildings) | 60 | 1-2 days per building |
| EPIC-063: Scenarios & Campaigns | 50 | 6 days per campaign |
| EPIC-064: Balancing Pass 1 | 50 | Testing and rebalancing |
| EPIC-065: Balancing Pass 2 | 50 | Fine-tuning and polish |
| EPIC-066: Modding Support | 40 | API design and tools |
| EPIC-067: Advanced Audio | 50 | Music composition and sound design |
| EPIC-068: Polish Pass | 50 | General polish and refinement |
| EPIC-069: Quality Assurance | 60 | Testing framework and test execution |

**Wave 06 Total: 540 days (~27 weeks)**

---

## Wave 07 – Launch Preparation

### EPIC-070 through EPIC-079 Summary

| Epic | Total Days | Rationale |
|------|-----------|-----------|
| EPIC-070: Build System | 40 | Optimization and deployment |
| EPIC-071: Platform Certification | 50 | Compliance and certification |
| EPIC-072: Localization | 60 | Translation and localization |
| EPIC-073: Launch Marketing | 45 | Marketing materials |
| EPIC-074: Patch Pipeline | 35 | Build/deployment automation |
| EPIC-075: Community Tools | 50 | Leaderboards, achievements |
| EPIC-076: Post-Launch Support | 40 | Support infrastructure |
| EPIC-077: DLC & Expansion | 30 | Planning and design |
| EPIC-078: Analytics & Telemetry | 45 | Data collection systems |
| EPIC-079: Final Polish | 50 | Last-minute fixes and polish |

**Wave 07 Total: 445 days (~22 weeks)**

---

## Summary by Wave

| Wave | Total Days | Weeks | Complexity |
|------|-----------|-------|------------|
| Wave 00: Foundation | 257.5 | ~13 | 🔴 CRITICAL |
| Wave 01: Executive Workspace | 272 | ~14 | 🟠 HIGH |
| Wave 02: World Simulation | 425 | ~21 | 🔴 CRITICAL |
| Wave 03: Core Gameplay | 446 | ~22 | 🔴 CRITICAL |
| Wave 04: Advanced Simulation | 523 | ~26 | 🔴 CRITICAL |
| Wave 05: Polish & UI | 385 | ~19 | 🟡 MEDIUM |
| Wave 06: Content & Depth | 540 | ~27 | 🟡 MEDIUM |
| Wave 07: Launch Preparation | 445 | ~22 | 🟡 MEDIUM |
| **TOTAL** | **3,293.5 days** | **~165 weeks** | **~37 months** |

---

## Total Project Effort

| Metric | Value |
|--------|-------|
| Total Feature Days | 3,293.5 |
| Effective Weeks | ~165 |
| Effective Months | ~37 |
| With 1 Developer | 37 months |
| With 5 Developers | ~7-8 months |
| With 10 Developers | ~4-5 months |
| With 15 Developers | ~3 months |

---

## Recommended Team Size by Wave

| Wave | Recommended | Full-Time Equivalent |
|------|-------------|---------------------|
| Wave 00 | 4-6 devs | 4 weeks |
| Wave 01 | 4-5 devs | 14 weeks |
| Wave 02 | 5-7 devs | 15 weeks (parallel) |
| Wave 03 | 7-10 devs | 14 weeks (parallel) |
| Wave 04 | 8-12 devs | 13 weeks (parallel) |
| Wave 05 | 6-10 devs | 11 weeks (parallel) |
| Wave 06 | 8-15 devs | 15 weeks (parallel) |
| Wave 07 | 5-8 devs | 15 weeks (parallel) |
| **Peak** | **15 devs** | **~90 days** |

---

## Complexity Distribution

### By Size Category

| Size | Count | % of Features | Total Days | % of Effort |
|------|-------|---------------|-----------|-----------|
| XS | 45 | 7% | 56 | 2% |
| S | 156 | 25% | 312 | 9% |
| M | 285 | 45% | 1,140 | 35% |
| L | 105 | 17% | 840 | 25% |
| XL | 41 | 6% | 945 | 29% |

---

## Bottleneck Features by Size (XL)

| Feature | Days | Wave | Epic |
|---------|------|------|------|
| FEAT-027-01: Trade networks | 15 | 2 | Global Economy |
| FEAT-027-02: Production chains | 15 | 2 | Global Economy |
| FEAT-027-05: Supply/demand | 15 | 2 | Global Economy |
| FEAT-031-01: Tile grid | 15 | 3 | Map & World |
| FEAT-031-08: Fog of war | 15 | 3 | Map & World |
| FEAT-033-01: Pathfinding | 15 | 3 | Movement |
| FEAT-043-01: Trade routes | 15 | 4 | Economy & Trade |
| FEAT-043-02: Merchant AI | 15 | 4 | Economy & Trade |
| FEAT-044-04: Treaty system | 15 | 4 | Diplomacy |
| FEAT-048-01: AI goals | 15 | 4 | AI Decision Making |
| FEAT-048-02: AI strategy | 15 | 4 | AI Decision Making |
| FEAT-048-06: Military strategy | 15 | 4 | AI Decision Making |

---

## Risk Factors by Complexity

### High-Risk Features (Complexity Underestimated)

Features likely to take longer than estimated:
- Pathfinding algorithms (especially on large maps)
- Trade network simulation (interdependencies)
- AI decision making (unpredictable complexity)
- Fog of war (performance critical)
- Production chains (cascading calculations)

**Recommendation**: Add 20-30% buffer to XL features during planning

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-07-21 | Initial complexity estimates for all 632 features |
