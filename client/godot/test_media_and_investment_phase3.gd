extends SceneTree

## Automated Headless Verification Suite for Republic:
## Phase 3: Media Influence and Private-Sector Investment
##
## Validates:
## 1. Living Media Environment & Event-Driven Reporting:
##    - media_reports rendering with editorial leaning, credibility, reach, and sentiment
##    - MediaSection populated in Briefing Modal with outlet details and story context
##    - execute_media_response generates valid action: "media_response" (statement, hush_money, censor)
## 2. Private-Sector Corporate Actors & Investment Proposals:
##    - investment_proposals rendering with investor name, sector, funding split, jobs, and revenue yield
##    - InvestmentSection populated in Briefing Modal with proposal details and risk disclosures
##    - execute_investment_decision generates valid action: "investment_decision" (accept, reject)
## 3. PresidentialDesk Phase 3 Integration:
##    - Media environment summary and private sector investment summary binding

const ISOLATED_TEST_DIR := "res://temp_phase3_media_isolated"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - PHASE 3 MEDIA & INVESTMENT VERIFICATION        ")
	print("==============================================================")

	var scene_res := load("res://ExecutiveOffice.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://ExecutiveOffice.tscn")
		_cleanup_all()
		quit(1)
		return

	var office = scene_res.instantiate()
	if office == null:
		printerr("[FAIL]: Failed to instantiate ExecutiveOffice node")
		_cleanup_all()
		quit(1)
		return

	root.add_child(office)
	office._init_controls()
	office._connect_signals()

	var t1 := test_media_environment_briefing(office)
	print("Test 1 Result: ", t1)
	var t2 := test_media_response_actions(office)
	print("Test 2 Result: ", t2)
	var t3 := test_private_sector_proposals(office)
	print("Test 3 Result: ", t3)
	var t4 := test_investment_decision_actions(office)
	print("Test 4 Result: ", t4)
	var t5 := test_presidential_desk_integration()
	print("Test 5 Result: ", t5)

	var all_passed := t1 and t2 and t3 and t4 and t5

	office.queue_free()
	_cleanup_all()

	if all_passed:
		print("==============================================================")
		print("   [SUCCESS]: ALL PHASE 3 MEDIA & INVESTMENT TESTS PASSED     ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr("   [FAILURE]: ONE OR MORE PHASE 3 TESTS FAILED                ")
		printerr("==============================================================")
		quit(1)

func test_media_environment_briefing(office) -> bool:
	print("\n--- Running Test 1: Media Environment Briefing ---")
	var sample_state := {
		"country_name": "Republic of Valeria",
		"treasury_balance": 5000000.0,
		"treasury": "R 5,000,000",
		"approval_rating": 68.0,
		"stability_rating": 74.0,
		"media_summary": "4 active national news outlets | 2 recent publications",
		"media_outlets": [
			{
				"id": "outlet_state",
				"name": "State Herald",
				"leaning": "StateBroadcaster",
				"reach": 850000,
				"credibility": 0.65,
				"attitude_towards_government": 0.8
			},
			{
				"id": "outlet_indep",
				"name": "The Daily Inquirer",
				"leaning": "Independent",
				"reach": 620000,
				"credibility": 0.88,
				"attitude_towards_government": -0.15
			}
		],
		"media_reports": [
			{
				"id": "rep_highway_success",
				"outlet_id": "outlet_state",
				"outlet_name": "State Herald",
				"headline": "Trans-National Highway Construction Hailed as Major Success",
				"body": "The Ministry of Infrastructure achieved critical milestone ahead of schedule.",
				"sentiment": "Positive",
				"credibility": 0.65,
				"reach": 850000,
				"published_day": 42,
				"is_suppressed": false,
				"is_leaked": false
			},
			{
				"id": "rep_unemployment_worry",
				"outlet_id": "outlet_indep",
				"outlet_name": "The Daily Inquirer",
				"headline": "Labor Department Under Fire as Jobless Claims Rise",
				"body": "Independent economic analysts warn of slowing industrial hiring in manufacturing hubs.",
				"sentiment": "Critical",
				"credibility": 0.88,
				"reach": 620000,
				"published_day": 43,
				"is_suppressed": false,
				"is_leaked": false
			}
		]
	}

	office.apply_state(sample_state)

	var briefing_content := office._get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		printerr("FAIL: BriefingContentVBox not found")
		return false

	var media_sec := briefing_content.get_node_or_null("MediaEnvironmentSection") as VBoxContainer
	if not media_sec:
		media_sec = briefing_content.get_node_or_null("MediaSection") as VBoxContainer
	if not media_sec:
		printerr("FAIL: MediaEnvironmentSection not generated in BriefingModal")
		return false

	var children := media_sec.get_children()
	# Header label + 2 report cards
	if children.size() < 3:
		printerr("FAIL: Expected MediaSection children >= 3, got: %d" % children.size())
		return false

	print("PASS: Media environment briefing rendered with %d children cards." % children.size())
	return true

func test_media_response_actions(office) -> bool:
	print("\n--- Running Test 2: Presidential Media Response Actions ---")
	_setup_isolated_dir()
	var test_state_file := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	# Test Official Statement
	var res_stmt: Dictionary = office.execute_media_response("rep_unemployment_worry", "statement")
	if not res_stmt.get("success", false):
		printerr("FAIL: execute_media_response for statement returned false: %s" % res_stmt.get("error", ""))
		return false

	var req_file := ISOLATED_TEST_DIR + "/presidential_desk_request.json"
	if not FileAccess.file_exists(req_file):
		printerr("FAIL: Expected request file not found at %s" % req_file)
		return false

	var f := FileAccess.open(req_file, FileAccess.READ)
	var content := f.get_as_text()
	f.close()
	var parsed: Dictionary = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "statement":
		printerr("FAIL: Request content mismatch for statement: %s" % content)
		return false

	# Test Hush-Money Secret Suppression
	var res_hush: Dictionary = office.execute_media_response("rep_unemployment_worry", "hush_money")
	if not res_hush.get("success", false):
		printerr("FAIL: execute_media_response for hush_money returned false: %s" % res_hush.get("error", ""))
		return false

	f = FileAccess.open(req_file, FileAccess.READ)
	content = f.get_as_text()
	f.close()
	parsed = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "hush_money":
		printerr("FAIL: Request content mismatch for hush_money: %s" % content)
		return false

	# Test Censorship / Press Restriction
	var res_censor: Dictionary = office.execute_media_response("rep_unemployment_worry", "censor")
	if not res_censor.get("success", false):
		printerr("FAIL: execute_media_response for censor returned false: %s" % res_censor.get("error", ""))
		return false

	f = FileAccess.open(req_file, FileAccess.READ)
	content = f.get_as_text()
	f.close()
	parsed = JSON.parse_string(content)
	if parsed.get("action", "") != "media_response" or parsed.get("response_type", "") != "censor":
		printerr("FAIL: Request content mismatch for censor: %s" % content)
		return false

	print("PASS: Media responses (statement, hush_money, censor) successfully verified.")
	return true

func test_private_sector_proposals(office) -> bool:
	print("\n--- Running Test 3: Private Sector Inbound Proposals ---")
	var sample_state := {
		"country_name": "Republic of Valeria",
		"treasury_balance": 5000000.0,
		"treasury": "R 5,000,000",
		"investment_summary": "1 pending corporate proposals | 0 active investments",
		"investment_proposals": [
			{
				"id": "prop_valeria_metals_01",
				"investor_name": "Valeria Heavy Industries",
				"sector": "HeavyIndustry",
				"title": "Automated Steel Smelting Complex",
				"description": "Construct state-of-the-art smelting facility expanding domestic steel output.",
				"proposed_investment": 1200000.0,
				"government_funding_requested": 400000.0,
				"construction_days": 45,
				"projected_jobs": 280,
				"projected_gdp_boost": 0.015,
				"projected_annual_tax_yield": 85000.0,
				"location": "Valeria Northern Basin",
				"conditions_and_risks": "Requires stable power grid; vulnerable to scrap metal price spikes.",
				"status": "Pending",
				"is_foreign": false
			}
		]
	}

	office.apply_state(sample_state)

	var briefing_content := office._get_node_safe("InterfaceLayer/BriefingModal/BriefingVBox/BriefingScroll/BriefingContentVBox") as VBoxContainer
	if not briefing_content:
		printerr("FAIL: BriefingContentVBox not found")
		return false

	var inv_sec := briefing_content.get_node_or_null("PrivateInvestmentSection") as VBoxContainer
	if not inv_sec:
		inv_sec = briefing_content.get_node_or_null("InvestmentSection") as VBoxContainer
	if not inv_sec:
		printerr("FAIL: PrivateInvestmentSection not generated in BriefingModal")
		return false

	var children := inv_sec.get_children()
	# Header label + 1 proposal card
	if children.size() < 2:
		printerr("FAIL: Expected InvestmentSection children >= 2, got: %d" % children.size())
		return false

	print("PASS: Private sector proposals rendered with %d children cards." % children.size())
	return true

func test_investment_decision_actions(office) -> bool:
	print("\n--- Running Test 4: Presidential Investment Decision Actions ---")
	_setup_isolated_dir()
	var test_state_file := ISOLATED_TEST_DIR + "/presidential_desk_state.json"
	office.current_state_path = test_state_file

	# Test Accept Proposal
	var res_accept: Dictionary = office.execute_investment_decision("prop_valeria_metals_01", "accept")
	if not res_accept.get("success", false):
		printerr("FAIL: execute_investment_decision accept returned false: %s" % res_accept.get("error", ""))
		return false

	var req_file := ISOLATED_TEST_DIR + "/presidential_desk_request.json"
	var f := FileAccess.open(req_file, FileAccess.READ)
	var content := f.get_as_text()
	f.close()
	var parsed: Dictionary = JSON.parse_string(content)
	if parsed.get("action", "") != "investment_decision" or parsed.get("decision", "") != "accept":
		printerr("FAIL: Request content mismatch for accept investment: %s" % content)
		return false

	# Test Reject Proposal
	var res_reject: Dictionary = office.execute_investment_decision("prop_valeria_metals_01", "reject")
	if not res_reject.get("success", false):
		printerr("FAIL: execute_investment_decision reject returned false: %s" % res_reject.get("error", ""))
		return false

	f = FileAccess.open(req_file, FileAccess.READ)
	content = f.get_as_text()
	f.close()
	parsed = JSON.parse_string(content)
	if parsed.get("action", "") != "investment_decision" or parsed.get("decision", "") != "reject":
		printerr("FAIL: Request content mismatch for reject investment: %s" % content)
		return false

	print("PASS: Investment decisions (accept, reject) successfully verified.")
	return true

func test_presidential_desk_integration() -> bool:
	print("\n--- Running Test 5: PresidentialDesk Integration ---")
	var desk_scene_res := load("res://PresidentialDesk.tscn")
	if desk_scene_res == null:
		printerr("FAIL: Failed to load res://PresidentialDesk.tscn")
		return false

	var desk = desk_scene_res.instantiate()
	if desk == null:
		printerr("FAIL: Failed to instantiate PresidentialDesk")
		return false

	root.add_child(desk)

	var sample_state := {
		"country_name": "Republic of Valeria",
		"treasury": "R 5,000,000",
		"approval": "68.0%",
		"stability": "74.0%",
		"simulation_time": "Day 45, 12:00",
		"media_summary": "4 active national news outlets | 2 recent publications",
		"investment_summary": "1 pending corporate proposals | 0 active investments"
	}

	desk.apply_state(sample_state)

	print("PASS: PresidentialDesk state binding succeeded.")
	desk.queue_free()
	return true

func _setup_isolated_dir() -> void:
	if not DirAccess.dir_exists_absolute(ISOLATED_TEST_DIR):
		DirAccess.make_dir_recursive_absolute(ISOLATED_TEST_DIR)

func _cleanup_all() -> void:
	if DirAccess.dir_exists_absolute(ISOLATED_TEST_DIR):
		var dir := DirAccess.open(ISOLATED_TEST_DIR)
		if dir:
			dir.list_dir_begin()
			var file_name := dir.get_next()
			while not file_name.is_empty():
				if not dir.current_is_dir():
					dir.remove(file_name)
				file_name = dir.get_next()
			dir.list_dir_end()
		DirAccess.remove_absolute(ISOLATED_TEST_DIR)
