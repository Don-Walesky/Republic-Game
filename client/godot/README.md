# Republic - Godot Client

This folder contains the Godot 4 client for Republic, connected directly to the authoritative `Republic.Core` simulation state and providing interactive presidential decision capabilities.

> **Developer Note**: Open `client/godot/project.godot` in Godot 4 and press **F5** to run; the bottom **Status** label reports whether `presidential_desk_state.json` was found, its last update timestamp, and whether a tax decision request is pending.

## Requirements

- [Godot Engine 4.x](https://godotengine.org/download) (Standard or .NET build)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for simulation core)

## Simulation State Connection

The Presidential Desk interface reads authoritative state exported from `Republic.Core`:
- **State File**: `res://presidential_desk_state.json` (also checks `user://presidential_desk_state.json` or custom path via `--state-file=<path>` or `REPUBLIC_STATE_FILE`).
- **Live Fields Connected**:
  - **Country**: Sovereign nation name (e.g. `Republic of Arcadia`).
  - **Treasury**: Sovereign liquid treasury balance in REPU (e.g. `R 15,000,000,000`).
  - **Approval**: Public approval rating percentage (e.g. `68.5%` or `78%`).
  - **Stability**: Baseline stability rating percentage (e.g. `85%`).
  - **Simulation Time**: Authoritative simulation time in West Africa Time (e.g. `Day 0, 00:00 WAT`).
  - **Ruling party**: Active ruling party name or `None`.
  - **Election**: Most recent democratic national election result or `No election`.
  - **Income Tax Rate**: Current personal income tax rate percentage (e.g. `25%`).
  - **Status**: Live state connection telemetry and sync health.
- **Auto-Refresh**: The scene automatically detects modification timestamps on the state file every 0.5s and reloads instantly when simulation time advances or tax policies change.
- **Safe Fallback**: If the state file is missing or contains invalid JSON, the interface displays `[No State Loaded]` with a visible status message; it never silently falls back to hardcoded sample values.

## Interactive Presidential Decision: Taxation Policy

The presidential desk provides interactive governance controls allowing the player to enact tax policy reforms:
1. **View Current Rate**: Displays the current personal income tax rate from live simulation state.
2. **Select Proposed Rate**: A `ProposedRateSpinBox` control bounded by constitutional limits (`0%` to `70%`).
3. **Dynamic Feedback Preview**: Automatically previews the expected public approval delta (`-0.5%` per `+1%` tax hike, `+0.5%` per `-1%` tax cut) and any stability warnings (such as the `-2.0%` penalty for exceeding `50%` income tax).
4. **File-Based Decision Flow (No dotnet spawn)**:
   - Tax decisions do not spawn OS processes or require local .NET SDK paths at runtime, ensuring complete compatibility with HTML5 web exports, containerized runs, and cross-platform clients.
   - When the player confirms or cancels a decision, Godot writes a small JSON request file (`presidential_desk_request.json`) next to the state file:
     ```json
     {
       "proposed_rate": 0.30,
       "proposed_rate_percent": 30.0,
       "confirmed": true
     }
     ```
   - **Simulation Processing**: When the request file appears, `PresidentialDeskBridge` (or the CLI request watcher) applies the decision using authoritative `ApplyTaxDecision` simulation rules, updates the persistent `.sav` save file and exported `presidential_desk_state.json`, and deletes `presidential_desk_request.json`.
   - **Cancellation**: If cancelled, the request is written with `"confirmed": false`. Existing tax rates, public approval, and simulation state are preserved 100% unchanged, and the selector resets back to the live rate.
   - **Timeout Warning**: If a request file is written but no updated state arrives within 4 seconds (e.g. if the simulation backend is stopped), a clear warning status message is displayed on the Godot presidential desk.
   - **Live Auto-Refresh**: The desk continues polling `presidential_desk_state.json` every 0.5s, reloading seamlessly once the backend processes the decision.

## Running the Republic Simulation Backend

You can run the simulation backend to service presidential desk requests:
```bash
# Option 1: Interactive Executive Desktop (automatically watches & processes Godot requests in the background)
dotnet run --project src/Republic.Cli

# Option 2: Dedicated request watcher daemon
dotnet run --project src/Republic.Cli -- --watch-requests

# Option 3: Process a single pending request
dotnet run --project src/Republic.Cli -- --process-request

# Option 4: Standalone state export
dotnet run --project src/Republic.Cli -- --export-godot
```

## Opening the Project in Godot

1. Launch **Godot 4**.
2. In the Project Manager, click **Import** (or **Scan**).
3. Browse to this folder (`client/godot/`) and select `project.godot`.
4. Click **Import & Edit**.

## Running the Scene

- **Run Main Scene**: Press **F5** (or click the **Play** button in the upper-right corner). `PresidentialDesk.tscn` is configured as the main scene and will launch automatically.
- **Run Current Scene**: Open `PresidentialDesk.tscn` in the FileSystem dock and press **F6** (or click **Play Current Scene**).

## Running Automated Headless Checks

To run the automated test suite verifying missing state safety, corrupt JSON handling, live state loading, dynamic state updates, taxation decision controls, Phase 11 consequence calculations, file-based request creation, cancellation preservation, timeout warnings, and constitutional bounds rejection:
```bash
godot --headless -s test_presidential_desk.gd
```
