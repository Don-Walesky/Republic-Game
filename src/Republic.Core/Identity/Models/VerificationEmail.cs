namespace Republic.Core.Identity.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing an authoritative email verification dispatch.
/// Each verification dispatch is uniquely tied to the player's registered email address.
/// </summary>
public sealed record VerificationEmail
{
    [JsonPropertyName("email_id")]
    public string EmailId { get; init; } = $"vemail_{Guid.NewGuid():N}";

    [JsonPropertyName("recipient_email")]
    public string RecipientEmail { get; init; } = string.Empty;

    [JsonPropertyName("recipient_name")]
    public string RecipientName { get; init; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; init; } = "Republic Sovereign Electoral & National Registration Commission <registry@republic.net>";

    [JsonPropertyName("subject")]
    public string Subject { get; init; } = "Official Verification: Confirm Your Republic Sovereign Account";

    [JsonPropertyName("verification_code")]
    public string VerificationCode { get; init; } = string.Empty;

    [JsonPropertyName("button_text")]
    public string ButtonText { get; init; } = "Verify Email";

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("sent_at_utc")]
    public DateTimeOffset SentAtUtc { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("expires_at_utc")]
    public DateTimeOffset ExpiresAtUtc { get; init; } = DateTimeOffset.UtcNow.AddHours(24);

    [JsonPropertyName("is_used")]
    public bool IsUsed { get; init; }

    /// <summary>
    /// Checks if this verification email dispatch is currently valid and unexpired.
    /// </summary>
    public bool IsValidAt(DateTimeOffset referenceTime)
    {
        return !IsUsed && referenceTime <= ExpiresAtUtc;
    }
}

