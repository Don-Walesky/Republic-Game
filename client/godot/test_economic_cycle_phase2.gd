extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: ECONOMIC CYCLE PHASE 2 (GODOT EXECUTIVE OFFICE)")
	print("=================================================================")

	var passed_checks := 0
	var total_checks := 7

	# -------------------------------------------------------------------------
	# CHECK 1: Discovery of Phase 2 Controls & Elements
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: Phase 2 UI Controls Discovery ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var card_bci = exec_instance.card_business_confidence
	var lbl_bci = exec_instance.business_confidence_label
	var sectors_container = exec_instance.stock_sectors_container
	var lbl_drivers = exec_instance.stock_drivers_label
	var forex_interv_box = exec_instance.forex_intervention_hbox
	var lbl_comp = exec_instance.forex_competitiveness_label
	var btn_defend = exec_instance.btn_defend_currency
	var btn_boost = exec_instance.btn_boost_exports
	var trade_tariff_box = exec_instance.trade_tariff_hbox
	var lbl_trade = exec_instance.trade_balance_details_label
	var lbl_tariff = exec_instance.tariff_rate_label
	var btn_t_cut = exec_instance.btn_tariff_cut
	var btn_t_hike = exec_instance.btn_tariff_hike
	var lbl_debt_sust = exec_instance.debt_sustainability_label

	if card_bci == null or lbl_bci == null:
		push_error("FAIL: TopBar Business Confidence Index controls not found.")
		quit(1)
		return

	if sectors_container == null or lbl_drivers == null:
		push_error("FAIL: Stock market sectors container or drivers label not found.")
		quit(1)
		return

	if forex_interv_box == null or lbl_comp == null or btn_defend == null or btn_boost == null:
		push_error("FAIL: Forex intervention controls or competitiveness label not found.")
		quit(1)
		return

	if trade_tariff_box == null or lbl_trade == null or lbl_tariff == null or btn_t_cut == null or btn_t_hike == null:
		push_error("FAIL: Trade and tariff controls not found.")
		quit(1)
		return

	if lbl_debt_sust == null:
		push_error("FAIL: Debt sustainability label not found.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: All Phase 2 controls discovered cleanly.")

	# -------------------------------------------------------------------------
	# CHECK 2: TopBar Business Confidence Barometer Rendering
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: TopBar Business Confidence Rendering ---")
	var mock_state_expansion := {
		"business_confidence_score": 78.5,
		"business_confidence_status": "Strong Expansion 🟢",
		"business_confidence_summary": "Private investment is accelerating rapidly."
	}
	exec_instance.apply_state(mock_state_expansion)
	print("Rendered BCI text: ", lbl_bci.text)
	if not ("78.5" in lbl_bci.text) or not ("Strong Expansion" in lbl_bci.text):
		push_error("FAIL: BCI expansion status not rendered accurately.")
		quit(1)
		return

	var mock_state_contraction := {
		"business_confidence_score": 38.2,
		"business_confidence_status": "Severe Contraction 🔴",
		"business_confidence_summary": "Macro headwinds and inflation suppress corporate sentiment."
	}
	exec_instance.apply_state(mock_state_contraction)
	print("Rendered BCI contraction: ", lbl_bci.text)
	if not ("38.2" in lbl_bci.text) or not ("Severe Contraction" in lbl_bci.text):
		push_error("FAIL: BCI contraction status not rendered accurately.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Business Confidence barometer responds dynamically.")

	# -------------------------------------------------------------------------
	# CHECK 3: Expanded Stock Market Sectors & Macro Drivers
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: Stock Market Sectors & Macro Drivers ---")
	var mock_stock_state := {
		"stock_market_name": "Republic Composite Index (RCI)",
		"stock_market_points": 2845.5,
		"stock_market_direction": "Bullish ↗️",
		"stock_market_change_pct": 3.4,
		"stock_market_sentiment": "Broad-Based Rally",
		"stock_sectors": [
			{
				"sector_name": "Technology & Telecommunications",
				"sector_points": 1450.0,
				"sector_change_pct": 4.5,
				"sector_sentiment": "Bullish Rally 🚀"
			},
			{
				"sector_name": "Industrial Manufacturing & Infrastructure",
				"sector_points": 1280.0,
				"sector_change_pct": 2.1,
				"sector_sentiment": "Moderate Expansion 🟢"
			},
			{
				"sector_name": "Banking & Financial Services",
				"sector_points": 1150.0,
				"sector_change_pct": -0.8,
				"sector_sentiment": "Neutral Stagnation 🟡"
			}
		],
		"stock_market_drivers": [
			"Domestic benchmark interest rate policy (4.5%)",
			"Corporate tax environment (20.0%)",
			"Institutional stability & anti-corruption safeguards"
		]
	}
	exec_instance._update_economy_central_bank_modal(mock_stock_state)

	var child_count: int = sectors_container.get_child_count()
	print("Rendered sector count: ", child_count)
	if child_count < 3:
		push_error("FAIL: Expected at least 3 sector labels in StockSectorsContainer.")
		quit(1)
		return

	var first_sector_lbl: Label = sectors_container.get_child(0) as Label
	print("First sector label: ", first_sector_lbl.text)
	if not ("Technology" in first_sector_lbl.text) or not ("1450" in first_sector_lbl.text):
		push_error("FAIL: First stock sector data incorrect.")
		quit(1)
		return

	print("Stock drivers label: ", lbl_drivers.text)
	if not ("interest rate" in lbl_drivers.text) or not ("anti-corruption" in lbl_drivers.text):
		push_error("FAIL: Stock market drivers label missing expected macro drivers.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Stock market sectors and drivers render successfully.")

	# -------------------------------------------------------------------------
	# CHECK 4: Forex Interventions & Dynamic Button Gating
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Forex Interventions & Button Gating ---")
	# Case A: Locked (reserves < 150M)
	var state_forex_locked := {
		"forex_unlocked": false,
		"foreign_reserves_formatted": "R 85.0M",
		"forex_export_competitiveness": "1.00x (Neutral)",
		"forex_import_cost_multiplier": "1.00x (Neutral)"
	}
	exec_instance._update_economy_central_bank_modal(state_forex_locked)
	if not btn_defend.disabled or not btn_boost.disabled:
		push_error("FAIL: Forex intervention buttons must be disabled when forex_unlocked is false.")
		quit(1)
		return

	# Case B: Unlocked (reserves >= 150M)
	var state_forex_unlocked := {
		"forex_unlocked": true,
		"foreign_reserves_formatted": "R 165.0M",
		"forex_export_competitiveness": "1.08x (Export Advantage)",
		"forex_import_cost_multiplier": "1.05x (Import Inflation)"
	}
	exec_instance._update_economy_central_bank_modal(state_forex_unlocked)
	if btn_defend.disabled or btn_boost.disabled:
		push_error("FAIL: Forex intervention buttons must be enabled when forex_unlocked is true.")
		quit(1)
		return

	print("Competitiveness label: ", lbl_comp.text)
	if not ("1.08x" in lbl_comp.text) or not ("Export Advantage" in lbl_comp.text):
		push_error("FAIL: Forex competitiveness label not updated properly.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Forex interventions and button gating verified.")

	# -------------------------------------------------------------------------
	# CHECK 5: Trade Balance, Tariffs & Debt Sustainability Status
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Trade Balance, Tariffs & Debt Sustainability ---")
	var state_macro_trade := {
		"quarterly_exports_formatted": "R 3,920.0M / Qtr",
		"quarterly_imports_formatted": "R 2,450.0M / Qtr",
		"net_trade_balance_formatted": "+R 1,470.0M Surplus",
		"import_tariff_rate_formatted": "8.0%",
		"debt_sustainability_status": "Sustainable (Low Risk) 🟢",
		"debt_to_gdp_ratio_formatted": "2.5%",
		"debt_service_to_revenue_ratio_formatted": "0.2%"
	}
	exec_instance._update_economy_central_bank_modal(state_macro_trade)

	print("Trade details: ", lbl_trade.text)
	if not ("3,920.0M" in lbl_trade.text) or not ("Surplus" in lbl_trade.text):
		push_error("FAIL: Trade balance details label incorrect.")
		quit(1)
		return

	print("Tariff rate: ", lbl_tariff.text)
	if not ("8.0%" in lbl_tariff.text):
		push_error("FAIL: Tariff rate label incorrect.")
		quit(1)
		return

	print("Debt sustainability: ", lbl_debt_sust.text)
	if not ("Sustainable" in lbl_debt_sust.text) or not ("2.5%" in lbl_debt_sust.text):
		push_error("FAIL: Debt sustainability label incorrect.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Trade balance, tariffs, and debt sustainability rendered accurately.")

	# -------------------------------------------------------------------------
	# CHECK 6: Forex Intervention Request Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 6: Forex Intervention Action Dispatch ---")
	exec_instance.current_state_path = BRIDGE_STATE_PATH
	exec_instance.request_file_override = BRIDGE_REQ_PATH
	if FileAccess.file_exists(BRIDGE_REQ_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(BRIDGE_REQ_PATH))

	# Defend currency
	exec_instance._on_defend_currency_pressed()
	var req_file := FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
	if req_file == null:
		push_error("FAIL: Did not write presidential_desk_request.json on defend currency.")
		quit(1)
		return

	var req_json = JSON.parse_string(req_file.get_as_text())
	req_file.close()
	print("Defend currency request: ", req_json)

	if req_json.get("action") != "forex_intervention" or req_json.get("intervention_type") != "defend":
		push_error("FAIL: Invalid action or intervention_type for defend currency.")
		quit(1)
		return

	# Boost exports
	exec_instance._on_boost_exports_pressed()
	req_file = FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
	var boost_json = JSON.parse_string(req_file.get_as_text())
	req_file.close()
	print("Boost exports request: ", boost_json)

	if boost_json.get("action") != "forex_intervention" or boost_json.get("intervention_type") != "boost_exports":
		push_error("FAIL: Invalid action or intervention_type for boost exports.")
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Forex intervention requests write authoritative payloads.")

	# -------------------------------------------------------------------------
	# CHECK 7: Tariff Adjustment Action Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 7: Tariff Adjustment Action Dispatch ---")
	exec_instance.latest_state = { "import_tariff_rate_formatted": "8.0%" }
	exec_instance._on_tariff_hike_pressed()

	req_file = FileAccess.open(BRIDGE_REQ_PATH, FileAccess.READ)
	var tariff_json = JSON.parse_string(req_file.get_as_text())
	req_file.close()
	print("Tariff hike request: ", tariff_json)

	if tariff_json.get("action") != "trade_tariff":
		push_error("FAIL: Action should be trade_tariff.")
		quit(1)
		return

	var proposed_pct: float = float(tariff_json.get("tariff_percent", 0.0))
	if abs(proposed_pct - 10.0) > 0.01:
		push_error("FAIL: Expected 8.0% + 2.0% = 10.0%, got %f" % proposed_pct)
		quit(1)
		return

	passed_checks += 1
	print("✓ PASS: Trade tariff hike successfully dispatched (+2.0%).")

	print("\n=================================================================")
	print("🎉 ALL %d/%d PHASE 2 ECONOMIC CYCLE CHECKS PASSED SUCCESSFULLY!" % [passed_checks, total_checks])
	print("=================================================================")
	quit(0)
