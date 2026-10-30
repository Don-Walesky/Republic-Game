namespace Republic.Core.Identity.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating sensitive personal identity data.
/// Kept strictly isolated from public diplomatic snapshots, peer multiplayer broadcasts, and sovereign world profiles.
/// </summary>
public sealed class PlayerPersonalIdentity
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; } = string.Empty;

    [JsonPropertyName("date_of_birth")]
    public string DateOfBirthString { get; set; } = string.Empty;

    [JsonIgnore]
    public DateOnly DateOfBirth
    {
        get => DateOnly.TryParse(DateOfBirthString, out var dob) ? dob : default;
        set => DateOfBirthString = value.ToString("O");
    }

    [JsonPropertyName("is_email_verified")]
    public bool IsEmailVerified { get; set; }

    [JsonPropertyName("verification_token")]
    public string? VerificationToken { get; set; }

    [JsonPropertyName("verification_token_expires_at_utc")]
    public DateTimeOffset? VerificationTokenExpiresAtUtc { get; set; }

    [JsonPropertyName("is_token_used")]
    public bool IsTokenUsed { get; set; }

    [JsonPropertyName("last_verification_request_utc")]
    public DateTimeOffset? LastVerificationRequestUtc { get; set; }

    [JsonPropertyName("pending_verification_email")]
    public VerificationEmail? PendingVerificationEmail { get; set; }

    [JsonPropertyName("created_at_utc")]
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("verified_at_utc")]
    public DateTimeOffset? VerifiedAtUtc { get; set; }
}
