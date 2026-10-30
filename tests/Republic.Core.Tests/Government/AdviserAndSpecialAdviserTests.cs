namespace Republic.Core.Tests.Government;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Government.Advisers.Models;
using Republic.Core.Government.Advisers.Services;
using Republic.Core.Identity.Models;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;
using Xunit;

public sealed class AdviserAndSpecialAdviserTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _statePath;
    private readonly string _savePath;

    public AdviserAndSpecialAdviserTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Republic_Adviser_Tests_" + Guid.NewGuid().ToString("N"));
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

    private void SetupWorldWithCompletedPlans(string accountId = "acc_elena_1", string countryId = "player-country")
    {
        var simTime = new RepublicTime(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var factory = new CountryProfileFactory();
        var country = factory.CreateCountry("Republic of Solaria", 42, simTime, id: countryId);

        country.LeadershipTypes = new List<LeadershipArchetype>
        {
            LeadershipArchetype.Visionary,
            LeadershipArchetype.Technocrat
        };
        country.LeadershipDoctrine = "Technocratic Visionary";
        country.DevelopmentPlans = new List<NationalDevelopmentPriority>
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
            CurrentStep = OnboardingStep.DevelopmentPlansSelected,
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
            OnboardingStep = OnboardingStep.DevelopmentPlansSelected,
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
    public void AdviserRoster_DefaultsContainGeneralAdviserPressSecretaryAndFiveSlots()
    {
        var country = new Country { Id = "test-country", Name = "Republic of Testing" };

        Assert.NotNull(country.GeneralAdviser);
        Assert.Equal("Marcus Vance", country.GeneralAdviser.Name);
        Assert.Equal("Principal Strategic Guide", country.GeneralAdviser.Role);
        Assert.True(country.GeneralAdviser.IsAppointed);

        Assert.NotNull(country.PressSecretary);
        Assert.Equal("Elena Vance", country.PressSecretary.Name);
        Assert.Equal("Official Spokesperson & Media Relations", country.PressSecretary.Role);
        Assert.True(country.PressSecretary.IsAppointed);

        Assert.NotNull(country.SpecialAdvisers);
        Assert.Equal(5, country.SpecialAdvisers.Count);

        // Slot 1: Active
        var slot1 = country.SpecialAdvisers[0];
        Assert.Equal(1, slot1.SlotNumber);
        Assert.Equal("Political and Social Affairs", slot1.Portfolio);
        Assert.Equal("Dr. Kemi Adeleke", slot1.AdviserName);
        Assert.Equal(SpecialAdviserSlotStatus.Active, slot1.Status);
        Assert.True(slot1.IsUnlocked);

        // Slots 2-5: Locked
        for (int i = 1; i < 5; i++)
        {
            var slot = country.SpecialAdvisers[i];
            Assert.Equal(i + 1, slot.SlotNumber);
            Assert.Equal(SpecialAdviserSlotStatus.Locked, slot.Status);
            Assert.False(slot.IsUnlocked);
            Assert.True(slot.GemCost > 0);
            Assert.False(string.IsNullOrWhiteSpace(slot.RequiredBuff));
        }

        Assert.Equal(500, country.SovereignGems);
    }

    [Fact]
    public void GeneralAdviser_GuidanceAdaptsToDoctrineAndPlans()
    {
        var country = new Country
        {
            Id = "c1",
            Name = "Republic of Solaria",
            LeadershipDoctrine = "Technocratic Visionary",
            DevelopmentPlans = new List<NationalDevelopmentPriority>
            {
                NationalDevelopmentPriority.IndustrialisationFirst,
                NationalDevelopmentPriority.AgriculturalTransformation,
                NationalDevelopmentPriority.TechnologyAndInnovation,
                NationalDevelopmentPriority.InfrastructureRevolution
            }
        };

        AdviserService.SynchronizeAdvisers(country);

        Assert.Contains("Technocratic Visionary", country.GeneralAdviser.StrategicGuidance);
        Assert.Contains("Industrialisation First", country.GeneralAdviser.StrategicGuidance);
        Assert.Contains("Infrastructure Revolution", country.GeneralAdviser.StrategicGuidance);

        Assert.NotEmpty(country.GeneralAdviser.Recommendations);
        Assert.Contains(country.GeneralAdviser.Recommendations, r => r.Contains("Technocratic Visionary"));
        Assert.Contains(country.GeneralAdviser.Recommendations, r => r.Contains("Industrialisation First"));

        Assert.Contains("Technocratic Visionary", country.PressSecretary.BriefingHeadline);
        Assert.True(country.PressSecretary.ApprovalImpact > 0);
    }

    [Fact]
    public void IntroduceAdvisersDecision_TransitionsOnboardingToAdvisersIntroduced()
    {
        SetupWorldWithCompletedPlans();

        var result = PresidentialDeskBridge.IntroduceAdvisersDecision(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_advisers_1",
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal(OnboardingStep.AdvisersIntroduced.ToString(), result.OnboardingStep);
        Assert.NotNull(result.GeneralAdviser);
        Assert.Equal("Marcus Vance", result.GeneralAdviser.Name);
        Assert.NotNull(result.PressSecretary);
        Assert.Equal("Elena Vance", result.PressSecretary.Name);
        Assert.Equal(5, result.SpecialAdvisers.Count);
        Assert.Equal(SpecialAdviserSlotStatus.Active, result.SpecialAdvisers[0].Status);
        Assert.Equal(SpecialAdviserSlotStatus.Locked, result.SpecialAdvisers[1].Status);

        // Verify state snapshot on disk
        var envelope = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.State);
        Assert.NotNull(envelope.State!.Account);
        Assert.Equal(OnboardingStep.AdvisersIntroduced, envelope.State!.OnboardingStep);
        Assert.Equal(OnboardingStep.AdvisersIntroduced, envelope.State!.Account!.CurrentStep);
    }

    [Fact]
    public void IntroduceAdvisersDecision_EnforcesMultiplayerOwnership()
    {
        SetupWorldWithCompletedPlans();

        // 1. Unauthorized account ID
        var badAccount = PresidentialDeskBridge.IntroduceAdvisersDecision(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "wrong_account");
        Assert.False(badAccount.Success);
        Assert.Equal("Unauthorized", badAccount.Result);

        // 2. Unauthorized country ID
        var badCountry = PresidentialDeskBridge.IntroduceAdvisersDecision(
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "other_foreign_country");
        Assert.False(badCountry.Success);
        Assert.Equal("Unauthorized", badCountry.Result);
    }

    [Fact]
    public void UnlockSpecialAdviserDecision_WithGems_DeductsGemsAndActivatesSlot()
    {
        SetupWorldWithCompletedPlans();

        // Slot 2 costs 500 gems, player starts with 500 gems
        var unlockResult = PresidentialDeskBridge.UnlockSpecialAdviserDecision(
            slotNumber: 2,
            unlockMethod: "gems",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.True(unlockResult.Success);
        Assert.Equal("Success", unlockResult.Result);
        Assert.Equal(0, unlockResult.SovereignGems);

        var slot2 = unlockResult.SpecialAdvisers.First(s => s.SlotNumber == 2);
        Assert.Equal(SpecialAdviserSlotStatus.Active, slot2.Status);
        Assert.Equal("Dr. Arthur Sterling", slot2.AdviserName);
        Assert.False(string.IsNullOrWhiteSpace(slot2.Advice));

        // Re-locking check / already unlocked check
        var dupResult = PresidentialDeskBridge.UnlockSpecialAdviserDecision(
            slotNumber: 2,
            unlockMethod: "gems",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.False(dupResult.Success);
        Assert.Equal("UnlockFailed", dupResult.Result);
        Assert.Contains("already unlocked", dupResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnlockSpecialAdviserDecision_InsufficientGems_RejectsWithValidation()
    {
        SetupWorldWithCompletedPlans();

        // Slot 3 costs 1000 gems, player only has 500 gems
        var failResult = PresidentialDeskBridge.UnlockSpecialAdviserDecision(
            slotNumber: 3,
            unlockMethod: "gems",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestingAccountId: "acc_elena_1",
            targetCountryId: "player-country");

        Assert.False(failResult.Success);
        Assert.Equal("UnlockFailed", failResult.Result);
        Assert.Contains("Insufficient Sovereign Gems", failResult.Message);
        Assert.Equal(500, failResult.SovereignGems);

        var slot3 = failResult.SpecialAdvisers.First(s => s.SlotNumber == 3);
        Assert.Equal(SpecialAdviserSlotStatus.Locked, slot3.Status);
    }
}
