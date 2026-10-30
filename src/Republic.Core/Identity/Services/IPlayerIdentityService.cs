namespace Republic.Core.Identity.Services;

using System;
using Republic.Core.Identity.Models;

/// <summary>
/// Service interface for player registration, age eligibility verification, email verification,
/// and personal identity isolation.
/// </summary>
public interface IPlayerIdentityService
{
    /// <summary>
    /// Validates prospective registration fields without mutating state.
    /// Checks name validity, RFC 5322 email syntax, and minimum age eligibility (13+).
    /// </summary>
    (bool IsValid, string Message) ValidateRegistration(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        DateOnly? referenceDate = null);

    /// <summary>
    /// Registers a new player account with unverified email status and advances the onboarding state to AccountRegistered.
    /// Generates the initial verification token and drafts the official verification email dispatch.
    /// </summary>
    PlayerAccount RegisterAccount(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        DateOnly? referenceDate = null,
        string? customAccountId = null);

    /// <summary>
    /// Verifies the player's email address using a verification token.
    /// Rejects expired or already used tokens.
    /// On success, advances the onboarding state to EmailVerified.
    /// </summary>
    (bool Success, string Message) VerifyEmail(
        PlayerAccount account,
        string verificationToken,
        DateTimeOffset? verificationTime = null);

    /// <summary>
    /// Requests and issues a new single-use verification email dispatch with rate-limit protection.
    /// </summary>
    (bool Success, string Message, VerificationEmail? Email) ResendVerificationEmail(
        PlayerAccount account,
        DateTimeOffset? now = null);

    /// <summary>
    /// Drafts the authoritative verification email content with sender, recipient, token, and interactive button text.
    /// </summary>
    VerificationEmail DraftVerificationEmail(
        PlayerAccount account,
        string? token = null,
        DateTimeOffset? now = null);

    /// <summary>
    /// Validates prospective country name length, permitted characters, and sovereign syntax.
    /// </summary>
    (bool IsValid, string Message) ValidateCountryName(string countryName);

    /// <summary>
    /// Generates a secure, human-friendly 6-character alphanumeric verification token.
    /// </summary>
    string GenerateVerificationToken();
}
