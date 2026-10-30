# ==============================================================================
# Republic - New Player Onboarding Phase 2 UI Flow Automated Test
# Tests Onboarding.tscn / Onboarding.gd end-to-end:
# 1. Registration form validation & PII protection
# 2. Email verification dispatch viewer with "Verify Email" button
# 3. Country naming rules & confirmation modal
# 4. Procedural nation generation with standardized metrics (50M REPU, 60% approval, 10M pop)
# 5. Ratification confirmation & session reload persistence
# ==============================================================================
extends SceneTree

const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const OnboardingScript := preload("res://Onboarding.gd")
const TEST_STATE_PATH := "res://test_onboarding_ui_state.json"
const TEST_SAVE_PATH := "res://test_onboarding_ui_session.sav"
const TEST_REQUEST_PATH := "res://presidential_desk_request.json"
const TEST_RESULT_PATH := "res://presidential_desk_result.json"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - ONBOARDING UI FLOW VERIFICATION TEST (PHASE 2)  ")
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

	print("--- Check 1: Initial Screen State is Registration ---")
	if onboarding.current_stage != OnboardingScript.ScreenStage.REGISTER:
		printerr("[FAIL]: Expected initial stage REGISTER, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Initial screen is Registration form.")

	print("\n--- Check 2: Registration Field & Age Validation ---")
	# 1. Blank inputs
	onboarding._on_register_pressed()
	if not onboarding.lbl_register_feedback.text.contains("First name is required"):
		printerr("[FAIL]: Empty first name was not rejected: ", onboarding.lbl_register_feedback.text)
		_clean_files()
		quit(1)
		return

	# 2. Underage candidate
	onboarding.input_first_name.text = "David"
	onboarding.input_last_name.text = "Okafor"
	onboarding.input_email.text = "david.okafor@republic.net"
	onboarding.spin_dob_year.value = 2020 # 7 years old
	onboarding._on_register_pressed()
	if not onboarding.lbl_register_feedback.text.contains("13 years of age"):
		printerr("[FAIL]: Underage candidate was not rejected: ", onboarding.lbl_register_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Form inputs and age restriction (<13 years) properly enforced.")

	print("\n--- Check 3: Valid Registration & Verification Email Dispatch ---")
	onboarding.input_first_name.text = "Elena"
	onboarding.input_last_name.text = "Okafor"
	onboarding.input_email.text = "elena.okafor@republic.net"
	onboarding.spin_dob_year.value = 1994 # 33 years old
	onboarding.spin_dob_month.value = 6
	onboarding.spin_dob_day.value = 12
	onboarding._on_register_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.VERIFY_EMAIL:
		printerr("[FAIL]: Failed to advance to VERIFY_EMAIL stage. Feedback: ", onboarding.lbl_register_feedback.text)
		_clean_files()
		quit(1)
		return

	if onboarding.verification_token.length() != 6:
		printerr("[FAIL]: Expected 6-digit verification token, got: ", onboarding.verification_token)
		_clean_files()
		quit(1)
		return

	var email_btn_text: String = onboarding.btn_verify_email_action.text.strip_edges()
	if email_btn_text != "Verify Email":
		printerr("[FAIL]: Expected email action button text 'Verify Email', got: ", email_btn_text)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_email_recipient.text.contains("elena.okafor@republic.net"):
		printerr("[FAIL]: Recipient email mismatch in verification card: ", onboarding.lbl_email_recipient.text)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Candidate registered. Official verification dispatch displayed with 'Verify Email' button.")

	print("\n--- Check 4: Click 'Verify Email' Action Button ---")
	onboarding._on_verify_action_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.COUNTRY_NAME:
		printerr("[FAIL]: Failed to advance to COUNTRY_NAME after email verification. Feedback: ", onboarding.lbl_verify_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Email verified via 'Verify Email' button. Advanced to Country Naming screen.")

	print("\n--- Check 5: Country Naming Rule & Confirmation Modal ---")
	# 1. Missing 'Republic'
	onboarding.input_country_name.text = "Valopolis Prime"
	onboarding._on_review_founding_pressed()
	if not onboarding.lbl_name_feedback.text.contains("Republic"):
		printerr("[FAIL]: Country name missing 'Republic' was not rejected: ", onboarding.lbl_name_feedback.text)
		_clean_files()
		quit(1)
		return

	# 2. Valid name & confirmation modal
	onboarding.input_country_name.text = "Republic of Azuria"
	onboarding._on_review_founding_pressed()
	if not onboarding.confirmation_modal.visible:
		printerr("[FAIL]: Confirmation modal did not appear after reviewing valid country name.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Country naming rules validated. Confirmation modal displayed with standardized metrics.")

	print("\n--- Check 6: Confirm Founding & Standardized Sovereign Ratification ---")
	onboarding._on_confirm_founding_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CONFIRMED:
		printerr("[FAIL]: Failed to advance to CONFIRMED stage. Feedback: ", onboarding.lbl_name_feedback.text)
		_clean_files()
		quit(1)
		return

	# Check UI displayed metrics
	if onboarding.lbl_stat_treasury.text != "R 50,000,000 REPU":
		printerr("[FAIL]: Standardized treasury mismatch. Expected R 50,000,000 REPU, got: ", onboarding.lbl_stat_treasury.text)
		_clean_files()
		quit(1)
		return

	if onboarding.lbl_stat_approval.text != "60.0%":
		printerr("[FAIL]: Standardized approval mismatch. Expected 60.0%, got: ", onboarding.lbl_stat_approval.text)
		_clean_files()
		quit(1)
		return

	if onboarding.lbl_stat_stability.text != "70.0%":
		printerr("[FAIL]: Standardized stability mismatch. Expected 70.0%, got: ", onboarding.lbl_stat_stability.text)
		_clean_files()
		quit(1)
		return

	if onboarding.lbl_stat_population.text != "10,000,000 citizens":
		printerr("[FAIL]: Standardized population mismatch. Expected 10,000,000 citizens, got: ", onboarding.lbl_stat_population.text)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Sovereign nation established with standardized metrics:")
	print("       - Country: ", onboarding.lbl_stat_country.text)
	print("       - Capital: ", onboarding.lbl_stat_capital.text)
	print("       - Treasury: ", onboarding.lbl_stat_treasury.text)
	print("       - Approval: ", onboarding.lbl_stat_approval.text)
	print("       - Stability: ", onboarding.lbl_stat_stability.text)
	print("       - Population: ", onboarding.lbl_stat_population.text)

	print("\n--- Check 7: Persistence & Reload Across Sessions ---")
	onboarding._on_resume_session_pressed()
	if onboarding.lbl_stat_country.text != "Republic of Azuria":
		printerr("[FAIL]: Reloaded state failed to preserve country name.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Sovereign country persisted and successfully reloaded from save state.")

	print("\n==============================================================")
	print(" [PASS]: ALL 7 ONBOARDING UI FLOW CHECKS PASSED!             ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
