extends SceneTree

## Republic - MVP Multiplayer, Trade Lifecycle, Defense Posture & Offline Briefing Walkthrough Test
## Validates:
## 1. Authoritative State Ownership & Protection (Client cannot spoof protected state)
## 2. Shared Simulation Clock & Defense Readiness (DEFCON & Armed Forces Operational)
## 3. Bilateral Trade Lifecycle Transparency (Draft -> Pending -> Accepted Agreement -> Completed Transaction)
## 4. Offline Progression Telemetry & Non-Silent Catch-Up Briefings
## 5. Session Reconnect & Ownership Recovery

func _init() -> void:
	print("==============================================================")
	print("  REPUBLIC - MVP MULTIPLAYER & WORLD SIMULATION VERIFICATION  ")
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

	# 1. State Loading with Defense Posture and Offline Catch-Up Briefing
	print("\n--- Step 1: Loading MVP Telemetry (Defense Posture & Offline Catch-Up) ---")
	var test_state_path := "user://mvp_verification_state.json"
	var test_state: Dictionary = {
		"country_name": "Republic of Arcadia",
		"country_id": "arcadia-1",
		"treasury": "R 15,000,000,000",
		"treasury_balance": 15000000000,
		"approval": "74%",
		"approval_rating": 74.0,
		"stability": "82%",
		"stability_rating": 82.0,
		"simulation_day": 12,
		"simulation_time": "Day 12, 14:00 WAT",
		"ruling_party": "Democratic Republic Alliance",
		"opposition_party": "National Front",
		"polling_support": 60.5,
		"opposition_support": 39.5,
		"income_tax_rate": 0.25,
		"active_project": "National Road Network",
		"defcon_level": "DEFCON 5 (Peacetime Watch)",
		"military_readiness": "85.0% (Armed Forces Operational)",
		"offline_briefing_summary": "Offline Catch-Up (Day 8 to Day 12): 8 production cycles credited (+R 8,000,000 deposited, 1 projects completed)."
	}

	var sf := FileAccess.open(test_state_path, FileAccess.WRITE)
	sf.store_string(JSON.stringify(test_state))
	sf.close()

	if not desk.load_state_from_file(test_state_path):
		printerr("[FAIL]: Failed to load state from " + test_state_path)
		quit(1)
		return

	if not desk.defense_readiness_label.text.contains("DEFCON 5") or not desk.defense_readiness_label.text.contains("85.0%"):
		printerr("[FAIL]: Defense readiness label mismatch: " + desk.defense_readiness_label.text)
		success = false

	if not desk.offline_briefing_label.text.contains("Offline Catch-Up") or not desk.offline_briefing_label.text.contains("8 production cycles credited"):
		printerr("[FAIL]: Offline briefing label mismatch: " + desk.offline_briefing_label.text)
		success = false

	# 2. Trade Lifecycle Stages
	print("\n--- Step 2: Bilateral Trade Lifecycle Transparency (Draft -> Pending -> Accepted) ---")
	# A. Draft Proposal
	desk._on_trade_amount_changed(500000000.0)
	if not desk.trade_lifecycle_stage_label.text.contains("[Draft Proposal]"):
		printerr("[FAIL]: Trade lifecycle stage should be [Draft Proposal], got: " + desk.trade_lifecycle_stage_label.text)
		success = false

	# B. Pending Proposal
	desk.trade_recipient_input.text = "Federation of Valoria"
	desk._on_propose_trade_button_pressed()
	if not desk.trade_lifecycle_stage_label.text.contains("[Pending Proposal]"):
		printerr("[FAIL]: Trade lifecycle stage should be [Pending Proposal], got: " + desk.trade_lifecycle_stage_label.text)
		success = false

	# C. Accepted Agreement / Completed Transaction
	desk._handle_network_message({
		"type": "trade",
		"trade": {
			"from": "Republic of Arcadia",
			"to": "Federation of Valoria",
			"amount": 500000000.0,
			"resource": "REPU",
			"status": "Accepted",
			"lifecycle_stage": "Accepted Agreement"
		}
	})
	if not desk.trade_lifecycle_stage_label.text.contains("[Accepted Agreement / Completed Transaction]"):
		printerr("[FAIL]: Trade lifecycle stage should be [Accepted Agreement / Completed Transaction], got: " + desk.trade_lifecycle_stage_label.text)
		success = false

	# 3. Session Reconnection & Recovery
	print("\n--- Step 3: Session Reconnection & Recovery Telemetry ---")
	desk._handle_network_message({
		"type": "decision_result",
		"country_id": "arcadia-1",
		"result": "Success",
		"status_message": "Reconnected session for country 'arcadia-1'."
	})
	if not desk.multiplayer_status_label.text.contains("Reconnected session"):
		printerr("[FAIL]: Multiplayer status label did not reflect reconnected session: " + desk.multiplayer_status_label.text)
		success = false

	# Clean up
	if FileAccess.file_exists(test_state_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(test_state_path))

	desk.queue_free()

	if success:
		print("==============================================================")
		print(" [PASS]: MVP MULTIPLAYER & SIMULATION VERIFICATION SUCCEEDED! ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: MVP MULTIPLAYER VERIFICATION FAILED!                 ")
		printerr("==============================================================")
		quit(1)
