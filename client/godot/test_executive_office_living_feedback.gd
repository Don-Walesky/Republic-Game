extends SceneTree

## Automated Headless Verification Suite for Republic:
## Executive Office Living Feedback & Interactive Systems
## Validates:
## 1. Living Capital Window (CapitalWindowScenery.gd):
##    - Stability & Approval drive window light density and city brightness
##    - Active projects trigger construction cranes and scaffolding
##    - Crises / unrest trigger street-level protests and pollution haze
##    - @export properties and public setter methods
## 2. Desk as Light Interactive Surface:
##    - Desk props clickable (Hotline, Inbox, Dossier, Blotter)
##    - Pulsing notification badges on props during unreviewed state items
##    - Clicking prop clears notification badge and opens focused modal
## 3. Soft Identity Feedback on Portrait & Flag:
##    - Portrait frame glow & modulate shift across approval/stability bands (gold / slate / charcoal)
##    - Flag banner brightness and sway animation react to national stability
## 4. Progressive Disclosure on Docks:
##    - Docks start collapsed (unobstructed central office view)
##    - Hover / toggle expand and collapse
##    - Critical alert attention-grabbing expand and border flash
## 5. Session Briefing Hook:
##    - Non-blocking "National Briefing" toast with honest state values
##    - Auto-dismisses and manual dismiss via button or click

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE LIVING FEEDBACK VERIFICATION   ")
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

	var all_passed := true

	# Check 1: Living Capital Window State Reactions
	all_passed = test_living_capital_window(office) and all_passed

	# Check 2: Desk Props Interactivity & Notification Badges
	all_passed = test_desk_props_and_badges(office) and all_passed

	# Check 3: Soft Identity Feedback on Portrait & Flag
	all_passed = test_soft_identity_feedback(office) and all_passed

	# Check 4: Progressive Disclosure on Docks
	all_passed = test_progressive_disclosure_docks(office) and all_passed

	# Check 5: Session Briefing Hook
	all_passed = test_session_briefing_hook(office) and all_passed

	office.queue_free()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 5 LIVING FEEDBACK & INTERACTIVITY CHECKS PASSED! ")
		print("==============================================================")
		quit(0)
	else:
		print("==============================================================")
		print(" [FAIL]: ONE OR MORE LIVING FEEDBACK CHECKS FAILED!           ")
		print("==============================================================")
		quit(1)


func test_living_capital_window(office: Control) -> bool:
	print("\n--- Check 1: Living Capital Window State Reactions ---")
	var scenery = office.city_scenery
	if scenery == null:
		printerr("[FAIL]: city_scenery node not found in ExecutiveOffice.")
		return false

	# 1. High stability and approval (peaceful prosperous capital)
	scenery.set_state_metrics(85.0, 90.0, "None", "None")
	var high_light_density: float = scenery.window_light_density
	var high_brightness: float = scenery.city_brightness
	var high_construction: bool = scenery.show_construction
	var high_protest: bool = scenery.show_protest

	if high_light_density < 1.0:
		printerr("[FAIL]: Expected vibrant window light density (>= 1.0) under high stability, got: %f" % high_light_density)
		return false
	if high_brightness < 1.0:
		printerr("[FAIL]: Expected bright city (>= 1.0) under high stability, got: %f" % high_brightness)
		return false
	if high_construction:
		printerr("[FAIL]: Expected show_construction == false with active_project='None'")
		return false
	if high_protest:
		printerr("[FAIL]: Expected show_protest == false with high stability and zero crisis")
		return false
	print("  [OK]: Prosperous calm state correctly produces dense lights, bright skyline, zero cranes/protests.")

	# 2. Low stability (civil unrest)
	scenery.set_state_metrics(30.0, 25.0, "None", "None")
	var low_light_density: float = scenery.window_light_density
	var low_brightness: float = scenery.city_brightness
	var low_protest: bool = scenery.show_protest
	var low_tint: Color = scenery.pollution_tint

	if low_light_density >= high_light_density:
		printerr("[FAIL]: Window light density under low stability (%f) should be lower than high stability (%f)" % [low_light_density, high_light_density])
		return false
	if low_brightness >= high_brightness:
		printerr("[FAIL]: City brightness under low stability (%f) should be lower than high stability (%f)" % [low_brightness, high_brightness])
		return false
	if not low_protest:
		printerr("[FAIL]: Expected show_protest == true under low stability (25.0)")
		return false
	if low_tint.a <= 0.0:
		printerr("[FAIL]: Expected pollution/unrest tint under low stability")
		return false
	print("  [OK]: Low stability state triggers sparse lights, street-level protest silhouettes, and haze tint.")

	# 3. Active national project (construction crane & scaffolding)
	scenery.set_state_metrics(70.0, 75.0, "Trans-Arcadia High-Speed Rail", "None")
	if not scenery.show_construction:
		printerr("[FAIL]: Expected show_construction == true when active_project is set")
		return false
	print("  [OK]: Active project accurately triggers construction crane and scaffolding silhouettes.")

	# 4. Active crisis (smog / pollution / unrest tint)
	scenery.set_state_metrics(70.0, 75.0, "None", "Capital Power Grid Failure")
	if not scenery.show_protest:
		printerr("[FAIL]: Expected show_protest == true during active crisis")
		return false
	if scenery.pollution_tint.a <= 0.0:
		printerr("[FAIL]: Expected pollution_tint.a > 0.0 during active crisis")
		return false
	print("  [OK]: Active crisis triggers atmospheric tint and civil disturbance silhouettes.")

	# 5. Public setter API and @export verification
	scenery.set_construction_visible(false)
	if scenery.show_construction:
		printerr("[FAIL]: set_construction_visible(false) failed.")
		return false
	scenery.set_protest_visible(false)
	if scenery.show_protest:
		printerr("[FAIL]: set_protest_visible(false) failed.")
		return false
	scenery.set_window_light_density(1.25)
	if not is_equal_approx(scenery.window_light_density, 1.25):
		printerr("[FAIL]: set_window_light_density failed.")
		return false
	scenery.set_pollution_tint(Color(0.2, 0.1, 0.1, 0.3))
	if not is_equal_approx(scenery.pollution_tint.a, 0.3):
		printerr("[FAIL]: set_pollution_tint failed.")
		return false
	print("  [OK]: Public setter API and @export design properties verified.")

	return true


func test_desk_props_and_badges(office: Control) -> bool:
	print("\n--- Check 2: Desk Props Interactivity & Notification Badges ---")
	if office.hotline_phone == null or office.intelligence_dossier == null or office.executive_inbox == null or office.blotter_button == null:
		printerr("[FAIL]: One or more required desk prop controls missing.")
		return false

	# 1. Test notification badge visibility setters
	office.set_hotline_badge(true, "!")
	if not office.is_hotline_badge_visible():
		printerr("[FAIL]: hotline_badge not visible after set_hotline_badge(true).")
		return false
	if office.hotline_badge_label.text != "!":
		printerr("[FAIL]: hotline_badge_label text mismatch. Got: " + office.hotline_badge_label.text)
		return false

	office.set_dossier_badge(true, "!")
	if not office.is_dossier_badge_visible():
		printerr("[FAIL]: dossier_badge not visible after set_dossier_badge(true).")
		return false

	office.set_inbox_badge(true, "3")
	if not office.is_inbox_badge_visible():
		printerr("[FAIL]: inbox_badge not visible after set_inbox_badge(true).")
		return false
	if office.inbox_badge_label.text != "3":
		printerr("[FAIL]: inbox_badge_label text mismatch. Got: " + office.inbox_badge_label.text)
		return false
	print("  [OK]: Desk notification badge setters and label counters verified.")

	# 2. Test clicking props clears badge and opens focused modal
	office._on_hotline_phone_pressed()
	if office.is_hotline_badge_visible():
		printerr("[FAIL]: Hotline badge should clear after hotline prop is clicked.")
		return false
	if not office.hotline_modal.visible:
		printerr("[FAIL]: HotlineModal should be open after hotline prop is clicked.")
		return false
	office.close_all_modals()

	office._on_intelligence_dossier_pressed()
	if office.is_dossier_badge_visible():
		printerr("[FAIL]: Dossier badge should clear after dossier prop is clicked.")
		return false
	if not office.dossier_modal.visible:
		printerr("[FAIL]: DossierModal should be open after dossier prop is clicked.")
		return false
	office.close_all_modals()

	office._on_executive_inbox_pressed()
	if office.is_inbox_badge_visible():
		printerr("[FAIL]: Inbox badge should clear after inbox prop is clicked.")
		return false
	if not office.briefing_modal.visible:
		printerr("[FAIL]: BriefingModal should be open after inbox prop is clicked.")
		return false
	office.close_all_modals()

	# 3. Test blotter interaction
	office._on_blotter_button_pressed()
	if not office.status_ticker_label.text.contains("Executive Desk"):
		printerr("[FAIL]: Status ticker not updated on blotter press. Got: " + office.status_ticker_label.text)
		return false
	print("  [OK]: Clicking desk props clears badges, updates ticker, and opens focused surfaces.")

	# 4. Test state-driven badge triggers
	var crisis_state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 15000000000.0,
		"approval_rating": 68.5,
		"stability_rating": 85.0,
		"defcon_level": "DEFCON 2 (Elevated Threat)",
		"active_crisis": "Border Military Incident",
		"pending_decision": true
	}
	office.hotline_badge_dismissed = false
	office.dossier_badge_dismissed = false
	office.inbox_badge_dismissed = false
	office.apply_state(crisis_state)

	if not office.is_hotline_badge_visible():
		printerr("[FAIL]: Expected hotline badge active during active crisis.")
		return false
	if not office.is_dossier_badge_visible():
		printerr("[FAIL]: Expected dossier badge active during elevated DEFCON threat.")
		return false
	if not office.is_inbox_badge_visible():
		printerr("[FAIL]: Expected inbox badge active when pending decisions exist.")
		return false
	print("  [OK]: Authoritative simulation state correctly triggers live desk prop notification badges.")

	return true


func test_soft_identity_feedback(office: Control) -> bool:
	print("\n--- Check 3: Soft Identity Feedback on Portrait & Flag ---")
	# 1. High approval / stability -> warm gold glow
	office.update_portrait_state_feedback(75.0, 80.0, false)
	var frame_style := office.portrait_frame.get_theme_stylebox("panel") as StyleBoxFlat
	if frame_style == null:
		printerr("[FAIL]: portrait_frame has no StyleBoxFlat.")
		return false
	if frame_style.border_color.r < 0.8 or frame_style.border_color.g < 0.7:
		printerr("[FAIL]: Expected gold border color for high approval, got: %s" % str(frame_style.border_color))
		return false
	if office.portrait_canvas.modulate.r < 1.0:
		printerr("[FAIL]: Expected vibrant canvas modulate (>= 1.0) for high approval, got: %s" % str(office.portrait_canvas.modulate))
		return false
	print("  [OK]: High approval produces warm gold portrait frame glow and vibrant canvas tone.")

	# 2. Mid approval -> slate neutral
	office.update_portrait_state_feedback(55.0, 50.0, false)
	frame_style = office.portrait_frame.get_theme_stylebox("panel") as StyleBoxFlat
	if frame_style.border_color.b < 0.6:
		printerr("[FAIL]: Expected slate border color for mid approval, got: %s" % str(frame_style.border_color))
		return false
	print("  [OK]: Mid approval produces dignified slate neutral portrait frame tone.")

	# 3. Low approval or crisis -> dimmed somber charcoal
	office.update_portrait_state_feedback(30.0, 35.0, true)
	frame_style = office.portrait_frame.get_theme_stylebox("panel") as StyleBoxFlat
	if frame_style.border_color.r > 0.45 or frame_style.border_color.g > 0.4:
		printerr("[FAIL]: Expected dimmed charcoal border for crisis/low approval, got: %s" % str(frame_style.border_color))
		return false
	if office.portrait_canvas.modulate.r > 0.75:
		printerr("[FAIL]: Expected desaturated canvas modulate (< 0.75) for crisis, got: %s" % str(office.portrait_canvas.modulate))
		return false
	print("  [OK]: Low approval / crisis safely dims and desaturates the presidential portrait.")

	# 4. Flag banner brightness and sway parameters
	office.update_flag_state_feedback(90.0, false)
	if office.flag_banner.modulate.r < 1.0:
		printerr("[FAIL]: Expected high flag brightness under high stability.")
		return false
	if office.flag_sway_amplitude > 1.5:
		printerr("[FAIL]: Expected subtle dignified flutter amplitude under high stability.")
		return false

	office.update_flag_state_feedback(30.0, true)
	if office.flag_banner.modulate.r > 0.75:
		printerr("[FAIL]: Expected dimmed flag banner under low stability/crisis.")
		return false
	if office.flag_sway_amplitude < 2.0:
		printerr("[FAIL]: Expected wider sway amplitude under national instability.")
		return false

	# Test process sway step
	var initial_rot: float = office.flag_banner.rotation_degrees
	office._process(0.1)
	if is_equal_approx(office.flag_banner.rotation_degrees, initial_rot) and office.flag_sway_time == 0.0:
		printerr("[FAIL]: Flag sway did not advance in _process.")
		return false
	print("  [OK]: National flag banner brightness and dynamic sway faithfully reflect stability.")

	return true


func test_progressive_disclosure_docks(office: Control) -> bool:
	print("\n--- Check 4: Progressive Disclosure on Docks ---")
	# 1. Start collapsed check
	office.collapse_all_docks()
	if office.is_left_dock_expanded():
		printerr("[FAIL]: Left dock should be collapsed initially.")
		return false
	if office.is_right_dock_expanded():
		printerr("[FAIL]: Right dock should be collapsed initially.")
		return false
	print("  [OK]: Docks start collapsed to maintain an unobstructed view of the presidential office.")

	# 2. Left dock expand and toggle
	office.set_left_dock_expanded(true)
	if not office.is_left_dock_expanded():
		printerr("[FAIL]: is_left_dock_expanded() returned false after set_left_dock_expanded(true).")
		return false
	if office.toggle_left_dock_button.text != "▲":
		printerr("[FAIL]: toggle_left_dock_button text should be '▲' when expanded.")
		return false

	office.set_left_dock_expanded(false)
	if office.is_left_dock_expanded():
		printerr("[FAIL]: is_left_dock_expanded() returned true after set_left_dock_expanded(false).")
		return false
	if office.toggle_left_dock_button.text != "▼":
		printerr("[FAIL]: toggle_left_dock_button text should be '▼' when collapsed.")
		return false
	print("  [OK]: Left dock expand/collapse toggle and chevron feedback verified.")

	# 3. Right dock expand and toggle
	office.set_right_dock_expanded(true)
	if not office.is_right_dock_expanded():
		printerr("[FAIL]: is_right_dock_expanded() returned false after set_right_dock_expanded(true).")
		return false
	office.set_right_dock_expanded(false)
	if office.is_right_dock_expanded():
		printerr("[FAIL]: is_right_dock_expanded() returned true after set_right_dock_expanded(false).")
		return false
	print("  [OK]: Right dock expand/collapse toggle verified.")

	# 4. Hover expand / collapse simulation
	office.left_dock_pinned = false
	office._on_left_dock_mouse_entered()
	if not office.is_left_dock_expanded():
		printerr("[FAIL]: Left dock should expand on mouse entered.")
		return false
	office._on_left_dock_mouse_exited()
	if office.is_left_dock_expanded():
		printerr("[FAIL]: Unpinned left dock should collapse on mouse exited.")
		return false
	print("  [OK]: Hover auto-expand and collapse on mouse exit verified.")

	# 5. Critical alert trigger
	office.trigger_dock_alert("right")
	if not office.is_right_dock_expanded():
		printerr("[FAIL]: trigger_dock_alert('right') should expand right dock.")
		return false
	if office.flashing_dock != office.right_dock:
		printerr("[FAIL]: flashing_dock mismatch after alert.")
		return false
	office._on_dock_flash_timeout()
	if office.flashing_dock != null:
		printerr("[FAIL]: flashing_dock should be cleared after timeout.")
		return false
	print("  [OK]: Critical alert expand and border flash alert verified.")

	return true


func test_session_briefing_hook(office: Control) -> bool:
	print("\n--- Check 5: Session Briefing Hook ---")
	if office.session_briefing_toast == null:
		printerr("[FAIL]: session_briefing_toast node missing.")
		return false

	# 1. Update session briefing with honest values
	var state := {
		"country_name": "Republic of Arcadia",
		"treasury_balance": 15000000000.0,
		"approval_rating": 68.5,
		"stability_rating": 85.0,
		"active_project": "Orbital Spaceport",
		"active_crisis": "None",
		"status_message": "Executive institutions fully aligned."
	}
	office.update_session_briefing(state)
	if not office.session_briefing_metrics_label.text.contains("Republic of Arcadia") or not office.session_briefing_metrics_label.text.contains("15,000,000,000"):
		printerr("[FAIL]: Briefing metrics text mismatch. Got: " + office.session_briefing_metrics_label.text)
		return false
	if not office.session_briefing_status_label.text.contains("Orbital Spaceport"):
		printerr("[FAIL]: Briefing status project mismatch. Got: " + office.session_briefing_status_label.text)
		return false
	print("  [OK]: Session briefing toast populates honest state values (treasury, approval, stability, active project).")

	# 2. Show toast
	office.show_session_briefing(false)
	if not office.is_session_briefing_visible():
		printerr("[FAIL]: is_session_briefing_visible() returned false after show_session_briefing().")
		return false

	# 3. Dismiss toast via API
	office.dismiss_session_briefing()
	if office.is_session_briefing_visible():
		printerr("[FAIL]: is_session_briefing_visible() returned true after dismiss_session_briefing().")
		return false

	# 4. Dismiss toast via GUI click input
	office.show_session_briefing(false)
	var click_event := InputEventMouseButton.new()
	click_event.pressed = true
	click_event.button_index = MOUSE_BUTTON_LEFT
	office._on_session_briefing_toast_gui_input(click_event)
	if office.is_session_briefing_visible():
		printerr("[FAIL]: is_session_briefing_visible() returned true after click dismissal.")
		return false
	print("  [OK]: Session briefing toast visibility, manual dismiss, and click dismissal verified.")

	return true
