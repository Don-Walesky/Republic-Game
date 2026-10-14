# Executive Workspace

The executive command interface and command centre for governing your nation.

---

## 1. Overview & Vision

The **Presidential Office** is the player's primary command interface. While the **country itself** is the central simulation object living in a persistent multiplayer world, the executive workspace is the vantage point from which the head of government directs statecraft:
- Monitor national indicators and State Capacity
- Access ministerial departments and agencies
- Make high-impact executive decisions with explicit pros and cons
- Track continuous development projects denominated in **REPU** (`R`)
- Review intelligence dossiers and diplomatic communications
- Read the **National Briefing** upon logging in after offline periods

---

## 2. Layout Structure

### Header Bar
- **Sovereign Emblem & Country Name** (e.g., *Republic of Solaria*)
- **Global Simulation Clock**: Real-world date and current **West Africa Time (WAT / UTC+1)** (e.g., `6 October 2026, 14:37 WAT`)
- **Country Age & Independence Day Indicator**
- **Sovereign Security Status**: National DEFCON and stability rating
- **System Menu**: Audio, accessibility, and session options

> [!NOTE]
> Unlike single-player games, the workspace does **not** feature artificial pause, play, or fast-forward buttons. The simulation runs continuously on the shared global WAT clock.

### Main Desk & Office View
- **Interactive Executive Desk**:
  - *Secure Red Telephone*: Urgent calls, diplomatic crises, intelligence alerts
  - *Executive Computer / Terminal*: Email, official memoranda, cabinet reports
  - *National Dossier Binders*: Intelligence dossiers, budget legislation, decrees
  - *Calendar & Appointments Ledger*: Scheduled diplomatic summits, cabinet meetings
  - *Broadcast Television*: Breaking geopolitical news and domestic press coverage
- **National Map & Provincial Overview**: Geographical integrity, regional infrastructure, and development project sites

### National Telemetry & Yield Panel
- **REPU Treasury Balance**: Liquid sovereign funds (e.g., `R450M`, `R1.2B`)
- **National Yield Overview**: Productive and institutional output across 14 capacity categories (Science, Industry, Energy, Infrastructure, Security, etc.)
- **State Capacity Rating**: Civil service effectiveness, cabinet competence, and governance integrity
- **Active Development Projects**: Ongoing infrastructure construction with real-time completion countdowns

---

## 3. The National Briefing (Login Flow)

When a player returns to the Executive Workspace after being offline, the Presidential Office immediately presents the **National Briefing**:

1. **Duration Away**: Elapsed real-world time in WAT since last active session.
2. **Treasury & Yield Collections**: Total REPU revenue accumulated across completed yield cycles.
3. **Completed Development Projects**: Infrastructure and facilities finalized and commissioned during absence.
4. **Diplomatic & World Events**: Treaties signed or expired, foreign summit proposals, market shifts, and geopolitical developments.
5. **Urgent Items Requiring Executive Action**: Matters demanding immediate presidential attention (e.g., cabinet vacancies, legislative votes, border alerts).

---

## 4. Decision Spaces Navigation

From the Executive Workspace, the player transitions seamlessly into specialized decision spaces:
- **Cabinet Chamber**: Deliberate appointments of portfolio ministers with genuine trade-offs (competence vs. loyalty vs. integrity).
- **Situation Room**: Military readiness, border deterrence, and strategic intelligence operations.
- **Economic Briefing Hall**: National budget allocation, taxation policy, food enabling environment investments, and large-scale development projects.
- **Diplomatic Reception Hall**: Bilateral trade negotiations, treaty signing, alliance summits, and foreign ambassador meetings.
- **Press Briefing Room**: Press conferences, public addresses, and media management.

---

## 5. Phase 10 Implementation: First Playable Presidential Executive Workspace

As of Phase 10 ([ADR-0033](../Architecture/Decisions/ADR-0033-presidential-executive-workspace.md)), the command line presidential desk implements the complete first playable executive decision loop:

1. **Presidential Desk Dashboard**:
   - Displays sovereign identity, founding moment, and age in Republic days.
   - Authoritative Republic simulation clock in West Africa Time (WAT) and next scheduled 6-hour cycle boundary.
   - REPU Treasury formatted with standard `R` notation (e.g. `R4.5M`).
   - National Yield and State Capacity indicators across primary capacities.
   - Domestic physical commodity stockpiles (Iron, Petroleum, Timber, etc.).
   - Active development projects with completion countdowns, start and finish boundaries, and percentage progress.
   - Major political and stability health indicators (approval, stability, ruling/opposition parties, pending bills, and active crises).
   - Clear distinction between unestablished indicators (`Not established`) and zero.

2. **National Development Decision**:
   - Interactive specification review and initiation of the **National Power Plant** (Cost: R1,000,000, 72 hours / 12 cycle boundaries, +5.0 Energy Capacity).
   - Explicit presidential confirmation prompt (`[Y/N]`) where cancellation deducts zero funds.
   - Fund validation preventing overdrafts or partial deductions.

3. **Authoritative Time Advancement**:
   - Discrete simulation time advancement by 1 boundary (6h), 4 boundaries (24h), 12 boundaries (72h), engine frames, or custom hours.
   - Progression of active capital projects strictly grounded in simulation time.
   - Deterministic project completion and one-time benefit application upon reaching the finish boundary.
   - Seamless processing of 6-hour National Yield cycles and domestic resource extraction.
   - Offline catch-up integration briefing the president upon returning after simulated absences.