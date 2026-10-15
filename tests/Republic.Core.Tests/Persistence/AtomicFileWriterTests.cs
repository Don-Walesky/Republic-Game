namespace Republic.Core.Tests.Persistence;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class AtomicFileWriterTests : IDisposable
{
    private readonly string _testDirectory;

    public AtomicFileWriterTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"republic_atomic_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void WriteAllTextSafely_CreatesNewFile_WithExactContent_AndNoTempFilesLeftBehind()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "test_file.sav");
        const string expectedContent = "{\"formatVersion\":1,\"state\":{\"world\":{\"name\":\"Test World\"}}}";

        // Act
        AtomicFileWriter.WriteAllTextSafely(targetFile, expectedContent);

        // Assert
        Assert.True(File.Exists(targetFile));
        Assert.Equal(expectedContent, File.ReadAllText(targetFile));

        var tmpFiles = Directory.GetFiles(_testDirectory, "*.tmp");
        Assert.Empty(tmpFiles);
    }

    [Fact]
    public void WriteAllTextSafely_SafelyReplacesExistingFile_WithNewContent()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "state_replace.sav");
        const string initialContent = "initial_valid_state_v1";
        const string updatedContent = "updated_valid_state_v2";

        AtomicFileWriter.WriteAllTextSafely(targetFile, initialContent);
        Assert.Equal(initialContent, File.ReadAllText(targetFile));

        // Act: Replace existing file safely
        AtomicFileWriter.WriteAllTextSafely(targetFile, updatedContent);

        // Assert
        Assert.Equal(updatedContent, File.ReadAllText(targetFile));
        var tmpFiles = Directory.GetFiles(_testDirectory, "*.tmp");
        Assert.Empty(tmpFiles);
    }

    [Fact]
    public void WriteAllTextSafely_WhenDestinationIsReadOnly_ThrowsAndPreservesOriginalSaveFile()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "readonly_save.sav");
        const string originalContent = "original_unmodified_save_data";
        AtomicFileWriter.WriteAllTextSafely(targetFile, originalContent);

        // Mark destination as read-only to simulate replace failure
        File.SetAttributes(targetFile, FileAttributes.ReadOnly);

        try
        {
            // Act & Assert: Attempting to replace a read-only file throws UnauthorizedAccessException
            Assert.Throws<UnauthorizedAccessException>(() =>
                AtomicFileWriter.WriteAllTextSafely(targetFile, "attempted_corrupt_data"));

            // Verify original file is still present and was NOT modified
            Assert.True(File.Exists(targetFile));
            Assert.Equal(originalContent, File.ReadAllText(targetFile));

            // Verify temporary files were cleaned up
            var tmpFiles = Directory.GetFiles(_testDirectory, "*.tmp");
            Assert.Empty(tmpFiles);
        }
        finally
        {
            File.SetAttributes(targetFile, FileAttributes.Normal);
        }
    }

    [Fact]
    public async Task WriteAllTextSafelyAsync_WritesAndReplacesFile_Correctly()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "async_save.sav");
        const string initialContent = "async_initial_payload";
        const string updatedContent = "async_updated_payload";

        // Act
        await AtomicFileWriter.WriteAllTextSafelyAsync(targetFile, initialContent);
        Assert.Equal(initialContent, await File.ReadAllTextAsync(targetFile));

        await AtomicFileWriter.WriteAllTextSafelyAsync(targetFile, updatedContent);

        // Assert
        Assert.Equal(updatedContent, await File.ReadAllTextAsync(targetFile));
        var tmpFiles = Directory.GetFiles(_testDirectory, "*.tmp");
        Assert.Empty(tmpFiles);
    }

    [Fact]
    public async Task WriteAllTextSafelyAsync_WhenCancelled_PreservesExistingFileAndCleansUpTemp()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "cancel_save.sav");
        const string originalContent = "valid_save_before_cancellation";
        await AtomicFileWriter.WriteAllTextSafelyAsync(targetFile, originalContent);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await AtomicFileWriter.WriteAllTextSafelyAsync(targetFile, "new_cancelled_content", cancellationToken: cts.Token));

        // Original save file must remain completely unchanged
        Assert.True(File.Exists(targetFile));
        Assert.Equal(originalContent, await File.ReadAllTextAsync(targetFile));

        // Temporary file must be cleaned up
        var tmpFiles = Directory.GetFiles(_testDirectory, "*.tmp");
        Assert.Empty(tmpFiles);
    }

    [Fact]
    public void WriteAllTextSafely_PreservesCompleteMultiCountryWorldStateEnvelope()
    {
        // Arrange
        var targetFile = Path.Combine(_testDirectory, "multi_country_world.sav");
        var epoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var simTime = new RepublicTime(epoch.AddDays(5), epoch);

        var arcadia = Country.Found("Republic of Arcadia", simTime, "arcadia-id",
            treasury: new RepuTreasury(10_000_000_000.0, "arcadia-id"));
        arcadia.TaxPolicy.IncomeTaxRate = 0.28;
        arcadia.HappinessRating = 72.0;

        var valoria = Country.Found("Republic of Valoria", simTime, "valoria-id",
            treasury: new RepuTreasury(25_000_000_000.0, "valoria-id"));
        valoria.TaxPolicy.IncomeTaxRate = 0.22;
        valoria.HappinessRating = 80.0;

        var worldId = Guid.NewGuid();
        var world = new WorldState
        {
            WorldId = worldId,
            Name = "Global Federation",
            CreatedAt = simTime.UtcTimestamp,
            CurrentTick = 250,
            Countries = new List<Country> { arcadia, valoria }
        };

        var fullState = new FullGameState
        {
            SaveName = "federation_session",
            CurrentTick = 250,
            SaveTimestamp = simTime.UtcTimestamp,
            World = world
        };

        var envelope = new SaveEnvelope<FullGameState>
        {
            FormatVersion = 1,
            SavedAt = simTime.UtcTimestamp,
            State = fullState
        };

        var serializer = new JsonStateSerializer();
        var payload = serializer.Serialize(envelope);

        // Act: Safely write envelope
        AtomicFileWriter.WriteAllTextSafely(targetFile, payload);

        // Assert: Read back and verify complete world fidelity
        var reloadedPayload = File.ReadAllText(targetFile);
        var reloadedEnvelope = serializer.Deserialize<FullGameState>(reloadedPayload);

        Assert.NotNull(reloadedEnvelope?.State?.World);
        var reloadedWorld = reloadedEnvelope.State.World;
        Assert.Equal(worldId, reloadedWorld.WorldId);
        Assert.Equal("Global Federation", reloadedWorld.Name);
        Assert.Equal(2, reloadedWorld.Countries.Count);

        var reloadedArcadia = reloadedWorld.Countries.First(c => c.Id == "arcadia-id");
        Assert.Equal(0.28, reloadedArcadia.TaxPolicy.IncomeTaxRate);
        Assert.Equal(72.0, reloadedArcadia.HappinessRating);
        Assert.Equal(10_000_000_000.0, reloadedArcadia.Treasury.Balance);

        var reloadedValoria = reloadedEnvelope.State.World.Countries.First(c => c.Id == "valoria-id");
        Assert.Equal(0.22, reloadedValoria.TaxPolicy.IncomeTaxRate);
        Assert.Equal(80.0, reloadedValoria.HappinessRating);
        Assert.Equal(25_000_000_000.0, reloadedValoria.Treasury.Balance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, recursive: true); } catch { }
        }
    }
}
