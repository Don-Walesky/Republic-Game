extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: MINISTERIAL APPOINTMENTS & FINANCIAL MANAGEMENT")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 5

	# -------------------------------------------------------------------------
	# CHECK 1: UI Nodes Discovery & Layout Integration
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: UI Nodes Discovery & Layout Integration ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	# Appointment Timer Card Nodes
	var timer_card = exec_instance.appointment_timer_card
	var timer_badge = exec_instance.appointment_timer_badge
	var progress_bar = exec_instance.appointment_progress_bar
	var details_label = exec_instance.appointment_details_label

	# Ongoing Expenses Card Nodes
	var expenses_card = exec_instance.ongoing_expenses_card
	var total_expenses_label = exec_instance.ongoing_expenses_total_label
	var salaries_label = exec_instance.salaries_expense_label
	var governance_label = exec_instance.governance_expense_label
	var maintenance_label = exec_instance.maintenance_expense_label

	# Revenue Activities Card Nodes
	var rev_card = exec_instance.revenue_activities_card
	var rev_badge = exec_instance.revenue_activities_boost_badge
	var rev_container = exec_instance.revenue_activities_container

	# Finance Minister Advisory Card Nodes
	var fm_card = exec_instance.finance_minister_advisory_card
	var fm_badge = exec_instance.finance_minister_active_badge
	var fm_headline = exec_instance.finance_minister_headline_label
	var fm_exp_advice = exec_instance.finance_expenditure_advice_label
	var fm_rev_advice = exec_instance.finance_revenue_advice_label
	var fm_loan_rec = exec_instance.finance_loan_rec_label
	var btn_loan = exec_instance.btn_take_recommended_loan
	var fm_repay_plan = exec_instance.finance_repayment_plan_label
	var btn_plan_std = exec_instance.btn_plan_standard
	var btn_plan_acc = exec_instance.btn_plan_accelerated
	var btn_plan_sur = exec_instance.btn_plan_surplus
	var fm_repay_adv = exec_instance.finance_repayment_advice_label

	if (timer_card and timer_badge and progress_bar and details_label and
		expenses_card and total_expenses_label and salaries_label and governance_label and maintenance_label and
		rev_card and rev_badge and rev_container and
		fm_card and fm_badge and fm_headline and fm_exp_advice and fm_rev_advice and
		fm_loan_rec and btn_loan and fm_repay_plan and btn_plan_std and btn_plan_acc and btn_plan_sur and fm_repay_adv):
		print("  ✓ AppointmentTimerCard bound in LeftDock/CabinetPortfoliosVBox")
		print("  ✓ OngoingExpensesCard and breakdown labels bound in EconomyModal")
		print("  ✓ RevenueActivitiesCard and container bound in EconomyModal")
		print("  ✓ FinanceMinisterAdvisoryCard, loan button, and repayment plan buttons bound")
		passed_checks += 1
	else:
		push_error("FAIL: One or more Ministerial Financial Management UI nodes missing from scene.")
		quit(1)
		return

	# -------------------------------------------------------------------------
	# CHECK 2: Ministerial Appointment Lock & Countdown Timer
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: Ministerial Appointment Lock & Countdown Timer ---")
	var mock_app_state := {
		"is_appointment_in_progress": true,
		"active_appointment_portfolio": "Finance",
		"active_appointment_minister_name": "Dr. Marcus Vance",
		"active_appointment_time_remaining_seconds": 600.0,
		"appointment_timer_formatted": "10:00"
	}
	exec_instance._update_cabinet_portfolios_ui(mock_app_state)

	if not timer_card.visible:
		push_error("FAIL: AppointmentTimerCard should be visible during active appointment hearing.")
		quit(1)
		return

	if not timer_badge.text.contains("10:00"):
		push_error("FAIL: Expected timer badge to show '10:00', got: " + timer_badge.text)
		quit(1)
		return

	if not exec_instance.appoint_defense_button.disabled:
		push_error("FAIL: Other ministerial appointment buttons must be locked while hearing is active.")
		quit(1)
		return

	if not exec_instance.appoint_defense_button.text.contains("Locked"):
		push_error("FAIL: Locked button should display lock status text, got: " + exec_instance.appoint_defense_button.text)
		quit(1)
		return

	# Attempt to open another appointment modal while hearing is active
	exec_instance.open_appointment_modal("Defense")
	if exec_instance.cabinet_appointment_modal and exec_instance.cabinet_appointment_modal.visible:
		push_error("FAIL: Cabinet appointment modal should NOT open when an active appointment hearing is underway.")
		quit(1)
		return
	print("  ✓ Blocked opening new appointment modal while hearing active")

	# Simulate time progress (delta = 15.0 seconds)
	exec_instance._process(15.0)
	if not timer_badge.text.contains("09:45"):
		push_error("FAIL: Expected timer badge to update to '09:45' after 15s delta, got: " + timer_badge.text)
		quit(1)
		return
	print("  ✓ Countdown timer updated to 09:45 after simulation step")

	# Complete appointment and verify unlocking
	exec_instance.execute_complete_appointment()
	if timer_card.visible:
		push_error("FAIL: AppointmentTimerCard should be hidden once appointment completes.")
		quit(1)
		return

	if exec_instance.appoint_defense_button.disabled:
		push_error("FAIL: Portfolio appoint buttons must unlock when hearing concludes.")
		quit(1)
		return
	print("  ✓ Appointment hearing completed successfully; ministerial slots unlocked")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 3: In-Country Ongoing Expenses Breakdown
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: In-Country Ongoing Expenses Breakdown ---")
	var mock_econ_state := {
		"budget_revenue_formatted": "R 2,400.0M / Qtr",
		"budget_expenditure_formatted": "R 2,505.0M / Qtr",
		"budget_balance_formatted": "-R 105.0M Deficit",
		"budget_salaries_allocation": 85000000.0,
		"budget_governance_allocation": 140000000.0,
		"budget_maintenance_allocation": 180000000.0,
		"budget_ongoing_governance_total": 405000000.0,
		"additional_quarterly_revenue_formatted": "+R 65M / Qtr",
		"available_revenue_activities": [
			{
				"id": "rev_digital_tax_audit",
				"title": "E-Invoicing & Automated Tax Audits",
				"description": "Deploy automated digital VAT and corporate audit reconciliation.",
				"revenue_boost_formatted": "+R 65M / Qtr",
				"implementation_cost_formatted": "R 30M",
				"is_enacted": true
			},
			{
				"id": "rev_commercial_concessions",
				"title": "Port & Logistics Commercial Concessions",
				"description": "Lease sovereign deepwater terminal and freight warehouse slots.",
				"revenue_boost_formatted": "+R 90M / Qtr",
				"implementation_cost_formatted": "R 45M",
				"is_enacted": false
			}
		],
		"is_finance_minister_appointed": true,
		"finance_minister_name": "Dr. Marcus Vance",
		"finance_minister_advice_headline": "Fiscal outlook: Prioritize digital compliance and maintain debt buffers.",
		"finance_minister_expenditure_advice": "Ongoing governance consumes R 405M quarterly; limit non-essential outlays.",
		"finance_minister_revenue_advice": "Active e-invoicing boosts quarterly receipts by +R 65M.",
		"finance_minister_loan_recommendation": "Recommended: Multilateral external concession loan of R 50M.",
		"finance_minister_recommended_loan_type": "external",
		"finance_minister_recommended_loan_amount": 50000000.0,
		"current_debt_repayment_plan": "Standard Amortization",
		"finance_minister_repayment_plan_advice": "Standard 10-year amortization preserves core quarterly cash balances."
	}

	exec_instance._update_economy_central_bank_modal(mock_econ_state)

	if not total_expenses_label.text.contains("405.0M"):
		push_error("FAIL: Total ongoing expenses should show R 405.0M, got: " + total_expenses_label.text)
		quit(1)
		return

	if not salaries_label.text.contains("85.0M"):
		push_error("FAIL: Salaries expense should show R 85.0M, got: " + salaries_label.text)
		quit(1)
		return

	if not governance_label.text.contains("140.0M"):
		push_error("FAIL: Cost of Governance should show R 140.0M, got: " + governance_label.text)
		quit(1)
		return

	if not maintenance_label.text.contains("180.0M"):
		push_error("FAIL: Maintenance expense should show R 180.0M, got: " + maintenance_label.text)
		quit(1)
		return
	print("  ✓ Verified ongoing expenses breakdown: Salaries (85M), Governance (140M), Maintenance (180M), Total (405M)")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 4: Revenue Enhancement Initiatives Dynamic Rendering & Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Revenue Enhancement Initiatives Rendering & Dispatch ---")
	if not rev_badge.text.contains("+R 65M"):
		push_error("FAIL: Revenue boost badge should display '+R 65M', got: " + rev_badge.text)
		quit(1)
		return

	var act_children = rev_container.get_children()
	if act_children.size() < 2:
		push_error("FAIL: Expected at least 2 revenue activity cards, got: " + str(act_children.size()))
		quit(1)
		return

	# Verify enact action dispatch
	var enact_res: Dictionary = exec_instance.execute_enact_revenue_activity("rev_commercial_concessions")
	if not enact_res.get("success", false):
		push_error("FAIL: execute_enact_revenue_activity returned failure: " + str(enact_res))
		quit(1)
		return

	# Verify request file content
	var req_file := FileAccess.open(exec_instance._resolve_request_path(), FileAccess.READ)
	if req_file == null:
		push_error("FAIL: Could not open written request file.")
		quit(1)
		return
	var req_data = JSON.parse_string(req_file.get_as_text())
	req_file.close()

	if req_data.get("action") != "enact_revenue_activity" or req_data.get("activity_id") != "rev_commercial_concessions":
		push_error("FAIL: Request file contents invalid: " + str(req_data))
		quit(1)
		return
	print("  ✓ Revenue activity dispatch verified for 'rev_commercial_concessions'")
	passed_checks += 1

	# -------------------------------------------------------------------------
	# CHECK 5: Finance Minister Advisory Role & Debt Operations
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Finance Minister Advisory Role & Debt Operations ---")
	if not fm_badge.text.contains("Dr. Marcus Vance (Active)"):
		push_error("FAIL: Finance Minister active badge should name Dr. Marcus Vance, got: " + fm_badge.text)
		quit(1)
		return

	if not fm_exp_advice.text.contains("405M"):
		push_error("FAIL: Expenditure advice should reference ongoing governance costs, got: " + fm_exp_advice.text)
		quit(1)
		return

	if not fm_rev_advice.text.contains("+R 65M"):
		push_error("FAIL: Revenue advice should reference e-invoicing boost, got: " + fm_rev_advice.text)
		quit(1)
		return

	if btn_loan.disabled:
		push_error("FAIL: Recommended loan button should be enabled when Finance Minister is appointed.")
		quit(1)
		return

	if not btn_loan.text.contains("External") or not btn_loan.text.contains("50M"):
		push_error("FAIL: Loan button text should reflect External and 50M, got: " + btn_loan.text)
		quit(1)
		return

	# Test Take Recommended Loan
	var loan_res: Dictionary = exec_instance.execute_take_recommended_loan()
	if not loan_res.get("success", false):
		push_error("FAIL: execute_take_recommended_loan returned failure: " + str(loan_res))
		quit(1)
		return

	var req_file2 := FileAccess.open(exec_instance._resolve_request_path(), FileAccess.READ)
	var req_data2 = JSON.parse_string(req_file2.get_as_text())
	req_file2.close()
	if req_data2.get("action") != "take_recommended_loan":
		push_error("FAIL: Request action should be 'take_recommended_loan', got: " + str(req_data2.get("action")))
		quit(1)
		return
	print("  ✓ Finance Minister recommended loan dispatch verified")

	# Test Set Repayment Plan
	var plan_res: Dictionary = exec_instance.execute_set_repayment_plan("Surplus-Linked Paydown")
	if not plan_res.get("success", false):
		push_error("FAIL: execute_set_repayment_plan returned failure: " + str(plan_res))
		quit(1)
		return

	var req_file3 := FileAccess.open(exec_instance._resolve_request_path(), FileAccess.READ)
	var req_data3 = JSON.parse_string(req_file3.get_as_text())
	req_file3.close()
	if req_data3.get("action") != "set_repayment_plan" or req_data3.get("plan_name") != "Surplus-Linked Paydown":
		push_error("FAIL: Request action should be 'set_repayment_plan' with plan 'Surplus-Linked Paydown', got: " + str(req_data3))
		quit(1)
		return
	print("  ✓ Sovereign debt repayment plan dispatch verified for 'Surplus-Linked Paydown'")
	passed_checks += 1

	print("\n=================================================================")
	print("🎉 ALL %d/%d CHECKS PASSED: MINISTERIAL & FINANCIAL SYSTEMS VERIFIED!" % [passed_checks, total_checks])
	print("=================================================================\n")
	quit(0)
