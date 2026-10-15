extends SceneTree

## Headless test suite validating the Godot Presidential Desk connection to Republic simulation state
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

	desk.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 4 GODOT PRESIDENTIAL DESK CHECKS SUCCEEDED!       ")
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
		desk.election_label.text
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

	if desk.simulation_time_label.text != "Simulation Time: Day 4, 18:00 WAT":
		printerr("[FAIL]: Updated simulation time not reflected. Got: " + desk.simulation_time_label.text)
		return false

	if desk.ruling_party_label.text != "Ruling party: Democratic Coalition":
		printerr("[FAIL]: Updated ruling party not reflected. Got: " + desk.ruling_party_label.text)
		return false

	print("  [OK]: Dynamic state changes verified on interface labels.")
	return true
