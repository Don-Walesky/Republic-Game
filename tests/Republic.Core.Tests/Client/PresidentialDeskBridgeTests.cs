namespace Republic.Core.Tests.Client;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Persistence;
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

            // Write request file proposing 30% income tax rate
            var requestJson = """
            {
                "proposed_income_tax_rate": 0.30,
                "confirmed": true,
                "timestamp": "2026-10-15T18:00:00Z"
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
            Assert.Equal(0.30, result.IncomeTaxRate);
            Assert.Equal(67.5, result.ApprovalRating);
            Assert.False(File.Exists(tempRequestFile), "Request file must be deleted after processing.");

            // Verify state file was updated
            var reloaded = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(reloaded);
            Assert.Equal(0.30, reloaded.IncomeTaxRate);
            Assert.Equal("30%", reloaded.IncomeTaxRateFormatted);
            Assert.Equal(67.5, reloaded.ApprovalRating);

            // Verify result file was written
            var expectedResultFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, "presidential_desk_result.json");
            Assert.True(File.Exists(expectedResultFile), "Result file presidential_desk_result.json must be written.");
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            var expectedResultFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, "presidential_desk_result.json");
            if (File.Exists(expectedResultFile)) File.Delete(expectedResultFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithMissingStateFile_EvenIfCompanionSavExists_IsRejected_WithoutMutatingState()
    {
        // Arrange
        var missingStatePath = Path.Combine(Path.GetTempPath(), $"missing_target_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(missingStatePath, ".sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            // Save authoritative state directly to companion .sav without creating the .json state file
            PresidentialDeskBridge.SaveAuthoritativeState(country, time, 10, companionSav);
            var originalSavPayload = File.ReadAllText(companionSav);

            // Act: Apply tax decision specifying the missing state file
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.35, confirmed: true, stateFilePath: missingStatePath);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("does not exist", actionResult.Message, StringComparison.OrdinalIgnoreCase);

            // Verify .sav was NOT mutated and .json was NOT created
            Assert.False(File.Exists(missingStatePath));
            Assert.Equal(originalSavPayload, File.ReadAllText(companionSav));
        }
        finally
        {
            if (File.Exists(missingStatePath)) File.Delete(missingStatePath);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WhenSnapshotPointsToNonExistentSaveFile_IsRejected()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"snapshot_missing_sav_{Guid.NewGuid():N}.json");
        var nonExistentSav = Path.Combine(Path.GetTempPath(), $"nonexistent_save_{Guid.NewGuid():N}.sav");

        try
        {
            var snapshotJson = $$"""
            {
                "country_name": "Republic of Arcadia",
                "income_tax_rate": 0.25,
                "save_file_path": "{{nonExistentSav.Replace("\\", "\\\\")}}"
            }
            """;
            File.WriteAllText(tempStateFile, snapshotJson);

            // Act: Apply tax decision
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.30, confirmed: true, stateFilePath: tempStateFile);

            // Assert
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("does not exist", actionResult.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(nonExistentSav));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_RepeatedInvocation_DoesNotApplyDecisionTwice()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"repeat_state_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"repeat_req_{Guid.NewGuid():N}.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 10, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act 1: First processing invocation
            var firstResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert 1: First call succeeded and applied decision
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Success);
            Assert.Equal(0.30, firstResult.IncomeTaxRate);
            Assert.Equal(67.5, firstResult.ApprovalRating);
            Assert.False(File.Exists(tempRequestFile), "Request file must be deleted after first execution.");

            // Act 2: Immediate repeated processing invocation
            var secondResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert 2: Second call returns null without re-applying decision
            Assert.Null(secondResult);

            // Verify persistent save file retains exactly one increment and values
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);
            Assert.Equal(11UL, reloadedEnvelope!.State!.CurrentTick);
            var reloadedCountry = reloadedEnvelope.State.World.Countries.First();
            Assert.Equal(0.30, reloadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(67.5, reloadedCountry.HappinessRating);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public async Task ProcessPendingDecisionRequest_ConcurrentCalls_ExecuteDecisionExactlyOnce()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"concurrent_state_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"concurrent_req_{Guid.NewGuid():N}.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.HappinessRating = 70.0;
            country.TaxPolicy.IncomeTaxRate = 0.25;

            PresidentialDeskBridge.ExportState(country, time, 20, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_rate": 0.28,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act: Run 5 concurrent tasks trying to process the same request
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => Task.Run(() => PresidentialDeskBridge.ProcessPendingDecisionRequest(
                    stateFilePath: tempStateFile,
                    saveFilePath: tempSaveFile,
                    requestFilePath: tempRequestFile)))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert: Exactly one invocation processed the request successfully; all others returned null
            var processedResults = results.Where(r => r != null).ToList();
            Assert.Single(processedResults);
            Assert.True(processedResults[0]!.Success);
            Assert.Equal(0.28, processedResults[0]!.IncomeTaxRate);

            // Verify authoritative save reflects exactly one decision application
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State);
            Assert.Equal(21UL, reloadedEnvelope!.State!.CurrentTick);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ExportState_DoesNotReduceNumberOfCountriesInAuthoritativeSave()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"multi_country_state_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            var time = CreateTime(5);
            var arcadia = Country.Found("Republic of Arcadia", time, "arcadia-id",
                treasury: new RepuTreasury(10_000_000_000.0, "arcadia-id"));
            var valoria = Country.Found("Republic of Valoria", time, "valoria-id",
                treasury: new RepuTreasury(25_000_000_000.0, "valoria-id"));
            var solaria = Country.Found("Republic of Solaria", time, "solaria-id",
                treasury: new RepuTreasury(18_000_000_000.0, "solaria-id"));

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Grand Continent",
                CreatedAt = LaunchEpoch,
                CurrentTick = 100,
                Countries = new List<Country> { arcadia, valoria, solaria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 100, tempSaveFile);

            var initialEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(initialEnvelope?.State?.World?.Countries);
            Assert.Equal(3, initialEnvelope.State.World.Countries.Count);

            // Act: Export Arcadia's dashboard state to JSON
            var exportedPath = PresidentialDeskBridge.ExportState(arcadia, time, 101, tempStateFile, tempSaveFile);

            // Assert: Dashboard exported correctly
            Assert.Equal(tempStateFile, exportedPath);
            Assert.True(File.Exists(tempStateFile));

            // Verify authoritative save STILL contains all 3 countries
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);
            Assert.Equal(3, reloadedEnvelope.State.World.Countries.Count);

            var countryIds = reloadedEnvelope.State.World.Countries.Select(c => c.Id).ToList();
            Assert.Contains("arcadia-id", countryIds);
            Assert.Contains("valoria-id", countryIds);
            Assert.Contains("solaria-id", countryIds);

            var loadedArcadia = reloadedEnvelope.State.World.Countries.First(c => c.Id == "arcadia-id");
            var loadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "valoria-id");
            var loadedSolaria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "solaria-id");

            Assert.Equal(10_000_000_000.0, loadedArcadia.Treasury.Balance);
            Assert.Equal(25_000_000_000.0, loadedValoria.Treasury.Balance);
            Assert.Equal(18_000_000_000.0, loadedSolaria.Treasury.Balance);
            Assert.Equal("Grand Continent", reloadedEnvelope.State.World.Name);
            Assert.Equal(world.WorldId, reloadedEnvelope.State.World.WorldId);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void ExportState_WithWorldStateOverload_PreservesCompleteMultiCountryWorldAndIdentity()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"world_export_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            var time = CreateTime(2);
            var worldId = Guid.NewGuid();
            var arcadia = Country.Found("Republic of Arcadia", time, "arcadia-id");
            var valoria = Country.Found("Republic of Valoria", time, "valoria-id");

            var world = new WorldState
            {
                WorldId = worldId,
                Name = "Unified World",
                CreatedAt = LaunchEpoch,
                CurrentTick = 40,
                Countries = new List<Country> { arcadia, valoria }
            };

            // Act
            PresidentialDeskBridge.ExportState(world, arcadia, time, 40, tempStateFile, tempSaveFile);

            // Assert
            var envelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(envelope?.State?.World?.Countries);
            Assert.Equal(2, envelope.State.World.Countries.Count);
            Assert.Equal(worldId, envelope.State.World.WorldId);
            Assert.Equal("Unified World", envelope.State.World.Name);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal("Republic of Arcadia", snapshot.CountryName);
            Assert.Equal(tempSaveFile, snapshot.SaveFilePath);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_PreservesUnrelatedCountriesAndTheirState()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"unrelated_country_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            var time = CreateTime(3);
            var arcadia = Country.Found("Republic of Arcadia", time, "arcadia-id",
                treasury: new RepuTreasury(12_000_000_000.0, "arcadia-id"));
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;

            var valoria = Country.Found("Republic of Valoria", time, "valoria-id",
                capitalCity: "Valopolis",
                territorySizeSqKm: 650_000.0,
                governmentType: "Parliamentary Republic",
                treasury: new RepuTreasury(55_000_000_000.0, "valoria-id"));
            valoria.HappinessRating = 82.5;
            valoria.BaselineStability = 91.0;
            valoria.TaxPolicy.IncomeTaxRate = 0.18;
            valoria.TaxPolicy.CorporateTaxRate = 0.22;
            valoria.TaxPolicy.ImportTariffRate = 0.07;
            valoria.AddResource("Food", 5000.0);
            valoria.AddResource("Energy", 2000.0);

            var solaria = Country.Found("Republic of Solaria", time, "solaria-id",
                treasury: new RepuTreasury(33_000_000_000.0, "solaria-id"));
            solaria.HappinessRating = 65.0;
            solaria.BaselineStability = 77.0;
            solaria.TaxPolicy.IncomeTaxRate = 0.30;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Continental Alliance",
                CreatedAt = LaunchEpoch,
                CurrentTick = 10,
                Countries = new List<Country> { arcadia, valoria, solaria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 10, tempSaveFile);
            PresidentialDeskBridge.ExportState(arcadia, time, 10, tempStateFile, tempSaveFile);

            // Act: Enact a tax hike on Arcadia from 25% to 35% (+10% tax -> -5% approval)
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.35, confirmed: true, tempStateFile);

            // Assert: Arcadia's decision was applied successfully
            Assert.True(actionResult.Success);
            Assert.Equal(0.35, actionResult.IncomeTaxRate);
            Assert.Equal(65.0, actionResult.ApprovalRating);

            // Reload authoritative state from disk
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);
            Assert.Equal(3, reloadedEnvelope.State.World.Countries.Count);

            // 1. Verify Arcadia updated
            var loadedArcadia = reloadedEnvelope.State.World.Countries.First(c => c.Id == "arcadia-id");
            Assert.Equal(0.35, loadedArcadia.TaxPolicy.IncomeTaxRate);
            Assert.Equal(65.0, loadedArcadia.HappinessRating);

            // 2. Verify Valoria remained strictly identical in every dimension
            var loadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "valoria-id");
            Assert.Equal("valoria-id", loadedValoria.Id);
            Assert.Equal("Republic of Valoria", loadedValoria.Name);
            Assert.Equal("Valopolis", loadedValoria.CapitalCity);
            Assert.Equal(650_000.0, loadedValoria.TerritorySizeSqKm);
            Assert.Equal("Parliamentary Republic", loadedValoria.GovernmentType);
            Assert.Equal(55_000_000_000.0, loadedValoria.Treasury.Balance);
            Assert.Equal(0.18, loadedValoria.TaxPolicy.IncomeTaxRate);
            Assert.Equal(0.22, loadedValoria.TaxPolicy.CorporateTaxRate);
            Assert.Equal(0.07, loadedValoria.TaxPolicy.ImportTariffRate);
            Assert.Equal(82.5, loadedValoria.HappinessRating);
            Assert.Equal(91.0, loadedValoria.BaselineStability);
            Assert.Equal(5000.0, loadedValoria.GetResourceQuantity("Food"));
            Assert.Equal(2000.0, loadedValoria.GetResourceQuantity("Energy"));

            // 3. Verify Solaria remained strictly identical
            var loadedSolaria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "solaria-id");
            Assert.Equal("solaria-id", loadedSolaria.Id);
            Assert.Equal(33_000_000_000.0, loadedSolaria.Treasury.Balance);
            Assert.Equal(0.30, loadedSolaria.TaxPolicy.IncomeTaxRate);
            Assert.Equal(65.0, loadedSolaria.HappinessRating);
            Assert.Equal(77.0, loadedSolaria.BaselineStability);

            // 4. Verify World identity preserved
            Assert.Equal("Continental Alliance", reloadedEnvelope.State.World.Name);
            Assert.Equal(world.WorldId, reloadedEnvelope.State.World.WorldId);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void WorldIdentityAndSupportedStateData_SurvivesSaveAndReloadCycle()
    {
        // Arrange
        var tempSaveFile = Path.Combine(Path.GetTempPath(), $"world_fidelity_{Guid.NewGuid():N}.sav");

        try
        {
            var time = CreateTime(10, 8, 30);
            var worldId = Guid.NewGuid();

            var treasury = new RepuTreasury(42_500_000_000.0, "arcadia-fidelity");
            var country = Country.Found(
                name: "Republic of Arcadia",
                foundingTime: time,
                id: "arcadia-fidelity",
                capitalCity: "Arcadia Heights",
                territorySizeSqKm: 520_000.0,
                governmentType: "Constitutional Democratic Republic",
                treasury: treasury);

            country.HappinessRating = 76.5;
            country.BaselineStability = 88.0;
            country.TaxPolicy.IncomeTaxRate = 0.28;
            country.TaxPolicy.CorporateTaxRate = 0.21;
            country.TaxPolicy.ImportTariffRate = 0.09;
            country.AddResource("Food", 1200.0);
            country.AddResource("Steel", 450.0);

            var roadProject = new NationalRoadProject("arcadia-fidelity", time, cost: 750_000.0);
            country.Projects.Add(roadProject);

            var entityId = Guid.NewGuid();
            var world = new WorldState
            {
                WorldId = worldId,
                Name = "Fidelity Test World",
                CreatedAt = LaunchEpoch,
                CurrentTick = 500,
                Entities = new List<WorldEntity>
                {
                    new(entityId, "Province", "Northern Province")
                },
                Countries = new List<Country> { country }
            };

            // Act: Save and reload
            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 500, tempSaveFile, saveName: "fidelity_test");
            var loadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);

            // Assert
            Assert.NotNull(loadedEnvelope?.State?.World);
            var loadedWorld = loadedEnvelope.State.World;

            // World Identity
            Assert.Equal(worldId, loadedWorld.WorldId);
            Assert.Equal("Fidelity Test World", loadedWorld.Name);
            Assert.Equal(LaunchEpoch, loadedWorld.CreatedAt);
            Assert.Equal(500UL, loadedWorld.CurrentTick);
            Assert.Single(loadedWorld.Entities);
            Assert.Equal("Northern Province", loadedWorld.Entities[0].Name);
            Assert.Equal(entityId, loadedWorld.Entities[0].Id);

            // Country Identity & Founding Data
            Assert.Single(loadedWorld.Countries);
            var loadedCountry = loadedWorld.Countries[0];
            Assert.Equal("arcadia-fidelity", loadedCountry.Id);
            Assert.Equal("Republic of Arcadia", loadedCountry.Name);
            Assert.Equal("Arcadia Heights", loadedCountry.CapitalCity);
            Assert.Equal(520_000.0, loadedCountry.TerritorySizeSqKm);
            Assert.Equal("Constitutional Democratic Republic", loadedCountry.GovernmentType);
            Assert.Equal(time, loadedCountry.FoundingTime);

            // Treasury & Taxation
            Assert.Equal(42_500_000_000.0, loadedCountry.Treasury.Balance);
            Assert.Equal(0.28, loadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(0.21, loadedCountry.TaxPolicy.CorporateTaxRate);
            Assert.Equal(0.09, loadedCountry.TaxPolicy.ImportTariffRate);

            // Approval & Stability
            Assert.Equal(76.5, loadedCountry.HappinessRating);
            Assert.Equal(88.0, loadedCountry.BaselineStability);

            // Resource Inventory
            Assert.Equal(1200.0, loadedCountry.GetResourceQuantity("Food"));
            Assert.Equal(450.0, loadedCountry.GetResourceQuantity("Steel"));

            // Development Projects
            Assert.Single(loadedCountry.Projects);
            var loadedProject = loadedCountry.Projects[0];
            Assert.IsType<NationalRoadProject>(loadedProject);
            Assert.Equal("arcadia-fidelity", loadedProject.CountryId);
            Assert.Equal(750_000.0, loadedProject.Cost);
        }
        finally
        {
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void SaveAuthoritativeState_WithCorruptSave_ThrowsInvalidDataException_WithoutOverwritingCorruptFile()
    {
        // Arrange
        var tempSaveFile = Path.Combine(Path.GetTempPath(), $"corrupt_save_guard_{Guid.NewGuid():N}.sav");
        const string CorruptContent = "{ \"formatVersion\": 1, \"state\": { corrupt: [ invalid json text ";

        try
        {
            File.WriteAllText(tempSaveFile, CorruptContent);
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia-guard");

            // Act & Assert: Saving country must reject with InvalidDataException
            var ex1 = Assert.Throws<InvalidDataException>(() =>
                PresidentialDeskBridge.SaveAuthoritativeState(country, time, 1, tempSaveFile));
            Assert.Contains("corrupted or contains invalid data", ex1.Message);

            // Verify file content is strictly unchanged
            Assert.Equal(CorruptContent, File.ReadAllText(tempSaveFile));

            // Act & Assert: Saving world must also reject with InvalidDataException
            var world = new WorldState { Countries = new List<Country> { country } };
            var ex2 = Assert.Throws<InvalidDataException>(() =>
                PresidentialDeskBridge.SaveAuthoritativeState(world, time, 1, tempSaveFile));
            Assert.Contains("corrupted or contains invalid data", ex2.Message);

            // Verify file content is still strictly unchanged
            Assert.Equal(CorruptContent, File.ReadAllText(tempSaveFile));
        }
        finally
        {
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void ExportState_WithCorruptCompanionSave_ThrowsInvalidDataException_WithoutOverwritingCorruptFile()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"corrupt_companion_{Guid.NewGuid():N}.json");
        var companionSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        const string CorruptContent = "{ malformed: incomplete save data";

        try
        {
            File.WriteAllText(companionSaveFile, CorruptContent);
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");

            // Act & Assert: Exporting state must reject with InvalidDataException rather than overwriting
            var ex = Assert.Throws<InvalidDataException>(() =>
                PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, companionSaveFile));
            Assert.Contains("corrupted or contains invalid data", ex.Message);

            // Verify companion save file content was NOT overwritten
            Assert.Equal(CorruptContent, File.ReadAllText(companionSaveFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(companionSaveFile)) File.Delete(companionSaveFile);
        }
    }

    [Fact]
    public void ApplyTaxDecision_WithTargetCountryMissingFromSave_RejectsWithoutMutatingUnrelatedCountries()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"missing_target_country_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            var time = CreateTime(1);
            var valoria = Country.Found("Republic of Valoria", time, "valoria-only",
                treasury: new RepuTreasury(50_000_000_000.0, "valoria-only"));
            valoria.TaxPolicy.IncomeTaxRate = 0.20;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Valoria Domain",
                Countries = new List<Country> { valoria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 1, tempSaveFile);
            var originalSaveBytes = File.ReadAllBytes(tempSaveFile);

            // Snapshot explicitly targets "arcadia-foreign", which does not exist in the world save
            var snapshot = new PresidentialDeskSnapshot
            {
                CountryName = "Republic of Arcadia",
                CountryId = "arcadia-foreign",
                IncomeTaxRate = 0.25,
                SaveFilePath = tempSaveFile
            };
            File.WriteAllText(tempStateFile, PresidentialDeskBridge.Serialize(snapshot));

            // Act: Apply tax decision targeting nonexistent country
            var actionResult = PresidentialDeskBridge.ApplyTaxDecision(0.35, confirmed: true, stateFilePath: tempStateFile);

            // Assert: Rejected with StateMissingOrCorrupt
            Assert.False(actionResult.Success);
            Assert.Equal("StateMissingOrCorrupt", actionResult.Result);
            Assert.Contains("arcadia-foreign", actionResult.Message);

            // Verify save file on disk is strictly unchanged
            Assert.Equal(originalSaveBytes, File.ReadAllBytes(tempSaveFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
        }
    }

    [Fact]
    public void ExportState_WhenAuthoritativeSaveFails_ThrowsException_AndDoesNotWriteMisleadingSnapshot()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"state_fail_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            // Create a companion .sav with corrupt content so SaveAuthoritativeState fails
            const string corruptSaveContent = "{ corrupt_incomplete_envelope: true ";
            File.WriteAllText(companionSav, corruptSaveContent);

            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");

            // Act & Assert: ExportState must throw InvalidDataException and not write snapshot
            Assert.Throws<InvalidDataException>(() =>
                PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, companionSav));

            // Verify misleading snapshot was NOT created
            Assert.False(File.Exists(tempStateFile), "Snapshot must not be written if authoritative save fails.");
            Assert.Equal(corruptSaveContent, File.ReadAllText(companionSav));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void TryExportState_WhenAuthoritativeSaveFails_ReturnsFalse_AndDoesNotWriteSnapshot()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"try_fail_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            const string corruptSaveContent = "{ corrupt_invalid_save: [ ";
            File.WriteAllText(companionSav, corruptSaveContent);

            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");

            // Act: TryExportState
            var success = PresidentialDeskBridge.TryExportState(country, time, out var exportedPath, 1, tempStateFile, companionSav);

            // Assert
            Assert.False(success, "TryExportState must return false when save fails.");
            Assert.Null(exportedPath);
            Assert.False(File.Exists(tempStateFile), "No snapshot should be created on save failure.");
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ExportState_WorldOverload_WhenAuthoritativeSaveFails_ThrowsException_AndDoesNotWriteSnapshot()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"world_fail_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            const string corruptSaveContent = "{ not_a_valid_json: ";
            File.WriteAllText(companionSav, corruptSaveContent);

            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Arcadia World",
                Countries = new List<Country> { country }
            };

            // Act & Assert
            Assert.Throws<InvalidDataException>(() =>
                PresidentialDeskBridge.ExportState(world, country, time, 1, tempStateFile, companionSav));

            Assert.False(File.Exists(tempStateFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ExportReadOnlyDashboard_WritesSnapshot_WithoutCreatingSaveFile_AndMarksStatusAsReadOnly()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"readonly_state_{Guid.NewGuid():N}.json");
        var companionSav = Path.ChangeExtension(tempStateFile, ".sav");

        try
        {
            var time = CreateTime(3);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");

            // Act
            var exportedPath = PresidentialDeskBridge.ExportReadOnlyDashboard(country, time, 5, tempStateFile);

            // Assert
            Assert.Equal(tempStateFile, exportedPath);
            Assert.True(File.Exists(tempStateFile));
            Assert.False(File.Exists(companionSav), "Read-only export must never create an authoritative save file.");

            var loaded = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(loaded);
            Assert.True(string.IsNullOrEmpty(loaded.SaveFilePath));
            Assert.Contains("Read-only presidential dashboard", loaded.StatusMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(companionSav)) File.Delete(companionSav);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_WhenRateFieldIsMissing_RejectsWithInvalidInput_DeletesRequest_AndLeavesCountryUnchanged()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_missing_rate_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_missing_rate_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);
            var originalSavePayload = File.ReadAllText(tempSaveFile);

            // Request omits rate field completely
            var requestJson = """
            {
                "confirmed": true,
                "timestamp": "2026-10-15T19:00:00Z"
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("InvalidInput", result.Result);
            Assert.Contains("missing", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(tempRequestFile), "Invalid request file must be deleted.");

            // Country and save file must be unmodified
            var reloadedSave = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.Equal(0.20, reloadedSave?.State?.World?.Countries?.First()?.TaxPolicy?.IncomeTaxRate);
            Assert.Equal(originalSavePayload, File.ReadAllText(tempSaveFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_WhenConfirmationFieldIsMissing_RejectsWithInvalidInput()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_missing_conf_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_missing_conf_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);
            var originalSavePayload = File.ReadAllText(tempSaveFile);

            // Request omits confirmation field completely
            var requestJson = """
            {
                "proposed_income_tax_rate": 0.35,
                "timestamp": "2026-10-15T19:00:00Z"
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("InvalidInput", result.Result);
            Assert.Contains("confirmation", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(tempRequestFile));

            // State file and save file must be unchanged
            Assert.Equal(originalSavePayload, File.ReadAllText(tempSaveFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Theory]
    [InlineData("\"true\"")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("null")]
    [InlineData("[true]")]
    [InlineData("{\"confirmed\": true}")]
    public void ProcessPendingDecisionRequest_WhenConfirmationFieldIsNotBoolean_RejectsWithInvalidInput(string rawConfirmation)
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_nonbool_conf_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_nonbool_conf_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var requestJson = $"{{\"proposed_income_tax_rate\": 0.30, \"confirmed\": {rawConfirmation}}}";
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("InvalidInput", result.Result);
            Assert.Contains("Boolean", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(tempRequestFile));

            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.Equal(0.20, reloaded?.State?.World?.Countries?.First()?.TaxPolicy?.IncomeTaxRate);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Theory]
    [InlineData("\"NaN\"")]
    [InlineData("\"Infinity\"")]
    [InlineData("\"-Infinity\"")]
    [InlineData("\"not_a_number\"")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("[0.30]")]
    [InlineData("{\"rate\": 0.30}")]
    [InlineData("-0.05")]
    [InlineData("0.71")]
    [InlineData("1.50")]
    [InlineData("\"-5%\"")]
    [InlineData("\"75%\"")]
    public void ProcessPendingDecisionRequest_WithInvalidMalformedOrOutOfRangeRates_RejectsWithInvalidInput(string rawRate)
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_invalid_rate_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_invalid_rate_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var requestJson = $"{{\"proposed_income_tax_rate\": {rawRate}, \"confirmed\": true}}";
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("InvalidInput", result.Result);
            Assert.False(File.Exists(tempRequestFile));

            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.Equal(0.20, reloaded?.State?.World?.Countries?.First()?.TaxPolicy?.IncomeTaxRate);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Theory]
    [InlineData("proposed_income_tax_rate", "0.30", 0.30)]
    [InlineData("proposed_rate", "0.35", 0.35)]
    [InlineData("proposed_rate_percent", "40.0", 0.40)]
    [InlineData("rate", "28", 0.28)]
    [InlineData("income_tax_rate", "0.22", 0.22)]
    [InlineData("proposed_income_tax_rate", "\"30%\"", 0.30)]
    [InlineData("rate", "\"25.5%\"", 0.255)]
    [InlineData("rate", "\"0.32\"", 0.32)]
    public void ProcessPendingDecisionRequest_PreservesSupportForDocumentedFieldNamesAndFormats(
        string fieldName,
        string rawValue,
        double expectedRate)
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_valid_format_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_valid_format_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var requestJson = $"{{\"{fieldName}\": {rawValue}, \"confirmed\": true}}";
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRate, result.IncomeTaxRate, precision: 4);
            Assert.False(File.Exists(tempRequestFile));

            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.Equal(expectedRate, reloaded?.State?.World?.Countries?.First()?.TaxPolicy?.IncomeTaxRate ?? 0, precision: 4);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_WhenCalledRepeatedly_DoesNotApplySameRequestTwice()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_repeat_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = Path.Combine(Path.GetDirectoryName(tempStateFile)!, $"req_repeat_{Guid.NewGuid():N}.req.json");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "arcadia");
            country.TaxPolicy.IncomeTaxRate = 0.20;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_income_tax_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act 1: Process request
            var firstResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            Assert.NotNull(firstResult);
            Assert.True(firstResult.Success);
            Assert.False(File.Exists(tempRequestFile));

            // Act 2: Process again without new request
            var secondResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert: Second call returns null since request file was consumed
            Assert.Null(secondResult);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ExecuteDecisionLoop_MultiCountryWorld_ValidTaxChange_SurvivesReload_AndRefreshesDashboard()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"multi_country_loop_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            var valoria = Country.Found("Commonwealth of Valoria", time, "neighbor-country");
            valoria.TaxPolicy.IncomeTaxRate = 0.20;
            valoria.HappinessRating = 75.0;
            valoria.BaselineStability = 85.0;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Greater Republic World",
                CreatedAt = time.UtcTimestamp,
                CurrentTick = 10,
                Countries = new List<Country> { arcadia, valoria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 10, tempSaveFile);
            PresidentialDeskBridge.ExportState(world, arcadia, time, 10, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_income_tax_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act: Process the decision request
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert: Decision result contract
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal(0.30, result.IncomeTaxRate);
            Assert.Equal(0.25, result.PreviousIncomeTaxRate);
            Assert.Equal(67.5, result.ApprovalRating);
            Assert.Equal(70.0, result.PreviousApprovalRating);
            Assert.Equal(80.0, result.StabilityRating);
            Assert.False(File.Exists(tempRequestFile));
            Assert.True(File.Exists(tempResultFile));

            // Assert: Authoritative saved game survives reload and preserves untouched neighbor
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);
            Assert.Equal(2, reloadedEnvelope.State.World.Countries.Count);

            var reloadedArcadia = reloadedEnvelope.State.World.Countries.First(c => c.Id == "player-country");
            Assert.Equal(0.30, reloadedArcadia.TaxPolicy.IncomeTaxRate);
            Assert.Equal(67.5, reloadedArcadia.HappinessRating);
            Assert.Equal(80.0, reloadedArcadia.BaselineStability);

            var reloadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "neighbor-country");
            Assert.Equal(0.20, reloadedValoria.TaxPolicy.IncomeTaxRate);
            Assert.Equal(75.0, reloadedValoria.HappinessRating);
            Assert.Equal(85.0, reloadedValoria.BaselineStability);

            // Assert: Dashboard state snapshot accurately refreshed
            var reloadedSnapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(reloadedSnapshot);
            Assert.Equal("Republic of Arcadia", reloadedSnapshot.CountryName);
            Assert.Equal(0.30, reloadedSnapshot.IncomeTaxRate);
            Assert.Equal("30%", reloadedSnapshot.IncomeTaxRateFormatted);
            Assert.Equal(67.5, reloadedSnapshot.ApprovalRating);
            Assert.Equal("67.5%", reloadedSnapshot.Approval);
            Assert.Equal(80.0, reloadedSnapshot.StabilityRating);
            Assert.Contains("Tax hike enacted", reloadedSnapshot.LatestDecisionResult);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public void ExecuteDecisionLoop_MultiCountryWorld_CancelledDecision_PreservesWorldAndTaxPolicyUnchanged()
    {
        // Arrange
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"multi_cancel_loop_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            var valoria = Country.Found("Commonwealth of Valoria", time, "neighbor-country");
            valoria.TaxPolicy.IncomeTaxRate = 0.20;
            valoria.HappinessRating = 75.0;
            valoria.BaselineStability = 85.0;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Greater Republic World",
                CreatedAt = time.UtcTimestamp,
                CurrentTick = 10,
                Countries = new List<Country> { arcadia, valoria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 10, tempSaveFile);
            PresidentialDeskBridge.ExportState(world, arcadia, time, 10, tempStateFile, tempSaveFile);

            var cancelRequestJson = """
            {
                "proposed_income_tax_rate": 0.40,
                "confirmed": false
            }
            """;
            File.WriteAllText(tempRequestFile, cancelRequestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert: Cancelled outcome
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Cancelled", result.Result);
            Assert.Equal(0.25, result.IncomeTaxRate);
            Assert.Equal(70.0, result.ApprovalRating);
            Assert.False(File.Exists(tempRequestFile));

            // Assert: Authoritative save file preserved without mutation
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);

            var reloadedArcadia = reloadedEnvelope.State.World.Countries.First(c => c.Id == "player-country");
            Assert.Equal(0.25, reloadedArcadia.TaxPolicy.IncomeTaxRate);
            Assert.Equal(70.0, reloadedArcadia.HappinessRating);

            var reloadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "neighbor-country");
            Assert.Equal(0.20, reloadedValoria.TaxPolicy.IncomeTaxRate);
            Assert.Equal(75.0, reloadedValoria.HappinessRating);

            // Assert: Snapshot tax rate unchanged
            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal(0.25, snapshot.IncomeTaxRate);
            Assert.Equal("25%", snapshot.IncomeTaxRateFormatted);
            Assert.Equal(70.0, snapshot.ApprovalRating);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public void ExecuteDecisionLoop_ExtremeTaxationThreshold_TriggersStabilityPenaltyAndPreservesAcrossReload()
    {
        // Arrange: 25% tax -> 55% tax (>50% extreme threshold triggers -2.0% stability penalty)
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"extreme_tax_loop_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            PresidentialDeskBridge.SaveAuthoritativeState(arcadia, time, 1, tempSaveFile);
            PresidentialDeskBridge.ExportState(arcadia, time, 1, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_income_tax_rate": 0.55,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert: +30% tax hike -> -15% approval, and stability penalty of -2.0%
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0.55, result.IncomeTaxRate);
            Assert.Equal(55.0, result.ApprovalRating);
            Assert.Equal(78.0, result.StabilityRating);

            // Assert: Reload confirms stability penalty persisted
            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            var reloadedCountry = reloaded?.State?.World?.Countries?.First();
            Assert.NotNull(reloadedCountry);
            Assert.Equal(0.55, reloadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(55.0, reloadedCountry.HappinessRating);
            Assert.Equal(78.0, reloadedCountry.BaselineStability);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ExecuteDecisionLoop_EasingFromExtremeTaxation_RestoresStabilityBonusAcrossReload()
    {
        // Arrange: 55% tax, 78% stability -> 45% tax (easing to <= 50% restores +2.0% stability)
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"easing_tax_loop_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.55;
            arcadia.HappinessRating = 55.0;
            arcadia.BaselineStability = 78.0;

            PresidentialDeskBridge.SaveAuthoritativeState(arcadia, time, 1, tempSaveFile);
            PresidentialDeskBridge.ExportState(arcadia, time, 1, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "proposed_income_tax_rate": 0.45,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert: -10% tax relief -> +5% approval (to 60%), +2.0% stability bonus (to 80%)
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0.45, result.IncomeTaxRate);
            Assert.Equal(60.0, result.ApprovalRating);
            Assert.Equal(80.0, result.StabilityRating);

            // Assert: Reload confirms stability bonus persisted
            var reloaded = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            var reloadedCountry = reloaded?.State?.World?.Countries?.First();
            Assert.NotNull(reloadedCountry);
            Assert.Equal(0.45, reloadedCountry.TaxPolicy.IncomeTaxRate);
            Assert.Equal(60.0, reloadedCountry.HappinessRating);
            Assert.Equal(80.0, reloadedCountry.BaselineStability);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_PropagatesRequestId_ToActionResultAndResultFile()
    {
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"req_id_test_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            PresidentialDeskBridge.SaveAuthoritativeState(arcadia, time, 1, tempSaveFile);
            PresidentialDeskBridge.ExportState(arcadia, time, 1, tempStateFile, tempSaveFile);

            var expectedRequestId = $"req_custom_{Guid.NewGuid():N}";
            var requestJson = $$"""
            {
                "request_id": "{{expectedRequestId}}",
                "proposed_income_tax_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRequestId, result.RequestId);

            // Assert result file on disk contains matching request_id
            Assert.True(File.Exists(tempResultFile));
            var resultJson = File.ReadAllText(tempResultFile);
            Assert.Contains(expectedRequestId, resultJson);

            using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
            Assert.True(doc.RootElement.TryGetProperty("request_id", out var reqIdProp));
            Assert.Equal(expectedRequestId, reqIdProp.GetString());
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_ExecutesOnlyOnce_DeletesRequestFile()
    {
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"single_exec_test_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            PresidentialDeskBridge.SaveAuthoritativeState(arcadia, time, 1, tempSaveFile);
            PresidentialDeskBridge.ExportState(arcadia, time, 1, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "request_id": "req_single_exec",
                "proposed_income_tax_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act 1: First call processes the request
            var firstResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            Assert.NotNull(firstResult);
            Assert.True(firstResult.Success);
            Assert.Equal("req_single_exec", firstResult.RequestId);
            Assert.False(File.Exists(tempRequestFile), "Request file must be deleted after processing.");

            // Act 2: Second call must return null without re-executing
            var secondResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            Assert.Null(secondResult);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_MultiCountryWorld_ConfirmedDecision_PreservesOtherCountries()
    {
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"multi_preserve_test_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var arcadia = Country.Found("Republic of Arcadia", time, "player-country");
            arcadia.TaxPolicy.IncomeTaxRate = 0.25;
            arcadia.HappinessRating = 70.0;
            arcadia.BaselineStability = 80.0;

            var valoria = Country.Found("Commonwealth of Valoria", time, "neighbor-country");
            valoria.TaxPolicy.IncomeTaxRate = 0.20;
            valoria.HappinessRating = 75.0;
            valoria.BaselineStability = 85.0;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Greater Republic World",
                CreatedAt = time.UtcTimestamp,
                CurrentTick = 10,
                Countries = new List<Country> { arcadia, valoria }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 10, tempSaveFile);
            PresidentialDeskBridge.ExportState(world, arcadia, time, 10, tempStateFile, tempSaveFile);

            var requestJson = """
            {
                "request_id": "req_multi_world",
                "proposed_income_tax_rate": 0.35,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Act
            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: tempRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0.35, result.IncomeTaxRate);

            // Reload authoritative world save
            var reloadedEnvelope = PresidentialDeskBridge.LoadAuthoritativeState(tempSaveFile);
            Assert.NotNull(reloadedEnvelope?.State?.World?.Countries);

            // Arcadia updated
            var reloadedArcadia = reloadedEnvelope.State.World.Countries.First(c => c.Id == "player-country");
            Assert.Equal(0.35, reloadedArcadia.TaxPolicy.IncomeTaxRate);
            Assert.Equal(65.0, reloadedArcadia.HappinessRating); // -10% tax hike -> -5% approval

            // Valoria completely preserved
            var reloadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "neighbor-country");
            Assert.Equal(0.20, reloadedValoria.TaxPolicy.IncomeTaxRate);
            Assert.Equal(75.0, reloadedValoria.HappinessRating);
            Assert.Equal(85.0, reloadedValoria.BaselineStability);
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public async Task CreateRequestWatcher_DetectsPendingRequest_AppliesDecision_UpdatesStateAndDeletesRequest()
    {
        var tempStateFile = Path.Combine(Path.GetTempPath(), $"watcher_test_{Guid.NewGuid():N}.json");
        var tempSaveFile = Path.ChangeExtension(tempStateFile, ".sav");
        var tempRequestFile = PresidentialDeskBridge.ResolveDefaultRequestFilePath(tempStateFile);
        var tempResultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Arcadia", time, "player-country");
            country.TaxPolicy.IncomeTaxRate = 0.25;
            country.HappinessRating = 70.0;
            country.BaselineStability = 80.0;

            var world = new WorldState
            {
                WorldId = Guid.NewGuid(),
                Name = "Arcadia World",
                CreatedAt = time.UtcTimestamp,
                CurrentTick = 1,
                Countries = new List<Country> { country }
            };

            PresidentialDeskBridge.SaveAuthoritativeState(world, time, 1, tempSaveFile);
            PresidentialDeskBridge.ExportState(world, country, time, 1, tempStateFile, tempSaveFile);

            var processedTcs = new TaskCompletionSource<PresidentialActionResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var watcher = PresidentialDeskBridge.CreateRequestWatcher(
                tempStateFile,
                tempSaveFile,
                result => processedTcs.TrySetResult(result));

            // Write request proposing 30% income tax
            var requestJson = """
            {
                "request_id": "req_watcher_test_01",
                "proposed_income_tax_rate": 0.30,
                "confirmed": true
            }
            """;
            File.WriteAllText(tempRequestFile, requestJson);

            // Wait for watcher to detect and process the request
            var completedTask = await Task.WhenAny(processedTcs.Task, Task.Delay(4000));
            Assert.Same(processedTcs.Task, completedTask);

            var actionResult = await processedTcs.Task;
            Assert.NotNull(actionResult);
            Assert.True(actionResult.Success);
            Assert.Equal(0.30, actionResult.IncomeTaxRate);
            Assert.Equal(0.25, actionResult.PreviousIncomeTaxRate);
            Assert.Equal(67.5, actionResult.ApprovalRating); // -5% tax hike -> -2.5% approval

            // Verify state file was updated
            var reloadedSnapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(reloadedSnapshot);
            Assert.Equal(0.30, reloadedSnapshot.IncomeTaxRate);
            Assert.Equal(67.5, reloadedSnapshot.ApprovalRating);

            // Verify result file exists on disk
            Assert.True(File.Exists(tempResultFile));

            // Verify request file was deleted
            Assert.False(File.Exists(tempRequestFile));
        }
        finally
        {
            if (File.Exists(tempStateFile)) File.Delete(tempStateFile);
            if (File.Exists(tempSaveFile)) File.Delete(tempSaveFile);
            if (File.Exists(tempRequestFile)) File.Delete(tempRequestFile);
            if (File.Exists(tempResultFile)) File.Delete(tempResultFile);
        }
    }

    [Fact]
    public void SinglePlayerLoop_CreateCountry_ApplyTaxDecision_ReloadWithChangeIntact()
    {
        var testId = Guid.NewGuid().ToString("N");
        var tempDir = Path.Combine(Path.GetTempPath(), $"Republic_Phase2_Test_{testId}");
        Directory.CreateDirectory(tempDir);

        var tempStateFile = Path.Combine(tempDir, "presidential_desk_state.json");
        var tempSaveFile = Path.Combine(tempDir, "republic_game.sav");

        try
        {
            // 1. Invalid country name without 'Republic' must be rejected
            var invalidCreation = PresidentialDeskBridge.CreateOrLoadCountry(
                "Kingdom of Arcadia",
                tempStateFile,
                tempSaveFile);
            Assert.False(invalidCreation.Success);
            Assert.Equal("InvalidName", invalidCreation.Result);
            Assert.Contains("must contain the whole word 'Republic'", invalidCreation.Message);

            // 2. Create valid sovereign country
            var countryCreation = PresidentialDeskBridge.CreateOrLoadCountry(
                "Republic of Arcadia",
                tempStateFile,
                tempSaveFile);
            Assert.True(countryCreation.Success);
            Assert.Equal("Success", countryCreation.Result);
            Assert.Equal("Republic of Arcadia", countryCreation.CountryName);
            Assert.Equal(0.25, countryCreation.IncomeTaxRate);
            Assert.Equal("None", countryCreation.ActiveProject);
            Assert.True(File.Exists(tempStateFile));
            Assert.True(File.Exists(tempSaveFile));

            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(initialSnapshot);
            Assert.Equal("Republic of Arcadia", initialSnapshot.CountryName);
            Assert.Equal(0.25, initialSnapshot.IncomeTaxRate);
            Assert.Equal("None", initialSnapshot.ActiveProject);
            var initialTreasury = initialSnapshot.TreasuryBalance;
            Assert.True(initialTreasury > 0);

            // 3. Apply a tax decision (hike to 32%)
            var taxResult = PresidentialDeskBridge.ApplyTaxDecision(
                0.32,
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);
            Assert.True(taxResult.Success);
            Assert.Equal("Success", taxResult.Result);
            Assert.Equal(0.32, taxResult.IncomeTaxRate);
            Assert.Equal("Republic of Arcadia", taxResult.CountryName);

            // 4. Apply a project decision (National Road Program, costing R500,000)
            var projectResult = PresidentialDeskBridge.ApplyProjectDecision(
                "Road",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);
            Assert.True(projectResult.Success);
            Assert.Equal("Success", projectResult.Result);
            Assert.Equal("National Road Program", projectResult.ActiveProject);
            Assert.Equal(initialTreasury - 500_000.0, projectResult.TreasuryBalance);

            // 5. Verify live state file reloads with changes intact
            var liveSnapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(liveSnapshot);
            Assert.Equal("Republic of Arcadia", liveSnapshot.CountryName);
            Assert.Equal(0.32, liveSnapshot.IncomeTaxRate);
            Assert.Contains("National Road Program", liveSnapshot.ActiveProject);
            Assert.Equal(initialTreasury - 500_000.0, liveSnapshot.TreasuryBalance);

            // 6. Resume from authoritative .sav file and verify persistence
            var resumeResult = PresidentialDeskBridge.ResumeFromSave(
                tempSaveFile,
                tempStateFile);
            Assert.True(resumeResult.Success);
            Assert.Equal("Success", resumeResult.Result);
            Assert.Equal("Republic of Arcadia", resumeResult.CountryName);
            Assert.Equal(0.32, resumeResult.IncomeTaxRate);
            Assert.Equal("National Road Program", resumeResult.ActiveProject);
            Assert.Equal(initialTreasury - 500_000.0, resumeResult.TreasuryBalance);

            var reloadedSaveSnapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(reloadedSaveSnapshot);
            Assert.Equal("Republic of Arcadia", reloadedSaveSnapshot.CountryName);
            Assert.Equal(0.32, reloadedSaveSnapshot.IncomeTaxRate);
            Assert.Contains("National Road Program", reloadedSaveSnapshot.ActiveProject);
            Assert.Equal(initialTreasury - 500_000.0, reloadedSaveSnapshot.TreasuryBalance);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyTownHallDecision_ValidatesCostAndBoostsApprovalAndStability()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"townhall_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            country.Treasury.Deposit(200_000.0);
            var initialTreasury = country.Treasury.Balance;
            var initialApproval = country.HappinessRating;
            var initialStability = country.BaselineStability;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            // Execute town hall
            var result = PresidentialDeskBridge.ApplyTownHallDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal(initialTreasury - 50_000.0, result.TreasuryBalance);
            Assert.Equal(initialApproval + 1.0, result.ApprovalRating);
            Assert.Equal(initialStability + 1.5, result.StabilityRating);
            Assert.Contains("Town Hall", result.Message);

            // Verify live snapshot
            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal(initialTreasury - 50_000.0, snapshot.TreasuryBalance);
            Assert.Equal(initialApproval + 1.0, snapshot.ApprovalRating);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyTownHallDecision_WhenInsufficientFunds_RejectsCleanly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"townhall_nofunds_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            // Clear treasury balance
            country.Treasury.Withdraw(country.Treasury.Balance);
            country.Treasury.Deposit(10_000.0); // only R10k, requires R50k
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var result = PresidentialDeskBridge.ApplyTownHallDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.False(result.Success);
            Assert.Equal("InsufficientFunds", result.Result);
            Assert.Contains("Insufficient", result.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCabinetAppointmentDecision_InteriorTechnocrat_ElevatesCorruptionControl()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"interior_tech_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            country.FoundParty("Democratic Union", time, support: 60.0);
            country.FoundParty("Civic Front", time, support: 40.0);
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var result = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Interior",
                "technocrat",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal(0.90, result.CorruptionControl);
            Assert.Contains("Dr. Alistair Finch", result.AppointedMinister);
            Assert.Contains("90%", result.Message);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal(0.90, snapshot.CorruptionControl);
            Assert.Equal("Dr. Alistair Finch", snapshot.InteriorMinister);
            Assert.Equal(57.0, snapshot.PollingSupport); // -3% polling penalty for technocrat
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCabinetAppointmentDecision_InteriorLoyalist_SetsCorruptionControl45()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"interior_loyal_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            country.FoundParty("Democratic Union", time, support: 50.0);
            country.FoundParty("Civic Front", time, support: 50.0);
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var result = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Interior",
                "loyalist",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal(0.45, result.CorruptionControl);
            Assert.Contains("Thomas Sterling", result.AppointedMinister);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal(0.45, snapshot.CorruptionControl);
            Assert.Equal("Party Whip Thomas Sterling", snapshot.InteriorMinister);
            Assert.Equal(53.0, snapshot.PollingSupport); // +3% polling rally for loyalist
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyProjectDecision_NationalUniversity_DeductsCostAndSetsActive()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"univ_proj_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            country.Treasury.Deposit(1_500_000.0);
            var initialTreasury = country.Treasury.Balance;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var result = PresidentialDeskBridge.ApplyProjectDecision(
                "University",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal("National University", result.ActiveProject);
            Assert.Equal(initialTreasury - 750_000.0, result.TreasuryBalance);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Contains("National University", snapshot.ActiveProject);
            Assert.Equal(initialTreasury - 750_000.0, snapshot.TreasuryBalance);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyElectionDecision_DemocraticTransition_VacatesCabinetPortfolios()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"election_trans_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            // Set up ruling party with low support and opposition with overwhelming support
            country.FoundParty("Incumbent Party", time, support: 30.0);
            country.FoundParty("Challenger Movement", time);
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 70.0 };
            }
            country.HappinessRating = 25.0; // unpopular government

            // Appoint Finance minister
            country.AppointMinister(new Minister
            {
                Name = "Old Finance Minister",
                Portfolio = CabinetPortfolio.Finance,
                Competence = 0.90,
                Integrity = 0.90
            }, CabinetPortfolio.Finance);
            Assert.Equal(0.90, country.StateCapacity?.CivilServiceQuality);

            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            var result = PresidentialDeskBridge.ApplyElectionDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Contains("Democratic Transition", result.Message);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal("Challenger Movement", snapshot.RulingParty);
            Assert.Equal("Incumbent Party", snapshot.OppositionParty);
            Assert.Equal("Vacant", snapshot.FinanceMinister);
            Assert.Equal(0.30, snapshot.CivilServiceQuality);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCrisisResponseDecision_CivicReliefAndAusterity_ExecutesCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"crisis_resp_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Eldoria", time, "eldoria");
            country.Treasury.Deposit(300_000.0);
            country.HappinessRating = 40.0;
            country.BaselineStability = 50.0;
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            // Test Civic Relief
            var reliefResult = PresidentialDeskBridge.ApplyCrisisResponseDecision(
                "civic_relief",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(reliefResult.Success);
            Assert.Equal("Success", reliefResult.Result);
            Assert.Contains("Civic Relief Package", reliefResult.Message);
            Assert.Equal(44.0, reliefResult.ApprovalRating);
            Assert.Equal(53.0, reliefResult.StabilityRating);

            // Test Austerity
            var austerityResult = PresidentialDeskBridge.ApplyCrisisResponseDecision(
                "austerity",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(austerityResult.Success);
            Assert.Equal("Success", austerityResult.Result);
            Assert.Contains("Emergency Austerity", austerityResult.Message);
            Assert.Equal(40.0, austerityResult.ApprovalRating);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void CreateSnapshot_ExportsStructuredCabinetWithAuthoritativeValues()
    {
        // Arrange
        var time = CreateTime(5);
        var country = Country.Found("Republic of Valoria", time, "valoria");

        var financeMinister = new Minister
        {
            Name = "Dr. Elena Rostova",
            Portfolio = CabinetPortfolio.Finance,
            Competence = 0.92,
            Integrity = 0.88,
            Loyalty = 0.75,
            Experience = 0.80,
            PoliticalConnections = 0.40,
            MonthlySalary = 180_000.0,
            FactionAlignmentId = "technocrat_caucus"
        };
        country.AppointMinister(financeMinister, CabinetPortfolio.Finance);

        var interiorMinister = new Minister
        {
            Name = "Dr. Alistair Finch",
            Portfolio = CabinetPortfolio.Interior,
            Competence = 0.85,
            Integrity = 0.85,
            Loyalty = 0.60,
            Experience = 0.70,
            PoliticalConnections = 0.50,
            MonthlySalary = 175_000.0,
            FactionAlignmentId = "civil_service"
        };
        country.AppointMinister(interiorMinister, CabinetPortfolio.Interior);

        var infraMinister = new Minister
        {
            Name = "Eng. Marcus Bennett",
            Portfolio = CabinetPortfolio.Infrastructure,
            Competence = 0.90,
            Integrity = 0.80,
            Loyalty = 0.70,
            Experience = 0.95,
            PoliticalConnections = 0.35,
            MonthlySalary = 160_000.0,
            FactionAlignmentId = "engineering_corps"
        };
        country.AppointMinister(infraMinister, CabinetPortfolio.Infrastructure);

        // Act
        var snapshot = PresidentialDeskSnapshot.Create(country, time, 10);

        // Assert - Top-level properties preserved and expanded
        Assert.Equal("Dr. Elena Rostova", snapshot.FinanceMinister);
        Assert.Equal("Dr. Alistair Finch", snapshot.InteriorMinister);
        Assert.Equal("Eng. Marcus Bennett", snapshot.InfrastructureMinister);
        Assert.Equal("Vacant", snapshot.DefenseMinister);
        Assert.Equal(0.88, snapshot.CivilServiceQuality);
        Assert.Equal(0.85, snapshot.CorruptionControl);
        Assert.Equal(0.90, snapshot.InfrastructureCondition);
        Assert.Contains("Dr. Elena Rostova", snapshot.CabinetSummary);

        // Assert - Structured Cabinet Collection contains all 6 constitutional portfolios
        Assert.NotNull(snapshot.Cabinet);
        Assert.Equal(6, snapshot.Cabinet.Count);

        // Finance (Appointed)
        var finSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "Finance");
        Assert.NotNull(finSnap);
        Assert.True(finSnap.IsAppointed);
        Assert.Equal("Appointed", finSnap.Status);
        Assert.Equal("Dr. Elena Rostova", finSnap.MinisterName);
        Assert.Equal(0.92, finSnap.Competence);
        Assert.Equal(0.75, finSnap.Loyalty);
        Assert.Equal(0.88, finSnap.Integrity);
        Assert.Equal(0.80, finSnap.Experience);
        Assert.Equal(0.40, finSnap.PoliticalConnections);
        Assert.Equal(180_000.0, finSnap.MonthlySalary);
        Assert.Equal("technocrat_caucus", finSnap.FactionAlignmentId);
        Assert.Equal("Civil Service Quality", finSnap.StateCapacityMetric);
        Assert.Equal(0.88, finSnap.StateCapacityValue);

        // Interior (Appointed)
        var intSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "Interior");
        Assert.NotNull(intSnap);
        Assert.True(intSnap.IsAppointed);
        Assert.Equal("Dr. Alistair Finch", intSnap.MinisterName);
        Assert.Equal("Corruption Control", intSnap.StateCapacityMetric);
        Assert.Equal(0.85, intSnap.StateCapacityValue);

        // Infrastructure (Appointed)
        var infSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "Infrastructure");
        Assert.NotNull(infSnap);
        Assert.True(infSnap.IsAppointed);
        Assert.Equal("Eng. Marcus Bennett", infSnap.MinisterName);
        Assert.Equal("Infrastructure Condition", infSnap.StateCapacityMetric);
        Assert.Equal(0.90, infSnap.StateCapacityValue);

        // Defense (Vacant)
        var defSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "Defense");
        Assert.NotNull(defSnap);
        Assert.False(defSnap.IsAppointed);
        Assert.Equal("Vacant", defSnap.Status);
        Assert.Equal("Vacant", defSnap.MinisterName);
        Assert.Equal(0.0, defSnap.Competence);
        Assert.Equal(0.0, defSnap.Loyalty);
        Assert.Equal(0.0, defSnap.MonthlySalary);
        Assert.Null(defSnap.StateCapacityMetric);
        Assert.Null(defSnap.StateCapacityValue);

        // ForeignAffairs (Vacant)
        var faSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "ForeignAffairs");
        Assert.NotNull(faSnap);
        Assert.False(faSnap.IsAppointed);
        Assert.Equal("Vacant", faSnap.Status);

        // Health (Vacant)
        var hlthSnap = snapshot.Cabinet.FirstOrDefault(c => c.Portfolio == "Health");
        Assert.NotNull(hlthSnap);
        Assert.False(hlthSnap.IsAppointed);
        Assert.Equal("Vacant", hlthSnap.Status);
    }

    [Fact]
    public void SerializeAndDeserialize_ExpandedCabinetDataPreservedInJsonRoundTrip()
    {
        // Arrange
        var time = CreateTime(3);
        var country = Country.Found("Republic of Valoria", time, "valoria");
        country.AppointMinister(new Minister
        {
            Name = "Dr. Elena Rostova",
            Portfolio = CabinetPortfolio.Finance,
            Competence = 0.90,
            Integrity = 0.90,
            Loyalty = 0.50,
            Experience = 0.85,
            PoliticalConnections = 0.40,
            MonthlySalary = 150_000.0
        }, CabinetPortfolio.Finance);

        var original = PresidentialDeskSnapshot.Create(country, time, 50);

        // Act
        var json = PresidentialDeskBridge.Serialize(original);
        var deserialized = PresidentialDeskBridge.Deserialize(json);

        // Assert - JSON string properties presence
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("cabinet", out var cabinetProp));
        Assert.Equal(System.Text.Json.JsonValueKind.Array, cabinetProp.ValueKind);
        Assert.Equal(6, cabinetProp.GetArrayLength());

        Assert.True(root.TryGetProperty("infrastructure_condition", out var infraCondProp));
        Assert.Equal(0.3, infraCondProp.GetDouble());

        Assert.True(root.TryGetProperty("infrastructure_minister", out var infraMinProp));
        Assert.Equal("Vacant", infraMinProp.GetString());

        Assert.True(root.TryGetProperty("defense_minister", out var defMinProp));
        Assert.Equal("Vacant", defMinProp.GetString());

        // Existing properties preserved
        Assert.True(root.TryGetProperty("cabinet_summary", out _));
        Assert.True(root.TryGetProperty("finance_minister", out _));
        Assert.True(root.TryGetProperty("interior_minister", out _));
        Assert.True(root.TryGetProperty("civil_service_quality", out _));
        Assert.True(root.TryGetProperty("corruption_control", out _));

        // Deserialized round-trip fidelity
        Assert.NotNull(deserialized);
        Assert.Equal(original.FinanceMinister, deserialized.FinanceMinister);
        Assert.Equal(original.InfrastructureMinister, deserialized.InfrastructureMinister);
        Assert.Equal(original.DefenseMinister, deserialized.DefenseMinister);
        Assert.Equal(original.InfrastructureCondition, deserialized.InfrastructureCondition);
        Assert.NotNull(deserialized.Cabinet);
        Assert.Equal(6, deserialized.Cabinet.Count);

        var deserializedFinance = deserialized.Cabinet.First(c => c.Portfolio == "Finance");
        Assert.True(deserializedFinance.IsAppointed);
        Assert.Equal("Dr. Elena Rostova", deserializedFinance.MinisterName);
        Assert.Equal(0.90, deserializedFinance.Competence);
        Assert.Equal(0.50, deserializedFinance.Loyalty);
        Assert.Equal(150_000.0, deserializedFinance.MonthlySalary);
        Assert.Equal("Civil Service Quality", deserializedFinance.StateCapacityMetric);
        Assert.Equal(0.90, deserializedFinance.StateCapacityValue);

        var deserializedDefense = deserialized.Cabinet.First(c => c.Portfolio == "Defense");
        Assert.False(deserializedDefense.IsAppointed);
        Assert.Equal("Vacant", deserializedDefense.MinisterName);
    }

    [Fact]
    public void ApplyCabinetAppointmentDecision_AppointsAndUpdatesStructuredCabinetSnapshot()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"cab_snap_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var tempSaveFile = Path.Combine(tempDir, "state.sav");

        try
        {
            var time = CreateTime(1);
            var country = Country.Found("Republic of Valoria", time, "valoria");
            country.FoundParty("Democratic Union", time, support: 55.0);
            PresidentialDeskBridge.ExportState(country, time, 1, tempStateFile, tempSaveFile);

            // Act 1: Appoint Technocrat to Finance
            var appointResult = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Finance",
                "technocrat",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(appointResult.Success);
            var snapshot1 = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot1);
            Assert.NotNull(snapshot1.Cabinet);

            var financeSlot = snapshot1.Cabinet.FirstOrDefault(c => c.Portfolio == "Finance");
            Assert.NotNull(financeSlot);
            Assert.True(financeSlot.IsAppointed);
            Assert.Equal("Dr. Elena Rostova", financeSlot.MinisterName);
            Assert.Equal(0.90, financeSlot.Competence);
            Assert.Equal(0.90, financeSlot.Integrity);
            Assert.Equal(0.50, financeSlot.Loyalty);
            Assert.Equal(0.90, snapshot1.CivilServiceQuality);

            // Act 2: Dismiss Finance Minister
            var dismissResult = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Finance",
                "dismiss",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(dismissResult.Success);
            var snapshot2 = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot2);

            var financeSlotAfterDismiss = snapshot2.Cabinet.FirstOrDefault(c => c.Portfolio == "Finance");
            Assert.NotNull(financeSlotAfterDismiss);
            Assert.False(financeSlotAfterDismiss.IsAppointed);
            Assert.Equal("Vacant", financeSlotAfterDismiss.Status);
            Assert.Equal("Vacant", financeSlotAfterDismiss.MinisterName);
            Assert.Equal(0.30, snapshot2.CivilServiceQuality);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }
}



