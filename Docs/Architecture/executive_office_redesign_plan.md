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
| **Phase 1** | System inspection, foundation scene architecture, decoupled visual layers (Window, Flag, Portrait, Desk), HUD docking zones, live state connector, test harness. | **Completed** |
| **Phase 2** | Functional Executive Office interface: Top resource bar live metrics (Country, Treasury, Approval, Stability, Defense, WAT), 6-section navigation tabs, Presidential Briefing modal with real state & honest disclosures, interactive office controls (Hotline phone, Inbox, Dossier, Vista, Mandate), bidirectional navigation with PresidentialDesk. | **Completed** |
| **Phase 3** | Capital City Scenery expansion (Day/Night cycle, weather, infrastructure development overlays). | Planned |
| **Phase 4** | Domestic Cabinet & Ministry Dock (Minister portraits, loyalty/competence trade-offs, executive orders). | Planned |
| **Phase 5** | Geopolitics & Foreign Relations Dock (Bilateral trade routes, DEFCON posture, summit negotiation). | Planned |
