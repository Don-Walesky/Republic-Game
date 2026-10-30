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
    /// On success, advances the onboarding state to EmailVerified.
    /// </summary>
    (bool Success, string Message) VerifyEmail(
        PlayerAccount account,
        string verificationToken,
        DateTimeOffset? verificationTime = null);

    /// <summary>
    /// Generates a secure, human-friendly 6-character alphanumeric verification token.
    /// </summary>
    string GenerateVerificationToken();
}

