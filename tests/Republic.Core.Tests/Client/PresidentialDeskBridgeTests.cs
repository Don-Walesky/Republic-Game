namespace Republic.Core.Tests.Client;

using System;
using System.IO;
using Republic.Core.Client;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class PresidentialDeskBridgeTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));

    private static RepublicTime CreateTime(int days, int hours = 0, int minutes = 0)
    {
        var timestamp = LaunchEpoch.AddDays(days).AddHours(hours).AddMinutes(minutes);
        return new RepublicTime(timestamp, LaunchEpoch);
    }

    [Fact]
    public void CreateSnapshot_AccuratelyCapturesCountryAndDashboardLiveState()
    {
        // Arrange
        var foundingTime = CreateTime(10);
        var treasury = new RepuTreasury(15_000_000_000.0, "player-country");
        var country = Country.Found("Republic of Arcadia", foundingTime, "player-country", treasury: treasury);
        country.HappinessRating = 78.5;
        country.BaselineStability = 85.0;
        country.FoundParty("Civic Democratic Union", foundingTime);

        var simTime = CreateTime(12, 14, 30);

        // Act
        var snapshot = PresidentialDeskSnapshot.Create(country, simTime, simulationTick: 42);

        // Assert
        Assert.Equal("Republic of Arcadia", snapshot.CountryName);
        Assert.Equal(15_000_000_000.0, snapshot.TreasuryBalance);
        Assert.Equal("R 15,000,000,000", snapshot.Treasury);
        Assert.Equal(78.5, snapshot.ApprovalRating);
        Assert.Equal("78.5%", snapshot.Approval);
        Assert.Equal(85.0, snapshot.StabilityRating);
        Assert.Equal("85%", snapshot.Stability);
        Assert.Equal(12, snapshot.SimulationDay);
        Assert.Equal("Day 12, 14:30 WAT", snapshot.SimulationTime);
        Assert.Contains("Day 12", snapshot.SimulationTimeDetailed);
        Assert.Equal("Civic Democratic Union", snapshot.RulingParty);
        Assert.Equal("No election", snapshot.Election);
        Assert.Equal(42UL, snapshot.SimulationTick);
        Assert.Equal("Connected to live Republic simulation state", snapshot.StatusMessage);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.ExportedAtUtc));
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTripsJsonFidelity()
    {
        // Arrange
        var time = CreateTime(5, 6, 0);
        var treasury = new RepuTreasury(2_500_000_000.0, "solaria-id");
        var country = Country.Found("Republic of Solaria", time, "solaria-id", treasury: treasury);
        country.HappinessRating = 62.0;
        country.BaselineStability = 70.0;

        var original = PresidentialDeskSnapshot.Create(country, time, 100);

        // Act
        var json = PresidentialDeskBridge.Serialize(original);
        var deserialized = PresidentialDeskBridge.Deserialize(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.CountryName, deserialized.CountryName);
        Assert.Equal(original.TreasuryBalance, deserialized.TreasuryBalance);
        Assert.Equal(original.Treasury, deserialized.Treasury);
        Assert.Equal(original.ApprovalRating, deserialized.ApprovalRating);
        Assert.Equal(original.Approval, deserialized.Approval);
        Assert.Equal(original.StabilityRating, deserialized.StabilityRating);
        Assert.Equal(original.Stability, deserialized.Stability);
        Assert.Equal(original.SimulationDay, deserialized.SimulationDay);
        Assert.Equal(original.SimulationTime, deserialized.SimulationTime);
        Assert.Equal(original.RulingParty, deserialized.RulingParty);
        Assert.Equal(original.Election, deserialized.Election);
        Assert.Equal(original.SimulationTick, deserialized.SimulationTick);
    }

    [Fact]
    public void StateUpdates_ReflectsTimeAdvancementAndTreasuryMutations()
    {
        // Arrange
        var time1 = CreateTime(1);
        var treasury = new RepuTreasury(10_000_000_000.0, "arcadia");
        var country = Country.Found("Republic of Arcadia", time1, "arcadia", treasury: treasury);

        var snapshot1 = PresidentialDeskSnapshot.Create(country, time1);
        Assert.Equal(1, snapshot1.SimulationDay);
        Assert.Equal("R 10,000,000,000", snapshot1.Treasury);

        // Act: Mutate treasury and advance time
        country.Treasury.Deposit(2_500_000_000.0);
        var time2 = CreateTime(8, 18, 0);
        var snapshot2 = PresidentialDeskSnapshot.Create(country, time2, simulationTick: 500);

        // Assert
        Assert.Equal(8, snapshot2.SimulationDay);
        Assert.Equal(12_500_000_000.0, snapshot2.TreasuryBalance);
        Assert.Equal("R 12,500,000,000", snapshot2.Treasury);
        Assert.Equal("Day 8, 18:00 WAT", snapshot2.SimulationTime);
        Assert.Equal(500UL, snapshot2.SimulationTick);
    }

    [Fact]
    public void StateUpdates_ReflectsTaxationPolicyAndCitizenApprovalConsequences()
    {
        // Arrange
        var time = CreateTime(1);
        var country = Country.Found("Republic of Arcadia", time, "arcadia");
        country.HappinessRating = 70.0;
        country.BaselineStability = 80.0;

        var snapshotBefore = PresidentialDeskSnapshot.Create(country, time);
        Assert.Equal(70.0, snapshotBefore.ApprovalRating);
        Assert.Equal("70%", snapshotBefore.Approval);

        // Act: Enact tax hike (+10% tax -> -5% approval penalty)
        var newPolicy = new TaxPolicy
        {
            IncomeTaxRate = 0.35,
            CorporateTaxRate = 0.20,
            ImportTariffRate = 0.10
        };
        var outcome = PresidentialWorkspaceHandler.AdjustTaxPolicy(country, newPolicy, time, confirmed: true);
        Assert.True(outcome.IsSuccess);

        var snapshotAfter = PresidentialDeskSnapshot.Create(country, time);

        // Assert: Citizen approval reaction accurately reflected in snapshot
        Assert.Equal(65.0, snapshotAfter.ApprovalRating);
        Assert.Equal("65%", snapshotAfter.Approval);
        Assert.Equal(80.0, snapshotAfter.StabilityRating);
    }

    [Fact]
    public void ExportState_WritesFormattedJsonFileToDisk_AndLoadStateReadsBack()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_test_state_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(3, 12, 0);
            var treasury = new RepuTreasury(5_000_000_000.0, "valoria");
            var country = Country.Found("Republic of Valoria", time, "valoria", treasury: treasury);
            country.HappinessRating = 60.0;

            // Act
            var exportedPath = PresidentialDeskBridge.ExportState(country, time, 25, tempFile);

            // Assert
            Assert.Equal(tempFile, exportedPath);
            Assert.True(File.Exists(tempFile));

            var loaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(loaded);
            Assert.Equal("Republic of Valoria", loaded.CountryName);
            Assert.Equal(5_000_000_000.0, loaded.TreasuryBalance);
            Assert.Equal(60.0, loaded.ApprovalRating);
            Assert.Equal(3, loaded.SimulationDay);
            Assert.Equal(25UL, loaded.SimulationTick);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void LoadState_ReturnsNullForNonExistentFile()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"non_existent_{Guid.NewGuid():N}.json");
        var loaded = PresidentialDeskBridge.LoadState(nonExistentPath);
        Assert.Null(loaded);
    }

    [Fact]
    public void LoadState_ReturnsNullForCorruptJsonFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"corrupt_{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(tempFile, "{ broken json content");
            var loaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.Null(loaded);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void DefaultPathResolution_FindsGodotFolderOrRelativeDefault()
    {
        var path = PresidentialDeskBridge.ResolveDefaultStateFilePath();
        Assert.False(string.IsNullOrWhiteSpace(path));
        Assert.EndsWith(PresidentialDeskBridge.DefaultFileName, path);
        Assert.True(Path.IsPathRooted(path));
    }

    [Fact]
    public void Election_ReflectsHeldNationalDemocraticElection()
    {
        // Arrange
        var time = CreateTime(10);
        var country = Country.Found("Republic of Arcadia", time, "arcadia");
        country.FoundParty("Social Democratic Alliance", time);
        country.FoundParty("Conservative Party", time);

        // Act: Hold national election
        var election = country.HoldElection(time);
        Assert.NotNull(election);

        var snapshot = PresidentialDeskSnapshot.Create(country, time);

        // Assert: Election details captured in snapshot
        Assert.Contains(election.WinnerPartyName, snapshot.Election);
        Assert.Contains($"Day {election.Boundary.DayNumber}", snapshot.Election);
        Assert.Equal(country.RulingParty!.Name, snapshot.RulingParty);
    }

    [Fact]
    public void Snapshot_IncludesTaxationPolicyAndFormattedRates()
    {
        // Arrange
        var time = CreateTime(2);
        var country = Country.Found("Republic of Arcadia", time, "arcadia");
        country.TaxPolicy.IncomeTaxRate = 0.30;
        country.TaxPolicy.CorporateTaxRate = 0.22;
        country.TaxPolicy.ImportTariffRate = 0.08;

        // Act
        var snapshot = PresidentialDeskSnapshot.Create(country, time);

        // Assert
        Assert.Equal(0.30, snapshot.IncomeTaxRate);
        Assert.Equal("30%", snapshot.IncomeTaxRateFormatted);
        Assert.Equal(0.22, snapshot.CorporateTaxRate);
        Assert.Equal(0.08, snapshot.ImportTariffRate);
        Assert.Equal("arcadia", snapshot.CountryId);
    }

    [Fact]
    public void ApplyTaxDecision_WithValidConfirmedHike_UpdatesCountryAndExportsState()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_tax_hike_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.HappinessRating = 70.0;
            country.BaselineStability = 80.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            // Export baseline state
            PresidentialDeskBridge.ExportState(country, time, 10, tempFile);

            // Act: Apply tax hike from 25% to 30% (+5% tax -> -2.5% approval)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.30, confirmed: true, tempFile);

            // Assert
            Assert.True(actionResult.Success);
            Assert.Equal("Success", actionResult.Result);
            Assert.Equal(0.30, actionResult.IncomeTaxRate);
            Assert.Equal(0.25, actionResult.PreviousIncomeTaxRate);
            Assert.Equal(67.5, actionResult.ApprovalRating);
            Assert.Equal(70.0, actionResult.PreviousApprovalRating);
            Assert.Contains("Tax hike enacted", actionResult.Message);

            // Verify persisted state on disk
            var reloaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.30, reloaded.IncomeTaxRate);
            Assert.Equal("30%", reloaded.IncomeTaxRateFormatted);
            Assert.Equal(67.5, reloaded.ApprovalRating);
            Assert.Equal("67.5%", reloaded.Approval);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithValidConfirmedCut_IncreasesApproval()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_tax_cut_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.HappinessRating = 60.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 5, tempFile);

            // Act: Apply tax cut from 25% to 20% (-5% tax -> +2.5% approval)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.20, confirmed: true, tempFile);

            // Assert
            Assert.True(actionResult.Success);
            Assert.Equal(0.20, actionResult.IncomeTaxRate);
            Assert.Equal(62.5, actionResult.ApprovalRating);
            Assert.Equal(60.0, actionResult.PreviousApprovalRating);
            Assert.Contains("Tax relief enacted", actionResult.Message);

            var reloaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.20, reloaded.IncomeTaxRate);
            Assert.Equal(62.5, reloaded.ApprovalRating);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithExtremeRateOver50Percent_InflictsStabilityPenalty()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_extreme_tax_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.HappinessRating = 70.0;
            country.BaselineStability = 85.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 1, tempFile);

            // Act: Enact 55% tax rate (> 50% threshold)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.55, confirmed: true, tempFile);

            // Assert
            Assert.True(actionResult.Success);
            Assert.Equal(0.55, actionResult.IncomeTaxRate);
            Assert.Equal(83.0, actionResult.StabilityRating); // -2.0% stability penalty
            Assert.Equal(85.0, actionResult.PreviousStabilityRating);
            Assert.Contains("stability", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            var reloaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(reloaded);
            Assert.Equal(83.0, reloaded.StabilityRating);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WhenCancelled_LeavesStateAndDiskCompletelyUnchanged()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_tax_cancel_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.HappinessRating = 72.0;
            country.BaselineStability = 80.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 1, tempFile);

            // Act: Propose 40% tax rate, but cancel
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.40, confirmed: false, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("Cancelled", actionResult.Result);
            Assert.Equal(0.25, actionResult.IncomeTaxRate);
            Assert.Equal(72.0, actionResult.ApprovalRating);
            Assert.Contains("cancelled by executive order", actionResult.Message);

            // Verify disk state was preserved
            var reloaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.25, reloaded.IncomeTaxRate);
            Assert.Equal(72.0, reloaded.ApprovalRating);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithInvalidRate_RejectsConstitutionalViolation()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_invalid_tax_{Guid.NewGuid():N}.json");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.HappinessRating = 70.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 1, tempFile);

            // Act: Propose 85% rate (> 70% constitutional maximum)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.85, confirmed: true, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("InvalidInput", actionResult.Result);
            Assert.Equal(0.25, actionResult.IncomeTaxRate);
            Assert.Equal(70.0, actionResult.ApprovalRating);
            Assert.Contains("constitutional bounds", actionResult.Message);

            // Verify state preserved
            var reloaded = PresidentialDeskBridge.LoadState(tempFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.25, reloaded.IncomeTaxRate);
            Assert.Equal(70.0, reloaded.ApprovalRating);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            var companion = Path.ChangeExtension(tempFile, ".sav");
            if (File.Exists(companion)) File.Delete(companion);
        }
    }

    [Fact]
    public void EstimateTaxImpact_CalculatesPhase11PreviewAndWarningsAccurately()
    {
        // 1. No change
        var noChange = PresidentialDeskBridge.EstimateTaxImpact(0.25, 0.25);
        Assert.Contains("No change", noChange);

        // 2. Hike +5% -> -2.5% approval
        var hike = PresidentialDeskBridge.EstimateTaxImpact(0.25, 0.30);
        Assert.Contains("-2.5%", hike);
        Assert.Contains("Tax hike", hike);

        // 3. Cut -5% -> +2.5% approval
        var cut = PresidentialDeskBridge.EstimateTaxImpact(0.25, 0.20);
        Assert.Contains("+2.5%", cut);
        Assert.Contains("Tax cut", cut);

        // 4. Over 50% boundary warning
        var extreme = PresidentialDeskBridge.EstimateTaxImpact(0.25, 0.55);
        Assert.Contains("-2.0%", extreme);
        Assert.Contains("Stability penalty", extreme);
    }

    [Fact]
    public void ApplyTaxDecision_ValidTaxDecision_PersistsToAuthoritativeSaveFile_AndSurvivesReload()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_persist_test_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        try
        {
            var time = CreateTime(5, 10, 0);
            var treasury = new RepuTreasury(25_000_000_000.0, "player-country");
            var country = Country.Found("Republic of Arcadia", time, "player-country", treasury: treasury);
            country.HappinessRating = 72.0;
            country.BaselineStability = 82.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            // Export baseline state and authoritative save
            PresidentialDeskBridge.ExportState(country, time, 100, tempFile);
            Assert.True(File.Exists(companionSav), "Companion .sav must exist.");

            // Act: Apply tax hike from 25% to 31% (+6% tax -> -3.0% approval)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.31, confirmed: true, tempFile);

            // Assert
            Assert.True(actionResult.Success);
            Assert.Equal("Success", actionResult.Result);
            Assert.Equal(0.31, actionResult.IncomeTaxRate);
            Assert.Equal(69.0, actionResult.ApprovalRating);

            // Act: Reload authoritative simulation state from .sav file
            var loadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(companionSav);
            Assert.NotNull(loadedEnvelope);
            Assert.NotNull(loadedEnvelope.State?.World?.Countries);
            Assert.NotEmpty(loadedEnvelope.State.World.Countries);

            var loadedCountry = loadedEnvelope.State.World.Countries.First();
            Assert.Equal("player-country", loadedCountry.Id);
            Assert.Equal("Republic of Arcadia", loadedCountry.Name);
            Assert.Equal(0.31, loadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(69.0, loadedCountry.HappinessRating);
            Assert.Equal(82.0, loadedCountry.BaselineStability);
            Assert.Equal(25_000_000_000.0, loadedCountry.Treasury.Balance);
            Assert.Equal(101UL, loadedEnvelope.State.CurrentTick);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_SecondLoad_RetainsExactCountryIdentityAndRelevantState()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_second_load_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        try
        {
            var foundingTime = CreateTime(3);
            var treasury = new RepuTreasury(18_500_000_000.0, "player-country");
            var country = Country.Found("Republic of Arcadia", foundingTime, "player-country", treasury: treasury);
            country.HappinessRating = 68.0;
            country.BaselineStability = 75.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, foundingTime, 50, tempFile);

            // Apply decision
            var result = PresidentialDeskBridge.ApplyTaxDecision(0.28, confirmed: true, tempFile);
            Assert.True(result.Success);

            // First load
            var firstEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(companionSav);
            Assert.NotNull(firstEnvelope?.State?.World?.Countries);
            Assert.NotEmpty(firstEnvelope.State.World.Countries);
            var firstCountry = firstEnvelope.State.World.Countries.First();

            // Second load
            var secondEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(companionSav);
            Assert.NotNull(secondEnvelope?.State?.World?.Countries);
            Assert.NotEmpty(secondEnvelope.State.World.Countries);
            var secondCountry = secondEnvelope.State.World.Countries.First();

            // Assert exact preservation across loads
            Assert.Equal(firstCountry.Id, secondCountry.Id);
            Assert.Equal(firstCountry.Name, secondCountry.Name);
            Assert.Equal(firstCountry.FoundingTime, secondCountry.FoundingTime);
            Assert.Equal(firstCountry.Treasury.Balance, secondCountry.Treasury.Balance);
            Assert.Equal(firstCountry.TaxPolicy.IncomeTaxRate, secondCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(firstCountry.HappinessRating, secondCountry.HappinessRating);
            Assert.Equal(firstCountry.BaselineStability, secondCountry.BaselineStability);
            Assert.Equal(firstEnvelope.State.CurrentTick, secondEnvelope.State.CurrentTick);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithMissingStateFile_IsRejected_WithoutCreatingDefaultCountryOrFiles()
    {
        // Arrange
        var missingPath = Path.Combine(Path.GetTempPath(), $"missing_state_{Guid.NewGuid():N}.json");
        var missingSav = Path.ChangeExtension(missingPath, ".sav");

        try
        {
            // Act: Apply tax decision against nonexistent file
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.30, confirmed: true, missingPath);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("does not exist", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            // Ensure no files were created and no sample country was founded
            Assert.False(File.Exists(missingPath));
            Assert.False(File.Exists(missingSav));
        }
        finally
        {
            if (File.Exists(missingPath)) File.Delete(missingPath);
            if (File.Exists(missingSav)) File.Delete(missingSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithCorruptStateFile_IsRejected_WithoutOverwritingOriginalFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"corrupt_state_{Guid.NewGuid():N}.json");
        const string CorruptContent = "{ broken: json: definitely: not: valid !!!";

        try
        {
            File.WriteAllText(tempFile, CorruptContent);

            // Act
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.30, confirmed: true, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("corrupted or invalid JSON", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            // Verify original file was not overwritten
            Assert.True(File.Exists(tempFile));
            Assert.Equal(CorruptContent, File.ReadAllText(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithCorruptSaveFile_IsRejected_WithoutOverwritingOriginalFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"valid_snapshot_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        const string CorruptSavContent = "{ invalid_sav: true, corrupted: [ ";

        try
        {
            // Write valid snapshot pointing to corrupt save
            var snapshotJson = $"{{\"country_name\": \"Republic of Arcadia\", \"save_file_path\": \"{companionSav.Replace("\\", "\\\\")}\"}}";
            File.WriteAllText(tempFile, snapshotJson);
            File.WriteAllText(companionSav, CorruptSavContent);

            // Act
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.30, confirmed: true, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("corrupted or contains invalid data", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            // Verify original corrupt .sav was NOT overwritten
            Assert.Equal(CorruptSavContent, File.ReadAllText(companionSav));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WhenCancelled_LeavesAuthoritativeSaveAndCountryCompletelyUnchanged()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"cancel_persist_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        try
        {
            var time = CreateTime(2);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 74.0;
            country.BaselineStability = 84.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 10, tempFile);
            var originalSavPayload = File.ReadAllText(companionSav);

            // Act: Propose 50% rate, but cancel
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.50, confirmed: false, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("Cancelled", actionResult.Result);
            Assert.Equal(0.25, actionResult.IncomeTaxRate);
            Assert.Equal(74.0, actionResult.ApprovalRating);
            Assert.Equal(84.0, actionResult.StabilityRating);

            // Verify .sav payload reloads exactly identical values
            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(companionSav);
            Assert.NotNull(reloaded?.State?.World?.Countries);
            Assert.NotEmpty(reloaded.State.World.Countries);
            var loadedCountry = reloaded.State.World.Countries.First();
            Assert.Equal(0.25, loadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(74.0, loadedCountry.HappinessRating);
            Assert.Equal(84.0, loadedCountry.BaselineStability);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.05)]
    [InlineData(0.71)]
    [InlineData(1.50)]
    public void ApplyTaxDecision_WithInvalidAndNonFiniteRates_RejectsWithoutModifyingCountryState(double invalidRate)
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"invalid_rates_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.BaselineStability = 80.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 1, tempFile);

            // Act
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(invalidRate, confirmed: true, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("InvalidInput", actionResult.Result);
            Assert.Equal(0.25, actionResult.IncomeTaxRate);
            Assert.Equal(70.0, actionResult.ApprovalRating);

            // Verify save file unchanged
            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(companionSav);
            Assert.NotNull(reloaded?.State?.World?.Countries);
            Assert.NotEmpty(reloaded.State.World.Countries);
            var loadedCountry = reloaded.State.World.Countries.First();
            Assert.Equal(0.25, loadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(70.0, loadedCountry.HappinessRating);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WhenPersistenceFails_ReturnsPersistenceFailure_AndRevertsInMemMutations()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"persist_fail_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempFile, ".sav");
        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.BaselineStability = 80.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 1, tempFile);

            // Make the companion .sav read-only to trigger an IO / persistence failure
            File.SetAttributes(companionSav, FileAttributes.ReadOnly);

            // Act: Apply tax hike
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.35, confirmed: true, tempFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("PersistenceFailure", actionResult.Result);
            Assert.Contains("Failed to save authoritative simulation state", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            // Verify in-memory values reported in result were reverted
            Assert.Equal(0.25, actionResult.IncomeTaxRate);
            Assert.Equal(70.0, actionResult.ApprovalRating);
        }
        finally
        {
            if (File.Exists(companionSav))
            {
                File.SetAttributes(companionSav, FileAttributes.Normal);
                File.Delete(companionSav);
            }
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_WhenRequestFileExists_AppliesDecision_UpdatesStateFile_AndDeletesRequestFile()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_state_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_file_{Guid.NewGuid():N}.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.BaselineStability = 80.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 10, tempStateFile, tempSaveFile);

            // Write request file proposing 32% income tax rate
            var requestJson = """
            {
                "proposed_rate": 0.32,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);
            Assert.True(File.Exists(tempRequestFile));

            // Act: Call bridge to process pending request
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0.32, result.IncomeTaxRate);
            Assert.Equal(66.5, result.ApprovalRating);
            Assert.False(File.Exists(tempRequestFile), "Request file must be deleted after processing.");

            // Verify state file was updated
            var reloaded = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.32, reloaded.IncomeTaxRate);
            Assert.Equal("32%", reloaded.IncomeTaxRateFormatted);
            Assert.Equal(66.5, reloaded.ApprovalRating);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }
}
