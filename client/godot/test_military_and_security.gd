extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: MILITARY & SECURITY MODULE (GODOT EXECUTIVE OFFICE)")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 5

	# -------------------------------------------------------------------------
	# CHECK 1: Discovery of Military & Security UI Controls
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: Military & Security UI Controls Discovery ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var btn_forces = exec_instance.btn_cmd_tab_forces
	var btn_internal = exec_instance.btn_cmd_tab_internal
	var btn_intel_ai = exec_instance.btn_cmd_tab_intel_ai
	var btn_warfare = exec_instance.btn_cmd_tab_warfare

	var v_forces = exec_instance.cmd_forces_view
	var v_internal = exec_instance.cmd_internal_view
	var v_intel_ai = exec_instance.cmd_intel_ai_view
	var v_warfare = exec_instance.cmd_warfare_view

	if btn_forces == null or btn_internal == null or btn_intel_ai == null or btn_warfare == null:
		push_error("FAIL: Command Centre navigation tab buttons missing.")
		quit(1)
		return

	if v_forces == null or v_internal == null or v_intel_ai == null or v_warfare == null:
		push_error("FAIL: Command Centre tab view containers missing.")
		quit(1)
		return

	# Branch controls
	if exec_instance.cmd_total_personnel_label == null or exec_instance.cmd_composite_readiness_label == null:
		push_error("FAIL: Armed forces summary labels missing.")
		quit(1)
		return

	if exec_instance.btn_recruit_army == null or exec_instance.btn_procure_army == null:
		push_error("FAIL: Army action buttons missing.")
		quit(1)
		return

	if exec_instance.army_budget_slider == null or exec_instance.btn_apply_military_budget == null:
		push_error("FAIL: Military budget sliders missing.")
		quit(1)
		return

	# Border & Police controls
	if exec_instance.border_metrics_label == null or exec_instance.btn_fund_border == null:
		push_error("FAIL: Border security controls missing.")
		quit(1)
		return

	if exec_instance.police_metrics_label == null or exec_instance.btn_expand_police == null:
		push_error("FAIL: Police force controls missing.")
		quit(1)
		return

	# SIB & Military AI controls
	if exec_instance.sib_metrics_label == null or exec_instance.btn_commission_sib == null:
		push_error("FAIL: SIB controls missing.")
		quit(1)
		return

	if exec_instance.ai_metrics_label == null or exec_instance.btn_upgrade_ai == null:
		push_error("FAIL: Military AI controls missing.")
		quit(1)
		return

	if exec_instance.start_sib_project_button == null:
		push_error("FAIL: Development modal SIB project button missing.")
		quit(1)
		return

	print("PASS: All Military & Security UI nodes and action buttons found and bound.")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 2: Command Centre Tab Switching Logic
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: Command Centre Tab Switching ---")
	exec_instance._switch_cmd_tab("forces")
	if not v_forces.visible or v_internal.visible or v_intel_ai.visible or v_warfare.visible:
		push_error("FAIL: 'forces' tab did not toggle views correctly.")
		quit(1)
		return

	exec_instance._switch_cmd_tab("internal")
	if v_forces.visible or not v_internal.visible or v_intel_ai.visible or v_warfare.visible:
		push_error("FAIL: 'internal' tab did not toggle views correctly.")
		quit(1)
		return

	exec_instance._switch_cmd_tab("intel_ai")
	if v_forces.visible or v_internal.visible or not v_intel_ai.visible or v_warfare.visible:
		push_error("FAIL: 'intel_ai' tab did not toggle views correctly.")
		quit(1)
		return

	exec_instance._switch_cmd_tab("warfare")
	if v_forces.visible or v_internal.visible or v_intel_ai.visible or not v_warfare.visible:
		push_error("FAIL: 'warfare' tab did not toggle views correctly.")
		quit(1)
		return

	print("PASS: Tab navigation correctly switches active views.")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 3: State Update Rendering (Armed Forces, Border, Police, SIB, AI)
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: State Update Rendering ---")
	var mock_state: Dictionary = {
		"ArmedForces": {
			"TotalPersonnel": 75000,
			"TotalEquipmentCount": 1850,
			"CompositeReadiness": 82.5,
			"TotalQuarterlyDefenseBudget": 1500000.0,
			"Army": {
				"BranchName": "Army",
				"ActivePersonnel": 50000,
				"EquipmentCount": 1200,
				"Readiness": 85.0,
				"CombatStrength": 42500.0,
				"BudgetSharePercent": 50.0
			},
			"Navy": {
				"BranchName": "Navy",
				"ActivePersonnel": 15000,
				"EquipmentCount": 400,
				"Readiness": 80.0,
				"CombatStrength": 12000.0,
				"BudgetSharePercent": 30.0
			},
			"AirForce": {
				"BranchName": "AirForce",
				"ActivePersonnel": 10000,
				"EquipmentCount": 250,
				"Readiness": 78.0,
				"CombatStrength": 7800.0,
				"BudgetSharePercent": 20.0
			}
		},
		"BorderSecurity": {
			"BorderFundingLevel": 1,
			"SurveillanceRating": 65.0,
			"PatrolPersonnel": 8500,
			"ContrabandInterceptionRate": 58.0,
			"InfiltrationDetectionChance": 62.0
		},
		"PoliceForce": {
			"ActiveOfficers": 42000,
			"FundingLevel": 2,
			"CrimeRateIndex": 24.5,
			"PublicOrderScore": 81.0,
			"CommunityTrustScore": 76.0
		},
		"InvestigativeBureau": {
			"IsEstablished": true,
			"BureauName": "Sovereign Intelligence Bureau",
			"DirectorName": "Evelyn Cross",
			"ActiveOperatives": 1200,
			"CounterIntelligenceRating": 88.5,
			"ForeignSpyDetectionRate": 72.0,
			"NeutralizedForeignSpies": 4,
			"IsConcealed": true
		},
		"MilitaryAi": {
			"IsActive": true,
			"AiSystemName": "Aegis-9 Sovereign Defense Neural Grid",
			"AnalysisLevel": 2,
			"ThreatDetectionBonus": 20.0,
			"RecentThreatAssessments": [
				"Autonomous frontier sector radar anomaly analyzed: Low Threat"
			]
		}
	}

	exec_instance._update_command_centre_modal(mock_state)

	# Verify Armed Forces view
	if not "75,000" in exec_instance.cmd_total_personnel_label.text:
		push_error("FAIL: cmd_total_personnel_label did not format correctly: " + exec_instance.cmd_total_personnel_label.text)
		quit(1)
		return

	if not "82.5%" in exec_instance.cmd_composite_readiness_label.text:
		push_error("FAIL: cmd_composite_readiness_label did not format correctly: " + exec_instance.cmd_composite_readiness_label.text)
		quit(1)
		return

	if not "50,000" in exec_instance.army_stats_label.text:
		push_error("FAIL: army_stats_label missing active personnel: " + exec_instance.army_stats_label.text)
		quit(1)
		return

	# Verify Internal Security view
	if not "58.0%" in exec_instance.border_metrics_label.text:
		push_error("FAIL: border_metrics_label missing interception rate: " + exec_instance.border_metrics_label.text)
		quit(1)
		return

	if not "24.5" in exec_instance.police_metrics_label.text:
		push_error("FAIL: police_metrics_label missing crime rate: " + exec_instance.police_metrics_label.text)
		quit(1)
		return

	# Verify SIB & AI view
	if not "88.5%" in exec_instance.sib_metrics_label.text:
		push_error("FAIL: sib_metrics_label missing counter-intelligence rating: " + exec_instance.sib_metrics_label.text)
		quit(1)
		return

	if not "Aegis-9" in exec_instance.ai_metrics_label.text:
		push_error("FAIL: ai_metrics_label missing AI system name: " + exec_instance.ai_metrics_label.text)
		quit(1)
		return

	print("PASS: State update successfully refreshed all Military & Security displays.")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 4: Military Actions Request Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Military Actions Request File Dispatch ---")

	exec_instance.request_file_override = BRIDGE_REQ_PATH

	# Clean any stale request file
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(BRIDGE_REQ_PATH))

	# 4a. Recruit Army
	exec_instance._on_recruit_army_pressed()
	var req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "recruit_military_branch" or req_data.get("branch") != "army":
		push_error("FAIL: recruit_military_branch request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	# 4b. Procure Navy
	exec_instance._on_procure_navy_pressed()
	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "procure_military_branch" or req_data.get("branch") != "navy":
		push_error("FAIL: procure_military_branch request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	# 4c. Border Security Funding
	exec_instance._on_fund_border_pressed()
	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "border_security_funding":
		push_error("FAIL: border_security_funding request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	# 4d. Police Force Expansion
	exec_instance._on_expand_police_pressed()
	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "police_force_expansion":
		push_error("FAIL: police_force_expansion request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	# 4e. Military AI Funding
	exec_instance._on_upgrade_military_ai_pressed()
	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "military_ai_funding":
		push_error("FAIL: military_ai_funding request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	print("PASS: Direct action requests correctly written to presidential_desk_request.json.")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 5: Budget Allocation & SIB Commission Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Budget Allocation & SIB Commission Dispatch ---")

	exec_instance.army_budget_slider.value = 220.0
	exec_instance.navy_budget_slider.value = 140.0
	exec_instance.air_budget_slider.value = 140.0
	exec_instance._on_apply_military_budget_pressed()

	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "military_budget_allocation" or abs(req_data.get("army_funding", 0.0) - 220000000.0) > 1.0:
		push_error("FAIL: military_budget_allocation request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return

	exec_instance._on_commission_sib_pressed()
	req_data = _read_json_file(BRIDGE_REQ_PATH)
	if req_data.get("action") != "commission_investigative_bureau":
		push_error("FAIL: commission_investigative_bureau request mismatch: " + JSON.stringify(req_data))
		quit(1)
		return
		quit(1)
		return

	print("PASS: Budget allocation and SIB commission dispatch verified.")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# SUMMARY
	# -------------------------------------------------------------------------
	print("\n=================================================================")
	print("🎉 ALL MILITARY & SECURITY TESTS PASSED: %d / %d" % [passed_checks, total_checks])
	print("=================================================================")
	quit(0)

func _read_json_file(path: String) -> Dictionary:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}
	var text := file.get_as_text()
	var parsed = JSON.parse_string(text)
	if parsed is Dictionary:
		return parsed
	return {}
