extends SceneTree

## Automated Headless Verification Suite for Republic Phase 3:
## Personalised Presidential Office & Modular Customisation
## Validates:
## 1. 8 Selectable Presidential Portraits (5 male, 3 female) and sovereign country flag display
## 2. 6 Modular Furniture & Interior Design Categories with preview and independent mixing
## 3. Authoritative Purchasing validation (REPU prices, Owned/Available/Locked, confirmation, facilities approval, request pipeline)
## 4. Save and restore persistence across game sessions and country isolation
## 5. Modern UX, modal lifecycle, and unobstructed navigation

const ExecutiveOfficeCatalog = preload("res://ExecutiveOfficeCatalog.gd")

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - EXECUTIVE OFFICE PHASE 3 CUSTOMISATION TEST     ")
	print("==============================================================")

	var scene_res := load("res://ExecutiveOffice.tscn")
	if scene_res == null:
		printerr("[FAIL]: Failed to load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var office = scene_res.instantiate()
	if office == null:
		printerr("[FAIL]: Failed to instantiate ExecutiveOffice node")
		quit(1)
		return

	root.add_child(office)
	office._init_controls()

	var all_passed := true

	# Check 1: Presidential Portraits and Country Identity
	all_passed = test_portraits_and_country_identity(office) and all_passed

	# Check 2: Modular Furniture Categories & Independent Mixing
	all_passed = test_modular_furniture_categories_and_mixing(office) and all_passed

	# Check 3: Authoritative Purchasing & Item Availability
	all_passed = test_purchasing_and_item_availability(office) and all_passed

	# Check 4: Purchase Confirmation Modal & Action Pipeline Request
	all_passed = test_purchase_confirmation_and_request_pipeline(office) and all_passed

	# Check 5: Save & Restore Persistence and Country Isolation
	all_passed = test_save_and_restore_persistence(office) and all_passed

	# Check 6: Customisation Modal Lifecycle & UX
	all_passed = test_customisation_modal_ux(office) and all_passed

	office.queue_free()

	# Clean up any test artifacts created during testing
	var temp_files := [
		"res://temp_phase3_customization.json",
		"res://temp_phase3_country_a.json",
		"res://temp_phase3_country_b.json",
		"res://presidential_desk_request.json"
	]
	for p in temp_files:
		if FileAccess.file_exists(p):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(p))

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL 6 PHASE 3 CUSTOMISATION VERIFICATION CHECKS PASSED! ")
		print("==============================================================")
		quit(0)
	else:
		print("==============================================================")
		print(" [FAIL]: ONE OR MORE PHASE 3 CUSTOMISATION CHECKS FAILED!     ")
		print("==============================================================")
		quit(1)


func test_portraits_and_country_identity(office: Control) -> bool:
	print("\n--- Check 1: Presidential Portraits (8 total: 5 male, 3 female) & Country Identity ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	# 1. Verify portrait catalog count and gender breakdown
	var portraits: Array = ExecutiveOfficeCatalog.get_portraits()
	if portraits.size() != 8:
		printerr("[FAIL]: Expected 8 presidential portraits, got: %d" % portraits.size())
		return false

	var male_count := 0
	var female_count := 0
	for p in portraits:
		var g: String = str(p.get("gender", "")).to_lower()
		if g == "male":
			male_count += 1
		elif g == "female":
			female_count += 1

	if male_count != 5 or female_count != 3:
		printerr("[FAIL]: Expected 5 male and 3 female portraits, got: %d male, %d female" % [male_count, female_count])
		return false
	print("  [OK]: Catalog contains exactly 8 portraits (5 male, 3 female).")

	# 2. Test male portrait selection: Marcus Sterling (The Reformist)
	office.select_and_install_portrait("portrait_m2")
	if office.portrait_icon_label.text != "📈":
		printerr("[FAIL]: Sterling portrait icon mismatch. Got: " + office.portrait_icon_label.text)
		return false
	if not office.portrait_name_label.text.contains("MARCUS STERLING"):
		printerr("[FAIL]: Sterling portrait name mismatch. Got: " + office.portrait_name_label.text)
		return false
	if not office.portrait_placeholder_label.text.contains("REFORMIST"):
		printerr("[FAIL]: Sterling portrait archetype mismatch. Got: " + office.portrait_placeholder_label.text)
		return false

	# 3. Test female portrait selection: Elena Rostova (The Economist)
	office.select_and_install_portrait("portrait_f1")
	if office.portrait_icon_label.text != "💼":
		printerr("[FAIL]: Rostova portrait icon mismatch. Got: " + office.portrait_icon_label.text)
		return false
	if not office.portrait_name_label.text.contains("ELENA ROSTOVA"):
		printerr("[FAIL]: Rostova portrait name mismatch. Got: " + office.portrait_name_label.text)
		return false
	if not office.portrait_placeholder_label.text.contains("ECONOMIST"):
		printerr("[FAIL]: Rostova portrait archetype mismatch. Got: " + office.portrait_placeholder_label.text)
		return false

	# 4. Verify player's real country name and selected flag
	var expected_country := "Country: Republic of Arcadia"
	if office.country_label.text != expected_country:
		printerr("[FAIL]: Country name mismatch. Expected '%s', got: '%s'" % [expected_country, office.country_label.text])
		return false

	# Test flag styles: 5 heraldic options
	var flags: Array = ExecutiveOfficeCatalog.get_flags()
	if flags.size() < 5:
		printerr("[FAIL]: Expected at least 5 flag heraldry options, got: %d" % flags.size())
		return false

	office.select_and_install_flag("flag_emerald_white")
	if office.flag_emblem_label.text != "ARCADIA":
		printerr("[FAIL]: Flag emblem text mismatch. Got: " + office.flag_emblem_label.text)
		return false

	print("  [OK]: 8 portraits (5 male, 3 female) and sovereign country flag displayed accurately.")
	return true


func test_modular_furniture_categories_and_mixing(office: Control) -> bool:
	print("\n--- Check 2: Six Modular Furniture Categories & Independent Mixing ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	var categories := [
		"desk_and_chair",
		"visitor_seating",
		"wall_and_flooring",
		"carpets_and_lighting",
		"bookshelves_and_decor",
		"artwork_and_gifts"
	]

	# 1. Verify items exist for all 6 categories
	for cat in categories:
		var items: Array = ExecutiveOfficeCatalog.get_items_by_category(cat)
		if items.is_empty():
			printerr("[FAIL]: Category '%s' has no catalog items." % cat)
			return false
	print("  [OK]: Catalog defines modular items for all 6 required categories.")

	# 2. Test independent item selection and mixing across categories:
	office.select_and_install_item("desk_obsidian_modern")
	office.select_and_install_item("visitor_teak_conference")
	office.select_and_install_item("wall_marble_herringbone")
	office.select_and_install_item("carpet_navy_crest")
	office.select_and_install_item("decor_global_cartography")
	office.select_and_install_item("art_sovereign_landscape")

	# Verify visual components independently took on the applied styling:
	# Obsidian desk: stylebox bg_color
	var desk_style := office.desk_surface.get_theme_stylebox("panel") as StyleBoxFlat
	if desk_style == null or desk_style.bg_color != Color(0.12, 0.14, 0.18, 1.0):
		printerr("[FAIL]: Desk surface style did not update to obsidian modern.")
		return false

	# Visitor seating: teak table
	var table_style := office.visitor_table.get_theme_stylebox("panel") as StyleBoxFlat
	if table_style == null or table_style.bg_color != Color(0.45, 0.30, 0.16, 1.0):
		printerr("[FAIL]: Visitor table did not update to teak conference.")
		return false

	# Wall & Flooring: Marble wall backdrop
	if office.wall_backdrop.color != Color(0.22, 0.24, 0.28, 1.0):
		printerr("[FAIL]: Wall backdrop did not update to marble color.")
		return false

	# Carpet & Lighting: Navy crest carpet
	var carpet_style := office.office_carpet.get_theme_stylebox("panel") as StyleBoxFlat
	if carpet_style == null or carpet_style.bg_color != Color(0.10, 0.16, 0.28, 0.85):
		printerr("[FAIL]: Carpet style did not update to navy crest.")
		return false

	# Bookshelves: Cartography symbol & label
	if office.bookshelf_icon_label.text != "🌐" or office.bookshelf_label.text != "WORLD CARTOGRAPHY":
		printerr("[FAIL]: Bookshelf visual decor did not update to world cartography.")
		return false

	# Artwork: Sovereign landscape symbol & label
	if office.artwork_icon_label.text != "🌄" or office.artwork_label.text != "SOVEREIGN VISTA":
		printerr("[FAIL]: Artwork visual did not update to sovereign vista.")
		return false

	# 3. Test Preview functionality: previewing an item should NOT permanently alter active_customization
	var pre_preview_carpet: String = str(office.active_customization.get("installed_items", {}).get("carpets_and_lighting", ""))
	office.preview_furniture_item("carpet_emerald_runner")
	var post_preview_carpet: String = str(office.active_customization.get("installed_items", {}).get("carpets_and_lighting", ""))
	if pre_preview_carpet != post_preview_carpet:
		printerr("[FAIL]: Preview modified active_customization installed_items. Expected: %s, got: %s" % [pre_preview_carpet, post_preview_carpet])
		return false
	if not office.customisation_status_label.text.contains("Previewing"):
		printerr("[FAIL]: Customisation status label did not reflect preview.")
		return false

	print("  [OK]: Independent selection, mixing, and non-destructive preview verified across all 6 categories.")
	return true


func test_purchasing_and_item_availability(office: Control) -> bool:
	print("\n--- Check 3: Authoritative Purchasing & Item Availability ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	# 1. Free starter item check: should always be owned/available
	var starter_desk: Dictionary = ExecutiveOfficeCatalog.get_item("desk_mahogany_classic")
	var starter_check: Dictionary = ExecutiveOfficeCatalog.check_item_availability(starter_desk, office.latest_state, [])
	if starter_check.get("status", "") != "OWNED":
		printerr("[FAIL]: Starter mahogany desk should have status OWNED.")
		return false

	# 2. Priced item within treasury: e.g. art_sovereign_landscape (R 400,000) with R 15,000,000,000 treasury
	var landscape_art: Dictionary = ExecutiveOfficeCatalog.get_item("art_sovereign_landscape")
	var landscape_check: Dictionary = ExecutiveOfficeCatalog.check_item_availability(landscape_art, office.latest_state, [])
	if landscape_check.get("status", "") != "AVAILABLE" or not landscape_check.get("can_purchase", false):
		printerr("[FAIL]: Affordable art_sovereign_landscape should be AVAILABLE for purchase. Reason: " + str(landscape_check.get("reason", "")))
		return false

	# 3. Insufficient treasury check: simulate impoverished country
	var broke_state := {
		"country_name": "Poor Republic",
		"treasury": "R 10,000",
		"treasury_balance": 10000.0
	}
	var expensive_item: Dictionary = ExecutiveOfficeCatalog.get_item("wall_walnut_granite") # R 1,200,000
	var broke_check: Dictionary = ExecutiveOfficeCatalog.check_item_availability(expensive_item, broke_state, [])
	if broke_check.get("status", "") != "LOCKED" or broke_check.get("can_purchase", true):
		printerr("[FAIL]: Imperial walnut wall should be LOCKED when treasury is insufficient.")
		return false
	if not str(broke_check.get("reason", "")).contains("Insufficient treasury"):
		printerr("[FAIL]: Expected 'Insufficient treasury' reason, got: " + str(broke_check.get("reason", "")))
		return false

	# 4. Ministry of Interior approval requirement check
	# In presidential_desk_state.json, interior_minister is "Vacant"
	var interior_item: Dictionary = ExecutiveOfficeCatalog.get_item("visitor_lounge_suite") # Requires Ministry of Interior
	var interior_check: Dictionary = ExecutiveOfficeCatalog.check_item_availability(interior_item, office.latest_state, [])
	if interior_check.get("status", "") != "LOCKED" or interior_check.get("can_purchase", true):
		printerr("[FAIL]: Diplomatic lounge should be LOCKED when Ministry of Interior is vacant.")
		return false
	if not str(interior_check.get("reason", "")).contains("Ministry of Interior"):
		printerr("[FAIL]: Expected Ministry of Interior reason, got: " + str(interior_check.get("reason", "")))
		return false

	print("  [OK]: Item pricing, owned/available/locked states, treasury validation, and ministerial approvals verified.")
	return true


func test_purchase_confirmation_and_request_pipeline(office: Control) -> bool:
	print("\n--- Check 4: Purchase Confirmation Modal & Action Pipeline Request ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	var item_to_buy: Dictionary = ExecutiveOfficeCatalog.get_item("carpet_emerald_runner")

	# 1. Prompt purchase: opens confirmation modal
	office.prompt_purchase_item(item_to_buy)
	if not office.purchase_confirm_modal.visible:
		printerr("[FAIL]: PurchaseConfirmModal not visible after prompt.")
		return false

	if not office.purchase_item_name_label.text.contains("Emerald Silk Runner"):
		printerr("[FAIL]: Purchase item name mismatch. Got: " + office.purchase_item_name_label.text)
		return false

	if not office.purchase_price_label.text.contains("R 520,000"):
		printerr("[FAIL]: Purchase price label mismatch. Got: " + office.purchase_price_label.text)
		return false

	if not office.purchase_approval_label.text.contains("Presidential Facilities"):
		printerr("[FAIL]: Purchase approval label mismatch. Got: " + office.purchase_approval_label.text)
		return false

	# 2. Confirm purchase:
	var result: Dictionary = office.confirm_purchase_pending_item()
	if not result.get("success", false):
		printerr("[FAIL]: confirm_purchase_pending_item failed: " + result.get("message", ""))
		return false

	# Verify modal closed
	if office.purchase_confirm_modal.visible:
		printerr("[FAIL]: PurchaseConfirmModal remained visible after confirmation.")
		return false

	# Verify item is now in owned_items
	var owned: Array = office.active_customization.get("owned_items", [])
	if not owned.has("carpet_emerald_runner"):
		printerr("[FAIL]: Purchased item 'carpet_emerald_runner' not added to owned_items.")
		return false

	# Verify authoritative request file was generated
	var req_path := "res://presidential_desk_request.json"
	if not FileAccess.file_exists(req_path):
		printerr("[FAIL]: Authoritative request file was not written: " + req_path)
		return false

	var req_text := FileAccess.get_file_as_string(req_path)
	var req_json = JSON.parse_string(req_text)
	if req_json == null or req_json.get("action", "") != "purchase_furniture":
		printerr("[FAIL]: Request JSON action mismatch. Expected 'purchase_furniture', got: " + str(req_json))
		return false
	if req_json.get("item_id", "") != "carpet_emerald_runner" or int(req_json.get("price_repu", 0)) != 520000:
		printerr("[FAIL]: Request JSON payload mismatch: " + str(req_json))
		return false

	print("  [OK]: Purchase confirmation modal, request generation, and owned list update verified.")
	return true


func test_save_and_restore_persistence(office: Control) -> bool:
	print("\n--- Check 5: Save & Restore Persistence & Country Isolation ---")
	var temp_save_path := "res://temp_phase3_customization.json"

	# Configure test customization
	office.custom_save_path = temp_save_path
	office.select_and_install_portrait("portrait_m3") # David Chen (Technocrat)
	office.select_and_install_flag("flag_azure_silver")
	office.select_and_install_item("wall_slate_parquet")
	office.select_and_install_item("carpet_navy_crest")
	office.save_current_customization()

	if not FileAccess.file_exists(temp_save_path):
		printerr("[FAIL]: Customization save file was not created: " + temp_save_path)
		return false

	# Verify saved content
	var file_content := FileAccess.get_file_as_string(temp_save_path)
	var saved_data = JSON.parse_string(file_content)
	if saved_data == null:
		printerr("[FAIL]: Failed to parse saved customization JSON.")
		return false

	var custom_dict: Dictionary = saved_data
	if custom_dict.get("selected_portrait_id", "") != "portrait_m3":
		printerr("[FAIL]: Saved portrait mismatch. Got: " + str(custom_dict.get("selected_portrait_id", "")))
		return false
	if custom_dict.get("selected_flag_id", "") != "flag_azure_silver":
		printerr("[FAIL]: Saved flag mismatch. Got: " + str(custom_dict.get("selected_flag_id", "")))
		return false

	var installed: Dictionary = custom_dict.get("installed_items", {})
	if installed.get("wall_and_flooring", "") != "wall_slate_parquet":
		printerr("[FAIL]: Saved walls mismatch. Got: " + str(installed.get("wall_and_flooring", "")))
		return false
	if installed.get("carpets_and_lighting", "") != "carpet_navy_crest":
		printerr("[FAIL]: Saved carpet mismatch. Got: " + str(installed.get("carpets_and_lighting", "")))
		return false

	# Test session reload: instantiate fresh office node and load from file
	var scene_res := load("res://ExecutiveOffice.tscn")
	var fresh_office = scene_res.instantiate()
	root.add_child(fresh_office)
	fresh_office._init_controls()
	fresh_office.custom_save_path = temp_save_path
	fresh_office.load_saved_customization()

	if fresh_office.active_customization.get("selected_portrait_id", "") != "portrait_m3":
		printerr("[FAIL]: Fresh office failed to restore portrait. Got: " + str(fresh_office.active_customization.get("selected_portrait_id", "")))
		fresh_office.queue_free()
		return false

	if fresh_office.portrait_icon_label.text != "⚙":
		printerr("[FAIL]: Fresh office visual portrait label mismatch. Got: " + fresh_office.portrait_icon_label.text)
		fresh_office.queue_free()
		return false

	fresh_office.queue_free()

	# Test country isolation: Country A vs Country B
	var country_a_file := "res://temp_phase3_country_a.json"
	var custom_a := { "selected_portrait_id": "portrait_m1", "installed_items": { "wall_and_flooring": "wall_slate_parquet" } }
	ExecutiveOfficeCatalog.save_customization(custom_a, country_a_file, "country-alpha")

	var country_b_file := "res://temp_phase3_country_b.json"
	var custom_b := { "selected_portrait_id": "portrait_m4", "installed_items": { "wall_and_flooring": "wall_walnut_granite" } }
	ExecutiveOfficeCatalog.save_customization(custom_b, country_b_file, "country-beta")

	var loaded_a := ExecutiveOfficeCatalog.load_customization(country_a_file, "country-alpha")
	var loaded_b := ExecutiveOfficeCatalog.load_customization(country_b_file, "country-beta")

	if loaded_a.selected_portrait_id != "portrait_m1" or loaded_b.selected_portrait_id != "portrait_m4":
		printerr("[FAIL]: Country isolation failed: Country A and B portraits intermingled.")
		return false

	print("  [OK]: Customization persists across game sessions, reloads faithfully, and enforces country isolation.")
	return true


func test_customisation_modal_ux(office: Control) -> bool:
	print("\n--- Check 6: Customisation Modal Lifecycle, Category Navigation & UX ---")
	office.load_state_from_file("res://presidential_desk_state.json")

	# 1. Open modal via Nav button
	office._on_nav_customise_pressed()
	if not office.customisation_modal.visible:
		printerr("[FAIL]: CustomisationModal not visible after nav_customise_button press.")
		return false

	if not office.modal_backdrop.visible:
		printerr("[FAIL]: Modal backdrop not visible.")
		return false

	# 2. Check treasury display in modal header
	if not office.customisation_treasury_label.text.contains("R 15,000,000,000"):
		printerr("[FAIL]: Available Treasury in modal header mismatch. Got: " + office.customisation_treasury_label.text)
		return false

	# 3. Check category switching
	office._on_tab_seating_pressed()
	if office.current_customization_category != "visitor_seating":
		printerr("[FAIL]: Category not switched to visitor_seating. Got: " + office.current_customization_category)
		return false

	office._on_tab_portraits_pressed()
	if office.current_customization_category != "portraits":
		printerr("[FAIL]: Category not switched to portraits. Got: " + office.current_customization_category)
		return false

	# 4. Check close button
	office.close_customisation_modal()
	if office.customisation_modal.visible or office.modal_backdrop.visible:
		printerr("[FAIL]: CustomisationModal remained visible after close_customisation_modal.")
		return false

	if office.active_nav_section != office.Section.OVERVIEW:
		printerr("[FAIL]: Active section did not reset to OVERVIEW after closing customisation.")
		return false

	# 5. Check backdrop dismissal
	office.open_customisation_modal("artwork_and_gifts")
	if not office.customisation_modal.visible:
		printerr("[FAIL]: Customisation modal not opened.")
		return false
	office._on_modal_backdrop_gui_input(_make_mouse_click_event())
	if office.customisation_modal.visible:
		printerr("[FAIL]: Modal not dismissed on backdrop click.")
		return false

	print("  [OK]: Customisation modal navigation, treasury display, category switching, and backdrop dismissal verified.")
	return true


func _make_mouse_click_event() -> InputEventMouseButton:
	var event := InputEventMouseButton.new()
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = true
	event.position = Vector2(10, 10)
	return event
