# ==============================================================================
# Republic - Executive Office Customisation Catalog (Phase 3)
# Manages catalogue definitions for portraits, flags, modular furniture, and
# interior design options with authoritative economy checking and persistence.
# ==============================================================================
class_name ExecutiveOfficeCatalog
extends RefCounted

const DEFAULT_CUSTOMIZATION_PATH := "user://executive_office_customization.json"
const FALLBACK_CUSTOMIZATION_PATH := "res://executive_office_customization.json"

# ==============================================================================
# 1. PRESIDENTIAL PORTRAITS (5 MALE, 3 FEMALE)
# ==============================================================================
const PORTRAITS := [
	{
		"id": "portrait_m1",
		"name": "Alexander Vance",
		"gender": "male",
		"archetype": "The Statesman",
		"description": "Experienced diplomat with a focus on constitutional governance and civic order.",
		"icon": "🏛",
		"accent_color": Color(0.15, 0.22, 0.36, 1.0),
		"suit_color": Color(0.10, 0.12, 0.18, 1.0)
	},
	{
		"id": "portrait_m2",
		"name": "Marcus Sterling",
		"gender": "male",
		"archetype": "The Reformist",
		"description": "Fiscal technocrat dedicated to civil service modernisation and anti-corruption.",
		"icon": "📈",
		"accent_color": Color(0.20, 0.22, 0.25, 1.0),
		"suit_color": Color(0.12, 0.14, 0.16, 1.0)
	},
	{
		"id": "portrait_m3",
		"name": "David Chen",
		"gender": "male",
		"archetype": "The Technocrat",
		"description": "Industrial planner focused on strategic energy grids and national infrastructure.",
		"icon": "⚙",
		"accent_color": Color(0.12, 0.28, 0.32, 1.0),
		"suit_color": Color(0.08, 0.14, 0.18, 1.0)
	},
	{
		"id": "portrait_m4",
		"name": "Samuel Adebayo",
		"gender": "male",
		"archetype": "The Peacemaker",
		"description": "Veteran coalition builder focused on regional reconciliation and national stability.",
		"icon": "🕊",
		"accent_color": Color(0.14, 0.30, 0.20, 1.0),
		"suit_color": Color(0.09, 0.15, 0.11, 1.0)
	},
	{
		"id": "portrait_m5",
		"name": "Viktor Lindqvist",
		"gender": "male",
		"archetype": "The Constitutionalist",
		"description": "Senior jurist devoted to parliamentary oversight and independent justice.",
		"icon": "⚖",
		"accent_color": Color(0.32, 0.15, 0.18, 1.0),
		"suit_color": Color(0.15, 0.10, 0.12, 1.0)
	},
	{
		"id": "portrait_f1",
		"name": "Elena Rostova",
		"gender": "female",
		"archetype": "The Economist",
		"description": "Central banking authority with deep expertise in monetary policy and capital liquidity.",
		"icon": "💼",
		"accent_color": Color(0.28, 0.16, 0.35, 1.0),
		"suit_color": Color(0.14, 0.09, 0.18, 1.0)
	},
	{
		"id": "portrait_f2",
		"name": "Amina Al-Mansoor",
		"gender": "female",
		"archetype": "The Visionary",
		"description": "Pioneering diplomat championing green modernization and scientific research hubs.",
		"icon": "🌟",
		"accent_color": Color(0.12, 0.26, 0.42, 1.0),
		"suit_color": Color(0.08, 0.12, 0.22, 1.0)
	},
	{
		"id": "portrait_f3",
		"name": "Victoria Mercer",
		"gender": "female",
		"archetype": "The Civic Leader",
		"description": "Grassroots reformer committed to healthcare access, basic education and labor welfare.",
		"icon": "🤝",
		"accent_color": Color(0.35, 0.14, 0.18, 1.0),
		"suit_color": Color(0.18, 0.08, 0.10, 1.0)
	}
]

# ==============================================================================
# 2. NATIONAL FLAG STYLES
# ==============================================================================
const FLAGS := [
	{
		"id": "flag_navy_gold",
		"name": "Arcadia Sovereign (Navy & Gold)",
		"banner_color": Color(0.12, 0.22, 0.38, 1.0),
		"emblem_color": Color(0.95, 0.85, 0.40, 1.0),
		"emblem_symbol": "★"
	},
	{
		"id": "flag_emerald_white",
		"name": "Solaria Federal (Emerald & Gold)",
		"banner_color": Color(0.12, 0.38, 0.22, 1.0),
		"emblem_color": Color(0.95, 0.90, 0.50, 1.0),
		"emblem_symbol": "☀"
	},
	{
		"id": "flag_crimson_gold",
		"name": "Valoria Commonwealth (Crimson & Gold)",
		"banner_color": Color(0.42, 0.12, 0.16, 1.0),
		"emblem_color": Color(0.95, 0.82, 0.35, 1.0),
		"emblem_symbol": "⚔"
	},
	{
		"id": "flag_azure_silver",
		"name": "Veridia Union (Azure & Silver)",
		"banner_color": Color(0.18, 0.32, 0.52, 1.0),
		"emblem_color": Color(0.90, 0.94, 0.98, 1.0),
		"emblem_symbol": "⚖"
	},
	{
		"id": "flag_obsidian_bronze",
		"name": "Obsidian Republic (Charcoal & Bronze)",
		"banner_color": Color(0.16, 0.16, 0.20, 1.0),
		"emblem_color": Color(0.85, 0.65, 0.35, 1.0),
		"emblem_symbol": "🛡"
	}
]

# ==============================================================================
# 3. FURNITURE & INTERIOR DESIGN CATEGORIES
# ==============================================================================
const CATEGORIES := [
	{ "id": "portraits", "name": "Portraits & Flag", "icon": "👑" },
	{ "id": "desk_and_chair", "name": "Desks & Chairs", "icon": "🪑" },
	{ "id": "visitor_seating", "name": "Visitor Seating", "icon": "🛋" },
	{ "id": "wall_and_flooring", "name": "Walls & Floors", "icon": "🧱" },
	{ "id": "carpets_and_lighting", "name": "Carpets & Lighting", "icon": "💡" },
	{ "id": "bookshelves_and_decor", "name": "Bookshelves & Decor", "icon": "📚" },
	{ "id": "artwork_and_gifts", "name": "Artwork & Gifts", "icon": "🖼" }
]

const ITEMS := [
	# --------------------------------------------------------------------------
	# Category: Desks & Chairs
	# --------------------------------------------------------------------------
	{
		"id": "desk_mahogany_classic",
		"category": "desk_and_chair",
		"name": "Mahogany Desk & Leather Highback",
		"description": "Traditional executive mahogany desk with oxblood blotter and brass stanchions.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"surface_color": Color(0.24, 0.12, 0.08, 1.0),
		"blotter_color": Color(0.32, 0.08, 0.08, 1.0),
		"chair_color": Color(0.28, 0.10, 0.08, 1.0)
	},
	{
		"id": "desk_obsidian_modern",
		"category": "desk_and_chair",
		"name": "Obsidian Glass & Ergonomic Mesh",
		"description": "Minimalist smoked obsidian glass desk with chrome trim and precision lumbar chair.",
		"price_repu": 350000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "Immediate",
		"surface_color": Color(0.12, 0.14, 0.18, 1.0),
		"blotter_color": Color(0.20, 0.24, 0.30, 1.0),
		"chair_color": Color(0.15, 0.18, 0.22, 1.0)
	},
	{
		"id": "desk_heritage_oak",
		"category": "desk_and_chair",
		"name": "Heritage Oak & Velvet Throne",
		"description": "Hand-carved century oak desk with emerald blotter and gold-embroidered velvet chair.",
		"price_repu": 750000,
		"approval_required": "Ministry of Interior",
		"install_time_wat": "1 Simulation Day",
		"surface_color": Color(0.42, 0.26, 0.14, 1.0),
		"blotter_color": Color(0.10, 0.28, 0.16, 1.0),
		"chair_color": Color(0.45, 0.15, 0.15, 1.0)
	},

	# --------------------------------------------------------------------------
	# Category: Visitor Seating & Meeting Table
	# --------------------------------------------------------------------------
	{
		"id": "visitor_leather_armchairs",
		"category": "visitor_seating",
		"name": "Executive Leather Armchairs",
		"description": "Pair of deep-tufted burgundy leather armchairs positioned for presidential briefings.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"seating_color": Color(0.35, 0.10, 0.10, 1.0),
		"table_color": Color(0.25, 0.12, 0.08, 1.0)
	},
	{
		"id": "visitor_teak_conference",
		"category": "visitor_seating",
		"name": "Teak Conference Table & Chairs",
		"description": "Polished circular teak table with 4 ergonomic chairs for cabinet sub-committees.",
		"price_repu": 450000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "Immediate",
		"seating_color": Color(0.18, 0.22, 0.28, 1.0),
		"table_color": Color(0.45, 0.30, 0.16, 1.0)
	},
	{
		"id": "visitor_lounge_suite",
		"category": "visitor_seating",
		"name": "Chesterfield Diplomatic Lounge",
		"description": "Classic low-profile leather chesterfield sofa with brass-inlaid walnut coffee table.",
		"price_repu": 850000,
		"approval_required": "Ministry of Interior",
		"install_time_wat": "1 Simulation Day",
		"seating_color": Color(0.22, 0.16, 0.12, 1.0),
		"table_color": Color(0.38, 0.24, 0.14, 1.0)
	},

	# --------------------------------------------------------------------------
	# Category: Walls & Flooring
	# --------------------------------------------------------------------------
	{
		"id": "wall_slate_parquet",
		"category": "wall_and_flooring",
		"name": "Midnight Slate & Polished Parquet",
		"description": "Deep blue acoustic slate wall panels framed with satin oak parquet flooring.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"wall_color": Color(0.08, 0.11, 0.16, 1.0),
		"floor_color": Color(0.24, 0.16, 0.10, 1.0)
	},
	{
		"id": "wall_marble_herringbone",
		"category": "wall_and_flooring",
		"name": "Neoclassical Marble & Herringbone",
		"description": "White Carrara marble wall pilasters matched with dark walnut herringbone flooring.",
		"price_repu": 600000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "1 Simulation Day",
		"wall_color": Color(0.22, 0.24, 0.28, 1.0),
		"floor_color": Color(0.18, 0.12, 0.08, 1.0)
	},
	{
		"id": "wall_walnut_granite",
		"category": "wall_and_flooring",
		"name": "Imperial Walnut & Black Granite",
		"description": "Bookmatched warm walnut wainscotting over polished mirror-finish black granite slabs.",
		"price_repu": 1200000,
		"approval_required": "Ministry of Interior",
		"install_time_wat": "2 Simulation Days",
		"wall_color": Color(0.25, 0.14, 0.08, 1.0),
		"floor_color": Color(0.06, 0.07, 0.09, 1.0)
	},

	# --------------------------------------------------------------------------
	# Category: Carpets & Lighting
	# --------------------------------------------------------------------------
	{
		"id": "carpet_crimson_state",
		"category": "carpets_and_lighting",
		"name": "Crimson State Rug & Brass Chandelier",
		"description": "Plush crimson wool rug with gold greek key border and 6-arm warm brass chandelier.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"carpet_color": Color(0.42, 0.10, 0.12, 0.85),
		"light_color": Color(1.0, 0.88, 0.65, 0.9)
	},
	{
		"id": "carpet_navy_crest",
		"category": "carpets_and_lighting",
		"name": "Navy Sovereign Crest & Recessed LEDs",
		"description": "Midnight navy carpet emblazoned with sovereign laurel crest and warm perimeter LED coves.",
		"price_repu": 280000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "Immediate",
		"carpet_color": Color(0.10, 0.16, 0.28, 0.85),
		"light_color": Color(0.92, 0.95, 1.0, 0.9)
	},
	{
		"id": "carpet_emerald_runner",
		"category": "carpets_and_lighting",
		"name": "Emerald Silk Runner & Modern Pendants",
		"description": "Hand-knotted jade runner with brushed aluminum minimalist pendant luminaires.",
		"price_repu": 520000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "1 Simulation Day",
		"carpet_color": Color(0.10, 0.28, 0.18, 0.85),
		"light_color": Color(0.98, 0.95, 0.85, 0.9)
	},

	# --------------------------------------------------------------------------
	# Category: Bookshelves & Decorations
	# --------------------------------------------------------------------------
	{
		"id": "decor_legal_archives",
		"category": "bookshelves_and_decor",
		"name": "Constitutional Archives & Bronze Scales",
		"description": "Built-in mahogany law library containing historical gazettes and judicial balance scales.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"decor_symbol": "📚",
		"decor_title": "CONSTITUTIONAL ARCHIVES",
		"decor_color": Color(0.24, 0.12, 0.08, 1.0)
	},
	{
		"id": "decor_global_cartography",
		"category": "bookshelves_and_decor",
		"name": "Cartography Shelves & Terrestrial Globe",
		"description": "Topographical maps, historical gazetteers, and illuminated revolving globe on stand.",
		"price_repu": 320000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "Immediate",
		"decor_symbol": "🌐",
		"decor_title": "WORLD CARTOGRAPHY",
		"decor_color": Color(0.18, 0.24, 0.32, 1.0)
	},
	{
		"id": "decor_modernist_sculpture",
		"category": "bookshelves_and_decor",
		"name": "Architectural Niches & Bronze Monolith",
		"description": "Illuminated wall niches presenting contemporary bronze civic sculpture and medals.",
		"price_repu": 680000,
		"approval_required": "Ministry of Interior",
		"install_time_wat": "1 Simulation Day",
		"decor_symbol": "🗿",
		"decor_title": "CIVIC MONOLITH",
		"decor_color": Color(0.28, 0.22, 0.16, 1.0)
	},

	# --------------------------------------------------------------------------
	# Category: National Artwork & Diplomatic Gifts
	# --------------------------------------------------------------------------
	{
		"id": "art_founding_charter",
		"category": "artwork_and_gifts",
		"name": "Founding Constitutional Charter",
		"description": "Archival preservation case displaying the original declaration of sovereign independence.",
		"price_repu": 0,
		"approval_required": "None",
		"install_time_wat": "Immediate",
		"art_symbol": "📜",
		"art_title": "FOUNDING CHARTER",
		"art_color": Color(0.85, 0.72, 0.35, 1.0)
	},
	{
		"id": "art_sovereign_landscape",
		"category": "artwork_and_gifts",
		"name": "Sovereign Landscapes Oil Canvas",
		"description": "Grand masterwork oil painting capturing the national capital river basin and horizon.",
		"price_repu": 400000,
		"approval_required": "Presidential Facilities",
		"install_time_wat": "Immediate",
		"art_symbol": "🌄",
		"art_title": "SOVEREIGN VISTA",
		"art_color": Color(0.40, 0.55, 0.70, 1.0)
	},
	{
		"id": "art_diplomatic_tributes",
		"category": "artwork_and_gifts",
		"name": "Diplomatic Tributes & Silver Urns",
		"description": "Secured vitrine presenting bilateral gifts and ceremonial chalices from allied heads of state.",
		"price_repu": 950000,
		"approval_required": "Ministry of Interior",
		"install_time_wat": "1 Simulation Day",
		"art_symbol": "🏺",
		"art_title": "DIPLOMATIC GIFTS",
		"art_color": Color(0.78, 0.82, 0.88, 1.0)
	}
]

# ==============================================================================
# 4. QUERY METHODS
# ==============================================================================
static func get_portraits() -> Array:
	return PORTRAITS

static func get_portrait(portrait_id: String) -> Dictionary:
	for p in PORTRAITS:
		if p.id == portrait_id:
			return p
	return PORTRAITS[0]

static func get_flags() -> Array:
	return FLAGS

static func get_flag(flag_id: String) -> Dictionary:
	for f in FLAGS:
		if f.id == flag_id:
			return f
	return FLAGS[0]

static func get_categories() -> Array:
	return CATEGORIES

static func get_items_by_category(category_id: String) -> Array:
	var result: Array = []
	for it in ITEMS:
		if it.category == category_id:
			result.append(it)
	return result

static func get_item(item_id: String) -> Dictionary:
	for it in ITEMS:
		if it.id == item_id:
			return it
	return {}

# ==============================================================================
# 5. AUTHORITATIVE ECONOMY & AVAILABILITY VALIDATION
# ==============================================================================
static func check_item_availability(item: Dictionary, latest_state: Dictionary, owned_items: Array) -> Dictionary:
	var item_id: String = item.get("id", "")
	var price: int = int(item.get("price_repu", 0))
	var approval: String = item.get("approval_required", "None")

	# 1. Check if already owned
	if owned_items.has(item_id) or price == 0:
		return {
			"status": "OWNED",
			"can_purchase": false,
			"can_install": true,
			"reason": "Acquired and ready for installation."
		}

	# 2. Check authoritative treasury balance (zero mock data)
	var treasury: float = float(latest_state.get("treasury_balance", 0.0))
	if latest_state.is_empty():
		return {
			"status": "LOCKED",
			"can_purchase": false,
			"can_install": false,
			"reason": "Cannot purchase: No authoritative Republic simulation state loaded."
		}

	if treasury < float(price):
		return {
			"status": "LOCKED",
			"can_purchase": false,
			"can_install": false,
			"reason": "Insufficient treasury: Requires R %s (Current liquid balance: R %s)." % [_format_commas(price), _format_commas(int(treasury))]
		}

	# 3. Check Ministry of Interior approval requirement
	if approval == "Ministry of Interior":
		var int_min: String = str(latest_state.get("interior_minister", "Vacant"))
		if int_min == "Vacant" or int_min.is_empty():
			return {
				"status": "LOCKED",
				"can_purchase": false,
				"can_install": false,
				"reason": "Cabinet Approval Required: Ministry of Interior portfolio is vacant."
			}

	# 4. Otherwise available to purchase
	return {
		"status": "AVAILABLE",
		"can_purchase": true,
		"can_install": false,
		"reason": "Available for purchase from Presidential Facilities budget."
	}

# ==============================================================================
# 6. PERSISTENCE (SAVE & LOAD CHOICES)
# ==============================================================================
static func get_default_customization() -> Dictionary:
	return {
		"version": 1,
		"country_id": "player-country",
		"selected_portrait_id": "portrait_m1",
		"selected_flag_id": "flag_navy_gold",
		"installed_items": {
			"desk_and_chair": "desk_mahogany_classic",
			"visitor_seating": "visitor_leather_armchairs",
			"wall_and_flooring": "wall_slate_parquet",
			"carpets_and_lighting": "carpet_crimson_state",
			"bookshelves_and_decor": "decor_legal_archives",
			"artwork_and_gifts": "art_founding_charter"
		},
		"owned_items": [
			"desk_mahogany_classic",
			"visitor_leather_armchairs",
			"wall_slate_parquet",
			"carpet_crimson_state",
			"decor_legal_archives",
			"art_founding_charter"
		],
		"saved_at_utc": Time.get_datetime_string_from_system(true)
	}

static func resolve_customization_path(custom_path: String = "") -> String:
	if not custom_path.is_empty():
		return custom_path
	if FileAccess.file_exists(DEFAULT_CUSTOMIZATION_PATH):
		return DEFAULT_CUSTOMIZATION_PATH
	if FileAccess.file_exists(FALLBACK_CUSTOMIZATION_PATH):
		return FALLBACK_CUSTOMIZATION_PATH
	return DEFAULT_CUSTOMIZATION_PATH

static func load_customization(custom_path: String = "", target_country_id: String = "") -> Dictionary:
	var path := resolve_customization_path(custom_path)
	if not FileAccess.file_exists(path):
		var default_data := get_default_customization()
		if not target_country_id.is_empty():
			default_data["country_id"] = target_country_id
		return default_data

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return get_default_customization()

	var text := file.get_as_text()
	file.close()

	var parsed: Variant = JSON.parse_string(text)
	if not (parsed is Dictionary):
		return get_default_customization()

	var data: Dictionary = parsed
	# If per-country map is stored
	if data.has("countries") and data["countries"] is Dictionary and not target_country_id.is_empty():
		var country_map: Dictionary = data["countries"]
		if country_map.has(target_country_id) and country_map[target_country_id] is Dictionary:
			return country_map[target_country_id]

	# Fallback to top-level customization dict
	return data

static func save_customization(customization: Dictionary, custom_path: String = "", target_country_id: String = "") -> bool:
	var path := custom_path if not custom_path.is_empty() else DEFAULT_CUSTOMIZATION_PATH
	customization["saved_at_utc"] = Time.get_datetime_string_from_system(true)
	if not target_country_id.is_empty():
		customization["country_id"] = target_country_id

	# Also store in country-indexed map if available
	var root_data: Dictionary = {}
	if FileAccess.file_exists(path):
		var existing_file := FileAccess.open(path, FileAccess.READ)
		if existing_file:
			var txt := existing_file.get_as_text()
			existing_file.close()
			var parsed: Variant = JSON.parse_string(txt)
			if parsed is Dictionary:
				root_data = parsed

	var countries: Dictionary = root_data.get("countries", {})
	var c_id := target_country_id if not target_country_id.is_empty() else str(customization.get("country_id", "player-country"))
	countries[c_id] = customization.duplicate(true)

	root_data["countries"] = countries
	# Keep top-level aligned with latest saved country
	for k in customization.keys():
		root_data[k] = customization[k]

	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		# Fallback to local project folder if user:// is restricted
		var fallback_file := FileAccess.open(FALLBACK_CUSTOMIZATION_PATH, FileAccess.WRITE)
		if fallback_file:
			fallback_file.store_string(JSON.stringify(root_data, "\t"))
			fallback_file.close()
			return true
		return false

	file.store_string(JSON.stringify(root_data, "\t"))
	file.close()
	return true

static func _format_commas(val: int) -> String:
	var s := str(val)
	var formatted := ""
	var count := 0
	for i in range(s.length() - 1, -1, -1):
		if count > 0 and count % 3 == 0:
			formatted = "," + formatted
		formatted = s[i] + formatted
		count += 1
	return formatted
