# ==============================================================================
# Republic - Start Gateway Controller (Auth.gd)
# Primary project entrypoint for all game launches (Web and Desktop .exe).
# Provides:
# 1. Sign Up flow -> transitions into Onboarding.tscn for candidate & nation creation
# 2. Log In flow -> inspects authoritative session from disk
# 3. Phase-based routing:
#    - Campaign Phase -> routes directly to Campaign Headquarters
#    - In Office Phase -> routes directly to Executive Office
# ==============================================================================
class_name AuthController
extends Control

var active_state_path: String = "res://presidential_desk_state.json"
var active_save_path: String = "res://session.sav"

@onready var btn_tab_sign_up: Button = $MainPanel/VBox/TabHBox/BtnTabSignUp
@onready var btn_tab_login: Button = $MainPanel/VBox/TabHBox/BtnTabLogin
@onready var sign_up_box: VBoxContainer = $MainPanel/VBox/SignUpBox
@onready var login_box: VBoxContainer = $MainPanel/VBox/LoginBox
@onready var btn_go_to_sign_up: Button = $MainPanel/VBox/SignUpBox/BtnGoToSignUp
@onready var lbl_detected_country: Label = $MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/LblDetectedCountry
@onready var lbl_detected_phase: Label = $MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/LblDetectedPhase
@onready var btn_resume_session: Button = $MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/BtnResumeSession
@onready var input_email: LineEdit = $MainPanel/VBox/LoginBox/ManualEmailBox/InputEmail
@onready var btn_email_login: Button = $MainPanel/VBox/LoginBox/ManualEmailBox/BtnEmailLogin
@onready var lbl_feedback: Label = $MainPanel/VBox/LblFeedback

var detected_session: Dictionary = {}

func _ready() -> void:
	_init_controls()
	_connect_signals()
	_refresh_detected_session()
	
	# If an authoritative session exists, indicate it on the Login tab
	if not detected_session.is_empty():
		_show_login_tab()
	else:
		_show_signup_tab()

func _init_controls() -> void:
	if btn_tab_sign_up == null: btn_tab_sign_up = get_node_or_null("MainPanel/VBox/TabHBox/BtnTabSignUp")
	if btn_tab_login == null: btn_tab_login = get_node_or_null("MainPanel/VBox/TabHBox/BtnTabLogin")
	if sign_up_box == null: sign_up_box = get_node_or_null("MainPanel/VBox/SignUpBox")
	if login_box == null: login_box = get_node_or_null("MainPanel/VBox/LoginBox")
	if btn_go_to_sign_up == null: btn_go_to_sign_up = get_node_or_null("MainPanel/VBox/SignUpBox/BtnGoToSignUp")
	if lbl_detected_country == null: lbl_detected_country = get_node_or_null("MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/LblDetectedCountry")
	if lbl_detected_phase == null: lbl_detected_phase = get_node_or_null("MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/LblDetectedPhase")
	if btn_resume_session == null: btn_resume_session = get_node_or_null("MainPanel/VBox/LoginBox/SavedSessionCard/SessionVBox/BtnResumeSession")
	if input_email == null: input_email = get_node_or_null("MainPanel/VBox/LoginBox/ManualEmailBox/InputEmail")
	if btn_email_login == null: btn_email_login = get_node_or_null("MainPanel/VBox/LoginBox/ManualEmailBox/BtnEmailLogin")
	if lbl_feedback == null: lbl_feedback = get_node_or_null("MainPanel/VBox/LblFeedback")

func _connect_signals() -> void:
	if btn_tab_sign_up and not btn_tab_sign_up.pressed.is_connected(_on_tab_sign_up_pressed):
		btn_tab_sign_up.pressed.connect(_on_tab_sign_up_pressed)
	if btn_tab_login and not btn_tab_login.pressed.is_connected(_on_tab_login_pressed):
		btn_tab_login.pressed.connect(_on_tab_login_pressed)
	if btn_go_to_sign_up and not btn_go_to_sign_up.pressed.is_connected(_on_go_to_sign_up_pressed):
		btn_go_to_sign_up.pressed.connect(_on_go_to_sign_up_pressed)
	if btn_resume_session and not btn_resume_session.pressed.is_connected(_on_resume_session_pressed):
		btn_resume_session.pressed.connect(_on_resume_session_pressed)
	if btn_email_login and not btn_email_login.pressed.is_connected(_on_email_login_pressed):
		btn_email_login.pressed.connect(_on_email_login_pressed)

func _on_tab_sign_up_pressed() -> void:
	_show_signup_tab()

func _on_tab_login_pressed() -> void:
	_show_login_tab()

func _show_signup_tab() -> void:
	if sign_up_box: sign_up_box.visible = true
	if login_box: login_box.visible = false
	if btn_tab_sign_up: btn_tab_sign_up.modulate = Color(1.0, 1.0, 1.0, 1.0)
	if btn_tab_login: btn_tab_login.modulate = Color(0.7, 0.7, 0.7, 0.8)
	if lbl_feedback: lbl_feedback.text = ""

func _show_login_tab() -> void:
	if sign_up_box: sign_up_box.visible = false
	if login_box: login_box.visible = true
	if btn_tab_sign_up: btn_tab_sign_up.modulate = Color(0.7, 0.7, 0.7, 0.8)
	if btn_tab_login: btn_tab_login.modulate = Color(1.0, 1.0, 1.0, 1.0)
	if lbl_feedback: lbl_feedback.text = ""
	_refresh_detected_session()

func _refresh_detected_session() -> Dictionary:
	detected_session = _load_session_data_from_disk()
	if not detected_session.is_empty():
		var country: String = str(detected_session.get("country_name", detected_session.get("CountryName", "Republic of Sovereign Nations")))
		var leader: String = str(detected_session.get("president_name", detected_session.get("leader_name", "Executive Leader")))
		var step: String = str(detected_session.get("onboarding_step", "InOffice"))
		
		if lbl_detected_country:
			lbl_detected_country.text = country + " (Leader: " + leader + ")"
		
		var phase_text := "Phase: In Office (Executive Office)"
		var phase_color := Color(0.4, 1.0, 0.5)
		var btn_text := "▶ Enter Executive Office"

		if step == "CampaignHeadquarters" or step == "AdvisersIntroduced":
			phase_text = "Phase: Campaign Headquarters (Presidential Race)"
			phase_color = Color(1.0, 0.85, 0.3)
			btn_text = "▶ Enter Campaign Headquarters"
		elif step == "InOffice" or detected_session.has("treasury_balance"):
			phase_text = "Phase: In Office (Executive Office)"
			phase_color = Color(0.4, 1.0, 0.5)
			btn_text = "▶ Enter Executive Office"
		else:
			phase_text = "Phase: Sovereign Founding (" + step + ")"
			phase_color = Color(0.6, 0.8, 1.0)
			btn_text = "▶ Continue Setup"

		if lbl_detected_phase:
			lbl_detected_phase.text = phase_text
			lbl_detected_phase.modulate = phase_color
		if btn_resume_session:
			btn_resume_session.text = btn_text
			btn_resume_session.disabled = false
		return detected_session
	else:
		if lbl_detected_country:
			lbl_detected_country.text = "No saved Republic found on this system."
		if lbl_detected_phase:
			lbl_detected_phase.text = "Phase: None (Select 'Sign Up' to establish a Republic)"
			lbl_detected_phase.modulate = Color(0.7, 0.7, 0.7)
		if btn_resume_session:
			btn_resume_session.text = "▶ No Saved Republic"
			btn_resume_session.disabled = true
		return {}

func _on_go_to_sign_up_pressed() -> void:
	print("[AUTH]: Navigating to Onboarding flow (New Republic creation).")
	if is_inside_tree() and get_tree() != null:
		get_tree().change_scene_to_file("res://Onboarding.tscn")

func _on_resume_session_pressed() -> void:
	if detected_session.is_empty():
		_refresh_detected_session()
	if detected_session.is_empty():
		if lbl_feedback:
			lbl_feedback.text = "No existing Republic session found. Please click 'Sign Up' to begin."
			lbl_feedback.modulate = Color(1.0, 0.5, 0.3)
		return
	
	if lbl_feedback:
		lbl_feedback.text = "Session verified. Launching..."
		lbl_feedback.modulate = Color(0.4, 1.0, 0.4)
	_route_to_phase(detected_session)

func _on_email_login_pressed() -> void:
	var email := input_email.text.strip_edges() if input_email else ""
	if email.is_empty():
		if lbl_feedback:
			lbl_feedback.text = "Please enter your registered email address."
			lbl_feedback.modulate = Color(1.0, 0.4, 0.4)
		return
	if not email.contains("@") or not email.contains("."):
		if lbl_feedback:
			lbl_feedback.text = "Please enter a valid email address."
			lbl_feedback.modulate = Color(1.0, 0.4, 0.4)
		return

	if detected_session.is_empty():
		_refresh_detected_session()
	if detected_session.is_empty():
		if lbl_feedback:
			lbl_feedback.text = "No Republic found. Please select 'Sign Up' to create one."
			lbl_feedback.modulate = Color(1.0, 0.6, 0.3)
		return

	if lbl_feedback:
		lbl_feedback.text = "Signed in as " + email + ". Launching to office..."
		lbl_feedback.modulate = Color(0.4, 1.0, 0.4)
	_route_to_phase(detected_session)

func _route_to_phase(session_data: Dictionary) -> void:
	var step: String = str(session_data.get("onboarding_step", ""))
	
	# 1. Office Phase -> Executive Office
	if step == "InOffice" or (session_data.has("country_name") and (step.is_empty() or step == "InOffice") and session_data.has("treasury_balance")):
		print("[AUTH/LAUNCH]: Routing player directly to Executive Office.")
		if is_inside_tree() and get_tree() != null:
			get_tree().change_scene_to_file("res://ExecutiveOffice.tscn")
		return

	# 2. Campaign Phase -> Campaign Headquarters
	if step == "CampaignHeadquarters" or step == "AdvisersIntroduced":
		print("[AUTH/LAUNCH]: Routing player directly to Campaign Headquarters.")
		Engine.set_meta("launch_to_campaign_hq", true)
		if is_inside_tree() and get_tree() != null:
			get_tree().change_scene_to_file("res://Onboarding.tscn")
		return

	# 3. In-Progress Founding
	print("[AUTH/LAUNCH]: Routing player to Onboarding founding stage (", step, ").")
	if is_inside_tree() and get_tree() != null:
		get_tree().change_scene_to_file("res://Onboarding.tscn")

func _load_session_data_from_disk() -> Dictionary:
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
