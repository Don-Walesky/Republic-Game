namespace Republic.Core.Persistence;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Service calculating and validating SHA256 checksum signatures for game save files to prevent tampered or corrupted saves.
/// </summary>
public static class SaveChecksumValidator
{
    public static string CalculateChecksum(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyChecksum(string content, string expectedChecksum)
    {
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(expectedChecksum))
        {
            return false;
        }

        string actual = CalculateChecksum(content);
        return string.Equals(actual, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }
}
