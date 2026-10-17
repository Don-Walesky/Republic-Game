extends SceneTree

## Republic - Phase 3 Governance Loop Automated Walkthrough Test
## Validates the coherent single-player governance loop:
## 1. Sovereign Founding & Telemetry Verification (GDP, Fiscal, Capacity)
## 2. Grassroots Campaign Town Hall Action (Costs, Approval, Stability, Polling)
## 3. Executive Cabinet Interior Appointment & Competence vs. Loyalty Trade-offs
## 4. Economic Development: National University Project Commissioning
## 5. Political Crisis & Emergency Decrees (Civic Relief vs. Austerity)
## 6. Democratic Election Defeat & Cabinet Vacation Transition
## 7. Authoritative Persistence & Reload Verification

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PHASE 3 GOVERNANCE LOOP WALKTHROUGH TEST        ")
	print("==============================================================")

	var scene_res := load("res://PresidentialDesk.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://PresidentialDesk.tscn")
		quit(1)
		return

	var desk = scene_res.instantiate()
	if desk == null:
		printerr("[FAIL]: Failed to instantiate PresidentialDesk")
		quit(1)
		return

	root.add_child(desk)

	var success := true

	# --- Step 1: Start / Found Sovereign Republic with Telemetry ---
	print("\n--- Step 1: Sovereign Founding & Phase 3 Telemetry Inspection ---")
	var test_state_path := "user://phase3_loop_state.json"
	var test_sav_path := "user://phase3_loop_state.sav"
	var req_path: String = desk._resolve_request_path(test_state_path)

	# Clean up any residual test files
	if FileAccess.file_exists(test_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_state_path))
	if FileAccess.file_exists(test_sav_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_sav_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	var current_state: Dictionary = {
		"country_name": "Republic of Valoria",
		"country_id": "valoria-sovereign",
		"treasury": "R 10,000,000",
		"treasury_balance": 10000000,
		"approval": "50.0%",
		"approval_rating": 50.0,
		"stability": "75.0%",
		"stability_rating": 75.0,
		"simulation_day": 1,
		"simulation_time": "Day 1, 00:00 WAT",
		"ruling_party": "Democratic Union",
		"opposition_party": "National Civic Front",
		"polling_support": 52.0,
		"opposition_support": 48.0,
		"election": "In 180 days",
		"next_election_day": 180,
		"income_tax_rate": 0.25,
		"income_tax_rate_formatted": "25%",
		"active_project": "None",
		"cabinet_summary": "Finance: Vacant | Interior: Vacant",
		"civil_service_quality": 0.30,
		"corruption_control": 0.30,
		"gdp_formatted": "R 10,000,000",
		"fiscal_report_summary": "Q1: Rev R 1,250,000 | Exp R 950,000 | Net +R 300,000",
		"fiscal_deficit_formatted": "Surplus: +R 300,000",
		"state_capacity_summary": "Overall: 30% | Civil: 30% | Anti-Corr: 30%",
		"active_crisis": "None",
		"active_crisis_details": "National stability within safe limits",
		"save_file_path": ProjectSettings.globalize_path(test_sav_path)
	}

	var sf := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf.store_string(JSON.stringify(current_state))
	sf.close()

	if not desk.load_state_from_file(test_state_path):
		printerr("[FAIL]: Failed to load initial Phase 3 state")
		quit(1)
		return

	if not desk.country_label.text.contains("Republic of Valoria"):
		printerr("[FAIL]: Country label mismatch: " + desk.country_label.text)
		success = false
	if not desk.fiscal_report_label.text.contains("Surplus: +R 300,000"):
		printerr("[FAIL]: Fiscal report label mismatch: " + desk.fiscal_report_label.text)
		success = false
	if not desk.state_capacity_label.text.contains("Overall: 30%"):
		printerr("[FAIL]: State capacity label mismatch: " + desk.state_capacity_label.text)
		success = false
	if not desk.crisis_status_label.text.contains("Active Crisis: None"):
		printerr("[FAIL]: Crisis status label mismatch: " + desk.crisis_status_label.text)
		success = false
	print("  [OK]: Sovereign 'Republic of Valoria' loaded with full Phase 3 telemetry verified.")

	# --- Step 2: Grassroots Campaign Town Hall ---
	print("\n--- Step 2: Grassroots Campaign Town Hall Decision ---")
	desk.switch_to_section(desk.Section.CAMPAIGN)
	var th_res: Dictionary = desk.execute_town_hall(true)
	if th_res.get("result") != "Pending":
		printerr("[FAIL]: Expected town hall to return Pending, got: " + str(th_res))
		success = false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Town hall request file was not created: " + req_path)
		success = false
	else:
		var req_f := FileAccess.open(req_path, FileAccess.READ)
		var req_json: Dictionary = JSON.parse_string(req_f.get_as_text())
		req_f.close()
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))
		if req_json.get("action") != "town_hall":
			printerr("[FAIL]: Town hall action mismatch in request file: " + str(req_json))
			success = false

	# Simulate authoritative town hall outcome: R50,000 cost, +1.0% approval, +1.5% stability, +1.5% polling
	current_state["treasury_balance"] = 9950000
	current_state["treasury"] = "R 9,950,000"
	current_state["approval_rating"] = 51.0
	current_state["approval"] = "51.0%"
	current_state["stability_rating"] = 76.5
	current_state["stability"] = "76.5%"
	current_state["polling_support"] = 53.5
	current_state["opposition_support"] = 46.5

	var sf2 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf2.store_string(JSON.stringify(current_state))
	sf2.close()

	var th_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Citizen town hall held: Public approval +1.0%, stability +1.5%, ruling party support +1.5%.",
		"treasury_balance": 9950000,
		"treasury": "R 9,950,000",
		"approval_rating": 51.0,
		"approval": "51.0%",
		"stability_rating": 76.5,
		"stability": "76.5%",
		"polling_support": 53.5,
		"opposition_support": 46.5
	}
	desk._handle_decision_result(th_sim_result)

	if not desk.treasury_label.text.contains("R 9,950,000"):
		printerr("[FAIL]: Treasury not updated after town hall: " + desk.treasury_label.text)
		success = false
	if not desk.approval_label.text.contains("51.0%"):
		printerr("[FAIL]: Approval not updated after town hall: " + desk.approval_label.text)
		success = false
	if not desk.ruling_party_label.text.contains("53.5%"):
		printerr("[FAIL]: Polling support not updated after town hall: " + desk.ruling_party_label.text)
		success = false
	print("  [OK]: Town hall executed: R50k cost, approval lifted to 51.0%, stability to 76.5%, polling to 53.5%.")

	# --- Step 3: Executive Cabinet Interior Appointment (Competence vs. Loyalty) ---
	print("\n--- Step 3: Executive Cabinet Interior Appointment & Competence vs Loyalty ---")
	desk.switch_to_section(desk.Section.CABINET)

	# 3A: Appoint Technocrat Interior Minister (Dr. Alistair Finch)
	var cab_res: Dictionary = desk.execute_cabinet_appointment("Interior", "technocrat", true)
	if cab_res.get("result") != "Pending":
		printerr("[FAIL]: Expected cabinet appointment to return Pending, got: " + str(cab_res))
		success = false

	if FileAccess.file_exists(req_path):
		var req_f := FileAccess.open(req_path, FileAccess.READ)
		var req_json: Dictionary = JSON.parse_string(req_f.get_as_text())
		req_f.close()
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))
		if req_json.get("portfolio") != "Interior" or req_json.get("candidate_type") != "technocrat":
			printerr("[FAIL]: Cabinet request parameters mismatch: " + str(req_json))
			success = false

	# Technocrat trade-off: Corruption control 90%, ruling polling drops by 3.0% (political friction)
	current_state["corruption_control"] = 0.90
	current_state["polling_support"] = 50.5
	current_state["opposition_support"] = 49.5
	current_state["cabinet_summary"] = "Finance: Vacant | Interior: Dr. Alistair Finch (Technocrat)"
	current_state["state_capacity_summary"] = "Overall: 30% | Civil: 30% | Anti-Corr: 90%"

	var sf3 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf3.store_string(JSON.stringify(current_state))
	sf3.close()

	var cab_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Appointed Dr. Alistair Finch as Minister of Interior. Corruption Control elevated to 90%, party polling adjusted (-3.0%).",
		"corruption_control": 0.90,
		"polling_support": 50.5,
		"opposition_support": 49.5,
		"cabinet_summary": current_state["cabinet_summary"]
	}
	desk._handle_decision_result(cab_sim_result)

	if not desk.cabinet_summary_label.text.contains("Dr. Alistair Finch"):
		printerr("[FAIL]: Cabinet label missing Dr. Alistair Finch: " + desk.cabinet_summary_label.text)
		success = false
	if not desk.cabinet_summary_label.text.contains("Corruption Control: 90%"):
		printerr("[FAIL]: Corruption control not 90%: " + desk.cabinet_summary_label.text)
		success = false
	print("  [OK]: Technocrat Interior Minister appointed: Corruption Control surged to 90%, party polling adjusted.")

	# --- Step 4: Economic Higher Education Development (National University) ---
	print("\n--- Step 4: National University Development Commissioning ---")
	desk.switch_to_section(desk.Section.ECONOMY)

	var proj_res: Dictionary = desk.execute_project_decision("University", true)
	if proj_res.get("result") != "Pending":
		printerr("[FAIL]: Expected university project to return Pending, got: " + str(proj_res))
		success = false

	if FileAccess.file_exists(req_path):
		var req_f := FileAccess.open(req_path, FileAccess.READ)
		var req_json: Dictionary = JSON.parse_string(req_f.get_as_text())
		req_f.close()
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))
		if req_json.get("project_type") != "University":
			printerr("[FAIL]: Project type mismatch in request: " + str(req_json))
			success = false

	# University cost R750k: Treasury R 9,950,000 -> R 9,200,000
	current_state["treasury_balance"] = 9200000
	current_state["treasury"] = "R 9,200,000"
	current_state["active_project"] = "National University"
	current_state["fiscal_report_summary"] = "Q1: Rev R 1,250,000 | Exp R 1,700,000 | Net -R 450,000"
	current_state["fiscal_deficit_formatted"] = "Deficit: -R 450,000"

	var sf4 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf4.store_string(JSON.stringify(current_state))
	sf4.close()

	var proj_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Initiated National University development program. Cost: R 750,000. Expected duration: 48h.",
		"treasury_balance": 9200000,
		"treasury": "R 9,200,000",
		"active_project": "National University"
	}
	desk._handle_decision_result(proj_sim_result)

	if not desk.treasury_label.text.contains("R 9,200,000"):
		printerr("[FAIL]: Treasury balance mismatch after university commissioning: " + desk.treasury_label.text)
		success = false
	if not desk.active_project_label.text.contains("National University"):
		printerr("[FAIL]: Active project label did not reflect National University: " + desk.active_project_label.text)
		success = false
	if not desk.fiscal_report_label.text.contains("Deficit: -R 450,000"):
		printerr("[FAIL]: Fiscal report did not reflect development deficit: " + desk.fiscal_report_label.text)
		success = false
	print("  [OK]: National University commissioned: R750k deducted, development deficit documented.")

	# --- Step 5: Political Crisis & Emergency Executive Decrees ---
	print("\n--- Step 5: Political Crisis Management & Emergency Decrees ---")
	desk.switch_to_section(desk.Section.BRIEFINGS)

	# 5A: Civic Relief Package (-R100k, +4% approval, +3% stability)
	var relief_res: Dictionary = desk.execute_crisis_response("civic_relief", true)
	if relief_res.get("result") != "Pending":
		printerr("[FAIL]: Expected civic relief to return Pending, got: " + str(relief_res))
		success = false

	if FileAccess.file_exists(req_path):
		var req_f := FileAccess.open(req_path, FileAccess.READ)
		var req_json: Dictionary = JSON.parse_string(req_f.get_as_text())
		req_f.close()
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))
		if req_json.get("action") != "crisis_response" or req_json.get("decree_type") != "civic_relief":
			printerr("[FAIL]: Crisis response request payload mismatch: " + str(req_json))
			success = false

	current_state["treasury_balance"] = 9100000
	current_state["treasury"] = "R 9,100,000"
	current_state["approval_rating"] = 55.0
	current_state["approval"] = "55.0%"
	current_state["stability_rating"] = 79.5
	current_state["stability"] = "79.5%"

	var sf5 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf5.store_string(JSON.stringify(current_state))
	sf5.close()

	var relief_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Civic Relief Package enacted: Allocated R 100,000 to citizen emergency services. Approval lifted to 55.0%, stability to 79.5%."
	}
	desk._handle_decision_result(relief_sim_result)

	if not desk.treasury_label.text.contains("R 9,100,000"):
		printerr("[FAIL]: Treasury balance not updated after civic relief: " + desk.treasury_label.text)
		success = false
	if not desk.approval_label.text.contains("55.0%"):
		printerr("[FAIL]: Approval not lifted after civic relief: " + desk.approval_label.text)
		success = false
	print("  [OK]: Civic Relief Package enacted: R100k allocated, approval boosted to 55.0%, stability to 79.5%.")

	# 5B: Emergency Austerity Decree (+R500k reserves, -4% approval, -2% stability)
	var aust_res: Dictionary = desk.execute_crisis_response("austerity", true)
	if aust_res.get("result") != "Pending":
		printerr("[FAIL]: Expected austerity to return Pending, got: " + str(aust_res))
		success = false

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	current_state["treasury_balance"] = 9600000
	current_state["treasury"] = "R 9,600,000"
	current_state["approval_rating"] = 51.0
	current_state["approval"] = "51.0%"
	current_state["stability_rating"] = 77.5
	current_state["stability"] = "77.5%"

	var sf6 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf6.store_string(JSON.stringify(current_state))
	sf6.close()

	var aust_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Emergency Austerity Decree enacted: Reallocated R 500,000 to liquid operating reserves from departmental reductions. Approval 51.0%, stability 77.5%."
	}
	desk._handle_decision_result(aust_sim_result)

	if not desk.treasury_label.text.contains("R 9,600,000"):
		printerr("[FAIL]: Treasury balance not updated after austerity: " + desk.treasury_label.text)
		success = false
	print("  [OK]: Emergency Austerity Decree enacted: +R500k liquidity replenished, approval adjusted.")

	# --- Step 6: Democratic Election Defeat & Cabinet Vacation Transition ---
	print("\n--- Step 6: Democratic Election Defeat & Peaceful Transition of Power ---")
	desk.switch_to_section(desk.Section.CAMPAIGN)

	# Shift majority voter sentiment to the opposition party
	current_state["polling_support"] = 35.0
	current_state["opposition_support"] = 65.0
	var sf7 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf7.store_string(JSON.stringify(current_state))
	sf7.close()
	desk.load_state_from_file(test_state_path)

	var defeat_res: Dictionary = desk.execute_contest_election(true)
	if defeat_res.get("result") != "Pending":
		printerr("[FAIL]: Expected election contest to return Pending, got: " + str(defeat_res))
		success = false

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Opposition victory: power transfers peacefully to National Civic Front, cabinet vacated, capacity reset
	current_state["ruling_party"] = "National Civic Front"
	current_state["opposition_party"] = "Democratic Union"
	current_state["polling_support"] = 62.0
	current_state["opposition_support"] = 38.0
	current_state["cabinet_summary"] = "Finance: Vacant | Interior: Vacant"
	current_state["civil_service_quality"] = 0.30
	current_state["corruption_control"] = 0.30
	current_state["state_capacity_summary"] = "Overall: 30% | Civil: 30% | Anti-Corr: 30%"
	current_state["election"] = "Opposition won election (Peaceful democratic transition)"
	current_state["next_election_day"] = 540

	var sf8 := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf8.store_string(JSON.stringify(current_state))
	sf8.close()

	var defeat_sim_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Democratic election held: Opposition 'National Civic Front' defeated ruling party. Cabinet portfolios vacated for incoming administration.",
		"ruling_party": "National Civic Front",
		"opposition_party": "Democratic Union",
		"polling_support": 62.0,
		"opposition_support": 38.0,
		"cabinet_summary": "Finance: Vacant | Interior: Vacant",
		"civil_service_quality": 0.30,
		"corruption_control": 0.30,
		"is_reelected": false
	}
	desk._handle_decision_result(defeat_sim_result)

	if not desk.ruling_party_label.text.contains("National Civic Front"):
		printerr("[FAIL]: Ruling party did not update to incoming government: " + desk.ruling_party_label.text)
		success = false
	if not desk.cabinet_overview_label.text.contains("Finance: Vacant | Interior: Vacant"):
		printerr("[FAIL]: Cabinet portfolios were not vacated upon democratic defeat: " + desk.cabinet_overview_label.text)
		success = false
	print("  [OK]: Democratic defeat enacted: Government peacefully transferred, cabinet vacated, capacity reset.")

	# --- Step 7: Persistence Verification Across Save & Reload ---
	print("\n--- Step 7: Authoritative Persistence Verification Across Save & Reload ---")
	desk.switch_to_section(desk.Section.OVERVIEW)

	var fresh_desk = scene_res.instantiate()
	root.add_child(fresh_desk)
	var reload_ok: bool = fresh_desk.load_state_from_file(test_state_path)
	if not reload_ok:
		printerr("[FAIL]: Fresh PresidentialDesk instance failed to load state file")
		success = false

	if not fresh_desk.country_label.text.contains("Republic of Valoria"):
		printerr("[FAIL]: Persisted country name mismatch: " + fresh_desk.country_label.text)
		success = false
	if not fresh_desk.treasury_label.text.contains("R 9,600,000"):
		printerr("[FAIL]: Persisted treasury mismatch: " + fresh_desk.treasury_label.text)
		success = false
	if not fresh_desk.active_project_label.text.contains("National University"):
		printerr("[FAIL]: Persisted active project mismatch: " + fresh_desk.active_project_label.text)
		success = false
	if not fresh_desk.ruling_party_label.text.contains("National Civic Front"):
		printerr("[FAIL]: Persisted ruling party mismatch: " + fresh_desk.ruling_party_label.text)
		success = false
	if not fresh_desk.cabinet_overview_label.text.contains("Vacant"):
		printerr("[FAIL]: Persisted cabinet state mismatch: " + fresh_desk.cabinet_overview_label.text)
		success = false
	print("  [OK]: Complete state survived reload cleanly (Treasury, University Project, Government Transition).")

	# Clean up test artifacts
	if FileAccess.file_exists(test_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_state_path))
	if FileAccess.file_exists(test_sav_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_sav_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	desk.queue_free()
	fresh_desk.queue_free()

	if success:
		print("==============================================================")
		print(" [PASS]: COMPLETE PHASE 3 GOVERNANCE LOOP SUCCEEDED!         ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: PHASE 3 GOVERNANCE LOOP ENCOUNTERED FAILURES         ")
		printerr("==============================================================")
		quit(1)
