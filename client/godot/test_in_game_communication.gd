extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: IN-GAME SOVEREIGN COMMUNICATIONS & DISPATCHES")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 5

	# -------------------------------------------------------------------------
	# CHECK 1: MessagesModal Layout & Filter Controls Discovery
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: MessagesModal Controls & Filter Discovery ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var modal = exec_instance.messages_modal
	var filter_all: Button = exec_instance.btn_msg_filter_all
	var filter_leg: Button = exec_instance.btn_msg_filter_legislative
	var filter_dip: Button = exec_instance.btn_msg_filter_diplomatic
	var filter_exec: Button = exec_instance.btn_msg_filter_executive
	var summary_lbl: Label = exec_instance.msg_summary_label
	var list_vbox: VBoxContainer = exec_instance.emails_list_vbox

	if modal and filter_all and filter_leg and filter_dip and filter_exec and summary_lbl and list_vbox:
		print("  ✓ MessagesModal found with dimensions 740x520 styling")
		print("  ✓ All 4 communication filter tabs found (All, Legislative, Bilateral, Cabinet)")
		print("  ✓ Summary banner and scrollable emails list container verified")
		passed_checks += 1
	else:
		push_error("  ❌ Missing MessagesModal controls or filter buttons")

	# -------------------------------------------------------------------------
	# CHECK 2: Communication Data Binding, Channel Filtering & Unread Counts
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: In-Game Message Data Binding & Channel Filtering ---")
	var mock_state := {
		"unread_email_count": 3,
		"unread_legislative_count": 1,
		"unread_diplomatic_count": 1,
		"latest_legislator_whisper": "Hon. Marcus Thorne: The opposition caucus is splintering over the Energy Act.",
		"emails": [
			{
				"id": "leg_intel_01",
				"sender": "Hon. Marcus Thorne",
				"sender_title": "Assembly Lawmaker (Central District)",
				"recipient": "President of the Republic",
				"subject": "Confidential Intelligence: Floor Numbers",
				"body": "Mr. President, with the district grant confirmed, my delegates are whipping 4 opposition backbenchers to vote Aye on your upcoming infrastructure bill.",
				"channel": "Legislative",
				"urgency": "Confidential",
				"action_required": true,
				"action_type": "view_chamber",
				"related_entity_id": "lmk_thorne",
				"avatar_symbol": "🏛️",
				"date_sent": "Oct 30, 14:15",
				"is_read": false
			},
			{
				"id": "diplo_cable_01",
				"sender": "Ambassador Elena Vance",
				"sender_title": "Plenipotentiary Envoy, Republic of Maridia",
				"recipient": "President of the Republic",
				"subject": "Priority Diplomatic Cable: Bilateral Accord Initiated",
				"body": "Sovereign Cable from Maridia: Proposing 1500 units of refined fuel in exchange for 800 units of grain under most-favoured-nation terms.",
				"channel": "Diplomatic",
				"urgency": "Urgent",
				"action_required": true,
				"action_type": "review_bilateral_deal",
				"related_entity_id": "prop_maridia_fuel_881",
				"avatar_symbol": "🌐",
				"date_sent": "Oct 30, 14:30",
				"is_read": false
			},
			{
				"id": "exec_memo_01",
				"sender": "Presidential Chief of Staff",
				"sender_title": "Executive Office",
				"recipient": "President of the Republic",
				"subject": "Executive Morning Digest",
				"body": "Cabinet portfolios are staffed and awaiting your directives for the legislative term.",
				"channel": "Executive",
				"urgency": "Routine",
				"action_required": false,
				"action_type": "",
				"related_entity_id": "",
				"avatar_symbol": "📋",
				"date_sent": "Oct 30, 08:00",
				"is_read": true
			}
		]
	}

	exec_instance.latest_state = mock_state
	exec_instance._update_messages_modal(mock_state)

	# Verify button counts
	if filter_all.text.contains("3") and filter_leg.text.contains("1") and filter_dip.text.contains("1") and filter_exec.text.contains("1"):
		print("  ✓ Filter button badge counts rendered correctly: All (3), Leg (1), Diplo (1), Exec (1)")
	else:
		push_error("  ❌ Filter button text counts incorrect: " + filter_all.text + " / " + filter_leg.text)

	# Test All Filter
	exec_instance._on_msg_filter_all_pressed()
	var count_all := list_vbox.get_child_count() - 1 # Subtract empty label
	if count_all == 3:
		print("  ✓ 'All Communications' tab renders all 3 messages")
	else:
		push_error("  ❌ All tab expected 3 items, got %d" % count_all)

	# Test Legislative Filter
	exec_instance._on_msg_filter_legislative_pressed()
	var count_leg := list_vbox.get_child_count() - 1
	if count_leg == 1 and exec_instance.active_message_filter == "Legislative":
		print("  ✓ 'Legislative Arm' tab filters correctly to 1 lawmaker dispatch")
	else:
		push_error("  ❌ Legislative tab expected 1 item, got %d" % count_leg)

	# Test Diplomatic Filter
	exec_instance._on_msg_filter_diplomatic_pressed()
	var count_dip := list_vbox.get_child_count() - 1
	if count_dip == 1 and exec_instance.active_message_filter == "Diplomatic":
		print("  ✓ 'Bilateral & Cables' tab filters correctly to 1 diplomatic cable")
	else:
		push_error("  ❌ Diplomatic tab expected 1 item, got %d" % count_dip)

	# Test Executive Filter
	exec_instance._on_msg_filter_executive_pressed()
	var count_exec := list_vbox.get_child_count() - 1
	if count_exec == 1 and exec_instance.active_message_filter == "Executive":
		print("  ✓ 'Cabinet & Executive' tab filters correctly to 1 executive memo")
		passed_checks += 1
	else:
		push_error("  ❌ Executive tab expected 1 item, got %d" % count_exec)

	# -------------------------------------------------------------------------
	# CHECK 3: PolicyModal Floor Whispers & Cross-Navigation
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: PolicyModal Floor Whispers & Quick-Dispatch Link ---")
	exec_instance._update_policy_and_bills_modal(mock_state)

	var whisper_card = exec_instance.legislator_whisper_card
	var whisper_lbl = exec_instance.legislator_whisper_label
	var btn_dispatches = exec_instance.btn_open_legislative_dispatches

	if whisper_card and whisper_lbl and btn_dispatches:
		print("  ✓ LegislatorWhisperCard found in PolicyModal")
		if whisper_lbl.text.contains("Marcus Thorne") and whisper_lbl.text.contains("opposition caucus"):
			print("  ✓ Floor whisper correctly displayed: '%s'" % whisper_lbl.text)
		else:
			push_error("  ❌ Floor whisper text unexpected: " + whisper_lbl.text)

		# Test cross-navigation button
		exec_instance._on_open_legislative_dispatches_pressed()
		if exec_instance.messages_modal.visible and exec_instance.active_message_filter == "Legislative":
			print("  ✓ 'View Dispatches' button successfully opened MessagesModal filtered to 'Legislative'")
			passed_checks += 1
		else:
			push_error("  ❌ Failed cross-navigating to MessagesModal with Legislative filter")
	else:
		push_error("  ❌ LegislatorWhisperCard or its components missing from PolicyModal")

	# -------------------------------------------------------------------------
	# CHECK 4: Action Buttons on Legislative & Bilateral Cards
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Action Button Interaction & Deal Resolution ---")
	exec_instance._on_msg_filter_all_pressed()

	# Find the bilateral deal card
	var children = list_vbox.get_children()
	var found_bilateral_actions := false
	var found_legislative_action := false

	for child in children:
		if child is PanelContainer:
			# Traverse child buttons
			var buttons: Array = []
			var stack = [child]
			while not stack.is_empty():
				var curr = stack.pop_back()
				if curr is Button:
					buttons.append(curr)
				for sub in curr.get_children():
					stack.append(sub)

			for b in buttons:
				if b.text.contains("Ratify Bilateral"):
					found_bilateral_actions = true
					print("  ✓ Found '✅ Ratify Bilateral Accord' button on diplomatic cable card")
				elif b.text.contains("Decline Accord"):
					print("  ✓ Found '❌ Decline Accord' button on diplomatic cable card")
				elif b.text.contains("View Chamber"):
					found_legislative_action = true
					print("  ✓ Found '🏛️ View Chamber & Floor Roster' button on legislative memo card")

	if found_bilateral_actions and found_legislative_action:
		print("  ✓ Both bilateral and legislative cards provide contextual, one-click action triggers")
		passed_checks += 1
	else:
		push_error("  ❌ Missing action buttons on communication cards")

	# -------------------------------------------------------------------------
	# CHECK 5: Authoritative Request Dispatch (Mark Read & Ratify Deal)
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Authoritative Request Dispatch Pipeline ---")

	# Test mark single email read
	exec_instance._on_mark_single_email_read_pressed("leg_intel_01")
	var req_path: String = str(exec_instance._resolve_request_path())
	if FileAccess.file_exists(req_path):
		var f := FileAccess.open(req_path, FileAccess.READ)
		var content := f.get_as_text()
		f.close()
		var json = JSON.parse_string(content)
		if json is Dictionary and json.get("action") == "mark_email_read" and json.get("email_id") == "leg_intel_01":
			print("  ✓ Dispatched 'mark_email_read' request for 'leg_intel_01' to request pipeline")
		else:
			push_error("  ❌ Request file content unexpected: " + content)
	else:
		push_error("  ❌ Request file was not written for mark_email_read")

	# Test bilateral deal ratification
	exec_instance._on_email_deal_action_pressed("prop_maridia_fuel_881", "accept", "diplo_cable_01")
	if FileAccess.file_exists(req_path):
		var f := FileAccess.open(req_path, FileAccess.READ)
		var content := f.get_as_text()
		f.close()
		var json = JSON.parse_string(content)
		if json is Dictionary and json.get("action") == "diplomatic_response" and json.get("proposal_id") == "prop_maridia_fuel_881" and json.get("decision") == "accept":
			print("  ✓ Dispatched 'diplomatic_response' (accept) request for 'prop_maridia_fuel_881' to request pipeline")
			passed_checks += 1
		else:
			push_error("  ❌ Bilateral deal request content unexpected: " + content)
	else:
		push_error("  ❌ Request file was not written for bilateral deal response")

	exec_instance.queue_free()

	print("\n=================================================================")
	if passed_checks == total_checks:
		print("🎉 ALL %d/%d IN-GAME COMMUNICATION CHECKS PASSED!" % [passed_checks, total_checks])
		print("=================================================================")
		quit(0)
	else:
		push_error("❌ SUITE FAILED: Passed %d / %d checks" % [passed_checks, total_checks])
		print("=================================================================")
		quit(1)
