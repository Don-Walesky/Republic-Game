namespace Republic.Core.Identity.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Canonical leadership types defining presidential executive doctrine,
/// ministerial alignment synergies, strategic opportunities, and governance tendencies.
/// </summary>
public enum LeadershipArchetype
{
    None = 0,

    /// <summary>
    /// The Visionary: Focus on innovation, transformation, and long-term national progress.
    /// Advantage: Better opportunities for long-term reforms and innovation (+10% Private Investment & Yield).
    /// Challenge: Resistance to rapid change and pressure to address immediate needs (-5% Environmental Stability).
    /// Cabinet Synergies: Commerce & Resources.
    /// </summary>
    Visionary = 1,

    /// <summary>
    /// The Diplomat: Focus on negotiation, compromise, alliances, and international relationships.
    /// Advantage: Stronger diplomatic opportunities and political negotiation (+15% Bilateral Trade, +10% Alliance Standing).
    /// Challenge: Excessive concessions or difficulty making unpopular decisions (-5% Aggressive Military Deterrence).
    /// Cabinet Synergies: Foreign Affairs.
    /// </summary>
    Diplomat = 2,

    /// <summary>
    /// The Strategist: Focus on planning, calculated decisions, national positioning, and security.
    /// Advantage: Stronger strategic planning and crisis preparation (+15% Infrastructure Speed, +10% National Stability).
    /// Challenge: Risk of overlooking public sentiment or social concerns (+5% Public Works Maintenance).
    /// Cabinet Synergies: Infrastructure & Transport.
    /// </summary>
    Strategist = 3,

    /// <summary>
    /// The People's Champion: Focus on public welfare, social justice, and popular support.
    /// Advantage: Better opportunities for social programmes and public engagement (+10% Base Public Approval, +15% Rally Support).
    /// Challenge: Expensive promises and pressure on national finances (-5% Diplomatic Standing).
    /// Cabinet Synergies: Interior & Labor.
    /// </summary>
    PeoplesChampion = 4,

    /// <summary>
    /// The Technocrat: Focus on expertise, efficiency, evidence-based decisions, and institutional performance.
    /// Advantage: Better opportunities for administrative and economic reforms (+15% State Capacity & Civil Service, +10% Tech Yield).
    /// Challenge: Difficulty navigating political sensitivities and public expectations (-5% Populist Sensitivity).
    /// Cabinet Synergies: Finance & Education.
    /// </summary>
    Technocrat = 5,

    // Aliases for backward compatibility
    EconomicVisionary = Visionary,
    DiplomaticStrategist = Diplomat,
    StatistBuilder = Strategist,
    PopulistReformer = PeoplesChampion
}

/// <summary>
/// Domain definition and metadata descriptor for a leadership type.
/// </summary>
public sealed record LeadershipArchetypeDefinition(
    LeadershipArchetype Archetype,
    string Key,
    string DisplayName,
    string Focus,
    string Advantage,
    string Challenge,
    IReadOnlyList<string> CabinetSynergies,
    string StatModifiersSummary);

/// <summary>
/// Canonical registry and utility helper for leadership types.
/// </summary>
public static class LeadershipArchetypeRegistry
{
    private static readonly Dictionary<LeadershipArchetype, LeadershipArchetypeDefinition> Definitions = new()
    {
        [LeadershipArchetype.Visionary] = new(
            LeadershipArchetype.Visionary,
            "visionary",
            "The Visionary",
            "Innovation, transformation and long-term national progress",
            "Better opportunities for long-term reforms, scientific patents, and private enterprise investment (+10% Investment Yield).",
            "Resistance to rapid change and continuous pressure to address short-term populist needs (-5% Environmental Stability).",
            new[] { "Commerce", "Resources" },
            "+10% Private Investment & National Yield, +10% Treasury Generation; -5% Environmental Stability"),

        [LeadershipArchetype.Diplomat] = new(
            LeadershipArchetype.Diplomat,
            "diplomat",
            "The Diplomat",
            "Negotiation, compromise, alliances and international relationships",
            "Stronger diplomatic opportunities, cross-border trade agreements, and bilateral treaty leverage (+15% Bilateral Trade).",
            "Excessive concessions in negotiations or difficulty making unpopular decisive security decisions (-5% Aggressive Deterrence).",
            new[] { "Foreign Affairs" },
            "+15% Bilateral Trade Yields, +10% Alliance Standing & Regional Influence; -5% Aggressive Deterrence"),

        [LeadershipArchetype.Strategist] = new(
            LeadershipArchetype.Strategist,
            "strategist",
            "The Strategist",
            "Planning, calculated decisions, national positioning and security",
            "Stronger strategic planning, arterial construction capacity, and crisis response resilience (+10% Stability, +15% Project Speed).",
            "Risk of overlooking public sentiment or immediate civic welfare concerns (+5% Infrastructure Maintenance Cost).",
            new[] { "Infrastructure", "Transport" },
            "+15% Infrastructure Construction Speed, +10% National Stability; +5% Public Works Maintenance"),

        [LeadershipArchetype.PeoplesChampion] = new(
            LeadershipArchetype.PeoplesChampion,
            "peoples_champion",
            "The People's Champion",
            "Public welfare, social justice and popular support",
            "Better opportunities for social welfare programmes, grassroots rally surges, and enduring public affection (+10% Approval).",
            "Expensive campaign promises and heightened fiscal pressure on the national sovereign treasury (-5% Foreign Diplomatic Standing).",
            new[] { "Interior", "Labor" },
            "+10% Base Public Approval, +15% Campaign Rally Effectiveness; -5% Foreign Diplomatic Standing"),

        [LeadershipArchetype.Technocrat] = new(
            LeadershipArchetype.Technocrat,
            "technocrat",
            "The Technocrat",
            "Expertise, efficiency, evidence-based decisions and institutional performance",
            "Better opportunities for administrative excellence, institutional capacity, and meritocratic reforms (+15% State Capacity).",
            "Difficulty navigating nuanced political sensitivities and emotive public expectations (-5% Populist Sensitivity).",
            new[] { "Finance", "Education" },
            "+15% State Capacity & Civil Service Quality, +10% Science & Tech Yield; -5% Populist Sensitivity")
    };

    public static IReadOnlyList<LeadershipArchetypeDefinition> All { get; } = Definitions.Values.ToList();

    public static LeadershipArchetypeDefinition Get(LeadershipArchetype archetype)
    {
        // Normalize alias values
        var canonical = archetype switch
        {
            LeadershipArchetype.EconomicVisionary => LeadershipArchetype.Visionary,
            LeadershipArchetype.DiplomaticStrategist => LeadershipArchetype.Diplomat,
            LeadershipArchetype.StatistBuilder => LeadershipArchetype.Strategist,
            LeadershipArchetype.PopulistReformer => LeadershipArchetype.PeoplesChampion,
            _ => archetype
        };

        if (Definitions.TryGetValue(canonical, out var def))
        {
            return def;
        }

        throw new KeyNotFoundException($"No leadership type definition found for '{archetype}'.");
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

        // Fuzzy and alias matches
        if (normalized.Contains("visionary") || normalized.Contains("economic"))
        {
            definition = Definitions[LeadershipArchetype.Visionary];
            return true;
        }
        if (normalized.Contains("diplomat"))
        {
            definition = Definitions[LeadershipArchetype.Diplomat];
            return true;
        }
        if (normalized.Contains("strategist") || normalized.Contains("statist") || normalized.Contains("builder"))
        {
            definition = Definitions[LeadershipArchetype.Strategist];
            return true;
        }
        if (normalized.Contains("champion") || normalized.Contains("people") || normalized.Contains("populist") || normalized.Contains("reformer"))
        {
            definition = Definitions[LeadershipArchetype.PeoplesChampion];
            return true;
        }
        if (normalized.Contains("technocrat"))
        {
            definition = Definitions[LeadershipArchetype.Technocrat];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Computes a canonical dual-trait executive doctrine title from two selected leadership types.
    /// </summary>
    public static string ComputeDoctrineTitle(LeadershipArchetype first, LeadershipArchetype second)
    {
        var set = new HashSet<LeadershipArchetype>
        {
            first == LeadershipArchetype.EconomicVisionary ? LeadershipArchetype.Visionary :
            first == LeadershipArchetype.DiplomaticStrategist ? LeadershipArchetype.Diplomat :
            first == LeadershipArchetype.StatistBuilder ? LeadershipArchetype.Strategist :
            first == LeadershipArchetype.PopulistReformer ? LeadershipArchetype.PeoplesChampion : first,

            second == LeadershipArchetype.EconomicVisionary ? LeadershipArchetype.Visionary :
            second == LeadershipArchetype.DiplomaticStrategist ? LeadershipArchetype.Diplomat :
            second == LeadershipArchetype.StatistBuilder ? LeadershipArchetype.Strategist :
            second == LeadershipArchetype.PopulistReformer ? LeadershipArchetype.PeoplesChampion : second
        };

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.Visionary))
            return "Technocratic Visionary";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.Strategist))
            return "Technocratic Strategist";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.Diplomat))
            return "Technocratic Diplomat";

        if (set.Contains(LeadershipArchetype.Technocrat) && set.Contains(LeadershipArchetype.PeoplesChampion))
            return "Technocratic Reformer";

        if (set.Contains(LeadershipArchetype.Visionary) && set.Contains(LeadershipArchetype.Strategist))
            return "Visionary Strategist";

        if (set.Contains(LeadershipArchetype.Visionary) && set.Contains(LeadershipArchetype.Diplomat))
            return "Visionary Diplomat";

        if (set.Contains(LeadershipArchetype.Visionary) && set.Contains(LeadershipArchetype.PeoplesChampion))
            return "Visionary Champion";

        if (set.Contains(LeadershipArchetype.Diplomat) && set.Contains(LeadershipArchetype.Strategist))
            return "Strategic Diplomat";

        if (set.Contains(LeadershipArchetype.Diplomat) && set.Contains(LeadershipArchetype.PeoplesChampion))
            return "Diplomatic Champion";

        if (set.Contains(LeadershipArchetype.Strategist) && set.Contains(LeadershipArchetype.PeoplesChampion))
            return "Strategic Champion";

        var d1 = Definitions.TryGetValue(first, out var def1) ? def1.DisplayName : first.ToString();
        var d2 = Definitions.TryGetValue(second, out var def2) ? def2.DisplayName : second.ToString();
        return $"{d1} & {d2}";
    }
}
