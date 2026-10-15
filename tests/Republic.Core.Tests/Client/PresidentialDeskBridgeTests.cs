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
}
