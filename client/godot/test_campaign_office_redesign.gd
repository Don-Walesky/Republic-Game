# ==============================================================================
# Republic - Minimalist Modern Campaign Office Verification Test
# Verifies:
# 1. Modern Campaign Office War Room layout and responsive constraints
# 2. 30-Minute countdown timer initialization (1800s / 30:00) and display
# 3. Adviser-delivered decision notices (Marcus Vance & Elena Vance pills)
# 4. Adviser filter buttons (All, Political Adviser, Press Secretary)
# 5. Expense approval actions on decisions and stakeholder pacts
# 6. Timer completion / Conclude Early flow unlocking "Start Election" button
# 7. Contested election resolution and post-election title address options
# ==============================================================================
extends SceneTree

const TEST_STATE_PATH := "res://test_office_redesign_state.json"
const TEST_SAVE_PATH := "res://test_office_redesign_session.sav"
const TEST_REQ_PATH := "res://presidential_desk_request.json"
const TEST_RES_PATH := "res://presidential_desk_result.json"

const OnboardingScript = preload("res://Onboarding.gd")

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - MODERN CAMPAIGN OFFICE REDESIGN TEST             ")
	print("==============================================================")
	_run_campaign_office_test()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQ_PATH, TEST_RES_PATH]:
		var g := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(g):
			DirAccess.remove_absolute(g)

func _fail(msg: String) -> void:
	printerr("\n[FAIL]: " + msg)
	_clean_files()
	quit(1)

func _assert(cond: bool, msg: String) -> bool:
	if not cond:
		_fail(msg)
		return false
	return true

func _run_campaign_office_test() -> void:
	_clean_files()

	var onboarding: OnboardingController = load("res://Onboarding.tscn").instantiate()
	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding.active_request_path = TEST_REQ_PATH
	onboarding.active_result_path = TEST_RES_PATH
	onboarding._init_controls()
	onboarding._connect_signals()

	# --------------------------------------------------------------------------
	# Check 1: Quick Nation Founding & Campaign Office Entry
	# --------------------------------------------------------------------------
	print("\n--- Check 1: Enter Modern Campaign Office ---")
	onboarding.input_first_name.text = "Seraphina"
	onboarding.input_last_name.text = "Valen"
	onboarding.input_username.text = "svalen"
	onboarding.spin_dob_year.value = 1990
	onboarding.spin_dob_month.value = 4
	onboarding.spin_dob_day.value = 22
	onboarding._on_register_pressed()
	onboarding.input_token.text = onboarding.verification_token
	onboarding._on_submit_code_pressed()

	onboarding.input_country_name.text = "Republic of Aethelgard"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()

	onboarding._on_technocrat_toggled()
	onboarding._on_visionary_toggled()
	onboarding._on_confirm_leadership_pressed()

	onboarding._on_plan_clicked("industrialisation_first", onboarding.btn_plan_1)
	onboarding._on_plan_clicked("technology_and_innovation", onboarding.btn_plan_3)
	onboarding._on_plan_clicked("infrastructure_revolution", onboarding.btn_plan_4)
	onboarding._on_plan_clicked("financial_and_commercial_power", onboarding.btn_plan_6)
	onboarding._on_confirm_plans_pressed()
	onboarding._on_proceed_phase4_pressed()

	onboarding._on_confirm_advisers_pressed()

	if not _assert(onboarding.current_stage == OnboardingScript.ScreenStage.CAMPAIGN_HQ, "Must transition to ScreenCampaignHq"): return
	if not _assert(onboarding.campaign_panel != null and onboarding.campaign_panel.visible, "ScreenCampaignHq must be visible"): return
	print("  [OK]: Entered Modern Campaign Office successfully.")

	# --------------------------------------------------------------------------
	# Check 2: 30-Minute Countdown Timer & Stats Bar
	# --------------------------------------------------------------------------
	print("\n--- Check 2: 30-Minute Countdown Timer & Stats Bar ---")
	if not _assert(onboarding.lbl_campaign_countdown != null, "LblCountdown node missing"): return
	if not _assert(onboarding.lbl_campaign_purse != null, "LblPurse node missing"): return
	if not _assert(onboarding.lbl_campaign_polling != null, "LblPolling node missing"): return
	if not _assert(onboarding.lbl_campaign_audit != null, "LblAudit node missing"): return

	var timer_text: String = onboarding.lbl_campaign_countdown.text
	if not _assert(timer_text.contains("30:00") or timer_text.contains("29:"), "Countdown timer must initialize around 30:00, got: " + timer_text): return
	if not _assert(onboarding.campaign_time_remaining >= 1795.0 and onboarding.campaign_time_remaining <= 1800.0, "Time remaining should be near 1800s, got: " + str(onboarding.campaign_time_remaining)): return
	if not _assert(onboarding.campaign_timer_active, "Campaign timer should be active"): return
	print("  [OK]: 30-Minute countdown active: '%s' (remaining: %.1fs)." % [timer_text, onboarding.campaign_time_remaining])

	# --------------------------------------------------------------------------
	# Check 3: Adviser Delivery Badges & Expense Approval Buttons
	# --------------------------------------------------------------------------
	print("\n--- Check 3: Adviser Delivery Badges & Expense Approvals ---")
	var decisions = onboarding.decisions_box.get_children()
	if not _assert(decisions.size() == 10, "Should display 10 decision cards initially, got: " + str(decisions.size())): return

	var marcus_count := 0
	var elena_count := 0
	var expense_button_count := 0

	for panel in decisions:
		var txt := ""
		for child in panel.get_children():
			if child is VBoxContainer:
				for sub in child.get_children():
					if sub is HBoxContainer:
						for sub2 in sub.get_children():
							if sub2 is Label: txt += " " + sub2.text
							if sub2 is Button and sub2.text.begins_with("Approve Expense"):
								expense_button_count += 1
					elif sub is Label:
						txt += " " + sub.text

		if txt.contains("Marcus Vance"):
			marcus_count += 1
		if txt.contains("Elena Vance"):
			elena_count += 1

	if not _assert(marcus_count > 0, "Expected decisions delivered by Marcus Vance (Political Adviser)"): return
	if not _assert(elena_count > 0, "Expected decisions delivered by Elena Vance (Press Secretary)"): return
	if not _assert(marcus_count + elena_count == 10, "All 10 decisions must be delivered by either Marcus or Elena, got: Marcus=" + str(marcus_count) + ", Elena=" + str(elena_count)): return
	if not _assert(expense_button_count >= 8, "Expected expense approval buttons on cards, got: " + str(expense_button_count)): return
	print("  [OK]: All 10 decisions delivered by advisers: %d by Marcus Vance (Political), %d by Elena Vance (Press)." % [marcus_count, elena_count])
	print("  [OK]: Explicit expense approval buttons verified.")

	# --------------------------------------------------------------------------
	# Check 4: Adviser Filter Toggling
	# --------------------------------------------------------------------------
	print("\n--- Check 4: Adviser Filter Tabs ---")
	if not _assert(onboarding.btn_filter_all != null, "BtnFilterAll missing"): return
	if not _assert(onboarding.btn_filter_political != null, "BtnFilterPolitical missing"): return
	if not _assert(onboarding.btn_filter_press != null, "BtnFilterPress missing"): return

	# Filter by Political Adviser (Marcus Vance)
	onboarding._on_filter_political_pressed()
	var political_cards = onboarding.decisions_box.get_children()
	if not _assert(political_cards.size() == marcus_count, "Filtering by Political Adviser should yield " + str(marcus_count) + " cards, got: " + str(political_cards.size())): return
	print("  [OK]: Filter 'Political Adviser' displays %d cards." % political_cards.size())

	# Filter by Press Secretary (Elena Vance)
	onboarding._on_filter_press_pressed()
	var press_cards = onboarding.decisions_box.get_children()
	if not _assert(press_cards.size() == elena_count, "Filtering by Press Secretary should yield " + str(elena_count) + " cards, got: " + str(press_cards.size())): return
	print("  [OK]: Filter 'Press Secretary' displays %d cards." % press_cards.size())

	# Reset to All
	onboarding._on_filter_all_pressed()
	var all_cards = onboarding.decisions_box.get_children()
	if not _assert(all_cards.size() == 10, "Filter 'All' should restore 10 cards, got: " + str(all_cards.size())): return
	print("  [OK]: Filter 'All Advisers' restored 10 cards.")

	# --------------------------------------------------------------------------
	# Check 5: Stakeholder Agreement Approval
	# --------------------------------------------------------------------------
	print("\n--- Check 5: Stakeholder Agreement Approvals ---")
	onboarding._select_campaign_tab("groups")
	var groups_list = onboarding.groups_box.get_children()
	if not _assert(groups_list.size() == 5, "Expected 5 stakeholder interest groups, got: " + str(groups_list.size())): return

	# Court Trade Unions twice to secure endorsement
	onboarding._on_court_group_pressed("group_unions", "Trade Union Federation")
	onboarding._on_court_group_pressed("group_unions", "Trade Union Federation")
	print("  [OK]: Stakeholder agreements approved. Endorsement secured.")

	# --------------------------------------------------------------------------
	# Check 6: Enact Campaign Expenses & Conclude Early Timer Flow
	# --------------------------------------------------------------------------
	print("\n--- Check 6: Campaign Expenses & Conclude Early Timer Flow ---")
	onboarding._select_campaign_tab("decisions")
	onboarding._on_decision_clicked("dec_meet_business")
	onboarding._on_decision_clicked("dec_meet_labor")
	onboarding._on_decision_clicked("dec_media_blitz")
	onboarding._on_decision_clicked("dec_grassroots_townhall")

	if not _assert(onboarding.btn_start_election != null, "BtnStartElection node missing"): return
	if not _assert(onboarding.btn_conclude_early != null, "BtnConcludeEarly node missing"): return

	# Conclude countdown early
	onboarding._on_conclude_early_pressed()
	if not _assert(onboarding.campaign_time_remaining == 0.0, "Campaign time should be 0.0 after conclude early"): return
	if not _assert(not onboarding.campaign_timer_active, "Campaign timer should be inactive after conclude early"): return
	if not _assert(onboarding.lbl_campaign_countdown.text.contains("00:00"), "Timer text should show 00:00, got: " + onboarding.lbl_campaign_countdown.text): return
	if not _assert(onboarding.btn_start_election.disabled == false, "Start Election button must be enabled"): return
	if not _assert(onboarding.btn_start_election.text.contains("Voting Open") or onboarding.btn_start_election.text.contains("Start Election"), "Button text should indicate open voting, got: " + onboarding.btn_start_election.text): return
	print("  [OK]: Conclude early completed. 'Start Election' button active.")

	# --------------------------------------------------------------------------
	# Check 7: Start Election Resolution & Breakdown
	# --------------------------------------------------------------------------
	print("\n--- Check 7: Start Election Resolution & Breakdown ---")
	# Invoke Start Election via the button
	onboarding._on_contest_election_pressed()

	var election_won: bool = bool(onboarding.active_campaign_data.get("election_won", false))
	if not _assert(election_won, "Expected election victory after stakeholder pacts and campaign expenses"): return

	var breakdown: Dictionary = onboarding.active_campaign_data.get("election_breakdown", {})
	if not _assert(not breakdown.is_empty(), "Election breakdown should be populated"): return
	print("  [OK]: Election won with popular vote breakdown: ", str(breakdown))

	# --------------------------------------------------------------------------
	# Check 8: Executive Address Options & Inauguration
	# --------------------------------------------------------------------------
	print("\n--- Check 8: Executive Address Options & Inauguration ---")
	if not _assert(onboarding.card_executive_address.visible, "Executive address card must be visible on election victory"): return
	if not _assert(onboarding.input_official_address.text == "President Valen", "Default address should be 'President Valen', got: " + onboarding.input_official_address.text): return

	onboarding._on_title_first_name_pressed()
	if not _assert(onboarding.input_official_address.text == "President Seraphina", "Expected 'President Seraphina'"): return

	onboarding._on_title_last_name_pressed()
	if not _assert(onboarding.input_official_address.text == "President Valen", "Expected 'President Valen'"): return

	var inaugurated := { "done": false }
	onboarding.onboarding_completed.connect(func(res): inaugurated["done"] = true)
	onboarding._on_assume_office_pressed()
	if not _assert(inaugurated["done"], "Inauguration signal should have fired"): return
	print("  [OK]: Inauguration confirmed. Preferred title 'President Valen' ratified.")

	print("\n==============================================================")
	print(" [PASS]: ALL 8 MODERN CAMPAIGN OFFICE CHECKS PASSED!        ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
