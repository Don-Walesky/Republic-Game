extends SceneTree

## Automated Focused Test Suite for Notification and Message Read/Unread Persistence
## Verifies:
## 1. Initial unread state and badge visibility.
## 2. Reading a notification or message and verifying the badge clears.
## 3. Reloading the scene or restarting and verifying read state persists.
## 4. Receiving a new item and verifying the badge increments correctly.
## 5. Unread badge count matches the actual number of unread items.
## 6. Cleanup of test files.

const SCENE_PATH := "res://ExecutiveOffice.tscn"
const TEST_CUSTOM_PATH := "user://test_notification_persistence_custom.json"
const TEST_LEDGER_PATH := "user://test_notification_persistence_custom_notifications.json"

func _init() -> void:
	print("==============================================================")
	print("   REPUBLIC - NOTIFICATION READ/UNREAD PERSISTENCE TEST      ")
	print("==============================================================")

	_cleanup_test_files()

	var scene := load(SCENE_PATH) as PackedScene
	if scene == null:
		printerr("[FAIL]: Could not load res://ExecutiveOffice.tscn")
		quit(1)
		return

	var all_passed := true
	all_passed = test_notification_read_and_clear(scene) and all_passed
	all_passed = test_read_state_persistence_across_reload(scene) and all_passed
	all_passed = test_new_item_increments_badge(scene) and all_passed

	_cleanup_test_files()

	if all_passed:
		print("==============================================================")
		print(" [PASS]: ALL NOTIFICATION PERSISTENCE CHECKS PASSED!          ")
		print("==============================================================")
		quit(0)
	else:
		printerr("==============================================================")
		printerr(" [FAIL]: ONE OR MORE NOTIFICATION PERSISTENCE CHECKS FAILED!  ")
		printerr("==============================================================")
		quit(1)

func _cleanup_test_files() -> void:
	var dir := DirAccess.open("user://")
	if dir:
		if dir.file_exists("test_notification_persistence_custom.json"):
			dir.remove("test_notification_persistence_custom.json")
		if dir.file_exists("test_notification_persistence_custom_notifications.json"):
			dir.remove("test_notification_persistence_custom_notifications.json")

func _create_test_state() -> Dictionary:
	return {
		"country_name": "Republic of Arcadia",
		"country_id": "test-persistence-country",
		"treasury_balance": 50000000.0,
		"approval_rating": 62.0,
		"stability_rating": 75.0,
		"emails": [
			{
				"id": "mail_test_1",
				"sender": "Speaker of the House",
				"subject": "Legislative Agenda",
				"body": "First session begins tomorrow.",
				"is_read": false
			},
			{
				"id": "mail_test_2",
				"sender": "Chief of Staff",
				"subject": "Daily Briefing",
				"body": "Daily briefing memorandum attached.",
				"is_read": false
			},
			{
				"id": "mail_test_3",
				"sender": "Defense Secretary",
				"subject": "Border Posture",
				"body": "Defense posture routine.",
				"is_read": true
			}
		],
		"unread_email_count": 2,
		"chief_of_staff_recommendations": [
			{
				"id": "rec_test_1",
				"title": "Port Modernization",
				"status": "Pending",
				"priority": "High"
			},
			{
				"id": "rec_test_2",
				"title": "Fiscal Audit",
				"status": "Pending",
				"priority": "Medium"
			}
		],
		"latest_decision_result": "Pending presidential directive"
	}

func test_notification_read_and_clear(scene: PackedScene) -> bool:
	print("\n--- Check 1: Reading Notifications & Verifying Badges Clear ---")
	var office = scene.instantiate()
	office.custom_save_path = TEST_CUSTOM_PATH
	office.notification_save_path = TEST_LEDGER_PATH
	root.add_child(office)
	office._ready()

	var state := _create_test_state()
	office.apply_state(state)

	# 1. Verify initial unread email count
	var unread_emails: int = office.get_unread_email_count(state)
	if unread_emails != 2:
		printerr("  [FAIL]: Expected 2 unread emails initially. Got: ", unread_emails)
		office.queue_free()
		return false

	if office.chat_unread_badge == null or not office.chat_unread_badge.visible:
		printerr("  [FAIL]: Chat unread badge should be visible with 2 unread emails.")
		office.queue_free()
		return false

	if office.chat_unread_count_label.text != "2":
		printerr("  [FAIL]: Chat badge count label should show '2'. Got: ", office.chat_unread_count_label.text)
		office.queue_free()
		return false

	# 2. Verify initial unread inbox badge
	if not office.is_inbox_badge_visible():
		printerr("  [FAIL]: Inbox badge should be visible with pending recommendations.")
		office.queue_free()
		return false

	# 3. Read emails via chat shortcut and verify chat badge clears
	office._on_chat_shortcut_pressed()
	if office.chat_unread_badge.visible:
		printerr("  [FAIL]: Chat unread badge should clear after opening chat.")
		office.queue_free()
		return false

	if office.get_unread_email_count() != 0:
		printerr("  [FAIL]: Unread email count should be 0 after reading all emails. Got: ", office.get_unread_email_count())
		office.queue_free()
		return false

	# 4. Read inbox via inbox button and verify inbox badge clears
	office._on_executive_inbox_pressed()
	if office.is_inbox_badge_visible():
		printerr("  [FAIL]: Inbox badge should clear after opening executive inbox.")
		office.queue_free()
		return false

	office.queue_free()
	print("  [OK]: Reading emails and inbox cleared respective badges and updated counts.")
	return true

func test_read_state_persistence_across_reload(scene: PackedScene) -> bool:
	print("\n--- Check 2: Verifying Read State Persists Across Scene Reload ---")
	# Simulate fresh scene instantiation / reload
	var office = scene.instantiate()
	office.custom_save_path = TEST_CUSTOM_PATH
	office.notification_save_path = TEST_LEDGER_PATH
	root.add_child(office)
	office._ready()

	# Re-apply the exact same state that was previously read
	var state := _create_test_state()
	office.apply_state(state)

	# Verify previously read emails STAY read
	var unread_emails: int = office.get_unread_email_count(state)
	if unread_emails != 0:
		printerr("  [FAIL]: Expected 0 unread emails on scene reload. Got: ", unread_emails)
		office.queue_free()
		return false

	if office.chat_unread_badge and office.chat_unread_badge.visible:
		printerr("  [FAIL]: Chat badge spuriously reappeared after scene reload!")
		office.queue_free()
		return false

	# Verify previously read inbox stays dismissed
	if office.is_inbox_badge_visible():
		printerr("  [FAIL]: Inbox badge spuriously reappeared after scene reload!")
		office.queue_free()
		return false

	office.queue_free()
	print("  [OK]: Previously read state persisted across reload; badges remained cleared.")
	return true

func test_new_item_increments_badge(scene: PackedScene) -> bool:
	print("\n--- Check 3: Receiving New Unread Items Increments Badges Correctly ---")
	var office = scene.instantiate()
	office.custom_save_path = TEST_CUSTOM_PATH
	office.notification_save_path = TEST_LEDGER_PATH
	root.add_child(office)
	office._ready()

	# Start with previously read state
	var state := _create_test_state()
	office.apply_state(state)

	# Verify it starts with 0 unread
	if office.get_unread_email_count(state) != 0:
		printerr("  [FAIL]: Precondition failed: unread email count should start at 0.")
		office.queue_free()
		return false

	# 1. Add a single new unread email
	var updated_emails: Array = (state["emails"] as Array).duplicate(true)
	updated_emails.append({
		"id": "mail_new_urgent_1",
		"sender": "Foreign Ministry",
		"subject": "Urgent Diplomatic Cable",
		"body": "Neighboring republic requests emergency summit.",
		"is_read": false
	})
	state["emails"] = updated_emails
	state["unread_email_count"] = 1

	office.apply_state(state)

	# Verify chat unread count increments to exactly 1
	var unread_count: int = office.get_unread_email_count(state)
	if unread_count != 1:
		printerr("  [FAIL]: Expected exactly 1 unread email after receiving new email. Got: ", unread_count)
		office.queue_free()
		return false

	if office.chat_unread_badge == null or not office.chat_unread_badge.visible:
		printerr("  [FAIL]: Chat badge should become visible when new email arrives.")
		office.queue_free()
		return false

	if office.chat_unread_count_label.text != "1":
		printerr("  [FAIL]: Chat count label should show '1'. Got: ", office.chat_unread_count_label.text)
		office.queue_free()
		return false

	# 2. Add a new unread recommendation to the inbox
	var updated_recs: Array = (state["chief_of_staff_recommendations"] as Array).duplicate(true)
	updated_recs.append({
		"id": "rec_test_new_crisis",
		"title": "Emergency Healthcare Grant",
		"status": "Pending",
		"priority": "Urgent"
	})
	state["chief_of_staff_recommendations"] = updated_recs

	office.apply_state(state)

	if not office.is_inbox_badge_visible():
		printerr("  [FAIL]: Inbox badge should become visible when new recommendation arrives.")
		office.queue_free()
		return false

	if office.inbox_badge_label.text != "1":
		printerr("  [FAIL]: Inbox badge label should show '1' for single new unread item. Got: ", office.inbox_badge_label.text)
		office.queue_free()
		return false

	# 3. Read the new items and verify they clear again
	office._on_chat_shortcut_pressed()
	if office.chat_unread_badge.visible:
		printerr("  [FAIL]: Chat badge failed to clear after reading new email.")
		office.queue_free()
		return false

	office._on_executive_inbox_pressed()
	if office.is_inbox_badge_visible():
		printerr("  [FAIL]: Inbox badge failed to clear after reading new recommendation.")
		office.queue_free()
		return false

	office.queue_free()
	print("  [OK]: New items correctly increment badges, and clear upon reading.")
	return true

