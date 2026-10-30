# ==============================================================================
# Republic - New Player Onboarding Phase 2 Verification Test
# Tests account registration, age verification, email verification tokens,
# duplicate account protection, country name validation, procedural country
# generation with standardized metrics (50M REPU, 60% approval, 10M population),
# duplicate country protection, and PII isolation.
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

	var button_text: String = str(reg_result.get("verification_button_text", "")).strip_edges()
	if button_text != "Verify Email":
		printerr("[FAIL]: Expected verification button text 'Verify Email', got: ", button_text)
		_clean_files()
		quit(1)
		return

	if reg_result.get("onboarding_step", "") != "AccountRegistered" or reg_result.get("is_email_verified", true):
		printerr("[FAIL]: Onboarding step mismatch. Expected AccountRegistered/Unverified, got: ", reg_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Registration successful. Generated token: %s, Step: %s" % [token, reg_result.get("onboarding_step", "")])
	print("  [OK]: Official verification email drafted with button text: '%s'" % [button_text])

	print("\n--- Check 3: Duplicate Account Registration Prevention ---")
	var dup_reg_req := {
		"action": "register_account",
		"first_name": "Elena",
		"last_name": "Okafor",
		"email": "elena.okafor@republic.net",
		"date_of_birth": "1994-06-12",
		"request_id": "test_dup_reg"
	}
	_write_request(dup_reg_req)
	var dup_reg_result = _run_bridge_sync()
	if dup_reg_result.get("success", false) or dup_reg_result.get("result", "") != "DuplicateAccount":
		printerr("[FAIL]: Duplicate account registration was not prevented. Result: ", dup_reg_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Duplicate account registration prevented: " + str(dup_reg_result.get("message", "")))

	print("\n--- Check 4: Enforcing Email Verification Precondition for Country Founding ---")
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

	print("\n--- Check 5: Email Verification Success ---")
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

	print("\n--- Check 6: Country Naming Rule Validation ---")
	var bad_name_req := {
		"action": "create_country",
		"country_name": "Azuria Kingdom", # Missing whole word 'Republic'
		"request_id": "test_bad_name"
	}
	_write_request(bad_name_req)
	var bad_name_result = _run_bridge_sync()
	if bad_name_result.get("success", false) or bad_name_result.get("result", "") != "InvalidName":
		printerr("[FAIL]: Non-republic country name was not rejected. Result: ", bad_name_result)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Country naming rule enforced: " + str(bad_name_result.get("message", "")))

	print("\n--- Check 7: Procedural Country Founding with Standardized Metrics ---")
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

	# Inspect state file for standardized initial metrics and procedural profile
	var state_file := FileAccess.open(ProjectSettings.globalize_path(TEST_STATE_PATH), FileAccess.READ)
	if state_file == null:
		printerr("[FAIL]: test_onboarding_state.json was not created.")
		_clean_files()
		quit(1)
		return

	var state_text := state_file.get_as_text()
	state_file.close()
	var state_json = JSON.parse_string(state_text)
	if state_json == null or typeof(state_json) != TYPE_DICTIONARY:
		printerr("[FAIL]: test_onboarding_state.json contains invalid JSON.")
		_clean_files()
		quit(1)
		return

	var capital := str(state_json.get("capital_city", "")).strip_edges()
	var constitution := str(state_json.get("constitution", "")).strip_edges()
	var resource := str(state_json.get("primary_resource", "")).strip_edges()
	var treasury_val: float = float(found_result.get("treasury_balance", 0.0))
	var approval_val: float = float(found_result.get("approval_rating", 0.0))
	var stability_val: float = float(found_result.get("stability_rating", 0.0))
	var population_val: int = int(found_result.get("population", 0))

	# Authoritative baseline checks
	if treasury_val != 50000000.0:
		printerr("[FAIL]: Expected standardized starting treasury 50M REPU (50000000.0), got: ", treasury_val)
		_clean_files()
		quit(1)
		return

	if approval_val != 60.0:
		printerr("[FAIL]: Expected standardized starting approval 60%, got: ", approval_val)
		_clean_files()
		quit(1)
		return

	if stability_val != 70.0:
		printerr("[FAIL]: Expected standardized starting stability 70%, got: ", stability_val)
		_clean_files()
		quit(1)
		return

	if population_val != 10000000:
		printerr("[FAIL]: Expected standardized starting population 10,000,000, got: ", population_val)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Standardized Sovereign Metrics Verified for all players:")
	print("       - Country Name: ", state_json.get("country_name", ""))
	print("       - Capital City: ", capital)
	print("       - Sovereign Treasury: R %s REPU" % [String.num_scientific(treasury_val) if treasury_val > 1e12 else "50,000,000"])
	print("       - Public Approval: %s%%" % [approval_val])
	print("       - National Stability: %s%%" % [stability_val])
	print("       - Baseline Population: %s citizens" % [population_val])
	print("       - Constitution: ", constitution)
	print("       - Primary Resource: ", resource)

	print("\n--- Check 8: Duplicate Country Creation Protection ---")
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

	print("\n--- Check 9: PII Isolation & Privacy ---")
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
	print(" [PASS]: ALL 9 ONBOARDING PHASE 2 CHECKS PASSED!             ")
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
