@tool
extends Control

## Procedural placeholder renderer for the National Capital city scenery viewed through the executive window.
## Can be seamlessly replaced by a custom Texture2D sprite or 3D viewport in future art milestones.
## Dynamically reacts to live Republic state metrics:
## - Stability & Approval: drives window light density, city brightness, and street peace vs protests.
## - Active Projects: renders temporary construction cranes and rooftop scaffolding.
## - Active Crises / Low Stability: renders smog/pollution haze tint and protest silhouettes at street level.

@export var sky_top_color: Color = Color(0.12, 0.18, 0.28, 1.0)
@export var sky_horizon_color: Color = Color(0.32, 0.38, 0.48, 1.0)
@export var distant_buildings_color: Color = Color(0.14, 0.17, 0.24, 0.9)
@export var mid_buildings_color: Color = Color(0.09, 0.12, 0.18, 0.95)
@export var near_buildings_color: Color = Color(0.05, 0.08, 0.12, 1.0)
@export var window_light_color: Color = Color(0.95, 0.85, 0.45, 0.75)
@export var dome_accent_color: Color = Color(0.75, 0.65, 0.35, 1.0)

# Living State-Driven Controls & Intensity Tuning
@export var window_light_density: float = 1.0
@export var city_brightness: float = 1.0
@export var pollution_tint: Color = Color(0.0, 0.0, 0.0, 0.0)
@export var show_construction: bool = false
@export var show_protest: bool = false
@export var construction_crane_color: Color = Color(0.90, 0.58, 0.15, 0.95)
@export var protest_silhouette_color: Color = Color(0.04, 0.06, 0.10, 0.85)

var custom_scenery_texture: Texture2D = null

# Cached state tracking
var current_approval: float = 68.5
var current_stability: float = 85.0
var current_active_project: String = "None"
var current_active_crisis: String = "None"

## Updates procedural scenery based on authoritative Republic state
func set_state_metrics(approval_val: float, stability_val: float, active_proj: String = "None", crisis_str: String = "None") -> void:
	current_approval = approval_val
	current_stability = stability_val
	current_active_project = active_proj
	current_active_crisis = crisis_str

	# 1. Normalize stability and approval to 0.0 - 1.0
	var stab_norm: float = clampf(stability_val / 100.0 if stability_val > 1.0 else stability_val, 0.0, 1.0)
	var appr_norm: float = clampf(approval_val / 100.0 if approval_val > 1.0 else approval_val, 0.0, 1.0)

	# 2. Window Light Density: High stability -> dense, vibrant lights; Low stability -> dimmed / sparse lights
	window_light_density = clampf(0.25 + (stab_norm * 1.1) + (appr_norm * 0.4), 0.2, 1.8)

	# 3. Overall city brightness
	city_brightness = clampf(0.65 + (stab_norm * 0.35) + (appr_norm * 0.20), 0.5, 1.25)

	# 4. Construction elements: Active project triggers crane and rooftop scaffolding
	var has_proj := (not active_proj.is_empty() and active_proj != "None" and active_proj != "[No State Loaded]")
	show_construction = has_proj

	# 5. Protest crowds & Smoke/Pollution Haze
	var has_crisis := (not crisis_str.is_empty() and crisis_str != "None" and crisis_str != "[No State Loaded]")
	show_protest = (stab_norm < 0.45 or appr_norm < 0.45 or has_crisis)

	if has_crisis:
		pollution_tint = Color(0.35, 0.18, 0.12, 0.28)
	elif stab_norm < 0.4:
		pollution_tint = Color(0.28, 0.22, 0.18, 0.22)
	elif stab_norm < 0.6:
		pollution_tint = Color(0.20, 0.20, 0.22, 0.10)
	else:
		pollution_tint = Color(0.0, 0.0, 0.0, 0.0)

	queue_redraw()

func set_construction_visible(visible_state: bool) -> void:
	show_construction = visible_state
	queue_redraw()

func set_protest_visible(visible_state: bool) -> void:
	show_protest = visible_state
	queue_redraw()

func set_window_light_density(density: float) -> void:
	window_light_density = density
	queue_redraw()

func set_pollution_tint(color: Color) -> void:
	pollution_tint = color
	queue_redraw()

func update_scenery(texture: Texture2D) -> void:
	custom_scenery_texture = texture
	queue_redraw()

func _draw() -> void:
	var rect := Rect2(Vector2.ZERO, size)
	if rect.size.x <= 0 or rect.size.y <= 0:
		return

	# If a custom art texture is assigned, draw it directly
	if custom_scenery_texture:
		draw_texture_rect(custom_scenery_texture, rect, false)
		return

	var w := rect.size.x
	var h := rect.size.y
	var b_mod := Color(city_brightness, city_brightness, city_brightness, 1.0)

	# 1. Sky Gradient (Atmospheric Capital Horizon)
	var sky_points := PackedVector2Array([
		Vector2(0, 0),
		Vector2(w, 0),
		Vector2(w, h * 0.75),
		Vector2(0, h * 0.75)
	])
	var sky_colors := PackedColorArray([
		sky_top_color * b_mod,
		sky_top_color * b_mod,
		sky_horizon_color * b_mod,
		sky_horizon_color * b_mod
	])
	draw_polygon(sky_points, sky_colors)

	# 2. Distant Skyline Layer (Towers & Spires)
	var distant_poly := PackedVector2Array([
		Vector2(0, h * 0.75),
		Vector2(0, h * 0.48),
		Vector2(w * 0.08, h * 0.48),
		Vector2(w * 0.10, h * 0.38),
		Vector2(w * 0.12, h * 0.38),
		Vector2(w * 0.13, h * 0.45),
		Vector2(w * 0.22, h * 0.45),
		Vector2(w * 0.25, h * 0.32),
		Vector2(w * 0.27, h * 0.32),
		Vector2(w * 0.29, h * 0.42),
		Vector2(w * 0.38, h * 0.42),
		Vector2(w * 0.42, h * 0.35),
		Vector2(w * 0.46, h * 0.35),
		Vector2(w * 0.50, h * 0.46),
		Vector2(w * 0.60, h * 0.46),
		Vector2(w * 0.64, h * 0.30),
		Vector2(w * 0.66, h * 0.30),
		Vector2(w * 0.68, h * 0.40),
		Vector2(w * 0.78, h * 0.40),
		Vector2(w * 0.82, h * 0.34),
		Vector2(w * 0.85, h * 0.34),
		Vector2(w * 0.88, h * 0.44),
		Vector2(w, h * 0.44),
		Vector2(w, h * 0.75)
	])
	draw_colored_polygon(distant_poly, distant_buildings_color * b_mod)

	# 3. National Capitol Dome & Midground Landmarks
	var dome_center_x := w * 0.48
	var dome_base_y := h * 0.52
	var dome_radius := w * 0.08
	draw_circle(Vector2(dome_center_x, dome_base_y), dome_radius, mid_buildings_color * b_mod)
	# Dome Cupola / Golden Spire
	draw_rect(Rect2(dome_center_x - 3, dome_base_y - dome_radius - 12, 6, 14), dome_accent_color * b_mod)
	draw_line(Vector2(dome_center_x, dome_base_y - dome_radius - 12), Vector2(dome_center_x, dome_base_y - dome_radius - 22), dome_accent_color * b_mod, 2.0)

	# Midground skyline blocks
	var mid_poly := PackedVector2Array([
		Vector2(0, h * 0.82),
		Vector2(0, h * 0.56),
		Vector2(w * 0.15, h * 0.56),
		Vector2(w * 0.18, h * 0.46),
		Vector2(w * 0.24, h * 0.46),
		Vector2(w * 0.27, h * 0.54),
		Vector2(w * 0.36, h * 0.54),
		Vector2(w * 0.38, dome_base_y),
		Vector2(w * 0.58, dome_base_y),
		Vector2(w * 0.62, h * 0.50),
		Vector2(w * 0.70, h * 0.50),
		Vector2(w * 0.73, h * 0.44),
		Vector2(w * 0.78, h * 0.44),
		Vector2(w * 0.82, h * 0.52),
		Vector2(w, h * 0.52),
		Vector2(w, h * 0.82)
	])
	draw_colored_polygon(mid_poly, mid_buildings_color * b_mod)

	# 4. Construction Elements (Temporary cranes & scaffolding during active national projects)
	if show_construction:
		var crane_base_x := w * 0.73
		var crane_base_y := h * 0.44
		var crane_top_y := h * 0.26
		var jib_left_x := w * 0.65
		var jib_right_x := w * 0.82
		var jib_y := h * 0.29

		# Vertical tower mast
		draw_line(Vector2(crane_base_x - 2, crane_base_y), Vector2(crane_base_x - 2, crane_top_y), construction_crane_color, 1.5)
		draw_line(Vector2(crane_base_x + 2, crane_base_y), Vector2(crane_base_x + 2, crane_top_y), construction_crane_color, 1.5)
		# Lattice cross-bracing
		for step in range(6):
			var ly: float = crane_base_y - (step * (crane_base_y - crane_top_y) / 6.0)
			draw_line(Vector2(crane_base_x - 2, ly), Vector2(crane_base_x + 2, ly - 4), construction_crane_color, 1.0)

		# Horizontal Jib arm
		draw_line(Vector2(jib_left_x, jib_y), Vector2(jib_right_x, jib_y), construction_crane_color, 2.0)
		# Counter-weight at tail
		draw_rect(Rect2(jib_left_x - 2, jib_y - 3, 6, 6), Color(0.35, 0.35, 0.38, 1.0))
		# Apex tie cables
		draw_line(Vector2(crane_base_x, crane_top_y), Vector2(jib_right_x - 4, jib_y), construction_crane_color * 0.8, 1.0)
		draw_line(Vector2(crane_base_x, crane_top_y), Vector2(jib_left_x + 4, jib_y), construction_crane_color * 0.8, 1.0)
		# Hoist cable & hook
		draw_line(Vector2(w * 0.78, jib_y), Vector2(w * 0.78, jib_y + 16), Color(0.7, 0.7, 0.7, 0.9), 1.0)
		draw_rect(Rect2(w * 0.78 - 2, jib_y + 16, 4, 3), Color(0.9, 0.6, 0.1, 1.0))
		# Warning hazard light on crane apex
		draw_circle(Vector2(crane_base_x, crane_top_y), 2.5, Color(1.0, 0.45, 0.1, 0.95))

		# Rooftop scaffolding on adjacent mid-rise building
		var scaf_x := w * 0.62
		var scaf_w := w * 0.08
		var scaf_y := h * 0.50
		for si in range(4):
			var sx: float = scaf_x + si * (scaf_w / 3.0)
			draw_line(Vector2(sx, scaf_y), Vector2(sx, scaf_y - 12), construction_crane_color * 0.75, 1.0)
		draw_line(Vector2(scaf_x, scaf_y - 6), Vector2(scaf_x + scaf_w, scaf_y - 6), construction_crane_color * 0.75, 1.0)
		draw_line(Vector2(scaf_x, scaf_y - 12), Vector2(scaf_x + scaf_w, scaf_y - 12), construction_crane_color * 0.75, 1.0)

	# 5. Near Skyline & Government Quarter
	var near_poly := PackedVector2Array([
		Vector2(0, h),
		Vector2(0, h * 0.65),
		Vector2(w * 0.12, h * 0.65),
		Vector2(w * 0.14, h * 0.58),
		Vector2(w * 0.22, h * 0.58),
		Vector2(w * 0.25, h * 0.68),
		Vector2(w * 0.35, h * 0.68),
		Vector2(w * 0.40, h * 0.60),
		Vector2(w * 0.56, h * 0.60),
		Vector2(w * 0.60, h * 0.66),
		Vector2(w * 0.75, h * 0.66),
		Vector2(w * 0.78, h * 0.58),
		Vector2(w * 0.86, h * 0.58),
		Vector2(w * 0.90, h * 0.64),
		Vector2(w, h * 0.64),
		Vector2(w, h)
	])
	draw_colored_polygon(near_poly, near_buildings_color * b_mod)

	# 6. Illuminated Windows (Night/Evening capital city lights scaled by state stability)
	var rng := RandomNumberGenerator.new()
	rng.seed = 1927
	var cur_light_col := window_light_color * b_mod
	cur_light_col.a = clampf(window_light_color.a * (0.35 + 0.65 * window_light_density), 0.15, 0.95)

	var cols: int = int(round(12.0 * clampf(window_light_density, 0.3, 1.6)))
	for col in range(cols):
		var bx: float = w * (0.04 + col * (0.92 / float(cols)))
		for row in range(5):
			var by: float = h * (0.62 + row * 0.055)
			if by < h - 10 and rng.randf() > (0.60 - 0.35 * window_light_density):
				draw_rect(Rect2(bx, by, 4, 6), cur_light_col)

	# 7. Street Level Civil Dynamics (Protest Crowds & Smoke Plumes during crises or low approval)
	if show_protest:
		var p_rng := RandomNumberGenerator.new()
		p_rng.seed = 2027
		# Left cluster
		for i in range(14):
			var fig_x: float = w * (0.16 + p_rng.randf() * 0.16)
			var fig_y: float = h * (0.94 + p_rng.randf() * 0.04)
			# Figure silhouette
			draw_circle(Vector2(fig_x, fig_y - 4), 1.5, protest_silhouette_color)
			draw_rect(Rect2(fig_x - 1, fig_y - 2, 2, 4), protest_silhouette_color)
			# Picket signs on some figures
			if i % 3 == 0:
				draw_line(Vector2(fig_x + 1, fig_y - 2), Vector2(fig_x + 1, fig_y - 8), protest_silhouette_color, 1.0)
				draw_rect(Rect2(fig_x - 1.5, fig_y - 12, 5, 4), Color(0.9, 0.88, 0.82, 0.85))

		# Right cluster
		for i in range(12):
			var fig_x: float = w * (0.62 + p_rng.randf() * 0.14)
			var fig_y: float = h * (0.94 + p_rng.randf() * 0.04)
			draw_circle(Vector2(fig_x, fig_y - 4), 1.5, protest_silhouette_color)
			draw_rect(Rect2(fig_x - 1, fig_y - 2, 2, 4), protest_silhouette_color)
			if i % 3 == 1:
				draw_line(Vector2(fig_x + 1, fig_y - 2), Vector2(fig_x + 1, fig_y - 8), protest_silhouette_color, 1.0)
				draw_rect(Rect2(fig_x - 1.5, fig_y - 12, 5, 4), Color(0.95, 0.9, 0.85, 0.85))

		# Faint smoke plumes rising from street disturbances
		draw_circle(Vector2(w * 0.24, h * 0.91), 7.0, Color(0.35, 0.25, 0.20, 0.18))
		draw_circle(Vector2(w * 0.25, h * 0.87), 10.0, Color(0.35, 0.25, 0.20, 0.12))
		draw_circle(Vector2(w * 0.68, h * 0.91), 6.0, Color(0.35, 0.25, 0.20, 0.18))
		draw_circle(Vector2(w * 0.69, h * 0.87), 9.0, Color(0.35, 0.25, 0.20, 0.12))

	# 8. Atmospheric & Pollution Tint Overlay
	if pollution_tint.a > 0.0:
		draw_rect(rect, pollution_tint)
