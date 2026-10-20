extends Control

var _warned_missing_nodes: Dictionary = {}

## Retrieves a required node or pushes a one-time warning to the Godot Output panel
func _get_required_node(node_path: String) -> Node:
	var n := get_node_or_null(node_path)
	if n == null:
		var node_name := node_path.get_file()
		n = find_child(node_name, true, false)
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
@onready var active_project_label: Label = _get_required_node("VBoxContainer/ActiveProjectLabel") as Label
@onready var status_label: Label = _get_required_node("VBoxContainer/StatusLabel") as Label

## National Briefing Panel
@onready var briefing_treasury_label: Label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingTreasuryLabel") as Label
@onready var briefing_approval_label: Label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingApprovalLabel") as Label
@onready var briefing_stability_label: Label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingStabilityLabel") as Label
@onready var briefing_active_project_label: Label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingActiveProjectLabel") as Label
@onready var briefing_latest_decision_label: Label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingLatestDecisionLabel") as Label

## Country Setup & Save Management Controls
@onready var country_name_line_edit: LineEdit = _get_required_node("VBoxContainer/CountrySetupHBox/CountryNameLineEdit") as LineEdit
@onready var found_country_button: Button = _get_required_node("VBoxContainer/CountrySetupHBox/FoundCountryButton") as Button
@onready var save_path_line_edit: LineEdit = _get_required_node("VBoxContainer/CountrySetupHBox/SavePathLineEdit") as LineEdit
@onready var resume_save_button: Button = _get_required_node("VBoxContainer/CountrySetupHBox/ResumeSaveButton") as Button
@onready var country_setup_status_label: Label = _get_required_node("VBoxContainer/CountrySetupStatusLabel") as Label

## Interactive Presidential Decision Controls
@onready var proposed_rate_spinbox: SpinBox = _get_required_node("VBoxContainer/ProposedRateHBox/ProposedRateSpinBox") as SpinBox
@onready var expected_consequence_label: Label = _get_required_node("VBoxContainer/ExpectedConsequenceLabel") as Label
@onready var confirm_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/ConfirmButton") as Button
@onready var cancel_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/CancelButton") as Button
@onready var refresh_button: Button = _get_required_node("VBoxContainer/DecisionControlsHBox/RefreshButton") as Button
@onready var decision_result_label: Label = _get_required_node("VBoxContainer/DecisionResultLabel") as Label

## National Project Decision Controls
@onready var start_road_project_button: Button = _get_required_node("VBoxContainer/ProjectControlsHBox/StartRoadProjectButton") as Button
@onready var start_university_project_button: Button = _get_required_node("StartUniversityProjectButton") as Button
@onready var cancel_project_button: Button = _get_required_node("VBoxContainer/ProjectControlsHBox/CancelProjectButton") as Button
@onready var project_status_label: Label = _get_required_node("VBoxContainer/ProjectStatusLabel") as Label
@onready var fiscal_report_label: Label = _get_required_node("FiscalReportLabel") as Label
@onready var state_capacity_label: Label = _get_required_node("StateCapacityLabel") as Label
@onready var gdp_label: Label = _get_required_node("GdpLabel") as Label

## Campaign & Election Decision Controls
@onready var hold_campaign_rally_button: Button = _get_required_node("VBoxContainer/ElectionControlsHBox/HoldCampaignRallyButton") as Button
@onready var hold_town_hall_button: Button = _get_required_node("HoldTownHallButton") as Button
@onready var contest_election_button: Button = _get_required_node("VBoxContainer/ElectionControlsHBox/ContestElectionButton") as Button
@onready var cancel_election_action_button: Button = _get_required_node("VBoxContainer/ElectionControlsHBox/CancelElectionActionButton") as Button
@onready var election_status_label: Label = _get_required_node("VBoxContainer/ElectionStatusLabel") as Label

## Executive Cabinet Decision Controls
@onready var cabinet_overview_label: Label = _get_required_node("VBoxContainer/CabinetOverviewLabel") as Label
@onready var cabinet_summary_label: Label = _get_required_node("VBoxContainer/CabinetSummaryLabel") as Label
@onready var appoint_technocrat_finance_button: Button = _get_required_node("VBoxContainer/CabinetControlsHBox/AppointTechnocratFinanceButton") as Button
@onready var appoint_loyalist_finance_button: Button = _get_required_node("VBoxContainer/CabinetControlsHBox/AppointLoyalistFinanceButton") as Button
@onready var appoint_technocrat_interior_button: Button = _get_required_node("AppointTechnocratInteriorButton") as Button
@onready var appoint_loyalist_interior_button: Button = _get_required_node("AppointLoyalistInteriorButton") as Button
@onready var cancel_cabinet_action_button: Button = _get_required_node("VBoxContainer/CabinetControlsHBox/CancelCabinetActionButton") as Button
@onready var cabinet_status_label: Label = _get_required_node("VBoxContainer/CabinetStatusLabel") as Label

## Phase 3 Crisis & Emergency Decrees
@onready var crisis_status_label: Label = _get_required_node("CrisisStatusLabel") as Label
@onready var emergency_austerity_button: Button = _get_required_node("EmergencyAusterityButton") as Button
@onready var emergency_relief_button: Button = _get_required_node("EmergencyReliefButton") as Button

## Defense Readiness & Offline Catch-Up
@onready var defense_readiness_label: Label = _get_required_node("DefenseReadinessLabel") as Label
@onready var offline_briefing_label: Label = _get_required_node("OfflineBriefingLabel") as Label

## Multiplayer Networking Controls & Display
@onready var host_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/HostButton") as Button
@onready var join_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/JoinButton") as Button
@onready var address_line_edit: LineEdit = _get_required_node("VBoxContainer/MultiplayerControlsHBox/AddressLineEdit") as LineEdit
@onready var disconnect_button: Button = _get_required_node("VBoxContainer/MultiplayerControlsHBox/DisconnectButton") as Button
@onready var network_status_label: Label = _get_required_node("VBoxContainer/NetworkStatusLabel") as Label
@onready var multiplayer_status_label: Label = _get_required_node("VBoxContainer/NetworkStatusLabel") as Label
@onready var foreign_country_name_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryNameLabel") as Label
@onready var foreign_country_treasury_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryTreasuryLabel") as Label
@onready var foreign_country_approval_label: Label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryApprovalLabel") as Label
@onready var last_ai_decision_label: Label = _get_required_node("VBoxContainer/LastAiDecisionLabel") as Label
@onready var shared_clock_label: Label = _get_required_node("VBoxContainer/SharedClockLabel") as Label

## Bilateral Diplomacy & Trade Controls
@onready var diplomacy_status_label: Label = _get_required_node("VBoxContainer/DiplomacyControlsHBox/DiplomacyStatusLabel") as Label
@onready var recognize_button: Button = _get_required_node("VBoxContainer/DiplomacyControlsHBox/RecognizeButton") as Button
@onready var message_line_edit: LineEdit = _get_required_node("VBoxContainer/MessageHBox/MessageLineEdit") as LineEdit
@onready var send_message_button: Button = _get_required_node("VBoxContainer/MessageHBox/SendMessageButton") as Button
@onready var latest_message_label: Label = _get_required_node("VBoxContainer/LatestMessageLabel") as Label
@onready var trade_amount_spinbox: SpinBox = _get_required_node("VBoxContainer/TradeControlsHBox/TradeAmountSpinBox") as SpinBox
@onready var propose_trade_button: Button = _get_required_node("VBoxContainer/TradeControlsHBox/ProposeTradeButton") as Button
@onready var accept_trade_button: Button = _get_required_node("VBoxContainer/TradeControlsHBox/AcceptTradeButton") as Button
@onready var reject_trade_button: Button = _get_required_node("VBoxContainer/TradeControlsHBox/RejectTradeButton") as Button
@onready var latest_trade_status_label: Label = _get_required_node("VBoxContainer/LatestTradeStatusLabel") as Label
@onready var trade_lifecycle_stage_label: Label = _get_required_node("TradeLifecycleStageLabel") as Label
@onready var trade_recipient_input: LineEdit = _get_required_node("TradeRecipientInput") as LineEdit

## Phase 5 Monetization & Ad Placeholders
@onready var buy_admin_buff_button: Button = _get_required_node("VBoxContainer/BuffsHBox/AdminBuffPanel/AdminBuffVBox/BuyAdminBuffButton") as Button
@onready var buy_investor_buff_button: Button = _get_required_node("VBoxContainer/BuffsHBox/InvestorBuffPanel/InvestorBuffVBox/BuyInvestorBuffButton") as Button
@onready var buff_status_label: Label = _get_required_node("VBoxContainer/BuffStatusLabel") as Label
@onready var ad_placeholder: ColorRect = _get_required_node("VBoxContainer/AdPlaceholder") as ColorRect

## Navigation Sections
enum Section { OVERVIEW, CABINET, ECONOMY, CAMPAIGN, DIPLOMACY, BRIEFINGS }
var active_section: Section = Section.OVERVIEW

## Navigation Tab Controls
@onready var switch_to_executive_office_button: Button = _get_required_node("SwitchToExecutiveOfficeButton") as Button
@onready var nav_overview_button: Button = _get_required_node("NavOverviewButton") as Button
@onready var nav_cabinet_button: Button = _get_required_node("NavCabinetButton") as Button
@onready var nav_economy_button: Button = _get_required_node("NavEconomyButton") as Button
@onready var nav_campaign_button: Button = _get_required_node("NavCampaignButton") as Button
@onready var nav_diplomacy_button: Button = _get_required_node("NavDiplomacyButton") as Button
@onready var nav_briefings_button: Button = _get_required_node("NavBriefingsButton") as Button

## Section Root Containers
@onready var section_overview: Control = _get_required_node("SectionOverview") as Control
@onready var section_cabinet: Control = _get_required_node("SectionCabinet") as Control
@onready var section_economy: Control = _get_required_node("SectionEconomy") as Control
@onready var section_campaign: Control = _get_required_node("SectionCampaign") as Control
@onready var section_diplomacy: Control = _get_required_node("SectionDiplomacy") as Control
@onready var section_briefings: Control = _get_required_node("SectionBriefings") as Control

## Quick Jump / Return Buttons
@onready var jump_cabinet_button: Button = _get_required_node("JumpToCabinetButton") as Button
@onready var jump_economy_button: Button = _get_required_node("JumpToEconomyButton") as Button
@onready var jump_campaign_button: Button = _get_required_node("JumpToCampaignButton") as Button
@onready var jump_diplomacy_button: Button = _get_required_node("JumpToDiplomacyButton") as Button
@onready var jump_briefings_button: Button = _get_required_node("JumpToBriefingsButton") as Button

@onready var return_cabinet_button: Button = _get_required_node("ReturnToOverviewButton_Cabinet") as Button
@onready var return_economy_button: Button = _get_required_node("ReturnToOverviewButton_Economy") as Button
@onready var return_campaign_button: Button = _get_required_node("ReturnToOverviewButton_Campaign") as Button
@onready var return_diplomacy_button: Button = _get_required_node("ReturnToOverviewButton_Diplomacy") as Button
@onready var return_briefings_button: Button = _get_required_node("ReturnToOverviewButton_Briefings") as Button

## File paths scanned for authoritative Republic simulation state
const DEFAULT_STATE_PATHS: Array[String] = [
	"res://presidential_desk_state.json",
	"user://presidential_desk_state.json"
]

## Polling interval in seconds to detect file-based state changes
@export var poll_interval: float = 0.5

const DEFAULT_REQUEST_FILENAME: String = "presidential_desk_request.json"
const DEFAULT_BUFF_REQUEST_FILENAME: String = "presidential_desk_buff_request.json"
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

## Bilateral diplomacy and trade state
var _is_recognized: bool = false
var _active_trade_proposal: Dictionary = {}


func _ready() -> void:
	if _local_client_id.is_empty():
		_local_client_id = "client_" + str(Time.get_ticks_msec())
	for arg in OS.get_cmdline_args():
		if arg.begins_with("--client-id="):
			_local_client_id = arg.substr("--client-id=".length()).strip_edges()
	_init_controls()
	_wire_signals()
	switch_to_section(Section.OVERVIEW)
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
	if multiplayer_status_label == null:
		multiplayer_status_label = network_status_label
	if foreign_country_name_label == null:
		foreign_country_name_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryNameLabel") as Label
	if foreign_country_treasury_label == null:
		foreign_country_treasury_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryTreasuryLabel") as Label
	if foreign_country_approval_label == null:
		foreign_country_approval_label = _get_required_node("VBoxContainer/ForeignCountryGrid/ForeignCountryApprovalLabel") as Label
	if last_ai_decision_label == null:
		last_ai_decision_label = _get_required_node("VBoxContainer/LastAiDecisionLabel") as Label
	if shared_clock_label == null:
		shared_clock_label = _get_required_node("VBoxContainer/SharedClockLabel") as Label

	if diplomacy_status_label == null:
		diplomacy_status_label = _get_required_node("VBoxContainer/DiplomacyControlsHBox/DiplomacyStatusLabel") as Label
	if recognize_button == null:
		recognize_button = _get_required_node("VBoxContainer/DiplomacyControlsHBox/RecognizeButton") as Button
	if message_line_edit == null:
		message_line_edit = _get_required_node("VBoxContainer/MessageHBox/MessageLineEdit") as LineEdit
	if send_message_button == null:
		send_message_button = _get_required_node("VBoxContainer/MessageHBox/SendMessageButton") as Button
	if latest_message_label == null:
		latest_message_label = _get_required_node("VBoxContainer/LatestMessageLabel") as Label
	if trade_amount_spinbox == null:
		trade_amount_spinbox = _get_required_node("VBoxContainer/TradeControlsHBox/TradeAmountSpinBox") as SpinBox
	if propose_trade_button == null:
		propose_trade_button = _get_required_node("VBoxContainer/TradeControlsHBox/ProposeTradeButton") as Button
	if accept_trade_button == null:
		accept_trade_button = _get_required_node("VBoxContainer/TradeControlsHBox/AcceptTradeButton") as Button
	if reject_trade_button == null:
		reject_trade_button = _get_required_node("VBoxContainer/TradeControlsHBox/RejectTradeButton") as Button
	if latest_trade_status_label == null:
		latest_trade_status_label = _get_required_node("VBoxContainer/LatestTradeStatusLabel") as Label
	if trade_lifecycle_stage_label == null:
		trade_lifecycle_stage_label = _get_required_node("TradeLifecycleStageLabel") as Label
	if trade_recipient_input == null:
		trade_recipient_input = _get_required_node("TradeRecipientInput") as LineEdit
	if trade_recipient_input == null:
		trade_recipient_input = LineEdit.new()
	if defense_readiness_label == null:
		defense_readiness_label = _get_required_node("DefenseReadinessLabel") as Label
	if offline_briefing_label == null:
		offline_briefing_label = _get_required_node("OfflineBriefingLabel") as Label

	if buy_admin_buff_button == null:
		buy_admin_buff_button = _get_required_node("VBoxContainer/BuffsHBox/AdminBuffPanel/AdminBuffVBox/BuyAdminBuffButton") as Button
	if buy_investor_buff_button == null:
		buy_investor_buff_button = _get_required_node("VBoxContainer/BuffsHBox/InvestorBuffPanel/InvestorBuffVBox/BuyInvestorBuffButton") as Button
	if buff_status_label == null:
		buff_status_label = _get_required_node("VBoxContainer/BuffStatusLabel") as Label
	if ad_placeholder == null:
		ad_placeholder = _get_required_node("VBoxContainer/AdPlaceholder") as ColorRect

	if active_project_label == null:
		active_project_label = _get_required_node("VBoxContainer/ActiveProjectLabel") as Label
	if briefing_treasury_label == null:
		briefing_treasury_label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingTreasuryLabel") as Label
	if briefing_approval_label == null:
		briefing_approval_label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingApprovalLabel") as Label
	if briefing_stability_label == null:
		briefing_stability_label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingStabilityLabel") as Label
	if briefing_active_project_label == null:
		briefing_active_project_label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingGrid/BriefingActiveProjectLabel") as Label
	if briefing_latest_decision_label == null:
		briefing_latest_decision_label = _get_required_node("VBoxContainer/NationalBriefingPanel/NationalBriefingVBox/BriefingLatestDecisionLabel") as Label

	if country_name_line_edit == null:
		country_name_line_edit = _get_required_node("VBoxContainer/CountrySetupHBox/CountryNameLineEdit") as LineEdit
	if found_country_button == null:
		found_country_button = _get_required_node("VBoxContainer/CountrySetupHBox/FoundCountryButton") as Button
	if save_path_line_edit == null:
		save_path_line_edit = _get_required_node("VBoxContainer/CountrySetupHBox/SavePathLineEdit") as LineEdit
	if resume_save_button == null:
		resume_save_button = _get_required_node("VBoxContainer/CountrySetupHBox/ResumeSaveButton") as Button
	if country_setup_status_label == null:
		country_setup_status_label = _get_required_node("VBoxContainer/CountrySetupStatusLabel") as Label

	if start_road_project_button == null:
		start_road_project_button = _get_required_node("VBoxContainer/ProjectControlsHBox/StartRoadProjectButton") as Button
	if start_university_project_button == null:
		start_university_project_button = _get_required_node("StartUniversityProjectButton") as Button
	if cancel_project_button == null:
		cancel_project_button = _get_required_node("VBoxContainer/ProjectControlsHBox/CancelProjectButton") as Button
	if project_status_label == null:
		project_status_label = _get_required_node("VBoxContainer/ProjectStatusLabel") as Label
	if fiscal_report_label == null:
		fiscal_report_label = _get_required_node("FiscalReportLabel") as Label
	if state_capacity_label == null:
		state_capacity_label = _get_required_node("StateCapacityLabel") as Label
	if gdp_label == null:
		gdp_label = _get_required_node("GdpLabel") as Label

	if hold_campaign_rally_button == null:
		hold_campaign_rally_button = _get_required_node("VBoxContainer/ElectionControlsHBox/HoldCampaignRallyButton") as Button
	if hold_town_hall_button == null:
		hold_town_hall_button = _get_required_node("HoldTownHallButton") as Button
	if contest_election_button == null:
		contest_election_button = _get_required_node("VBoxContainer/ElectionControlsHBox/ContestElectionButton") as Button
	if cancel_election_action_button == null:
		cancel_election_action_button = _get_required_node("VBoxContainer/ElectionControlsHBox/CancelElectionActionButton") as Button
	if election_status_label == null:
		election_status_label = _get_required_node("VBoxContainer/ElectionStatusLabel") as Label

	if cabinet_overview_label == null:
		cabinet_overview_label = _get_required_node("VBoxContainer/CabinetOverviewLabel") as Label
	if cabinet_summary_label == null:
		cabinet_summary_label = _get_required_node("VBoxContainer/CabinetSummaryLabel") as Label
	if appoint_technocrat_finance_button == null:
		appoint_technocrat_finance_button = _get_required_node("VBoxContainer/CabinetControlsHBox/AppointTechnocratFinanceButton") as Button
	if appoint_loyalist_finance_button == null:
		appoint_loyalist_finance_button = _get_required_node("VBoxContainer/CabinetControlsHBox/AppointLoyalistFinanceButton") as Button
	if appoint_technocrat_interior_button == null:
		appoint_technocrat_interior_button = _get_required_node("AppointTechnocratInteriorButton") as Button
	if appoint_loyalist_interior_button == null:
		appoint_loyalist_interior_button = _get_required_node("AppointLoyalistInteriorButton") as Button
	if cancel_cabinet_action_button == null:
		cancel_cabinet_action_button = _get_required_node("VBoxContainer/CabinetControlsHBox/CancelCabinetActionButton") as Button
	if cabinet_status_label == null:
		cabinet_status_label = _get_required_node("VBoxContainer/CabinetStatusLabel") as Label

	if crisis_status_label == null:
		crisis_status_label = _get_required_node("CrisisStatusLabel") as Label
	if emergency_austerity_button == null:
		emergency_austerity_button = _get_required_node("EmergencyAusterityButton") as Button
	if emergency_relief_button == null:
		emergency_relief_button = _get_required_node("EmergencyReliefButton") as Button

	if switch_to_executive_office_button == null:
		switch_to_executive_office_button = _get_required_node("SwitchToExecutiveOfficeButton") as Button
	if nav_overview_button == null:
		nav_overview_button = _get_required_node("NavOverviewButton") as Button
	if nav_cabinet_button == null:
		nav_cabinet_button = _get_required_node("NavCabinetButton") as Button
	if nav_economy_button == null:
		nav_economy_button = _get_required_node("NavEconomyButton") as Button
	if nav_campaign_button == null:
		nav_campaign_button = _get_required_node("NavCampaignButton") as Button
	if nav_diplomacy_button == null:
		nav_diplomacy_button = _get_required_node("NavDiplomacyButton") as Button
	if nav_briefings_button == null:
		nav_briefings_button = _get_required_node("NavBriefingsButton") as Button

	if section_overview == null:
		section_overview = _get_required_node("SectionOverview") as Control
	if section_cabinet == null:
		section_cabinet = _get_required_node("SectionCabinet") as Control
	if section_economy == null:
		section_economy = _get_required_node("SectionEconomy") as Control
	if section_campaign == null:
		section_campaign = _get_required_node("SectionCampaign") as Control
	if section_diplomacy == null:
		section_diplomacy = _get_required_node("SectionDiplomacy") as Control
	if section_briefings == null:
		section_briefings = _get_required_node("SectionBriefings") as Control

	if jump_cabinet_button == null:
		jump_cabinet_button = _get_required_node("JumpToCabinetButton") as Button
	if jump_economy_button == null:
		jump_economy_button = _get_required_node("JumpToEconomyButton") as Button
	if jump_campaign_button == null:
		jump_campaign_button = _get_required_node("JumpToCampaignButton") as Button
	if jump_diplomacy_button == null:
		jump_diplomacy_button = _get_required_node("JumpToDiplomacyButton") as Button
	if jump_briefings_button == null:
		jump_briefings_button = _get_required_node("JumpToBriefingsButton") as Button

	if return_cabinet_button == null:
		return_cabinet_button = _get_required_node("ReturnToOverviewButton_Cabinet") as Button
	if return_economy_button == null:
		return_economy_button = _get_required_node("ReturnToOverviewButton_Economy") as Button
	if return_campaign_button == null:
		return_campaign_button = _get_required_node("ReturnToOverviewButton_Campaign") as Button
	if return_diplomacy_button == null:
		return_diplomacy_button = _get_required_node("ReturnToOverviewButton_Diplomacy") as Button
	if return_briefings_button == null:
		return_briefings_button = _get_required_node("ReturnToOverviewButton_Briefings") as Button

	if not _is_network_connected and not _is_network_connecting:
		if host_button:
			host_button.disabled = false
		if join_button:
			join_button.disabled = false
		if disconnect_button:
			disconnect_button.disabled = true
		if recognize_button:
			recognize_button.disabled = true
		if send_message_button:
			send_message_button.disabled = true
		if propose_trade_button:
			propose_trade_button.disabled = true
		if accept_trade_button:
			accept_trade_button.disabled = true
		if reject_trade_button:
			reject_trade_button.disabled = true

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
		if found_country_button:
			found_country_button.disabled = false
		if resume_save_button:
			resume_save_button.disabled = false
		if start_road_project_button:
			start_road_project_button.disabled = false
		if start_university_project_button:
			start_university_project_button.disabled = false
		if cancel_project_button:
			cancel_project_button.disabled = false
		if hold_campaign_rally_button:
			hold_campaign_rally_button.disabled = false
		if hold_town_hall_button:
			hold_town_hall_button.disabled = false
		if contest_election_button:
			contest_election_button.disabled = false
		if cancel_election_action_button:
			cancel_election_action_button.disabled = false
		if appoint_technocrat_finance_button:
			appoint_technocrat_finance_button.disabled = false
		if appoint_loyalist_finance_button:
			appoint_loyalist_finance_button.disabled = false
		if appoint_technocrat_interior_button:
			appoint_technocrat_interior_button.disabled = false
		if appoint_loyalist_interior_button:
			appoint_loyalist_interior_button.disabled = false
		if cancel_cabinet_action_button:
			cancel_cabinet_action_button.disabled = false
		if emergency_austerity_button:
			emergency_austerity_button.disabled = false
		if emergency_relief_button:
			emergency_relief_button.disabled = false

	_setup_tooltips()
	_wire_signals()


## Configures rich, concise contextual tooltips explaining political/economic terminology, costs, and risks
func _setup_tooltips() -> void:
	if treasury_label:
		treasury_label.tooltip_text = "State Treasury: Total liquid sovereign reserves in Republic Currency (REPU). Used to fund infrastructure, cabinet appointments, campaign events, and emergency decrees."
	if approval_label:
		approval_label.tooltip_text = "Public Approval: Overall national confidence in your administration. Drops when taxes rise or crises go unaddressed. High approval aids election victories."
	if stability_label:
		stability_label.tooltip_text = "National Stability: Degree of public order and constitutional resilience. High stability safeguards government functions against civil unrest."
	if simulation_time_label:
		simulation_time_label.tooltip_text = "Simulation Time: Universal elapsed time in days and hours, authoritatively synchronized across host and clients."
	if ruling_party_label:
		ruling_party_label.tooltip_text = "Political Parties: Shows current polling support between the ruling administration coalition and the parliamentary opposition."
	if election_label:
		election_label.tooltip_text = "Electoral Mandate: Time remaining until the next mandatory election. Rallies boost polling; contesting elections renews your democratic mandate."
	if cabinet_summary_label:
		cabinet_summary_label.tooltip_text = "Executive Cabinet: Balances institutional competence against political loyalty. Technocrats elevate Civil Service capacity; Loyalists rally party support."
	if appoint_technocrat_finance_button:
		appoint_technocrat_finance_button.tooltip_text = "Appoint Technocrat Minister:\n• Cost: Zero upfront\n• Effect: Civil Service Quality jumps to 90%, lifting national yield and policy bottlenecks\n• Risk: Minor party friction"
	if appoint_loyalist_finance_button:
		appoint_loyalist_finance_button.tooltip_text = "Appoint Loyalist Minister:\n• Cost: Zero upfront\n• Effect: Polling support +4.0%, consolidating legislative majority\n• Risk: Civil Service Quality capped at 30%"
	if appoint_technocrat_interior_button:
		appoint_technocrat_interior_button.tooltip_text = "Appoint Technocrat Interior Minister (Dr. Finch):\n• Cost: Zero upfront\n• Effect: Corruption Control elevated to 90%, lifting national yield bottleneck\n• Risk: Faction dissent (-3% polling)"
	if appoint_loyalist_interior_button:
		appoint_loyalist_interior_button.tooltip_text = "Appoint Loyalist Interior Minister (Whip Sterling):\n• Cost: Zero upfront\n• Effect: Polling support +3.0%, party cohesion\n• Risk: Corruption Control capped at 45%"
	if hold_campaign_rally_button:
		hold_campaign_rally_button.tooltip_text = "Hold National Campaign Rally:\n• Cost: R 250,000\n• Effect: Polling support +4.0%, Approval +1.5%\n• Risk: Depletes treasury reserves if repeated excessively"
	if hold_town_hall_button:
		hold_town_hall_button.tooltip_text = "Hold Citizen Town Hall:\n• Cost: R 50,000\n• Effect: Public Approval +1.0%, Stability +1.5%, Polling +1.5%\n• Risk: Modest treasury expenditure"
	if contest_election_button:
		contest_election_button.tooltip_text = "Contest Democratic Election:\n• Cost: Zero\n• Effect: Renews presidential mandate (+2.5% Approval, +2.0% Stability) if polling exceeds opposition\n• Risk: Democratic defeat if opposition leads"
	if proposed_rate_spinbox:
		proposed_rate_spinbox.tooltip_text = "Income Tax Rate (%):\n• Baseline: 25.0%\n• Effect: Each 1% hike generates revenue but reduces Approval by 0.5% and Stability by 0.3%"
	if start_road_project_button:
		start_road_project_button.tooltip_text = "Commission National Road Program:\n• Cost: R 500,000\n• Effect: Enhances logistics infrastructure, accelerates National Yield and private sector productivity"
	if start_university_project_button:
		start_university_project_button.tooltip_text = "Commission National University:\n• Cost: R 750,000\n• Effect: Elevates Human Capital, State Capacity, and scientific innovation over 48h"
	if emergency_austerity_button:
		emergency_austerity_button.tooltip_text = "Emergency Austerity Decree:\n• Gain: +R 500,000 reserves from spending cuts\n• Risk: Public approval drops -4.0%, stability drops -2.0%"
	if emergency_relief_button:
		emergency_relief_button.tooltip_text = "Civic Relief Package:\n• Cost: R 100,000 treasury stimulus\n• Effect: Approval +4.0%, Stability +3.0%\n• Use: Resolves civil unrest emergencies"
	if cancel_button:
		cancel_button.tooltip_text = "Cancel Proposal: Dismisses the pending adjustment and reverts tax proposal to the current constitutional law without cost."
	if confirm_button:
		confirm_button.tooltip_text = "Confirm Executive Decree: Enacts the proposed income tax rate immediately into law, applying fiscal and approval consequences."
	if buy_admin_buff_button:
		buy_admin_buff_button.tooltip_text = "Phase 5 Sponsored Decree: Emergency administrative procurement via official commercial partners."
	if buy_investor_buff_button:
		buy_investor_buff_button.tooltip_text = "Phase 5 Sponsored Decree: Foreign direct investment promotion package via accredited industrial sponsors."
	if recognize_button:
		recognize_button.tooltip_text = "Diplomatic Recognition: Formal recognition of peer sovereign republic, establishing official embassy ties and unlocking bilateral trade."
	if propose_trade_button:
		propose_trade_button.tooltip_text = "Propose Bilateral Trade: Transfers REPU currency reserves to peer republic in exchange for reciprocal commodity flow."


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
	if found_country_button and not found_country_button.pressed.is_connected(_on_found_country_button_pressed):
		found_country_button.pressed.connect(_on_found_country_button_pressed)
	if resume_save_button and not resume_save_button.pressed.is_connected(_on_resume_save_button_pressed):
		resume_save_button.pressed.connect(_on_resume_save_button_pressed)
	if start_road_project_button and not start_road_project_button.pressed.is_connected(_on_start_road_project_button_pressed):
		start_road_project_button.pressed.connect(_on_start_road_project_button_pressed)
	if start_university_project_button and not start_university_project_button.pressed.is_connected(_on_start_university_project_button_pressed):
		start_university_project_button.pressed.connect(_on_start_university_project_button_pressed)
	if cancel_project_button and not cancel_project_button.pressed.is_connected(_on_cancel_project_button_pressed):
		cancel_project_button.pressed.connect(_on_cancel_project_button_pressed)
	if hold_campaign_rally_button and not hold_campaign_rally_button.pressed.is_connected(_on_hold_campaign_rally_button_pressed):
		hold_campaign_rally_button.pressed.connect(_on_hold_campaign_rally_button_pressed)
	if hold_town_hall_button and not hold_town_hall_button.pressed.is_connected(_on_hold_town_hall_button_pressed):
		hold_town_hall_button.pressed.connect(_on_hold_town_hall_button_pressed)
	if contest_election_button and not contest_election_button.pressed.is_connected(_on_contest_election_button_pressed):
		contest_election_button.pressed.connect(_on_contest_election_button_pressed)
	if cancel_election_action_button and not cancel_election_action_button.pressed.is_connected(_on_cancel_election_action_button_pressed):
		cancel_election_action_button.pressed.connect(_on_cancel_election_action_button_pressed)
	if appoint_technocrat_finance_button and not appoint_technocrat_finance_button.pressed.is_connected(_on_appoint_technocrat_finance_button_pressed):
		appoint_technocrat_finance_button.pressed.connect(_on_appoint_technocrat_finance_button_pressed)
	if appoint_loyalist_finance_button and not appoint_loyalist_finance_button.pressed.is_connected(_on_appoint_loyalist_finance_button_pressed):
		appoint_loyalist_finance_button.pressed.connect(_on_appoint_loyalist_finance_button_pressed)
	if appoint_technocrat_interior_button and not appoint_technocrat_interior_button.pressed.is_connected(_on_appoint_technocrat_interior_button_pressed):
		appoint_technocrat_interior_button.pressed.connect(_on_appoint_technocrat_interior_button_pressed)
	if appoint_loyalist_interior_button and not appoint_loyalist_interior_button.pressed.is_connected(_on_appoint_loyalist_interior_button_pressed):
		appoint_loyalist_interior_button.pressed.connect(_on_appoint_loyalist_interior_button_pressed)
	if cancel_cabinet_action_button and not cancel_cabinet_action_button.pressed.is_connected(_on_cancel_cabinet_action_button_pressed):
		cancel_cabinet_action_button.pressed.connect(_on_cancel_cabinet_action_button_pressed)
	if emergency_austerity_button and not emergency_austerity_button.pressed.is_connected(_on_emergency_austerity_button_pressed):
		emergency_austerity_button.pressed.connect(_on_emergency_austerity_button_pressed)
	if emergency_relief_button and not emergency_relief_button.pressed.is_connected(_on_emergency_relief_button_pressed):
		emergency_relief_button.pressed.connect(_on_emergency_relief_button_pressed)
	if host_button and not host_button.pressed.is_connected(_on_host_button_pressed):
		host_button.pressed.connect(_on_host_button_pressed)
	if join_button and not join_button.pressed.is_connected(_on_join_button_pressed):
		join_button.pressed.connect(_on_join_button_pressed)
	if disconnect_button and not disconnect_button.pressed.is_connected(_on_disconnect_button_pressed):
		disconnect_button.pressed.connect(_on_disconnect_button_pressed)
	if recognize_button and not recognize_button.pressed.is_connected(_on_recognize_button_pressed):
		recognize_button.pressed.connect(_on_recognize_button_pressed)
	if send_message_button and not send_message_button.pressed.is_connected(_on_send_message_button_pressed):
		send_message_button.pressed.connect(_on_send_message_button_pressed)
	if trade_amount_spinbox and not trade_amount_spinbox.value_changed.is_connected(_on_trade_amount_changed):
		trade_amount_spinbox.value_changed.connect(_on_trade_amount_changed)
	if propose_trade_button and not propose_trade_button.pressed.is_connected(_on_propose_trade_button_pressed):
		propose_trade_button.pressed.connect(_on_propose_trade_button_pressed)
	if accept_trade_button and not accept_trade_button.pressed.is_connected(_on_accept_trade_button_pressed):
		accept_trade_button.pressed.connect(_on_accept_trade_button_pressed)
	if reject_trade_button and not reject_trade_button.pressed.is_connected(_on_reject_trade_button_pressed):
		reject_trade_button.pressed.connect(_on_reject_trade_button_pressed)
	if buy_admin_buff_button and not buy_admin_buff_button.pressed.is_connected(_on_buy_admin_buff_pressed):
		buy_admin_buff_button.pressed.connect(_on_buy_admin_buff_pressed)
	if buy_investor_buff_button and not buy_investor_buff_button.pressed.is_connected(_on_buy_investor_buff_pressed):
		buy_investor_buff_button.pressed.connect(_on_buy_investor_buff_pressed)

	if nav_overview_button and not nav_overview_button.pressed.is_connected(_on_nav_overview_pressed):
		nav_overview_button.pressed.connect(_on_nav_overview_pressed)
	if nav_cabinet_button and not nav_cabinet_button.pressed.is_connected(_on_nav_cabinet_pressed):
		nav_cabinet_button.pressed.connect(_on_nav_cabinet_pressed)
	if nav_economy_button and not nav_economy_button.pressed.is_connected(_on_nav_economy_pressed):
		nav_economy_button.pressed.connect(_on_nav_economy_pressed)
	if nav_campaign_button and not nav_campaign_button.pressed.is_connected(_on_nav_campaign_pressed):
		nav_campaign_button.pressed.connect(_on_nav_campaign_pressed)
	if nav_diplomacy_button and not nav_diplomacy_button.pressed.is_connected(_on_nav_diplomacy_pressed):
		nav_diplomacy_button.pressed.connect(_on_nav_diplomacy_pressed)
	if nav_briefings_button and not nav_briefings_button.pressed.is_connected(_on_nav_briefings_pressed):
		nav_briefings_button.pressed.connect(_on_nav_briefings_pressed)

	if jump_cabinet_button and not jump_cabinet_button.pressed.is_connected(_on_nav_cabinet_pressed):
		jump_cabinet_button.pressed.connect(_on_nav_cabinet_pressed)
	if jump_economy_button and not jump_economy_button.pressed.is_connected(_on_nav_economy_pressed):
		jump_economy_button.pressed.connect(_on_nav_economy_pressed)
	if jump_campaign_button and not jump_campaign_button.pressed.is_connected(_on_nav_campaign_pressed):
		jump_campaign_button.pressed.connect(_on_nav_campaign_pressed)
	if jump_diplomacy_button and not jump_diplomacy_button.pressed.is_connected(_on_nav_diplomacy_pressed):
		jump_diplomacy_button.pressed.connect(_on_nav_diplomacy_pressed)
	if jump_briefings_button and not jump_briefings_button.pressed.is_connected(_on_nav_briefings_pressed):
		jump_briefings_button.pressed.connect(_on_nav_briefings_pressed)

	if return_cabinet_button and not return_cabinet_button.pressed.is_connected(return_to_overview):
		return_cabinet_button.pressed.connect(return_to_overview)
	if return_economy_button and not return_economy_button.pressed.is_connected(return_to_overview):
		return_economy_button.pressed.connect(return_to_overview)
	if return_campaign_button and not return_campaign_button.pressed.is_connected(return_to_overview):
		return_campaign_button.pressed.connect(return_to_overview)
	if return_diplomacy_button and not return_diplomacy_button.pressed.is_connected(return_to_overview):
		return_diplomacy_button.pressed.connect(return_to_overview)
	if return_briefings_button and not return_briefings_button.pressed.is_connected(return_to_overview):
		return_briefings_button.pressed.connect(return_to_overview)
	if switch_to_executive_office_button and not switch_to_executive_office_button.pressed.is_connected(_on_switch_to_executive_office_button_pressed):
		switch_to_executive_office_button.pressed.connect(_on_switch_to_executive_office_button_pressed)


func _on_switch_to_executive_office_button_pressed() -> void:
	get_tree().change_scene_to_file("res://ExecutiveOffice.tscn")


## Switches the active UI section and highlights navigation controls
func switch_to_section(section: Section) -> void:
	active_section = section
	if section_overview:
		section_overview.visible = (section == Section.OVERVIEW)
	if section_cabinet:
		section_cabinet.visible = (section == Section.CABINET)
	if section_economy:
		section_economy.visible = (section == Section.ECONOMY)
	if section_campaign:
		section_campaign.visible = (section == Section.CAMPAIGN)
	if section_diplomacy:
		section_diplomacy.visible = (section == Section.DIPLOMACY)
	if section_briefings:
		section_briefings.visible = (section == Section.BRIEFINGS)
	_update_nav_button_styles()


## Returns immediately to the main Presidential Overview section
func return_to_overview() -> void:
	switch_to_section(Section.OVERVIEW)


## Updates button highlights to provide clear visual feedback of current active section
func _update_nav_button_styles() -> void:
	var buttons := [nav_overview_button, nav_cabinet_button, nav_economy_button, nav_campaign_button, nav_diplomacy_button, nav_briefings_button]
	for i in range(buttons.size()):
		var b: Button = buttons[i]
		if b:
			if i == active_section:
				b.modulate = Color(1.0, 0.85, 0.3, 1.0) # Gold accent for active tab
			else:
				b.modulate = Color(0.85, 0.9, 0.95, 1.0)


func _on_nav_overview_pressed() -> void:
	switch_to_section(Section.OVERVIEW)

func _on_nav_cabinet_pressed() -> void:
	switch_to_section(Section.CABINET)

func _on_nav_economy_pressed() -> void:
	switch_to_section(Section.ECONOMY)

func _on_nav_campaign_pressed() -> void:
	switch_to_section(Section.CAMPAIGN)

func _on_nav_diplomacy_pressed() -> void:
	switch_to_section(Section.DIPLOMACY)

func _on_nav_briefings_pressed() -> void:
	switch_to_section(Section.BRIEFINGS)


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

	var opposition_party: String = str(data.get("opposition_party", data.get("OppositionParty", "None")))
	var polling_support: float = float(data.get("polling_support", data.get("PollingSupport", 0.0)))
	var opposition_support: float = float(data.get("opposition_support", data.get("OppositionSupport", 0.0)))
	var next_election_day: int = int(data.get("next_election_day", data.get("NextElectionDay", 0)))

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
		if opposition_party != "None" and not opposition_party.is_empty():
			ruling_party_label.text = "Ruling: %s (%.1f%%) | Opposition: %s (%.1f%%)" % [ruling_party, polling_support, opposition_party, opposition_support]
		else:
			ruling_party_label.text = "Ruling party: " + ruling_party
	if election_label:
		if next_election_day > 0:
			election_label.text = "Election: %s (Next Election: Day %d)" % [election, next_election_day]
		else:
			election_label.text = "Election: " + election
	if tax_rate_label:
		tax_rate_label.text = "Income Tax Rate: " + tax_rate_fmt

	var active_proj: String = str(data.get("active_project", data.get("ActiveProject", "None")))
	var latest_result: String = str(data.get("latest_decision_result", data.get("LatestDecisionResult", "")))
	var cabinet_sum: String = str(data.get("cabinet_summary", data.get("CabinetSummary", "Finance: Vacant | Interior: Vacant")))
	var civil_quality: float = float(data.get("civil_service_quality", data.get("CivilServiceQuality", 0.3)))
	var corr_control: float = float(data.get("corruption_control", data.get("CorruptionControl", 0.3)))

	var fisc_sum: String = str(data.get("fiscal_report_summary", data.get("FiscalReportSummary", "")))
	var fisc_def: String = str(data.get("fiscal_deficit_formatted", data.get("FiscalDeficitFormatted", "")))
	var gdp_val: String = str(data.get("gdp_formatted", data.get("GdpFormatted", "")))
	var cap_sum: String = str(data.get("state_capacity_summary", data.get("StateCapacitySummary", "")))
	var crisis_val: String = str(data.get("active_crisis", data.get("ActiveCrisis", "None")))
	var crisis_det: String = str(data.get("active_crisis_details", data.get("ActiveCrisisDetails", "National stability within safe limits")))

	if fiscal_report_label:
		if not fisc_sum.is_empty():
			fiscal_report_label.text = "Fiscal Report: %s (%s)" % [fisc_sum, fisc_def]
		else:
			fiscal_report_label.text = "Fiscal Report: [No Data]"
	if state_capacity_label:
		if not cap_sum.is_empty():
			state_capacity_label.text = "State Capacity: " + cap_sum
		else:
			state_capacity_label.text = "State Capacity: [No Data]"
	if gdp_label:
		if not gdp_val.is_empty():
			gdp_label.text = "GDP: " + gdp_val
		else:
			gdp_label.text = "GDP: [No Data]"
	if crisis_status_label:
		if crisis_val != "None" and not crisis_val.is_empty():
			crisis_status_label.text = "Active Crisis: %s — %s" % [crisis_val, crisis_det]
		else:
			crisis_status_label.text = "Active Crisis: None (National stability within safe limits)"

	if active_project_label:
		active_project_label.text = "Active Project: " + active_proj
	if cabinet_overview_label:
		cabinet_overview_label.text = "Cabinet: " + cabinet_sum
	if cabinet_summary_label:
		cabinet_summary_label.text = "Cabinet: %s (Civil Service: %.0f%%, Corruption Control: %.0f%%)" % [cabinet_sum, civil_quality * 100.0, corr_control * 100.0]
	if briefing_treasury_label:
		briefing_treasury_label.text = "Treasury: " + treasury
	if briefing_approval_label:
		briefing_approval_label.text = "Approval: " + approval
	if briefing_stability_label:
		briefing_stability_label.text = "Stability: " + stability
	if briefing_active_project_label:
		briefing_active_project_label.text = "Active Project: " + active_proj
	if briefing_latest_decision_label:
		if not latest_result.is_empty():
			briefing_latest_decision_label.text = "Latest Decision: " + latest_result
		else:
			briefing_latest_decision_label.text = "Latest Decision: [No decisions recorded]"

	var defcon: String = str(data.get("defcon_level", data.get("DefconLevel", "DEFCON 5 (Peacetime Watch)")))
	var mil_readiness: String = str(data.get("military_readiness", data.get("MilitaryReadiness", "85.0% (Armed Forces Operational)")))
	if defense_readiness_label:
		defense_readiness_label.text = "Defense Posture: %s | Armed Forces: %s" % [defcon, mil_readiness]

	var offline_briefing: String = str(data.get("offline_briefing_summary", data.get("OfflineBriefingSummary", "Offline Status: Real-time simulation aligned. Zero pending offline cycles.")))
	if offline_briefing_label:
		offline_briefing_label.text = offline_briefing

	if status_label:
		status_label.text = _format_status_text("Status: " + status_msg, "None")

	var last_ai_dec: String = str(data.get("last_ai_decision", data.get("LastAiDecision", "")))
	if last_ai_decision_label:
		if not last_ai_dec.is_empty():
			last_ai_decision_label.text = "Last AI Decision: " + last_ai_dec
		else:
			last_ai_decision_label.text = "Last AI Decision: [No AI decisions recorded]"

	var current_rate_pct := raw_tax_rate * 100.0

	# Only synchronize spinbox if player is not currently typing / modifying
	if proposed_rate_spinbox and not _user_is_adjusting:
		proposed_rate_spinbox.set_value_no_signal(current_rate_pct)
		_update_expected_consequence_display(current_rate_pct)

	# Only update decision result label if not actively waiting for a decision outcome
	if not was_waiting:
		if not latest_result.is_empty() and decision_result_label:
			decision_result_label.text = "Decision Result: " + latest_result
		else:
			var result_data := _read_result_file()
			if not result_data.is_empty() and decision_result_label:
				var res_msg := str(result_data.get("message", ""))
				if not res_msg.is_empty():
					decision_result_label.text = "Decision Result: " + res_msg

	# If connected to multiplayer network host, broadcast updated country snapshot and update status
	if _is_network_connected:
		_send_local_country_snapshot()
		_update_network_status_display()


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
	if active_project_label:
		active_project_label.text = "Active Project: [No State Loaded]"
	if fiscal_report_label:
		fiscal_report_label.text = "Fiscal Report: [No State Loaded]"
	if state_capacity_label:
		state_capacity_label.text = "State Capacity: [No State Loaded]"
	if gdp_label:
		gdp_label.text = "GDP: [No State Loaded]"
	if crisis_status_label:
		crisis_status_label.text = "Active Crisis: None"
	if cabinet_overview_label:
		cabinet_overview_label.text = "Cabinet: [No State Loaded]"
	if cabinet_summary_label:
		cabinet_summary_label.text = "Cabinet: [No State Loaded]"
	if briefing_treasury_label:
		briefing_treasury_label.text = "Treasury: [No State Loaded]"
	if briefing_approval_label:
		briefing_approval_label.text = "Approval: [No State Loaded]"
	if briefing_stability_label:
		briefing_stability_label.text = "Stability: [No State Loaded]"
	if briefing_active_project_label:
		briefing_active_project_label.text = "Active Project: [No State Loaded]"
	if briefing_latest_decision_label:
		briefing_latest_decision_label.text = "Latest Decision: [No State Loaded]"
	if defense_readiness_label:
		defense_readiness_label.text = "Defense Posture: DEFCON 5 (Peacetime Watch) | Armed Forces: 85.0% Operational"
	if offline_briefing_label:
		offline_briefing_label.text = "Offline Status: Real-time simulation aligned. Zero pending offline cycles."
	if trade_lifecycle_stage_label:
		trade_lifecycle_stage_label.text = "Trade Lifecycle: [Draft Proposal] — Configure amount above and propose to peer"
	if status_label:
		status_label.text = _format_status_text("Status: Waiting for Republic simulation state (" + details + ")", "None")
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: [No State Loaded]"
	if decision_result_label:
		decision_result_label.text = "Decision Result: [No State Loaded]"
	if buff_status_label:
		buff_status_label.text = "Buff Status: Ready (No active purchase requests)"

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
	if active_project_label:
		active_project_label.text = "Active Project: [No State Loaded]"
	if fiscal_report_label:
		fiscal_report_label.text = "Fiscal Report: Error"
	if state_capacity_label:
		state_capacity_label.text = "State Capacity: Error"
	if gdp_label:
		gdp_label.text = "GDP: Error"
	if crisis_status_label:
		crisis_status_label.text = "Active Crisis: Error - " + error_msg
	if briefing_treasury_label:
		briefing_treasury_label.text = "Treasury: [No State Loaded]"
	if briefing_approval_label:
		briefing_approval_label.text = "Approval: [No State Loaded]"
	if briefing_stability_label:
		briefing_stability_label.text = "Stability: [No State Loaded]"
	if briefing_active_project_label:
		briefing_active_project_label.text = "Active Project: [No State Loaded]"
	if briefing_latest_decision_label:
		briefing_latest_decision_label.text = "Latest Decision: Error - " + error_msg
	if status_label:
		status_label.text = _format_status_text("Status: Error - " + error_msg, "None")
	if expected_consequence_label:
		expected_consequence_label.text = "Expected Approval Effect: Error"
	if decision_result_label:
		decision_result_label.text = "Decision Result: Error - " + error_msg
	if last_ai_decision_label:
		last_ai_decision_label.text = "Last AI Decision: [No AI decisions recorded]"
	if buff_status_label:
		buff_status_label.text = "Buff Status: Ready (No active purchase requests)"

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


## Handler for found country button
func _on_found_country_button_pressed() -> void:
	var c_name := country_name_line_edit.text.strip_edges() if country_name_line_edit else ""
	execute_create_country(c_name)


## Handler for resume save button
func _on_resume_save_button_pressed() -> void:
	var s_path := save_path_line_edit.text.strip_edges() if save_path_line_edit else ""
	execute_load_save(s_path)


## Handler for start road project button
func _on_start_road_project_button_pressed() -> void:
	execute_project_decision("Road", true)


## Handler for cancel project button
func _on_cancel_project_button_pressed() -> void:
	execute_project_decision("Road", false)


## Handler for hold campaign rally button
func _on_hold_campaign_rally_button_pressed() -> void:
	execute_campaign_rally(true)


## Handler for contest election button
func _on_contest_election_button_pressed() -> void:
	execute_contest_election(true)


## Handler for cancel election action button
func _on_cancel_election_action_button_pressed() -> void:
	execute_campaign_rally(false)


## Handler for appointing technocrat finance minister
func _on_appoint_technocrat_finance_button_pressed() -> void:
	execute_cabinet_appointment("Finance", "technocrat", true)


## Handler for appointing party loyalist finance minister
func _on_appoint_loyalist_finance_button_pressed() -> void:
	execute_cabinet_appointment("Finance", "loyalist", true)


## Handler for appointing technocrat interior minister
func _on_appoint_technocrat_interior_button_pressed() -> void:
	execute_cabinet_appointment("Interior", "technocrat", true)


## Handler for appointing party loyalist interior minister
func _on_appoint_loyalist_interior_button_pressed() -> void:
	execute_cabinet_appointment("Interior", "loyalist", true)


## Handler for start university project button
func _on_start_university_project_button_pressed() -> void:
	execute_project_decision("University", true)


## Handler for hold citizen town hall button
func _on_hold_town_hall_button_pressed() -> void:
	execute_town_hall(true)


## Handler for emergency austerity decree button
func _on_emergency_austerity_button_pressed() -> void:
	execute_crisis_response("austerity", true)


## Handler for civic relief package button
func _on_emergency_relief_button_pressed() -> void:
	execute_crisis_response("civic_relief", true)


## Handler for cancelling cabinet action order
func _on_cancel_cabinet_action_button_pressed() -> void:
	execute_cabinet_appointment("Finance", "technocrat", false)


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

	if _is_network_connected:
		var tax_net_msg := {
			"type": "tax_decision",
			"sender_id": _local_client_id,
			"country_id": _local_client_id,
			"tax_rate": proposed_rate_percent / 100.0,
			"request_id": _active_request_id
		}
		_send_network_message(tax_net_msg)

	var submit_msg := "Proposed %.1f%% tax submitted. Awaiting simulation..." % proposed_rate_percent
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Decision request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return {
		"success": true,
		"result": "Pending",
		"message": submit_msg
	}


## Submits a request to create/found a sovereign country
func execute_create_country(c_name: String) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if c_name.is_empty():
		var empty_msg := "Country name cannot be empty."
		if country_setup_status_label:
			country_setup_status_label.text = "Founder Status: [Rejected] " + empty_msg
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Invalid Input - " + empty_msg
		return {
			"success": false,
			"result": "InvalidInput",
			"message": empty_msg
		}

	var regex := RegEx.new()
	regex.compile("(?i)\\bRepublic\\b")
	if not regex.search(c_name):
		var name_msg := "Country name '%s' is invalid. Every player country name must contain the whole word 'Republic'." % c_name
		if country_setup_status_label:
			country_setup_status_label.text = "Founder Status: [Rejected] " + name_msg
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Invalid Name - " + name_msg
		return {
			"success": false,
			"result": "InvalidName",
			"message": name_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_found_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "create_country",
		"country_name": c_name,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if found_country_button:
		found_country_button.disabled = true
	if resume_save_button:
		resume_save_button.disabled = true
	if confirm_button:
		confirm_button.disabled = true
	if cancel_button:
		cancel_button.disabled = true
	if start_road_project_button:
		start_road_project_button.disabled = true
	if cancel_project_button:
		cancel_project_button.disabled = true

	var submit_msg := "Founding request for '%s' submitted. Awaiting simulation..." % c_name
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if country_setup_status_label:
		country_setup_status_label.text = "Founder Status: [Pending] Founding %s..." % c_name
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Country founding request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits a request to resume/load an authoritative save file
func execute_load_save(save_file: String) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_load_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "load_save",
		"save_file": save_file,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if found_country_button:
		found_country_button.disabled = true
	if resume_save_button:
		resume_save_button.disabled = true
	if confirm_button:
		confirm_button.disabled = true
	if cancel_button:
		cancel_button.disabled = true
	if start_road_project_button:
		start_road_project_button.disabled = true
	if cancel_project_button:
		cancel_project_button.disabled = true

	var submit_msg := "Resume request for '%s' submitted. Awaiting simulation..." % save_file
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if country_setup_status_label:
		country_setup_status_label.text = "Founder Status: [Pending] Resuming save..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Save resume request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits a project commissioning decision (e.g. "Road")
func execute_project_decision(project_type: String, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot submit project decision: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		if project_status_label:
			project_status_label.text = "Project Status: [Rejected] " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_proj_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "start_project",
		"project_type": project_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create project request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "Project initiation cancelled by executive order. Zero funds deducted."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if project_status_label:
			project_status_label.text = "Project Status: [Cancelled] Order cancelled."
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Project cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if start_road_project_button:
		start_road_project_button.disabled = true
	if cancel_project_button:
		cancel_project_button.disabled = true
	if confirm_button:
		confirm_button.disabled = true
	if cancel_button:
		cancel_button.disabled = true

	var submit_msg := "Commissioning of %s project submitted. Awaiting simulation..." % project_type
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if project_status_label:
		project_status_label.text = "Project Status: [Pending] Submitting commission..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Project request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits a campaign rally decision via file-based request (presidential_desk_request.json)
func execute_campaign_rally(confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot submit campaign rally decision: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Rejected] " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_rally_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "campaign_rally",
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create rally request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "Campaign rally cancelled by executive order. Zero funds deducted."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Cancelled] Order cancelled."
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Rally cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if hold_campaign_rally_button:
		hold_campaign_rally_button.disabled = true
	if contest_election_button:
		contest_election_button.disabled = true
	if cancel_election_action_button:
		cancel_election_action_button.disabled = true

	var submit_msg := "Campaign rally request submitted. Awaiting simulation..."
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if election_status_label:
		election_status_label.text = "Electoral Status: [Pending] Organizing national rally..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Rally request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits a national election decision via file-based request (presidential_desk_request.json)
func execute_contest_election(confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot contest election: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Rejected] " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_elect_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "contest_election",
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create election request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "National election call cancelled by executive order. Mandate remains intact."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Cancelled] Order cancelled."
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Election cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if hold_campaign_rally_button:
		hold_campaign_rally_button.disabled = true
	if contest_election_button:
		contest_election_button.disabled = true
	if cancel_election_action_button:
		cancel_election_action_button.disabled = true

	var submit_msg := "National election call submitted. Awaiting simulation..."
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if election_status_label:
		election_status_label.text = "Electoral Status: [Pending] Polls opening..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Election request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits an executive cabinet appointment decision via file-based request (presidential_desk_request.json)
func execute_cabinet_appointment(portfolio: String, candidate_type: String, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot submit cabinet appointment: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		if cabinet_status_label:
			cabinet_status_label.text = "Cabinet Status: [Rejected] " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_cab_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "appoint_minister",
		"portfolio": portfolio,
		"candidate_type": candidate_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create cabinet request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "Cabinet appointment order for %s cancelled by executive decree." % portfolio
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if cabinet_status_label:
			cabinet_status_label.text = "Cabinet Status: [Cancelled] Order cancelled."
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Cabinet appointment cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if appoint_technocrat_finance_button:
		appoint_technocrat_finance_button.disabled = true
	if appoint_loyalist_finance_button:
		appoint_loyalist_finance_button.disabled = true
	if appoint_technocrat_interior_button:
		appoint_technocrat_interior_button.disabled = true
	if appoint_loyalist_interior_button:
		appoint_loyalist_interior_button.disabled = true
	if cancel_cabinet_action_button:
		cancel_cabinet_action_button.disabled = true

	var submit_msg := "Commissioning %s candidate for Ministry of %s submitted. Awaiting simulation..." % [candidate_type, portfolio]
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if cabinet_status_label:
		cabinet_status_label.text = "Cabinet Status: [Pending] Vetting appointment..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Cabinet request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits a citizen town hall campaign decision via file-based request (presidential_desk_request.json)
func execute_town_hall(confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot hold town hall: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Rejected] " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_th_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "town_hall",
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create town hall request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "Town hall cancelled by executive order. Zero funds deducted."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if election_status_label:
			election_status_label.text = "Electoral Status: [Cancelled] Order cancelled."
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Town hall cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if hold_town_hall_button:
		hold_town_hall_button.disabled = true
	if hold_campaign_rally_button:
		hold_campaign_rally_button.disabled = true
	if contest_election_button:
		contest_election_button.disabled = true
	if cancel_election_action_button:
		cancel_election_action_button.disabled = true

	var submit_msg := "Citizen Town Hall request submitted. Awaiting simulation..."
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if election_status_label:
		election_status_label.text = "Electoral Status: [Pending] Convening citizens..."
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Town hall request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


## Submits an emergency crisis decree decision via file-based request (presidential_desk_request.json)
func execute_crisis_response(decree_type: String, confirmed: bool) -> Dictionary:
	_user_is_adjusting = false
	if _is_waiting_for_decision:
		return {
			"success": false,
			"result": "Pending",
			"message": "A decision request is already being processed. Please wait."
		}

	if not _is_connected or _latest_state.is_empty():
		var no_state_msg := "Cannot enact emergency decree: No Republic simulation state loaded."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Missing or Corrupted Save - " + no_state_msg
		return {
			"success": false,
			"result": "StateMissingOrCorrupt",
			"message": no_state_msg
		}

	var state_path := _resolve_state_path()
	var request_path := _resolve_request_path(state_path)
	var stale_result_path := _resolve_result_path(state_path)
	if FileAccess.file_exists(stale_result_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_result_path))

	var req_id := "req_crisis_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	_active_request_id = req_id if confirmed else ""
	var timestamp_str := Time.get_datetime_string_from_system()
	var request_dict := {
		"request_id": req_id,
		"action": "crisis_response",
		"decree_type": decree_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(request_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to create crisis decree request file: " + request_path
		_active_request_id = ""
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Rejected] Error - " + err_msg
		return { "success": false, "result": "Error", "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	if not confirmed:
		_is_waiting_for_decision = false
		_active_request_id = ""
		var cancel_msg := "Emergency decree cancelled by presidential directive. Zero state changes enacted."
		if decision_result_label:
			decision_result_label.text = "Decision Result: [Cancelled] " + cancel_msg
		if status_label:
			status_label.text = _format_status_text("Status: [Cancelled] Decree cancellation written to " + DEFAULT_REQUEST_FILENAME, "Cancelled")
		return {
			"success": false,
			"result": "Cancelled",
			"message": cancel_msg
		}

	_pending_request_path = request_path
	_is_waiting_for_decision = true
	_request_wait_timer = 0.0

	if emergency_austerity_button:
		emergency_austerity_button.disabled = true
	if emergency_relief_button:
		emergency_relief_button.disabled = true

	var submit_msg := "Emergency decree '%s' submitted. Awaiting simulation..." % decree_type
	if decision_result_label:
		decision_result_label.text = "Decision Result: [Pending] " + submit_msg
	if status_label:
		status_label.text = _format_status_text("Status: [Pending] Crisis request written to " + DEFAULT_REQUEST_FILENAME, "Pending")

	_try_run_cli_request_processor()
	return { "success": true, "result": "Pending", "message": submit_msg }


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
	if found_country_button:
		found_country_button.disabled = false
	if resume_save_button:
		resume_save_button.disabled = false
	if start_road_project_button:
		start_road_project_button.disabled = false
	if start_university_project_button:
		start_university_project_button.disabled = false
	if cancel_project_button:
		cancel_project_button.disabled = false
	if hold_campaign_rally_button:
		hold_campaign_rally_button.disabled = false
	if hold_town_hall_button:
		hold_town_hall_button.disabled = false
	if contest_election_button:
		contest_election_button.disabled = false
	if cancel_election_action_button:
		cancel_election_action_button.disabled = false
	if appoint_technocrat_finance_button:
		appoint_technocrat_finance_button.disabled = false
	if appoint_loyalist_finance_button:
		appoint_loyalist_finance_button.disabled = false
	if appoint_technocrat_interior_button:
		appoint_technocrat_interior_button.disabled = false
	if appoint_loyalist_interior_button:
		appoint_loyalist_interior_button.disabled = false
	if cancel_cabinet_action_button:
		cancel_cabinet_action_button.disabled = false
	if emergency_austerity_button:
		emergency_austerity_button.disabled = false
	if emergency_relief_button:
		emergency_relief_button.disabled = false

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
				decision_result_label.text = "Decision Result: [Success] Successfully applied - " + message
			if country_setup_status_label:
				country_setup_status_label.text = "Founder Status: [Success] " + message
			if project_status_label:
				project_status_label.text = "Project Status: [Success] " + message
			if election_status_label:
				election_status_label.text = "Electoral Status: [Success] " + message
			if cabinet_status_label:
				cabinet_status_label.text = "Cabinet Status: [Success] " + message
			if status_label:
				status_label.text = _format_status_text("Status: [Success] Decision enacted and persisted successfully", "Completed")
		else:
			# Authoritative save succeeded, but refreshing the display failed
			if decision_result_label:
				decision_result_label.text = "Decision Result: Authoritative decision saved, but refreshing the display state file failed."
			if country_setup_status_label:
				country_setup_status_label.text = "Founder Status: [Success] " + message
			if project_status_label:
				project_status_label.text = "Project Status: [Success] " + message
			if election_status_label:
				election_status_label.text = "Electoral Status: [Success] " + message
			if cabinet_status_label:
				cabinet_status_label.text = "Cabinet Status: [Success] " + message
			if status_label:
				status_label.text = _format_status_text("Status: Warning - Decision saved, but display refresh failed", "RefreshFailed")
	else:
		# Handle specific failure outcomes honestly
		var formatted_result := ""
		match result_type:
			"Cancelled":
				formatted_result = "Decision Result: [Cancelled] Cancelled - " + message
			"InvalidName":
				formatted_result = "Decision Result: [Rejected] Invalid Name - " + message
			"InvalidInput":
				formatted_result = "Decision Result: [Rejected] Invalid Input - " + message
			"StateMissingOrCorrupt":
				formatted_result = "Decision Result: [Rejected] Missing or Corrupted Save - " + message
			"PersistenceFailure":
				formatted_result = "Decision Result: [Rejected] Persistence Failure - " + message
			_:
				formatted_result = "Decision Result: [Rejected] Rejected - " + message

		if decision_result_label:
			decision_result_label.text = formatted_result
		if country_setup_status_label and (result_type == "InvalidName" or result_type == "InvalidInput"):
			country_setup_status_label.text = "Founder Status: [Rejected] " + message
		if project_status_label and (result_type == "Cancelled" or result_type == "InvalidInput" or result_type == "StateMissingOrCorrupt"):
			project_status_label.text = "Project Status: [Rejected] " + message
		if election_status_label:
			if result_type == "Cancelled":
				election_status_label.text = "Electoral Status: [Cancelled] " + message
			else:
				election_status_label.text = "Electoral Status: [Rejected] " + message
		if cabinet_status_label:
			if result_type == "Cancelled":
				cabinet_status_label.text = "Cabinet Status: [Cancelled] " + message
			else:
				cabinet_status_label.text = "Cabinet Status: [Rejected] " + message
		if status_label:
			status_label.text = _format_status_text("Status: [Rejected] " + message, result_type)

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
	var args := ["run", "--project", "src/Republic.Cli", "--", "--host", "--port=7777", "--save-file=shared_world.json"]
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
	if last_ai_decision_label:
		last_ai_decision_label.text = "Last AI Decision: [No AI decisions recorded]"
	if shared_clock_label:
		shared_clock_label.text = "Shared World Clock: [Local Clock]"
	if diplomacy_status_label:
		diplomacy_status_label.text = "Diplomatic Standing: Unrecognized"
	if recognize_button:
		recognize_button.disabled = true
	if latest_message_label:
		latest_message_label.text = "Latest Dispatch: [No messages exchanged]"
	if send_message_button:
		send_message_button.disabled = true
	if latest_trade_status_label:
		latest_trade_status_label.text = "Trade Status: [No active proposal]"
	if trade_lifecycle_stage_label:
		trade_lifecycle_stage_label.text = "Trade Lifecycle: [Draft Proposal] — Configure amount above and propose to peer"
	if accept_trade_button:
		accept_trade_button.disabled = true
	if reject_trade_button:
		reject_trade_button.disabled = true
	if propose_trade_button:
		propose_trade_button.disabled = true
	_is_recognized = false
	_active_trade_proposal = {}


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
			_update_network_status_display()
			if host_button:
				host_button.disabled = true
			if join_button:
				join_button.disabled = true
			if disconnect_button:
				disconnect_button.disabled = false
			if recognize_button and not _is_recognized:
				recognize_button.disabled = false
			if send_message_button:
				send_message_button.disabled = false
			if propose_trade_button:
				propose_trade_button.disabled = false
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


## Updates the multiplayer network status label to clearly indicate connection
## and authoritative sovereign ownership of the local country.
func _update_network_status_display() -> void:
	if not network_status_label:
		return
	if not _is_network_connected:
		return
	var country_name := str(_latest_state.get("country_name", _latest_state.get("CountryName", "Republic of Arcadia")))
	if country_name.is_empty() or country_name == "Unknown":
		country_name = "Republic of Arcadia"
	network_status_label.text = "Network: Connected as owner of [%s]" % country_name


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
	_update_network_status_display()


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

	elif msg_type == "country" or msg_type == "country_snapshot":
		var snap: Variant = msg.get("country", msg.get("snapshot", {}))
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
				var peer_ai_dec := str(snap.get("last_ai_decision", ""))
				if not peer_ai_dec.is_empty() and last_ai_decision_label:
					last_ai_decision_label.text = "Last AI Decision: " + peer_ai_dec

	elif msg_type == "ai_decision":
		var dec_note := str(msg.get("decision", msg.get("text", "")))
		if not dec_note.is_empty() and last_ai_decision_label:
			last_ai_decision_label.text = "Last AI Decision: " + dec_note

	elif msg_type == "diplomacy":
		var dip_data: Variant = msg.get("diplomacy", {})
		if dip_data is Dictionary:
			var status := str(dip_data.get("status", "Unrecognized"))
			_is_recognized = (status == "Recognized")
			if diplomacy_status_label:
				diplomacy_status_label.text = "Diplomatic Standing: " + status
			if recognize_button and _is_recognized:
				recognize_button.disabled = true

	elif msg_type == "message":
		var message_data: Variant = msg.get("message", {})
		if message_data is Dictionary:
			var from_str := str(message_data.get("from", "Diplomat"))
			var text_str := str(message_data.get("text", ""))
			var ts_str := str(message_data.get("timestamp", ""))
			if latest_message_label:
				latest_message_label.text = "Latest Dispatch [%s]: %s - \"%s\"" % [ts_str, from_str, text_str]

	elif msg_type == "trade":
		var trade_data: Variant = msg.get("trade", {})
		if trade_data is Dictionary:
			_active_trade_proposal = trade_data
			var from_id := str(trade_data.get("from", ""))
			var to_id := str(trade_data.get("to", ""))
			var amount := float(trade_data.get("amount", 0.0))
			var status := str(trade_data.get("status", "Pending"))

			var amount_str := "R " + str(amount)
			if amount >= 1000000000.0:
				amount_str = "R %.1fB" % (amount / 1000000000.0)
			elif amount >= 1000000.0:
				amount_str = "R %.1fM" % (amount / 1000000.0)

			if status == "Pending":
				if latest_trade_status_label:
					latest_trade_status_label.text = "Trade Status: Pending proposal (%s) from %s" % [amount_str, from_id]
				if trade_lifecycle_stage_label:
					if from_id != _local_client_id:
						trade_lifecycle_stage_label.text = "Trade Lifecycle: [Pending Proposal] — Inbound offer from %s awaiting your executive decision" % from_id
					else:
						trade_lifecycle_stage_label.text = "Trade Lifecycle: [Pending Proposal] — Outbound offer awaiting foreign peer acceptance"
				if from_id != _local_client_id:
					if accept_trade_button:
						accept_trade_button.disabled = false
					if reject_trade_button:
						reject_trade_button.disabled = false
				else:
					if accept_trade_button:
						accept_trade_button.disabled = true
					if reject_trade_button:
						reject_trade_button.disabled = true

			elif status == "Accepted":
				if latest_trade_status_label:
					latest_trade_status_label.text = "Trade Status: Accepted - %s transferred from %s to %s" % [amount_str, from_id, to_id]
				if trade_lifecycle_stage_label:
					trade_lifecycle_stage_label.text = "Trade Lifecycle: [Accepted Agreement / Completed Transaction] — Sovereign trade ratified and settled authoritatively"
				if accept_trade_button:
					accept_trade_button.disabled = true
				if reject_trade_button:
					reject_trade_button.disabled = true
				_apply_local_trade_treasury_adjustment(from_id, to_id, amount)

			elif status == "Rejected":
				if latest_trade_status_label:
					latest_trade_status_label.text = "Trade Status: Proposal rejected"
				if trade_lifecycle_stage_label:
					trade_lifecycle_stage_label.text = "Trade Lifecycle: [Rejected Proposal] — Proposal declined"
				if accept_trade_button:
					accept_trade_button.disabled = true
				if reject_trade_button:
					reject_trade_button.disabled = true

	elif msg_type == "decision_result":
		var success := bool(msg.get("success", false))
		var result := str(msg.get("result", ""))
		var status_msg := str(msg.get("status_message", ""))
		if decision_result_label and not status_msg.is_empty():
			decision_result_label.text = "Decision Result: [%s] %s" % [result, status_msg]
		if status_label and not status_msg.is_empty():
			status_label.text = _format_status_text("Host Result: " + status_msg, "None")
		if multiplayer_status_label and not status_msg.is_empty():
			multiplayer_status_label.text = "Network: " + status_msg
		else:
			_update_network_status_display()


func _on_recognize_button_pressed() -> void:
	var peer_id := _peer_country_id if not _peer_country_id.is_empty() else "peer"
	var dip := {
		"from_country_id": _local_client_id,
		"to_country_id": peer_id,
		"status": "Recognized"
	}
	var msg := {
		"type": "diplomacy",
		"sender_id": _local_client_id,
		"diplomacy": dip
	}
	_send_network_message(msg)
	_handle_network_message(msg)


func _on_send_message_button_pressed() -> void:
	if message_line_edit == null or message_line_edit.text.strip_edges().is_empty():
		return
	var text := message_line_edit.text.strip_edges()
	var peer_id := _peer_country_id if not _peer_country_id.is_empty() else "peer"
	var sender_name := str(_latest_state.get("country_name", _local_client_id))
	var msg_payload := {
		"from": sender_name,
		"to": peer_id,
		"text": text,
		"timestamp": Time.get_time_string_from_system()
	}
	var net_msg := {
		"type": "message",
		"sender_id": _local_client_id,
		"message": msg_payload
	}
	_send_network_message(net_msg)
	_handle_network_message(net_msg)
	message_line_edit.text = ""


func _on_trade_amount_changed(new_val: float) -> void:
	if _active_trade_proposal.is_empty() or str(_active_trade_proposal.get("status", "")) != "Pending":
		var peer_name := "foreign peer"
		if foreign_country_name_label and not foreign_country_name_label.text.contains("[None Connected]"):
			peer_name = foreign_country_name_label.text.replace("Foreign Country: ", "").replace("Peer Country: ", "").strip_edges()
		if trade_lifecycle_stage_label:
			trade_lifecycle_stage_label.text = "Trade Lifecycle: [Draft Proposal] — Prepared offer of R %s for %s (Click 'Propose Trade' to send)" % [_format_number_commas(new_val), peer_name]


func _on_propose_trade_button_pressed() -> void:
	var amount := 1000000000.0
	if trade_amount_spinbox:
		amount = trade_amount_spinbox.value
	var peer_id := _peer_country_id if not _peer_country_id.is_empty() else "peer"
	if trade_recipient_input and not trade_recipient_input.text.is_empty():
		peer_id = trade_recipient_input.text
	var trade_id := "tr_" + str(Time.get_ticks_msec())
	var trade_payload := {
		"id": trade_id,
		"from": _local_client_id,
		"to": peer_id,
		"amount": amount,
		"resource": "REPU",
		"status": "Pending"
	}
	var net_msg := {
		"type": "trade",
		"sender_id": _local_client_id,
		"trade": trade_payload
	}
	if trade_lifecycle_stage_label:
		trade_lifecycle_stage_label.text = "Trade Lifecycle: [Pending Proposal] — Outbound proposal for R %s submitted to foreign sovereign" % _format_number_commas(amount)
	_send_network_message(net_msg)
	_handle_network_message(net_msg)


func _on_accept_trade_button_pressed() -> void:
	if _active_trade_proposal.is_empty():
		return
	_active_trade_proposal["status"] = "Accepted"
	var net_msg := {
		"type": "trade",
		"sender_id": _local_client_id,
		"trade": _active_trade_proposal
	}
	_send_network_message(net_msg)
	_handle_network_message(net_msg)


func _on_reject_trade_button_pressed() -> void:
	if _active_trade_proposal.is_empty():
		return
	_active_trade_proposal["status"] = "Rejected"
	var net_msg := {
		"type": "trade",
		"sender_id": _local_client_id,
		"trade": _active_trade_proposal
	}
	_send_network_message(net_msg)
	_handle_network_message(net_msg)


func _apply_local_trade_treasury_adjustment(from_id: String, to_id: String, amount: float) -> void:
	# If local client is sender or recipient and we have a treasury displayed, update it locally
	if treasury_label and not treasury_label.text.is_empty():
		var raw_str := treasury_label.text.replace("Treasury: R ", "").replace("Treasury: ", "").replace(",", "").strip_edges()
		if raw_str.is_valid_float():
			var cur_val := raw_str.to_float()
			if from_id == _local_client_id:
				cur_val = max(0.0, cur_val - amount)
				treasury_label.text = "Treasury: R " + _format_number_commas(cur_val)
			elif to_id == _local_client_id:
				cur_val += amount
				treasury_label.text = "Treasury: R " + _format_number_commas(cur_val)

	if foreign_country_treasury_label and not foreign_country_treasury_label.text.is_empty():
		var peer_raw := foreign_country_treasury_label.text.replace("Treasury: R ", "").replace("Treasury: ", "").replace(",", "").strip_edges()
		if peer_raw.is_valid_float():
			var cur_peer := peer_raw.to_float()
			if from_id == _peer_country_id:
				cur_peer = max(0.0, cur_peer - amount)
				foreign_country_treasury_label.text = "Treasury: R " + _format_number_commas(cur_peer)
			elif to_id == _peer_country_id:
				cur_peer += amount
				foreign_country_treasury_label.text = "Treasury: R " + _format_number_commas(cur_peer)


func _format_number_commas(val: float) -> String:
	var int_val := int(round(val))
	var s := str(int_val)
	var res := ""
	var count := 0
	for i in range(s.length() - 1, -1, -1):
		res = s[i] + res
		count += 1
		if count % 3 == 0 and i > 0:
			res = "," + res
	return res


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST or what == NOTIFICATION_PREDELETE:
		if _is_hosting_process and _host_process_pid > 0:
			OS.kill(_host_process_pid)
		disconnect_network()


## Resolves target path for writing simulated buff purchase requests
func _resolve_buff_request_path() -> String:
	if not _active_state_path.is_empty():
		return _active_state_path.get_base_dir().path_join(DEFAULT_BUFF_REQUEST_FILENAME)
	return "user://" + DEFAULT_BUFF_REQUEST_FILENAME


## Simulates purchasing an executive buff by writing a request file and updating status
func buy_buff(buff_id: String, buff_name: String, price_usd: float) -> bool:
	_init_controls()
	var country_id: String = ""
	if _latest_state.has("country_id"):
		country_id = str(_latest_state["country_id"])
	elif _latest_state.has("country"):
		country_id = str(_latest_state["country"])

	var req_id := "buff_req_" + str(Time.get_ticks_msec())
	var request_data := {
		"request_id": req_id,
		"buff_id": buff_id,
		"buff_name": buff_name,
		"price_usd": price_usd,
		"country_id": country_id,
		"status": "SimulatedPurchaseRequested",
		"timestamp_utc": Time.get_datetime_string_from_system(true)
	}

	var json_str := JSON.stringify(request_data, "\t")
	var path := _resolve_buff_request_path()
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		if buff_status_label:
			buff_status_label.text = "Buff Status: Error writing purchase request to " + path
		return false

	file.store_string(json_str)
	file.close()

	if buff_status_label:
		buff_status_label.text = "Buff Status: Simulated purchase request written for '%s' ($%.2f USD)" % [buff_name, price_usd]
	return true


func _on_buy_admin_buff_pressed() -> void:
	buy_buff("admin_efficiency", "Administrative Efficiency", 0.99)


func _on_buy_investor_buff_pressed() -> void:
	buy_buff("investor_confidence", "Investor Confidence", 1.99)

