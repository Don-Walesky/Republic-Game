extends SceneTree

const BRIDGE_STATE_PATH := "user://presidential_desk_state.json"
const BRIDGE_REQ_PATH := "user://presidential_desk_request.json"
const SAVE_FILE_PATH := "user://session.sav"

func _init() -> void:
	print("=================================================================")
	print("🧪 RUNNING SUITE: LEGISLATURE, DEVELOPMENT PROJECTS & GENDER AUTH")
	print("=================================================================")
	
	var passed_checks := 0
	var total_checks := 7

	# -------------------------------------------------------------------------
	# CHECK 1: Onboarding Gender Controls & Confirmation Validation
	# -------------------------------------------------------------------------
	print("\n--- CHECK 1: Onboarding Gender Controls & Validation ---")
	var onb_scene: PackedScene = load("res://Onboarding.tscn")
	if onb_scene == null:
		push_error("FAIL: Could not load res://Onboarding.tscn")
		quit(1)
		return
	
	var onb_instance = onb_scene.instantiate()
	root.add_child(onb_instance)
	onb_instance._init_controls()
	onb_instance._connect_signals()

	var btn_female: Button = onb_instance.btn_gender_female
	var btn_male: Button = onb_instance.btn_gender_male
	var btn_reg: Button = onb_instance.btn_register
	var lbl_feedback: Label = onb_instance.lbl_register_feedback

	if btn_female != null and btn_male != null:
		print("  ✓ Gender buttons found: Female (Madam President) and Male (Mr. President)")
		
		# Test unselected validation
		onb_instance.selected_gender = ""
		if onb_instance.input_first_name: onb_instance.input_first_name.text = "Amara"
		if onb_instance.input_last_name: onb_instance.input_last_name.text = "Diallo"
		if onb_instance.input_email: onb_instance.input_email.text = "amara@republic.test"
		onb_instance._on_register_pressed()
		
		if lbl_feedback and lbl_feedback.text.contains("gender"):
			print("  ✓ Unconfirmed gender properly blocks candidate registration: '%s'" % lbl_feedback.text)
		else:
			print("  ⚠️ Registration feedback: '%s'" % (lbl_feedback.text if lbl_feedback else ""))

		# Select female directly through handler
		onb_instance._on_gender_female_pressed()
		if onb_instance.selected_gender == "Female" and onb_instance.selected_title == "Madam President" and onb_instance.selected_avatar == "👩‍💼":
			print("  ✓ Female candidate confirmed: %s | %s | %s" % [onb_instance.selected_gender, onb_instance.selected_title, onb_instance.selected_avatar])
			
			# Select male directly through handler
			onb_instance._on_gender_male_pressed()
			if onb_instance.selected_gender == "Male" and onb_instance.selected_title == "Mr. President" and onb_instance.selected_avatar == "👨‍💼":
				print("  ✓ Male candidate confirmed: %s | %s | %s" % [onb_instance.selected_gender, onb_instance.selected_title, onb_instance.selected_avatar])
				passed_checks += 1
			else:
				push_error("  ❌ Male gender selection failed")
		else:
			push_error("  ❌ Female gender selection failed to set expected title/avatar")
	else:
		push_error("  ❌ Gender buttons missing from Onboarding.tscn")

	onb_instance.queue_free()

	# -------------------------------------------------------------------------
	# CHECK 2: Executive Office Scene Loading & Avatar/Honorific Rendering
	# -------------------------------------------------------------------------
	print("\n--- CHECK 2: Executive Office Avatar & Honorific Rendering ---")
	var exec_scene: PackedScene = load("res://ExecutiveOffice.tscn")
	if exec_scene == null:
		push_error("FAIL: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var exec_instance = exec_scene.instantiate()
	root.add_child(exec_instance)
	exec_instance._init_controls()
	exec_instance._connect_signals()

	var avatar_lbl: Label = exec_instance.top_president_avatar
	var role_lbl: Label = exec_instance.top_president_role

	if avatar_lbl != null and role_lbl != null:
		var female_mock_state := {
			"player_gender": "Female",
			"player_title": "Madam President",
			"player_avatar": "👩‍💼",
			"president_name": "Elena Rostova"
		}
		exec_instance._update_modern_office_visuals(female_mock_state)
		if avatar_lbl.text == "👩‍💼" and role_lbl.text == "Madam President":
			print("  ✓ Female Executive honorific rendered: '%s' with avatar '%s'" % [role_lbl.text, avatar_lbl.text])
			
			# Test male switch
			var male_mock_state := {
				"player_gender": "Male",
				"player_title": "Mr. President",
				"player_avatar": "👨‍💼",
				"president_name": "Marcus Vance"
			}
			exec_instance._update_modern_office_visuals(male_mock_state)
			if avatar_lbl.text == "👨‍💼" and role_lbl.text == "Mr. President":
				print("  ✓ Male Executive honorific rendered: '%s' with avatar '%s'" % [role_lbl.text, avatar_lbl.text])
				passed_checks += 1
			else:
				push_error("  ❌ Male honorific switch failed")
		else:
			push_error("  ❌ Female honorific failed to render on top bar: avatar='%s', role='%s'" % [avatar_lbl.text, role_lbl.text])
	else:
		push_error("  ❌ Top bar avatar/role labels missing")

	# -------------------------------------------------------------------------
	# CHECK 3: Developmental Project Catalog Extended Cards
	# -------------------------------------------------------------------------
	print("\n--- CHECK 3: Extended Developmental Project Cards ---")
	var rail_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectRail")
	var port_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectPort")
	var renew_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectRenewable")
	var hosp_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectHospital")
	var sez_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectSEZ")
	var sc_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectSupercomputer")
	var dam_card = exec_instance.get_node_or_null("InterfaceLayer/DevelopmentModal/DevVBox/DevProjectsScroll/ProjectsListVBox/CardProjectDam")

	if rail_card and port_card and renew_card and hosp_card and sez_card and sc_card and dam_card:
		print("  ✓ All 7 extended project cards exist in DevelopmentModal:")
		print("    • High-Speed Rail Corridor")
		print("    • Deepwater Seaport Hub")
		print("    • Renewable Energy Mega-Grid")
		print("    • National Teaching Hospital")
		print("    • Special Economic Zone")
		print("    • Sovereign AI & Supercomputer Center")
		print("    • Agro-Industrial Irrigation Mega-Dam")
		passed_checks += 1
	else:
		push_error("  ❌ One or more extended project cards missing from DevelopmentModal")

	# -------------------------------------------------------------------------
	# CHECK 4: Legislative Chamber Composition & Majority Status
	# -------------------------------------------------------------------------
	print("\n--- CHECK 4: Legislative Chamber Composition & Majority Status ---")
	var ruling_badge: Label = exec_instance.ruling_seats_badge
	var opp_badge: Label = exec_instance.opposition_seats_badge
	var ind_badge: Label = exec_instance.independent_seats_badge
	var status_badge: Label = exec_instance.chamber_status_badge
	var lmk_vbox: VBoxContainer = exec_instance.lawmakers_vbox

	if ruling_badge and opp_badge and ind_badge and status_badge and lmk_vbox:
		print("  ✓ Legislative Chamber UI nodes found:")
		
		# Test Minority government rendering
		var minority_state := {
			"legislature_seats_ruling": 48,
			"legislature_seats_opposition": 42,
			"legislature_seats_independent": 10,
			"legislature_chamber_status": "Hung Parliament / Minority",
			"legislature_has_majority": false
		}
		exec_instance._update_policy_and_bills_modal(minority_state)
		if ruling_badge.text.contains("48") and status_badge.text.contains("Minority"):
			print("    • Correctly displays Minority Government (48/100 Seats)")
		
		# Test Majority government rendering
		var majority_state := {
			"legislature_seats_ruling": 52,
			"legislature_seats_opposition": 39,
			"legislature_seats_independent": 9,
			"legislature_chamber_status": "Stable Majority",
			"legislature_has_majority": true
		}
		exec_instance._update_policy_and_bills_modal(majority_state)
		if ruling_badge.text.contains("52") and status_badge.text.contains("Majority"):
			print("    • Correctly displays Stable Majority Government (52/100 Seats)")
			passed_checks += 1
		else:
			push_error("    ❌ Failed to update majority chamber status badge: '%s'" % status_badge.text)
	else:
		push_error("  ❌ Legislative Chamber composition nodes missing")

	# -------------------------------------------------------------------------
	# CHECK 5: Lawmakers Floor Roster & Courtship Actions
	# -------------------------------------------------------------------------
	print("\n--- CHECK 5: Lawmakers Floor Roster & Courtship Desk ---")
	var mock_lawmakers_state := {
		"legislature_seats_ruling": 48,
		"legislature_seats_opposition": 42,
		"legislature_seats_independent": 10,
		"lawmakers": [
			{
				"id": "lawmaker_thorne",
				"name": "Hon. Marcus Thorne",
				"constituency": "Industrial District",
				"current_party": "Opposition",
				"seat_weight": 2,
				"stance": "Fiscal Conservative",
				"has_defected": false
			},
			{
				"id": "lawmaker_sterling",
				"name": "Hon. Clara Sterling",
				"constituency": "Capital District",
				"current_party": "Ruling",
				"seat_weight": 1,
				"stance": "Centrist Technocrat",
				"has_defected": true,
				"defection_incentive": "District Development Grant (Technocratic)"
			}
		]
	}
	exec_instance._update_policy_and_bills_modal(mock_lawmakers_state)
	
	if lmk_vbox and lmk_vbox.get_child_count() == 2:
		var thorne_card = lmk_vbox.get_child(0)
		var sterling_card = lmk_vbox.get_child(1)
		
		# Verify buttons on opposition lawmaker (Thorne)
		var thorne_buttons = thorne_card.find_children("", "Button", true, false)
		# Verify badge on defected lawmaker (Sterling)
		var sterling_labels = sterling_card.find_children("", "Label", true, false)
		var has_ally_badge := false
		for lbl in sterling_labels:
			if lbl.text.contains("Party Ally"):
				has_ally_badge = true
				break
				
		if thorne_buttons.size() == 3 and has_ally_badge:
			print("  ✓ Hon. Marcus Thorne has 3 courtship choices (Grant, Committee Chair, Patronage)")
			print("  ✓ Hon. Clara Sterling displays '✅ Party Ally (District Development Grant)'")
			passed_checks += 1
		else:
			push_error("  ❌ Lawmaker card courtship options / ally badge mismatch")
	else:
		push_error("  ❌ Failed to render lawmaker cards in LawmakersVBox")

	# -------------------------------------------------------------------------
	# CHECK 6: Project Initiation Handler Dispatch
	# -------------------------------------------------------------------------
	print("\n--- CHECK 6: Project Initiation Handlers Dispatch ---")
	exec_instance._initiate_project("HighSpeedRail")
	if exec_instance.dev_status_label and exec_instance.dev_status_label.text.contains("HighSpeedRail"):
		print("  ✓ Project initiation request dispatched cleanly: '%s'" % exec_instance.dev_status_label.text)
		passed_checks += 1
	else:
		push_error("  ❌ Project initiation failed to dispatch: '%s'" % (exec_instance.dev_status_label.text if exec_instance.dev_status_label else "null"))

	# -------------------------------------------------------------------------
	# CHECK 7: Lawmaker Courtship Dispatch Handler
	# -------------------------------------------------------------------------
	print("\n--- CHECK 7: Lawmaker Courtship Dispatch Handler ---")
	exec_instance._on_court_lawmaker_pressed("lawmaker_thorne", "district_grant")
	if exec_instance.pol_status_label and exec_instance.pol_status_label.text.contains("district_grant"):
		print("  ✓ Lawmaker courtship request dispatched cleanly: '%s'" % exec_instance.pol_status_label.text)
		passed_checks += 1
	else:
		push_error("  ❌ Lawmaker courtship failed to dispatch: '%s'" % (exec_instance.pol_status_label.text if exec_instance.pol_status_label else "null"))

	exec_instance.queue_free()

	print("\n=================================================================")
	if passed_checks == total_checks:
		print("🎉 ALL %d/%d LEGISLATURE, PROJECT & GENDER CHECKS PASSED!" % [passed_checks, total_checks])
		print("=================================================================")
		quit(0)
	else:
		push_error("❌ SUITE FAILED: Passed %d/%d checks" % [passed_checks, total_checks])
		print("=================================================================")
		quit(1)
