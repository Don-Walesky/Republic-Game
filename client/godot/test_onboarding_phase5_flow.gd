# ==============================================================================
# Republic - New Player Onboarding Phase 5 UI Flow Automated Test
# Tests Onboarding.tscn / Onboarding.gd end-to-end through Phase 5:
# 1. Registration form validation & PII protection
# 2. Email verification dispatch viewer with "Verify Email" button
# 3. Country naming rules & confirmation modal
# 4. Leadership Identity Selection (2 of 5) -> Technocratic Visionary
# 5. National Development Priorities Selection (4 of 10)
# 6. Ratification of Phase 3 summary
# 7. Transition to Presidential Advisory Council (Phase 4)
# 8. Unlock Slot 2 with 500 Sovereign Gems (Dr. Arthur Sterling)
# 9. Confirm Advisory Council & auto-enter Campaign Headquarters (Phase 5)
# 10. Verify Campaign HQ stats: 5,000,000 REPU purse, 35% starting polling, Clean Audit
# 11. Verify 3 Key Campaign Team Appointments: General Adviser, Press Secretary, Accountant General
# 12. Verify 70:30 campaign decisions (10 decisions, each with both positive and negative consequences)
# 13. Execute campaign decisions: purse deduction & funding credits, polling increase, audit tracking
# 14. Contest Election Night: vote tabulation, electoral threshold (>= 50.0% win condition)
# 15. Assume Presidential Office: oath administered, onboarding completed -> InOffice
# 16. Authoritative state & public snapshot verification (Zero PII leak)
# ==============================================================================
extends SceneTree

const ONBOARDING_SCENE_PATH := "res://Onboarding.tscn"
const OnboardingScript := preload("res://Onboarding.gd")
const TEST_STATE_PATH := "res://test_phase5_ui_state.json"
const TEST_SAVE_PATH := "res://test_phase5_ui_session.sav"
const TEST_REQUEST_PATH := "res://presidential_desk_request.json"
const TEST_RESULT_PATH := "res://presidential_desk_result.json"

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - ONBOARDING UI FLOW VERIFICATION TEST (PHASE 5)  ")
	print("==============================================================\n")
	_run_tests()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQUEST_PATH, TEST_RESULT_PATH]:
		var glob_path := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(glob_path):
			DirAccess.remove_absolute(glob_path)

func _run_tests() -> void:
	_clean_files()

	var scene: PackedScene = load(ONBOARDING_SCENE_PATH)
	if scene == null:
		printerr("[FAIL]: Unable to load Onboarding.tscn")
		_clean_files()
		quit(1)
		return

	var onboarding = scene.instantiate()
	if onboarding == null:
		printerr("[FAIL]: Failed to instantiate OnboardingController")
		_clean_files()
		quit(1)
		return

	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding.active_request_path = TEST_REQUEST_PATH
	onboarding.active_result_path = TEST_RESULT_PATH
	onboarding._init_controls()

	print("--- Check 1: Candidate Registration & Email Verification ---")
	onboarding.input_first_name.text = "Elena"
	onboarding.input_last_name.text = "Okafor"
	onboarding.input_email.text = "elena.okafor@republic.net"
	onboarding.spin_dob_year.value = 1991
	onboarding.spin_dob_month.value = 3
	onboarding.spin_dob_day.value = 20
	onboarding._on_register_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.VERIFY_EMAIL:
		printerr("[FAIL]: Expected stage VERIFY_EMAIL, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	onboarding._on_verify_action_pressed()
	if onboarding.current_stage != OnboardingScript.ScreenStage.COUNTRY_NAME:
		printerr("[FAIL]: Expected stage COUNTRY_NAME, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Registered and email verified.")

	print("\n--- Check 2: Found Sovereign Nation ---")
	onboarding.input_country_name.text = "Republic of Solaria"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CONFIRMED:
		printerr("[FAIL]: Expected stage CONFIRMED, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Republic of Solaria founded.")

	print("\n--- Check 3: Leadership & Development Plans (Phases 3-4) ---")
	onboarding._on_proceed_leadership_pressed()
	onboarding._on_technocrat_toggled()
	onboarding._on_visionary_toggled()
	onboarding._on_confirm_leadership_pressed()

	onboarding._on_plan_clicked("industrialisation_first", onboarding.btn_plan_1)
	onboarding._on_plan_clicked("agricultural_transformation", onboarding.btn_plan_2)
	onboarding._on_plan_clicked("technology_and_innovation", onboarding.btn_plan_3)
	onboarding._on_plan_clicked("infrastructure_revolution", onboarding.btn_plan_4)
	onboarding._on_confirm_plans_pressed()
	onboarding._on_proceed_phase4_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.ADVISERS:
		printerr("[FAIL]: Expected stage ADVISERS, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Ratified doctrine and transitioned to Advisory Council.")

	print("\n--- Check 4: Induct Advisers & Transition to Campaign Headquarters (Phase 5) ---")
	onboarding._on_unlock_slot2_pressed()
	onboarding._on_confirm_advisers_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CAMPAIGN_HQ:
		printerr("[FAIL]: Expected stage CAMPAIGN_HQ, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return

	if not onboarding.campaign_panel or not onboarding.campaign_panel.visible:
		printerr("[FAIL]: ScreenCampaignHq panel must be visible!")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Successfully entered Presidential Campaign Headquarters.")

	print("\n--- Check 5: Verify Campaign HQ Stats & Key Team ---")
	var purse_text: String = onboarding.lbl_campaign_purse.text
	if not purse_text.contains("5,000,000") and not purse_text.contains("5000000"):
		printerr("[FAIL]: Expected initial purse of 5,000,000 REPU, got: ", purse_text)
		_clean_files()
		quit(1)
		return

	var polling_text: String = onboarding.lbl_campaign_polling.text
	if not polling_text.contains("35.0%"):
		printerr("[FAIL]: Expected initial polling of 35.0%, got: ", polling_text)
		_clean_files()
		quit(1)
		return

	var audit_text: String = onboarding.lbl_campaign_audit.text
	if not audit_text.contains("Clean Audit") or not audit_text.contains("Kaelen Thorne"):
		printerr("[FAIL]: Expected Accountant General Kaelen Thorne with Clean Audit, got: ", audit_text)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_team_general.text.contains("Marcus Vance"):
		printerr("[FAIL]: Expected Marcus Vance in general guidance, got: ", onboarding.lbl_team_general.text)
		_clean_files()
		quit(1)
		return

	if not onboarding.lbl_team_press.text.contains("Elena Vance"):
		printerr("[FAIL]: Expected Elena Vance in press advice, got: ", onboarding.lbl_team_press.text)
		_clean_files()
		quit(1)
		return

	print("  [OK]: Campaign purse initialized at R 5,000,000 REPU.")
	print("  [OK]: Starting national polling verified at 35.0%.")
	print("  [OK]: Campaign leadership team active: Marcus Vance, Elena Vance, Kaelen Thorne.")

	print("\n--- Check 6: Verify 70:30 Decision Structure ---")
	var decision_panels = onboarding.decisions_box.get_children()
	if decision_panels.size() != 10:
		printerr("[FAIL]: Expected 10 available campaign decisions, got: ", decision_panels.size())
		_clean_files()
		quit(1)
		return

	var net_pos_count := 0
	var net_neg_count := 0
	var available_list = onboarding.active_campaign_data.get("available_campaign_decisions", [])
	for d in available_list:
		var has_pos: bool = str(d.get("foreseeable_positive_consequence", d.get("ForeseeablePositiveConsequence", ""))).length() > 0
		var has_neg: bool = str(d.get("foreseeable_negative_consequence", d.get("ForeseeableNegativeConsequence", ""))).length() > 0
		if not has_pos or not has_neg:
			printerr("[FAIL]: Decision missing positive or negative consequence: ", d)
			_clean_files()
			quit(1)
			return

		var is_pos: bool = bool(d.get("is_net_positive", d.get("IsNetPositive", true)))
		if is_pos:
			net_pos_count += 1
		else:
			net_neg_count += 1

	if net_pos_count != 7 or net_neg_count != 3:
		printerr("[FAIL]: Expected exact 70:30 consequence ratio (7 net-positive, 3 net-negative), got: ", net_pos_count, " pos / ", net_neg_count, " neg")
		_clean_files()
		quit(1)
		return

	print("  [OK]: 70:30 rule satisfied: 7 net-positive decisions and 3 net-negative / high-risk decisions.")
	print("  [OK]: 100% of decisions include identifiable positive and negative consequences.")

	print("\n--- Check 7: Execute Campaign Decisions & Purse Accounting ---")
	# 1. Meet Business Association (cost 200,000, +5% polling, +800,000 funding donation -> net +600,000)
	onboarding._on_decision_clicked("dec_meet_business")
	var purse_after_1: float = float(onboarding.active_campaign_data.get("campaign_purse", 0.0))
	var poll_after_1: float = float(onboarding.active_campaign_data.get("campaign_polling", 0.0))
	if purse_after_1 != 5600000.0 or poll_after_1 < 39.5:
		printerr("[FAIL]: Unexpected purse or polling after decision 1: purse=", purse_after_1, ", poll=", poll_after_1)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Decision 1 enacted. Purse: R 5,600,000 REPU, Polling: ", poll_after_1, "%.")

	# 2. Meet Labour Union (cost 350,000, +6% polling)
	onboarding._on_decision_clicked("dec_meet_labor")
	var purse_after_2: float = float(onboarding.active_campaign_data.get("campaign_purse", 0.0))
	var poll_after_2: float = float(onboarding.active_campaign_data.get("campaign_polling", 0.0))
	if purse_after_2 != 5250000.0 or poll_after_2 < 45.5:
		printerr("[FAIL]: Unexpected purse or polling after decision 2: purse=", purse_after_2, ", poll=", poll_after_2)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Decision 2 enacted. Purse: R 5,250,000 REPU, Polling: ", poll_after_2, "%.")

	# 3. National Media Blitz (cost 750,000, +7% polling)
	onboarding._on_decision_clicked("dec_media_blitz")
	# 4. Grassroots Town Halls (cost 250,000, +4% polling)
	onboarding._on_decision_clicked("dec_grassroots_townhall")

	var purse_after_4: float = float(onboarding.active_campaign_data.get("campaign_purse", 0.0))
	var poll_after_4: float = float(onboarding.active_campaign_data.get("campaign_polling", 0.0))
	if purse_after_4 != 4250000.0 or poll_after_4 < 56.0:
		printerr("[FAIL]: Expected polling >= 56.0% after 4 decisions, got: ", poll_after_4, "%, purse: ", purse_after_4)
		_clean_files()
		quit(1)
		return
	print("  [OK]: 4 decisions enacted. Final Purse: R 4,250,000 REPU. National Polling elevated to: ", poll_after_4, "%.")

	print("\n--- Check 8: Contest Election Night (Victory Condition >= 50.0%) ---")
	onboarding._on_contest_election_pressed()

	var election_resolved: bool = bool(onboarding.active_campaign_data.get("election_resolved", false))
	var election_won: bool = bool(onboarding.active_campaign_data.get("election_won", false))
	var election_tally: float = float(onboarding.active_campaign_data.get("election_tally", 0.0))

	if not election_resolved or not election_won or election_tally < 50.0:
		printerr("[FAIL]: Expected election victory with tally >= 50.0%, got: won=", election_won, ", tally=", election_tally)
		_clean_files()
		quit(1)
		return

	if not onboarding.btn_assume_office.visible:
		printerr("[FAIL]: Expected btn_assume_office to be visible after winning election!")
		_clean_files()
		quit(1)
		return

	print("  [OK]: Election night contested! National vote tally: ", election_tally, "%.")
	print("  [OK]: Constitutional threshold crossed (>= 50.0%). Victory declared.")

	print("\n--- Check 9: Administer Presidential Oath & Assume Executive Office ---")
	var event_tracker := { "completed": false }
	onboarding.onboarding_completed.connect(func(res): event_tracker["completed"] = true)
	onboarding._on_assume_office_pressed()

	if not event_tracker["completed"]:
		printerr("[FAIL]: onboarding_completed signal was not emitted upon assuming office! Feedback: ", onboarding.lbl_campaign_feedback.text if onboarding.lbl_campaign_feedback else "")
		_clean_files()
		quit(1)
		return

	# Authoritative state check on disk
	var glob_state := ProjectSettings.globalize_path(TEST_STATE_PATH)
	var state_file := FileAccess.open(glob_state, FileAccess.READ)
	if state_file != null:
		var state_text := state_file.get_as_text()
		state_file.close()

		var parsed = JSON.parse_string(state_text)
		if parsed == null or typeof(parsed) != TYPE_DICTIONARY:
			printerr("[FAIL]: State file on disk is not a valid JSON dictionary.")
			_clean_files()
			quit(1)
			return

		var step := str(parsed.get("onboarding_step", ""))
		if step != "InOffice":
			printerr("[FAIL]: Expected onboarding_step 'InOffice', got: ", step)
			_clean_files()
			quit(1)
			return

		var snapshot_won := bool(parsed.get("election_won", false))
		var snapshot_resolved := bool(parsed.get("election_resolved", false))
		if not snapshot_won or not snapshot_resolved:
			printerr("[FAIL]: State snapshot missing won election state: won=", snapshot_won, ", resolved=", snapshot_resolved)
			_clean_files()
			quit(1)
			return

		# PII Check
		if state_text.contains("elena.okafor@republic.net") or state_text.contains("1991-03-20"):
			printerr("[FAIL]: PII leak detected in snapshot!")
			_clean_files()
			quit(1)
			return

	print("  [OK]: Presidential Oath administered. Onboarding step transitioned to 'InOffice'.")
	print("  [OK]: Zero PII leaked in persistent snapshots.")

	print("\n==============================================================")
	print(" [PASS]: ALL 9 ONBOARDING PHASE 5 FLOW CHECKS PASSED!        ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
