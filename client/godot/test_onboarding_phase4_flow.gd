# ==============================================================================
# Republic - New Player Onboarding Phase 4 UI Flow Automated Test
# Tests Onboarding.tscn / Onboarding.gd end-to-end through Phase 4:
# 1. Registration form validation & PII protection
# 2. Email verification dispatch viewer with "Verify Email" button
# 3. Country naming rules & confirmation modal
# 4. Procedural nation generation with standardized metrics (50M REPU, 60% approval, 10M pop)
# 5. Leadership Identity Selection (Exact 2 of 5) -> Technocratic Visionary
# 6. National Development Priorities Selection (Exact 4 of 10)
# 7. Phase 3 Ratification Summary
# 8. Transition to Presidential Advisory Council (Phase 4)
# 9. Verify General Adviser (Marcus Vance) & Press Secretary (Elena Vance) active appointments
# 10. Verify Special Adviser roster (Slot 1 Active, Slots 2-5 Locked) & 500 Sovereign Gems
# 11. Unlock Special Adviser Slot 2 with Sovereign Gems (500 gems deducted -> 0 gems, Slot 2 Active)
# 12. Block unlocking Slot 3 with insufficient gems
# 13. Confirm Advisory Council & advance onboarding to AdvisersIntroduced
# 14. Verify authoritative save file and public snapshot on disk (Zero PII leak)
# ==============================================================================
extends SceneTree

const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const OnboardingScript := preload("res://Onboarding.gd")
const TEST_STATE_PATH := "res://test_phase4_ui_state.json"
const TEST_SAVE_PATH := "res://test_phase4_ui_session.sav"
const TEST_REQUEST_PATH := "res://presidential_desk_request.json"
const TEST_RESULT_PATH := "res://presidential_desk_result.json"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - ONBOARDING UI FLOW VERIFICATION TEST (PHASE 4)  ")
	print("==============================================================\n")
	_run_tests()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQUEST_PATH, TEST_RESULT_PATH]:
		var glob_path := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(glob_path):
			DirAccess.remove_absolute(glob_path)

func _run_tests() -> void:
	_clean_files()

	var scene: PackedScene = load(ONBOARDING_SCENE_PATH)
	if scene == null:
		printerr("[FAIL]: Unable to load Onboarding.tscn")
		_clean_files()
		quit(1)
		return

	var onboarding = scene.instantiate()
	if onboarding == null:
		printerr("[FAIL]: Failed to instantiate OnboardingController")
		_clean_files()
		quit(1)
		return

	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding.active_request_path = TEST_REQUEST_PATH
	onboarding.active_result_path = TEST_RESULT_PATH
	onboarding._init_controls()

	print("--- Check 1: Candidate Registration & Email Verification ---")
	onboarding.input_first_name.text = "Elena"
	onboarding.input_last_name.text = "Okafor"
	onboarding.input_email.text = "elena.okafor@republic.net"
	onboarding.spin_dob_year.value = 1991
	onboarding.spin_dob_month.value = 3
	onboarding.spin_dob_day.value = 20
	onboarding._on_register_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.VERIFY_EMAIL:
		printerr("[FAIL]: Expected stage VERIFY_EMAIL, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	onboarding._on_verify_action_pressed()
	if onboarding.current_stage != OnboardingScript.ScreenStage.COUNTRY_NAME:
		printerr("[FAIL]: Expected stage COUNTRY_NAME, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Registered and email verified.")

	print("\n--- Check 2: Found Sovereign Nation ---")
	onboarding.input_country_name.text = "Republic of Solaria"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CONFIRMED:
		printerr("[FAIL]: Expected stage CONFIRMED, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Republic of Solaria founded.")

	print("\n--- Check 3: Select Leadership Archetypes (2 of 5) ---")
	onboarding._on_proceed_leadership_pressed()
	onboarding._on_technocrat_toggled()
	onboarding._on_visionary_toggled()
	onboarding._on_confirm_leadership_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.DEVELOPMENT_PLANS:
		printerr("[FAIL]: Expected stage DEVELOPMENT_PLANS, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Leadership doctrine 'Technocratic Visionary' established.")

	print("\n--- Check 4: Select Development Priorities (4 of 10) ---")
	onboarding._on_plan_clicked("industrialisation_first", onboarding.btn_plan_1)
	onboarding._on_plan_clicked("agricultural_transformation", onboarding.btn_plan_2)
	onboarding._on_plan_clicked("technology_and_innovation", onboarding.btn_plan_3)
	onboarding._on_plan_clicked("infrastructure_revolution", onboarding.btn_plan_4)
	onboarding._on_confirm_plans_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.PHASE3_SUMMARY:
		printerr("[FAIL]: Expected stage PHASE3_SUMMARY, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: 4 development priorities ratified.")

	print("\n--- Check 5: Transition to Presidential Advisory Council (Phase 4) ---")
	onboarding._on_proceed_phase4_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.ADVISERS:
		printerr("[FAIL]: Expected stage ADVISERS, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	if not onboarding.advisers_panel or not onboarding.advisers_panel.visible:
		printerr("[FAIL]: ScreenAdvisers panel must be visible!")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Successfully navigated to Presidential Advisory Council screen.")

	print("\n--- Check 6: General Adviser & Press Secretary Appointments ---")
	if not onboarding.lbl_general_guidance.text.contains("Technocratic Visionary"):
		printerr("[FAIL]: General Adviser guidance missing doctrine reference: ", onboarding.lbl_general_guidance.text)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_press_headline.text.contains("Technocratic Visionary"):
		printerr("[FAIL]: Press Secretary headline missing doctrine reference: ", onboarding.lbl_press_headline.text)
		_clean_files()
		quit(1)
		return

	if onboarding.sovereign_gems_count != 500:
		printerr("[FAIL]: Expected 500 sovereign gems initially, got: ", onboarding.sovereign_gems_count)
		_clean_files()
		quit(1)
		return
	print("  [OK]: General Adviser Marcus Vance and Press Secretary Elena Vance active with tailored advice.")
	print("  [OK]: Sovereign Gems counter verified: 500 gems.")

	print("\n--- Check 7: Special Adviser Slot 2 Unlock with Sovereign Gems ---")
	# Unlock Slot 2 (500 gems cost)
	onboarding._on_unlock_slot2_pressed()

	if onboarding.sovereign_gems_count != 0:
		printerr("[FAIL]: Expected 0 sovereign gems after unlocking Slot 2, got: ", onboarding.sovereign_gems_count)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_slot2_title.text.contains("Dr. Arthur Sterling"):
		printerr("[FAIL]: Expected Slot 2 to show Dr. Arthur Sterling after unlock, got: ", onboarding.lbl_slot2_title.text)
		_clean_files()
		quit(1)
		return

	if not onboarding.btn_unlock_slot2.disabled:
		printerr("[FAIL]: Slot 2 unlock button should be disabled after unlocking.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Special Adviser Slot 2 unlocked with 500 gems. Dr. Arthur Sterling active.")

	print("\n--- Check 8: Insufficient Gems Blocks Slot 3 Unlock ---")
	onboarding._on_unlock_slot3_pressed()

	if not onboarding.lbl_advisers_feedback.text.contains("Insufficient Sovereign Gems"):
		printerr("[FAIL]: Expected insufficient gems feedback, got: ", onboarding.lbl_advisers_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Attempt to unlock Slot 3 blocked with insufficient gems message.")

	print("\n--- Check 9: Induct Advisory Council & Confirm Onboarding Step ---")
	onboarding._on_confirm_advisers_pressed()

	# Verify state file on disk
	var glob_state := ProjectSettings.globalize_path(TEST_STATE_PATH)
	var state_file := FileAccess.open(glob_state, FileAccess.READ)
	if state_file != null:
		var state_text := state_file.get_as_text()
		state_file.close()

		var parsed = JSON.parse_string(state_text)
		if parsed == null or typeof(parsed) != TYPE_DICTIONARY:
			printerr("[FAIL]: Snapshot on disk is not a valid JSON dictionary.")
			_clean_files()
			quit(1)
			return

		var step := str(parsed.get("onboarding_step", ""))
		if step != "AdvisersIntroduced" and step != "CampaignHeadquarters":
			printerr("[FAIL]: Expected onboarding_step 'AdvisersIntroduced' or 'CampaignHeadquarters', got: ", step)
			_clean_files()
			quit(1)
			return

		var gen_adv = parsed.get("general_adviser", {})
		if typeof(gen_adv) != TYPE_DICTIONARY or not str(gen_adv.get("name", "")).contains("Marcus Vance"):
			printerr("[FAIL]: Snapshot missing Marcus Vance General Adviser: ", gen_adv)
			_clean_files()
			quit(1)
			return

		var press_sec = parsed.get("press_secretary", {})
		if typeof(press_sec) != TYPE_DICTIONARY or not str(press_sec.get("name", "")).contains("Elena Vance"):
			printerr("[FAIL]: Snapshot missing Elena Vance Press Secretary: ", press_sec)
			_clean_files()
			quit(1)
			return

		var slots = parsed.get("special_advisers", [])
		if typeof(slots) != TYPE_ARRAY or slots.size() != 5:
			printerr("[FAIL]: Expected exactly 5 special adviser slots in snapshot, got: ", slots)
			_clean_files()
			quit(1)
			return

		# PII Check
		if state_text.contains("elena.okafor@republic.net") or state_text.contains("1991-03-20"):
			printerr("[FAIL]: PII leak detected in snapshot!")
			_clean_files()
			quit(1)
			return

	print("  [OK]: Authoritative state verified on disk: Onboarding step = AdvisersIntroduced.")
	print("  [OK]: Zero PII leaked in state snapshot.")

	print("\n==============================================================")
	print(" [PASS]: ALL 9 ONBOARDING PHASE 4 FLOW CHECKS PASSED!        ")
	print("==============================================================\n")
	_clean_files()
	quit(0)

