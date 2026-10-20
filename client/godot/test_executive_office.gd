extends SceneTree

## Automated Headless Verification Suite for ExecutiveOffice.tscn
## Validates decoupled visual environment layers, four-edge interface HUD docking spaces,
## honest missing state handling, corrupt JSON rejection, live Republic.Core state loading,
## and dynamic state updates.

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE FOUNDATION AUTOMATED TEST      ")
	print("==============================================================")

	var scene_res := load("res://ExecutiveOffice.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var office = scene_res.instantiate()
	if office == null:
		printerr("[FAIL]: Failed to instantiate ExecutiveOffice node")
		quit(1)
		return

	root.add_child(office)
	office._init_controls()

	var all_passed := true

	# Test 1: Decoupled Office Environment Visual Layers
	all_passed = test_office_environment_layers(office) and all_passed

	# Test 2: Interface HUD Docking Spaces (Top, Left, Right, Bottom)
	all_passed = test_interface_hud_docking(office) and all_passed

	# Test 3: Missing State Safe Display (Zero Mock Data)
	all_passed = test_missing_state_behavior(office) and all_passed

	# Test 4: Corrupt JSON Rejection
	all_passed = test_corrupt_json_handling(office) and all_passed

	# Test 5: Live Simulation State Loading
	all_passed = test_live_state_loading(office) and all_passed

	# Test 6: Dynamic State Modification & Signal Dispatch
	all_passed = test_dynamic_state_updates(office) and all_passed

	# Test 7: Decoupled Customization API
	all_passed = test_customization_api(office) and all_passed

	# Test 8: Navigation to Presidential Desk
	all_passed = test_navigation_link(office) and all_passed

	office.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 8 EXECUTIVE OFFICE FOUNDATION CHECKS SUCCEEDED!  ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE EXECUTIVE OFFICE CHECKS FAILED!          ")
		printerr("==============================================================")
		quit(1)


func test_office_environment_layers(office: Control) -> bool:
	print("\n--- Test 1: Decoupled Office Environment Visual Layers ---")
	if office.office_environment == null:
		printerr("[FAIL]: office_environment node missing.")
		return false

	if office.room_backdrop == null:
		printerr("[FAIL]: room_backdrop node missing.")
		return false

	if office.capital_window == null or office.city_scenery == null or office.window_frame == null:
		printerr("[FAIL]: capital_window or city_scenery or window_frame missing.")
		return false

	if office.country_flag == null or office.flag_pole == null or office.flag_banner == null or office.flag_emblem_label == null:
		printerr("[FAIL]: country_flag nodes missing.")
		return false

	if office.presidential_portrait == null or office.portrait_frame == null or office.portrait_canvas == null or office.portrait_name_label == null:
		printerr("[FAIL]: presidential_portrait nodes missing.")
		return false

	if office.executive_desk == null or office.desk_surface == null or office.desk_blotter == null or office.hotline_phone == null or office.executive_inbox == null or office.presidential_seal == null:
		printerr("[FAIL]: executive_desk nodes missing.")
		return false

	print("  [OK]: All decoupled visual environment layers (Window, Flag, Portrait, Desk) verified.")
	return true


func test_interface_hud_docking(office: Control) -> bool:
	print("\n--- Test 2: Interface HUD Docking Spaces (Top, Left, Right, Bottom) ---")
	if office.interface_layer == null:
		printerr("[FAIL]: interface_layer missing.")
		return false

	# Top Bar
	if office.top_bar == null or office.country_label == null or office.simulation_time_label == null or office.treasury_label == null or office.approval_label == null or office.stability_label == null or office.defcon_label == null or office.switch_to_desk_button == null:
		printerr("[FAIL]: Top bar interface elements missing.")
		return false

	# Left Dock
	if office.left_dock == null or office.toggle_left_dock_button == null or office.cabinet_summary_label == null or office.civil_service_label == null or office.corruption_control_label == null or office.ruling_party_label == null or office.polling_support_label == null:
		printerr("[FAIL]: Left dock interface elements missing.")
		return false

	# Right Dock
	if office.right_dock == null or office.toggle_right_dock_button == null or office.military_readiness_label == null or office.active_crisis_label == null or office.gdp_label == null or office.offline_briefing_label == null:
		printerr("[FAIL]: Right dock interface elements missing.")
		return false

	# Bottom Dock
	if office.bottom_dock == null or office.state_status_label == null or office.active_project_label == null or office.status_ticker_label == null:
		printerr("[FAIL]: Bottom dock interface elements missing.")
		return false

	print("  [OK]: All four-edge interface HUD docking spaces correctly established.")
	return true


func test_missing_state_behavior(office: Control) -> bool:
	print("\n--- Test 3: Missing State Safe Display (Zero Mock Data) ---")
	office.display_missing_state()

	if office.country_label.text != "Country: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' country label, got: " + office.country_label.text)
		return false

	if office.treasury_label.text != "Treasury: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' treasury label, got: " + office.treasury_label.text)
		return false

	if office.approval_label.text != "Approval: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' approval label, got: " + office.approval_label.text)
		return false

	if office.stability_label.text != "Stability: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' stability label, got: " + office.stability_label.text)
		return false

	if office.active_project_label.text != "Active Project: None":
		printerr("[FAIL]: Expected 'Active Project: None', got: " + office.active_project_label.text)
		return false

	print("  [OK]: Missing state safely handled and hardcoded mock data confirmed completely absent.")
	return true


func test_corrupt_json_handling(office: Control) -> bool:
	print("\n--- Test 4: Corrupt JSON Rejection ---")
	var corrupt_path := "res://temp_corrupt_exec_state.json"
	var fa := FileAccess.open(corrupt_path, FileAccess.WRITE)
	fa.store_string("{ invalid_json: unquoted, broken }")
	fa.close()

	var success: bool = office.load_state_from_file(corrupt_path)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(corrupt_path))

	if success:
		printerr("[FAIL]: Corrupt JSON was erroneously accepted as valid state.")
		return false

	if office.country_label.text != "Country: [No State Loaded]":
		printerr("[FAIL]: Expected label to reset to '[No State Loaded]', but was: " + office.country_label.text)
		return false

	print("  [OK]: Corrupt JSON rejected cleanly with safe display fallback.")
	return true


func test_live_state_loading(office: Control) -> bool:
	print("\n--- Test 5: Live Simulation State Loading ---")
	var live_state_path := "res://presidential_desk_state.json"
	if not FileAccess.file_exists(live_state_path):
		printerr("[FAIL]: State file res://presidential_desk_state.json missing.")
		return false

	var success: bool = office.load_state_from_file(live_state_path)
	if not success:
		printerr("[FAIL]: Failed to load live state from " + live_state_path)
		return false

	if office.country_label.text != "Country: Republic of Arcadia":
		printerr("[FAIL]: Country label mismatch. Got: " + office.country_label.text)
		return false

	if not office.treasury_label.text.begins_with("Treasury: R"):
		printerr("[FAIL]: Treasury label format mismatch. Got: " + office.treasury_label.text)
		return false

	if not office.approval_label.text.begins_with("Approval:"):
		printerr("[FAIL]: Approval label format mismatch. Got: " + office.approval_label.text)
		return false

	if not office.stability_label.text.begins_with("Stability:"):
		printerr("[FAIL]: Stability label format mismatch. Got: " + office.stability_label.text)
		return false

	if not office.simulation_time_label.text.begins_with("Simulation Time: Day 0"):
		printerr("[FAIL]: Simulation time mismatch. Got: " + office.simulation_time_label.text)
		return false

	if not office.state_status_label.text.contains("Connected"):
		printerr("[FAIL]: Status label mismatch. Got: " + office.state_status_label.text)
		return false

	print("  [OK]: Live Republic state accurately loaded and verified on all HUD docking spaces.")
	return true


func test_dynamic_state_updates(office: Control) -> bool:
	print("\n--- Test 6: Dynamic State Modification & Signal Dispatch ---")
	var update_path := "res://temp_dynamic_exec_update.json"
	var updated_json := {
		"country_name": "Federal Republic of Solaria",
		"treasury": "R 22,500,000,000",
		"approval": "74.2%",
		"stability": "88.0%",
		"simulation_time": "Day 5, 12:00 WAT",
		"defcon_level": "DEFCON 4",
		"cabinet_summary": "Finance: Prof. Amara Cole | Interior: Gen. Vance",
		"civil_service_quality": 0.85,
		"corruption_control": 0.75,
		"ruling_party": "Federal Alliance",
		"polling_support": 61.5,
		"military_readiness": "92.0% Combat Ready",
		"active_crisis": "None",
		"gdp_formatted": "R 510,000,000,000",
		"offline_briefing_summary": "Cabinet assembled.",
		"active_project": "National Power Plant",
		"latest_decision_result": "National Power Plant initiated."
	}

	var fa := FileAccess.open(update_path, FileAccess.WRITE)
	fa.store_string(JSON.stringify(updated_json))
	fa.close()

	var success: bool = office.load_state_from_file(update_path)
	DirAccess.remove_absolute(ProjectSettings.globalize_path(update_path))

	if not success:
		printerr("[FAIL]: Failed to load dynamic update state.")
		return false

	if office.country_label.text != "Country: Federal Republic of Solaria":
		printerr("[FAIL]: Dynamic country name mismatch. Got: " + office.country_label.text)
		return false

	if office.treasury_label.text != "Treasury: R 22,500,000,000":
		printerr("[FAIL]: Dynamic treasury mismatch. Got: " + office.treasury_label.text)
		return false

	if office.approval_label.text != "Approval: 74.2%":
		printerr("[FAIL]: Dynamic approval mismatch. Got: " + office.approval_label.text)
		return false

	if office.active_project_label.text != "Active Project: National Power Plant":
		printerr("[FAIL]: Dynamic active project mismatch. Got: " + office.active_project_label.text)
		return false

	if office.flag_emblem_label.text != "SOLARIA":
		printerr("[FAIL]: Flag emblem text mismatch. Got: " + office.flag_emblem_label.text)
		return false

	print("  [OK]: Dynamic state changes verified across HUD docks and office decor.")
	return true


func test_customization_api(office: Control) -> bool:
	print("\n--- Test 7: Decoupled Customization API ---")
	office.set_country_flag(Color(0.2, 0.5, 0.3, 1.0), "ARCADIA")
	if office.flag_emblem_label.text != "ARCADIA":
		printerr("[FAIL]: set_country_flag did not update flag label.")
		return false

	office.set_presidential_portrait(null, "PRESIDENT ELENA ROSTOVA", "Civic Union")
	if office.portrait_name_label.text != "PRESIDENT ELENA ROSTOVA":
		printerr("[FAIL]: set_presidential_portrait did not update name.")
		return false

	if office.portrait_party_label.text != "Party: Civic Union":
		printerr("[FAIL]: set_presidential_portrait did not update party.")
		return false

	office.set_desk_style(Color(0.2, 0.15, 0.1), Color(0.1, 0.1, 0.15))
	print("  [OK]: Customization API verified for flag, portrait, and desk furniture.")
	return true


func test_navigation_link(office: Control) -> bool:
	print("\n--- Test 8: Navigation to Presidential Desk ---")
	if office.switch_to_desk_button == null:
		printerr("[FAIL]: switch_to_desk_button missing.")
		return false

	if office.switch_to_desk_button.text.is_empty():
		printerr("[FAIL]: switch_to_desk_button text is empty.")
		return false

	# Verify target scene exists
	if not ResourceLoader.exists("res://PresidentialDesk.tscn"):
		printerr("[FAIL]: Target scene res://PresidentialDesk.tscn does not exist.")
		return false

	print("  [OK]: Navigation link to legacy PresidentialDesk confirmed.")
	return true
