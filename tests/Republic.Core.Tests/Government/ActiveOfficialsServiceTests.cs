namespace Republic.Core.Tests.Government;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Government.Officials.Services;
using Republic.Core.Military.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class ActiveOfficialsServiceTests
{
    private static RepublicTime CreateTestTime(int day = 15)
    {
        var launch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var current = launch.AddDays(day);
        return new RepublicTime(current, launch);
    }

    [Fact]
    public void ChiefOfStaff_GeneratesProactiveRecommendations_BasedOnRealCountryConditions()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test",
            Treasury = new RepuTreasury(300_000.0), // Fiscal crisis trigger
            HappinessRating = 30.0, // Civil unrest trigger
            BaselineStability = 32.0
        };

        var simTime = CreateTestTime(10);
        var recs = service.EvaluateChiefOfStaffRecommendations(country, simTime);

        Assert.NotEmpty(recs);
        // Critical issues must be present
        var insolvency = recs.FirstOrDefault(r => r.Category == "FiscalCrisis");
        Assert.NotNull(insolvency);
        Assert.Equal(RecommendationPriority.Critical, insolvency.Priority);
        Assert.True(insolvency.IsUrgent);

        var unrest = recs.FirstOrDefault(r => r.Category == "CivilOrder");
        Assert.NotNull(unrest);
        Assert.Equal(RecommendationPriority.Critical, unrest.Priority);
        Assert.True(unrest.IsUrgent);

        // Sorting check: Critical recommendations appear first
        Assert.Equal(RecommendationPriority.Critical, recs[0].Priority);
    }

    [Fact]
    public void ChiefOfStaff_AppliesDecision_AndTracksStatusAcrossCycles()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test",
            Treasury = new RepuTreasury(200_000.0)
        };
        var simTime = CreateTestTime(12);

        var recs = service.EvaluateChiefOfStaffRecommendations(country, simTime);
        var insolvency = recs.First(r => r.Category == "FiscalCrisis");

        // Accept the recommendation
        bool success = service.ApplyRecommendationDecision(country, insolvency.Id, "accept", out string message);
        Assert.True(success);
        Assert.Contains("Emergency austerity enacted", message);
        Assert.Equal(RecommendationStatus.Accepted, insolvency.Status);
        Assert.True(country.Treasury.Balance >= 700_000.0);

        // Re-evaluating next cycle preserves status without duplicating
        var updatedRecs = service.EvaluateChiefOfStaffRecommendations(country, CreateTestTime(13));
        var acceptedInsolvency = updatedRecs.FirstOrDefault(r => r.Id == insolvency.Id);
        Assert.NotNull(acceptedInsolvency);
        Assert.Equal(RecommendationStatus.Accepted, acceptedInsolvency.Status);
    }

    [Fact]
    public void ChiefOfStaff_RejectAndDefer_UpdatesStatusCorrectly()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test",
            HappinessRating = 28.0
        };
        var simTime = CreateTestTime(5);

        var recs = service.EvaluateChiefOfStaffRecommendations(country, simTime);
        var unrest = recs.First(r => r.Category == "CivilOrder");

        // Reject recommendation
        bool rejected = service.ApplyRecommendationDecision(country, unrest.Id, "reject", out string rejectMsg);
        Assert.True(rejected);
        Assert.Equal(RecommendationStatus.Rejected, unrest.Status);
        Assert.Contains("Rejected", rejectMsg);

        // Defer recommendation
        bool deferred = service.ApplyRecommendationDecision(country, unrest.Id, "defer", out string deferMsg);
        Assert.True(deferred);
        Assert.Equal(RecommendationStatus.Pending, unrest.Status);
        Assert.Contains("Deferred", deferMsg);
    }

    [Fact]
    public void MinisterialAdvice_GeneratesDistinctPortfolioAdvice_AndWeightsByTraits()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test"
        };
        var simTime = CreateTestTime(20);

        // Appoint corrupt / disloyal minister of interior
        country.AppointMinister(new Minister
        {
            Name = "Col. Victor Sterling",
            Portfolio = CabinetPortfolio.Interior,
            Competence = 0.85,
            Loyalty = 0.25, // Low loyalty
            Integrity = 0.20 // Low integrity
        });

        // Appoint loyal competent finance minister
        country.AppointMinister(new Minister
        {
            Name = "Dr. Elena Rostova",
            Portfolio = CabinetPortfolio.Finance,
            Competence = 0.90,
            Loyalty = 0.88,
            Integrity = 0.92
        });

        var advice = service.GenerateMinisterialAdvice(country, simTime);
        Assert.Equal(6, advice.Count);

        var interiorAdvice = advice.First(a => a.Portfolio == CabinetPortfolio.Interior);
        Assert.True(interiorAdvice.IsAppointed);
        Assert.True(interiorAdvice.IsSelfServing);
        Assert.Contains("Disloyalty Alert", interiorAdvice.WarningNotes);
        Assert.Contains("Integrity Risk", interiorAdvice.WarningNotes);

        var financeAdvice = advice.First(a => a.Portfolio == CabinetPortfolio.Finance);
        Assert.True(financeAdvice.IsAppointed);
        Assert.False(financeAdvice.IsSelfServing);
        Assert.Empty(financeAdvice.WarningNotes);
        Assert.True(financeAdvice.ConfidenceRating > 80.0);
    }

    [Fact]
    public void SecurityLeadership_AssessesThreats_AndDistinguishesConfidenceLevels()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test",
            HappinessRating = 35.0
        };
        country.StateCapacity.CorruptionControl = 0.30; // Triggers customs contraband threat
        var simTime = CreateTestTime(14);

        var assessment = service.EvaluateSecurityLeadership(country, simTime);

        Assert.NotNull(assessment);
        Assert.NotEmpty(assessment.Threats);

        // Confirm threat levels and confidence differentiation
        var unrestThreat = assessment.Threats.FirstOrDefault(t => t.Category == "DomesticSecurity");
        Assert.NotNull(unrestThreat);
        Assert.Equal(IntelligenceConfidence.Confirmed, unrestThreat.Confidence);
        Assert.True(unrestThreat.IsConfirmed);

        var customsThreat = assessment.Threats.FirstOrDefault(t => t.Category == "EconomicSabotage");
        Assert.NotNull(customsThreat);
        Assert.Equal(IntelligenceConfidence.HighProbability, customsThreat.Confidence);

        var smugglingRumor = assessment.Threats.FirstOrDefault(t => t.Category == "BorderSecurity");
        Assert.NotNull(smugglingRumor);
        Assert.Equal(IntelligenceConfidence.UnverifiedRumor, smugglingRumor.Confidence);
        Assert.False(smugglingRumor.IsConfirmed);

        // Security indexes
        Assert.True(assessment.DomesticThreatIndex > 40.0);
    }

    [Fact]
    public void SecurityLeadership_Directives_MitigateThreatsAndApplyConsequences()
    {
        var service = new ActiveOfficialsService();
        var country = new Country
        {
            Id = "test-country",
            Name = "Republic of Test",
            HappinessRating = 35.0,
            BaselineStability = 60.0
        };
        country.StateCapacity.CorruptionControl = 0.30;
        var simTime = CreateTestTime(15);

        var assessment = service.EvaluateSecurityLeadership(country, simTime);
        var unrest = assessment.Threats.First(t => t.Category == "DomesticSecurity");

        // Mobilize directive
        bool success = service.ApplySecurityDirective(country, unrest.ThreatId, "mobilize", out string msg);
        Assert.True(success);
        Assert.Contains("Security units mobilized", msg);
        Assert.Equal("Mitigated", unrest.Status);
        Assert.True(country.BaselineStability > 60.0);

        // Sweep directive
        var customs = assessment.Threats.First(t => t.Category == "EconomicSabotage");
        bool sweepSuccess = service.ApplySecurityDirective(country, customs.ThreatId, "sweep", out string sweepMsg);
        Assert.True(sweepSuccess);
        Assert.Contains("Intelligence sweep conducted", sweepMsg);
        Assert.Equal("Mitigated", customs.Status);
        Assert.True(country.StateCapacity.CorruptionControl > 0.30);
    }

    [Fact]
    public void PresidentialDeskBridge_E2E_ProcessesChiefOfStaffAndSecurityActions()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "republic_phase2_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var statePath = Path.Combine(tempDir, "presidential_desk_state.json");
            var savePath = Path.Combine(tempDir, "presidential_desk_state.sav");

            var country = new Country
            {
                Id = "player-country",
                Name = "Republic of Test",
                Treasury = new RepuTreasury(250_000.0), // Fiscal crisis
                HappinessRating = 32.0
            };
            var simTime = CreateTestTime(10);

            // Export initial state and save
            PresidentialDeskBridge.SaveAuthoritativeState(country, simTime, 1, savePath);
            var snapshot = PresidentialDeskSnapshot.Create(country, simTime, 1, savePath);
            File.WriteAllText(statePath, PresidentialDeskBridge.Serialize(snapshot));

            // Verify phase 2 fields populated in snapshot
            Assert.NotEmpty(snapshot.ChiefOfStaffRecommendations);
            Assert.NotEmpty(snapshot.MinisterialAdvice);
            Assert.NotEmpty(snapshot.SecurityThreats);
            Assert.Contains("DEFCON", snapshot.DefconLevel);

            // Execute Chief of Staff decision via Bridge
            var recId = snapshot.ChiefOfStaffRecommendations.First().Id;
            var cosResult = PresidentialDeskBridge.ApplyChiefOfStaffDecision(
                recId,
                "accept",
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(cosResult.Success);
            Assert.Equal("Success", cosResult.Result);

            // Execute Security Directive via Bridge
            var threatId = snapshot.SecurityThreats.First().ThreatId;
            var secResult = PresidentialDeskBridge.ApplySecurityDirective(
                threatId,
                "mobilize",
                confirmed: true,
                stateFilePath: statePath,
                saveFilePath: savePath);

            Assert.True(secResult.Success);
            Assert.Equal("Success", secResult.Result);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
