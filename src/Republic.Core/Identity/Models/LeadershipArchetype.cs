namespace Republic.Core.Identity.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Canonical leadership archetypes defining presidential executive doctrine,
/// ministerial alignment synergies, and governance stat modifiers.
/// </summary>
public enum LeadershipArchetype
{
    /// <summary>
    /// Unspecified or uninitialized archetype.
    /// </summary>
    None = 0,

    /// <summary>
    /// Systematic, data-driven governance.
    /// +15% State Capacity / Civil Service Quality, +10% Science & Tech yield, -5% Populist approval sensitivity.
    /// Cabinet Synergies: Finance & Education.
    /// </summary>
    Technocrat = 1,

    /// <summary>
    /// Grassroots popular movement leadership.
    /// +10% Base Public Approval, +15% Campaign Rally effectiveness, -5% Foreign diplomatic standing.
    /// Cabinet Synergies: Interior & Labor.
    /// </summary>
    PopulistReformer = 2,

    /// <summary>
    /// Market innovation and enterprise growth leadership.
    /// +10% Private Sector Investment & National Yield, +10% REPU Treasury generation, -5% Environmental stability.
    /// Cabinet Synergies: Commerce & Resources.
    /// </summary>
    EconomicVisionary = 3,

    /// <summary>
    /// Large-scale national infrastructure and central institution architect.
    /// +15% Infrastructure construction speed & project capacity, +10% National Stability, +5% Public Works maintenance.
    /// Cabinet Synergies: Infrastructure & Transport.
    /// </summary>
    StatistBuilder = 4,

    /// <summary>
    /// Geopolitical alliance builder and international statesperson.
    /// +15% Bilateral trade yields, +10% Alliance approval, +10% Regional Influence, -5% Military aggressive deterrence.
    /// Cabinet Synergies: Foreign Affairs.
    /// </summary>
    DiplomaticStrategist = 5
}

/// <summary>
/// Domain definition and metadata descriptor for a leadership archetype.
/// </summary>
public sealed record LeadershipArchetypeDefinition(
    LeadershipArchetype Archetype,
    string Key,
    string DisplayName,
    string Tagline,
    string Description,
    IReadOnlyList<string> PositiveModifiers,
    IReadOnlyList<string> Tradeoffs,
    IReadOnlyList<string> CabinetSynergies);

/// <summary>
/// Canonical registry and utility helper for leadership archetypes.
/// </summary>
public static class LeadershipArchetypeRegistry
{
    private static readonly Dictionary<LeadershipArchetype, LeadershipArchetypeDefinition> Definitions = new()
    {
        [LeadershipArchetype.Technocrat] = new(
            LeadershipArchetype.Technocrat,
            "technocrat",
            "Technocrat",
            "Data-driven governance and administrative competence",
            "Prioritizes meritocratic civil service, scientific policy analysis, and institutional modernization over populist rhetoric.",
            new[] { "+15% State Capacity & Civil Service Quality", "+10% Science & Technology Yield" },
            new[] { "-5% Populist Approval Sensitivity" },
            new[] { "Finance", "Education" }),

        [LeadershipArchetype.PopulistReformer] = new(
            LeadershipArchetype.PopulistReformer,
            "populist_reformer",
            "Populist Reformer",
            "Direct popular mandate and institutional reform",
            "Champions working-class welfare, citizen rallies, and systemic anti-corruption over bureaucratic diplomacy.",
            new[] { "+10% Base Public Approval", "+15% Campaign Rally Effectiveness" },
            new[] { "-5% Foreign Diplomatic Standing" },
            new[] { "Interior", "Labor" }),

        [LeadershipArchetype.EconomicVisionary] = new(
            LeadershipArchetype.EconomicVisionary,
            "economic_visionary",
            "Economic Visionary",
            "Private sector investment and wealth creation",
            "Mobilizes capital markets, deregulation, and industrial entrepreneurship to accelerate national treasury expansion.",
            new[] { "+10% Private Sector Investment & National Yield", "+10% REPU Treasury Generation" },
            new[] { "-5% Environmental Stability" },
            new[] { "Commerce", "Resources" }),

        [LeadershipArchetype.StatistBuilder] = new(
            LeadershipArchetype.StatistBuilder,
            "statist_builder",
            "Statist Builder",
            "Monumental infrastructure and public sector engineering",
            "Leverages the full weight of the state to erect arterial transport, power grids, and civic mega-projects.",
            new[] { "+15% Infrastructure Construction Speed", "+10% National Stability" },
            new[] { "+5% Public Works Maintenance Cost" },
            new[] { "Infrastructure", "Transport" }),

        [LeadershipArchetype.DiplomaticStrategist] = new(
            LeadershipArchetype.DiplomaticStrategist,
            "diplomatic_strategist",
            "Diplomatic Strategist",
            "Geopolitical alliances and sovereign trade leadership",
            "Projects soft power across global borders through multilateral treaties, bilateral trade compacts, and international prestige.",
            new[] { "+15% Bilateral Trade Yields", "+10% Alliance Approval & Regional Influence" },
            new[] { "-5% Aggressive Military Deterrence" },
            new[] { "Foreign Affairs" })
    };

    public static IReadOnlyList<LeadershipArchetypeDefinition> All { get; } = Definitions.Values.ToList();

    public static LeadershipArchetypeDefinition Get(LeadershipArchetype archetype)
    {
        if (Definitions.TryGetValue(archetype, out var def))
        {
            return def;
        }

        throw new KeyNotFoundException($"No leadership archetype definition found for '{archetype}'.");
    }

    public static bool TryGet(string input, out LeadershipArchetypeDefinition? definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = input.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");

        foreach (var def in Definitions.Values)
        {
            if (string.Equals(def.Key, normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(def.DisplayName, input.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(def.Archetype.ToString(), input.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                definition = def;
                return true;
            }
        }

        // Fuzzy fallback for common variations
        if (normalized.Contains("technocrat"))
        {
            definition = Definitions[LeadershipArchetype.Technocrat];
            return true;
        }
        if (normalized.Contains("populist") || normalized.Contains("reformer"))
        {
            definition = Definitions[LeadershipArchetype.PopulistReformer];
            return true;
        }
        if (normalized.Contains("visionary") || normalized.Contains("economic"))
        {
            definition = Definitions[LeadershipArchetype.EconomicVisionary];
            return true;
        }
        if (normalized.Contains("statist") || normalized.Contains("builder"))
        {
            definition = Definitions[LeadershipArchetype.StatistBuilder];
            return true;
        }
        if (normalized.Contains("diplomat") || normalized.Contains("strategist"))
        {
            definition = Definitions[LeadershipArchetype.DiplomaticStrategist];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Computes a canonical dual-trait executive doctrine title from two selected archetypes.
    /// </summary>
    public static string ComputeDoctrineTitle(LeadershipArchetype first, LeadershipArchetype second)
    {
        var set = new HashSet<LeadershipArchetype> { first, second };

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.EconomicVisionary))
            return "Technocratic Visionary";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.StatistBuilder))
            return "Technocratic Builder";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.DiplomaticStrategist))
            return "Technocratic Diplomat";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.PopulistReformer))
            return "Technocratic Reformer";

        if (set.Contains(LeadershipArchetype.PopulistReformer) && set.Contains(LeadershipArchetype.StatistBuilder))
            return "Populist Builder";

        if (set.Contains(LeadershipArchetype.PopulistReformer) && set.Contains(LeadershipArchetype.EconomicVisionary))
            return "Populist Visionary";

        if (set.Contains(LeadershipArchetype.PopulistReformer) && set.Contains(LeadershipArchetype.DiplomaticStrategist))
            return "Populist Diplomat";

        if (set.Contains(LeadershipArchetype.EconomicVisionary) && set.Contains(LeadershipArchetype.StatistBuilder))
            return "Visionary Builder";

        if (set.Contains(LeadershipArchetype.EconomicVisionary) && set.Contains(LeadershipArchetype.DiplomaticStrategist))
            return "Visionary Diplomat";

        if (set.Contains(LeadershipArchetype.StatistBuilder) && set.Contains(LeadershipArchetype.DiplomaticStrategist))
            return "Strategic Builder";

        var def1 = Definitions.TryGetValue(first, out var d1) ? d1.DisplayName : first.ToString();
        var def2 = Definitions.TryGetValue(second, out var d2) ? d2.DisplayName : second.ToString();
        return $"{def1}-{def2}";
    }
}
