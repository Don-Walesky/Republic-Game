@tool
class_name FlagRenderer
extends Control

## Procedural Flag Renderer for Republic Sovereign Nations.
## Provides vexillological patterns, statecraft color palettes, and presidential charges/emblems.
## Used in Onboarding (Flag Creator) and Executive Office (Standing Office Flag Standard).

@export var pattern: String = "horizontal_tricolor":
	set(v):
		pattern = v
		queue_redraw()

@export var primary_color: Color = Color(0.08, 0.18, 0.38):
	set(v):
		primary_color = v
		queue_redraw()

@export var secondary_color: Color = Color(0.96, 0.97, 0.98):
	set(v):
		secondary_color = v
		queue_redraw()

@export var accent_color: Color = Color(0.85, 0.72, 0.28):
	set(v):
		accent_color = v
		queue_redraw()

@export var emblem: String = "star":
	set(v):
		emblem = v
		queue_redraw()

@export var show_fringe: bool = false:
	set(v):
		show_fringe = v
		queue_redraw()

const PATTERNS: Array[String] = [
	"horizontal_tricolor",
	"vertical_tricolor",
	"bicolor_horizontal",
	"sovereign_cross",
	"canton_star",
	"solid_emblem"
]

const EMBLEMS: Array[String] = [
	"star",
	"eagle",
	"sun",
	"temple",
	"shield",
	"sabres",
	"wreath",
	"none"
]

const PALETTE: Array[Color] = [
	Color(0.08, 0.18, 0.38), # 0: Deep Sovereign Navy
	Color(0.60, 0.12, 0.15), # 1: Crimson Red
	Color(0.11, 0.35, 0.20), # 2: Emerald Green
	Color(0.88, 0.72, 0.22), # 3: Imperial Gold
	Color(0.96, 0.97, 0.98), # 4: Pure Ivory White
	Color(0.12, 0.14, 0.18), # 5: Obsidian Slate
	Color(0.18, 0.46, 0.75), # 6: Sky Blue
	Color(0.78, 0.38, 0.12), # 7: Saffron Amber
	Color(0.32, 0.14, 0.44), # 8: Royal Amethyst
]

func _ready() -> void:
	queue_redraw()

func set_flag_data(d: Dictionary) -> void:
	if d.is_empty(): return
	if d.has("pattern"): pattern = str(d["pattern"])
	if d.has("primary_color"): primary_color = _parse_color(d["primary_color"], primary_color)
	if d.has("secondary_color"): secondary_color = _parse_color(d["secondary_color"], secondary_color)
	if d.has("accent_color"): accent_color = _parse_color(d["accent_color"], accent_color)
	if d.has("emblem"): emblem = str(d["emblem"])
	queue_redraw()

func to_flag_data() -> Dictionary:
	return {
		"pattern": pattern,
		"primary_color": primary_color.to_html(false),
		"secondary_color": secondary_color.to_html(false),
		"accent_color": accent_color.to_html(false),
		"emblem": emblem
	}

func randomize_flag() -> void:
	var rng := RandomNumberGenerator.new()
	rng.randomize()
	pattern = PATTERNS[rng.randi() % PATTERNS.size()]
	
	# Pick contrasting colors
	var p_idx := rng.randi() % PALETTE.size()
	primary_color = PALETTE[p_idx]
	
	var s_idx := (p_idx + 1 + (rng.randi() % (PALETTE.size() - 1))) % PALETTE.size()
	secondary_color = PALETTE[s_idx]
	
	# Accent is usually gold or white or crimson for visibility
	var accent_options := [
		Color(0.88, 0.72, 0.22), # Gold
		Color(0.96, 0.97, 0.98), # White
		Color(0.60, 0.12, 0.15), # Crimson
		Color(0.08, 0.18, 0.38)  # Navy
	]
	accent_color = accent_options[rng.randi() % accent_options.size()]
	
	emblem = EMBLEMS[rng.randi() % EMBLEMS.size()]
	queue_redraw()

func _parse_color(val, default_val: Color) -> Color:
	if val is Color: return val
	if val is String:
		if val.begins_with("#"):
			return Color.from_string(val, default_val)
		elif val.begins_with("Color"):
			return Color.from_string(val.replace("Color(", "").replace(")", ""), default_val)
		else:
			return Color.from_string("#" + val, default_val)
	return default_val

func _draw() -> void:
	var w := size.x
	var h := size.y
	if w <= 0 or h <= 0: return

	# 1. Base Field
	draw_rect(Rect2(0, 0, w, h), primary_color)

	# 2. Pattern Layer
	match pattern:
		"horizontal_tricolor":
			draw_rect(Rect2(0, 0, w, h / 3.0), primary_color)
			draw_rect(Rect2(0, h / 3.0, w, h / 3.0), secondary_color)
			draw_rect(Rect2(0, (h * 2.0) / 3.0, w, h / 3.0), accent_color)

		"vertical_tricolor":
			draw_rect(Rect2(0, 0, w / 3.0, h), primary_color)
			draw_rect(Rect2(w / 3.0, 0, w / 3.0, h), secondary_color)
			draw_rect(Rect2((w * 2.0) / 3.0, 0, w / 3.0, h), accent_color)

		"bicolor_horizontal":
			draw_rect(Rect2(0, 0, w, h / 2.0), primary_color)
			draw_rect(Rect2(0, h / 2.0, w, h / 2.0), secondary_color)

		"sovereign_cross":
			var bar_w := w * 0.14
			var cross_x := w * 0.35
			# Thick cross in secondary
			draw_rect(Rect2(cross_x - bar_w / 2.0, 0, bar_w, h), secondary_color)
			draw_rect(Rect2(0, h / 2.0 - bar_w / 2.0, w, bar_w), secondary_color)
			# Thin inner cross in accent
			var inner_w := bar_w * 0.5
			draw_rect(Rect2(cross_x - inner_w / 2.0, 0, inner_w, h), accent_color)
			draw_rect(Rect2(0, h / 2.0 - inner_w / 2.0, w, inner_w), accent_color)

		"canton_star":
			# Field stripes
			var stripe_h := h / 5.0
			for i in range(5):
				draw_rect(Rect2(0, i * stripe_h, w, stripe_h), primary_color if i % 2 == 0 else secondary_color)
			# Upper-left Canton
			var canton_w := w * 0.42
			var canton_h := h * 0.60
			draw_rect(Rect2(0, 0, canton_w, canton_h), primary_color)
			# Canton border
			draw_line(Vector2(canton_w, 0), Vector2(canton_w, canton_h), accent_color, 1.5)
			draw_line(Vector2(0, canton_h), Vector2(canton_w, canton_h), accent_color, 1.5)

		"solid_emblem":
			# Rich solid field with double decorative gold border
			draw_rect(Rect2(0, 0, w, h), primary_color)
			draw_rect(Rect2(3, 3, w - 6, h - 6), accent_color, false, 1.5)
			draw_rect(Rect2(6, 6, w - 12, h - 12), accent_color * Color(1, 1, 1, 0.5), false, 1.0)

	# 3. Emblem / Charge Layer
	var emblem_center := Vector2(w * 0.5, h * 0.5)
	var emblem_radius := minf(w, h) * 0.22

	if pattern == "canton_star":
		emblem_center = Vector2(w * 0.21, h * 0.30)
		emblem_radius = minf(w * 0.42, h * 0.60) * 0.26

	_draw_emblem(emblem_center, emblem_radius, accent_color)

	# 4. Ceremonial Gold Fringe (Government office flag standard)
	if show_fringe:
		var fringe_w := 3.0
		var fringe_col := Color(0.85, 0.72, 0.28, 0.9)
		draw_line(Vector2(w - fringe_w, 0), Vector2(w - fringe_w, h), fringe_col, fringe_w)
		draw_line(Vector2(0, h - fringe_w), Vector2(w, h - fringe_w), fringe_col, fringe_w)
		# Fringe tick tassels
		var ticks: int = int(h / 6.0)
		for t in range(ticks):
			var ty: float = t * 6.0
			draw_line(Vector2(w - fringe_w, ty), Vector2(w, ty + 2), Color(1.0, 0.85, 0.35, 0.9), 1.0)

func _draw_emblem(center: Vector2, radius: float, col: Color) -> void:
	if emblem == "none": return

	match emblem:
		"star":
			_draw_star(center, radius, 5, col)

		"eagle":
			# Heraldic Eagle silhouette
			# Central body
			draw_circle(center, radius * 0.35, col)
			# Wings spread
			var left_wing := PackedVector2Array([
				center + Vector2(-radius * 0.2, -radius * 0.1),
				center + Vector2(-radius * 0.95, -radius * 0.6),
				center + Vector2(-radius * 0.85, 0),
				center + Vector2(-radius * 0.3, radius * 0.3)
			])
			draw_colored_polygon(left_wing, col)
			var right_wing := PackedVector2Array([
				center + Vector2(radius * 0.2, -radius * 0.1),
				center + Vector2(radius * 0.95, -radius * 0.6),
				center + Vector2(radius * 0.85, 0),
				center + Vector2(radius * 0.3, radius * 0.3)
			])
			draw_colored_polygon(right_wing, col)
			# Head & Crest
			draw_circle(center + Vector2(0, -radius * 0.5), radius * 0.22, col)
			# Beak
			draw_line(center + Vector2(0, -radius * 0.5), center + Vector2(radius * 0.35, -radius * 0.5), col, 2.0)

		"sun":
			# Radiant Sun disc
			draw_circle(center, radius * 0.45, col)
			# 8 Sun Rays
			for i in range(8):
				var angle: float = i * (TAU / 8.0)
				var r_start := center + Vector2(cos(angle), sin(angle)) * (radius * 0.55)
				var r_end := center + Vector2(cos(angle), sin(angle)) * (radius * 0.95)
				draw_line(r_start, r_end, col, 2.0)

		"temple":
			# Classical Civic Temple / Capitol
			var tw := radius * 1.4
			var th := radius * 1.1
			# Pediment (Triangle Roof)
			var pediment := PackedVector2Array([
				center + Vector2(-tw * 0.5, -th * 0.2),
				center + Vector2(0, -th * 0.65),
				center + Vector2(tw * 0.5, -th * 0.2)
			])
			draw_colored_polygon(pediment, col)
			# Columns
			for c in range(4):
				var cx: float = center.x - tw * 0.36 + c * (tw * 0.24)
				draw_line(Vector2(cx, center.y - th * 0.15), Vector2(cx, center.y + th * 0.35), col, 2.0)
			# Base Plinth
			draw_rect(Rect2(center.x - tw * 0.55, center.y + th * 0.35, tw * 1.1, th * 0.15), col)

		"shield":
			# Heraldic Escutcheon Shield
			var sw := radius * 0.8
			var sh := radius * 1.0
			var shield_pts := PackedVector2Array([
				center + Vector2(-sw, -sh * 0.6),
				center + Vector2(sw, -sh * 0.6),
				center + Vector2(sw, sh * 0.2),
				center + Vector2(0, sh * 0.7),
				center + Vector2(-sw, sh * 0.2)
			])
			draw_colored_polygon(shield_pts, col)
			# Inner chevron
			draw_line(center + Vector2(-sw * 0.6, -sh * 0.1), center + Vector2(0, sh * 0.3), Color(1, 1, 1, 0.7), 1.5)
			draw_line(center + Vector2(sw * 0.6, -sh * 0.1), center + Vector2(0, sh * 0.3), Color(1, 1, 1, 0.7), 1.5)

		"sabres":
			# Crossed Sabres / Swords
			var sword_len := radius * 0.95
			draw_line(center - Vector2(sword_len, sword_len) * 0.65, center + Vector2(sword_len, sword_len) * 0.65, col, 2.0)
			draw_line(center - Vector2(-sword_len, sword_len) * 0.65, center + Vector2(-sword_len, sword_len) * 0.65, col, 2.0)
			# Star in center
			_draw_star(center, radius * 0.35, 5, col)

		"wreath":
			# Laurel Wreath
			var arc_r := radius * 0.65
			draw_arc(center, arc_r, PI * 0.25, PI * 1.25, 16, col, 2.5)
			draw_arc(center, arc_r, -PI * 0.25, PI * 0.75, 16, col, 2.5)
			# Central Star
			_draw_star(center, radius * 0.35, 5, col)

func _draw_star(center: Vector2, radius: float, points: int, col: Color) -> void:
	var pts := PackedVector2Array()
	var inner_radius := radius * 0.42
	for i in range(points * 2):
		var r: float = radius if i % 2 == 0 else inner_radius
		var angle: float = i * PI / float(points) - (PI / 2.0)
		pts.append(center + Vector2(cos(angle), sin(angle)) * r)
	draw_colored_polygon(pts, col)
