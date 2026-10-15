# ADR-0037: Godot Presidential Desk State Bridge

## Status
Accepted

## Context
In previous phases, a preliminary Godot 4 interface prototype was introduced under `client/godot/` with hard-coded demonstration values (`Republic of Walesky`, `R 1,250,000,000`, `62`, `Civic Union`, `No election`).

Phase 14 requires connecting the existing Godot presidential desk interface directly to the authoritative `Republic.Core` simulation state, replacing hard-coded values with live data while adhering to core architecture constraints:
1. Standard Godot builds (running GDScript) do not embed the .NET CLR and cannot directly link or execute C# assemblies.
2. The solution must not introduce heavy networking, multiplayer infrastructure, or a new web server/backend.
3. Treasury, approval, stability, tax policy, and simulation-time calculations must remain strictly inside `Republic.Core` and never be duplicated in GDScript.
4. If live state is missing or corrupted, the interface must visibly and safely report the condition rather than silently falling back to mock or demo data.
5. The connection must refresh automatically when the underlying authoritative state advances or mutates.

## Decision

1. **Dedicated Serializable Snapshot Contract (`PresidentialDeskSnapshot.cs`)**:
   We introduced [`PresidentialDeskSnapshot`](../../../src/Republic.Core/Client/PresidentialDeskSnapshot.cs) in `Republic.Core.Client`:
   - Encapsulates national identity (`country_name`), liquid treasury balance (`treasury_balance`, `treasury`), public approval rating (`approval_rating`, `approval`), baseline stability (`stability_rating`, `stability`), authoritative simulation time (`simulation_day`, `simulation_time`, `simulation_time_detailed`), ruling party (`ruling_party`), democratic election status (`election`), connection health (`status_message`), export timestamp (`exported_at_utc`), and engine tick (`simulation_tick`).
   - Uses `Country.GetDashboard(simulationTime)` and `Country.Treasury` directly to extract authoritative values without creating parallel game logic.

2. **File-Based State Bridge (`PresidentialDeskBridge.cs`)**:
   We introduced [`PresidentialDeskBridge`](../../../src/Republic.Core/Client/PresidentialDeskBridge.cs) in `Republic.Core.Client`:
   - Coordinates JSON serialization and disk export to `client/godot/presidential_desk_state.json`.
   - Supports upward path resolution to find the `client/godot` directory from any working directory, as well as explicit path overrides via `--state-file` and the `REPUBLIC_STATE_FILE` environment variable.
   - Provides safe export methods (`ExportState`, `TryExportState`) and deserialization (`LoadState`).
   - CLI integration: Running `dotnet run --project src/Republic.Cli -- --export-godot` exports the latest live snapshot immediately. In addition, the running CLI automatically synchronizes state on scenario startup, dashboard rendering, simulation time advancement, and tax policy enactment.

3. **Godot Client State Binding (`PresidentialDesk.gd` & `PresidentialDesk.tscn`)**:
   - Updated `client/godot/PresidentialDesk.gd`:
     - Reads the state file using `FileAccess` and `JSON.parse_string`.
     - Scans `res://presidential_desk_state.json`, `user://presidential_desk_state.json`, and environment/CLI overrides.
     - Automatically polls the file modification timestamp every `0.5s` and reloads immediately when changed.
     - Exposes a public `refresh()` function for manual or automated test execution.
     - Safe error handling: Missing files or malformed JSON update all labels to `[No State Loaded]` with a visible status message; zero hardcoded demonstration strings remain in the code or default scene.
   - Updated `client/godot/PresidentialDesk.tscn`:
     - Extended label layout to display country name, treasury, approval, stability, simulation date/time, ruling party, election, and connection status.
     - Default label values set to `[No State Loaded]`, completely eliminating legacy demo strings.

4. **Automated Testing & Verification**:
   - **.NET Test Suite**: Added [`PresidentialDeskBridgeTests.cs`](../../../tests/Republic.Core.Tests/Client/PresidentialDeskBridgeTests.cs) covering snapshot creation, JSON roundtrip fidelity, treasury/time mutation sync, tax policy approval feedback, file export/loading, corrupt JSON handling, path resolution, and election state reflection.
   - **Godot Headless Test Suite**: Added [`test_presidential_desk.gd`](../../../client/godot/test_presidential_desk.gd) executed headlessly via `godot --headless -s test_presidential_desk.gd`, validating missing state safety, corrupt JSON rejection, live state loading, and dynamic state updates.

## Consequences

- The Godot client displays genuine, live Republic simulation state with zero hardcoded demo values.
- Zero duplication of simulation logic in GDScript; all calculations remain authoritative in `Republic.Core`.
- No new external networking dependencies, ports, or background servers required.
- Both the terminal executive workspace and Godot client can run alongside each other, with Godot updating in real time as the terminal workspace advances time or enacts policies.
