namespace Republic.Core.Tests.Persistence;

using Xunit;
using Republic.Core.Persistence;

public class SaveChecksumTests
{
    [Fact]
    public void CalculateChecksum_Produces_Consistent_SHA256_Hash()
    {
        string content = "{\"FormatVersion\":1,\"SavedAt\":\"2026-08-12T00:00:00Z\",\"State\":{\"SaveName\":\"test\"}}";
        string hash1 = SaveChecksumValidator.CalculateChecksum(content);
        string hash2 = SaveChecksumValidator.CalculateChecksum(content);

        Assert.NotNull(hash1);
        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // SHA256 hex string length is 64 chars
    }

    [Fact]
    public void VerifyChecksum_Returns_True_For_Matching_Hash()
    {
        string content = "SaveFileValidDataContent";
        string checksum = SaveChecksumValidator.CalculateChecksum(content);

        bool isValid = SaveChecksumValidator.VerifyChecksum(content, checksum);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyChecksum_Returns_False_For_Tampered_Data()
    {
        string originalContent = "OriginalValidState";
        string checksum = SaveChecksumValidator.CalculateChecksum(originalContent);

        string tamperedContent = "OriginalValidState_TamperedWithExtraMoney";
        bool isValid = SaveChecksumValidator.VerifyChecksum(tamperedContent, checksum);

        Assert.False(isValid);
    }
}
