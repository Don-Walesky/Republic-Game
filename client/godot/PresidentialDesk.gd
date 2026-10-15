extends Control

## UI Labels in the Presidential Desk
@onready var country_label: Label = $VBoxContainer/CountryLabel
@onready var treasury_label: Label = $VBoxContainer/TreasuryLabel
@onready var approval_label: Label = $VBoxContainer/ApprovalLabel
@onready var stability_label: Label = get_node_or_null("VBoxContainer/StabilityLabel")
@onready var simulation_time_label: Label = get_node_or_null("VBoxContainer/SimulationTimeLabel")
@onready var ruling_party_label: Label = $VBoxContainer/RulingPartyLabel
@onready var election_label: Label = $VBoxContainer/ElectionLabel
@onready var status_label: Label = get_node_or_null("VBoxContainer/StatusLabel")

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


func _ready() -> void:
	_init_labels()
	refresh()


## Initializes node references if not already bound
func _init_labels() -> void:
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
	if status_label == null:
		status_label = get_node_or_null("VBoxContainer/StatusLabel")


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
	_init_labels()
	var country_name: String = str(data.get("country_name", data.get("CountryName", "[Unknown]")))
	var treasury: String = str(data.get("treasury", data.get("Treasury", "[Unknown]")))
	var approval: String = str(data.get("approval", data.get("Approval", "[Unknown]")))
	var stability: String = str(data.get("stability", data.get("Stability", "[Unknown]")))
	var sim_time: String = str(data.get("simulation_time", data.get("SimulationTime", "[Unknown]")))
	var ruling_party: String = str(data.get("ruling_party", data.get("RulingParty", "None")))
	var election: String = str(data.get("election", data.get("Election", "No election")))
	var status_msg: String = str(data.get("status_message", data.get("StatusMessage", "Connected to live Republic simulation state")))

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
	if status_label:
		status_label.text = "Status: " + status_msg


## Visually displays a missing state notice without falling back to sample demonstration data
func display_missing_state(details: String = "") -> void:
	_init_labels()
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
	if status_label:
		status_label.text = "Status: Waiting for Republic simulation state (" + details + ")"


## Visually displays an error condition without falling back to sample demonstration data
func display_error(error_msg: String) -> void:
	_init_labels()
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
	if status_label:
		status_label.text = "Status: Error - " + error_msg


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
