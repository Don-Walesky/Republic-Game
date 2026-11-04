extends SceneTree

## Automated Headless Verification Suite for Republic:
## Comprehensive Dashboard & User Interface Interactivity Audit
##
## Validates:
## 1. TopBar Time Speed Controls (Pause, Play 1x, Fast 2x) & Visual Active State Feedback.
## 2. TopBar HUD Telemetry Cards (Treasury, Approval, Stability, Economy/Defcon, President Mandate).
## 3. Floating Telemetry Deck Cards (Yield, Tax, Capacity, Welfare, Decisions).
## 4. RightDock Events "View All →" and Objectives Card Navigation.
## 5. BottomDock "Projects" Button Navigation to Infrastructure Development.
## 6. All 9 Executive Cabinet Portfolios have active "Appoint / Reassign" buttons and candidate commissioning.
## 7. Bottom Navigation 10 Destinations routing.

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - DASHBOARD INTERACTIVITY VERIFICATION SUITE      ")
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
	office._apply_tooltips()
	office._update_speed_controls_ui()

	var t1 := test_time_speed_controls(office)
	print("Test 1 (TopBar Speed Controls - Pause/Play/Fast): ", "PASS" if t1 else "FAIL")

	var t2 := test_topbar_metric_cards_interactivity(office)
	print("Test 2 (TopBar Metric Cards Direct Inspection): ", "PASS" if t2 else "FAIL")

	var t3 := test_floating_telemetry_deck_interactivity(office)
	print("Test 3 (Floating Telemetry Deck Direct Inspection): ", "PASS" if t3 else "FAIL")

	var t4 := test_right_dock_events_and_objectives(office)
	print("Test 4 (RightDock Events View-All & Objectives): ", "PASS" if t4 else "FAIL")

	var t5 := test_bottom_dock_projects_button(office)
	print("Test 5 (BottomDock Projects Button Navigation): ", "PASS" if t5 else "FAIL")

	var t6 := test_all_nine_cabinet_portfolio_appointments(office)
	print("Test 6 (All 9 Cabinet Portfolio Commissioning): ", "PASS" if t6 else "FAIL")

	var t7 := test_bottom_navigation_destinations(office)
	print("Test 7 (10-Section Navigation Bar Switching): ", "PASS" if t7 else "FAIL")

	office.queue_free()

	if t1 and t2 and t3 and t4 and t5 and t6 and t7:
		print("==============================================================")
		print("   ALL DASHBOARD INTERACTIVITY TESTS PASSED (7/7)            ")
		print("==============================================================")
		quit(0)
	else:
		printerr("[FAIL]: One or more dashboard interactivity tests failed.")
		quit(1)

func _simulate_card_click(card: Control) -> void:
	if card == null: return
	var ev := InputEventMouseButton.new()
	ev.button_index = MOUSE_BUTTON_LEFT
	ev.pressed = true
	ev.position = Vector2(5, 5)
	card.gui_input.emit(ev)

func test_time_speed_controls(office) -> bool:
	# Verify speed pause button
	if office.speed_pause_btn == null or office.speed_play_btn == null or office.speed_fast_btn == null:
		printerr("[FAIL]: Speed buttons missing on TopBar")
		return false

	# 1. Test Pause
	office.speed_pause_btn.emit_signal("pressed")
	if office.simulation_speed != 0:
		printerr("[FAIL]: Simulation speed not 0 after pause. Got: ", office.simulation_speed)
		return false
	if office.poll_timer and not office.poll_timer.paused:
		printerr("[FAIL]: Poll timer not paused when speed is 0")
		return false
	if not office.status_ticker_label.text.contains("PAUSED"):
		printerr("[FAIL]: Status ticker did not announce PAUSED")
		return false

	# 2. Test Play (1x)
	office.speed_play_btn.emit_signal("pressed")
	if office.simulation_speed != 1:
		printerr("[FAIL]: Simulation speed not 1 after play. Got: ", office.simulation_speed)
		return false
	if office.poll_timer and office.poll_timer.paused:
		printerr("[FAIL]: Poll timer remained paused when speed set to 1")
		return false

	# 3. Test Fast (2x)
	office.speed_fast_btn.emit_signal("pressed")
	if office.simulation_speed != 2:
		printerr("[FAIL]: Simulation speed not 2 after fast. Got: ", office.simulation_speed)
		return false
	if not office.status_ticker_label.text.contains("2.0x"):
		printerr("[FAIL]: Status ticker did not announce 2.0x")
		return false

	# Return to 1x
	office.speed_play_btn.emit_signal("pressed")
	return true

func test_topbar_metric_cards_interactivity(office) -> bool:
	office.close_all_modals()

	# 1. CardTreasury -> EconomyModal
	if office.card_treasury == null:
		printerr("[FAIL]: card_treasury node not found")
		return false
	_simulate_card_click(office.card_treasury)
	if office.economy_modal == null or not office.economy_modal.visible:
		printerr("[FAIL]: Clicking CardTreasury did not open EconomyModal")
		return false
	office.close_all_modals()

	# 2. CardApproval -> BriefingModal
	if office.card_approval == null:
		printerr("[FAIL]: card_approval node not found")
		return false
	_simulate_card_click(office.card_approval)
	if office.briefing_modal == null or not office.briefing_modal.visible:
		printerr("[FAIL]: Clicking CardApproval did not open BriefingModal")
		return false
	office.close_all_modals()

	# 3. CardStability -> BriefingModal
	if office.card_stability == null:
		printerr("[FAIL]: card_stability node not found")
		return false
	_simulate_card_click(office.card_stability)
	if office.briefing_modal == null or not office.briefing_modal.visible:
		printerr("[FAIL]: Clicking CardStability did not open BriefingModal")
		return false
	office.close_all_modals()

	# 4. CardEconomy -> CommandCentreModal
	if office.card_economy == null:
		printerr("[FAIL]: card_economy node not found")
		return false
	_simulate_card_click(office.card_economy)
	if office.command_centre_modal == null or not office.command_centre_modal.visible:
		printerr("[FAIL]: Clicking CardEconomy did not open CommandCentreModal")
		return false
	office.close_all_modals()

	# 5. PresidentCard -> CustomisationModal
	if office.president_card == null:
		printerr("[FAIL]: president_card node not found")
		return false
	_simulate_card_click(office.president_card)
	if office.customisation_modal == null or not office.customisation_modal.visible:
		printerr("[FAIL]: Clicking PresidentCard did not open CustomisationModal")
		return false
	office.close_all_modals()

	return true

func test_floating_telemetry_deck_interactivity(office) -> bool:
	office.close_all_modals()

	# 1. DeckYieldCard -> EconomyModal
	if office.deck_yield_card == null:
		printerr("[FAIL]: deck_yield_card not found")
		return false
	_simulate_card_click(office.deck_yield_card)
	if office.economy_modal == null or not office.economy_modal.visible:
		printerr("[FAIL]: Clicking DeckYieldCard did not open EconomyModal")
		return false
	office.close_all_modals()

	# 2. DeckTaxCard -> EconomyModal
	if office.deck_tax_card == null:
		printerr("[FAIL]: deck_tax_card not found")
		return false
	_simulate_card_click(office.deck_tax_card)
	if office.economy_modal == null or not office.economy_modal.visible:
		printerr("[FAIL]: Clicking DeckTaxCard did not open EconomyModal")
		return false
	office.close_all_modals()

	# 3. DeckCapacityCard -> DevelopmentModal
	if office.deck_capacity_card == null:
		printerr("[FAIL]: deck_capacity_card not found")
		return false
	_simulate_card_click(office.deck_capacity_card)
	if office.development_modal == null or not office.development_modal.visible:
		printerr("[FAIL]: Clicking DeckCapacityCard did not open DevelopmentModal")
		return false
	office.close_all_modals()

	# 4. DeckWelfareCard -> BriefingModal
	if office.deck_welfare_card == null:
		printerr("[FAIL]: deck_welfare_card not found")
		return false
	_simulate_card_click(office.deck_welfare_card)
	if office.briefing_modal == null or not office.briefing_modal.visible:
		printerr("[FAIL]: Clicking DeckWelfareCard did not open BriefingModal")
		return false
	office.close_all_modals()

	# 5. DeckDecisionsCard -> PolicyModal
	if office.deck_decisions_card == null:
		printerr("[FAIL]: deck_decisions_card not found")
		return false
	_simulate_card_click(office.deck_decisions_card)
	if office.policy_modal == null or not office.policy_modal.visible:
		printerr("[FAIL]: Clicking DeckDecisionsCard did not open PolicyModal")
		return false
	office.close_all_modals()

	return true

func test_right_dock_events_and_objectives(office) -> bool:
	office.close_all_modals()

	# 1. EventsViewAll -> BriefingModal
	if office.events_view_all == null:
		printerr("[FAIL]: events_view_all node not found")
		return false
	office.events_view_all.emit_signal("pressed")
	if office.briefing_modal == null or not office.briefing_modal.visible:
		printerr("[FAIL]: Pressing EventsViewAll did not open BriefingModal")
		return false
	office.close_all_modals()

	# 2. CardObjectives -> DevelopmentModal
	if office.card_objectives == null:
		printerr("[FAIL]: card_objectives node not found")
		return false
	_simulate_card_click(office.card_objectives)
	if office.development_modal == null or not office.development_modal.visible:
		printerr("[FAIL]: Clicking CardObjectives did not open DevelopmentModal")
		return false
	office.close_all_modals()

	return true

func test_bottom_dock_projects_button(office) -> bool:
	office.close_all_modals()

	if office.open_projects_button == null:
		printerr("[FAIL]: open_projects_button node not found")
		return false
	office.open_projects_button.emit_signal("pressed")
	if office.development_modal == null or not office.development_modal.visible:
		printerr("[FAIL]: Pressing OpenProjectsButton did not open DevelopmentModal")
		return false
	office.close_all_modals()
	return true

func test_all_nine_cabinet_portfolio_appointments(office) -> bool:
	office.close_all_modals()

	var portfolios := [
		{"name": "Finance", "btn": office.appoint_finance_button},
		{"name": "Defense", "btn": office.appoint_defense_button},
		{"name": "ForeignAffairs", "btn": office.appoint_foreign_affairs_button},
		{"name": "Interior", "btn": office.appoint_interior_button},
		{"name": "Health", "btn": office.appoint_health_button},
		{"name": "Infrastructure", "btn": office.appoint_infrastructure_button},
		{"name": "Education", "btn": office.appoint_education_button},
		{"name": "OilAndGas", "btn": office.appoint_oil_and_gas_button},
		{"name": "Resources", "btn": office.appoint_resources_button}
	]

	for p in portfolios:
		var pname: String = p["name"]
		var btn: Button = p["btn"]
		if btn == null:
			printerr("[FAIL]: Appoint button for portfolio '", pname, "' is null")
			return false

		office.close_all_modals()
		btn.emit_signal("pressed")

		if office.cabinet_appointment_modal == null or not office.cabinet_appointment_modal.visible:
			printerr("[FAIL]: Appoint button for '", pname, "' did not open CabinetAppointmentModal")
			return false

		if office.current_appointment_portfolio != pname:
			printerr("[FAIL]: Current appointment portfolio mismatch. Expected: ", pname, ", Got: ", office.current_appointment_portfolio)
			return false

		if not office.CANDIDATE_DEFINITIONS.has(pname):
			printerr("[FAIL]: CANDIDATE_DEFINITIONS missing portfolio '", pname, "'")
			return false

		# Verify candidate definitions have technocrat, loyalist, dismiss
		var pdefs: Dictionary = office.CANDIDATE_DEFINITIONS[pname]
		if not pdefs.has("technocrat") or not pdefs.has("loyalist") or not pdefs.has("dismiss"):
			printerr("[FAIL]: Portfolio '", pname, "' is missing candidate types in CANDIDATE_DEFINITIONS")
			return false

	office.close_all_modals()
	return true

func test_bottom_navigation_destinations(office) -> bool:
	office.close_all_modals()

	# Test each navigation pill button
	var nav_tests := [
		{"btn": office.btn_nav_overview, "sec": office.Section.OVERVIEW},
		{"btn": office.btn_nav_government, "sec": office.Section.GOVERNMENT},
		{"btn": office.btn_nav_economy, "sec": office.Section.ECONOMY},
		{"btn": office.btn_nav_policy, "sec": office.Section.POLICY},
		{"btn": office.btn_nav_development, "sec": office.Section.DEVELOPMENT},
		{"btn": office.btn_nav_diplomacy, "sec": office.Section.DIPLOMACY},
		{"btn": office.btn_nav_security, "sec": office.Section.SECURITY},
		{"btn": office.btn_nav_messages, "sec": office.Section.MESSAGES},
		{"btn": office.btn_nav_buffs, "sec": office.Section.BUFFS},
		{"btn": office.btn_nav_command, "sec": office.Section.COMMAND_CENTRE}
	]

	for nt in nav_tests:
		var btn: Button = nt["btn"]
		var expected_sec = nt["sec"]
		if btn == null:
			printerr("[FAIL]: Nav button missing for section: ", expected_sec)
			return false

		btn.emit_signal("pressed")
		if office.active_nav_section != expected_sec:
			printerr("[FAIL]: active_nav_section mismatch for ", expected_sec, ". Got: ", office.active_nav_section)
			return false

	office.close_all_modals()
	return true

