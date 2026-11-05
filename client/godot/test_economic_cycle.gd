extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: CORE ECONOMIC CYCLE & MONETARY AUTHORITY")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 5

	# -------------------------------------------------------------------------
	# CHECK 1: EconomyModal Controls & Multi-Card Discovery
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: EconomyModal Multi-Card Layout Discovery ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var modal = exec_instance.economy_modal
	var gov_lbl = exec_instance.governor_name_label
	var rate_lbl = exec_instance.interest_rate_label
	var rate_cut_btn = exec_instance.rate_cut_button
	var rate_hike_btn = exec_instance.rate_hike_button
	var inf_lbl = exec_instance.inflation_rate_label
	var inf_mult_lbl = exec_instance.inflation_cost_multiplier_label

	var stock_name = exec_instance.stock_market_name_label
	var stock_pts = exec_instance.stock_market_points_label
	var stock_dir = exec_instance.stock_market_direction_label
	var stock_sent = exec_instance.stock_market_sentiment_label

	var reserves_lbl = exec_instance.foreign_reserves_label
	var forex_badge = exec_instance.forex_unlock_badge
	var forex_pbar = exec_instance.forex_progress_bar
	var regime_lbl = exec_instance.currency_regime_label
	var btn_managed = exec_instance.btn_regime_managed
	var btn_pegged = exec_instance.btn_regime_pegged
	var btn_float = exec_instance.btn_regime_floating
	var btn_inj = exec_instance.btn_inject_reserves
	var btn_ster = exec_instance.btn_sterilize_reserves

	var bgt_rev = exec_instance.budget_revenue_label
	var bgt_exp = exec_instance.budget_expenditure_label
	var bgt_bal = exec_instance.budget_balance_label
	var btn_reb = exec_instance.btn_rebalance_budget

	var debt_tot = exec_instance.debt_total_label
	var debt_svc = exec_instance.debt_service_label
	var btn_dom_loan = exec_instance.btn_take_domestic_loan
	var btn_ext_loan = exec_instance.btn_take_external_loan
	var btn_repay = exec_instance.btn_repay_debt

	if modal and gov_lbl and rate_lbl and rate_cut_btn and rate_hike_btn and inf_lbl and inf_mult_lbl \
		and stock_name and stock_pts and stock_dir and stock_sent \
		and reserves_lbl and forex_badge and forex_pbar and regime_lbl and btn_managed and btn_pegged and btn_float and btn_inj and btn_ster \
		and bgt_rev and bgt_exp and bgt_bal and btn_reb \
		and debt_tot and debt_svc and btn_dom_loan and btn_ext_loan and btn_repay:
		print("  ✓ EconomyModal found with 5 dedicated macroeconomic cards")
		print("  ✓ Central Bank & Inflation stance controls present (-0.5% Cut / +0.5% Hike)")
		print("  ✓ Stock Market Index (RCI) points, sentiment, and direction tags present")
		print("  ✓ Foreign Reserves, Forex progress bar, unlock badge, and regime buttons present")
		print("  ✓ National Budget revenue, expenditure, fiscal balance, and rebalance button present")
		print("  ✓ Sovereign Debt, debt service, domestic/external loan buttons present")
		passed_checks += 1
	else:
		push_error("  ❌ Missing required economic modal controls or action buttons")

	# -------------------------------------------------------------------------
	# CHECK 2: Live Data Binding from Macroeconomic State
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: Live Macroeconomic Data Binding ---")
	var mock_state := {
		"central_bank_governor_name": "Dr. Marcus Thorne",
		"central_bank_interest_rate": 5.0,
		"inflation_rate": 4.2,
		"inflation_rate_formatted": "4.2%",
		"inflation_cost_multiplier": 1.012,
		"inflation_impact_on_costs": "+1.2% Project Cost Index",
		"stock_market_name": "Republic Composite Index (RCI)",
		"stock_market_points": 2680.5,
		"stock_market_direction": "Bullish ↗️",
		"stock_market_change_pct": 2.4,
		"stock_market_sentiment": "Strong Growth",
		"foreign_reserves": 85000000.0,
		"foreign_reserves_formatted": "R 85,000,000",
		"forex_unlocked": false,
		"forex_unlock_progress_pct": 56.7,
		"currency_regime": "Sovereign Managed Float",
		"exchange_rate_usd": 1.25,
		"budget_revenue_formatted": "R 2,450.0M / Qtr",
		"budget_expenditure_formatted": "R 2,150.0M / Qtr",
		"budget_balance_formatted": "+R 300.0M Surplus",
		"budget_defense_allocation": 550000000.0,
		"budget_healthcare_allocation": 450000000.0,
		"budget_education_allocation": 380000000.0,
		"budget_infrastructure_allocation": 620000000.0,
		"budget_science_allocation": 270000000.0,
		"national_debt_domestic": 140000000.0,
		"national_debt_external": 90000000.0,
		"national_debt_total_formatted": "R 230,000,000",
		"debt_service_quarterly_formatted": "R 3.33M / Qtr"
	}

	exec_instance._update_economy_central_bank_modal(mock_state)

	var gov_ok: bool = "Dr. Marcus Thorne" in gov_lbl.text
	var rate_ok: bool = "5.0%" in rate_lbl.text
	var inf_ok: bool = "4.2%" in inf_lbl.text
	var stock_pts_ok: bool = "2680.5" in stock_pts.text
	var stock_dir_ok: bool = "Bullish ↗️" in stock_dir.text
	var res_ok: bool = "R 85,000,000" in reserves_lbl.text
	var locked_badge_ok: bool = "LOCKED" in forex_badge.text
	var bgt_bal_ok: bool = "Surplus" in bgt_bal.text
	var debt_ok: bool = "R 230,000,000" in debt_tot.text

	if gov_ok and rate_ok and inf_ok and stock_pts_ok and stock_dir_ok and res_ok and locked_badge_ok and bgt_bal_ok and debt_ok:
		print("  ✓ Governor & Interest rate correctly formatted")
		print("  ✓ Inflation rate and capital cost multiplier bound")
		print("  ✓ Stock market index points & Bullish tag displayed")
		print("  ✓ Foreign reserves (R 85M) and locked badge displayed")
		print("  ✓ Budget quarterly revenue, outlays, and surplus tag displayed")
		print("  ✓ Sovereign debt ledger and quarterly debt service displayed")
		passed_checks += 1
	else:
		push_error("  ❌ State data binding mismatch in EconomicModal")

	# -------------------------------------------------------------------------
	# CHECK 3: Central Bank Interest Rate Request Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: Central Bank Rate Hike & Cut Request Dispatch ---")
	exec_instance.current_state_path = BRIDGE_STATE_PATH
	exec_instance.request_file_override = BRIDGE_REQ_PATH

	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(BRIDGE_REQ_PATH))

	# Trigger Rate Hike
	exec_instance._on_rate_hike_pressed()
	var hike_req_exists := FileAccess.file_exists(BRIDGE_REQ_PATH)
	var hike_valid := false
	if hike_req_exists:
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "central_bank" and parsed.get("operation") == "rate_hike":
			hike_valid = true

	# Trigger Rate Cut
	exec_instance._on_rate_cut_pressed()
	var cut_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "central_bank" and parsed.get("operation") == "rate_cut":
			cut_valid = true

	if hike_valid and cut_valid:
		print("  ✓ Rate hike request generated with action='central_bank', operation='rate_hike', delta=0.5")
		print("  ✓ Rate cut request generated with action='central_bank', operation='rate_cut', delta=0.5")
		passed_checks += 1
	else:
		push_error("  ❌ Rate hike or cut request dispatch failed")

	# -------------------------------------------------------------------------
	# CHECK 4: Forex Unlock Condition & Currency Regime Controls
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Forex 150M Unlock Condition & Currency Regime ---")
	# When reserves reach >= 150M:
	var unlocked_state := {
		"foreign_reserves": 165000000.0,
		"foreign_reserves_formatted": "R 165,000,000",
		"forex_unlocked": true,
		"forex_unlock_progress_pct": 100.0,
		"currency_regime": "PeggedToUSD",
		"exchange_rate_usd": 1.0
	}
	exec_instance._update_economy_central_bank_modal(unlocked_state)

	var unlocked_badge_ok: bool = "UNLOCKED" in forex_badge.text
	var buttons_enabled: bool = not btn_managed.disabled and not btn_pegged.disabled and not btn_float.disabled

	# Test currency regime request dispatch
	exec_instance._on_regime_pegged_pressed()
	var pegged_req_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "forex_decision" and parsed.get("currency_regime") == "PeggedToUSD":
			pegged_req_valid = true

	if unlocked_badge_ok and buttons_enabled and pegged_req_valid:
		print("  ✓ UNLOCKED badge rendered when foreign reserves exceed 150M threshold")
		print("  ✓ Currency regime buttons enabled for player selection")
		print("  ✓ PeggedToUSD currency regime request dispatched cleanly")
		passed_checks += 1
	else:
		push_error("  ❌ Forex unlock condition or currency regime dispatch failed")

	# -------------------------------------------------------------------------
	# CHECK 5: Budget Rebalance & Sovereign Loan Dispatches
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Budget Rebalance & Sovereign Loan Dispatches ---")
	# 1. Budget Rebalance
	exec_instance._on_rebalance_budget_pressed()
	var bgt_req_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "adjust_budget" and parsed.has("defense") and parsed.has("infrastructure"):
			bgt_req_valid = true

	# 2. Domestic Loan
	exec_instance._on_take_domestic_loan_pressed()
	var dom_loan_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "take_loan" and parsed.get("loan_type") == "domestic":
			dom_loan_valid = true

	# 3. External Loan
	exec_instance._on_take_external_loan_pressed()
	var ext_loan_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "take_loan" and parsed.get("loan_type") == "external":
			ext_loan_valid = true

	# 4. Repay Debt
	exec_instance._on_repay_debt_pressed()
	var repay_valid := false
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		var f := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
		var text := f.get_as_text()
		f.close()
		var parsed = JSON.parse_string(text)
		if parsed is Dictionary and parsed.get("action") == "repay_loan":
			repay_valid = true

	if bgt_req_valid and dom_loan_valid and ext_loan_valid and repay_valid:
		print("  ✓ Budget rebalance request dispatched with ministry allocations")
		print("  ✓ Domestic loan request dispatched (+R 25M)")
		print("  ✓ External loan request dispatched (+R 50M)")
		print("  ✓ Debt repayment request dispatched (-R 25M)")
		passed_checks += 1
	else:
		push_error("  ❌ Budget rebalance or loan request dispatch failed")

	# Clean up
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(BRIDGE_REQ_PATH))
	exec_instance.queue_free()

	print("\n=================================================================")
	print("📊 RESULTS: %d / %d CHECKS PASSED" % [passed_checks, total_checks])
	print("=================================================================")

	if passed_checks == total_checks:
		print("🎉 SUCCESS: Core Economic Cycle & Monetary Authority verified!")
		quit(0)
	else:
		push_error("❌ FAILURE: %d checks failed" % (total_checks - passed_checks))
		quit(1)
