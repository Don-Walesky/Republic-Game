namespace Republic.Core.Tests.Military;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Projects;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.Military.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class MilitarySecurityModuleTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);

    private static RepublicTime CreateTime(int day, int hour = 9)
    {
        return RepublicTime.FromDayAndTime(day, new TimeOnly(hour, 0), LaunchEpoch);
    }

    [Fact]
    public void ArmedForcesBranch_RecruitAndProcure_UpdatesCountsAndReadiness()
    {
        var army = new ArmedForcesBranch
        {
            Branch = MilitaryBranch.Army,
            Name = "National Ground Forces",
            PersonnelCount = 40000,
            EquipmentCount = 500,
            ReadinessScore = 80.0,
            QuarterlyFunding = 180_000_000.0
        };

        double initialStrength = army.CombatStrength;
        army.Recruit(5000);
        Assert.Equal(45000, army.PersonnelCount);

        army.Procure(50);
        Assert.Equal(550, army.EquipmentCount);
        Assert.True(army.CombatStrength > initialStrength);

        army.SetFunding(200_000_000.0);
        Assert.Equal(200_000_000.0, army.QuarterlyFunding);
    }

    [Fact]
    public void ArmedForcesRecord_AllocateBudget_DistributesAcrossBranches()
    {
        var forces = new ArmedForcesRecord();
        forces.AllocateBudget(220_000_000.0, 160_000_000.0, 170_000_000.0);

        Assert.Equal(220_000_000.0, forces.Army.QuarterlyFunding);
        Assert.Equal(160_000_000.0, forces.Navy.QuarterlyFunding);
        Assert.Equal(170_000_000.0, forces.AirForce.QuarterlyFunding);
        Assert.Equal(550_000_000.0, forces.TotalBudget);
        Assert.True(forces.TotalCombatStrength > 0.0);
    }

    [Fact]
    public void BorderSecurityRecord_FundBorderSecurity_ImprovesInterceptionAndDetection()
    {
        var border = new BorderSecurityRecord
        {
            FundingLevel = 30_000_000.0,
            SurveillanceTechLevel = 60.0
        };

        double initialIntercept = border.ContrabandInterceptionRate;
        double initialDetect = border.InfiltrationDetectionChance;

        border.FundBorderSecurity(75_000_000.0);

        Assert.True(border.ContrabandInterceptionRate > initialIntercept);
        Assert.True(border.InfiltrationDetectionChance > initialDetect);
        Assert.True(border.EffectivenessScore >= 70.0);
        Assert.Contains("Frontiers", border.StatusSummary);
    }

    [Fact]
    public void PoliceForceRecord_ExpandForce_SuppressesCrimeAndBoostsOrder()
    {
        var police = new PoliceForceRecord
        {
            OfficerCount = 25000,
            QuarterlyFunding = 40_000_000.0
        };

        double initialCrime = police.CrimeRateIndex;
        double initialOrder = police.PublicOrderScore;

        police.ExpandForce(5000, 10_000_000.0);

        Assert.Equal(30000, police.OfficerCount);
        Assert.Equal(50_000_000.0, police.QuarterlyFunding);
        Assert.True(police.CrimeRateIndex < initialCrime);
        Assert.True(police.PublicOrderScore > initialOrder);
    }

    [Fact]
    public void InvestigativeBureauRecord_CommissionAndNeutralizeSpy_OperatesClassified()
    {
        var sib = new InvestigativeBureauRecord();
        Assert.False(sib.IsCommissioned);
        Assert.True(sib.IsConcealed);

        sib.Commission();
        Assert.True(sib.IsCommissioned);
        Assert.True(sib.CounterIntelligenceRating >= 80.0);
        Assert.True(sib.ForeignSpyDetectionRate >= 80.0);
        Assert.NotEmpty(sib.ActiveInvestigations);

        sib.RecordNeutralizedSpy("Ostmark Republic");
        Assert.Equal(1, sib.NeutralizedForeignSpiesCount);
        Assert.Contains("Ostmark Republic", sib.ActiveInvestigations.Last());
    }

    [Fact]
    public void MilitaryAiRecord_UpgradeTierAndFunding_GrantsThreatBonus()
    {
        var ai = new MilitaryAiRecord();
        Assert.Equal(1, ai.AnalysisLevel);
        double tier1Bonus = ai.ThreatDetectionBonus;

        ai.UpgradeAnalysisTier(3);
        Assert.Equal(3, ai.AnalysisLevel);
        Assert.True(ai.ThreatDetectionBonus > tier1Bonus);
        Assert.Contains("satellite", ai.PredictiveThreatSummaries.Last(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NationalInvestigativeBureauProject_CompletesAndCommissionsBureau()
    {
        var country = new Country { Id = "valoria", Name = "Republic of Valoria" };
        Assert.False(country.InvestigativeBureau.IsCommissioned);

        var startTime = CreateTime(10);
        var project = new NationalInvestigativeBureauProject(country.Id, startTime);

        Assert.Equal(NationalInvestigativeBureauProject.StandardCost, project.Cost);
        Assert.False(project.IsCompleted);

        var finishTime = startTime.Add(TimeSpan.FromHours(49));
        Assert.True(project.CanComplete(finishTime));

        project.Complete(country, finishTime);
        Assert.True(project.IsCompleted);
        Assert.True(country.InvestigativeBureau.IsCommissioned);
        Assert.True(country.InvestigativeBureau.CounterIntelligenceRating >= 80.0);
    }

    [Fact]
    public void Espionage_AgainstTargetWithSIB_AndAI_ElevatesDetectionRisk()
    {
        var initiator = new Country { Id = "arlandia", Name = "Arlandia", BaseMilitaryLevel = 25 };
        initiator.Treasury.Deposit(100_000_000.0);

        var targetUnprepared = new Country { Id = "alpha", Name = "Neighbor Alpha", BaseMilitaryLevel = 10 };
        var targetFortified = new Country { Id = "beta", Name = "Fortified Beta", BaseMilitaryLevel = 20 };
        targetFortified.InvestigativeBureau.Commission();
        targetFortified.MilitaryAi.UpgradeAnalysisTier(3);
        targetFortified.BorderSecurity.FundBorderSecurity(80_000_000.0);

        var simTime = CreateTime(50);
        var intelService = new IntelligenceProgressionService();

        var (_, _, opUnprepared) = intelService.LaunchOperation(
            initiator, targetUnprepared, IntelligenceOperationType.MilitaryReconnaissance, "Recon Alpha", simTime);
        var (_, _, opFortified) = intelService.LaunchOperation(
            initiator, targetFortified, IntelligenceOperationType.MilitaryReconnaissance, "Recon Beta", simTime);

        Assert.NotNull(opUnprepared);
        Assert.NotNull(opFortified);

        // Resolve without forced rolls to check detection calculation
        intelService.ResolveOperation(opUnprepared, initiator, targetUnprepared, simTime, forcedSuccessRoll: 0.1, forcedDetectionRoll: 0.99);
        intelService.ResolveOperation(opFortified, initiator, targetFortified, simTime, forcedSuccessRoll: 0.1, forcedDetectionRoll: 0.99);

        // Target with SIB, Military AI, and high border security has significantly higher detection probability
        Assert.True(opFortified.DetectionProbability > opUnprepared.DetectionProbability);
    }

    [Fact]
    public void EspionageDetection_RecordsNeutralizedSpyInTargetSIB()
    {
        var initiator = new Country { Id = "aggressor", Name = "Aggressor Nation", BaseMilitaryLevel = 20 };
        initiator.Treasury.Deposit(50_000_000.0);

        var target = new Country { Id = "defender", Name = "Defender Republic", BaseMilitaryLevel = 20 };
        target.InvestigativeBureau.Commission();
        Assert.Equal(0, target.InvestigativeBureau.NeutralizedForeignSpiesCount);

        var simTime = CreateTime(20);
        var intelService = new IntelligenceProgressionService();
        var (_, _, op) = intelService.LaunchOperation(initiator, target, IntelligenceOperationType.MilitaryReconnaissance, "Recon Defender", simTime);

        // Force detection roll = 0.0 (guaranteed detected)
        intelService.ResolveOperation(op!, initiator, target, simTime, forcedSuccessRoll: 0.5, forcedDetectionRoll: 0.01);

        Assert.True(op!.IsDetected);
        Assert.True(target.InvestigativeBureau.NeutralizedForeignSpiesCount > 0);
        Assert.Contains(target.InvestigativeBureau.ActiveInvestigations, inv => inv.Contains("Aggressor Nation"));
    }

    [Fact]
    public void Concealment_MasksSIBInstallationsFromStandardReconnaissance()
    {
        var initiator = new Country { Id = "scout", Name = "Scout Nation", BaseMilitaryLevel = 20 };
        initiator.Treasury.Deposit(50_000_000.0);

        var target = new Country { Id = "secret", Name = "Secretive State", BaseMilitaryLevel = 20 };
        target.InvestigativeBureau.Commission();
        target.InvestigativeBureau.IsConcealed = true;

        var simTime = CreateTime(30);
        var intelService = new IntelligenceProgressionService();
        var (_, _, op) = intelService.LaunchOperation(initiator, target, IntelligenceOperationType.MilitaryReconnaissance, "Tactical Border Scan", simTime);

        // Force success = 0.01 (succeeds), detect = 0.99 (undetected)
        intelService.ResolveOperation(op!, initiator, target, simTime, forcedSuccessRoll: 0.01, forcedDetectionRoll: 0.99);

        Assert.NotNull(op!.ResultDossier);
        Assert.Contains("masked black-budget installations", op.ResultDossier.ReportSummary);
        Assert.DoesNotContain(op.ResultDossier.DiscoveredProjects, p => p.Contains("Investigative", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Bridge_MilitaryBudgetAllocation_UpdatesArmedForcesAndMinistry()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mil_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var statePath = Path.Combine(tempDir, "presidential_desk_state.json");
        var savePath = Path.Combine(tempDir, "session.sav");

        try
        {
            var country = new Country { Id = "fed_rep", Name = "Federal Republic" };
            country.Treasury.Deposit(500_000_000.0);
            var simTime = CreateTime(1);
            PresidentialDeskBridge.ExportState(country, simTime, 1, statePath, savePath);

            var result = PresidentialDeskBridge.ApplyMilitaryBudgetAllocation(
                230_000_000.0, 160_000_000.0, 160_000_000.0,
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(result.Success);
            Assert.Contains("Military budget reallocated", result.Message);

            // Verify updated snapshot
            var updatedJson = File.ReadAllText(statePath);
            Assert.Contains("armed_forces", updatedJson);
            Assert.Contains("230000000", updatedJson);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void Bridge_PoliceExpansionAndBorderSecurityFunding_ExecuteCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "police_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var statePath = Path.Combine(tempDir, "presidential_desk_state.json");
        var savePath = Path.Combine(tempDir, "session.sav");

        try
        {
            var country = new Country { Id = "civil_rep", Name = "Civil Republic" };
            country.Treasury.Deposit(500_000_000.0);
            var simTime = CreateTime(1);
            PresidentialDeskBridge.ExportState(country, simTime, 1, statePath, savePath);

            var policeRes = PresidentialDeskBridge.ApplyPoliceForceExpansion(
                3000, 15_000_000.0,
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(policeRes.Success);
            Assert.Contains("Police force expanded", policeRes.Message);

            var borderRes = PresidentialDeskBridge.ApplyBorderSecurityFunding(
                60_000_000.0,
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(borderRes.Success);
            Assert.Contains("Border security funding set", borderRes.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void Bridge_ProcessPendingDecisionRequest_DispatchesMilitaryActions()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "req_mil_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var statePath = Path.Combine(tempDir, "presidential_desk_state.json");
        var savePath = Path.Combine(tempDir, "session.sav");
        var requestPath = Path.Combine(tempDir, "presidential_desk_request.json");

        try
        {
            var country = new Country { Id = "sov_state", Name = "Sovereign State" };
            country.Treasury.Deposit(600_000_000.0);
            var simTime = CreateTime(1);
            PresidentialDeskBridge.ExportState(country, simTime, 1, statePath, savePath);

            // Test recruit military
            File.WriteAllText(requestPath, "{\"action\": \"recruit_military_branch\", \"branch\": \"army\", \"count\": 2000, \"confirmed\": true}");
            var recruitRes = PresidentialDeskBridge.ProcessPendingDecisionRequest(statePath, savePath, requestPath);
            Assert.NotNull(recruitRes);
            Assert.True(recruitRes.Success);
            Assert.Contains("Recruited 2,000 personnel", recruitRes.Message);

            // Test procure hardware
            File.WriteAllText(requestPath, "{\"action\": \"procure_military_branch\", \"branch\": \"air\", \"units\": 10, \"confirmed\": true}");
            var procureRes = PresidentialDeskBridge.ProcessPendingDecisionRequest(statePath, savePath, requestPath);
            Assert.NotNull(procureRes);
            Assert.True(procureRes.Success);
            Assert.Contains("Procured 10 military hardware units", procureRes.Message);

            // Test military AI funding
            File.WriteAllText(requestPath, "{\"action\": \"military_ai_funding\", \"funding\": 40000000.0, \"tier\": 2, \"confirmed\": true}");
            var aiRes = PresidentialDeskBridge.ProcessPendingDecisionRequest(statePath, savePath, requestPath);
            Assert.NotNull(aiRes);
            Assert.True(aiRes.Success);
            Assert.Contains("Tier 2", aiRes.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
