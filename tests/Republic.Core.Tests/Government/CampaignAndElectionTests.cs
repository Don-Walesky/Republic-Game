namespace Republic.Core.Tests.Government;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Government;
using Republic.Core.Government.Campaign.Services;
using Republic.Core.Identity.Models;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Xunit;

public sealed class CampaignAndElectionTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _statePath;
    private readonly string _savePath;

    public CampaignAndElectionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Republic_Campaign_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        _statePath = Path.Combine(_testDir, "presidential_desk_state.json");
        _savePath = Path.Combine(_testDir, "session.sav");
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

    private void SetupWorldAtAdvisersStage(string accountId = "acc_elena_1", string countryId = "player-country")
    {
        var simTime = new RepublicTime(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry("Republic of Solaria", 42, simTime, id: countryId);

        country.LeadershipTypes = new() { LeadershipArchetype.Visionary, LeadershipArchetype.Technocrat };
        country.LeadershipDoctrine = "Technocratic Visionary";
        country.DevelopmentPlans = new()
        {
            NationalDevelopmentPriority.IndustrialisationFirst,
            NationalDevelopmentPriority.AgriculturalTransformation,
            NationalDevelopmentPriority.TechnologyAndInnovation,
            NationalDevelopmentPriority.InfrastructureRevolution
        };

        var account = new PlayerAccount
        {
            AccountId = accountId,
            PersonalIdentity = new PlayerPersonalIdentity
            {
                FirstName = "Elena",
                LastName = "Okafor",
                EmailAddress = "elena.okafor@republic.net",
                DateOfBirth = new DateOnly(1991, 3, 20),
                IsEmailVerified = true
            },
            OwnedCountryId = countryId,
            CurrentStep = OnboardingStep.AdvisersIntroduced,
            SelectedLeadershipArchetypes = country.LeadershipTypes,
            LeadershipDoctrineTitle = country.LeadershipDoctrine,
            SelectedDevelopmentPlans = country.DevelopmentPlans
        };

        var world = new WorldState();
        world.Countries.Add(country);

        var fullState = new FullGameState
        {
            World = world,
            Account = account,
            OnboardingStep = OnboardingStep.AdvisersIntroduced,
            CurrentTick = 100
        };

        var envelope = new SaveEnvelope<FullGameState>
        {
            SavedAt = DateTimeOffset.UtcNow,
            State = fullState
        };

        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(_savePath, serializer.Serialize(envelope));
        PresidentialDeskBridge.ExportState(world, country, simTime, 100, _statePath, _savePath);
    }

    [Fact]
    public void CanonicalDecisions_FollowsSeventyThirtyConsequenceRule()
    {
        var decisions = CampaignDecision.CreateCanonicalDecisions();

        // Exactly 10 canonical decisions
        Assert.Equal(10, decisions.Count);

        // 70:30 Distribution: Exactly 7 Net-Positive (70%), 3 Net-Negative (30%)
        var positiveCount = decisions.Count(d => d.IsNetPositive);
        var negativeCount = decisions.Count(d => !d.IsNetPositive);

        Assert.Equal(7, positiveCount);
        Assert.Equal(3, negativeCount);

        // Every single decision must have identifiable positive and negative consequences
        foreach (var d in decisions)
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Id), "Decision ID must not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(d.Title), "Decision title must not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(d.ForeseeablePositiveConsequence), $"Decision '{d.Title}' missing foreseeable positive consequence.");
            Assert.False(string.IsNullOrWhiteSpace(d.ForeseeableNegativeConsequence), $"Decision '{d.Title}' missing foreseeable negative consequence.");
            Assert.True(d.CostRepu >= 0m, $"Decision '{d.Title}' cost cannot be negative.");
        }
    }

    [Fact]
    public void EnterCampaignHeadquarters_InitializesPurseAndTeam()
    {
        SetupWorldAtAdvisersStage();

        var result = PresidentialDeskBridge.EnterCampaignHeadquartersDecision(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_enter_hq_1",
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal(OnboardingStep.CampaignHeadquarters.ToString(), result.OnboardingStep);

        // R 5,000,000 REPU initial purse
        Assert.Equal(5_000_000m, result.CampaignPurse);
        Assert.Contains("5,000,000", result.CampaignPurseFormatted);
        Assert.Equal(35.0m, result.CampaignPolling);

        // Campaign Accountant General verified
        Assert.NotNull(result.AccountantGeneral);
        Assert.Equal("Kaelen Thorne", result.AccountantGeneral.Name);
        Assert.Equal(5_000_000m, result.AccountantGeneral.CurrentPurse);
        Assert.Contains("Clean Audit", result.AccountantGeneral.AuditStatus);

        // 10 Available decisions ready
        Assert.Equal(10, result.AvailableCampaignDecisions.Count);
        Assert.Empty(result.CompletedCampaignDecisions);

        // General Adviser & Press Secretary active
        Assert.NotNull(result.GeneralAdviser);
        Assert.NotNull(result.PressSecretary);
    }

    [Fact]
    public void ExecuteCampaignDecision_DeductsPurseAndAppliesConsequences()
    {
        SetupWorldAtAdvisersStage();
        PresidentialDeskBridge.EnterCampaignHeadquartersDecision(stateFilePath: _statePath, saveFilePath: _savePath);

        // Execute "Meet Business Association" (Cost: 200k, Donations: +800k, Polling: +5%)
        var result = PresidentialDeskBridge.ExecuteCampaignDecisionAction(
            decisionId: "dec_meet_business",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_exec_1",
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);

        // 5M - 200k + 800k = 5.6M REPU
        Assert.Equal(5_600_000m, result.CampaignPurse);
        Assert.Equal(40.0m, result.CampaignPolling); // 35% + 5% = 40%

        // Accountant general tracked expenditures
        Assert.NotNull(result.AccountantGeneral);
        Assert.Equal(200_000m, result.AccountantGeneral.TotalExpenditures);
        Assert.Equal(5_600_000m, result.AccountantGeneral.CurrentPurse);

        // 9 Available, 1 Completed
        Assert.Equal(9, result.AvailableCampaignDecisions.Count);
        Assert.Single(result.CompletedCampaignDecisions);

        var executed = result.CompletedCampaignDecisions[0];
        Assert.Equal("dec_meet_business", executed.Id);
        Assert.True(executed.Executed);
        Assert.Contains("Positive consequence", executed.OutcomeExplanation);
        Assert.Contains("Negative consequence", executed.OutcomeExplanation);
    }

    [Fact]
    public void ExecuteCampaignDecision_InsufficientFunds_RejectsTransaction()
    {
        SetupWorldAtAdvisersStage();
        PresidentialDeskBridge.EnterCampaignHeadquartersDecision(stateFilePath: _statePath, saveFilePath: _savePath);

        // Manually artificially drain purse to test rejection
        var envelope = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.State);
        var country = envelope!.State!.World.Countries.First(c => c.Id == "player-country");
        country.CampaignOffice.Purse = 100_000m; // Less than 1.2M gala cost
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(_savePath, serializer.Serialize(envelope));

        var failResult = PresidentialDeskBridge.ExecuteCampaignDecisionAction(
            decisionId: "dec_expensive_gala", // Costs 1.2M
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.False(failResult.Success);
        Assert.Equal("CampaignDecisionFailed", failResult.Result);
        Assert.Contains("Insufficient Campaign Purse funds", failResult.Message);
    }

    [Fact]
    public void ResolveElectionNight_AboveFiftyPercent_WinsPresidency()
    {
        SetupWorldAtAdvisersStage();
        PresidentialDeskBridge.EnterCampaignHeadquartersDecision(stateFilePath: _statePath, saveFilePath: _savePath);

        // Execute 3 positive decisions to lift polling from 35% to 52%:
        // 1. Meet Business (+5% -> 40%)
        PresidentialDeskBridge.ExecuteCampaignDecisionAction("dec_meet_business", _statePath, _savePath);
        // 2. Meet Labour (+6% -> 46%)
        PresidentialDeskBridge.ExecuteCampaignDecisionAction("dec_meet_labor", _statePath, _savePath);
        // 3. Media Blitz (+7% -> 53%)
        PresidentialDeskBridge.ExecuteCampaignDecisionAction("dec_media_blitz", _statePath, _savePath);

        // Contest election night
        var electionResult = PresidentialDeskBridge.ResolveElectionDecisionAction(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_election_1",
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(electionResult.Success);
        Assert.Equal("Victory", electionResult.Result);
        Assert.True(electionResult.ElectionWon);
        Assert.True(electionResult.ElectionTally >= 50.0m);
        Assert.Contains("VICTORY", electionResult.ElectionSummary);
        Assert.Equal(OnboardingStep.InOffice.ToString(), electionResult.OnboardingStep);

        // Authoritative verification on disk
        var env = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(env);
        Assert.NotNull(env.State);
        Assert.NotNull(env.State!.Account);
        Assert.Equal(OnboardingStep.InOffice, env!.State!.OnboardingStep);
        Assert.Equal(OnboardingStep.InOffice, env!.State!.Account!.CurrentStep);
    }

    [Fact]
    public void ResolveElectionNight_UnderFiftyPercent_DefeatedAndAllowsRally()
    {
        SetupWorldAtAdvisersStage();
        PresidentialDeskBridge.EnterCampaignHeadquartersDecision(stateFilePath: _statePath, saveFilePath: _savePath);

        // Execute net-negative decisions that drop polling:
        // Gala (-3% -> 32%)
        PresidentialDeskBridge.ExecuteCampaignDecisionAction("dec_expensive_gala", _statePath, _savePath);

        // Drain remaining purse to prevent funding bonus
        var envelope = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.State);
        var country = envelope!.State!.World.Countries.First(c => c.Id == "player-country");
        country.CampaignOffice.Purse = 0m;
        var serializer = new JsonStateSerializer();
        AtomicFileWriter.WriteAllTextSafely(_savePath, serializer.Serialize(envelope));

        var electionResult = PresidentialDeskBridge.ResolveElectionDecisionAction(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(electionResult.Success);
        Assert.Equal("Defeat", electionResult.Result);
        Assert.False(electionResult.ElectionWon);
        Assert.True(electionResult.ElectionTally < 50.0m);
        Assert.Contains("DEFEAT", electionResult.ElectionSummary);
    }
}
