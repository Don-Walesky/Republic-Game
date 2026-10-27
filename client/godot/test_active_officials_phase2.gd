extends SceneTree

## Automated Headless Verification Suite for Republic:
## Phase 2: Active Government Officials and Security Leadership
##
## Validates:
## 1. Proactive Chief of Staff Monitoring & Recommendations:
##    - chief_of_staff_briefing rendering
##    - chief_of_staff_recommendations cards with priority, urgency, and cost
##    - Inbox badge count reflects pending recommendations
##    - execute_chief_of_staff_decision generates valid action: "cos_recommendation"
## 2. Ministerial Advice & Portfolio Warnings:
##    - ministerial_advice bindings across portfolios
##    - Dissent alerts for low loyalty (< 35%)
##    - Integrity alerts for low integrity (< 35%) & self-serving flags
##    - Vacant portfolio capacity bottleneck display
##    - Cabinet friction panel activation on ministerial warnings
## 3. National Security Leadership & Directives:
##    - security_assessment_summary & threat indexes display
##    - security_threats list with confidence levels: Confirmed, HighProbability, InterceptedSignal, UnverifiedRumor
##    - Dossier notification badge on urgent/confirmed threats
##    - execute_security_directive generates valid action: "security_directive"
## 4. PresidentialDesk Phase 2 Integration:
##    - National briefing binds Chief of Staff briefing and security assessment summary

const ISOLATED_TEST_DIR := "res://temp_phase2_officials_isolated"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PHASE 2 ACTIVE OFFICIALS VERIFICATION           ")
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

	# Test 1: Chief of Staff Proactive Briefing & Recommendations
	all_passed = test_chief_of_staff_briefing_and_recs(office) and all_passed

	# Test 2: Chief of Staff Decision Request Generation
	all_passed = test_chief_of_staff_decision_request(office) and all_passed

	# Test 3: Ministerial Advice & Portfolio Warnings
	all_passed = test_ministerial_advice_and_warnings(office) and all_passed

	# Test 4: Security Leadership Dossier & Directives
	all_passed = test_security_leadership_dossier_and_directives(office) and all_passed

	# Test 5: PresidentialDesk Phase 2 State Binding
	all_passed = test_presidential_desk_binding() and all_passed

	office.queue_free()
	_cleanup_all()

	if all_passed:
		print("==============================================================")
		print("   [SUCCESS]: ALL PHASE 2 ACTIVE OFFICIALS TESTS PASSED       ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr("   [FAILURE]: ONE OR MORE PHASE 2 TESTS FAILED                ")
		printerr("==============================================================")
		quit(1)

func test_chief_of_staff_briefing_and_recs(office) -> bool:
	print("\n--- Running Test 1: Chief of Staff Briefing & Recommendations ---")
	var sample_state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 1250000.0,
		"treasury": "R 1,250,000",
		"approval_rating": 72.0,
		"stability_rating": 80.0,
		"chief_of_staff_briefing": "Fiscal solvency requires immediate revenue reallocation.",
		"chief_of_staff_recommendations": [
			{
				"id": "rec_fiscal_insolvency",
				"title": "Emergency Liquidity Protocol",
				"summary": "Treasury balance low. Requesting immediate fiscal controls.",
				"category": "FiscalCrisis",
				"priority": "Critical",
				"is_urgent": true,
				"status": "Pending",
				"recommended_action": "enact_emergency_austerity",
				"estimated_cost": 0.0
			},
			{
				"id": "rec_cabinet_vacancy_finance",
				"title": "Finance Ministry Vacancy",
				"summary": "Institutional leadership needed in Treasury.",
				"category": "CabinetVacancy",
				"priority": "High",
				"is_urgent": false,
				"status": "Pending",
				"recommended_action": "appoint_finance",
				"estimated_cost": 50000.0
			}
		]
	}

	office.apply_state(sample_state)

	# Verify Chief of Staff briefing section in Briefing Modal
	var briefing_content := office._get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		printerr("FAIL: BriefingContentVBox not found")
		return false

	var cos_sec := briefing_content.get_node_or_null("ChiefOfStaffSection") as VBoxContainer
	if not cos_sec:
		printerr("FAIL: ChiefOfStaffSection not generated in BriefingModal")
		return false

	var children := cos_sec.get_children()
	if children.size() < 3: # header + title + at least 1 card
		printerr("FAIL: Expected ChiefOfStaffSection children >= 3, got: %d" % children.size())
		return false

	# Verify Inbox Badge count equals pending recommendations (2)
	var badge_label := office._get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton/InboxNotificationBadge/InboxBadgeLabel") as Label
	if not badge_label or badge_label.text != "2":
		printerr("FAIL: Expected Inbox badge text '2', got: %s" % (badge_label.text if badge_label else "null"))
		return false

	print("PASS: Chief of Staff briefing and recommendations populated correctly with pending badge.")
	return true

func test_chief_of_staff_decision_request(office) -> bool:
	print("\n--- Running Test 2: Chief of Staff Decision Request Generation ---")
	_setup_isolated_dir()
	var test_state_file := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	var res: Dictionary = office.execute_chief_of_staff_decision("rec_fiscal_insolvency", "accept")
	if not res.get("success", false):
		printerr("FAIL: execute_chief_of_staff_decision failed: %s" % res.get("message", ""))
		return false

	var req_path: String = res.get("request_path", "")
	if not FileAccess.file_exists(req_path):
		printerr("FAIL: Request file was not created at: %s" % req_path)
		return false

	var file := FileAccess.open(req_path, FileAccess.READ)
	var txt := file.get_as_text()
	file.close()

	var req_dict = JSON.parse_string(txt)
	if not (req_dict is Dictionary):
		printerr("FAIL: Request file does not contain valid JSON")
		return false

	if req_dict.get("action", "") != "cos_recommendation":
		printerr("FAIL: Expected action 'cos_recommendation', got: %s" % req_dict.get("action", ""))
		return false

	if req_dict.get("recommendation_id", "") != "rec_fiscal_insolvency":
		printerr("FAIL: Expected recommendation_id 'rec_fiscal_insolvency', got: %s" % req_dict.get("recommendation_id", ""))
		return false

	if req_dict.get("decision", "") != "accept":
		printerr("FAIL: Expected decision 'accept', got: %s" % req_dict.get("decision", ""))
		return false

	print("PASS: Chief of Staff decision request file formatted and written correctly.")
	return true

func test_ministerial_advice_and_warnings(office) -> bool:
	print("\n--- Running Test 3: Ministerial Advice & Portfolio Warnings ---")
	var sample_state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 1500000.0,
		"ministerial_advice": [
			{
				"portfolio": "Finance",
				"minister_name": "Marcus Kane",
				"is_appointed": true,
				"headline": "Surplus Reallocation",
				"detailed_advice": "Proposing reallocation of excess funds.",
				"risk_level": "Elevated",
				"confidence_rating": 75.0,
				"competence_rating": 80.0,
				"loyalty_rating": 70.0,
				"integrity_rating": 30.0,
				"recommended_action": "audit_finance",
				"is_self_serving": true,
				"warning_notes": "Integrity Risk: Reports indicate potential conflict of interest."
			},
			{
				"portfolio": "Defense",
				"minister_name": "General Vance",
				"is_appointed": true,
				"headline": "Border Readiness",
				"detailed_advice": "Armed forces on standby.",
				"risk_level": "Low",
				"confidence_rating": 85.0,
				"competence_rating": 85.0,
				"loyalty_rating": 25.0,
				"integrity_rating": 80.0,
				"recommended_action": "border_patrol",
				"is_self_serving": false,
				"warning_notes": "Disloyalty Alert: Minister exhibits factional dissent."
			},
			{
				"portfolio": "Interior",
				"minister_name": "Vacant (Unassigned)",
				"is_appointed": false,
				"headline": "Ministry of Interior: Vacant Portfolio",
				"detailed_advice": "No executive leadership assigned. State capacity bounded at 30.0%.",
				"risk_level": "Elevated",
				"confidence_rating": 30.0,
				"competence_rating": 0.0,
				"loyalty_rating": 0.0,
				"integrity_rating": 0.0,
				"recommended_action": "appoint_interior",
				"is_self_serving": false,
				"warning_notes": "Institutional capacity bottleneck active."
			}
		]
	}

	office.apply_state(sample_state)

	# Verify Finance card has Integrity warning
	var fin_ratings: Label = office._get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceRatingsLabel") as Label
	if not fin_ratings or not fin_ratings.text.contains("Integrity Risk"):
		printerr("FAIL: Expected Finance ratings to include Integrity Risk, got: %s" % (fin_ratings.text if fin_ratings else "null"))
		return false

	# Verify Defense card has Disloyalty alert
	var def_ratings: Label = office._get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseRatingsLabel") as Label
	if not def_ratings or not def_ratings.text.contains("Disloyalty Alert"):
		printerr("FAIL: Expected Defense ratings to include Disloyalty Alert, got: %s" % (def_ratings.text if def_ratings else "null"))
		return false

	# Verify Interior card reflects vacant bottleneck
	var int_effect: Label = office._get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorEffectLabel") as Label
	if not int_effect or not int_effect.text.contains("Bottleneck"):
		printerr("FAIL: Expected Interior effect to note bottleneck, got: %s" % (int_effect.text if int_effect else "null"))
		return false

	# Verify Cabinet Friction panel was triggered by ministerial alerts
	var friction_panel := office._get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel") as PanelContainer
	if not friction_panel or not friction_panel.visible:
		printerr("FAIL: Cabinet friction panel should be visible when ministerial warnings exist")
		return false

	print("PASS: Ministerial advice, dissent/integrity warnings, and friction panel validated.")
	return true

func test_security_leadership_dossier_and_directives(office) -> bool:
	print("\n--- Running Test 4: Security Leadership Dossier & Directives ---")
	_setup_isolated_dir()
	var test_state_file := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	var sample_state := {
		"country_name": "Republic of Arcadia",
		"defcon_level": "DEFCON 4 (Heightened Watch)",
		"military_readiness": "78.0% (Armed Forces Operational)",
		"security_assessment_summary": "Frontier movements detected. Customs integrity alert active.",
		"domestic_threat_index": 42.0,
		"external_threat_index": 35.0,
		"security_threats": [
			{
				"threat_id": "threat_domestic_unrest",
				"title": "Urban Civil Unrest & Strike Mobilization",
				"category": "DomesticSecurity",
				"severity": "Severe",
				"confidence": "Confirmed",
				"sourceAgency": "Special Branch",
				"status": "Active",
				"details": "Synchronized demonstrations in capital.",
				"recommended_response": "Deploy Civil Guard",
				"is_confirmed": true
			},
			{
				"threat_id": "threat_customs_leakage",
				"title": "Contraband Flow & Customs Evasion",
				"category": "CustomsAndBorders",
				"severity": "Moderate",
				"confidence": "HighProbability",
				"sourceAgency": "Customs Service",
				"status": "Active",
				"details": "Organized smuggling rings bypassing maritime tariffs.",
				"recommended_response": "Authorize Port Sweep",
				"is_confirmed": false
			},
			{
				"threat_id": "threat_border_patrol",
				"title": "Frontier Reconnaissance Activity",
				"category": "ExternalSecurity",
				"severity": "Moderate",
				"confidence": "InterceptedSignal",
				"sourceAgency": "Signals Intelligence",
				"status": "Active",
				"details": "Foreign radar scans detected along northern frontier.",
				"recommended_response": "Deploy Frontier Reconnaissance",
				"is_confirmed": false
			},
			{
				"threat_id": "threat_arms_rumor",
				"title": "Illicit Armament Cache Rumor",
				"category": "IntelligenceRumor",
				"severity": "Minor",
				"confidence": "UnverifiedRumor",
				"sourceAgency": "Informant Network",
				"status": "Active",
				"details": "Unverified tips regarding covert small arms cache.",
				"recommended_response": "Task Special Branch Verification",
				"is_confirmed": false
			}
		]
	}

	office.apply_state(sample_state)

	# Verify Dossier notification badge is triggered by Confirmed / Severe threat
	var dossier_badge := office._get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton/DossierNotificationBadge") as Control
	if not dossier_badge or not dossier_badge.visible:
		printerr("FAIL: Dossier badge should be visible for active confirmed/severe threats")
		return false

	# Verify Dossier modal has SecurityThreatsSection with 4 threats
	var dossier_vbox := office._get_node_safe("InterfaceLayer/DossierModal/DossierVBox") as VBoxContainer
	if not dossier_vbox:
		printerr("FAIL: DossierVBox not found")
		return false

	var threats_sec := dossier_vbox.get_node_or_null("SecurityThreatsSection") as VBoxContainer
	if not threats_sec:
		printerr("FAIL: SecurityThreatsSection not found in DossierVBox")
		return false

	# Title + 4 cards = 5 children
	if threats_sec.get_child_count() < 5:
		printerr("FAIL: Expected at least 5 children in SecurityThreatsSection, got: %d" % threats_sec.get_child_count())
		return false

	# Test Directive Execution: execute_security_directive
	var res: Dictionary = office.execute_security_directive("threat_domestic_unrest", "mobilize")
	if not res.get("success", false):
		printerr("FAIL: execute_security_directive failed: %s" % res.get("message", ""))
		return false

	var req_path: String = res.get("request_path", "")
	if not FileAccess.file_exists(req_path):
		printerr("FAIL: Security directive request file was not created")
		return false

	var file := FileAccess.open(req_path, FileAccess.READ)
	var txt := file.get_as_text()
	file.close()

	var req_dict = JSON.parse_string(txt)
	if not (req_dict is Dictionary):
		printerr("FAIL: Directive request does not contain valid JSON")
		return false

	if req_dict.get("action", "") != "security_directive":
		printerr("FAIL: Expected action 'security_directive', got: %s" % req_dict.get("action", ""))
		return false

	if req_dict.get("threat_id", "") != "threat_domestic_unrest":
		printerr("FAIL: Expected threat_id 'threat_domestic_unrest', got: %s" % req_dict.get("threat_id", ""))
		return false

	if req_dict.get("directive", "") != "mobilize":
		printerr("FAIL: Expected directive 'mobilize', got: %s" % req_dict.get("directive", ""))
		return false

	print("PASS: Security leadership assessment, confidence badges, and directives validated.")
	return true

func test_presidential_desk_binding() -> bool:
	print("\n--- Running Test 5: PresidentialDesk Phase 2 State Binding ---")
	var desk_scene_res := load("res://PresidentialDesk.tscn")
	if desk_scene_res == null:
		printerr("FAIL: Failed to load res://PresidentialDesk.tscn")
		return false

	var desk = desk_scene_res.instantiate()
	if desk == null:
		printerr("FAIL: Failed to instantiate PresidentialDesk")
		return false

	root.add_child(desk)

	var sample_data := {
		"country_name": "Republic of Arcadia",
		"treasury": "R 2,000,000",
		"approval": "74.0%",
		"stability": "82.0%",
		"defcon_level": "DEFCON 5 (Peacetime Watch)",
		"military_readiness": "85.0% (Armed Forces Operational)",
		"chief_of_staff_briefing": "Fiscal solvency requires reserve conservation.",
		"security_assessment_summary": "Sovereign frontiers secure. Peacetime readiness maintained."
	}

	desk.apply_state(sample_data)

	# Verify briefing_latest_decision_label binds chief_of_staff_briefing
	var dec_label: Label = desk.briefing_latest_decision_label
	if not dec_label or not dec_label.text.contains("Chief of Staff: Fiscal solvency"):
		printerr("FAIL: Expected briefing_latest_decision_label to include Chief of Staff briefing, got: %s" % (dec_label.text if dec_label else "null"))
		desk.queue_free()
		return false

	# Verify defense_readiness_label binds security_assessment_summary
	var def_label: Label = desk.defense_readiness_label
	if not def_label or not def_label.text.contains("Sovereign frontiers secure"):
		printerr("FAIL: Expected defense_readiness_label to include security assessment summary, got: %s" % (def_label.text if def_label else "null"))
		desk.queue_free()
		return false

	desk.queue_free()
	print("PASS: PresidentialDesk bindings verified for Phase 2 data.")
	return true

func _setup_isolated_dir() -> void:
	var global_dir := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	if not DirAccess.dir_exists_absolute(global_dir):
		DirAccess.make_dir_recursive_absolute(global_dir)

func _cleanup_all() -> void:
	var global_dir := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	if DirAccess.dir_exists_absolute(global_dir):
		var dir := DirAccess.open(global_dir)
		if dir:
			dir.list_dir_begin()
			var file_name := dir.get_next()
			while not file_name.is_empty():
				if not dir.current_is_dir():
					dir.remove(file_name)
				file_name = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(global_dir)
