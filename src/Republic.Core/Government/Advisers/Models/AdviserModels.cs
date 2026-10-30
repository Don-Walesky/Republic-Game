namespace Republic.Core.Government.Advisers.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Status of a presidential special adviser position.
/// </summary>
public enum SpecialAdviserSlotStatus
{
    Active = 0,
    Locked = 1,
    AvailableToRecruit = 2
}

/// <summary>
/// Domain model representing the President's General Adviser.
/// Functions as the principal strategic guide throughout the administration.
/// Separate from Special Adviser positions and does not occupy a slot.
/// </summary>
public sealed class GeneralAdviser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "gen_adv_1";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Marcus Vance";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "General Adviser to the President";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Principal Strategic Guide";

    [JsonPropertyName("strategic_guidance")]
    public string StrategicGuidance { get; set; } = string.Empty;

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("is_appointed")]
    public bool IsAppointed { get; set; } = true;
}

/// <summary>
/// Domain model representing the Presidential Press Secretary.
/// Manages media relations, press conferences, and public communication.
/// Separate from Special Adviser positions and does not occupy a slot.
/// </summary>
public sealed class PresidentialPressSecretary
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "press_sec_1";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Elena Vance";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Presidential Press Secretary";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Official Spokesperson & Media Relations";

    [JsonPropertyName("briefing_headline")]
    public string BriefingHeadline { get; set; } = "Administration Advances National Strategic Priorities";

    [JsonPropertyName("briefing_summary")]
    public string BriefingSummary { get; set; } = "Official communications confirm readiness for national elections and governance.";

    [JsonPropertyName("approval_impact")]
    public double ApprovalImpact { get; set; } = 2.5;

    [JsonPropertyName("is_appointed")]
    public bool IsAppointed { get; set; } = true;
}

/// <summary>
/// Domain model representing a Special Adviser position.
/// The country has a maximum of 5 positions: 1 initially active (Political & Social Affairs),
/// and 4 locked positions unlockable through designated buffs or sovereign gems.
/// </summary>
public sealed class SpecialAdviserSlot
{
    [JsonPropertyName("slot_number")]
    public int SlotNumber { get; set; } = 1;

    [JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = string.Empty;

    [JsonPropertyName("adviser_name")]
    public string AdviserName { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public SpecialAdviserSlotStatus Status { get; set; } = SpecialAdviserSlotStatus.Locked;

    [JsonPropertyName("is_unlocked")]
    public bool IsUnlocked => Status != SpecialAdviserSlotStatus.Locked;

    [JsonPropertyName("focus")]
    public string Focus { get; set; } = string.Empty;

    [JsonPropertyName("advice")]
    public string Advice { get; set; } = string.Empty;

    [JsonPropertyName("unlock_requirement")]
    public string UnlockRequirement { get; set; } = string.Empty;

    [JsonPropertyName("gem_cost")]
    public int GemCost { get; set; }

    [JsonPropertyName("required_buff")]
    public string RequiredBuff { get; set; } = string.Empty;

    /// <summary>
    /// Generates canonical default 5-slot Special Adviser roster.
    /// Slot 1: Active (Political & Social Affairs)
    /// Slots 2-5: Locked (unlockable via gems or designated buffs)
    /// </summary>
    public static List<SpecialAdviserSlot> CreateDefaultRoster()
    {
        return new List<SpecialAdviserSlot>
        {
            new SpecialAdviserSlot
            {
                SlotNumber = 1,
                Portfolio = "Political and Social Affairs",
                AdviserName = "Dr. Kemi Adeleke",
                Title = "Special Adviser on Political & Social Affairs",
                Status = SpecialAdviserSlotStatus.Active,
                Focus = "Public sentiment, voting bloc alignment, civic cohesion, and social stability.",
                Advice = "Prioritise coalition stability and maintain strong ties with labor and civic associations.",
                UnlockRequirement = "Inaugural Presidential Council",
                GemCost = 0,
                RequiredBuff = "None"
            },
            new SpecialAdviserSlot
            {
                SlotNumber = 2,
                Portfolio = "Economic Strategy & Investment",
                AdviserName = "Vacant (Position Locked)",
                Title = "Special Adviser on Economic Strategy",
                Status = SpecialAdviserSlotStatus.Locked,
                Focus = "Macroeconomic resilience, capital inflow, and sovereign wealth expansion.",
                Advice = "Position locked. Unlock with Sovereign Gems or Economic Reform Buff to activate advice.",
                UnlockRequirement = "500 Sovereign Gems or Economic Modernisation Buff",
                GemCost = 500,
                RequiredBuff = "Economic Modernisation Buff"
            },
            new SpecialAdviserSlot
            {
                SlotNumber = 3,
                Portfolio = "National Security & Intelligence",
                AdviserName = "Vacant (Position Locked)",
                Title = "Special Adviser on National Security",
                Status = SpecialAdviserSlotStatus.Locked,
                Focus = "DEFCON readiness, counter-espionage, border integrity, and regional deterrence.",
                Advice = "Position locked. Unlock with Sovereign Gems or Strategic Defense Buff to activate advice.",
                UnlockRequirement = "1,000 Sovereign Gems or Strategic Defense Buff",
                GemCost = 1000,
                RequiredBuff = "Strategic Defense Buff"
            },
            new SpecialAdviserSlot
            {
                SlotNumber = 4,
                Portfolio = "Infrastructure & Digital Innovation",
                AdviserName = "Vacant (Position Locked)",
                Title = "Special Adviser on Infrastructure & Tech",
                Status = SpecialAdviserSlotStatus.Locked,
                Focus = "Transport logistics, sovereign cloud networks, and public utilities efficiency.",
                Advice = "Position locked. Unlock with Sovereign Gems or Infrastructure Drive Buff to activate advice.",
                UnlockRequirement = "1,500 Sovereign Gems or Infrastructure Drive Buff",
                GemCost = 1500,
                RequiredBuff = "Infrastructure Drive Buff"
            },
            new SpecialAdviserSlot
            {
                SlotNumber = 5,
                Portfolio = "Diplomacy & International Commerce",
                AdviserName = "Vacant (Position Locked)",
                Title = "Special Adviser on International Affairs",
                Status = SpecialAdviserSlotStatus.Locked,
                Focus = "Bilateral trade pacts, regional alliances, and global soft power projection.",
                Advice = "Position locked. Unlock with Sovereign Gems or Global Diplomatic Buff to activate advice.",
                UnlockRequirement = "2,500 Sovereign Gems or Global Diplomatic Buff",
                GemCost = 2500,
                RequiredBuff = "Global Diplomatic Buff"
            }
        };
    }
}
