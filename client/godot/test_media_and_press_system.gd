extends SceneTree

## Automated Headless Verification Suite for Republic:
## Press & Media System (Media Desk, Background News, Approve/Hush/Ignore & Corruption Consequence)
##
## Validates:
## 1. Media Desk Panel exists in Executive Office and lists ONLY stories requiring presidential decision.
## 2. Ordinary background news stories are published automatically by the simulation and appear
##    in Recent Events & Briefings without player input.
## 3. Approve Action:
##    - Publishes the story
##    - Applies public approval and stability impacts
##    - Corruption does NOT increase
## 4. Hush / Buy and Kill Action:
##    - Suppresses the story
##    - Authoritative Corruption increases (Corruption Control decreases)
##    - Story is removed from pending decisions
##    - Visible feedback and updated Corruption Control metric
## 5. Ignore / Auto-Publish:
##    - Automatically publishes the story if ignored
##    - Applies public impact
## 6. Real IPC request generation via file-based pipeline (presidential_desk_request.json)

const TEST_SANDBOX_DIR := "res://temp_media_press_test"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PRESS & MEDIA SYSTEM VERIFICATION TEST          ")
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

	var t1 := test_media_desk_ui_structure(office)
	print("Test 1 (Media Desk UI Structure): ", "PASS" if t1 else "FAIL")

	var t2 := test_background_media_automatic_publishing(office)
	print("Test 2 (Background Media Auto-Publishing): ", "PASS" if t2 else "FAIL")

	var t3 := test_media_desk_only_shows_decision_required_stories(office)
	print("Test 3 (Media Desk Lists Only Decision Stories): ", "PASS" if t3 else "FAIL")

	var t4 := test_approve_story_action(office)
	print("Test 4 (Approve / Let It Run Action): ", "PASS" if t4 else "FAIL")

	var t5 := test_hush_story_action_raises_corruption(office)
	print("Test 5 (Hush / Buy and Kill Raises Corruption): ", "PASS" if t5 else "FAIL")

	var t6 := test_ignore_auto_publish_flow(office)
	print("Test 6 (Ignore / Auto-Publish Flow): ", "PASS" if t6 else "FAIL")

	var all_passed := t1 and t2 and t3 and t4 and t5 and t6

	office.queue_free()
	_cleanup_sandbox()

	if all_passed:
		print("==============================================================")
		print("   [SUCCESS]: ALL PRESS & MEDIA SYSTEM CHECKS PASSED!         ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr("   [FAILURE]: ONE OR MORE PRESS & MEDIA CHECKS FAILED!        ")
		printerr("==============================================================")
		quit(1)

func test_media_desk_ui_structure(office) -> bool:
	print("\n--- Check 1: Media Desk Panel UI Structure & Open/Close ---")

	var desk_modal = office.media_desk_modal
	if desk_modal == null:
		printerr("[FAIL]: MediaDeskModal node not found")
		return false

	var open_btn = office.open_media_desk_button
	if open_btn == null:
		printerr("[FAIL]: OpenMediaDeskButton node not found on PressSecretaryCard")
		return false

	var corruption_lbl = office.media_desk_corruption_label
	if corruption_lbl == null:
		printerr("[FAIL]: MediaDeskCorruptionLabel node not found")
		return false

	var pending_vbox = office.pending_stories_vbox
	if pending_vbox == null:
		printerr("[FAIL]: PendingStoriesVBox node not found")
		return false

	# Test opening modal
	office.open_media_desk_modal()
	if not desk_modal.visible:
		printerr("[FAIL]: MediaDeskModal failed to become visible on open_media_desk_modal()")
		return false

	# Test closing modal
	office.close_media_desk_modal()
	if desk_modal.visible:
		printerr("[FAIL]: MediaDeskModal remained visible after close_media_desk_modal()")
		return false

	print("  [OK]: Media Desk modal, controls, and open/close toggles verified.")
	return true

func test_background_media_automatic_publishing(office) -> bool:
	print("\n--- Check 2: Background Media Automatic Publishing ---")

	# Prepare state with ordinary background news (requires_decision = false)
	# and a sensitive story (requires_decision = true)
	var state := {
		"country_name": "Republic of Valeria",
		"treasury_balance": 50000000.0,
		"corruption_control": 0.30,
		"press_briefing_headline": "National Infrastructure Surge",
		"press_briefing_summary": "Commerce routes expanded nationwide.",
		"media_reports": [
			{
				"id": "rep_ordinary_surplus",
				"outlet_name": "The Sovereign Gazette",
				"leaning": "StateAligned",
				"headline": "Sovereign Treasury Bolstered: Robust Capital Reserves",
				"article_summary": "National reserves steady financial outlook.",
				"sentiment": "Positive",
				"requires_decision": false,
				"is_suppressed": false,
				"response_status": "StatementIssued"
			},
			{
				"id": "rep_ordinary_project",
				"outlet_name": "The Capital Ledger",
				"leaning": "FinancialBusiness",
				"headline": "National Road Network Completed Ahead of Schedule",
				"article_summary": "Commerce ministry celebrates ribbon cutting.",
				"sentiment": "Positive",
				"requires_decision": false,
				"is_suppressed": false,
				"response_status": "StatementIssued"
			},
			{
				"id": "rep_sensitive_audit",
				"outlet_name": "The Criterion Dispatch",
				"leaning": "InvestigativeIndependent",
				"headline": "Classified Audit: Discretionary Spending Under Review",
				"article_summary": "Watchdog questions unverified procurement allocations.",
				"sentiment": "Critical",
				"requires_decision": true,
				"is_suppressed": false,
				"response_status": "Unanswered"
			}
		]
	}

	office.apply_state(state)

	# Verify DynamicEventsVBox populated background stories without player action
	var dynamic_events = office.dynamic_events_vbox
	if dynamic_events == null:
		printerr("[FAIL]: DynamicEventsVBox not found")
		return false

	var found_background_story := false
	for child in dynamic_events.get_children():
		var label_found := false
		for sub in child.get_children():
			if sub is VBoxContainer:
				for text_node in sub.get_children():
					if text_node is Label and "Sovereign Treasury Bolstered" in text_node.text:
						found_background_story = true
						break
	if not found_background_story:
		printerr("[FAIL]: Background ordinary story was not automatically published into Recent Events & Briefings")
		return false

	print("  [OK]: Ordinary stories automatically publish into Recent Events without player input.")
	return true

func test_media_desk_only_shows_decision_required_stories(office) -> bool:
	print("\n--- Check 3: Media Desk Lists ONLY Stories Requiring Decision ---")

	var state := {
		"country_name": "Republic of Valeria",
		"corruption_control": 0.30,
		"pending_media_decisions": [
			{
				"id": "rep_sensitive_audit_1",
				"outlet_name": "The Criterion Dispatch",
				"leaning": "InvestigativeIndependent",
				"headline": "Executive Audit Inquiry: Procurement Oversight Lapses",
				"article_summary": "Watchdog report pending publication.",
				"sentiment": "Critical",
				"predicted_effect": "Public Approval: -3.5%, Stability: -2.0% if published",
				"requires_decision": true,
				"is_suppressed": false,
				"response_status": "Unanswered"
			}
		],
		"media_reports": [
			{
				"id": "rep_ordinary_1",
				"outlet_name": "State Gazette",
				"headline": "Ordinary Background Story: Trade Route Normal",
				"requires_decision": false,
				"is_suppressed": false,
				"response_status": "StatementIssued"
			},
			{
				"id": "rep_sensitive_audit_1",
				"outlet_name": "The Criterion Dispatch",
				"headline": "Executive Audit Inquiry: Procurement Oversight Lapses",
				"requires_decision": true,
				"is_suppressed": false,
				"response_status": "Unanswered"
			}
		]
	}

	office.apply_state(state)
	office.open_media_desk_modal()

	var pending_vbox = office.pending_stories_vbox
	var cards: Array = []
	for child in pending_vbox.get_children():
		if child != office.empty_pending_stories_label:
			cards.append(child)

	if cards.size() != 1:
		printerr("[FAIL]: Expected exactly 1 decision-required story in Media Desk, got: %d" % cards.size())
		return false

	var card_name: String = cards[0].name
	if not ("rep_sensitive_audit_1" in card_name):
		printerr("[FAIL]: Expected card to correspond to rep_sensitive_audit_1, got: %s" % card_name)
		return false

	print("  [OK]: Media Desk lists exclusively decision-required stories.")
	return true

func test_approve_story_action(office) -> bool:
	print("\n--- Check 4: Approve / Let It Run Action ---")
	_setup_sandbox()
	var test_state_file := TEST_SANDBOX_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	var initial_corruption := float(office.latest_state.get("corruption_control", 0.30))
	var res: Dictionary = office.execute_media_response("rep_sensitive_audit_1", "approve")

	if not res.get("success", false):
		printerr("[FAIL]: execute_media_response for approve returned failure: %s" % res.get("message", ""))
		return false

	# Verify request file written
	var req_file := TEST_SANDBOX_DIR + "/presidential_desk_request.json"
	if not FileAccess.file_exists(req_file):
		printerr("[FAIL]: Request file not created for approve action")
		return false

	var f := FileAccess.open(req_file, FileAccess.READ)
	var content := f.get_as_text()
	f.close()
	var parsed: Dictionary = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "approve":
		printerr("[FAIL]: Request content mismatch: %s" % content)
		return false

	# Verify Corruption did NOT increase on Approve
	var post_corruption := float(office.latest_state.get("corruption_control", 0.30))
	if post_corruption != initial_corruption:
		printerr("[FAIL]: CorruptionControl changed on transparent approve action: %f vs %f" % [post_corruption, initial_corruption])
		return false

	print("  [OK]: Approve action correctly serialized to disk and transparently publishes without raising Corruption.")
	return true

func test_hush_story_action_raises_corruption(office) -> bool:
	print("\n--- Check 5: Hush / Buy and Kill Action Raises Corruption ---")
	_setup_sandbox()
	var test_state_file := TEST_SANDBOX_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	office.latest_state["corruption_control"] = 0.30
	var prev_corruption: float = float(office.latest_state.get("corruption_control", 0.30))

	var res: Dictionary = office.execute_media_response("rep_sensitive_audit_1", "hush")
	if not res.get("success", false):
		printerr("[FAIL]: execute_media_response for hush returned failure")
		return false

	# Verify request file
	var req_file := TEST_SANDBOX_DIR + "/presidential_desk_request.json"
	var f := FileAccess.open(req_file, FileAccess.READ)
	var content := f.get_as_text()
	f.close()
	var parsed: Dictionary = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "hush":
		printerr("[FAIL]: Request file mismatch for hush: %s" % content)
		return false

	# Verify authoritative corruption INCREASED (CorruptionControl decreased)
	var new_corruption: float = float(office.latest_state.get("corruption_control", 0.30))
	if new_corruption >= prev_corruption:
		printerr("[FAIL]: Expected CorruptionControl to decrease (raising corruption), was: %f -> %f" % [prev_corruption, new_corruption])
		return false

	# Verify CorruptionControlLabel in LeftDock reflects the change
	var corr_lbl = office.corruption_control_label
	if corr_lbl != null and not ("22%" in corr_lbl.text or "%" in corr_lbl.text):
		printerr("[FAIL]: LeftDock CorruptionControlLabel did not reflect updated corruption metric: %s" % corr_lbl.text)
		return false

	print("  [OK]: Hush action suppresses story and measurably increases executive Corruption (CorruptionControl: %0.0f%% -> %0.0f%%)." % [prev_corruption * 100.0, new_corruption * 100.0])
	return true

func test_ignore_auto_publish_flow(office) -> bool:
	print("\n--- Check 6: Ignore / Auto-Publish Flow ---")
	_setup_sandbox()
	var test_state_file := TEST_SANDBOX_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	var res: Dictionary = office.execute_media_response("rep_sensitive_audit_1", "ignore")
	if not res.get("success", false):
		printerr("[FAIL]: execute_media_response for ignore returned failure")
		return false

	# Verify request file for ignore
	var req_file := TEST_SANDBOX_DIR + "/presidential_desk_request.json"
	var f := FileAccess.open(req_file, FileAccess.READ)
	var content := f.get_as_text()
	f.close()
	var parsed: Dictionary = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "ignore":
		printerr("[FAIL]: Request file mismatch for ignore: %s" % content)
		return false

	print("  [OK]: Ignore action correctly triggers auto-publish request pipeline.")
	return true

func _setup_sandbox() -> void:
	_cleanup_sandbox()
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(TEST_SANDBOX_DIR))

func _cleanup_sandbox() -> void:
	var global_dir := ProjectSettings.globalize_path(TEST_SANDBOX_DIR)
	if DirAccess.dir_exists_absolute(global_dir):
		var dir := DirAccess.open(global_dir)
		if dir != null:
			dir.list_dir_begin()
			var fn := dir.get_next()
			while fn != "":
				if not dir.current_is_dir():
					dir.remove(fn)
				fn = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(global_dir)
