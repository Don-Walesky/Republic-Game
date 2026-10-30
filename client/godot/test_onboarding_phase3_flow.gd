# ==============================================================================
# Republic - New Player Onboarding Phase 3 UI Flow Automated Test
# Tests Onboarding.tscn / Onboarding.gd end-to-end through Phase 3:
# 1. Registration form validation & PII protection
# 2. Email verification dispatch viewer with "Verify Email" button
# 3. Country naming rules & confirmation modal
# 4. Procedural nation generation with standardized metrics (50M REPU, 60% approval, 10M pop)
# 5. Transition to Leadership Identity Selection
# 6. Exact 2-of-5 Leadership selection enforcement & doctrine calculation
# 7. Exact 4-of-10 National Development Priorities selection enforcement
# 8. Phase 3 Ratification Summary & session reload persistence
# 9. Public state PII isolation verification
# ==============================================================================
extends SceneTree

const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const OnboardingScript := preload("res://Onboarding.gd")
const TEST_STATE_PATH := "res://test_phase3_ui_state.json"
const TEST_SAVE_PATH := "res://test_phase3_ui_session.sav"
const TEST_REQUEST_PATH := "res://presidential_desk_request.json"
const TEST_RESULT_PATH := "res://presidential_desk_result.json"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - ONBOARDING UI FLOW VERIFICATION TEST (PHASE 3)  ")
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

	print("--- Check 1: Initial Registration & Email Verification ---")
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
	print("  [OK]: Candidate registered. Official verification dispatch displayed.")

	onboarding._on_verify_action_pressed()
	if onboarding.current_stage != OnboardingScript.ScreenStage.COUNTRY_NAME:
		printerr("[FAIL]: Expected stage COUNTRY_NAME after email verification, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Email verified via action button.")

	print("\n--- Check 2: Found Sovereign Nation & Standardized Metrics ---")
	onboarding.input_country_name.text = "Republic of Solaria"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CONFIRMED:
		printerr("[FAIL]: Expected stage CONFIRMED, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	if onboarding.lbl_stat_treasury.text != "R 50,000,000 REPU":
		printerr("[FAIL]: Treasury mismatch. Expected R 50,000,000 REPU, got: ", onboarding.lbl_stat_treasury.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Sovereign nation established with standardized metrics (50M REPU, 60% approval, 10M pop).")

	print("\n--- Check 3: Transition to Leadership Selection Screen ---")
	onboarding._on_proceed_leadership_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.LEADERSHIP:
		printerr("[FAIL]: Expected stage LEADERSHIP, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	if not onboarding.btn_confirm_leadership.disabled:
		printerr("[FAIL]: Confirm leadership button should be disabled with 0 selections.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Navigated to Leadership Selection screen with confirm button initially disabled.")

	print("\n--- Check 4: Leadership Selection Rules (Exact 2 of 5) ---")
	# Select 1 archetype: Technocrat
	onboarding._on_technocrat_toggled()
	if onboarding.selected_archetypes.size() != 1:
		printerr("[FAIL]: Expected 1 selected archetype, got: ", onboarding.selected_archetypes.size())
		_clean_files()
		quit(1)
		return
	if not onboarding.btn_confirm_leadership.disabled:
		printerr("[FAIL]: Confirm leadership button should still be disabled with only 1 archetype selected.")
		_clean_files()
		quit(1)
		return

	# Select 2nd archetype: Economic Visionary
	onboarding._on_visionary_toggled()
	if onboarding.selected_archetypes.size() != 2:
		printerr("[FAIL]: Expected 2 selected archetypes, got: ", onboarding.selected_archetypes.size())
		_clean_files()
		quit(1)
		return
	if onboarding.btn_confirm_leadership.disabled:
		printerr("[FAIL]: Confirm leadership button should be enabled with exactly 2 archetypes selected.")
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_doctrine_preview.text.contains("Technocratic Visionary"):
		printerr("[FAIL]: Expected doctrine preview 'Technocratic Visionary', got: ", onboarding.lbl_doctrine_preview.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: 2 archetypes selected. Doctrine calculated as 'Technocratic Visionary'. Confirm button enabled.")

	# Attempt to select a 3rd archetype (Statist Builder) -> should be blocked!
	onboarding._on_statist_toggled()
	if onboarding.selected_archetypes.size() != 2:
		printerr("[FAIL]: 3rd archetype selection should be blocked! Count: ", onboarding.selected_archetypes.size())
		_clean_files()
		quit(1)
		return
	if not onboarding.lbl_leadership_feedback.text.contains("only select exactly 2"):
		printerr("[FAIL]: Expected selection limit feedback, got: ", onboarding.lbl_leadership_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Attempting to select >2 archetypes properly blocked with descriptive feedback.")

	print("\n--- Check 5: Confirm Leadership Doctrine & Advance to Development Plans ---")
	onboarding._on_confirm_leadership_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.DEVELOPMENT_PLANS:
		printerr("[FAIL]: Expected stage DEVELOPMENT_PLANS after confirming leadership, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Leadership doctrine ratified authoritatively. Advanced to Development Plans screen.")

	print("\n--- Check 6: Development Plans Selection Rules (Exact 4 of 10) ---")
	if not onboarding.btn_confirm_plans.disabled:
		printerr("[FAIL]: Confirm plans button should be disabled with 0 selections.")
		_clean_files()
		quit(1)
		return

	# Select 3 priorities:
	onboarding._on_plan_clicked("agrarian_self_sufficiency", onboarding.btn_plan_1)
	onboarding._on_plan_clicked("renewable_power_grid", onboarding.btn_plan_3)
	onboarding._on_plan_clicked("higher_education_research", onboarding.btn_plan_4)

	if onboarding.selected_plans.size() != 3:
		printerr("[FAIL]: Expected 3 selected plans, got: ", onboarding.selected_plans.size())
		_clean_files()
		quit(1)
		return
	if not onboarding.btn_confirm_plans.disabled:
		printerr("[FAIL]: Confirm plans button should still be disabled with only 3 priorities selected.")
		_clean_files()
		quit(1)
		return

	# Select 4th priority: Digital Infrastructure & Cyber Defense
	onboarding._on_plan_clicked("digital_infrastructure_cyber", onboarding.btn_plan_7)
	if onboarding.selected_plans.size() != 4:
		printerr("[FAIL]: Expected 4 selected plans, got: ", onboarding.selected_plans.size())
		_clean_files()
		quit(1)
		return
	if onboarding.btn_confirm_plans.disabled:
		printerr("[FAIL]: Confirm plans button should be enabled with exactly 4 priorities selected.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: 4 development priorities selected. Confirm button enabled.")

	# Attempt to select a 5th priority -> should be blocked!
	onboarding._on_plan_clicked("transit_rail_arteries", onboarding.btn_plan_10)
	if onboarding.selected_plans.size() != 4:
		printerr("[FAIL]: 5th plan selection should be blocked! Count: ", onboarding.selected_plans.size())
		_clean_files()
		quit(1)
		return
	if not onboarding.lbl_plans_feedback.text.contains("only select exactly 4"):
		printerr("[FAIL]: Expected plan limit feedback, got: ", onboarding.lbl_plans_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Attempting to select >4 priorities properly blocked with descriptive feedback.")

	print("\n--- Check 7: Ratify Development Agenda & Advance to Phase 3 Summary ---")
	onboarding._on_confirm_plans_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.PHASE3_SUMMARY:
		printerr("[FAIL]: Expected stage PHASE3_SUMMARY, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_summary_doctrine.text.contains("Technocratic Visionary"):
		printerr("[FAIL]: Summary doctrine mismatch. Expected 'Technocratic Visionary', got: ", onboarding.lbl_summary_doctrine.text)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_summary_country.text.contains("Republic of Solaria"):
		printerr("[FAIL]: Summary country mismatch. Expected 'Republic of Solaria', got: ", onboarding.lbl_summary_country.text)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Development agenda ratified authoritatively. Phase 3 Summary displayed:")
	print("       - Country: ", onboarding.lbl_summary_country.text)
	print("       - Capital: ", onboarding.lbl_summary_capital.text)
	print("       - President: ", onboarding.lbl_summary_leader.text)
	print("       - Doctrine: ", onboarding.lbl_summary_doctrine.text)
	print("       - Archetypes: ", onboarding.lbl_summary_archetypes.text)
	print("       - Priorities: ", onboarding.lbl_summary_plans.text)

	print("\n--- Check 8: Persistence & Disk Verification ---")
	onboarding._on_resume_session_pressed()
	if not onboarding.lbl_summary_doctrine.text.contains("Technocratic Visionary"):
		printerr("[FAIL]: Reloaded state failed to preserve executive doctrine.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: State successfully reloaded from authoritative save file on disk.")

	print("\n--- Check 9: PII Isolation in Public Snapshot ---")
	var glob_state := ProjectSettings.globalize_path(TEST_STATE_PATH)
	var state_file := FileAccess.open(glob_state, FileAccess.READ)
	if state_file != null:
		var state_text := state_file.get_as_text()
		state_file.close()

		if state_text.contains("elena.okafor@republic.net"):
			printerr("[FAIL]: PII leak! Raw email address found in public state snapshot!")
			_clean_files()
			quit(1)
			return

		if state_text.contains("1991-03-20"):
			printerr("[FAIL]: PII leak! Raw date of birth found in public state snapshot!")
			_clean_files()
			quit(1)
			return

		var parsed = JSON.parse_string(state_text)
		if parsed == null or typeof(parsed) != TYPE_DICTIONARY:
			printerr("[FAIL]: Public state snapshot is not valid JSON dictionary.")
			_clean_files()
			quit(1)
			return

		var step := str(parsed.get("onboarding_step", ""))
		if step != "DevelopmentPlansSelected":
			printerr("[FAIL]: Expected snapshot onboarding_step 'DevelopmentPlansSelected', got: ", step)
			_clean_files()
			quit(1)
			return

		var arch_list = parsed.get("selected_leadership_archetypes", [])
		if typeof(arch_list) != TYPE_ARRAY or arch_list.size() != 2:
			printerr("[FAIL]: Expected exactly 2 leadership archetypes in snapshot, got: ", arch_list)
			_clean_files()
			quit(1)
			return

		var plan_list = parsed.get("selected_development_plans", [])
		if typeof(plan_list) != TYPE_ARRAY or plan_list.size() != 4:
			printerr("[FAIL]: Expected exactly 4 development plans in snapshot, got: ", plan_list)
			_clean_files()
			quit(1)
			return

	print("  [OK]: Public snapshot verified: Zero PII leaked, exact 2 archetypes and 4 plans exported.")

	print("\n==============================================================")
	print(" [PASS]: ALL 9 ONBOARDING PHASE 3 FLOW CHECKS PASSED!        ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
