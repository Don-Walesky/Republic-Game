# Player Flow

The authoritative user flow and interaction progression for **Republic**.

---

## 1. Authentication & Country Founding Flow

```
Launch Republic
       ↓
Authenticate / Sign In
       ↓
┌──────────────────────────────────────────────┐
│  Has Sovereign Nation Already?               │
└──────────────────────┬───────────────────────┘
          │ NO                       │ YES
          ▼                          ▼
Procedural Country Creation      Connect to Persistent World
- Choose Country Name & Flag                 ↓
- Review Starting Geography        Generate National Briefing
- Establish Founding Constitution  (Recaps offline events, yields,
          ↓                         revenues, project completions)
Founding Timestamp Recorded                  ↓
(Independence Day & Time in WAT)   Enter Executive Workspace
          ↓                       (Presidential Office)
Founding Period Initiated
(10x Yield & Momentum Buffs)
          ↓
Enter Executive Workspace
```

---

## 2. The Persistent Governance Loop

Unlike turn-based or pause-driven single-player strategy titles, Republic runs continuously on the **shared global simulation clock in West Africa Time (WAT / UTC+1)**. The gameplay loop is structured around continuous statecraft:

```
Review National Status & Briefing (Executive Desk Telemetry)
       ↓
Engage Decision Spaces (Cabinet, Economy, Military, Diplomacy)
       ↓
Make Executive Choices (Analyze explicit pros and cons)
       ↓
Commit REPU Capital to Development Projects & Programs
       ↓
Citizens, Businesses & Institutions Respond in the Country
       ↓
National Capacity & Yield Change Deterministically
       ↓
Interact with Other Sovereign Countries (Trade, Treaties, Alliances, Spies, War)
       ↓
World Produces Geopolitical & Market Consequences
       ↓
Country Changes & Evolves
       ↓
Respond, Adapt, and Continue Sustaining the Nation
```

---

## 3. Persistent Real-Time Progression

Republic does **not** feature single-player time manipulation controls (pause, fast-forward, rewind, or arbitrary date skipping).

### Living Time Progression:
- The global world clock advances continuously in **West Africa Time (WAT / UTC+1)**.
- Development projects advance their real-time construction countdowns uninterruptedly.
- Synchronized National Yield cycles occur at regular global simulation intervals across all nations concurrently.
- Other player-governed and AI-governed nations make decisions, adjust trade tariffs, move military forces, and propose treaties continuously.

---

## 4. The Offline & Reconnection Cycle

> *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*

```
Player Logs Off
       ↓
Country Persists in Living World
- Collects REPU Treasury Revenue across scheduled yield cycles
- Progresses Development Projects toward completion
- Civil service executes standard institutional policies
- Defenses maintain deterrence and border postures
- Bilateral trade agreements and treaties remain active
       ↓
Player Logs In
       ↓
National Briefing Presented
- Highlights elapsed time away in WAT
- Summarizes REPU revenue collected and capacity shifts
- Reports completed infrastructure and development projects
- Flags diplomatic cables, treaty alerts, and intelligence developments
- Outlines urgent matters demanding immediate executive action
       ↓
Player Enters Executive Workspace to Govern
```

---

## 5. Navigation Between Decision Spaces

From the Executive Workspace, the player moves directly between specialized decision spaces:

```
Executive Workspace (Central Command Hub)
├── Cabinet Chamber → Appoint Ministers, Review Portfolios, Manage Stability
├── Economic Briefing Hall → Set REPU Budget, Tax Rates, Launch Projects
├── Situation Room → Review Military Readiness, DEFCON Levels, Operations
├── Diplomatic Reception Hall → Bilateral Trade, Treaties, Summits, Alliances
├── Intelligence Office → Review Covert Dossiers, Counter-Intel, Infiltration
└── Press Briefing Room → Hold Press Conferences, Track Public Sentiment
```