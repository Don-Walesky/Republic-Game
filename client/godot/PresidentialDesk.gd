extends Control

var _warned_missing_nodes: Dictionary = {}

## Retrieves a required node or pushes a one-time warning to the Godot Output panel
func _get_required_node(node_path: String) -> Node:
	var n := get_node_or_null(node_path)
	if n == null and not _warned_missing_nodes.has(node_path):
		_warned_missing_nodes[node_path] = true
		push_warning("PresidentialDesk: Required node not found at path '%s'. Please check your scene tree." % node_path)
	return n

## UI Labels in the Presidential Desk
@onready var country_label: Label = _get_required_node("VBoxContainer/CountryLabel") as Label
@onready var treasury_label: Label = _get_required_node("VBoxContainer/TreasuryLabel") as Label
@onready var approval_label: Label = _get_required_node("VBoxContainer/ApprovalLabel") as Label
@onready var stability_label: Label = _get_required_node("VBoxContainer/StabilityLabel") as Label
@onready var simulation_time_label: Label = _get_required_node("VBoxContainer/SimulationTimeLabel") as Label
@onready var ruling_party_label: Label = _get_required_node("VBoxContainer/RulingPartyLabel") as Label
@onready var election_label: Label = _get_required_node("VBoxContainer/ElectionLabel") as Label
@onready var tax_rate_label: Label = _get_required_node("VBoxContainer/TaxRateLabel") as Label
@onready var status_label: Label = _get_required_node("VBoxContainer/StatusLabel") as Label

## Interactive Presidential Decision Controls
@onready var proposed_rate_spinbox: SpinBox = _get_required_node("VBoxContainer/ProposedRateHBox/ProposedRateSpinBox") as SpinBox
@onready var expected_consequence_label: Label = _get_required_node("VBoxContainer/ExpectedConsequenceLabel") as Label
@onready var confirm_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/ConfirmButton") as Button
@onready var cancel_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/CancelButton") as Button
@onready var refresh_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/RefreshButton") as Button
@onready var decision_result_label: Label = _get_required_node("VBoxContainer/DecisionResultLabel") as Label

## Multiplayer Networking Controls & Display
@onready var host_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/HostButton") as Button
@onready var join_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/JoinButton") as Button
@onready var address_line_edit: LineEdit = _get_required_node("VBoxContainer/MultiplayerControlsHBox/AddressLineEdit") as LineEdit
@onready var disconnect_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/DisconnectButton") as Button
@onready var network_status_label: Label = _get_required_node("VBoxContainer/NetworkStatusLabel") as Label
@onready var foreign_country_name_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryNameLabel") as Label
@onready var foreign_country_treasury_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryTreasuryLabel") as Label
@onready var foreign_country_approval_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryApprovalLabel") as Label
@onready var shared_clock_label: Label = _get_required_node("VBoxContainer/SharedClockLabel") as Label

## File paths scanned for authoritative Republic simulation state
const DEFAULT_STATE_PATHS: Array[String] = [
	"res://presidential_desk_state.json",
	"user://presidential_desk_state.json"
]

## Polling interval in seconds to detect file-based state changes
@export var poll_interval: float = 0.5

const DEFAULT_REQUEST_FILENAME: String = "presidential_desk_request.json"
const REQUEST_TIMEOUT_SECONDS: float = 4.0

var _active_state_path: String = ""
var _last_modified_time: int = 0
var _poll_timer: float = 0.0
var _is_connected: bool = false
var _latest_state: Dictionary = {}
var _user_is_adjusting: bool = false
var _pending_request_path: String = ""
var _is_waiting_for_decision: bool = false
var _active_request_id: String = ""
var _request_wait_timer: float = 0.0

## Multiplayer network session state
var _tcp_client: StreamPeerTCP = null
var _is_network_connected: bool = false
var _is_network_connecting: bool = false
var _network_read_buffer: String = ""
var _local_client_id: String = ""
var _peer_country_id: String = ""
var _is_hosting_process: bool = false
var _host_process_pid: int = -1


func _ready() -> void:
	if _local_client_id.is_empty():
		_local_client_id = "client_" + str(Time.get_ticks_msec())
	for arg in OS.get_cmdline_args():
		if arg.begins_with("--client-id="):
			_local_client_id = arg.substr("--client-id=".length()).strip_edges()
	_init_controls()
	_wire_signals()
	refresh()


## Initializes node references if not already bound and guarantees interactive controls remain usable
func _init_controls() -> void:
	if country_label == null:
		country_label = _get_required_node("VBoxContainer/CountryLabel") as Label
	if treasury_label == null:
		treasury_label = _get_required_node("VBoxContainer/TreasuryLabel") as Label
	if approval_label == null:
		approval_label = _get_required_node("VBoxContainer/ApprovalLabel") as Label
	if stability_label == null:
		stability_label = _get_required_node("VBoxContainer/StabilityLabel") as Label
	if simulation_time_label == null:
		simulation_time_label = _get_required_node("VBoxContainer/SimulationTimeLabel") as Label
	if ruling_party_label == null:
		ruling_party_label = _get_required_node("VBoxContainer/RulingPartyLabel") as Label
	if election_label == null:
		election_label = _get_required_node("VBoxContainer/ElectionLabel") as Label
	if tax_rate_label == null:
		tax_rate_label = _get_required_node("VBoxContainer/TaxRateLabel") as Label
	if status_label == null:
		status_label = _get_required_node("VBoxContainer/StatusLabel") as Label

	if proposed_rate_spinbox == null:
		proposed_rate_spinbox = _get_required_node("VBoxContainer/ProposedRateHBox/ProposedRateSpinBox") as SpinBox
	if expected_consequence_label == null:
		expected_consequence_label = _get_required_node("VBoxContainer/ExpectedConsequenceLabel") as Label
	if confirm_button == null:
		confirm_button = _get_required_node("VBoxContainer/DecisionControlsHBox/ConfirmButton") as Button
	if cancel_button == null:
		cancel_button = _get_required_node("VBoxContainer/DecisionControlsHBox/CancelButton") as Button
	if refresh_button == null:
		refresh_button = _get_required_node("VBoxContainer/DecisionControlsHBox/RefreshButton") as Button
	if decision_result_label == null:
		decision_result_label = _get_required_node("VBoxContainer/DecisionResultLabel") as Label

	if host_button == null:
		host_button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/HostButton") as Button
	if join_button == null:
		join_button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/JoinButton") as Button
	if address_line_edit == null:
		address_line_edit = _get_required_node("VBoxContainer/MultiplayerControlsHBox/AddressLineEdit") as LineEdit
	if disconnect_button == null:
		disconnect_button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/DisconnectButton") as Button
	if network_status_label == null:
		network_status_label = _get_required_node("VBoxContainer/NetworkStatusLabel") as Label
	if foreign_country_name_label == null:
		foreign_country_name_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryNameLabel") as Label
	if foreign_country_treasury_label == null:
		foreign_country_treasury_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryTreasuryLabel") as Label
	if foreign_country_approval_label == null:
		foreign_country_approval_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryApprovalLabel") as Label
	if shared_clock_label == null:
		shared_clock_label = _get_required_node("VBoxContainer/SharedClockLabel") as Label

	if not _is_network_connected and not _is_network_connecting:
		if host_button:
			host_button.disabled = false
		if join_button:
			join_button.disabled = false
		if disconnect_button:
			disconnect_button.disabled = true

	# Ensure SpinBox and buttons remain completely usable even if state file is temporarily missing (unless waiting for decision)
	if not _is_waiting_for_decision:
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = true
		if confirm_button:
			confirm_button.disabled = false
		if cancel_button:
			cancel_button.disabled = false
		if refresh_button:
			refresh_button.disabled = false


## Connects interactive control signals
func _wire_signals() -> void:
	if proposed_rate_spinbox and not proposed_rate_spinbox.value_changed.is_connected(_on_proposed_rate_changed):
		proposed_rate_spinbox.value_changed.connect(_on_proposed_rate_changed)
	if confirm_button and not confirm_button.pressed.is_connected(_on_confirm_button_pressed):
		confirm_button.pressed.connect(_on_confirm_button_pressed)
	if cancel_button and not cancel_button.pressed.is_connected(_on_cancel_button_pressed):
		cancel_button.pressed.connect(_on_cancel_button_pressed)
	if refresh_button and not refresh_button.pressed.is_connected(_on_refresh_button_pressed):
		refresh_button.pressed.connect(_on_refresh_button_pressed)
	if host_button and not host_button.pressed.is_connected(_on_host_button_pressed):
		host_button.pressed.connect(_on_host_button_pressed)
	if join_button and not join_button.pressed.is_connected(_on_join_button_pressed):
		join_button.pressed.connect(_on_join_button_pressed)
	if disconnect_button and not disconnect_button.pressed.is_connected(_on_disconnect_button_pressed):
		disconnect_button.pressed.connect(_on_disconnect_button_pressed)


func _process(delta: float) -> void:
	# 1. Process pending decision results first to avoid race conditions with state polling
	if _is_waiting_for_decision:
		_request_wait_timer += delta

		# Check if Republic simulation has produced presidential_desk_result.json
		var res_path := _resolve_result_path(_active_state_path)
		if FileAccess.file_exists(res_path):
			var file := FileAccess.open(res_path, FileAccess.READ)
			if file:
				var text := file.get_as_text()
				file.close()
				var parsed: Variant = JSON.parse_string(text)
				if parsed is Dictionary and not parsed.is_empty():
					var res_req_id: String = str(parsed.get("request_id", ""))
					# Accept result if matching active request, or if request_id wasn't specified (backward compat)
					if _active_request_id.is_empty() or res_req_id.is_empty() or res_req_id == _active_request_id:
						_handle_decision_result(parsed)

		if _is_waiting_for_decision and _request_wait_timer >= REQUEST_TIMEOUT_SECONDS:
			_is_waiting_for_decision = false
			_active_request_id = ""
			if confirm_button:
				confirm_button.disabled = false
			if cancel_button:
				cancel_button.disabled = false
			if refresh_button:
				refresh_button.disabled = false
			if proposed_rate_spinbox:
				proposed_rate_spinbox.editable = true
			if not _pending_request_path.is_empty() and FileAccess.file_exists(_pending_request_path):
				if status_label:
					var warn_text := "Status: Warning - Decision request written, but no updated simulation state arrived within %.0fs. Is Republic simulation running?" % REQUEST_TIMEOUT_SECONDS
					status_label.text = _format_status_text(warn_text, "Timed out")

	# 2. Check for live state updates
	_poll_timer += delta
	if _poll_timer >= poll_interval:
		_poll_timer = 0.0
		_check_for_live_updates()

	# 3. Poll multiplayer network stream
	_poll_network()



## Manually triggers a reload of the simulation state file
func refresh() -> bool:
	var path := _resolve_state_path()
	if path.is_empty():
		display_missing_state("No simulation state file located in standard search paths.")
		return false
	return load_state_from_file(path)


## Resolves the authoritative state file path from arguments, environment, or default paths
func _resolve_state_path() -> String:
	# 0. Prioritize active state path if currently loaded and exists
	if not _active_state_path.is_empty() and FileAccess.file_exists(_active_state_path):
		return _active_state_path

	# 1. Check command-line override (--state-file=<path>)
	for arg in OS.get_cmdline_args():
		if arg.begins_with("--state-file="):
			var custom_path := arg.substr("--state-file=".length()).strip_edges()
			if FileAccess.file_exists(custom_path):
				return custom_path

	# 2. Check environment variable override
	var env_path := OS.get_environment("REPUBLIC_STATE_FILE")
	if not env_path.is_empty() and FileAccess.file_exists(env_path):
		return env_path

	# 3. Check default candidate paths
	for candidate in DEFAULT_STATE_PATHS:
		if FileAccess.file_exists(candidate):
			return candidate

	return ""


## Loads and parses a JSON simulation state snapshot from disk
func load_state_from_file(path: String) -> bool:
	if not FileAccess.file_exists(path):
		display_missing_state("State file not found: " + path)
		return false

	if status_label:
		if _is_connected:
			status_label.text = _format_status_text("Status: Refreshing simulation state...", "None")
		else:
			status_label.text = _format_status_text("Status: Loading simulation state...", "None")

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		display_error("Failed to open simulation state file: " + path)
		return false

	var content := file.get_as_text()
	var json_result: Variant = JSON.parse_string(content)

	if json_result == null or not (json_result is Dictionary):
		display_error("Invalid or corrupted JSON in state file: " + path)
		return false

	var data: Dictionary = json_result
	_active_state_path = path
	_last_modified_time = FileAccess.get_modified_time(path)
	_latest_state = data
	_is_connected = true

	apply_state(data)
	return true


## Updates interface labels with live simulation state values
func apply_state(data: Dictionary) -> void:
	_init_controls()
	var was_waiting := _is_waiting_for_decision

	# Only reset waiting state and re-enable controls if we are NOT actively waiting for a decision result
	if not was_waiting:
		_is_waiting_for_decision = false
		_request_wait_timer = 0.0
		if confirm_button:
			confirm_button.disabled = false
		if cancel_button:
			cancel_button.disabled = false
		if refresh_button:
			refresh_button.disabled = false
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = true

	var country_name: String = str(data.get("country_name", data.get("CountryName", "[Unknown]")))
	var treasury: String = str(data.get("treasury", data.get("Treasury", "[Unknown]")))
	var approval: String = str(data.get("approval", data.get("Approval", "[Unknown]")))
	var stability: String = str(data.get("stability", data.get("Stability", "[Unknown]")))
	var sim_time: String = str(data.get("simulation_time", data.get("SimulationTime", "[Unknown]")))
	var ruling_party: String = str(data.get("ruling_party", data.get("RulingParty", "None")))
	var election: String = str(data.get("election", data.get("Election", "No election")))
	var status_msg: String = str(data.get("status_message", data.get("StatusMessage", "Connected to live Republic simulation state")))

	var tax_rate_fmt: String = str(data.get("income_tax_rate_formatted", data.get("IncomeTaxRateFormatted", "")))
	var raw_tax_rate: float = float(data.get("income_tax_rate", data.get("IncomeTaxRate", 0.0)))
	if tax_rate_fmt.is_empty():
		tax_rate_fmt = str(round(raw_tax_rate * 1000.0) / 10.0) + "%"

	if country_label:
		country_label.text = "Country: " + country_name
	if treasury_label:
		treasury_label.text = "Treasury: " + treasury
	if approval_label:
		approval_label.text = "Approval: " + approval
	if stability_label:
		stability_label.text = "Stability: " + stability
	if simulation_time_label:
		simulation_time_label.text = "Simulation Time: " + sim_time
	if ruling_party_label:
		ruling_party_label.text = "Ruling party: " + ruling_party
	if election_label:
		election_label.text = "Election: " + election
	if tax_rate_label:
		tax_rate_label.text = "Income Tax Rate: " + tax_rate_fmt
	if status_label:
		status_label.text = _format_status_text("Status: " + status_msg, "None")

	var current_rate_pct := raw_tax_rate * 100.0

	# Only synchronize spinbox if player is not currently typing / modifying
	if proposed_rate_spinbox and not _user_is_adjusting:
		proposed_rate_spinbox.set_value_no_signal(current_rate_pct)
		_update_expected_consequence_display(current_rate_pct)

	# Only update decision result label if not actively waiting for a decision outcome
	if not was_waiting:
		var latest_result: String = str(data.get("latest_decision_result", data.get("LatestDecisionResult", "")))
		if not latest_result.is_empty() and decision_result_label:
			decision_result_label.text = "Decision Result: " + latest_result
		else:
			var result_data := _read_result_file()
			if not result_data.is_empty() and decision_result_label:
				var res_msg := str(result_data.get("message", ""))
				if not res_msg.is_empty():
					decision_result_label.text = "Decision Result: " + res_msg

	# If connected to multiplayer network host, broadcast updated country snapshot
	if _is_network_connected:
		_send_local_country_snapshot()


## Formats the status label text to clearly display connection state, state file presence,
## last updated timestamp, and tax request status.
func _format_status_text(base_msg: String = "", tax_req_status: String = "") -> String:
	var prefix := base_msg.strip_edges()
	if prefix.is_empty():
		prefix = "Status: Connected to live Republic simulation state" if _is_connected else "Status: Waiting for Republic simulation state"

	# 1. State file presence
	var file_status: String
	var resolved_path := _active_state_path if (_is_connected and not _active_state_path.is_empty()) else _resolve_state_path()
	if not resolved_path.is_empty() and FileAccess.file_exists(resolved_path):
		file_status = "State file: Found (%s)" % resolved_path
	else:
		file_status = "State file: Not found"

	# 2. Last updated timestamp
	var updated_str: String
	if _last_modified_time > 0:
		updated_str = "Last updated: " + Time.get_datetime_string_from_unix_time(_last_modified_time, false).replace("T", " ")
	else:
		updated_str = "Last updated: Never"

	# 3. Tax request status
	var req_status_text: String
	if _is_waiting_for_decision:
		req_status_text = "Tax request: Pending"
	elif not tax_req_status.is_empty():
		req_status_text = "Tax request: " + tax_req_status
	else:
		var req_path := _resolve_request_path(resolved_path)
		if not req_path.is_empty() and FileAccess.file_exists(req_path):
			req_status_text = "Tax request: Pending"
		else:
			req_status_text = "Tax request: None"

	return "%s | %s | %s | %s" % [prefix, file_status, updated_str, req_status_text]


## Visually displays a missing state notice without falling back to sample demonstration data
func display_missing_state(details: String = "") -> void:
	_init_controls()
	_is_connected = false
	_latest_state = {}

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
	if ruling_party_label:
		ruling_party_label.text = "Ruling party: [No State Loaded]"
	if election_label:
		election_label.text = "Election: [No State Loaded]"
	if tax_rate_label:
		tax_rate_label.text = "Income Tax Rate: [No State Loaded]"
	if status_label:
		status_label.text = _format_status_text("Status: Waiting for Republic simulation state (" + details + ")", "None")
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: [No State Loaded]"
	if decision_result_label:
		decision_result_label.text = "Decision Result: [No State Loaded]"

	# Ensure SpinBox and buttons remain completely usable even if state file is temporarily missing
	if proposed_rate_spinbox:
		proposed_rate_spinbox.editable = true
	if confirm_button:
		confirm_button.disabled = false
	if cancel_button:
		cancel_button.disabled = false
	if refresh_button:
		refresh_button.disabled = false


## Visually displays an error condition without falling back to sample demonstration data
func display_error(error_msg: String) -> void:
	_init_controls()
	_is_connected = false
	_latest_state = {}

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
	if ruling_party_label:
		ruling_party_label.text = "Ruling party: [No State Loaded]"
	if election_label:
		election_label.text = "Election: [No State Loaded]"
	if tax_rate_label:
		tax_rate_label.text = "Income Tax Rate: [No State Loaded]"
	if status_label:
		status_label.text = _format_status_text("Status: Error - " + error_msg, "None")
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: Error"
	if decision_result_label:
		decision_result_label.text = "Decision Result: Error - " + error_msg

	# Ensure SpinBox and buttons remain completely usable even if state file is corrupted
	if proposed_rate_spinbox:
		proposed_rate_spinbox.editable = true
	if confirm_button:
		confirm_button.disabled = false
	if cancel_button:
		cancel_button.disabled = false
	if refresh_button:
		refresh_button.disabled = false


## Checks if the state file has been modified on disk and reloads when changed
func _check_for_live_updates() -> void:
	var path := _resolve_state_path()

	if path.is_empty():
		if _is_connected:
			display_missing_state("Active simulation state file was removed")
		return

	if not _is_connected or path != _active_state_path:
		load_state_from_file(path)
		return

	var current_mod_time := FileAccess.get_modified_time(path)
	if current_mod_time != _last_modified_time:
		load_state_from_file(path)


## Handler for user modifying the proposed tax rate spinbox
func _on_proposed_rate_changed(new_value: float) -> void:
	_user_is_adjusting = true
	_update_expected_consequence_display(new_value)

	if _is_connected and not _latest_state.is_empty():
		var current_rate_pct: float = float(_latest_state.get("income_tax_rate", 0.0)) * 100.0
		if abs(new_value - current_rate_pct) >= 0.001:
			if decision_result_label:
				decision_result_label.text = "Decision Status: Awaiting player confirmation (Proposed: %.1f%%, Current: %.1f%%). Press 'Confirm Decision' to submit." % [new_value, current_rate_pct]
		else:
			if decision_result_label:
				decision_result_label.text = "Decision Status: No rate change proposed (Current: %.1f%%)." % current_rate_pct
	else:
		if decision_result_label:
			decision_result_label.text = "Decision Status: No simulation state loaded."


## Updates expected consequence label preview using authoritative Phase 11 feedback rules
func _update_expected_consequence_display(proposed_rate_pct: float) -> void:
	if expected_consequence_label == null:
		return
	if not _is_connected or _latest_state.is_empty():
		expected_consequence_label.text = "Expected Approval Effect: [No State Loaded]"
		return
	var current_rate_pct: float = float(_latest_state.get("income_tax_rate", 0.0)) * 100.0
	expected_consequence_label.text = calculate_expected_consequence(current_rate_pct, proposed_rate_pct)


## Calculates the expected approval impact according to Phase 11 rules:
## -0.5% approval per +1% tax increase, +0.5% approval per -1% tax decrease.
## Also evaluates the >50% extreme taxation stability threshold warning.
func calculate_expected_consequence(current_rate_pct: float, proposed_rate_pct: float) -> String:
	var delta_pct: float = proposed_rate_pct - current_rate_pct
	if abs(delta_pct) < 0.001:
		return "Expected Approval Effect: No change (0.0%). Existing approval rating will be maintained."

	var approval_delta: float = -delta_pct * 0.5
	var sign_str: String = "+" if approval_delta > 0.0 else ""
	var effect_text: String = "Expected Approval Effect: " + sign_str + ("%.1f" % approval_delta) + "% "

	if delta_pct > 0.0:
		effect_text += "(Tax hike reduces household disposable income; -0.5% approval per +1% tax)."
	else:
		effect_text += "(Tax relief expands household disposable income; +0.5% approval per -1% tax)."

	if current_rate_pct <= 50.0 and proposed_rate_pct > 50.0:
		effect_text += " WARNING: Exceeds 50% limit; stability penalty of -2.0% will apply!"
	elif current_rate_pct > 50.0 and proposed_rate_pct <= 50.0:
		effect_text += " BENEFIT: Easing to 50% or below restores +2.0% stability!"

	return effect_text


## Handler for confirm decision button
func _on_confirm_button_pressed() -> void:
	var rate_val := proposed_rate_spinbox.value if proposed_rate_spinbox else 0.0
	execute_tax_decision(rate_val, true)


## Handler for cancel decision button
func _on_cancel_button_pressed() -> void:
	var rate_val := proposed_rate_spinbox.value if proposed_rate_spinbox else 0.0
	execute_tax_decision(rate_val, false)


## Handler for refresh / apply pending button
func _on_refresh_button_pressed() -> void:
	apply_pending_or_refresh()


## Manually refreshes the simulation state, checks for decision results, and processes any pending request
func apply_pending_or_refresh() -> bool:
	# 1. Process pending decision results first if result file is already available
	var res_path := _resolve_result_path(_active_state_path)
	if FileAccess.file_exists(res_path):
		var file := FileAccess.open(res_path, FileAccess.READ)
		if file:
			var text := file.get_as_text()
			file.close()
			var parsed: Variant = JSON.parse_string(text)
			if parsed is Dictionary and not parsed.is_empty():
				_handle_decision_result(parsed)
				return true

	# 2. Check if a request file is pending on disk; if so, attempt non-blocking backend processing
	var target_state := _active_state_path if not _active_state_path.is_empty() else _resolve_state_path()
	var req_path := _resolve_request_path(target_state)
	if FileAccess.file_exists(req_path):
		_try_run_cli_request_processor()

	# 3. Reload state file
	var reloaded := refresh()
	if reloaded and status_label:
		status_label.text = _format_status_text("Status: Refreshed live Republic simulation state")
	return reloaded


## Attempts non-blocking execution of the CLI request processor if DLL or project is present
func _try_run_cli_request_processor() -> void:
	var candidates: Array[String] = [
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll"),
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"),
		"src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll",
		"src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"
	]
	var cli_dll := ""
	for c in candidates:
		if FileAccess.file_exists(c):
			cli_dll = c
			break

	var state_path := _resolve_state_path()
	if not cli_dll.is_empty():
		var args: PackedStringArray = [cli_dll, "--process-request"]
		if not state_path.is_empty():
			args.append("--state-file=" + ProjectSettings.globalize_path(state_path))
		OS.create_process("dotnet", args)
	else:
		var proj_candidates: Array[String] = [
			ProjectSettings.globalize_path("res://../../src/Republic.Cli/Republic.Cli.csproj"),
			"src/Republic.Cli/Republic.Cli.csproj"
		]
		for proj in proj_candidates:
			if FileAccess.file_exists(proj):
				var args: PackedStringArray = ["run", "--project", proj, "--no-build", "--", "--process-request"]
				if not state_path.is_empty():
					args.append("--state-file=" + ProjectSettings.globalize_path(state_path))
				OS.create_process("dotnet", args)
				break


## Submits a presidential taxation decision via file-based request (presidential_desk_request.json)
func execute_tax_decision(proposed_rate_percent: float, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false

	# Prevent repeated clicks while decision is being processed
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	# 1. Reject if no valid simulation state is connected
	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot submit decision: No Republic simulation state loaded."
		if not confirmed:
			no_state_msg = "Decision cancelled: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: Missing or Corrupted Save - " + no_state_msg
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = true
		if confirm_button:
			confirm_button.disabled = false
		if cancel_button:
			cancel_button.disabled = false
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	# 2. Constitutional bounds validation (0% to 70%) for confirmed decisions
	if confirmed and (proposed_rate_percent < 0.0 or proposed_rate_percent > 70.0):
		var invalid_msg: String = "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%)."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + invalid_msg
		return {
			"success": false,
			"result": "InvalidInput",
			"message": invalid_msg
		}

	# Disable buttons during active processing to prevent duplicate submissions
	if confirmed:
		if confirm_button:
			confirm_button.disabled = true
		if cancel_button:
			cancel_button.disabled = true
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = false

	# 3. Resolve paths
	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)

	# Clean up any stale result file so we only read fresh results
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	# 4. Write presidential_desk_request.json next to state file
	var req_id := "req_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var proposed_rate_decimal: float = proposed_rate_percent / 100.0
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"proposed_income_tax_rate": proposed_rate_decimal,
		"proposed_rate": proposed_rate_decimal,
		"proposed_rate_percent": proposed_rate_percent,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create decision request file: " + request_path
		_active_request_id = ""
		if confirm_button:
			confirm_button.disabled = false
		if cancel_button:
			cancel_button.disabled = false
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = true
		if status_label:
			status_label.text = _format_status_text("Status: Error - " + err_msg, "Failed")
		if decision_result_label:
			decision_result_label.text = "Decision Result: Error - " + err_msg
		return {
			"success": false,
			"result": "Error",
			"message": err_msg
		}

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	# 5. UI feedback for confirmed vs cancelled
	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		if confirm_button:
			confirm_button.disabled = false
		if cancel_button:
			cancel_button.disabled = false
		if proposed_rate_spinbox:
			proposed_rate_spinbox.editable = true

		var cancel_msg: String = "Decision cancelled: Existing tax rates and citizen approval preserved unchanged."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + cancel_msg
		if status_label:
			status_label.text = _format_status_text("Status: Cancellation request written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		# Reset spinbox back to current live rate
		if _is_connected and not _latest_state.is_empty():
			var current_rate: float = float(_latest_state.get("income_tax_rate", 0.0)) * 100.0
			if proposed_rate_spinbox:
				proposed_rate_spinbox.set_value_no_signal(current_rate)
				_update_expected_consequence_display(current_rate)
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	var submit_msg := "Proposed %.1f%% tax submitted. Awaiting simulation..." % proposed_rate_percent
	if decision_result_label:
		decision_result_label.text = "Decision Result: " + submit_msg
	if status_label:
		status_label.text = _format_status_text("Status: Decision request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	return {
		"success": true,
		"result": "Pending",
		"message": submit_msg
	}


## Handles incoming structured action results from Republic simulation
func _handle_decision_result(result_dict: Dictionary) -> void:
	_is_waiting_for_decision = false
	_active_request_id = ""
	_request_wait_timer = 0.0
	if confirm_button:
		confirm_button.disabled = false
	if cancel_button:
		cancel_button.disabled = false
	if refresh_button:
		refresh_button.disabled = false
	if proposed_rate_spinbox:
		proposed_rate_spinbox.editable = true

	# Clean up result file so each result is handled only once and not mistaken for future decisions
	var res_path := _resolve_result_path(_active_state_path)
	if FileAccess.file_exists(res_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(res_path))

	var is_success: bool = bool(result_dict.get("success", false))
	var result_type: String = str(result_dict.get("result", ""))
	var message: String = str(result_dict.get("message", ""))

	if is_success:
		# Reload state file to reflect authoritative saved changes
		var reload_success := false
		var path_to_load := _active_state_path
		if path_to_load.is_empty():
			path_to_load = _resolve_state_path()

		if not path_to_load.is_empty() and FileAccess.file_exists(path_to_load):
			reload_success = load_state_from_file(path_to_load)

		if reload_success:
			if decision_result_label:
				decision_result_label.text = "Decision Result: Successfully applied - " + message
			if status_label:
				status_label.text = _format_status_text("Status: Decision enacted and persisted successfully", "Completed")
		else:
			# Authoritative save succeeded, but refreshing the display failed
			if decision_result_label:
				decision_result_label.text = "Decision Result: Authoritative decision saved, but refreshing the display state file failed."
			if status_label:
				status_label.text = _format_status_text("Status: Warning - Decision saved, but display refresh failed", "RefreshFailed")
	else:
		# Handle specific failure outcomes honestly
		var formatted_result := ""
		match result_type:
			"Cancelled":
				formatted_result = "Decision Result: Cancelled - " + message
			"InvalidInput":
				formatted_result = "Decision Result: Invalid Input - " + message
			"StateMissingOrCorrupt":
				formatted_result = "Decision Result: Missing or Corrupted Save - " + message
			"PersistenceFailure":
				formatted_result = "Decision Result: Persistence Failure - " + message
			_:
				formatted_result = "Decision Result: Rejected - " + message

		if decision_result_label:
			decision_result_label.text = formatted_result
		if status_label:
			status_label.text = _format_status_text("Status: " + message, result_type)

		# Reset SpinBox to current live rate on cancellation or failure
		if _is_connected and not _latest_state.is_empty():
			var live_rate: float = float(_latest_state.get("income_tax_rate", 0.0)) * 100.0
			if proposed_rate_spinbox:
				proposed_rate_spinbox.set_value_no_signal(live_rate)
				_update_expected_consequence_display(live_rate)


## Resolves the request file path adjacent to the active simulation state file
func _resolve_request_path(state_path: String = "") -> String:
	var base_path := state_path
	if base_path.is_empty():
		base_path = _resolve_state_path()

	if not base_path.is_empty():
		var dir := base_path.get_base_dir()
		if not dir.is_empty():
			return dir + "/" + DEFAULT_REQUEST_FILENAME

	return "res://" + DEFAULT_REQUEST_FILENAME


## Resolves the result file path adjacent to the active simulation state file
func _resolve_result_path(state_path: String = "") -> String:
	var base_path := state_path
	if base_path.is_empty():
		base_path = _resolve_state_path()

	if not base_path.is_empty():
		var dir := base_path.get_base_dir()
		if not dir.is_empty():
			return dir + "/presidential_desk_result.json"

	return "res://presidential_desk_result.json"


## Reads structured action result from presidential_desk_result.json
func _read_result_file() -> Dictionary:
	var result_candidates: Array[String] = []
	if not _active_state_path.is_empty():
		result_candidates.append(_resolve_result_path(_active_state_path))
	result_candidates.append("res://presidential_desk_result.json")
	result_candidates.append("user://presidential_desk_result.json")

	for path in result_candidates:
		if FileAccess.file_exists(path):
			var file := FileAccess.open(path, FileAccess.READ)
			if file:
				var parsed: Variant = JSON.parse_string(file.get_as_text())
				file.close()
				if parsed is Dictionary and not parsed.is_empty():
					return parsed
	return {}


## -----------------------------------------------------------------------------
## PHASE 2 MULTIPLAYER NETWORKING METHODS
## -----------------------------------------------------------------------------

func _on_host_button_pressed() -> void:
	if network_status_label:
		network_status_label.text = "Network: Starting host & connecting..."
	_try_start_host_cli()
	var port := 7777
	if address_line_edit and not address_line_edit.text.is_empty():
		var parts := address_line_edit.text.split(":")
		if parts.size() >= 2 and parts[1].is_valid_int():
			port = int(parts[1])
	_connect_to_network("127.0.0.1", port)


func _on_join_button_pressed() -> void:
	var host := "127.0.0.1"
	var port := 7777
	if address_line_edit and not address_line_edit.text.is_empty():
		var addr_text := address_line_edit.text.strip_edges()
		if ":" in addr_text:
			var parts := addr_text.split(":")
			host = parts[0].strip_edges()
			if parts.size() > 1 and parts[1].is_valid_int():
				port = int(parts[1])
		else:
			host = addr_text

	if network_status_label:
		network_status_label.text = "Network: Joining %s:%d..." % [host, port]
	_connect_to_network(host, port)


func _on_disconnect_button_pressed() -> void:
	disconnect_network()


func _try_start_host_cli() -> void:
	var args := ["run", "--project", "src/Republic.Cli", "--", "--host", "--port=7777"]
	var repo_root := ProjectSettings.globalize_path("res://../../")
	if FileAccess.file_exists(repo_root + "src/Republic.Cli/Republic.Cli.csproj"):
		var pid := OS.create_process("dotnet", args, false)
		if pid > 0:
			_host_process_pid = pid
			_is_hosting_process = true


func _connect_to_network(host: String, port: int) -> void:
	if _tcp_client != null:
		disconnect_network()

	_tcp_client = StreamPeerTCP.new()
	var err := _tcp_client.connect_to_host(host, port)
	if err != OK:
		if network_status_label:
			network_status_label.text = "Network: Failed to connect to %s:%d (Error %d)" % [host, port, err]
		_tcp_client = null
		return

	_is_network_connecting = true
	_is_network_connected = false
	_network_read_buffer = ""
	if host_button:
		host_button.disabled = true
	if join_button:
		join_button.disabled = true
	if disconnect_button:
		disconnect_button.disabled = false


func disconnect_network() -> void:
	if _tcp_client != null:
		_tcp_client.disconnect_from_host()
		_tcp_client = null
	_is_network_connected = false
	_is_network_connecting = false
	_network_read_buffer = ""
	_peer_country_id = ""

	if _is_hosting_process and _host_process_pid > 0:
		OS.kill(_host_process_pid)
		_host_process_pid = -1
		_is_hosting_process = false

	if network_status_label:
		network_status_label.text = "Network: Disconnected (Single-player local mode)"
	if host_button:
		host_button.disabled = false
	if join_button:
		join_button.disabled = false
	if disconnect_button:
		disconnect_button.disabled = true

	_reset_foreign_country_display()


func _reset_foreign_country_display() -> void:
	if foreign_country_name_label:
		foreign_country_name_label.text = "Foreign Country: [None Connected]"
	if foreign_country_treasury_label:
		foreign_country_treasury_label.text = "Treasury: [N/A]"
	if foreign_country_approval_label:
		foreign_country_approval_label.text = "Approval: [N/A]"
	if shared_clock_label:
		shared_clock_label.text = "Shared World Clock: [Local Clock]"


func _poll_network() -> void:
	if _tcp_client == null:
		return

	_tcp_client.poll()
	var status := _tcp_client.get_status()

	if status == StreamPeerTCP.STATUS_CONNECTING:
		return

	if status == StreamPeerTCP.STATUS_CONNECTED:
		if _is_network_connecting:
			_is_network_connecting = false
			_is_network_connected = true
			if network_status_label:
				network_status_label.text = "Network: Connected to host"
			if host_button:
				host_button.disabled = true
			if join_button:
				join_button.disabled = true
			if disconnect_button:
				disconnect_button.disabled = false
			_send_local_country_snapshot()

		var avail := _tcp_client.get_available_bytes()
		if avail > 0:
			var incoming_str := _tcp_client.get_utf8_string(avail)
			_network_read_buffer += incoming_str

			while "\n" in _network_read_buffer:
				var newline_pos := _network_read_buffer.find("\n")
				var line := _network_read_buffer.substr(0, newline_pos).strip_edges()
				_network_read_buffer = _network_read_buffer.substr(newline_pos + 1)
				if not line.is_empty():
					var parsed: Variant = JSON.parse_string(line)
					if parsed is Dictionary:
						_handle_network_message(parsed)

	elif status == StreamPeerTCP.STATUS_ERROR or status == StreamPeerTCP.STATUS_NONE:
		if _is_network_connected or _is_network_connecting:
			disconnect_network()
			if network_status_label:
				network_status_label.text = "Network: Connection closed or unreachable"


func _send_network_message(msg: Dictionary) -> void:
	if _tcp_client == null or not _is_network_connected:
		return
	var json_str := JSON.stringify(msg) + "\n"
	_tcp_client.put_utf8_string(json_str)


func _send_local_country_snapshot() -> void:
	var country_name := str(_latest_state.get("country_name", _latest_state.get("CountryName", "Republic Sovereign")))
	var treasury := str(_latest_state.get("treasury", _latest_state.get("Treasury", "$0")))
	var approval := str(_latest_state.get("approval", _latest_state.get("Approval", "50.0%")))
	var raw_tax_rate := float(_latest_state.get("income_tax_rate", _latest_state.get("IncomeTaxRate", 0.15)))

	var snap := {
		"id": _local_client_id,
		"name": country_name,
		"treasury": treasury,
		"approval": approval,
		"tax_rate": raw_tax_rate
	}

	var msg := {
		"type": "country",
		"sender_id": _local_client_id,
		"country": snap
	}
	_send_network_message(msg)


func _handle_network_message(msg: Dictionary) -> void:
	var msg_type := str(msg.get("type", ""))
	var sender_id := str(msg.get("sender_id", ""))

	if msg_type == "clock":
		var clock_data: Variant = msg.get("clock", {})
		if clock_data is Dictionary:
			var time_str := str(clock_data.get("time_string", ""))
			var tick := int(clock_data.get("tick", 0))
			var day := int(clock_data.get("day", 1))
			if shared_clock_label:
				shared_clock_label.text = "Shared World Clock: Day %d - %s (Tick %d)" % [day, time_str, tick]

	elif msg_type == "country":
		var snap: Variant = msg.get("country", {})
		if snap is Dictionary:
			var snap_id := str(snap.get("id", sender_id))
			if snap_id != _local_client_id:
				_peer_country_id = snap_id
				var peer_name := str(snap.get("name", "Unknown Sovereign"))
				var peer_treasury := str(snap.get("treasury", "N/A"))
				var peer_approval := str(snap.get("approval", "N/A"))
				if foreign_country_name_label:
					foreign_country_name_label.text = "Foreign Country: " + peer_name
				if foreign_country_treasury_label:
					foreign_country_treasury_label.text = "Treasury: " + peer_treasury
				if foreign_country_approval_label:
					foreign_country_approval_label.text = "Approval: " + peer_approval


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST or what == NOTIFICATION_PREDELETE:
		if _is_hosting_process and _host_process_pid > 0:
			OS.kill(_host_process_pid)
		disconnect_network()

