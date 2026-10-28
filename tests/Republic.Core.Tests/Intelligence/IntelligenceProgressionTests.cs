namespace Republic.Core.Tests.Intelligence;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class IntelligenceProgressionTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(1980, 1, 1, 0, 0, 0, RepublicTime.WatOffset);

    private static RepublicTime CreateTime(long day = 1)
    {
        return new RepublicTime(LaunchEpoch.AddDays(day), LaunchEpoch);
    }

    private static (Country Initiator, Country Target, RepublicTime Time) CreateIntelligenceEnvironment(long day = 1)
    {
        var time = CreateTime(day);

        var initiator = new Country
        {
            Id = "country_veridia",
            Name = "Republic of Veridia",
            CapitalCity = "Veridia City",
            Treasury = new RepuTreasury(500_000_000.0, "country_veridia"),
            BaselineStability = 80.0,
            HappinessRating = 65.0,
            BaseMilitaryLevel = 16
        };

        var target = new Country
        {
            Id = "country_aethelgard",
            Name = "Dominion of Aethelgard",
            CapitalCity = "Oakhaven",
            Treasury = new RepuTreasury(350_000_000.0, "country_aethelgard"),
            BaselineStability = 70.0,
            HappinessRating = 55.0,
            BaseMilitaryLevel = 14
        };

        return (initiator, target, time);
    }

    [Fact]
    public void Prerequisites_BasicEspionage_LockedUnderLevel15()
    {
        var (initiator, _, _) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 5; // Ensures composite MilitaryLevel < 15

        var service = new IntelligenceProgressionService();
        var (isUnlocked, reason, reqLevel) = service.CheckOperationPrerequisites(
            initiator,
            IntelligenceOperationType.StrategicIntelligence);

        Assert.False(isUnlocked);
        Assert.Equal(15, reqLevel);
        Assert.Contains("Requires Military Level 15", reason);
    }

    [Fact]
    public void Prerequisites_BasicEspionage_UnlockedAtLevel15()
    {
        var (initiator, _, _) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 15;

        var service = new IntelligenceProgressionService();
        var (isUnlocked, reason, reqLevel) = service.CheckOperationPrerequisites(
            initiator,
            IntelligenceOperationType.StrategicIntelligence);

        Assert.True(isUnlocked);
        Assert.Equal(15, reqLevel);
    }

    [Fact]
    public void Prerequisites_AdvancedEspionage_LockedUnderLevel20()
    {
        var (initiator, _, _) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 10; // Ensures composite MilitaryLevel is 18 (< 20)

        var service = new IntelligenceProgressionService();
        var (isUnlocked, reason, reqLevel) = service.CheckOperationPrerequisites(
            initiator,
            IntelligenceOperationType.AdvancedEspionage);

        Assert.False(isUnlocked);
        Assert.Equal(20, reqLevel);
        Assert.Contains("Requires Military Level 20", reason);
    }

    [Fact]
    public void Prerequisites_Tier0DomesticAndRecon_AlwaysUnlocked()
    {
        var (initiator, _, _) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 1;

        var service = new IntelligenceProgressionService();
        var (isUnlockedRecon, _, _) = service.CheckOperationPrerequisites(initiator, IntelligenceOperationType.MilitaryReconnaissance);
        var (isUnlockedSweep, _, _) = service.CheckOperationPrerequisites(initiator, IntelligenceOperationType.CounterintelligenceSweep);

        Assert.True(isUnlockedRecon);
        Assert.True(isUnlockedSweep);
    }

    [Fact]
    public void LaunchOperation_DeductsTreasuryCost_AndEnforcesSovereignty()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 16;
        var service = new IntelligenceProgressionService();

        double initialTreasury = initiator.Treasury.Balance;
        double expectedCost = 30_000_000.0;

        var (success, message, op) = service.LaunchOperation(
            initiator,
            target,
            IntelligenceOperationType.StrategicIntelligence,
            "Operation Iron Eagle",
            time);

        Assert.True(success);
        Assert.NotNull(op);
        Assert.True(op.Status is IntelligenceOperationStatus.Preparing or IntelligenceOperationStatus.InProgress);
        Assert.Equal(expectedCost, op.FinancialCost);
        Assert.Equal(initialTreasury - expectedCost, initiator.Treasury.Balance);
        Assert.Contains(op, initiator.IntelligenceOperations);
    }

    [Fact]
    public void ResolveOperation_Success_GeneratesClassifiedDossierReport()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 22; // Meets level 20 for Tier 2 advanced espionage
        var service = new IntelligenceProgressionService();

        var (launchSuccess, _, op) = service.LaunchOperation(
            initiator,
            target,
            IntelligenceOperationType.AdvancedEspionage,
            "Operation Deep Vault",
            time);

        Assert.True(launchSuccess);
        Assert.NotNull(op);

        // Force successful roll, no detection
        var (resolveSuccess, message, resolvedOp) = service.ResolveOperation(
            op,
            initiator,
            target,
            time,
            forcedSuccessRoll: 0.1,  // Low roll => success
            forcedDetectionRoll: 0.9); // High roll => undetected

        Assert.True(resolveSuccess);
        Assert.Equal(IntelligenceOperationStatus.Succeeded, resolvedOp.Status);
        Assert.NotNull(resolvedOp.ResultDossier);
        Assert.Equal(IntelligenceDossierConfidence.ConfirmedFact, resolvedOp.ResultDossier.Confidence);
        Assert.Equal(target.Treasury.Balance, resolvedOp.ResultDossier.ConfirmedTreasury);
        Assert.Contains(resolvedOp.ResultDossier, initiator.AcquiredIntelligence);
    }

    [Fact]
    public void ClassifiedDossier_UnmasksConfirmedValuesInDiplomaticProfile()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 20;
        var service = new IntelligenceProgressionService();

        var (_, _, op) = service.LaunchOperation(
            initiator,
            target,
            IntelligenceOperationType.AdvancedEspionage,
            "Operation Asset Probe",
            time);

        service.ResolveOperation(op!, initiator, target, time, forcedSuccessRoll: 0.05, forcedDetectionRoll: 0.95);

        // Foreign viewing without authorization should now reveal confirmed data from acquired dossier
        var profile = DiplomaticCountryProfile.Create(initiator, target, time);

        Assert.True(profile.MilitaryIsConfirmed);
        Assert.Contains("Covert Reconnaissance Confirmed", profile.MilitaryReadinessDisplay);
        Assert.True(profile.TreasuryIsConfirmed);
    }

    [Fact]
    public void ClassifiedDossier_DegradesFreshnessOverTime()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        var service = new IntelligenceProgressionService();

        var dossier = new IntelligenceDossierReport
        {
            InitiatorCountryId = initiator.Id,
            TargetCountryId = target.Id,
            TargetCountryName = target.Name,
            OperationType = IntelligenceOperationType.StrategicIntelligence,
            Title = "Dossier Alpha",
            AcquiredDay = 1,
            FreshnessDaysRemaining = 30,
            Confidence = IntelligenceDossierConfidence.HighConfidenceEstimate
        };
        initiator.StoreAcquiredIntelligence(dossier);

        // Advance 10 days
        var day11 = CreateTime(11);
        service.DegradeIntelligenceFreshness(initiator, day11);
        Assert.Equal(20, dossier.FreshnessDaysRemaining);
        Assert.Equal(IntelligenceDossierConfidence.HighConfidenceEstimate, dossier.Confidence);

        // Advance past 30 days
        var day35 = CreateTime(35);
        service.DegradeIntelligenceFreshness(initiator, day35);
        Assert.Equal(0, dossier.FreshnessDaysRemaining);
        Assert.Equal(IntelligenceDossierConfidence.StaleOrOutdated, dossier.Confidence);
        Assert.Contains("[OUTDATED DOSSIER]", dossier.ReportSummary);
    }

    [Fact]
    public void ResolveOperation_Detected_TriggersDiplomaticFalloutAndThreatAlert()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 16;
        var service = new IntelligenceProgressionService();

        var (_, _, op) = service.LaunchOperation(
            initiator,
            target,
            IntelligenceOperationType.StrategicIntelligence,
            "Operation Broken Wing",
            time);

        double initialInitiatorStability = initiator.BaselineStability;
        double initialInitiatorApproval = initiator.ApprovalRating;

        // Force detection roll => detected
        var (resolveSuccess, message, resolvedOp) = service.ResolveOperation(
            op!,
            initiator,
            target,
            time,
            forcedSuccessRoll: 0.8,   // failed
            forcedDetectionRoll: 0.1); // detected

        Assert.Equal(IntelligenceOperationStatus.Detected, resolvedOp.Status);
        Assert.True(resolvedOp.DiplomaticPenaltyApplied);

        // Initiator stability & approval reduced due to public international exposure
        Assert.True(initiator.BaselineStability < initialInitiatorStability);
        Assert.True(initiator.ApprovalRating < initialInitiatorApproval);

        // Target security leadership alerted
        var threatAlert = target.ActiveSecurityThreats.FirstOrDefault(t => t.Category == "Espionage" && t.Title.Contains("Hostile Foreign Espionage"));
        Assert.NotNull(threatAlert);
        Assert.Equal(IntelligenceConfidence.Confirmed, threatAlert.Confidence);
    }

    [Fact]
    public void CounterintelligenceSweep_NeutralizesHostileOperations()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 16;
        var service = new IntelligenceProgressionService();

        // Target launches hostile operation against initiator
        var (_, _, hostileOp) = service.LaunchOperation(
            target,
            initiator,
            IntelligenceOperationType.StrategicIntelligence,
            "Hostile Probe",
            time);

        Assert.NotNull(hostileOp);

        // Initiator conducts counterintelligence sweep
        var (sweepSuccess, sweepMessage, neutralized) = service.ConductCounterintelligenceSweep(
            initiator,
            time,
            new[] { initiator, target });

        Assert.True(sweepSuccess);
        Assert.Contains("counterintelligence sweep completed", sweepMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(neutralized >= 1);
        Assert.Equal(IntelligenceOperationStatus.Compromised, hostileOp.Status);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyLaunchIntelligenceOperation_EndToEnd()
    {
        var (initiator, target, time) = CreateIntelligenceEnvironment(1);
        initiator.BaseMilitaryLevel = 16;

        var world = new WorldState();
        world.Countries.Add(initiator);
        world.Countries.Add(target);

        var tempState = Path.Combine(Path.GetTempPath(), $"state_intel_{Guid.NewGuid():N}.json");
        var tempSave = Path.Combine(Path.GetTempPath(), $"save_intel_{Guid.NewGuid():N}.sav");

        try
        {
            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 1, tempSave);
            var snapshot = PresidentialDeskSnapshot.Create(world, initiator, time, 1, tempSave);
            File.WriteAllText(tempState, PresidentialDeskBridge.Serialize(snapshot));

            // 1. Dry run confirmation quote
            var quoteResult = PresidentialDeskBridge.ApplyLaunchIntelligenceOperation(
                "StrategicIntelligence",
                target.Id,
                title: "Eagle Eye",
                confirmed: false,
                resolveImmediately: false,
                stateFilePath: tempState,
                saveFilePath: tempSave,
                requestId: "req-intel-01");

            Assert.False(quoteResult.Success);
            Assert.Equal("AwaitingConfirmation", quoteResult.Result);

            // 2. Confirmed launch and immediate resolve
            var actionResult = PresidentialDeskBridge.ApplyLaunchIntelligenceOperation(
                "StrategicIntelligence",
                target.Id,
                title: "Eagle Eye",
                confirmed: true,
                resolveImmediately: true,
                stateFilePath: tempState,
                saveFilePath: tempSave,
                requestId: "req-intel-02");

            Assert.True(actionResult.Success);
            Assert.Equal("Success", actionResult.Result);

            // 3. Verify state file was re-exported
            var reloaded = PresidentialDeskBridge.LoadState(tempState);
            Assert.NotNull(reloaded);
            Assert.True(reloaded.MilitaryLevel >= 15);
            Assert.True(reloaded.BasicEspionageUnlocked);
            Assert.NotEmpty(reloaded.ActiveIntelligenceOperations);
        }
        finally
        {
            if (File.Exists(tempState)) File.Delete(tempState);
            if (File.Exists(tempSave)) File.Delete(tempSave);
        }
    }
}
