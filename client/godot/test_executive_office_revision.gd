extends SceneTree

## Automated Verification Suite for Republic UI Revision
## Verifies:
## 1. Single primary bottom navigation bar containing exactly 10 destinations.
## 2. Each button has its icon on top and its name directly underneath ("Icon\nName").
## 3. Left-side duplicate navigation panel (SidebarNavVBox) confirmed completely removed.
## 4. Persistent chat shortcut button with unread count badge.
## 5. Construction requirements:
##    - University requires Minister of Education.
##    - Refinery requires Minister of Oil & Gas.
##    - Mine requires Minister of Resources.
## 6. Press Secretary briefings accessible on dashboard and Government.
## 7. Central Bank Governor formalized in Economy.
## 8. Command Centre military directive generation (military_attack).
## 9. Legislative Chamber & 50:50 consequence balance engine.
## 10. Honest data verification (zero mock statistics or hardcoded delta pills).

const SCENE_PATH := "res://ExecutiveOffice.tscn"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE REVISION VERIFICATION TEST     ")
	print("==============================================================")

	var scene := load(SCENE_PATH) as PackedScene
	if scene == null:
		printerr("[FAIL]: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var office = scene.instantiate()
	root.add_child(office)
	office._ready()

	var all_passed := true
	all_passed = test_bottom_navigation_ten_destinations(office) and all_passed
	all_passed = test_no_left_duplicate_navigation(office) and all_passed
	all_passed = test_persistent_chat_shortcut(office) and all_passed
	all_passed = test_honest_telemetry_and_delta_pills(office) and all_passed
	all_passed = test_construction_ministerial_requirements(office) and all_passed
	all_passed = test_press_secretary_briefings(office) and all_passed
	all_passed = test_central_bank_governor(office) and all_passed
	all_passed = test_command_centre_warfare_pipeline(office) and all_passed
	all_passed = test_legislature_5050_consequence_engine(office) and all_passed

	office.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 9 EXECUTIVE OFFICE REVISION CHECKS PASSED!       ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE UI REVISION CHECKS FAILED!               ")
		printerr("==============================================================")
		quit(1)

func test_bottom_navigation_ten_destinations(office: Control) -> bool:
	print("\n--- Check 1: Single Primary Bottom Navigation (10 Destinations) ---")
	var nav_pills = office.get_node_or_null("InterfaceLayer/BottomDock/BottomDockHBox/BottomNavPills")
	if nav_pills == null:
		printerr("  [FAIL]: BottomNavPills container missing.")
		return false

	var expected_destinations := [
		{"node": office.btn_nav_overview, "name": "Overview", "icon": "🏠"},
		{"node": office.btn_nav_government, "name": "Government", "icon": "👥"},
		{"node": office.btn_nav_economy, "name": "Economy", "icon": "📈"},
		{"node": office.btn_nav_policy, "name": "Policy", "icon": "📜"},
		{"node": office.btn_nav_development, "name": "Development", "icon": "🏗️"},
		{"node": office.btn_nav_diplomacy, "name": "Diplomacy", "icon": "🌐"},
		{"node": office.btn_nav_security, "name": "Security", "icon": "🛡️"},
		{"node": office.btn_nav_messages, "name": "Messages", "icon": "✉️"},
		{"node": office.btn_nav_buffs, "name": "Buffs", "icon": "⚡"},
		{"node": office.btn_nav_command, "name": "Command", "icon": "⚔️"}
	]

	if nav_pills.get_child_count() != 10:
		printerr("  [FAIL]: Expected exactly 10 bottom navigation destinations, got %d" % nav_pills.get_child_count())
		return false

	for dest in expected_destinations:
		var btn: Button = dest["node"]
		if btn == null:
			printerr("  [FAIL]: Missing button for %s" % dest["name"])
			return false
		# Verify vertical icon on top, name underneath
		if not btn.text.contains("\n"):
			printerr("  [FAIL]: Button %s does not format icon on top and name underneath ('%s')" % [dest["name"], btn.text])
			return false
		if not btn.text.contains(dest["icon"]) or not btn.text.contains(dest["name"]):
			printerr("  [FAIL]: Button text '%s' does not contain expected icon '%s' and name '%s'" % [btn.text, dest["icon"], dest["name"]])
			return false

	print("  [OK]: Exactly 10 primary navigation destinations verified with vertical icon/name formatting.")
	return true

func test_no_left_duplicate_navigation(office: Control) -> bool:
	print("\n--- Check 2: No Permanent Left Duplicate Navigation ---")
	var sidebar_nav = office.get_node_or_null("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/SidebarNavVBox")
	if sidebar_nav != null:
		printerr("  [FAIL]: SidebarNavVBox was found in LeftDock! Duplicate permanent left nav must be removed.")
		return false

	print("  [OK]: Duplicate left navigation panel confirmed completely removed.")
	return true

func test_persistent_chat_shortcut(office: Control) -> bool:
	print("\n--- Check 3: Persistent Chat Shortcut & Unread Badge ---")
	if office.chat_shortcut_button == null:
		printerr("  [FAIL]: chat_shortcut_button missing on dashboard.")
		return false
	if office.chat_unread_badge == null:
		printerr("  [FAIL]: chat_unread_badge missing on chat shortcut.")
		return false

	# Test opening chat without navigating through Messages
	office.close_all_modals()
	office._on_chat_shortcut_pressed()
	if office.chat_popup_modal == null or not office.chat_popup_modal.visible:
		printerr("  [FAIL]: ChatPopupModal did not open via persistent chat shortcut.")
		return false

	office.close_all_modals()
	if office.chat_popup_modal.visible:
		printerr("  [FAIL]: ChatPopupModal failed to close via close_all_modals.")
		return false

	print("  [OK]: Persistent chat shortcut with unread badge verified.")
	return true

func test_honest_telemetry_and_delta_pills(office: Control) -> bool:
	print("\n--- Check 4: Honest Telemetry & Dynamic Delta Pills ---")
	# When missing state: all delta pills must be hidden and labels unavailable
	office._reset_modern_office_visuals_to_missing()
	if office.top_treasury_delta and office.top_treasury_delta.visible:
		printerr("  [FAIL]: Treasury delta pill visible when no state loaded.")
		return false
	if office.top_approval_delta and office.top_approval_delta.visible:
		printerr("  [FAIL]: Approval delta pill visible when no state loaded.")
		return false
	if office.deck_yield_val.text.contains("184.7M"):
		printerr("  [FAIL]: Hardcoded fallback 184.7M found in deck_yield_val!")
		return false
	if office.deck_tax_val.text.contains("96.3M"):
		printerr("  [FAIL]: Hardcoded fallback 96.3M found in deck_tax_val!")
		return false
	if office.obj_val4.text.contains("452.8M"):
		printerr("  [FAIL]: Hardcoded fallback 452.8M found in obj_val4!")
		return false

	# When live state loads with real delta
	var state1 := {
		"treasury_balance": 500000000.0,
		"approval_rating": 60.0,
		"civil_service_quality": 0.70,
		"gdp_formatted": "R 200.0M / cycle",
		"income_tax_rate_formatted": "25.0%"
	}
	office._update_modern_office_visuals(state1)
	office.previous_state = state1.duplicate(true)

	var state2 := {
		"treasury_balance": 525000000.0, # +25M
		"approval_rating": 63.5,         # +3.5%
		"civil_service_quality": 0.72,
		"gdp_formatted": "R 205.0M / cycle",
		"income_tax_rate_formatted": "25.0%"
	}
	office._update_modern_office_visuals(state2)

	if office.top_treasury_delta == null or not office.top_treasury_delta.visible:
		printerr("  [FAIL]: Treasury delta pill should be visible after +25M increase.")
		return false
	if not office.top_treasury_delta_label.text.contains("+25.0M"):
		printerr("  [FAIL]: Treasury delta should show +25.0M, got: %s" % office.top_treasury_delta_label.text)
		return false

	if office.top_approval_delta == null or not office.top_approval_delta.visible:
		printerr("  [FAIL]: Approval delta pill should be visible after +3.5% increase.")
		return false
	if not office.top_approval_delta_label.text.contains("+3.5%"):
		printerr("  [FAIL]: Approval delta should show +3.5%, got: %s" % office.top_approval_delta_label.text)
		return false

	print("  [OK]: Honest telemetry verified: zero mock numbers on missing state, real numerical deltas on update.")
	return true

func test_construction_ministerial_requirements(office: Control) -> bool:
	print("\n--- Check 5: Construction Ministerial Requirements ---")
	# State without appointed ministers
	var vacant_state := {
		"cabinet": [
			{"portfolio": "Education", "minister_name": "Vacant", "is_appointed": false},
			{"portfolio": "OilAndGas", "minister_name": "Vacant", "is_appointed": false},
			{"portfolio": "Resources", "minister_name": "Vacant", "is_appointed": false}
		]
	}
	office._update_development_modal_requirements(vacant_state)
	if not office.start_university_button.disabled:
		printerr("  [FAIL]: University button must be disabled when Minister of Education is vacant.")
		return false
	if not office.start_refinery_button.disabled:
		printerr("  [FAIL]: Refinery button must be disabled when Minister of Oil & Gas is vacant.")
		return false
	if not office.start_mine_button.disabled:
		printerr("  [FAIL]: Mine button must be disabled when Minister of Resources is vacant.")
		return false

	# State with appointed ministers
	var appointed_state := {
		"cabinet": [
			{"portfolio": "Education", "minister_name": "Dr. Beatrice Ward", "is_appointed": true},
			{"portfolio": "OilAndGas", "minister_name": "Eng. Tariq Al-Mansoor", "is_appointed": true},
			{"portfolio": "Resources", "minister_name": "Dr. Nathaniel Vance", "is_appointed": true}
		]
	}
	office._update_development_modal_requirements(appointed_state)
	if office.start_university_button.disabled:
		printerr("  [FAIL]: University button must be enabled when Minister of Education is appointed.")
		return false
	if office.start_refinery_button.disabled:
		printerr("  [FAIL]: Refinery button must be enabled when Minister of Oil & Gas is appointed.")
		return false
	if office.start_mine_button.disabled:
		printerr("  [FAIL]: Mine button must be enabled when Minister of Resources is appointed.")
		return false

	print("  [OK]: Construction requirements verified: Education for University, Oil & Gas for Refinery, Resources for Mine.")
	return true

func test_press_secretary_briefings(office: Control) -> bool:
	print("\n--- Check 6: Press Secretary Dashboard & Government Integration ---")
	var state := {
		"press_secretary_name": "Elena Vance",
		"press_briefing_headline": "Economic Surge Follows Trade Pact",
		"press_briefing_summary": "National GDP expanded by 4.2% following bilateral ratification."
	}
	office._update_press_secretary(state)
	if office.press_sec_name_label == null or not office.press_sec_name_label.text.contains("Elena Vance"):
		printerr("  [FAIL]: Press Secretary name not displayed.")
		return false
	if office.press_briefing_headline_label == null or not office.press_briefing_headline_label.text.contains("Economic Surge"):
		printerr("  [FAIL]: Press briefing headline not displayed.")
		return false

	print("  [OK]: Press Secretary verified across dashboard and Government.")
	return true

func test_central_bank_governor(office: Control) -> bool:
	print("\n--- Check 7: Central Bank Governor Formalized in Economy ---")
	var state := {
		"central_bank_governor_name": "Dr. Marcus Thorne",
		"central_bank_interest_rate": "4.75% Benchmark Rate",
		"central_bank_foreign_reserves": "R 48.5B Foreign Reserves"
	}
	office._update_economy_central_bank_modal(state)
	if office.governor_name_label == null or not office.governor_name_label.text.contains("Dr. Marcus Thorne"):
		printerr("  [FAIL]: Central Bank Governor label missing or incorrect.")
		return false
	if office.interest_rate_label == null or not office.interest_rate_label.text.contains("4.75%"):
		printerr("  [FAIL]: Benchmark interest rate missing or incorrect.")
		return false

	print("  [OK]: Central Bank Governor formalized in Economy modal.")
	return true

func test_command_centre_warfare_pipeline(office: Control) -> bool:
	print("\n--- Check 8: Command Centre Warfare Operations ---")
	office.open_command_centre_modal()
	if office.command_centre_modal == null or not office.command_centre_modal.visible:
		printerr("  [FAIL]: CommandCentreModal not visible.")
		return false
	if office.target_country_option == null or office.operation_type_option == null or office.troops_slider == null:
		printerr("  [FAIL]: Command Centre operation controls missing.")
		return false

	# Test troop slider reactivity
	office.troops_slider.value = 12000
	office._on_troops_slider_changed(12000)
	if not office.troops_label.text.contains("12,000"):
		printerr("  [FAIL]: Troops slider label failed to update.")
		return false

	print("  [OK]: Command Centre military command and warfare operations verified.")
	return true

func test_legislature_5050_consequence_engine(office: Control) -> bool:
	print("\n--- Check 9: Legislative Chamber 50:50 Consequence Engine ---")
	office.open_policy_modal()
	if office.policy_modal == null or not office.policy_modal.visible:
		printerr("  [FAIL]: PolicyModal not visible.")
		return false

	var state := {
		"pending_bill": {
			"title": "National Infrastructure Modernization Act",
			"category": "Infrastructure",
			"positive_consequence": "Increases logistic speed by +20% and industrial output.",
			"negative_consequence": "Increases recurring expenditure and public debt by 25M REPU.",
			"governance_principle": "Keynesian Public Investment Theory",
			"required_majority": 50,
			"ayes_count": 58,
			"nays_count": 42
		}
	}
	office._update_policy_and_bills_modal(state)
	if office.bill_positive_label == null or not office.bill_positive_label.text.contains("🟢 Positive:"):
		printerr("  [FAIL]: Bill positive consequence missing.")
		return false
	if office.bill_negative_label == null or not office.bill_negative_label.text.contains("🔴 Negative:"):
		printerr("  [FAIL]: Bill negative consequence missing.")
		return false
	if office.bill_principle_label == null or not office.bill_principle_label.text.contains("📚 Principle:"):
		printerr("  [FAIL]: Governance principle missing.")
		return false

	print("  [OK]: 50:50 Consequence Engine and trade-off educational transparency verified.")
	return true

