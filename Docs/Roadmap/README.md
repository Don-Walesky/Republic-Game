# Roadmap

The authoritative, canonical production roadmap for **Republic**.

---

## Foundational Multiplayer Delivery Model

> [!IMPORTANT]
> **Multiplayer is part of Republic's identity from the beginning, not an afterthought or late-stage add-on.**  
> Republic is conceived, designed, and architected as a persistent multiplayer political and nation-building simulation where every player governs their own procedurally generated country in a shared living world.
>
> While low-level network transport, multi-client replication, and live server synchronization are staged for engineering delivery in **Wave 9**, all preceding waves (from the Wave 0 foundation onwards) model countries, world persistence, diplomacy, trade, and the global simulation clock as inherently multi-nation and multiplayer-ready.

---

## Canonical Production Waves

Republic utilizes a **10-wave player-experience roadmap**. Each wave builds upon the last to deliver an executable, verifiable milestone:

| Wave | Name | Core Milestone Outcome | Multiplayer & Systemic Alignment |
|---|---|---|---|
| **0** | **Foundation** | The deterministic simulation core runs | Headless .NET core, global simulation clock (WAT / UTC+1), event bus, persistence, and multi-country world shell. |
| **1** | **Executive Workspace** | The player has a command interface | Presidential office as executive command centre; desks, communication channels, and national briefing on login. |
| **2** | **World Simulation** | The country lives and persists | The country as central simulation object; procedural generation, Independence Day timestamps, 10x founding boost, REPU monetary baseline, State Capacity, and National Yield engine. |
| **3** | **Campaign** | The player seeks and renews legitimacy | Candidacy, political factions, elections, public sentiment, and constitutional mandates. |
| **4** | **Government** | The player governs with real trade-offs | Cabinet appointments (competence vs. loyalty trade-offs), State Capacity dynamics, civil service integrity, and executive decrees. |
| **5** | **Economy** | Economic production & development emerge | REPU treasury budget, public enabling environment for food (no magic deposits), upstream science research, and development projects progressing independently of yield cycles. |
| **6** | **Legislature** | Power balances through institutions | Parliamentary coalitions, bills, constitutional amendments, and factional compromise. |
| **7** | **Military** | Sovereign defense carries deep consequences | Armed branches, DEFCON readiness, deterrence postures, border deployments, and crisis resolution. |
| **8** | **Diplomacy** | International statecraft becomes playable | Bilateral trade agreements, treaties, alliances, intelligence operations, and geopolitical competition between sovereign countries. |
| **9** | **Multiplayer Networking** | Politics becomes live and personal | Real-time network transport, live client-server session synchronization, and direct inter-player treaty and summit execution. |

---

## Delivery Rules

1. **Waves are sequential and dependency-driven**: Systems are validated thoroughly in headless automated suites before higher-level presentation and networking layers wrap them.
2. **Each wave ends in an executable milestone**: Every wave produces verifiable, testable software.
3. **Multiplayer identity guides all data structures**: Entity definitions, persistence schemas, and trade/diplomacy logic must assume multi-country concurrency and real-world persistence from Wave 0.
4. **Continuous real-world simulation**: Time is anchored to the global simulation clock in West Africa Time (WAT / UTC+1). No offline pausing or rewind mechanics.

---

## Current Status

**Waves 0 through 4 are implemented and verified**, alongside authoritative multiplayer networking (Wave 9), bilateral diplomacy and trade (Wave 8), and executive defense monitoring (Wave 7). The simulation core and Presidential Desk interface are backed by **772 passing C# unit tests** in .NET 8.0 and automated headless checks in Godot 4.

---

## MVP Scope Decisions & Deferred Advanced Systems

Following the architectural audit of dependencies, player value, and technical risk for the first public MVP, the following explicit scope determinations are recorded:

### Included in Public MVP Scope
1. **Multiplayer Reliability & Authoritative State Ownership**:
   - Host/join flows, persistent player identity (`ClientId`), authoritative state ownership validation, shared simulation clock synchronization in WAT (UTC+1).
   - Authoritative anti-cheat and security: Clients *never* authoritatively declare their own Treasury balance, country strength, or trade results; all transactions and state updates are sanitized and validated by the host.
   - Idempotent request handling, session recovery, reconnect routing, and corrupt save preservation.
2. **Bilateral Diplomacy and Trade**:
   - Sovereign bilateral recognition, direct diplomatic dispatches, and authoritative treasury-backed trade proposals.
   - Lifecycle stage transparency: Clear distinction between **Draft Proposal**, **Pending Proposal**, **Accepted Agreement**, and **Completed Transaction**.
   - Exactly-once settlement, duplicate execution prevention, and persistent agreements across reload.
3. **Offline Progression & Returning Player Briefings**:
   - Durable simulation catch-up crediting 6-hour production cycles, net REPU deposits, and completed capital projects.
   - Clear, non-silent `OfflineBriefingSummary` ensuring returning players understand all state changes that occurred during their absence.
4. **Military Readiness & Sovereign Defense Posture**:
   - National defense readiness percentage and DEFCON alert levels (DEFCON 5 to 1 peacetime-to-wartime monitoring) exposed directly within the Presidential Briefing. Low technical risk with immediate sovereign immersion.

### Deferred to Post-MVP Roadmap Waves
1. **Full Legislative Assembly & Parliamentary Decision-Making (Deferred to Wave 6)**:
   - *Rationale*: Candidate campaigns, party polling, elections, and democratic transitions into government are fully functioning in the MVP. Multi-faction legislative voting grids, parliamentary bill drafting, and coalition bargaining are deferred to avoid delaying the core multiplayer launch with complex non-player party AI.
2. **Military Operations, Combat Simulation & War (Deferred to Wave 7)**:
   - *Rationale*: High technical risk. Simulating tactical troop deployments, casualty tracking, border shifts, and multiplayer war exhaustion requires extensive balancing and would introduce unvetted exploit vectors into initial public play.
3. **Intelligence Operations & Covert Influence (Deferred to Wave 8)**:
   - *Rationale*: Espionage networks, covert election interference, and sabotage depend upon established bilateral diplomatic baselines. These systems will be introduced once inter-player peacetime statecraft is proven stable in live environments.
4. **Advanced Alliances, Multilateral Blocs & International Coalitions (Deferred to Wave 8)**:
   - *Rationale*: Bilateral agreements provide robust player interaction for the MVP. Multi-party consensus protocols and supranational voting bodies require complex quorum mechanics best layered onto a stable bilateral networking foundation.
5. **Additional Multilateral Treaties & Dynamic International Crises (Deferred to Wave 8)**:
   - *Rationale*: Domestic fiscal and civil unrest crises are active in the MVP. Global crisis engines (e.g. climate summits, international sanctions regimes) are deferred to post-MVP expansion.

