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

**Wave 0 is implemented as a testable .NET 8.0 foundation** with configuration management, multi-sink logging, high-performance event bus, deterministic time abstractions, persistence frameworks, and a headless world shell backed by 160+ automated unit tests.
