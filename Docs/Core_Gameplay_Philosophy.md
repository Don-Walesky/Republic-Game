# Game Design Document — Core Gameplay Philosophy

**Version**: 2.0 (Canonical Revised Republic Design)  
**Status**: Active  
**Last Updated**: 2026-10-06  

---

## 1. Sovereign Nation-Building Identity

Republic is a **persistent multiplayer political and nation-building simulation**.

Every player governs their own procedurally generated country in an unbroken, evolving geopolitical world.

### The Player's Objective
> **BUILD, STRENGTHEN, PROTECT AND SUSTAIN THEIR COUNTRY.**

### The Country as the Central Simulation Object
The **country itself**—its land, demographics, economy, infrastructure, civic institutions, and defense—is the central object of the simulation.

The **Presidential Office** serves as the player's command interface and executive command centre. Players experience the burden and privilege of leadership as head of government, but the entire country is what lives, develops, and endures.

### Foundational Multiplayer Reality
Republic is a multiplayer simulation from its very core. Sovereignty does not exist in a vacuum. Players continually interact with other player-governed countries through:
- Bilateral trade contracts and resource flows
- Diplomatic missions, summits, and ambassadors
- Treaties, non-aggression pacts, and mutual defense accords
- Cross-border capital investments and infrastructure corridors
- Geopolitical alliances and voting blocs
- Covert intelligence, counter-espionage, and reconnaissance
- Currency competition and trade friction
- Military posture, deterrence, and open conflict
- Multilateral negotiations and international governance

The world continues to exist, simulate, and evolve even when an individual player is offline.

---

## 2. The Canonical Core Game Loop

The fundamental gameplay cycle of Republic is structured as follows:

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

### Guiding Loop Rule:
> **Every important action should ultimately affect the strength, stability, prosperity, security or political survival of the country.**

---

## 3. Official Monetary System — REPU

The official currency of Republic is **REPU**.

- **Currency Symbol**: `R`
- **Standard Formatting**:
  - `R45M` = 45,000,000 REPU
  - `R450M` = 450,000,000 REPU
  - `R1.2B` = 1,200,000,000 REPU

> [!IMPORTANT]
> **REPU IS THE MONEY OF THE GAME.**  
> It is not a secondary token or conversion commodity. Do not use the `₱` symbol.  
> All budgets, taxes, trade agreements, and infrastructure costs are denominated exclusively in REPU (`R`).

---

## 4. National Yield & State Capacity

### National Yield Defined
> *"National Yield is not free stuff given to the player. It is the measurable output and accumulating capacity of a functioning state."*

National Yield is **not** an arbitrary allotment of free wealth. It is calculated directly from the actual physical, economic, and institutional condition of the country.

### Yield & Capacity Categories:
- **REPU Treasury Revenue**
- **Science / Research Capacity**
- **Industrial Capacity**
- **Energy Capacity**
- **Infrastructure Capacity**
- **Human Capital**
- **Administrative Capacity**
- **Military Readiness**
- **Intelligence Capacity**
- **Diplomatic Capacity**
- **Innovation Capacity**
- **Financial Capacity**
- **Security Capacity**
- **Natural Resource Output** (where geographically present)

Not every category represents a spendable numeric balance deposited each cycle. Many categories represent **accumulated institutional capacity** that empowers, accelerates, and stabilizes the rest of the nation.

### State Capacity as the Conversion Engine
**State Capacity** is the measure of how effectively a government converts resources, laws, and decisions into tangible results.

Influenced by:
- Government effectiveness and bureaucratic integrity
- Cabinet competence and institutional leadership
- Civil service professionalism vs. corruption and graft
- Rule of law and property rights enforcement
- Infrastructure reliability and educational attainment
- Political stability and constitutional legitimacy

*Two countries with identical natural resources and population will achieve radically different outcomes because of differences in governance and State Capacity.*

---

## 5. The Food Economy: Enabling Environment vs. Magic Yield

Food is **never** a direct National Yield deposit.

Governments do not conjure grain out of thin air. Citizens and agribusinesses produce food.

The government's role is to create the **enabling environment**:
- Modernizing irrigation networks and water security
- Constructing rural feeder roads and farm-to-market transit
- Funding agricultural research institutes and pest-resistant seed banks
- Providing rural electrification, cold storage, and logistics hubs
- Guaranteeing agricultural credit and fertilizer supply chains
- Securing land tenure and supporting agricultural mechanization

When the state provides this foundation, citizens and commercial enterprises produce food, and a dynamic, resilient food economy emerges naturally.

---

## 6. Science as an Upstream National Capability

Science and Research Capacity is not a trivial commodity to be hoarded or traded away; it is an **upstream national capability**.

Accumulating scientific capacity elevates all downstream sectors of the country:
- Accelerates commercial technology and innovation
- Boosts agricultural yield and food security
- Enhances public healthcare and citizen life expectancy
- Improves energy efficiency and industrial margins
- Advances sovereign defense systems and intelligence capability
- Drives long-term structural GDP expansion

---

## 7. Political Appointments & Meaningful Consequences

Appointing ministers and senior officials is a core gameplay mechanic with deep trade-offs.

### Candidate Profiles:
Candidates possess unique combinations of:
- Technical Competence
- Political Loyalty
- Personal Integrity
- Administrative Ability
- Public Popularity
- Ideological Alignment
- Institutional Experience
- Political Connections

### The Trade-Off Reality:
- A brilliant economist may deliver stellar growth but harbor low political loyalty, threatening cabinet revolts during difficult reforms.
- A loyal partisan may secure parliamentary discipline but lack the technical competence to manage complex ministries.
- Poorly staffed or vacant portfolios degrade the performance and yield of that entire sector.
- Appointments interact dynamically with institutions and factions rather than acting as simplistic flat `+5%` modifiers.

---

## 8. Every Important Decision Has Pros and Cons

Republic rejects simplistic "always choose the biggest number" gameplay. Every major policy choice involves real tension:
- **Competence vs. Loyalty**
- **Economic Growth vs. Social Stability**
- **Military Spending vs. Civilian Development**
- **Short-term Popularity vs. Long-term Productivity**
- **Capital Investment vs. Immediate Subsidies**
- **Diplomatic Cooperation vs. Sovereign Autonomy**
- **Scientific Freedom vs. State Control**
- **Taxation vs. Private Investment**
- **National Security vs. Civil Liberties**

---

## 9. Development Projects

National development transforms REPU treasury capital into lasting physical and institutional capacity:
- Transport (highways, ports, rail, airports)
- Health & Education (hospitals, universities, research parks)
- Energy & Utilities (power stations, electrical grids, water networks)
- Industry & Agriculture (industrial corridors, irrigation networks)
- Defense & Intelligence (military bases, radar grids, intelligence facilities)

**Project Mechanics**:
- Defined by REPU cost, build duration, capacity gains, and ongoing maintenance.
- **Independence from Yield Cycles**: Project construction is continuous and independent of National Yield cycles (e.g., an `R180M` project continues building over days of real simulation time while scheduled yield cycles occur around it).

---

## 10. Simulation Time & Shared Global Clock

Republic is synchronized to real-world time:
- **Date Alignment**: In-game calendar matches the **real-world calendar**.
- **Official Game Clock**: **West Africa Time (WAT / UTC+1)**.
- **Official Launch**: **Republic Day 0, 00:00 WAT**, advancing continuously.
- **No Fictional Calendar**: Time is unified, shared, and grounded in real-world date progression.
- **Global Simulation Clock**: All countries share the same global timeline. National Yield cycles are synchronized to the global simulation clock, not individual player session timers.

---

## 11. Founding, Independence Day & National Age

### Founding & Independence Day
When a player creates their country, that moment is recorded as its permanent **Independence Day**:
- Independence Date and Time in WAT (e.g., `Republic of X, Independence Day: 6 October 2026, 14:37 WAT`)
- Founding conditions and initial parameters
- National Age tracked continuously from that moment

### The Founding Period
Newly founded countries receive a temporary **Founding Period**:
- **10x National Yield Multiplier** to facilitate initial institutional and economic standup.
- **Temporary Founding Buffs**: Administrative Momentum, National Innovation Drive, Founding Development Initiative, Investor Confidence, Diplomatic Recognition Momentum, Constitutional Institution-Building Momentum, Temporary National Unity.
- *Strictly temporary*: buffs taper off as the country stabilizes, preventing permanent imbalances against older nations.

### National Age
Accumulated National Age reflects institutional maturity, constitutional traditions, and historical legitimacy.
> **Critical Rule: AGE MUST NOT AUTOMATICALLY EQUAL STRENGTH.**  
> A well-governed young nation can rapidly surpass an older, corrupt, or mismanaged rival.

---

## 12. Persistent Offline Simulation

> *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*

When a player disconnects, their country remains an active participant in the persistent world:
- Continuously earns REPU revenue and generates National Yield
- Advances project construction without interruption
- Sustains treaties, trade agreements, and diplomatic presence
- Experiences market movements, factional politics, and global events
- Maintains defense readiness and gathers intelligence

Upon logging back in, the Presidential Office provides a concise **National Briefing** highlighting all occurrences, milestones, and urgent alerts that unfolded while the leader was away.
