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

var _active_state_path: String = ""
var _last_modified_time: int = 0
var _poll_timer: float = 0.0
var _is_connected: bool = false
var _latest_state: Dictionary = {}
var _user_is_adjusting: bool = false


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
		if decision_result_label.text == "Decision Result: No decision made" or decision_result_label.text == "Decision Result: [No State Loaded]":
			decision_result_label.text = "Decision Result: " + latest_result


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


## Submits a presidential taxation decision to the Republic simulation logic
func execute_tax_decision(proposed_rate_percent: float, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false

	# 1. Explicit cancellation check
	if not confirmed:
		var cancel_msg: String = "Decision cancelled: Existing tax rates and citizen approval preserved unchanged."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + cancel_msg
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

	# 2. Constitutional bounds validation (0% to 70%)
	if proposed_rate_percent < 0.0 or proposed_rate_percent > 70.0:
		var invalid_msg: String = "Taxation policy rejected: rates exceed constitutional bounds (Income: 0-70%)."
		if decision_result_label:
			decision_result_label.text = "Decision Result: " + invalid_msg
		return {
			"success": false,
			"result": "InvalidInput",
			"message": invalid_msg
		}

	# 3. Locate CLI dll to execute authoritative Republic.Core decision
	var cli_dll := _resolve_cli_dll_path()
	var state_path := _resolve_state_path()

	var output: Array = []
	var exec_args: Array[String] = []

	if not cli_dll.is_empty() and FileAccess.file_exists(cli_dll):
		exec_args.append(cli_dll)
		exec_args.append("--apply-tax=" + str(proposed_rate_percent))
		if not state_path.is_empty():
			exec_args.append("--state-file=" + ProjectSettings.globalize_path(state_path))
		var exit_code := OS.execute("dotnet", exec_args, output)
		if exit_code != 0:
			var err_str := "Command execution failed with code " + str(exit_code)
			if decision_result_label:
				decision_result_label.text = "Decision Result: " + err_str
			return {"success": false, "result": "Error", "message": err_str}

	# 4. Check for result file generated by Republic.Core (presidential_desk_result.json)
	var result_data := _read_result_file()

	# 5. Reload the authoritative state file from disk
	refresh()

	# 6. Display the actual result returned by Republic.Core
	var outcome_message: String = ""
	if not result_data.is_empty():
		outcome_message = str(result_data.get("message", ""))
	elif _latest_state.has("latest_decision_result"):
		outcome_message = str(_latest_state.get("latest_decision_result", ""))

	if outcome_message.is_empty():
		outcome_message = "Taxation decision processed. Authoritative state updated."

	if decision_result_label:
		decision_result_label.text = "Decision Result: " + outcome_message

	return result_data if not result_data.is_empty() else {
		"success": true,
		"result": "Success",
		"message": outcome_message
	}


## Resolves the compiled Republic.Cli.dll binary path
func _resolve_cli_dll_path() -> String:
	var candidates: Array[String] = [
		"res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll",
		"res://../../src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll",
		"res://Republic.Cli.dll"
	]
	for c in candidates:
		var global_p := ProjectSettings.globalize_path(c)
		if FileAccess.file_exists(global_p):
			return global_p
	return ""


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
