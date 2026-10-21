# Executive Office Redesign: Architecture & Foundation Plan

## 1. Project Overview & Redesign Objective

Republic is a 2D political governance strategy game where the player acts as the head of state of a sovereign republic.
The redesign goal is to transform the existing data-dense interface into a rich, immersive **Executive Presidential Office** inspired by strategy classics, set in a modern sovereign government environment.

The new interface establishes:
- A visual **Executive Presidential Office** environment with modular, interchangeable layers:
  - Capital window view (city scenery, horizon, weather, future development).
  - Country flag (sovereign colors, heraldry, national emblem).
  - Presidential portrait (head of state, framed brass plaque, political party).
  - Grand executive desk (mahogany/obsidian surface, blotter, hotline phone, presidential dossiers).
- Clear interface docking areas:
  - **Top Bar**: National identity, simulation clock (WAT / UTC+1), liquid treasury (REPU / R), public approval, stability, defense posture.
  - **Left Dock**: Domestic governance, cabinet ministers, civic capacity.
  - **Right Dock**: Bilateral diplomacy, intelligence, geopolitical trade corridors.
  - **Bottom Dock**: National infrastructure projects, crisis alerts, and situation ticker.
- Strict preservation of the authoritative C# simulation core (`Republic.Core`), persistent saves (`.sav`), and the existing working `PresidentialDesk.tscn`.

```
+-----------------------------------------------------------------------------------+
|  [TOP BAR]: Republic of Arcadia | Day 0 00:00 WAT | R 15,000,000,000 | 68.5% Appr |
+-----------------------------------------------------------------------------------+
| [LEFT DOCK]         |                 [OFFICE ENVIRONMENT]          | [RIGHT DOCK] |
|                     |   +-------------------+  +-----------------+  |              |
| Cabinet Ministers   |   |   CAPITAL WINDOW  |  | PRESIDENT       |  | Intelligence |
| Civil Service Merit |   |   (City Skyline)  |  | PORTRAIT        |  | Diplomacy    |
| Polling & Mandates  |   +-------------------+  +-----------------+  | Bilateral    |
|                     |               +------------+                  | Trade        |
|                     |               | FLAG POLE  |                  |              |
|                     |   +-----------+------------+---------------+  |              |
|                     |   |          EXECUTIVE DESK                |  |              |
|                     |   |   [Hotline]   [Blotter]   [Dossiers]   |  |              |
|                     |   +----------------------------------------+  |              |
+-----------------------------------------------------------------------------------+
|  [BOTTOM DOCK]: Active Project | National Situation Ticker | [Data Desk View]     |
+-----------------------------------------------------------------------------------+
```

---

## 2. Technical Inspection of Existing Systems

### Current Main Scene & Launch Mechanism
- **Main Scene**: `res://PresidentialDesk.tscn` (configured in `client/godot/project.godot`).
- **Script**: `res://PresidentialDesk.gd` (2,940+ lines of GDScript).
- **Execution Mode**: Runs directly in Godot 4.7+ standard engine, supports headless testing via `godot --headless -s <test_script>.gd`.
- **Backend Service**: `Republic.Cli` (`dotnet run --project src/Republic.Cli -- --watch-requests`).

### Authoritative State & Persistence Architecture
1. **Live State Export**: `PresidentialDeskBridge.cs` in `Republic.Core` exports `PresidentialDeskSnapshot` to `res://presidential_desk_state.json`.
2. **State Consumption**: Godot monitors file modification time (`FileAccess.get_modified_time`) every 0.5 seconds, auto-refreshing UI without polling loops.
3. **Decision Pipeline**: User interactions write `presidential_desk_request.json` -> `PresidentialDeskBridge` validates and applies changes in `Republic.Core` -> writes `presidential_desk_result.json` and authoritative `.sav` -> updates `presidential_desk_state.json`.
4. **Honest Data Rule**: If state is missing, the interface displays `[No State Loaded]`. Mock, invented, or hardcoded sample numbers are strictly prohibited.

### Asset Audit
- Existing repo contains zero external image assets for office backgrounds, flags, or portraits (only Godot web export icons in `builds/web`).
- Therefore, procedural 2D CanvasItem vector rendering, `StyleBoxFlat`, `Gradient`, and layered `Control` nodes provide high-quality foundation placeholders until custom artistic sprites are authored in future steps.

---

## 3. Executive Office Foundation Architecture

### Layer 1: Visual Environment (`OfficeEnvironment`)
Separated completely from the HUD to allow independent swapping of scenery, furniture, and decor:
1. **`RoomBackdrop`**: Deep navy/slate executive wall paneling with brushed brass accent molding.
2. **`CapitalWindow`**:
   - `SkyBackdrop`: Atmospheric horizon gradient (WAT daylight/twilight).
   - `CityScenery`: Modular capital skyline with government dome, parliamentary spires, and modern cityscape silhouettes.
   - `WindowMullions`: Architectural frame separating the interior from the capital view.
3. **`PresidentialPortrait`**:
   - `PortraitFrame`: Gilded executive frame.
   - `PortraitCanvas`: Head of state silhouette/avatar.
   - `Plaque`: Brass nameplate dynamically displaying president name and party.
4. **`CountryFlag`**:
   - `FlagPole`: Executive brass stanchion and finial.
   - `FlagCanvas`: Sovereign banner dynamically reflecting national colors.
5. **`ExecutiveDesk`**:
   - `DeskSurface`: Rich mahogany/obsidian executive desk occupying the lower-center canvas.
   - `DeskBlotter`: Leather blotter with gold stitching.
   - `DeskProps`: Modular placeholders for executive hotline telephone, daily intelligence inbox/dossier, and national seal.

### Layer 2: Interface HUD (`InterfaceLayer`)
Anchored along screen edges with high information density and semi-transparent glassmorphic panels:
- **`TopBar`**: Compact, premium status header showing live authoritative metrics.
- **`LeftDock`**: Domestic & Cabinet dock (expandable/collapsible).
- **`RightDock`**: Intelligence & Diplomacy dock (expandable/collapsible).
- **`BottomDock`**: Projects & Status ticker with quick navigation button to legacy `PresidentialDesk`.

### Layer 3: Authoritative State Connector (`StateConnector`)
- Reusable state loader observing `presidential_desk_state.json`.
- Zero mock data: displays `[No State Loaded]` when uninitialized.
- Emits typed state signals when simulation advances.

---

## 4. Phased Implementation Roadmap

| Phase | Scope | Status |
| :--- | :--- | :--- |
| **Phase 1** | System inspection, foundation scene architecture, decoupled visual layers (Window, Flag, Portrait, Desk), HUD docking zones, live state connector, test harness. | **Verified** |
| **Phase 2** | Functional Executive Office interface: Top resource bar live metrics (Country, Treasury, Approval, Stability, Defense, WAT), 6-section navigation tabs, Presidential Briefing modal with real state & honest disclosures, interactive office controls (Hotline phone, Inbox, Dossier, Vista, Mandate), bidirectional navigation with PresidentialDesk. | **Verified** |
| **Phase 3** | Personalised Presidential Office & Modular Customisation: 8 selectable presidential portraits (5 male, 3 female), sovereign country flag & heraldry, 6 modular furniture categories (desks & chairs, visitor seating, walls & flooring, carpets & lighting, bookshelves & decor, artwork & gifts), authoritative REPU pricing & purchasing validation, facilities/interior approvals, country-isolated persistent storage, and customisation modal UX. | **Verified** |
| **Living Scenery & Desk Interactivity** | State-driven capital window feedback (prosperity, construction cranes, civil unrest, atmospheric crisis haze), interactive desk controls with notification badges, progressive dock disclosure, soft portrait/flag mood reactivity, and session briefing toast. | **Verified** |
| **Phase 4** | Domestic Cabinet & Ministry Dock: Left Dock cabinet panel with all 6 constitutional portfolios (Finance, Defense, ForeignAffairs, Interior, Health, Infrastructure), minister appointment cards with competence, loyalty, and integrity ratings, honest state capacity effects (Civil Service Quality, Corruption Control, Infrastructure Condition), appointment modal for supported portfolios with Technocrat, Loyalist, and Dismiss choices, core rules consequence previews, and file-based appointment action pipeline (`presidential_desk_request.json`). | **Verified** |
| **Phase 5** | Geopolitics & Foreign Relations Dock (Bilateral trade routes, DEFCON posture, summit negotiation). | Planned |
| **Phase 6** | Capital City Scenery expansion (Day/Night cycle, weather, infrastructure development overlays). | Planned |

---

## 5. Verification Checkpoint Audit & Test Registry

All automated suites were executed against the Godot 4.7.2 engine and .NET 8 test runner:

| Test Suite | Command | Total Checks | Result | Exit Code |
| :--- | :--- | :--- | :--- | :--- |
| `test_executive_office.gd` | `godot --headless --path client/godot -s test_executive_office.gd` | 8 | **PASS** (8/8) | 0 |
| `test_executive_office_phase2.gd` | `godot --headless --path client/godot -s test_executive_office_phase2.gd` | 7 | **PASS** (7/7) | 0 |
| `test_executive_office_phase3.gd` | `godot --headless --path client/godot -s test_executive_office_phase3.gd` | 6 | **PASS** (6/6) | 0 |
| `test_executive_office_living_feedback.gd` | `godot --headless --path client/godot -s test_executive_office_living_feedback.gd` | 5 | **PASS** (5/5) | 0 |
| `test_executive_office_phase4.gd` | `godot --headless --path client/godot -s test_executive_office_phase4.gd` | 8 | **PASS** (8/8) | 0 |
| `test_presidential_desk.gd` | `godot --headless --path client/godot -s test_presidential_desk.gd` | 28 | **PASS** (28/28) | 0 |
| `test_first_playable_loop.gd` | `godot --headless --path client/godot -s test_first_playable_loop.gd` | 8 | **PASS** (8/8) | 0 |
| `test_phase3_governance_loop.gd` | `godot --headless --path client/godot -s test_phase3_governance_loop.gd` | 7 | **PASS** (7/7) | 0 |
| `test_mvp_multiplayer_simulation.gd` | `godot --headless --path client/godot -s test_mvp_multiplayer_simulation.gd` | 3 | **PASS** (3/3) | 0 |
| `Republic.Core.Tests` | `dotnet test --nologo` | 775 | **PASS** (775/775) | 0 |
| **Total** | | **855** | **100% PASS** | **0** |

### Launch Behaviour Analysis (`project.godot`)
- **Current Main Scene**: `res://PresidentialDesk.tscn`
- **Confirmation of Intent**: Intentionally preserved. `PresidentialDesk.tscn` houses the full feature-complete gameplay and decision loops (cabinet appointments, economic projects, campaign rallies, diplomacy, and multiplayer).
- **Bidirectional Bridge**: Both scenes are tightly integrated:
  - `PresidentialDesk.tscn` provides a title button (`SwitchToExecutiveOfficeButton`) to enter the Executive Office.
  - `ExecutiveOffice.tscn` provides direct navigation (`SwitchToDeskButton` / section tabs) to seamlessly jump into specific tabs of `PresidentialDesk.tscn` via `PresidentialDesk.requested_section`.
- **Verdict**: Retaining `PresidentialDesk.tscn` as the launch scene guarantees stability and regression safety until user sign-off authorizes making `ExecutiveOffice.tscn` the primary entrypoint.

---

## 6. Phase 4 Architecture & Audit Specification: Domestic Cabinet & Ministry Dock

### 6.1 Audit of Existing Systems & Files

| Component / Layer | Relevant Files & Services | Current Implementation State |
| :--- | :--- | :--- |
| **Domain Models** | [`src/Republic.Core/Cabinet/Models/CabinetPortfolio.cs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Cabinet/Models/CabinetPortfolio.cs)<br>[`src/Republic.Core/Cabinet/Models/Minister.cs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Cabinet/Models/Minister.cs) | **Authoritative & Fully Implemented**.<br>Defines 6 portfolios (`Finance`, `Defense`, `ForeignAffairs`, `Interior`, `Health`, `Infrastructure`). Models 5 normalized ratings: `Competence` (0–1), `Loyalty` (0–1), `Integrity` (0–1), `Experience` (0–1), `PoliticalConnections` (0–1), plus `MonthlySalary` and `IsAppointed`. |
| **State Capacity & Country Logic** | [`src/Republic.Core/World/Models/Country.cs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/World/Models/Country.cs) (`RecalculateCabinetStateCapacity()`, `AppointMinister()`, `VacatePortfolio()`) | **Authoritative & Fully Implemented**.<br>- Finance (`Competence` & `Integrity`) sets `StateCapacity.CivilServiceQuality` (or 0.3 if vacant).<br>- Interior (`Competence` & `Integrity`) sets `StateCapacity.CorruptionControl` (or 0.3 if vacant).<br>- Infrastructure (`Competence` & `Experience`) sets `StateCapacity.InfrastructureCondition` (or 0.3 if vacant).<br>- *Note*: Loyalty and political connections deliberately do not elevate state capacity. |
| **Decision Bridge & Request Processing** | [`src/Republic.Core/Client/PresidentialDeskBridge.cs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Client/PresidentialDeskBridge.cs) (`ApplyCabinetAppointmentDecision()`, `ApplyCrisisResponseDecision()`) | **Authoritative & Fully Implemented**.<br>- Action `appoint_minister` handles `portfolio` (`Finance`, `Interior`, `Infrastructure`) with candidates: `technocrat`, `loyalist`, or `dismiss`.<br>- Technocrat appointment: sets competence/integrity to 0.90, loyalty to 0.50; approval +1.5%, party polling support -3.0%.<br>- Loyalist appointment: sets competence/integrity to 0.45, loyalty to 0.95; approval -1.5%, party polling support +3.0%.<br>- Action `crisis_response` handles `civic_relief` (costs R100k; approval +4.0%, stability +3.0%) and `austerity` (+R500k treasury; approval -4.0%, stability -2.0%). |
| **State Export & Telemetry** | [`src/Republic.Core/Client/PresidentialDeskSnapshot.cs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/Client/PresidentialDeskSnapshot.cs)<br>[`client/godot/presidential_desk_state.json`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/client/godot/presidential_desk_state.json) | **Partially Implemented in Snapshot**.<br>Exports `cabinet_summary`, `finance_minister`, `interior_minister`, `civil_service_quality`, `corruption_control`, and `state_capacity_summary`. Detailed candidate roster and other 4 portfolios are currently not expanded in the flat JSON snapshot. |
| **Legacy Desk UI & Controls** | [`client/godot/PresidentialDesk.gd`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/client/godot/PresidentialDesk.gd)<br>[`client/godot/PresidentialDesk.tscn`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/client/godot/PresidentialDesk.tscn) | **Working & Fully Tested**.<br>Contains `appoint_technocrat_finance_button`, `appoint_loyalist_finance_button`, `appoint_technocrat_interior_button`, `appoint_loyalist_interior_button`, `cancel_cabinet_action_button`, `emergency_austerity_button`, `emergency_relief_button`. |
| **Executive Office Scene** | [`client/godot/ExecutiveOffice.tscn`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/client/godot/ExecutiveOffice.tscn)<br>[`client/godot/ExecutiveOffice.gd`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/client/godot/ExecutiveOffice.gd) | **Foundation Implemented; In-Office Dock Missing**.<br>`LeftDock` currently displays read-only metrics (`CabinetSummaryLabel`, `CivilServiceLabel`, `CorruptionControlLabel`, `RulingPartyLabel`, `PollingSupportLabel`) and an `OpenCabinetButton`. Clicking `OpenCabinetButton` or `NavCabinetButton` currently switches the entire scene back to `PresidentialDesk.tscn`. |

---

### 6.2 Intended Cabinet Dock Specification

The Cabinet Dock inside the Executive Office will provide a rich in-office domestic governance interface:
1. **Ministerial Portfolios Roster**:
   - Tabbed/card overview of key ministries: **Finance** (Civil Service), **Interior** (Corruption Control), **Infrastructure** (Public Works), and **Defense** (Readiness).
   - Shows current appointee, status (`Appointed` vs `Vacant`), and monthly ministerial salary.
2. **Minister Profile & Trait Inspection**:
   - Visual breakdown of the minister's normalized attributes: Competence, Loyalty, Integrity, Experience, and Political Connections.
   - Narrative background, strengths, and weaknesses (e.g. "Tenured Economist: High fiscal discipline, aloof towards parliamentary whips").
3. **Transparent Trade-off Communication**:
   - **Technocrat vs. Loyalist Matrix**:
     - *Technocrat*: High competence & integrity (elevates State Capacity to 90%, +1.5% public approval), but lower political loyalty (party base feels alienated, -3.0% polling support).
     - *Loyalist*: High loyalty & connections (rallies party base, +3.0% polling support), but compromised administrative competence (State Capacity capped at 45%, -1.5% public approval).
     - *Vacant*: Institutional capacity drops to emergency baseline (30%).
4. **Authoritative Action Pipeline**:
   - Selecting a candidate opens a vetting/confirmation card displaying projected delta consequences.
   - Confirming generates a standard `appoint_minister` request to `presidential_desk_request.json` with `portfolio`, `candidate_type`, and `confirmed: true`.
   - Results are processed by `Republic.Cli` / `PresidentialDeskBridge`, updating the authoritative `.sav` and refreshing the UI.
5. **Executive Orders & Emergency Decrees Surface**:
   - Dedicated tab within the Cabinet Dock for presidential decrees:
     - **Civic Relief Package**: Allocates R100,000 liquid funds to citizen emergency services; restores +4.0% approval, +3.0% stability. Validates treasury balance before submission.
     - **Emergency Austerity Decree**: Reallocates +R500,000 into liquid treasury from ministerial operational reductions; causes -4.0% approval, -2.0% stability.
     - **Citizen Town Hall / Public Address**: Direct citizen engagement directive (R50,000 cost; +1.0% approval, +1.5% stability, +1.5% polling support).

---

### 6.3 Implementation Plan (Phased Subtasks)

To ensure zero risk to existing gameplay and regression suites, Phase 4 will be implemented in small, verifiable steps:

1. **Subtask 4.1: Snapshot Telemetry & Catalog Expansion**
   - Enhance snapshot serialization in `PresidentialDeskSnapshot.cs` (or create a dedicated GDScript catalog helper `CabinetMinisterCatalog.gd`) defining candidate biographies, traits, portraits, and projected impacts for all portfolios.
   - Maintain 100% backward compatibility with `presidential_desk_state.json`.

2. **Subtask 4.2: In-Office Cabinet Dock UI Surface**
   - Expand `LeftDock` in `ExecutiveOffice.tscn` to include:
     - Ministry selector tab bar (Finance, Interior, Infrastructure, Defense).
     - Minister dossier card with attribute gauges (Competence, Loyalty, Integrity).
     - Interactive candidate vetting slots (Technocrat option, Party Loyalist option, Vacate/Dismiss option).
     - Dedicated Executive Orders sub-panel with live cost validation.
   - Style to seamlessly match the modern presidential glassmorphic aesthetic (`StyleBoxFlat_glassdock`).

3. **Subtask 4.3: Interactive Controller & Request Integration**
   - Connect `ExecutiveOffice.gd` controls to write standard `presidential_desk_request.json` actions (`appoint_minister`, `crisis_response`, `town_hall`).
   - Implement client-side confirmation modals and real-time validation (e.g., preventing Civic Relief submission if treasury < R100,000).
   - Display pending spinner, status feedback, and ticker updates.

4. **Subtask 4.4: Headless Automated Acceptance Suite**
   - Create `test_executive_office_phase4_cabinet.gd` validating:
     - Dock expansion and ministry selection.
     - Telemetry inspection and vacant baseline display.
     - Candidate selection and trade-off preview calculation.
     - Confirmed Technocrat and Loyalist request generation.
     - Executive order eligibility and treasury deduction validation.
     - Bidirectional consistency with `PresidentialDesk.gd`.

---

### 6.4 Safety Protections & Non-Goals
- **No Simulation Core Forking**: All state modifications route through `PresidentialDeskBridge` and `Country.cs`. No duplicate C# services or parallel simulations will be created.
- **Save/Load Integrity**: All state persists to the authoritative `.sav` envelope. Country isolation is preserved via `country_id`.
- **Default Launch Scene**: `project.godot` retains `run/main_scene="res://PresidentialDesk.tscn"`.
- **Strict Boundaries**: National Buff Office, purchasable buffs, and military command centers remain excluded.

---

### 6.5 Phase 4 Verification & Validation Status (Independently Verified Checkpoint)

| Component | Implementation Status | Independently Verified Behavior |
| :--- | :--- | :--- |
| **Test Cleanup Safety Hazard Fix** | **Resolved & Protected** | • `res://presidential_desk_request.json` removed from cleanup lists across all test suites.<br>• Isolated test sandbox directory `res://temp_phase4_isolated/` created for test-owned temporary state, save, request, and result files.<br>• Cleanup runs safely on pass/fail without deleting or modifying real player files.<br>• **Regression Check 9**: Actively proves that pre-existing player request files are preserved untouched throughout test execution. |
| **Real C# CLI Simulation Pipeline** | **Integrated & Verified** | • `Republic.Cli/Program.cs` parses `--request-file=` alongside `--state-file=` and `--save-file=`.<br>• `ExecutiveOffice.gd` resolves request/result files dynamically adjacent to the active state file.<br>• `ExecutiveOffice.gd` passes normalized, simplified paths to `dotnet` and synchronously/asynchronously detects execution results from `presidential_desk_result.json`.<br>• Real test country founded via `Republic.Cli --create-country` in isolated directory without mock data.<br>• **Check 11**: End-to-end round-trip verified: Godot request write → C# CLI process execution → `Republic.Core` appointment processing → `.sav` persistence → snapshot export → Godot state reload & UI display. |
| **Supported Portfolios & Actions** | **All 3 Portfolios Verified** | Verified across all 3 active ministries (`Finance`, `Interior`, `Infrastructure`) for all 3 supported actions (`Technocrat`, `Loyalist`, `Dismiss/Vacate`):<br>• **Finance**: Dr. Elena Rostova (Civil Service Quality: 90.0%), Senator Donald Vance (45.0%), Dismiss (reverts to baseline 30.0%).<br>• **Interior**: Dr. Alistair Finch (Corruption Control: 90.0%), Party Whip Thomas Sterling (45.0%), Dismiss (baseline 30.0%).<br>• **Infrastructure**: Eng. Marcus Bennett (Infrastructure Condition: 85.0%), Alderman Gregory Cross (45.0%), Dismiss (baseline 30.0%). |
| **Edge Cases & Failure Resilience** | **Verified & Guarded** | • **Cancellation**: Unconfirmed appointment (`confirmed = false`) causes zero state mutation to state or `.sav`.<br>• **Invalid Input**: Unknown candidate type produces `[Rejected]` status with authoritative core message.<br>• **Data Preservation**: Unrelated country data (sovereign treasury R 10B, ruling party Democratic Union) preserved intact.<br>• **Missing Processor (Check 10)**: Missing or failed simulation processor cleanly reports explicit failure message instead of false success.<br>• **Non-Interactive Cards**: Defense, Foreign Affairs, Health cards remain visible with unassigned indicators. |
| **Windows CLI File Locking Fix** | **Resolved & Hardened** | `FileLogSink.cs` updated to use `FileShare.ReadWrite` and catch transient I/O exceptions, ensuring that CLI instances running in parallel or alongside Godot logging never crash from Windows file locks. |
| **Automated Test Suites** | **100% Passing** | • `test_executive_office_phase4.gd`: **11/11 checks passed** (exit code 0)<br>• `test_executive_office.gd`: **8/8 checks passed** (exit code 0)<br>• `test_executive_office_phase2.gd`: **7/7 checks passed** (exit code 0)<br>• `test_executive_office_phase3.gd`: **6/6 checks passed** (exit code 0)<br>• `test_executive_office_living_feedback.gd`: **5/5 checks passed** (exit code 0)<br>• `test_presidential_desk.gd`: **28/28 checks passed** (exit code 0)<br>• `dotnet test --nologo`: **785/785 tests passed** (exit code 0) |



