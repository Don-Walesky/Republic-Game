# Game Design Document (GDD)

The official Game Design Document for **Republic** defines all gameplay mechanics, features, simulation systems, and player experience.

---

## 1. Executive Summary & Vision

Republic is a **persistent multiplayer political and nation-building simulation**.

Every player governs their own procedurally generated country in an unbroken, evolving geopolitical world.

### The Player's Objective
> **BUILD, STRENGTHEN, PROTECT AND SUSTAIN THEIR COUNTRY.**

### The Country as the Central Simulation Object
The **country itself** is the central object of the simulation. Its territorial integrity, demographic health, infrastructure, productive economy, civic institutions, and defense capabilities form the core simulation state.

The **Presidential Office** serves as the executive command interface and command centre. Players direct the nation from the presidential suite, while the country is what lives, develops, and endures.

---

## 2. Foundational Multiplayer Systems

Multiplayer is a foundational property of Republic from day one. Sovereign player-governed nations coexist and interact continuously through:
- **Bilateral Trade**: Negotiated trade agreements, resource import/export contracts, and market competition denominated in REPU (`R`).
- **Diplomacy & Treaties**: Non-aggression treaties, mutual defense pacts, trade corridor access, diplomatic summits, and ambassadorial missions.
- **Foreign Investment**: Cross-border capital flows, joint infrastructure corridors, and special enterprise zones.
- **Alliances & Blocs**: Multi-nation security alliances, regional economic councils, and voting coalitions.
- **Intelligence & Espionage**: Covert operative networks, industrial espionage, counter-intelligence, border surveillance, and classified dossiers.
- **Economic & Geopolitical Rivalry**: Trade sanctions, tariff disputes, currency competition, and diplomatic pressure.
- **Military Pressure & Warfare**: Defensive readiness, border troop deployments, blockades, deterrence postures, and armed conflict.
- **International Negotiations**: Multilateral treaties, global peace accords, and international crisis mediation.

---

## 3. Canonical Core Game Loop

```
Govern country
   ↓
Develop country
   ↓
Make political / economic / security decisions
   ↓
Citizens, businesses, and institutions respond
   ↓
National capacity changes
   ↓
Interact with other countries
   ↓
Trade / negotiate / invest / ally / compete / conflict
   ↓
World produces consequences
   ↓
Country changes
   ↓
Respond and adapt
   ↓
Continue building the country
```

> **Guiding Principle**: Every important action should ultimately affect the strength, stability, prosperity, security, or political survival of the country.

---

## 4. Official Monetary System — REPU

The official currency of Republic is **REPU**.

- **Symbol**: `R`
- **Standard Formatting**: `R45M`, `R450M`, `R1.2B`
- **Rule**: REPU is the money of the game. It is not a secondary token or conversion commodity. Do not use the `₱` symbol.

---

## 5. National Yield & State Capacity

### National Yield
> *"National Yield is not free stuff given to the player. It is the measurable output and accumulating capacity of a functioning state."*

Calculated directly from the actual state of the country across 14 core categories:
1. REPU Treasury Revenue
2. Science / Research Capacity
3. Industrial Capacity
4. Energy Capacity
5. Infrastructure Capacity
6. Human Capital
7. Administrative Capacity
8. Military Readiness
9. Intelligence Capacity
10. Diplomatic Capacity
11. Innovation Capacity
12. Financial Capacity
13. Security Capacity
14. Natural Resource Output (where geographically applicable)

Many categories represent **accumulated institutional and structural capacity** rather than raw periodic deposits, systematically influencing other sectors.

### State Capacity
State Capacity measures the efficiency with which a government converts public resources and laws into tangible national outcomes. Governed by cabinet competence, civil service professionalism, institutional integrity, rule of law, anti-corruption enforcement, and constitutional legitimacy.

---

## 6. The Food Economy & Enabling Governance

- Food must **NOT** be a direct National Yield deposit.
- Citizens and agribusinesses produce food.
- The state creates the **enabling environment**: irrigation, rural roads, agricultural research, rural electrification, storage silos, agricultural credit, fertilizer supply chains, and secure land tenure.
- The food economy emerges organically from the nation's productive system.

---

## 7. Science as an Upstream National Capability

Science and Research Capacity acts as an **upstream multiplier**:
- Drives commercial innovation and patent generation
- Boosts crop yields and food resilience
- Improves healthcare and labor force stamina
- Enhances energy efficiency and manufacturing margins
- Modernizes defense weapons and cyber-intelligence
- Fuels long-term structural GDP growth

---

## 8. Government Appointments & Inherent Trade-offs

- Deliberate selection of cabinet ministers and agency directors based on technical competence, political loyalty, integrity, administrative ability, popularity, and ideology.
- Inherent trade-offs: Technocratic brilliance often carries low political loyalty; partisan loyalty often lacks technical capability.
- Vacant or incompetent appointments degrade ministry performance and National Yield.
- Appointments interact dynamically with institutions and legislative factions; they are never flat stat buffs.
- **Rule: Every important decision has pros and cons.**

---

## 9. Development Projects

- Tangible infrastructure: highways, universities, power plants, hospitals, railways, deepwater ports, industrial zones, research complexes, military bases.
- Defined by REPU cost, real simulation build duration, capacity gains, and maintenance costs.
- **Independence from Yield Cycles**: Project construction progresses continuously across real simulation time independent of scheduled National Yield cycles.

---

## 10. Simulation Time & Global Simulation Clock

- **Real-World Date Alignment**: In-game calendar matches the **real-world calendar**.
- **Game Clock**: **West Africa Time (WAT / UTC+1)**.
- **Official Launch**: **Republic Day 0, 00:00 WAT**, advancing continuously.
- **Shared Global Clock**: All player and AI nations exist on the same timeline. National Yield cycles synchronize to this global clock.

---

## 11. Founding, Independence Day & National Age

- **Independence Day**: Timestamped upon country creation (Date and Time in WAT, founding parameters, country age).
- **Founding Period**: Temporary period with **10x National Yield Multiplier** and temporary buffs (Administrative Momentum, National Innovation Drive, Founding Development Initiative, Investor Confidence, Diplomatic Recognition Momentum, Constitutional Institution-Building Momentum, Temporary National Unity). Tapers off to avoid permanent imbalances.
- **National Age**: Accumulated age reflects institutional tradition and legitimacy, but:  
  > **AGE MUST NOT AUTOMATICALLY EQUAL STRENGTH.**

---

## 12. Persistent Offline Simulation

> *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*

- The simulation never halts when a player logs off. REPU revenue accumulates, construction proceeds, diplomacy persists, and world events unfold.
- Players receive a concise **National Briefing** upon logging back in.

---

## Cross-References

- **Philosophy & Manifesto** → [Gameplay Philosophy](../Gameplay%20Philosophy/README.md)
- **Feature Map** → [Features](../Features/README.md)
- **Wave Roadmap** → [Roadmap](../Roadmap/README.md)
- **Executive Workspace** → [UX](../UX/README.md)