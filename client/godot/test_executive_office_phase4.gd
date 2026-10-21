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
## 7. Action pipeline request generation (presidential_desk_request.json appoint_minister)
## 8. Progressive dock integration & navigation to PresidentialDesk cabinet section

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE PHASE 4 CABINET TEST           ")
	print("==============================================================")

	var scene_res := load("res://ExecutiveOffice.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var office = scene_res.instantiate()
	if office == null:
		printerr("[FAIL]: Failed to instantiate ExecutiveOffice node")
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

	office.queue_free()

	# Clean up any test artifacts
	var temp_files := [
		"res://temp_phase4_request.json",
		"res://presidential_desk_request.json"
	]
	for tf in temp_files:
		if FileAccess.file_exists(tf):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(tf))

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 8 PHASE 4 CABINET VERIFICATION CHECKS PASSED!    ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE PHASE 4 CABINET VERIFICATION CHECKS FAILED ")
		printerr("==============================================================")
		quit(1)

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
			printerr("  [FAIL]: Portfolio card missing: %s" % p_card_name)
			return false

	# Verify supported interactive appointment buttons exist
	if office.appoint_finance_button == null:
		printerr("  [FAIL]: appoint_finance_button is null.")
		return false
	if office.appoint_interior_button == null:
		printerr("  [FAIL]: appoint_interior_button is null.")
		return false
	if office.appoint_infrastructure_button == null:
		printerr("  [FAIL]: appoint_infrastructure_button is null.")
		return false

	print("  [OK]: All 6 constitutional portfolios rendered with active appointment hooks.")
	return true

# ------------------------------------------------------------------------------
# Check 3: Authoritative State Loading (Appointed Ministers & Capacity)
# ------------------------------------------------------------------------------
func test_authoritative_state_loading(office: Node) -> bool:
	print("\n--- Check 3: Authoritative State Loading (Appointed Ministers & Capacity) ---")

	var mock_state: Dictionary = {
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 5200000.0,
		"treasury": "R 5,200,000",
		"approval_rating": 74.0,
		"approval": "74%",
		"stability_rating": 80.0,
		"stability": "80%",
		"simulation_time": "Day 12, 10:00 WAT",
		"ruling_party": "Democratic Coalition",
		"polling_support": 56.5,
		"civil_service_quality": 0.90,
		"corruption_control": 0.85,
		"infrastructure_condition": 0.88,
		"defense_minister": "Gen. Jack Sterling",
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
				"state_capacity_value": 0.90
			},
			{
				"portfolio": "Interior",
				"is_appointed": true,
				"status": "Appointed",
				"minister_name": "Dr. Alistair Finch",
				"competence": 0.88,
				"loyalty": 0.55,
				"integrity": 0.92,
				"monthly_salary": 140000.0,
				"state_capacity_metric": "Corruption Control",
				"state_capacity_value": 0.85
			},
			{
				"portfolio": "Infrastructure",
				"is_appointed": true,
				"status": "Appointed",
				"minister_name": "Eng. Marcus Bennett",
				"competence": 0.90,
				"loyalty": 0.60,
				"integrity": 0.85,
				"monthly_salary": 145000.0,
				"state_capacity_metric": "Infrastructure Condition",
				"state_capacity_value": 0.88
			}
		]
	}

	office.apply_state(mock_state)

	# Verify Finance
	if office.finance_status_label.text != "Appointed (Active)":
		printerr("  [FAIL]: Finance status expected 'Appointed (Active)', got: %s" % office.finance_status_label.text)
		return false
	if not office.finance_minister_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Finance minister expected 'Dr. Elena Rostova', got: %s" % office.finance_minister_label.text)
		return false
	if not office.finance_ratings_label.text.contains("90%"):
		printerr("  [FAIL]: Finance ratings expected competence 90%, got: %s" % office.finance_ratings_label.text)
		return false
	if not office.finance_effect_label.text.contains("Civil Service Quality: 90.0%"):
		printerr("  [FAIL]: Finance effect expected 'Civil Service Quality: 90.0%', got: %s" % office.finance_effect_label.text)
		return false

	# Verify Interior
	if not office.interior_minister_label.text.contains("Dr. Alistair Finch"):
		printerr("  [FAIL]: Interior minister expected 'Dr. Alistair Finch', got: %s" % office.interior_minister_label.text)
		return false
	if not office.interior_effect_label.text.contains("Corruption Control: 85.0%"):
		printerr("  [FAIL]: Interior effect expected 'Corruption Control: 85.0%', got: %s" % office.interior_effect_label.text)
		return false

	# Verify Infrastructure
	if not office.infrastructure_minister_label.text.contains("Eng. Marcus Bennett"):
		printerr("  [FAIL]: Infrastructure minister expected 'Eng. Marcus Bennett', got: %s" % office.infrastructure_minister_label.text)
		return false

	# Verify Defense (top-level fallback)
	if not office.defense_minister_label.text.contains("Gen. Jack Sterling"):
		printerr("  [FAIL]: Defense minister fallback expected 'Gen. Jack Sterling', got: %s" % office.defense_minister_label.text)
		return false

	print("  [OK]: Authoritative minister names, key ratings, and capacity effects loaded faithfully.")
	return true

# ------------------------------------------------------------------------------
# Check 4: Vacant Portfolios & Baseline Capacity
# ------------------------------------------------------------------------------
func test_vacant_portfolios_and_baseline(office: Node) -> bool:
	print("\n--- Check 4: Vacant Portfolios & Baseline Capacity ---")

	var vacant_state: Dictionary = {
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 1000000.0,
		"civil_service_quality": 0.30,
		"corruption_control": 0.30,
		"infrastructure_condition": 0.30,
		"cabinet": [
			{
				"portfolio": "Finance",
				"is_appointed": false,
				"status": "Vacant",
				"minister_name": "Vacant"
			},
			{
				"portfolio": "Interior",
				"is_appointed": false,
				"status": "Vacant",
				"minister_name": "Vacant"
			},
			{
				"portfolio": "Infrastructure",
				"is_appointed": false,
				"status": "Vacant",
				"minister_name": "Vacant"
			}
		]
	}

	office.apply_state(vacant_state)

	if office.finance_status_label.text != "Vacant (Unassigned)":
		printerr("  [FAIL]: Finance expected 'Vacant (Unassigned)', got: %s" % office.finance_status_label.text)
		return false
	if office.finance_minister_label.text != "Minister: [Vacant]":
		printerr("  [FAIL]: Finance minister expected 'Minister: [Vacant]', got: %s" % office.finance_minister_label.text)
		return false
	if office.finance_ratings_label.text != "Ratings: --":
		printerr("  [FAIL]: Finance ratings expected 'Ratings: --', got: %s" % office.finance_ratings_label.text)
		return false
	if not office.finance_effect_label.text.contains("30.0%"):
		printerr("  [FAIL]: Finance baseline effect expected 30.0%, got: %s" % office.finance_effect_label.text)
		return false

	print("  [OK]: Vacant portfolios show honest unassigned presentation and baseline capacity.")
	return true

# ------------------------------------------------------------------------------
# Check 5: Missing State Safe Fallback
# ------------------------------------------------------------------------------
func test_missing_state_safe_fallback(office: Node) -> bool:
	print("\n--- Check 5: Missing State Safe Fallback ---")

	office.display_missing_state()

	var labels: Array[Label] = [
		office.finance_status_label,
		office.defense_status_label,
		office.foreign_affairs_status_label,
		office.interior_status_label,
		office.health_status_label,
		office.infrastructure_status_label
	]

	for lbl in labels:
		if lbl != null and lbl.text != "Vacant (Unassigned)":
			printerr("  [FAIL]: Expected 'Vacant (Unassigned)' on missing state, got: %s" % lbl.text)
			return false

	var minister_labels: Array[Label] = [
		office.finance_minister_label,
		office.defense_minister_label,
		office.foreign_affairs_minister_label,
		office.interior_minister_label,
		office.health_minister_label,
		office.infrastructure_minister_label
	]

	for mlbl in minister_labels:
		if mlbl != null and mlbl.text != "Minister: [Vacant]":
			printerr("  [FAIL]: Expected 'Minister: [Vacant]' on missing state, got: %s" % mlbl.text)
			return false

	print("  [OK]: Missing state safely handles all 6 portfolios without mock data or errors.")
	return true

# ------------------------------------------------------------------------------
# Check 6: Appointment Modal & Consequence Previews
# ------------------------------------------------------------------------------
func test_appointment_modal_and_previews(office: Node) -> bool:
	print("\n--- Check 6: Appointment Modal & Consequence Previews ---")

	# Populate state so modal has context
	office.apply_state({
		"country_name": "Federal Republic of Testland",
		"treasury_balance": 3000000.0,
		"civil_service_quality": 0.30
	})

	# 1. Open Finance modal
	office.open_appointment_modal("Finance")
	if not office.cabinet_appointment_modal.visible:
		printerr("  [FAIL]: Cabinet appointment modal should be visible.")
		return false
	if not office.modal_backdrop.visible:
		printerr("  [FAIL]: Modal backdrop should be visible.")
		return false
	if not office.appointment_portfolio_label.text.contains("Finance"):
		printerr("  [FAIL]: Portfolio label should mention Finance.")
		return false

	# Default is technocrat
	if not office.candidate_preview_name_label.text.contains("Dr. Elena Rostova"):
		printerr("  [FAIL]: Finance technocrat name should be Elena Rostova, got: %s" % office.candidate_preview_name_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("+1.5%"):
		printerr("  [FAIL]: Technocrat consequences should mention +1.5% approval, got: %s" % office.candidate_preview_consequences_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("-3.0%"):
		printerr("  [FAIL]: Technocrat consequences should mention -3.0% polling, got: %s" % office.candidate_preview_consequences_label.text)
		return false
	if not office.candidate_preview_salary_label.text.contains("150,000"):
		printerr("  [FAIL]: Technocrat salary should be 150,000, got: %s" % office.candidate_preview_salary_label.text)
		return false

	# 2. Switch to Loyalist
	office._on_candidate_loyalist_pressed()
	if not office.candidate_preview_name_label.text.contains("Donald Vance"):
		printerr("  [FAIL]: Finance loyalist should be Donald Vance, got: %s" % office.candidate_preview_name_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("-1.5%"):
		printerr("  [FAIL]: Loyalist consequences should mention -1.5% approval, got: %s" % office.candidate_preview_consequences_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("+3.0%"):
		printerr("  [FAIL]: Loyalist consequences should mention +3.0% polling, got: %s" % office.candidate_preview_consequences_label.text)
		return false

	# 3. Switch to Dismiss
	office._on_candidate_dismiss_pressed()
	if not office.candidate_preview_name_label.text.contains("Vacate") and not office.candidate_preview_name_label.text.contains("Dismiss"):
		printerr("  [FAIL]: Dismiss candidate name should mention Dismiss or Vacate, got: %s" % office.candidate_preview_name_label.text)
		return false
	if not office.candidate_preview_salary_label.text.contains("R 0"):
		printerr("  [FAIL]: Dismiss salary should be R 0, got: %s" % office.candidate_preview_salary_label.text)
		return false
	if not office.candidate_preview_consequences_label.text.contains("30%"):
		printerr("  [FAIL]: Dismiss consequences should mention baseline 30%, got: %s" % office.candidate_preview_consequences_label.text)
		return false

	# 4. Check Interior modal
	office.open_appointment_modal("Interior")
	if not office.candidate_preview_name_label.text.contains("Dr. Alistair Finch"):
		printerr("  [FAIL]: Interior technocrat should be Alistair Finch, got: %s" % office.candidate_preview_name_label.text)
		return false

	# 5. Check Infrastructure modal
	office.open_appointment_modal("Infrastructure")
	if not office.candidate_preview_name_label.text.contains("Marcus Bennett"):
		printerr("  [FAIL]: Infrastructure technocrat should be Marcus Bennett, got: %s" % office.candidate_preview_name_label.text)
		return false

	office.close_all_modals()
	if office.cabinet_appointment_modal.visible:
		printerr("  [FAIL]: Cabinet appointment modal should be hidden after close_all_modals().")
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

	# Ensure state loaded
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
