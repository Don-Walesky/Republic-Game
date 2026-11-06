# ==============================================================================
# Republic - New Player Onboarding Controller (Phase 2 & Phase 3)
# Handles the complete onboarding sequence:
# 1. Account Registration (First/Last name, Email, DOB - PII isolated)
# 2. Email Verification (Official dispatch display, "Verify Email" button, code input, resend)
# 3. Country Naming (Validation, 'Republic' requirement, confirmation modal)
# 4. Procedural Country Generation (Animated progress, 50M REPU, 60% approval, 10M pop)
# 5. Country Creation Confirmation (Ratified sovereign profile, persists & resumes)
# 6. Leadership Identity Selection (Exactly 2 of 5 archetypes -> dual-trait doctrine)
# 7. National Development Agenda (Exactly 4 of 10 priorities -> inaugural focus)
# 8. Phase 3 Ratification Summary (Sovereign charter ratified, ready for Phase 4)
# ==============================================================================
class_name OnboardingController
extends Control

signal onboarding_completed(country_data: Dictionary)
signal onboarding_step_changed(step_name: String)

enum ScreenStage {
	REGISTER,
	VERIFY_EMAIL,
	COUNTRY_NAME,
	GENERATING,
	CONFIRMED,
	LEADERSHIP,
	DEVELOPMENT_PLANS,
	PHASE3_SUMMARY,
	ADVISERS,
	CAMPAIGN_HQ
}

var current_stage: ScreenStage = ScreenStage.REGISTER
var active_state_path: String = "res://presidential_desk_state.json"
var active_save_path: String = "res://session.sav"
var active_request_path: String = "res://presidential_desk_request.json"
var active_result_path: String = "res://presidential_desk_result.json"

# In-memory session state
var registered_first_name: String = ""
var registered_last_name: String = ""
var registered_username: String = ""
var campaign_time_remaining: float = 1800.0
var campaign_timer_active: bool = false
var active_adviser_filter: String = "all" 
var registered_email: String = ""
var registered_dob: String = ""
var selected_gender: String = ""
var selected_title: String = ""
var selected_avatar: String = ""
var verification_token: String = ""
var verification_email_data: Dictionary = {}
var created_country_data: Dictionary = {}

# Phase 3 selections
var selected_archetypes: Array[String] = []
var selected_plans: Array[String] = []
var selected_doctrine_title: String = ""
var selected_plan_names: Array[String] = []
var active_advisers_data: Dictionary = {}
var sovereign_gems_count: int = 500
var active_campaign_data: Dictionary = {}

var is_processing: bool = false
var resend_cooldown_timer: float = 0.0

# Screen Panels
@onready var register_panel: Control = $PanelContainer/VBox/ScreenRegister
@onready var verify_panel: Control = $PanelContainer/VBox/ScreenVerify
@onready var name_panel: Control = $PanelContainer/VBox/ScreenCountryName
@onready var generating_panel: Control = $PanelContainer/VBox/ScreenGenerating
@onready var confirmed_panel: Control = $PanelContainer/VBox/ScreenConfirmed
@onready var leadership_panel: Control = $PanelContainer/VBox/ScreenLeadership
@onready var plans_panel: Control = $PanelContainer/VBox/ScreenDevelopmentPlans
@onready var summary_panel: Control = $PanelContainer/VBox/ScreenPhase3Summary

# Registration Nodes
@onready var btn_mode_new_republic: Button = $PanelContainer/VBox/ScreenRegister/SessionModeHBox/BtnModeNewRepublic
@onready var btn_mode_resume: Button = $PanelContainer/VBox/ScreenRegister/SessionModeHBox/BtnModeResumeSession
@onready var form_register: VBoxContainer = $PanelContainer/VBox/ScreenRegister/Form
@onready var login_form: VBoxContainer = $PanelContainer/VBox/ScreenRegister/LoginForm
@onready var lbl_session_details: Label = $PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/LblSessionDetails
@onready var lbl_session_phase: Label = $PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/LblSessionPhase
@onready var btn_quick_login: Button = $PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/BtnQuickLogin
@onready var input_login_email: LineEdit = $PanelContainer/VBox/ScreenRegister/LoginForm/ManualLoginVBox/LoginEmailInput
@onready var btn_submit_login: Button = $PanelContainer/VBox/ScreenRegister/LoginForm/ManualLoginVBox/BtnSubmitLogin
@onready var lbl_login_feedback: Label = $PanelContainer/VBox/ScreenRegister/LoginForm/LblLoginFeedback
@onready var btn_campaign_signout: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignActions/BtnCampaignSignOut
@onready var input_first_name: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/FirstNameInput
@onready var input_last_name: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/LastNameInput
@onready var input_username: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/UsernameInput
@onready var input_email: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/EmailInput
@onready var spin_dob_year: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/YearSpin
@onready var spin_dob_month: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/MonthSpin
@onready var spin_dob_day: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/DaySpin
@onready var btn_gender_female: Button = $PanelContainer/VBox/ScreenRegister/Form/HBoxGender/BtnGenderFemale
@onready var btn_gender_male: Button = $PanelContainer/VBox/ScreenRegister/Form/HBoxGender/BtnGenderMale
@onready var btn_register: Button = $PanelContainer/VBox/ScreenRegister/BtnRegister
@onready var lbl_register_feedback: Label = $PanelContainer/VBox/ScreenRegister/LblFeedback

# Verification Nodes
@onready var lbl_email_sender: Label = $PanelContainer/VBox/ScreenVerify/CardEmail/LblSender
@onready var lbl_email_recipient: Label = $PanelContainer/VBox/ScreenVerify/CardEmail/LblRecipient
@onready var lbl_email_subject: Label = $PanelContainer/VBox/ScreenVerify/CardEmail/LblSubject
@onready var lbl_email_body: Label = $PanelContainer/VBox/ScreenVerify/CardEmail/LblBody
@onready var btn_verify_email_action: Button = $PanelContainer/VBox/ScreenVerify/CardEmail/BtnVerifyEmailAction
@onready var input_token: LineEdit = $PanelContainer/VBox/ScreenVerify/ManualVerify/TokenInput
@onready var btn_submit_code: Button = $PanelContainer/VBox/ScreenVerify/ManualVerify/BtnSubmitCode
@onready var btn_resend_email: Button = $PanelContainer/VBox/ScreenVerify/BtnResendEmail
@onready var lbl_verify_feedback: Label = $PanelContainer/VBox/ScreenVerify/LblFeedback

# Country Naming Nodes
@onready var input_country_name: LineEdit = $PanelContainer/VBox/ScreenCountryName/InputCountryName
@onready var btn_review_founding: Button = $PanelContainer/VBox/ScreenCountryName/BtnReviewFounding
@onready var lbl_name_feedback: Label = $PanelContainer/VBox/ScreenCountryName/LblFeedback
@onready var confirmation_modal: Control = $PanelContainer/VBox/ScreenCountryName/ConfirmationModal
@onready var lbl_modal_country: Label = $PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/LblCountryPreview
@onready var btn_confirm_founding: Button = $PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/HBox/BtnConfirm
@onready var btn_cancel_modal: Button = $PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/HBox/BtnCancel

# Flag Creator Nodes
@onready var flag_preview: Control = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagPreviewContainer/FlagPreview
@onready var btn_random_flag: Button = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagCreatorHeader/BtnRandomFlag
@onready var opt_flag_pattern: OptionButton = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptPattern
@onready var opt_flag_primary: OptionButton = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptPrimary
@onready var opt_flag_secondary: OptionButton = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptSecondary
@onready var opt_flag_emblem: OptionButton = $PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptEmblem

# Generating Nodes
@onready var lbl_generating_status: Label = $PanelContainer/VBox/ScreenGenerating/LblStatus
@onready var progress_bar: ProgressBar = $PanelContainer/VBox/ScreenGenerating/ProgressBar

# Confirmed Nodes
@onready var lbl_confirmed_title: Label = $PanelContainer/VBox/ScreenConfirmed/LblTitle
@onready var lbl_stat_country: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblCountryVal
@onready var lbl_stat_capital: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblCapitalVal
@onready var lbl_stat_population: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblPopVal
@onready var lbl_stat_treasury: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblTreasuryVal
@onready var lbl_stat_approval: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblApprovalVal
@onready var lbl_stat_stability: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblStabilityVal
@onready var lbl_stat_leader: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblLeaderVal
@onready var lbl_stat_constitution: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblConstitutionVal
@onready var lbl_stat_resource: Label = $PanelContainer/VBox/ScreenConfirmed/Grid/LblResourceVal
@onready var btn_resume_session: Button = $PanelContainer/VBox/ScreenConfirmed/BtnResumeSession
@onready var btn_proceed_leadership: Button = $PanelContainer/VBox/ScreenConfirmed/BtnProceedLeadership

# Leadership Selection Nodes
@onready var lbl_leader_count: Label = $PanelContainer/VBox/ScreenLeadership/LblLeaderCount
@onready var btn_random_leadership: Button = $PanelContainer/VBox/ScreenLeadership/BtnRandomLeadership
@onready var lbl_doctrine_preview: Label = $PanelContainer/VBox/ScreenLeadership/LblDoctrinePreview
@onready var btn_confirm_leadership: Button = $PanelContainer/VBox/ScreenLeadership/BtnConfirmLeadership
@onready var lbl_leadership_feedback: Label = $PanelContainer/VBox/ScreenLeadership/LblFeedback
@onready var btn_arch_technocrat: Button = $PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardTechnocrat/VBox/HBoxHeader/BtnSelectTechnocrat
@onready var btn_arch_populist: Button = $PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardPopulist/VBox/HBoxHeader/BtnSelectPopulist
@onready var btn_arch_visionary: Button = $PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardVisionary/VBox/HBoxHeader/BtnSelectVisionary
@onready var btn_arch_statist: Button = $PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardStatist/VBox/HBoxHeader/BtnSelectStatist
@onready var btn_arch_diplomat: Button = $PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardDiplomat/VBox/HBoxHeader/BtnSelectDiplomat

# Development Plans Nodes
@onready var lbl_plans_count: Label = $PanelContainer/VBox/ScreenDevelopmentPlans/LblPlansCount
@onready var btn_random_plans: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/BtnRandomPlans
@onready var btn_confirm_plans: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/BtnConfirmDevelopmentPlans
@onready var lbl_plans_feedback: Label = $PanelContainer/VBox/ScreenDevelopmentPlans/LblFeedback
@onready var btn_plan_1: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan1/VBox/HBox/BtnPlan1
@onready var btn_plan_2: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan2/VBox/HBox/BtnPlan2
@onready var btn_plan_3: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan3/VBox/HBox/BtnPlan3
@onready var btn_plan_4: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan4/VBox/HBox/BtnPlan4
@onready var btn_plan_5: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan5/VBox/HBox/BtnPlan5
@onready var btn_plan_6: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan6/VBox/HBox/BtnPlan6
@onready var btn_plan_7: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan7/VBox/HBox/BtnPlan7
@onready var btn_plan_8: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan8/VBox/HBox/BtnPlan8
@onready var btn_plan_9: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan9/VBox/HBox/BtnPlan9
@onready var btn_plan_10: Button = $PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan10/VBox/HBox/BtnPlan10

# Phase 3 Summary Nodes
@onready var lbl_summary_title: Label = $PanelContainer/VBox/ScreenPhase3Summary/LblTitle
@onready var lbl_summary_country: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblCountryVal
@onready var lbl_summary_capital: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblCapitalVal
@onready var lbl_summary_leader: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblLeaderVal
@onready var lbl_summary_doctrine: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblDoctrineVal
@onready var lbl_summary_archetypes: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblArchetypesVal
@onready var lbl_summary_plans: Label = $PanelContainer/VBox/ScreenPhase3Summary/Grid/LblPlansVal
@onready var btn_verify_phase3_reload: Button = $PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnVerifyPhase3Reload
@onready var btn_change_selections: Button = $PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnChangeSelections
@onready var btn_proceed_phase4: Button = $PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnProceedPhase4

# Phase 4 Advisers Nodes
@onready var advisers_panel: Control = $PanelContainer/VBox/ScreenAdvisers
@onready var lbl_advisers_title: Label = $PanelContainer/VBox/ScreenAdvisers/LblTitle
@onready var lbl_general_guidance: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardGeneralAdviser/VBox/LblGuidance
@onready var lbl_general_recs: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardGeneralAdviser/VBox/LblRecommendations
@onready var lbl_press_headline: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardPressSecretary/VBox/LblHeadline
@onready var lbl_press_summary: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardPressSecretary/VBox/LblSummary
@onready var lbl_special_gems: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/HBoxSpecialHeader/LblGems
@onready var btn_unlock_slot2: Button = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/HBox/BtnUnlockSlot2
@onready var lbl_slot2_title: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/HBox/Title
@onready var lbl_slot2_desc: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/Desc
@onready var btn_unlock_slot3: Button = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/HBox/BtnUnlockSlot3
@onready var lbl_slot3_title: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/HBox/Title
@onready var lbl_slot3_desc: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/Desc
@onready var btn_unlock_slot4: Button = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/HBox/BtnUnlockSlot4
@onready var lbl_slot4_title: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/HBox/Title
@onready var lbl_slot4_desc: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/Desc
@onready var btn_unlock_slot5: Button = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/HBox/BtnUnlockSlot5
@onready var lbl_slot5_title: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/HBox/Title
@onready var lbl_slot5_desc: Label = $PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/Desc
@onready var lbl_advisers_feedback: Label = $PanelContainer/VBox/ScreenAdvisers/LblAdvisersFeedback
@onready var btn_back_to_summary: Button = $PanelContainer/VBox/ScreenAdvisers/HBoxAdvisersActions/BtnBackToSummary
@onready var btn_confirm_advisers: Button = $PanelContainer/VBox/ScreenAdvisers/HBoxAdvisersActions/BtnConfirmAdvisers

# Phase 5 Campaign HQ Nodes
@onready var campaign_panel: Control = $PanelContainer/VBox/ScreenCampaignHq
@onready var lbl_campaign_title: Label = $PanelContainer/VBox/ScreenCampaignHq/LblTitle
@onready var lbl_campaign_purse: Label = $PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblPurse
@onready var lbl_campaign_polling: Label = $PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblPolling
@onready var lbl_campaign_audit: Label = $PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblAudit
@onready var lbl_campaign_countdown: Label = $PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblCountdown
@onready var btn_filter_all: Button = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterAll
@onready var btn_filter_political: Button = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterPolitical
@onready var btn_filter_press: Button = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterPress
@onready var btn_start_election: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnStartElection
@onready var btn_conclude_early: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnConcludeEarly

# Race Polling Bar
@onready var lbl_projected_turnout: Label = $PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/HBoxHdr/LblProjectedTurnout
@onready var lbl_race_player: Label = $PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRacePlayer
@onready var lbl_race_thorne: Label = $PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceThorne
@onready var lbl_race_drake: Label = $PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceDrake
@onready var lbl_race_undecided: Label = $PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceUndecided

# Tabs
@onready var btn_tab_decisions: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabDecisions
@onready var btn_tab_groups: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabGroups
@onready var btn_tab_opponents: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabOpponents
@onready var btn_tab_election: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabElection

@onready var tab_decisions_container: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer
@onready var tab_groups_container: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabInterestGroupsContainer
@onready var tab_opponents_container: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer
@onready var tab_election_container: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer

# Decisions Tab
@onready var lbl_team_general: Label = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblGeneralAdvice
@onready var lbl_team_press: Label = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblPressAdvice
@onready var lbl_team_accountant: Label = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblAccountantAdvice
@onready var decisions_box: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/ScrollDecisions/DecisionsBox

# Groups Tab
@onready var groups_box: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabInterestGroupsContainer/ScrollGroups/GroupsBox

# Opponents & News Tab
@onready var lbl_thorne_desc: Label = $PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardOpponents/VBox/HBoxOpponents/CardThorne/VBoxThorne/LblThorneDesc
@onready var lbl_drake_desc: Label = $PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardOpponents/VBox/HBoxOpponents/CardDrake/VBoxDrake/LblDrakeDesc
@onready var news_box: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardNewsWire/VBox/ScrollNews/NewsBox

# Election Tab
@onready var lbl_election_tally: Label = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/LblElectionTally
@onready var btn_contest_election: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnContestElection
@onready var lbl_election_result: Label = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/LblElectionResult
@onready var election_breakdown_box: VBoxContainer = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/ElectionBreakdownBox
@onready var card_executive_address: PanelContainer = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress
@onready var input_official_address: LineEdit = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressInput/InputOfficialAddress
@onready var btn_title_last_name: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleLastName
@onready var btn_title_first_name: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleFirstName
@onready var btn_title_full_name: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleFullName
@onready var btn_title_madam: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleMadam
@onready var btn_title_mister: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleMister
@onready var btn_assume_office: Button = $PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/BtnAssumeOffice

@onready var lbl_campaign_feedback: Label = $PanelContainer/VBox/ScreenCampaignHq/LblCampaignFeedback
@onready var btn_back_to_advisers: Button = $PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignActions/BtnBackToAdvisers

func _ready() -> void:
	_init_controls()
	_connect_signals()
	if Engine.has_meta("launch_to_campaign_hq") and Engine.get_meta("launch_to_campaign_hq"):
		Engine.remove_meta("launch_to_campaign_hq")
		var session := _check_authoritative_session()
		_populate_campaign_hq_view(session)
		_show_stage(ScreenStage.CAMPAIGN_HQ)
		return
	_load_initial_stage()

func _init_controls() -> void:
	if register_panel == null: register_panel = get_node_or_null("PanelContainer/VBox/ScreenRegister")
	if verify_panel == null: verify_panel = get_node_or_null("PanelContainer/VBox/ScreenVerify")
	if name_panel == null: name_panel = get_node_or_null("PanelContainer/VBox/ScreenCountryName")
	if generating_panel == null: generating_panel = get_node_or_null("PanelContainer/VBox/ScreenGenerating")
	if confirmed_panel == null: confirmed_panel = get_node_or_null("PanelContainer/VBox/ScreenConfirmed")
	if leadership_panel == null: leadership_panel = get_node_or_null("PanelContainer/VBox/ScreenLeadership")
	if plans_panel == null: plans_panel = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans")
	if summary_panel == null: summary_panel = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary")

	if btn_mode_new_republic == null: btn_mode_new_republic = get_node_or_null("PanelContainer/VBox/ScreenRegister/SessionModeHBox/BtnModeNewRepublic")
	if btn_mode_resume == null: btn_mode_resume = get_node_or_null("PanelContainer/VBox/ScreenRegister/SessionModeHBox/BtnModeResumeSession")
	if form_register == null: form_register = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form")
	if login_form == null: login_form = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm")
	if lbl_session_details == null: lbl_session_details = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/LblSessionDetails")
	if lbl_session_phase == null: lbl_session_phase = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/LblSessionPhase")
	if btn_quick_login == null: btn_quick_login = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/SavedSessionCard/SessionCardVBox/BtnQuickLogin")
	if input_login_email == null: input_login_email = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/ManualLoginVBox/LoginEmailInput")
	if btn_submit_login == null: btn_submit_login = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/ManualLoginVBox/BtnSubmitLogin")
	if lbl_login_feedback == null: lbl_login_feedback = get_node_or_null("PanelContainer/VBox/ScreenRegister/LoginForm/LblLoginFeedback")
	if btn_campaign_signout == null: btn_campaign_signout = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignActions/BtnCampaignSignOut")
	if input_first_name == null: input_first_name = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/FirstNameInput")
	if input_last_name == null: input_last_name = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/LastNameInput")
	if input_username == null: input_username = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/UsernameInput")
	if input_email == null: input_email = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/EmailInput")
	if spin_dob_year == null: spin_dob_year = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/YearSpin")
	if spin_dob_month == null: spin_dob_month = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/MonthSpin")
	if spin_dob_day == null: spin_dob_day = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/DaySpin")
	if btn_gender_female == null: btn_gender_female = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxGender/BtnGenderFemale")
	if btn_gender_male == null: btn_gender_male = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxGender/BtnGenderMale")
	if btn_register == null: btn_register = get_node_or_null("PanelContainer/VBox/ScreenRegister/BtnRegister")
	if lbl_register_feedback == null: lbl_register_feedback = get_node_or_null("PanelContainer/VBox/ScreenRegister/LblFeedback")

	if lbl_email_sender == null: lbl_email_sender = get_node_or_null("PanelContainer/VBox/ScreenVerify/CardEmail/LblSender")
	if lbl_email_recipient == null: lbl_email_recipient = get_node_or_null("PanelContainer/VBox/ScreenVerify/CardEmail/LblRecipient")
	if lbl_email_subject == null: lbl_email_subject = get_node_or_null("PanelContainer/VBox/ScreenVerify/CardEmail/LblSubject")
	if lbl_email_body == null: lbl_email_body = get_node_or_null("PanelContainer/VBox/ScreenVerify/CardEmail/LblBody")
	if btn_verify_email_action == null: btn_verify_email_action = get_node_or_null("PanelContainer/VBox/ScreenVerify/CardEmail/BtnVerifyEmailAction")
	if input_token == null: input_token = get_node_or_null("PanelContainer/VBox/ScreenVerify/ManualVerify/TokenInput")
	if btn_submit_code == null: btn_submit_code = get_node_or_null("PanelContainer/VBox/ScreenVerify/ManualVerify/BtnSubmitCode")
	if btn_resend_email == null: btn_resend_email = get_node_or_null("PanelContainer/VBox/ScreenVerify/BtnResendEmail")
	if lbl_verify_feedback == null: lbl_verify_feedback = get_node_or_null("PanelContainer/VBox/ScreenVerify/LblFeedback")

	if input_country_name == null: input_country_name = get_node_or_null("PanelContainer/VBox/ScreenCountryName/InputCountryName")
	if btn_review_founding == null: btn_review_founding = get_node_or_null("PanelContainer/VBox/ScreenCountryName/BtnReviewFounding")
	if lbl_name_feedback == null: lbl_name_feedback = get_node_or_null("PanelContainer/VBox/ScreenCountryName/LblFeedback")
	if confirmation_modal == null: confirmation_modal = get_node_or_null("PanelContainer/VBox/ScreenCountryName/ConfirmationModal")
	if lbl_modal_country == null: lbl_modal_country = get_node_or_null("PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/LblCountryPreview")
	if btn_confirm_founding == null: btn_confirm_founding = get_node_or_null("PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/HBox/BtnConfirm")
	if btn_cancel_modal == null: btn_cancel_modal = get_node_or_null("PanelContainer/VBox/ScreenCountryName/ConfirmationModal/Box/HBox/BtnCancel")
	if flag_preview == null: flag_preview = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagPreviewContainer/FlagPreview")
	if btn_random_flag == null: btn_random_flag = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagCreatorHeader/BtnRandomFlag")
	if opt_flag_pattern == null: opt_flag_pattern = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptPattern")
	if opt_flag_primary == null: opt_flag_primary = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptPrimary")
	if opt_flag_secondary == null: opt_flag_secondary = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptSecondary")
	if opt_flag_emblem == null: opt_flag_emblem = get_node_or_null("PanelContainer/VBox/ScreenCountryName/FlagCreatorPanel/FlagCreatorVBox/FlagBodyHBox/FlagControlsGrid/OptEmblem")
	_init_flag_options()

	if lbl_generating_status == null: lbl_generating_status = get_node_or_null("PanelContainer/VBox/ScreenGenerating/LblStatus")
	if progress_bar == null: progress_bar = get_node_or_null("PanelContainer/VBox/ScreenGenerating/ProgressBar")

	if lbl_confirmed_title == null: lbl_confirmed_title = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/LblTitle")
	if lbl_stat_country == null: lbl_stat_country = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblCountryVal")
	if lbl_stat_capital == null: lbl_stat_capital = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblCapitalVal")
	if lbl_stat_population == null: lbl_stat_population = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblPopVal")
	if lbl_stat_treasury == null: lbl_stat_treasury = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblTreasuryVal")
	if lbl_stat_approval == null: lbl_stat_approval = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblApprovalVal")
	if lbl_stat_stability == null: lbl_stat_stability = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblStabilityVal")
	if lbl_stat_leader == null: lbl_stat_leader = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblLeaderVal")
	if lbl_stat_constitution == null: lbl_stat_constitution = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblConstitutionVal")
	if lbl_stat_resource == null: lbl_stat_resource = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/Grid/LblResourceVal")
	if btn_resume_session == null: btn_resume_session = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/BtnResumeSession")
	if btn_proceed_leadership == null: btn_proceed_leadership = get_node_or_null("PanelContainer/VBox/ScreenConfirmed/BtnProceedLeadership")

	# Leadership Nodes
	if lbl_leader_count == null: lbl_leader_count = get_node_or_null("PanelContainer/VBox/ScreenLeadership/LblLeaderCount")
	if btn_random_leadership == null: btn_random_leadership = get_node_or_null("PanelContainer/VBox/ScreenLeadership/BtnRandomLeadership")
	if lbl_doctrine_preview == null: lbl_doctrine_preview = get_node_or_null("PanelContainer/VBox/ScreenLeadership/LblDoctrinePreview")
	if btn_confirm_leadership == null: btn_confirm_leadership = get_node_or_null("PanelContainer/VBox/ScreenLeadership/BtnConfirmLeadership")
	if lbl_leadership_feedback == null: lbl_leadership_feedback = get_node_or_null("PanelContainer/VBox/ScreenLeadership/LblFeedback")

	if btn_arch_technocrat == null: btn_arch_technocrat = get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardTechnocrat/VBox/HBoxHeader/BtnSelectTechnocrat")
	if btn_arch_populist == null: btn_arch_populist = get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardPopulist/VBox/HBoxHeader/BtnSelectPopulist")
	if btn_arch_visionary == null: btn_arch_visionary = get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardVisionary/VBox/HBoxHeader/BtnSelectVisionary")
	if btn_arch_statist == null: btn_arch_statist = get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardStatist/VBox/HBoxHeader/BtnSelectStatist")
	if btn_arch_diplomat == null: btn_arch_diplomat = get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardDiplomat/VBox/HBoxHeader/BtnSelectDiplomat")

	# Development Plans Nodes
	if lbl_plans_count == null: lbl_plans_count = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/LblPlansCount")
	if btn_random_plans == null: btn_random_plans = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/BtnRandomPlans")
	if btn_confirm_plans == null: btn_confirm_plans = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/BtnConfirmDevelopmentPlans")
	if lbl_plans_feedback == null: lbl_plans_feedback = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/LblFeedback")

	if btn_plan_1 == null: btn_plan_1 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan1/VBox/HBox/BtnPlan1")
	if btn_plan_2 == null: btn_plan_2 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan2/VBox/HBox/BtnPlan2")
	if btn_plan_3 == null: btn_plan_3 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan3/VBox/HBox/BtnPlan3")
	if btn_plan_4 == null: btn_plan_4 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan4/VBox/HBox/BtnPlan4")
	if btn_plan_5 == null: btn_plan_5 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan5/VBox/HBox/BtnPlan5")
	if btn_plan_6 == null: btn_plan_6 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan6/VBox/HBox/BtnPlan6")
	if btn_plan_7 == null: btn_plan_7 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan7/VBox/HBox/BtnPlan7")
	if btn_plan_8 == null: btn_plan_8 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan8/VBox/HBox/BtnPlan8")
	if btn_plan_9 == null: btn_plan_9 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan9/VBox/HBox/BtnPlan9")
	if btn_plan_10 == null: btn_plan_10 = get_node_or_null("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan10/VBox/HBox/BtnPlan10")

	# Summary Nodes
	if lbl_summary_title == null: lbl_summary_title = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/LblTitle")
	if lbl_summary_country == null: lbl_summary_country = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblCountryVal")
	if lbl_summary_capital == null: lbl_summary_capital = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblCapitalVal")
	if lbl_summary_leader == null: lbl_summary_leader = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblLeaderVal")
	if lbl_summary_doctrine == null: lbl_summary_doctrine = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblDoctrineVal")
	if lbl_summary_archetypes == null: lbl_summary_archetypes = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblArchetypesVal")
	if lbl_summary_plans == null: lbl_summary_plans = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/Grid/LblPlansVal")
	if btn_verify_phase3_reload == null: btn_verify_phase3_reload = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnVerifyPhase3Reload")
	if btn_change_selections == null: btn_change_selections = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnChangeSelections")
	if btn_proceed_phase4 == null: btn_proceed_phase4 = get_node_or_null("PanelContainer/VBox/ScreenPhase3Summary/HBoxActions/BtnProceedPhase4")
	if advisers_panel == null: advisers_panel = get_node_or_null("PanelContainer/VBox/ScreenAdvisers")
	if lbl_advisers_title == null: lbl_advisers_title = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/LblTitle")
	if lbl_general_guidance == null: lbl_general_guidance = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardGeneralAdviser/VBox/LblGuidance")
	if lbl_general_recs == null: lbl_general_recs = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardGeneralAdviser/VBox/LblRecommendations")
	if lbl_press_headline == null: lbl_press_headline = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardPressSecretary/VBox/LblHeadline")
	if lbl_press_summary == null: lbl_press_summary = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardPressSecretary/VBox/LblSummary")
	if lbl_special_gems == null: lbl_special_gems = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/HBoxSpecialHeader/LblGems")
	if btn_unlock_slot2 == null: btn_unlock_slot2 = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/HBox/BtnUnlockSlot2")
	if lbl_slot2_title == null: lbl_slot2_title = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/HBox/Title")
	if lbl_slot2_desc == null: lbl_slot2_desc = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot2/VBox/Desc")
	if btn_unlock_slot3 == null: btn_unlock_slot3 = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/HBox/BtnUnlockSlot3")
	if lbl_slot3_title == null: lbl_slot3_title = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/HBox/Title")
	if lbl_slot3_desc == null: lbl_slot3_desc = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot3/VBox/Desc")
	if btn_unlock_slot4 == null: btn_unlock_slot4 = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/HBox/BtnUnlockSlot4")
	if lbl_slot4_title == null: lbl_slot4_title = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/HBox/Title")
	if lbl_slot4_desc == null: lbl_slot4_desc = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot4/VBox/Desc")
	if btn_unlock_slot5 == null: btn_unlock_slot5 = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/HBox/BtnUnlockSlot5")
	if lbl_slot5_title == null: lbl_slot5_title = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/HBox/Title")
	if lbl_slot5_desc == null: lbl_slot5_desc = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/ScrollAdvisers/AdvisersBox/CardSlot5/VBox/Desc")
	if lbl_advisers_feedback == null: lbl_advisers_feedback = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/LblAdvisersFeedback")
	if btn_back_to_summary == null: btn_back_to_summary = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/HBoxAdvisersActions/BtnBackToSummary")
	if btn_confirm_advisers == null: btn_confirm_advisers = get_node_or_null("PanelContainer/VBox/ScreenAdvisers/HBoxAdvisersActions/BtnConfirmAdvisers")
	if campaign_panel == null: campaign_panel = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq")
	if lbl_campaign_title == null: lbl_campaign_title = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/LblTitle")
	if lbl_campaign_purse == null: lbl_campaign_purse = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblPurse")
	if lbl_campaign_polling == null: lbl_campaign_polling = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblPolling")
	if lbl_campaign_audit == null: lbl_campaign_audit = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblAudit")
	if lbl_campaign_countdown == null: lbl_campaign_countdown = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxStats/LblCountdown")
	if btn_filter_all == null: btn_filter_all = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterAll")
	if btn_filter_political == null: btn_filter_political = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterPolitical")
	if btn_filter_press == null: btn_filter_press = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/HBoxAdviserFilter/BtnFilterPress")
	if btn_start_election == null: btn_start_election = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnStartElection")
	if btn_conclude_early == null: btn_conclude_early = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnConcludeEarly")

	if lbl_projected_turnout == null: lbl_projected_turnout = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/HBoxHdr/LblProjectedTurnout")
	if lbl_race_player == null: lbl_race_player = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRacePlayer")
	if lbl_race_thorne == null: lbl_race_thorne = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceThorne")
	if lbl_race_drake == null: lbl_race_drake = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceDrake")
	if lbl_race_undecided == null: lbl_race_undecided = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardRaceOverview/VBox/GridRace/LblRaceUndecided")

	if btn_tab_decisions == null: btn_tab_decisions = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabDecisions")
	if btn_tab_groups == null: btn_tab_groups = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabGroups")
	if btn_tab_opponents == null: btn_tab_opponents = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabOpponents")
	if btn_tab_election == null: btn_tab_election = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignTabs/BtnTabElection")

	if tab_decisions_container == null: tab_decisions_container = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer")
	if tab_groups_container == null: tab_groups_container = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabInterestGroupsContainer")
	if tab_opponents_container == null: tab_opponents_container = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer")
	if tab_election_container == null: tab_election_container = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer")

	if lbl_team_general == null: lbl_team_general = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblGeneralAdvice")
	if lbl_team_general == null: lbl_team_general = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardTeam/VBox/LblGeneralAdvice")
	if lbl_team_press == null: lbl_team_press = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblPressAdvice")
	if lbl_team_press == null: lbl_team_press = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardTeam/VBox/LblPressAdvice")
	if lbl_team_accountant == null: lbl_team_accountant = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/CardTeam/VBox/LblAccountantAdvice")
	if lbl_team_accountant == null: lbl_team_accountant = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardTeam/VBox/LblAccountantAdvice")
	if decisions_box == null: decisions_box = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabDecisionsContainer/ScrollDecisions/DecisionsBox")
	if decisions_box == null: decisions_box = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/ScrollDecisions/DecisionsBox")

	if groups_box == null: groups_box = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabInterestGroupsContainer/ScrollGroups/GroupsBox")
	if lbl_thorne_desc == null: lbl_thorne_desc = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardOpponents/VBox/HBoxOpponents/CardThorne/VBoxThorne/LblThorneDesc")
	if lbl_drake_desc == null: lbl_drake_desc = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardOpponents/VBox/HBoxOpponents/CardDrake/VBoxDrake/LblDrakeDesc")
	if news_box == null: news_box = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabOpponentsNewsContainer/CardNewsWire/VBox/ScrollNews/NewsBox")

	if lbl_election_tally == null: lbl_election_tally = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/LblElectionTally")
	if lbl_election_tally == null: lbl_election_tally = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardElectionNight/VBox/HBoxElection/LblElectionTally")
	if btn_contest_election == null: btn_contest_election = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/HBoxElection/BtnContestElection")
	if btn_contest_election == null: btn_contest_election = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardElectionNight/VBox/HBoxElection/BtnContestElection")
	if lbl_election_result == null: lbl_election_result = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/LblElectionResult")
	if lbl_election_result == null: lbl_election_result = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardElectionNight/VBox/LblElectionResult")
	if election_breakdown_box == null: election_breakdown_box = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/ElectionBreakdownBox")
	if card_executive_address == null: card_executive_address = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress")
	if input_official_address == null: input_official_address = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressInput/InputOfficialAddress")
	if btn_title_last_name == null: btn_title_last_name = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleLastName")
	if btn_title_first_name == null: btn_title_first_name = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleFirstName")
	if btn_title_full_name == null: btn_title_full_name = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleFullName")
	if btn_title_madam == null: btn_title_madam = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleMadam")
	if btn_title_mister == null: btn_title_mister = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/CardExecutiveAddress/AddressVBox/HBoxAddressPresets/BtnTitleMister")
	if btn_assume_office == null: btn_assume_office = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/TabElectionContainer/CardElectionNight/VBox/BtnAssumeOffice")
	if btn_assume_office == null: btn_assume_office = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/CardElectionNight/VBox/BtnAssumeOffice")
	if lbl_campaign_feedback == null: lbl_campaign_feedback = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/LblCampaignFeedback")
	if btn_back_to_advisers == null: btn_back_to_advisers = get_node_or_null("PanelContainer/VBox/ScreenCampaignHq/HBoxCampaignActions/BtnBackToAdvisers")
	_connect_signals()

func _process(delta: float) -> void:
	if resend_cooldown_timer > 0.0:
		resend_cooldown_timer = max(0.0, resend_cooldown_timer - delta)
		if btn_resend_email:
			if resend_cooldown_timer > 0.0:
				btn_resend_email.text = "Resend Verification Email (%ds)" % [ceil(resend_cooldown_timer)]
				btn_resend_email.disabled = true
			else:
				btn_resend_email.text = "Resend Verification Email"
				btn_resend_email.disabled = false

	if current_stage == ScreenStage.CAMPAIGN_HQ and campaign_timer_active:
		if campaign_time_remaining > 0.0:
			campaign_time_remaining = max(0.0, campaign_time_remaining - delta)
			_update_campaign_timer_display()
		else:
			campaign_time_remaining = 0.0
			campaign_timer_active = false
			_on_campaign_timer_completed()

func _connect_signals() -> void:
	if btn_mode_new_republic and not btn_mode_new_republic.pressed.is_connected(_on_entry_new_republic_pressed):
		btn_mode_new_republic.pressed.connect(_on_entry_new_republic_pressed)
	if btn_mode_resume and not btn_mode_resume.pressed.is_connected(_on_entry_resume_pressed):
		btn_mode_resume.pressed.connect(_on_entry_resume_pressed)
	if btn_quick_login and not btn_quick_login.pressed.is_connected(_on_quick_login_pressed):
		btn_quick_login.pressed.connect(_on_quick_login_pressed)
	if btn_submit_login and not btn_submit_login.pressed.is_connected(_on_submit_login_pressed):
		btn_submit_login.pressed.connect(_on_submit_login_pressed)
	if btn_filter_all and not btn_filter_all.pressed.is_connected(_on_filter_all_pressed):
		btn_filter_all.pressed.connect(_on_filter_all_pressed)
	if btn_filter_political and not btn_filter_political.pressed.is_connected(_on_filter_political_pressed):
		btn_filter_political.pressed.connect(_on_filter_political_pressed)
	if btn_filter_press and not btn_filter_press.pressed.is_connected(_on_filter_press_pressed):
		btn_filter_press.pressed.connect(_on_filter_press_pressed)
	if btn_start_election and not btn_start_election.pressed.is_connected(_on_contest_election_pressed):
		btn_start_election.pressed.connect(_on_contest_election_pressed)
	if btn_conclude_early and not btn_conclude_early.pressed.is_connected(_on_conclude_early_pressed):
		btn_conclude_early.pressed.connect(_on_conclude_early_pressed)

	if btn_campaign_signout and not btn_campaign_signout.pressed.is_connected(_on_campaign_signout_pressed):
		btn_campaign_signout.pressed.connect(_on_campaign_signout_pressed)
	if btn_gender_female and not btn_gender_female.pressed.is_connected(_on_gender_female_pressed):
		btn_gender_female.pressed.connect(_on_gender_female_pressed)
	if btn_gender_male and not btn_gender_male.pressed.is_connected(_on_gender_male_pressed):
		btn_gender_male.pressed.connect(_on_gender_male_pressed)
	if btn_register and not btn_register.pressed.is_connected(_on_register_pressed):
		btn_register.pressed.connect(_on_register_pressed)
	if btn_verify_email_action and not btn_verify_email_action.pressed.is_connected(_on_verify_action_pressed):
		btn_verify_email_action.pressed.connect(_on_verify_action_pressed)
	if btn_submit_code and not btn_submit_code.pressed.is_connected(_on_submit_code_pressed):
		btn_submit_code.pressed.connect(_on_submit_code_pressed)
	if btn_resend_email and not btn_resend_email.pressed.is_connected(_on_resend_email_pressed):
		btn_resend_email.pressed.connect(_on_resend_email_pressed)
	if btn_review_founding and not btn_review_founding.pressed.is_connected(_on_review_founding_pressed):
		btn_review_founding.pressed.connect(_on_review_founding_pressed)
	if btn_confirm_founding and not btn_confirm_founding.pressed.is_connected(_on_confirm_founding_pressed):
		btn_confirm_founding.pressed.connect(_on_confirm_founding_pressed)
	if btn_random_flag and not btn_random_flag.pressed.is_connected(_on_random_flag_pressed):
		btn_random_flag.pressed.connect(_on_random_flag_pressed)
	if opt_flag_pattern and not opt_flag_pattern.item_selected.is_connected(_on_flag_pattern_selected):
		opt_flag_pattern.item_selected.connect(_on_flag_pattern_selected)
	if opt_flag_primary and not opt_flag_primary.item_selected.is_connected(_on_flag_primary_selected):
		opt_flag_primary.item_selected.connect(_on_flag_primary_selected)
	if opt_flag_secondary and not opt_flag_secondary.item_selected.is_connected(_on_flag_secondary_selected):
		opt_flag_secondary.item_selected.connect(_on_flag_secondary_selected)
	if opt_flag_emblem and not opt_flag_emblem.item_selected.is_connected(_on_flag_emblem_selected):
		opt_flag_emblem.item_selected.connect(_on_flag_emblem_selected)
	if btn_cancel_modal and not btn_cancel_modal.pressed.is_connected(_on_cancel_modal_pressed):
		btn_cancel_modal.pressed.connect(_on_cancel_modal_pressed)
	if btn_resume_session and not btn_resume_session.pressed.is_connected(_on_resume_session_pressed):
		btn_resume_session.pressed.connect(_on_resume_session_pressed)
	if btn_change_selections and not btn_change_selections.pressed.is_connected(_on_change_selections_pressed):
		btn_change_selections.pressed.connect(_on_change_selections_pressed)
	if btn_proceed_phase4 and not btn_proceed_phase4.pressed.is_connected(_on_proceed_phase4_pressed):
		btn_proceed_phase4.pressed.connect(_on_proceed_phase4_pressed)
	if btn_back_to_summary and not btn_back_to_summary.pressed.is_connected(_on_back_to_summary_pressed):
		btn_back_to_summary.pressed.connect(_on_back_to_summary_pressed)
	if btn_confirm_advisers and not btn_confirm_advisers.pressed.is_connected(_on_confirm_advisers_pressed):
		btn_confirm_advisers.pressed.connect(_on_confirm_advisers_pressed)
	if btn_unlock_slot2 and not btn_unlock_slot2.pressed.is_connected(_on_unlock_slot2_pressed):
		btn_unlock_slot2.pressed.connect(_on_unlock_slot2_pressed)
	if btn_unlock_slot3 and not btn_unlock_slot3.pressed.is_connected(_on_unlock_slot3_pressed):
		btn_unlock_slot3.pressed.connect(_on_unlock_slot3_pressed)
	if btn_unlock_slot4 and not btn_unlock_slot4.pressed.is_connected(_on_unlock_slot4_pressed):
		btn_unlock_slot4.pressed.connect(_on_unlock_slot4_pressed)
	if btn_unlock_slot5 and not btn_unlock_slot5.pressed.is_connected(_on_unlock_slot5_pressed):
		btn_unlock_slot5.pressed.connect(_on_unlock_slot5_pressed)
	if btn_tab_decisions and not btn_tab_decisions.pressed.is_connected(_select_campaign_tab.bind("decisions")):
		btn_tab_decisions.pressed.connect(_select_campaign_tab.bind("decisions"))
	if btn_tab_groups and not btn_tab_groups.pressed.is_connected(_select_campaign_tab.bind("groups")):
		btn_tab_groups.pressed.connect(_select_campaign_tab.bind("groups"))
	if btn_tab_opponents and not btn_tab_opponents.pressed.is_connected(_select_campaign_tab.bind("opponents")):
		btn_tab_opponents.pressed.connect(_select_campaign_tab.bind("opponents"))
	if btn_tab_election and not btn_tab_election.pressed.is_connected(_select_campaign_tab.bind("election")):
		btn_tab_election.pressed.connect(_select_campaign_tab.bind("election"))

	if btn_back_to_advisers and not btn_back_to_advisers.pressed.is_connected(_on_back_to_advisers_pressed):
		btn_back_to_advisers.pressed.connect(_on_back_to_advisers_pressed)
	if btn_contest_election and not btn_contest_election.pressed.is_connected(_on_contest_election_pressed):
		btn_contest_election.pressed.connect(_on_contest_election_pressed)
	if btn_assume_office and not btn_assume_office.pressed.is_connected(_on_assume_office_pressed):
		btn_assume_office.pressed.connect(_on_assume_office_pressed)
	if btn_proceed_leadership and not btn_proceed_leadership.pressed.is_connected(_on_proceed_leadership_pressed):
		btn_proceed_leadership.pressed.connect(_on_proceed_leadership_pressed)

	if btn_random_leadership and not btn_random_leadership.pressed.is_connected(_on_random_leadership_pressed):
		btn_random_leadership.pressed.connect(_on_random_leadership_pressed)
	if btn_random_plans and not btn_random_plans.pressed.is_connected(_on_random_plans_pressed):
		btn_random_plans.pressed.connect(_on_random_plans_pressed)

	if btn_title_last_name and not btn_title_last_name.pressed.is_connected(_on_title_last_name_pressed):
		btn_title_last_name.pressed.connect(_on_title_last_name_pressed)
	if btn_title_first_name and not btn_title_first_name.pressed.is_connected(_on_title_first_name_pressed):
		btn_title_first_name.pressed.connect(_on_title_first_name_pressed)
	if btn_title_full_name and not btn_title_full_name.pressed.is_connected(_on_title_full_name_pressed):
		btn_title_full_name.pressed.connect(_on_title_full_name_pressed)
	if btn_title_madam and not btn_title_madam.pressed.is_connected(_on_title_madam_pressed):
		btn_title_madam.pressed.connect(_on_title_madam_pressed)
	if btn_title_mister and not btn_title_mister.pressed.is_connected(_on_title_mister_pressed):
		btn_title_mister.pressed.connect(_on_title_mister_pressed)

	# Direct card clicking for Leadership
	_connect_card_click("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardTechnocrat", _on_technocrat_toggled)
	_connect_card_click("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardPopulist", _on_populist_toggled)
	_connect_card_click("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardVisionary", _on_visionary_toggled)
	_connect_card_click("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardStatist", _on_statist_toggled)
	_connect_card_click("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardDiplomat", _on_diplomat_toggled)

	# Direct card clicking for Development Plans
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan1", _on_plan_clicked.bind("industrialisation_first", btn_plan_1))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan2", _on_plan_clicked.bind("agricultural_transformation", btn_plan_2))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan3", _on_plan_clicked.bind("technology_and_innovation", btn_plan_3))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan4", _on_plan_clicked.bind("infrastructure_revolution", btn_plan_4))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan5", _on_plan_clicked.bind("human_capital_development", btn_plan_5))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan6", _on_plan_clicked.bind("financial_and_commercial_power", btn_plan_6))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan7", _on_plan_clicked.bind("energy_and_natural_resources", btn_plan_7))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan8", _on_plan_clicked.bind("national_security_and_defence", btn_plan_8))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan9", _on_plan_clicked.bind("sustainable_development", btn_plan_9))
	_connect_card_click("PanelContainer/VBox/ScreenDevelopmentPlans/ScrollPlans/PlansBox/CardPlan10", _on_plan_clicked.bind("global_trade_and_international_influence", btn_plan_10))

	# Leadership buttons
	if btn_arch_technocrat and not btn_arch_technocrat.pressed.is_connected(_on_technocrat_toggled):
		btn_arch_technocrat.pressed.connect(_on_technocrat_toggled)
	if btn_arch_populist and not btn_arch_populist.pressed.is_connected(_on_populist_toggled):
		btn_arch_populist.pressed.connect(_on_populist_toggled)
	if btn_arch_visionary and not btn_arch_visionary.pressed.is_connected(_on_visionary_toggled):
		btn_arch_visionary.pressed.connect(_on_visionary_toggled)
	if btn_arch_statist and not btn_arch_statist.pressed.is_connected(_on_statist_toggled):
		btn_arch_statist.pressed.connect(_on_statist_toggled)
	if btn_arch_diplomat and not btn_arch_diplomat.pressed.is_connected(_on_diplomat_toggled):
		btn_arch_diplomat.pressed.connect(_on_diplomat_toggled)
	if btn_confirm_leadership and not btn_confirm_leadership.pressed.is_connected(_on_confirm_leadership_pressed):
		btn_confirm_leadership.pressed.connect(_on_confirm_leadership_pressed)

	# Development Plan buttons
	_connect_plan_button(btn_plan_1, "industrialisation_first")
	_connect_plan_button(btn_plan_2, "agricultural_transformation")
	_connect_plan_button(btn_plan_3, "technology_and_innovation")
	_connect_plan_button(btn_plan_4, "infrastructure_revolution")
	_connect_plan_button(btn_plan_5, "human_capital_development")
	_connect_plan_button(btn_plan_6, "financial_and_commercial_power")
	_connect_plan_button(btn_plan_7, "energy_and_natural_resources")
	_connect_plan_button(btn_plan_8, "national_security_and_defence")
	_connect_plan_button(btn_plan_9, "sustainable_development")
	_connect_plan_button(btn_plan_10, "global_trade_and_international_influence")
	if btn_confirm_plans and not btn_confirm_plans.pressed.is_connected(_on_confirm_plans_pressed):
		btn_confirm_plans.pressed.connect(_on_confirm_plans_pressed)

	# Summary buttons
	if btn_verify_phase3_reload and not btn_verify_phase3_reload.pressed.is_connected(_on_resume_session_pressed):
		btn_verify_phase3_reload.pressed.connect(_on_resume_session_pressed)

func _connect_card_click(node_path: String, callback: Callable) -> void:
	var card = get_node_or_null(node_path)
	if card and not card.gui_input.is_connected(_on_card_gui_input.bind(callback)):
		card.gui_input.connect(_on_card_gui_input.bind(callback))

func _on_card_gui_input(event: InputEvent, callback: Callable) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		callback.call()

func _connect_plan_button(btn: Button, plan_key: String) -> void:
	if btn and not btn.pressed.is_connected(_on_plan_clicked.bind(plan_key, btn)):
		btn.pressed.connect(_on_plan_clicked.bind(plan_key, btn))

func _load_initial_stage() -> void:
	# Initial start-of-game gateway: Always present Sign Up or Log In interface
	_show_stage(ScreenStage.REGISTER)
	_show_sign_up_mode()
	_refresh_detected_session()

func _show_stage(stage: ScreenStage) -> void:
	current_stage = stage
	if register_panel: register_panel.visible = (stage == ScreenStage.REGISTER)
	if verify_panel: verify_panel.visible = (stage == ScreenStage.VERIFY_EMAIL)
	if name_panel:
		name_panel.visible = (stage == ScreenStage.COUNTRY_NAME)
		if confirmation_modal: confirmation_modal.visible = false
	if generating_panel: generating_panel.visible = (stage == ScreenStage.GENERATING)
	if confirmed_panel: confirmed_panel.visible = (stage == ScreenStage.CONFIRMED)
	if leadership_panel: leadership_panel.visible = (stage == ScreenStage.LEADERSHIP)
	if plans_panel: plans_panel.visible = (stage == ScreenStage.DEVELOPMENT_PLANS)
	if summary_panel: summary_panel.visible = (stage == ScreenStage.PHASE3_SUMMARY)
	if advisers_panel: advisers_panel.visible = (stage == ScreenStage.ADVISERS)
	if campaign_panel:
		campaign_panel.visible = (stage == ScreenStage.CAMPAIGN_HQ)
		if stage == ScreenStage.CAMPAIGN_HQ:
			if not active_campaign_data.get("election_resolved", false) and campaign_time_remaining > 0.0:
				campaign_timer_active = true
			_update_campaign_timer_display()

	var stage_name := "Register"
	match stage:
		ScreenStage.REGISTER: stage_name = "Register"
		ScreenStage.VERIFY_EMAIL: stage_name = "VerifyEmail"
		ScreenStage.COUNTRY_NAME: stage_name = "CountryName"
		ScreenStage.GENERATING: stage_name = "Generating"
		ScreenStage.CONFIRMED: stage_name = "Confirmed"
		ScreenStage.LEADERSHIP: stage_name = "Leadership"
		ScreenStage.DEVELOPMENT_PLANS: stage_name = "DevelopmentPlans"
		ScreenStage.PHASE3_SUMMARY: stage_name = "Phase3Summary"
		ScreenStage.ADVISERS: stage_name = "Advisers"
		ScreenStage.CAMPAIGN_HQ: stage_name = "CampaignHeadquarters"

	onboarding_step_changed.emit(stage_name)

# ==============================================================================
# Entry Navigation: Sign Up vs Log In / Sign In
# ==============================================================================
func _on_entry_new_republic_pressed() -> void:
	_show_sign_up_mode()

func _on_entry_resume_pressed() -> void:
	_show_log_in_mode()

func _show_sign_up_mode() -> void:
	if form_register: form_register.visible = true
	if btn_register: btn_register.visible = true
	if login_form: login_form.visible = false
	if lbl_register_feedback:
		lbl_register_feedback.text = ""
	_update_session_mode_buttons(true)

func _show_log_in_mode() -> void:
	if form_register: form_register.visible = false
	if btn_register: btn_register.visible = false
	if login_form: login_form.visible = true
	if lbl_login_feedback:
		lbl_login_feedback.text = ""
	_update_session_mode_buttons(false)
	_refresh_detected_session()

func _refresh_detected_session() -> Dictionary:
	var session_data := _check_authoritative_session()
	if not session_data.is_empty():
		var c_name: String = str(session_data.get("country_name", "Republic of Sovereign Nations"))
		var leader: String = str(session_data.get("president_name", session_data.get("leader_name", session_data.get("registered_name", "Executive Leader"))))
		var step: String = str(session_data.get("onboarding_step", "InOffice"))
		
		if lbl_session_details:
			lbl_session_details.text = c_name + " (Leader: " + leader + ")"
		
		var phase_text := "Phase: In Office (Presidential Desk)"
		var phase_color := Color(0.4, 1.0, 0.5)
		var btn_text := "▶ Enter Executive Office"
		if step == "CampaignHeadquarters" or step == "AdvisersIntroduced":
			phase_text = "Phase: Campaign Headquarters (Presidential Campaign)"
			phase_color = Color(1.0, 0.85, 0.3)
			btn_text = "▶ Enter Campaign Headquarters"
		elif step == "InOffice" or session_data.has("treasury_balance"):
			phase_text = "Phase: In Office (Presidential Desk)"
			phase_color = Color(0.4, 1.0, 0.5)
			btn_text = "▶ Enter Executive Office"
		else:
			phase_text = "Phase: Sovereign Founding (" + step + ")"
			phase_color = Color(0.6, 0.8, 1.0)
			btn_text = "▶ Resume Republic Setup"
		
		if lbl_session_phase:
			lbl_session_phase.text = phase_text
			lbl_session_phase.modulate = phase_color
		if btn_quick_login:
			btn_quick_login.text = btn_text
			btn_quick_login.disabled = false
		return session_data
	else:
		if lbl_session_details:
			lbl_session_details.text = "No saved Republic found on this system."
		if lbl_session_phase:
			lbl_session_phase.text = "Phase: None (Select 'Sign Up' to establish a Republic)"
			lbl_session_phase.modulate = Color(0.7, 0.7, 0.7)
		if btn_quick_login:
			btn_quick_login.text = "▶ No Saved Republic"
			btn_quick_login.disabled = true
		return {}

func _on_quick_login_pressed() -> void:
	var session_data := _refresh_detected_session()
	if session_data.is_empty():
		if lbl_login_feedback:
			lbl_login_feedback.text = "No saved Republic found. Please Sign Up to create one."
			lbl_login_feedback.modulate = Color(1.0, 0.5, 0.3)
		return
	if lbl_login_feedback:
		lbl_login_feedback.text = "Session verified. Launching straight to office..."
		lbl_login_feedback.modulate = Color(0.4, 1.0, 0.4)
	_launch_to_office_or_resume(session_data)

func _on_submit_login_pressed() -> void:
	var email := input_login_email.text.strip_edges() if input_login_email else ""
	if email.is_empty():
		if lbl_login_feedback:
			lbl_login_feedback.text = "Please enter your registered email address."
			lbl_login_feedback.modulate = Color(1.0, 0.4, 0.4)
		return
	if not email.contains("@") or not email.contains("."):
		if lbl_login_feedback:
			lbl_login_feedback.text = "Please enter a valid email address (e.g. candidate@republic.gov)."
			lbl_login_feedback.modulate = Color(1.0, 0.4, 0.4)
		return
	
	var session_data := _refresh_detected_session()
	if session_data.is_empty():
		if lbl_login_feedback:
			lbl_login_feedback.text = "No Republic found. Please select 'Sign Up' above to register candidate."
			lbl_login_feedback.modulate = Color(1.0, 0.7, 0.2)
		return
	
	registered_email = email
	if lbl_login_feedback:
		lbl_login_feedback.text = "Login successful for " + email + ". Launching to office..."
		lbl_login_feedback.modulate = Color(0.4, 1.0, 0.4)
	_launch_to_office_or_resume(session_data)

func _launch_to_office_or_resume(session_data: Dictionary) -> void:
	var step: String = str(session_data.get("onboarding_step", ""))
	
	# Check if player is in office
	if step == "InOffice" or (session_data.has("country_name") and (step.is_empty() or step == "InOffice") and session_data.has("treasury_balance")):
		print("[AUTH/LAUNCH]: Player is in office. Launching straight to Executive Office.")
		if is_inside_tree() and get_tree() != null:
			get_tree().change_scene_to_file("res://ExecutiveOffice.tscn")
		return
	
	# Check if player is in campaign phase
	if step == "CampaignHeadquarters" or step == "AdvisersIntroduced":
		print("[AUTH/LAUNCH]: Player is in campaign. Launching straight to Campaign Headquarters.")
		_populate_campaign_hq_view(session_data)
		_show_stage(ScreenStage.CAMPAIGN_HQ)
		return
	
	# In-progress founding journey
	match step:
		"DevelopmentPlansSelected":
			_populate_phase3_summary_view(session_data)
			_show_stage(ScreenStage.PHASE3_SUMMARY)
		"LeadershipSelected":
			_show_stage(ScreenStage.DEVELOPMENT_PLANS)
		"CountryCreated":
			_populate_confirmed_view(session_data)
			_show_stage(ScreenStage.CONFIRMED)
		"EmailVerified":
			_show_stage(ScreenStage.COUNTRY_NAME)
		"AccountRegistered":
			_show_stage(ScreenStage.VERIFY_EMAIL)
		_:
			_show_stage(ScreenStage.REGISTER)

func _on_campaign_signout_pressed() -> void:
	print("[AUTH]: Signing out from Campaign Headquarters to Start Gateway (Auth.tscn).")
	if is_inside_tree() and get_tree() != null:
		get_tree().change_scene_to_file("res://Auth.tscn")
	else:
		_show_stage(ScreenStage.REGISTER)
		_show_sign_up_mode()

func _check_authoritative_session() -> Dictionary:
	var paths := [
		active_state_path,
		"user://presidential_desk_state.json",
		"res://presidential_desk_state.json",
		active_save_path
	]
	for p in paths:
		var global_p := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(global_p):
			var file := FileAccess.open(global_p, FileAccess.READ)
			if file != null:
				var content := file.get_as_text()
				file.close()
				var parsed = JSON.parse_string(content)
				if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
					var c_name: String = str(parsed.get("country_name", ""))
					if not c_name.is_empty() and c_name != "[No State Loaded]":
						return parsed
	return {}

func _update_session_mode_buttons(is_new_mode: bool) -> void:
	if btn_mode_new_republic:
		btn_mode_new_republic.modulate = Color(1.0, 1.0, 1.0) if is_new_mode else Color(0.65, 0.65, 0.7)
	if btn_mode_resume:
		btn_mode_resume.modulate = Color(1.0, 1.0, 1.0) if not is_new_mode else Color(0.65, 0.65, 0.7)

# ==============================================================================
# Step 1: Account Registration
# ==============================================================================

func _on_gender_female_pressed() -> void:
	selected_gender = "Female"
	selected_title = "Madam President"
	selected_avatar = "👩‍💼"
	_update_gender_button_styles()

func _on_gender_male_pressed() -> void:
	selected_gender = "Male"
	selected_title = "Mr. President"
	selected_avatar = "👨‍💼"
	_update_gender_button_styles()

func _update_gender_button_styles() -> void:
	if btn_gender_female:
		if selected_gender == "Female":
			btn_gender_female.modulate = Color(1.0, 0.9, 0.4)
		else:
			btn_gender_female.modulate = Color(0.7, 0.7, 0.7)
	if btn_gender_male:
		if selected_gender == "Male":
			btn_gender_male.modulate = Color(1.0, 0.9, 0.4)
		else:
			btn_gender_male.modulate = Color(0.7, 0.7, 0.7)

func _on_register_pressed() -> void:
	if is_processing: return
	if lbl_register_feedback: lbl_register_feedback.text = ""

	var first_name := input_first_name.text.strip_edges() if input_first_name else ""
	var last_name := input_last_name.text.strip_edges() if input_last_name else ""
	var username := input_username.text.strip_edges() if input_username else ""
	var email := input_email.text.strip_edges() if input_email else ""
	var year := int(spin_dob_year.value) if spin_dob_year else 1990
	var month := int(spin_dob_month.value) if spin_dob_month else 1
	var day := int(spin_dob_day.value) if spin_dob_day else 1

	if first_name.is_empty():
		_display_register_error("First name is required.")
		return
	if last_name.is_empty():
		_display_register_error("Last name is required.")
		return

	# Handle username
	if username.is_empty():
		if not first_name.is_empty() and not last_name.is_empty():
			username = (first_name.substr(0, 1) + "_" + last_name).to_lower()
			if input_username: input_username.text = username
		else:
			_display_register_error("Username is required.")
			return

	if username.length() < 3:
		_display_register_error("Username must be at least 3 characters.")
		return

	# Default email and date of birth if not set via hidden form
	if email.is_empty():
		email = "%s@republic.net" % username.to_lower()
		if input_email: input_email.text = email

	if year <= 1900 or year > 2014:
		year = 1990
		if spin_dob_year: spin_dob_year.value = 1990

	var dob_str := "%04d-%02d-%02d" % [year, month, day]

	if selected_gender.is_empty():
		selected_gender = "Female"
	if selected_title.is_empty():
		selected_title = "President " + last_name
	if selected_avatar.is_empty():
		selected_avatar = "👩‍💼"

	registered_username = username
	registered_first_name = first_name
	registered_last_name = last_name
	registered_email = email
	registered_dob = dob_str

	is_processing = true
	if btn_register: btn_register.disabled = true
	if lbl_register_feedback: lbl_register_feedback.text = "Registering account with Sovereign Electoral Registry..."

	var req := {
		"action": "register_account",
		"first_name": first_name,
		"last_name": last_name,
		"username": username,
		"email": email,
		"gender": selected_gender,
		"title": selected_title,
		"avatar": selected_avatar,
		"date_of_birth": dob_str,
		"dob": dob_str,
		"request_id": "onboarding_reg_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false
	if btn_register: btn_register.disabled = false

	if res.get("success", false) and res.get("result", "") == "Success":
		registered_first_name = first_name
		registered_last_name = last_name
		registered_email = email
		registered_dob = dob_str
		verification_token = str(res.get("verification_token", "")).strip_edges()
		verification_email_data = {
			"sender": str(res.get("verification_email_sender", "Republic Sovereign Electoral Commission <registry@republic.net>")),
			"recipient": str(res.get("verification_email_recipient", email)),
			"subject": str(res.get("verification_email_subject", "Official Verification: Confirm Your Republic Sovereign Account")),
			"body": str(res.get("verification_email_body", "Your 6-digit verification security code is: " + verification_token)),
			"button_text": str(res.get("verification_button_text", "Verify Email"))
		}
		_populate_email_view()
		_show_stage(ScreenStage.VERIFY_EMAIL)
	else:
		_display_register_error(str(res.get("message", "Registration failed. Please check inputs.")))

func _display_register_error(msg: String) -> void:
	if lbl_register_feedback:
		lbl_register_feedback.text = msg
		lbl_register_feedback.modulate = Color(1.0, 0.4, 0.4)

# ==============================================================================
# Step 2: Email Verification
# ==============================================================================
func _populate_email_view() -> void:
	if lbl_email_sender: lbl_email_sender.text = "From: " + str(verification_email_data.get("sender", ""))
	if lbl_email_recipient: lbl_email_recipient.text = "To: " + str(verification_email_data.get("recipient", registered_email))
	if lbl_email_subject: lbl_email_subject.text = "Subject: " + str(verification_email_data.get("subject", ""))
	if lbl_email_body: lbl_email_body.text = str(verification_email_data.get("body", ""))
	if btn_verify_email_action: btn_verify_email_action.text = str(verification_email_data.get("button_text", "Verify Email"))

func _on_verify_action_pressed() -> void:
	if verification_token.is_empty():
		_display_verify_error("Verification token is missing. Please request a new code.")
		return
	_execute_verification(verification_token)

func _on_submit_code_pressed() -> void:
	var code := input_token.text.strip_edges() if input_token else ""
	if code.length() != 6:
		_display_verify_error("Verification token must be exactly 6 characters.")
		return
	_execute_verification(code)

func _execute_verification(code: String) -> void:
	if is_processing: return
	is_processing = true
	if btn_verify_email_action: btn_verify_email_action.disabled = true
	if btn_submit_code: btn_submit_code.disabled = true
	if lbl_verify_feedback: lbl_verify_feedback.text = "Verifying security token..."

	var req := {
		"action": "verify_email",
		"token": code,
		"request_id": "onboarding_ver_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false
	if btn_verify_email_action: btn_verify_email_action.disabled = false
	if btn_submit_code: btn_submit_code.disabled = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if lbl_verify_feedback:
			lbl_verify_feedback.text = "Email verified successfully! Proceeding to nation founding..."
			lbl_verify_feedback.modulate = Color(0.4, 1.0, 0.4)
		_show_stage(ScreenStage.COUNTRY_NAME)
	else:
		_display_verify_error(str(res.get("message", "Verification token invalid or expired.")))

func _on_resend_email_pressed() -> void:
	if is_processing or resend_cooldown_timer > 0.0: return
	is_processing = true
	if lbl_verify_feedback: lbl_verify_feedback.text = "Requesting new verification code..."

	var req := {
		"action": "resend_verification_email",
		"request_id": "onboarding_resend_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		verification_token = str(res.get("verification_token", "")).strip_edges()
		verification_email_data["body"] = str(res.get("verification_email_body", ""))
		_populate_email_view()
		resend_cooldown_timer = 10.0
		if lbl_verify_feedback:
			lbl_verify_feedback.text = "New verification email dispatched! Please check your inbox."
			lbl_verify_feedback.modulate = Color(0.4, 1.0, 0.4)
	else:
		_display_verify_error(str(res.get("message", "Could not resend verification email.")))

func _display_verify_error(msg: String) -> void:
	if lbl_verify_feedback:
		lbl_verify_feedback.text = msg
		lbl_verify_feedback.modulate = Color(1.0, 0.4, 0.4)

# ==============================================================================
# Step 3: Country Naming & Confirmation
# ==============================================================================
func _init_flag_options() -> void:
	if opt_flag_pattern and opt_flag_pattern.item_count == 0:
		opt_flag_pattern.add_item("Horizontal Tricolor", 0)
		opt_flag_pattern.add_item("Vertical Tricolor", 1)
		opt_flag_pattern.add_item("Bicolor Split", 2)
		opt_flag_pattern.add_item("Sovereign Cross", 3)
		opt_flag_pattern.add_item("Canton Star", 4)
		opt_flag_pattern.add_item("Solid with Crest", 5)
		opt_flag_pattern.select(0)

	var color_names := [
		"Sovereign Navy", "Crimson Red", "Emerald Green", "Imperial Gold",
		"Ivory White", "Obsidian Slate", "Sky Blue", "Saffron Amber", "Royal Amethyst"
	]

	if opt_flag_primary and opt_flag_primary.item_count == 0:
		for i in range(color_names.size()):
			opt_flag_primary.add_item(color_names[i], i)
		opt_flag_primary.select(0)

	if opt_flag_secondary and opt_flag_secondary.item_count == 0:
		for i in range(color_names.size()):
			opt_flag_secondary.add_item(color_names[i], i)
		opt_flag_secondary.select(4) # White

	if opt_flag_emblem and opt_flag_emblem.item_count == 0:
		opt_flag_emblem.add_item("⭐ Sovereign Star", 0)
		opt_flag_emblem.add_item("🦅 Imperial Eagle", 1)
		opt_flag_emblem.add_item("☀️ Radiant Sun", 2)
		opt_flag_emblem.add_item("🏛️ Civic Capitol", 3)
		opt_flag_emblem.add_item("🛡️ Heraldic Shield", 4)
		opt_flag_emblem.add_item("⚔️ Crossed Sabres", 5)
		opt_flag_emblem.add_item("🌿 Laurel Wreath", 6)
		opt_flag_emblem.add_item("None (Clean)", 7)
		opt_flag_emblem.select(0)

func _on_random_flag_pressed() -> void:
	if flag_preview and flag_preview.has_method("randomize_flag"):
		flag_preview.randomize_flag()
		_sync_flag_options_from_preview()

func _sync_flag_options_from_preview() -> void:
	if not flag_preview: return
	var p = flag_preview.pattern
	var pat_map := {
		"horizontal_tricolor": 0, "vertical_tricolor": 1, "bicolor_horizontal": 2,
		"sovereign_cross": 3, "canton_star": 4, "solid_emblem": 5
	}
	if opt_flag_pattern and pat_map.has(p):
		opt_flag_pattern.select(pat_map[p])

	var emb = flag_preview.emblem
	var emb_map := {
		"star": 0, "eagle": 1, "sun": 2, "temple": 3, "shield": 4, "sabres": 5, "wreath": 6, "none": 7
	}
	if opt_flag_emblem and emb_map.has(emb):
		opt_flag_emblem.select(emb_map[emb])

func _on_flag_pattern_selected(index: int) -> void:
	if not flag_preview: return
	var patterns := ["horizontal_tricolor", "vertical_tricolor", "bicolor_horizontal", "sovereign_cross", "canton_star", "solid_emblem"]
	if index >= 0 and index < patterns.size():
		flag_preview.pattern = patterns[index]

func _on_flag_primary_selected(index: int) -> void:
	if not flag_preview or not flag_preview.get("PALETTE"): return
	var pal = flag_preview.PALETTE
	if index >= 0 and index < pal.size():
		flag_preview.primary_color = pal[index]

func _on_flag_secondary_selected(index: int) -> void:
	if not flag_preview or not flag_preview.get("PALETTE"): return
	var pal = flag_preview.PALETTE
	if index >= 0 and index < pal.size():
		flag_preview.secondary_color = pal[index]

func _on_flag_emblem_selected(index: int) -> void:
	if not flag_preview: return
	var emblems := ["star", "eagle", "sun", "temple", "shield", "sabres", "wreath", "none"]
	if index >= 0 and index < emblems.size():
		flag_preview.emblem = emblems[index]

func _on_review_founding_pressed() -> void:
	var name_text := input_country_name.text.strip_edges() if input_country_name else ""
	if name_text.length() < 4:
		_display_name_error("Country name must be at least 4 characters.")
		return
	if not name_text.containsn("Republic"):
		_display_name_error("Country name must contain the whole word 'Republic' (e.g. 'Republic of Azuria').")
		return

	if lbl_name_feedback: lbl_name_feedback.text = ""
	if lbl_modal_country: lbl_modal_country.text = "Nation Name: " + name_text
	if confirmation_modal: confirmation_modal.visible = true

func _on_cancel_modal_pressed() -> void:
	if confirmation_modal: confirmation_modal.visible = false

func _on_confirm_founding_pressed() -> void:
	if is_processing: return
	var name_text := input_country_name.text.strip_edges() if input_country_name else ""
	if confirmation_modal: confirmation_modal.visible = false

	_show_stage(ScreenStage.GENERATING)
	_run_procedural_generation_flow(name_text)

func _display_name_error(msg: String) -> void:
	if lbl_name_feedback:
		lbl_name_feedback.text = msg
		lbl_name_feedback.modulate = Color(1.0, 0.4, 0.4)

# ==============================================================================
# Step 4: Procedural Generation & Standardized Metrics
# ==============================================================================
func _run_procedural_generation_flow(country_name: String) -> void:
	is_processing = true
	if lbl_generating_status: lbl_generating_status.text = "Ratifying sovereign charter with Global Registry..."
	if progress_bar: progress_bar.value = 25.0

	var flag_dict: Dictionary = {}
	if flag_preview and flag_preview.has_method("to_flag_data"):
		flag_dict = flag_preview.to_flag_data()
	else:
		flag_dict = {
			"pattern": "horizontal_tricolor",
			"primary_color": "142e61",
			"secondary_color": "f5f7fa",
			"accent_color": "dfb847",
			"emblem": "star"
		}

	var req := {
		"action": "create_country",
		"country_name": country_name,
		"flag_data": flag_dict,
		"request_id": "onboarding_create_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if progress_bar: progress_bar.value = 100.0
		created_country_data = res
		_populate_confirmed_view(res)
		_show_stage(ScreenStage.CONFIRMED)
		onboarding_completed.emit(res)
	else:
		_show_stage(ScreenStage.COUNTRY_NAME)
		_display_name_error(str(res.get("message", "Country founding failed. Duplicate or invalid name.")))

# ==============================================================================
# Step 5: Country Creation Confirmed & Navigation to Phase 3
# ==============================================================================
func _populate_confirmed_view(data: Dictionary) -> void:
	if lbl_confirmed_title:
		lbl_confirmed_title.text = "SOVEREIGN NATION ESTABLISHED: " + str(data.get("country_name", ""))
	if lbl_stat_country: lbl_stat_country.text = str(data.get("country_name", ""))
	if lbl_stat_capital: lbl_stat_capital.text = str(data.get("capital_city", "Solaria Prime"))
	if lbl_stat_population: lbl_stat_population.text = "10,000,000 citizens"
	if lbl_stat_treasury: lbl_stat_treasury.text = "R 50,000,000 REPU"
	if lbl_stat_approval: lbl_stat_approval.text = "60.0%"
	if lbl_stat_stability: lbl_stat_stability.text = "70.0%"
	if lbl_stat_leader: lbl_stat_leader.text = str(data.get("player_display_name", registered_first_name + " " + registered_last_name))
	if lbl_stat_constitution: lbl_stat_constitution.text = str(data.get("constitution_name", "National Democratic Constitution"))
	if lbl_stat_resource: lbl_stat_resource.text = str(data.get("primary_resource", "Strategic Minerals"))

func _on_proceed_leadership_pressed() -> void:
	_show_stage(ScreenStage.LEADERSHIP)

# ==============================================================================
# Step 6: Leadership Identity Selection (Phase 3 Part 1)
# ==============================================================================
func _on_technocrat_toggled() -> void:
	_toggle_archetype("technocrat", btn_arch_technocrat)

func _on_populist_toggled() -> void:
	_toggle_archetype("peoples_champion", btn_arch_populist)

func _on_visionary_toggled() -> void:
	_toggle_archetype("visionary", btn_arch_visionary)

func _on_statist_toggled() -> void:
	_toggle_archetype("strategist", btn_arch_statist)

func _on_diplomat_toggled() -> void:
	_toggle_archetype("diplomat", btn_arch_diplomat)

func _toggle_archetype(key: String, btn: Button) -> void:
	if lbl_leadership_feedback: lbl_leadership_feedback.text = ""

	if selected_archetypes.has(key):
		selected_archetypes.erase(key)
		if btn:
			btn.text = "Select"
			btn.modulate = Color(1.0, 1.0, 1.0)
	else:
		if selected_archetypes.size() >= 2:
			if lbl_leadership_feedback:
				lbl_leadership_feedback.text = "You may only select exactly 2 leadership archetypes. Deselect one first."
				lbl_leadership_feedback.modulate = Color(1.0, 0.4, 0.4)
			return
		selected_archetypes.append(key)
		if btn:
			btn.text = "Selected ✓"
			btn.modulate = Color(0.4, 1.0, 0.4)

	var count := selected_archetypes.size()
	if lbl_leader_count:
		lbl_leader_count.text = "Selected: %d / 2%s" % [count, " (Ready to Confirm!)" if count == 2 else " (Select 2 to continue)"]
		lbl_leader_count.modulate = Color(0.4, 1.0, 0.4) if count == 2 else Color(0.9, 0.8, 0.4)

	var doc_title := _compute_doctrine_title(selected_archetypes)
	if lbl_doctrine_preview:
		lbl_doctrine_preview.text = "Executive Doctrine: " + doc_title

	if btn_confirm_leadership:
		btn_confirm_leadership.disabled = (count != 2)

func _compute_doctrine_title(archetypes: Array[String]) -> String:
	if archetypes.size() != 2:
		return "None (Select 2 archetypes)"

	var set := {}
	for a in archetypes:
		var norm := a.to_lower().replace("-", "_")
		if norm.contains("visionary") or norm.contains("economic"): set["visionary"] = true
		elif norm.contains("diplomat"): set["diplomat"] = true
		elif norm.contains("strategist") or norm.contains("statist") or norm.contains("builder"): set["strategist"] = true
		elif norm.contains("champion") or norm.contains("people") or norm.contains("populist") or norm.contains("reformer"): set["champion"] = true
		elif norm.contains("technocrat"): set["technocrat"] = true

	if set.has("technocrat") and set.has("visionary"): return "Technocratic Visionary"
	if set.has("technocrat") and set.has("strategist"): return "Technocratic Strategist"
	if set.has("technocrat") and set.has("diplomat"): return "Technocratic Diplomat"
	if set.has("technocrat") and set.has("champion"): return "Technocratic Reformer"
	if set.has("visionary") and set.has("strategist"): return "Visionary Strategist"
	if set.has("visionary") and set.has("diplomat"): return "Visionary Diplomat"
	if set.has("visionary") and set.has("champion"): return "Visionary Champion"
	if set.has("diplomat") and set.has("strategist"): return "Strategic Diplomat"
	if set.has("diplomat") and set.has("champion"): return "Diplomatic Champion"
	if set.has("strategist") and set.has("champion"): return "Strategic Champion"

	return archetypes[0].capitalize() + "-" + archetypes[1].capitalize()

func _on_confirm_leadership_pressed() -> void:
	if is_processing or selected_archetypes.size() != 2: return
	is_processing = true
	if btn_confirm_leadership: btn_confirm_leadership.disabled = true
	if lbl_leadership_feedback: lbl_leadership_feedback.text = "Ratifying executive leadership doctrine..."

	var req := {
		"action": "select_leadership",
		"archetypes": selected_archetypes,
		"request_id": "onboarding_lead_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false
	if btn_confirm_leadership: btn_confirm_leadership.disabled = false

	if res.get("success", false) and res.get("result", "") == "Success":
		selected_doctrine_title = str(res.get("leadership_doctrine_title", _compute_doctrine_title(selected_archetypes)))
		if lbl_leadership_feedback:
			lbl_leadership_feedback.text = "Leadership doctrine established! Proceeding to national development agenda..."
			lbl_leadership_feedback.modulate = Color(0.4, 1.0, 0.4)
		_show_stage(ScreenStage.DEVELOPMENT_PLANS)
	else:
		if lbl_leadership_feedback:
			lbl_leadership_feedback.text = str(res.get("message", "Failed to ratify leadership doctrine."))
			lbl_leadership_feedback.modulate = Color(1.0, 0.4, 0.4)

# ==============================================================================
# Step 7: National Development Agenda (Phase 3 Part 2)
# ==============================================================================
func _on_plan_clicked(plan_key: String, btn: Button) -> void:
	if lbl_plans_feedback: lbl_plans_feedback.text = ""

	if selected_plans.has(plan_key):
		selected_plans.erase(plan_key)
		if btn:
			btn.text = "Select"
			btn.modulate = Color(1.0, 1.0, 1.0)
	else:
		if selected_plans.size() >= 4:
			if lbl_plans_feedback:
				lbl_plans_feedback.text = "You may only select exactly 4 priorities. Deselect one first."
				lbl_plans_feedback.modulate = Color(1.0, 0.4, 0.4)
			return
		selected_plans.append(plan_key)
		if btn:
			btn.text = "Selected ✓"
			btn.modulate = Color(0.4, 1.0, 0.4)

	var count := selected_plans.size()
	if lbl_plans_count:
		lbl_plans_count.text = "Selected: %d / 4%s" % [count, " (Ready to Ratify!)" if count == 4 else " (Select 4 to continue)"]
		lbl_plans_count.modulate = Color(0.4, 1.0, 0.4) if count == 4 else Color(0.9, 0.8, 0.4)

	if btn_confirm_plans:
		btn_confirm_plans.disabled = (count != 4)

func _on_confirm_plans_pressed() -> void:
	if is_processing or selected_plans.size() != 4: return
	is_processing = true
	if btn_confirm_plans: btn_confirm_plans.disabled = true
	if lbl_plans_feedback: lbl_plans_feedback.text = "Ratifying inaugural national development agenda..."

	var req := {
		"action": "select_development_plans",
		"plans": selected_plans,
		"request_id": "onboarding_plans_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false
	if btn_confirm_plans: btn_confirm_plans.disabled = false

	if res.get("success", false) and res.get("result", "") == "Success":
		_populate_phase3_summary_view(res)
		_show_stage(ScreenStage.PHASE3_SUMMARY)
	else:
		if lbl_plans_feedback:
			lbl_plans_feedback.text = str(res.get("message", "Failed to ratify national development agenda."))
			lbl_plans_feedback.modulate = Color(1.0, 0.4, 0.4)

# ==============================================================================
# Step 8: Phase 3 Completion Summary
# ==============================================================================
func _populate_phase3_summary_view(data: Dictionary) -> void:
	var c_name := str(data.get("country_name", ""))
	if c_name.is_empty(): c_name = str(created_country_data.get("country_name", "Republic of Solaria"))

	var leader := str(data.get("player_display_name", ""))
	if leader.is_empty(): leader = registered_first_name + " " + registered_last_name
	if leader.strip_edges().is_empty(): leader = "Elena Okafor"

	var doctrine := str(data.get("leadership_doctrine_title", selected_doctrine_title))
	if doctrine.is_empty(): doctrine = "Technocratic Builder"

	var archetypes_raw = data.get("selected_leadership_archetypes", selected_archetypes)
	var archetypes_str := ""
	if typeof(archetypes_raw) == TYPE_ARRAY:
		var items: Array = archetypes_raw
		archetypes_str = ", ".join(items)
	if archetypes_str.is_empty(): archetypes_str = "Technocrat, Statist Builder"

	var plans_raw = data.get("selected_development_plan_names", [])
	var plans_str := ""
	if typeof(plans_raw) == TYPE_ARRAY and plans_raw.size() > 0:
		var p_items: Array = plans_raw
		plans_str = ", ".join(p_items)
	else:
		var p_keys = data.get("selected_development_plans", selected_plans)
		if typeof(p_keys) == TYPE_ARRAY:
			var p_keys_arr: Array = p_keys
			plans_str = ", ".join(p_keys_arr)
	if plans_str.is_empty(): plans_str = "Agrarian Self-Sufficiency, Renewable Power Grid, Higher Education, Digital Cyber"

	if lbl_summary_title: lbl_summary_title.text = "PHASE 3 COMPLETE: " + c_name
	if lbl_summary_country: lbl_summary_country.text = c_name
	if lbl_summary_capital: lbl_summary_capital.text = str(data.get("capital_city", "Solaria Prime"))
	if lbl_summary_leader: lbl_summary_leader.text = leader
	if lbl_summary_doctrine: lbl_summary_doctrine.text = doctrine
	if lbl_summary_archetypes: lbl_summary_archetypes.text = archetypes_str
	if lbl_summary_plans: lbl_summary_plans.text = plans_str

func _on_change_selections_pressed() -> void:
	# Allows the player to adjust choices prior to confirming to next stage
	_show_stage(ScreenStage.LEADERSHIP)

func _on_resume_session_pressed() -> void:
	# Reloads state from disk to confirm persistent survival across restart
	var file := FileAccess.open(ProjectSettings.globalize_path(active_state_path), FileAccess.READ)
	if file != null:
		var text := file.get_as_text()
		file.close()
		var parsed = JSON.parse_string(text)
		if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
			_populate_confirmed_view(parsed)
			_populate_phase3_summary_view(parsed)
			print("[ONBOARDING]: Authoritative state successfully verified from disk.")

# ==============================================================================
# Backend IPC Request Helper
# ==============================================================================
func _submit_request_and_wait(req: Dictionary) -> Dictionary:
	var req_full := ProjectSettings.globalize_path(active_request_path)
	var res_full := ProjectSettings.globalize_path(active_result_path)

	# Clean stale result file
	if FileAccess.file_exists(res_full):
		DirAccess.remove_absolute(res_full)

	var f := FileAccess.open(req_full, FileAccess.WRITE)
	if f == null:
		return { "success": false, "result": "IOError", "message": "Failed to write request file." }
	f.store_string(JSON.stringify(req, "\t"))
	f.close()

	# Execute CLI bridge synchronously
	var dll_candidates := [
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Debug/net8.0/Republic.Cli.dll"),
		ProjectSettings.globalize_path("res://../../src/Republic.Cli/bin/Release/net8.0/Republic.Cli.dll")
	]

	var dll_path := ""
	for c in dll_candidates:
		if FileAccess.file_exists(c):
			dll_path = c
			break

	if dll_path.is_empty():
		return { "success": false, "result": "ConfigError", "message": "Republic.Cli.dll not found." }

	var cli_args: PackedStringArray = [
		dll_path,
		"--process-request",
		"--state-file=" + ProjectSettings.globalize_path(active_state_path),
		"--save-file=" + ProjectSettings.globalize_path(active_save_path),
		"--request-file=" + req_full
	]

	var output := []
	OS.execute("dotnet", cli_args, output, true)

	var rf := FileAccess.open(res_full, FileAccess.READ)
	if rf != null:
		var content := rf.get_as_text()
		rf.close()
		var parsed = JSON.parse_string(content)
		if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
			return parsed

	return { "success": false, "result": "Timeout", "message": "Simulation did not respond with result file." }


# ==============================================================================
# Step 9: Presidential Advisory Council (Phase 4)
# ==============================================================================
func _on_proceed_phase4_pressed() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(active_state_path), FileAccess.READ)
	if file != null:
		var txt := file.get_as_text()
		file.close()
		var parsed = JSON.parse_string(txt)
		if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
			_populate_advisers_view(parsed)
	_show_stage(ScreenStage.ADVISERS)

func _on_back_to_summary_pressed() -> void:
	_show_stage(ScreenStage.PHASE3_SUMMARY)

func _populate_advisers_view(data: Dictionary) -> void:
	active_advisers_data = data
	sovereign_gems_count = int(data.get("sovereign_gems", 500))
	if lbl_special_gems:
		lbl_special_gems.text = "Sovereign Gems: %d 💎" % sovereign_gems_count

	var gen_adv = data.get("general_adviser", {})
	if typeof(gen_adv) == TYPE_DICTIONARY:
		var guidance := str(gen_adv.get("strategic_guidance", ""))
		if guidance.is_empty():
			guidance = "As your General Adviser, I stand ready to assist your administration in balancing national priorities and preserving macroeconomic stability."
		if lbl_general_guidance: lbl_general_guidance.text = guidance

		var recs = gen_adv.get("recommendations", [])
		if typeof(recs) == TYPE_ARRAY and recs.size() > 0:
			var formatted_recs: Array[String] = []
			for r in recs:
				formatted_recs.append("• " + str(r))
			if lbl_general_recs: lbl_general_recs.text = "\n".join(formatted_recs)

	var press = data.get("press_secretary", {})
	if typeof(press) == TYPE_DICTIONARY:
		var headline := str(press.get("briefing_headline", "Administration Advances National Strategic Priorities"))
		var summary := str(press.get("briefing_summary", "Official press briefing confirming readiness for national governance."))
		if lbl_press_headline: lbl_press_headline.text = headline
		if lbl_press_summary: lbl_press_summary.text = summary

	var slots = data.get("special_advisers", [])
	if typeof(slots) == TYPE_ARRAY:
		for s in slots:
			if typeof(s) == TYPE_DICTIONARY:
				var s_num := int(s.get("slot_number", 0))
				var s_status := int(s.get("status", 1))
				var is_active := (s_status == 0)
				var portfolio := str(s.get("portfolio", ""))
				var adv_name := str(s.get("adviser_name", ""))
				var advice := str(s.get("advice", ""))
				var req_text := str(s.get("unlock_requirement", ""))

				match s_num:
					2:
						if is_active:
							if lbl_slot2_title: lbl_slot2_title.text = "%s — %s [ACTIVE]" % [adv_name, portfolio]
							if lbl_slot2_desc: lbl_slot2_desc.text = "Advice: %s" % advice
							if btn_unlock_slot2:
								btn_unlock_slot2.text = "Unlocked ✓"
								btn_unlock_slot2.disabled = true
						else:
							if lbl_slot2_title: lbl_slot2_title.text = "%s (Slot 2)" % portfolio
							if lbl_slot2_desc: lbl_slot2_desc.text = "Status: Vacant (Position Locked) • Requirement: %s" % req_text
							if btn_unlock_slot2:
								btn_unlock_slot2.text = "Unlock (500 Gems)"
								btn_unlock_slot2.disabled = (sovereign_gems_count < 500)
					3:
						if is_active:
							if lbl_slot3_title: lbl_slot3_title.text = "%s — %s [ACTIVE]" % [adv_name, portfolio]
							if lbl_slot3_desc: lbl_slot3_desc.text = "Advice: %s" % advice
							if btn_unlock_slot3:
								btn_unlock_slot3.text = "Unlocked ✓"
								btn_unlock_slot3.disabled = true
						else:
							if lbl_slot3_title: lbl_slot3_title.text = "%s (Slot 3)" % portfolio
							if lbl_slot3_desc: lbl_slot3_desc.text = "Status: Vacant (Position Locked) • Requirement: %s" % req_text
							if btn_unlock_slot3:
								btn_unlock_slot3.text = "Unlock (1,000 Gems)"
								btn_unlock_slot3.disabled = (sovereign_gems_count < 1000)
					4:
						if is_active:
							if lbl_slot4_title: lbl_slot4_title.text = "%s — %s [ACTIVE]" % [adv_name, portfolio]
							if lbl_slot4_desc: lbl_slot4_desc.text = "Advice: %s" % advice
							if btn_unlock_slot4:
								btn_unlock_slot4.text = "Unlocked ✓"
								btn_unlock_slot4.disabled = true
						else:
							if lbl_slot4_title: lbl_slot4_title.text = "%s (Slot 4)" % portfolio
							if lbl_slot4_desc: lbl_slot4_desc.text = "Status: Vacant (Position Locked) • Requirement: %s" % req_text
							if btn_unlock_slot4:
								btn_unlock_slot4.text = "Unlock (1,500 Gems)"
								btn_unlock_slot4.disabled = (sovereign_gems_count < 1500)
					5:
						if is_active:
							if lbl_slot5_title: lbl_slot5_title.text = "%s — %s [ACTIVE]" % [adv_name, portfolio]
							if lbl_slot5_desc: lbl_slot5_desc.text = "Advice: %s" % advice
							if btn_unlock_slot5:
								btn_unlock_slot5.text = "Unlocked ✓"
								btn_unlock_slot5.disabled = true
						else:
							if lbl_slot5_title: lbl_slot5_title.text = "%s (Slot 5)" % portfolio
							if lbl_slot5_desc: lbl_slot5_desc.text = "Status: Vacant (Position Locked) • Requirement: %s" % req_text
							if btn_unlock_slot5:
								btn_unlock_slot5.text = "Unlock (2,500 Gems)"
								btn_unlock_slot5.disabled = (sovereign_gems_count < 2500)

func _on_unlock_slot2_pressed() -> void: _unlock_slot(2)
func _on_unlock_slot3_pressed() -> void: _unlock_slot(3)
func _on_unlock_slot4_pressed() -> void: _unlock_slot(4)
func _on_unlock_slot5_pressed() -> void: _unlock_slot(5)

func _unlock_slot(slot_num: int) -> void:
	if is_processing: return
	is_processing = true
	if lbl_advisers_feedback: lbl_advisers_feedback.text = "Unlocking Special Adviser Slot %d..." % slot_num

	var req := {
		"action": "unlock_special_adviser",
		"slot_number": slot_num,
		"unlock_method": "gems",
		"request_id": "onboarding_unlock_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if lbl_advisers_feedback:
			lbl_advisers_feedback.text = str(res.get("message", "Slot unlocked successfully!"))
			lbl_advisers_feedback.modulate = Color(0.4, 1.0, 0.4)
		_populate_advisers_view(res)
	else:
		if lbl_advisers_feedback:
			lbl_advisers_feedback.text = str(res.get("message", "Failed to unlock Special Adviser slot."))
			lbl_advisers_feedback.modulate = Color(1.0, 0.4, 0.4)

func _on_confirm_advisers_pressed() -> void:
	if is_processing: return
	is_processing = true
	if btn_confirm_advisers: btn_confirm_advisers.disabled = true
	if lbl_advisers_feedback: lbl_advisers_feedback.text = "Inducting Presidential Advisory Council..."

	var req := {
		"action": "introduce_advisers",
		"request_id": "onboarding_adv_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false
	if btn_confirm_advisers: btn_confirm_advisers.disabled = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if lbl_advisers_feedback:
			lbl_advisers_feedback.text = "Presidential Advisory Council Inducted! Entering Campaign Headquarters..."
			lbl_advisers_feedback.modulate = Color(0.4, 1.0, 0.4)
		_enter_campaign_hq()
	else:
		if lbl_advisers_feedback:
			lbl_advisers_feedback.text = str(res.get("message", "Failed to induct Advisory Council."))
			lbl_advisers_feedback.modulate = Color(1.0, 0.4, 0.4)


# ==============================================================================
# Step 10: Presidential Campaign Headquarters & Election (Phase 5)
# ==============================================================================
func _on_back_to_advisers_pressed() -> void:
	_show_stage(ScreenStage.ADVISERS)

func _enter_campaign_hq() -> void:
	if is_processing: return
	is_processing = true
	if lbl_campaign_feedback: lbl_campaign_feedback.text = "Setting up Campaign Headquarters..."

	var req := {
		"action": "enter_campaign_hq",
		"request_id": "onboarding_hq_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		_populate_campaign_hq_view(res)
		_show_stage(ScreenStage.CAMPAIGN_HQ)
	else:
		# Fallback to local snapshot if present
		var file := FileAccess.open(ProjectSettings.globalize_path(active_state_path), FileAccess.READ)
		if file != null:
			var txt := file.get_as_text()
			file.close()
			var parsed = JSON.parse_string(txt)
			if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
				_populate_campaign_hq_view(parsed)
		_show_stage(ScreenStage.CAMPAIGN_HQ)

func _format_number(val: float) -> String:
	var s := String.num(val, 0)
	var res := ""
	var count := 0
	for i in range(s.length() - 1, -1, -1):
		res = s[i] + res
		count += 1
		if count % 3 == 0 and i > 0:
			res = "," + res
	return res

func _select_campaign_tab(tab_name: String) -> void:
	if tab_decisions_container: tab_decisions_container.visible = (tab_name == "decisions")
	if tab_groups_container: tab_groups_container.visible = (tab_name == "groups")
	if tab_opponents_container: tab_opponents_container.visible = (tab_name == "opponents")
	if tab_election_container: tab_election_container.visible = (tab_name == "election")

	var gold := Color(0.85, 0.72, 0.35)
	var dim := Color(0.8, 0.8, 0.8)
	if btn_tab_decisions: btn_tab_decisions.modulate = gold if tab_name == "decisions" else dim
	if btn_tab_groups: btn_tab_groups.modulate = gold if tab_name == "groups" else dim
	if btn_tab_opponents: btn_tab_opponents.modulate = gold if tab_name == "opponents" else dim
	if btn_tab_election: btn_tab_election.modulate = gold if tab_name == "election" else dim

func _populate_campaign_hq_view(data: Dictionary) -> void:
	active_campaign_data = data
	if data.has("campaign_time_remaining_seconds"):
		campaign_time_remaining = float(data.get("campaign_time_remaining_seconds", campaign_time_remaining))
	_update_campaign_timer_display()
	_update_adviser_filter_buttons()
	var purse := float(data.get("campaign_purse", 5000000.0))
	var polling := float(data.get("campaign_polling", 35.0))
	var accountant = data.get("campaign_accountant_general", {})

	if lbl_campaign_purse:
		var formatted := str(data.get("formatted_campaign_purse", ""))
		if not formatted.is_empty():
			lbl_campaign_purse.text = "Campaign Purse: %s" % [formatted]
		else:
			lbl_campaign_purse.text = "Campaign Purse: R %s REPU" % [_format_number(purse)]
	if lbl_campaign_polling:
		lbl_campaign_polling.text = "National Polling: %.1f%%" % [polling]

	if typeof(accountant) == TYPE_DICTIONARY:
		var acct_name := str(accountant.get("name", "Kaelen Thorne"))
		var acct_audit := str(accountant.get("audit_status", "Clean Audit"))
		if lbl_campaign_audit:
			lbl_campaign_audit.text = "Audit: %s (%s)" % [acct_audit, acct_name]
		if lbl_team_accountant:
			var adv := str(accountant.get("financial_advice", "Strict financial controls in place."))
			lbl_team_accountant.text = "• %s (Accountant General): %s" % [acct_name, adv]

	var gen_adv = data.get("general_adviser", {})
	if typeof(gen_adv) == TYPE_DICTIONARY and lbl_team_general:
		var g_adv := str(gen_adv.get("strategic_guidance", "Balance outreach across capital and labor to build electoral momentum."))
		lbl_team_general.text = "• Marcus Vance (General Adviser): %s" % [g_adv]

	var press = data.get("press_secretary", {})
	if typeof(press) == TYPE_DICTIONARY and lbl_team_press:
		var p_adv := str(press.get("briefing_summary", "Campaign rallies and media tours are elevating public recognition."))
		lbl_team_press.text = "• Elena Vance (Press Secretary): %s" % [p_adv]

	# Race Polling Overview & Contenders
	var undecided := float(data.get("campaign_undecided_polling", 10.0))
	var opponents = data.get("campaign_ai_opponents", [])
	var thorne_poll := 32.0
	var thorne_mom := -0.5
	var drake_poll := 23.0
	var drake_mom := 1.2
	var thorne_action := "Addressed conservative gala pledging fiscal austerity."
	var drake_action := "Staged viral rally criticizing status quo institutions."

	if typeof(opponents) == TYPE_ARRAY:
		for opp in opponents:
			if typeof(opp) == TYPE_DICTIONARY:
				var opp_id := str(opp.get("id", opp.get("Id", "")))
				var p := float(opp.get("polling", opp.get("Polling", 0.0)))
				var m := float(opp.get("momentum", opp.get("Momentum", 0.0)))
				var act := str(opp.get("recent_action", opp.get("RecentAction", "")))
				if opp_id == "opp_establishment":
					thorne_poll = p
					thorne_mom = m
					if not act.is_empty(): thorne_action = act
				elif opp_id == "opp_populist":
					drake_poll = p
					drake_mom = m
					if not act.is_empty(): drake_action = act

	if lbl_race_player:
		lbl_race_player.text = "👤 You (Reform): %.1f%% ▲" % [polling]
	if lbl_race_thorne:
		var arrow := "▲" if thorne_mom >= 0 else "▼"
		lbl_race_thorne.text = "🏛️ M. Thorne: %.1f%% %s" % [thorne_poll, arrow]
	if lbl_race_drake:
		var arrow := "▲" if drake_mom >= 0 else "▼"
		lbl_race_drake.text = "🔥 C. Drake: %.1f%% %s" % [drake_poll, arrow]
	if lbl_race_undecided:
		lbl_race_undecided.text = "❓ Undecided: %.1f%%" % [undecided]
	if lbl_projected_turnout:
		var projected := float(data.get("total_turnout_projected", 68.0 + (polling * 0.1)))
		lbl_projected_turnout.text = "Projected Turnout: %.1f%%" % [projected]

	if lbl_thorne_desc:
		lbl_thorne_desc.text = "Strategy: Institutional stability & corporate donors • Polling: %.1f%%
Latest Move: %s" % [thorne_poll, thorne_action]
	if lbl_drake_desc:
		lbl_drake_desc.text = "Strategy: Grassroots outrage & mass rallies • Polling: %.1f%%
Latest Move: %s" % [drake_poll, drake_action]

	# Populate Decision Cards via filtered adviser delivery list
	_populate_decisions_list()

	# Populate Interest Groups Cards
	if groups_box:
		for child in groups_box.get_children():
			groups_box.remove_child(child)
			child.queue_free()

		var groups = data.get("campaign_interest_groups", [])
		if typeof(groups) == TYPE_ARRAY and groups.size() > 0:
			for g in groups:
				if typeof(g) == TYPE_DICTIONARY:
					_create_interest_group_card(g)

	# Populate Campaign News Wire
	if news_box:
		for child in news_box.get_children():
			news_box.remove_child(child)
			child.queue_free()

		var news_items = data.get("campaign_news", [])
		if typeof(news_items) == TYPE_ARRAY and news_items.size() > 0:
			for n in news_items:
				if typeof(n) == TYPE_DICTIONARY:
					_create_news_item_card(n)

	# Election Night Status & Breakdown
	var election_resolved := bool(data.get("election_resolved", false))
	var election_won := bool(data.get("election_won", false))
	var election_tally := float(data.get("election_tally", 0.0))
	var election_summary := str(data.get("election_summary", ""))

	if lbl_election_tally:
		lbl_election_tally.text = "Election Readiness: Polling Support %.1f%% (Target: 50.0%% to Win)" % [polling]

	if election_breakdown_box:
		for child in election_breakdown_box.get_children():
			election_breakdown_box.remove_child(child)
			child.queue_free()

		var breakdown = data.get("election_breakdown", {})
		if typeof(breakdown) == TYPE_DICTIONARY and breakdown.size() > 0:
			var hdr := Label.new()
			hdr.text = "Certified Election Night Popular Vote Results:"
			hdr.modulate = Color(0.85, 0.72, 0.35)
			hdr.set("theme_override_font_sizes/font_size", 12)
			election_breakdown_box.add_child(hdr)

			var grid := GridContainer.new()
			grid.columns = 3
			grid.set("theme_override_constants/h_separation", 16)
			election_breakdown_box.add_child(grid)

			for candidate_name in breakdown.keys():
				var share := float(breakdown[candidate_name])
				var lbl_c := Label.new()
				lbl_c.text = "%s: %.1f%%" % [candidate_name, share]
				if candidate_name.to_lower().contains("you") or candidate_name == "Player":
					lbl_c.modulate = Color(0.4, 1.0, 0.4) if share >= 50.0 else Color(1.0, 0.6, 0.4)
				else:
					lbl_c.modulate = Color(0.7, 0.85, 1.0)
				grid.add_child(lbl_c)

	if election_resolved:
		if lbl_election_result:
			lbl_election_result.text = election_summary
			lbl_election_result.modulate = Color(0.4, 1.0, 0.4) if election_won else Color(1.0, 0.4, 0.4)
		if btn_assume_office:
			btn_assume_office.visible = election_won
		if card_executive_address:
			card_executive_address.visible = election_won
			var fname := registered_first_name if not registered_first_name.is_empty() else (input_first_name.text.strip_edges() if input_first_name else "Leader")
			var lname := registered_last_name if not registered_last_name.is_empty() else (input_last_name.text.strip_edges() if input_last_name else "Executive")
			if btn_title_last_name:
				btn_title_last_name.text = "President " + lname
			if btn_title_first_name:
				btn_title_first_name.text = "President " + fname
			if btn_title_full_name:
				btn_title_full_name.text = ("President %s %s" % [fname, lname]).strip_edges()
			if input_official_address and (input_official_address.text.is_empty() or input_official_address.text == "e.g. President Vance"):
				input_official_address.text = "President " + (lname if not lname.is_empty() else fname)
		if btn_contest_election:
			btn_contest_election.disabled = election_won
		if btn_start_election:
			btn_start_election.disabled = election_won
			btn_start_election.visible = not election_won
		if btn_conclude_early:
			btn_conclude_early.visible = not election_won
		if election_won:
			campaign_timer_active = false
	else:
		if lbl_election_result:
			lbl_election_result.text = "Election Status: Campaign in progress. Build public support and endorsements before contesting election night."
			lbl_election_result.modulate = Color(0.9, 0.9, 0.6)
		if btn_assume_office:
			btn_assume_office.visible = false
		if card_executive_address:
			card_executive_address.visible = false
		if btn_contest_election:
			btn_contest_election.disabled = false

func _populate_decisions_list() -> void:
	if not decisions_box: return
	for child in decisions_box.get_children():
		decisions_box.remove_child(child)
		child.queue_free()

	var decisions = active_campaign_data.get("available_campaign_decisions", [])
	if typeof(decisions) == TYPE_ARRAY:
		for d in decisions:
			if typeof(d) == TYPE_DICTIONARY:
				var cat := str(d.get("category", d.get("Category", "Campaign"))).to_lower()
				var is_press := cat.contains("media") or cat.contains("press") or cat.contains("broadcast") or cat.contains("debate")
				if active_adviser_filter == "political" and is_press:
					continue
				if active_adviser_filter == "press" and not is_press:
					continue
				_create_decision_card(d)

func _create_decision_card(d: Dictionary) -> void:
	var dec_id := str(d.get("id", d.get("Id", "")))
	var title := str(d.get("title", d.get("Title", "")))
	var category := str(d.get("category", d.get("Category", "Campaign")))
	var desc := str(d.get("description", d.get("Description", "")))
	var pos := str(d.get("foreseeable_positive_consequence", d.get("ForeseeablePositiveConsequence", "")))
	var neg := str(d.get("foreseeable_negative_consequence", d.get("ForeseeableNegativeConsequence", "")))
	var cost := float(d.get("cost_repu", d.get("CostRepu", 0.0)))
	var is_net_pos := bool(d.get("is_net_positive", d.get("IsNetPositive", true)))

	var is_press := category.to_lower().contains("media") or category.to_lower().contains("press") or category.to_lower().contains("broadcast") or category.to_lower().contains("debate")
	var adviser_tag := "🎙️ Brought by Elena Vance — Press Secretary" if is_press else "👔 Brought by Marcus Vance — Political Adviser"
	var adviser_color := Color(0.4, 0.85, 1.0) if is_press else Color(0.95, 0.78, 0.35)

	var panel := PanelContainer.new()
	var vbox := VBoxContainer.new()
	vbox.set("theme_override_constants/separation", 4)
	panel.add_child(vbox)

	# Adviser delivery notice pill
	var hbox_adviser := HBoxContainer.new()
	vbox.add_child(hbox_adviser)

	var lbl_adv := Label.new()
	lbl_adv.text = adviser_tag
	lbl_adv.modulate = adviser_color
	lbl_adv.set("theme_override_font_sizes/font_size", 10)
	lbl_adv.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	hbox_adviser.add_child(lbl_adv)

	var lbl_cat_badge := Label.new()
	lbl_cat_badge.text = "[Category: %s]" % category
	lbl_cat_badge.set("theme_override_font_sizes/font_size", 10)
	lbl_cat_badge.modulate = Color(0.7, 0.7, 0.7)
	hbox_adviser.add_child(lbl_cat_badge)

	# Header with title and expense approval button
	var hbox_hdr := HBoxContainer.new()
	vbox.add_child(hbox_hdr)

	var lbl_t := Label.new()
	lbl_t.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	lbl_t.text = "%s [%s]" % [title, category]
	lbl_t.modulate = Color(0.95, 0.95, 0.95)
	hbox_hdr.add_child(lbl_t)

	var btn_exec := Button.new()
	btn_exec.custom_minimum_size = Vector2(210, 28)
	if cost > 0.0:
		btn_exec.text = "Approve Expense (R %s)" % [_format_number(cost)]
	else:
		btn_exec.text = "Approve Agreement (R 0)"
	hbox_hdr.add_child(btn_exec)
	btn_exec.pressed.connect(_on_decision_clicked.bind(dec_id))

	# Memo description
	var lbl_d := Label.new()
	lbl_d.text = desc
	lbl_d.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_d.set("theme_override_font_sizes/font_size", 11)
	vbox.add_child(lbl_d)

	# Positive & negative consequences (strictly keeping labels test_onboarding_phase5_flow expects)
	var lbl_pos := Label.new()
	lbl_pos.text = "• Positive Consequence: " + pos
	lbl_pos.modulate = Color(0.4, 0.9, 0.4)
	lbl_pos.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_pos.set("theme_override_font_sizes/font_size", 10)
	vbox.add_child(lbl_pos)

	var lbl_neg := Label.new()
	lbl_neg.text = "• Negative Consequence: " + neg
	lbl_neg.modulate = Color(1.0, 0.6, 0.4)
	lbl_neg.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_neg.set("theme_override_font_sizes/font_size", 10)
	vbox.add_child(lbl_neg)

	decisions_box.add_child(panel)

func _create_interest_group_card(g: Dictionary) -> void:
	var gid := str(g.get("id", g.get("Id", "")))
	var name := str(g.get("name", g.get("Name", "")))
	var icon := str(g.get("icon", g.get("Icon", "🤝")))
	var desc := str(g.get("description", g.get("Description", "")))
	var demands := str(g.get("demands", g.get("Demands", "")))
	var benefit := str(g.get("endorsement_benefit", g.get("EndorsementBenefit", "")))
	var rep := float(g.get("player_reputation", g.get("PlayerReputation", 50.0)))
	var status := str(g.get("endorsement_status", g.get("EndorsementStatus", "Neutral")))
	var endorsed_cand := str(g.get("endorsed_candidate", g.get("EndorsedCandidate", "")))

	var panel := PanelContainer.new()
	var vbox := VBoxContainer.new()
	vbox.set("theme_override_constants/separation", 4)
	panel.add_child(vbox)

	var hbox_hdr := HBoxContainer.new()
	vbox.add_child(hbox_hdr)

	var lbl_title := Label.new()
	lbl_title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	lbl_title.text = "%s %s" % [icon, name]
	lbl_title.modulate = Color(0.85, 0.72, 0.35)
	hbox_hdr.add_child(lbl_title)

	var lbl_status := Label.new()
	if status == "EndorsingPlayer" or endorsed_cand.to_lower().contains("player") or endorsed_cand.to_lower().contains("you"):
		lbl_status.text = "🏆 ENDORSING YOU"
		lbl_status.modulate = Color(0.4, 1.0, 0.4)
	elif status == "EndorsingOpponent":
		lbl_status.text = "⚔️ BACKING: " + endorsed_cand
		lbl_status.modulate = Color(1.0, 0.4, 0.4)
	else:
		lbl_status.text = "⚖️ NEUTRAL / UNDECIDED"
		lbl_status.modulate = Color(0.9, 0.85, 0.4)
	hbox_hdr.add_child(lbl_status)

	var btn_court := Button.new()
	btn_court.custom_minimum_size = Vector2(240, 26)
	btn_court.text = "Approve Stakeholder Pact (R 200,000)"
	hbox_hdr.add_child(btn_court)
	btn_court.pressed.connect(_on_court_group_pressed.bind(gid, name))

	var lbl_adviser_memo := Label.new()
	lbl_adviser_memo.text = "👔 Marcus Vance: \"Securing an endorsement agreement here will shift grassroots turnout and bolster our coalition.\""
	lbl_adviser_memo.modulate = Color(0.75, 0.85, 1.0)
	lbl_adviser_memo.set("theme_override_font_sizes/font_size", 10)
	vbox.add_child(lbl_adviser_memo)

	var lbl_desc := Label.new()
	lbl_desc.text = "%s\n• Demands: %s\n• Endorsement Benefit: %s" % [desc, demands, benefit]
	lbl_desc.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_desc.set("theme_override_font_sizes/font_size", 11)
	vbox.add_child(lbl_desc)

	var hbox_rep := HBoxContainer.new()
	vbox.add_child(hbox_rep)

	var lbl_rep := Label.new()
	lbl_rep.text = "Reputation Standing: %.0f/100" % [rep]
	lbl_rep.set("theme_override_font_sizes/font_size", 10)
	lbl_rep.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	hbox_rep.add_child(lbl_rep)

	var pb := ProgressBar.new()
	pb.custom_minimum_size = Vector2(160, 14)
	pb.max_value = 100.0
	pb.value = rep
	hbox_rep.add_child(pb)

	groups_box.add_child(panel)

func _create_news_item_card(n: Dictionary) -> void:
	var headline := str(n.get("headline", n.get("Headline", "")))
	var summary := str(n.get("summary", n.get("Summary", "")))
	var source := str(n.get("source", n.get("Source", "National Press")))
	var tone := str(n.get("tone", n.get("Tone", "Neutral")))

	var panel := PanelContainer.new()
	var vbox := VBoxContainer.new()
	vbox.set("theme_override_constants/separation", 2)
	panel.add_child(vbox)

	var hbox_hdr := HBoxContainer.new()
	vbox.add_child(hbox_hdr)

	var lbl_t := Label.new()
	lbl_t.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	lbl_t.text = "%s — [%s]" % [headline, source]
	if tone.to_lower().contains("positive"):
		lbl_t.modulate = Color(0.4, 1.0, 0.4)
	elif tone.to_lower().contains("crit") or tone.to_lower().contains("neg"):
		lbl_t.modulate = Color(1.0, 0.4, 0.4)
	elif tone.to_lower().contains("contro"):
		lbl_t.modulate = Color(1.0, 0.7, 0.3)
	else:
		lbl_t.modulate = Color(0.7, 0.85, 1.0)
	hbox_hdr.add_child(lbl_t)

	var lbl_s := Label.new()
	lbl_s.text = summary
	lbl_s.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_s.set("theme_override_font_sizes/font_size", 10)
	vbox.add_child(lbl_s)

	news_box.add_child(panel)

func _on_court_group_pressed(gid: String, gname: String) -> void:
	if is_processing: return
	is_processing = true
	if lbl_campaign_feedback: lbl_campaign_feedback.text = "Courting %s with policy pledge and campaign expenditure..." % [gname]

	var req := {
		"action": "court_interest_group",
		"group_id": gid,
		"cost": 200000.0,
		"expenditure_repu": 200000.0,
		"pledge": "Commitment to policy priorities of " + gname,
		"request_id": "onboarding_court_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Group successfully courted."))
			lbl_campaign_feedback.modulate = Color(0.4, 1.0, 0.4)
		_populate_campaign_hq_view(res)
	else:
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Failed to court group."))
			lbl_campaign_feedback.modulate = Color(1.0, 0.4, 0.4)

func _on_decision_clicked(dec_id: String) -> void:
	if is_processing: return
	is_processing = true
	if lbl_campaign_feedback: lbl_campaign_feedback.text = "Executing campaign decision..."

	var req := {
		"action": "execute_campaign_decision",
		"decision_id": dec_id,
		"request_id": "onboarding_dec_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false) and res.get("result", "") == "Success":
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Decision enacted."))
			lbl_campaign_feedback.modulate = Color(0.4, 1.0, 0.4)
		_populate_campaign_hq_view(res)
	else:
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Failed to execute decision."))
			lbl_campaign_feedback.modulate = Color(1.0, 0.4, 0.4)

func _on_contest_election_pressed() -> void:
	if is_processing: return
	campaign_timer_active = false
	is_processing = true
	_select_campaign_tab("election")
	if btn_contest_election: btn_contest_election.disabled = true
	if lbl_campaign_feedback: lbl_campaign_feedback.text = "Tabulating national election night votes across all constituencies..."

	var req := {
		"action": "resolve_campaign_election",
		"request_id": "onboarding_elect_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false):
		_populate_campaign_hq_view(res)
		var won := bool(res.get("election_won", false))
		if won:
			if lbl_campaign_feedback:
				lbl_campaign_feedback.text = "ELECTION VICTORY! Democratic mandate established."
				lbl_campaign_feedback.modulate = Color(0.4, 1.0, 0.4)
		else:
			if btn_contest_election: btn_contest_election.disabled = false
			if lbl_campaign_feedback:
				lbl_campaign_feedback.text = "Electoral shortfall: Tally fell below 50.0%. Court key groups or enact campaign actions to surge."
				lbl_campaign_feedback.modulate = Color(1.0, 0.4, 0.4)
	else:
		if btn_contest_election: btn_contest_election.disabled = false
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Failed to resolve election."))
			lbl_campaign_feedback.modulate = Color(1.0, 0.4, 0.4)

func _on_title_last_name_pressed() -> void:
	if input_official_address:
		var lname := registered_last_name if not registered_last_name.is_empty() else (input_last_name.text.strip_edges() if input_last_name else "Executive")
		input_official_address.text = "President " + lname

func _on_title_first_name_pressed() -> void:
	if input_official_address:
		var fname := registered_first_name if not registered_first_name.is_empty() else (input_first_name.text.strip_edges() if input_first_name else "Leader")
		input_official_address.text = "President " + fname

func _on_title_full_name_pressed() -> void:
	if input_official_address:
		var fname := registered_first_name if not registered_first_name.is_empty() else (input_first_name.text.strip_edges() if input_first_name else "")
		var lname := registered_last_name if not registered_last_name.is_empty() else (input_last_name.text.strip_edges() if input_last_name else "")
		input_official_address.text = ("President %s %s" % [fname, lname]).strip_edges()

func _on_title_madam_pressed() -> void:
	if input_official_address:
		input_official_address.text = "Madam President"

func _on_title_mister_pressed() -> void:
	if input_official_address:
		input_official_address.text = "Mr. President"

func _on_random_leadership_pressed() -> void:
	var archetypes := ["technocrat", "peoples_champion", "visionary", "strategist", "diplomat"]
	archetypes.shuffle()
	var pick1: String = archetypes[0]
	var pick2: String = archetypes[1]

	selected_archetypes.clear()
	for btn in [btn_arch_technocrat, btn_arch_populist, btn_arch_visionary, btn_arch_statist, btn_arch_diplomat]:
		if btn:
			btn.text = "Select"
			btn.modulate = Color(1.0, 1.0, 1.0)
			var card = btn.get_parent().get_parent().get_parent() if btn.get_parent() and btn.get_parent().get_parent() else null
			if card is Control: card.modulate = Color(1.0, 1.0, 1.0)

	_toggle_archetype(pick1, _get_archetype_button(pick1))
	_toggle_archetype(pick2, _get_archetype_button(pick2))

	if lbl_leadership_feedback:
		lbl_leadership_feedback.text = "Randomized leadership behavior doctrine: %s!" % _compute_doctrine_title(selected_archetypes)
		lbl_leadership_feedback.modulate = Color(0.4, 1.0, 0.4)

func _get_archetype_button(key: String) -> Button:
	match key:
		"technocrat": return btn_arch_technocrat
		"peoples_champion": return btn_arch_populist
		"visionary": return btn_arch_visionary
		"strategist": return btn_arch_statist
		"diplomat": return btn_arch_diplomat
		_: return null

func _on_random_plans_pressed() -> void:
	var all_plans := [
		"industrialisation_first",
		"agricultural_transformation",
		"technology_and_innovation",
		"infrastructure_revolution",
		"human_capital_development",
		"financial_and_commercial_power",
		"energy_and_natural_resources",
		"national_security_and_defence",
		"sustainable_development",
		"global_trade_and_international_influence"
	]
	all_plans.shuffle()

	selected_plans.clear()
	var all_buttons := [
		btn_plan_1, btn_plan_2, btn_plan_3, btn_plan_4, btn_plan_5,
		btn_plan_6, btn_plan_7, btn_plan_8, btn_plan_9, btn_plan_10
	]
	for btn in all_buttons:
		if btn:
			btn.text = "Select"
			btn.modulate = Color(1.0, 1.0, 1.0)
			var card = btn.get_parent().get_parent().get_parent() if btn.get_parent() and btn.get_parent().get_parent() else null
			if card is Control: card.modulate = Color(1.0, 1.0, 1.0)

	for i in range(4):
		var p_key: String = all_plans[i]
		var btn = _get_plan_button_by_key(p_key)
		_on_plan_clicked(p_key, btn)

	if lbl_plans_feedback:
		lbl_plans_feedback.text = "Randomized inaugural national development priorities!"
		lbl_plans_feedback.modulate = Color(0.4, 1.0, 0.4)

func _get_plan_button_by_key(key: String) -> Button:
	match key:
		"industrialisation_first": return btn_plan_1
		"agricultural_transformation": return btn_plan_2
		"technology_and_innovation": return btn_plan_3
		"infrastructure_revolution": return btn_plan_4
		"human_capital_development": return btn_plan_5
		"financial_and_commercial_power": return btn_plan_6
		"energy_and_natural_resources": return btn_plan_7
		"national_security_and_defence": return btn_plan_8
		"sustainable_development": return btn_plan_9
		"global_trade_and_international_influence": return btn_plan_10
		_: return null

func _on_assume_office_pressed() -> void:
	if is_processing: return
	is_processing = true
	if btn_assume_office: btn_assume_office.disabled = true
	if lbl_campaign_feedback: lbl_campaign_feedback.text = "Administering presidential oath of office..."

	var pref_title := input_official_address.text.strip_edges() if input_official_address else ""
	if pref_title.is_empty():
		var lname := registered_last_name if not registered_last_name.is_empty() else (input_last_name.text.strip_edges() if input_last_name else "")
		var fname := registered_first_name if not registered_first_name.is_empty() else (input_first_name.text.strip_edges() if input_first_name else "")
		pref_title = "President " + (lname if not lname.is_empty() else fname)

	selected_title = pref_title

	var req := {
		"action": "assume_office",
		"preferred_title": pref_title,
		"player_display_name": pref_title,
		"official_title": pref_title,
		"request_id": "onboarding_inaug_" + str(Time.get_unix_time_from_system())
	}

	var res := _submit_request_and_wait(req)
	is_processing = false

	if res.get("success", false):
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = "Inauguration complete! Welcome to the Presidential Executive Office, %s." % pref_title
			lbl_campaign_feedback.modulate = Color(0.4, 1.0, 0.4)
		onboarding_completed.emit(res)
		if is_inside_tree() and get_tree() != null:
			get_tree().change_scene_to_file("res://ExecutiveOffice.tscn")
	else:
		if btn_assume_office: btn_assume_office.disabled = false
		if lbl_campaign_feedback:
			lbl_campaign_feedback.text = str(res.get("message", "Failed to assume office."))
			lbl_campaign_feedback.modulate = Color(1.0, 0.4, 0.4)

func _update_campaign_timer_display() -> void:
	var total_sec := int(max(0.0, campaign_time_remaining))
	var mins := total_sec / 60
	var secs := total_sec % 60
	var time_str := "%02d:%02d" % [mins, secs]

	if lbl_campaign_countdown:
		if campaign_time_remaining <= 0.0:
			lbl_campaign_countdown.text = "⏱️ 00:00 (Campaign Closed)"
			lbl_campaign_countdown.modulate = Color(1.0, 0.45, 0.45)
		else:
			lbl_campaign_countdown.text = "⏱️ %s to Election" % [time_str]
			lbl_campaign_countdown.modulate = Color(1.0, 0.85, 0.35)

	if btn_start_election:
		if campaign_time_remaining <= 0.0:
			btn_start_election.text = "🗳️ Start Election (Voting Open!)"
			btn_start_election.disabled = false
			btn_start_election.modulate = Color(0.4, 1.0, 0.4)
		else:
			btn_start_election.text = "🗳️ Start Election (%s)" % [time_str]
			btn_start_election.disabled = false
			btn_start_election.modulate = Color(0.9, 0.85, 0.7)

	if btn_conclude_early:
		btn_conclude_early.visible = (campaign_time_remaining > 0.0)

func _on_campaign_timer_completed() -> void:
	campaign_time_remaining = 0.0
	campaign_timer_active = false
	_update_campaign_timer_display()
	if lbl_campaign_feedback:
		lbl_campaign_feedback.text = "⏱️ Active campaign period concluded! Polling stations are open across the sovereign nation."
		lbl_campaign_feedback.modulate = Color(0.4, 1.0, 0.4)
	if btn_contest_election:
		btn_contest_election.disabled = false
	if btn_start_election:
		btn_start_election.disabled = false

func _on_conclude_early_pressed() -> void:
	campaign_time_remaining = 0.0
	campaign_timer_active = false
	_on_campaign_timer_completed()
	_select_campaign_tab("election")

func _on_filter_all_pressed() -> void:
	_on_filter_adviser_pressed("all")

func _on_filter_political_pressed() -> void:
	_on_filter_adviser_pressed("political")

func _on_filter_press_pressed() -> void:
	_on_filter_adviser_pressed("press")

func _on_filter_adviser_pressed(mode: String) -> void:
	active_adviser_filter = mode
	_update_adviser_filter_buttons()
	_populate_decisions_list()

func _update_adviser_filter_buttons() -> void:
	if btn_filter_all:
		btn_filter_all.modulate = Color(1.0, 0.9, 0.4) if active_adviser_filter == "all" else Color(0.65, 0.65, 0.65)
	if btn_filter_political:
		btn_filter_political.modulate = Color(1.0, 0.9, 0.4) if active_adviser_filter == "political" else Color(0.65, 0.65, 0.65)
	if btn_filter_press:
		btn_filter_press.modulate = Color(1.0, 0.9, 0.4) if active_adviser_filter == "press" else Color(0.65, 0.65, 0.65)
