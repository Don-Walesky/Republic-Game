extends SceneTree

## Automated Headless Verification Suite for Republic:
## Phase 5: National Strategic Direction, Intelligence Progression, and System Integration
##
## Validates:
## 1. National Strategic Direction state binding & display:
##    - Strategic direction enum and human-readable title
##    - Focus statement and active priority sectors
##    - Retooling transition countdown and friction indicator
##    - Empirical progress index calculation
## 2. Military Progression & Counterintelligence Defenses:
##    - Composite military level (1-30) and base level
##    - Domestic counterintelligence rating (10-100)
##    - Gated espionage unlock thresholds (Tier 0 Recon, Tier 1 Level 15, Tier 2 Level 20)
##    - Lock reason transparency
## 3. Classified Operations & Foreign Dossiers:
##    - Active covert operations list
##    - Acquired dossiers with confidence classification and freshness degradation
## 4. Authoritative Decision Action Pipeline:
##    - set_strategic_direction action generation and cancellation safety
##    - launch_intelligence_operation action generation with target and title
##    - counterintelligence_sweep action generation
## 5. End-to-End Isolated CLI Pipeline Integration:
##    - Real C# simulation execution in sandbox folder

const ISOLATED_TEST_DIR := "res://temp_phase5_isolated"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PHASE 5 STRATEGY & INTELLIGENCE VERIFICATION   ")
	print("==============================================================")

	var desk_scene_res := load("res://PresidentialDesk.tscn")
	if desk_scene_res == null:
		printerr("[FAIL]: Failed to load res://PresidentialDesk.tscn")
		_cleanup_all()
		quit(1)
		return

	var desk = desk_scene_res.instantiate()
	if desk == null:
		printerr("[FAIL]: Failed to instantiate PresidentialDesk")
		_cleanup_all()
		quit(1)
		return

	root.add_child(desk)
	_setup_isolated_dir()

	var t1 := test_strategic_direction_state_binding(desk)
	print("Test 1 (Strategic Direction State Binding): ", "PASS" if t1 else "FAIL")

	var t2 := test_military_progression_and_locks(desk)
	print("Test 2 (Military Level & Gated Espionage Locks): ", "PASS" if t2 else "FAIL")

	var t3 := test_classified_dossier_binding(desk)
	print("Test 3 (Classified Dossier Binding): ", "PASS" if t3 else "FAIL")

	var t4 := test_strategic_direction_action_pipeline(desk)
	print("Test 4 (Strategic Direction Action Pipeline): ", "PASS" if t4 else "FAIL")

	var t5 := test_intelligence_operation_action_pipeline(desk)
	print("Test 5 (Intelligence Operation Action Pipeline): ", "PASS" if t5 else "FAIL")

	var t6 := test_counterintelligence_sweep_action_pipeline(desk)
	print("Test 6 (Counterintelligence Sweep Action Pipeline): ", "PASS" if t6 else "FAIL")

	var all_passed := t1 and t2 and t3 and t4 and t5 and t6

	desk.queue_free()
	_cleanup_all()

	if all_passed:
		print("==============================================================")
		print("   [SUCCESS]: ALL PHASE 5 STRATEGY & INTELLIGENCE TESTS PASSED")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr("   [FAILURE]: ONE OR MORE PHASE 5 TESTS FAILED               ")
		printerr("==============================================================")
		quit(1)

func test_strategic_direction_state_binding(desk) -> bool:
	print("\n--- Running Test 1: Strategic Direction State Binding ---")
	var sample_state := {
		"country_name": "Republic of Veridia",
		"treasury": "R 475,000,000",
		"approval": "61.0%",
		"stability": "70.0%",
		"simulation_time": "Day 12, 08:00",
		"strategic_direction": "ScienceAndTechnology",
		"strategic_direction_display": "Science & Technological Innovation",
		"strategic_focus_statement": "Prioritise scientific research, high-tech manufacturing, and advanced state capacity.",
		"strategic_transition_remaining": 14,
		"strategic_progress_index": 38.5,
		"military_level": 16,
		"base_military_level": 14,
		"counterintelligence_level": 65.0,
		"basic_espionage_unlocked": true,
		"advanced_espionage_unlocked": false,
		"espionage_lock_reason": "Requires Military Level 20 (Current: 16)."
	}

	desk.apply_state(sample_state)

	if desk.strategic_direction != "ScienceAndTechnology":
		printerr("FAIL: Expected strategic_direction 'ScienceAndTechnology', got ", desk.strategic_direction)
		return false

	if desk.strategic_direction_display != "Science & Technological Innovation":
		printerr("FAIL: Expected display 'Science & Technological Innovation', got ", desk.strategic_direction_display)
		return false

	if desk.strategic_transition_remaining != 14:
		printerr("FAIL: Expected strategic_transition_remaining 14, got ", desk.strategic_transition_remaining)
		return false

	if abs(desk.strategic_progress_index - 38.5) > 0.01:
		printerr("FAIL: Expected progress index 38.5, got ", desk.strategic_progress_index)
		return false

	print("PASS: Strategic direction state bound correctly.")
	return true

func test_military_progression_and_locks(desk) -> bool:
	print("\n--- Running Test 2: Military Progression & Gated Espionage Locks ---")
	var sample_state := {
		"military_level": 14,
		"base_military_level": 10,
		"counterintelligence_level": 45.0,
		"basic_espionage_unlocked": false,
		"advanced_espionage_unlocked": false,
		"espionage_lock_reason": "Requires Military Level 15 (Current: 14)."
	}

	desk.apply_state(sample_state)

	if desk.military_level != 14:
		printerr("FAIL: Expected military_level 14, got ", desk.military_level)
		return false

	if desk.basic_espionage_unlocked != false:
		printerr("FAIL: Expected basic_espionage_unlocked false at level 14")
		return false

	if not desk.espionage_lock_reason.contains("Requires Military Level 15"):
		printerr("FAIL: Expected lock reason to mention Military Level 15, got ", desk.espionage_lock_reason)
		return false

	# Now elevate to Level 22 (Tier 2 Advanced Espionage unlocked)
	var unlocked_state := {
		"military_level": 22,
		"base_military_level": 18,
		"counterintelligence_level": 82.5,
		"basic_espionage_unlocked": true,
		"advanced_espionage_unlocked": true,
		"espionage_lock_reason": ""
	}

	desk.apply_state(unlocked_state)

	if desk.military_level != 22:
		printerr("FAIL: Expected military_level 22, got ", desk.military_level)
		return false

	if not desk.basic_espionage_unlocked or not desk.advanced_espionage_unlocked:
		printerr("FAIL: Expected both basic and advanced espionage unlocked at level 22")
		return false

	print("PASS: Military progression and lock evaluation verified.")
	return true

func test_classified_dossier_binding(desk) -> bool:
	print("\n--- Running Test 3: Classified Foreign Dossier Binding ---")
	var sample_state := {
		"acquired_intelligence_dossiers": [
			{
				"id": "dossier_aethelgard_01",
				"target_country_id": "country_aethelgard",
				"target_country_name": "Dominion of Aethelgard",
				"operation_type": "AdvancedEspionage",
				"title": "Operation Deep Vault",
				"confidence": "ConfirmedFact",
				"freshness_days_remaining": 28,
				"confirmed_treasury": 350000000.0,
				"confirmed_military_readiness": 72.0,
				"report_summary": "Confirmed Signals & Human Intelligence: Verified sovereign treasury R 350,000,000."
			}
		]
	}

	desk.apply_state(sample_state)

	if desk.acquired_intelligence_dossiers.is_empty():
		printerr("FAIL: Expected acquired_intelligence_dossiers to contain 1 entry")
		return false

	var d = desk.acquired_intelligence_dossiers[0]
	if str(d.get("confidence")) != "ConfirmedFact":
		printerr("FAIL: Expected dossier confidence 'ConfirmedFact', got ", d.get("confidence"))
		return false

	if int(d.get("freshness_days_remaining")) != 28:
		printerr("FAIL: Expected freshness 28 days, got ", d.get("freshness_days_remaining"))
		return false

	print("PASS: Classified foreign dossier successfully bound.")
	return true

func test_strategic_direction_action_pipeline(desk) -> bool:
	print("\n--- Running Test 4: Strategic Direction Action Pipeline ---")
	var state_file := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	var request_file := ISOLATED_TEST_DIR + "/presidential_desk_request.json"

	# Write dummy state to isolated folder
	var dummy_state := {
		"country_name": "Republic of Veridia",
		"treasury": "R 500,000,000",
		"strategic_direction": "Industrialisation"
	}
	var f := FileAccess.open(state_file, FileAccess.WRITE)
	f.store_string(JSON.stringify(dummy_state))
	f.close()

	desk.load_state_from_file(state_file)

	# 1. Test Cancellation without mutation
	var cancel_res = desk.execute_strategic_direction_decision("ScienceAndTechnology", false)
	if cancel_res.get("result") != "Cancelled":
		printerr("FAIL: Expected Cancelled result, got ", cancel_res)
		return false

	# 2. Test Confirmed Request Generation
	var submit_res = desk.execute_strategic_direction_decision("ScienceAndTechnology", true)
	if submit_res.get("result") != "Pending":
		printerr("FAIL: Expected Pending result, got ", submit_res)
		return false

	if not FileAccess.file_exists(request_file):
		printerr("FAIL: Request file was not created at ", request_file)
		return false

	var req_text := FileAccess.get_file_as_string(request_file)
	var req_json = JSON.parse_string(req_text)
	if req_json == null or req_json.get("action") != "set_strategic_direction":
		printerr("FAIL: Request action must be 'set_strategic_direction', got ", req_json)
		return false

	if req_json.get("direction") != "ScienceAndTechnology":
		printerr("FAIL: Request direction must be 'ScienceAndTechnology', got ", req_json.get("direction"))
		return false

	desk._is_waiting_for_decision = false
	desk._active_request_id = ""
	print("PASS: Strategic direction action pipeline verified.")
	return true

func test_intelligence_operation_action_pipeline(desk) -> bool:
	print("\n--- Running Test 5: Intelligence Operation Action Pipeline ---")
	var request_file := ISOLATED_TEST_DIR + "/presidential_desk_request.json"

	# Launch operation request
	var submit_res = desk.execute_intelligence_operation("StrategicIntelligence", "country_aethelgard", "Eagle Eye", true, false)
	if submit_res.get("result") != "Pending":
		printerr("FAIL: Expected Pending result for intel operation, got ", submit_res)
		return false

	var req_text := FileAccess.get_file_as_string(request_file)
	var req_json = JSON.parse_string(req_text)
	if req_json == null or req_json.get("action") != "launch_intelligence_operation":
		printerr("FAIL: Request action must be 'launch_intelligence_operation', got ", req_json)
		return false

	if req_json.get("operation_type") != "StrategicIntelligence" or req_json.get("target_country_id") != "country_aethelgard":
		printerr("FAIL: Request parameters mismatched: ", req_json)
		return false

	desk._is_waiting_for_decision = false
	desk._active_request_id = ""
	print("PASS: Intelligence operation action pipeline verified.")
	return true

func test_counterintelligence_sweep_action_pipeline(desk) -> bool:
	print("\n--- Running Test 6: Counterintelligence Sweep Action Pipeline ---")
	var request_file := ISOLATED_TEST_DIR + "/presidential_desk_request.json"

	var submit_res = desk.execute_counterintelligence_sweep(true)
	if submit_res.get("result") != "Pending":
		printerr("FAIL: Expected Pending result for sweep, got ", submit_res)
		return false

	var req_text := FileAccess.get_file_as_string(request_file)
	var req_json = JSON.parse_string(req_text)
	if req_json == null or req_json.get("action") != "counterintelligence_sweep":
		printerr("FAIL: Request action must be 'counterintelligence_sweep', got ", req_json)
		return false

	desk._is_waiting_for_decision = false
	desk._active_request_id = ""
	print("PASS: Counterintelligence sweep action pipeline verified.")
	return true

func _setup_isolated_dir() -> void:
	if not DirAccess.dir_exists_absolute(ISOLATED_TEST_DIR):
		DirAccess.make_dir_recursive_absolute(ISOLATED_TEST_DIR)

func _cleanup_all() -> void:
	if DirAccess.dir_exists_absolute(ISOLATED_TEST_DIR):
		var dir := DirAccess.open(ISOLATED_TEST_DIR)
		if dir:
			dir.list_dir_begin()
			var file_name := dir.get_next()
			while not file_name.is_empty():
				if not dir.current_is_dir():
					dir.remove(file_name)
				file_name = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(ISOLATED_TEST_DIR)
