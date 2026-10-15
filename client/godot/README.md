# Republic - Godot Client

This folder contains the Godot 4 client for Republic, connected directly to the authoritative `Republic.Core` simulation state and providing interactive presidential decision capabilities.

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

## Interactive Presidential Decision: Taxation Policy (Phase 15)

The presidential desk provides interactive governance controls allowing the player to enact tax policy reforms:
1. **View Current Rate**: Displays the current personal income tax rate from live simulation state.
2. **Select Proposed Rate**: A `ProposedRateSpinBox` control bounded by constitutional limits (`0%` to `70%`).
3. **Dynamic Feedback Preview**: Automatically previews the expected public approval delta (`-0.5%` per `+1%` tax hike, `+0.5%` per `-1%` tax cut) and any stability warnings (such as the `-2.0%` penalty for exceeding `50%` income tax).
4. **Confirm or Cancel**:
   - **Confirm Decision**: Submits the decision through `Republic.Core.World.PresidentialWorkspaceHandler.AdjustTaxPolicy`. The country state updates, citizen approval changes, and the dashboard reloads with the genuine result.
   - **Cancel Decision**: Explicitly cancels the proposal. Existing tax rates, public approval, and country state are preserved 100% unchanged, and the selector resets back to the current rate.
5. **Decision Result**: Displays the verified outcome message returned by `Republic.Core`.

## Exporting Live State from Republic.Core

You can generate or refresh the live simulation state snapshot at any time:
```bash
# Option 1: Standalone CLI export
dotnet run --project src/Republic.Cli -- --export-godot

# Option 2: Running the terminal executive desk automatically syncs state on startup, time advance, and policy enactments
dotnet run --project src/Republic.Cli
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

To run the automated test suite verifying missing state safety, corrupt JSON handling, live state loading, dynamic state updates, taxation decision controls, Phase 11 consequence calculations, confirmed decision execution, cancellation preservation, and constitutional bounds rejection:
```bash
godot --headless -s test_presidential_desk.gd
```
