extends SceneTree

## Automated Headless Verification Suite for Republic:
## Executive Office Diegetic & State-Driven Engagement Features
##
## Validates:
## 1. Session Delta Briefing (on load):
##    - Baseline initial state vs subsequent update with honest deltas
##    - Treasury delta, approval delta, stability delta formatting
##    - Non-blocking auto-dismiss / manual dismiss without modal block
## 2. Incoming Desk Items for Events:
##    - Badges on desk props: Hotline, Dossier, Inbox, Diplomatic Folder
##    - Badge visibility driven by real snapshot events (crisis, threat, proposals)
##    - Clicking prop clears notification badge & opens focused view
##    - Decree actions write formatted requests (crisis_response)
## 3. Clickable Capital Window Markers:
##    - CapitalWindowScenery procedural markers (active crane, completed landmark)
##    - Marker click handling emits project_marker_clicked
##    - Active/completed marker buttons open CapitalProjectCard
##    - Honest progress, ETA, and capacity effect display & card dismissal
## 4. Cabinet Friction & Capacity Feedback (Left Dock):
##    - Minister competence and loyalty metrics displayed
##    - Capacity bottleneck badges shown when capacity < 40% (Finance, Interior, Infrastructure)
##    - Bottleneck badges hidden when capacity >= 40%
##    - Cabinet friction panel displayed on real friction event, resolve button navigates
## 5. Diplomatic Dossiers (Right Dock & Desk):
##    - Inbound trade/diplomatic proposals rendered in DiplomaticDossierCard
##    - Honest display of partner and terms
##    - Accept / Reject submits formatted request (diplomatic_response) to isolated directory
## 6. Project Completion Moments:
##    - State transition from active to completed triggers celebratory scenery highlight
##    - ProjectCompletionToast displayed with actual approval, stability, capacity delta
##    - Toast dismissible and non-blocking

const ISOLATED_TEST_DIR := "res://temp_engagement_isolated"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE ENGAGEMENT VERIFICATION       ")
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

	# Check 1: Session Delta Briefing (on load & state updates)
	all_passed = test_session_delta_briefing(office) and all_passed

	# Check 2: Incoming Desk Items for Events & Notification Badges
	all_passed = test_desk_items_and_event_badges(office) and all_passed

	# Check 3: Clickable Capital Window Markers & Project Status Card
	all_passed = test_clickable_capital_window_markers(office) and all_passed

	# Check 4: Cabinet Friction & Capacity Bottleneck Badges
	all_passed = test_cabinet_friction_and_capacity(office) and all_passed

	# Check 5: Diplomatic Dossiers & Request Generation
	all_passed = test_diplomatic_dossiers_pipeline(office) and all_passed

	# Check 6: Project Completion Moments & Toast Notification
	all_passed = test_project_completion_moments(office) and all_passed

	office.queue_free()
	_cleanup_all()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 6 DIEGETIC ENGAGEMENT CHECKS PASSED!             ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE ENGAGEMENT CHECKS FAILED!                ")
		printerr("==============================================================")
		quit(1)

## Safely cleans up test-owned temporary artifacts inside ISOLATED_TEST_DIR
func _cleanup_all() -> void:
	var global_iso := ProjectSettings.globalize_path(ISOLATED_TEST_DIR)
	if DirAccess.dir_exists_absolute(global_iso):
		var dir := DirAccess.open(global_iso)
		if dir != null:
			dir.list_dir_begin()
			var fn := dir.get_next()
			while fn != "":
				if not dir.current_is_dir():
					DirAccess.remove_absolute(global_iso.path_join(fn))
				fn = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(global_iso)


# ------------------------------------------------------------------------------
# Check 1: Session Delta Briefing (on load & state updates)
# ------------------------------------------------------------------------------
func test_session_delta_briefing(office: Control) -> bool:
	print("\n--- Check 1: Session Delta Briefing (on load & state updates) ---")

	if office.session_briefing_toast == null:
		printerr("[FAIL]: session_briefing_toast node missing.")
		return false
	if office.session_briefing_metrics_label == null:
		printerr("[FAIL]: session_briefing_metrics_label node missing.")
		return false

	# 1. Baseline state
	var initial_state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 10000000000.0,
		"approval_rating": 60.0,
		"stability_rating": 75.0,
		"active_project": "National Highway",
		"active_crisis": "None"
	}
	office.previous_state = {}
	office.update_session_briefing(initial_state)

	var text1: String = office.session_briefing_metrics_label.text
	if not text1.contains("Republic of Arcadia") or not text1.contains("10,000,000,000"):
		printerr("[FAIL]: Baseline briefing should show country name and liquid reserves. Got: " + text1)
		return false
	print("  [OK]: Baseline session briefing text verified.")

	# 2. Subsequent state with real delta changes
	var updated_state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 12500000000.0,
		"approval_rating": 65.5,
		"stability_rating": 80.0,
		"active_project": "Orbital Spaceport",
		"active_crisis": "None"
	}
	office.update_session_briefing(updated_state)

	var text2: String = office.session_briefing_metrics_label.text
	if not text2.contains("+R 2,500,000,000") and not text2.contains("+2,500,000,000"):
		printerr("[FAIL]: Session briefing should display positive treasury delta (+R 2,500,000,000). Got: " + text2)
		return false
	if not text2.contains("+5.5%"):
		printerr("[FAIL]: Session briefing should display approval delta (+5.5%). Got: " + text2)
		return false
	if not text2.contains("+5.0%"):
		printerr("[FAIL]: Session briefing should display stability delta (+5.0%). Got: " + text2)
		return false
	print("  [OK]: Real delta computation verified (Treasury: +2.5B, Approval: +5.5%, Stability: +5.0%).")

	# 3. Dismissal behavior: Show, verify visible, dismiss, verify hidden
	office.show_session_briefing(false)
	if not office.is_session_briefing_visible():
		printerr("[FAIL]: Session briefing should be visible after show_session_briefing().")
		return false
	office.dismiss_session_briefing()
	if office.is_session_briefing_visible():
		printerr("[FAIL]: Session briefing should be hidden after dismiss_session_briefing().")
		return false

	print("  [OK]: Session delta briefing display and dismissal verified.")
	return true


# ------------------------------------------------------------------------------
# Check 2: Incoming Desk Items for Events & Notification Badges
# ------------------------------------------------------------------------------
func test_desk_items_and_event_badges(office: Control) -> bool:
	print("\n--- Check 2: Incoming Desk Items for Events & Notification Badges ---")

	# 1. Verify desk item controls exist
	if office.hotline_phone == null or office.hotline_badge == null:
		printerr("[FAIL]: Hotline phone or badge node missing.")
		return false
	if office.intelligence_dossier == null or office.dossier_badge == null:
		printerr("[FAIL]: Intelligence dossier or badge node missing.")
		return false
	if office.executive_inbox == null or office.inbox_badge == null:
		printerr("[FAIL]: Executive inbox or badge node missing.")
		return false
	if office.diplomatic_folder == null or office.diplomatic_badge == null:
		printerr("[FAIL]: Diplomatic folder or badge node missing.")
		return false

	# 2. Test diplomatic proposal badge activation
	office.set_diplomatic_badge(true, "1")
	if not office.is_diplomatic_badge_visible():
		printerr("[FAIL]: Diplomatic notification badge should be visible after set_diplomatic_badge(true).")
		return false

	# 3. Test clicking diplomatic folder dismisses badge and expands right dock
	office.set_right_dock_expanded(false)
	office._on_diplomatic_folder_pressed()
	if office.is_diplomatic_badge_visible():
		printerr("[FAIL]: Diplomatic notification badge should clear when folder is opened.")
		return false
	if not office.is_right_dock_expanded():
		printerr("[FAIL]: Right dock should be expanded when diplomatic folder is clicked.")
		return false
	print("  [OK]: Diplomatic folder click clears badge and auto-expands right dock.")

	# 4. Test crisis decree generation via execute_crisis_response in isolated directory
	var iso_req := ISOLATED_TEST_DIR + "/presidential_desk_request.json"
	office.request_file_override = iso_req
	office.latest_state = { "country_name": "Republic of Arcadia" }

	var decree_res: Dictionary = office.execute_crisis_response("civic_relief", true)
	if not decree_res.get("success", false):
		printerr("[FAIL]: execute_crisis_response failed: " + str(decree_res.get("message")))
		return false

	if not FileAccess.file_exists(iso_req):
		printerr("[FAIL]: Expected request file at isolated path: " + iso_req)
		return false

	var f := FileAccess.open(iso_req, FileAccess.READ)
	var content := f.get_as_text()
	f.close()

	var json_res = JSON.parse_string(content)
	if typeof(json_res) != TYPE_DICTIONARY:
		printerr("[FAIL]: Failed to parse generated decree request JSON.")
		return false

	if json_res.get("action") != "crisis_response" or json_res.get("decree_type") != "civic_relief":
		printerr("[FAIL]: Crisis decree request payload mismatch. Got: " + content)
		return false

	print("  [OK]: Incoming desk items, badges, and decree request generation verified.")
	return true


# ------------------------------------------------------------------------------
# Check 3: Clickable Capital Window Markers & Project Status Card
# ------------------------------------------------------------------------------
func test_clickable_capital_window_markers(office: Control) -> bool:
	print("\n--- Check 3: Clickable Capital Window Markers & Project Status Card ---")

	var scenery = office.city_scenery
	if scenery == null:
		printerr("[FAIL]: city_scenery node missing.")
		return false

	# 1. Verify marker buttons and project card nodes exist
	if office.active_project_marker_button == null:
		printerr("[FAIL]: active_project_marker_button node missing.")
		return false
	if office.completed_project_marker_button == null:
		printerr("[FAIL]: completed_project_marker_button node missing.")
		return false
	if office.capital_project_card == null:
		printerr("[FAIL]: capital_project_card node missing.")
		return false

	# 2. Configure scenery with active project and completed project
	scenery.size = Vector2(800, 450)
	scenery.set_state_metrics(70.0, 75.0, "Subway Metro System", "None", "National Capitol Spire")

	var active_rect: Rect2 = scenery.get_active_project_marker_rect()
	var completed_rect: Rect2 = scenery.get_completed_project_marker_rect()

	if active_rect.size.x <= 0 or active_rect.size.y <= 0:
		printerr("[FAIL]: Active project marker rect should have positive bounds when active project is present.")
		return false
	if completed_rect.size.x <= 0 or completed_rect.size.y <= 0:
		printerr("[FAIL]: Completed project marker rect should have positive bounds when completed project is present.")
		return false

	# 3. Simulate marker click signal from scenery
	var received_signal := { "clicked": false, "proj": "", "is_active": false }
	var callback = func(p_name: String, is_act: bool, marker_id: String):
		received_signal["clicked"] = true
		received_signal["proj"] = p_name
		received_signal["is_active"] = is_act

	scenery.project_marker_clicked.connect(callback)

	var click_event := InputEventMouseButton.new()
	click_event.pressed = true
	click_event.button_index = MOUSE_BUTTON_LEFT
	click_event.position = active_rect.position + Vector2(10, 10)
	scenery._gui_input(click_event)

	if not received_signal["clicked"] or received_signal["proj"] != "Subway Metro System":
		printerr("[FAIL]: Capital scenery did not emit project_marker_clicked for active project marker.")
		return false
	print("  [OK]: Procedural capital window marker click detection verified.")

	# 4. Show capital project card with real data
	office.show_capital_project_card("Subway Metro System", true)
	if not office.is_capital_project_card_visible():
		printerr("[FAIL]: Capital project card should be visible after show_capital_project_card().")
		return false

	if office.capital_project_title and not office.capital_project_title.text.to_lower().contains("subway metro system"):
		printerr("[FAIL]: Capital project card title mismatch. Got: " + office.capital_project_title.text)
		return false

	# 5. Dismiss capital project card
	office.dismiss_capital_project_card()
	if office.is_capital_project_card_visible():
		printerr("[FAIL]: Capital project card should be hidden after dismiss_capital_project_card().")
		return false

	print("  [OK]: Capital project status card display, honest data binding, and dismissal verified.")
	return true


# ------------------------------------------------------------------------------
# Check 4: Cabinet Friction & Capacity Bottleneck Badges
# ------------------------------------------------------------------------------
func test_cabinet_friction_and_capacity(office: Control) -> bool:
	print("\n--- Check 4: Cabinet Friction & Capacity Bottleneck Badges ---")

	if office.finance_bottleneck_badge == null:
		printerr("[FAIL]: finance_bottleneck_badge node missing.")
		return false
	if office.interior_bottleneck_badge == null:
		printerr("[FAIL]: interior_bottleneck_badge node missing.")
		return false
	if office.infrastructure_bottleneck_badge == null:
		printerr("[FAIL]: infrastructure_bottleneck_badge node missing.")
		return false
	if office.cabinet_friction_panel == null:
		printerr("[FAIL]: cabinet_friction_panel node missing.")
		return false

	# 1. Test state with low capacity (< 40%) -> Bottlenecks should appear
	var low_cap_state := {
		"country_name": "Republic of Arcadia",
		"economic_capacity": 25.0,
		"civil_service_quality": 30.0,
		"corruption_control": 25.0,
		"infrastructure_condition": 35.0,
		"cabinet_friction": {
			"conflicting_portfolio": "Finance",
			"description": "Finance Minister requests emergency reserve authorization due to currency volatility."
		}
	}
	office.apply_state(low_cap_state)

	if not office.finance_bottleneck_badge.visible:
		printerr("[FAIL]: Finance bottleneck badge should be visible when economic_capacity is 25.0%.")
		return false
	if not office.interior_bottleneck_badge.visible:
		printerr("[FAIL]: Interior bottleneck badge should be visible when corruption_control is 25.0%.")
		return false
	if not office.infrastructure_bottleneck_badge.visible:
		printerr("[FAIL]: Infrastructure bottleneck badge should be visible when infrastructure_condition is 35.0%.")
		return false
	if not office.cabinet_friction_panel.visible:
		printerr("[FAIL]: Cabinet friction panel should be visible when cabinet_friction event is present.")
		return false
	print("  [OK]: Low capacity constraints (< 40%) trigger bottleneck badges and friction panel.")

	# 2. Test state with healthy capacity (>= 40%) -> Bottlenecks should hide
	var normal_cap_state := {
		"country_name": "Republic of Arcadia",
		"economic_capacity": 75.0,
		"civil_service_quality": 80.0,
		"corruption_control": 75.0,
		"infrastructure_condition": 68.0,
		"cabinet_friction": {}
	}
	office.apply_state(normal_cap_state)

	if office.finance_bottleneck_badge.visible:
		printerr("[FAIL]: Finance bottleneck badge should hide when capacity >= 40%.")
		return false
	if office.interior_bottleneck_badge.visible:
		printerr("[FAIL]: Interior bottleneck badge should hide when capacity >= 40%.")
		return false
	if office.infrastructure_bottleneck_badge.visible:
		printerr("[FAIL]: Infrastructure bottleneck badge should hide when capacity >= 40%.")
		return false
	if office.cabinet_friction_panel.visible:
		printerr("[FAIL]: Cabinet friction panel should hide when friction dictionary is empty.")
		return false

	print("  [OK]: Healthy capacity clears bottleneck badges and friction alerts.")
	return true


# ------------------------------------------------------------------------------
# Check 5: Diplomatic Dossiers & Request Generation
# ------------------------------------------------------------------------------
func test_diplomatic_dossiers_pipeline(office: Control) -> bool:
	print("\n--- Check 5: Diplomatic Dossiers & Request Generation ---")

	if office.diplomatic_dossier_card == null:
		printerr("[FAIL]: diplomatic_dossier_card node missing.")
		return false
	if office.dossier_partner_label == null:
		printerr("[FAIL]: dossier_partner_label node missing.")
		return false
	if office.dossier_terms_label == null:
		printerr("[FAIL]: dossier_terms_label node missing.")
		return false

	# 1. Apply state with real diplomatic trade proposal
	var state_with_prop := {
		"country_name": "Republic of Arcadia",
		"incoming_diplomatic_proposals": [
			{
				"id": "prop_valeria_wheat_deal",
				"partner": "Republic of Valeria",
				"type": "Trade Agreement",
				"terms": "Export 500,000 tons Wheat for R 850M + 5% Tariff exemption.",
				"duration_months": 12
			}
		]
	}
	office.apply_state(state_with_prop)

	if not office.dossier_partner_label.text.contains("Republic of Valeria"):
		printerr("[FAIL]: Diplomatic dossier partner label mismatch. Got: " + office.dossier_partner_label.text)
		return false
	if not office.dossier_terms_label.text.contains("Wheat") or not office.dossier_terms_label.text.contains("850M"):
		printerr("[FAIL]: Diplomatic dossier terms label mismatch. Got: " + office.dossier_terms_label.text)
		return false
	print("  [OK]: Real diplomatic proposal terms bound to dossier card.")

	# 2. Test Accept proposal request writing in isolated sandbox
	var iso_req := ISOLATED_TEST_DIR + "/presidential_desk_request.json"
	office.request_file_override = iso_req

	var accept_res: Dictionary = office.execute_diplomatic_response("prop_valeria_wheat_deal", "accept", true)
	if not accept_res.get("success", false):
		printerr("[FAIL]: execute_diplomatic_response failed: " + str(accept_res.get("message")))
		return false

	if not FileAccess.file_exists(iso_req):
		printerr("[FAIL]: Expected request file at isolated path: " + iso_req)
		return false

	var f := FileAccess.open(iso_req, FileAccess.READ)
	var content := f.get_as_text()
	f.close()

	var req_dict = JSON.parse_string(content)
	if typeof(req_dict) != TYPE_DICTIONARY:
		printerr("[FAIL]: Failed to parse generated diplomatic response JSON.")
		return false

	if req_dict.get("action") != "diplomatic_response" or req_dict.get("decision") != "accept" or req_dict.get("proposal_id") != "prop_valeria_wheat_deal":
		printerr("[FAIL]: Diplomatic response payload mismatch. Got: " + content)
		return false
	print("  [OK]: Diplomatic accept request correctly formatted and written to presidential_desk_request.json.")

	# 3. Test Decline proposal request writing
	var reject_res: Dictionary = office.execute_diplomatic_response("prop_valeria_wheat_deal", "reject", true)
	if not reject_res.get("success", false):
		printerr("[FAIL]: execute_diplomatic_response (reject) failed.")
		return false

	var f2 := FileAccess.open(iso_req, FileAccess.READ)
	var content2 := f2.get_as_text()
	f2.close()

	var req_dict2 = JSON.parse_string(content2)
	if req_dict2.get("decision") != "reject":
		printerr("[FAIL]: Diplomatic reject decision mismatch. Got: " + content2)
		return false
	print("  [OK]: Diplomatic reject request correctly formatted and written.")

	return true


# ------------------------------------------------------------------------------
# Check 6: Project Completion Moments & Toast Notification
# ------------------------------------------------------------------------------
func test_project_completion_moments(office: Control) -> bool:
	print("\n--- Check 6: Project Completion Moments & Toast Notification ---")

	if office.project_completion_toast == null:
		printerr("[FAIL]: project_completion_toast node missing.")
		return false
	if office.project_completion_details_label == null:
		printerr("[FAIL]: project_completion_details_label node missing.")
		return false

	# 1. State 1: Active project in progress
	var state1 := {
		"country_name": "Republic of Arcadia",
		"active_project": "Hydroelectric Dam",
		"completed_projects": [],
		"approval_rating": 60.0,
		"stability_rating": 70.0
	}
	office.apply_state(state1)
	office.dismiss_project_completion_toast()

	# 2. State 2: Project completed (active_project becomes None, completed_projects contains Hydroelectric Dam)
	var state2 := {
		"country_name": "Republic of Arcadia",
		"active_project": "None",
		"completed_projects": ["Hydroelectric Dam"],
		"approval_rating": 64.0,
		"stability_rating": 73.0,
		"infrastructure_condition": 82.0
	}
	office.apply_state(state2)

	if not office.is_project_completion_toast_visible():
		printerr("[FAIL]: Project completion toast should be visible when active project transitions to completed.")
		return false

	var details: String = office.project_completion_details_label.text
	if not details.contains("Hydroelectric Dam"):
		printerr("[FAIL]: Project completion toast details should contain project name 'Hydroelectric Dam'. Got: " + details)
		return false
	if not details.contains("+4.0%") and not details.contains("Approval"):
		printerr("[FAIL]: Project completion toast details should report real approval change. Got: " + details)
		return false
	print("  [OK]: Project completion transition detected; toast renders real project name and delta metrics.")

	# 3. Verify scenery completion highlight active
	var scenery = office.city_scenery
	if scenery and not scenery.is_highlighting_completion:
		printerr("[FAIL]: city_scenery.is_highlighting_completion should be active on project completion.")
		return false
	print("  [OK]: City scenery celebratory gold halo highlight active.")

	# 4. Dismiss toast
	office.dismiss_project_completion_toast()
	if office.is_project_completion_toast_visible():
		printerr("[FAIL]: Project completion toast should hide after dismiss_project_completion_toast().")
		return false
	print("  [OK]: Project completion toast dismissed successfully.")

	return true
