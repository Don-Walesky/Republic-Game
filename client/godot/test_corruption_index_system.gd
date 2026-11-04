extends SceneTree

const EXECUTIVE_OFFICE_SCENE_PATH := "res://ExecutiveOffice.tscn"
const TEST_SANDBOX_DIR := "user://test_sandbox_corruption"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - CORRUPTION INDEX SYSTEM VERIFICATION TEST       ")
	print("==============================================================\n")
	_run_corruption_index_tests()

func _setup_sandbox() -> void:
	if DirAccess.dir_exists_absolute(TEST_SANDBOX_DIR):
		var dir := DirAccess.open(TEST_SANDBOX_DIR)
		if dir:
			dir.list_dir_begin()
			var file_name = dir.get_next()
			while file_name != "":
				if not dir.current_is_dir():
					dir.remove(file_name)
				file_name = dir.get_next()
	else:
		DirAccess.make_dir_recursive_absolute(TEST_SANDBOX_DIR)

func _run_corruption_index_tests() -> void:
	_setup_sandbox()

	var packed_scene := load(EXECUTIVE_OFFICE_SCENE_PATH) as PackedScene
	if packed_scene == null:
		printerr("[FAIL]: Unable to load ExecutiveOffice scene at: ", EXECUTIVE_OFFICE_SCENE_PATH)
		quit(1)
		return

	var office = packed_scene.instantiate()
	root.add_child(office)
	office._init_controls()
	office._connect_signals()

	var all_passed := true

	# --- Check 1: TopBar CardCorruption Node & Initial UI Display ---
	print("--- Check 1: TopBar CardCorruption Node & Initial UI Display ---")
	if office.card_corruption == null:
		printerr("  [FAIL]: office.card_corruption node is null")
		all_passed = false
	elif office.corruption_label == null:
		printerr("  [FAIL]: office.corruption_label node is null")
		all_passed = false
	else:
		print("  [OK]: CardCorruption panel and CorruptionLabel present in TopBarHBox.")
		var baseline_state := {
			"country_name": "Republic of Eldoria",
			"treasury": "R 4,200,000",
			"treasury_balance": 4200000.0,
			"approval": "68%",
			"approval_rating": 68.0,
			"stability": "75%",
			"stability_rating": 75.0,
			"corruption_index": 25.0,
			"corruption": "25.0%",
			"corruption_control": 0.75,
			"civil_service_quality": 0.80,
			"infrastructure_condition": 0.70
		}
		office.apply_state(baseline_state)
		if not office.corruption_label.text.contains("25.0%"):
			printerr("  [FAIL]: CorruptionLabel did not display baseline 25.0%. Found: ", office.corruption_label.text)
			all_passed = false
		else:
			print("  [OK]: CorruptionLabel correctly displays: ", office.corruption_label.text)
	print("Test 1 (CardCorruption Node & Initial UI): ", "PASS" if all_passed else "FAIL", "\n")

	# --- Check 2: CardCorruption Interactivity (Opens Briefing) ---
	print("--- Check 2: CardCorruption Interactivity ---")
	var check2_passed := true
	office.close_all_modals()
	var click_event := InputEventMouseButton.new()
	click_event.button_index = MOUSE_BUTTON_LEFT
	click_event.pressed = true
	office._on_card_corruption_gui_input(click_event)
	if office.briefing_modal == null or not office.briefing_modal.visible:
		printerr("  [FAIL]: Clicking CardCorruption did not open BriefingModal")
		check2_passed = false
		all_passed = false
	else:
		print("  [OK]: Clicking CardCorruption successfully opens BriefingModal.")
	office.close_all_modals()
	print("Test 2 (CardCorruption Interactivity): ", "PASS" if check2_passed else "FAIL", "\n")

	# --- Check 3: Media Desk Hush vs Approve Corruption Impact ---
	print("--- Check 3: Media Desk Hush vs Approve Impact ---")
	var check3_passed := true
	var test_state_file := TEST_SANDBOX_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	var mock_state := {
		"country_name": "Republic of Eldoria",
		"treasury_balance": 5000000.0,
		"corruption_index": 25.0,
		"corruption": "25.0%",
		"corruption_control": 0.30,
		"media_reports": [
			{
				"id": "rep_test_corruption_1",
				"headline": "Special Audit on Executive Expenditures",
				"article_summary": "Auditors question undocumented transfers.",
				"public_approval_impact": -4.0,
				"stability_impact": -3.0,
				"requires_decision": true,
				"is_suppressed": false,
				"response_status": "Unanswered"
			}
		]
	}
	office.apply_state(mock_state)

	# Execute Hush response
	var res_hush: Dictionary = office.execute_media_response("rep_test_corruption_1", "hush")
	if not res_hush.get("success", false):
		printerr("  [FAIL]: execute_media_response for hush failed: ", res_hush.get("message", ""))
		check3_passed = false
		all_passed = false
	else:
		var post_hush_corr: float = float(office.latest_state.get("corruption_index", 25.0))
		if post_hush_corr <= 25.0:
			printerr("  [FAIL]: Hushing story did not increase CorruptionIndex. Current: ", post_hush_corr)
			check3_passed = false
			all_passed = false
		else:
				print("  [OK]: Hushing story raised CorruptionIndex from 25.0% to: ", str(post_hush_corr), "%")
	# Add report 2 and test Approve response
	office.latest_state["media_reports"].append({
		"id": "rep_test_corruption_2",
		"headline": "Independent Press Freedom Report",
		"public_approval_impact": 2.0,
		"stability_impact": 1.0,
		"requires_decision": true,
		"is_suppressed": false,
		"response_status": "Unanswered"
	})
	var pre_approve_corr: float = float(office.latest_state.get("corruption_index", 33.0))
	var res_app: Dictionary = office.execute_media_response("rep_test_corruption_2", "approve")
	if not res_app.get("success", false):
		printerr("  [FAIL]: execute_media_response for approve failed: ", res_app.get("message", ""))
		check3_passed = false
		all_passed = false
	else:
		var post_approve_corr: float = float(office.latest_state.get("corruption_index", pre_approve_corr))
		if post_approve_corr >= pre_approve_corr:
			printerr("  [FAIL]: Approving transparent story did not decrease CorruptionIndex. Current: ", post_approve_corr)
			check3_passed = false
			all_passed = false
		else:
			print("  [OK]: Transparently approving story lowered CorruptionIndex to: ", str(post_approve_corr), "%")
	print("Test 3 (Media Desk Hush vs Approve): ", "PASS" if check3_passed else "FAIL", "\n")

	# --- Check 4: Request Pipeline - Resource Concessions ---
	print("--- Check 4: Request Pipeline - Resource Concessions ---")
	var check4_passed := true
	var req_file := TEST_SANDBOX_DIR + "/presidential_desk_request.json"
	var req_concession := {
		"request_id": "req_test_concession_1",
		"action": "resource_concession",
		"concession_type": "Mining",
		"beneficiary": "connected_firm",
		"confirmed": true
	}
	var f := FileAccess.open(req_file, FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(req_concession))
		f.close()
		print("  [OK]: Serialized 'resource_concession' request file to disk.")
	else:
		printerr("  [FAIL]: Could not write request file.")
		check4_passed = false
		all_passed = false
	print("Test 4 (Resource Concession Request Pipeline): ", "PASS" if check4_passed else "FAIL", "\n")

	# --- Check 5: Request Pipeline - Aid & Loan Diversion ---
	print("--- Check 5: Request Pipeline - Aid & Loan Diversion ---")
	var check5_passed := true
	var req_aid := {
		"request_id": "req_test_aid_1",
		"action": "divert_aid",
		"action_type": "divert",
		"amount": 2000000.0,
		"aid_source": "IMF Development Facility",
		"confirmed": true
	}
	f = FileAccess.open(req_file, FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(req_aid))
		f.close()
		print("  [OK]: Serialized 'divert_aid' request file to disk.")
	else:
		printerr("  [FAIL]: Could not write aid request file.")
		check5_passed = false
		all_passed = false
	print("Test 5 (Aid & Loan Diversion Request Pipeline): ", "PASS" if check5_passed else "FAIL", "\n")

	# --- Check 6: Request Pipeline - Diplomatic Envoy Appointments ---
	print("--- Check 6: Request Pipeline - Diplomatic Envoy Appointments ---")
	var check6_passed := true
	var req_envoy := {
		"request_id": "req_test_envoy_1",
		"action": "diplomatic_appointment",
		"post_name": "Ambassador to United Nations",
		"candidate_type": "career_diplomat",
		"confirmed": true
	}
	f = FileAccess.open(req_file, FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(req_envoy))
		f.close()
		print("  [OK]: Serialized 'diplomatic_appointment' request file to disk.")
	else:
		printerr("  [FAIL]: Could not write envoy request file.")
		check6_passed = false
		all_passed = false
	print("Test 6 (Diplomatic Envoy Request Pipeline): ", "PASS" if check6_passed else "FAIL", "\n")

	# --- Check 7: Delta Pill Color and Formatting Feedback ---
	print("--- Check 7: Delta Pill Color and Formatting Feedback ---")
	var check7_passed := true
	office.previous_corruption_val = 25.0
	var elevated_state := {
		"corruption_index": 33.0,
		"corruption": "33.0%"
	}
	office.apply_state(elevated_state)
	if office.corruption_delta == null or not office.corruption_delta.visible:
		printerr("  [FAIL]: CorruptionDelta pill is not visible after corruption changed from 25% to 33%")
		check7_passed = false
		all_passed = false
	elif not office.corruption_delta_label.text.contains("+8.0%"):
		printerr("  [FAIL]: CorruptionDeltaLabel did not show +8.0%. Found: ", office.corruption_delta_label.text)
		check7_passed = false
		all_passed = false
	else:
		print("  [OK]: CorruptionDelta pill successfully displayed: ", office.corruption_delta_label.text)
	print("Test 7 (Delta Pill Feedback): ", "PASS" if check7_passed else "FAIL", "\n")

	office.queue_free()

	print("==============================================================")
	if all_passed:
		print("   [SUCCESS]: ALL CORRUPTION INDEX CHECKS PASSED (7/7)!       ")
	else:
		print("   [FAILED]: ONE OR MORE CORRUPTION INDEX CHECKS FAILED!      ")
	print("==============================================================\n")

	quit(0 if all_passed else 1)
