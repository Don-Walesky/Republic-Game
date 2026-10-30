namespace Republic.Core.Government.Advisers.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Government.Advisers.Models;
using Republic.Core.Identity.Models;
using Republic.Core.World.Models;

/// <summary>
/// Service managing the presidential advisory council, synchronization of General Adviser guidance,
/// Press Secretary briefings, and progression of Special Adviser slots.
/// </summary>
public static class AdviserService
{
    /// <summary>
    /// Synchronizes the player's advisory council, tailoring General Adviser recommendations
    /// and Press Secretary briefings to the player's leadership doctrine and national development priorities.
    /// </summary>
    public static void SynchronizeAdvisers(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.GeneralAdviser ??= new GeneralAdviser();
        country.PressSecretary ??= new PresidentialPressSecretary();
        if (country.SpecialAdvisers == null || country.SpecialAdvisers.Count == 0)
        {
            country.SpecialAdvisers = SpecialAdviserSlot.CreateDefaultRoster();
        }

        var doctrine = !string.IsNullOrWhiteSpace(country.LeadershipDoctrine)
            ? country.LeadershipDoctrine
            : (country.LeadershipTypes.Count > 0
                ? string.Join(" & ", country.LeadershipTypes.Select(t => t.ToString()))
                : "Balanced Governance");

        var planNames = country.DevelopmentPlans.Count > 0
            ? country.DevelopmentPlans.Select(p => NationalDevelopmentPlanRegistry.Get(p).DisplayName).ToList()
            : new List<string> { "Industrialisation First", "Agricultural Transformation", "Technology and Innovation", "Infrastructure Revolution" };

        var guidanceText = $"As your General Adviser, I stand ready to assist your {doctrine} administration. " +
            $"Our primary national agenda centers on {string.Join(", ", planNames)}. " +
            $"We must balance strategic investments in these core sectors while preserving macroeconomic stability, civic order, and democratic institutions.";

        var recs = new List<string>
        {
            $"Execute national development priorities: {string.Join(", ", planNames.Take(2))}.",
            $"Address institutional hurdles in {string.Join(" and ", planNames.Skip(2).Take(2))}.",
            $"Leverage your {doctrine} mandate to rally civic support and maintain legislative alignment.",
            "Consult Dr. Kemi Adeleke on voting bloc sentiment and civil society cohesion.",
            "Unlock additional Special Adviser portfolios through Sovereign Gems or designated national buffs as governance demands grow."
        };

        country.GeneralAdviser.StrategicGuidance = guidanceText;
        country.GeneralAdviser.Recommendations = recs;
        country.GeneralAdviser.IsAppointed = true;

        country.PressSecretary.BriefingHeadline = $"{doctrine} Administration Enacts Sovereign Strategic Agenda";
        country.PressSecretary.BriefingSummary = $"Press Secretary Elena Vance confirms national priority focus on {string.Join(", ", planNames.Take(3))}. Press corps reports positive public anticipation.";
        country.PressSecretary.ApprovalImpact = 2.5;
        country.PressSecretary.IsAppointed = true;
    }

    /// <summary>
    /// Unlocks a locked Special Adviser slot using Sovereign Gems or designated buffs.
    /// </summary>
    public static (bool Success, string Message) UnlockSpecialAdviser(Country country, int slotNumber, string unlockMethod)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (slotNumber < 2 || slotNumber > 5)
        {
            return (false, $"Invalid Special Adviser slot number: {slotNumber}. Only slots 2 through 5 can be unlocked.");
        }

        country.SpecialAdvisers ??= SpecialAdviserSlot.CreateDefaultRoster();
        var slot = country.SpecialAdvisers.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot == null)
        {
            return (false, $"Special Adviser slot {slotNumber} not found.");
        }

        if (slot.Status != SpecialAdviserSlotStatus.Locked)
        {
            return (false, $"Special Adviser slot {slotNumber} ({slot.Portfolio}) is already unlocked and active.");
        }

        var normMethod = (unlockMethod ?? "gems").Trim().ToLowerInvariant();
        if (normMethod is "gems" or "gem" or "sovereign_gems")
        {
            if (country.SovereignGems < slot.GemCost)
            {
                return (false, $"Insufficient Sovereign Gems. Required: {slot.GemCost:N0} gems, Available: {country.SovereignGems:N0} gems.");
            }

            country.SovereignGems -= slot.GemCost;
            slot.Status = SpecialAdviserSlotStatus.Active;
            AssignAdviserDetails(slot);

            return (true, $"Successfully unlocked {slot.Portfolio} for {slot.GemCost:N0} Sovereign Gems. {slot.AdviserName} has assumed the office.");
        }
        else if (normMethod is "buff" or "buffs")
        {
            slot.Status = SpecialAdviserSlotStatus.Active;
            AssignAdviserDetails(slot);

            return (true, $"Successfully unlocked {slot.Portfolio} via designated buff: {slot.RequiredBuff}. {slot.AdviserName} has assumed the office.");
        }

        return (false, $"Unsupported unlock method: '{unlockMethod}'. Supported methods: 'gems', 'buff'.");
    }

    private static void AssignAdviserDetails(SpecialAdviserSlot slot)
    {
        switch (slot.SlotNumber)
        {
            case 2:
                slot.AdviserName = "Dr. Arthur Sterling";
                slot.Title = "Special Adviser on Economic Strategy & Investment";
                slot.Advice = "Recommend boosting sovereign capital reserves, securing private investment consortia, and optimizing treasury allocation.";
                break;
            case 3:
                slot.AdviserName = "Gen. Tariq Al-Mansoor";
                slot.Title = "Special Adviser on National Security & Intelligence";
                slot.Advice = "Recommend hardening domestic counterintelligence, fortifying border installations, and maintaining peak readiness.";
                break;
            case 4:
                slot.AdviserName = "Eng. Maya Lin";
                slot.Title = "Special Adviser on Infrastructure & Digital Innovation";
                slot.Advice = "Recommend modernising national transport corridors, rolling out high-speed data trunks, and expanding sovereign utility networks.";
                break;
            case 5:
                slot.AdviserName = "Ambassador Claire Dupont";
                slot.Title = "Special Adviser on Diplomacy & International Commerce";
                slot.Advice = "Recommend establishing bilateral trade pacts with neighboring nations and projecting sovereign influence across international forums.";
                break;
        }
    }
}

