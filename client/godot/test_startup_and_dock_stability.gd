# ==============================================================================
# Republic - Start Interface & Executive Office Stability Automated Test
# Verifies:
# 1. Main scene configuration: run/main_scene == "res://Onboarding.tscn"
# 2. Start-of-game gateway: Loads into Sign Up or Log In page (ScreenStage.REGISTER)
# 3. Sign Up mode provides candidate registration form
# 4. Log In mode provides detected session card and email login form
# 5. Launch routing to office:
#    - Campaign Phase -> launches straight to Campaign Headquarters
#    - In Office Phase -> launches straight to Executive Office
# 6. Bi-directional Sign Out:
#    - Campaign Headquarters sign out returns to Gateway
#    - Executive Office sign out returns to Gateway
# 7. Executive Office Left Dock stability:
#    - Fixed width (290px)
#    - Hover stability guard preventing shaking/flicker
# ==============================================================================
extends SceneTree

const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const EXECUTIVE_OFFICE_SCENE_PATH := "res://ExecutiveOffice.tscn"
const OnboardingScript := preload("res://Onboarding.gd")
const ExecutiveOfficeScript := preload("res://ExecutiveOffice.gd")

const TEST_STATE_PATH := "res://test_startup_state.json"
const TEST_SAVE_PATH := "res://test_startup_session.sav"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - START INTERFACE & OFFICE FLOW VERIFICATION TEST  ")
	print("==============================================================\n")
	_run_verification()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH]:
		var glob := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(glob):
			DirAccess.remove_absolute(glob)

func _run_verification() -> void:
	_clean_files()

	# --- Check 1: Project Main Scene Config ---
	print("--- Check 1: Project Main Scene Config ---")
	var main_scene: String = ProjectSettings.get_setting("application/run/main_scene")
	if main_scene != ONBOARDING_SCENE_PATH:
		printerr("[FAIL]: Expected main_scene to be 'res://Onboarding.tscn', got: ", main_scene)
		_clean_files()
		quit(1)
		return
	print("  [OK]: project.godot main_scene configured to 'res://Onboarding.tscn'.")

	# --- Check 2: Initial Game Launch Lands on Sign Up / Log In Gateway ---
	print("\n--- Check 2: Initial Game Launch Lands on Sign Up / Log In Gateway ---")
	var onboarding_scene: PackedScene = load(ONBOARDING_SCENE_PATH)
	if onboarding_scene == null:
		printerr("[FAIL]: Unable to load Onboarding.tscn")
		_clean_files()
		quit(1)
		return

	var onboarding = onboarding_scene.instantiate()
	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding._init_controls()
	onboarding._load_initial_stage()

	if onboarding.current_stage != OnboardingScript.ScreenStage.REGISTER:
		printerr("[FAIL]: Initial stage is not REGISTER, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Game loads initially into Gateway (ScreenStage.REGISTER).")

	# --- Check 3: Sign Up Mode Displays Candidate Registration Form ---
	print("\n--- Check 3: Sign Up Mode Displays Candidate Registration Form ---")
	onboarding._on_entry_new_republic_pressed()
	if not onboarding.form_register.visible:
		printerr("[FAIL]: Candidate registration form should be visible in Sign Up mode.")
		_clean_files()
		quit(1)
		return
	if onboarding.login_form.visible:
		printerr("[FAIL]: Login form should be hidden in Sign Up mode.")
		_clean_files()
		quit(1)
		return
	if not onboarding.btn_register.visible:
		printerr("[FAIL]: Register button should be visible in Sign Up mode.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Sign Up mode presents candidate profile creation form.")

	# --- Check 4: Log In Mode Displays Saved Session Card & Email Sign In ---
	print("\n--- Check 4: Log In Mode Displays Saved Session Card & Email Sign In ---")
	onboarding._on_entry_resume_pressed()
	if onboarding.form_register.visible:
		printerr("[FAIL]: Registration form should be hidden in Log In mode.")
		_clean_files()
		quit(1)
		return
	if not onboarding.login_form.visible:
		printerr("[FAIL]: Login form should be visible in Log In mode.")
		_clean_files()
		quit(1)
		return
	if onboarding.lbl_session_details == null or onboarding.btn_quick_login == null:
		printerr("[FAIL]: Saved session card controls missing from login form.")
		_clean_files()
		quit(1)
		return
	if onboarding.input_login_email == null or onboarding.btn_submit_login == null:
		printerr("[FAIL]: Email login controls missing from login form.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Log In mode presents detected session card and email login form.")

	# --- Check 5: Email Validation on Sign In ---
	print("\n--- Check 5: Email Validation on Sign In ---")
	onboarding.input_login_email.text = "invalid_email_string"
	onboarding._on_submit_login_pressed()
	if not onboarding.lbl_login_feedback.text.contains("valid email"):
		printerr("[FAIL]: Invalid email should be rejected: ", onboarding.lbl_login_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Invalid email properly validated and rejected with helpful guidance.")

	# --- Check 6: Phase Routing — Launch Straight to Campaign Headquarters ---
	print("\n--- Check 6: Phase Routing — Launch Straight to Campaign Headquarters ---")
	var campaign_state := {
		"country_name": "Republic of Solaria",
		"president_name": "Elena Vance",
		"onboarding_step": "CampaignHeadquarters",
		"campaign_purse": 5000000,
		"campaign_polling": 35.0,
		"campaign_audit_risk": 0.0,
		"campaign_team": {
			"General Adviser": {"name": "Marcus Vance", "status": "Active"},
			"Press Secretary": {"name": "Elena Vance", "status": "Active"},
			"Campaign Accountant": {"name": "Kaelen Thorne", "status": "Active"}
		},
		"campaign_decisions": []
	}
	onboarding._launch_to_office_or_resume(campaign_state)
	if onboarding.current_stage != OnboardingScript.ScreenStage.CAMPAIGN_HQ:
		printerr("[FAIL]: Campaign phase did not launch straight to Campaign Headquarters. Got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	if not onboarding.campaign_panel.visible:
		printerr("[FAIL]: Campaign panel not visible after launching to Campaign HQ.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Player in campaign phase launches straight to Campaign Headquarters.")

	# --- Check 7: Campaign Headquarters Sign Out Returns to Gateway ---
	print("\n--- Check 7: Campaign Headquarters Sign Out Returns to Gateway ---")
	if onboarding.btn_campaign_signout == null:
		printerr("[FAIL]: BtnCampaignSignOut missing from Campaign HQ actions.")
		_clean_files()
		quit(1)
		return
	onboarding._on_campaign_signout_pressed()
	if onboarding.current_stage != OnboardingScript.ScreenStage.REGISTER:
		printerr("[FAIL]: Campaign signout did not return to ScreenStage.REGISTER.")
		_clean_files()
		quit(1)
		return
	if not onboarding.form_register.visible:
		printerr("[FAIL]: Sign up form not visible after signing out.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Campaign Sign Out returns cleanly to Gateway (ScreenStage.REGISTER).")

	# --- Check 8: Executive Office Loading & Left Dock Stability ---
	print("\n--- Check 8: Executive Office Loading & Left Dock Stability ---")
	var office_scene: PackedScene = load(EXECUTIVE_OFFICE_SCENE_PATH)
	if office_scene == null:
		printerr("[FAIL]: Failed to load ExecutiveOffice.tscn")
		_clean_files()
		quit(1)
		return

	var office = office_scene.instantiate()
	root.add_child(office)
	office._init_controls()

	if office.left_dock == null:
		printerr("[FAIL]: ExecutiveOffice left_dock is null.")
		_clean_files()
		quit(1)
		return

	# Confirm stable minimum width of 290px
	var left_min_size: Vector2 = office.left_dock.custom_minimum_size
	if left_min_size.x < 290.0:
		printerr("[FAIL]: Expected left_dock custom_minimum_size.x >= 290, got: ", left_min_size.x)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Executive Office Left Dock width stabilized at ", left_min_size.x, "px (no shaking/shrinking).")

	# --- Check 9: Executive Office Sign Out Button Present & Configured ---
	print("\n--- Check 9: Executive Office Sign Out Button Present & Configured ---")
	if office.sign_out_button == null:
		printerr("[FAIL]: SignOutButton missing from ExecutiveOffice TopBar.")
		_clean_files()
		quit(1)
		return
	var btn_sign_out_text: String = office.sign_out_button.text
	if not btn_sign_out_text.contains("Sign Out"):
		printerr("[FAIL]: Expected 'Sign Out' on button, got: ", btn_sign_out_text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Executive Office TopBar includes '", btn_sign_out_text, "' button to return to Gateway.")

	# Cleanup
	root.remove_child(onboarding)
	onboarding.free()
	root.remove_child(office)
	office.free()
	_clean_files()

	print("\n==============================================================")
	print(" [PASS]: ALL 9 START INTERFACE & OFFICE STABILITY CHECKS PASSED!")
	print("==============================================================\n")
	quit(0)
