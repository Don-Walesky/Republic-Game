extends SceneTree

## Headless test suite validating the Godot Presidential Desk connection to Republic simulation state
## and Phase 15 interactive presidential decisions.
func _init() -> void:
	print("==============================================================")
	print("    REPUBLIC - GODOT PRESIDENTIAL DESK AUTOMATED TEST SUITE   ")
	print("==============================================================")

	var scene_res := load("res://PresidentialDesk.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://PresidentialDesk.tscn")
		quit(1)
		return

	var desk = scene_res.instantiate()
	if desk == null:
		printerr("[FAIL]: Failed to instantiate PresidentialDesk node")
		quit(1)
		return

	root.add_child(desk)

	var all_passed := true

	# Test 1: Missing state handling and absence of demo values
	all_passed = test_missing_state_behavior(desk) and all_passed

	# Test 2: Invalid / Corrupt JSON handling
	all_passed = test_corrupted_json_handling(desk) and all_passed

	# Test 3: Live Republic state loading from exported JSON
	all_passed = test_live_simulation_state_loading(desk) and all_passed

	# Test 4: Dynamic state modification and UI refresh
	all_passed = test_dynamic_state_updates(desk) and all_passed

	# Test 5: Phase 15 interactive taxation decision UI controls
	all_passed = test_taxation_decision_ui_controls(desk) and all_passed

	# Test 6: Phase 11 expected consequence preview calculations
	all_passed = test_expected_consequence_calculations(desk) and all_passed

	# Test 7: Confirming presidential decision changes authoritative state and refreshes UI
	all_passed = test_confirmed_decision_execution(desk) and all_passed

	# Test 8: Cancelling decision preserves state without change and resets controls
	all_passed = test_cancelled_decision_preserves_state(desk) and all_passed

	# Test 9: Constitutional limits and input validation reject invalid rates
	all_passed = test_invalid_decision_validation(desk) and all_passed

	# Test 10: Missing state or save file rejects decision without creating default country
	all_passed = test_missing_state_file_decision_rejection(desk) and all_passed

	# Test 11: Corrupted state or save file rejects decision without overwriting original file
	all_passed = test_corrupt_state_file_decision_rejection(desk) and all_passed

	# Test 12: Clear status message when request file written but no state arrives within timeout
	all_passed = test_request_timeout_warning(desk) and all_passed

	# Test 13: Second load retains identical country identity and state across reloads
	all_passed = test_second_load_retains_identical_state(desk) and all_passed

	# Test 14: Repeated click prevention and button disabling while processing
	all_passed = test_repeated_clicks_and_button_disabling(desk) and all_passed

	# Test 15: Awaiting confirmation status feedback
	all_passed = test_awaiting_confirmation_feedback(desk) and all_passed

	# Test 16: Immediate decision result processing via result file
	all_passed = test_immediate_result_processing_from_result_file(desk) and all_passed

	# Test 17: Honest reporting when decision persists but state refresh fails
	all_passed = test_honest_reporting_when_state_refresh_fails(desk) and all_passed

	# Test 18: Race condition regression - state refreshes before decision result file arrives
	all_passed = test_race_condition_state_refreshes_before_result(desk) and all_passed

	# Test 19: Stale / mismatched request ID results are rejected and cannot satisfy active decision
	all_passed = test_stale_mismatched_request_id_rejected(desk) and all_passed

	# Test 20: Refresh / Apply Pending flow and button
	all_passed = test_refresh_and_apply_pending_flow(desk) and all_passed

	# Test 21: Phase 2 Multiplayer networking UI controls, shared clock sync, and peer country state
	all_passed = test_multiplayer_networking_and_country_sync(desk) and all_passed

	# Test 22: Phase 3 Diplomacy recognition, messaging, and trade proposal acceptance UI
	all_passed = test_diplomacy_and_trade_sync(desk) and all_passed

	# Test 23: Phase 4 Offline progression and Last AI decision line display
	all_passed = test_phase4_ai_decision_display(desk) and all_passed

	# Test 24: Phase 5 Monetization hooks and Ad placeholder
	all_passed = test_phase5_monetization_and_ad_hooks(desk) and all_passed

	# Test 25: Phase 2 Single-player presidential game loop (country creation, briefing, project decisions)
	all_passed = test_phase2_single_player_loop(desk) and all_passed

	# Test 26: Political campaign rally and democratic national election decision loop
	all_passed = test_political_campaign_and_election_loop(desk) and all_passed

	# Test 27: Executive cabinet appointments and competence vs loyalty trade-offs
	all_passed = test_cabinet_appointment_and_tradeoff_loop(desk) and all_passed

	# Test 28: MVP Multiplayer reliability, trade lifecycle stages, defense posture, and offline briefing
	all_passed = test_mvp_multiplayer_simulation_and_trade_lifecycle(desk) and all_passed

	desk.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 28 GODOT PRESIDENTIAL DESK CHECKS SUCCEEDED!     ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE GODOT TESTS FAILED!                      ")
		printerr("==============================================================")
		quit(1)


func test_missing_state_behavior(desk: Control) -> bool:
	print("\n--- Test 1: Missing State Safe Display & Demo Value Removal ---")
	desk.display_missing_state("Testing missing state condition")

	var labels := [
		desk.country_label.text,
		desk.treasury_label.text,
		desk.approval_label.text,
		desk.stability_label.text,
		desk.simulation_time_label.text,
		desk.ruling_party_label.text,
		desk.election_label.text,
		desk.tax_rate_label.text,
		desk.cabinet_overview_label.text,
		desk.cabinet_summary_label.text
	]

	# Verify every label displays safe [No State Loaded] fallback
	for label_text in labels:
		if not label_text.contains("[No State Loaded]"):
			printerr("[FAIL]: Expected label to contain '[No State Loaded]', but found: " + label_text)
			return false

	# Verify zero hardcoded demonstration strings remain
	for label_text in labels:
		if label_text.contains("Republic of Walesky") or label_text.contains("Civic Union") or label_text.contains("1,250,000,000"):
			printerr("[FAIL]: Hardcoded demo text detected in label: " + label_text)
			return false

	# Verify status label is visible and indicates waiting/missing, state file presence, timestamp, and request status
	if not desk.status_label.text.contains("Waiting for Republic simulation state"):
		printerr("[FAIL]: Status label does not indicate waiting state: " + desk.status_label.text)
		return false

	if not desk.status_label.text.contains("State file:") or not desk.status_label.text.contains("Last updated:") or not desk.status_label.text.contains("Tax request:"):
		printerr("[FAIL]: Status label missing improved state/file/request telemetry: " + desk.status_label.text)
		return false

	# Verify SpinBox and buttons remain usable even when state file is missing
	if not desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: ProposedRateSpinBox should remain editable when state is missing!")
		return false
	if desk.confirm_button.disabled or desk.cancel_button.disabled:
		printerr("[FAIL]: Decision buttons should remain enabled when state is missing!")
		return false

	print("  [OK]: Missing state safely handled and hardcoded demo values confirmed absent.")
	return true



func test_corrupted_json_handling(desk: Control) -> bool:
	print("\n--- Test 2: Corrupt / Invalid JSON Handling ---")
	var corrupt_path := "res://temp_corrupt_state.json"
	var file := FileAccess.open(corrupt_path, FileAccess.WRITE)
	file.store_string("{ this is: not valid: json !!!")
	file.close()

	var success: bool = desk.load_state_from_file(corrupt_path)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(corrupt_path))

	if success:
		printerr("[FAIL]: load_state_from_file should return false for invalid JSON.")
		return false

	if not desk.country_label.text.contains("[No State Loaded]"):
		printerr("[FAIL]: Expected label to reset to '[No State Loaded]', but was: " + desk.country_label.text)
		return false

	if not desk.status_label.text.contains("Error"):
		printerr("[FAIL]: Expected status label to indicate error, but was: " + desk.status_label.text)
		return false

	print("  [OK]: Corrupt JSON rejected cleanly with visible error status.")
	return true


func test_live_simulation_state_loading(desk: Control) -> bool:
	print("\n--- Test 3: Live Simulation State Loading ---")
	var live_state_path := "res://presidential_desk_state.json"

	if not FileAccess.file_exists(live_state_path):
		printerr("[FAIL]: Live state file res://presidential_desk_state.json does not exist. Run CLI export first.")
		return false

	var success: bool = desk.load_state_from_file(live_state_path)
	if not success:
		printerr("[FAIL]: Failed to load live state from " + live_state_path)
		return false

	if desk.country_label.text != "Country: Republic of Arcadia":
		printerr("[FAIL]: Country label mismatch. Got: " + desk.country_label.text)
		return false

	if desk.treasury_label.text != "Treasury: R 15,000,000,000":
		printerr("[FAIL]: Treasury label mismatch. Got: " + desk.treasury_label.text)
		return false

	if desk.approval_label.text != "Approval: 68.5%":
		printerr("[FAIL]: Approval label mismatch. Got: " + desk.approval_label.text)
		return false

	if desk.stability_label.text != "Stability: 85%":
		printerr("[FAIL]: Stability label mismatch. Got: " + desk.stability_label.text)
		return false

	if not desk.simulation_time_label.text.begins_with("Simulation Time: Day 0"):
		printerr("[FAIL]: Simulation time mismatch. Got: " + desk.simulation_time_label.text)
		return false

	if desk.tax_rate_label.text != "Income Tax Rate: 25%":
		printerr("[FAIL]: Income tax rate label mismatch. Got: " + desk.tax_rate_label.text)
		return false

	if not desk.status_label.text.begins_with("Status: Connected to live Republic simulation state"):
		printerr("[FAIL]: Status label mismatch. Got: " + desk.status_label.text)
		return false

	print("  [OK]: Live Republic simulation state accurately loaded and verified on all labels.")
	return true


func test_dynamic_state_updates(desk: Control) -> bool:
	print("\n--- Test 4: Dynamic State Update & Refresh ---")
	var update_path := "res://temp_dynamic_update.json"
	var updated_json := {
		"country_name": "Republic of Arcadia",
		"treasury": "R 19,450,000,000",
		"approval": "76.4%",
		"stability": "89%",
		"simulation_time": "Day 4, 18:00 WAT",
		"ruling_party": "Democratic Coalition",
		"election": "Day 4 (Democratic Coalition victory)",
		"income_tax_rate": 0.22,
		"income_tax_rate_formatted": "22%",
		"status_message": "Connected to live Republic simulation state (Advancement cycle credited)"
	}

	var file := FileAccess.open(update_path, FileAccess.WRITE)
	file.store_string(JSON.stringify(updated_json))
	file.close()

	var success: bool = desk.load_state_from_file(update_path)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(update_path))

	if not success:
		printerr("[FAIL]: Failed to load updated state file.")
		return false

	if desk.treasury_label.text != "Treasury: R 19,450,000,000":
		printerr("[FAIL]: Updated treasury not reflected. Got: " + desk.treasury_label.text)
		return false

	if desk.approval_label.text != "Approval: 76.4%":
		printerr("[FAIL]: Updated approval not reflected. Got: " + desk.approval_label.text)
		return false

	if desk.stability_label.text != "Stability: 89%":
		printerr("[FAIL]: Updated stability not reflected. Got: " + desk.stability_label.text)
		return false

	if desk.tax_rate_label.text != "Income Tax Rate: 22%":
		printerr("[FAIL]: Updated tax rate not reflected. Got: " + desk.tax_rate_label.text)
		return false

	print("  [OK]: Dynamic state changes verified on interface labels.")
	return true


func test_taxation_decision_ui_controls(desk: Control) -> bool:
	print("\n--- Test 5: Taxation Decision UI Controls Presence ---")
	if desk.tax_rate_label == null:
		printerr("[FAIL]: TaxRateLabel node is missing.")
		return false
	if desk.proposed_rate_spinbox == null:
		printerr("[FAIL]: ProposedRateSpinBox node is missing.")
		return false
	if desk.expected_consequence_label == null:
		printerr("[FAIL]: ExpectedConsequenceLabel node is missing.")
		return false
	if desk.confirm_button == null:
		printerr("[FAIL]: ConfirmButton node is missing.")
		return false
	if desk.cancel_button == null:
		printerr("[FAIL]: CancelButton node is missing.")
		return false
	if desk.decision_result_label == null:
		printerr("[FAIL]: DecisionResultLabel node is missing.")
		return false

	# Verify SpinBox bounds match constitutional limits (0 to 70%)
	if desk.proposed_rate_spinbox.min_value != 0.0 or desk.proposed_rate_spinbox.max_value != 70.0:
		printerr("[FAIL]: SpinBox limits do not match 0-70% range. Min: %f, Max: %f" % [desk.proposed_rate_spinbox.min_value, desk.proposed_rate_spinbox.max_value])
		return false

	# Verify missing node warning system warns once
	var missing_node: Node = desk._get_required_node("NonExistent/MissingPath")
	if missing_node != null:
		printerr("[FAIL]: Expected null for non-existent node path.")
		return false
	if not desk._warned_missing_nodes.has("NonExistent/MissingPath"):
		printerr("[FAIL]: Expected missing node path to be recorded in _warned_missing_nodes.")
		return false
	var warned_count_before: int = desk._warned_missing_nodes.size()
	var missing_node_again: Node = desk._get_required_node("NonExistent/MissingPath")
	if desk._warned_missing_nodes.size() != warned_count_before:
		printerr("[FAIL]: Missing node warning was triggered more than once for the same path!")
		return false


	print("  [OK]: All 6 decision controls correctly declared and configured within constitutional bounds.")
	return true



func test_expected_consequence_calculations(desk: Control) -> bool:
	print("\n--- Test 6: Phase 11 Expected Consequence Preview Calculations ---")
	# 1. No change
	var same_preview: String = desk.calculate_expected_consequence(25.0, 25.0)
	if not same_preview.contains("No change") or not same_preview.contains("0.0%"):
		printerr("[FAIL]: Unexpected consequence for identical rate: " + same_preview)
		return false

	# 2. Tax hike: +5% tax (25% -> 30%) -> -2.5% approval
	var hike_preview: String = desk.calculate_expected_consequence(25.0, 30.0)
	if not hike_preview.contains("-2.5%") or not hike_preview.contains("Tax hike"):
		printerr("[FAIL]: Unexpected consequence for tax hike: " + hike_preview)
		return false

	# 3. Tax cut: -5% tax (25% -> 20%) -> +2.5% approval
	var cut_preview: String = desk.calculate_expected_consequence(25.0, 20.0)
	if not cut_preview.contains("+2.5%") or not cut_preview.contains("Tax relief"):
		printerr("[FAIL]: Unexpected consequence for tax cut: " + cut_preview)
		return false

	# 4. Extreme tax threshold warning (> 50%)
	var extreme_preview: String = desk.calculate_expected_consequence(25.0, 55.0)
	if not extreme_preview.contains("WARNING") or not extreme_preview.contains("-2.0%"):
		printerr("[FAIL]: Expected extreme tax warning missing: " + extreme_preview)
		return false

	print("  [OK]: Consequence previews faithfully adhere to Phase 11 approval and stability rules.")
	return true


func test_confirmed_decision_execution(desk: Control) -> bool:
	print("\n--- Test 7: Confirmed Presidential Decision Execution & UI Refresh ---")
	var temp_path := "res://temp_decision_test_state.json"
	var temp_sav_path := "res://temp_decision_test_state.sav"
	var temp_req_path := "res://presidential_desk_request.json"

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	var sav_data := {
		"formatVersion": 1,
		"savedAt": "2027-02-10T00:00:00+01:00",
		"state": {
			"saveName": "test-session",
			"currentTick": 0,
			"saveTimestamp": "2027-02-10T00:00:00+01:00",
			"world": {
				"worldId": "11111111-1111-1111-1111-111111111111",
				"name": "Republic Sandbox",
				"createdAt": "2027-02-10T00:00:00+01:00",
				"currentTick": 0,
				"countries": [
					{
						"id": "player-country",
						"name": "Republic of Arcadia",
						"foundingDate": "2027-02-10T00:00:00+01:00",
						"happinessRating": 70.0,
						"baselineStability": 80.0,
						"taxPolicy": {
							"incomeTaxRate": 0.25,
							"corporateTaxRate": 0.20,
							"importTariffRate": 0.05
						},
						"treasury": {
							"countryId": "player-country",
							"balance": 15000000000.0,
							"transactions": []
						}
					}
				]
			}
		}
	}

	var f_sav := FileAccess.open(temp_sav_path, FileAccess.WRITE)
	f_sav.store_string(JSON.stringify(sav_data))
	f_sav.close()

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"treasury": "R 15,000,000,000",
		"treasury_balance": 15000000000,
		"approval": "70%",
		"approval_rating": 70.0,
		"stability": "80%",
		"stability_rating": 80.0,
		"simulation_day": 0,
		"simulation_time": "Day 0, 00:00 WAT",
		"ruling_party": "None",
		"election": "No election",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"corporate_tax_rate": 0.20,
		"import_tariff_rate": 0.05,
		"save_file_path": ProjectSettings.globalize_path(temp_sav_path),
		"simulation_tick": 0
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	# Load test baseline
	desk.load_state_from_file(temp_path)
	if desk.tax_rate_label.text != "Income Tax Rate: 25%":
		printerr("[FAIL]: Expected baseline tax rate 25%, got: " + desk.tax_rate_label.text)
		return false

	# Act: Enact 30% income tax rate (+5% hike -> writes request file)
	var outcome: Dictionary = desk.execute_tax_decision(30.0, true)

	if not outcome.get("success", false):
		printerr("[FAIL]: Expected decision execution to succeed, but returned: " + JSON.stringify(outcome))
		return false

	# Verify request file was written to disk
	if not FileAccess.file_exists(temp_req_path):
		printerr("[FAIL]: Expected presidential_desk_request.json to be created on disk!")
		return false

	var req_check := FileAccess.open(temp_req_path, FileAccess.READ)
	var req_parsed: Variant = JSON.parse_string(req_check.get_as_text()) if req_check else null
	if req_check:
		req_check.close()
	if req_parsed == null or not (req_parsed is Dictionary):
		printerr("[FAIL]: Request file does not contain valid JSON!")
		return false

	var req_rate: float = float(req_parsed.get("proposed_rate", 0.0))
	if abs(req_rate - 0.30) > 0.001:
		printerr("[FAIL]: Request file proposed_rate mismatch. Got: %f" % req_rate)
		return false

	var req_confirmed: bool = bool(req_parsed.get("confirmed", false))
	if not req_confirmed:
		printerr("[FAIL]: Request file confirmed flag mismatch. Expected true.")
		return false

	# Simulate simulation processing: update state file, write result file, delete request
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	initial_state["income_tax_rate"] = 0.30
	initial_state["income_tax_rate_formatted"] = "30%"
	initial_state["approval"] = "67.5%"
	initial_state["approval_rating"] = 67.5
	initial_state["latest_decision_result"] = "Tax hike enacted: Personal Income Tax increased to 30.0% (-2.5% citizen approval)."

	var f_up := FileAccess.open(temp_path, FileAccess.WRITE)
	f_up.store_string(JSON.stringify(initial_state))
	f_up.close()

	var temp_res_path := "res://presidential_desk_result.json"
	var result_data := {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Tax hike enacted: Personal Income Tax increased to 30.0% (-2.5% citizen approval).",
		"income_tax_rate": 0.30,
		"approval_rating": 67.5,
		"stability_rating": 80.0
	}
	var f_res := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_res.store_string(JSON.stringify(result_data))
	f_res.close()

	desk._process(0.1)

	# Verify UI labels refreshed from authoritative game state
	if desk.tax_rate_label.text != "Income Tax Rate: 30%":
		printerr("[FAIL]: UI Tax Rate label did not refresh to 30%. Got: " + desk.tax_rate_label.text)
		return false

	if desk.approval_label.text != "Approval: 67.5%":
		printerr("[FAIL]: UI Approval label did not reflect -2.5% penalty. Got: " + desk.approval_label.text)
		return false

	if not desk.decision_result_label.text.contains("Tax hike enacted"):
		printerr("[FAIL]: DecisionResultLabel missing reaction summary: " + desk.decision_result_label.text)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_sav_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	print("  [OK]: Confirmed decision successfully modified authoritative country state and refreshed Godot UI.")
	return true


func test_cancelled_decision_preserves_state(desk: Control) -> bool:
	print("\n--- Test 8: Cancelled Decision Preserves State & Resets Controls ---")
	var temp_path := "res://temp_cancel_test_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"treasury": "R 15,000,000,000",
		"treasury_balance": 15000000000,
		"approval": "72%",
		"approval_rating": 72.0,
		"stability": "85%",
		"stability_rating": 85.0,
		"simulation_day": 1,
		"simulation_time": "Day 1, 00:00 WAT",
		"ruling_party": "None",
		"election": "No election",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"corporate_tax_rate": 0.20,
		"import_tariff_rate": 0.05,
		"simulation_tick": 0
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Propose 45% rate, but cancel
	var outcome: Dictionary = desk.execute_tax_decision(45.0, false)

	if outcome.get("success", true):
		printerr("[FAIL]: Cancelled decision should have success=false.")
		return false

	if not desk.decision_result_label.text.contains("cancelled"):
		printerr("[FAIL]: Result label should indicate cancellation: " + desk.decision_result_label.text)
		return false

	# Verify cancellation request file was written
	if not FileAccess.file_exists(temp_req_path):
		printerr("[FAIL]: Expected cancellation request file to be created!")
		return false

	var req_check := FileAccess.open(temp_req_path, FileAccess.READ)
	var req_parsed: Variant = JSON.parse_string(req_check.get_as_text()) if req_check else null
	if req_check:
		req_check.close()
	if req_parsed == null or not (req_parsed is Dictionary):
		printerr("[FAIL]: Cancellation request file is not valid JSON!")
		return false

	if bool(req_parsed.get("confirmed", true)) != false:
		printerr("[FAIL]: Expected confirmed=false in cancellation request file!")
		return false

	# Verify state remains completely intact
	if desk.tax_rate_label.text != "Income Tax Rate: 25%":
		printerr("[FAIL]: Tax rate was mutated despite cancellation! Got: " + desk.tax_rate_label.text)
		return false

	if desk.approval_label.text != "Approval: 72%":
		printerr("[FAIL]: Approval was mutated despite cancellation! Got: " + desk.approval_label.text)
		return false

	if desk.proposed_rate_spinbox.value != 25.0:
		printerr("[FAIL]: ProposedRateSpinBox was not reset to current rate (25.0). Got: %f" % desk.proposed_rate_spinbox.value)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	print("  [OK]: Cancelling decision preserved country state completely unchanged, wrote cancellation request, and reset selector.")
	return true


func test_invalid_decision_validation(desk: Control) -> bool:
	print("\n--- Test 9: Constitutional Bounds Rejection ---")
	var temp_path := "res://temp_validation_test_state.json"
	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"treasury": "R 15,000,000,000",
		"treasury_balance": 15000000000,
		"approval": "65%",
		"approval_rating": 65.0,
		"stability": "80%",
		"stability_rating": 80.0,
		"simulation_day": 0,
		"simulation_time": "Day 0, 00:00 WAT",
		"ruling_party": "None",
		"election": "No election",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"corporate_tax_rate": 0.20,
		"import_tariff_rate": 0.05,
		"simulation_tick": 0
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Propose 80% (violates 70% constitutional maximum)
	var outcome: Dictionary = desk.execute_tax_decision(80.0, true)

	if outcome.get("success", true):
		printerr("[FAIL]: Invalid rate should not succeed.")
		return false

	if not desk.decision_result_label.text.contains("constitutional bounds") and not desk.decision_result_label.text.contains("rejected"):
		printerr("[FAIL]: Result label should indicate rejection: " + desk.decision_result_label.text)
		return false

	if desk.tax_rate_label.text != "Income Tax Rate: 25%":
		printerr("[FAIL]: Tax rate was illegally mutated on rejected input: " + desk.tax_rate_label.text)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	print("  [OK]: Unconstitutional tax rate rejected cleanly; zero state changes enacted.")
	return true


func test_missing_state_file_decision_rejection(desk: Control) -> bool:
	print("\n--- Test 10: Missing State File Decision Rejection ---")
	var non_existent_path := "res://non_existent_decision_test_state.json"
	var non_existent_sav := "res://non_existent_decision_test_state.sav"

	# Ensure files do not exist
	DirAccess.remove_absolute(ProjectSettings.globalize_path(non_existent_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(non_existent_sav))

	# Attempt to load and execute decision against missing state
	desk.load_state_from_file(non_existent_path)
	var outcome: Dictionary = desk.execute_tax_decision(35.0, true)

	if outcome.get("success", false):
		printerr("[FAIL]: Missing state file must reject decision execution with success=false.")
		return false

	if outcome.get("result", "") != "StateMissingOrCorrupt":
		printerr("[FAIL]: Expected result 'StateMissingOrCorrupt', got: " + str(outcome.get("result", "")))
		return false

	# Verify no new files were created and no sample country was founded
	if FileAccess.file_exists(non_existent_path) or FileAccess.file_exists(non_existent_sav):
		printerr("[FAIL]: Missing state file caused new files to be illegally created!")
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path("res://presidential_desk_result.json"))
	print("  [OK]: Missing state file correctly rejected without creating default sample country.")
	return true


func test_corrupt_state_file_decision_rejection(desk: Control) -> bool:
	print("\n--- Test 11: Corrupt State File Decision Rejection ---")
	var corrupt_path := "res://temp_corrupt_decision_state.json"
	var original_corrupt_text := "{ invalid: corrupted: content !!!"

	var f := FileAccess.open(corrupt_path, FileAccess.WRITE)
	f.store_string(original_corrupt_text)
	f.close()

	desk.load_state_from_file(corrupt_path)
	var outcome: Dictionary = desk.execute_tax_decision(35.0, true)

	if outcome.get("success", false):
		printerr("[FAIL]: Corrupt state file must reject decision execution.")
		return false

	if outcome.get("result", "") != "StateMissingOrCorrupt":
		printerr("[FAIL]: Expected result 'StateMissingOrCorrupt', got: " + str(outcome.get("result", "")))
		return false

	# Verify corrupt file was not overwritten
	var check_file := FileAccess.open(corrupt_path, FileAccess.READ)
	var current_content := check_file.get_as_text() if check_file else ""
	if check_file:
		check_file.close()

	DirAccess.remove_absolute(ProjectSettings.globalize_path(corrupt_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path("res://presidential_desk_result.json"))

	if current_content != original_corrupt_text:
		printerr("[FAIL]: Corrupted file was overwritten or modified during failed decision!")
		return false

	print("  [OK]: Corrupt state file rejected cleanly without overwriting the original file.")
	return true


func test_request_timeout_warning(desk: Control) -> bool:
	print("\n--- Test 12: Request Timeout Warning When Simulation Not Running ---")
	var temp_path := "res://temp_timeout_test_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"treasury": "R 15,000,000,000",
		"approval": "70%",
		"stability": "80%",
		"simulation_time": "Day 0, 00:00 WAT",
		"income_tax_rate": 0.25
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Execute decision
	desk.execute_tax_decision(30.0, true)
	if not desk._is_waiting_for_decision:
		printerr("[FAIL]: Expected _is_waiting_for_decision to be true after submitting decision.")
		return false

	# Simulate 4.5 seconds passing without state update (REQUEST_TIMEOUT_SECONDS is 4.0)
	desk._process(4.5)

	# Verify clear warning status message
	if not desk.status_label.text.contains("Warning") or not desk.status_label.text.contains("within 4s"):
		printerr("[FAIL]: Status label did not display timeout warning! Got: " + desk.status_label.text)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	print("  [OK]: Clear status warning displayed when request is not serviced within timeout.")
	return true


func test_second_load_retains_identical_state(desk: Control) -> bool:
	print("\n--- Test 13: Second Load Retains Identical Country Identity and State ---")
	var temp_path := "res://temp_second_load_state.json"
	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"treasury": "R 18,500,000,000",
		"treasury_balance": 18500000000,
		"approval": "74.5%",
		"approval_rating": 74.5,
		"stability": "86%",
		"stability_rating": 86.0,
		"simulation_day": 5,
		"simulation_time": "Day 5, 12:00 WAT",
		"ruling_party": "Democratic Union",
		"election": "Day 4 (Democratic Union victory)",
		"income_tax_rate": 0.28,
		"income_tax_rate_formatted": "28%",
		"status_message": "Connected to live Republic simulation state"
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	# First load
	var first_success: bool = desk.load_state_from_file(temp_path)
	if not first_success:
		printerr("[FAIL]: First load failed.")
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
		return false

	var first_country: String = str(desk.country_label.text)
	var first_treasury: String = str(desk.treasury_label.text)
	var first_approval: String = str(desk.approval_label.text)
	var first_stability: String = str(desk.stability_label.text)
	var first_tax: String = str(desk.tax_rate_label.text)

	# Second load
	var second_success: bool = desk.load_state_from_file(temp_path)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))

	if not second_success:
		printerr("[FAIL]: Second load failed.")
		return false

	if desk.country_label.text != first_country:
		printerr("[FAIL]: Country label mismatch on second load.")
		return false
	if desk.treasury_label.text != first_treasury:
		printerr("[FAIL]: Treasury label mismatch on second load.")
		return false
	if desk.approval_label.text != first_approval:
		printerr("[FAIL]: Approval label mismatch on second load.")
		return false
	if desk.stability_label.text != first_stability:
		printerr("[FAIL]: Stability label mismatch on second load.")
		return false
	if desk.tax_rate_label.text != first_tax:
		printerr("[FAIL]: Tax rate label mismatch on second load.")
		return false

	print("  [OK]: Second load preserved exact country identity and state across reloads.")
	return true


func test_repeated_clicks_and_button_disabling(desk: Control) -> bool:
	print("\n--- Test 14: Repeated Clicks Prevention and Button Disabling ---")
	var temp_path := "res://temp_repeat_test_state.json"
	var temp_sav_path := "res://temp_repeat_test_state.sav"
	var temp_req_path := "res://presidential_desk_request.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"treasury": "R 15,000,000,000",
		"approval": "70%",
		"approval_rating": 70.0,
		"stability": "80%",
		"stability_rating": 80.0,
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"simulation_time": "Day 0, 00:00 WAT",
		"simulation_tick": 0
	}

	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# First submit
	var first_call: Dictionary = desk.execute_tax_decision(30.0, true)
	if not first_call.get("success", false):
		printerr("[FAIL]: First decision submit should succeed.")
		return false

	# Verify buttons are disabled while waiting
	if not desk.confirm_button.disabled or not desk.cancel_button.disabled or desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Interactive controls were not disabled during pending decision!")
		return false

	# Attempt second click while waiting
	var second_call: Dictionary = desk.execute_tax_decision(35.0, true)
	if second_call.get("success", true) or second_call.get("result", "") != "Pending":
		printerr("[FAIL]: Repeated decision click should be rejected with result='Pending'!")
		return false

	# Verify that an intermediate state refresh does NOT prematurely re-enable controls or clear waiting state
	desk.apply_state(initial_state)
	if not desk._is_waiting_for_decision or not desk.confirm_button.disabled or not desk.cancel_button.disabled or desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: State refresh should NOT clear waiting state or re-enable controls before decision result arrives!")
		return false

	# Simulate completion via decision result
	desk._handle_decision_result({
		"success": true,
		"result": "Success",
		"message": "Enacted."
	})
	if desk.confirm_button.disabled or desk.cancel_button.disabled or not desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Interactive controls were not re-enabled after decision result completed!")
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_sav_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	print("  [OK]: Controls disabled during pending request and repeated clicks prevented.")
	return true


func test_awaiting_confirmation_feedback(desk: Control) -> bool:
	print("\n--- Test 15: Awaiting Confirmation Status Feedback ---")
	var temp_path := "res://temp_confirm_feedback_state.json"
	var initial_state := {
		"country_name": "Republic of Arcadia",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%"
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Change proposed rate to 35%
	desk._on_proposed_rate_changed(35.0)
	if not desk.decision_result_label.text.contains("Awaiting player confirmation") or not desk.decision_result_label.text.contains("35.0%"):
		printerr("[FAIL]: Expected awaiting confirmation status text, got: " + desk.decision_result_label.text)
		return false

	# Change proposed rate back to current rate 25%
	desk._on_proposed_rate_changed(25.0)
	if not desk.decision_result_label.text.contains("No rate change proposed"):
		printerr("[FAIL]: Expected no change status text, got: " + desk.decision_result_label.text)
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	print("  [OK]: Interactive preview and awaiting confirmation status correctly displayed.")
	return true


func test_immediate_result_processing_from_result_file(desk: Control) -> bool:
	print("\n--- Test 16: Immediate Decision Result Processing via Result File ---")
	var temp_path := "res://temp_immediate_test_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	var temp_res_path := "res://presidential_desk_result.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"country_id": "player-country",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"approval": "70%",
		"stability": "80%"
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	desk.execute_tax_decision(30.0, true)
	if not desk._is_waiting_for_decision:
		printerr("[FAIL]: Expected waiting for decision to be true.")
		return false

	# Update state file to reflect 30%
	initial_state["income_tax_rate"] = 0.30
	initial_state["income_tax_rate_formatted"] = "30%"
	var f_up := FileAccess.open(temp_path, FileAccess.WRITE)
	f_up.store_string(JSON.stringify(initial_state))
	f_up.close()

	# Create presidential_desk_result.json
	var result_data := {
		"success": true,
		"result": "Success",
		"message": "Personal income tax rate adjusted to 30.0%.",
		"income_tax_rate": 0.30,
		"approval_rating": 67.5,
		"stability_rating": 80.0
	}
	var f_res := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_res.store_string(JSON.stringify(result_data))
	f_res.close()

	# Process delta 0.1s (far below 4s timeout)
	desk._process(0.1)

	# Verify it immediately detected the result and updated
	if desk._is_waiting_for_decision:
		printerr("[FAIL]: Expected _is_waiting_for_decision to be false after result file arrival!")
		return false

	if desk.confirm_button.disabled:
		printerr("[FAIL]: Confirm button should be re-enabled after decision completed!")
		return false

	if not desk.decision_result_label.text.contains("Successfully applied"):
		printerr("[FAIL]: Decision result label did not report success! Got: " + desk.decision_result_label.text)
		return false

	if desk.tax_rate_label.text != "Income Tax Rate: 30%":
		printerr("[FAIL]: Display tax rate label did not refresh! Got: " + desk.tax_rate_label.text)
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))
	print("  [OK]: Result file immediately processed without waiting for request timeout.")
	return true


func test_honest_reporting_when_state_refresh_fails(desk: Control) -> bool:
	print("\n--- Test 17: Honest Reporting When Decision Persists but State Refresh Fails ---")
	var temp_path := "res://temp_fail_refresh_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	var temp_res_path := "res://presidential_desk_result.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"income_tax_rate": 0.25
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)
	desk.execute_tax_decision(30.0, true)

	# Remove state file so reload fails
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))

	# Write result file with success
	var result_data := {
		"success": true,
		"result": "Success",
		"message": "Enacted in authoritative save.",
		"income_tax_rate": 0.30
	}
	var f_res := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_res.store_string(JSON.stringify(result_data))
	f_res.close()

	desk._process(0.1)

	# Verify honest reporting
	if not desk.decision_result_label.text.contains("Authoritative decision saved, but refreshing the display state file failed"):
		printerr("[FAIL]: Expected honest reporting of refresh failure, got: " + desk.decision_result_label.text)
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))
	print("  [OK]: Distinction honestly reported when decision persists but display refresh fails.")
	return true


func test_race_condition_state_refreshes_before_result(desk: Control) -> bool:
	print("\n--- Test 18: Race Condition: State Refreshes Before Result Arrives ---")
	var temp_path := "res://temp_race_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	var temp_res_path := "res://presidential_desk_result.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"approval": "70%",
		"approval_rating": 70.0,
		"stability": "80%",
		"stability_rating": 80.0
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)
	if desk.tax_rate_label.text != "Income Tax Rate: 25%":
		printerr("[FAIL]: Expected initial tax rate 25%, got: " + desk.tax_rate_label.text)
		return false

	# Act: Submit decision (30% tax)
	var outcome: Dictionary = desk.execute_tax_decision(30.0, true)
	if not outcome.get("success", false):
		printerr("[FAIL]: execute_tax_decision failed: " + JSON.stringify(outcome))
		return false

	var active_req_id: String = desk._active_request_id
	if active_req_id.is_empty():
		printerr("[FAIL]: Expected _active_request_id to be populated upon submitting decision!")
		return false

	# Verify desk is waiting and controls are disabled
	if not desk._is_waiting_for_decision:
		printerr("[FAIL]: Expected desk to be waiting for decision!")
		return false
	if not desk.confirm_button.disabled or not desk.cancel_button.disabled or desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Decision controls should be disabled while awaiting result!")
		return false

	# SIMULATE RACE: State file refreshes on disk BEFORE result file arrives!
	# For example, background simulation loop wrote updated state file
	initial_state["income_tax_rate"] = 0.30
	initial_state["income_tax_rate_formatted"] = "30%"
	initial_state["approval"] = "67.5%"
	initial_state["approval_rating"] = 67.5
	var f2 := FileAccess.open(temp_path, FileAccess.WRITE)
	f2.store_string(JSON.stringify(initial_state))
	f2.close()

	# Process frame (which reads state update)
	desk._process(0.1)

	# CRITICAL ASSERTION: The state update must NOT reset _is_waiting_for_decision
	# or re-enable the controls prematurely before the result file is processed!
	if not desk._is_waiting_for_decision:
		printerr("[FAIL]: Race condition regression! State refresh prematurely cleared _is_waiting_for_decision!")
		return false
	if not desk.confirm_button.disabled or not desk.cancel_button.disabled or desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Race condition regression! Controls re-enabled prematurely on state refresh while awaiting result!")
		return false
	if desk._active_request_id != active_req_id:
		printerr("[FAIL]: State refresh cleared or corrupted _active_request_id!")
		return false

	# Now simulation writes the result file
	var result_data := {
		"request_id": active_req_id,
		"success": true,
		"result": "Success",
		"message": "Enacted in authoritative save.",
		"income_tax_rate": 0.30
	}
	var f_res := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_res.store_string(JSON.stringify(result_data))
	f_res.close()

	# Next frame processes the result file
	desk._process(0.1)

	# Now waiting must be false, controls re-enabled, and result displayed
	if desk._is_waiting_for_decision:
		printerr("[FAIL]: Expected _is_waiting_for_decision to be false after result file processed!")
		return false
	if desk.confirm_button.disabled or desk.cancel_button.disabled or not desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Controls should be re-enabled after decision completed!")
		return false
	if not desk.decision_result_label.text.contains("Successfully applied"):
		printerr("[FAIL]: Result label should show success! Got: " + desk.decision_result_label.text)
		return false
	if desk.tax_rate_label.text != "Income Tax Rate: 30%":
		printerr("[FAIL]: Tax rate label should be 30%! Got: " + desk.tax_rate_label.text)
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))
	print("  [OK]: Race condition resolved: state file refresh does not discard pending decision result.")
	return true


func test_stale_mismatched_request_id_rejected(desk: Control) -> bool:
	print("\n--- Test 19: Stale / Mismatched Request ID Result Rejected ---")
	var temp_path := "res://temp_mismatch_state.json"
	var temp_req_path := "res://presidential_desk_request.json"
	var temp_res_path := "res://presidential_desk_result.json"
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"approval": "70%",
		"approval_rating": 70.0
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Submit decision
	desk.execute_tax_decision(28.0, true)
	var active_id: String = desk._active_request_id

	# Write a stale result file from a PREVIOUS decision with a different request_id
	var stale_result := {
		"request_id": "req_old_stale_12345",
		"success": true,
		"result": "Success",
		"message": "Old result from previous session",
		"income_tax_rate": 0.22
	}
	var f_stale := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_stale.store_string(JSON.stringify(stale_result))
	f_stale.close()

	# Process frame
	desk._process(0.1)

	# Assert stale result was rejected/ignored
	if not desk._is_waiting_for_decision:
		printerr("[FAIL]: Stale result prematurely cleared _is_waiting_for_decision!")
		return false
	if desk.decision_result_label.text.contains("Old result"):
		printerr("[FAIL]: Stale result message was applied to UI!")
		return false

	# Now write matching result
	var matching_result := {
		"request_id": active_id,
		"success": true,
		"result": "Success",
		"message": "Enacted correctly for active request",
		"income_tax_rate": 0.28
	}
	var f_match := FileAccess.open(temp_res_path, FileAccess.WRITE)
	f_match.store_string(JSON.stringify(matching_result))
	f_match.close()

	desk._process(0.1)

	if desk._is_waiting_for_decision:
		printerr("[FAIL]: Matching result failed to be processed!")
		return false
	if not desk.decision_result_label.text.contains("Successfully applied"):
		printerr("[FAIL]: Result label did not reflect matching result success!")
		return false

	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))
	print("  [OK]: Mismatched request ID results are rejected without satisfying active decision.")
	return true


func test_refresh_and_apply_pending_flow(desk: Control) -> bool:
	print("\n--- Test 20: Refresh / Apply Pending Flow & Button ---")
	if desk.refresh_button == null:
		printerr("[FAIL]: RefreshButton node is missing on PresidentialDesk.")
		return false

	if desk.refresh_button.text != "Refresh / Apply Pending":
		printerr("[FAIL]: RefreshButton text unexpected: " + desk.refresh_button.text)
		return false

	var temp_path := "res://temp_refresh_test_state.json"
	var temp_res_path := "res://presidential_desk_result.json"

	var initial_state := {
		"country_name": "Republic of Arcadia",
		"treasury": "R 15,000,000,000",
		"approval": "70%",
		"stability": "80%",
		"simulation_time": "Day 0, 00:00 WAT",
		"income_tax_rate": 0.25
	}
	var f := FileAccess.open(temp_path, FileAccess.WRITE)
	f.store_string(JSON.stringify(initial_state))
	f.close()

	desk.load_state_from_file(temp_path)

	# Write result file representing a processed pending decision
	var pending_result := {
		"request_id": "req_refresh_test",
		"success": true,
		"result": "Success",
		"message": "Tax rate updated to 32.0% via manual refresh flow",
		"income_tax_rate": 0.32
	}
	var rf := FileAccess.open(temp_res_path, FileAccess.WRITE)
	rf.store_string(JSON.stringify(pending_result))
	rf.close()

	# Also update state file to reflect the 32% rate
	initial_state["income_tax_rate"] = 0.32
	var uf := FileAccess.open(temp_path, FileAccess.WRITE)
	uf.store_string(JSON.stringify(initial_state))
	uf.close()

	# Call apply_pending_or_refresh()
	var refresh_ok: bool = desk.apply_pending_or_refresh()
	if not refresh_ok:
		printerr("[FAIL]: apply_pending_or_refresh() returned false!")
		return false

	if not desk.decision_result_label.text.contains("Tax rate updated to 32.0%"):
		printerr("[FAIL]: Decision result label did not update via apply_pending_or_refresh: " + desk.decision_result_label.text)
		return false

	if not desk.tax_rate_label.text.contains("32.0%"):
		printerr("[FAIL]: TaxRateLabel was not updated: " + desk.tax_rate_label.text)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_res_path))

	print("  [OK]: RefreshButton and apply_pending_or_refresh() successfully verified.")
	return true


func test_multiplayer_networking_and_country_sync(desk: Control) -> bool:
	print("\n--- Test 21: Phase 2 Multiplayer Networking UI & Country State Sync ---")

	# 1. Verify presence and types of multiplayer UI nodes
	if desk.host_button == null:
		printerr("[FAIL]: HostButton not found or bound on PresidentialDesk.")
		return false
	if desk.join_button == null:
		printerr("[FAIL]: JoinButton not found or bound on PresidentialDesk.")
		return false
	if desk.address_line_edit == null:
		printerr("[FAIL]: AddressLineEdit not found or bound on PresidentialDesk.")
		return false
	if desk.disconnect_button == null:
		printerr("[FAIL]: DisconnectButton not found or bound on PresidentialDesk.")
		return false
	if desk.network_status_label == null:
		printerr("[FAIL]: NetworkStatusLabel not found or bound on PresidentialDesk.")
		return false
	if desk.foreign_country_name_label == null:
		printerr("[FAIL]: ForeignCountryNameLabel not found or bound on PresidentialDesk.")
		return false
	if desk.foreign_country_treasury_label == null:
		printerr("[FAIL]: ForeignCountryTreasuryLabel not found or bound on PresidentialDesk.")
		return false
	if desk.foreign_country_approval_label == null:
		printerr("[FAIL]: ForeignCountryApprovalLabel not found or bound on PresidentialDesk.")
		return false
	if desk.shared_clock_label == null:
		printerr("[FAIL]: SharedClockLabel not found or bound on PresidentialDesk.")
		return false

	# 2. Test shared clock sync handling
	var clock_msg := {
		"type": "clock",
		"sender_id": "host",
		"clock": {
			"tick": 120,
			"day": 5,
			"time_string": "14:00 WAT",
			"is_paused": false
		}
	}
	desk._handle_network_message(clock_msg)
	if not desk.shared_clock_label.text.contains("Day 5 - 14:00 WAT (Tick 120)"):
		printerr("[FAIL]: SharedClockLabel failed to update with network clock sync: " + desk.shared_clock_label.text)
		return false

	# 3. Test peer country state snapshot handling
	var peer_msg := {
		"type": "country",
		"sender_id": "peer_player_2",
		"country": {
			"id": "peer_player_2",
			"name": "Republic of Valoria",
			"treasury": "R 42,000,000,000",
			"approval": "68.5%",
			"tax_rate": 0.20
		}
	}
	desk._handle_network_message(peer_msg)
	if not desk.foreign_country_name_label.text.contains("Republic of Valoria"):
		printerr("[FAIL]: ForeignCountryNameLabel did not display peer country name: " + desk.foreign_country_name_label.text)
		return false
	if not desk.foreign_country_treasury_label.text.contains("R 42,000,000,000"):
		printerr("[FAIL]: ForeignCountryTreasuryLabel did not display peer treasury: " + desk.foreign_country_treasury_label.text)
		return false
	if not desk.foreign_country_approval_label.text.contains("68.5%"):
		printerr("[FAIL]: ForeignCountryApprovalLabel did not display peer approval: " + desk.foreign_country_approval_label.text)
		return false

	# 4. Test self reflection rejection (should not overwrite peer with own ID)
	var self_msg := {
		"type": "country",
		"sender_id": desk._local_client_id,
		"country": {
			"id": desk._local_client_id,
			"name": "Self Sovereign",
			"treasury": "$100",
			"approval": "10%"
		}
	}
	desk._handle_network_message(self_msg)
	if desk.foreign_country_name_label.text.contains("Self Sovereign"):
		printerr("[FAIL]: Foreign country label should not display local client's own reflection!")
	# 4b. Test connected status display with authoritative country ownership
	desk._is_network_connected = true
	desk._latest_state = {"country_name": "Republic of Arcadia"}
	desk._update_network_status_display()
	if not desk.network_status_label.text.to_lower().contains("connected as owner of [republic of arcadia]"):
		printerr("[FAIL]: NetworkStatusLabel should contain 'connected as owner of [Republic of Arcadia]'! Got: " + desk.network_status_label.text)
		return false

	# 5. Test disconnect reset behavior
	desk.disconnect_network()
	if not desk.foreign_country_name_label.text.contains("[None Connected]"):
		printerr("[FAIL]: ForeignCountryNameLabel did not reset after disconnect: " + desk.foreign_country_name_label.text)
		return false
	if not desk.shared_clock_label.text.contains("[Local Clock]"):
		printerr("[FAIL]: SharedClockLabel did not reset after disconnect: " + desk.shared_clock_label.text)
		return false
	if not desk.network_status_label.text.contains("Disconnected"):
		printerr("[FAIL]: NetworkStatusLabel did not reflect disconnected state: " + desk.network_status_label.text)
		return false

	print("  [OK]: Phase 2 multiplayer UI controls, shared clock sync, and peer country display verified.")
	return true


## Test 22: Phase 3 Diplomacy recognition, messaging, and trade proposal acceptance UI
func test_diplomacy_and_trade_sync(desk: Node) -> bool:
	print("[RUN]: Test 22 - Phase 3 Diplomacy recognition, messaging, and trade proposal acceptance UI...")

	desk._init_controls()

	# 1. Verify existence and binding of all Phase 3 diplomacy and trade nodes
	if desk.diplomacy_status_label == null:
		printerr("[FAIL]: DiplomacyStatusLabel not found or bound on PresidentialDesk.")
		return false
	if desk.recognize_button == null:
		printerr("[FAIL]: RecognizeButton not found or bound on PresidentialDesk.")
		return false
	if desk.message_line_edit == null:
		printerr("[FAIL]: MessageLineEdit not found or bound on PresidentialDesk.")
		return false
	if desk.send_message_button == null:
		printerr("[FAIL]: SendMessageButton not found or bound on PresidentialDesk.")
		return false
	if desk.latest_message_label == null:
		printerr("[FAIL]: LatestMessageLabel not found or bound on PresidentialDesk.")
		return false
	if desk.trade_amount_spinbox == null:
		printerr("[FAIL]: TradeAmountSpinBox not found or bound on PresidentialDesk.")
		return false
	if desk.propose_trade_button == null:
		printerr("[FAIL]: ProposeTradeButton not found or bound on PresidentialDesk.")
		return false
	if desk.accept_trade_button == null:
		printerr("[FAIL]: AcceptTradeButton not found or bound on PresidentialDesk.")
		return false
	if desk.reject_trade_button == null:
		printerr("[FAIL]: RejectTradeButton not found or bound on PresidentialDesk.")
		return false
	if desk.latest_trade_status_label == null:
		printerr("[FAIL]: LatestTradeStatusLabel not found or bound on PresidentialDesk.")
		return false

	# 2. Test diplomacy recognition update via network sync
	var dip_msg := {
		"type": "diplomacy",
		"sender_id": "host",
		"diplomacy": {
			"from_country_id": "peer_player_2",
			"to_country_id": desk._local_client_id,
			"status": "Recognized",
			"is_recognized": true
		}
	}
	desk._handle_network_message(dip_msg)
	if not desk.diplomacy_status_label.text.contains("Recognized"):
		printerr("[FAIL]: DiplomacyStatusLabel did not reflect recognized status: " + desk.diplomacy_status_label.text)
		return false
	if not desk._is_recognized:
		printerr("[FAIL]: _is_recognized flag not set to true after recognition message.")
		return false
	if not desk.recognize_button.disabled:
		printerr("[FAIL]: RecognizeButton should be disabled after recognition is established.")
		return false

	# 3. Test diplomatic message reception and display
	var chat_msg := {
		"type": "message",
		"sender_id": "peer_player_2",
		"message": {
			"from": "Valoria Sovereign",
			"to": desk._local_client_id,
			"text": "Greetings from across the border.",
			"timestamp": "14:30 WAT"
		}
	}
	desk._handle_network_message(chat_msg)
	if not desk.latest_message_label.text.contains("Greetings from across the border."):
		printerr("[FAIL]: LatestMessageLabel did not display incoming message text: " + desk.latest_message_label.text)
		return false
	if not desk.latest_message_label.text.contains("Valoria Sovereign"):
		printerr("[FAIL]: LatestMessageLabel did not display sender name: " + desk.latest_message_label.text)
		return false

	# 4. Test trade proposal from peer to local client (recipient perspective)
	var trade_prop_msg := {
		"type": "trade",
		"sender_id": "peer_player_2",
		"trade": {
			"id": "prop-101",
			"from": "peer_player_2",
			"to": desk._local_client_id,
			"amount": 50000000.0,
			"resource": "Currency",
			"status": "Pending"
		}
	}
	desk._handle_network_message(trade_prop_msg)
	if not desk.latest_trade_status_label.text.contains("Pending proposal"):
		printerr("[FAIL]: LatestTradeStatusLabel did not reflect pending trade: " + desk.latest_trade_status_label.text)
		return false
	if desk.accept_trade_button.disabled:
		printerr("[FAIL]: AcceptTradeButton should be enabled for trade proposal recipient.")
		return false
	if desk.reject_trade_button.disabled:
		printerr("[FAIL]: RejectTradeButton should be enabled for trade proposal recipient.")
		return false

	# 5. Test trade proposal from local client to peer (proposer perspective)
	var outgoing_prop := {
		"type": "trade",
		"sender_id": desk._local_client_id,
		"trade": {
			"id": "prop-102",
			"from": desk._local_client_id,
			"to": "peer_player_2",
			"amount": 25000000.0,
			"resource": "Currency",
			"status": "Pending"
		}
	}
	desk._handle_network_message(outgoing_prop)
	if not desk.accept_trade_button.disabled:
		printerr("[FAIL]: Proposer must not be allowed to accept their own trade proposal!")
		return false
	if not desk.reject_trade_button.disabled:
		printerr("[FAIL]: Proposer must not be allowed to reject their own trade proposal!")
		return false

	# 6. Test trade acceptance & treasury adjustment
	desk._latest_state = {
		"treasury": "R 100,000,000",
		"Treasury": "R 100,000,000",
		"country_name": "Republic Sovereign"
	}
	desk.treasury_label.text = "Treasury: R 100,000,000"

	var trade_accept_msg := {
		"type": "trade",
		"sender_id": "host",
		"trade": {
			"id": "prop-101",
			"from": "peer_player_2",
			"to": desk._local_client_id,
			"amount": 50000000.0,
			"resource": "Currency",
			"status": "Accepted"
		}
	}
	desk._handle_network_message(trade_accept_msg)
	if not desk.latest_trade_status_label.text.contains("Accepted"):
		printerr("[FAIL]: LatestTradeStatusLabel did not reflect accepted trade: " + desk.latest_trade_status_label.text)
		return false
	if not desk.accept_trade_button.disabled:
		printerr("[FAIL]: AcceptTradeButton should be disabled after trade is resolved.")
		return false
	if not desk.reject_trade_button.disabled:
		printerr("[FAIL]: RejectTradeButton should be disabled after trade is resolved.")
		return false
	if not desk.treasury_label.text.contains("150"):
		printerr("[FAIL]: Local treasury was not adjusted on desk upon receiving accepted trade funds: " + desk.treasury_label.text)
		return false

	# 7. Test trade rejection
	var trade_reject_msg := {
		"type": "trade",
		"sender_id": "peer_player_2",
		"trade": {
			"id": "prop-103",
			"from": "peer_player_2",
			"to": desk._local_client_id,
			"amount": 10000000.0,
			"resource": "Currency",
			"status": "Rejected"
		}
	}
	desk._handle_network_message(trade_reject_msg)
	if not desk.latest_trade_status_label.text.contains("rejected"):
		printerr("[FAIL]: LatestTradeStatusLabel did not reflect rejected trade: " + desk.latest_trade_status_label.text)
		return false

	# 8. Test disconnect reset behavior
	desk.disconnect_network()
	if not desk.diplomacy_status_label.text.contains("Unrecognized"):
		printerr("[FAIL]: DiplomacyStatusLabel did not reset after disconnect: " + desk.diplomacy_status_label.text)
		return false
	if not desk.latest_message_label.text.contains("[No messages exchanged]"):
		printerr("[FAIL]: LatestMessageLabel did not reset after disconnect: " + desk.latest_message_label.text)
		return false
	if not desk.latest_trade_status_label.text.contains("[No active proposal]"):
		printerr("[FAIL]: LatestTradeStatusLabel did not reset after disconnect: " + desk.latest_trade_status_label.text)
		return false

	print("  [OK]: Phase 3 diplomacy recognition, messaging, and trade proposal acceptance UI verified.")
	return true


func test_phase4_ai_decision_display(desk: Control) -> bool:
	print("\n--- Test 23: Phase 4 Offline Progression & Last AI Decision Line Display ---")

	# 1. Verify LastAiDecisionLabel node exists and is bound
	if desk.last_ai_decision_label == null:
		printerr("[FAIL]: desk.last_ai_decision_label is null!")
		return false

	# 2. Verify display_missing_state sets default message
	desk.display_missing_state("Testing AI decision missing state")
	if not (desk.last_ai_decision_label.text.contains("No AI decisions recorded") or desk.last_ai_decision_label.text.contains("None recorded")):
		printerr("[FAIL]: LastAiDecisionLabel did not show default on missing state: " + desk.last_ai_decision_label.text)
		return false

	# 3. Verify apply_state with last_ai_decision updates the label
	var state_with_ai := {
		"country_name": "Republic of Arcadia",
		"treasury": "R 10,000,000",
		"approval_rating": "65.0%",
		"stability_rating": "70.0%",
		"tax_rate": "25.0%",
		"simulation_time": "Day 1, 06:00 WAT",
		"ruling_party": "Progressive Unity",
		"election_status": "No election held",
		"last_ai_decision": "Republic of Valoria set tax to 26.0% (Day 1, 06:00 WAT) - Lowered income tax to stimulate public approval."
	}
	desk.apply_state(state_with_ai)

	if not desk.last_ai_decision_label.text.contains("Republic of Valoria set tax to 26.0%"):
		printerr("[FAIL]: LastAiDecisionLabel did not display AI decision from exported state: " + desk.last_ai_decision_label.text)
		return false

	# 4. Verify network message of type 'ai_decision' updates the label in real time
	var ai_net_msg := {
		"type": "ai_decision",
		"sender_id": "host",
		"decision": "Republic of Valoria set tax to 27.0% (Day 1, 12:00 WAT) - Raised income tax to reinforce sovereign treasury reserves."
	}
	desk._handle_network_message(ai_net_msg)

	if not desk.last_ai_decision_label.text.contains("27.0%"):
		printerr("[FAIL]: LastAiDecisionLabel did not update from live ai_decision network message: " + desk.last_ai_decision_label.text)
		return false

	# 5. Verify peer snapshot with last_ai_decision updates label
	var peer_snap_msg := {
		"type": "country",
		"sender_id": "peer_ai_host",
		"country": {
			"id": "valoria",
			"name": "Republic of Valoria",
			"treasury": 12500000.0,
			"approval": 62.0,
			"last_ai_decision": "Republic of Valoria set tax to 28.0% (Day 1, 18:00 WAT) - Adjusted income tax."
		}
	}
	desk._handle_network_message(peer_snap_msg)

	if not desk.last_ai_decision_label.text.contains("28.0%"):
		printerr("[FAIL]: LastAiDecisionLabel did not update from peer country snapshot: " + desk.last_ai_decision_label.text)
		return false

	print("  [OK]: Phase 4 offline progression and Last AI Decision UI line verified.")
	return true


## Validates Phase 5 monetization hooks (Buffs panel and simulated purchases) and Ad placeholder
func test_phase5_monetization_and_ad_hooks(desk: Control) -> bool:
	print("\n--- Test 24: Phase 5 Monetization Hooks & Ad Placeholders ---")

	# 1. Verify existence of Buffs UI controls and Ad placeholder
	if desk.buy_admin_buff_button == null:
		printerr("[FAIL]: BuyAdminBuffButton not found on PresidentialDesk.")
		return false
	if desk.buy_investor_buff_button == null:
		printerr("[FAIL]: BuyInvestorBuffButton not found on PresidentialDesk.")
		return false
	if desk.buff_status_label == null:
		printerr("[FAIL]: BuffStatusLabel not found on PresidentialDesk.")
		return false
	if desk.ad_placeholder == null:
		printerr("[FAIL]: AdPlaceholder ColorRect not found on PresidentialDesk.")
		return false

	# 2. Test initial status label text
	if not desk.buff_status_label.text.contains("Buff Status: Ready"):
		printerr("[FAIL]: BuffStatusLabel does not have ready initial text: " + desk.buff_status_label.text)
		return false

	# 3. Clean up any existing buff request file
	var buff_req_path: String = desk._resolve_buff_request_path()
	if FileAccess.file_exists(buff_req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(buff_req_path))

	# 4. Trigger buy_admin_buff
	desk.buy_admin_buff_button.pressed.emit()
	if not FileAccess.file_exists(buff_req_path):
		printerr("[FAIL]: Buff request file was not created at " + buff_req_path)
		return false

	var file := FileAccess.open(buff_req_path, FileAccess.READ)
	if file == null:
		printerr("[FAIL]: Could not open written buff request file at " + buff_req_path)
		return false
	var content := file.get_as_text()
	file.close()

	var parsed: Variant = JSON.parse_string(content)
	if not (parsed is Dictionary):
		printerr("[FAIL]: Buff request JSON is invalid: " + content)
		return false

	var req_dict: Dictionary = parsed
	if req_dict.get("buff_id") != "admin_efficiency":
		printerr("[FAIL]: Expected buff_id 'admin_efficiency' but got: " + str(req_dict.get("buff_id")))
		return false
	if req_dict.get("price_usd") != 0.99:
		printerr("[FAIL]: Expected price_usd 0.99 but got: " + str(req_dict.get("price_usd")))
		return false
	if req_dict.get("status") != "SimulatedPurchaseRequested":
		printerr("[FAIL]: Expected status 'SimulatedPurchaseRequested' but got: " + str(req_dict.get("status")))
		return false

	if not desk.buff_status_label.text.contains("Administrative Efficiency"):
		printerr("[FAIL]: BuffStatusLabel did not reflect purchased buff: " + desk.buff_status_label.text)
		return false

	# 5. Trigger buy_investor_buff
	desk.buy_investor_buff_button.emit_signal("pressed")
	if not FileAccess.file_exists(buff_req_path):
		printerr("[FAIL]: Buff request file was not found after second purchase")
		return false

	file = FileAccess.open(buff_req_path, FileAccess.READ)
	content = file.get_as_text()
	file.close()
	parsed = JSON.parse_string(content)
	req_dict = parsed
	if req_dict.get("buff_id") != "investor_confidence":
		printerr("[FAIL]: Expected buff_id 'investor_confidence' but got: " + str(req_dict.get("buff_id")))
		return false
	if req_dict.get("price_usd") != 1.99:
		printerr("[FAIL]: Expected price_usd 1.99 but got: " + str(req_dict.get("price_usd")))
		return false

	if not desk.buff_status_label.text.contains("Investor Confidence"):
		printerr("[FAIL]: BuffStatusLabel did not reflect investor buff: " + desk.buff_status_label.text)
		return false

	# 6. Verify Ad Placeholder dimensions and visibility
	if not desk.ad_placeholder.visible:
		printerr("[FAIL]: AdPlaceholder is not visible.")
		return false
	if desk.ad_placeholder.custom_minimum_size.x < 300:
		printerr("[FAIL]: AdPlaceholder width is unreasonably small.")
		return false

	# Clean up test file
	if FileAccess.file_exists(buff_req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(buff_req_path))

	print("  [OK]: Phase 5 Buffs panel, simulated buy requests, status feedback, and Ad placeholder verified.")
	return true


func test_phase2_single_player_loop(desk: Control) -> bool:
	print("\n--- Test 25: Phase 2 Single-Player Presidential Game Loop ---")

	var temp_state_path := "res://temp_phase2_state.json"
	var temp_sav_path := "res://temp_phase2_state.sav"
	var req_path := "res://presidential_desk_request.json"

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# 1. Country creation validation: Non-'Republic' name must be rejected
	var invalid_res: Dictionary = desk.execute_create_country("Kingdom of Arcadia")
	if invalid_res.get("success", true):
		printerr("[FAIL]: Country creation with 'Kingdom of Arcadia' should be rejected!")
		return false
	if invalid_res.get("result", "") != "InvalidName":
		printerr("[FAIL]: Expected result 'InvalidName', got: " + str(invalid_res.get("result")))
		return false
	if not desk.country_setup_status_label.text.contains("[Rejected]"):
		printerr("[FAIL]: Expected CountrySetupStatusLabel to indicate [Rejected]: " + desk.country_setup_status_label.text)
		return false

	# 2. Valid country creation request: Must contain 'Republic'
	var valid_res: Dictionary = desk.execute_create_country("Republic of Solaria")
	if not valid_res.get("success", false) or valid_res.get("result", "") != "Pending":
		printerr("[FAIL]: Expected valid creation to return Pending: " + str(valid_res))
		return false
	if not desk.country_setup_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected CountrySetupStatusLabel to indicate [Pending]: " + desk.country_setup_status_label.text)
		return false

	# Verify request file content
	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Expected presidential_desk_request.json to be created on disk for country creation!")
		return false
	var req_file := FileAccess.open(req_path, FileAccess.READ)
	var req_data: Variant = JSON.parse_string(req_file.get_as_text()) if req_file else null
	if req_file:
		req_file.close()
	if req_data == null or req_data.get("action") != "create_country" or req_data.get("country_name") != "Republic of Solaria":
		printerr("[FAIL]: Request file contents mismatch: " + JSON.stringify(req_data))
		return false

	# Clean up request file
	DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate successful creation result
	var create_sim_result := {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Sovereign republic established: Republic of Solaria"
	}
	desk._handle_decision_result(create_sim_result)
	if not desk.country_setup_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected CountrySetupStatusLabel to indicate [Success]: " + desk.country_setup_status_label.text)
		return false

	# 3. National Briefing & Active Project display verification
	var briefing_state := {
		"country_name": "Republic of Solaria",
		"country_id": "solaria-1",
		"treasury": "R 25,000,000",
		"treasury_balance": 25000000,
		"approval": "74%",
		"approval_rating": 74.0,
		"stability": "82%",
		"stability_rating": 82.0,
		"simulation_day": 1,
		"simulation_time": "Day 1, 08:00 WAT",
		"ruling_party": "Democratic Coalition",
		"election": "In 364 days",
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"corporate_tax_rate": 0.20,
		"import_tariff_rate": 0.05,
		"active_project": "None",
		"latest_decision_result": "Constitution enacted",
		"save_file_path": ProjectSettings.globalize_path(temp_sav_path),
		"simulation_tick": 1
	}

	var state_file := FileAccess.open(temp_state_path, FileAccess.WRITE)
	state_file.store_string(JSON.stringify(briefing_state))
	state_file.close()

	if not desk.load_state_from_file(temp_state_path):
		printerr("[FAIL]: Failed to load state from file: " + temp_state_path)
		return false

	if not desk.briefing_treasury_label.text.contains("25,000,000"):
		printerr("[FAIL]: BriefingTreasuryLabel did not display treasury: " + desk.briefing_treasury_label.text)
		return false
	if not desk.briefing_approval_label.text.contains("74%"):
		printerr("[FAIL]: BriefingApprovalLabel did not display approval: " + desk.briefing_approval_label.text)
		return false
	if not desk.briefing_stability_label.text.contains("82%"):
		printerr("[FAIL]: BriefingStabilityLabel did not display stability: " + desk.briefing_stability_label.text)
		return false
	if not desk.briefing_active_project_label.text.contains("None"):
		printerr("[FAIL]: BriefingActiveProjectLabel did not display active project: " + desk.briefing_active_project_label.text)
		return false
	if not desk.briefing_latest_decision_label.text.contains("Constitution enacted"):
		printerr("[FAIL]: BriefingLatestDecisionLabel did not display latest decision: " + desk.briefing_latest_decision_label.text)
		return false
	if not desk.active_project_label.text.contains("None"):
		printerr("[FAIL]: ActiveProjectLabel did not display active project: " + desk.active_project_label.text)
		return false

	# 4. Project decision cancellation
	var cancel_proj_res: Dictionary = desk.execute_project_decision("Road", false)
	if cancel_proj_res.get("result", "") != "Cancelled":
		printerr("[FAIL]: Expected project cancellation result, got: " + JSON.stringify(cancel_proj_res))
		return false
	if not desk.project_status_label.text.contains("[Cancelled]"):
		printerr("[FAIL]: Expected ProjectStatusLabel to indicate [Cancelled]: " + desk.project_status_label.text)
		return false

	# 5. Project decision commissioning (Road)
	var start_proj_res: Dictionary = desk.execute_project_decision("Road", true)
	if not start_proj_res.get("success", false) or start_proj_res.get("result", "") != "Pending":
		printerr("[FAIL]: Expected project commissioning to return Pending: " + JSON.stringify(start_proj_res))
		return false
	if not desk.project_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected ProjectStatusLabel to indicate [Pending]: " + desk.project_status_label.text)
		return false

	# Verify project request file written to disk
	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Expected presidential_desk_request.json to be created for project commissioning!")
		return false

	var proj_req_file := FileAccess.open(req_path, FileAccess.READ)
	var proj_req_data: Variant = JSON.parse_string(proj_req_file.get_as_text()) if proj_req_file else null
	if proj_req_file:
		proj_req_file.close()
	if proj_req_data == null or proj_req_data.get("action") != "start_project" or proj_req_data.get("project_type") != "Road":
		printerr("[FAIL]: Project request file invalid: " + JSON.stringify(proj_req_data))
		return false

	# 6. Simulate project execution result from simulation backend
	var sim_result := {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "National Road Program initiated. R500,000 deducted from treasury."
	}
	desk._handle_decision_result(sim_result)

	if not desk.decision_result_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected DecisionResultLabel to contain [Success]: " + desk.decision_result_label.text)
		return false
	if not desk.project_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected ProjectStatusLabel to contain [Success]: " + desk.project_status_label.text)
		return false

	# Clean up temporary test files
	if FileAccess.file_exists(temp_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_state_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	print("  [OK]: Phase 2 single-player loop (country creation, briefing panel, and project decision) fully verified.")
	return true


func test_political_campaign_and_election_loop(desk: Control) -> bool:
	print("\n--- Test 26: Political Campaign & Democratic Election Decision Loop ---")

	# 1. Verify existence of UI components
	if desk.hold_campaign_rally_button == null:
		printerr("[FAIL]: HoldCampaignRallyButton not found")
		return false
	if desk.contest_election_button == null:
		printerr("[FAIL]: ContestElectionButton not found")
		return false
	if desk.cancel_election_action_button == null:
		printerr("[FAIL]: CancelElectionActionButton not found")
		return false
	if desk.election_status_label == null:
		printerr("[FAIL]: ElectionStatusLabel not found")
		return false

	var temp_state_path: String = "user://test_election_loop_state.json"
	var req_path: String = desk._resolve_request_path(temp_state_path)

	# 2. Write initial state with multi-party election landscape
	var initial_state: Dictionary = {
		"country_name": "Republic of Solaria",
		"country_id": "solaria-1",
		"treasury": "R 15,000,000",
		"treasury_balance": 15000000,
		"approval": "58%",
		"approval_rating": 58.0,
		"stability": "75%",
		"stability_rating": 75.0,
		"simulation_day": 120,
		"simulation_time": "Day 120, 10:00 WAT",
		"ruling_party": "Democratic Coalition",
		"opposition_party": "Liberty Front",
		"polling_support": 54.2,
		"opposition_support": 45.8,
		"election": "In 180 days",
		"next_election_day": 180,
		"income_tax_rate": 0.25,
		"active_project": "None"
	}

	var state_file := FileAccess.open(temp_state_path, FileAccess.WRITE)
	state_file.store_string(JSON.stringify(initial_state))
	state_file.close()

	if not desk.load_state_from_file(temp_state_path):
		printerr("[FAIL]: Failed to load campaign election state: " + temp_state_path)
		return false

	# Verify party display and election schedule
	if not desk.ruling_party_label.text.contains("Democratic Coalition (54.2%)") or not desk.ruling_party_label.text.contains("Liberty Front (45.8%)"):
		printerr("[FAIL]: Ruling party label did not format multi-party polling properly: " + desk.ruling_party_label.text)
		return false
	if not desk.election_label.text.contains("In 180 days") or not desk.election_label.text.contains("Day 180"):
		printerr("[FAIL]: Election label did not format election schedule: " + desk.election_label.text)
		return false

	# 3. Test Campaign Rally cancellation
	var cancel_rally_res: Dictionary = desk.execute_campaign_rally(false)
	if cancel_rally_res.get("result") != "Cancelled":
		printerr("[FAIL]: Expected rally cancellation result to be 'Cancelled', got: " + str(cancel_rally_res))
		return false
	if not desk.election_status_label.text.contains("[Cancelled]"):
		printerr("[FAIL]: Expected election_status_label to show [Cancelled]: " + desk.election_status_label.text)
		return false

	# 4. Test Campaign Rally submission
	var rally_res: Dictionary = desk.execute_campaign_rally(true)
	if rally_res.get("result") != "Pending":
		printerr("[FAIL]: Expected rally result to be 'Pending', got: " + str(rally_res))
		return false
	if not desk.election_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected election_status_label to show [Pending]: " + desk.election_status_label.text)
		return false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Rally request file not created at " + req_path)
		return false

	var req_file := FileAccess.open(req_path, FileAccess.READ)
	var req_data = JSON.parse_string(req_file.get_as_text())
	req_file.close()

	if req_data == null or req_data.get("action") != "campaign_rally" or req_data.get("confirmed") != true:
		printerr("[FAIL]: Rally request data mismatch: " + JSON.stringify(req_data))
		return false

	# Clean up request file
	DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate rally success from simulation backend
	var rally_sim_res: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "National campaign rally held successfully. Public approval rose by 3.5%."
	}
	desk._handle_decision_result(rally_sim_res)
	if not desk.election_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected election_status_label to show [Success]: " + desk.election_status_label.text)
		return false

	# 5. Test Election Contest cancellation
	var cancel_elect_res: Dictionary = desk.execute_contest_election(false)
	if cancel_elect_res.get("result") != "Cancelled":
		printerr("[FAIL]: Expected election cancellation result to be 'Cancelled', got: " + str(cancel_elect_res))
		return false
	if not desk.election_status_label.text.contains("[Cancelled]"):
		printerr("[FAIL]: Expected election_status_label to show [Cancelled]: " + desk.election_status_label.text)
		return false

	# 6. Test Election Contest submission
	var contest_res: Dictionary = desk.execute_contest_election(true)
	if contest_res.get("result") != "Pending":
		printerr("[FAIL]: Expected contest election result to be 'Pending', got: " + str(contest_res))
		return false
	if not desk.election_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected election_status_label to show [Pending]: " + desk.election_status_label.text)
		return false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Election request file not created at " + req_path)
		return false

	req_file = FileAccess.open(req_path, FileAccess.READ)
	req_data = JSON.parse_string(req_file.get_as_text())
	req_file.close()

	if req_data == null or req_data.get("action") != "contest_election" or req_data.get("confirmed") != true:
		printerr("[FAIL]: Election request data mismatch: " + JSON.stringify(req_data))
		return false

	# Clean up request file
	DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate election victory result
	var elect_sim_res := {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Democratic election concluded. Democratic Coalition re-elected with 56.8% majority mandate!"
	}
	desk._handle_decision_result(elect_sim_res)
	if not desk.election_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected election_status_label to show [Success]: " + desk.election_status_label.text)
		return false

	# Clean up temporary test files
	if FileAccess.file_exists(temp_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_state_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	print("  [OK]: Test 26 Political campaign & democratic national election decision loop fully verified.")
	return true


func test_cabinet_appointment_and_tradeoff_loop(desk: Control) -> bool:
	print("\n--- Test 27: Executive Cabinet Appointments & Competence vs Loyalty Loop ---")

	# 1. Verify existence of UI components
	if desk.appoint_technocrat_finance_button == null:
		printerr("[FAIL]: AppointTechnocratFinanceButton not found")
		return false
	if desk.appoint_loyalist_finance_button == null:
		printerr("[FAIL]: AppointLoyalistFinanceButton not found")
		return false
	if desk.cancel_cabinet_action_button == null:
		printerr("[FAIL]: CancelCabinetActionButton not found")
		return false
	if desk.cabinet_status_label == null:
		printerr("[FAIL]: CabinetStatusLabel not found")
		return false
	if desk.cabinet_overview_label == null:
		printerr("[FAIL]: CabinetOverviewLabel not found")
		return false
	if desk.cabinet_summary_label == null:
		printerr("[FAIL]: CabinetSummaryLabel not found")
		return false

	var temp_state_path: String = "user://test_cabinet_loop_state.json"
	var req_path: String = desk._resolve_request_path(temp_state_path)

	# 2. Write initial state with vacant cabinet positions
	var initial_state: Dictionary = {
		"country_name": "Republic of Solaria",
		"country_id": "solaria-1",
		"treasury": "R 15,000,000",
		"treasury_balance": 15000000,
		"approval": "58%",
		"approval_rating": 58.0,
		"stability": "75%",
		"stability_rating": 75.0,
		"simulation_day": 120,
		"simulation_time": "Day 120, 10:00 WAT",
		"ruling_party": "Democratic Coalition",
		"opposition_party": "Liberty Front",
		"polling_support": 54.2,
		"opposition_support": 45.8,
		"election": "In 180 days",
		"next_election_day": 180,
		"income_tax_rate": 0.25,
		"active_project": "None",
		"cabinet_summary": "Finance: Vacant | Interior: Vacant",
		"finance_minister": "Vacant",
		"interior_minister": "Vacant",
		"civil_service_quality": 0.30,
		"corruption_control": 0.30
	}

	var state_file := FileAccess.open(temp_state_path, FileAccess.WRITE)
	state_file.store_string(JSON.stringify(initial_state))
	state_file.close()

	if not desk.load_state_from_file(temp_state_path):
		printerr("[FAIL]: Failed to load cabinet test state: " + temp_state_path)
		return false

	# Verify cabinet display formatting
	if not desk.cabinet_overview_label.text.contains("Finance: Vacant"):
		printerr("[FAIL]: Cabinet overview label did not show initial vacant status: " + desk.cabinet_overview_label.text)
		return false
	if not desk.cabinet_summary_label.text.contains("Civil Service: 30%") or not desk.cabinet_summary_label.text.contains("Corruption Control: 30%"):
		printerr("[FAIL]: Cabinet summary label did not format capacity ratings: " + desk.cabinet_summary_label.text)
		return false

	# 3. Test Cabinet Appointment cancellation
	var cancel_res: Dictionary = desk.execute_cabinet_appointment("Finance", "technocrat", false)
	if cancel_res.get("result") != "Cancelled":
		printerr("[FAIL]: Expected cabinet cancellation result to be 'Cancelled', got: " + str(cancel_res))
		return false
	if not desk.cabinet_status_label.text.contains("[Cancelled]"):
		printerr("[FAIL]: Expected cabinet_status_label to show [Cancelled]: " + desk.cabinet_status_label.text)
		return false

	# 4. Test Technocrat Appointment submission
	var tech_res: Dictionary = desk.execute_cabinet_appointment("Finance", "technocrat", true)
	if tech_res.get("result") != "Pending":
		printerr("[FAIL]: Expected technocrat appointment result to be 'Pending', got: " + str(tech_res))
		return false
	if not desk.cabinet_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected cabinet_status_label to show [Pending]: " + desk.cabinet_status_label.text)
		return false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Cabinet request file not created at " + req_path)
		return false

	var req_file := FileAccess.open(req_path, FileAccess.READ)
	var req_data = JSON.parse_string(req_file.get_as_text())
	req_file.close()

	if req_data == null or req_data.get("action") != "appoint_minister" or req_data.get("portfolio") != "Finance" or req_data.get("candidate_type") != "technocrat" or req_data.get("confirmed") != true:
		printerr("[FAIL]: Cabinet request data mismatch: " + JSON.stringify(req_data))
		return false

	# Clean up request file
	DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate technocrat appointment success from simulation backend
	var sim_tech_res: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Cabinet appointment confirmed: Appointed Technocrat to Finance. Institutional capacity strengthened.",
		"cabinet_summary": "Finance: Technocrat (0.90) | Interior: Vacant (0.30)",
		"civil_service_quality": 0.90,
		"corruption_control": 0.30
	}
	desk._handle_decision_result(sim_tech_res)
	if not desk.cabinet_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected cabinet_status_label to show [Success]: " + desk.cabinet_status_label.text)
		return false

	# 5. Test Party Loyalist Appointment submission
	var loyalist_res: Dictionary = desk.execute_cabinet_appointment("Finance", "loyalist", true)
	if loyalist_res.get("result") != "Pending":
		printerr("[FAIL]: Expected loyalist appointment result to be 'Pending', got: " + str(loyalist_res))
		return false
	if not desk.cabinet_status_label.text.contains("[Pending]"):
		printerr("[FAIL]: Expected cabinet_status_label to show [Pending]: " + desk.cabinet_status_label.text)
		return false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Cabinet request file for loyalist not created at " + req_path)
		return false

	req_file = FileAccess.open(req_path, FileAccess.READ)
	req_data = JSON.parse_string(req_file.get_as_text())
	req_file.close()

	if req_data == null or req_data.get("candidate_type") != "loyalist":
		printerr("[FAIL]: Cabinet loyalist request data mismatch: " + JSON.stringify(req_data))
		return false

	# Clean up request file
	DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate party loyalist appointment result
	var sim_loyalist_res: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Cabinet appointment confirmed: Appointed Party Loyalist to Finance. Ruling party polling boosted, administrative capacity reduced.",
		"cabinet_summary": "Finance: Loyalist (0.45) | Interior: Vacant (0.30)",
		"civil_service_quality": 0.45,
		"corruption_control": 0.30
	}
	desk._handle_decision_result(sim_loyalist_res)
	if not desk.cabinet_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Expected cabinet_status_label to show [Success]: " + desk.cabinet_status_label.text)
		return false

	# Clean up temporary test files
	if FileAccess.file_exists(temp_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_state_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	print("  [OK]: Test 27 Executive cabinet appointments & competence vs loyalty loop fully verified.")
	return true


func test_mvp_multiplayer_simulation_and_trade_lifecycle(desk: Control) -> bool:
	print("\n--- Test 28: MVP Multiplayer Reliability, Trade Lifecycle Stages, Defense Posture & Offline Briefing ---")

	# 1. Verify UI nodes exist
	if desk.defense_readiness_label == null:
		printerr("[FAIL]: DefenseReadinessLabel not found")
		return false
	if desk.offline_briefing_label == null:
		printerr("[FAIL]: OfflineBriefingLabel not found")
		return false
	if desk.trade_lifecycle_stage_label == null:
		printerr("[FAIL]: TradeLifecycleStageLabel not found")
		return false

	var temp_state_path: String = "user://test_mvp_multiplayer_state.json"

	# 2. Test state application with defense posture and offline catch-up briefing
	var test_state: Dictionary = {
		"country_name": "Republic of Arcadia",
		"country_id": "arcadia-1",
		"treasury": "R 10,000,000,000",
		"treasury_balance": 10000000000,
		"approval": "72%",
		"approval_rating": 72.0,
		"stability": "85%",
		"stability_rating": 85.0,
		"simulation_day": 10,
		"simulation_time": "Day 10, 12:00 WAT",
		"ruling_party": "Federal Republic Movement",
		"opposition_party": "Civic Union",
		"polling_support": 58.0,
		"opposition_support": 42.0,
		"income_tax_rate": 0.25,
		"active_project": "None",
		"defcon_level": "DEFCON 5 (Peacetime Watch)",
		"military_readiness": "85.0% (Armed Forces Operational)",
		"offline_briefing_summary": "Offline Catch-Up (Day 6 to Day 10): 4 production cycles credited (+R 4,000,000 deposited, 1 projects completed)."
	}

	var state_file := FileAccess.open(temp_state_path, FileAccess.WRITE)
	state_file.store_string(JSON.stringify(test_state))
	state_file.close()

	if not desk.load_state_from_file(temp_state_path):
		printerr("[FAIL]: Failed to load MVP test state from " + temp_state_path)
		return false

	# Verify defense posture and offline briefing labels
	if not desk.defense_readiness_label.text.contains("DEFCON 5") or not desk.defense_readiness_label.text.contains("Armed Forces"):
		printerr("[FAIL]: Defense readiness label did not display DEFCON and armed forces: " + desk.defense_readiness_label.text)
		return false

	if not desk.offline_briefing_label.text.contains("Offline Catch-Up") or not desk.offline_briefing_label.text.contains("4 production cycles credited"):
		printerr("[FAIL]: Offline briefing label did not display catch-up summary: " + desk.offline_briefing_label.text)
		return false

	# 3. Test Trade Lifecycle Stages
	# A. Tuning trade amount -> [Draft Proposal]
	desk._on_trade_amount_changed(250000000.0)
	if not desk.trade_lifecycle_stage_label.text.contains("[Draft Proposal]"):
		printerr("[FAIL]: Expected trade_lifecycle_stage_label to show [Draft Proposal], got: " + desk.trade_lifecycle_stage_label.text)
		return false

	# B. Submitting trade proposal -> [Pending Proposal]
	desk.trade_recipient_input.text = "Federation of Valoria"
	desk._on_propose_trade_button_pressed()
	if not desk.trade_lifecycle_stage_label.text.contains("[Pending Proposal]"):
		printerr("[FAIL]: Expected trade_lifecycle_stage_label to show [Pending Proposal], got: " + desk.trade_lifecycle_stage_label.text)
		return false

	# C. Receiving Accepted / Completed Trade -> [Accepted Agreement / Completed Transaction]
	desk._handle_network_message({
		"type": "trade",
		"trade": {
			"from": "Republic of Arcadia",
			"to": "Federation of Valoria",
			"amount": 250000000.0,
			"resource": "REPU",
			"status": "Accepted",
			"lifecycle_stage": "Accepted Agreement"
		}
	})
	if not desk.trade_lifecycle_stage_label.text.contains("[Accepted Agreement / Completed Transaction]"):
		printerr("[FAIL]: Expected trade_lifecycle_stage_label to show [Accepted Agreement / Completed Transaction], got: " + desk.trade_lifecycle_stage_label.text)
		return false

	# D. Receiving Rejected Trade -> [Rejected Proposal]
	desk._handle_network_message({
		"type": "trade",
		"trade": {
			"from": "Republic of Arcadia",
			"to": "Federation of Valoria",
			"amount": 250000000.0,
			"resource": "REPU",
			"status": "Rejected",
			"lifecycle_stage": "Rejected Proposal"
		}
	})
	if not desk.trade_lifecycle_stage_label.text.contains("[Rejected Proposal]"):
		printerr("[FAIL]: Expected trade_lifecycle_stage_label to show [Rejected Proposal], got: " + desk.trade_lifecycle_stage_label.text)
		return false

	# 4. Test Session Reconnection / Recovery Message Handling
	desk._handle_network_message({
		"type": "decision_result",
		"country_id": "arcadia-1",
		"result": "Success",
		"status_message": "Reconnected session for country 'arcadia-1'."
	})
	if not desk.multiplayer_status_label.text.contains("Reconnected session"):
		printerr("[FAIL]: Multiplayer status label did not reflect reconnected session: " + desk.multiplayer_status_label.text)
		return false

	# Clean up temporary test file
	if FileAccess.file_exists(temp_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_state_path))

	print("  [OK]: Test 28 MVP multiplayer reliability, trade lifecycle stages, defense posture, and offline briefing fully verified.")
	return true









