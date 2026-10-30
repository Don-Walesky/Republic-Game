namespace Republic.Core.Tests.Identity;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Identity.Models;
using Republic.Core.Identity.Services;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Xunit;

public sealed class LeadershipAndDevelopmentPlanTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _statePath;
    private readonly string _savePath;
    private readonly string _requestPath;

    public LeadershipAndDevelopmentPlanTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Republic_Phase3_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        _statePath = Path.Combine(_testDir, "presidential_desk_state.json");
        _savePath = Path.Combine(_testDir, "session.sav");
        _requestPath = Path.Combine(_testDir, "presidential_desk_request.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // cleanup best-effort
        }
    }

    [Fact]
    public void ValidateLeadershipSelection_RequiresExactlyTwoDistinctArchetypes()
    {
        var service = new PlayerIdentityService();

        // 1. Null / Empty
        var (valEmpty, _, _, _) = service.ValidateLeadershipSelection(null);
        Assert.False(valEmpty);

        var (valZero, _, _, _) = service.ValidateLeadershipSelection(Array.Empty<string>());
        Assert.False(valZero);

        // 2. Only 1 archetype
        var (valOne, msgOne, _, _) = service.ValidateLeadershipSelection(new[] { "technocrat" });
        Assert.False(valOne);
        Assert.Contains("Exactly two", msgOne);

        // 3. Duplicates (2 identical archetypes)
        var (valDup, msgDup, _, _) = service.ValidateLeadershipSelection(new[] { "technocrat", "technocrat" });
        Assert.False(valDup);
        Assert.Contains("duplicate", msgDup, StringComparison.OrdinalIgnoreCase);

        // 4. 3 archetypes
        var (valThree, msgThree, _, _) = service.ValidateLeadershipSelection(new[] { "technocrat", "populist_reformer", "statist_builder" });
        Assert.False(valThree);
        Assert.Contains("Exactly two", msgThree);

        // 5. Unrecognized archetype
        var (valBad, msgBad, _, _) = service.ValidateLeadershipSelection(new[] { "technocrat", "anarcho_capitalist" });
        Assert.False(valBad);
        Assert.Contains("Unrecognized leadership archetype", msgBad);

        // 6. Valid 2 archetypes
        var (valOk, msgOk, archetypes, doctrineTitle) = service.ValidateLeadershipSelection(new[] { "technocrat", "economic_visionary" });
        Assert.True(valOk);
        Assert.Equal(2, archetypes.Count);
        Assert.Contains(LeadershipArchetype.Technocrat, archetypes);
        Assert.Contains(LeadershipArchetype.EconomicVisionary, archetypes);
        Assert.Equal("Technocratic Visionary", doctrineTitle);
    }

    [Fact]
    public void ValidateDevelopmentPlansSelection_RequiresExactlyFourDistinctPriorities()
    {
        var service = new PlayerIdentityService();

        // 1. Null / Empty
        var (valEmpty, _, _) = service.ValidateDevelopmentPlansSelection(null);
        Assert.False(valEmpty);

        // 2. 3 priorities (too few)
        var (valThree, msgThree, _) = service.ValidateDevelopmentPlansSelection(new[]
        {
            "agrarian_self_sufficiency",
            "mineral_industrialization",
            "renewable_power_grid"
        });
        Assert.False(valThree);
        Assert.Contains("Exactly four", msgThree);

        // 3. 5 priorities (too many)
        var (valFive, msgFive, _) = service.ValidateDevelopmentPlansSelection(new[]
        {
            "agrarian_self_sufficiency",
            "mineral_industrialization",
            "renewable_power_grid",
            "higher_education_research",
            "border_security_defense"
        });
        Assert.False(valFive);
        Assert.Contains("Exactly four", msgFive);

        // 4. Duplicates
        var (valDup, msgDup, _) = service.ValidateDevelopmentPlansSelection(new[]
        {
            "agrarian_self_sufficiency",
            "mineral_industrialization",
            "renewable_power_grid",
            "agrarian_self_sufficiency"
        });
        Assert.False(valDup);
        Assert.Contains("duplicate", msgDup, StringComparison.OrdinalIgnoreCase);

        // 5. Unrecognized priority
        var (valBad, msgBad, _) = service.ValidateDevelopmentPlansSelection(new[]
        {
            "agrarian_self_sufficiency",
            "mineral_industrialization",
            "renewable_power_grid",
            "interstellar_warp_drive"
        });
        Assert.False(valBad);
        Assert.Contains("Unrecognized national development priority", msgBad);

        // 6. Valid 4 priorities
        var (valOk, msgOk, priorities) = service.ValidateDevelopmentPlansSelection(new[]
        {
            "agrarian_self_sufficiency",
            "mineral_industrialization",
            "renewable_power_grid",
            "higher_education_research"
        });
        Assert.True(valOk);
        Assert.Equal(4, priorities.Count);
        Assert.Contains(NationalDevelopmentPriority.AgrarianSelfSufficiency, priorities);
        Assert.Contains(NationalDevelopmentPriority.HigherEducationResearch, priorities);
    }

    [Fact]
    public void FullPhase3Flow_LeadershipAndDevelopmentPlanSelection_AdvancesProgressionAndPersistsAuthoritatively()
    {
        var refDate = new DateOnly(2027, 2, 10);

        // Precondition Check: Selecting leadership before country creation fails
        var earlyLeaderResult = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "technocrat", "populist_reformer" },
            _statePath, _savePath, "req_early_1");
        Assert.False(earlyLeaderResult.Success);
        Assert.Equal("InvalidOnboardingStep", earlyLeaderResult.Result);

        // Step 1: Register Account
        var reg = PresidentialDeskBridge.RegisterAccountDecision(
            "Elena", "Okafor", "elena@republic.net", "1991-03-20",
            _statePath, _savePath, "req_reg", refDate);
        Assert.True(reg.Success);

        // Step 2: Verify Email
        var ver = PresidentialDeskBridge.VerifyEmailDecision(reg.VerificationToken, _statePath, _savePath, "req_ver");
        Assert.True(ver.Success);

        // Step 3: Found Sovereign Country
        var found = PresidentialDeskBridge.CreateOrLoadCountry("Republic of Solaria", _statePath, _savePath, "req_found");
        Assert.True(found.Success);
        Assert.Equal("CountryCreated", found.OnboardingStep);

        // Precondition Check: Selecting development plans before leadership fails
        var earlyPlanResult = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[] { "agrarian_self_sufficiency", "mineral_industrialization", "renewable_power_grid", "higher_education_research" },
            _statePath, _savePath, "req_early_plans");
        Assert.False(earlyPlanResult.Success);
        Assert.Equal("InvalidOnboardingStep", earlyPlanResult.Result);

        // Step 4: Select Leadership Identity (Phase 3 Part 1)
        var leaderResult = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "technocrat", "statist_builder" },
            _statePath, _savePath, "req_leader_1");

        Assert.True(leaderResult.Success);
        Assert.Equal("Success", leaderResult.Result);
        Assert.Equal("LeadershipSelected", leaderResult.OnboardingStep);
        Assert.Equal("Technocratic Strategist", leaderResult.LeadershipDoctrineTitle);
        Assert.Equal(2, leaderResult.SelectedLeadershipArchetypes.Count);
        Assert.Contains("Technocrat", leaderResult.SelectedLeadershipArchetypes);
        Assert.Contains("Strategist", leaderResult.SelectedLeadershipArchetypes);

        // Step 5: Select National Development Agenda (Phase 3 Part 2)
        var devResult = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[]
            {
                "agricultural_transformation",
                "sustainable_development",
                "human_capital_development",
                "technology_and_innovation"
            },
            _statePath, _savePath, "req_dev_1");

        Assert.True(devResult.Success);
        Assert.Equal("Success", devResult.Result);
        Assert.Equal("DevelopmentPlansSelected", devResult.OnboardingStep);
        Assert.Equal(4, devResult.SelectedDevelopmentPlans.Count);
        Assert.Contains("AgriculturalTransformation", devResult.SelectedDevelopmentPlans);
        Assert.Contains("TechnologyAndInnovation", devResult.SelectedDevelopmentPlans);
        Assert.Equal(4, devResult.SelectedDevelopmentPlanNames.Count);

        // Step 6: Verify Authoritative Save Round-Trip
        var serializer = new JsonStateSerializer();
        var rawSave = File.ReadAllText(_savePath);
        var envelope = serializer.Deserialize<FullGameState>(rawSave);

        Assert.NotNull(envelope?.State?.Account);
        Assert.Equal(OnboardingStep.DevelopmentPlansSelected, envelope.State.OnboardingStep);
        Assert.Equal(OnboardingStep.DevelopmentPlansSelected, envelope.State.Account.CurrentStep);
        Assert.Equal("Technocratic Strategist", envelope.State.Account.LeadershipDoctrineTitle);
        Assert.Equal(2, envelope.State.Account.SelectedLeadershipArchetypes.Count);
        Assert.Equal(4, envelope.State.Account.SelectedDevelopmentPlans.Count);

        // Step 7: Verify Public Snapshot Integrity & PII Isolation
        var rawSnapshot = File.ReadAllText(_statePath);
        Assert.DoesNotContain("elena@republic.net", rawSnapshot);
        Assert.DoesNotContain("1991-03-20", rawSnapshot);

        using var doc = JsonDocument.Parse(rawSnapshot);
        var root = doc.RootElement;
        Assert.Equal("DevelopmentPlansSelected", root.GetProperty("onboarding_step").GetString());
        Assert.Equal("Technocratic Strategist", root.GetProperty("leadership_doctrine_title").GetString());
        Assert.Equal(2, root.GetProperty("selected_leadership_archetypes").GetArrayLength());
        Assert.Equal(4, root.GetProperty("selected_development_plans").GetArrayLength());
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesIpcSelectionActions()
    {
        var refDate = new DateOnly(2027, 2, 10);

        // Setup country
        var reg = PresidentialDeskBridge.RegisterAccountDecision("Marcus", "Vance", "marcus@republic.net", "1988-11-05", _statePath, _savePath, "r1", refDate);
        PresidentialDeskBridge.VerifyEmailDecision(reg.VerificationToken, _statePath, _savePath, "r2");
        PresidentialDeskBridge.CreateOrLoadCountry("Republic of Vance", _statePath, _savePath, "r3");

        // 1. IPC Leadership Request
        var leaderReqJson = JsonSerializer.Serialize(new
        {
            action = "select_leadership",
            archetypes = new[] { "populist_reformer", "economic_visionary" },
            request_id = "ipc_leader_1"
        });
        File.WriteAllText(_requestPath, leaderReqJson);

        var leaderResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath, _requestPath);
        Assert.NotNull(leaderResult);
        Assert.True(leaderResult.Success);
        Assert.Equal("Visionary Champion", leaderResult.LeadershipDoctrineTitle);
        Assert.Equal("LeadershipSelected", leaderResult.OnboardingStep);
        Assert.False(File.Exists(_requestPath)); // Request file deleted after processing

        // 2. IPC Development Plans Request
        var devReqJson = JsonSerializer.Serialize(new
        {
            action = "select_development_plans",
            plans = new[] { "mineral_industrialization", "border_security_defense", "maritime_trade_ports", "fiscal_reserve_fortress" },
            request_id = "ipc_dev_1"
        });
        File.WriteAllText(_requestPath, devReqJson);

        var devResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath, _requestPath);
        Assert.NotNull(devResult);
        Assert.True(devResult.Success);
        Assert.Equal("DevelopmentPlansSelected", devResult.OnboardingStep);
        Assert.Equal(4, devResult.SelectedDevelopmentPlans.Count);
        Assert.False(File.Exists(_requestPath));
    }

    [Fact]
    public void LeadershipAndDevelopmentPlans_EnforcesMultiplayerOwnershipValidation()
    {
        var refDate = new DateOnly(2027, 2, 10);
        var reg = PresidentialDeskBridge.RegisterAccountDecision("Amina", "Diallo", "amina@republic.net", "1994-07-15", _statePath, _savePath, "r_own_1", refDate);
        PresidentialDeskBridge.VerifyEmailDecision(reg.VerificationToken, _statePath, _savePath, "r_own_2");
        var found = PresidentialDeskBridge.CreateOrLoadCountry("Republic of Diallo", _statePath, _savePath, "r_own_3");

        // 1. Leadership ownership check: Impostor account ID
        var impostorLeader = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "visionary", "technocrat" },
            _statePath, _savePath, "r_lead_fail",
            requestingAccountId: "acc_impostor_999");

        Assert.False(impostorLeader.Success);
        Assert.Equal("Unauthorized", impostorLeader.Result);

        // 2. Leadership ownership check: Wrong country ID
        var wrongCountryLeader = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "visionary", "technocrat" },
            _statePath, _savePath, "r_lead_fail_2",
            targetCountryId: "country_foreign_foreign");

        Assert.False(wrongCountryLeader.Success);
        Assert.Equal("Unauthorized", wrongCountryLeader.Result);

        // 3. Legitimate leadership selection succeeds
        var legitimateLeader = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "visionary", "technocrat" },
            _statePath, _savePath, "r_lead_ok");
        Assert.True(legitimateLeader.Success);

        // 4. Plans ownership check: Impostor account ID
        var impostorPlans = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[] { "industrialisation_first", "agricultural_transformation", "technology_and_innovation", "infrastructure_revolution" },
            _statePath, _savePath, "r_plan_fail",
            requestingAccountId: "acc_impostor_999");

        Assert.False(impostorPlans.Success);
        Assert.Equal("Unauthorized", impostorPlans.Result);

        // 5. Plans ownership check: Wrong country ID
        var wrongCountryPlans = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[] { "industrialisation_first", "agricultural_transformation", "technology_and_innovation", "infrastructure_revolution" },
            _statePath, _savePath, "r_plan_fail_2",
            targetCountryId: "country_foreign_foreign");

        Assert.False(wrongCountryPlans.Success);
        Assert.Equal("Unauthorized", wrongCountryPlans.Result);

        // 6. Legitimate plans selection succeeds
        var legitimatePlans = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[] { "industrialisation_first", "agricultural_transformation", "technology_and_innovation", "infrastructure_revolution" },
            _statePath, _savePath, "r_plan_ok");
        Assert.True(legitimatePlans.Success);
    }

    [Fact]
    public void LeadershipAndDevelopmentPlans_GeneratesAuthoritativeObjectivesAndOpportunities()
    {
        var refDate = new DateOnly(2027, 2, 10);
        var reg = PresidentialDeskBridge.RegisterAccountDecision("Kenzo", "Tanaka", "kenzo@republic.net", "1989-05-12", _statePath, _savePath, "rk1", refDate);
        PresidentialDeskBridge.VerifyEmailDecision(reg.VerificationToken, _statePath, _savePath, "rk2");
        PresidentialDeskBridge.CreateOrLoadCountry("Republic of Yamato", _statePath, _savePath, "rk3");

        // Step 1: Select Leadership -> Expect strategic opportunities generated
        var leaderResult = PresidentialDeskBridge.SelectLeadershipDecision(
            new[] { "strategist", "diplomat" },
            _statePath, _savePath, "rk4");

        Assert.True(leaderResult.Success);
        Assert.Equal(2, leaderResult.StrategicOpportunities.Count);
        Assert.Contains(leaderResult.StrategicOpportunities, o => o.Source.Contains("Strategist"));
        Assert.Contains(leaderResult.StrategicOpportunities, o => o.Source.Contains("Diplomat"));

        // Step 2: Select 4 National Development Plans -> Expect starting objectives generated
        var devResult = PresidentialDeskBridge.SelectDevelopmentPlansDecision(
            new[]
            {
                "industrialisation_first",
                "energy_and_natural_resources",
                "national_security_and_defence",
                "global_trade_and_international_influence"
            },
            _statePath, _savePath, "rk5");

        Assert.True(devResult.Success);
        Assert.Equal(4, devResult.StartingObjectives.Count);
        Assert.Contains(devResult.StartingObjectives, o => o.Id == "obj_industrialisation_first");
        Assert.Contains(devResult.StartingObjectives, o => o.Id == "obj_energy_and_natural_resources");
        Assert.Contains(devResult.StartingObjectives, o => o.Id == "obj_national_security_and_defence");
        Assert.Contains(devResult.StartingObjectives, o => o.Id == "obj_global_trade_and_international_influence");

        // Step 3: Inspect Authoritative Save Envelope
        var serializer = new JsonStateSerializer();
        var rawSave = File.ReadAllText(_savePath);
        var envelope = serializer.Deserialize<FullGameState>(rawSave);
        var country = envelope?.State?.World?.Countries?.FirstOrDefault(c => c.Name == "Republic of Yamato");

        Assert.NotNull(country);
        Assert.Equal(2, country.LeadershipTypes.Count);
        Assert.Equal(4, country.DevelopmentPlans.Count);
        Assert.Equal(4, country.StartingObjectives.Count);
        Assert.Equal(2, country.Opportunities.Count);

        // Verify CountryProfile has leadership & development metadata
        Assert.NotNull(country.Profile);
        Assert.Equal(2, country.Profile.LeadershipTypes.Count);
        Assert.Equal(4, country.Profile.DevelopmentPlans.Count);

        // Step 4: Verify Chief of Staff / General Adviser Advice References Doctrine & Development Agenda
        var simTime = new Republic.Core.Time.RepublicTime(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var officialsService = new Republic.Core.Government.Officials.Services.ActiveOfficialsService();
        var recommendations = officialsService.EvaluateChiefOfStaffRecommendations(country, simTime);

        var doctrineRec = recommendations.FirstOrDefault(r => r.Category == "ExecutiveDoctrine");
        Assert.NotNull(doctrineRec);
        Assert.Contains(country.LeadershipDoctrine, doctrineRec.Title);

        var devRec = recommendations.FirstOrDefault(r => r.Category == "DevelopmentAgenda");
        Assert.NotNull(devRec);
        Assert.Contains("Industrialisation First", devRec.Title);
    }
}

