# Production Waves

Republic uses a **10-wave player-experience roadmap** organized around the player's evolving experience of statecraft.

---

## The 10 Waves

0. **Foundation**: Deterministic headless simulation core, real-world calendar in West Africa Time (WAT / UTC+1), event bus, persistence, and multi-country world shell.
1. **Executive Workspace**: Presidential Office command interface, executive desk suite, communications, and National Briefing.
2. **World Simulation**: The country as central simulation object, procedural generation, Independence Day, 10x founding boost, REPU monetary baseline, State Capacity, and National Yield engine.
3. **Campaign**: Political factions, democratic elections, public sentiment, and constitutional mandates.
4. **Government**: Cabinet appointments with competence vs. loyalty trade-offs, State Capacity dynamics, and executive decrees.
5. **Economy**: REPU national budget, enabling environment for food (no magic deposits), upstream science, and development projects.
6. **Legislature**: Parliamentary coalitions, legislative bills, and constitutional amendments.
7. **Military**: Armed branches, DEFCON readiness, border deterrence, and strategic directives.
8. **Diplomacy**: Sovereign inter-country bilateral trade, treaties, alliances, and intelligence operations.
9. **Multiplayer Networking**: Live network transport, client-server synchronization, and real-time inter-player summits.

---

## Foundational Delivery Principles

1. **Multiplayer from Day One**:
   Multiplayer is part of Republic's identity and architectural foundation from the beginning. While live network transport and client-server synchronization are staged in Wave 9, all preceding waves model multi-nation persistence and international interactions as foundational properties.
2. **The Country as the Central Simulation Object**:
   The country itself is the central object of the simulation. The Presidential Office serves as the player's command interface and command centre.
3. **Continuous Real-World Timeline**:
   The simulation advances continuously on West Africa Time (WAT / UTC+1) across real-world calendar dates. The world does not pause when a player logs off.
4. **Measurable National Yield & State Capacity**:
   *"National Yield is not free stuff given to the player. It is the measurable output and accumulating capacity of a functioning state."*
5. **Headless Deterministic Core**:
   The simulation core in `src/Republic.Core` remains fully testable without presentation dependencies.

---

## Current Gate Status

### Wave 0 — "The skeleton works"
- Solution scaffolding and CI automation
- Configuration loading and validation
- Multi-sink structured logging
- High-performance event bus
- Deterministic simulation clock abstractions aligned with WAT (UTC+1)
- Persistence framework with SHA256 integrity verification
- Multi-country world manager shell
- Automated unit and integration tests passing in CI
