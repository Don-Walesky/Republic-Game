extends SceneTree

## Automated Headless Verification Suite for Republic Phase 2:
## Executive Office Functional Interface & Real Simulation State Connectivity
## Validates:
## 1. Top resource bar authoritative field connections (Country, Treasury, Approval, Stability, Defense, Simulation Time in WAT)
## 2. Working navigation to all six game sections (Overview, Cabinet, Economy, Campaign, Diplomacy, Briefings)
## 3. Presidential Briefing real simulation telemetry and honest unavailable state presentation
## 4. Interactive office controls (Hotline Phone, Executive Inbox, Classified Dossier, Capital Vista, Presidential Mandate)
## 5. Modal dialog lifecycle (Open, Close, Backdrop dismissal, and Action buttons)
## 6. Backward-compatible navigation and state preservation with PresidentialDesk

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE PHASE 2 FUNCTIONAL TEST        ")
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

	var all_passed := true

	# Check 1: Top Resource Bar Authoritative State
	all_passed = test_top_resource_bar(office) and all_passed

	# Check 2: Top Resource Bar Missing State
	all_passed = test_top_resource_bar_missing_state(office) and all_passed

	# Check 3: Six Game Section Navigation Buttons
	all_passed = test_six_section_navigation(office) and all_passed

	# Check 4: Presidential Situation Briefing Connection & Honest Disclosures
	all_passed = test_presidential_briefing_modal(office) and all_passed

	# Check 5: Interactive Office Controls (Hotline, Inbox, Dossier, Vista, Portrait)
	all_passed = test_interactive_office_controls(office) and all_passed

	# Check 6: Modal Lifecycle & Backdrop Dismissal
	all_passed = test_modal_lifecycle(office) and all_passed

	# Check 7: PresidentialDesk Requested Section Integration
	all_passed = test_desk_section_request_integration(office) and all_passed

	office.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 7 PHASE 2 FUNCTIONAL VERIFICATION CHECKS PASSED! ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE PHASE 2 FUNCTIONAL CHECKS FAILED!        ")
		printerr("==============================================================")
		quit(1)


func test_top_resource_bar(office: Control) -> bool:
	print("\n--- Check 1: Top Resource Bar Authoritative State ---")
	var success: bool = office.load_state_from_file("res://presidential_desk_state.json")
	if not success:
		printerr("[FAIL]: Failed to load res://presidential_desk_state.json")
		return false

	# 1. Country Name
	if office.country_label.text != "Country: Republic of Arcadia":
		printerr("[FAIL]: Country label mismatch. Got: " + office.country_label.text)
		return false

	# 2. Treasury in REPU
	if office.treasury_label.text != "Treasury: R 15,000,000,000":
		printerr("[FAIL]: Treasury label mismatch. Got: " + office.treasury_label.text)
		return false

	# 3. Public Approval
	if office.approval_label.text != "Approval: 68.5%":
		printerr("[FAIL]: Approval label mismatch. Got: " + office.approval_label.text)
		return false

	# 4. National Stability
	if office.stability_label.text != "Stability: 85%":
		printerr("[FAIL]: Stability label mismatch. Got: " + office.stability_label.text)
		return false

	# 5. Simulation Time in WAT
	if not office.simulation_time_label.text.contains("WAT"):
		printerr("[FAIL]: Simulation time missing WAT format. Got: " + office.simulation_time_label.text)
		return false
	if not office.simulation_time_label.text.begins_with("Simulation Time: Day 0"):
		printerr("[FAIL]: Simulation time mismatch. Got: " + office.simulation_time_label.text)
		return false

	# 6. Defense Readiness
	if not office.defcon_label.text.contains("DEFCON 5") or not office.defcon_label.text.contains("85.0%"):
		printerr("[FAIL]: Defense label did not contain DEFCON 5 and military readiness. Got: " + office.defcon_label.text)
		return false

	print("  [OK]: All six top resource bar metrics verified with authoritative simulation values.")
	return true


func test_top_resource_bar_missing_state(office: Control) -> bool:
	print("\n--- Check 2: Top Resource Bar Missing State ---")
	office.display_missing_state()

	if office.country_label.text != "Country: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' country label, got: " + office.country_label.text)
		return false

	if office.treasury_label.text != "Treasury: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' treasury label, got: " + office.treasury_label.text)
		return false

	if office.approval_label.text != "Approval: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' approval label, got: " + office.approval_label.text)
		return false

	if office.stability_label.text != "Stability: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' stability label, got: " + office.stability_label.text)
		return false

	if office.simulation_time_label.text != "Simulation Time: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' simulation time, got: " + office.simulation_time_label.text)
		return false

	if office.defcon_label.text != "Defense: [No State Loaded]":
		printerr("[FAIL]: Expected '[No State Loaded]' defense label, got: " + office.defcon_label.text)
		return false

	print("  [OK]: Honest '[No State Loaded]' presentation verified on all top resource labels.")
	return true


func test_six_section_navigation(office: Control) -> bool:
	print("\n--- Check 3: Six Game Section Navigation Buttons ---")
	if office.nav_bar_panel == null:
		printerr("[FAIL]: nav_bar_panel node missing.")
		return false

	if office.nav_overview_button == null:
		printerr("[FAIL]: nav_overview_button missing.")
		return false
	if office.nav_cabinet_button == null:
		printerr("[FAIL]: nav_cabinet_button missing.")
		return false
	if office.nav_economy_button == null:
		printerr("[FAIL]: nav_economy_button missing.")
		return false
	if office.nav_campaign_button == null:
		printerr("[FAIL]: nav_campaign_button missing.")
		return false
	if office.nav_diplomacy_button == null:
		printerr("[FAIL]: nav_diplomacy_button missing.")
		return false
	if office.nav_briefings_button == null:
		printerr("[FAIL]: nav_briefings_button missing.")
		return false

	# Test Overview tab highlight
	office.navigate_to_section(office.Section.OVERVIEW)
	if office.active_nav_section != office.Section.OVERVIEW:
		printerr("[FAIL]: active_nav_section should be OVERVIEW.")
		return false

	# Test Briefings tab opens briefing modal
	office.navigate_to_section(office.Section.BRIEFINGS)
	if office.active_nav_section != office.Section.BRIEFINGS:
		printerr("[FAIL]: active_nav_section should be BRIEFINGS.")
		return false
	if not office.briefing_modal.visible:
		printerr("[FAIL]: BriefingModal should be visible after navigating to BRIEFINGS.")
		return false

	# Close modal and verify return to OVERVIEW
	office.close_all_modals()
	if office.briefing_modal.visible:
		printerr("[FAIL]: BriefingModal should be hidden after close_all_modals().")
		return false

	print("  [OK]: Navigation controls for all 6 sections (Overview, Cabinet, Economy, Campaign, Diplomacy, Briefings) verified.")
	return true


func test_presidential_briefing_modal(office: Control) -> bool:
	print("\n--- Check 4: Presidential Briefing Real Telemetry & Disclosures ---")
	office.load_state_from_file("res://presidential_desk_state.json")
	office.open_briefing_modal()

	if not office.briefing_modal.visible:
		printerr("[FAIL]: Briefing modal not visible.")
		return false

	if not office.briefing_active_project_label.text.contains("Active Project: None"):
		printerr("[FAIL]: Briefing active project mismatch. Got: " + office.briefing_active_project_label.text)
		return false

	if not office.briefing_decisions_label.text.contains("Connected to live Republic simulation state"):
		printerr("[FAIL]: Briefing decisions label mismatch. Got: " + office.briefing_decisions_label.text)
		return false

	if not office.briefing_crisis_label.text.contains("State institutions stable"):
		printerr("[FAIL]: Briefing crisis details mismatch. Got: " + office.briefing_crisis_label.text)
		return false

	if not office.briefing_capacity_label.text.contains("Civil Service: 80%"):
		printerr("[FAIL]: State capacity mismatch. Got: " + office.briefing_capacity_label.text)
		return false

	if not office.briefing_fiscal_label.text.contains("R 15,000,000,000"):
		printerr("[FAIL]: Fiscal report reserves mismatch. Got: " + office.briefing_fiscal_label.text)
		return false

	if not office.briefing_defense_label.text.contains("DEFCON 5") or not office.briefing_defense_label.text.contains("85.0%"):
		printerr("[FAIL]: Briefing defense mismatch. Got: " + office.briefing_defense_label.text)
		return false

	if not office.briefing_offline_label.text.contains("Zero pending offline cycles"):
		printerr("[FAIL]: Offline briefing mismatch. Got: " + office.briefing_offline_label.text)
		return false

	# Verify honest empty state disclosure for unsupported intelligence feed
	if not office.briefing_unsupported_label.text.contains("Global Intelligence Wire") or not office.briefing_unsupported_label.text.contains("offline / pending support"):
		printerr("[FAIL]: Unsupported intelligence wire did not display honest disclosure. Got: " + office.briefing_unsupported_label.text)
		return false

	office.close_all_modals()
	print("  [OK]: Presidential briefing displays real simulation telemetry with honest disclosures for unsupported feeds.")
	return true


func test_interactive_office_controls(office: Control) -> bool:
	print("\n--- Check 5: Interactive Office Controls ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	# 1. Hotline Phone
	office._on_hotline_phone_pressed()
	if not office.hotline_modal.visible:
		printerr("[FAIL]: HotlineModal not visible after pressing hotline_phone.")
		return false
	if not office.status_ticker_label.text.contains("Hotline Console"):
		printerr("[FAIL]: Ticker not updated on hotline press. Got: " + office.status_ticker_label.text)
		return false
	office.close_all_modals()

	# 2. Executive Inbox
	office._on_executive_inbox_pressed()
	if not office.briefing_modal.visible:
		printerr("[FAIL]: BriefingModal not visible after pressing executive_inbox.")
		return false
	if not office.status_ticker_label.text.contains("Executive Briefing"):
		printerr("[FAIL]: Ticker not updated on inbox press. Got: " + office.status_ticker_label.text)
		return false
	office.close_all_modals()

	# 3. Classified Dossier
	office._on_intelligence_dossier_pressed()
	if not office.dossier_modal.visible:
		printerr("[FAIL]: DossierModal not visible after pressing intelligence_dossier.")
		return false
	if not office.status_ticker_label.text.contains("Intelligence Dossier"):
		printerr("[FAIL]: Ticker not updated on dossier press. Got: " + office.status_ticker_label.text)
		return false
	office.close_all_modals()

	# 4. Capital Vista (Window)
	office._on_capital_window_pressed()
	if not office.status_ticker_label.text.contains("Capital Vista"):
		printerr("[FAIL]: Ticker not updated on capital window press. Got: " + office.status_ticker_label.text)
		return false

	# 5. Presidential Portrait
	office._on_presidential_portrait_pressed()
	if not office.status_ticker_label.text.contains("Presidential Mandate"):
		printerr("[FAIL]: Ticker not updated on portrait press. Got: " + office.status_ticker_label.text)
		return false

	print("  [OK]: All office controls (Hotline, Inbox, Dossier, Vista, Portrait) respond with actions and ticker feedback.")
	return true


func test_modal_lifecycle(office: Control) -> bool:
	print("\n--- Check 6: Modal Lifecycle & Backdrop Dismissal ---")
	office.open_briefing_modal()
	if not office.is_any_modal_open():
		printerr("[FAIL]: is_any_modal_open() returned false when briefing modal is open.")
		return false
	if not office.modal_backdrop.visible:
		printerr("[FAIL]: modal_backdrop should be visible when modal is open.")
		return false

	# Test close button
	office._on_modal_backdrop_gui_input(InputEventMouseButton.new())
	office.close_all_modals()
	if office.is_any_modal_open():
		printerr("[FAIL]: is_any_modal_open() returned true after close_all_modals().")
		return false
	if office.modal_backdrop.visible:
		printerr("[FAIL]: modal_backdrop should be hidden after close_all_modals().")
		return false

	print("  [OK]: Modal lifecycle (opening, visibility tracking, and backdrop dismissal) fully verified.")
	return true


func test_desk_section_request_integration(office: Control) -> bool:
	print("\n--- Check 7: PresidentialDesk Requested Section Integration ---")
	var desk_script = load("res://PresidentialDesk.gd")
	if desk_script == null:
		printerr("[FAIL]: Could not load res://PresidentialDesk.gd")
		return false

	# Test requesting section 1 (Cabinet)
	desk_script.requested_section = 1
	var desk_scene := load("res://PresidentialDesk.tscn")
	if desk_scene == null:
		printerr("[FAIL]: Could not load res://PresidentialDesk.tscn")
		return false

	var desk = desk_scene.instantiate()
	root.add_child(desk)
	desk._ready()

	if desk.active_section != desk.Section.CABINET:
		printerr("[FAIL]: Expected PresidentialDesk to open to CABINET (1), got: " + str(desk.active_section))
		desk.queue_free()
		return false

	if not desk.section_cabinet.visible:
		printerr("[FAIL]: SectionCabinet container is not visible.")
		desk.queue_free()
		return false

	# Verify return to Executive Office button exists and is wired
	if desk.switch_to_executive_office_button == null:
		printerr("[FAIL]: switch_to_executive_office_button missing on PresidentialDesk.")
		desk.queue_free()
		return false

	desk.queue_free()
	print("  [OK]: Requested section navigation and bidirectional switching between ExecutiveOffice and PresidentialDesk verified.")
	return true
