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
	ADVISERS
}

var current_stage: ScreenStage = ScreenStage.REGISTER
var active_state_path: String = "res://presidential_desk_state.json"
var active_save_path: String = "res://session.sav"
var active_request_path: String = "res://presidential_desk_request.json"
var active_result_path: String = "res://presidential_desk_result.json"

# In-memory session state
var registered_first_name: String = ""
var registered_last_name: String = ""
var registered_email: String = ""
var registered_dob: String = ""
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
@onready var input_first_name: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/FirstNameInput
@onready var input_last_name: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/LastNameInput
@onready var input_email: LineEdit = $PanelContainer/VBox/ScreenRegister/Form/EmailInput
@onready var spin_dob_year: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/YearSpin
@onready var spin_dob_month: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/MonthSpin
@onready var spin_dob_day: SpinBox = $PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/DaySpin
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

func _ready() -> void:
	_init_controls()
	_connect_signals()
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

	if input_first_name == null: input_first_name = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/FirstNameInput")
	if input_last_name == null: input_last_name = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/LastNameInput")
	if input_email == null: input_email = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/EmailInput")
	if spin_dob_year == null: spin_dob_year = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/YearSpin")
	if spin_dob_month == null: spin_dob_month = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/MonthSpin")
	if spin_dob_day == null: spin_dob_day = get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB/DaySpin")
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

func _connect_signals() -> void:
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
	if btn_proceed_leadership and not btn_proceed_leadership.pressed.is_connected(_on_proceed_leadership_pressed):
		btn_proceed_leadership.pressed.connect(_on_proceed_leadership_pressed)

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

func _connect_plan_button(btn: Button, plan_key: String) -> void:
	if btn and not btn.pressed.is_connected(_on_plan_clicked.bind(plan_key, btn)):
		btn.pressed.connect(_on_plan_clicked.bind(plan_key, btn))

func _load_initial_stage() -> void:
	# Checks disk for existing authoritative session
	var global_state := ProjectSettings.globalize_path(active_state_path)
	if FileAccess.file_exists(global_state):
		var file := FileAccess.open(global_state, FileAccess.READ)
		if file != null:
			var content := file.get_as_text()
			file.close()
			var parsed = JSON.parse_string(content)
			if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
				var step: String = str(parsed.get("onboarding_step", "NotStarted"))
				match step:
					"NotStarted":
						_show_stage(ScreenStage.REGISTER)
					"AccountRegistered":
						_show_stage(ScreenStage.VERIFY_EMAIL)
					"EmailVerified":
						_show_stage(ScreenStage.COUNTRY_NAME)
					"CountryCreated":
						_populate_confirmed_view(parsed)
						_show_stage(ScreenStage.CONFIRMED)
					"LeadershipSelected":
						_show_stage(ScreenStage.DEVELOPMENT_PLANS)
					"DevelopmentPlansSelected", "AdvisersIntroduced", "CampaignHeadquarters", "InOffice":
						_populate_phase3_summary_view(parsed)
						_show_stage(ScreenStage.PHASE3_SUMMARY)
					_:
						_show_stage(ScreenStage.REGISTER)
				return
	_show_stage(ScreenStage.REGISTER)

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

	onboarding_step_changed.emit(stage_name)

# ==============================================================================
# Step 1: Account Registration
# ==============================================================================
func _on_register_pressed() -> void:
	if is_processing: return
	if lbl_register_feedback: lbl_register_feedback.text = ""

	var first_name := input_first_name.text.strip_edges() if input_first_name else ""
	var last_name := input_last_name.text.strip_edges() if input_last_name else ""
	var email := input_email.text.strip_edges() if input_email else ""
	var year := int(spin_dob_year.value) if spin_dob_year else 1990
	var month := int(spin_dob_month.value) if spin_dob_month else 1
	var day := int(spin_dob_day.value) if spin_dob_day else 1
	var dob_str := "%04d-%02d-%02d" % [year, month, day]

	if first_name.is_empty():
		_display_register_error("First name is required.")
		return
	if last_name.is_empty():
		_display_register_error("Last name is required.")
		return
	if email.is_empty() or not email.contains("@") or not email.contains("."):
		_display_register_error("Please provide a valid email address.")
		return

	# Age verification (<13 years old)
	var sim_year := 2027
	var age := sim_year - year
	if age < 13:
		_display_register_error("Candidates must be at least 13 years of age to register (age: %d)." % [age])
		return

	is_processing = true
	if btn_register: btn_register.disabled = true
	if lbl_register_feedback: lbl_register_feedback.text = "Registering account with Sovereign Electoral Registry..."

	var req := {
		"action": "register_account",
		"first_name": first_name,
		"last_name": last_name,
		"email": email,
		"date_of_birth": dob_str,
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

	var req := {
		"action": "create_country",
		"country_name": country_name,
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
		onboarding_completed.emit(res)
	else:
		if lbl_advisers_feedback:
			lbl_advisers_feedback.text = str(res.get("message", "Failed to induct Advisory Council."))
			lbl_advisers_feedback.modulate = Color(1.0, 0.4, 0.4)
