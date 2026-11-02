namespace Republic.Core.Workspace.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Data contract representing an executive office modular furniture, decor, portrait, or architectural item.
/// </summary>
public sealed record OfficeFurnitureItem(
    string Id,
    string Name,
    string Category,
    double PriceRepu,
    string ApprovalRequired = "None"
);

/// <summary>
/// Authoritative catalog of modular furniture, decor, portraits, and architectural styles
/// for the Presidential Executive Office matching the Godot ExecutiveOfficeCatalog definitions.
/// </summary>
public static class OfficeFurnitureCatalog
{
    private static readonly Dictionary<string, OfficeFurnitureItem> ItemsById = new(StringComparer.OrdinalIgnoreCase)
    {
        // ----------------------------------------------------------------------
        // Category: Desks & Chairs
        // ----------------------------------------------------------------------
        ["desk_mahogany_classic"] = new("desk_mahogany_classic", "Mahogany Desk & Leather Highback", "desk_and_chair", 0),
        ["desk_obsidian_modern"] = new("desk_obsidian_modern", "Obsidian Glass & Ergonomic Mesh", "desk_and_chair", 350_000, "Presidential Facilities"),
        ["desk_heritage_oak"] = new("desk_heritage_oak", "Heritage Oak & Velvet Throne", "desk_and_chair", 750_000, "Ministry of Interior"),

        // ----------------------------------------------------------------------
        // Category: Visitor Seating & Meeting Table
        // ----------------------------------------------------------------------
        ["visitor_leather_armchairs"] = new("visitor_leather_armchairs", "Executive Leather Armchairs", "visitor_seating", 0),
        ["visitor_teak_conference"] = new("visitor_teak_conference", "Teak Conference Table & Chairs", "visitor_seating", 450_000, "Presidential Facilities"),
        ["visitor_lounge_suite"] = new("visitor_lounge_suite", "Chesterfield Diplomatic Lounge", "visitor_seating", 850_000, "Ministry of Interior"),

        // ----------------------------------------------------------------------
        // Category: Walls & Flooring
        // ----------------------------------------------------------------------
        ["wall_slate_parquet"] = new("wall_slate_parquet", "Midnight Slate & Polished Parquet", "wall_and_flooring", 0),
        ["wall_marble_herringbone"] = new("wall_marble_herringbone", "Neoclassical Marble & Herringbone", "wall_and_flooring", 600_000, "Presidential Facilities"),
        ["wall_walnut_granite"] = new("wall_walnut_granite", "Imperial Walnut & Black Granite", "wall_and_flooring", 1_200_000, "Ministry of Interior"),

        // ----------------------------------------------------------------------
        // Category: Carpets & Lighting
        // ----------------------------------------------------------------------
        ["carpet_crimson_state"] = new("carpet_crimson_state", "Crimson State Rug & Brass Chandelier", "carpets_and_lighting", 0),
        ["carpet_navy_crest"] = new("carpet_navy_crest", "Navy Sovereign Crest & Recessed LEDs", "carpets_and_lighting", 280_000, "Presidential Facilities"),
        ["carpet_emerald_runner"] = new("carpet_emerald_runner", "Emerald Silk Runner & Modern Pendants", "carpets_and_lighting", 520_000, "Presidential Facilities"),

        // ----------------------------------------------------------------------
        // Category: Bookshelves & Decor
        // ----------------------------------------------------------------------
        ["decor_legal_archives"] = new("decor_legal_archives", "Constitutional Archives & Bronze Scales", "bookshelves_and_decor", 0),
        ["decor_global_cartography"] = new("decor_global_cartography", "Cartography Shelves & Terrestrial Globe", "bookshelves_and_decor", 320_000, "Presidential Facilities"),
        ["decor_modernist_sculpture"] = new("decor_modernist_sculpture", "Architectural Niches & Bronze Monolith", "bookshelves_and_decor", 680_000, "Ministry of Interior"),

        // ----------------------------------------------------------------------
        // Category: National Artwork & Diplomatic Gifts
        // ----------------------------------------------------------------------
        ["art_founding_charter"] = new("art_founding_charter", "Founding Constitutional Charter", "artwork_and_gifts", 0),
        ["art_sovereign_landscape"] = new("art_sovereign_landscape", "Sovereign Landscapes Oil Canvas", "artwork_and_gifts", 400_000, "Presidential Facilities"),
        ["art_diplomatic_tributes"] = new("art_diplomatic_tributes", "Diplomatic Tributes & Silver Urns", "artwork_and_gifts", 950_000, "Ministry of Interior"),

        // ----------------------------------------------------------------------
        // Category: Presidential Portraits
        // ----------------------------------------------------------------------
        ["portrait_m1"] = new("portrait_m1", "Alexander Vance", "portraits", 0),
        ["portrait_m2"] = new("portrait_m2", "Marcus Sterling", "portraits", 0),
        ["portrait_m3"] = new("portrait_m3", "David Chen", "portraits", 0),
        ["portrait_m4"] = new("portrait_m4", "Samuel Adebayo", "portraits", 0),
        ["portrait_m5"] = new("portrait_m5", "Viktor Lindqvist", "portraits", 0),
        ["portrait_f1"] = new("portrait_f1", "Elena Rostova", "portraits", 0),
        ["portrait_f2"] = new("portrait_f2", "Amina Al-Mansoor", "portraits", 0),
        ["portrait_f3"] = new("portrait_f3", "Victoria Mercer", "portraits", 0),

        // ----------------------------------------------------------------------
        // Category: National Flag Styles
        // ----------------------------------------------------------------------
        ["flag_navy_gold"] = new("flag_navy_gold", "Arcadia Sovereign (Navy & Gold)", "flags", 0),
        ["flag_emerald_white"] = new("flag_emerald_white", "Solaria Federal (Emerald & Gold)", "flags", 0),
        ["flag_crimson_gold"] = new("flag_crimson_gold", "Valoria Commonwealth (Crimson & Gold)", "flags", 0),
        ["flag_azure_silver"] = new("flag_azure_silver", "Veridia Union (Azure & Silver)", "flags", 0),
        ["flag_obsidian_bronze"] = new("flag_obsidian_bronze", "Obsidian Republic (Charcoal & Bronze)", "flags", 0),
    };

    /// <summary>
    /// Attempts to find a furniture item by unique catalog ID.
    /// </summary>
    public static bool TryGetItem(string itemId, out OfficeFurnitureItem item)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            item = null!;
            return false;
        }

        return ItemsById.TryGetValue(itemId.Trim(), out item!);
    }

    /// <summary>
    /// Checks whether an item ID exists in the catalog.
    /// </summary>
    public static bool ContainsItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && ItemsById.ContainsKey(itemId.Trim());
    }

    /// <summary>
    /// Returns all registered items in the presidential office catalog.
    /// </summary>
    public static IReadOnlyCollection<OfficeFurnitureItem> GetAllItems() => ItemsById.Values;
}

