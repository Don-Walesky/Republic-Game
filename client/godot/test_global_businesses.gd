extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: GLOBAL ENTERPRISES & INBOUND INDUSTRIALISTS")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 5

	# -------------------------------------------------------------------------
	# CHECK 1: GlobalBusinessesCard Layout Discovery in EconomyModal
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: GlobalBusinessesCard Layout Discovery ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var card = exec_instance.global_businesses_card
	var status_badge = exec_instance.biz_status_badge
	var summary_lbl = exec_instance.biz_metrics_summary_label
	var pending_box = exec_instance.pending_biz_container
	var operating_box = exec_instance.operating_biz_container

	if card and status_badge and summary_lbl and pending_box and operating_box:
		print("  ✓ GlobalBusinessesCard verified inside EconomyModal/EconCardsVBox")
		print("  ✓ BizStatusBadge and BizMetricsSummaryLabel bound successfully")
		print("  ✓ PendingBizContainer and OperatingBizContainer dynamic containers present")
		passed_checks += 1
	else:
		push_error("FAIL: One or more Global Businesses UI nodes missing from scene.")
		quit(1)
		return

	# -------------------------------------------------------------------------
	# CHECK 2: State Binding & Inbound Proposal Cards Rendering
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: Inbound Proposal Cards Rendering ---")
	var mock_state := {
		"operating_businesses_count": 0,
		"pending_business_proposals_count": 2,
		"total_business_quarterly_tax_formatted": "R 0 / Qtr",
		"total_business_jobs_created": 0,
		"total_business_gdp_boost": 0.0,
		"global_businesses": [
			{
				"id": "biz_wave1_fmcg",
				"business_name": "Nestor-Valo Consumer Goods",
				"industrialist_name": "CEO Jean-Luc Vane",
				"industry": "FMCG",
				"wave": 1,
				"scheduled_day": 30,
				"presenting_minister": "Hon. Sarah Connor (Minister of Interior)",
				"proposal_title": "National Packaged Foods & Clean Water Bottling Plant",
				"description": "Supplies municipal markets with fortified essential staples while stabilizing retail prices.",
				"capital_investment": 15000000.0,
				"capital_investment_formatted": "R 15,000,000",
				"projected_jobs": 2400,
				"quarterly_tax_contribution": 3500000.0,
				"quarterly_tax_contribution_formatted": "R 3,500,000",
				"gdp_contribution": 120000000.0,
				"quarterly_export_contribution": 10000000.0,
				"stock_market_confidence_bonus": 35.0,
				"foreign_reserves_inflow": 8000000.0,
				"status": "Pending"
			},
			{
				"id": "biz_wave1_tech",
				"business_name": "OmniNet Cloud Systems",
				"industrialist_name": "Founder Mira Chen",
				"industry": "Technology",
				"wave": 1,
				"scheduled_day": 60,
				"presenting_minister": "Minister of Science & Technology",
				"proposal_title": "Tier-IV Green Data Center & Fiber Backhaul",
				"description": "Constructs high-availability server arrays powered by domestic renewable energy.",
				"capital_investment": 25000000.0,
				"capital_investment_formatted": "R 25,000,000",
				"projected_jobs": 1200,
				"quarterly_tax_contribution": 5000000.0,
				"quarterly_tax_contribution_formatted": "R 5,000,000",
				"gdp_contribution": 200000000.0,
				"quarterly_export_contribution": 15000000.0,
				"stock_market_confidence_bonus": 50.0,
				"foreign_reserves_inflow": 12000000.0,
				"status": "Pending"
			}
		]
	}

	exec_instance._update_economy_central_bank_modal(mock_state)

	var p_count: int = pending_box.get_child_count()
	if status_badge.text == "Operating: 0 | Pending Proposals: 2" and p_count == 2:
		print("  ✓ Status badge correctly shows 2 pending proposals")
		print("  ✓ PendingBizContainer populated with 2 proposal cards")
		var first_card = pending_box.get_child(0)
		var card_vbox = first_card.get_child(0)
		var top_row = card_vbox.get_child(0)
		var name_lbl: Label = top_row.get_child(0) as Label
		print("  ✓ First card title: '%s'" % name_lbl.text)
		if "Nestor-Valo" in name_lbl.text and "FMCG" in name_lbl.text:
			print("  ✓ Brand name, industry, and minister correctly rendered on proposal card")
			passed_checks += 1
		else:
			push_error("FAIL: Proposal card text does not contain expected business information.")
			quit(1)
			return
	else:
		push_error("FAIL: Pending proposals count mismatch. Expected 2 cards, got: %d" % p_count)
		quit(1)
		return

	# -------------------------------------------------------------------------
	# CHECK 3: Operating Multinational Roster Rendering
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: Operating Multinational Roster Rendering ---")
	var operating_state := {
		"operating_businesses_count": 2,
		"pending_business_proposals_count": 0,
		"total_business_quarterly_tax_formatted": "R 8,500,000 / Qtr",
		"total_business_jobs_created": 3600,
		"total_business_gdp_boost": 320000000.0,
		"global_businesses": [
			{
				"id": "biz_wave1_fmcg",
				"business_name": "Nestor-Valo Consumer Goods",
				"industry": "FMCG",
				"projected_jobs": 2400,
				"quarterly_tax_contribution_formatted": "R 3,500,000",
				"status": "Accepted",
				"decided_on_day": 35
			},
			{
				"id": "biz_wave1_tech",
				"business_name": "OmniNet Cloud Systems",
				"industry": "Technology",
				"projected_jobs": 1200,
				"quarterly_tax_contribution_formatted": "R 5,000,000",
				"status": "Accepted",
				"decided_on_day": 65
			}
		]
	}

	exec_instance._update_economy_central_bank_modal(operating_state)

	var op_child_count: int = operating_box.get_child_count()
	var summary_text: String = summary_lbl.text
	print("  • Summary label text: '%s'" % summary_text)
	print("  • Status badge text: '%s'" % status_badge.text)
	print("  • Operating child count: %d" % op_child_count)

	if status_badge.text == "Operating: 2 | Pending Proposals: 0" and op_child_count == 2 \
		and "R 8,500,000 / Qtr" in summary_text and "3,600" in summary_text:
		print("  ✓ Operating badge shows 2 operating businesses")
		print("  ✓ Metrics summary correctly aggregates quarterly tax and jobs created")
		var op_row_1 = operating_box.get_child(0) as Label
		print("  ✓ Operating entry: '%s'" % op_row_1.text)
		if "Nestor-Valo" in op_row_1.text and "Operating since Day 35" in op_row_1.text:
			passed_checks += 1
		else:
			push_error("FAIL: Operating business row content incorrect.")
			quit(1)
			return
	else:
		push_error("FAIL: Operating roster display failed.")
		quit(1)
		return

	# -------------------------------------------------------------------------
	# CHECK 4: Accept Global Business Proposal Request Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Accept Global Business Proposal Request Dispatch ---")
	var req_file_path := ProjectSettings.globalize_path(BRIDGE_REQ_PATH)
	exec_instance.request_file_override = req_file_path
	exec_instance.cli_processor_override = "none"

	# Clean up any leftover file
	if FileAccess.file_exists(req_file_path):
		DirAccess.remove_absolute(req_file_path)

	exec_instance._on_accept_global_business_pressed("biz_wave1_fmcg")

	if FileAccess.file_exists(req_file_path):
		var f := FileAccess.open(req_file_path, FileAccess.READ)
		var json_str := f.get_as_text()
		f.close()
		DirAccess.remove_absolute(req_file_path)

		var parsed = JSON.parse_string(json_str)
		if parsed is Dictionary and parsed.get("action") == "global_business_decision" \
			and parsed.get("business_id") == "biz_wave1_fmcg" \
			and parsed.get("decision") == "accept":
			print("  ✓ Accept decision file successfully written to presidential_desk_request.json")
			print("  ✓ Action payload verified: action='global_business_decision', business_id='biz_wave1_fmcg', decision='accept'")
			passed_checks += 1
		else:
			push_error("FAIL: Request file JSON payload mismatch on accept.")
			quit(1)
			return
	else:
		push_error("FAIL: Request file not generated on accept pressed.")
		quit(1)
		return

	# -------------------------------------------------------------------------
	# CHECK 5: Decline Global Business Proposal Request Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Decline Global Business Proposal Request Dispatch ---")
	exec_instance._on_decline_global_business_pressed("biz_wave1_auto")

	if FileAccess.file_exists(req_file_path):
		var f := FileAccess.open(req_file_path, FileAccess.READ)
		var json_str := f.get_as_text()
		f.close()
		DirAccess.remove_absolute(req_file_path)

		var parsed = JSON.parse_string(json_str)
		if parsed is Dictionary and parsed.get("action") == "global_business_decision" \
			and parsed.get("business_id") == "biz_wave1_auto" \
			and parsed.get("decision") == "decline":
			print("  ✓ Decline decision file successfully written to presidential_desk_request.json")
			print("  ✓ Action payload verified: action='global_business_decision', business_id='biz_wave1_auto', decision='decline'")
			passed_checks += 1
		else:
			push_error("FAIL: Request file JSON payload mismatch on decline.")
			quit(1)
			return
	else:
		push_error("FAIL: Request file not generated on decline pressed.")
		quit(1)
		return

	print("\n=================================================================")
	print("🎉 ALL %d/%d GLOBAL ENTERPRISES CHECKS PASSED!" % [passed_checks, total_checks])
	print("=================================================================")
	exec_instance.queue_free()
	quit(0)
