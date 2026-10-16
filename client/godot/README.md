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
