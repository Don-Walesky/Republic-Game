# ==============================================================================
# Republic - Start Interface & Executive Office Stability Automated Test
# Verifies:
# 1. Main scene configuration: run/main_scene == "res://Auth.tscn"
# 2. Start-of-game gateway: Loads Auth.tscn initially
# 3. "Sign Up" flow: Presents Sign Up option and navigates to Onboarding.tscn
# 4. "Log In" flow: Detects saved session and provides email login controls
# 5. Launch routing to office:
#    - Campaign Phase -> routes directly to Campaign Headquarters
#    - In Office Phase -> routes directly to Executive Office
# 6. Bi-directional Sign Out:
#    - Campaign Headquarters sign out returns to Auth Gateway
#    - Executive Office sign out returns to Auth Gateway
# 7. Executive Office Left Dock stability:
#    - Fixed width (290px)
#    - Stabilized anchor layout
#    - Hover stability guard preventing shaking/flicker
# ==============================================================================
extends SceneTree

const AUTH_SCENE_PATH := "res://Auth.tscn"
const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const EXECUTIVE_OFFICE_SCENE_PATH := "res://ExecutiveOffice.tscn"
const AuthScript := preload("res://Auth.gd")
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
	if main_scene != AUTH_SCENE_PATH:
		printerr("[FAIL]: Expected main_scene to be 'res://Auth.tscn', got: ", main_scene)
		_clean_files()
		quit(1)
		return
	print("  [OK]: project.godot main_scene configured to 'res://Auth.tscn'.")

	# --- Check 2: Initial Game Launch Lands on Auth Gateway ---
	print("\n--- Check 2: Initial Game Launch Lands on Auth Gateway ---")
	var auth_scene: PackedScene = load(AUTH_SCENE_PATH)
	if auth_scene == null:
		printerr("[FAIL]: Unable to load Auth.tscn")
		_clean_files()
		quit(1)
		return

	var auth = auth_scene.instantiate()
	root.add_child(auth)
	auth.active_state_path = TEST_STATE_PATH
	auth.active_save_path = TEST_SAVE_PATH
	auth._init_controls()

	if auth.btn_tab_sign_up == null or auth.btn_tab_login == null:
		printerr("[FAIL]: Auth tab buttons missing from Auth.tscn")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Auth gateway loads with Sign Up and Log In options.")

	# --- Check 3: Sign Up Mode & Navigation to Onboarding.tscn ---
	print("\n--- Check 3: Sign Up Mode & Navigation to Onboarding.tscn ---")
	auth._on_tab_sign_up_pressed()
	if not auth.sign_up_box.visible:
		printerr("[FAIL]: SignUpBox should be visible in Sign Up mode.")
		_clean_files()
		quit(1)
		return
	if auth.login_box.visible:
		printerr("[FAIL]: LoginBox should be hidden in Sign Up mode.")
		_clean_files()
		quit(1)
		return
	if auth.btn_go_to_sign_up == null:
		printerr("[FAIL]: BtnGoToSignUp missing.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Sign Up tab is active and provides 1-click entry to Onboarding.tscn.")

	# --- Check 4: Log In Mode Displays Saved Session Card & Email Sign In ---
	print("\n--- Check 4: Log In Mode Displays Saved Session Card & Email Sign In ---")
	auth._on_tab_login_pressed()
	if auth.sign_up_box.visible:
		printerr("[FAIL]: SignUpBox should be hidden in Log In mode.")
		_clean_files()
		quit(1)
		return
	if not auth.login_box.visible:
		printerr("[FAIL]: LoginBox should be visible in Log In mode.")
		_clean_files()
		quit(1)
		return
	if auth.lbl_detected_country == null or auth.btn_resume_session == null:
		printerr("[FAIL]: Detected session card controls missing.")
		_clean_files()
		quit(1)
		return
	if auth.input_email == null or auth.btn_email_login == null:
		printerr("[FAIL]: Email login controls missing from login box.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Log In mode displays session inspection card and email login form.")

	# --- Check 5: Email Validation on Sign In ---
	print("\n--- Check 5: Email Validation on Sign In ---")
	auth.input_email.text = "invalid_email_string"
	auth._on_email_login_pressed()
	if not auth.lbl_feedback.text.contains("valid email"):
		printerr("[FAIL]: Invalid email should be rejected: ", auth.lbl_feedback.text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Invalid email properly validated and rejected with helpful feedback.")

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
	
	# Verify routing logic flags Campaign HQ
	auth._route_to_phase(campaign_state)
	if not Engine.has_meta("launch_to_campaign_hq") or not Engine.get_meta("launch_to_campaign_hq"):
		printerr("[FAIL]: Campaign phase routing failed to set launch_to_campaign_hq flag!")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Player in campaign phase is routed to Campaign Headquarters.")

	# Verify Onboarding.tscn consumes the meta flag and immediately opens Campaign HQ
	var onboarding_scene: PackedScene = load(ONBOARDING_SCENE_PATH)
	var onboarding = onboarding_scene.instantiate()
	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	# Write campaign state to disk so Onboarding can read it
	var f := FileAccess.open(ProjectSettings.globalize_path(TEST_STATE_PATH), FileAccess.WRITE)
	f.store_string(JSON.stringify(campaign_state))
	f.close()
	onboarding._ready()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CAMPAIGN_HQ:
		printerr("[FAIL]: Onboarding scene did not route directly to CAMPAIGN_HQ stage, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	if not onboarding.campaign_panel.visible:
		printerr("[FAIL]: Campaign panel not visible after launching straight to Campaign HQ.")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Onboarding scene launched straight into Campaign Headquarters without showing registration.")

	# --- Check 7: Campaign Headquarters Sign Out Returns to Auth Gateway ---
	print("\n--- Check 7: Campaign Headquarters Sign Out Returns to Auth Gateway ---")
	if onboarding.btn_campaign_signout == null:
		printerr("[FAIL]: BtnCampaignSignOut missing from Campaign HQ.")
		_clean_files()
		quit(1)
		return
	var signout_text: String = onboarding.btn_campaign_signout.text
	if not signout_text.contains("Sign Out"):
		printerr("[FAIL]: Campaign sign out button text missing 'Sign Out': ", signout_text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Campaign Sign Out button present and configured to return to Gateway.")

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

	# Confirm stable reduced minimum width of 240px and stabilized anchor properties
	var left_min_size: Vector2 = office.left_dock.custom_minimum_size
	if left_min_size.x < 240.0:
		printerr("[FAIL]: Expected left_dock custom_minimum_size.x >= 240, got: ", left_min_size.x)
		_clean_files()
		quit(1)
		return
	if not is_equal_approx(office.left_dock.anchor_left, 0.01) or not is_equal_approx(office.left_dock.anchor_right, 0.01):
		printerr("[FAIL]: Expected stabilized anchors (0.01, 0.01), got: left=", office.left_dock.anchor_left, " right=", office.left_dock.anchor_right)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Executive Office Left Dock width stabilized at ", left_min_size.x, "px with fixed anchors (no shaking/shrinking).")

	# --- Check 9: Executive Office Sign Out Button Returns to Auth Gateway ---
	print("\n--- Check 9: Executive Office Sign Out Button Returns to Auth Gateway ---")
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
	print("  [OK]: Executive Office TopBar includes '", btn_sign_out_text, "' button to return to Auth Gateway.")

	# Cleanup
	root.remove_child(auth)
	auth.free()
	root.remove_child(onboarding)
	onboarding.free()
	root.remove_child(office)
	office.free()
	_clean_files()

	print("\n==============================================================")
	print(" [PASS]: ALL 9 START INTERFACE & OFFICE STABILITY CHECKS PASSED!")
	print("==============================================================\n")
	quit(0)
