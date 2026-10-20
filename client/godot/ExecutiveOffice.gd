extends Control

const ExecutiveOfficeCatalog = preload("res://ExecutiveOfficeCatalog.gd")

## ExecutiveOffice.gd
## Controller for the rich, interactive Executive Presidential Office main screen in Republic.
## Provides decoupled visual office environment layers (Window, Flag, Portrait, Desk, Decor,
## Lighting, Seating, Carpets) and four-edge interface HUD docking spaces connected directly
## to authoritative Republic.Core state and persistent player office customisation.

signal state_loaded(state: Dictionary)
signal state_cleared()
signal customization_applied(customization: Dictionary)
signal customization_saved(customization: Dictionary)

## Navigation Sections matching PresidentialDesk.Section enum
enum Section { OVERVIEW, CABINET, ECONOMY, CAMPAIGN, DIPLOMACY, BRIEFINGS, CUSTOMISE }
var active_nav_section: Section = Section.OVERVIEW

# ==============================================================================
# 1. OFFICE ENVIRONMENT REFERENCES (DECOUPLED VISUAL LAYERS)
# ==============================================================================
@onready var office_environment: Control = _get_node_safe("OfficeEnvironment") as Control
@onready var room_backdrop: Control = _get_node_safe("OfficeEnvironment/RoomBackdrop") as Control
@onready var wall_backdrop: ColorRect = _get_node_safe("OfficeEnvironment/RoomBackdrop/WallBackdrop") as ColorRect
@onready var molding_strip: ColorRect = _get_node_safe("OfficeEnvironment/RoomBackdrop/MoldingStrip") as ColorRect
@onready var floor_backdrop: ColorRect = _get_node_safe("OfficeEnvironment/RoomBackdrop/FloorBackdrop") as ColorRect

@onready var office_carpet: PanelContainer = _get_node_safe("OfficeEnvironment/OfficeCarpet") as PanelContainer
@onready var ceiling_lighting: Control = _get_node_safe("OfficeEnvironment/CeilingLighting") as Control
@onready var chandelier_label: Label = _get_node_safe("OfficeEnvironment/CeilingLighting/ChandelierLabel") as Label

@onready var bookshelves_and_decor: Control = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor") as Control
@onready var bookshelf_visual: PanelContainer = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual") as PanelContainer
@onready var bookshelf_icon_label: Label = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual/BookshelfVBox/BookshelfIconLabel") as Label
@onready var bookshelf_label: Label = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual/BookshelfVBox/BookshelfLabel") as Label

@onready var artwork_and_gifts: Control = _get_node_safe("OfficeEnvironment/ArtworkAndGifts") as Control
@onready var artwork_visual: PanelContainer = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual") as PanelContainer
@onready var artwork_icon_label: Label = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual/ArtworkVBox/ArtworkIconLabel") as Label
@onready var artwork_label: Label = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual/ArtworkVBox/ArtworkLabel") as Label

@onready var visitor_seating: Control = _get_node_safe("OfficeEnvironment/VisitorSeating") as Control
@onready var visitor_table: PanelContainer = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorTable") as PanelContainer
@onready var visitor_chair1: PanelContainer = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorChair1") as PanelContainer
@onready var visitor_chair2: PanelContainer = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorChair2") as PanelContainer

@onready var capital_window: Control = _get_node_safe("OfficeEnvironment/CapitalWindow") as Control
@onready var city_scenery: Control = _get_node_safe("OfficeEnvironment/CapitalWindow/CityScenery") as Control
@onready var window_frame: Control = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowFrame") as Control
@onready var window_button: Button = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowButton") as Button

@onready var country_flag: Control = _get_node_safe("OfficeEnvironment/CountryFlag") as Control
@onready var flag_pole: Control = _get_node_safe("OfficeEnvironment/CountryFlag/FlagPole") as Control
@onready var flag_banner: PanelContainer = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner") as PanelContainer
@onready var flag_emblem_label: Label = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner/FlagEmblemLabel") as Label

@onready var presidential_portrait: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait") as Control
@onready var portrait_frame: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame") as Control
@onready var portrait_canvas: Control = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas") as Control
@onready var portrait_icon_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas/PortraitVBox/PortraitIconLabel") as Label
@onready var portrait_placeholder_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas/PortraitVBox/PortraitPlaceholderLabel") as Label
@onready var portrait_name_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitNameLabel") as Label
@onready var portrait_party_label: Label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitPartyLabel") as Label
@onready var portrait_button: Button = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitButton") as Button

@onready var executive_desk: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk") as Control
@onready var presidential_chair: PanelContainer = _get_node_safe("OfficeEnvironment/ExecutiveDesk/PresidentialChair") as PanelContainer
@onready var desk_surface: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskSurface") as Control
@onready var desk_blotter: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter") as Control
@onready var blotter_button: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter/BlotterButton") as Button
@onready var hotline_phone: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton") as Button
@onready var hotline_badge: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton/HotlineNotificationBadge") as Control
@onready var hotline_badge_label: Label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton/HotlineNotificationBadge/HotlineBadgeLabel") as Label
@onready var intelligence_dossier: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton") as Button
@onready var dossier_badge: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton/DossierNotificationBadge") as Control
@onready var dossier_badge_label: Label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton/DossierNotificationBadge/DossierBadgeLabel") as Label
@onready var executive_inbox: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton") as Button
@onready var inbox_badge: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton/InboxNotificationBadge") as Control
@onready var inbox_badge_label: Label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton/InboxNotificationBadge/InboxBadgeLabel") as Label
@onready var presidential_seal: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/PresidentialSeal") as Control

# ==============================================================================
# 2. INTERFACE HUD DOCK REFERENCES (TOP, NAV, LEFT, RIGHT, BOTTOM)
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

# Navigation Tab Bar
@onready var nav_bar_panel: PanelContainer = _get_node_safe("InterfaceLayer/NavBarPanel") as PanelContainer
@onready var nav_overview_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavOverviewButton") as Button
@onready var nav_cabinet_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCabinetButton") as Button
@onready var nav_economy_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavEconomyButton") as Button
@onready var nav_campaign_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCampaignButton") as Button
@onready var nav_diplomacy_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavDiplomacyButton") as Button
@onready var nav_briefings_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavBriefingsButton") as Button
@onready var nav_customise_button: Button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCustomiseButton") as Button

# Left Dock: Domestic Governance & Cabinet
@onready var left_dock: PanelContainer = _get_node_safe("InterfaceLayer/LeftDock") as PanelContainer
@onready var toggle_left_dock_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockHeader/ToggleLeftDockButton") as Button
@onready var cabinet_summary_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetSummaryLabel") as Label
@onready var civil_service_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CivilServiceLabel") as Label
@onready var corruption_control_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CorruptionControlLabel") as Label
@onready var ruling_party_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/RulingPartyLabel") as Label
@onready var polling_support_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/PollingSupportLabel") as Label
@onready var open_cabinet_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/OpenCabinetButton") as Button

# Right Dock: Foreign Affairs & Intelligence
@onready var right_dock: PanelContainer = _get_node_safe("InterfaceLayer/RightDock") as PanelContainer
@onready var toggle_right_dock_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockHeader/ToggleRightDockButton") as Button
@onready var military_readiness_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/MilitaryReadinessLabel") as Label
@onready var active_crisis_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/ActiveCrisisLabel") as Label
@onready var gdp_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/GdpLabel") as Label
@onready var offline_briefing_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OfflineBriefingLabel") as Label
@onready var open_diplomacy_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OpenDiplomacyButton") as Button

# Bottom Dock: Situation Ticker & Infrastructure Projects
@onready var bottom_dock: PanelContainer = _get_node_safe("InterfaceLayer/BottomDock") as PanelContainer
@onready var status_ticker_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StatusTickerLabel") as Label
@onready var active_project_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/ActiveProjectLabel") as Label
@onready var open_projects_button: Button = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/OpenProjectsButton") as Button
@onready var state_status_label: Label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StateStatusLabel") as Label

# Session Briefing Toast
@onready var session_briefing_toast: PanelContainer = _get_node_safe("InterfaceLayer/SessionBriefingToast") as PanelContainer
@onready var session_briefing_title: Label = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/ToastHeader/SessionBriefingTitle") as Label
@onready var dismiss_briefing_toast_button: Button = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/ToastHeader/DismissBriefingToastButton") as Button
@onready var session_briefing_metrics_label: Label = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/SessionBriefingMetricsLabel") as Label
@onready var session_briefing_status_label: Label = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/SessionBriefingStatusLabel") as Label

# ==============================================================================
# 3. INTERACTIVE MODAL PANELS (BRIEFING, HOTLINE, DOSSIER, CUSTOMISATION)
# ==============================================================================
@onready var modal_backdrop: ColorRect = _get_node_safe("InterfaceLayer/ModalBackdrop") as ColorRect

# Briefing Modal
@onready var briefing_modal: PanelContainer = _get_node_safe("InterfaceLayer/BriefingModal") as PanelContainer
@onready var close_briefing_x_button: Button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingHeaderHBox/CloseBriefingXButton") as Button
@onready var briefing_active_project_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingActiveProjectLabel") as Label
@onready var briefing_decisions_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingDecisionsLabel") as Label
@onready var briefing_crisis_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingCrisisLabel") as Label
@onready var briefing_capacity_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingCapacityLabel") as Label
@onready var briefing_fiscal_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingFiscalLabel") as Label
@onready var briefing_defense_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingDefenseLabel") as Label
@onready var briefing_offline_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingOfflineLabel") as Label
@onready var briefing_unsupported_label: Label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingUnsupportedLabel") as Label
@onready var briefing_desk_button: Button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingActionsHBox/BriefingDeskButton") as Button
@onready var close_briefing_button: Button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingActionsHBox/CloseBriefingButton") as Button

# Hotline Modal
@onready var hotline_modal: PanelContainer = _get_node_safe("InterfaceLayer/HotlineModal") as PanelContainer
@onready var close_hotline_x_button: Button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineHeaderHBox/CloseHotlineXButton") as Button
@onready var hotline_status_label: Label = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineStatusLabel") as Label
@onready var hotline_standing_label: Label = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineStandingLabel") as Label
@onready var hotline_desk_button: Button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineActionsHBox/HotlineDeskButton") as Button
@onready var close_hotline_button: Button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineActionsHBox/CloseHotlineButton") as Button

# Classified Dossier Modal
@onready var dossier_modal: PanelContainer = _get_node_safe("InterfaceLayer/DossierModal") as PanelContainer
@onready var close_dossier_x_button: Button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierHeaderHBox/CloseDossierXButton") as Button
@onready var dossier_defense_label: Label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierDefenseLabel") as Label
@onready var dossier_crisis_label: Label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierCrisisLabel") as Label
@onready var dossier_espionage_label: Label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierEspionageLabel") as Label
@onready var dossier_desk_button: Button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/DossierDeskButton") as Button
@onready var close_dossier_button: Button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/CloseDossierButton") as Button

# Phase 3 Customisation Modal
@onready var customisation_modal: PanelContainer = _get_node_safe("InterfaceLayer/CustomisationModal") as PanelContainer
@onready var close_customisation_x_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationHeaderHBox/CloseCustomisationXButton") as Button
@onready var customisation_treasury_label: Label = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationHeaderHBox/CustomisationTreasuryLabel") as Label
@onready var tab_portraits_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabPortraitsButton") as Button
@onready var tab_desks_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabDesksButton") as Button
@onready var tab_seating_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabSeatingButton") as Button
@onready var tab_walls_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabWallsButton") as Button
@onready var tab_carpets_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabCarpetsButton") as Button
@onready var tab_bookshelves_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabBookshelvesButton") as Button
@onready var tab_artwork_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabArtworkButton") as Button
@onready var catalog_grid: VBoxContainer = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CatalogScroll/CatalogGrid") as VBoxContainer
@onready var customisation_status_label: Label = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/CustomisationStatusLabel") as Label
@onready var save_customisation_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/SaveCustomisationButton") as Button
@onready var reset_customisation_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/ResetCustomisationButton") as Button
@onready var close_customisation_button: Button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/CloseCustomisationButton") as Button

# Phase 3 Purchase Confirmation Modal
@onready var purchase_confirm_modal: PanelContainer = _get_node_safe("InterfaceLayer/PurchaseConfirmModal") as PanelContainer
@onready var purchase_item_name_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseItemNameLabel") as Label
@onready var purchase_item_desc_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseItemDescLabel") as Label
@onready var purchase_price_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchasePriceLabel") as Label
@onready var purchase_approval_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseApprovalLabel") as Label
@onready var purchase_install_time_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseInstallTimeLabel") as Label
@onready var purchase_treasury_impact_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseTreasuryImpactLabel") as Label
@onready var purchase_notice_label: Label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseNoticeLabel") as Label
@onready var confirm_purchase_button: Button = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseActionsHBox/ConfirmPurchaseButton") as Button
@onready var cancel_purchase_button: Button = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseActionsHBox/CancelPurchaseButton") as Button

# ==============================================================================
# 4. STATE & PERSISTENCE VARIABLES
# ==============================================================================
const DEFAULT_STATE_FILE := "res://presidential_desk_state.json"
const USER_STATE_FILE := "user://presidential_desk_state.json"

var current_state_path: String = DEFAULT_STATE_FILE
var latest_state: Dictionary = {}
var last_modified_time: int = 0
var poll_timer: Timer

# Phase 3 Customisation Tracking
var active_customization: Dictionary = {}
var current_customization_category: String = "portraits"
var pending_purchase_item: Dictionary = {}
var custom_save_path: String = ""

# Living Feedback & Notification State
var hotline_badge_dismissed: bool = false
var dossier_badge_dismissed: bool = false
var inbox_badge_dismissed: bool = false

var flag_sway_time: float = 0.0
var flag_sway_amplitude: float = 1.2
var flag_sway_frequency: float = 1.5

var left_dock_pinned: bool = false
var right_dock_pinned: bool = false

var session_briefing_timer: Timer = null
var dock_flash_timer: Timer = null
var flashing_dock: PanelContainer = null

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
	if wall_backdrop == null:
		wall_backdrop = _get_node_safe("OfficeEnvironment/RoomBackdrop/WallBackdrop") as ColorRect
	if molding_strip == null:
		molding_strip = _get_node_safe("OfficeEnvironment/RoomBackdrop/MoldingStrip") as ColorRect
	if floor_backdrop == null:
		floor_backdrop = _get_node_safe("OfficeEnvironment/RoomBackdrop/FloorBackdrop") as ColorRect

	if office_carpet == null:
		office_carpet = _get_node_safe("OfficeEnvironment/OfficeCarpet") as PanelContainer
	if ceiling_lighting == null:
		ceiling_lighting = _get_node_safe("OfficeEnvironment/CeilingLighting") as Control
	if chandelier_label == null:
		chandelier_label = _get_node_safe("OfficeEnvironment/CeilingLighting/ChandelierLabel") as Label

	if bookshelves_and_decor == null:
		bookshelves_and_decor = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor") as Control
	if bookshelf_visual == null:
		bookshelf_visual = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual") as PanelContainer
	if bookshelf_icon_label == null:
		bookshelf_icon_label = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual/BookshelfVBox/BookshelfIconLabel") as Label
	if bookshelf_label == null:
		bookshelf_label = _get_node_safe("OfficeEnvironment/BookshelvesAndDecor/BookshelfVisual/BookshelfVBox/BookshelfLabel") as Label

	if artwork_and_gifts == null:
		artwork_and_gifts = _get_node_safe("OfficeEnvironment/ArtworkAndGifts") as Control
	if artwork_visual == null:
		artwork_visual = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual") as PanelContainer
	if artwork_icon_label == null:
		artwork_icon_label = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual/ArtworkVBox/ArtworkIconLabel") as Label
	if artwork_label == null:
		artwork_label = _get_node_safe("OfficeEnvironment/ArtworkAndGifts/ArtworkVisual/ArtworkVBox/ArtworkLabel") as Label

	if visitor_seating == null:
		visitor_seating = _get_node_safe("OfficeEnvironment/VisitorSeating") as Control
	if visitor_table == null:
		visitor_table = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorTable") as PanelContainer
	if visitor_chair1 == null:
		visitor_chair1 = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorChair1") as PanelContainer
	if visitor_chair2 == null:
		visitor_chair2 = _get_node_safe("OfficeEnvironment/VisitorSeating/VisitorChair2") as PanelContainer

	if capital_window == null:
		capital_window = _get_node_safe("OfficeEnvironment/CapitalWindow") as Control
	if city_scenery == null:
		city_scenery = _get_node_safe("OfficeEnvironment/CapitalWindow/CityScenery") as Control
	if window_frame == null:
		window_frame = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowFrame") as Control
	if window_button == null:
		window_button = _get_node_safe("OfficeEnvironment/CapitalWindow/WindowButton") as Button

	if country_flag == null:
		country_flag = _get_node_safe("OfficeEnvironment/CountryFlag") as Control
	if flag_pole == null:
		flag_pole = _get_node_safe("OfficeEnvironment/CountryFlag/FlagPole") as Control
	if flag_banner == null:
		flag_banner = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner") as PanelContainer
	if flag_emblem_label == null:
		flag_emblem_label = _get_node_safe("OfficeEnvironment/CountryFlag/FlagBanner/FlagEmblemLabel") as Label

	if presidential_portrait == null:
		presidential_portrait = _get_node_safe("OfficeEnvironment/PresidentialPortrait") as Control
	if portrait_frame == null:
		portrait_frame = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame") as Control
	if portrait_canvas == null:
		portrait_canvas = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas") as Control
	if portrait_icon_label == null:
		portrait_icon_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas/PortraitVBox/PortraitIconLabel") as Label
	if portrait_placeholder_label == null:
		portrait_placeholder_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitFrame/PortraitCanvas/PortraitVBox/PortraitPlaceholderLabel") as Label
	if portrait_name_label == null:
		portrait_name_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitNameLabel") as Label
	if portrait_party_label == null:
		portrait_party_label = _get_node_safe("OfficeEnvironment/PresidentialPortrait/Plaque/PortraitPartyLabel") as Label
	if portrait_button == null:
		portrait_button = _get_node_safe("OfficeEnvironment/PresidentialPortrait/PortraitButton") as Button

	if executive_desk == null:
		executive_desk = _get_node_safe("OfficeEnvironment/ExecutiveDesk") as Control
	if presidential_chair == null:
		presidential_chair = _get_node_safe("OfficeEnvironment/ExecutiveDesk/PresidentialChair") as PanelContainer
	if desk_surface == null:
		desk_surface = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskSurface") as Control
	if desk_blotter == null:
		desk_blotter = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter") as Control
	if blotter_button == null:
		blotter_button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DeskBlotter/BlotterButton") as Button
	if hotline_phone == null:
		hotline_phone = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton") as Button
	if hotline_badge == null:
		hotline_badge = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton/HotlineNotificationBadge") as Control
	if hotline_badge_label == null:
		hotline_badge_label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/HotlinePhoneButton/HotlineNotificationBadge/HotlineBadgeLabel") as Label
	if intelligence_dossier == null:
		intelligence_dossier = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton") as Button
	if dossier_badge == null:
		dossier_badge = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton/DossierNotificationBadge") as Control
	if dossier_badge_label == null:
		dossier_badge_label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/IntelligenceDossierButton/DossierNotificationBadge/DossierBadgeLabel") as Label
	if executive_inbox == null:
		executive_inbox = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton") as Button
	if inbox_badge == null:
		inbox_badge = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton/InboxNotificationBadge") as Control
	if inbox_badge_label == null:
		inbox_badge_label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/ExecutiveInboxButton/InboxNotificationBadge/InboxBadgeLabel") as Label
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

	if nav_bar_panel == null:
		nav_bar_panel = _get_node_safe("InterfaceLayer/NavBarPanel") as PanelContainer
	if nav_overview_button == null:
		nav_overview_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavOverviewButton") as Button
	if nav_cabinet_button == null:
		nav_cabinet_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCabinetButton") as Button
	if nav_economy_button == null:
		nav_economy_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavEconomyButton") as Button
	if nav_campaign_button == null:
		nav_campaign_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCampaignButton") as Button
	if nav_diplomacy_button == null:
		nav_diplomacy_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavDiplomacyButton") as Button
	if nav_briefings_button == null:
		nav_briefings_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavBriefingsButton") as Button
	if nav_customise_button == null:
		nav_customise_button = _get_node_safe("InterfaceLayer/NavBarPanel/NavBarHBox/NavCustomiseButton") as Button

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
	if open_cabinet_button == null:
		open_cabinet_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/OpenCabinetButton") as Button

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
	if open_diplomacy_button == null:
		open_diplomacy_button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OpenDiplomacyButton") as Button

	if bottom_dock == null:
		bottom_dock = _get_node_safe("InterfaceLayer/BottomDock") as PanelContainer
	if status_ticker_label == null:
		status_ticker_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StatusTickerLabel") as Label
	if active_project_label == null:
		active_project_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/ActiveProjectLabel") as Label
	if open_projects_button == null:
		open_projects_button = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/OpenProjectsButton") as Button
	if state_status_label == null:
		state_status_label = _get_node_safe("InterfaceLayer/BottomDock/BottomDockHBox/StateStatusLabel") as Label

	if modal_backdrop == null:
		modal_backdrop = _get_node_safe("InterfaceLayer/ModalBackdrop") as ColorRect
	if briefing_modal == null:
		briefing_modal = _get_node_safe("InterfaceLayer/BriefingModal") as PanelContainer
	if close_briefing_x_button == null:
		close_briefing_x_button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingHeaderHBox/CloseBriefingXButton") as Button
	if briefing_active_project_label == null:
		briefing_active_project_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingActiveProjectLabel") as Label
	if briefing_decisions_label == null:
		briefing_decisions_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingDecisionsLabel") as Label
	if briefing_crisis_label == null:
		briefing_crisis_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingCrisisLabel") as Label
	if briefing_capacity_label == null:
		briefing_capacity_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingCapacityLabel") as Label
	if briefing_fiscal_label == null:
		briefing_fiscal_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingFiscalLabel") as Label
	if briefing_defense_label == null:
		briefing_defense_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingDefenseLabel") as Label
	if briefing_offline_label == null:
		briefing_offline_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingOfflineLabel") as Label
	if briefing_unsupported_label == null:
		briefing_unsupported_label = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox/BriefingUnsupportedLabel") as Label
	if briefing_desk_button == null:
		briefing_desk_button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingActionsHBox/BriefingDeskButton") as Button
	if close_briefing_button == null:
		close_briefing_button = _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingActionsHBox/CloseBriefingButton") as Button

	if hotline_modal == null:
		hotline_modal = _get_node_safe("InterfaceLayer/HotlineModal") as PanelContainer
	if close_hotline_x_button == null:
		close_hotline_x_button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineHeaderHBox/CloseHotlineXButton") as Button
	if hotline_status_label == null:
		hotline_status_label = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineStatusLabel") as Label
	if hotline_standing_label == null:
		hotline_standing_label = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineStandingLabel") as Label
	if hotline_desk_button == null:
		hotline_desk_button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineActionsHBox/HotlineDeskButton") as Button
	if close_hotline_button == null:
		close_hotline_button = _get_node_safe("InterfaceLayer/HotlineModal/HotlineVBox/HotlineActionsHBox/CloseHotlineButton") as Button

	if dossier_modal == null:
		dossier_modal = _get_node_safe("InterfaceLayer/DossierModal") as PanelContainer
	if close_dossier_x_button == null:
		close_dossier_x_button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierHeaderHBox/CloseDossierXButton") as Button
	if dossier_defense_label == null:
		dossier_defense_label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierDefenseLabel") as Label
	if dossier_crisis_label == null:
		dossier_crisis_label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierCrisisLabel") as Label
	if dossier_espionage_label == null:
		dossier_espionage_label = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierEspionageLabel") as Label
	if dossier_desk_button == null:
		dossier_desk_button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/DossierDeskButton") as Button
	if close_dossier_button == null:
		close_dossier_button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/CloseDossierButton") as Button

	if customisation_modal == null:
		customisation_modal = _get_node_safe("InterfaceLayer/CustomisationModal") as PanelContainer
	if close_customisation_x_button == null:
		close_customisation_x_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationHeaderHBox/CloseCustomisationXButton") as Button
	if customisation_treasury_label == null:
		customisation_treasury_label = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationHeaderHBox/CustomisationTreasuryLabel") as Label
	if tab_portraits_button == null:
		tab_portraits_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabPortraitsButton") as Button
	if tab_desks_button == null:
		tab_desks_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabDesksButton") as Button
	if tab_seating_button == null:
		tab_seating_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabSeatingButton") as Button
	if tab_walls_button == null:
		tab_walls_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabWallsButton") as Button
	if tab_carpets_button == null:
		tab_carpets_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabCarpetsButton") as Button
	if tab_bookshelves_button == null:
		tab_bookshelves_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabBookshelvesButton") as Button
	if tab_artwork_button == null:
		tab_artwork_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CategoryTabBarScroll/CategoryTabBarHBox/TabArtworkButton") as Button
	if catalog_grid == null:
		catalog_grid = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CatalogScroll/CatalogGrid") as VBoxContainer
	if customisation_status_label == null:
		customisation_status_label = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/CustomisationStatusLabel") as Label
	if save_customisation_button == null:
		save_customisation_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/SaveCustomisationButton") as Button
	if reset_customisation_button == null:
		reset_customisation_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/ResetCustomisationButton") as Button
	if close_customisation_button == null:
		close_customisation_button = _get_node_safe("InterfaceLayer/CustomisationModal/CustomisationVBox/CustomisationFooterHBox/CloseCustomisationButton") as Button

	if purchase_confirm_modal == null:
		purchase_confirm_modal = _get_node_safe("InterfaceLayer/PurchaseConfirmModal") as PanelContainer
	if purchase_item_name_label == null:
		purchase_item_name_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseItemNameLabel") as Label
	if purchase_item_desc_label == null:
		purchase_item_desc_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseItemDescLabel") as Label
	if purchase_price_label == null:
		purchase_price_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchasePriceLabel") as Label
	if purchase_approval_label == null:
		purchase_approval_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseApprovalLabel") as Label
	if purchase_install_time_label == null:
		purchase_install_time_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseInstallTimeLabel") as Label
	if purchase_treasury_impact_label == null:
		purchase_treasury_impact_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseTreasuryImpactLabel") as Label
	if purchase_notice_label == null:
		purchase_notice_label = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseNoticeLabel") as Label
	if confirm_purchase_button == null:
		confirm_purchase_button = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseActionsHBox/ConfirmPurchaseButton") as Button
	if cancel_purchase_button == null:
		cancel_purchase_button = _get_node_safe("InterfaceLayer/PurchaseConfirmModal/PurchaseConfirmVBox/PurchaseActionsHBox/CancelPurchaseButton") as Button

	if session_briefing_toast == null:
		session_briefing_toast = _get_node_safe("InterfaceLayer/SessionBriefingToast") as PanelContainer
	if session_briefing_title == null:
		session_briefing_title = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/ToastHeader/SessionBriefingTitle") as Label
	if dismiss_briefing_toast_button == null:
		dismiss_briefing_toast_button = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/ToastHeader/DismissBriefingToastButton") as Button
	if session_briefing_metrics_label == null:
		session_briefing_metrics_label = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/SessionBriefingMetricsLabel") as Label
	if session_briefing_status_label == null:
		session_briefing_status_label = _get_node_safe("InterfaceLayer/SessionBriefingToast/ToastVBox/SessionBriefingStatusLabel") as Label

func _ready() -> void:
	_init_controls()
	_connect_signals()
	_apply_tooltips()

	# Customisation initialization
	load_saved_customization()
	apply_customization(active_customization)

	# Setup 0.5s state polling
	poll_timer = Timer.new()
	poll_timer.wait_time = 0.5
	poll_timer.autostart = true
	poll_timer.timeout.connect(_on_poll_timer_timeout)
	add_child(poll_timer)

	# Session Briefing Timer
	session_briefing_timer = Timer.new()
	session_briefing_timer.wait_time = 5.0
	session_briefing_timer.one_shot = true
	session_briefing_timer.timeout.connect(dismiss_session_briefing)
	add_child(session_briefing_timer)

	# Dock Alert Flash Timer
	dock_flash_timer = Timer.new()
	dock_flash_timer.wait_time = 2.5
	dock_flash_timer.one_shot = true
	dock_flash_timer.timeout.connect(_on_dock_flash_timeout)
	add_child(dock_flash_timer)

	# Progressive Disclosure: Start docks collapsed
	collapse_all_docks()

	load_live_state()

func _process(delta: float) -> void:
	# 1. Subtle flag flutter / sway reacting to national stability
	if flag_banner:
		flag_sway_time += delta * flag_sway_frequency
		flag_banner.rotation_degrees = sin(flag_sway_time) * flag_sway_amplitude

	# 2. Subtle pulse on active desk notification badges
	var pulse := 0.70 + 0.30 * sin(Time.get_ticks_msec() * 0.006)
	if hotline_badge and hotline_badge.visible:
		hotline_badge.modulate.a = pulse
	if inbox_badge and inbox_badge.visible:
		inbox_badge.modulate.a = pulse
	if dossier_badge and dossier_badge.visible:
		dossier_badge.modulate.a = pulse

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		if purchase_confirm_modal and purchase_confirm_modal.visible:
			_close_purchase_confirm_modal()
			get_viewport().set_input_as_handled()
		elif customisation_modal and customisation_modal.visible:
			close_customisation_modal()
			get_viewport().set_input_as_handled()
		elif is_any_modal_open():
			close_all_modals()
			get_viewport().set_input_as_handled()

func _connect_signals() -> void:
	if switch_to_desk_button and not switch_to_desk_button.pressed.is_connected(_on_switch_to_desk_button_pressed):
		switch_to_desk_button.pressed.connect(_on_switch_to_desk_button_pressed)
	if refresh_button and not refresh_button.pressed.is_connected(_on_refresh_button_pressed):
		refresh_button.pressed.connect(_on_refresh_button_pressed)

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
	if nav_customise_button and not nav_customise_button.pressed.is_connected(_on_nav_customise_pressed):
		nav_customise_button.pressed.connect(_on_nav_customise_pressed)

	if hotline_phone and not hotline_phone.pressed.is_connected(_on_hotline_phone_pressed):
		hotline_phone.pressed.connect(_on_hotline_phone_pressed)
	if intelligence_dossier and not intelligence_dossier.pressed.is_connected(_on_intelligence_dossier_pressed):
		intelligence_dossier.pressed.connect(_on_intelligence_dossier_pressed)
	if executive_inbox and not executive_inbox.pressed.is_connected(_on_executive_inbox_pressed):
		executive_inbox.pressed.connect(_on_executive_inbox_pressed)
	if blotter_button and not blotter_button.pressed.is_connected(_on_blotter_button_pressed):
		blotter_button.pressed.connect(_on_blotter_button_pressed)
	if window_button and not window_button.pressed.is_connected(_on_capital_window_pressed):
		window_button.pressed.connect(_on_capital_window_pressed)
	if portrait_button and not portrait_button.pressed.is_connected(_on_presidential_portrait_pressed):
		portrait_button.pressed.connect(_on_presidential_portrait_pressed)

	if dismiss_briefing_toast_button and not dismiss_briefing_toast_button.pressed.is_connected(dismiss_session_briefing):
		dismiss_briefing_toast_button.pressed.connect(dismiss_session_briefing)
	if session_briefing_toast and not session_briefing_toast.gui_input.is_connected(_on_session_briefing_toast_gui_input):
		session_briefing_toast.gui_input.connect(_on_session_briefing_toast_gui_input)

	if left_dock and not left_dock.mouse_entered.is_connected(_on_left_dock_mouse_entered):
		left_dock.mouse_entered.connect(_on_left_dock_mouse_entered)
	if left_dock and not left_dock.mouse_exited.is_connected(_on_left_dock_mouse_exited):
		left_dock.mouse_exited.connect(_on_left_dock_mouse_exited)
	if right_dock and not right_dock.mouse_entered.is_connected(_on_right_dock_mouse_entered):
		right_dock.mouse_entered.connect(_on_right_dock_mouse_entered)
	if right_dock and not right_dock.mouse_exited.is_connected(_on_right_dock_mouse_exited):
		right_dock.mouse_exited.connect(_on_right_dock_mouse_exited)

	if toggle_left_dock_button and not toggle_left_dock_button.pressed.is_connected(_on_toggle_left_dock_pressed):
		toggle_left_dock_button.pressed.connect(_on_toggle_left_dock_pressed)
	if toggle_right_dock_button and not toggle_right_dock_button.pressed.is_connected(_on_toggle_right_dock_pressed):
		toggle_right_dock_button.pressed.connect(_on_toggle_right_dock_pressed)
	if open_cabinet_button and not open_cabinet_button.pressed.is_connected(_on_nav_cabinet_pressed):
		open_cabinet_button.pressed.connect(_on_nav_cabinet_pressed)
	if open_diplomacy_button and not open_diplomacy_button.pressed.is_connected(_on_nav_diplomacy_pressed):
		open_diplomacy_button.pressed.connect(_on_nav_diplomacy_pressed)
	if open_projects_button and not open_projects_button.pressed.is_connected(_on_nav_economy_pressed):
		open_projects_button.pressed.connect(_on_nav_economy_pressed)

	if modal_backdrop and not modal_backdrop.gui_input.is_connected(_on_modal_backdrop_gui_input):
		modal_backdrop.gui_input.connect(_on_modal_backdrop_gui_input)

	if close_briefing_x_button and not close_briefing_x_button.pressed.is_connected(close_all_modals):
		close_briefing_x_button.pressed.connect(close_all_modals)
	if close_briefing_button and not close_briefing_button.pressed.is_connected(close_all_modals):
		close_briefing_button.pressed.connect(close_all_modals)
	if briefing_desk_button and not briefing_desk_button.pressed.is_connected(_on_briefing_desk_button_pressed):
		briefing_desk_button.pressed.connect(_on_briefing_desk_button_pressed)

	if close_hotline_x_button and not close_hotline_x_button.pressed.is_connected(close_all_modals):
		close_hotline_x_button.pressed.connect(close_all_modals)
	if close_hotline_button and not close_hotline_button.pressed.is_connected(close_all_modals):
		close_hotline_button.pressed.connect(close_all_modals)
	if hotline_desk_button and not hotline_desk_button.pressed.is_connected(_on_hotline_desk_button_pressed):
		hotline_desk_button.pressed.connect(_on_hotline_desk_button_pressed)

	if close_dossier_x_button and not close_dossier_x_button.pressed.is_connected(close_all_modals):
		close_dossier_x_button.pressed.connect(close_all_modals)
	if close_dossier_button and not close_dossier_button.pressed.is_connected(close_all_modals):
		close_dossier_button.pressed.connect(close_all_modals)
	if dossier_desk_button and not dossier_desk_button.pressed.is_connected(_on_dossier_desk_button_pressed):
		dossier_desk_button.pressed.connect(_on_dossier_desk_button_pressed)

	# Customisation Modal Signals
	if close_customisation_x_button and not close_customisation_x_button.pressed.is_connected(close_customisation_modal):
		close_customisation_x_button.pressed.connect(close_customisation_modal)
	if close_customisation_button and not close_customisation_button.pressed.is_connected(close_customisation_modal):
		close_customisation_button.pressed.connect(close_customisation_modal)
	if save_customisation_button and not save_customisation_button.pressed.is_connected(save_current_customization):
		save_customisation_button.pressed.connect(save_current_customization)
	if reset_customisation_button and not reset_customisation_button.pressed.is_connected(reset_customization_to_default):
		reset_customisation_button.pressed.connect(reset_customization_to_default)

	if tab_portraits_button and not tab_portraits_button.pressed.is_connected(_on_tab_portraits_pressed):
		tab_portraits_button.pressed.connect(_on_tab_portraits_pressed)
	if tab_desks_button and not tab_desks_button.pressed.is_connected(_on_tab_desks_pressed):
		tab_desks_button.pressed.connect(_on_tab_desks_pressed)
	if tab_seating_button and not tab_seating_button.pressed.is_connected(_on_tab_seating_pressed):
		tab_seating_button.pressed.connect(_on_tab_seating_pressed)
	if tab_walls_button and not tab_walls_button.pressed.is_connected(_on_tab_walls_pressed):
		tab_walls_button.pressed.connect(_on_tab_walls_pressed)
	if tab_carpets_button and not tab_carpets_button.pressed.is_connected(_on_tab_carpets_pressed):
		tab_carpets_button.pressed.connect(_on_tab_carpets_pressed)
	if tab_bookshelves_button and not tab_bookshelves_button.pressed.is_connected(_on_tab_bookshelves_pressed):
		tab_bookshelves_button.pressed.connect(_on_tab_bookshelves_pressed)
	if tab_artwork_button and not tab_artwork_button.pressed.is_connected(_on_tab_artwork_pressed):
		tab_artwork_button.pressed.connect(_on_tab_artwork_pressed)

	# Purchase Modal Signals
	if confirm_purchase_button and not confirm_purchase_button.pressed.is_connected(confirm_purchase_pending_item):
		confirm_purchase_button.pressed.connect(confirm_purchase_pending_item)
	if cancel_purchase_button and not cancel_purchase_button.pressed.is_connected(_close_purchase_confirm_modal):
		cancel_purchase_button.pressed.connect(_close_purchase_confirm_modal)

func _apply_tooltips() -> void:
	if window_button:
		window_button.tooltip_text = "Capital Vista: Inspect current capital development and atmospheric lighting."
	if portrait_button:
		portrait_button.tooltip_text = "Presidential Portrait: View executive mandate and customize presidential portrait & flag."
	if hotline_phone:
		hotline_phone.tooltip_text = "Presidential Hotline: Access secure diplomatic lines to foreign heads of state."
	if intelligence_dossier:
		intelligence_dossier.tooltip_text = "Classified Dossier: Review defense readiness and sovereign threats."
	if executive_inbox:
		executive_inbox.tooltip_text = "Executive Memoranda: Review comprehensive presidential situation briefing."
	if blotter_button:
		blotter_button.tooltip_text = "Executive Blotter: Click to inspect presidential briefing or daily schedule."
	if nav_customise_button:
		nav_customise_button.tooltip_text = "Office Décor: Customise presidential portrait, flag, furniture, and interior architecture."

# ==============================================================================
# 5. STATE CONNECTOR & SYNCHRONIZATION
# ==============================================================================
func load_live_state() -> bool:
	var path_to_load := DEFAULT_STATE_FILE
	var cl_args := OS.get_cmdline_args()
	for arg in cl_args:
		if arg.begins_with("--state-file="):
			path_to_load = arg.split("=")[1].strip_edges()
			break

	var env_path := OS.get_environment("REPUBLIC_STATE_FILE")
	if not env_path.is_empty():
		path_to_load = env_path

	if not FileAccess.file_exists(path_to_load) and FileAccess.file_exists(USER_STATE_FILE):
		path_to_load = USER_STATE_FILE

	return load_state_from_file(path_to_load)

func load_state_from_file(file_path: String) -> bool:
	current_state_path = file_path
	if not FileAccess.file_exists(file_path):
		display_missing_state()
		return false

	var file := FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		display_missing_state()
		return false

	var text := file.get_as_text()
	file.close()

	if text.strip_edges().is_empty():
		display_missing_state()
		return false

	var json_res: Variant = JSON.parse_string(text)
	if not (json_res is Dictionary):
		display_corrupted_state()
		return false

	var state: Dictionary = json_res
	if state.is_empty():
		display_missing_state()
		return false

	apply_state(state)
	return true

func display_missing_state() -> void:
	latest_state = {}
	if country_label:
		country_label.text = "Country: [No State Loaded]"
	if treasury_label:
		treasury_label.text = "Treasury: [No State Loaded]"
	if approval_label:
		approval_label.text = "Approval: [No State Loaded]"
	if stability_label:
		stability_label.text = "Stability: [No State Loaded]"
	if defcon_label:
		defcon_label.text = "Defense: [No State Loaded]"
	if simulation_time_label:
		simulation_time_label.text = "Simulation Time: [No State Loaded]"

	if cabinet_summary_label:
		cabinet_summary_label.text = "Cabinet: [No State Loaded]"
	if civil_service_label:
		civil_service_label.text = "Civil Service Merit: [No State Loaded]"
	if corruption_control_label:
		corruption_control_label.text = "Corruption Control: [No State Loaded]"
	if ruling_party_label:
		ruling_party_label.text = "Ruling Party: [No State Loaded]"
	if polling_support_label:
		polling_support_label.text = "National Polling: [No State Loaded]"

	if military_readiness_label:
		military_readiness_label.text = "Readiness: [No State Loaded]"
	if active_crisis_label:
		active_crisis_label.text = "Active Crisis: [No State Loaded]"
	if gdp_label:
		gdp_label.text = "GDP: [No State Loaded]"
	if offline_briefing_label:
		offline_briefing_label.text = "Simulation: [No State Loaded]"

	if active_project_label:
		active_project_label.text = "Active Project: None"
	if status_ticker_label:
		status_ticker_label.text = "Executive Briefing: No Republic state loaded. Launch simulation watcher daemon."
	if state_status_label:
		state_status_label.text = "Status: [No State Loaded]"

	if customisation_treasury_label:
		customisation_treasury_label.text = "Available Treasury: [No State Loaded]"

	# Ensure decor plaque indicates uninitialized state
	if portrait_name_label:
		var p_id: String = str(active_customization.get("selected_portrait_id", "portrait_m1"))
		var p: Dictionary = ExecutiveOfficeCatalog.get_portrait(p_id)
		portrait_name_label.text = "PRES. " + str(p.get("name", "ALEXANDER VANCE")).to_upper()
	if portrait_party_label:
		portrait_party_label.text = "Party: [No State Loaded]"
	if flag_emblem_label:
		flag_emblem_label.text = "REPUBLIC"

	# Scenery and living visual feedback resets
	if city_scenery and city_scenery.has_method("set_state_metrics"):
		city_scenery.set_state_metrics(50.0, 50.0, "None", "None")
	update_portrait_state_feedback(50.0, 50.0, false)
	update_flag_state_feedback(50.0, false)
	set_hotline_badge(false)
	set_dossier_badge(false)
	set_inbox_badge(false)
	dismiss_session_briefing()

	state_cleared.emit()

func display_corrupted_state() -> void:
	display_missing_state()
	if status_ticker_label:
		status_ticker_label.text = "Executive Warning: Corrupted JSON encountered in simulation state file."
	if state_status_label:
		state_status_label.text = "Status: Corrupt State JSON"

func apply_state(state: Dictionary) -> void:
	latest_state = state

	# 1. Top HUD Bar: Live authoritatively connected values
	var country_name := str(state.get("country_name", "[No State Loaded]"))
	if country_label:
		country_label.text = "Country: " + country_name

	if treasury_label:
		var treas := str(state.get("treasury", ""))
		if treas.is_empty() and state.has("treasury_balance"):
			var bal: float = float(state.get("treasury_balance", 0.0))
			treas = "R " + _format_number_commas(bal)
		treasury_label.text = "Treasury: " + (treas if not treas.is_empty() else "[No State Loaded]")

	if customisation_treasury_label:
		var treas_val := str(state.get("treasury", ""))
		if treas_val.is_empty() and state.has("treasury_balance"):
			treas_val = "R " + _format_number_commas(float(state.get("treasury_balance", 0.0)))
		customisation_treasury_label.text = "Available Treasury: " + (treas_val if not treas_val.is_empty() else "[No State Loaded]")

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
		var defcon := str(state.get("defcon_level", ""))
		var mil_readiness := str(state.get("military_readiness", ""))
		var defense_str := ""
		if not defcon.is_empty() and not mil_readiness.is_empty():
			defense_str = "%s | %s" % [defcon, mil_readiness]
		elif not defcon.is_empty():
			defense_str = defcon
		elif not mil_readiness.is_empty():
			defense_str = mil_readiness
		else:
			defense_str = "[No State Loaded]"
		defcon_label.text = "Defense: " + defense_str

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

	# 5. Environment Decor Dynamic Updates (Respecting selected portrait & flag)
	if portrait_name_label:
		var p_id: String = str(active_customization.get("selected_portrait_id", "portrait_m1"))
		var p: Dictionary = ExecutiveOfficeCatalog.get_portrait(p_id)
		var p_name: String = str(p.get("name", "Alexander Vance")).to_upper()
		portrait_name_label.text = "PRES. " + p_name
	if portrait_party_label:
		portrait_party_label.text = "Party: " + str(state.get("ruling_party", "Independent Sovereign"))
	if flag_emblem_label:
		var code := country_name.replace("Federal Republic of ", "").replace("Republic of ", "").strip_edges()
		flag_emblem_label.text = code.to_upper() if not code.is_empty() else "REPUBLIC"

	# 6. Presidential Situation Briefing Modal Updates
	if briefing_active_project_label:
		var proj_name := str(state.get("active_project", "None"))
		briefing_active_project_label.text = "Active Project: " + proj_name + (" (In Progress)" if proj_name != "None" else " (Zero active infrastructure programs)")
	if briefing_decisions_label:
		var dec := str(state.get("latest_decision_result", ""))
		if dec.is_empty():
			dec = str(state.get("status_message", "No recent executive decrees issued."))
		briefing_decisions_label.text = "Latest Decision: " + dec
	if briefing_crisis_label:
		var cr := str(state.get("active_crisis", "None"))
		var cr_details := str(state.get("active_crisis_details", "State institutions stable. Sovereign order maintained."))
		briefing_crisis_label.text = "Crisis Status: %s — %s" % [cr, cr_details]
	if briefing_capacity_label:
		briefing_capacity_label.text = "State Capacity: " + str(state.get("state_capacity_summary", "Civil Service: 80% | Corruption: 30% | Infrastructure: 30%"))
	if briefing_fiscal_label:
		var fisc := str(state.get("fiscal_report_summary", "No quarterly fiscal reports recorded"))
		var treas := str(state.get("treasury", "[No State Loaded]"))
		briefing_fiscal_label.text = "Fiscal Report: %s | Liquid Reserves: %s" % [fisc, treas]
	if briefing_defense_label:
		briefing_defense_label.text = "Defense Posture: %s | %s" % [str(state.get("defcon_level", "DEFCON 5 (Peacetime Watch)")), str(state.get("military_readiness", "Armed Forces Operational"))]
	if briefing_offline_label:
		briefing_offline_label.text = str(state.get("offline_briefing_summary", "Offline Status: Real-time simulation aligned. Zero pending offline cycles."))
	if briefing_unsupported_label:
		briefing_unsupported_label.text = "Global Intelligence Wire: [Live simulation feed offline / pending support]"

	# 7. Hotline & Classified Dossier Modal Updates
	if hotline_status_label:
		hotline_status_label.text = "Secure diplomatic line operational. Connects directly to foreign heads of state."
	if hotline_standing_label:
		hotline_standing_label.text = "Diplomatic Standing: %s | Bilateral recognition & trade managed in Foreign Affairs." % str(state.get("status_message", "Connected to live simulation"))

	if dossier_defense_label:
		dossier_defense_label.text = "Defense Readiness: %s | %s" % [str(state.get("defcon_level", "DEFCON 5 (Peacetime Watch)")), str(state.get("military_readiness", "85.0% Operational"))]
	if dossier_crisis_label:
		dossier_crisis_label.text = "Active National Threat: %s — %s" % [str(state.get("active_crisis", "None")), str(state.get("active_crisis_details", "State institutions stable."))]
	if dossier_espionage_label:
		dossier_espionage_label.text = "Covert Intelligence Operations: [Feature not yet supported by simulation engine]"

	# 8. Living Capital Window Scenery: state-driven skyline reaction
	if city_scenery and city_scenery.has_method("set_state_metrics"):
		var app_val := float(state.get("approval_rating", 68.5))
		var stab_val := float(state.get("stability_rating", 85.0))
		var act_proj := str(state.get("active_project", "None"))
		var act_crisis := str(state.get("active_crisis", "None"))
		city_scenery.set_state_metrics(app_val, stab_val, act_proj, act_crisis)

	# 9. Soft Identity Feedback (Portrait Frame Glow & Flag Dynamic Sway)
	var appr_num := float(state.get("approval_rating", 68.5))
	var stab_num := float(state.get("stability_rating", 85.0))
	var cr_name := str(state.get("active_crisis", "None"))
	var has_crisis := (not cr_name.is_empty() and cr_name != "None" and cr_name != "[No State Loaded]")
	update_portrait_state_feedback(appr_num, stab_num, has_crisis)
	update_flag_state_feedback(stab_num, has_crisis)

	# 10. Desk Notification Badges
	var defcon := str(state.get("defcon_level", "DEFCON 5"))
	var mil_threat := (not defcon.begins_with("DEFCON 5") and not defcon.is_empty() and defcon != "[No State Loaded]")
	if has_crisis:
		if not hotline_badge_dismissed:
			set_hotline_badge(true, "!")
	else:
		set_hotline_badge(false)

	if mil_threat or has_crisis:
		if not dossier_badge_dismissed:
			set_dossier_badge(true, "!")
	else:
		set_dossier_badge(false)

	var dec_res := str(state.get("latest_decision_result", ""))
	var has_pending := dec_res.contains("Pending") or state.has("pending_decision")
	if has_pending:
		if not inbox_badge_dismissed:
			set_inbox_badge(true, "1")
	else:
		set_inbox_badge(false)

	# 11. Session Briefing Toast Update
	update_session_briefing(state)
	if session_briefing_toast and not session_briefing_toast.visible:
		show_session_briefing(true)

	# 12. Critical Alert Trigger
	if has_crisis:
		trigger_dock_alert("right")

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
# 6. PHASE 3 CUSTOMISATION & PERSISTENCE API
# ==============================================================================

## Loads saved customisation for the active country/session from persistent disk
func load_saved_customization(target_country_id: String = "") -> void:
	var c_id := target_country_id
	if c_id.is_empty():
		c_id = str(latest_state.get("country_id", "player-country"))
	active_customization = ExecutiveOfficeCatalog.load_customization(custom_save_path, c_id)
	apply_customization(active_customization)

## Saves current customisation choices to disk persistently
func save_current_customization() -> bool:
	var c_id := str(latest_state.get("country_id", "player-country"))
	var success := ExecutiveOfficeCatalog.save_customization(active_customization, custom_save_path, c_id)
	if success:
		if customisation_status_label:
			customisation_status_label.text = "Office Décor Saved: Design and portraits safely persisted to disk."
		if status_ticker_label:
			status_ticker_label.text = "Executive Office: Customised presidential office saved successfully."
		customization_saved.emit(active_customization)
	else:
		if customisation_status_label:
			customisation_status_label.text = "Save Error: Unable to write customisation file to disk."
	return success

## Applies an entire customisation dictionary to the decoupled visual layers
func apply_customization(custom_data: Dictionary) -> void:
	active_customization = custom_data

	# 1. Portrait
	var portrait_id: String = str(custom_data.get("selected_portrait_id", "portrait_m1"))
	set_portrait_by_id(portrait_id)

	# 2. Flag
	var flag_id: String = str(custom_data.get("selected_flag_id", "flag_navy_gold"))
	set_flag_by_id(flag_id)

	# 3. Furniture Categories
	var installed: Dictionary = custom_data.get("installed_items", {})
	for cat in installed.keys():
		var item_id: String = installed[cat]
		_apply_item_visuals(item_id)

	customization_applied.emit(active_customization)

## Updates the office with a specific presidential portrait by catalogue ID (5 male, 3 female)
func set_portrait_by_id(portrait_id: String) -> void:
	var p: Dictionary = ExecutiveOfficeCatalog.get_portrait(portrait_id)
	if p.is_empty():
		return

	active_customization["selected_portrait_id"] = portrait_id

	if portrait_name_label:
		portrait_name_label.text = "PRES. " + str(p.get("name", "")).to_upper()
	if portrait_placeholder_label:
		portrait_placeholder_label.text = str(p.get("archetype", "")).to_upper()
	if portrait_icon_label:
		portrait_icon_label.text = str(p.get("icon", ""))

	if portrait_canvas:
		var s := portrait_canvas.get_theme_stylebox("panel")
		var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
		new_s.bg_color = Color(p.get("accent_color", Color(0.15, 0.22, 0.36, 1.0)))
		portrait_canvas.add_theme_stylebox_override("panel", new_s)

## Updates the country flag using a catalogue flag ID
func set_flag_by_id(flag_id: String) -> void:
	var f: Dictionary = ExecutiveOfficeCatalog.get_flag(flag_id)
	if f.is_empty():
		return

	active_customization["selected_flag_id"] = flag_id

	if flag_banner:
		var s := flag_banner.get_theme_stylebox("panel")
		var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
		new_s.bg_color = Color(f.get("banner_color", Color(0.12, 0.22, 0.38, 1.0)))
		flag_banner.add_theme_stylebox_override("panel", new_s)

	if flag_emblem_label:
		var c_name := str(latest_state.get("country_name", "REPUBLIC"))
		var code := c_name.replace("Federal Republic of ", "").replace("Republic of ", "").strip_edges()
		flag_emblem_label.text = code.to_upper() if not code.is_empty() and code != "[No State Loaded]" else "REPUBLIC"

## Applies an item to the visual room layers
func _apply_item_visuals(item_id: String) -> void:
	var item: Dictionary = ExecutiveOfficeCatalog.get_item(item_id)
	if item.is_empty():
		return

	var cat: String = str(item.get("category", ""))
	match cat:
		"desk_and_chair":
			if desk_surface and item.has("surface_color"):
				var s := desk_surface.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.surface_color
				desk_surface.add_theme_stylebox_override("panel", new_s)
			if desk_blotter and item.has("blotter_color"):
				var s := desk_blotter.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.blotter_color
				desk_blotter.add_theme_stylebox_override("panel", new_s)
			if presidential_chair and item.has("chair_color"):
				var s := presidential_chair.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.chair_color
				presidential_chair.add_theme_stylebox_override("panel", new_s)

		"visitor_seating":
			if visitor_table and item.has("table_color"):
				var s := visitor_table.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.table_color
				visitor_table.add_theme_stylebox_override("panel", new_s)
			if visitor_chair1 and item.has("seating_color"):
				var s := visitor_chair1.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.seating_color
				visitor_chair1.add_theme_stylebox_override("panel", new_s)
			if visitor_chair2 and item.has("seating_color"):
				var s := visitor_chair2.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.seating_color
				visitor_chair2.add_theme_stylebox_override("panel", new_s)

		"wall_and_flooring":
			if wall_backdrop and item.has("wall_color"):
				wall_backdrop.color = item.wall_color
			if floor_backdrop and item.has("floor_color"):
				floor_backdrop.color = item.floor_color

		"carpets_and_lighting":
			if office_carpet and item.has("carpet_color"):
				var s := office_carpet.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.carpet_color
				office_carpet.add_theme_stylebox_override("panel", new_s)
			if chandelier_label and item.has("light_color"):
				chandelier_label.set("theme_override_colors/font_color", item.light_color)

		"bookshelves_and_decor":
			if bookshelf_visual and item.has("decor_color"):
				var s := bookshelf_visual.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.decor_color
				bookshelf_visual.add_theme_stylebox_override("panel", new_s)
			if bookshelf_icon_label and item.has("decor_symbol"):
				bookshelf_icon_label.text = item.decor_symbol
			if bookshelf_label and item.has("decor_title"):
				bookshelf_label.text = item.decor_title

		"artwork_and_gifts":
			if artwork_visual and item.has("art_color"):
				var s := artwork_visual.get_theme_stylebox("panel")
				var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
				new_s.bg_color = item.art_color
				artwork_visual.add_theme_stylebox_override("panel", new_s)
			if artwork_icon_label and item.has("art_symbol"):
				artwork_icon_label.text = item.art_symbol
			if artwork_label and item.has("art_title"):
				artwork_label.text = item.art_title

## Temporarily previews an item in the office without saving
func preview_furniture_item(item_id: String) -> void:
	_apply_item_visuals(item_id)
	var item: Dictionary = ExecutiveOfficeCatalog.get_item(item_id)
	var name_str := str(item.get("name", item_id))
	if customisation_status_label:
		customisation_status_label.text = "Previewing: %s. Click Install/Equip to apply." % name_str
	if status_ticker_label:
		status_ticker_label.text = "Office Décor Preview: Previewing %s." % name_str

## Temporarily previews a portrait
func preview_portrait(portrait_id: String) -> void:
	set_portrait_by_id(portrait_id)
	var p: Dictionary = ExecutiveOfficeCatalog.get_portrait(portrait_id)
	var name_str := str(p.get("name", portrait_id))
	if customisation_status_label:
		customisation_status_label.text = "Previewing Portrait: Pres. %s (%s)." % [name_str, p.get("archetype", "Leader")]
	if status_ticker_label:
		status_ticker_label.text = "Office Décor Preview: Previewing Pres. %s." % name_str

## Temporarily previews a flag
func preview_flag(flag_id: String) -> void:
	set_flag_by_id(flag_id)
	var f: Dictionary = ExecutiveOfficeCatalog.get_flag(flag_id)
	var name_str := str(f.get("name", flag_id))
	if customisation_status_label:
		customisation_status_label.text = "Previewing Flag: %s." % name_str
	if status_ticker_label:
		status_ticker_label.text = "Office Décor Preview: Previewing %s." % name_str

## Selects, equips, and persists an item
func select_and_install_item(item_id: String) -> void:
	var item: Dictionary = ExecutiveOfficeCatalog.get_item(item_id)
	if item.is_empty():
		return

	var cat: String = str(item.get("category", ""))
	var installed: Dictionary = active_customization.get("installed_items", {})
	installed[cat] = item_id
	active_customization["installed_items"] = installed

	_apply_item_visuals(item_id)
	save_current_customization()

	if customisation_status_label:
		customisation_status_label.text = "Installed: '%s' is now equipped in the presidential office." % str(item.get("name", item_id))
	if status_ticker_label:
		status_ticker_label.text = "Office Décor: Equipped %s." % str(item.get("name", item_id))

	_populate_catalog_category(current_customization_category)

## Selects, equips, and persists a portrait
func select_and_install_portrait(portrait_id: String) -> void:
	set_portrait_by_id(portrait_id)
	save_current_customization()
	var p: Dictionary = ExecutiveOfficeCatalog.get_portrait(portrait_id)
	if customisation_status_label:
		customisation_status_label.text = "Installed: Pres. %s is now displayed in the office." % str(p.get("name", portrait_id))
	if status_ticker_label:
		status_ticker_label.text = "Office Décor: Displaying portrait of Pres. %s." % str(p.get("name", portrait_id))
	_populate_catalog_category("portraits")

## Selects, equips, and persists a flag
func select_and_install_flag(flag_id: String) -> void:
	set_flag_by_id(flag_id)
	save_current_customization()
	var f: Dictionary = ExecutiveOfficeCatalog.get_flag(flag_id)
	if customisation_status_label:
		customisation_status_label.text = "Installed: Flag '%s' is now displayed on the stanchion." % f.name
	if status_ticker_label:
		status_ticker_label.text = "Office Décor: National flag updated to %s." % f.name
	_populate_catalog_category("portraits")

## Prompts purchase confirmation modal for an unowned item
func prompt_purchase_item(item: Dictionary) -> void:
	pending_purchase_item = item

	var price: int = int(item.get("price_repu", 0))
	var treas_val: float = float(latest_state.get("treasury_balance", 0.0))
	var remaining: float = treas_val - float(price)

	if purchase_item_name_label:
		purchase_item_name_label.text = "Item: " + str(item.get("name", "Unknown Item"))
	if purchase_item_desc_label:
		purchase_item_desc_label.text = str(item.get("description", ""))
	if purchase_price_label:
		purchase_price_label.text = "REPU Price: R " + _format_number_commas(price)
	if purchase_approval_label:
		purchase_approval_label.text = "Approval: " + str(item.get("approval_required", "Presidential Facilities Cleared"))
	if purchase_install_time_label:
		purchase_install_time_label.text = "Installation Time: " + str(item.get("install_time_wat", "Immediate"))
	if purchase_treasury_impact_label:
		purchase_treasury_impact_label.text = "Treasury: R %s | Projected: R %s" % [_format_number_commas(int(treas_val)), _format_number_commas(int(remaining))]

	if purchase_notice_label:
		purchase_notice_label.text = "Authoritative Accounting: Submits formal procurement order. Real simulation balance verified."

	if purchase_confirm_modal:
		purchase_confirm_modal.visible = true

func _close_purchase_confirm_modal() -> void:
	pending_purchase_item = {}
	if purchase_confirm_modal:
		purchase_confirm_modal.visible = false

## Executes purchase confirmation using authoritative economy validation
func confirm_purchase_pending_item() -> Dictionary:
	if pending_purchase_item.is_empty():
		return { "success": false, "message": "No pending purchase item selected." }

	var item := pending_purchase_item
	var owned: Array = active_customization.get("owned_items", [])
	var check := ExecutiveOfficeCatalog.check_item_availability(item, latest_state, owned)

	if not check.can_purchase and not check.can_install:
		if customisation_status_label:
			customisation_status_label.text = "Purchase Rejected: " + check.reason
		_close_purchase_confirm_modal()
		return { "success": false, "message": check.reason }

	# 1. Write authoritative request file to simulation pipeline
	var req_path := DEFAULT_STATE_FILE.replace("presidential_desk_state.json", "presidential_desk_request.json")
	var req_id := "req_furniture_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var item_id_str: String = str(item.get("id", ""))
	var item_cat_str: String = str(item.get("category", ""))
	var item_price_val: int = int(item.get("price_repu", 0))
	var item_name_str: String = str(item.get("name", "Unknown Item"))

	var request_data := {
		"request_id": req_id,
		"action": "purchase_furniture",
		"item_id": item_id_str,
		"category": item_cat_str,
		"price_repu": item_price_val,
		"country_name": latest_state.get("country_name", ""),
		"timestamp": Time.get_datetime_string_from_system(true)
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file:
		req_file.store_string(JSON.stringify(request_data, "\t"))
		req_file.close()

	# 2. Add to player's persistent owned items
	if not owned.has(item_id_str):
		owned.append(item_id_str)
		active_customization["owned_items"] = owned

	# 3. Install item and save
	_close_purchase_confirm_modal()
	select_and_install_item(item_id_str)

	var success_msg := "Procurement Order Confirmed: Acquired '%s' for R %s." % [item_name_str, _format_number_commas(item_price_val)]
	if customisation_status_label:
		customisation_status_label.text = success_msg
	if status_ticker_label:
		status_ticker_label.text = "Office Décor: " + success_msg

	return { "success": true, "message": success_msg }

## Resets all office customisation to default
func reset_customization_to_default() -> void:
	var def := ExecutiveOfficeCatalog.get_default_customization()
	apply_customization(def)
	save_current_customization()
	if customisation_status_label:
		customisation_status_label.text = "Office Décor: Reset to standard presidential office specifications."
	if status_ticker_label:
		status_ticker_label.text = "Executive Office: Reset office customisation to default."
	_populate_catalog_category(current_customization_category)

# ==============================================================================
# 7. DECOUPLED VISUAL LAYER CUSTOMIZATION API (PRESERVED)
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
# 8. INTERACTIVE NAVIGATION (GAME SECTIONS)
# ==============================================================================
func _on_switch_to_desk_button_pressed() -> void:
	navigate_to_section(Section.OVERVIEW)
	get_tree().change_scene_to_file("res://PresidentialDesk.tscn")

func _on_refresh_button_pressed() -> void:
	load_live_state()
	if status_ticker_label:
		status_ticker_label.text = "Executive Briefing: Refreshed live authoritative simulation state from disk."

func _on_nav_overview_pressed() -> void:
	navigate_to_section(Section.OVERVIEW)

func _on_nav_cabinet_pressed() -> void:
	navigate_to_section(Section.CABINET)

func _on_nav_economy_pressed() -> void:
	navigate_to_section(Section.ECONOMY)

func _on_nav_campaign_pressed() -> void:
	navigate_to_section(Section.CAMPAIGN)

func _on_nav_diplomacy_pressed() -> void:
	navigate_to_section(Section.DIPLOMACY)

func _on_nav_briefings_pressed() -> void:
	navigate_to_section(Section.BRIEFINGS)

func _on_nav_customise_pressed() -> void:
	open_customisation_modal("portraits")

## Navigates to a specific game section reusing existing PresidentialDesk functionality
func navigate_to_section(section: Section) -> void:
	active_nav_section = section
	_update_nav_button_highlights()

	if section == Section.OVERVIEW:
		close_all_modals()
		if status_ticker_label:
			status_ticker_label.text = "Executive Office: Main presidential workspace and situational telemetry."
		return

	if section == Section.BRIEFINGS:
		open_briefing_modal()
		return

	if section == Section.CUSTOMISE:
		open_customisation_modal()
		return

	# Reuses PresidentialDesk functionality without duplicating simulation logic
	var desk_script = load("res://PresidentialDesk.gd")
	if desk_script and "requested_section" in desk_script:
		desk_script.requested_section = int(section)
	get_tree().change_scene_to_file("res://PresidentialDesk.tscn")

func _update_nav_button_highlights() -> void:
	var buttons := [nav_overview_button, nav_cabinet_button, nav_economy_button, nav_campaign_button, nav_diplomacy_button, nav_briefings_button, nav_customise_button]
	for i in range(buttons.size()):
		var b: Button = buttons[i]
		if b:
			if i == int(active_nav_section):
				b.modulate = Color(1.0, 0.85, 0.3, 1.0)
			else:
				b.modulate = Color(0.85, 0.9, 0.95, 1.0)

# ==============================================================================
# 9. OFFICE CONTROLS & MODAL DIALOGS
# ==============================================================================
func _on_hotline_phone_pressed() -> void:
	set_hotline_badge(false)
	hotline_badge_dismissed = true
	open_hotline_modal()

func _on_executive_inbox_pressed() -> void:
	set_inbox_badge(false)
	inbox_badge_dismissed = true
	open_briefing_modal()

func _on_intelligence_dossier_pressed() -> void:
	set_dossier_badge(false)
	dossier_badge_dismissed = true
	open_dossier_modal()

func _on_blotter_button_pressed() -> void:
	if is_session_briefing_visible():
		dismiss_session_briefing()
	else:
		if latest_state.is_empty():
			load_live_state()
		update_session_briefing(latest_state)
		show_session_briefing(false)
	if status_ticker_label:
		status_ticker_label.text = "Executive Desk: Presidential blotter and schedule reviewed."

func _on_capital_window_pressed() -> void:
	var time_str := str(latest_state.get("simulation_time", "Day 0, 00:00 WAT"))
	var stab_str := str(latest_state.get("stability", "Stable"))
	if status_ticker_label:
		status_ticker_label.text = "Capital Vista: Overlooking Arcadia capital district. Simulation time: %s | Stability: %s." % [time_str, stab_str]

func _on_presidential_portrait_pressed() -> void:
	if status_ticker_label:
		status_ticker_label.text = "Presidential Mandate: Viewing head of state executive portrait and office customisation."
	open_customisation_modal("portraits")

func open_briefing_modal() -> void:
	close_all_modals()
	active_nav_section = Section.BRIEFINGS
	_update_nav_button_highlights()
	if modal_backdrop:
		modal_backdrop.visible = true
	if briefing_modal:
		briefing_modal.visible = true
	if status_ticker_label:
		status_ticker_label.text = "Executive Briefing: Presidential situation report and active memoranda opened."

func open_hotline_modal() -> void:
	close_all_modals()
	if modal_backdrop:
		modal_backdrop.visible = true
	if hotline_modal:
		hotline_modal.visible = true
	if status_ticker_label:
		status_ticker_label.text = "Hotline Console: Secure diplomatic channel opened."

func open_dossier_modal() -> void:
	close_all_modals()
	if modal_backdrop:
		modal_backdrop.visible = true
	if dossier_modal:
		dossier_modal.visible = true
	if status_ticker_label:
		status_ticker_label.text = "Intelligence Dossier: National security posture and military readiness reviewed."

func open_customisation_modal(category_id: String = "portraits") -> void:
	close_all_modals()
	active_nav_section = Section.CUSTOMISE
	_update_nav_button_highlights()
	if modal_backdrop:
		modal_backdrop.visible = true
	if customisation_modal:
		customisation_modal.visible = true

	# Update live treasury balance display in modal header
	if customisation_treasury_label:
		var treas := str(latest_state.get("treasury", ""))
		if treas.is_empty() and latest_state.has("treasury_balance"):
			treas = "R " + _format_number_commas(float(latest_state.get("treasury_balance", 0.0)))
		customisation_treasury_label.text = "Available Treasury: " + (treas if not treas.is_empty() else "[No State Loaded]")

	_populate_catalog_category(category_id)

func close_customisation_modal() -> void:
	if customisation_modal:
		customisation_modal.visible = false
	if purchase_confirm_modal:
		purchase_confirm_modal.visible = false
	if modal_backdrop:
		modal_backdrop.visible = false
	active_nav_section = Section.OVERVIEW
	_update_nav_button_highlights()

func close_all_modals() -> void:
	if modal_backdrop:
		modal_backdrop.visible = false
	if briefing_modal:
		briefing_modal.visible = false
	if hotline_modal:
		hotline_modal.visible = false
	if dossier_modal:
		dossier_modal.visible = false
	if customisation_modal:
		customisation_modal.visible = false
	if purchase_confirm_modal:
		purchase_confirm_modal.visible = false
	active_nav_section = Section.OVERVIEW
	_update_nav_button_highlights()

func is_any_modal_open() -> bool:
	return (briefing_modal and briefing_modal.visible) or \
		(hotline_modal and hotline_modal.visible) or \
		(dossier_modal and dossier_modal.visible) or \
		(customisation_modal and customisation_modal.visible) or \
		(purchase_confirm_modal and purchase_confirm_modal.visible)

func _on_modal_backdrop_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if purchase_confirm_modal and purchase_confirm_modal.visible:
			_close_purchase_confirm_modal()
		else:
			close_all_modals()

func _on_briefing_desk_button_pressed() -> void:
	navigate_to_section(Section.BRIEFINGS)

func _on_hotline_desk_button_pressed() -> void:
	navigate_to_section(Section.DIPLOMACY)

func _on_dossier_desk_button_pressed() -> void:
	navigate_to_section(Section.BRIEFINGS)

func _on_toggle_left_dock_pressed() -> void:
	var now_expanded := not is_left_dock_expanded()
	set_left_dock_expanded(now_expanded)
	left_dock_pinned = now_expanded

func _on_toggle_right_dock_pressed() -> void:
	var now_expanded := not is_right_dock_expanded()
	set_right_dock_expanded(now_expanded)
	right_dock_pinned = now_expanded

func _on_left_dock_mouse_entered() -> void:
	set_left_dock_expanded(true)

func _on_left_dock_mouse_exited() -> void:
	if not left_dock_pinned:
		set_left_dock_expanded(false)

func _on_right_dock_mouse_entered() -> void:
	set_right_dock_expanded(true)

func _on_right_dock_mouse_exited() -> void:
	if not right_dock_pinned:
		set_right_dock_expanded(false)

# ==============================================================================
# PROGRESSIVE DISCLOSURE & DOCK MANAGEMENT
# ==============================================================================
func collapse_all_docks() -> void:
	set_left_dock_expanded(false)
	set_right_dock_expanded(false)

func set_left_dock_expanded(expanded: bool) -> void:
	if left_dock:
		var content := left_dock.find_child("LeftDockContent", true, false) as Control
		if content:
			content.visible = expanded
		if toggle_left_dock_button:
			toggle_left_dock_button.text = "▲" if expanded else "▼"

func set_right_dock_expanded(expanded: bool) -> void:
	if right_dock:
		var content := right_dock.find_child("RightDockContent", true, false) as Control
		if content:
			content.visible = expanded
		if toggle_right_dock_button:
			toggle_right_dock_button.text = "▲" if expanded else "▼"

func is_left_dock_expanded() -> bool:
	if left_dock:
		var content := left_dock.find_child("LeftDockContent", true, false) as Control
		return content.visible if content else false
	return false

func is_right_dock_expanded() -> bool:
	if right_dock:
		var content := right_dock.find_child("RightDockContent", true, false) as Control
		return content.visible if content else false
	return false

func trigger_dock_alert(dock_id: String) -> void:
	var target: PanelContainer = null
	if dock_id == "left" or dock_id == "domestic":
		set_left_dock_expanded(true)
		target = left_dock
	elif dock_id == "right" or dock_id == "foreign" or dock_id == "crisis":
		set_right_dock_expanded(true)
		target = right_dock
	elif dock_id == "bottom" or dock_id == "project":
		target = bottom_dock

	if target:
		flashing_dock = target
		var s := target.get_theme_stylebox("panel")
		var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
		new_s.border_color = Color(0.95, 0.75, 0.2, 0.95)
		target.add_theme_stylebox_override("panel", new_s)
		if dock_flash_timer:
			dock_flash_timer.start(2.5)

func _on_dock_flash_timeout() -> void:
	if flashing_dock:
		var s := flashing_dock.get_theme_stylebox("panel")
		var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
		new_s.border_color = Color(0.2, 0.28, 0.4, 0.7)
		flashing_dock.add_theme_stylebox_override("panel", new_s)
		flashing_dock = null

# ==============================================================================
# DESK NOTIFICATION BADGES API
# ==============================================================================
func set_hotline_badge(visible_state: bool, badge_text: String = "!") -> void:
	if hotline_badge:
		hotline_badge.visible = visible_state
	if hotline_badge_label and not badge_text.is_empty():
		hotline_badge_label.text = badge_text

func set_dossier_badge(visible_state: bool, badge_text: String = "!") -> void:
	if dossier_badge:
		dossier_badge.visible = visible_state
	if dossier_badge_label and not badge_text.is_empty():
		dossier_badge_label.text = badge_text

func set_inbox_badge(visible_state: bool, badge_text: String = "1") -> void:
	if inbox_badge:
		inbox_badge.visible = visible_state
	if inbox_badge_label and not badge_text.is_empty():
		inbox_badge_label.text = badge_text

func is_hotline_badge_visible() -> bool:
	return hotline_badge != null and hotline_badge.visible

func is_dossier_badge_visible() -> bool:
	return dossier_badge != null and dossier_badge.visible

func is_inbox_badge_visible() -> bool:
	return inbox_badge != null and inbox_badge.visible

# ==============================================================================
# SOFT IDENTITY FEEDBACK (PORTRAIT & FLAG)
# ==============================================================================
func update_portrait_state_feedback(approval: float, stability: float, has_crisis: bool = false) -> void:
	if portrait_frame:
		var s := portrait_frame.get_theme_stylebox("panel")
		var new_s := (s.duplicate() if s else StyleBoxFlat.new()) as StyleBoxFlat
		if not has_crisis and approval >= 65.0 and stability >= 60.0:
			new_s.border_color = Color(0.95, 0.80, 0.25, 0.95) # warm radiant gold
			new_s.shadow_color = Color(0.85, 0.65, 0.15, 0.45)
			new_s.shadow_size = 8
			if portrait_canvas:
				portrait_canvas.modulate = Color(1.05, 1.02, 0.98, 1.0)
		elif not has_crisis and approval >= 45.0 and stability >= 45.0:
			new_s.border_color = Color(0.50, 0.58, 0.70, 0.85) # slate neutral
			new_s.shadow_color = Color(0.1, 0.15, 0.25, 0.3)
			new_s.shadow_size = 4
			if portrait_canvas:
				portrait_canvas.modulate = Color(0.92, 0.92, 0.95, 1.0)
		else:
			new_s.border_color = Color(0.38, 0.32, 0.32, 0.75) # dimmed somber charcoal
			new_s.shadow_color = Color(0.0, 0.0, 0.0, 0.0)
			new_s.shadow_size = 0
			if portrait_canvas:
				portrait_canvas.modulate = Color(0.68, 0.68, 0.72, 1.0) # desaturated/shadowed
		portrait_frame.add_theme_stylebox_override("panel", new_s)

func update_flag_state_feedback(stability: float, has_crisis: bool = false) -> void:
	if flag_banner:
		if not has_crisis and stability >= 70.0:
			flag_banner.modulate = Color(1.05, 1.05, 1.05, 1.0)
			flag_sway_amplitude = 1.2
			flag_sway_frequency = 1.5
		elif not has_crisis and stability >= 45.0:
			flag_banner.modulate = Color(0.88, 0.88, 0.90, 1.0)
			flag_sway_amplitude = 1.8
			flag_sway_frequency = 2.0
		else:
			flag_banner.modulate = Color(0.68, 0.65, 0.70, 1.0)
			flag_sway_amplitude = 2.6
			flag_sway_frequency = 2.8

# ==============================================================================
# SESSION BRIEFING TOAST API
# ==============================================================================
func update_session_briefing(state: Dictionary) -> void:
	if session_briefing_metrics_label:
		var c_name := str(state.get("country_name", "Republic"))
		var treas := str(state.get("treasury", ""))
		if treas.is_empty() and state.has("treasury_balance"):
			treas = "R " + _format_number_commas(float(state.get("treasury_balance", 0.0)))
		var appr := str(state.get("approval", ""))
		if appr.is_empty() and state.has("approval_rating"):
			appr = str(state.get("approval_rating", 0.0)) + "%"
		var stab := str(state.get("stability", ""))
		if stab.is_empty() and state.has("stability_rating"):
			stab = str(state.get("stability_rating", 0.0)) + "%"
		
		session_briefing_metrics_label.text = "%s | Liquid Reserves: %s | Approval: %s | Stability: %s" % [
			c_name,
			(treas if not treas.is_empty() else "[No State Loaded]"),
			(appr if not appr.is_empty() else "[No State Loaded]"),
			(stab if not stab.is_empty() else "[No State Loaded]")
		]

	if session_briefing_status_label:
		var proj := str(state.get("active_project", "None"))
		var cr := str(state.get("active_crisis", "None"))
		var msg := str(state.get("latest_decision_result", ""))
		if msg.is_empty():
			msg = str(state.get("status_message", "Executive office online."))
		session_briefing_status_label.text = "Active Program: %s | Crisis Posture: %s | Status: %s" % [proj, cr, msg]

func show_session_briefing(auto_dismiss: bool = true) -> void:
	if session_briefing_toast:
		session_briefing_toast.visible = true
	if auto_dismiss:
		if session_briefing_timer:
			session_briefing_timer.start(5.0)

func dismiss_session_briefing() -> void:
	if session_briefing_toast:
		session_briefing_toast.visible = false
	if session_briefing_timer and not session_briefing_timer.is_stopped():
		session_briefing_timer.stop()

func is_session_briefing_visible() -> bool:
	return session_briefing_toast != null and session_briefing_toast.visible

func _on_session_briefing_toast_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		dismiss_session_briefing()

# ==============================================================================
# 10. CATALOG VIEW POPULATION & USER ACTIONS
# ==============================================================================
func _on_tab_portraits_pressed() -> void:
	_populate_catalog_category("portraits")

func _on_tab_desks_pressed() -> void:
	_populate_catalog_category("desk_and_chair")

func _on_tab_seating_pressed() -> void:
	_populate_catalog_category("visitor_seating")

func _on_tab_walls_pressed() -> void:
	_populate_catalog_category("wall_and_flooring")

func _on_tab_carpets_pressed() -> void:
	_populate_catalog_category("carpets_and_lighting")

func _on_tab_bookshelves_pressed() -> void:
	_populate_catalog_category("bookshelves_and_decor")

func _on_tab_artwork_pressed() -> void:
	_populate_catalog_category("artwork_and_gifts")

func _update_tab_button_highlights(selected_cat: String) -> void:
	var tabs := {
		"portraits": tab_portraits_button,
		"desk_and_chair": tab_desks_button,
		"visitor_seating": tab_seating_button,
		"wall_and_flooring": tab_walls_button,
		"carpets_and_lighting": tab_carpets_button,
		"bookshelves_and_decor": tab_bookshelves_button,
		"artwork_and_gifts": tab_artwork_button
	}
	for cat_key in tabs.keys():
		var b: Button = tabs[cat_key]
		if b:
			if cat_key == selected_cat:
				b.modulate = Color(1.0, 0.85, 0.3, 1.0)
			else:
				b.modulate = Color(0.85, 0.9, 0.95, 1.0)

func _populate_catalog_category(category_id: String) -> void:
	current_customization_category = category_id
	_update_tab_button_highlights(category_id)

	if catalog_grid == null:
		return

	# Clear previous children
	for child in catalog_grid.get_children():
		child.queue_free()

	if category_id == "portraits":
		_populate_portraits_and_flags()
		return

	var items: Array = ExecutiveOfficeCatalog.get_items_by_category(category_id)
	var owned: Array = active_customization.get("owned_items", [])
	var installed: Dictionary = active_customization.get("installed_items", {})
	var currently_installed: String = installed.get(category_id, "")

	for it in items:
		var card := _create_furniture_card(it, currently_installed, owned)
		catalog_grid.add_child(card)

func _populate_portraits_and_flags() -> void:
	var portraits: Array = ExecutiveOfficeCatalog.get_portraits()
	var selected_p_id: String = str(active_customization.get("selected_portrait_id", "portrait_m1"))

	# Section Header: Selectable Presidential Portraits (5 male, 3 female)
	var p_hdr := Label.new()
	p_hdr.text = "👑 SELECTABLE PRESIDENTIAL PORTRAITS (5 MALE, 3 FEMALE)"
	p_hdr.set("theme_override_colors/font_color", Color(0.92, 0.70, 0.03, 1.0))
	p_hdr.set("theme_override_font_sizes/font_size", 12)
	catalog_grid.add_child(p_hdr)

	for p in portraits:
		var p_card := _create_portrait_card(p, selected_p_id)
		catalog_grid.add_child(p_card)

	# Section Header: National Flags
	var f_sep := HSeparator.new()
	catalog_grid.add_child(f_sep)

	var f_hdr := Label.new()
	f_hdr.text = "🚩 NATIONAL FLAGS & HERALDRY"
	f_hdr.set("theme_override_colors/font_color", Color(0.92, 0.70, 0.03, 1.0))
	f_hdr.set("theme_override_font_sizes/font_size", 12)
	catalog_grid.add_child(f_hdr)

	var flags: Array = ExecutiveOfficeCatalog.get_flags()
	var selected_f_id: String = str(active_customization.get("selected_flag_id", "flag_navy_gold"))
	for f in flags:
		var f_card := _create_flag_card(f, selected_f_id)
		catalog_grid.add_child(f_card)

func _create_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.06, 0.08, 0.12, 0.92)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.25, 0.35, 0.5, 0.7)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.content_margin_left = 12.0
	style.content_margin_top = 8.0
	style.content_margin_right = 12.0
	style.content_margin_bottom = 8.0
	return style

func _create_furniture_card(item: Dictionary, installed_id: String, owned: Array) -> PanelContainer:
	var card := PanelContainer.new()
	card.add_theme_stylebox_override("panel", _create_card_style())

	var hbox := HBoxContainer.new()
	hbox.set("theme_override_constants/separation", 14)
	card.add_child(hbox)

	# Info VBox
	var info_vbox := VBoxContainer.new()
	info_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	info_vbox.set("theme_override_constants/separation", 3)
	hbox.add_child(info_vbox)

	var title_lbl := Label.new()
	title_lbl.text = item.name
	title_lbl.set("theme_override_font_sizes/font_size", 12)
	title_lbl.set("theme_override_colors/font_color", Color(0.95, 0.88, 0.75, 1.0))
	info_vbox.add_child(title_lbl)

	var desc_lbl := Label.new()
	desc_lbl.text = item.description
	desc_lbl.set("theme_override_font_sizes/font_size", 10)
	desc_lbl.set("theme_override_colors/font_color", Color(0.7, 0.75, 0.85, 1.0))
	desc_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	info_vbox.add_child(desc_lbl)

	var meta_lbl := Label.new()
	var price_val: int = int(item.get("price_repu", 0))
	var price_str := "Price: FREE (Default)" if price_val == 0 else ("Price: R " + _format_number_commas(price_val) + " REPU")
	var req_appr: String = item.get("approval_required", "None")
	meta_lbl.text = "%s | Approval: %s | Time: %s" % [price_str, req_appr, item.get("install_time_wat", "Immediate")]
	meta_lbl.set("theme_override_font_sizes/font_size", 10)
	meta_lbl.set("theme_override_colors/font_color", Color(0.85, 0.70, 0.35, 1.0))
	info_vbox.add_child(meta_lbl)

	# Actions VBox
	var act_vbox := VBoxContainer.new()
	act_vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	act_vbox.set("theme_override_constants/separation", 4)
	hbox.add_child(act_vbox)

	var is_installed: bool = (str(item.get("id", "")) == installed_id)
	var is_owned: bool = owned.has(str(item.get("id", ""))) or price_val == 0
	var check: Dictionary = ExecutiveOfficeCatalog.check_item_availability(item, latest_state, owned)

	var preview_btn := Button.new()
	preview_btn.text = "👁 Preview"
	preview_btn.set("theme_override_font_sizes/font_size", 10)
	var item_id_copy: String = str(item.get("id", ""))
	preview_btn.pressed.connect(func(): preview_furniture_item(item_id_copy))
	act_vbox.add_child(preview_btn)

	var main_btn := Button.new()
	main_btn.set("theme_override_font_sizes/font_size", 11)

	if is_installed:
		main_btn.text = "✔ Equipped"
		main_btn.disabled = true
		main_btn.modulate = Color(0.6, 0.9, 0.6, 1.0)
	elif is_owned:
		main_btn.text = "Equip / Install"
		main_btn.pressed.connect(func(): select_and_install_item(item_id_copy))
	elif check.can_purchase:
		main_btn.text = "Acquire (R %s)" % _format_number_commas(price_val)
		main_btn.pressed.connect(func(): prompt_purchase_item(item))
	else:
		main_btn.text = "🔒 Locked"
		main_btn.disabled = true
		main_btn.tooltip_text = check.reason

	act_vbox.add_child(main_btn)
	return card

func _create_portrait_card(p: Dictionary, selected_id: String) -> PanelContainer:
	var card := PanelContainer.new()
	card.add_theme_stylebox_override("panel", _create_card_style())

	var hbox := HBoxContainer.new()
	hbox.set("theme_override_constants/separation", 14)
	card.add_child(hbox)

	var icon_lbl := Label.new()
	icon_lbl.text = str(p.get("icon", "👤"))
	icon_lbl.set("theme_override_font_sizes/font_size", 28)
	hbox.add_child(icon_lbl)

	var info_vbox := VBoxContainer.new()
	info_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	info_vbox.set("theme_override_constants/separation", 2)
	hbox.add_child(info_vbox)

	var p_name: String = str(p.get("name", "President"))
	var p_gender: String = str(p.get("gender", "")).capitalize()
	var p_archetype: String = str(p.get("archetype", ""))
	var p_desc: String = str(p.get("description", ""))
	var p_id: String = str(p.get("id", ""))

	var title_lbl := Label.new()
	title_lbl.text = "Pres. %s (%s) — %s" % [p_name, p_gender, p_archetype]
	title_lbl.set("theme_override_font_sizes/font_size", 12)
	title_lbl.set("theme_override_colors/font_color", Color(0.95, 0.88, 0.75, 1.0))
	info_vbox.add_child(title_lbl)

	var desc_lbl := Label.new()
	desc_lbl.text = p_desc
	desc_lbl.set("theme_override_font_sizes/font_size", 10)
	desc_lbl.set("theme_override_colors/font_color", Color(0.7, 0.75, 0.85, 1.0))
	desc_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	info_vbox.add_child(desc_lbl)

	var act_vbox := VBoxContainer.new()
	act_vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	act_vbox.set("theme_override_constants/separation", 4)
	hbox.add_child(act_vbox)

	var preview_btn := Button.new()
	preview_btn.text = "👁 Preview"
	preview_btn.set("theme_override_font_sizes/font_size", 10)
	var p_id_copy: String = p_id
	preview_btn.pressed.connect(func(): preview_portrait(p_id_copy))
	act_vbox.add_child(preview_btn)

	var select_btn := Button.new()
	select_btn.set("theme_override_font_sizes/font_size", 11)
	if p_id == selected_id:
		select_btn.text = "✔ Selected"
		select_btn.disabled = true
		select_btn.modulate = Color(0.6, 0.9, 0.6, 1.0)
	else:
		select_btn.text = "Select Portrait"
		select_btn.pressed.connect(func(): select_and_install_portrait(p_id_copy))
	act_vbox.add_child(select_btn)

	return card

func _create_flag_card(f: Dictionary, selected_id: String) -> PanelContainer:
	var card := PanelContainer.new()
	card.add_theme_stylebox_override("panel", _create_card_style())

	var hbox := HBoxContainer.new()
	hbox.set("theme_override_constants/separation", 14)
	card.add_child(hbox)

	var swatch := ColorRect.new()
	swatch.custom_minimum_size = Vector2(36, 24)
	swatch.color = f.get("banner_color", Color.WHITE)
	hbox.add_child(swatch)

	var info_vbox := VBoxContainer.new()
	info_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	info_vbox.set("theme_override_constants/separation", 2)
	hbox.add_child(info_vbox)

	var f_name: String = str(f.get("name", "Flag"))
	var f_id: String = str(f.get("id", ""))

	var title_lbl := Label.new()
	title_lbl.text = f_name
	title_lbl.set("theme_override_font_sizes/font_size", 12)
	title_lbl.set("theme_override_colors/font_color", Color(0.95, 0.88, 0.75, 1.0))
	info_vbox.add_child(title_lbl)

	var c_name := str(latest_state.get("country_name", "Republic"))
	var desc_lbl := Label.new()
	desc_lbl.text = "Banners emblazoned with sovereign country identity [%s]." % c_name
	desc_lbl.set("theme_override_font_sizes/font_size", 10)
	desc_lbl.set("theme_override_colors/font_color", Color(0.7, 0.75, 0.85, 1.0))
	info_vbox.add_child(desc_lbl)

	var act_vbox := VBoxContainer.new()
	act_vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	act_vbox.set("theme_override_constants/separation", 4)
	hbox.add_child(act_vbox)

	var preview_btn := Button.new()
	preview_btn.text = "👁 Preview"
	preview_btn.set("theme_override_font_sizes/font_size", 10)
	var f_id_copy: String = f_id
	preview_btn.pressed.connect(func(): preview_flag(f_id_copy))
	act_vbox.add_child(preview_btn)

	var select_btn := Button.new()
	select_btn.set("theme_override_font_sizes/font_size", 11)
	if f_id == selected_id:
		select_btn.text = "✔ Selected"
		select_btn.disabled = true
		select_btn.modulate = Color(0.6, 0.9, 0.6, 1.0)
	else:
		select_btn.text = "Select Flag"
		select_btn.pressed.connect(func(): select_and_install_flag(f_id_copy))
	act_vbox.add_child(select_btn)

	return card
