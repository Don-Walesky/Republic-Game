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

	desk.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 9 GODOT PRESIDENTIAL DESK CHECKS SUCCEEDED!       ")
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

	# Verify status label is visible and indicates waiting/missing
	if not desk.status_label.text.contains("Waiting for Republic simulation state"):
		printerr("[FAIL]: Status label does not indicate waiting state: " + desk.status_label.text)
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

	# Act: Enact 30% income tax rate (+5% hike -> approval drops from 70% to 67.5%)
	var outcome: Dictionary = desk.execute_tax_decision(30.0, true)

	if not outcome.get("success", false):
		printerr("[FAIL]: Expected decision execution to succeed, but returned: " + JSON.stringify(outcome))
		return false

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

	# Verify reopening / reloading does not revert to old or demo values
	desk.load_state_from_file(temp_path)
	if desk.tax_rate_label.text != "Income Tax Rate: 30%" or desk.approval_label.text != "Approval: 67.5%":
		printerr("[FAIL]: Reopening state reverted values! Tax: " + desk.tax_rate_label.text + ", Approval: " + desk.approval_label.text)
		return false

	# Clean up
	DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_path))
	DirAccess.remove_absolute(ProjectSettings.globalize_path("res://presidential_desk_result.json"))

	print("  [OK]: Confirmed decision successfully modified authoritative country state and refreshed Godot UI.")
	return true


func test_cancelled_decision_preserves_state(desk: Control) -> bool:
	print("\n--- Test 8: Cancelled Decision Preserves State & Resets Controls ---")
	var temp_path := "res://temp_cancel_test_state.json"
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
	print("  [OK]: Cancelling decision preserved country state completely unchanged and reset selector.")
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
