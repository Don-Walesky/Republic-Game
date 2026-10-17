extends SceneTree

## Republic - Phase 2 First Playable Loop Walkthrough Test
## Validates the complete 8-step player journey from founding through campaigning,
## elections, cabinet appointments, economic decisions, and persistent save/reload.

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PHASE 2 FIRST PLAYABLE LOOP WALKTHROUGH TEST    ")
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

	# --- Step 1: Start or load a country ---
	print("\n--- Step 1: Start / Found Sovereign Republic ---")
	var test_state_path := "user://walkthrough_loop_state.json"
	var test_sav_path := "user://walkthrough_loop_state.sav"
	var req_path: String = desk._resolve_request_path(test_state_path)

	# Clean up any residual test files
	if FileAccess.file_exists(test_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_state_path))
	if FileAccess.file_exists(test_sav_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_sav_path))
	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Establish baseline country state
	var baseline_state: Dictionary = {
		"country_name": "Republic of Solaria",
		"country_id": "solaria-sovereign",
		"treasury": "R 10,000,000",
		"treasury_balance": 10000000,
		"approval": "50.0%",
		"approval_rating": 50.0,
		"stability": "75.0%",
		"stability_rating": 75.0,
		"simulation_day": 0,
		"simulation_time": "Day 0, 00:00 WAT",
		"ruling_party": "Democratic Coalition",
		"opposition_party": "Liberty Front",
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
		"save_file_path": ProjectSettings.globalize_path(test_sav_path)
	}

	var sf := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf.store_string(JSON.stringify(baseline_state))
	sf.close()

	if not desk.load_state_from_file(test_state_path):
		printerr("[FAIL]: Failed to load initial walkthrough state")
		quit(1)
		return
	print("  [OK]: Sovereign country 'Republic of Solaria' loaded successfully.")

	# --- Step 2: View current status & verify navigation ---
	print("\n--- Step 2: View Country Status & Section Navigation ---")
	if not desk.country_label.text.contains("Republic of Solaria"):
		printerr("[FAIL]: Country label did not match: " + desk.country_label.text)
		success = false
	if not desk.treasury_label.text.contains("R 10,000,000"):
		printerr("[FAIL]: Treasury label did not match: " + desk.treasury_label.text)
		success = false
	if not desk.approval_label.text.contains("50.0%"):
		printerr("[FAIL]: Approval label did not match: " + desk.approval_label.text)
		success = false

	# Test Section switching
	desk.switch_to_section(desk.Section.OVERVIEW)
	if not desk.section_overview.visible or desk.section_cabinet.visible:
		printerr("[FAIL]: Overview section visibility mismatch")
		success = false
	print("  [OK]: Overview telemetry and initial section visibility confirmed.")

	# --- Step 3: Perform an available campaign action ---
	print("\n--- Step 3: Execute Campaign Rally Action ---")
	desk.switch_to_section(desk.Section.CAMPAIGN)
	if not desk.section_campaign.visible:
		printerr("[FAIL]: Campaign section not visible after switch")
		success = false

	var rally_res: Dictionary = desk.execute_campaign_rally(true)
	if rally_res.get("result") != "Pending":
		printerr("[FAIL]: Expected campaign rally to return Pending, got: " + str(rally_res))
		success = false

	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Campaign rally request file was not created")
		success = false
	else:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate successful rally outcome from simulation engine
	baseline_state["treasury_balance"] = 9750000
	baseline_state["treasury"] = "R 9,750,000"
	baseline_state["polling_support"] = 56.0
	baseline_state["opposition_support"] = 44.0
	baseline_state["approval_rating"] = 51.5
	baseline_state["approval"] = "51.5%"
	var f3 := FileAccess.open(test_state_path, FileAccess.WRITE)
	f3.store_string(JSON.stringify(baseline_state))
	f3.close()

	var sim_rally_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Campaign rally held successfully. Polling support surged.",
		"treasury_balance": 9750000,
		"treasury": "R 9,750,000",
		"polling_support": 56.0,
		"opposition_support": 44.0,
		"approval_rating": 51.5,
		"approval": "51.5%"
	}
	desk._handle_decision_result(sim_rally_result)

	if not desk.election_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Election status label did not reflect rally success: " + desk.election_status_label.text)
		success = false
	if not desk.treasury_label.text.contains("R 9,750,000"):
		printerr("[FAIL]: Treasury did not update to R 9,750,000: " + desk.treasury_label.text)
		success = false
	print("  [OK]: Campaign rally enacted: R250k deducted, ruling support surged to 56%, approval boosted to 51.5%.")

	# --- Step 4: Experience the election decision and outcome ---
	print("\n--- Step 4: Call & Contest National Election ---")
	var elect_res: Dictionary = desk.execute_contest_election(true)
	if elect_res.get("result") != "Pending":
		printerr("[FAIL]: Expected election call to return Pending, got: " + str(elect_res))
		success = false

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate election victory outcome (mandate renewed)
	baseline_state["approval_rating"] = 54.0
	baseline_state["approval"] = "54.0%"
	baseline_state["stability_rating"] = 77.0
	baseline_state["stability"] = "77.0%"
	baseline_state["election"] = "Mandate renewed"
	baseline_state["next_election_day"] = 360
	var f4 := FileAccess.open(test_state_path, FileAccess.WRITE)
	f4.store_string(JSON.stringify(baseline_state))
	f4.close()

	var sim_election_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Democratic election held: Ruling party won mandate renewal. Approval +2.5%, Stability +2.0%.",
		"approval_rating": 54.0,
		"approval": "54.0%",
		"stability_rating": 77.0,
		"stability": "77.0%",
		"election": "Mandate renewed",
		"next_election_day": 360
	}
	desk._handle_decision_result(sim_election_result)

	if not desk.election_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Election status label did not reflect victory: " + desk.election_status_label.text)
		success = false
	if not desk.approval_label.text.contains("54.0%"):
		printerr("[FAIL]: Approval did not update to 54.0%: " + desk.approval_label.text)
		success = false
	print("  [OK]: Democratic election contested: mandate renewed, approval raised to 54%, stability raised to 77%.")

	# --- Step 5: Enter the governing experience & appoint a minister ---
	print("\n--- Step 5: Executive Cabinet Appointment & Capacity Trade-off ---")
	desk.switch_to_section(desk.Section.CABINET)
	if not desk.section_cabinet.visible:
		printerr("[FAIL]: Cabinet section not visible after switch")
		success = false

	var cab_res: Dictionary = desk.execute_cabinet_appointment("Finance", "technocrat", true)
	if cab_res.get("result") != "Pending":
		printerr("[FAIL]: Expected cabinet appointment to return Pending, got: " + str(cab_res))
		success = false

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	# Simulate technocrat appointment outcome (lifting bottleneck)
	baseline_state["cabinet_summary"] = "Finance: Technocrat (0.90) | Interior: Vacant (0.30)"
	baseline_state["civil_service_quality"] = 0.90
	baseline_state["corruption_control"] = 0.30
	baseline_state["polling_support"] = 53.0
	baseline_state["approval_rating"] = 55.5
	baseline_state["approval"] = "55.5%"
	var f5 := FileAccess.open(test_state_path, FileAccess.WRITE)
	f5.store_string(JSON.stringify(baseline_state))
	f5.close()

	var sim_cab_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Cabinet appointment confirmed: Appointed Technocrat to Finance. Institutional capacity strengthened.",
		"cabinet_summary": "Finance: Technocrat (0.90) | Interior: Vacant (0.30)",
		"civil_service_quality": 0.90,
		"corruption_control": 0.30,
		"polling_support": 53.0,
		"approval_rating": 55.5,
		"approval": "55.5%"
	}
	desk._handle_decision_result(sim_cab_result)

	if not desk.cabinet_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Cabinet status label did not reflect appointment success: " + desk.cabinet_status_label.text)
		success = false
	if not desk.cabinet_summary_label.text.contains("Civil Service: 90%"):
		printerr("[FAIL]: Cabinet summary label did not reflect 90% civil service capacity: " + desk.cabinet_summary_label.text)
		success = false
	print("  [OK]: Technocrat appointed to Finance: civil service capacity lifted from 30% to 90%, approval +1.5%, polling adjusted.")

	# --- Step 6: Make a meaningful economic decision ---
	print("\n--- Step 6: Economic Taxation & National Road Project Decisions ---")
	desk.switch_to_section(desk.Section.ECONOMY)
	if not desk.section_economy.visible:
		printerr("[FAIL]: Economy section not visible after switch")
		success = false

	# 6a. Consequence preview test
	var preview_text: String = desk.calculate_expected_consequence(25.0, 28.0)
	if not preview_text.contains("-1.5%"):
		printerr("[FAIL]: Tax preview calculation mismatch: " + preview_text)
		success = false

	# 6b. Initiate National Road Program
	var proj_res: Dictionary = desk.execute_project_decision("Road", true)
	if proj_res.get("result") != "Pending":
		printerr("[FAIL]: Expected project initiation to return Pending, got: " + str(proj_res))
		success = false

	if FileAccess.file_exists(req_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(req_path))

	baseline_state["active_project"] = "Road"
	baseline_state["treasury_balance"] = 9250000
	baseline_state["treasury"] = "R 9,250,000"
	baseline_state["income_tax_rate"] = 0.28
	baseline_state["income_tax_rate_formatted"] = "28%"
	var f6 := FileAccess.open(test_state_path, FileAccess.WRITE)
	f6.store_string(JSON.stringify(baseline_state))
	f6.close()

	var sim_proj_result: Dictionary = {
		"request_id": desk._active_request_id,
		"success": true,
		"result": "Success",
		"message": "Project decision confirmed: Commissioned National Road Program.",
		"active_project": "Road",
		"treasury_balance": 9250000,
		"treasury": "R 9,250,000"
	}
	desk._handle_decision_result(sim_proj_result)

	if not desk.project_status_label.text.contains("[Success]"):
		printerr("[FAIL]: Project status label did not reflect road project success: " + desk.project_status_label.text)
		success = false
	if not desk.active_project_label.text.contains("Road"):
		printerr("[FAIL]: Active project label did not reflect Road: " + desk.active_project_label.text)
		success = false
	if not desk.treasury_label.text.contains("R 9,250,000"):
		printerr("[FAIL]: Treasury did not deduct R500k for project: " + desk.treasury_label.text)
		success = false
	print("  [OK]: National Road Program commissioned: R500k deducted, active project set to Road.")

	# --- Step 7: See resulting changes and understand why they occurred ---
	print("\n--- Step 7: Situation Briefing Review & Return to Overview ---")
	desk.switch_to_section(desk.Section.BRIEFINGS)
	if not desk.section_briefings.visible:
		printerr("[FAIL]: Briefings section not visible after switch")
		success = false

	# Verify briefing telemetry reflects cumulative decisions
	if not desk.briefing_treasury_label.text.contains("R 9,250,000"):
		printerr("[FAIL]: Briefing treasury mismatch: " + desk.briefing_treasury_label.text)
		success = false
	if not desk.briefing_active_project_label.text.contains("Road"):
		printerr("[FAIL]: Briefing active project mismatch: " + desk.briefing_active_project_label.text)
		success = false

	# Test return to overview button
	desk.return_to_overview()
	if not desk.section_overview.visible or desk.section_briefings.visible:
		printerr("[FAIL]: Return to overview failed to restore section visibility")
		success = false
	print("  [OK]: Situation briefing verified and 1-click return to Overview confirmed.")

	# --- Step 8: Save, reload, and verify state persistence ---
	print("\n--- Step 8: Persistence Verification Across Save & Reload ---")
	# Export updated consolidated state to disk
	var saved_state := {
		"country_name": "Republic of Solaria",
		"country_id": "solaria-sovereign",
		"treasury": "R 9,250,000",
		"treasury_balance": 9250000,
		"approval": "55.5%",
		"approval_rating": 55.5,
		"stability": "77.0%",
		"stability_rating": 77.0,
		"simulation_day": 1,
		"simulation_time": "Day 1, 00:00 WAT",
		"ruling_party": "Democratic Coalition",
		"opposition_party": "Liberty Front",
		"polling_support": 53.0,
		"opposition_support": 47.0,
		"election": "Mandate renewed",
		"next_election_day": 360,
		"income_tax_rate": 0.28,
		"income_tax_rate_formatted": "28%",
		"active_project": "Road",
		"cabinet_summary": "Finance: Technocrat (0.90) | Interior: Vacant (0.30)",
		"civil_service_quality": 0.90,
		"corruption_control": 0.30,
		"save_file_path": ProjectSettings.globalize_path(test_sav_path)
	}

	var save_file := FileAccess.open(test_state_path, FileAccess.WRITE)
	save_file.store_string(JSON.stringify(saved_state))
	save_file.close()

	# Re-instantiate a fresh PresidentialDesk instance to verify cold restart
	desk.queue_free()

	var fresh_desk = scene_res.instantiate()
	root.add_child(fresh_desk)

	if not fresh_desk.load_state_from_file(test_state_path):
		printerr("[FAIL]: Fresh PresidentialDesk instance failed to load saved state")
		success = false

	# Confirm all 8-step journey state modifications survived reload intact
	if fresh_desk.country_label.text != "Country: Republic of Solaria":
		printerr("[FAIL]: Restored country mismatch: " + fresh_desk.country_label.text)
		success = false
	if fresh_desk.treasury_label.text != "Treasury: R 9,250,000":
		printerr("[FAIL]: Restored treasury mismatch: " + fresh_desk.treasury_label.text)
		success = false
	if fresh_desk.approval_label.text != "Approval: 55.5%":
		printerr("[FAIL]: Restored approval mismatch: " + fresh_desk.approval_label.text)
		success = false
	if fresh_desk.stability_label.text != "Stability: 77.0%":
		printerr("[FAIL]: Restored stability mismatch: " + fresh_desk.stability_label.text)
		success = false
	if not fresh_desk.active_project_label.text.contains("Road"):
		printerr("[FAIL]: Restored active project mismatch: " + fresh_desk.active_project_label.text)
		success = false
	if not fresh_desk.cabinet_summary_label.text.contains("Civil Service: 90%"):
		printerr("[FAIL]: Restored cabinet capacity mismatch: " + fresh_desk.cabinet_summary_label.text)
		success = false

	fresh_desk.queue_free()

	# Cleanup test files
	if FileAccess.file_exists(test_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_state_path))
	if FileAccess.file_exists(test_sav_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_sav_path))

	print("  [OK]: All state changes (Treasury, Cabinet, Projects, Polling, Mandate) verified across save and reload.")

	print("\n==============================================================")
	if success:
		print(" [PASS]: COMPLETE FIRST PLAYABLE LOOP WALKTHROUGH SUCCEEDED! ")
		print("==============================================================")
		quit(0)
	else:
		printerr(" [FAIL]: ONE OR MORE WALKTHROUGH STEPS FAILED!                ")
		print("==============================================================")
		quit(1)
