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
@onready var active_project_marker_button: Button = _get_node_safe("OfficeEnvironment/CapitalWindow/ActiveProjectMarkerButton") as Button
@onready var completed_project_marker_button: Button = _get_node_safe("OfficeEnvironment/CapitalWindow/CompletedProjectMarkerButton") as Button

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
@onready var diplomatic_folder: Button = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton") as Button
@onready var diplomatic_badge: Control = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton/DiplomaticNotificationBadge") as Control
@onready var diplomatic_badge_label: Label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton/DiplomaticNotificationBadge/DiplomaticBadgeLabel") as Label
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

# Phase 4 Cabinet & Ministries Dock References
@onready var cabinet_scroll: ScrollContainer = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll") as ScrollContainer
@onready var cabinet_portfolios_vbox: VBoxContainer = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox") as VBoxContainer

# Finance Portfolio Card
@onready var finance_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance") as Control
@onready var finance_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceHeaderHBox/FinanceStatusLabel") as Label
@onready var finance_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceMinisterLabel") as Label
@onready var finance_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceRatingsLabel") as Label
@onready var finance_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceEffectLabel") as Label
@onready var appoint_finance_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/AppointFinanceButton") as Button

# Defense Portfolio Card
@onready var defense_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense") as Control
@onready var defense_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseHeaderHBox/DefenseStatusLabel") as Label
@onready var defense_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseMinisterLabel") as Label
@onready var defense_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseRatingsLabel") as Label
@onready var defense_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseEffectLabel") as Label

# Foreign Affairs Portfolio Card
@onready var foreign_affairs_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs") as Control
@onready var foreign_affairs_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsHeaderHBox/ForeignAffairsStatusLabel") as Label
@onready var foreign_affairs_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsMinisterLabel") as Label
@onready var foreign_affairs_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsRatingsLabel") as Label
@onready var foreign_affairs_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsEffectLabel") as Label

# Interior Portfolio Card
@onready var interior_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior") as Control
@onready var interior_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorHeaderHBox/InteriorStatusLabel") as Label
@onready var interior_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorMinisterLabel") as Label
@onready var interior_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorRatingsLabel") as Label
@onready var interior_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorEffectLabel") as Label
@onready var appoint_interior_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/AppointInteriorButton") as Button

# Health Portfolio Card
@onready var health_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health") as Control
@onready var health_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthHeaderHBox/HealthStatusLabel") as Label
@onready var health_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthMinisterLabel") as Label
@onready var health_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthRatingsLabel") as Label
@onready var health_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthEffectLabel") as Label

# Infrastructure Portfolio Card
@onready var infrastructure_card: Control = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure") as Control
@onready var infrastructure_status_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureHeaderHBox/InfrastructureStatusLabel") as Label
@onready var infrastructure_minister_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureMinisterLabel") as Label
@onready var infrastructure_ratings_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureRatingsLabel") as Label
@onready var infrastructure_effect_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureEffectLabel") as Label
@onready var appoint_infrastructure_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/AppointInfrastructureButton") as Button

# Left Dock Bottlenecks & Friction
@onready var finance_bottleneck_badge: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceHeaderHBox/FinanceBottleneckBadge") as Label
@onready var interior_bottleneck_badge: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorHeaderHBox/InteriorBottleneckBadge") as Label
@onready var infrastructure_bottleneck_badge: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureHeaderHBox/InfrastructureBottleneckBadge") as Label
@onready var cabinet_friction_panel: PanelContainer = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel") as PanelContainer
@onready var cabinet_friction_label: Label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel/FrictionVBox/CabinetFrictionLabel") as Label
@onready var resolve_friction_button: Button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel/FrictionVBox/ResolveFrictionButton") as Button

# Right Dock: Foreign Affairs & Intelligence
@onready var right_dock: PanelContainer = _get_node_safe("InterfaceLayer/RightDock") as PanelContainer
@onready var toggle_right_dock_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockHeader/ToggleRightDockButton") as Button
@onready var military_readiness_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/MilitaryReadinessLabel") as Label
@onready var active_crisis_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/ActiveCrisisLabel") as Label
@onready var gdp_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/GdpLabel") as Label
@onready var offline_briefing_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OfflineBriefingLabel") as Label
@onready var open_diplomacy_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/OpenDiplomacyButton") as Button

# Right Dock Diplomatic Dossier
@onready var diplomatic_dossier_title: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierTitleLabel") as Label
@onready var diplomatic_dossier_card: PanelContainer = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard") as PanelContainer
@onready var dossier_partner_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierPartnerLabel") as Label
@onready var dossier_terms_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierTermsLabel") as Label
@onready var accept_dossier_proposal_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierCardActionsHBox/AcceptDossierProposalButton") as Button
@onready var reject_dossier_proposal_button: Button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierCardActionsHBox/RejectDossierProposalButton") as Button
@onready var dossier_status_label: Label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierStatusLabel") as Label

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

# Project Completion Toast
@onready var project_completion_toast: PanelContainer = _get_node_safe("InterfaceLayer/ProjectCompletionToast") as PanelContainer
@onready var project_completion_title: Label = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ToastHeader/ProjectCompletionTitle") as Label
@onready var dismiss_project_toast_button: Button = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ToastHeader/DismissProjectToastButton") as Button
@onready var project_completion_details_label: Label = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ProjectCompletionDetailsLabel") as Label

# Capital Project Card
@onready var capital_project_card: PanelContainer = _get_node_safe("InterfaceLayer/CapitalProjectCard") as PanelContainer
@onready var capital_project_title: Label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardHeader/CapitalProjectTitle") as Label
@onready var close_capital_project_button: Button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardHeader/CloseCapitalProjectButton") as Button
@onready var capital_project_status_label: Label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectStatusLabel") as Label
@onready var capital_project_eta_label: Label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectEtaLabel") as Label
@onready var capital_project_capacity_label: Label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectCapacityLabel") as Label
@onready var capital_project_desk_button: Button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardActions/CapitalProjectDeskButton") as Button
@onready var dismiss_project_card_button: Button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardActions/DismissProjectCardButton") as Button

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
@onready var civic_relief_button: Button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/CivicReliefButton") as Button
@onready var austerity_decree_button: Button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/AusterityDecreeButton") as Button

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

# Phase 4 Cabinet Appointment Modal References
@onready var cabinet_appointment_modal: PanelContainer = _get_node_safe("InterfaceLayer/CabinetAppointmentModal") as PanelContainer
@onready var close_cabinet_appointment_x_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentHeaderHBox/CloseCabinetAppointmentXButton") as Button
@onready var appointment_portfolio_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentPortfolioLabel") as Label
@onready var appointment_current_minister_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentCurrentMinisterLabel") as Label
@onready var candidate_technocrat_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateTechnocratButton") as Button
@onready var candidate_loyalist_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateLoyalistButton") as Button
@onready var candidate_dismiss_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateDismissButton") as Button
@onready var candidate_preview_name_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewNameLabel") as Label
@onready var candidate_preview_ratings_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewRatingsLabel") as Label
@onready var candidate_preview_salary_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewSalaryLabel") as Label
@onready var candidate_preview_consequences_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewConsequencesLabel") as Label
@onready var appointment_status_label: Label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentStatusLabel") as Label
@onready var confirm_appointment_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/ConfirmAppointmentButton") as Button
@onready var cancel_appointment_button: Button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/CancelAppointmentButton") as Button

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

# Phase 4 Cabinet Appointment Tracking
var pending_appointment_request_id: String = ""
var pending_appointment_request_time: int = 0

# Living Feedback & Notification State
var hotline_badge_dismissed: bool = false
var dossier_badge_dismissed: bool = false
var inbox_badge_dismissed: bool = false

var flag_sway_time: float = 0.0
var flag_sway_amplitude: float = 1.2
var flag_sway_frequency: float = 1.5

var left_dock_pinned: bool = false
var right_dock_pinned: bool = false

# Engagement & Delta Tracking State
var previous_state: Dictionary = {}
var last_session_delta_summary: String = ""
var diplomatic_badge_dismissed: bool = false
var current_diplomatic_proposal: Dictionary = {}
var previous_active_project: String = ""
var previous_completed_projects: String = ""
var project_toast_timer: Timer = null

# Phase 4 Cabinet Appointment Tracking & Honest Simulation Rules
var current_appointment_portfolio: String = "Finance"
var selected_candidate_type: String = "technocrat"
var request_file_override: String = ""

const CANDIDATE_DEFINITIONS := {
	"Finance": {
		"capacity_metric": "Civil Service Quality",
		"technocrat": {
			"name": "Dr. Elena Rostova",
			"competence": 0.90,
			"integrity": 0.90,
			"loyalty": 0.50,
			"experience": 0.85,
			"connections": 0.40,
			"salary": 150000,
			"consequences": "• Public Approval: +1.5%\n• Ruling Party Polling: -3.0% (Discontented party base)\n• Civil Service Quality: 90%\n• High fiscal discipline with reduced factional loyalty."
		},
		"loyalist": {
			"name": "Senator Donald Vance",
			"competence": 0.45,
			"integrity": 0.45,
			"loyalty": 0.95,
			"experience": 0.50,
			"connections": 0.90,
			"salary": 150000,
			"consequences": "• Public Approval: -1.5%\n• Ruling Party Polling: +3.0% (Rallied party base)\n• Civil Service Quality: Capped at 45%\n• Strong legislative loyalty with compromised administrative rigor."
		},
		"dismiss": {
			"name": "Vacate Portfolio (Dismiss Minister)",
			"competence": 0.0,
			"integrity": 0.0,
			"loyalty": 0.0,
			"experience": 0.0,
			"connections": 0.0,
			"salary": 0,
			"consequences": "• Civil Service Quality: Reverts to emergency baseline (30%)\n• Ministerial salary removed (R 0/mo)\n• Public Approval & Polling: No immediate delta."
		}
	},
	"Interior": {
		"capacity_metric": "Corruption Control",
		"technocrat": {
			"name": "Dr. Alistair Finch",
			"competence": 0.90,
			"integrity": 0.90,
			"loyalty": 0.50,
			"experience": 0.85,
			"connections": 0.40,
			"salary": 150000,
			"consequences": "• Public Approval: +1.5%\n• Ruling Party Polling: -3.0% (Discontented party base)\n• Corruption Control: 90%\n• Strict rule of law enforcement with lower political deference."
		},
		"loyalist": {
			"name": "Party Whip Thomas Sterling",
			"competence": 0.45,
			"integrity": 0.45,
			"loyalty": 0.95,
			"experience": 0.50,
			"connections": 0.90,
			"salary": 150000,
			"consequences": "• Public Approval: -1.5%\n• Ruling Party Polling: +3.0% (Rallied party base)\n• Corruption Control: Capped at 45%\n• High domestic security loyalty with compromised anti-corruption enforcement."
		},
		"dismiss": {
			"name": "Vacate Portfolio (Dismiss Minister)",
			"competence": 0.0,
			"integrity": 0.0,
			"loyalty": 0.0,
			"experience": 0.0,
			"connections": 0.0,
			"salary": 0,
			"consequences": "• Corruption Control: Reverts to emergency baseline (30%)\n• Ministerial salary removed (R 0/mo)\n• Public Approval & Polling: No immediate delta."
		}
	},
	"Infrastructure": {
		"capacity_metric": "Infrastructure Condition",
		"technocrat": {
			"name": "Eng. Marcus Bennett",
			"competence": 0.90,
			"integrity": 0.90,
			"loyalty": 0.50,
			"experience": 0.85,
			"connections": 0.40,
			"salary": 150000,
			"consequences": "• Public Approval: +1.5%\n• Ruling Party Polling: -3.0% (Discontented party base)\n• Infrastructure Condition: 85% (Competence 90%, Experience 85%)\n• Expert engineering oversight with lower political patronage."
		},
		"loyalist": {
			"name": "Alderman Gregory Cross",
			"competence": 0.45,
			"integrity": 0.45,
			"loyalty": 0.95,
			"experience": 0.50,
			"connections": 0.90,
			"salary": 150000,
			"consequences": "• Public Approval: -1.5%\n• Ruling Party Polling: +3.0% (Rallied party base)\n• Infrastructure Condition: Capped at 45%\n• High political patronage with compromised engineering standards."
		},
		"dismiss": {
			"name": "Vacate Portfolio (Dismiss Minister)",
			"competence": 0.0,
			"integrity": 0.0,
			"loyalty": 0.0,
			"experience": 0.0,
			"connections": 0.0,
			"salary": 0,
			"consequences": "• Infrastructure Condition: Reverts to emergency baseline (30%)\n• Ministerial salary removed (R 0/mo)\n• Public Approval & Polling: No immediate delta."
		}
	}
}

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
	if active_project_marker_button == null:
		active_project_marker_button = _get_node_safe("OfficeEnvironment/CapitalWindow/ActiveProjectMarkerButton") as Button
	if completed_project_marker_button == null:
		completed_project_marker_button = _get_node_safe("OfficeEnvironment/CapitalWindow/CompletedProjectMarkerButton") as Button

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
	if diplomatic_folder == null:
		diplomatic_folder = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton") as Button
	if diplomatic_badge == null:
		diplomatic_badge = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton/DiplomaticNotificationBadge") as Control
	if diplomatic_badge_label == null:
		diplomatic_badge_label = _get_node_safe("OfficeEnvironment/ExecutiveDesk/DiplomaticFolderButton/DiplomaticNotificationBadge/DiplomaticBadgeLabel") as Label
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

	# Phase 4 Left Dock Portfolio Lookups
	if cabinet_scroll == null:
		cabinet_scroll = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll") as ScrollContainer
	if cabinet_portfolios_vbox == null:
		cabinet_portfolios_vbox = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox") as VBoxContainer

	# Finance
	if finance_card == null:
		finance_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance") as Control
	if finance_status_label == null:
		finance_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceHeaderHBox/FinanceStatusLabel") as Label
	if finance_minister_label == null:
		finance_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceMinisterLabel") as Label
	if finance_ratings_label == null:
		finance_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceRatingsLabel") as Label
	if finance_effect_label == null:
		finance_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceEffectLabel") as Label
	if appoint_finance_button == null:
		appoint_finance_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/AppointFinanceButton") as Button

	# Defense
	if defense_card == null:
		defense_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense") as Control
	if defense_status_label == null:
		defense_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseHeaderHBox/DefenseStatusLabel") as Label
	if defense_minister_label == null:
		defense_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseMinisterLabel") as Label
	if defense_ratings_label == null:
		defense_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseRatingsLabel") as Label
	if defense_effect_label == null:
		defense_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Defense/DefenseVBox/DefenseEffectLabel") as Label

	# Foreign Affairs
	if foreign_affairs_card == null:
		foreign_affairs_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs") as Control
	if foreign_affairs_status_label == null:
		foreign_affairs_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsHeaderHBox/ForeignAffairsStatusLabel") as Label
	if foreign_affairs_minister_label == null:
		foreign_affairs_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsMinisterLabel") as Label
	if foreign_affairs_ratings_label == null:
		foreign_affairs_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsRatingsLabel") as Label
	if foreign_affairs_effect_label == null:
		foreign_affairs_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_ForeignAffairs/ForeignAffairsVBox/ForeignAffairsEffectLabel") as Label

	# Interior
	if interior_card == null:
		interior_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior") as Control
	if interior_status_label == null:
		interior_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorHeaderHBox/InteriorStatusLabel") as Label
	if interior_minister_label == null:
		interior_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorMinisterLabel") as Label
	if interior_ratings_label == null:
		interior_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorRatingsLabel") as Label
	if interior_effect_label == null:
		interior_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorEffectLabel") as Label
	if appoint_interior_button == null:
		appoint_interior_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/AppointInteriorButton") as Button

	# Health
	if health_card == null:
		health_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health") as Control
	if health_status_label == null:
		health_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthHeaderHBox/HealthStatusLabel") as Label
	if health_minister_label == null:
		health_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthMinisterLabel") as Label
	if health_ratings_label == null:
		health_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthRatingsLabel") as Label
	if health_effect_label == null:
		health_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Health/HealthVBox/HealthEffectLabel") as Label

	# Infrastructure
	if infrastructure_card == null:
		infrastructure_card = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure") as Control
	if infrastructure_status_label == null:
		infrastructure_status_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureHeaderHBox/InfrastructureStatusLabel") as Label
	if infrastructure_minister_label == null:
		infrastructure_minister_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureMinisterLabel") as Label
	if infrastructure_ratings_label == null:
		infrastructure_ratings_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureRatingsLabel") as Label
	if infrastructure_effect_label == null:
		infrastructure_effect_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureEffectLabel") as Label
	if appoint_infrastructure_button == null:
		appoint_infrastructure_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/AppointInfrastructureButton") as Button

	# Phase 4 Cabinet Appointment Modal Lookups
	if cabinet_appointment_modal == null:
		cabinet_appointment_modal = _get_node_safe("InterfaceLayer/CabinetAppointmentModal") as PanelContainer
	if close_cabinet_appointment_x_button == null:
		close_cabinet_appointment_x_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentHeaderHBox/CloseCabinetAppointmentXButton") as Button
	if appointment_portfolio_label == null:
		appointment_portfolio_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentPortfolioLabel") as Label
	if appointment_current_minister_label == null:
		appointment_current_minister_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentCurrentMinisterLabel") as Label
	if candidate_technocrat_button == null:
		candidate_technocrat_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateTechnocratButton") as Button
	if candidate_loyalist_button == null:
		candidate_loyalist_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateLoyalistButton") as Button
	if candidate_dismiss_button == null:
		candidate_dismiss_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetCandidateChoiceHBox/CandidateDismissButton") as Button
	if candidate_preview_name_label == null:
		candidate_preview_name_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewNameLabel") as Label
	if candidate_preview_ratings_label == null:
		candidate_preview_ratings_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewRatingsLabel") as Label
	if candidate_preview_salary_label == null:
		candidate_preview_salary_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewSalaryLabel") as Label
	if candidate_preview_consequences_label == null:
		candidate_preview_consequences_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CandidatePreviewCard/CandidatePreviewVBox/CandidatePreviewConsequencesLabel") as Label
	if appointment_status_label == null:
		appointment_status_label = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentStatusLabel") as Label
	if confirm_appointment_button == null:
		confirm_appointment_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/ConfirmAppointmentButton") as Button
	if cancel_appointment_button == null:
		cancel_appointment_button = _get_node_safe("InterfaceLayer/CabinetAppointmentModal/CabinetAppointmentVBox/CabinetAppointmentActionsHBox/CancelAppointmentButton") as Button

	# Left Dock Bottlenecks & Friction Lookups
	if finance_bottleneck_badge == null:
		finance_bottleneck_badge = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Finance/FinanceVBox/FinanceHeaderHBox/FinanceBottleneckBadge") as Label
	if interior_bottleneck_badge == null:
		interior_bottleneck_badge = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Interior/InteriorVBox/InteriorHeaderHBox/InteriorBottleneckBadge") as Label
	if infrastructure_bottleneck_badge == null:
		infrastructure_bottleneck_badge = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetScroll/CabinetPortfoliosVBox/PortfolioCard_Infrastructure/InfrastructureVBox/InfrastructureHeaderHBox/InfrastructureBottleneckBadge") as Label
	if cabinet_friction_panel == null:
		cabinet_friction_panel = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel") as PanelContainer
	if cabinet_friction_label == null:
		cabinet_friction_label = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel/FrictionVBox/CabinetFrictionLabel") as Label
	if resolve_friction_button == null:
		resolve_friction_button = _get_node_safe("InterfaceLayer/LeftDock/LeftDockVBox/LeftDockContent/CabinetFrictionPanel/FrictionVBox/ResolveFrictionButton") as Button

	# Right Dock Diplomatic Dossier Lookups
	if diplomatic_dossier_title == null:
		diplomatic_dossier_title = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierTitleLabel") as Label
	if diplomatic_dossier_card == null:
		diplomatic_dossier_card = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard") as PanelContainer
	if dossier_partner_label == null:
		dossier_partner_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierPartnerLabel") as Label
	if dossier_terms_label == null:
		dossier_terms_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierTermsLabel") as Label
	if accept_dossier_proposal_button == null:
		accept_dossier_proposal_button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierCardActionsHBox/AcceptDossierProposalButton") as Button
	if reject_dossier_proposal_button == null:
		reject_dossier_proposal_button = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierCardActionsHBox/RejectDossierProposalButton") as Button
	if dossier_status_label == null:
		dossier_status_label = _get_node_safe("InterfaceLayer/RightDock/RightDockVBox/RightDockContent/DiplomaticDossierCard/DossierCardVBox/DossierStatusLabel") as Label

	# Project Completion Toast Lookups
	if project_completion_toast == null:
		project_completion_toast = _get_node_safe("InterfaceLayer/ProjectCompletionToast") as PanelContainer
	if project_completion_title == null:
		project_completion_title = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ToastHeader/ProjectCompletionTitle") as Label
	if dismiss_project_toast_button == null:
		dismiss_project_toast_button = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ToastHeader/DismissProjectToastButton") as Button
	if project_completion_details_label == null:
		project_completion_details_label = _get_node_safe("InterfaceLayer/ProjectCompletionToast/ToastVBox/ProjectCompletionDetailsLabel") as Label

	# Capital Project Card Lookups
	if capital_project_card == null:
		capital_project_card = _get_node_safe("InterfaceLayer/CapitalProjectCard") as PanelContainer
	if capital_project_title == null:
		capital_project_title = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardHeader/CapitalProjectTitle") as Label
	if close_capital_project_button == null:
		close_capital_project_button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardHeader/CloseCapitalProjectButton") as Button
	if capital_project_status_label == null:
		capital_project_status_label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectStatusLabel") as Label
	if capital_project_eta_label == null:
		capital_project_eta_label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectEtaLabel") as Label
	if capital_project_capacity_label == null:
		capital_project_capacity_label = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CapitalProjectCapacityLabel") as Label
	if capital_project_desk_button == null:
		capital_project_desk_button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardActions/CapitalProjectDeskButton") as Button
	if dismiss_project_card_button == null:
		dismiss_project_card_button = _get_node_safe("InterfaceLayer/CapitalProjectCard/CardVBox/CardActions/DismissProjectCardButton") as Button

	# Dossier Modal Crisis Decree Buttons
	if civic_relief_button == null:
		civic_relief_button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/CivicReliefButton") as Button
	if austerity_decree_button == null:
		austerity_decree_button = _get_node_safe("InterfaceLayer/DossierModal/DossierVBox/DossierActionsHBox/AusterityDecreeButton") as Button

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

	# Project Completion Toast Timer
	project_toast_timer = Timer.new()
	project_toast_timer.wait_time = 6.0
	project_toast_timer.one_shot = true
	project_toast_timer.timeout.connect(dismiss_project_completion_toast)
	add_child(project_toast_timer)

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
	if diplomatic_badge and diplomatic_badge.visible:
		diplomatic_badge.modulate.a = pulse

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

	# Phase 4 Cabinet Signals
	if appoint_finance_button and not appoint_finance_button.pressed.is_connected(_on_appoint_finance_pressed):
		appoint_finance_button.pressed.connect(_on_appoint_finance_pressed)
	if appoint_interior_button and not appoint_interior_button.pressed.is_connected(_on_appoint_interior_pressed):
		appoint_interior_button.pressed.connect(_on_appoint_interior_pressed)
	if appoint_infrastructure_button and not appoint_infrastructure_button.pressed.is_connected(_on_appoint_infrastructure_pressed):
		appoint_infrastructure_button.pressed.connect(_on_appoint_infrastructure_pressed)

	if close_cabinet_appointment_x_button and not close_cabinet_appointment_x_button.pressed.is_connected(close_all_modals):
		close_cabinet_appointment_x_button.pressed.connect(close_all_modals)
	if cancel_appointment_button and not cancel_appointment_button.pressed.is_connected(close_all_modals):
		cancel_appointment_button.pressed.connect(close_all_modals)
	if confirm_appointment_button and not confirm_appointment_button.pressed.is_connected(_on_confirm_appointment_pressed):
		confirm_appointment_button.pressed.connect(_on_confirm_appointment_pressed)

	if candidate_technocrat_button and not candidate_technocrat_button.pressed.is_connected(_on_candidate_technocrat_pressed):
		candidate_technocrat_button.pressed.connect(_on_candidate_technocrat_pressed)
	if candidate_loyalist_button and not candidate_loyalist_button.pressed.is_connected(_on_candidate_loyalist_pressed):
		candidate_loyalist_button.pressed.connect(_on_candidate_loyalist_pressed)
	if candidate_dismiss_button and not candidate_dismiss_button.pressed.is_connected(_on_candidate_dismiss_pressed):
		candidate_dismiss_button.pressed.connect(_on_candidate_dismiss_pressed)

	# Engagement Feature Signals
	if city_scenery and city_scenery.has_signal("project_marker_clicked") and not city_scenery.project_marker_clicked.is_connected(_on_city_scenery_project_marker_clicked):
		city_scenery.project_marker_clicked.connect(_on_city_scenery_project_marker_clicked)
	if active_project_marker_button and not active_project_marker_button.pressed.is_connected(_on_active_project_marker_pressed):
		active_project_marker_button.pressed.connect(_on_active_project_marker_pressed)
	if completed_project_marker_button and not completed_project_marker_button.pressed.is_connected(_on_completed_project_marker_pressed):
		completed_project_marker_button.pressed.connect(_on_completed_project_marker_pressed)

	if diplomatic_folder and not diplomatic_folder.pressed.is_connected(_on_diplomatic_folder_pressed):
		diplomatic_folder.pressed.connect(_on_diplomatic_folder_pressed)
	if accept_dossier_proposal_button and not accept_dossier_proposal_button.pressed.is_connected(_on_accept_dossier_proposal_pressed):
		accept_dossier_proposal_button.pressed.connect(_on_accept_dossier_proposal_pressed)
	if reject_dossier_proposal_button and not reject_dossier_proposal_button.pressed.is_connected(_on_reject_dossier_proposal_pressed):
		reject_dossier_proposal_button.pressed.connect(_on_reject_dossier_proposal_pressed)

	if resolve_friction_button and not resolve_friction_button.pressed.is_connected(_on_resolve_friction_pressed):
		resolve_friction_button.pressed.connect(_on_resolve_friction_pressed)

	if dismiss_project_toast_button and not dismiss_project_toast_button.pressed.is_connected(dismiss_project_completion_toast):
		dismiss_project_toast_button.pressed.connect(dismiss_project_completion_toast)
	if project_completion_toast and not project_completion_toast.gui_input.is_connected(_on_project_completion_toast_gui_input):
		project_completion_toast.gui_input.connect(_on_project_completion_toast_gui_input)

	if close_capital_project_button and not close_capital_project_button.pressed.is_connected(dismiss_capital_project_card):
		close_capital_project_button.pressed.connect(dismiss_capital_project_card)
	if dismiss_project_card_button and not dismiss_project_card_button.pressed.is_connected(dismiss_capital_project_card):
		dismiss_project_card_button.pressed.connect(dismiss_capital_project_card)
	if capital_project_desk_button and not capital_project_desk_button.pressed.is_connected(_on_capital_project_desk_button_pressed):
		capital_project_desk_button.pressed.connect(_on_capital_project_desk_button_pressed)

	if civic_relief_button and not civic_relief_button.pressed.is_connected(_on_civic_relief_pressed):
		civic_relief_button.pressed.connect(_on_civic_relief_pressed)
	if austerity_decree_button and not austerity_decree_button.pressed.is_connected(_on_austerity_decree_pressed):
		austerity_decree_button.pressed.connect(_on_austerity_decree_pressed)

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

	_clear_cabinet_portfolios_ui()

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

	_update_cabinet_portfolios_ui(state)

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

	# Phase 2: Proactive Chief of Staff Briefing & Recommendations
	_update_chief_of_staff_briefing_ui(state)

	# Phase 3: Living Media Environment & Action Management
	_update_media_environment_briefing_ui(state)

	# Phase 3: Private Sector Inbound Proposals & Investment
	_update_private_sector_investment_briefing_ui(state)

	# 7. Hotline & Classified Dossier Modal Updates
	if hotline_status_label:
		hotline_status_label.text = "Secure diplomatic line operational. Connects directly to foreign heads of state."
	if hotline_standing_label:
		hotline_standing_label.text = "Diplomatic Standing: %s | Bilateral recognition & trade managed in Foreign Affairs." % str(state.get("status_message", "Connected to live simulation"))

	# Phase 2: Proactive National Security Leadership & Assessment
	_update_security_leadership_dossier_ui(state)

	if dossier_defense_label:
		var defcon_dossier := str(state.get("defcon_level", "DEFCON 5 (Peacetime Watch)"))
		var readiness_dossier := str(state.get("military_readiness", "85.0% Operational"))
		var sec_summary := str(state.get("security_assessment_summary", ""))
		if not sec_summary.is_empty():
			dossier_defense_label.text = "Defense Readiness: %s | %s\nAssessment: %s" % [defcon_dossier, readiness_dossier, sec_summary]
		else:
			dossier_defense_label.text = "Defense Readiness: %s | %s" % [defcon_dossier, readiness_dossier]
	if dossier_crisis_label:
		var sec_threats_list = state.get("security_threats", [])
		var threat_summary := ""
		if sec_threats_list is Array and not sec_threats_list.is_empty():
			threat_summary = "Active Security Threats: %d tracked by National Command." % sec_threats_list.size()
		else:
			threat_summary = "State institutions stable. Sovereign order maintained."
		dossier_crisis_label.text = "Active National Threat: %s — %s" % [str(state.get("active_crisis", "None")), threat_summary]
	if dossier_espionage_label:
		dossier_espionage_label.text = "Covert Intelligence Operations: [Feature not yet supported by simulation engine]"

	# 8. Living Capital Window Scenery: state-driven skyline reaction
	var curr_active_proj := str(state.get("active_project", "None"))
	var curr_completed_summary := ""
	if state.has("completed_projects_summary"):
		curr_completed_summary = str(state.get("completed_projects_summary"))
	elif state.has("completed_projects"):
		var cp = state.get("completed_projects")
		if cp is Array:
			curr_completed_summary = ", ".join(PackedStringArray(cp))
		else:
			curr_completed_summary = str(cp)
	if curr_completed_summary.is_empty():
		curr_completed_summary = "None"

	if city_scenery and city_scenery.has_method("set_state_metrics"):
		var app_val := float(state.get("approval_rating", 68.5))
		var stab_val := float(state.get("stability_rating", 85.0))
		var act_proj := str(state.get("active_project", "None"))
		var act_crisis := str(state.get("active_crisis", "None"))
		var first_comp := curr_completed_summary.split(",")[0].strip_edges()
		city_scenery.set_state_metrics(app_val, stab_val, act_proj, act_crisis, first_comp)

	if active_project_marker_button:
		active_project_marker_button.visible = (curr_active_proj != "None" and not curr_active_proj.is_empty())
	if completed_project_marker_button:
		completed_project_marker_button.visible = (curr_completed_summary != "None" and not curr_completed_summary.is_empty())

	# 8b. Project Completion Moment Detection
	var latest_dec := str(state.get("latest_decision_result", ""))
	var is_project_finished := false
	var finished_name := ""

	if not previous_active_project.is_empty() and previous_active_project != "None" and (curr_active_proj != previous_active_project or curr_active_proj == "None"):
		if curr_completed_summary.contains(previous_active_project) or (state.has("completed_projects") and state.get("completed_projects") is Array and state.get("completed_projects").has(previous_active_project)) or latest_dec.contains("Completed") or latest_dec.contains("commissioned"):
			is_project_finished = true
			finished_name = previous_active_project
	elif not previous_completed_projects.is_empty() and curr_completed_summary != previous_completed_projects and curr_completed_summary != "None":
		is_project_finished = true
		finished_name = curr_completed_summary.split(",")[0].strip_edges()

	if is_project_finished and not finished_name.is_empty():
		var app_delta_str := ""
		if state.has("latest_approval_delta"):
			app_delta_str = str(state.get("latest_approval_delta"))
		elif not previous_state.is_empty() and previous_state.has("approval_rating") and state.has("approval_rating"):
			var diff := float(state.get("approval_rating", 0.0)) - float(previous_state.get("approval_rating", 0.0))
			app_delta_str = ("+" if diff >= 0 else "") + "%0.1f%%" % diff

		var stab_delta_str := ""
		if state.has("latest_stability_delta"):
			stab_delta_str = str(state.get("latest_stability_delta"))
		elif not previous_state.is_empty() and previous_state.has("stability_rating") and state.has("stability_rating"):
			var diff := float(state.get("stability_rating", 0.0)) - float(previous_state.get("stability_rating", 0.0))
			stab_delta_str = ("+" if diff >= 0 else "") + "%0.1f%%" % diff

		var cap_delta_str := ""
		if state.has("latest_capacity_delta"):
			cap_delta_str = str(state.get("latest_capacity_delta"))
		elif state.has("infrastructure_condition"):
			var ic_val := float(state.get("infrastructure_condition", 0.85))
			if ic_val <= 1.0:
				ic_val *= 100.0
			cap_delta_str = "%0.1f%% Condition" % ic_val

		trigger_project_completion(finished_name, app_delta_str, stab_delta_str, cap_delta_str)

	previous_active_project = curr_active_proj
	previous_completed_projects = curr_completed_summary

	# 9. Soft Identity Feedback (Portrait Frame Glow & Flag Dynamic Sway)
	var appr_num := float(state.get("approval_rating", 68.5))
	var stab_num := float(state.get("stability_rating", 85.0))
	var cr_name := str(state.get("active_crisis", "None"))
	var has_crisis := (not cr_name.is_empty() and cr_name != "None" and cr_name != "[No State Loaded]")
	update_portrait_state_feedback(appr_num, stab_num, has_crisis)
	update_flag_state_feedback(stab_num, has_crisis)

	# 10. Desk Notification Badges & Diplomatic Dossiers
	var defcon := str(state.get("defcon_level", "DEFCON 5"))
	var mil_threat := (not defcon.begins_with("DEFCON 5") and not defcon.is_empty() and defcon != "[No State Loaded]")

	var sec_threats_arr = state.get("security_threats", [])
	var has_urgent_security_threat := false
	if sec_threats_arr is Array:
		for t in sec_threats_arr:
			if t is Dictionary:
				var sev_str: String = str(t.get("severity", ""))
				var conf_str: String = str(t.get("confidence", ""))
				if sev_str == "Critical" or sev_str == "Severe" or conf_str == "Confirmed":
					has_urgent_security_threat = true
					break

	if has_crisis:
		if not hotline_badge_dismissed:
			set_hotline_badge(true, "!")
	else:
		set_hotline_badge(false)

	if mil_threat or has_crisis or has_urgent_security_threat:
		if not dossier_badge_dismissed:
			set_dossier_badge(true, "!")
	else:
		set_dossier_badge(false)

	var dec_res := str(state.get("latest_decision_result", ""))
	var has_pending := dec_res.contains("Pending") or state.has("pending_decision")
	var recs_val = state.get("chief_of_staff_recommendations", [])
	var pending_cos_count := 0
	if recs_val is Array:
		for r in recs_val:
			if r is Dictionary and str(r.get("status", "Pending")) == "Pending":
				pending_cos_count += 1

	if has_pending or pending_cos_count > 0:
		if not inbox_badge_dismissed:
			var badge_text := str(max(1, pending_cos_count))
			set_inbox_badge(true, badge_text)
	else:
		set_inbox_badge(false)

	_update_diplomatic_dossiers_ui(state)

	# 11. Session Briefing Toast Update
	update_session_briefing(state)
	if session_briefing_toast and not session_briefing_toast.visible:
		show_session_briefing(true)

	# 12. Critical Alert Trigger
	if has_crisis:
		trigger_dock_alert("right")

	previous_state = state.duplicate(true)
	state_loaded.emit(state)

func _on_poll_timer_timeout() -> void:
	check_for_decision_results()

	if not pending_appointment_request_id.is_empty():
		if Time.get_ticks_msec() - pending_appointment_request_time > 15000:
			var timeout_msg := "Order Warning: Cabinet appointment request timed out without response from simulation processor."
			if appointment_status_label:
				appointment_status_label.text = timeout_msg
			if status_ticker_label:
				status_ticker_label.text = timeout_msg
			pending_appointment_request_id = ""

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
	if cabinet_appointment_modal:
		cabinet_appointment_modal.visible = false
	if capital_project_card:
		capital_project_card.visible = false
	active_nav_section = Section.OVERVIEW
	_update_nav_button_highlights()

func is_any_modal_open() -> bool:
	return (briefing_modal and briefing_modal.visible) or \
		(hotline_modal and hotline_modal.visible) or \
		(dossier_modal and dossier_modal.visible) or \
		(customisation_modal and customisation_modal.visible) or \
		(purchase_confirm_modal and purchase_confirm_modal.visible) or \
		(cabinet_appointment_modal and cabinet_appointment_modal.visible) or \
		(capital_project_card and capital_project_card.visible)

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
func _extract_numeric_value(val: Variant) -> float:
	if val is float or val is int:
		return float(val)
	var s := str(val).replace("R", "").replace("$", "").replace(",", "").replace("%", "").strip_edges()
	if s.is_valid_float():
		return s.to_float()
	return 0.0

func update_session_briefing(state: Dictionary) -> void:
	if state.is_empty():
		return

	var c_name := str(state.get("country_name", "Republic"))
	var curr_treas_val: float = float(state.get("treasury_balance", _extract_numeric_value(state.get("treasury", 0.0))))
	var curr_app: float = float(state.get("approval_rating", _extract_numeric_value(state.get("approval", 0.0))))
	var curr_stab: float = float(state.get("stability_rating", _extract_numeric_value(state.get("stability", 0.0))))
	var curr_proj := str(state.get("active_project", "None"))
	var curr_crisis := str(state.get("active_crisis", "None"))

	var treas_str := str(state.get("treasury", ""))
	if treas_str.is_empty() and state.has("treasury_balance"):
		treas_str = "R " + _format_number_commas(curr_treas_val)
	var app_str := str(state.get("approval", ""))
	if app_str.is_empty() and state.has("approval_rating"):
		app_str = "%0.1f%%" % curr_app
	var stab_str := str(state.get("stability", ""))
	if stab_str.is_empty() and state.has("stability_rating"):
		stab_str = "%0.1f%%" % curr_stab

	var base_metrics := "%s | Liquid Reserves: %s | Approval: %s | Stability: %s" % [
		c_name,
		(treas_str if not treas_str.is_empty() else "[No State Loaded]"),
		(app_str if not app_str.is_empty() else "[No State Loaded]"),
		(stab_str if not stab_str.is_empty() else "[No State Loaded]")
	]

	if previous_state.is_empty():
		# Initial load: honest baseline summary
		if session_briefing_title:
			session_briefing_title.text = "📋 EXECUTIVE SESSION BRIEFING"
		if session_briefing_metrics_label:
			session_briefing_metrics_label.text = base_metrics
		if session_briefing_status_label:
			var msg := str(state.get("latest_decision_result", ""))
			if msg.is_empty():
				msg = str(state.get("status_message", "Executive office online."))
			session_briefing_status_label.text = "Active Program: %s | Crisis Posture: %s | Status: %s" % [curr_proj, curr_crisis, msg]
		last_session_delta_summary = base_metrics
		previous_state = state.duplicate(true)
	else:
		# Delta calculation between previous_state and new state
		var prev_treas_val: float = float(previous_state.get("treasury_balance", _extract_numeric_value(previous_state.get("treasury", 0.0))))
		var prev_app: float = float(previous_state.get("approval_rating", _extract_numeric_value(previous_state.get("approval", 0.0))))
		var prev_stab: float = float(previous_state.get("stability_rating", _extract_numeric_value(previous_state.get("stability", 0.0))))
		var prev_proj := str(previous_state.get("active_project", "None"))

		var diff_treas: float = curr_treas_val - prev_treas_val
		var diff_app: float = curr_app - prev_app
		var diff_stab: float = curr_stab - prev_stab

		var treas_delta_str := ("+R " if diff_treas >= 0.0 else "-R ") + _format_number_commas(abs(diff_treas))
		var app_delta_str := ("+" if diff_app >= 0.0 else "") + "%0.1f%%" % diff_app
		var stab_delta_str := ("+" if diff_stab >= 0.0 else "") + "%0.1f%%" % diff_stab

		var proj_delta_str := ""
		if curr_proj != prev_proj and prev_proj != "None":
			proj_delta_str = "Project Shift: %s (was %s)" % [curr_proj, prev_proj]
		else:
			proj_delta_str = "Active Program: " + curr_proj

		var cab_delta_str := ""
		var prev_cab = previous_state.get("cabinet", [])
		var curr_cab = state.get("cabinet", [])
		if prev_cab != curr_cab and not str(prev_cab).is_empty():
			cab_delta_str = "Cabinet: Portfolio shifts detected"
		else:
			var cs := float(state.get("civil_service_quality", 0.3)) * 100.0
			var cc := float(state.get("corruption_control", 0.3)) * 100.0
			cab_delta_str = "Capacity: CS %0.0f%% | CC %0.0f%%" % [cs, cc]

		if session_briefing_title:
			session_briefing_title.text = "📋 SESSION DELTA BRIEFING"
		if session_briefing_metrics_label:
			session_briefing_metrics_label.text = "%s | Deltas: Treas %s, App %s, Stab %s" % [base_metrics, treas_delta_str, app_delta_str, stab_delta_str]
		if session_briefing_status_label:
			session_briefing_status_label.text = "%s | %s" % [proj_delta_str, cab_delta_str]
		last_session_delta_summary = "Treasury Delta: %s | Approval: %s | Stability: %s" % [treas_delta_str, app_delta_str, stab_delta_str]
		previous_state = state.duplicate(true)

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

# ------------------------------------------------------------------------------
# DIPLOMATIC DESK PROP & DOSSIER API
# ------------------------------------------------------------------------------
func set_diplomatic_badge(visible_flag: bool, text: String = "!") -> void:
	if diplomatic_badge:
		diplomatic_badge.visible = visible_flag
	if diplomatic_badge_label and visible_flag:
		diplomatic_badge_label.text = text

func is_diplomatic_badge_visible() -> bool:
	return diplomatic_badge != null and diplomatic_badge.visible

func _on_diplomatic_folder_pressed() -> void:
	diplomatic_badge_dismissed = true
	set_diplomatic_badge(false)
	set_right_dock_expanded(true)
	right_dock_pinned = true
	if status_ticker_label:
		status_ticker_label.text = "Diplomatic Folder: Bilateral proposals and foreign affairs dossier opened."

func _update_diplomatic_dossiers_ui(state: Dictionary) -> void:
	var prop = null
	if state.has("diplomatic_proposals") and state.get("diplomatic_proposals") is Array and not state.get("diplomatic_proposals").is_empty():
		prop = state.get("diplomatic_proposals")[0]
	elif state.has("incoming_diplomatic_proposals") and state.get("incoming_diplomatic_proposals") is Array and not state.get("incoming_diplomatic_proposals").is_empty():
		prop = state.get("incoming_diplomatic_proposals")[0]
	elif state.has("incoming_trade_offers") and state.get("incoming_trade_offers") is Array and not state.get("incoming_trade_offers").is_empty():
		prop = state.get("incoming_trade_offers")[0]
	elif state.has("diplomatic_proposal") and state.get("diplomatic_proposal") is Dictionary:
		prop = state.get("diplomatic_proposal")
	elif state.has("diplomatic_offer") and not str(state.get("diplomatic_offer", "")).is_empty():
		prop = {
			"partner": str(state.get("diplomatic_partner", "Bilateral Partner")),
			"terms": str(state.get("diplomatic_offer", "")),
			"proposal_id": str(state.get("diplomatic_proposal_id", "prop_01"))
		}

	if prop is Dictionary and not prop.is_empty():
		current_diplomatic_proposal = prop
		if diplomatic_dossier_card:
			diplomatic_dossier_card.visible = true
		if diplomatic_dossier_title:
			diplomatic_dossier_title.visible = true
		if dossier_partner_label:
			dossier_partner_label.text = "Partner: " + str(prop.get("partner", prop.get("partner_name", "Foreign Sovereign")))
		if dossier_terms_label:
			dossier_terms_label.text = "Terms: " + str(prop.get("terms", prop.get("offer_summary", "Bilateral trade agreement proposed.")))
		if dossier_status_label:
			dossier_status_label.text = "Status: Pending Sovereign Response"
		if not diplomatic_badge_dismissed:
			set_diplomatic_badge(true, "!")
	else:
		current_diplomatic_proposal = {}
		if diplomatic_dossier_card:
			diplomatic_dossier_card.visible = false
		if diplomatic_dossier_title:
			diplomatic_dossier_title.visible = false
		set_diplomatic_badge(false)

func _on_accept_dossier_proposal_pressed() -> void:
	var prop_id := str(current_diplomatic_proposal.get("id", current_diplomatic_proposal.get("proposal_id", "prop_default")))
	execute_diplomatic_response(prop_id, "accept", true)

func _on_reject_dossier_proposal_pressed() -> void:
	var prop_id := str(current_diplomatic_proposal.get("id", current_diplomatic_proposal.get("proposal_id", "prop_default")))
	execute_diplomatic_response(prop_id, "reject", true)

func _on_resolve_friction_pressed() -> void:
	var cs_val := float(latest_state.get("civil_service_quality", 0.3))
	var cc_val := float(latest_state.get("corruption_control", 0.3))
	var ic_val := float(latest_state.get("infrastructure_condition", 0.3))

	if cs_val < 0.40:
		open_appointment_modal("Finance")
	elif cc_val < 0.40:
		open_appointment_modal("Interior")
	elif ic_val < 0.40:
		open_appointment_modal("Infrastructure")
	else:
		open_appointment_modal("Finance")

# ------------------------------------------------------------------------------
# CAPITAL WINDOW PROJECT MARKERS & STATUS CARD API
# ------------------------------------------------------------------------------
func show_capital_project_card(project_name: String, is_active: bool) -> void:
	if capital_project_card:
		capital_project_card.visible = true
	if capital_project_title:
		if is_active:
			capital_project_title.text = "🏗️ CAPITAL PROJECT: " + project_name.to_upper()
		else:
			capital_project_title.text = "🏛️ CIVIC LANDMARK: " + project_name.to_upper()
	if capital_project_status_label:
		if is_active:
			capital_project_status_label.text = "Status: Active Capital Construction"
		else:
			capital_project_status_label.text = "Status: Commissioned & Fully Operational"
	if capital_project_eta_label:
		if is_active:
			var proj_str := str(latest_state.get("active_project", ""))
			if proj_str.contains("Finishes:"):
				var parts := proj_str.split("Finishes:")
				capital_project_eta_label.text = "Estimated Completion: Finishes" + parts[1].replace(")", "")
			else:
				capital_project_eta_label.text = "Estimated Completion: Construction underway"
		else:
			capital_project_eta_label.text = "Commission Status: Completed and permanently serving the Republic"
	if capital_project_capacity_label:
		var cap := float(latest_state.get("infrastructure_condition", 0.85)) * 100.0
		if is_active:
			capital_project_capacity_label.text = "Capacity Impact: Current Infrastructure Condition at %0.1f%%" % cap
		else:
			capital_project_capacity_label.text = "Capacity Contribution: Infrastructure Condition secured at %0.1f%%" % cap

func dismiss_capital_project_card() -> void:
	if capital_project_card:
		capital_project_card.visible = false

func is_capital_project_card_visible() -> bool:
	return capital_project_card != null and capital_project_card.visible

func _on_city_scenery_project_marker_clicked(project_name: String, is_active: bool, _marker_id: String) -> void:
	show_capital_project_card(project_name, is_active)

func _on_active_project_marker_pressed() -> void:
	var proj := str(latest_state.get("active_project", "National Highway System"))
	show_capital_project_card(proj, true)

func _on_completed_project_marker_pressed() -> void:
	var comp_str := str(latest_state.get("completed_projects_summary", "Civic Spire & Landmark"))
	var first_comp := comp_str.split(",")[0].strip_edges()
	if first_comp.is_empty() or first_comp == "None":
		first_comp = "National Civic Landmark"
	show_capital_project_card(first_comp, false)

func _on_capital_project_desk_button_pressed() -> void:
	dismiss_capital_project_card()
	navigate_to_section(Section.ECONOMY)

# ------------------------------------------------------------------------------
# PROJECT COMPLETION MOMENT & TOAST API
# ------------------------------------------------------------------------------
func trigger_project_completion(project_name: String, app_change: String = "", stab_change: String = "", cap_change: String = "") -> void:
	if city_scenery and city_scenery.has_method("highlight_project_completion"):
		city_scenery.highlight_project_completion(project_name, 6.0)
	if completed_project_marker_button:
		completed_project_marker_button.visible = true

	var details := "Project '%s' commissioned into sovereign service for the Republic." % project_name
	if not app_change.is_empty() or not stab_change.is_empty() or not cap_change.is_empty():
		details += "\n• Approval Effect: " + (app_change if not app_change.is_empty() else "Positive")
		details += "\n• Stability Effect: " + (stab_change if not stab_change.is_empty() else "Elevated")
		if not cap_change.is_empty():
			details += "\n• Capacity Effect: " + cap_change

	show_project_completion_toast(project_name, details)

func show_project_completion_toast(project_name: String, details: String = "") -> void:
	if project_completion_toast:
		project_completion_toast.visible = true
	if project_completion_title:
		project_completion_title.text = "🏛️ CIVIC PROJECT COMPLETED: " + project_name.to_upper()
	if project_completion_details_label:
		project_completion_details_label.text = details if not details.is_empty() else "Infrastructure project commissioned into sovereign service."
	if project_toast_timer:
		project_toast_timer.start(6.0)

func dismiss_project_completion_toast() -> void:
	if project_completion_toast:
		project_completion_toast.visible = false
	if project_toast_timer and not project_toast_timer.is_stopped():
		project_toast_timer.stop()

func is_project_completion_toast_visible() -> bool:
	return project_completion_toast != null and project_completion_toast.visible

func _on_project_completion_toast_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		dismiss_project_completion_toast()

# ------------------------------------------------------------------------------
# CRISIS DECREE BUTTON HANDLERS
# ------------------------------------------------------------------------------
func _on_civic_relief_pressed() -> void:
	execute_crisis_response("civic_relief", true)

func _on_austerity_decree_pressed() -> void:
	execute_crisis_response("austerity", true)

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

# ==============================================================================
# PHASE 4: DOMESTIC CABINET & MINISTRIES DOCK METHODS
# ==============================================================================

func _clear_cabinet_portfolios_ui() -> void:
	# Finance
	if finance_status_label:
		finance_status_label.text = "Vacant (Unassigned)"
	if finance_minister_label:
		finance_minister_label.text = "Minister: [Vacant]"
	if finance_ratings_label:
		finance_ratings_label.text = "Ratings: --"
	if finance_effect_label:
		finance_effect_label.text = "State Capacity Effect: Civil Service Quality reverts to baseline 30.0%"
	if appoint_finance_button:
		appoint_finance_button.disabled = false
		appoint_finance_button.text = "Appoint Minister"

	# Defense
	if defense_status_label:
		defense_status_label.text = "Vacant (Unassigned)"
	if defense_minister_label:
		defense_minister_label.text = "Minister: [Vacant]"
	if defense_ratings_label:
		defense_ratings_label.text = "Ratings: --"
	if defense_effect_label:
		defense_effect_label.text = "State Capacity Effect: Armed Forces coordination inactive"

	# Foreign Affairs
	if foreign_affairs_status_label:
		foreign_affairs_status_label.text = "Vacant (Unassigned)"
	if foreign_affairs_minister_label:
		foreign_affairs_minister_label.text = "Minister: [Vacant]"
	if foreign_affairs_ratings_label:
		foreign_affairs_ratings_label.text = "Ratings: --"
	if foreign_affairs_effect_label:
		foreign_affairs_effect_label.text = "State Capacity Effect: Sovereign diplomatic missions unassigned"

	# Interior
	if interior_status_label:
		interior_status_label.text = "Vacant (Unassigned)"
	if interior_minister_label:
		interior_minister_label.text = "Minister: [Vacant]"
	if interior_ratings_label:
		interior_ratings_label.text = "Ratings: --"
	if interior_effect_label:
		interior_effect_label.text = "State Capacity Effect: Corruption Control reverts to baseline 30.0%"
	if appoint_interior_button:
		appoint_interior_button.disabled = false
		appoint_interior_button.text = "Appoint Minister"

	# Health
	if health_status_label:
		health_status_label.text = "Vacant (Unassigned)"
	if health_minister_label:
		health_minister_label.text = "Minister: [Vacant]"
	if health_ratings_label:
		health_ratings_label.text = "Ratings: --"
	if health_effect_label:
		health_effect_label.text = "State Capacity Effect: Healthcare infrastructure oversight unassigned"

	# Infrastructure
	if infrastructure_status_label:
		infrastructure_status_label.text = "Vacant (Unassigned)"
	if infrastructure_minister_label:
		infrastructure_minister_label.text = "Minister: [Vacant]"
	if infrastructure_ratings_label:
		infrastructure_ratings_label.text = "Ratings: --"
	if infrastructure_effect_label:
		infrastructure_effect_label.text = "State Capacity Effect: Infrastructure Condition reverts to baseline 30.0%"
	if appoint_infrastructure_button:
		appoint_infrastructure_button.disabled = false
		appoint_infrastructure_button.text = "Appoint Minister"

	if finance_bottleneck_badge:
		finance_bottleneck_badge.visible = false
	if interior_bottleneck_badge:
		interior_bottleneck_badge.visible = false
	if infrastructure_bottleneck_badge:
		infrastructure_bottleneck_badge.visible = false
	if cabinet_friction_panel:
		cabinet_friction_panel.visible = false

func _update_cabinet_portfolios_ui(state: Dictionary) -> void:
	if state.is_empty():
		_clear_cabinet_portfolios_ui()
		return

	var portfolios := ["Finance", "Defense", "ForeignAffairs", "Interior", "Health", "Infrastructure"]
	for p in portfolios:
		var p_data := _resolve_portfolio_display_data(p, state)
		_apply_portfolio_card_data(p, p_data)

	# Capacity bottlenecks and ministerial friction checks
	var econ_val := float(state.get("economic_capacity", state.get("civil_service_quality", 0.5)))
	var cs_val := float(state.get("civil_service_quality", 0.5))
	var cc_val := float(state.get("corruption_control", state.get("civil_service_quality", 0.5)))
	var ic_val := float(state.get("infrastructure_condition", 0.5))

	var econ_norm := econ_val / 100.0 if econ_val > 1.0 else econ_val
	var cs_norm := cs_val / 100.0 if cs_val > 1.0 else cs_val
	var cc_norm := cc_val / 100.0 if cc_val > 1.0 else cc_val
	var ic_norm := ic_val / 100.0 if ic_val > 1.0 else ic_val

	if finance_bottleneck_badge:
		finance_bottleneck_badge.visible = (econ_norm < 0.40 or cs_norm < 0.40)
	if interior_bottleneck_badge:
		interior_bottleneck_badge.visible = (cc_norm < 0.40)
	if infrastructure_bottleneck_badge:
		infrastructure_bottleneck_badge.visible = (ic_norm < 0.40)

	if cabinet_friction_panel:
		var friction_obj = state.get("cabinet_friction", state.get("minister_request", null))
		var friction_msg := ""
		if friction_obj is Dictionary:
			friction_msg = str(friction_obj.get("description", friction_obj.get("message", "")))
		elif friction_obj != null:
			friction_msg = str(friction_obj)

		if not friction_msg.is_empty():
			cabinet_friction_panel.visible = true
			if cabinet_friction_label:
				cabinet_friction_label.text = "⚠️ Ministerial Friction: " + friction_msg
			if resolve_friction_button:
				resolve_friction_button.text = "Review Cabinet Order"
		elif cs_norm < 0.40 or econ_norm < 0.40:
			cabinet_friction_panel.visible = true
			if cabinet_friction_label:
				cabinet_friction_label.text = "⚠️ Administrative Bottleneck: Civil Service Quality constrained at %0.0f%%. Treasury efficiency degraded." % (cs_norm * 100.0)
			if resolve_friction_button:
				resolve_friction_button.text = "Commission Finance Minister"
		elif cc_norm < 0.40:
			cabinet_friction_panel.visible = true
			if cabinet_friction_label:
				cabinet_friction_label.text = "⚠️ Regulatory Friction: Corruption Control below threshold at %0.0f%%. Institutional compliance compromised." % (cc_norm * 100.0)
			if resolve_friction_button:
				resolve_friction_button.text = "Commission Interior Minister"
		elif ic_norm < 0.40:
			cabinet_friction_panel.visible = true
			if cabinet_friction_label:
				cabinet_friction_label.text = "⚠️ Infrastructure Deficit: Condition at %0.0f%%. Capital projects delayed." % (ic_norm * 100.0)
			if resolve_friction_button:
				resolve_friction_button.text = "Commission Infrastructure Minister"
		else:
			# Check for ministerial advice friction / disloyalty / integrity warnings
			var advice_list = state.get("ministerial_advice", null)
			var has_friction := false
			var friction_headline := ""
			if advice_list is Array:
				for adv in advice_list:
					if adv is Dictionary:
						var warn_str: String = str(adv.get("warning_notes", ""))
						var is_self: bool = bool(adv.get("is_self_serving", false))
						var loy_val: float = float(adv.get("loyalty_rating", 100.0))
						var is_app: bool = bool(adv.get("is_appointed", false))
						if is_app and (is_self or not warn_str.is_empty() or loy_val < 35.0):
							has_friction = true
							var port_name: String = str(adv.get("portfolio", ""))
							friction_headline = port_name + ": " + (warn_str if not warn_str.is_empty() else "Institutional friction detected")
							break
			if has_friction:
				cabinet_friction_panel.visible = true
				if cabinet_friction_label:
					cabinet_friction_label.text = "⚠️ Cabinet Warning: " + friction_headline
				if resolve_friction_button:
					resolve_friction_button.text = "Review Cabinet Portfolio"
			else:
				cabinet_friction_panel.visible = false

func _resolve_portfolio_display_data(portfolio: String, state: Dictionary) -> Dictionary:
	# 1. Check for Phase 2 ministerial_advice
	var advice_list = state.get("ministerial_advice", null)
	var matched_advice: Dictionary = {}
	if advice_list is Array:
		for adv in advice_list:
			if adv is Dictionary and str(adv.get("portfolio", "")).to_lower() == portfolio.to_lower():
				matched_advice = adv
				break

	if not matched_advice.is_empty():
		var is_app: bool = bool(matched_advice.get("is_appointed", false))
		var min_name: String = str(matched_advice.get("minister_name", "Vacant"))
		if min_name == "" or min_name.to_lower() == "vacant" or min_name == "[No State Loaded]":
			is_app = false

		if is_app:
			var comp: float = float(matched_advice.get("competence_rating", 0.0))
			var loy: float = float(matched_advice.get("loyalty_rating", 0.0))
			var integ: float = float(matched_advice.get("integrity_rating", 0.0))
			var headline: String = str(matched_advice.get("headline", ""))
			var risk_lvl: String = str(matched_advice.get("risk_level", "Low"))
			var conf_val: float = float(matched_advice.get("confidence_rating", 0.0))
			var warn_str: String = str(matched_advice.get("warning_notes", ""))
			var action_str: String = str(matched_advice.get("recommended_action", ""))

			var ratings_text := "Ratings: Comp %d%% | Loy %d%% | Int %d%%" % [int(round(comp)), int(round(loy)), int(round(integ))]
			if not warn_str.is_empty():
				ratings_text += " | ⚠️ " + warn_str

			var cap_text := _get_appointed_effect_string(portfolio, state)
			var effect_text := "%s | Advice: %s (Risk: %s, Conf: %0.0f%%) — %s" % [cap_text, headline, risk_lvl, conf_val, action_str]

			return {
				"is_appointed": true,
				"status": "Appointed (Active)",
				"minister_name": min_name,
				"ratings": ratings_text,
				"effect": effect_text
			}
		else:
			var head_vac: String = str(matched_advice.get("headline", "Vacant Portfolio"))
			var det_vac: String = str(matched_advice.get("detailed_advice", "Emergency baseline 30.0%."))
			return {
				"is_appointed": false,
				"status": "Vacant (Unassigned)",
				"minister_name": "Minister: [Vacant]",
				"ratings": "Ratings: -- (Baseline 30%)",
				"effect": "Bottleneck: %s — %s" % [head_vac, det_vac]
			}

	# 2. Check traditional cabinet list
	var cabinet_list = state.get("cabinet", null)
	var found_entry: Dictionary = {}
	if cabinet_list is Array:
		for entry in cabinet_list:
			if entry is Dictionary and str(entry.get("portfolio", "")) == portfolio:
				found_entry = entry
				break

	if not found_entry.is_empty():
		var is_appointed: bool = bool(found_entry.get("is_appointed", false))
		var min_name: String = str(found_entry.get("minister_name", "Vacant"))
		if min_name == "" or min_name.to_lower() == "vacant" or min_name == "[No State Loaded]":
			is_appointed = false

		if is_appointed:
			var comp: float = float(found_entry.get("competence", 0.0))
			var loy: float = float(found_entry.get("loyalty", 0.0))
			var integ: float = float(found_entry.get("integrity", 0.0))
			var metric_name: String = str(found_entry.get("state_capacity_metric", ""))
			var cap_val = found_entry.get("state_capacity_value")
			var effect_text := ""
			if metric_name != "" and cap_val != null:
				effect_text = "State Capacity: %s: %0.1f%%" % [metric_name, float(cap_val) * 100.0]
			else:
				effect_text = _get_appointed_effect_string(portfolio, state)

			return {
				"is_appointed": true,
				"status": "Appointed (Active)",
				"minister_name": min_name,
				"ratings": "Ratings: Comp %d%% | Loy %d%% | Int %d%%" % [int(round(comp * 100.0)), int(round(loy * 100.0)), int(round(integ * 100.0))],
				"effect": effect_text
			}
		else:
			return {
				"is_appointed": false,
				"status": "Vacant (Unassigned)",
				"minister_name": "Minister: [Vacant]",
				"ratings": "Ratings: --",
				"effect": _get_vacant_effect_string(portfolio, state)
			}

	# Fallback to top-level fields
	var top_level_name := ""
	match portfolio:
		"Finance":
			top_level_name = str(state.get("finance_minister", "Vacant"))
		"Interior":
			top_level_name = str(state.get("interior_minister", "Vacant"))
		"Infrastructure":
			top_level_name = str(state.get("infrastructure_minister", "Vacant"))
		"Defense":
			top_level_name = str(state.get("defense_minister", "Vacant"))
		"ForeignAffairs":
			top_level_name = str(state.get("foreign_affairs_minister", "Vacant"))
		"Health":
			top_level_name = str(state.get("health_minister", "Vacant"))

	if top_level_name != "" and top_level_name.to_lower() != "vacant" and top_level_name != "[No State Loaded]":
		var effect_text := _get_appointed_effect_string(portfolio, state)
		return {
			"is_appointed": true,
			"status": "Appointed (Active)",
			"minister_name": top_level_name,
			"ratings": "Ratings: Appointed Minister",
			"effect": effect_text
		}

	return {
		"is_appointed": false,
		"status": "Vacant (Unassigned)",
		"minister_name": "Minister: [Vacant]",
		"ratings": "Ratings: --",
		"effect": _get_vacant_effect_string(portfolio, state)
	}

func _get_vacant_effect_string(portfolio: String, state: Dictionary) -> String:
	match portfolio:
		"Finance":
			var val := float(state.get("civil_service_quality", 0.3)) * 100.0
			return "State Capacity Effect: Civil Service Quality at %0.1f%%" % val
		"Interior":
			var val := float(state.get("corruption_control", 0.3)) * 100.0
			return "State Capacity Effect: Corruption Control at %0.1f%%" % val
		"Infrastructure":
			var val := float(state.get("infrastructure_condition", 0.3)) * 100.0
			return "State Capacity Effect: Infrastructure Condition at %0.1f%%" % val
		"Defense":
			return "State Capacity Effect: Armed Forces coordination inactive"
		"ForeignAffairs":
			return "State Capacity Effect: Sovereign diplomatic missions unassigned"
		"Health":
			return "State Capacity Effect: Healthcare infrastructure oversight unassigned"
		_:
			return "State Capacity Effect: Baseline administration"

func _get_appointed_effect_string(portfolio: String, state: Dictionary) -> String:
	match portfolio:
		"Finance":
			var val := float(state.get("civil_service_quality", 0.9)) * 100.0
			return "State Capacity: Civil Service Quality: %0.1f%%" % val
		"Interior":
			var val := float(state.get("corruption_control", 0.9)) * 100.0
			return "State Capacity: Corruption Control: %0.1f%%" % val
		"Infrastructure":
			var val := float(state.get("infrastructure_condition", 0.9)) * 100.0
			return "State Capacity: Infrastructure Condition: %0.1f%%" % val
		"Defense":
			return "Armed Forces: Military readiness coordination active"
		"ForeignAffairs":
			return "Diplomacy: Sovereign diplomatic missions active"
		"Health":
			return "Public Health: Healthcare delivery active"
		_:
			return "Portfolio administration active"

func _apply_portfolio_card_data(portfolio: String, data: Dictionary) -> void:
	var is_app: bool = data.get("is_appointed", false)
	var status_str: String = data.get("status", "Vacant (Unassigned)")
	var min_name: String = data.get("minister_name", "[Vacant]")
	var ratings_str: String = data.get("ratings", "--")
	var effect_str: String = data.get("effect", "")

	match portfolio:
		"Finance":
			if finance_status_label:
				finance_status_label.text = status_str
			if finance_minister_label:
				finance_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if finance_ratings_label:
				finance_ratings_label.text = ratings_str
			if finance_effect_label:
				finance_effect_label.text = effect_str
			if appoint_finance_button:
				appoint_finance_button.text = "Manage Appointment" if is_app else "Appoint Minister"
		"Defense":
			if defense_status_label:
				defense_status_label.text = status_str
			if defense_minister_label:
				defense_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if defense_ratings_label:
				defense_ratings_label.text = ratings_str
			if defense_effect_label:
				defense_effect_label.text = effect_str
		"ForeignAffairs":
			if foreign_affairs_status_label:
				foreign_affairs_status_label.text = status_str
			if foreign_affairs_minister_label:
				foreign_affairs_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if foreign_affairs_ratings_label:
				foreign_affairs_ratings_label.text = ratings_str
			if foreign_affairs_effect_label:
				foreign_affairs_effect_label.text = effect_str
		"Interior":
			if interior_status_label:
				interior_status_label.text = status_str
			if interior_minister_label:
				interior_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if interior_ratings_label:
				interior_ratings_label.text = ratings_str
			if interior_effect_label:
				interior_effect_label.text = effect_str
			if appoint_interior_button:
				appoint_interior_button.text = "Manage Appointment" if is_app else "Appoint Minister"
		"Health":
			if health_status_label:
				health_status_label.text = status_str
			if health_minister_label:
				health_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if health_ratings_label:
				health_ratings_label.text = ratings_str
			if health_effect_label:
				health_effect_label.text = effect_str
		"Infrastructure":
			if infrastructure_status_label:
				infrastructure_status_label.text = status_str
			if infrastructure_minister_label:
				infrastructure_minister_label.text = "Minister: " + min_name if not min_name.begins_with("Minister:") else min_name
			if infrastructure_ratings_label:
				infrastructure_ratings_label.text = ratings_str
			if infrastructure_effect_label:
				infrastructure_effect_label.text = effect_str
			if appoint_infrastructure_button:
				appoint_infrastructure_button.text = "Manage Appointment" if is_app else "Appoint Minister"

# ==============================================================================
# PHASE 4: CABINET APPOINTMENT MODAL & REQUEST FLOW
# ==============================================================================

func open_appointment_modal(portfolio: String) -> void:
	if not CANDIDATE_DEFINITIONS.has(portfolio):
		if appointment_status_label:
			appointment_status_label.text = "Portfolio '%s' does not currently support candidate commissioning." % portfolio
		return

	current_appointment_portfolio = portfolio
	selected_candidate_type = "technocrat"

	close_all_modals()
	if modal_backdrop:
		modal_backdrop.visible = true
	if cabinet_appointment_modal:
		cabinet_appointment_modal.visible = true

	_update_appointment_modal_content()

func _update_appointment_modal_content() -> void:
	if appointment_portfolio_label:
		appointment_portfolio_label.text = "Portfolio: Ministry of %s" % current_appointment_portfolio

	var p_data := _resolve_portfolio_display_data(current_appointment_portfolio, latest_state)
	var current_name: String = p_data.get("minister_name", "[Vacant]")
	var current_status: String = p_data.get("status", "Vacant (Unassigned)")
	if appointment_current_minister_label:
		appointment_current_minister_label.text = "Current Status: %s | %s" % [current_status, current_name]

	# Update candidate buttons styling / selection
	if candidate_technocrat_button:
		candidate_technocrat_button.text = "▶ Technocrat Candidate" if selected_candidate_type == "technocrat" else "Technocrat Candidate"
	if candidate_loyalist_button:
		candidate_loyalist_button.text = "▶ Loyalist Candidate" if selected_candidate_type == "loyalist" else "Loyalist Candidate"
	if candidate_dismiss_button:
		candidate_dismiss_button.text = "▶ Dismiss / Vacate" if selected_candidate_type == "dismiss" else "Dismiss / Vacate"

	var p_defs: Dictionary = CANDIDATE_DEFINITIONS.get(current_appointment_portfolio, {})
	var c_def: Dictionary = p_defs.get(selected_candidate_type, {})
	var c_name: String = str(c_def.get("name", "Unknown Candidate"))
	var comp: float = float(c_def.get("competence", 0.0))
	var loy: float = float(c_def.get("loyalty", 0.0))
	var integ: float = float(c_def.get("integrity", 0.0))
	var exp: float = float(c_def.get("experience", 0.0))
	var conn: float = float(c_def.get("connections", 0.0))
	var salary: int = int(c_def.get("salary", 0))
	var consequences: String = str(c_def.get("consequences", "No consequences detailed."))

	if candidate_preview_name_label:
		candidate_preview_name_label.text = "Candidate: " + c_name
	if candidate_preview_ratings_label:
		if selected_candidate_type == "dismiss":
			candidate_preview_ratings_label.text = "Ratings: Vacant position (portfolio unstaffed)"
		else:
			candidate_preview_ratings_label.text = "Competence: %d%% | Loyalty: %d%% | Integrity: %d%%\nExperience: %d%% | Political Connections: %d%%" % [
				int(round(comp * 100.0)), int(round(loy * 100.0)), int(round(integ * 100.0)), int(round(exp * 100.0)), int(round(conn * 100.0))
			]
	if candidate_preview_salary_label:
		candidate_preview_salary_label.text = "Monthly Ministerial Salary: R %s" % _format_number_commas(salary)
	if candidate_preview_consequences_label:
		candidate_preview_consequences_label.text = consequences

	if appointment_status_label:
		appointment_status_label.text = "Ready to commission %s candidate for Ministry of %s." % [selected_candidate_type.capitalize(), current_appointment_portfolio]

func _on_appoint_finance_pressed() -> void:
	open_appointment_modal("Finance")

func _on_appoint_interior_pressed() -> void:
	open_appointment_modal("Interior")

func _on_appoint_infrastructure_pressed() -> void:
	open_appointment_modal("Infrastructure")

func _on_candidate_technocrat_pressed() -> void:
	selected_candidate_type = "technocrat"
	_update_appointment_modal_content()

func _on_candidate_loyalist_pressed() -> void:
	selected_candidate_type = "loyalist"
	_update_appointment_modal_content()

func _on_candidate_dismiss_pressed() -> void:
	selected_candidate_type = "dismiss"
	_update_appointment_modal_content()

func _on_confirm_appointment_pressed() -> void:
	var result := execute_cabinet_appointment(current_appointment_portfolio, selected_candidate_type, true)
	if result.get("success", false):
		if appointment_status_label:
			appointment_status_label.text = result.get("message", "Appointment order submitted.")
		close_all_modals()
	else:
		if appointment_status_label:
			appointment_status_label.text = "Error: " + result.get("message", "Failed to submit appointment.")

var cli_processor_override: String = ""
var last_cli_pid: int = -1

func _resolve_request_path() -> String:
	if request_file_override != "":
		return request_file_override
	var base_path: String = current_state_path if not current_state_path.is_empty() else DEFAULT_STATE_FILE
	var dir := base_path.get_base_dir()
	return dir.path_join("presidential_desk_request.json")

func _resolve_result_path() -> String:
	var base_path: String = current_state_path if not current_state_path.is_empty() else DEFAULT_STATE_FILE
	var dir := base_path.get_base_dir()
	return dir.path_join("presidential_desk_result.json")

## Attempts execution of the CLI request processor if DLL or project is present
## Returns Dictionary: { "success": bool, "pid": int, "message": String }
func _try_run_cli_request_processor() -> Dictionary:
	if cli_processor_override == "none" or cli_processor_override == "missing":
		return {
			"success": false,
			"pid": -1,
			"message": "Simulation processor executable not found."
		}

	var candidates: Array[String] = [
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll"),
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"),
		"src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll",
		"src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll"
	]
	var cli_dll := ""
	if cli_processor_override != "":
		if FileAccess.file_exists(cli_processor_override):
			cli_dll = cli_processor_override
		else:
			return {
				"success": false,
				"pid": -1,
				"message": "Custom simulation processor not found: " + cli_processor_override
			}
	else:
		for c in candidates:
			var simp := c.simplify_path()
			if FileAccess.file_exists(simp):
				cli_dll = simp
				break
			elif FileAccess.file_exists(c):
				cli_dll = c
				break

	var state_path := current_state_path
	var req_path := _resolve_request_path()

	if not cli_dll.is_empty():
		var norm_dll := cli_dll.simplify_path()
		var norm_state := ProjectSettings.globalize_path(state_path).simplify_path()
		var norm_req := ProjectSettings.globalize_path(req_path).simplify_path()
		if OS.get_name() == "Windows":
			norm_dll = norm_dll.replace("/", "\\")
			norm_state = norm_state.replace("/", "\\")
			norm_req = norm_req.replace("/", "\\")

		var args: PackedStringArray = [norm_dll, "--process-request"]
		if not state_path.is_empty():
			args.append("--state-file=" + norm_state)
		if not req_path.is_empty():
			args.append("--request-file=" + norm_req)
		if not custom_save_path.is_empty():
			var norm_save := ProjectSettings.globalize_path(custom_save_path).simplify_path()
			if OS.get_name() == "Windows":
				norm_save = norm_save.replace("/", "\\")
			args.append("--save-file=" + norm_save)
		var pid := OS.create_process("dotnet", args)
		if pid < 0:
			last_cli_pid = -1
			return {
				"success": false,
				"pid": pid,
				"message": "Failed to launch simulation processor process."
			}
		last_cli_pid = pid
		return {
			"success": true,
			"pid": pid,
			"message": "Simulation processor launched successfully."
		}
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
				if not req_path.is_empty():
					args.append("--request-file=" + ProjectSettings.globalize_path(req_path))
				if not custom_save_path.is_empty():
					args.append("--save-file=" + ProjectSettings.globalize_path(custom_save_path))
				var pid := OS.create_process("dotnet", args)
				if pid < 0:
					last_cli_pid = -1
					return {
						"success": false,
						"pid": pid,
						"message": "Failed to launch simulation processor project."
					}
				last_cli_pid = pid
				return {
					"success": true,
					"pid": pid,
					"message": "Simulation processor launched via dotnet run."
				}
		last_cli_pid = -1
		return {
			"success": false,
			"pid": -1,
			"message": "Simulation processor executable not found."
		}

## Synchronously executes the CLI request processor and reloads state upon completion
func run_cli_request_processor_sync(timeout_msec: int = 8000) -> Dictionary:
	var pid: int = -1
	if last_cli_pid > 0 and OS.is_process_running(last_cli_pid):
		pid = last_cli_pid
	else:
		var proc := _try_run_cli_request_processor()
		if not proc.get("success", false):
			return proc
		pid = proc.get("pid", -1)
		if pid <= 0:
			return proc

	var start_t := Time.get_ticks_msec()
	while OS.is_process_running(pid):
		OS.delay_msec(25)
		if Time.get_ticks_msec() - start_t > timeout_msec:
			last_cli_pid = -1
			pending_appointment_request_id = ""
			var timeout_msg := "CLI processor timed out after %d ms." % timeout_msec
			if appointment_status_label:
				appointment_status_label.text = "Order Warning: " + timeout_msg
			return {
				"success": false,
				"message": timeout_msg,
				"pid": pid
			}
	last_cli_pid = -1

	var handled := check_for_decision_results()
	if not handled:
		var res_path := _resolve_result_path()
		if FileAccess.file_exists(res_path):
			return {
				"success": false,
				"message": "Decision result arrived, but its request ID did not match the pending appointment.",
				"pid": pid
			}
		else:
			var no_res_msg := "No decision result produced by simulation processor."
			if appointment_status_label:
				appointment_status_label.text = "Order Warning: " + no_res_msg
			return {
				"success": false,
				"message": no_res_msg,
				"pid": pid
			}

	return {
		"success": true,
		"message": "CLI processor completed successfully and matching result applied.",
		"pid": pid
	}

## Checks for and applies structured presidential decision results exported by C# core.
## Only accepts results matching the currently pending request ID.
func check_for_decision_results() -> bool:
	var res_path := _resolve_result_path()
	if not FileAccess.file_exists(res_path):
		return false

	var f := FileAccess.open(res_path, FileAccess.READ)
	if f == null:
		return false
	var text := f.get_as_text()
	f.close()

	var parsed: Variant = JSON.parse_string(text)
	if not (parsed is Dictionary) or parsed.is_empty():
		return false

	var res_dict: Dictionary = parsed
	var res_req_id: String = str(res_dict.get("request_id", res_dict.get("RequestId", "")))

	# Enforce request/result ID matching: reject mismatched or stale results
	if not pending_appointment_request_id.is_empty() and res_req_id != pending_appointment_request_id:
		var warn_msg := "[Mismatched Result] Ignored unrelated result for ID: " + res_req_id
		if appointment_status_label:
			appointment_status_label.text = warn_msg
		if status_ticker_label:
			status_ticker_label.text = "Cabinet Warning: " + warn_msg
		# Do NOT reload state, do NOT delete unrelated result file, do NOT clear pending_appointment_request_id
		return false

	var success: bool = bool(res_dict.get("success", false))
	var msg: String = str(res_dict.get("message", ""))
	var result_code: String = str(res_dict.get("result", ""))

	if appointment_status_label:
		if success:
			appointment_status_label.text = "Certified: " + msg
		elif result_code == "Cancelled":
			appointment_status_label.text = "[Cancelled] " + msg
		else:
			appointment_status_label.text = "[Rejected] " + msg

	if status_ticker_label:
		status_ticker_label.text = "Cabinet Result: " + msg

	# Reload the state to show the updated minister and capacities
	load_state_from_file(current_state_path)

	# Clean up processed matching result file and reset pending ID
	pending_appointment_request_id = ""
	DirAccess.remove_absolute(ProjectSettings.globalize_path(res_path))
	return true

func execute_cabinet_appointment(portfolio: String, candidate_type: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit cabinet appointment: No Republic simulation state loaded."
		if appointment_status_label:
			appointment_status_label.text = "Appointment Rejected: " + err_msg
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	# Clear any stale result file for this path before writing new request
	var stale_res_path := _resolve_result_path()
	if FileAccess.file_exists(stale_res_path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(stale_res_path))

	var req_id := "req_cab_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	pending_appointment_request_id = req_id
	pending_appointment_request_time = Time.get_ticks_msec()

	var request_dict := {
		"request_id": req_id,
		"action": "appoint_minister",
		"portfolio": portfolio,
		"candidate_type": candidate_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		pending_appointment_request_id = ""
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var proc_res := { "success": true, "message": "" }
	if confirmed:
		proc_res = _try_run_cli_request_processor()

	if confirmed and not proc_res.get("success", true):
		pending_appointment_request_id = ""
		var fail_msg: String = "Order written, but simulation processor failed: " + str(proc_res.get("message", "Processor unavailable."))
		if appointment_status_label:
			appointment_status_label.text = fail_msg
		if status_ticker_label:
			status_ticker_label.text = "Cabinet Order Warning: " + fail_msg
		return {
			"success": false,
			"processor_launched": false,
			"message": fail_msg,
			"request_id": req_id,
			"request_path": req_path
		}

	var success_msg := "Executive Appointment Submitted: %s for %s." % [candidate_type.capitalize(), portfolio]
	if appointment_status_label:
		appointment_status_label.text = success_msg
	if status_ticker_label:
		status_ticker_label.text = "Cabinet Order: " + success_msg

	return {
		"success": true,
		"processor_launched": proc_res.get("success", false),
		"message": success_msg,
		"request_id": req_id,
		"request_path": req_path
	}

func execute_diplomatic_response(proposal_id: String, decision: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit diplomatic response: No Republic simulation state loaded."
		if dossier_status_label:
			dossier_status_label.text = "Response Failed: " + err_msg
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_dip_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "diplomatic_response",
		"proposal_id": proposal_id,
		"decision": decision,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Diplomatic order submitted: %s proposal '%s'." % [decision.capitalize(), proposal_id]
	if dossier_status_label:
		dossier_status_label.text = "Certified: " + msg
	if status_ticker_label:
		status_ticker_label.text = "Diplomatic Dispatch: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

func execute_diplomatic_counter_proposal(
	proposal_id: String,
	offered_res: String,
	offered_qty: float,
	requested_res: String,
	requested_qty: float,
	currency_pmt: float,
	notes: String = "",
	confirmed: bool = true
) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit diplomatic counter-proposal: No Republic simulation state loaded."
		if dossier_status_label:
			dossier_status_label.text = "Counter Failed: " + err_msg
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_cntr_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "diplomatic_response",
		"proposal_id": proposal_id,
		"decision": "counter",
		"offered_resource": offered_res,
		"offered_quantity": offered_qty,
		"requested_resource": requested_res,
		"requested_quantity": requested_qty,
		"currency_payment": currency_pmt,
		"notes": notes,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Diplomatic counter-proposal submitted for '%s'. Prior approvals invalidated." % proposal_id
	if dossier_status_label:
		dossier_status_label.text = "Certified: " + msg
	if status_ticker_label:
		status_ticker_label.text = "Diplomatic Dispatch: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

func execute_crisis_response(decree_type: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit crisis response: No Republic simulation state loaded."
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_cr_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "crisis_response",
		"decree_type": decree_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Crisis decree enacted: %s." % decree_type.capitalize()
	if status_ticker_label:
		status_ticker_label.text = "Presidential Decree: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

# ==============================================================================
# 7. PHASE 2 ACTIVE OFFICIALS & SECURITY LEADERSHIP HANDLERS
# ==============================================================================
func _update_chief_of_staff_briefing_ui(state: Dictionary) -> void:
	if not briefing_modal:
		return
	var briefing_content := _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		return

	var cos_section := briefing_content.get_node_or_null("ChiefOfStaffSection") as VBoxContainer
	if not cos_section:
		cos_section = VBoxContainer.new()
		cos_section.name = "ChiefOfStaffSection"
		cos_section.add_theme_constant_override("separation", 6)
		briefing_content.add_child(cos_section)
		briefing_content.move_child(cos_section, 0)

	for child in cos_section.get_children():
		child.queue_free()

	var cos_briefing := str(state.get("chief_of_staff_briefing", ""))
	if not cos_briefing.is_empty():
		var header_label := Label.new()
		header_label.text = "🎖️ Chief of Staff Memorandum: " + cos_briefing
		header_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		header_label.add_theme_color_override("font_color", Color(0.95, 0.8, 0.2))
		header_label.add_theme_font_size_override("font_size", 12)
		cos_section.add_child(header_label)

	var recs = state.get("chief_of_staff_recommendations", [])
	if recs is Array and not recs.is_empty():
		var recs_title := Label.new()
		recs_title.text = "Priority Government Recommendations:"
		recs_title.add_theme_font_size_override("font_size", 11)
		recs_title.add_theme_color_override("font_color", Color(0.8, 0.85, 0.9))
		cos_section.add_child(recs_title)

		for rec in recs:
			if not (rec is Dictionary):
				continue
			var r_id: String = str(rec.get("id", ""))
			var r_title: String = str(rec.get("title", "Untitled Recommendation"))
			var r_priority: String = str(rec.get("priority", "Medium"))
			var r_urgent: bool = bool(rec.get("is_urgent", false))
			var r_status: String = str(rec.get("status", "Pending"))
			var r_action: String = str(rec.get("recommended_action", ""))
			var r_summary: String = str(rec.get("summary", ""))
			var r_cost: float = float(rec.get("estimated_cost", 0.0))

			var card := PanelContainer.new()
			var card_vbox := VBoxContainer.new()
			card_vbox.add_theme_constant_override("separation", 3)
			card.add_child(card_vbox)

			var header_hbox := HBoxContainer.new()
			var badge := Label.new()
			var pri_prefix := "[%s]" % r_priority.to_upper()
			if r_urgent:
				pri_prefix = "⚠️ [CRITICAL URGENT]"
			badge.text = pri_prefix
			if r_urgent or r_priority.to_lower() == "critical":
				badge.add_theme_color_override("font_color", Color(1.0, 0.3, 0.3))
			elif r_priority.to_lower() == "high":
				badge.add_theme_color_override("font_color", Color(1.0, 0.65, 0.2))
			else:
				badge.add_theme_color_override("font_color", Color(0.4, 0.8, 0.4))
			badge.add_theme_font_size_override("font_size", 10)
			header_hbox.add_child(badge)

			var title_lbl := Label.new()
			title_lbl.text = r_title
			title_lbl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
			title_lbl.add_theme_font_size_override("font_size", 11)
			header_hbox.add_child(title_lbl)

			var status_lbl := Label.new()
			status_lbl.text = "Status: " + r_status
			status_lbl.add_theme_font_size_override("font_size", 10)
			status_lbl.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
			header_hbox.add_child(status_lbl)
			card_vbox.add_child(header_hbox)

			var desc_lbl := Label.new()
			var cost_str := " (Cost: R %s)" % _format_number_commas(r_cost) if r_cost > 0 else " (No Direct Cost)"
			desc_lbl.text = "%s Recommended: %s%s" % [r_summary, r_action, cost_str]
			desc_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
			desc_lbl.add_theme_font_size_override("font_size", 10)
			card_vbox.add_child(desc_lbl)

			if r_status == "Pending":
				var btn_hbox := HBoxContainer.new()
				btn_hbox.alignment = BoxContainer.ALIGNMENT_END

				var accept_btn := Button.new()
				accept_btn.text = "Accept"
				accept_btn.add_theme_font_size_override("font_size", 10)
				accept_btn.pressed.connect(func():
					execute_chief_of_staff_decision(r_id, "accept")
					status_lbl.text = "Status: Accepted"
					accept_btn.disabled = true
				)
				btn_hbox.add_child(accept_btn)

				var reject_btn := Button.new()
				reject_btn.text = "Reject"
				reject_btn.add_theme_font_size_override("font_size", 10)
				reject_btn.pressed.connect(func():
					execute_chief_of_staff_decision(r_id, "reject")
					status_lbl.text = "Status: Rejected"
					reject_btn.disabled = true
				)
				btn_hbox.add_child(reject_btn)

				var defer_btn := Button.new()
				defer_btn.text = "Defer"
				defer_btn.add_theme_font_size_override("font_size", 10)
				defer_btn.pressed.connect(func():
					execute_chief_of_staff_decision(r_id, "defer")
					status_lbl.text = "Status: Deferred"
					defer_btn.disabled = true
				)
				btn_hbox.add_child(defer_btn)

				card_vbox.add_child(btn_hbox)

			cos_section.add_child(card)

func _update_security_leadership_dossier_ui(state: Dictionary) -> void:
	if not dossier_modal:
		return
	var dossier_vbox := _get_node_safe("InterfaceLayer/DossierModal/DossierVBox") as VBoxContainer
	if not dossier_vbox:
		return

	var sec_summary := str(state.get("security_assessment_summary", ""))
	var dom_idx := float(state.get("domestic_threat_index", 20.0))
	var ext_idx := float(state.get("external_threat_index", 15.0))
	if not sec_summary.is_empty() and dossier_defense_label:
		var defcon := str(state.get("defcon_level", "DEFCON 5 (Peacetime Watch)"))
		var readiness := str(state.get("military_readiness", "85.0% Operational"))
		dossier_defense_label.text = "Defense Readiness: %s | %s\nThreat Index: Domestic %0.1f | External %0.1f\nAssessment: %s" % [defcon, readiness, dom_idx, ext_idx, sec_summary]

	var threats_section := dossier_vbox.get_node_or_null("SecurityThreatsSection") as VBoxContainer
	if not threats_section:
		threats_section = VBoxContainer.new()
		threats_section.name = "SecurityThreatsSection"
		threats_section.add_theme_constant_override("separation", 6)
		var actions_idx := dossier_vbox.get_child_count() - 1
		dossier_vbox.add_child(threats_section)
		if actions_idx >= 0:
			dossier_vbox.move_child(threats_section, actions_idx)

	for child in threats_section.get_children():
		child.queue_free()

	var threats = state.get("security_threats", [])
	if threats is Array and not threats.is_empty():
		var threats_title := Label.new()
		threats_title.text = "Active Security Threats & Operational Directives:"
		threats_title.add_theme_font_size_override("font_size", 11)
		threats_title.add_theme_color_override("font_color", Color(0.9, 0.7, 0.2))
		threats_section.add_child(threats_title)

		for threat in threats:
			if not (threat is Dictionary):
				continue
			var t_id: String = str(threat.get("threat_id", ""))
			var t_title: String = str(threat.get("title", "Unknown Threat"))
			var t_cat: String = str(threat.get("category", "General"))
			var t_agency: String = str(threat.get("source_agency", "Security Council"))
			var t_conf: String = str(threat.get("confidence", "UnverifiedRumor"))
			var t_sev: String = str(threat.get("severity", "Moderate"))
			var t_status: String = str(threat.get("status", "Active"))
			var t_details: String = str(threat.get("details", ""))
			var t_rec: String = str(threat.get("recommended_response", ""))

			var card := PanelContainer.new()
			var card_vbox := VBoxContainer.new()
			card_vbox.add_theme_constant_override("separation", 2)
			card.add_child(card_vbox)

			var header_hbox := HBoxContainer.new()

			var conf_badge := Label.new()
			var conf_text := ""
			match t_conf:
				"Confirmed":
					conf_text = "[CONFIRMED]"
					conf_badge.add_theme_color_override("font_color", Color(1.0, 0.2, 0.2))
				"HighProbability":
					conf_text = "[PROBABLE]"
					conf_badge.add_theme_color_override("font_color", Color(1.0, 0.6, 0.2))
				"InterceptedSignal":
					conf_text = "[SIGNAL]"
					conf_badge.add_theme_color_override("font_color", Color(0.3, 0.7, 1.0))
				_:
					conf_text = "[UNVERIFIED]"
					conf_badge.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
			conf_badge.text = conf_text
			conf_badge.add_theme_font_size_override("font_size", 10)
			header_hbox.add_child(conf_badge)

			var title_lbl := Label.new()
			title_lbl.text = "%s (%s)" % [t_title, t_agency]
			title_lbl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
			title_lbl.add_theme_font_size_override("font_size", 11)
			header_hbox.add_child(title_lbl)

			var sev_lbl := Label.new()
			sev_lbl.text = "Severity: " + t_sev
			sev_lbl.add_theme_font_size_override("font_size", 10)
			header_hbox.add_child(sev_lbl)
			card_vbox.add_child(header_hbox)

			var det_lbl := Label.new()
			det_lbl.text = "%s Recommended: %s" % [t_details, t_rec]
			det_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
			det_lbl.add_theme_font_size_override("font_size", 10)
			card_vbox.add_child(det_lbl)

			var btn_hbox := HBoxContainer.new()
			btn_hbox.alignment = BoxContainer.ALIGNMENT_END

			var mob_btn := Button.new()
			mob_btn.text = "Mobilize Defense"
			mob_btn.add_theme_font_size_override("font_size", 10)
			mob_btn.pressed.connect(func():
				execute_security_directive(t_id, "mobilize")
				mob_btn.disabled = true
			)
			btn_hbox.add_child(mob_btn)

			var sweep_btn := Button.new()
			sweep_btn.text = "Sweep Operations"
			sweep_btn.add_theme_font_size_override("font_size", 10)
			sweep_btn.pressed.connect(func():
				execute_security_directive(t_id, "sweep")
				sweep_btn.disabled = true
			)
			btn_hbox.add_child(sweep_btn)

			var deesc_btn := Button.new()
			deesc_btn.text = "De-escalate"
			deesc_btn.add_theme_font_size_override("font_size", 10)
			deesc_btn.pressed.connect(func():
				execute_security_directive(t_id, "deescalate")
				deesc_btn.disabled = true
			)
			btn_hbox.add_child(deesc_btn)

			card_vbox.add_child(btn_hbox)
			threats_section.add_child(card)

func execute_chief_of_staff_decision(recommendation_id: String, decision: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit Chief of Staff decision: No Republic simulation state loaded."
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_cos_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "cos_recommendation",
		"recommendation_id": recommendation_id,
		"decision": decision,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Chief of Staff order submitted: %s recommendation '%s'." % [decision.capitalize(), recommendation_id]
	if status_ticker_label:
		status_ticker_label.text = "Executive Order: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

func execute_security_directive(threat_id: String, directive: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit security directive: No Republic simulation state loaded."
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_sec_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "security_directive",
		"threat_id": threat_id,
		"directive": directive,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "National security directive issued: %s on threat '%s'." % [directive.capitalize(), threat_id]
	if status_ticker_label:
		status_ticker_label.text = "National Command: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

# ==============================================================================
# 8. PHASE 3 MEDIA INFLUENCE & PRIVATE-SECTOR INVESTMENT HANDLERS
# ==============================================================================
func _update_media_environment_briefing_ui(state: Dictionary) -> void:
	if not briefing_modal:
		return
	var briefing_content := _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		return

	var media_section := briefing_content.get_node_or_null("MediaEnvironmentSection") as VBoxContainer
	if not media_section:
		media_section = VBoxContainer.new()
		media_section.name = "MediaEnvironmentSection"
		media_section.add_theme_constant_override("separation", 6)
		briefing_content.add_child(media_section)

	for child in media_section.get_children():
		child.queue_free()

	var media_summary := str(state.get("media_summary", "Media Environment: Free press operating."))
	var header_label := Label.new()
	header_label.text = "📰 National Press & Media Wire: " + media_summary
	header_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	header_label.add_theme_color_override("font_color", Color(0.3, 0.8, 0.95))
	header_label.add_theme_font_size_override("font_size", 12)
	media_section.add_child(header_label)

	var reports = state.get("media_reports", [])
	if reports is Array and not reports.is_empty():
		var reports_title := Label.new()
		reports_title.text = "Recent Journalistic Dispatches & Public Coverage:"
		reports_title.add_theme_font_size_override("font_size", 11)
		reports_title.add_theme_color_override("font_color", Color(0.8, 0.85, 0.9))
		media_section.add_child(reports_title)

		for rep in reports:
			if not (rep is Dictionary):
				continue
			var rep_id: String = str(rep.get("id", ""))
			var outlet_name: String = str(rep.get("outlet_name", "Press"))
			var leaning: String = str(rep.get("leaning", "StateAligned"))
			var headline: String = str(rep.get("headline", "Untitled Report"))
			var summary: String = str(rep.get("article_summary", rep.get("body", "")))
			var sentiment: String = str(rep.get("sentiment", "Neutral"))
			var event_source: String = str(rep.get("event_source", "General"))
			var credibility: float = float(rep.get("outlet_credibility", rep.get("credibility", 0.7)))
			var reach: float = float(rep.get("outlet_reach", rep.get("reach", 50000.0)))
			var resp_status: String = str(rep.get("response_status", "Unanswered"))
			var is_suppressed: bool = bool(rep.get("is_suppressed", false))
			var is_leak: bool = bool(rep.get("is_underground_leak", rep.get("is_leaked", false)))

			var card := PanelContainer.new()
			var card_vbox := VBoxContainer.new()
			card_vbox.add_theme_constant_override("separation", 3)
			card.add_child(card_vbox)

			var header_hbox := HBoxContainer.new()
			var outlet_badge := Label.new()
			var leak_tag := " [UNDERGROUND SAMIZDAT]" if is_leak else ""
			outlet_badge.text = "[%s | %s | Reach: %0.0f%% | Cred: %0.0f%%%s]" % [outlet_name, leaning, reach, credibility, leak_tag]
			if is_leak:
				outlet_badge.add_theme_color_override("font_color", Color(1.0, 0.25, 0.25))
			elif sentiment.to_lower() == "critical" or sentiment.to_lower() == "sensationalized":
				outlet_badge.add_theme_color_override("font_color", Color(1.0, 0.65, 0.2))
			elif sentiment.to_lower() == "positive":
				outlet_badge.add_theme_color_override("font_color", Color(0.4, 0.9, 0.4))
			else:
				outlet_badge.add_theme_color_override("font_color", Color(0.7, 0.8, 0.9))
			outlet_badge.add_theme_font_size_override("font_size", 10)
			header_hbox.add_child(outlet_badge)
			card_vbox.add_child(header_hbox)

			var headline_lbl := Label.new()
			headline_lbl.text = "Headline: " + headline
			headline_lbl.add_theme_font_size_override("font_size", 11)
			card_vbox.add_child(headline_lbl)

			var body_lbl := Label.new()
			body_lbl.text = "%s (Trigger: %s)" % [summary, event_source]
			body_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
			body_lbl.add_theme_font_size_override("font_size", 10)
			card_vbox.add_child(body_lbl)

			var status_hbox := HBoxContainer.new()
			var status_lbl := Label.new()
			status_lbl.text = "Status: " + resp_status
			status_lbl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
			status_lbl.add_theme_font_size_override("font_size", 10)
			status_lbl.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
			status_hbox.add_child(status_lbl)
			card_vbox.add_child(status_hbox)

			if resp_status == "Unanswered" and not is_suppressed:
				var btn_hbox := HBoxContainer.new()
				btn_hbox.alignment = BoxContainer.ALIGNMENT_END

				var statement_btn := Button.new()
				statement_btn.text = "Statement (R 10,000)"
				statement_btn.add_theme_font_size_override("font_size", 10)
				statement_btn.pressed.connect(func():
					execute_media_response(rep_id, "statement")
					status_lbl.text = "Status: StatementIssued"
					statement_btn.disabled = true
				)
				btn_hbox.add_child(statement_btn)

				var bribe_btn := Button.new()
				bribe_btn.text = "Hush Money (R 150,000)"
				bribe_btn.add_theme_font_size_override("font_size", 10)
				bribe_btn.pressed.connect(func():
					execute_media_response(rep_id, "hush_money")
					status_lbl.text = "Status: HushMoneyAttempted"
					bribe_btn.disabled = true
				)
				btn_hbox.add_child(bribe_btn)

				var censor_btn := Button.new()
				censor_btn.text = "Gag Order / Censor"
				censor_btn.add_theme_font_size_override("font_size", 10)
				censor_btn.pressed.connect(func():
					execute_media_response(rep_id, "censorship")
					status_lbl.text = "Status: Censored"
					censor_btn.disabled = true
				)
				btn_hbox.add_child(censor_btn)

				card_vbox.add_child(btn_hbox)

			media_section.add_child(card)

func _update_private_sector_investment_briefing_ui(state: Dictionary) -> void:
	if not briefing_modal:
		return
	var briefing_content := _get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		return

	var invest_section := briefing_content.get_node_or_null("PrivateInvestmentSection") as VBoxContainer
	if not invest_section:
		invest_section = VBoxContainer.new()
		invest_section.name = "PrivateInvestmentSection"
		invest_section.add_theme_constant_override("separation", 6)
		briefing_content.add_child(invest_section)

	for child in invest_section.get_children():
		child.queue_free()

	var invest_summary := str(state.get("investment_summary", "Private Sector: Corporate investment channels open."))
	var header_label := Label.new()
	header_label.text = "💼 Private Sector Inbound Proposals: " + invest_summary
	header_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	header_label.add_theme_color_override("font_color", Color(0.4, 0.9, 0.5))
	header_label.add_theme_font_size_override("font_size", 12)
	invest_section.add_child(header_label)

	var proposals = state.get("investment_proposals", [])
	if proposals is Array and not proposals.is_empty():
		for prop in proposals:
			if not (prop is Dictionary):
				continue
			var prop_id: String = str(prop.get("id", ""))
			var investor_name: String = str(prop.get("investor_name", "Corporate Investor"))
			var title: String = str(prop.get("title", "Industrial Venture"))
			var sector: String = str(prop.get("sector", "Manufacturing"))
			var location: String = str(prop.get("location", "Capital Region"))
			var inv_contribution: float = float(prop.get("investor_contribution", prop.get("proposed_investment", 0.0)))
			var gov_funding: float = float(prop.get("requested_government_funding", prop.get("government_funding_requested", 0.0)))
			var tax_concession: float = float(prop.get("requested_tax_concession", prop.get("tax_concession_requested", 0.0)))
			var construction_days: int = int(prop.get("estimated_construction_days", prop.get("construction_days", 5)))
			var jobs_created: int = int(prop.get("projected_jobs_created", prop.get("projected_jobs", 500)))
			var gdp_boost: float = float(prop.get("projected_gdp_boost", 0.0))
			var annual_tax: float = float(prop.get("projected_annual_tax_revenue", prop.get("projected_annual_tax_yield", 0.0)))
			var risks: String = str(prop.get("risks_and_conditions", prop.get("conditions_and_risks", "")))
			var p_status: String = str(prop.get("status", "Pending"))

			var card := PanelContainer.new()
			var card_vbox := VBoxContainer.new()
			card_vbox.add_theme_constant_override("separation", 3)
			card.add_child(card_vbox)

			var header_hbox := HBoxContainer.new()
			var sector_badge := Label.new()
			sector_badge.text = "[%s | %s | %s]" % [investor_name, sector, location]
			sector_badge.add_theme_color_override("font_color", Color(0.9, 0.8, 0.3))
			sector_badge.add_theme_font_size_override("font_size", 10)
			header_hbox.add_child(sector_badge)

			var title_lbl := Label.new()
			title_lbl.text = title
			title_lbl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
			title_lbl.add_theme_font_size_override("font_size", 11)
			header_hbox.add_child(title_lbl)

			var status_lbl := Label.new()
			status_lbl.text = "Status: " + p_status
			status_lbl.add_theme_font_size_override("font_size", 10)
			status_lbl.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
			header_hbox.add_child(status_lbl)
			card_vbox.add_child(header_hbox)

			var finance_lbl := Label.new()
			finance_lbl.text = "Private Capital: R %s | Requested Public Co-Funding: R %s (Tax Relief: %0.1f%%)" % [
				_format_number_commas(inv_contribution), _format_number_commas(gov_funding), tax_concession
			]
			finance_lbl.add_theme_font_size_override("font_size", 10)
			card_vbox.add_child(finance_lbl)

			var impact_lbl := Label.new()
			impact_lbl.text = "Expected Return: +%d Full-Time Jobs | +R %s GDP Output | +R %s/yr Tax Revenue | Construction: %d Days" % [
				jobs_created, _format_number_commas(gdp_boost), _format_number_commas(annual_tax), construction_days
			]
			impact_lbl.add_theme_font_size_override("font_size", 10)
			card_vbox.add_child(impact_lbl)

			if not risks.is_empty():
				var risks_lbl := Label.new()
				risks_lbl.text = "Conditions & Risk: " + risks
				risks_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
				risks_lbl.add_theme_font_size_override("font_size", 9)
				risks_lbl.add_theme_color_override("font_color", Color(0.85, 0.7, 0.7))
				card_vbox.add_child(risks_lbl)

			if p_status == "Pending" or p_status == "Negotiated":
				var btn_hbox := HBoxContainer.new()
				btn_hbox.alignment = BoxContainer.ALIGNMENT_END

				var accept_btn := Button.new()
				accept_btn.text = "Authorize Co-Funding (R %s)" % _format_number_commas(gov_funding)
				accept_btn.add_theme_font_size_override("font_size", 10)
				accept_btn.pressed.connect(func():
					execute_investment_decision(prop_id, "accept")
					status_lbl.text = "Status: Accepted"
					accept_btn.disabled = true
				)
				btn_hbox.add_child(accept_btn)

				if p_status == "Pending":
					var negotiate_btn := Button.new()
					negotiate_btn.text = "Renegotiate Terms (-25% State Share)"
					negotiate_btn.add_theme_font_size_override("font_size", 10)
					negotiate_btn.pressed.connect(func():
						execute_investment_decision(prop_id, "negotiate")
						status_lbl.text = "Status: Negotiated"
						negotiate_btn.disabled = true
					)
					btn_hbox.add_child(negotiate_btn)

				var reject_btn := Button.new()
				reject_btn.text = "Decline Proposal"
				reject_btn.add_theme_font_size_override("font_size", 10)
				reject_btn.pressed.connect(func():
					execute_investment_decision(prop_id, "reject")
					status_lbl.text = "Status: Rejected"
					reject_btn.disabled = true
				)
				btn_hbox.add_child(reject_btn)

				card_vbox.add_child(btn_hbox)

			invest_section.add_child(card)

func execute_media_response(report_id: String, response_type: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit media response: No Republic simulation state loaded."
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_media_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "media_response",
		"report_id": report_id,
		"response_type": response_type,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Media directive enacted: %s on report '%s'." % [response_type.capitalize(), report_id]
	if status_ticker_label:
		status_ticker_label.text = "Press Office: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}

func execute_investment_decision(proposal_id: String, decision: String, confirmed: bool = true) -> Dictionary:
	if latest_state.is_empty():
		var err_msg := "Cannot submit investment decision: No Republic simulation state loaded."
		return { "success": false, "message": err_msg }

	var req_path := _resolve_request_path()
	var req_dir := req_path.get_base_dir()
	if not req_dir.is_empty():
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(req_dir))

	var req_id := "req_invest_" + str(Time.get_unix_time_from_system()) + "_" + str(randi())
	var timestamp_str := Time.get_datetime_string_from_system()

	var request_dict := {
		"request_id": req_id,
		"action": "investment_decision",
		"proposal_id": proposal_id,
		"decision": decision,
		"confirmed": confirmed,
		"timestamp": timestamp_str,
		"requested_at": timestamp_str
	}

	var req_file := FileAccess.open(req_path, FileAccess.WRITE)
	if req_file == null:
		var err_msg := "Failed to open request file for writing: " + req_path
		return { "success": false, "message": err_msg }

	req_file.store_string(JSON.stringify(request_dict, "\t"))
	req_file.close()

	var msg := "Commercial investment decision registered: %s on proposal '%s'." % [decision.capitalize(), proposal_id]
	if status_ticker_label:
		status_ticker_label.text = "Commercial Registry: " + msg

	return {
		"success": true,
		"message": msg,
		"request_id": req_id,
		"request_path": req_path
	}





