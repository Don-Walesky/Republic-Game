# Republic - Godot Client

This folder contains the Godot 4 client for Republic, connected directly to the authoritative `Republic.Core` simulation state.

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
  - **Status**: Live state connection telemetry and sync health.
- **Auto-Refresh**: The scene automatically detects modification timestamps on the state file every 0.5s and reloads instantly when simulation time advances or tax policies change.
- **Safe Fallback**: If the state file is missing or contains invalid JSON, the interface displays `[No State Loaded]` with a visible status message; it never silently falls back to hardcoded sample values.

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

To run the automated test suite verifying missing state safety, corrupt JSON handling, live state loading, and dynamic state updates:
```bash
godot --headless -s test_presidential_desk.gd
```
