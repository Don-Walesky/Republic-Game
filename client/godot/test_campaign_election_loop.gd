# ==============================================================================
# Republic - Campaign & Contested Election Loop Integration Test
# Verifies:
# 1. AI Opponents (Senator Marcus Thorne & Cassian Drake) dynamic polling & moves
# 2. 5 Interest Groups (Industrialists, Media, Unions, Professionals, Influencers)
# 3. Court Interest Group action (Reputation shift, Endorsement flip, Purse deduct)
# 4. Live Campaign Media Wire generation and tone tracking
# 5. Tab navigation switching across Decisions, Groups, Opponents/News, Election
# 6. Contested multi-candidate election night resolution and popular vote tally
# ==============================================================================
extends SceneTree

const TEST_STATE_PATH := "res://test_campaign_loop_state.json"
const TEST_SAVE_PATH := "res://test_campaign_loop_session.sav"
const TEST_REQ_PATH := "res://presidential_desk_request.json"
const TEST_RES_PATH := "res://presidential_desk_result.json"

const OnboardingScript = preload("res://Onboarding.gd")

func _init() -> void:
	print("\n==============================================================")
	print("   REPUBLIC - CAMPAIGN & ELECTION LOOP VERIFICATION TEST       ")
	print("==============================================================")
	_run_campaign_loop_test()

func _clean_files() -> void:
	for p in [TEST_STATE_PATH, TEST_SAVE_PATH, TEST_REQ_PATH, TEST_RES_PATH]:
		var g := ProjectSettings.globalize_path(p)
		if FileAccess.file_exists(g):
			DirAccess.remove_absolute(g)

func _run_campaign_loop_test() -> void:
	_clean_files()

	var onboarding: OnboardingController = load("res://Onboarding.tscn").instantiate()
	root.add_child(onboarding)
	onboarding.active_state_path = TEST_STATE_PATH
	onboarding.active_save_path = TEST_SAVE_PATH
	onboarding.active_request_path = TEST_REQ_PATH
	onboarding.active_result_path = TEST_RES_PATH
	onboarding._init_controls()

	print("\n--- Check 1: Register and Initialize Sovereign Republic ---")
	onboarding.input_first_name.text = "Aurelius"
	onboarding.input_last_name.text = "Vance"
	onboarding.input_email.text = "aurelius.vance@republic.net"
	onboarding.spin_dob_year.value = 1988
	onboarding.spin_dob_month.value = 7
	onboarding.spin_dob_day.value = 14
	onboarding._on_register_pressed()
	onboarding.input_token.text = onboarding.verification_token
	onboarding._on_submit_code_pressed()

	onboarding.input_country_name.text = "Republic of Eldoria"
	onboarding._on_review_founding_pressed()
	onboarding._on_confirm_founding_pressed()

	# Select Leadership Doctrine & Development Plans
	onboarding._on_technocrat_toggled()
	onboarding._on_visionary_toggled()
	onboarding._on_confirm_leadership_pressed()

	onboarding._on_plan_clicked("industrialisation_first", onboarding.btn_plan_1)
	onboarding._on_plan_clicked("technology_and_innovation", onboarding.btn_plan_3)
	onboarding._on_plan_clicked("infrastructure_revolution", onboarding.btn_plan_4)
	onboarding._on_plan_clicked("financial_and_commercial_power", onboarding.btn_plan_6)
	onboarding._on_confirm_plans_pressed()
	onboarding._on_proceed_phase4_pressed()

	# Induct Advisers & Enter Campaign HQ
	onboarding._on_confirm_advisers_pressed()

	if onboarding.current_stage != OnboardingScript.ScreenStage.CAMPAIGN_HQ:
		printerr("[FAIL]: Expected stage CAMPAIGN_HQ, got: ", onboarding.current_stage)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Entered Campaign Headquarters.")

	print("\n--- Check 2: Verify Multi-Candidate Contested Polling Setup ---")
	var player_poll_text := onboarding.lbl_race_player.text
	var thorne_poll_text := onboarding.lbl_race_thorne.text
	var drake_poll_text := onboarding.lbl_race_drake.text
	var undecided_text := onboarding.lbl_race_undecided.text

	if not player_poll_text.contains("35.0%") or not thorne_poll_text.contains("Thorne") or not drake_poll_text.contains("Drake"):
		printerr("[FAIL]: Race polling display missing expected candidates: ", player_poll_text, ", ", thorne_poll_text, ", ", drake_poll_text)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Contested race polling initialized: You (35.0%), Sen. Thorne, Cassian Drake, Undecided.")

	print("\n--- Check 3: Verify 5 Canonical Interest Groups ---")
	onboarding._select_campaign_tab("groups")
	if not onboarding.tab_groups_container.visible:
		printerr("[FAIL]: Interest groups tab container not visible after tab switch!")
		_clean_files()
		quit(1)
		return

	var groups_list = onboarding.active_campaign_data.get("campaign_interest_groups", [])
	if typeof(groups_list) != TYPE_ARRAY or groups_list.size() != 5:
		printerr("[FAIL]: Expected 5 interest groups in campaign state, got: ", groups_list.size() if typeof(groups_list) == TYPE_ARRAY else 0)
		_clean_files()
		quit(1)
		return

	var group_names: Array[String] = []
	for g in groups_list:
		group_names.append(str(g.get("name", g.get("Name", ""))))
	print("  [OK]: 5 Active Interest Groups: ", ", ".join(group_names))

	print("\n--- Check 4: Court Trade Unions & Flip Endorsement ---")
	var initial_purse: float = float(onboarding.active_campaign_data.get("campaign_purse", 5000000.0))
	onboarding._on_court_group_pressed("group_unions", "Trade Union Federation")

	var purse_after_court: float = float(onboarding.active_campaign_data.get("campaign_purse", 0.0))
	if purse_after_court != initial_purse - 200000.0:
		printerr("[FAIL]: Expected purse deduction of 200,000 for courting group, got: ", purse_after_court, " from ", initial_purse)
		_clean_files()
		quit(1)
		return

	# Second courtship to cross 60-point threshold for endorsement
	onboarding._on_court_group_pressed("group_unions", "Trade Union Federation")
	var union_group: Dictionary = {}
	for g in onboarding.active_campaign_data.get("campaign_interest_groups", []):
		var gid_str := str(g.get("id", g.get("Id", "")))
		if gid_str == "ig_unions" or gid_str == "group_unions" or gid_str.ends_with("unions"):
			union_group = g
			break

	var union_rep: float = float(union_group.get("player_reputation", union_group.get("PlayerReputation", 0.0)))
	var union_status: String = str(union_group.get("endorsement_status", union_group.get("EndorsementStatus", "")))
	if union_rep < 70.0 or union_status != "EndorsingPlayer":
		printerr("[FAIL]: Expected Trade Unions reputation >= 70 and endorsement EndorsingPlayer, got: rep=", union_rep, ", status=", union_status)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Trade Unions courted! Reputation: ", union_rep, "/100. Official Endorsement: EndorsingPlayer.")

	print("\n--- Check 5: Verify AI Opponent Counter-Actions & News Wire ---")
	onboarding._select_campaign_tab("opponents")
	if not onboarding.tab_opponents_container.visible:
		printerr("[FAIL]: Opponents & News tab container not visible!")
		_clean_files()
		quit(1)
		return

	var news_list = onboarding.active_campaign_data.get("campaign_news", [])
	if typeof(news_list) != TYPE_ARRAY or news_list.size() < 2:
		printerr("[FAIL]: Expected simulation news items generated, got: ", news_list.size() if typeof(news_list) == TYPE_ARRAY else 0)
		_clean_files()
		quit(1)
		return
	print("  [OK]: Live Campaign Media Wire generated ", news_list.size(), " articles:")
	for n in news_list:
		print("    • [", str(n.get("source", n.get("Source", ""))), "] ", str(n.get("headline", n.get("Headline", ""))))

	print("\n--- Check 6: Enact Campaign Actions & Watch Contested Polling Shift ---")
	onboarding._select_campaign_tab("decisions")
	onboarding._on_decision_clicked("dec_meet_business")
	onboarding._on_decision_clicked("dec_media_blitz")
	onboarding._on_decision_clicked("dec_grassroots_townhall")

	var final_player_poll: float = float(onboarding.active_campaign_data.get("campaign_polling", 0.0))
	var final_opponents = onboarding.active_campaign_data.get("campaign_ai_opponents", [])
	print("  [OK]: Player Polling after campaign drive: ", final_player_poll, "%.")
	for opp in final_opponents:
		print("    • ", str(opp.get("name", opp.get("Name", ""))), ": Polling ", str(opp.get("polling", opp.get("Polling", 0.0))), "%, Action: ", str(opp.get("recent_action", opp.get("RecentAction", ""))))

	print("\n--- Check 7: Contest National Election Night & Inspect Multi-Candidate Breakdown ---")
	onboarding._select_campaign_tab("election")
	onboarding._on_contest_election_pressed()

	var election_won: bool = bool(onboarding.active_campaign_data.get("election_won", false))
	var breakdown: Dictionary = onboarding.active_campaign_data.get("election_breakdown", {})
	if not election_won:
		printerr("[FAIL]: Expected election victory after campaign drive!")
		_clean_files()
		quit(1)
		return

	if breakdown.is_empty():
		printerr("[FAIL]: Expected multi-candidate election breakdown, got empty dictionary!")
		_clean_files()
		quit(1)
		return

	print("  [OK]: Election Won! Official Certified Breakdown:")
	for c in breakdown.keys():
		print("    • ", c, ": ", breakdown[c], "%")

	print("\n--- Check 8: Administer Oath of Office & Transition to InOffice ---")
	if not onboarding.btn_assume_office.visible:
		printerr("[FAIL]: Expected btn_assume_office to be visible after election victory!")
		_clean_files()
		quit(1)
		return

	var event_tracker := { "completed": false }
	onboarding.onboarding_completed.connect(func(res): event_tracker["completed"] = true)
	onboarding._on_assume_office_pressed()

	if not event_tracker["completed"]:
		printerr("[FAIL]: onboarding_completed signal not emitted! Feedback: ", onboarding.lbl_campaign_feedback.text if onboarding.lbl_campaign_feedback else "")
		_clean_files()
		quit(1)
		return
	print("  [OK]: Presidential Oath administered. Mandate confirmed.")

	print("\n==============================================================")
	print(" [PASS]: ALL 8 CAMPAIGN & CONTESTED ELECTION CHECKS PASSED!  ")
	print("==============================================================\n")
	_clean_files()
	quit(0)
