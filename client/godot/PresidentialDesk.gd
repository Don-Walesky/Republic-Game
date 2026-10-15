extends Control

## UI Labels in the Presidential Desk
@onready var country_label: Label = get_node_or_null("VBoxContainer/CountryLabel")
@onready var treasury_label: Label = get_node_or_null("VBoxContainer/TreasuryLabel")
@onready var approval_label: Label = get_node_or_null("VBoxContainer/ApprovalLabel")
@onready var stability_label: Label = get_node_or_null("VBoxContainer/StabilityLabel")
@onready var simulation_time_label: Label = get_node_or_null("VBoxContainer/SimulationTimeLabel")
@onready var ruling_party_label: Label = get_node_or_null("VBoxContainer/RulingPartyLabel")
@onready var election_label: Label = get_node_or_null("VBoxContainer/ElectionLabel")
@onready var tax_rate_label: Label = get_node_or_null("VBoxContainer/TaxRateLabel")
@onready var status_label: Label = get_node_or_null("VBoxContainer/StatusLabel")

## Interactive Presidential Decision Controls
@onready var proposed_rate_spinbox: SpinBox = get_node_or_null("VBoxContainer/ProposedRateHBox/ProposedRateSpinBox")
@onready var expected_consequence_label: Label = get_node_or_null("VBoxContainer/ExpectedConsequenceLabel")
@onready var confirm_button: Button = get_node_or_null("VBoxContainer/DecisionControlsHBox/ConfirmButton")
@onready var cancel_button: Button = get_node_or_null("VBoxContainer/DecisionControlsHBox/CancelButton")
@onready var decision_result_label: Label = get_node_or_null("VBoxContainer/DecisionResultLabel")

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
var _request_wait_timer: float = 0.0


func _ready() -> void:
	_init_controls()
	_wire_signals()
	refresh()


## Initializes node references if not already bound
func _init_controls() -> void:
	if country_label == null:
		country_label = get_node_or_null("VBoxContainer/CountryLabel")
	if treasury_label == null:
		treasury_label = get_node_or_null("VBoxContainer/TreasuryLabel")
	if approval_label == null:
		approval_label = get_node_or_null("VBoxContainer/ApprovalLabel")
	if stability_label == null:
		stability_label = get_node_or_null("VBoxContainer/StabilityLabel")
	if simulation_time_label == null:
		simulation_time_label = get_node_or_null("VBoxContainer/SimulationTimeLabel")
	if ruling_party_label == null:
		ruling_party_label = get_node_or_null("VBoxContainer/RulingPartyLabel")
	if election_label == null:
		election_label = get_node_or_null("VBoxContainer/ElectionLabel")
	if tax_rate_label == null:
		tax_rate_label = get_node_or_null("VBoxContainer/TaxRateLabel")
	if status_label == null:
		status_label = get_node_or_null("VBoxContainer/StatusLabel")

	if proposed_rate_spinbox == null:
		proposed_rate_spinbox = get_node_or_null("VBoxContainer/ProposedRateHBox/ProposedRateSpinBox")
	if expected_consequence_label == null:
		expected_consequence_label = get_node_or_null("VBoxContainer/ExpectedConsequenceLabel")
	if confirm_button == null:
		confirm_button = get_node_or_null("VBoxContainer/DecisionControlsHBox/ConfirmButton")
	if cancel_button == null:
		cancel_button = get_node_or_null("VBoxContainer/DecisionControlsHBox/CancelButton")
	if decision_result_label == null:
		decision_result_label = get_node_or_null("VBoxContainer/DecisionResultLabel")


## Connects interactive control signals
func _wire_signals() -> void:
	if proposed_rate_spinbox and not proposed_rate_spinbox.value_changed.is_connected(_on_proposed_rate_changed):
		proposed_rate_spinbox.value_changed.connect(_on_proposed_rate_changed)
	if confirm_button and not confirm_button.pressed.is_connected(_on_confirm_button_pressed):
		confirm_button.pressed.connect(_on_confirm_button_pressed)
	if cancel_button and not cancel_button.pressed.is_connected(_on_cancel_button_pressed):
		cancel_button.pressed.connect(_on_cancel_button_pressed)


func _process(delta: float) -> void:
	_poll_timer += delta
	if _poll_timer >= poll_interval:
		_poll_timer = 0.0
		_check_for_live_updates()

	if _is_waiting_for_decision:
		_request_wait_timer += delta
		if _request_wait_timer >= REQUEST_TIMEOUT_SECONDS:
			_is_waiting_for_decision = false
			if not _pending_request_path.is_empty() and FileAccess.file_exists(_pending_request_path):
				if status_label:
					status_label.text = "Status: Warning - Decision request written, but no updated simulation state arrived within %.0fs. Is Republic simulation running?" % REQUEST_TIMEOUT_SECONDS


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
	_is_waiting_for_decision = false
	_request_wait_timer = 0.0
	var country_name: String = str(data.get("country_name", data.get("CountryName", "[Unknown]")))
	var treasury: String = str(data.get("treasury", data.get("Treasury", "[Unknown]")))
	var approval: String = str(data.get("approval", data.get("Approval", "[Unknown]")))
	var stability: String = str(data.get("stability", data.get("Stability", "[Unknown]")))
	var sim_time: String = str(data.get("simulation_time", data.get("SimulationTime", "[Unknown]")))
	var ruling_party: String = str(data.get("ruling_party", data.get("RulingParty", "None")))
	var election: String = str(data.get("election", data.get("Election", "No election")))
	var status_msg: String = str(data.get("status_message", data.get("StatusMessage", "Connected to live Republic simulation state")))

	var tax_rate_fmt: String = str(data.get("income_tax_rate_formatted", data.get("IncomeTaxRateFormatted", "")))
	var raw_tax_rate: float = float(data.get("income_tax_rate", data.get("IncomeTaxRate", 0.25)))
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
		status_label.text = "Status: " + status_msg

	var current_rate_pct := raw_tax_rate * 100.0

	# Only synchronize spinbox if player is not currently typing / modifying
	if proposed_rate_spinbox and not _user_is_adjusting:
		proposed_rate_spinbox.set_value_no_signal(current_rate_pct)
		_update_expected_consequence_display(current_rate_pct)

	var latest_result: String = str(data.get("latest_decision_result", data.get("LatestDecisionResult", "")))
	if not latest_result.is_empty() and decision_result_label:
		decision_result_label.text = "Decision Result: " + latest_result
	else:
		var result_data := _read_result_file()
		if not result_data.is_empty() and decision_result_label:
			var res_msg := str(result_data.get("message", ""))
			if not res_msg.is_empty():
				decision_result_label.text = "Decision Result: " + res_msg


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
		status_label.text = "Status: Waiting for Republic simulation state (" + details + ")"
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: [No State Loaded]"
	if decision_result_label:
		decision_result_label.text = "Decision Result: [No State Loaded]"


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
		status_label.text = "Status: Error - " + error_msg
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: Error"
	if decision_result_label:
		decision_result_label.text = "Decision Result: Error - " + error_msg


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


## Updates expected consequence label preview using authoritative Phase 11 feedback rules
func _update_expected_consequence_display(proposed_rate_pct: float) -> void:
	if expected_consequence_label == null:
		return
	var current_rate_pct: float = float(_latest_state.get("income_tax_rate", 0.25)) * 100.0
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
	var rate_val := proposed_rate_spinbox.value if proposed_rate_spinbox else 25.0
	execute_tax_decision(rate_val, true)


## Handler for cancel decision button
func _on_cancel_button_pressed() -> void:
	var rate_val := proposed_rate_spinbox.value if proposed_rate_spinbox else 25.0
	execute_tax_decision(rate_val, false)


## Submits a presidential taxation decision via file-based request (presidential_desk_request.json)
func execute_tax_decision(proposed_rate_percent: float, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false

	# 1. Reject if no valid simulation state is connected
	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot submit decision: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + no_state_msg
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

	# 3. Resolve paths
	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)

	# 4. Write presidential_desk_request.json next to state file
	var proposed_rate_decimal: float = proposed_rate_percent / 100.0
	var request_dict := {
		"proposed_rate": proposed_rate_decimal,
		"proposed_rate_percent": proposed_rate_percent,
		"confirmed": confirmed,
		"requested_at": Time.get_datetime_string_from_system()
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create decision request file: " + request_path
		if status_label:
			status_label.text = "Status: Error - " + err_msg
		if decision_result_label:
			decision_result_label.text = "Decision Result: Error - " + err_msg
		return {
			"success": false,
			"result": "Error",
			"message": err_msg
		}

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	# 5. UI feedback for confirmed vs cancelled
	if not confirmed:
		var cancel_msg: String = "Decision cancelled: Existing tax rates and citizen approval preserved unchanged."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + cancel_msg
		if status_label:
			status_label.text = "Status: Cancellation request written to " + DEFAULT_REQUEST_FILENAME
		# Reset spinbox back to current live rate
		var current_rate: float = float(_latest_state.get("income_tax_rate", 0.25)) * 100.0
		if proposed_rate_spinbox:
			proposed_rate_spinbox.set_value_no_signal(current_rate)
			_update_expected_consequence_display(current_rate)
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	var submit_msg := "Proposed %.1f%% tax submitted. Awaiting simulation..." % proposed_rate_percent
	if decision_result_label:
		decision_result_label.text = "Decision Result: " + submit_msg
	if status_label:
		status_label.text = "Status: Decision request written to " + DEFAULT_REQUEST_FILENAME

	return {
		"success": true,
		"result": "Pending",
		"message": submit_msg
	}


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


## Reads structured action result from presidential_desk_result.json
func _read_result_file() -> Dictionary:
	var result_candidates: Array[String] = []
	if not _active_state_path.is_empty():
		var dir := _active_state_path.get_base_dir()
		result_candidates.append(dir + "/presidential_desk_result.json")
	result_candidates.append("res://presidential_desk_result.json")
	result_candidates.append("user://presidential_desk_result.json")

	for path in result_candidates:
		if FileAccess.file_exists(path):
			var file := FileAccess.open(path, FileAccess.READ)
			if file:
				var parsed: Variant = JSON.parse_string(file.get_as_text())
				file.close()
				if parsed is Dictionary:
					return parsed
	return {}
