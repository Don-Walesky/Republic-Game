# ADR-0038: Interactive Presidential Decision via Godot Client

## Status
Accepted

## Context
Phase 14 established a live, read-only data bridge connecting the Godot 4 Presidential Desk interface to the authoritative `Republic.Core` simulation state via structured JSON snapshots.

Phase 15 requires enabling the player to make one real presidential decision—adjusting personal income taxation policy—directly through the Godot presidential desk interface and seeing the genuine outcome reflected in the simulation state:
1. The player must be able to view the current income tax rate, select a proposed rate (within constitutional bounds 0% - 70%), preview the expected citizen approval consequence, and explicitly confirm or cancel the decision.
2. The decision must be executed by the existing domain logic in `Republic.Core` (`PresidentialWorkspaceHandler.AdjustTaxPolicy` and `Country.ApplyTaxPolicy`), applying Phase 11 citizen approval and stability consequences.
3. Cancelling the decision must leave the country state completely unchanged.
4. The interface must refresh from the authoritative simulation state rather than simulating success by modifying labels alone.
5. Do not duplicate taxation business rules in GDScript, create a second taxation engine, or introduce external networking/web backends.

## Decision

1. **Extended Snapshot Data Contract (`PresidentialDeskSnapshot.cs`)**:
   - Added `country_id`, `income_tax_rate`, `income_tax_rate_formatted`, `corporate_tax_rate`, `import_tariff_rate`, and `latest_decision_result`.
   - Populated directly from `country.TaxPolicy` and `country.LatestCitizenFeedback`.

2. **Bidirectional Decision Bridge (`PresidentialDeskBridge.cs`)**:
   - Introduced [`PresidentialActionResult`](../../../src/Republic.Core/Client/PresidentialDeskBridge.cs): A structured result contract containing `success`, `result`, `message`, `income_tax_rate`, `previous_income_tax_rate`, `approval_rating`, `previous_approval_rating`, `stability_rating`, and `previous_stability_rating`.
   - Added `ApplyTaxDecision(double proposedIncomeTaxRate, bool confirmed = true, string? stateFilePath = null)`:
     - Loads authoritative state from `presidential_desk_state.json`.
     - Submits proposed policy to `PresidentialWorkspaceHandler.AdjustTaxPolicy(country, proposedPolicy, simTime, confirmed)`.
     - If confirmed and valid, executes `country.ApplyTaxPolicy`, evaluates Phase 11 approval delta (`-0.5%` per `+1%` tax hike, `+0.5%` per `-1%` tax cut; `-2.0%` stability penalty if rate exceeds `50%`), exports updated `presidential_desk_state.json`, and records the result to `presidential_desk_result.json`.
     - If cancelled or invalid, leaves country state completely untouched and records failure/cancellation.
   - Added `EstimateTaxImpact(double currentRate, double proposedRate)` helper calculating Phase 11 approval and stability projections.

3. **Sub-process CLI Execution (`Program.cs`)**:
   - Added CLI arguments `--apply-tax=<rate>`, `--cancel-tax`, and `--state-file=<path>` to `Republic.Cli`.
   - Enables headless sub-process invocation in ~100ms via `dotnet Republic.Cli.dll --apply-tax=<rate>` without entering the interactive console menu.

4. **Godot UI Controls & Interaction (`PresidentialDesk.tscn` & `PresidentialDesk.gd`)**:
   - Added UI controls to `client/godot/PresidentialDesk.tscn`:
     - `TaxRateLabel`: Displays live income tax rate (e.g. `Income Tax Rate: 25%`).
     - `ProposedRateSpinBox`: Number input bounded to `0.0%` - `70.0%` with step `1.0%`.
     - `ExpectedConsequenceLabel`: Dynamically previews expected citizen approval delta and stability warnings on value change.
     - `ConfirmButton`: Confirms decision and executes policy change.
     - `CancelButton`: Explicitly cancels decision, preserving state unchanged and resetting the spinbox.
     - `DecisionResultLabel`: Displays authoritative outcome message returned by `Republic.Core`.
   - Updated `client/godot/PresidentialDesk.gd`:
     - `execute_tax_decision(proposed_rate_percent, confirmed)`: Handles cancellation cleanly, performs client-side bounds checking, invokes `Republic.Cli.dll` via `OS.execute`, reads `presidential_desk_result.json`, reloads live state from `presidential_desk_state.json`, and updates all UI elements.

5. **Automated Verification**:
   - **.NET Unit Tests**: Added 7 new tests in [`PresidentialDeskBridgeTests.cs`](../../../tests/Republic.Core.Tests/Client/PresidentialDeskBridgeTests.cs) (664 total tests passing across solution).
   - **Godot Headless Tests**: Extended [`test_presidential_desk.gd`](../../../client/godot/test_presidential_desk.gd) to 9 automated test suites verifying UI presence, consequence calculations, confirmed execution, cancellation preservation, and constitutional bounds rejection.

## Consequences

- The player can make an authoritative presidential taxation decision directly from Godot and immediately see updated approval, stability, and tax rates.
- No business logic is duplicated in GDScript; `Republic.Core` remains the sole authority for taxation calculations.
- Cancelling guarantees zero state mutations.
- The interface does not fake results by modifying labels in isolation; all displayed changes correspond to verified game state updates.
