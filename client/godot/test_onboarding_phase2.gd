# ==============================================================================
# Republic - New Player Onboarding Phase 2 Verification Test
# Tests account registration, age verification, email verification tokens,
# procedural country generation, duplicate protection, and PII isolation.
# ==============================================================================
extends SceneTree

const DESK_SCENE_PATH := "res://PresidentialDesk.tscn"
const TEST_STATE_PATH := "res://test_onboarding_state.json"
const TEST_SAVE_PATH := "res://test_onboarding_session.sav"
const TEST_REQUEST_PATH := "res://presidential_desk_request.json"
const TEST_RESULT_PATH := "res://presidential_desk_result.json"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - NEW PLAYER ONBOARDING PHASE 2 TEST             ")
	print("==============================================================\n")
	_run_tests()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQUEST_PATH, TEST_RESULT_PATH]:
		var glob_path := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(glob_path):
			DirAccess.remove_absolute(glob_path)

func _run_tests() -> void:
	_clean_files()

	var desk_scene: PackedScene = load(DESK_SCENE_PATH)
	if desk_scene == null:
		printerr("[FAIL]: Unable to load PresidentialDesk.tscn")
		_clean_files()
		quit(1)
		return

	var desk = desk_scene.instantiate()
	root.add_child(desk)

	print("--- Check 1: Account Registration Validation Rejections ---")
	# 1. Test Underage Date of Birth (< 13 years old as of 2027)
	var underage_req := {
		"action": "register_account",
		"first_name": "Young",
		"last_name": "Player",
		"email": "young@republic.net",
		"date_of_birth": "2020-01-01",
		"request_id": "test_underage"
	}
	_write_request(underage_req)
	var underage_result = _run_bridge_sync()
	if underage_result.get("success", false) or underage_result.get("result", "") != "ValidationFailed":
		printerr("[FAIL]: Underage registration was not rejected. Result: ", underage_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Underage registration (<13 years) successfully rejected: " + str(underage_result.get("message", "")))

	# 2. Test Invalid Email
	var bad_email_req := {
		"action": "register_account",
		"first_name": "David",
		"last_name": "Vance",
		"email": "not-an-email",
		"date_of_birth": "1995-05-15",
		"request_id": "test_bad_email"
	}
	_write_request(bad_email_req)
	var bad_email_result = _run_bridge_sync()
	if bad_email_result.get("success", false) or bad_email_result.get("result", "") != "ValidationFailed":
		printerr("[FAIL]: Invalid email registration was not rejected. Result: ", bad_email_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Invalid email format successfully rejected: " + str(bad_email_result.get("message", "")))

	print("\n--- Check 2: Successful Registration & Verification Token Generation ---")
	var valid_reg_req := {
		"action": "register_account",
		"first_name": "Elena",
		"last_name": "Okafor",
		"email": "elena.okafor@republic.net",
		"date_of_birth": "1994-06-12",
		"request_id": "test_valid_reg"
	}
	_write_request(valid_reg_req)
	var reg_result = _run_bridge_sync()
	if not reg_result.get("success", false) or reg_result.get("result", "") != "Success":
		printerr("[FAIL]: Valid registration failed. Result: ", reg_result)
		_clean_files()
		quit(1)
		return

	var token: String = str(reg_result.get("verification_token", "")).strip_edges()
	if token.length() != 6:
		printerr("[FAIL]: Expected 6-digit verification token, got: ", token)
		_clean_files()
		quit(1)
		return

	if reg_result.get("onboarding_step", "") != "AccountRegistered" or reg_result.get("is_email_verified", true):
		printerr("[FAIL]: Onboarding step mismatch. Expected AccountRegistered/Unverified, got: ", reg_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Registration successful. Generated token: %s, Step: %s" % [token, reg_result.get("onboarding_step", "")])

	print("\n--- Check 3: Enforcing Email Verification Precondition for Country Founding ---")
	var unverified_found_req := {
		"action": "create_country",
		"country_name": "Republic of Azuria",
		"request_id": "test_unverified_found"
	}
	_write_request(unverified_found_req)
	var unverified_found_result = _run_bridge_sync()
	if unverified_found_result.get("success", false) or unverified_found_result.get("result", "") != "EmailNotVerified":
		printerr("[FAIL]: Premature country creation without email verification was not rejected. Result: ", unverified_found_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Country creation blocked while email is unverified: " + str(unverified_found_result.get("message", "")))

	print("\n--- Check 4: Email Verification Success ---")
	var verify_req := {
		"action": "verify_email",
		"token": token,
		"request_id": "test_verify_email"
	}
	_write_request(verify_req)
	var verify_result = _run_bridge_sync()
	if not verify_result.get("success", false) or verify_result.get("result", "") != "Success" or not verify_result.get("is_email_verified", false):
		printerr("[FAIL]: Email verification failed with valid token. Result: ", verify_result)
		_clean_files()
		quit(1)
		return

	if verify_result.get("onboarding_step", "") != "EmailVerified":
		printerr("[FAIL]: Expected OnboardingStep 'EmailVerified', got: ", verify_result.get("onboarding_step", ""))
		_clean_files()
		quit(1)
		return
	print("  [OK]: Email successfully verified. Advanced to OnboardingStep: EmailVerified.")

	print("\n--- Check 5: Procedural Country Founding via CountryProfileFactory ---")
	var found_req := {
		"action": "create_country",
		"country_name": "Republic of Azuria",
		"request_id": "test_found_success"
	}
	_write_request(found_req)
	var found_result = _run_bridge_sync()
	if not found_result.get("success", false) or found_result.get("result", "") != "Success":
		printerr("[FAIL]: Country founding failed after email verification. Result: ", found_result)
		_clean_files()
		quit(1)
		return

	if found_result.get("onboarding_step", "") != "CountryCreated":
		printerr("[FAIL]: Expected OnboardingStep 'CountryCreated', got: ", found_result.get("onboarding_step", ""))
		_clean_files()
		quit(1)
		return

	# Inspect state file for procedural profile fields
	var state_file := FileAccess.open(ProjectSettings.globalize_path(TEST_STATE_PATH), FileAccess.READ)
	if state_file == null:
		printerr("[FAIL]: presidential_desk_state.json was not created.")
		_clean_files()
		quit(1)
		return

	var state_text := state_file.get_as_text()
	state_file.close()
	var state_json = JSON.parse_string(state_text)
	if state_json == null or typeof(state_json) != TYPE_DICTIONARY:
		printerr("[FAIL]: presidential_desk_state.json contains invalid JSON.")
		_clean_files()
		quit(1)
		return

	var capital := str(state_json.get("capital_city", "")).strip_edges()
	var geography := str(state_json.get("geography", "")).strip_edges()
	var demographics := str(state_json.get("demographics", "")).strip_edges()
	var constitution := str(state_json.get("constitution", "")).strip_edges()
	var resource := str(state_json.get("primary_resource", "")).strip_edges()

	if capital.is_empty() or geography.is_empty() or demographics.is_empty() or constitution.is_empty() or resource.is_empty():
		printerr("[FAIL]: Procedural country profile missing required fields: ", state_json)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Procedural Country Created successfully:")
	print("       - Country Name: ", state_json.get("country_name", ""))
	print("       - Capital City: ", capital)
	print("       - Geography: ", geography)
	print("       - Demographics: ", demographics)
	print("       - Constitution: ", constitution)
	print("       - Primary Resource: ", resource)

	print("\n--- Check 6: Duplicate Country Creation Protection ---")
	var duplicate_req := {
		"action": "create_country",
		"country_name": "Republic of Solaria",
		"request_id": "test_duplicate_attempt"
	}
	_write_request(duplicate_req)
	var duplicate_result = _run_bridge_sync()
	if duplicate_result.get("success", false) or duplicate_result.get("result", "") != "DuplicateCountryCreation":
		printerr("[FAIL]: Duplicate country creation was not rejected. Result: ", duplicate_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Duplicate country creation in same session rejected: " + str(duplicate_result.get("message", "")))

	print("\n--- Check 7: PII Isolation & Privacy ---")
	if state_text.contains("elena.okafor@republic.net") or state_text.contains("1994-06-12"):
		printerr("[FAIL]: PII leak detected! Public state file contains raw email or date of birth.")
		_clean_files()
		quit(1)
		return

	var player_display: String = str(state_json.get("player_display_name", "")).strip_edges()
	if player_display != "Elena Okafor":
		printerr("[FAIL]: Expected sanitized player display name 'Elena Okafor', got: ", player_display)
		_clean_files()
		quit(1)
		return
	print("  [OK]: PII strictly isolated: zero email or DOB exposure in public state. Display name: " + player_display)

	print("\n==============================================================")
	print(" [PASS]: ALL 7 ONBOARDING PHASE 2 CHECKS PASSED!             ")
	print("==============================================================\n")
	_clean_files()
	quit(0)

func _write_request(dict: Dictionary) -> void:
	var req_file := FileAccess.open(ProjectSettings.globalize_path(TEST_REQUEST_PATH), FileAccess.WRITE)
	if req_file != null:
		req_file.store_string(JSON.stringify(dict, "\t"))
		req_file.close()

func _run_bridge_sync() -> Dictionary:
	var dll_path := ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll")
	var state_file := ProjectSettings.globalize_path(TEST_STATE_PATH)
	var save_file := ProjectSettings.globalize_path(TEST_SAVE_PATH)
	var req_file := ProjectSettings.globalize_path(TEST_REQUEST_PATH)

	var cli_args: PackedStringArray = [
		dll_path,
		"--process-request",
		"--state-file=" + state_file,
		"--save-file=" + save_file,
		"--request-file=" + req_file
	]
	var output := []
	var exit_code := OS.execute("dotnet", cli_args, output, true)
	if exit_code != 0:
		print("CLI execution failed (exit code %d): %s" % [exit_code, "\n".join(output)])

	var res_file := FileAccess.open(ProjectSettings.globalize_path(TEST_RESULT_PATH), FileAccess.READ)
	if res_file != null:
		var content := res_file.get_as_text()
		res_file.close()
		var parsed = JSON.parse_string(content)
		if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
			return parsed

	return { "success": false, "result": "Error", "message": "Failed to read result file" }
