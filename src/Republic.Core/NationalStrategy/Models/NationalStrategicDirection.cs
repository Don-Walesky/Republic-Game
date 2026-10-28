namespace Republic.Core.NationalStrategy.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Authoritative long-term strategic direction defining national development priorities,
/// ministerial agenda weighting, private investment incentives, and foreign diplomatic focus.
/// </summary>
public enum NationalStrategicDirection
{
    /// <summary>
    /// Baseline balanced development with no single dominant sector bias.
    /// </summary>
    BalancedGrowth = 0,

    /// <summary>
    /// Heavy manufacturing, mineral extraction, steel/iron refinement, industrial factories, and capital exports.
    /// </summary>
    Industrialisation = 1,

    /// <summary>
    /// High-tech innovation, cyber warfare, scientific research labs, academic grants, and signals intelligence.
    /// </summary>
    ScienceAndTechnology = 2,

    /// <summary>
    /// Domestic oil/gas extraction, electrical grid resilience, energy stockpiles, and fuel sovereignty.
    /// </summary>
    EnergyIndependence = 3,

    /// <summary>
    /// Universal healthcare, public education, poverty reduction, civil service merit, and citizen happiness.
    /// </summary>
    HumanDevelopment = 4,

    /// <summary>
    /// Transportation corridors, deepwater ports, railway arteries, public telecommunications, and logistical hubs.
    /// </summary>
    InfrastructureDevelopment = 5,

    /// <summary>
    /// Sovereign diplomacy, regional military power projection, foreign intelligence networks, and trade treaties.
    /// </summary>
    RegionalInfluence = 6
}

/// <summary>
/// Informational descriptor and metadata for national strategic directions.
/// </summary>
public static class NationalStrategicDirectionExtensions
{
    public static string GetDisplayName(this NationalStrategicDirection direction) => direction switch
    {
        NationalStrategicDirection.Industrialisation => "Industrialisation & Manufacturing",
        NationalStrategicDirection.ScienceAndTechnology => "Science, Cyber & Technology",
        NationalStrategicDirection.EnergyIndependence => "Energy Independence & Power Security",
        NationalStrategicDirection.HumanDevelopment => "Human Capital & Social Development",
        NationalStrategicDirection.InfrastructureDevelopment => "National Infrastructure & Logistics",
        NationalStrategicDirection.RegionalInfluence => "Regional Influence & Power Projection",
        _ => "Balanced National Growth"
    };

    public static string GetDescription(this NationalStrategicDirection direction) => direction switch
    {
        NationalStrategicDirection.Industrialisation =>
            "Mobilises sovereign capital towards factories, mineral extraction, steel production, and industrial export capacity.",
        NationalStrategicDirection.ScienceAndTechnology =>
            "Prioritises research laboratories, computing universities, cyber warfare capabilities, and domestic technological patents.",
        NationalStrategicDirection.EnergyIndependence =>
            "Guarantees national energy sovereignty by developing oil reserves, electrical power generation, and strategic fuel stockpiles.",
        NationalStrategicDirection.HumanDevelopment =>
            "Focuses executive attention on public hospitals, universal schooling, social welfare, and institutional public trust.",
        NationalStrategicDirection.InfrastructureDevelopment =>
            "Constructs arterial railways, highways, sea ports, and logistical hubs connecting inland production to national markets.",
        NationalStrategicDirection.RegionalInfluence =>
            "Projects sovereign authority across regional neighbours through diplomatic alliances, intelligence networks, and trade leadership.",
        _ =>
            "Maintains equitable investment across all executive portfolios without sector concentration."
    };

    public static IReadOnlyList<string> GetPrimarySectors(this NationalStrategicDirection direction) => direction switch
    {
        NationalStrategicDirection.Industrialisation => new[] { "Manufacturing", "Mining", "Heavy Industry", "Steel", "Metals" },
        NationalStrategicDirection.ScienceAndTechnology => new[] { "Technology", "Cyber", "Research", "Education", "Telecommunications" },
        NationalStrategicDirection.EnergyIndependence => new[] { "Energy", "Petroleum", "Power", "Natural Resources", "Fuel" },
        NationalStrategicDirection.HumanDevelopment => new[] { "Healthcare", "Education", "Social Services", "Civil Service" },
        NationalStrategicDirection.InfrastructureDevelopment => new[] { "Transport", "Construction", "Logistics", "Utilities", "Public Works" },
        NationalStrategicDirection.RegionalInfluence => new[] { "Defense", "Foreign Affairs", "Intelligence", "Trade", "Diplomacy" },
        _ => new[] { "General" }
    };
}
