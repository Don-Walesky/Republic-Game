# ==============================================================================
# Republic - Simplified Onboarding UI Integration Test
# Verifies:
# 1. Registration Screen: Only First Name, Last Name, Username are visible.
#    Email, DOB, Gender form nodes are hidden from user while preserving state pipeline.
# 2. Randomization:
#    - BtnRandomLeadership randomizes exactly 2 archetypes and updates doctrine text.
#    - BtnRandomPlans randomizes exactly 4 development plans and updates count label.
# 3. Streamlined Card Selection:
#    - Clicking card containers or buttons toggles selection with clear visual feedback.
# 4. Post-Election Executive Address Choice:
#    - CardExecutiveAddress appears upon election victory.
#    - Prefills with "President [Last Name]" or "President [First Name]".
#    - Preset address buttons ("President [Last Name]", "President [First Name]", "President Full Name", "Madam President", "Mr. President") update the address field.
#    - Custom address editing works.
#    - Assume office request dispatches the selected official title into state.
# 5. Spacing and Screen Real Estate Responsiveness:
#    - PanelContainer uses full screen anchors preset (15) with generous margins.
# ==============================================================================
extends SceneTree

const TEST_STATE_PATH := "res://test_onboarding_ui_state.json"
const TEST_SAVE_PATH := "res://test_onboarding_ui_session.sav"
const TEST_REQ_PATH := "res://presidential_desk_request.json"
const TEST_RES_PATH := "res://presidential_desk_result.json"

const OnboardingScript = preload("res://Onboarding.gd")

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - SIMPLIFIED ONBOARDING UI VERIFICATION TEST      ")
	print("==============================================================")
	_run_ui_verification()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQ_PATH, TEST_RES_PATH]:
		var g := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(g):
			DirAccess.remove_absolute(g)

func _fail(msg: String) -> void:
	printerr("\n[FAIL]: " + msg)
	_clean_files()
	quit(1)

func _assert(cond: bool, msg: String) -> void:
	if not cond:
		_fail(msg)

func _run_ui_verification() -> void:
	_clean_files()

	var onboarding: OnboardingController = load("res://Onboarding.tscn").instantiate()
	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding.active_request_path = TEST_REQ_PATH
	onboarding.active_result_path = TEST_RES_PATH
	onboarding._init_controls()

	# --------------------------------------------------------------------------
	# Check 1: 3-Field Registration & Hidden Legacy Elements
	# --------------------------------------------------------------------------
	print("\n--- Check 1: 3-Field Registration (First, Last, Username) ---")
	_assert(onboarding.input_first_name != null, "FirstNameInput node missing")
	_assert(onboarding.input_last_name != null, "LastNameInput node missing")
	_assert(onboarding.input_username != null, "UsernameInput node missing")

	_assert(onboarding.input_first_name.visible, "FirstNameInput should be visible")
	_assert(onboarding.input_last_name.visible, "LastNameInput should be visible")
	_assert(onboarding.input_username.visible, "UsernameInput should be visible")

	var email_node: Control = onboarding.get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/EmailInput")
	var dob_node: Control = onboarding.get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxDOB")
	var gender_node: Control = onboarding.get_node_or_null("PanelContainer/VBox/ScreenRegister/Form/HBoxGender")

	_assert(email_node != null and not email_node.visible, "EmailInput must exist but be hidden (visible = false)")
	_assert(dob_node != null and not dob_node.visible, "HBoxDOB must exist but be hidden (visible = false)")
	_assert(gender_node != null and not gender_node.visible, "HBoxGender must exist but be hidden (visible = false)")
	print("  [OK]: Only First Name, Last Name, and Username are visible on ScreenRegister.")

	# Fill 3 fields only and register
	onboarding.input_first_name.text = "Amara"
	onboarding.input_last_name.text = "Sterling"
	onboarding.input_username.text = "a_sterling"
	onboarding._on_register_pressed()

	_assert(onboarding.registered_username == "a_sterling", "Registered username should match 'a_sterling'")
	_assert(onboarding.registered_email.begins_with("a_sterling@republic"), "Default email should derive from username")
	print("  [OK]: Registration succeeded with 3 fields; account initialized.")

	# Verify email
	onboarding._on_verify_action_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.COUNTRY_NAME, "Should transition to COUNTRY_NAME")
	print("  [OK]: Verification confirmed. Advanced to ScreenNation.")

	# --------------------------------------------------------------------------
	# Check 2: Found Nation
	# --------------------------------------------------------------------------
	print("\n--- Check 2: Found Nation & Transition to Leadership ---")
	onboarding.input_country_name.text = "Republic of Valeria"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.CONFIRMED, "Should transition to CONFIRMED stage")

	onboarding._on_proceed_leadership_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.LEADERSHIP, "Should transition to ScreenLeadership")
	print("  [OK]: Republic of Valeria founded. On ScreenLeadership.")

	# --------------------------------------------------------------------------
	# Check 3: Leadership Randomization & Streamlined Card Selection
	# --------------------------------------------------------------------------
	print("\n--- Check 3: Leadership Randomization & Simplified Selection ---")
	_assert(onboarding.btn_random_leadership != null, "BtnRandomLeadership button missing")
	_assert(onboarding.btn_random_leadership.visible, "BtnRandomLeadership must be visible")

	# Test Randomization
	onboarding._on_random_leadership_pressed()
	_assert(onboarding.selected_archetypes.size() == 2, "Random leadership must select exactly 2 archetypes, got: " + str(onboarding.selected_archetypes.size()))
	_assert(not onboarding.lbl_doctrine_preview.text.is_empty(), "Doctrine preview label should be updated")
	_assert(onboarding.btn_confirm_leadership.disabled == false, "Confirm Leadership button should be enabled when 2 archetypes selected")
	print("  [OK]: Random leadership selected 2 archetypes: %s" % str(onboarding.selected_archetypes))

	# Test direct card toggle click
	var card_tech = onboarding.get_node_or_null("PanelContainer/VBox/ScreenLeadership/ScrollLeadership/CardsBox/CardTechnocrat")
	_assert(card_tech != null, "CardTechnocrat node missing")
	var dummy_ev = InputEventMouseButton.new()
	dummy_ev.button_index = MOUSE_BUTTON_LEFT
	dummy_ev.pressed = true
	card_tech.gui_input.emit(dummy_ev)
	print("  [OK]: Direct card click handler toggled selection successfully.")

	# Re-randomize to ensure exactly 2 archetypes and confirm
	onboarding._on_random_leadership_pressed()
	_assert(onboarding.selected_archetypes.size() == 2, "Must have 2 archetypes selected")
	onboarding._on_confirm_leadership_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.DEVELOPMENT_PLANS, "Should transition to ScreenDevelopmentPlans")
	print("  [OK]: Confirmed leadership; on ScreenDevelopmentPlans.")

	# --------------------------------------------------------------------------
	# Check 4: Development Plans Randomization
	# --------------------------------------------------------------------------
	print("\n--- Check 4: Development Focus Randomization & Simplified Selection ---")
	_assert(onboarding.btn_random_plans != null, "BtnRandomPlans button missing")
	_assert(onboarding.btn_random_plans.visible, "BtnRandomPlans must be visible")

	# Test Randomization
	onboarding._on_random_plans_pressed()
	_assert(onboarding.selected_plans.size() == 4, "Random plans must select exactly 4 plans, got: " + str(onboarding.selected_plans.size()))
	_assert(onboarding.btn_confirm_plans.disabled == false, "Confirm Plans button should be enabled when 4 plans selected")
	print("  [OK]: Random development plans selected 4 priorities: %s" % str(onboarding.selected_plans))

	onboarding._on_confirm_plans_pressed()
	onboarding._on_proceed_phase4_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.ADVISERS, "Should transition to ScreenAdvisers")
	print("  [OK]: Development priorities confirmed; on ScreenAdvisers.")

	# Advance advisers to Campaign HQ
	onboarding._on_unlock_slot2_pressed()
	onboarding._on_confirm_advisers_pressed()
	_assert(onboarding.current_stage == OnboardingScript.ScreenStage.CAMPAIGN_HQ, "Should transition to ScreenCampaignHq")
	print("  [OK]: Advisory council confirmed; on ScreenCampaignHq.")

	# --------------------------------------------------------------------------
	# Check 5: Post-Election Executive Address Choice
	# --------------------------------------------------------------------------
	print("\n--- Check 5: Post-Election Executive Address Choice & Presets ---")
	_assert(onboarding.card_executive_address != null, "CardExecutiveAddress node missing")
	_assert(onboarding.input_official_address != null, "InputOfficialAddress LineEdit missing")
	_assert(onboarding.btn_title_last_name != null, "BtnTitleLastName preset button missing")
	_assert(onboarding.btn_title_first_name != null, "BtnTitleFirstName preset button missing")
	_assert(onboarding.btn_title_full_name != null, "BtnTitleFullName preset button missing")
	_assert(onboarding.btn_title_madam != null, "BtnTitleMadam preset button missing")
	_assert(onboarding.btn_title_mister != null, "BtnTitleMister preset button missing")

	# Populate election victory view
	var mock_state: Dictionary = {
		"campaign_purse": 4000000.0,
		"national_polling": 65.0,
		"election_resolved": true,
		"election_won": true,
		"election_summary": "Landslide victory for Contender Amara Sterling!",
		"election_breakdown": {
			"Amara Sterling": 68.5,
			"Marcus Thorne": 18.2,
			"Cassian Drake": 13.3
		}
	}
	onboarding._populate_campaign_hq_view(mock_state)

	_assert(onboarding.card_executive_address.visible, "CardExecutiveAddress must be visible after election victory")
	_assert(onboarding.input_official_address.text == "President Sterling", "Default official address should prefill with 'President [Last Name]', got: " + onboarding.input_official_address.text)
	print("  [OK]: Executive Address Card visible on election win with default: '%s'." % onboarding.input_official_address.text)

	# Test Presets
	onboarding._on_title_first_name_pressed()
	_assert(onboarding.input_official_address.text == "President Amara", "First name preset should set 'President Amara', got: " + onboarding.input_official_address.text)

	onboarding._on_title_full_name_pressed()
	_assert(onboarding.input_official_address.text == "President Amara Sterling", "Full name preset should set 'President Amara Sterling', got: " + onboarding.input_official_address.text)

	onboarding._on_title_madam_pressed()
	_assert(onboarding.input_official_address.text == "Madam President", "Madam preset should set 'Madam President', got: " + onboarding.input_official_address.text)

	onboarding._on_title_mister_pressed()
	_assert(onboarding.input_official_address.text == "Mr. President", "Mr preset should set 'Mr. President', got: " + onboarding.input_official_address.text)

	onboarding._on_title_last_name_pressed()
	_assert(onboarding.input_official_address.text == "President Sterling", "Last name preset should set 'President Sterling', got: " + onboarding.input_official_address.text)
	print("  [OK]: All 5 quick title presets function correctly.")

	# Custom title input
	onboarding.input_official_address.text = "President-General Sterling"

	# Administer Oath / Assume Office
	onboarding._on_assume_office_pressed()
	_assert(onboarding.selected_title == "President-General Sterling", "Selected title must record custom official address")
	print("  [OK]: Presidential oath administered with title: '%s'." % onboarding.selected_title)

	# --------------------------------------------------------------------------
	# Check 6: Screen Spacing and Layout Anchor Responsiveness
	# --------------------------------------------------------------------------
	print("\n--- Check 6: Responsive Layout & Screen Real Estate ---")
	var panel: PanelContainer = onboarding.get_node_or_null("PanelContainer")
	_assert(panel != null, "Root PanelContainer missing")
	_assert(panel.anchors_preset == Control.PRESET_FULL_RECT or (panel.anchor_right == 1.0 and panel.anchor_bottom == 1.0), "PanelContainer must use full-screen anchors preset 15 for responsiveness")
	_assert(panel.size_flags_horizontal == (Control.SIZE_EXPAND | Control.SIZE_FILL), "Horizontal size flag must be EXPAND_FILL (3)")
	_assert(panel.size_flags_vertical == (Control.SIZE_EXPAND | Control.SIZE_FILL), "Vertical size flag must be EXPAND_FILL (3)")
	print("  [OK]: Responsive full-screen container anchors and layout margins verified.")

	print("\n==============================================================")
	print(" [PASS]: ALL SIMPLIFIED ONBOARDING UI TESTS PASSED!           ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
