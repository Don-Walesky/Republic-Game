extends Control

## ExecutiveOffice.gd
## Controller for the rich, interactive Executive Presidential Office main screen in Republic.
## Provides decoupled visual office environment layers (Window, Flag, Portrait, Desk)
## and four-edge interface HUD docking spaces connected directly to authoritative Republic.Core state.

signal state_loaded(state: Dictionary)
signal state_cleared()

# ==============================================================================
# 1. OFFICE ENVIRONMENT REFERENCES (DECOUPLED VISUAL LAYERS)
# ==============================================================================
@onready var office_environment: Control = _get_node_safe("OfficeEnvironment") as Control
@onready var room_backdrop: Control = _get_node_safe("OfficeEnvironment/RoomBackdrop") as Control
@onready var capital_window: Control = _get_node_safe("OfficeEnvironment/CapitalWindow") as Control
@onready var city_scenery: Control = _get_node_safe("OfficeEnvironment/CapitalWindow/CityScenery") as Control
@onready var window_frame: Control = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowFrame") as Control

@onready var country_flag: Control = _get_node_safe("OfficeEnvironment/CountryFlag") as Control
@onready var flag_pole: Control = _get_node_safe("OfficeEnvironment/CountryFlag/FlagPole") as Control
@onready var flag_banner: Control = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner") as Control
@onready var flag_emblem_label: Label = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner/FlagEmblemLabel") as Label

@onready var presidential_portrait: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait") as Control
@onready var portrait_frame: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame") as Control
@onready var portrait_canvas: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas") as Control
@onready var portrait_name_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitNameLabel") as Label
@onready var portrait_party_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitPartyLabel") as Label

@onready var executive_desk: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk") as Control
@onready var desk_surface: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskSurface") as Control
@onready var desk_blotter: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter") as Control
@onready var hotline_phone: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton") as Button
@onready var executive_inbox: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton") as Button
@onready var presidential_seal: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/PresidentialSeal") as Control

# ==============================================================================
# 2. INTERFACE HUD DOCK REFERENCES (TOP, LEFT, RIGHT, BOTTOM)
# ==============================================================================
@onready var interface_layer: Control = _get_node_safe("InterfaceLayer") as Control

# Top HUD Bar
@onready var top_bar: PanelContainer = _get_node_safe("InterfaceLayer/TopBar") as PanelContainer
@onready var country_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/CountryLabel") as Label
@onready var simulation_time_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/SimulationTimeLabel") as Label
@onready var treasury_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/TreasuryLabel") as Label
@onready var approval_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/ApprovalLabel") as Label
@onready var stability_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/StabilityLabel") as Label
@onready var defcon_label: Label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/DefconLabel") as Label
@onready var switch_to_desk_button: Button = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/SwitchToDeskButton") as Button
@onready var refresh_button: Button = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/RefreshButton") as Button

# Left Dock: Domestic Governance & Cabinet
@onready var left_dock: PanelContainer = _get_node_safe("InterfaceLayer/LeftDock") as PanelContainer
@onready var toggle_left_dock_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockHeader/ToggleLeftDockButton") as Button
@onready var cabinet_summary_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetSummaryLabel") as Label
@onready var civil_service_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CivilServiceLabel") as Label
@onready var corruption_control_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CorruptionControlLabel") as Label
@onready var ruling_party_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/RulingPartyLabel") as Label
@onready var polling_support_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/PollingSupportLabel") as Label

# Right Dock: Foreign Affairs & Intelligence
@onready var right_dock: PanelContainer = _get_node_safe("InterfaceLayer/RightDock") as PanelContainer
@onready var toggle_right_dock_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockHeader/ToggleRightDockButton") as Button
@onready var military_readiness_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/MilitaryReadinessLabel") as Label
@onready var active_crisis_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/ActiveCrisisLabel") as Label
@onready var gdp_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/GdpLabel") as Label
@onready var offline_briefing_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OfflineBriefingLabel") as Label

# Bottom Dock: Situation Ticker & Infrastructure Projects
@onready var bottom_dock: PanelContainer = _get_node_safe("InterfaceLayer/BottomDock") as PanelContainer
@onready var status_ticker_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StatusTickerLabel") as Label
@onready var active_project_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/ActiveProjectLabel") as Label
@onready var state_status_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StateStatusLabel") as Label

# ==============================================================================
# 3. STATE SYNCHRONIZATION VARIABLES
# ==============================================================================
const DEFAULT_STATE_FILE := "res://presidential_desk_state.json"
const USER_STATE_FILE := "user://presidential_desk_state.json"

var current_state_path: String = DEFAULT_STATE_FILE
var latest_state: Dictionary = {}
var last_modified_time: int = 0
var poll_timer: Timer

func _get_node_safe(path: String) -> Node:
	var n := get_node_or_null(path)
	if n == null:
		var node_name := path.get_file()
		n = find_child(node_name, true, false)
	return n

func _init_controls() -> void:
	if office_environment == null:
		office_environment = _get_node_safe("OfficeEnvironment") as Control
	if room_backdrop == null:
		room_backdrop = _get_node_safe("OfficeEnvironment/RoomBackdrop") as Control
	if capital_window == null:
		capital_window = _get_node_safe("OfficeEnvironment/CapitalWindow") as Control
	if city_scenery == null:
		city_scenery = _get_node_safe("OfficeEnvironment/CapitalWindow/CityScenery") as Control
	if window_frame == null:
		window_frame = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowFrame") as Control

	if country_flag == null:
		country_flag = _get_node_safe("OfficeEnvironment/CountryFlag") as Control
	if flag_pole == null:
		flag_pole = _get_node_safe("OfficeEnvironment/CountryFlag/FlagPole") as Control
	if flag_banner == null:
		flag_banner = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner") as Control
	if flag_emblem_label == null:
		flag_emblem_label = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner/FlagEmblemLabel") as Label

	if presidential_portrait == null:
		presidential_portrait = _get_node_safe("OfficeEnvironment/PresidentialPortrait") as Control
	if portrait_frame == null:
		portrait_frame = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame") as Control
	if portrait_canvas == null:
		portrait_canvas = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas") as Control
	if portrait_name_label == null:
		portrait_name_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitNameLabel") as Label
	if portrait_party_label == null:
		portrait_party_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitPartyLabel") as Label

	if executive_desk == null:
		executive_desk = _get_node_safe("OfficeEnvironment/ExecutiveDesk") as Control
	if desk_surface == null:
		desk_surface = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskSurface") as Control
	if desk_blotter == null:
		desk_blotter = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter") as Control
	if hotline_phone == null:
		hotline_phone = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton") as Button
	if executive_inbox == null:
		executive_inbox = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton") as Button
	if presidential_seal == null:
		presidential_seal = _get_node_safe("OfficeEnvironment/ExecutiveDesk/PresidentialSeal") as Control

	if interface_layer == null:
		interface_layer = _get_node_safe("InterfaceLayer") as Control

	if top_bar == null:
		top_bar = _get_node_safe("InterfaceLayer/TopBar") as PanelContainer
	if country_label == null:
		country_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/CountryLabel") as Label
	if simulation_time_label == null:
		simulation_time_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/SimulationTimeLabel") as Label
	if treasury_label == null:
		treasury_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/TreasuryLabel") as Label
	if approval_label == null:
		approval_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/ApprovalLabel") as Label
	if stability_label == null:
		stability_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/StabilityLabel") as Label
	if defcon_label == null:
		defcon_label = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/DefconLabel") as Label
	if switch_to_desk_button == null:
		switch_to_desk_button = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/SwitchToDeskButton") as Button
	if refresh_button == null:
		refresh_button = _get_node_safe("InterfaceLayer/TopBar/TopBarHBox/RefreshButton") as Button

	if left_dock == null:
		left_dock = _get_node_safe("InterfaceLayer/LeftDock") as PanelContainer
	if toggle_left_dock_button == null:
		toggle_left_dock_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockHeader/ToggleLeftDockButton") as Button
	if cabinet_summary_label == null:
		cabinet_summary_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetSummaryLabel") as Label
	if civil_service_label == null:
		civil_service_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CivilServiceLabel") as Label
	if corruption_control_label == null:
		corruption_control_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CorruptionControlLabel") as Label
	if ruling_party_label == null:
		ruling_party_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/RulingPartyLabel") as Label
	if polling_support_label == null:
		polling_support_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/PollingSupportLabel") as Label

	if right_dock == null:
		right_dock = _get_node_safe("InterfaceLayer/RightDock") as PanelContainer
	if toggle_right_dock_button == null:
		toggle_right_dock_button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockHeader/ToggleRightDockButton") as Button
	if military_readiness_label == null:
		military_readiness_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/MilitaryReadinessLabel") as Label
	if active_crisis_label == null:
		active_crisis_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/ActiveCrisisLabel") as Label
	if gdp_label == null:
		gdp_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/GdpLabel") as Label
	if offline_briefing_label == null:
		offline_briefing_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OfflineBriefingLabel") as Label

	if bottom_dock == null:
		bottom_dock = _get_node_safe("InterfaceLayer/BottomDock") as PanelContainer
	if status_ticker_label == null:
		status_ticker_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StatusTickerLabel") as Label
	if active_project_label == null:
		active_project_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/ActiveProjectLabel") as Label
	if state_status_label == null:
		state_status_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StateStatusLabel") as Label

func _ready() -> void:
	_init_controls()
	_init_state_path()
	_connect_signals()
	_setup_poll_timer()
	load_live_state()

func _init_state_path() -> void:
	# Support custom state override via environment variable or commandline args
	var env_path := OS.get_environment("REPUBLIC_STATE_FILE")
	if not env_path.is_empty():
		current_state_path = env_path
		return

	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--state-file="):
			current_state_path = arg.substr("--state-file=".length())
			return

	if FileAccess.file_exists(DEFAULT_STATE_FILE):
		current_state_path = DEFAULT_STATE_FILE
	elif FileAccess.file_exists(USER_STATE_FILE):
		current_state_path = USER_STATE_FILE
	else:
		current_state_path = DEFAULT_STATE_FILE

func _setup_poll_timer() -> void:
	poll_timer = Timer.new()
	poll_timer.name = "StatePollTimer"
	poll_timer.wait_time = 0.5
	poll_timer.autostart = true
	poll_timer.one_shot = false
	poll_timer.timeout.connect(_on_poll_timer_timeout)
	add_child(poll_timer)

func _connect_signals() -> void:
	if switch_to_desk_button and not switch_to_desk_button.pressed.is_connected(_on_switch_to_desk_button_pressed):
		switch_to_desk_button.pressed.connect(_on_switch_to_desk_button_pressed)
	if refresh_button and not refresh_button.pressed.is_connected(_on_refresh_button_pressed):
		refresh_button.pressed.connect(_on_refresh_button_pressed)
	if toggle_left_dock_button and not toggle_left_dock_button.pressed.is_connected(_on_toggle_left_dock_pressed):
		toggle_left_dock_button.pressed.connect(_on_toggle_left_dock_pressed)
	if toggle_right_dock_button and not toggle_right_dock_button.pressed.is_connected(_on_toggle_right_dock_pressed):
		toggle_right_dock_button.pressed.connect(_on_toggle_right_dock_pressed)
	if hotline_phone and not hotline_phone.pressed.is_connected(_on_hotline_phone_pressed):
		hotline_phone.pressed.connect(_on_hotline_phone_pressed)
	if executive_inbox and not executive_inbox.pressed.is_connected(_on_executive_inbox_pressed):
		executive_inbox.pressed.connect(_on_executive_inbox_pressed)

# ==============================================================================
# 4. STATE LOADING & PRESENTATION (HONEST DATA — ZERO INVENTED VALUES)
# ==============================================================================
func load_live_state() -> bool:
	return load_state_from_file(current_state_path)

func load_state_from_file(file_path: String) -> bool:
	_init_controls()
	if not FileAccess.file_exists(file_path):
		display_missing_state()
		if state_status_label:
			state_status_label.text = "Status: [No State Loaded] — Missing state file: %s" % file_path
		return false

	var fa := FileAccess.open(file_path, FileAccess.READ)
	if fa == null:
		var err := FileAccess.get_open_error()
		display_missing_state()
		if state_status_label:
			state_status_label.text = "Status: [Error] — Cannot open state file (Error %d)" % err
		return false

	var content := fa.get_as_text()
	fa.close()

	var json := JSON.new()
	var parse_err := json.parse(content)
	if parse_err != OK:
		display_missing_state()
		if state_status_label:
			state_status_label.text = "Status: [Error] — Corrupt state JSON (Line %d)" % json.get_error_line()
		return false

	var data = json.get_data()
	if typeof(data) != TYPE_DICTIONARY:
		display_missing_state()
		if state_status_label:
			state_status_label.text = "Status: [Error] — Invalid state payload format"
		return false

	last_modified_time = FileAccess.get_modified_time(file_path)
	apply_state(data as Dictionary)
	return true

func display_missing_state() -> void:
	_init_controls()
	latest_state = {}
	if country_label:
		country_label.text = "Country: [No State Loaded]"
	if treasury_label:
		treasury_label.text = "Treasury: [No State Loaded]"
	if approval_label:
		approval_label.text = "Approval: [No State Loaded]"
	if stability_label:
		stability_label.text = "Stability: [No State Loaded]"
	if simulation_time_label:
		simulation_time_label.text = "Simulation Time: [No State Loaded]"
	if defcon_label:
		defcon_label.text = "Defense: [No State Loaded]"
	if cabinet_summary_label:
		cabinet_summary_label.text = "Cabinet: [No State Loaded]"
	if civil_service_label:
		civil_service_label.text = "Civil Service: [No State Loaded]"
	if corruption_control_label:
		corruption_control_label.text = "Corruption Control: [No State Loaded]"
	if ruling_party_label:
		ruling_party_label.text = "Ruling Party: [No State Loaded]"
	if polling_support_label:
		polling_support_label.text = "Polling: [No State Loaded]"
	if military_readiness_label:
		military_readiness_label.text = "Readiness: [No State Loaded]"
	if active_crisis_label:
		active_crisis_label.text = "Crisis Status: [No State Loaded]"
	if gdp_label:
		gdp_label.text = "GDP: [No State Loaded]"
	if offline_briefing_label:
		offline_briefing_label.text = "Offline Catch-up: [No State Loaded]"
	if active_project_label:
		active_project_label.text = "Active Project: None"
	if status_ticker_label:
		status_ticker_label.text = "Executive Briefing: Awaiting sovereign state initialization from Republic.Core"
	if portrait_name_label:
		portrait_name_label.text = "HEAD OF STATE: [Unassigned]"
	if portrait_party_label:
		portrait_party_label.text = "Party: [No State Loaded]"
	if flag_emblem_label:
		flag_emblem_label.text = "REPUBLIC"
	state_cleared.emit()

func apply_state(state: Dictionary) -> void:
	latest_state = state

	# 1. Top HUD Bar Metrics
	var country_name := str(state.get("country_name", "[No State Loaded]"))
	if country_label:
		country_label.text = "Country: " + country_name

	if treasury_label:
		var treas := str(state.get("treasury", ""))
		if treas.is_empty() and state.has("treasury_balance"):
			treas = "R " + _format_number_commas(float(state.get("treasury_balance", 0.0)))
		treasury_label.text = "Treasury: " + (treas if not treas.is_empty() else "[No State Loaded]")

	if approval_label:
		var appr := str(state.get("approval", ""))
		if appr.is_empty() and state.has("approval_rating"):
			appr = str(state.get("approval_rating", 0.0)) + "%"
		approval_label.text = "Approval: " + (appr if not appr.is_empty() else "[No State Loaded]")

	if stability_label:
		var stab := str(state.get("stability", ""))
		if stab.is_empty() and state.has("stability_rating"):
			stab = str(state.get("stability_rating", 0.0)) + "%"
		stability_label.text = "Stability: " + (stab if not stab.is_empty() else "[No State Loaded]")

	if simulation_time_label:
		var stime := str(state.get("simulation_time", "[No State Loaded]"))
		simulation_time_label.text = "Simulation Time: " + stime

	if defcon_label:
		var defcon := str(state.get("defcon_level", "DEFCON 5"))
		defcon_label.text = "Defense: " + defcon

	# 2. Left Dock: Domestic Governance
	if cabinet_summary_label:
		cabinet_summary_label.text = str(state.get("cabinet_summary", "Finance: Vacant | Interior: Vacant"))
	if civil_service_label:
		var cs_val := float(state.get("civil_service_quality", 0.3)) * 100.0
		civil_service_label.text = "Civil Service Merit: %0.0f%%" % cs_val
	if corruption_control_label:
		var cc_val := float(state.get("corruption_control", 0.3)) * 100.0
		corruption_control_label.text = "Corruption Control: %0.0f%%" % cc_val
	if ruling_party_label:
		ruling_party_label.text = "Ruling Party: " + str(state.get("ruling_party", "None"))
	if polling_support_label:
		var poll_val := float(state.get("polling_support", 0.0))
		polling_support_label.text = "National Polling: %0.1f%%" % poll_val

	# 3. Right Dock: Geopolitics & Foreign Affairs
	if military_readiness_label:
		military_readiness_label.text = "Military Readiness: " + str(state.get("military_readiness", "85% Operational"))
	if active_crisis_label:
		var crisis := str(state.get("active_crisis", "None"))
		active_crisis_label.text = "Active Crisis: " + crisis
	if gdp_label:
		gdp_label.text = "GDP: " + str(state.get("gdp_formatted", "R 450B"))
	if offline_briefing_label:
		offline_briefing_label.text = str(state.get("offline_briefing_summary", "Simulation aligned."))

	# 4. Bottom Dock: Projects & Ticker
	if active_project_label:
		var proj := str(state.get("active_project", "None"))
		active_project_label.text = "Active Project: " + proj
	if status_ticker_label:
		var latest_result := str(state.get("latest_decision_result", ""))
		if latest_result.is_empty():
			latest_result = str(state.get("status_message", "Presidential executive office online."))
		status_ticker_label.text = "Executive Briefing: " + latest_result
	if state_status_label:
		state_status_label.text = "Status: Connected to live Republic simulation state"

	# 5. Environment Decor Dynamic Updates
	if portrait_name_label:
		var fin_min := str(state.get("finance_minister", ""))
		var pres_title := "HEAD OF STATE: " + country_name
		if not fin_min.is_empty() and fin_min != "Vacant":
			portrait_name_label.text = "PRESIDENT OF THE REPUBLIC"
		else:
			portrait_name_label.text = pres_title
	if portrait_party_label:
		portrait_party_label.text = "Party: " + str(state.get("ruling_party", "Independent Sovereign"))
	if flag_emblem_label:
		var code := country_name.replace("Federal Republic of ", "").replace("Republic of ", "").strip_edges()
		flag_emblem_label.text = code.to_upper() if not code.is_empty() else "REPUBLIC"

	state_loaded.emit(state)

func _on_poll_timer_timeout() -> void:
	if not FileAccess.file_exists(current_state_path):
		return
	var mtime := FileAccess.get_modified_time(current_state_path)
	if mtime != last_modified_time:
		last_modified_time = mtime
		load_state_from_file(current_state_path)

func _format_number_commas(val: float) -> String:
	var rounded := int(round(val))
	var s := str(rounded)
	var is_neg := s.begins_with("-")
	if is_neg:
		s = s.substr(1)
	var formatted := ""
	var count := 0
	for i in range(s.length() - 1, -1, -1):
		if count > 0 and count % 3 == 0:
			formatted = "," + formatted
		formatted = s[i] + formatted
		count += 1
	return ("-" if is_neg else "") + formatted

# ==============================================================================
# 5. DECOUPLED VISUAL LAYER CUSTOMIZATION API
# ==============================================================================
## Updates the capital window view with custom scenery art
func set_city_scenery_texture(texture: Texture2D) -> void:
	if city_scenery and city_scenery.has_method("update_scenery"):
		city_scenery.update_scenery(texture)

## Updates the framed presidential portrait in the office
func set_presidential_portrait(portrait_texture: Texture2D, president_name: String = "", party_name: String = "") -> void:
	if portrait_name_label and not president_name.is_empty():
		portrait_name_label.text = president_name
	if portrait_party_label and not party_name.is_empty():
		portrait_party_label.text = "Party: " + party_name
	if portrait_canvas:
		var tr := portrait_canvas.find_child("PortraitTexture", true, false) as TextureRect
		if tr:
			tr.texture = portrait_texture

## Updates the country flag colors and heraldry in the office
func set_country_flag(banner_color: Color, emblem_text: String = "") -> void:
	if flag_banner:
		var style := flag_banner.get_theme_stylebox("panel") as StyleBoxFlat
		if style:
			style.bg_color = banner_color
	if flag_emblem_label and not emblem_text.is_empty():
		flag_emblem_label.text = emblem_text

## Updates desk surface materials
func set_desk_style(surface_color: Color, blotter_color: Color) -> void:
	if desk_surface:
		var style := desk_surface.get_theme_stylebox("panel") as StyleBoxFlat
		if style:
			style.bg_color = surface_color
	if desk_blotter:
		var style := desk_blotter.get_theme_stylebox("panel") as StyleBoxFlat
		if style:
			style.bg_color = blotter_color

# ==============================================================================
# 6. INTERACTIVE DOCKS & NAVIGATION
# ==============================================================================
func _on_switch_to_desk_button_pressed() -> void:
	get_tree().change_scene_to_file("res://PresidentialDesk.tscn")

func _on_refresh_button_pressed() -> void:
	load_live_state()

func _on_toggle_left_dock_pressed() -> void:
	if left_dock:
		var content := left_dock.find_child("LeftDockContent", true, false) as Control
		if content:
			content.visible = not content.visible
			if toggle_left_dock_button:
				toggle_left_dock_button.text = "▲" if content.visible else "▼"

func _on_toggle_right_dock_pressed() -> void:
	if right_dock:
		var content := right_dock.find_child("RightDockContent", true, false) as Control
		if content:
			content.visible = not content.visible
			if toggle_right_dock_button:
				toggle_right_dock_button.text = "▲" if content.visible else "▼"

func _on_hotline_phone_pressed() -> void:
	if status_ticker_label:
		status_ticker_label.text = "Hotline Console: Secure line operational. Awaiting international diplomatic calls."

func _on_executive_inbox_pressed() -> void:
	if status_ticker_label:
		status_ticker_label.text = "Executive Inbox: Cabinet memoranda and daily intelligence briefings filed."
