# ==============================================================================
# Republic - New Player Onboarding Controller (Phase 2)
# Handles the 5-stage onboarding sequence:
# 1. Account Registration (First/Last name, Email, DOB - No password, PII isolated)
# 2. Email Verification (Official dispatch display, "Verify Email" button, token input, resend)
# 3. Country Naming (Validation, 'Republic' requirement, confirmation step)
# 4. Procedural Country Generation (Animated progress, 50M REPU, 60% approval, 10M pop)
# 5. Country Creation Confirmation (Ratified sovereign profile, persists & resumes)
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
	CONFIRMED
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

var is_processing: bool = false
var resend_cooldown_timer: float = 0.0

# Node References
@onready var register_panel: Control = $PanelContainer/VBox/ScreenRegister
@onready var verify_panel: Control = $PanelContainer/VBox/ScreenVerify
@onready var name_panel: Control = $PanelContainer/VBox/ScreenCountryName
@onready var generating_panel: Control = $PanelContainer/VBox/ScreenGenerating
@onready var confirmed_panel: Control = $PanelContainer/VBox/ScreenConfirmed

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

func _ready() -> void:
	_init_controls()
	_connect_signals()
	_show_stage(ScreenStage.REGISTER)

func _init_controls() -> void:
	if register_panel == null: register_panel = get_node_or_null("PanelContainer/VBox/ScreenRegister")
	if verify_panel == null: verify_panel = get_node_or_null("PanelContainer/VBox/ScreenVerify")
	if name_panel == null: name_panel = get_node_or_null("PanelContainer/VBox/ScreenCountryName")
	if generating_panel == null: generating_panel = get_node_or_null("PanelContainer/VBox/ScreenGenerating")
	if confirmed_panel == null: confirmed_panel = get_node_or_null("PanelContainer/VBox/ScreenConfirmed")

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

func _show_stage(stage: ScreenStage) -> void:
	current_stage = stage
	if register_panel: register_panel.visible = (stage == ScreenStage.REGISTER)
	if verify_panel: verify_panel.visible = (stage == ScreenStage.VERIFY_EMAIL)
	if name_panel:
		name_panel.visible = (stage == ScreenStage.COUNTRY_NAME)
		if confirmation_modal: confirmation_modal.visible = false
	if generating_panel: generating_panel.visible = (stage == ScreenStage.GENERATING)
	if confirmed_panel: confirmed_panel.visible = (stage == ScreenStage.CONFIRMED)

	var stage_name := "Register"
	match stage:
		ScreenStage.REGISTER: stage_name = "Register"
		ScreenStage.VERIFY_EMAIL: stage_name = "VerifyEmail"
		ScreenStage.COUNTRY_NAME: stage_name = "CountryName"
		ScreenStage.GENERATING: stage_name = "Generating"
		ScreenStage.CONFIRMED: stage_name = "Confirmed"

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
# Step 5: Country Creation Confirmed
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

func _on_resume_session_pressed() -> void:
	# Reloads state from disk to confirm persistent survival across restart
	var file := FileAccess.open(ProjectSettings.globalize_path(active_state_path), FileAccess.READ)
	if file != null:
		var text := file.get_as_text()
		file.close()
		var parsed = JSON.parse_string(text)
		if parsed != null and typeof(parsed) == TYPE_DICTIONARY:
			_populate_confirmed_view(parsed)
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
