# Republic - Godot Client

This folder contains the Godot 4 client for Republic, connected directly to the authoritative `Republic.Core` simulation state and providing interactive presidential decision capabilities.

---

## Novice Quick Start: Single-Player Presidential Game Loop

Follow these exact steps to run the complete, playable single-player presidential loop: create a country, view the national briefing, enact tax reform, commission a national project, and verify persistence.

### Step 1 — Start the Simulation Watcher

In your terminal at the repository root, start the background request watcher daemon:

```bash
dotnet run --project src/Republic.Cli -- --watch-requests
```

*(Alternatively, run `dotnet run --project src/Republic.Cli` for the interactive CLI which also processes requests in the background).*

### Step 2 — Open and Run the Presidential Desk in Godot

1. Open **Godot Engine 4.x**.
2. Open or import `client/godot/project.godot`.
3. Press **F5** (or click the **Play** button in the top right).

The Presidential Desk opens with the dark executive theme. If no state has been loaded yet, the desk safely displays `[No State Loaded]` without fallback demo values.

### Step 3 — Create or Load Your Republic

Under **Country Setup (Single-Player Loop)**:
1. **Found a New Country**:
   - In **Country Name**, enter a sovereign name that contains the whole word **"Republic"** (case-insensitive), for example:
     `Republic of Arcadia` (or `Federal Republic of Nigeria`, `Republic of Solaria`).
     *(Names omitting "Republic", such as "Kingdom of Arcadia", will be rejected with `Founder Status: [Rejected] Invalid Name`).*
   - Click **Found Republic**.
   - Notice the status update: `Founder Status: [Pending] Founding Republic of Arcadia...`.
   - The `.NET` watcher validates constitutional rules, establishes the sovereign nation, generates an initial treasury of **R 10,000,000**, saves `republic_game.sav`, and writes `presidential_desk_state.json`.
   - Within 1–2 seconds, the desk updates: `Founder Status: [Success] Sovereign republic established: Republic of Arcadia`.
2. **Resume an Existing Save** (Alternative):
   - Enter your save file path (e.g. `republic_game.sav` or custom path) and click **Resume Save**.
   - The authoritative save is loaded, live state is refreshed, and your sovereign state is restored intact.

### Step 4 — Inspect the National Briefing

Review the **National Briefing** panel in the center of the desk:
- **Treasury**: Current sovereign treasury balance (e.g. `R 10,000,000`).
- **Approval**: Citizen public approval rating (e.g. `70.0%`).
- **Stability**: Sovereign stability rating (e.g. `80.0%`).
- **Active Project**: Displays currently commissioned national infrastructure (initially `None`).
- **Latest Decision**: Displays the outcome of the most recent executive action.

### Step 5 — Enact a Tax Decision

1. Under **Presidential Decision: Taxation Policy**, find the **Proposed Income Tax Rate (%)** spinbox.
2. Adjust the rate from `25.0%` to `30.0%`.
3. Note the live dynamic preview:
   `Expected Approval Effect: -2.5% (Tax hike reduces household disposable income; -0.5% approval per +1% tax).`
4. Click **Confirm Decision**:
   - Status updates: `Decision Result: [Pending] Tax decision submitted...`.
   - The watcher processes the request in `Republic.Core`, applies the fiscal change, updates citizen approval (to `67.5%`), writes the persistent `.sav` and `.json` state.
   - Within 1–2 seconds, the desk updates:
     `Decision Result: [Success] Successfully applied - Income tax rate updated from 25.0% to 30.0%...`.
   - The top header and the **National Briefing** both reflect the new approval rating and tax rate.

### Step 6 — Commission a National Project (National Road Program)

1. Under **National Projects (Single-Player Loop)**, click **Start Road Program (R500,000)**.
2. The desk immediately writes a project initiation request:
   `Project Status: [Pending] Submitting commission...`.
3. The simulation core verifies available treasury funds (cost: R500,000), checks for active project concurrency, commissions the **National Road Program** (with a 90-day completion ETA), deducts R500,000 from the treasury, and updates persistent state.
4. The desk updates:
   - `Project Status: [Success] National Road Program initiated. R500,000 deducted from treasury.`
   - **Active Project** (both in header and National Briefing): `National Road Program (ETA: ...)`
   - **Treasury**: Decreased by R500,000 (e.g. `R 9,500,000`).
5. Clicking **Cancel Order** cancels unconfirmed project requests without any treasury deduction:
   `Project Status: [Cancelled] Order cancelled.`

### Step 7 — Confirm Persistence Across Reload

1. Close the Godot window and press **F5** to reopen it, or click **Refresh / Apply Pending**.
2. Notice that the country identity (`Republic of Arcadia`), treasury balance (`R 9,500,000`), income tax rate (`30.0%`), approval rating, stability, and active project (`National Road Program`) are completely preserved and restored intact from the authoritative save.

---

## Requirements

- [Godot Engine 4.x](https://godotengine.org/download) (Standard or .NET build)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for simulation core)

---

## Simulation State Connection & Architecture

The Presidential Desk interface reads authoritative state exported from `Republic.Core`:
- **State File**: `res://presidential_desk_state.json` (also checks `user://presidential_desk_state.json` or custom path via `--state-file=<path>` or `REPUBLIC_STATE_FILE`).
- **Live Fields Connected**:
  - **Country**: Sovereign nation name (e.g. `Republic of Arcadia`).
  - **Treasury**: Sovereign liquid treasury balance in REPU (e.g. `R 9,500,000`).
  - **Approval**: Public approval rating percentage (e.g. `67.5%`).
  - **Stability**: Baseline stability rating percentage (e.g. `80.0%`).
  - **Active Project**: Currently active commissioned project and completion ETA (e.g. `National Road Program`).
  - **Simulation Time**: Authoritative simulation time in West Africa Time (e.g. `Day 0, 00:00 WAT`).
  - **Ruling party**: Active ruling party name or `None`.
  - **Election**: Most recent democratic national election result or `No election`.
  - **Income Tax Rate**: Current personal income tax rate percentage (e.g. `30.0%`).
  - **System Status**: Full connection status, state file presence, last update timestamp, and request telemetry.
- **Auto-Refresh**: The scene automatically detects modification timestamps on the state file every 0.5s and reloads instantly when simulation time advances or policies change.
- **Safe Fallback**: If the state file is missing or contains invalid JSON, the interface displays `[No State Loaded]` with a visible status message; it never silently falls back to hardcoded sample values.

---

## Interactive Presidential Decision & Management Controls

The presidential desk provides interactive governance controls allowing the player to manage sovereign affairs:
1. **Country Creation & Loading**:
   - `CountryNameLineEdit` & `FoundCountryButton`: Creates a new sovereign nation. Validates that the country name contains the word "Republic".
   - `SavePathLineEdit` & `ResumeSaveButton`: Resumes from an existing authoritative `.sav` file.
2. **National Briefing Panel**:
   - Centrally presents five key pillars: Treasury balance, Public approval, Baseline stability, Active infrastructure project, and Latest decision outcome.
3. **Taxation Policy Reform**:
   - `ProposedRateSpinBox`: Adjusts personal income tax rate within constitutional bounds (`0%` to `70%`).
   - Dynamic preview calculates expected approval changes (`-0.5%` per `+1%` tax hike) and stability penalties (> 50%).
4. **National Projects**:
   - `StartRoadProjectButton`: Commissions the National Road Program (cost: R500,000). Deducts funds, sets the active project, and updates national stability upon completion.
   - `CancelProjectButton`: Cancels unconfirmed commissions with zero funds deducted.
5. **Standardized In-Desk Outcome Feedback**:
   - All actions display clear bracketed outcomes: `[Success]`, `[Rejected]`, `[Pending]`, and `[Cancelled]`.
6. **File-Based Decision Flow (`presidential_desk_request.json`)**:
   - Requests are written to disk as JSON:
     ```json
     {
       "request_id": "req_1729000000_12345",
       "action": "create_country",
       "country_name": "Republic of Arcadia",
       "timestamp": "2026-10-16T14:00:00"
     }
     ```
   - Supported actions: `"tax"`, `"create_country"`, `"load_save"`, `"start_project"`.
   - The CLI or bridge watcher processes the request, applies core simulation logic, writes updated `.sav`, `presidential_desk_state.json`, and `presidential_desk_result.json`, then deletes the request file.

---

## Running the Republic Simulation Backend

You can run the simulation backend to service presidential desk requests:

```bash
# Option 1: Dedicated request watcher daemon (recommended for single-player desk loop)
dotnet run --project src/Republic.Cli -- --watch-requests

# Option 2: Interactive Executive Desktop (automatically watches & processes Godot requests in background)
dotnet run --project src/Republic.Cli

# Option 3: Process a single pending request
dotnet run --project src/Republic.Cli -- --process-request

# Option 4: Create a country via CLI
dotnet run --project src/Republic.Cli -- --create-country="Republic of Arcadia"

# Option 5: Commission a project via CLI
dotnet run --project src/Republic.Cli -- --start-project="Road"

# Option 6: Standalone state export
dotnet run --project src/Republic.Cli -- --export-godot
```

---

## Running Automated Headless Checks

To run the automated Godot test suite verifying missing state safety, corrupt JSON handling, live state loading, dynamic state updates, taxation decision controls, Phase 11 consequence calculations, country creation validation, national briefing panel, project commissions, timeout warnings, and multiplayer networking (25 automated checks):

```bash
godot --headless -s test_presidential_desk.gd
```


---

## Phase 2 Multiplayer: Running Two Connected Clients (Host & Join)

Phase 2 establishes the minimal multiplayer foundation in `Republic.Core/Networking` allowing two sovereign players to connect, synchronize to a shared simulation world clock, and view each other's national state in real time.

### Multiplayer Architecture
- **Host**: A lightweight TCP listener (`RepublicHost`) running on localhost (default port `7777`). Manages the single authoritative `RepublicTime` clock and relays state between sovereign clients.
- **Client**: `StreamPeerTCP` non-blocking socket loop in Godot (`PresidentialDesk.gd`) connecting via `RepublicNetworkClient` protocol.
- **Shared World Clock**: The host periodically broadcasts ticks (`NetworkClockSnapshot`), updating the synchronized `SharedWorldClock` label on all connected desks.
- **State Synchronization**: When a sovereign state loads or updates (e.g. following a tax policy decision), a lightweight `CountrySnapshot` (`id`, `name`, `treasury`, `approval`, `tax_rate`) is automatically broadcast to connected peers.

### Exact Local Test Steps

#### Step 1 — Start the Network Host
You can start the host in either of two ways:
- **Option A (Terminal CLI Host)**:
  Run in your terminal at repository root:
  ```bash
  dotnet run --project src/Republic.Cli -- --host --port=7777
  ```
- **Option B (Directly from Godot)**:
  Click the **Host (Port 7777)** button in the Presidential Desk interface (it will automatically launch the background CLI host and connect).

#### Step 2 — Run Client 1 (Player 1 Desk)
1. Run the Presidential Desk scene in Godot (**F5** or `Play Scene`).
2. If using the CLI host, click **Join** with address `127.0.0.1:7777`.
3. Verify connection status displays:
   `Network: Connected to host`
4. Verify the **Shared World Clock** begins ticking:
   `Shared World Clock: Day 0 - 02:00 WAT (Tick 2)`

#### Step 3 — Run Client 2 (Player 2 Desk)
1. In a second terminal or Godot instance, launch the second client (optionally providing a separate state file or client identifier):
   ```bash
   # From repository root:
   godot client/godot/project.godot -- --client-id=valoria_sovereign --state-file=client/godot/presidential_desk_state_p2.json
   ```
   *(If running standard two windows on the same machine, Godot will assign distinct client IDs automatically).*
2. Ensure the address field contains `127.0.0.1:7777` and click **Join**.
3. Verify connection status on Client 2 displays:
   `Network: Connected to host`

#### Step 4 — Confirm Mutual Visibility & Synchronization
1. **On Client 1**:
   Under **Foreign Sovereign State (Connected Peer)**, verify:
   - **Foreign Country**: Displays Client 2's nation name (e.g. `Republic of Valoria` or Sovereign name).
   - **Treasury**: Displays Client 2's treasury balance.
   - **Approval**: Displays Client 2's approval rating.
2. **On Client 2**:
   Under **Foreign Sovereign State (Connected Peer)**, verify:
   - **Foreign Country**: Displays Client 1's nation name (e.g. `Republic of Arcadia`).
   - **Treasury**: Displays Client 1's treasury balance.
   - **Approval**: Displays Client 1's approval rating.
3. **On Both Desks**:
   - The **Shared World Clock** shows the exact same simulation day, time string, and tick number.
   - When Player 1 enacts a tax decision on their desk, their updated treasury and approval are immediately broadcast to Player 2's desk.

#### Step 5 — Disconnect
Click **Disconnect** on either client. The interface immediately clears foreign country metrics, resets the clock to local mode, and resumes single-player local operation.

---

## Phase 3 Diplomacy & Economy: Two-Player Test Steps (Recognition, Messaging, & Trade)

Phase 3 builds on the multiplayer foundation to provide bilateral diplomatic recognition, diplomatic message broadcasts (cables), and trade proposals with authoritative treasury adjustments between two connected players.

### Prerequisites
1. Start the host:
   ```bash
   dotnet run --project src/Republic.Cli -- --host --port=7777
   ```
2. Launch Client 1 (Player 1) and click **Join** (`127.0.0.1:7777`).
3. Launch Client 2 (Player 2) and click **Join** (`127.0.0.1:7777`).
4. Both players verify connection and peer country detection under **Foreign Sovereign State (Connected Peer)**.

---

### Step 1 — Diplomatic Recognition
1. Under **Diplomacy & Trade Relations**, verify the baseline standing displays:
   `Diplomatic Standing: Unrecognized`
2. Player 1 or Player 2 clicks **Recognize Country**.
3. **Verification on Both Desks**:
   - The standing immediately updates to: `Diplomatic Standing: Recognized`.
   - The **Recognize Country** button disables to indicate bilateral recognition is active.

---

### Step 2 — Exchange Diplomatic Cables / Messages
1. In the **MessageLineEdit** text field, Player 1 types a diplomatic dispatch:
   `"We offer peace, cooperation, and open borders."`
2. Player 1 clicks **Send Cable**.
3. **Verification on Both Desks**:
   - Under **Latest Dispatch**, both Player 1 and Player 2 see:
     `Latest Dispatch [12:00 WAT]: Player1_ID - "We offer peace, cooperation, and open borders."`
4. Player 2 can type a reply (e.g., `"Agreed. Let us discuss mutual trade."`) and click **Send Cable**. The latest dispatch immediately updates on both desks.

---

### Step 3 — Propose a Trade (Currency Transfer)
1. On Player 1's desk, locate the **Trade Controls** section.
2. In the **Trade Amount (REPU)** spinbox, select an amount (e.g. `50,000,000.0` or `50000000`).
3. Player 1 clicks **Propose Trade**.
4. **Verification**:
   - **Player 1 (Proposer Desk)**:
     - Latest trade status displays: `Trade Status: Pending proposal (R 50.0M) from Player1_ID`.
     - The **Accept Trade** and **Reject Trade** buttons remain disabled (a nation cannot accept its own trade proposal).
   - **Player 2 (Recipient Desk)**:
     - Latest trade status displays: `Trade Status: Pending proposal (R 50.0M) from Player1_ID`.
     - The **Accept Trade** and **Reject Trade** buttons become enabled.

---

### Step 4 — Accept the Trade & Verify Authoritative Treasury Updates
1. On Player 2's desk, click **Accept Trade**.
2. **Authoritative Execution**:
   - The host evaluates the transaction against `Republic.Core` treasury rules (`TryWithdraw` on Player 1 and `Deposit` on Player 2).
   - If Player 1 has sufficient liquid reserves, the funds are safely debited and credited.
3. **Verification on Both Desks**:
   - Both desks display:
     `Trade Status: Accepted - R 50.0M transferred from Player1_ID to Player2_ID`
   - Both **Accept Trade** and **Reject Trade** buttons disable.
   - **Player 1's Desk**: Treasury balance reflects the `R 50,000,000` deduction.
   - **Player 2's Desk**: Treasury balance reflects the `R 50,000,000` addition.

---

### Step 5 — Alternate Flow: Rejecting a Trade Proposal
1. Propose another trade from either desk.
2. On the recipient's desk, click **Reject Trade**.
3. **Verification on Both Desks**:
   - Latest trade status displays: `Trade Status: Proposal rejected`.
   - Neither country's treasury balance is modified.
   - Buttons are reset.

---

## Phase 4 Offline Progression & AI Governments: Offline Test Steps

In Phase 4, the world continues to progress while players are offline. AI-controlled sovereign countries run scheduled National Yield cycles and enact tax rate adjustments within constitutional bounds, saving full state (treasuries, yields, parties, bills, elections, diplomatic relations, dispatches, and trades). When the player returns and reloads, AI progression and decisions are immediately visible.

### Prerequisites
Ensure the .NET CLI and Godot client are built:
```bash
dotnet build Republic.sln
```

---

### Step 1 — Export Initial State & Save Game
Start the CLI simulation and save your active world:
```bash
dotnet run --project src/Republic.Cli -- --export-godot
```
Notice `client/godot/presidential_desk_state.json` and the authoritative save file are created. In the Godot desk (press **F5** in Godot), the player sees:
- **Country**: `Republic of Arcadia`
- **Foreign Country**: `Republic of Valoria` (or AI peer nation)
- **Last AI Decision**: `Last AI Decision: None recorded` (or initial baseline note)

---

### Step 2 — Quit and Advance the Clock (Offline Gap)
Close the Godot desk and quit the application to simulate going offline.
Advance the global simulation time past one or more six-hour National Yield cycle boundaries (00:00, 06:00, 12:00, 18:00 WAT). You can simulate an offline gap using the CLI startup flag:
```bash
dotnet run --project src/Republic.Cli -- --startup-gap=18h --export-godot
```
Or run the watcher daemon while waiting for time boundaries to pass:
```bash
dotnet run --project src/Republic.Cli -- --watch-requests
```

During this gap:
- The authoritative Republic simulation clock advances.
- At each 6-hour boundary, the AI government (`AIGovernment` in `Republic.Core`) processes a National Yield cycle, collecting tax revenue into its treasury.
- The AI evaluates domestic stability, treasury reserves, and citizen approval, selecting a constitutional income tax rate (0%–70%).
- The AI applies its decision through the same `ApplyTaxDecision` pipeline used by players, logging a decision rationale note and updating its sovereign state.

---

### Step 3 — Reload and Inspect the Presidential Desk
Relaunch or return to the Godot Presidential Desk:
1. Open Godot and press **F5** (or click **Play**).
2. The desk automatically reloads the restored authoritative state.

---

### Step 4 — Confirm AI Progression & Tax Decision
On the Presidential Desk:
1. Under **Foreign Sovereign State**, inspect the **Last AI Decision** line:
   - It clearly displays the action taken while offline, for example:
     `Last AI Decision: Republic of Valoria set tax to 26.0% (Day 1, 06:00 WAT) - Lowered income tax to stimulate public approval.`
2. Inspect the foreign country's updated **Treasury** and **Approval** ratings reflecting the completed yield cycles and tax policy changes.
3. Verify that all domestic player data (treasury, approval, tax rate, bills, parties, dispatches, and relations) was fully preserved across the save/load cycle.

---

## Phase 5 Launch Readiness: Web Export, UI Polish & Monetization Hooks

Phase 5 brings the Godot client to launch readiness for an Early Access candidate with HTML5 web export capability, executive UI polish, simulated IAP buffs, and web ad integration hooks.

### 1. HTML5 Web Export Instructions

The Godot client includes configured web export presets (`export_presets.cfg`) and a custom HTML shell (`shell.html`).

#### Option A: Command Line Interface (CLI / Headless)
Run the following command from the repository root:
```powershell
godot --headless --path client/godot --export-release "Web" "builds/web/index.html"
```
*(Replace `godot` with the path to your Godot 4 console binary, e.g. `C:\Users\...\Godot_v4.7.2-stable_win64_console.exe`).*

#### Option B: Godot Editor GUI
1. Open `client/godot` in Godot Engine 4.x.
2. In the top menu, navigate to **Project > Export...**.
3. Select the **Web** preset from the Presets list.
4. Click **Export Project...** at the bottom.
5. Ensure the export target path is set to `builds/web/index.html` and click **Save**.

#### Resulting Build Artifacts
When the export completes, the web distribution files are generated in `client/godot/builds/web/`:
- `index.html` — The main entrypoint web page container with canvas and loader.
- `index.pck` — Packed game scene resources, textures, scripts, and initial state.
- `index.png` — High-resolution application splash image.
- `index.icon.png` — Web browser favicon and application shortcut icon.
- `index.apple-touch-icon.png` — iOS home screen touch icon.

---

### 2. Presidential Desk Visual Polish

The Presidential Desk features clean, intentional 3D-adjacent styling:
- **Executive Dark Palette**: Slate-navy background (`#0B0F17`) with a solid 4px executive gold accent border (`#D9AD48`) running across the upper margin.
- **Structured Card StyleBoxes**: Flat panels with 1px slate-grey borders (`#2E3E58`), rounded corners, and interior padding.
- **High-Contrast Typography**: Clear hierarchy with executive amber/gold section headers (`#EAB308`), cyan system headers (`#60A5FA`), and legible off-white text values (`#E2E8F0`).
- **Clean Spacing**: Expanded layout bounds (`960x1450`) with consistent vertical separation between governance panels.

---

### 3. Executive Orders & National Buffs (Simulated IAP)

A dedicated Buffs panel provides simulated in-app purchases:
- **Administrative Efficiency** ($0.99 USD): +10% National Yield Processing Speed (12h duration).
- **Investor Confidence** ($1.99 USD): +5.0% Sovereign Stability & Approval Bonus (24h duration).
- **Simulated Purchase Flow**:
  - Clicking **Buy Decrees** writes a request file (`presidential_desk_buff_request.json`) adjacent to the state file with request ID, buff ID, price, and timestamp.
  - Real payment processing is bypassed in Phase 5; the UI status label updates to confirm:
    `Buff Status: Simulated purchase request written for '<Buff Name>' ($<Price> USD)`

---

### 4. Web Ad Unit Placeholder & Google AdSense Injection Guide

The Presidential Desk includes a labeled leaderboard placeholder (`AdPlaceholder`, 728x70) positioned cleanly below the Buffs panel.

When deploying the HTML5 export to production, replace or augment this placeholder with a live web ad unit such as Google AdSense:

#### Injecting Google AdSense in `shell.html` / `index.html`

1. **Add the AdSense Script Tag** in the `<head>` of `client/godot/shell.html`:
   ```html
   <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXXXXXXXX"
        crossorigin="anonymous"></script>
   ```

2. **Add the Ad Container** inside the `<body>` of `shell.html`:
   Place the ad container either in a dedicated header/footer bar outside the canvas:
   ```html
   <div id="ad-banner-container" style="width: 728px; height: 90px; margin: 10px auto; z-index: 100;">
       <!-- Republic Leaderboard Ad -->
       <ins class="adsbygoogle"
            style="display:inline-block;width:728px;height:90px"
            data-ad-client="ca-pub-XXXXXXXXXXXXXXXX"
            data-ad-slot="1234567890"></ins>
       <script>
            (adsbygoogle = window.adsbygoogle || []).push({});
       </script>
   </div>
   ```

3. **Canvas Ad Overlay via JavaScript Bridge**:
   In Godot 4 Web builds, JavaScript interop (`JavaScriptBridge.eval(...)`) can trigger ad units on demand or communicate with parent DOM elements without interrupting game execution.


