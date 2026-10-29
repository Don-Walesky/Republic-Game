extends SceneTree

## Regression Test: Presidential Desk Phase 3-5 UI Node Presence & State Binding
## Verifies that all 7 Phase 3-5 nodes exist in PresidentialDesk.tscn and update their
## display text from authoritative simulation state snapshots without engine warnings.

func _init() -> void:
	print("==============================================================")
	print("  REPUBLIC - PRESIDENTIAL DESK PHASE 3-5 NODE REGRESSION TEST ")
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
	desk._init_controls()

	var success := true

	# 1. Verify Node References Exist and Are Not Null
	print("\n--- Check 1: Node References in Scene Tree ---")
	if desk.media_summary_label == null:
		printerr("[FAIL]: media_summary_label is null!")
		success = false
	else:
		print("  [OK]: media_summary_label found in scene tree.")

	if desk.investment_summary_label == null:
		printerr("[FAIL]: investment_summary_label is null!")
		success = false
	else:
		print("  [OK]: investment_summary_label found in scene tree.")

	if desk.strategic_direction_label == null:
		printerr("[FAIL]: strategic_direction_label is null!")
		success = false
	else:
		print("  [OK]: strategic_direction_label found in scene tree.")

	if desk.strategic_focus_label == null:
		printerr("[FAIL]: strategic_focus_label is null!")
		success = false
	else:
		print("  [OK]: strategic_focus_label found in scene tree.")

	if desk.military_progression_label == null:
		printerr("[FAIL]: military_progression_label is null!")
		success = false
	else:
		print("  [OK]: military_progression_label found in scene tree.")

	if desk.intelligence_status_label == null:
		printerr("[FAIL]: intelligence_status_label is null!")
		success = false
	else:
		print("  [OK]: intelligence_status_label found in scene tree.")

	if desk.intelligence_dossier_label == null:
		printerr("[FAIL]: intelligence_dossier_label is null!")
		success = false
	else:
		print("  [OK]: intelligence_dossier_label found in scene tree.")

	# 2. Verify State Binding & Rendering
	print("\n--- Check 2: Authoritative State Snapshot Binding ---")
	var sample_state: Dictionary = {
		"country_name": "Republic of Solaria",
		"treasury": "R 10,000,000",
		"approval": "55.0%",
		"stability": "78.0%",
		"media_summary": "3 active news outlets | Independent press rating 85%",
		"investment_summary": "2 pending domestic investment proposals (R 45M capital)",
		"strategic_direction": "Industrialisation",
		"strategic_direction_display": "Industrialisation & Manufacturing Expansion",
		"strategic_focus_statement": "Prioritise industrial infrastructure and manufacturing capability.",
		"strategic_transition_remaining": 5,
		"strategic_progress_index": 42.5,
		"military_level": 18,
		"base_military_level": 14,
		"counterintelligence_level": 62.0,
		"basic_espionage_unlocked": true,
		"advanced_espionage_unlocked": false,
		"espionage_lock_reason": "Requires Military Level 20 (Current: 18).",
		"acquired_intelligence_dossiers": [
			{
				"title": "Operation Nightfall Reconnaissance",
				"target_country_name": "Valoria",
				"confidence": "High",
				"freshness_days_remaining": 25
			}
		]
	}

	desk.apply_state(sample_state)

	if not desk.media_summary_label.text.contains("Independent press rating 85%"):
		printerr("[FAIL]: MediaSummaryLabel text did not reflect media_summary: ", desk.media_summary_label.text)
		success = false
	else:
		print("  [OK]: MediaSummaryLabel renders authoritative media summary.")

	if not desk.investment_summary_label.text.contains("R 45M capital"):
		printerr("[FAIL]: InvestmentSummaryLabel text did not reflect investment_summary: ", desk.investment_summary_label.text)
		success = false
	else:
		print("  [OK]: InvestmentSummaryLabel renders authoritative investment summary.")

	if not desk.strategic_direction_label.text.contains("Industrialisation") or not desk.strategic_direction_label.text.contains("42.5%"):
		printerr("[FAIL]: StrategicDirectionLabel text did not reflect direction/progress: ", desk.strategic_direction_label.text)
		success = false
	else:
		print("  [OK]: StrategicDirectionLabel renders strategic direction and transition countdown.")

	if not desk.strategic_focus_label.text.contains("manufacturing capability"):
		printerr("[FAIL]: StrategicFocusLabel text did not reflect focus statement: ", desk.strategic_focus_label.text)
		success = false
	else:
		print("  [OK]: StrategicFocusLabel renders strategic focus statement.")

	if not desk.military_progression_label.text.contains("Military Level: 18") or not desk.military_progression_label.text.contains("Counterintelligence: 62.0"):
		printerr("[FAIL]: MilitaryProgressionLabel text did not reflect military progression: ", desk.military_progression_label.text)
		success = false
	else:
		print("  [OK]: MilitaryProgressionLabel renders composite military level & counterintelligence.")

	if not desk.intelligence_status_label.text.contains("Tier 1") or not desk.intelligence_status_label.text.contains("Requires Military Level 20"):
		printerr("[FAIL]: IntelligenceStatusLabel text did not reflect intelligence status: ", desk.intelligence_status_label.text)
		success = false
	else:
		print("  [OK]: IntelligenceStatusLabel renders intelligence tier and lock reasons.")

	if not desk.intelligence_dossier_label.text.contains("Operation Nightfall Reconnaissance"):
		printerr("[FAIL]: IntelligenceDossierLabel text did not reflect acquired dossier: ", desk.intelligence_dossier_label.text)
		success = false
	else:
		print("  [OK]: IntelligenceDossierLabel renders classified dossier summary.")

	desk.queue_free()

	if success:
		print("\n==============================================================")
		print(" [PASS]: ALL 7 PHASE 3-5 UI NODES CONFIRMED PRESENT & FUNCTIONAL! ")
		print("==============================================================")
		quit(0)
	else:
		printerr("\n==============================================================")
		printerr(" [FAIL]: REGRESSION DETECTED IN PHASE 3-5 UI NODES!           ")
		printerr("==============================================================")
		quit(1)

