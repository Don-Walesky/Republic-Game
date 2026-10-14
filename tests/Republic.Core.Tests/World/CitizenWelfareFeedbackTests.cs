namespace Republic.Core.Tests.World;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Scenarios.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

/// <summary>
/// Phase 11 test suite: First Citizen Welfare and Public Approval Feedback Loop.
/// Validates domestic governance taxation decisions and their direct citizen-facing consequences:
/// approval rating response to tax changes, stability penalty at extreme taxation thresholds,
/// idempotency, cancellation ergonomics, foreign sovereign isolation, dashboard reflection,
/// interactive workspace handling, and persistence.
/// </summary>
public sealed class CitizenWelfareFeedbackTests
{
    private readonly RepublicClock _clock;

    public CitizenWelfareFeedbackTests()
    {
        _clock = RepublicClock.CreateControlled();
    }

    private Country CreateTestCountry(
        string name = "Republic of Arcadia",
        double startingApproval = 68.5,
        double startingStability = 75.0,
        double startingIncomeTax = 0.25,
        double startingCorpTax = 0.20)
    {
        var countryId = name.ToLowerInvariant().Replace(" ", "-");
        var time = _clock.CurrentTime;
        var country = Country.Found(
            name,
            time,
            id: countryId,
            baselineStability: startingStability,
            happinessRating: startingApproval);

        country.TaxPolicy.IncomeTaxRate = startingIncomeTax;
        country.TaxPolicy.CorporateTaxRate = startingCorpTax;
        return country;
    }

    private RepublicApplication CreateTestApplication()
    {
        var bootstrapper = new ApplicationBootstrapper();
        var app = bootstrapper.Bootstrap();
        var scenarioService = new ScenarioBootstrapper(
            app.WorldManager,
            app.WorkspaceManager,
            app.CabinetService,
            app.LegislatureService,
            null,
            null,
            app.Clock);

        scenarioService.BootstrapScenarioAsync("arcadia-day1").GetAwaiter().GetResult();
        return app;
    }

    [Fact]
    public void TaxHike_ReducesPublicApproval_Proportionally()
    {
        // 1. Arrange: Baseline 25% income tax and 68.5% approval
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act: Increase income tax rate by +5% (25% -> 30%)
        var newPolicy = new TaxPolicy { IncomeTaxRate = 0.30, CorporateTaxRate = 0.20 };
        var feedback = country.ApplyTaxPolicy(newPolicy, time);

        // 3. Assert:
        // Rule: -0.5 points per +1% tax rate increase => +5% tax produces -2.5 approval points drop.
        Assert.Equal(0.30, country.TaxPolicy.IncomeTaxRate);
        Assert.Equal(-2.5, feedback.ApprovalDelta);
        Assert.Equal(66.0, country.HappinessRating);
        Assert.Equal(66.0, country.ApprovalRating);
        Assert.Equal(68.5, feedback.PreviousApproval);
        Assert.Equal(66.0, feedback.CurrentApproval);
        Assert.Equal(0.05, Math.Round(feedback.IncomeTaxDelta, 4));
        Assert.Equal(0.0, feedback.StabilityDelta);
        Assert.Same(feedback, country.LatestCitizenFeedback);
        Assert.Contains("Tax hike enacted", feedback.Description);
        Assert.Contains("public approval dropped by 2.5%", feedback.Description);
    }

    [Fact]
    public void TaxCut_IncreasesPublicApproval_Proportionally()
    {
        // 1. Arrange: Baseline 25% income tax and 68.5% approval
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act: Decrease income tax rate by -5% (25% -> 20%)
        var newPolicy = new TaxPolicy { IncomeTaxRate = 0.20, CorporateTaxRate = 0.20 };
        var feedback = country.ApplyTaxPolicy(newPolicy, time);

        // 3. Assert:
        // Rule: +0.5 points per -1% tax rate decrease => -5% tax produces +2.5 approval points rise.
        Assert.Equal(0.20, country.TaxPolicy.IncomeTaxRate);
        Assert.Equal(2.5, feedback.ApprovalDelta);
        Assert.Equal(71.0, country.HappinessRating);
        Assert.Equal(71.0, country.ApprovalRating);
        Assert.Equal(68.5, feedback.PreviousApproval);
        Assert.Equal(71.0, feedback.CurrentApproval);
        Assert.Equal(-0.05, Math.Round(feedback.IncomeTaxDelta, 4));
        Assert.Contains("Tax relief enacted", feedback.Description);
        Assert.Contains("public approval rose by 2.5%", feedback.Description);
    }

    [Fact]
    public void ReapplyingSamePolicy_IsIdempotent_ProducesZeroDelta()
    {
        // 1. Arrange: Baseline 25% income tax
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act: Re-apply 25% income tax
        var policy = new TaxPolicy { IncomeTaxRate = 0.25, CorporateTaxRate = 0.20 };
        var feedback = country.ApplyTaxPolicy(policy, time);

        // 3. Assert: Zero change in approval or stability
        Assert.Equal(0.0, feedback.ApprovalDelta);
        Assert.Equal(0.0, feedback.StabilityDelta);
        Assert.Equal(68.5, country.HappinessRating);
        Assert.Equal(75.0, country.BaselineStability);
        Assert.Contains("Tax policy maintained at 25%", feedback.Description);
        Assert.Contains("Citizen sentiment remains steady", feedback.Description);
    }

    [Fact]
    public void ExtremeTaxationThreshold_InflictsStabilityPenalty_AndRestoresUponRepeal()
    {
        // 1. Arrange: Baseline 25% tax, 75% stability, 68.5% approval
        var country = CreateTestCountry(startingStability: 75.0, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act 1: Raise income tax to 55% (crosses the > 50% extreme threshold)
        var extremePolicy = new TaxPolicy { IncomeTaxRate = 0.55, CorporateTaxRate = 0.20 };
        var feedback1 = country.ApplyTaxPolicy(extremePolicy, time);

        // Assert 1: Stability dropped by -2.0% due to extreme societal stress
        Assert.Equal(-2.0, feedback1.StabilityDelta);
        Assert.Equal(73.0, country.BaselineStability);
        Assert.Contains("extreme tax (>50%) reduced national stability by 2%", feedback1.Description);

        // 3. Act 2: Adjusting tax from 55% to 60% (still within > 50% territory) does NOT duplicate stability penalty
        var furtherExtreme = new TaxPolicy { IncomeTaxRate = 0.60, CorporateTaxRate = 0.20 };
        var feedback2 = country.ApplyTaxPolicy(furtherExtreme, time);
        Assert.Equal(0.0, feedback2.StabilityDelta);
        Assert.Equal(73.0, country.BaselineStability);

        // 4. Act 3: Repeal tax back down to 40% (easing extreme taxation to <= 50%)
        var moderatePolicy = new TaxPolicy { IncomeTaxRate = 0.40, CorporateTaxRate = 0.20 };
        var feedback3 = country.ApplyTaxPolicy(moderatePolicy, time);

        // Assert 3: Restores the +2.0% stability
        Assert.Equal(2.0, feedback3.StabilityDelta);
        Assert.Equal(75.0, country.BaselineStability);
        Assert.Contains("Easing extreme taxation restored national stability by 2%", feedback3.Description);
    }

    [Fact]
    public void PresidentialWorkspaceHandler_AdjustTaxPolicy_RequiresConfirmation()
    {
        // 1. Arrange
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;
        var proposedPolicy = new TaxPolicy { IncomeTaxRate = 0.35, CorporateTaxRate = 0.20 };

        // 2. Act: Cancelled by executive
        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, proposedPolicy, time, confirmed: false);

        // 3. Assert: Cancelled, zero mutations on country state
        Assert.False(outcome.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Cancelled, outcome.Result);
        Assert.Equal(0.25, country.TaxPolicy.IncomeTaxRate);
        Assert.Equal(68.5, country.HappinessRating);
        Assert.Null(country.LatestCitizenFeedback);
        Assert.Contains("cancelled by executive order", outcome.Message);
    }

    [Fact]
    public void PresidentialWorkspaceHandler_AdjustTaxPolicy_RejectsInvalidRates()
    {
        // 1. Arrange
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act & Assert: NaN rate
        var nanPolicy = new TaxPolicy { IncomeTaxRate = double.NaN };
        var outcomeNan = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, nanPolicy, time, confirmed: true);
        Assert.Equal(ExecutiveActionResult.InvalidInput, outcomeNan.Result);

        // 3. Act & Assert: Negative rate
        var negativePolicy = new TaxPolicy { IncomeTaxRate = -0.10 };
        var outcomeNegative = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, negativePolicy, time, confirmed: true);
        Assert.Equal(ExecutiveActionResult.InvalidInput, outcomeNegative.Result);

        // 4. Act & Assert: Exceeds constitutional limit (> 70%)
        var overPolicy = new TaxPolicy { IncomeTaxRate = 0.85 };
        var outcomeOver = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, overPolicy, time, confirmed: true);
        Assert.Equal(ExecutiveActionResult.InvalidInput, outcomeOver.Result);

        // Country remains completely unchanged
        Assert.Equal(0.25, country.TaxPolicy.IncomeTaxRate);
        Assert.Equal(68.5, country.HappinessRating);
    }

    [Fact]
    public void SovereignIsolation_AdjustingTaxForCountryA_DoesNotAffectCountryB()
    {
        // 1. Arrange: Two sovereign nations
        var time = _clock.CurrentTime;
        var countryA = CreateTestCountry("Republic of Alpha", startingApproval: 70.0, startingIncomeTax: 0.25);
        var countryB = CreateTestCountry("Republic of Beta", startingApproval: 80.0, startingIncomeTax: 0.20);

        var initialApprovalB = countryB.ApprovalRating;
        var initialTaxB = countryB.TaxPolicy.IncomeTaxRate;
        var initialStabilityB = countryB.BaselineStability;

        // 2. Act: Enact major tax hike in Country A
        var policyA = new TaxPolicy { IncomeTaxRate = 0.40, CorporateTaxRate = 0.20 };
        var outcomeA = PresidentialWorkspaceHandler.AdjustTaxPolicy(countryA, policyA, time, confirmed: true);

        // 3. Assert: Country A updated, Country B completely isolated
        Assert.True(outcomeA.IsSuccess);
        Assert.Equal(0.40, countryA.TaxPolicy.IncomeTaxRate);
        Assert.Equal(62.5, countryA.HappinessRating);

        Assert.Equal(initialTaxB, countryB.TaxPolicy.IncomeTaxRate);
        Assert.Equal(initialApprovalB, countryB.ApprovalRating);
        Assert.Equal(initialStabilityB, countryB.BaselineStability);
        Assert.Null(countryB.LatestCitizenFeedback);
    }

    [Fact]
    public void NationalDashboard_ReflectsUpdatedTaxationAndCitizenFeedback()
    {
        // 1. Arrange
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;

        // 2. Act: Enact tax relief to 20%
        country.ApplyTaxPolicy(new TaxPolicy { IncomeTaxRate = 0.20, CorporateTaxRate = 0.18 }, time);
        var dashboard = country.GetDashboard(time);

        // 3. Assert: Read model accurately reflects new state without mutation
        Assert.Equal("71%", dashboard.Approval);
        Assert.Equal(71.0, dashboard.ApprovalRating);
        Assert.Equal("Income: 20% | Corporate: 18%", dashboard.Taxation);
        Assert.NotNull(dashboard.LatestCitizenFeedback);
        Assert.Contains("Tax relief enacted", dashboard.CitizenSentiment);
        Assert.Contains("Taxation: Income: 20% | Corporate: 18%", dashboard.Lines);
        Assert.Contains("Approval: 71%", dashboard.Lines);
    }

    [Fact]
    public async Task ExecutiveAction_AdjustTaxPolicy_WhenConfirmed_EnactsFeedbackAndSyncsDemographics()
    {
        // 1. Arrange: Application with player country
        var app = CreateTestApplication();
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country");
        Assert.NotNull(playerCountry);
        playerCountry.HappinessRating = 68.5;
        playerCountry.TaxPolicy.IncomeTaxRate = 0.25;
        var time = app.TimeSystem.CurrentRepublicTime;

        // 2. Act: Propose and confirm 30% income tax (+5% hike)
        var proposedPolicy = new TaxPolicy { IncomeTaxRate = 0.30, CorporateTaxRate = playerCountry.TaxPolicy.CorporateTaxRate };
        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(playerCountry, proposedPolicy, time, confirmed: true);

        if (outcome.IsSuccess)
        {
            await app.BudgetService.UpdateTaxPolicyAsync(proposedPolicy);
            var demo = app.WorldManager.Demographics.GetDemographics();
            demo.HappinessRating = playerCountry.HappinessRating;
        }

        // 3. Assert:
        Assert.True(outcome.IsSuccess);
        Assert.Equal(0.30, playerCountry.TaxPolicy.IncomeTaxRate);
        Assert.Equal(66.0, playerCountry.HappinessRating);
        Assert.Equal(66.0, playerCountry.ApprovalRating);
        Assert.NotNull(playerCountry.LatestCitizenFeedback);
        Assert.Equal(-2.5, playerCountry.LatestCitizenFeedback.ApprovalDelta);
        Assert.Contains("Tax hike enacted", outcome.Message);

        var updatedDemo = app.WorldManager.Demographics.GetDemographics();
        Assert.Equal(66.0, updatedDemo.HappinessRating);
    }

    [Fact]
    public void ExecutiveAction_AdjustTaxPolicy_WhenDeclined_PreservesAllState()
    {
        // 1. Arrange
        var app = CreateTestApplication();
        var playerCountry = app.WorldManager.Countries.GetCountry("player-country");
        Assert.NotNull(playerCountry);
        playerCountry.HappinessRating = 68.5;
        playerCountry.TaxPolicy.IncomeTaxRate = 0.25;
        var time = app.TimeSystem.CurrentRepublicTime;

        // 2. Act: Executive cancels decision (confirmed: false)
        var proposedPolicy = new TaxPolicy { IncomeTaxRate = 0.35, CorporateTaxRate = 0.20 };
        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(playerCountry, proposedPolicy, time, confirmed: false);

        // 3. Assert: Unchanged
        Assert.False(outcome.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Cancelled, outcome.Result);
        Assert.Equal(0.25, playerCountry.TaxPolicy.IncomeTaxRate);
        Assert.Equal(68.5, playerCountry.HappinessRating);
        Assert.Null(playerCountry.LatestCitizenFeedback);
        Assert.Contains("cancelled by executive order", outcome.Message);
    }

    [Fact]
    public void SaveLoadPersistence_PreservesTaxPolicyAndCitizenFeedback()
    {
        // 1. Arrange: Country with custom policy and citizen feedback
        var country = CreateTestCountry(startingApproval: 68.5, startingIncomeTax: 0.25);
        var time = _clock.CurrentTime;
        country.ApplyTaxPolicy(new TaxPolicy { IncomeTaxRate = 0.32, CorporateTaxRate = 0.22 }, time);

        // 2. Act: Serialize and Deserialize via JSON
        var json = JsonSerializer.Serialize(country);
        var deserialized = JsonSerializer.Deserialize<Country>(json);

        // 3. Assert: Restored state matches
        Assert.NotNull(deserialized);
        Assert.Equal(0.32, deserialized.TaxPolicy.IncomeTaxRate);
        Assert.Equal(0.22, deserialized.TaxPolicy.CorporateTaxRate);
        Assert.Equal(country.HappinessRating, deserialized.HappinessRating);
        Assert.Equal(country.BaselineStability, deserialized.BaselineStability);
        Assert.NotNull(deserialized.LatestCitizenFeedback);
        Assert.Equal(-3.5, deserialized.LatestCitizenFeedback.ApprovalDelta);
        Assert.NotNull(country.LatestCitizenFeedback);
        Assert.Equal(country.LatestCitizenFeedback.Description, deserialized.LatestCitizenFeedback.Description);
    }
}
