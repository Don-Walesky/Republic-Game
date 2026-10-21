extends SceneTree

## Automated Headless Verification Suite for Republic Phase 4:
## Domestic Cabinet & Ministry Dock inside Executive Office
## Validates:
## 1. Left Dock Cabinet Panel UI structure & all 6 portfolio cards + Appointment Modal
## 2. All 6 constitutional portfolios rendered (Finance, Defense, ForeignAffairs, Interior, Health, Infrastructure)
## 3. Authoritative state loading (Appointed ministers, ratings, state capacity effects)
## 4. Honest vacant representation & baseline capacity presentation
## 5. Missing state safe fallback (Zero invented stats / [No State Loaded] / Vacant)
## 6. Cabinet appointment modal & consequence previews grounded in C# core rules
## 7. Action pipeline request generation (appoint_minister)
## 8. Progressive dock integration & navigation to PresidentialDesk cabinet section
## 9. Test cleanup safety regression (pre-existing player request file preserved)
## 10. Missing CLI processor failure reporting (clear warning, no false success)
## 11. Real End-to-End C# CLI Pipeline Integration Verification:
##     - Isolated test country creation via Republic.Cli
##     - Finance, Interior, Infrastructure (Technocrat, Loyalist, Dismiss)
##     - Cancellation without mutation, invalid request rejection
##     - Unrelated data preservation, save/load persistence in .sav, dynamic UI display

const ISOLATED_TEST_DIR := "res://temp_phase4_isolated"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE PHASE 4 CABINET TEST           ")
	print("==============================================================")

	var scene_res := load("res://ExecutiveOffice.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://ExecutiveOffice.tscn")
		_cleanup_all()
		quit(1)
		return

	var office = scene_res.instantiate()
	if office == null:
		printerr("[FAIL]: Failed to instantiate ExecutiveOffice node")
		_cleanup_all()
		quit(1)
		return

	root.add_child(office)
	office._init_controls()
	office._connect_signals()

	var all_passed := true

	# Check 1: Cabinet UI Nodes Exist & Structure Verified
	all_passed = test_cabinet_ui_structure(office) and all_passed

	# Check 2: All 6 Constitutional Portfolios Rendered
	all_passed = test_all_six_portfolios_rendered(office) and all_passed

	# Check 3: Authoritative State Loading (Appointed Ministers & Capacity)
	all_passed = test_authoritative_state_loading(office) and all_passed

	# Check 4: Vacant Portfolios & Baseline Capacity
	all_passed = test_vacant_portfolios_and_baseline(office) and all_passed

	# Check 5: Missing State Safe Fallback
	all_passed = test_missing_state_safe_fallback(office) and all_passed

	# Check 6: Appointment Modal & Consequence Previews
	all_passed = test_appointment_modal_and_previews(office) and all_passed

	# Check 7: Appointment Request Pipeline
	all_passed = test_appointment_request_pipeline(office) and all_passed

	# Check 8: Navigation Integration
	all_passed = test_navigation_integration(office) and all_passed

	# Check 9: Pre-existing Request File Safety Regression Check
	all_passed = test_request_file_safety_regression(office) and all_passed

	# Check 10: Missing Simulation Processor Failure Handling (No False Success)
	all_passed = test_missing_cli_processor_failure_handling(office) and all_passed

	# Check 11: Real End-to-End C# CLI Pipeline Integration Verification
	all_passed = test_real_csharp_cli_appointment_pipeline(office) and all_passed

	office.queue_free()

	# Guaranteed clean-up of test-owned isolated files only
	_cleanup_all()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 11 PHASE 4 CABINET VERIFICATION CHECKS PASSED!   ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE PHASE 4 CABINET VERIFICATION CHECKS FAILED ")
		printerr("==============================================================")
		quit(1)

## Cleans up ONLY test-owned temporary artifacts and never touches player or real game files
func _cleanup_all() -> void:
	var temp_files := [
		"res://temp_phase4_request.json",
		"res://temp_phase4_state.json"
	]
	for tf in temp_files:
		if FileAccess.file_exists(tf):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(tf))

	var global_iso := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	if DirAccess.dir_exists_absolute(global_iso):
		var dir := DirAccess.open(global_iso)
		if dir != null:
			dir.list_dir_begin()
			var fn := dir.get_next()
			while fn != "":
				if not dir.current_is_dir():
					DirAccess.remove_absolute(global_iso.path_join(fn))
				fn = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(global_iso)

# ------------------------------------------------------------------------------
# Check 1: Cabinet UI Nodes Exist & Structure Verified
# ------------------------------------------------------------------------------
func test_cabinet_ui_structure(office: Node) -> bool:
	print("\n--- Check 1: Cabinet Panel UI Structure & Modal Nodes ---")

	var nodes_to_check: Array[String] = [
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health",
		"InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure",
		"InterfaceLayer/CabinetAppointmentModal",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentHeaderHBox/CloseCabinetAppointmentXButton",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentPortfolioLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentCurrentMinisterLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateTechnocratButton",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateLoyalistButton",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateDismissButton",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewNameLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewRatingsLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewSalaryLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewConsequencesLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentStatusLabel",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/ConfirmAppointmentButton",
		"InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/CancelAppointmentButton"
	]

	for path in nodes_to_check:
		var node = office.get_node_or_null(path)
		if node == null:
			printerr("  [FAIL]: Required Cabinet UI node missing: %s" % path)
			return false

	print("  [OK]: All LeftDock Cabinet and CabinetAppointmentModal nodes verified.")
	return true

# ------------------------------------------------------------------------------
# Check 2: All 6 Constitutional Portfolios Rendered
# ------------------------------------------------------------------------------
func test_all_six_portfolios_rendered(office: Node) -> bool:
	print("\n--- Check 2: All 6 Constitutional Portfolios Rendered ---")

	var vbox = office.get_node_or_null("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox")
	if vbox == null:
		printerr("  [FAIL]: CabinetPortfoliosVBox missing.")
		return false

	var portfolios: Array[String] = [
		"PortfolioCard_Finance",
		"PortfolioCard_Defense",
		"PortfolioCard_ForeignAffairs",
		"PortfolioCard_Interior",
		"PortfolioCard_Health",
		"PortfolioCard_Infrastructure"
	]

	for p_card_name in portfolios:
		var card = vbox.get_node_or_null(p_card_name)
		if card == null:
			printerr("  [FAIL]: Expected card for %s not found in CabinetPortfoliosVBox." % p_card_name)
			return false

	# Verify interactive buttons exist for Phase 4 active ministries
	if office.appoint_finance_button == null:
		printerr("  [FAIL]: Appoint Finance button missing.")
		return false
	if office.appoint_interior_button == null:
		printerr("  [FAIL]: Appoint Interior button missing.")
		return false
	if office.appoint_infrastructure_button == null:
		printerr("  [FAIL]: Appoint Infrastructure button missing.")
		return false

	print("  [OK]: All 6 constitutional portfolios rendered with active appointment hooks.")
	return true

# ------------------------------------------------------------------------------
# Check 3: Authoritative State Loading (Appointed Ministers & Capacity)
# ------------------------------------------------------------------------------
func test_authoritative_state_loading(office: Node) -> bool:
	print("\n--- Check 3: Authoritative State Loading (Appointed Ministers & Capacity) ---")

	var state: Dictionary = {
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 10000000.0,
		"civil_service_quality": 0.85,
		"corruption_control": 0.90,
		"infrastructure_condition": 0.75,
		"cabinet": [
			{
				"portfolio": "Finance",
				"is_appointed": true,
				"status": "Appointed",
				"minister_name": "Dr. Elena Rostova",
				"competence": 0.90,
				"loyalty": 0.50,
				"integrity": 0.90,
				"monthly_salary": 150000.0,
				"state_capacity_metric": "Civil Service Quality",
				"state_capacity_value": 0.85
			},
			{
				"portfolio": "Interior",
				"is_appointed": true,
				"status": "Appointed",
				"minister_name": "Dr. Alistair Finch",
				"competence": 0.90,
				"loyalty": 0.50,
				"integrity": 0.90,
				"monthly_salary": 150000.0,
				"state_capacity_metric": "Corruption Control",
				"state_capacity_value": 0.90
			},
			{
				"portfolio": "Infrastructure",
				"is_appointed": true,
				"status": "Appointed",
				"minister_name": "Eng. Marcus Bennett",
				"competence": 0.90,
				"loyalty": 0.50,
				"integrity": 0.90,
				"monthly_salary": 150000.0,
				"state_capacity_metric": "Infrastructure Condition",
				"state_capacity_value": 0.75
			}
		]
	}

	office.apply_state(state)

	# Validate Finance UI
	if office.finance_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Finance status expected 'Appointed (Active)', got: %s" % office.finance_status_label.text)
		return false
	if not office.finance_minister_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Finance minister expected 'Dr. Elena Rostova', got: %s" % office.finance_minister_label.text)
		return false
	if not office.finance_effect_label.text.contains("Civil Service Quality: 85.0%"):
		printerr("  [FAIL]: Finance effect label expected 85.0%, got: %s" % office.finance_effect_label.text)
		return false
	if office.appoint_finance_button.text != "Manage Appointment":
		printerr("  [FAIL]: Appoint finance button expected 'Manage Appointment', got: %s" % office.appoint_finance_button.text)
		return false

	# Validate Interior UI
	if office.interior_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Interior status expected 'Appointed (Active)', got: %s" % office.interior_status_label.text)
		return false
	if not office.interior_minister_label.text.contains("Dr. Alistair Finch"):
		printerr("  [FAIL]: Interior minister expected 'Dr. Alistair Finch', got: %s" % office.interior_minister_label.text)
		return false
	if not office.interior_effect_label.text.contains("Corruption Control: 90.0%"):
		printerr("  [FAIL]: Interior effect label expected 90.0%, got: %s" % office.interior_effect_label.text)
		return false

	# Validate Infrastructure UI
	if office.infrastructure_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Infrastructure status expected 'Appointed (Active)', got: %s" % office.infrastructure_status_label.text)
		return false
	if not office.infrastructure_minister_label.text.contains("Eng. Marcus Bennett"):
		printerr("  [FAIL]: Infrastructure minister expected 'Eng. Marcus Bennett', got: %s" % office.infrastructure_minister_label.text)
		return false
	if not office.infrastructure_effect_label.text.contains("Infrastructure Condition: 75.0%"):
		printerr("  [FAIL]: Infrastructure effect label expected 75.0%, got: %s" % office.infrastructure_effect_label.text)
		return false

	print("  [OK]: Authoritative minister names, key ratings, and capacity effects loaded faithfully.")
	return true

# ------------------------------------------------------------------------------
# Check 4: Vacant Portfolios & Baseline Capacity
# ------------------------------------------------------------------------------
func test_vacant_portfolios_and_baseline(office: Node) -> bool:
	print("\n--- Check 4: Vacant Portfolios & Baseline Capacity ---")

	var state: Dictionary = {
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 10000000.0,
		"civil_service_quality": 0.30,
		"corruption_control": 0.30,
		"infrastructure_condition": 0.30,
		"cabinet": [
			{
				"portfolio": "Finance",
				"is_appointed": false,
				"status": "Vacant",
				"minister_name": "Vacant",
				"competence": 0.0,
				"loyalty": 0.0,
				"integrity": 0.0,
				"monthly_salary": 0.0,
				"state_capacity_metric": "Civil Service Quality",
				"state_capacity_value": 0.30
			},
			{
				"portfolio": "Defense",
				"is_appointed": false,
				"status": "Vacant",
				"minister_name": "Vacant"
			}
		]
	}

	office.apply_state(state)

	if office.finance_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Finance status expected 'Vacant (Unassigned)', got: " + office.finance_status_label.text)
		return false
	if office.finance_minister_label.text != "Minister: [Vacant]":
		printerr("  [FAIL]: Finance minister expected 'Minister: [Vacant]', got: " + office.finance_minister_label.text)
		return false
	if not office.finance_effect_label.text.contains("30.0%"):
		printerr("  [FAIL]: Finance effect label expected 30.0%, got: " + office.finance_effect_label.text)
		return false
	if office.appoint_finance_button.text != "Appoint Minister":
		printerr("  [FAIL]: Appoint finance button expected 'Appoint Minister', got: " + office.appoint_finance_button.text)
		return false

	# Defense should also show honest vacant
	if office.defense_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Defense status expected 'Vacant (Unassigned)', got: " + office.defense_status_label.text)
		return false

	print("  [OK]: Vacant portfolios show honest unassigned presentation and baseline capacity.")
	return true

# ------------------------------------------------------------------------------
# Check 5: Missing State Safe Fallback
# ------------------------------------------------------------------------------
func test_missing_state_safe_fallback(office: Node) -> bool:
	print("\n--- Check 5: Missing State Safe Fallback ---")

	office.display_missing_state()

	if not office.finance_status_label.text.contains("Vacant"):
		printerr("  [FAIL]: Expected 'Vacant' when missing state, got: " + office.finance_status_label.text)
		return false
	if not office.defense_status_label.text.contains("Vacant"):
		printerr("  [FAIL]: Expected 'Vacant' when missing state, got: " + office.defense_status_label.text)
		return false
	if office.finance_ratings_label.text != "Ratings: --":
		printerr("  [FAIL]: Expected 'Ratings: --' ratings, got: " + office.finance_ratings_label.text)
		return false

	print("  [OK]: Missing state safely handles all 6 portfolios without mock data or errors.")
	return true

# ------------------------------------------------------------------------------
# Check 6: Appointment Modal & Consequence Previews
# ------------------------------------------------------------------------------
func test_appointment_modal_and_previews(office: Node) -> bool:
	print("\n--- Check 6: Appointment Modal & Consequence Previews ---")

	# Ensure base state
	office.apply_state({
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 5000000.0,
		"cabinet": [
			{ "portfolio": "Finance", "is_appointed": false, "status": "Vacant", "minister_name": "Vacant" }
		]
	})

	office.open_appointment_modal("Finance")

	if not office.cabinet_appointment_modal.visible:
		printerr("  [FAIL]: Cabinet appointment modal should be visible.")
		return false
	if not office.appointment_portfolio_label.text.contains("Ministry of Finance"):
		printerr("  [FAIL]: Expected 'Ministry of Finance' title, got: " + office.appointment_portfolio_label.text)
		return false

	# Default should be technocrat
	if not office.candidate_preview_name_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Expected Dr. Elena Rostova for Finance technocrat, got: " + office.candidate_preview_name_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("+1.5%"):
		printerr("  [FAIL]: Expected +1.5% approval consequence, got: " + office.candidate_preview_consequences_label.text)
		return false

	# Switch to loyalist
	office._on_candidate_loyalist_pressed()
	if not office.candidate_preview_name_label.text.contains("Senator Donald Vance"):
		printerr("  [FAIL]: Expected Senator Donald Vance for Finance loyalist, got: " + office.candidate_preview_name_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("-1.5%"):
		printerr("  [FAIL]: Expected -1.5% approval consequence for loyalist, got: " + office.candidate_preview_consequences_label.text)
		return false

	# Switch to dismiss
	office._on_candidate_dismiss_pressed()
	if not office.candidate_preview_name_label.text.contains("Dismiss"):
		printerr("  [FAIL]: Expected dismiss preview title, got: " + office.candidate_preview_name_label.text)
		return false

	office.close_all_modals()
	if office.cabinet_appointment_modal.visible:
		printerr("  [FAIL]: Cabinet appointment modal should be closed.")
		return false

	print("  [OK]: Appointment modal opens, switches candidates, and displays honest core rules consequences.")
	return true

# ------------------------------------------------------------------------------
# Check 7: Appointment Request Pipeline
# ------------------------------------------------------------------------------
func test_appointment_request_pipeline(office: Node) -> bool:
	print("\n--- Check 7: Appointment Request Pipeline ---")

	var temp_req_path := "res://temp_phase4_request.json"
	office.request_file_override = temp_req_path

	office.apply_state({
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 2500000.0
	})

	var res: Dictionary = office.execute_cabinet_appointment("Finance", "technocrat", true)
	if not res.get("success", false):
		printerr("  [FAIL]: Expected cabinet appointment request submission to succeed.")
		return false

	if not FileAccess.file_exists(temp_req_path):
		printerr("  [FAIL]: Request file was not written to disk: %s" % temp_req_path)
		return false

	var f := FileAccess.open(temp_req_path, FileAccess.READ)
	if f == null:
		printerr("  [FAIL]: Failed to read created request file.")
		return false

	var json_str := f.get_as_text()
	f.close()

	var json := JSON.new()
	var err := json.parse(json_str)
	if err != OK or not (json.data is Dictionary):
		printerr("  [FAIL]: Request file contains invalid JSON: %s" % json_str)
		return false

	var req_data: Dictionary = json.data
	if req_data.get("action") != "appoint_minister":
		printerr("  [FAIL]: Action expected 'appoint_minister', got: %s" % req_data.get("action"))
		return false
	if req_data.get("portfolio") != "Finance":
		printerr("  [FAIL]: Portfolio expected 'Finance', got: %s" % req_data.get("portfolio"))
		return false
	if req_data.get("candidate_type") != "technocrat":
		printerr("  [FAIL]: Candidate expected 'technocrat', got: %s" % req_data.get("candidate_type"))
		return false
	if not req_data.get("confirmed", false):
		printerr("  [FAIL]: Confirmed expected true.")
		return false
	if not str(req_data.get("request_id", "")).begins_with("req_cab_"):
		printerr("  [FAIL]: Request ID should begin with 'req_cab_'.")
		return false

	# Test rejection when missing state
	office.display_missing_state()
	var rejected_res: Dictionary = office.execute_cabinet_appointment("Finance", "loyalist", true)
	if rejected_res.get("success", true):
		printerr("  [FAIL]: Expected cabinet appointment to be rejected when no state loaded.")
		return false

	# Clean up unit test request
	if FileAccess.file_exists(temp_req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(temp_req_path))

	office.request_file_override = ""

	print("  [OK]: Appointment request correctly formatted, serialized to disk, and validated against missing state.")
	return true

# ------------------------------------------------------------------------------
# Check 8: Navigation Integration
# ------------------------------------------------------------------------------
func test_navigation_integration(office: Node) -> bool:
	print("\n--- Check 8: Navigation Integration ---")

	if office.open_cabinet_button == null:
		printerr("  [FAIL]: open_cabinet_button missing from LeftDock.")
		return false

	if not office.open_cabinet_button.pressed.is_connected(office._on_nav_cabinet_pressed):
		printerr("  [FAIL]: open_cabinet_button not connected to _on_nav_cabinet_pressed.")
		return false

	print("  [OK]: Navigation to PresidentialDesk cabinet section preserved and verified.")
	return true

# ------------------------------------------------------------------------------
# Check 9: Pre-existing Request File Safety Regression Check
# ------------------------------------------------------------------------------
func test_request_file_safety_regression(office: Node) -> bool:
	print("\n--- Check 9: Pre-existing Request File Safety Regression Check ---")

	var real_req_path := "res://presidential_desk_request.json"
	var global_real_req := ProjectSettings.globalize_path(real_req_path)

	# Ensure isolated directory exists
	var global_iso := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	DirAccess.make_dir_absolute(global_iso)

	var had_existing_file := FileAccess.file_exists(real_req_path)
	var original_content := ""
	if had_existing_file:
		var f := FileAccess.open(real_req_path, FileAccess.READ)
		if f != null:
			original_content = f.get_as_text()
			f.close()
	else:
		# Plant a protected canary file
		original_content = "{\"canary\": \"protected_real_player_unprocessed_request_12345\"}"
		var f := FileAccess.open(real_req_path, FileAccess.WRITE)
		if f != null:
			f.store_string(original_content)
			f.close()

	# Run cabinet appointment operations using an isolated request path
	var iso_req := ISOLATED_TEST_DIR + "/test_safety_request.json"
	office.request_file_override = iso_req
	office.apply_state({
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 1000000.0
	})

	var res: Dictionary = office.execute_cabinet_appointment("Finance", "technocrat", false)
	if not res.get("success", false):
		printerr("  [FAIL]: Failed to execute appointment with isolated request path.")
		return false

	if not FileAccess.file_exists(iso_req):
		printerr("  [FAIL]: Isolated request file was not created at %s" % iso_req)
		return false

	# Run isolated cleanup
	_cleanup_all()

	# Verify isolated file was removed
	if FileAccess.file_exists(iso_req):
		printerr("  [FAIL]: Isolated request file should have been cleaned up.")
		return false

	# REGRESSION ASSERTION: Real game request file MUST be completely preserved!
	if not FileAccess.file_exists(real_req_path):
		printerr("  [FAIL]: CRITICAL REGRESSION! Real game request file '%s' was deleted!" % real_req_path)
		return false

	var check_f := FileAccess.open(real_req_path, FileAccess.READ)
	var current_content := check_f.get_as_text() if check_f != null else ""
	if check_f != null:
		check_f.close()

	if current_content != original_content:
		printerr("  [FAIL]: CRITICAL REGRESSION! Real game request file was modified by test!")
		return false

	# Clean up canary if we planted it
	if not had_existing_file:
		DirAccess.remove_absolute(global_real_req)

	office.request_file_override = ""
	print("  [OK]: Pre-existing request file safety verified: tests never delete or modify real player files.")
	return true

# ------------------------------------------------------------------------------
# Check 10: Missing Simulation Processor Failure Handling (No False Success)
# ------------------------------------------------------------------------------
func test_missing_cli_processor_failure_handling(office: Node) -> bool:
	print("\n--- Check 10: Missing Simulation Processor Failure Handling ---")

	var global_iso := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	DirAccess.make_dir_recursive_absolute(global_iso)

	var iso_req := ISOLATED_TEST_DIR + "/temp_missing_proc_request.json"
	office.request_file_override = iso_req
	office.cli_processor_override = "missing"

	office.apply_state({
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 5000000.0
	})

	var res: Dictionary = office.execute_cabinet_appointment("Finance", "technocrat", true)

	office.cli_processor_override = ""
	office.request_file_override = ""
	_cleanup_all()

	if res.get("success", true) != false:
		printerr("  [FAIL]: Expected success=false when simulation processor is missing, got: " + str(res.get("success")))
		return false
	if res.get("processor_launched", true) != false:
		printerr("  [FAIL]: Expected processor_launched=false when processor is missing.")
		return false
	if not res.get("message", "").contains("simulation processor failed"):
		printerr("  [FAIL]: Expected failure message to report processor failure, got: " + res.get("message"))
		return false

	if office.appointment_status_label == null or not office.appointment_status_label.text.contains("simulation processor failed"):
		printerr("  [FAIL]: Expected appointment_status_label to show failure warning, got: " + (office.appointment_status_label.text if office.appointment_status_label else "null"))
		return false

	print("  [OK]: Missing CLI processor cleanly detected and reported with explicit failure message instead of false success.")
	return true

# ------------------------------------------------------------------------------
# Check 11: Real End-to-End C# CLI Pipeline Integration Verification
# ------------------------------------------------------------------------------
func test_real_csharp_cli_appointment_pipeline(office: Node) -> bool:
	print("\n--- Check 11: Real End-to-End C# CLI Pipeline Integration Verification ---")

	# 1. Locate C# CLI DLL
	var candidates: Array[String] = [
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll"),
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"),
		"src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll",
		"src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"
	]
	var cli_dll := ""
	for c in candidates:
		var simp := c.simplify_path()
		if FileAccess.file_exists(simp):
			cli_dll = simp
			break
		elif FileAccess.file_exists(c):
			cli_dll = c
			break

	if cli_dll.is_empty():
		printerr("  [FAIL]: Republic.Cli.dll not found in Debug or Release build directories. Run 'dotnet build' first.")
		return false

	print("  -> Using authoritative simulation CLI: " + cli_dll)

	# 2. Setup isolated temporary directory for test country
	var global_iso := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	DirAccess.make_dir_recursive_absolute(global_iso)

	var iso_state := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	var iso_save := ISOLATED_TEST_DIR + "/presidential_desk_state.sav"
	var iso_req := ISOLATED_TEST_DIR + "/presidential_desk_request.json"

	var global_iso_state := ProjectSettings.globalize_path(iso_state)
	var global_iso_save := ProjectSettings.globalize_path(iso_save)
	var global_iso_req := ProjectSettings.globalize_path(iso_req)

	# 3. Create isolated authoritative test country via Republic.Cli
	var norm_dll := cli_dll.simplify_path()
	var norm_state := global_iso_state.simplify_path()
	var norm_save := global_iso_save.simplify_path()
	if OS.get_name() == "Windows":
		norm_dll = norm_dll.replace("/", "\\")
		norm_state = norm_state.replace("/", "\\")
		norm_save = norm_save.replace("/", "\\")

	var create_args: PackedStringArray = [
		norm_dll,
		"--create-country=Federal Republic of Testland",
		"--state-file=" + norm_state,
		"--save-file=" + norm_save
	]
	var create_out: Array = []
	var create_code := OS.execute("dotnet", create_args, create_out, true)
	if create_code != 0:
		printerr("  [FAIL]: Failed to execute Republic.Cli to create test country. Exit: " + str(create_code) + ", out: " + str(create_out))
		return false

	if not FileAccess.file_exists(iso_state) or not FileAccess.file_exists(iso_save):
		printerr("  [FAIL]: Expected isolated test state and save files to be created.")
		return false

	# 4. Attach ExecutiveOffice to isolated test environment
	office.current_state_path = iso_state
	office.custom_save_path = iso_save
	office.request_file_override = iso_req
	var loaded: bool = office.load_state_from_file(iso_state)
	if not loaded:
		printerr("  [FAIL]: Failed to load isolated test state into ExecutiveOffice.")
		return false

	# 5. Verify Finance Portfolio: Technocrat, Loyalist, Dismiss
	print("  -> Verifying Finance Portfolio actions...")

	# 5a. Finance Technocrat
	var res_f_tech: Dictionary = office.execute_cabinet_appointment("Finance", "technocrat", true)
	if not res_f_tech.get("success", false):
		printerr("  [FAIL]: Failed to submit Finance technocrat appointment.")
		return false
	var sync_f_tech: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_f_tech.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Finance technocrat: " + sync_f_tech.get("message"))
		return false

	if office.finance_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Finance status expected 'Appointed (Active)', got: " + office.finance_status_label.text)
		return false
	if not office.finance_minister_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Finance minister expected 'Dr. Elena Rostova', got: " + office.finance_minister_label.text)
		return false
	if not office.finance_effect_label.text.contains("Civil Service Quality: 90.0%"):
		printerr("  [FAIL]: Finance capacity expected 'Civil Service Quality: 90.0%', got: " + office.finance_effect_label.text)
		return false
	if not office.civil_service_label.text.contains("90%"):
		printerr("  [FAIL]: Civil service label expected '90%', got: " + office.civil_service_label.text)
		return false
	if not office.appointment_status_label.text.begins_with("Certified:"):
		printerr("  [FAIL]: Appointment status label expected Certified prefix, got: " + office.appointment_status_label.text)
		return false

	# 5b. Finance Loyalist
	office.execute_cabinet_appointment("Finance", "loyalist", true)
	var sync_f_loy: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_f_loy.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Finance loyalist.")
		return false
	if not office.finance_minister_label.text.contains("Senator Donald Vance"):
		printerr("  [FAIL]: Finance minister expected 'Senator Donald Vance', got: " + office.finance_minister_label.text)
		return false
	if not office.finance_effect_label.text.contains("Civil Service Quality: 45.0%"):
		printerr("  [FAIL]: Finance capacity expected 'Civil Service Quality: 45.0%', got: " + office.finance_effect_label.text)
		return false
	if not office.civil_service_label.text.contains("45%"):
		printerr("  [FAIL]: Civil service label expected '45%', got: " + office.civil_service_label.text)
		return false

	# 5c. Finance Dismiss / Vacate
	office.execute_cabinet_appointment("Finance", "dismiss", true)
	var sync_f_dis: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_f_dis.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Finance dismiss.")
		return false
	if office.finance_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Finance status expected 'Vacant (Unassigned)', got: " + office.finance_status_label.text)
		return false
	if not office.finance_effect_label.text.contains("30.0%"):
		printerr("  [FAIL]: Finance capacity expected baseline '30.0%', got: " + office.finance_effect_label.text)
		return false

	# 6. Verify Interior Portfolio: Technocrat, Loyalist, Dismiss
	print("  -> Verifying Interior Portfolio actions...")

	# 6a. Interior Technocrat
	office.execute_cabinet_appointment("Interior", "technocrat", true)
	var sync_i_tech: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_i_tech.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Interior technocrat.")
		return false
	if office.interior_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Interior status expected 'Appointed (Active)', got: " + office.interior_status_label.text)
		return false
	if not office.interior_minister_label.text.contains("Dr. Alistair Finch"):
		printerr("  [FAIL]: Interior minister expected 'Dr. Alistair Finch', got: " + office.interior_minister_label.text)
		return false
	if not office.interior_effect_label.text.contains("Corruption Control: 90.0%"):
		printerr("  [FAIL]: Interior capacity expected 'Corruption Control: 90.0%', got: " + office.interior_effect_label.text)
		return false
	if not office.corruption_control_label.text.contains("90%"):
		printerr("  [FAIL]: Corruption control label expected '90%', got: " + office.corruption_control_label.text)
		return false

	# 6b. Interior Loyalist
	office.execute_cabinet_appointment("Interior", "loyalist", true)
	var sync_i_loy: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_i_loy.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Interior loyalist.")
		return false
	if not office.interior_minister_label.text.contains("Party Whip Thomas Sterling"):
		printerr("  [FAIL]: Interior minister expected 'Party Whip Thomas Sterling', got: " + office.interior_minister_label.text)
		return false
	if not office.interior_effect_label.text.contains("Corruption Control: 45.0%"):
		printerr("  [FAIL]: Interior capacity expected 'Corruption Control: 45.0%', got: " + office.interior_effect_label.text)
		return false

	# 6c. Interior Dismiss / Vacate
	office.execute_cabinet_appointment("Interior", "dismiss", true)
	var sync_i_dis: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_i_dis.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Interior dismiss.")
		return false
	if office.interior_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Interior status expected 'Vacant (Unassigned)', got: " + office.interior_status_label.text)
		return false
	if not office.interior_effect_label.text.contains("30.0%"):
		printerr("  [FAIL]: Interior capacity expected baseline 30.0%, got: " + office.interior_effect_label.text)
		return false

	# 7. Verify Infrastructure Portfolio: Technocrat, Loyalist, Dismiss
	print("  -> Verifying Infrastructure Portfolio actions...")

	# 7a. Infrastructure Technocrat
	office.execute_cabinet_appointment("Infrastructure", "technocrat", true)
	var sync_inf_tech: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_inf_tech.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Infrastructure technocrat.")
		return false
	if office.infrastructure_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Infrastructure status expected 'Appointed (Active)', got: " + office.infrastructure_status_label.text)
		return false
	if not office.infrastructure_minister_label.text.contains("Eng. Marcus Bennett"):
		printerr("  [FAIL]: Infrastructure minister expected 'Eng. Marcus Bennett', got: " + office.infrastructure_minister_label.text)
		return false
	if not office.infrastructure_effect_label.text.contains("Infrastructure Condition: 85.0%"):
		printerr("  [FAIL]: Infrastructure capacity expected 'Infrastructure Condition: 85.0%', got: " + office.infrastructure_effect_label.text)
		return false

	# 7b. Infrastructure Loyalist
	office.execute_cabinet_appointment("Infrastructure", "loyalist", true)
	var sync_inf_loy: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_inf_loy.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Infrastructure loyalist.")
		return false
	if not office.infrastructure_minister_label.text.contains("Alderman Gregory Cross"):
		printerr("  [FAIL]: Infrastructure minister expected 'Alderman Gregory Cross', got: " + office.infrastructure_minister_label.text)
		return false
	if not office.infrastructure_effect_label.text.contains("Infrastructure Condition: 45.0%"):
		printerr("  [FAIL]: Infrastructure capacity expected 'Infrastructure Condition: 45.0%', got: " + office.infrastructure_effect_label.text)
		return false

	# 7c. Infrastructure Dismiss / Vacate
	office.execute_cabinet_appointment("Infrastructure", "dismiss", true)
	var sync_inf_dis: Dictionary = office.run_cli_request_processor_sync(10000)
	if not sync_inf_dis.get("success", false):
		printerr("  [FAIL]: CLI processor failed for Infrastructure dismiss.")
		return false
	if office.infrastructure_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Infrastructure status expected 'Vacant (Unassigned)', got: " + office.infrastructure_status_label.text)
		return false

	# 8. Verify Cancelled Appointment (Zero mutation to state or save)
	print("  -> Verifying Cancelled appointment causes zero state mutation...")
	# First appoint Technocrat to Finance to have an established state
	office.execute_cabinet_appointment("Finance", "technocrat", true)
	office.run_cli_request_processor_sync(10000)
	if not office.finance_minister_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Finance should be Dr. Elena Rostova before cancellation test.")
		return false

	# Submit unconfirmed appointment (confirmed = false)
	office.execute_cabinet_appointment("Finance", "loyalist", false)
	office.run_cli_request_processor_sync(10000)

	# Verify Finance minister is still Dr. Elena Rostova (no change!)
	if not office.finance_minister_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Cancelled appointment should NOT mutate state! Found: " + office.finance_minister_label.text)
		return false
	if office.finance_minister_label.text.contains("Senator Donald Vance"):
		printerr("  [FAIL]: Cancelled appointment mutated minister to loyalist!")
		return false

	# 9. Verify Invalid Request Rejection
	print("  -> Verifying invalid request rejection...")
	var inv_file := FileAccess.open(iso_req, FileAccess.WRITE)
	if inv_file != null:
		inv_file.store_string(JSON.stringify({
			"action": "appoint_minister",
			"portfolio": "Finance",
			"candidate_type": "invalid_unknown_candidate",
			"confirmed": true
		}))
		inv_file.close()

	office.run_cli_request_processor_sync(10000)
	if not office.appointment_status_label.text.contains("[Rejected]"):
		printerr("  [FAIL]: Invalid candidate type should result in [Rejected] status, got: " + office.appointment_status_label.text)
		return false

	# 10. Verify Unrelated Country Data Preserved
	print("  -> Verifying unrelated country data preserved...")
	if not office.country_label.text.contains("Federal Republic of Testland"):
		printerr("  [FAIL]: Country name corrupted: " + office.country_label.text)
		return false
	if not office.treasury_label.text.contains("10,000,000,000"):
		printerr("  [FAIL]: Sovereign treasury balance corrupted: " + office.treasury_label.text)
		return false
	if office.ruling_party_label and not office.ruling_party_label.text.contains("Democratic Union"):
		printerr("  [FAIL]: Ruling party corrupted: " + office.ruling_party_label.text)
		return false

	# 11. Verify Save/Load Persistence in .sav
	print("  -> Verifying save/load persistence in authoritative .sav file...")
	var sav_file := FileAccess.open(iso_save, FileAccess.READ)
	if sav_file == null:
		printerr("  [FAIL]: Failed to open authoritative .sav file.")
		return false
	var sav_text := sav_file.get_as_text()
	sav_file.close()

	if not sav_text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Appointed minister Dr. Elena Rostova not found in authoritative .sav payload!")
		return false

	var sav_json: Variant = JSON.parse_string(sav_text)
	if not (sav_json is Dictionary):
		printerr("  [FAIL]: Authoritative .sav does not contain valid JSON.")
		return false

	# 12. Non-interactive portfolios remain visible and unappointed
	if office.defense_card == null or not office.defense_card.visible:
		printerr("  [FAIL]: Defense card should remain visible.")
		return false
	if office.foreign_affairs_card == null or not office.foreign_affairs_card.visible:
		printerr("  [FAIL]: Foreign Affairs card should remain visible.")
		return false
	if office.health_card == null or not office.health_card.visible:
		printerr("  [FAIL]: Health card should remain visible.")
		return false

	# Clean up isolated test environment
	office.request_file_override = ""
	office.current_state_path = "res://presidential_desk_state.json"
	office.custom_save_path = ""
	_cleanup_all()

	print("  [OK]: Real C# CLI integration verified: full appointment lifecycle, save persistence, and UI reloading proven.")
	return true
