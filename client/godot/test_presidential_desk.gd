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

	desk.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 17 GODOT PRESIDENTIAL DESK CHECKS SUCCEEDED!     ")
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
		desk.tax_rate_label.text
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

	# Simulate simulation processing: update state file, delete request
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	initial_state["income_tax_rate"] = 0.30
	initial_state["income_tax_rate_formatted"] = "30%"
	initial_state["approval"] = "67.5%"
	initial_state["approval_rating"] = 67.5
	initial_state["latest_decision_result"] = "Tax hike enacted: Personal Income Tax increased to 30.0% (-2.5% citizen approval)."

	var f_up := FileAccess.open(temp_path, FileAccess.WRITE)
	f_up.store_string(JSON.stringify(initial_state))
	f_up.close()

	desk.load_state_from_file(temp_path)

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

	# Simulate completion: call apply_state to re-enable
	desk.apply_state(initial_state)
	if desk.confirm_button.disabled or desk.cancel_button.disabled or not desk.proposed_rate_spinbox.editable:
		printerr("[FAIL]: Interactive controls were not re-enabled after state refresh!")
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



