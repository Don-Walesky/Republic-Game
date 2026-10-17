# Republic - Operational Monitoring & Crash Reporting

This document defines logging conventions, telemetry observation, and crash diagnostics for the Republic simulation engine, CLI, and Godot client.

---

## 1. Engine & Server Diagnostics

The .NET authoritative simulation core writes structured diagnostic telemetry to stdout and file logs:

### 1.1 Logging Structure
- **Level Conventions**:
  - `[Debug]`: Internal event queue dispatching (`WorldCreatedEvent`, `MinisterAppointedEvent`).
  - `[Info]`: State transitions, country founding, cabinet appointments, news broadcasts.
  - `[Warning]`: Rejected decisions, insolvency alerts, trade timeouts, non-fatal parsing fallbacks.
  - `[Error]`: Save corruption, unhandled exceptions, socket disconnection errors.

### 1.2 Log Output Destinations
- Headless / CLI: Output directly to terminal and appended to `logs/republic_simulation.log`.
- Single-player Bridge: Records request/response transaction lifecycle in `presidential_desk_result.json`.

---

## 2. Godot Client Error & Exception Tracking

The Godot 4 Presidential Desk client tracks operational health through:
1. **Multiplayer Status Banner (`multiplayer_status_label`)**:
   - Displays real-time TCP socket state (`Connecting`, `Connected`, `Reconnected session`, `Disconnected`, `Error`).
2. **Decision Outcome Indicators (`decision_result_label`, `project_status_label`, `cabinet_status_label`)**:
   - Displays explicit `[Success]`, `[Pending]`, `[Rejected]`, or `[Cancelled]` status for all presidential actions.
3. **Engine Console Logs**:
   - Standard Godot error macros (`printerr`, `push_error`, `push_warning`) output to browser console (WebAssembly) or stdout (Desktop).
   - Unhandled exceptions trigger browser error overlays in debug mode and graceful fallback notices in release builds.

---

## 3. Production Health Metrics

For live multiplayer or community hosting:
- **Tick Liveness**: Clock tick monotonically increasing at 1 second per simulation minute.
- **Client Connection Concurrency**: Active TCP sockets connected to `RepublicHost.Port`.
- **Transaction Idempotency**: Zero double-deductions in `RepuTreasury` across duplicate requests.
- **Save Integrity**: 100% successful verification on periodic `SaveWorld()` rounds with zero unhandled I/O faults.
