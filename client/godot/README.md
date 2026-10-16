# Republic - Godot Client

This folder contains the Godot 4 client for Republic, connected directly to the authoritative `Republic.Core` simulation state and providing interactive presidential decision capabilities.

---

## Novice Quick Start: Running the Presidential Desk & Enacting a Tax Decision

Follow these exact steps to run the playable presidential desk and see a tax decision update live country data in real time:

### Step 1 — Start the Simulation Watcher or Export State

In your terminal at the repository root, start the background request watcher daemon:

```bash
dotnet run --project src/Republic.Cli -- --watch-requests
```

*(Alternatively, run `dotnet run --project src/Republic.Cli -- --export-godot` once to export initial state, or use the interactive CLI with `dotnet run --project src/Republic.Cli` which also processes requests in the background).*

### Step 2 — Open the Project in Godot

1. Open **Godot Engine 4.x**.
2. Click **Import** in the Project Manager.
3. Browse to `client/godot/` and select `project.godot`.
4. Click **Import & Edit**.

### Step 3 — Run the Presidential Desk Scene

Press **F5** (or click the **Play** button in the upper-right corner of the Godot editor).

The Presidential Desk window opens with the dark executive theme, displaying live national telemetry:
- **Country**: Sovereign nation name (e.g. `Republic of Arcadia`)
- **Treasury**: Liquid balance in REPU (e.g. `R 15,000,000,000`)
- **Approval**: Citizen approval rating (e.g. `70.0%`)
- **Stability**: Baseline stability rating (e.g. `80.0%`)
- **Income Tax Rate**: Current personal income tax rate (e.g. `25.0%`)
- **System Status**: Displays connection health, state file path, last update timestamp, and request status (`Tax request: None`)

### Step 4 — Propose a New Tax Rate

1. Under **Presidential Decision: Taxation Policy**, find the **Proposed Income Tax Rate (%)** spinbox.
2. Adjust the spinbox value from `25.0%` to `30.0%`.
3. Notice the live **Expected Approval Effect** label update dynamically:
   - Increasing the tax rate displays: `Expected Approval Effect: -2.5% (Tax hike reduces household disposable income; -0.5% approval per +1% tax).`

### Step 5 — Confirm the Decision

Click the **Confirm Decision** button.
- The desk status updates to indicate `Tax request: Pending` and writes `presidential_desk_request.json`.
- The decision controls disable temporarily to prevent duplicate submissions.

### Step 6 — See the Live Update Take Effect

- **Automatic Loop**: The `.NET` watcher daemon running in your terminal detects the request, evaluates authoritative simulation rules in `Republic.Core`, applies the change, updates `presidential_desk_state.json` and `presidential_desk_result.json`, and deletes the request file.
- **Immediate Refresh**: Within 1–2 seconds, the Godot desk automatically detects the state update and displays:
  - **Income Tax Rate**: `30.0%`
  - **Approval**: Updated according to simulation rules (e.g. `67.5%`)
  - **Decision Result**: `Successfully applied - Income tax rate updated from 25.0% to 30.0%. Approval adjusted...`
  - **System Status**: Shows `Connected`, refreshed timestamp, and `Tax request: None`
- **Manual Path**: You can also click **Refresh / Apply Pending** at any time to immediately trigger result pickup and reload state.

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
  - **Treasury**: Sovereign liquid treasury balance in REPU (e.g. `R 15,000,000,000`).
  - **Approval**: Public approval rating percentage (e.g. `70.0%`).
  - **Stability**: Baseline stability rating percentage (e.g. `80.0%`).
  - **Simulation Time**: Authoritative simulation time in West Africa Time (e.g. `Day 0, 00:00 WAT`).
  - **Ruling party**: Active ruling party name or `None`.
  - **Election**: Most recent democratic national election result or `No election`.
  - **Income Tax Rate**: Current personal income tax rate percentage (e.g. `25.0%`).
  - **System Status**: Full connection status, state file presence, last update timestamp, and request telemetry.
- **Auto-Refresh**: The scene automatically detects modification timestamps on the state file every 0.5s and reloads instantly when simulation time advances or tax policies change.
- **Safe Fallback**: If the state file is missing or contains invalid JSON, the interface displays `[No State Loaded]` with a visible status message; it never silently falls back to hardcoded sample values.

---

## Interactive Presidential Decision Controls

The presidential desk provides interactive governance controls allowing the player to enact tax policy reforms:
1. **View Current Rate**: Displays the current personal income tax rate from live simulation state.
2. **Select Proposed Rate**: A `ProposedRateSpinBox` control bounded by constitutional limits (`0%` to `70%`).
3. **Dynamic Feedback Preview**: Automatically previews the expected public approval delta (`-0.5%` per `+1%` tax hike, `+0.5%` per `-1%` tax cut) and any stability warnings (such as the `-2.0%` penalty for exceeding `50%` income tax).
4. **File-Based Decision Flow (`presidential_desk_request.json`)**:
   - When the player confirms or cancels a decision, Godot writes a small JSON request file (`presidential_desk_request.json`) next to the state file:
     ```json
     {
       "request_id": "req_1729000000_12345",
       "proposed_income_tax_rate": 0.30,
       "confirmed": true,
       "timestamp": "2026-10-16T14:00:00"
     }
     ```
   - **Simulation Processing**: When the request file appears, `PresidentialDeskBridge.CreateRequestWatcher` or the CLI detects it, applies the decision using authoritative `ApplyTaxDecision` simulation rules in `Republic.Core`, updates the persistent `.sav` save file, writes `presidential_desk_state.json` and `presidential_desk_result.json`, and deletes `presidential_desk_request.json`.
   - **Cancellation**: If cancelled, the request is written with `"confirmed": false`. Existing tax rates, public approval, and simulation state are preserved 100% unchanged, and the selector resets back to the live rate.
   - **Refresh / Apply Pending**: The **Refresh / Apply Pending** button checks for pending result files, triggers backend processing if a request is waiting, and refreshes the display immediately.
   - **Timeout Warning**: If a request file is written but no updated state arrives within 4 seconds (e.g. if the simulation backend is stopped), a clear warning status message is displayed on the desk.

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

# Option 4: Standalone state export
dotnet run --project src/Republic.Cli -- --export-godot
```

---

## Running Automated Headless Checks

To run the automated Godot test suite verifying missing state safety, corrupt JSON handling, live state loading, dynamic state updates, taxation decision controls, Phase 11 consequence calculations, file-based request creation, cancellation preservation, timeout warnings, constitutional bounds rejection, and the refresh/apply flow:

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

