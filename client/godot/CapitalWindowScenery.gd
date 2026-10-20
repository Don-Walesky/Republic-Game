@tool
extends Control

## Procedural placeholder renderer for the National Capital city scenery viewed through the executive window.
## Can be seamlessly replaced by a custom Texture2D sprite or 3D viewport in future art milestones.

@export var sky_top_color: Color = Color(0.12, 0.18, 0.28, 1.0)
@export var sky_horizon_color: Color = Color(0.32, 0.38, 0.48, 1.0)
@export var distant_buildings_color: Color = Color(0.14, 0.17, 0.24, 0.9)
@export var mid_buildings_color: Color = Color(0.09, 0.12, 0.18, 0.95)
@export var near_buildings_color: Color = Color(0.05, 0.08, 0.12, 1.0)
@export var window_light_color: Color = Color(0.95, 0.85, 0.45, 0.75)
@export var dome_accent_color: Color = Color(0.75, 0.65, 0.35, 1.0)

var custom_scenery_texture: Texture2D = null

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

	# 1. Sky Gradient (Atmospheric Capital Horizon)
	var sky_points := PackedVector2Array([
		Vector2(0, 0),
		Vector2(w, 0),
		Vector2(w, h * 0.75),
		Vector2(0, h * 0.75)
	])
	var sky_colors := PackedColorArray([
		sky_top_color,
		sky_top_color,
		sky_horizon_color,
		sky_horizon_color
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
	draw_colored_polygon(distant_poly, distant_buildings_color)

	# 3. National Capitol Dome & Midground Landmarks
	var dome_center_x := w * 0.48
	var dome_base_y := h * 0.52
	var dome_radius := w * 0.08
	draw_circle(Vector2(dome_center_x, dome_base_y), dome_radius, mid_buildings_color)
	# Dome Cupola / Golden Spire
	draw_rect(Rect2(dome_center_x - 3, dome_base_y - dome_radius - 12, 6, 14), dome_accent_color)
	draw_line(Vector2(dome_center_x, dome_base_y - dome_radius - 12), Vector2(dome_center_x, dome_base_y - dome_radius - 22), dome_accent_color, 2.0)

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
	draw_colored_polygon(mid_poly, mid_buildings_color)

	# 4. Near Skyline & Government Quarter
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
	draw_colored_polygon(near_poly, near_buildings_color)

	# 5. Illuminated Windows (Night/Evening capital city lights)
	var rng := RandomNumberGenerator.new()
	rng.seed = 1927
	for col in range(12):
		var bx: float = w * (0.04 + col * 0.078)
		for row in range(5):
			var by: float = h * (0.62 + row * 0.055)
			if by < h - 10 and rng.randf() > 0.35:
				draw_rect(Rect2(bx, by, 4, 6), window_light_color)

func update_scenery(texture: Texture2D) -> void:
	custom_scenery_texture = texture
	queue_redraw()
