namespace Republic.Core.Identity.Services;

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Republic.Core.Identity.Models;

/// <summary>
/// Authoritative implementation of player registration, age-eligibility validation,
/// and email verification token management.
/// </summary>
public sealed class PlayerIdentityService : IPlayerIdentityService
{
    public const int MinimumAgeYears = 13;
    public const int MaximumAgeYears = 125;

    // Strict standard RFC-compatible email pattern
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    /// <inheritdoc />
    public (bool IsValid, string Message) ValidateRegistration(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        DateOnly? referenceDate = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return (false, "First name is required.");
        }

        if (firstName.Trim().Length > 50)
        {
            return (false, "First name must not exceed 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return (false, "Last name is required.");
        }

        if (lastName.Trim().Length > 50)
        {
            return (false, "Last name must not exceed 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email address is required.");
        }

        var trimmedEmail = email.Trim();
        if (trimmedEmail.Length > 254 || !EmailRegex.IsMatch(trimmedEmail))
        {
            return (false, "Please provide a valid email address.");
        }

        var today = referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (dateOfBirth == default)
        {
            return (false, "Date of birth is required.");
        }

        if (dateOfBirth > today)
        {
            return (false, "Date of birth cannot be in the future.");
        }

        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        if (age < MinimumAgeYears)
        {
            return (false, $"Player must be at least {MinimumAgeYears} years of age to register (age: {age}).");
        }

        if (age > MaximumAgeYears)
        {
            return (false, "Please provide a valid date of birth.");
        }

        return (true, "Registration details valid.");
    }

    /// <inheritdoc />
    public PlayerAccount RegisterAccount(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        DateOnly? referenceDate = null,
        string? customAccountId = null)
    {
        var (isValid, message) = ValidateRegistration(firstName, lastName, email, dateOfBirth, referenceDate);
        if (!isValid)
        {
            throw new ArgumentException($"Registration validation failed: {message}");
        }

        var token = GenerateVerificationToken();
        var accountId = customAccountId ?? $"acc_{Guid.NewGuid():N}";
        var cleanFirst = firstName.Trim();
        var cleanLast = lastName.Trim();
        var cleanEmail = email.Trim().ToLowerInvariant();

        var identity = new PlayerPersonalIdentity
        {
            FirstName = cleanFirst,
            LastName = cleanLast,
            EmailAddress = cleanEmail,
            DateOfBirth = dateOfBirth,
            IsEmailVerified = false,
            VerificationToken = token,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        return new PlayerAccount
        {
            AccountId = accountId,
            DisplayName = $"{cleanFirst} {cleanLast}",
            CurrentStep = OnboardingStep.AccountRegistered,
            PersonalIdentity = identity,
            LastLoginUtc = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public (bool Success, string Message) VerifyEmail(
        PlayerAccount account,
        string verificationToken,
        DateTimeOffset? verificationTime = null)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.PersonalIdentity.IsEmailVerified)
        {
            return (true, "Email is already verified.");
        }

        if (string.IsNullOrWhiteSpace(verificationToken))
        {
            return (false, "Verification token is required.");
        }

        var expected = account.PersonalIdentity.VerificationToken?.Trim();
        var actual = verificationToken.Trim();

        if (string.IsNullOrEmpty(expected) || !string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Invalid verification token. Please verify the code and try again.");
        }

        account.PersonalIdentity.IsEmailVerified = true;
        account.PersonalIdentity.VerifiedAtUtc = verificationTime ?? DateTimeOffset.UtcNow;

        if (account.CurrentStep == OnboardingStep.AccountRegistered)
        {
            account.CurrentStep = OnboardingStep.EmailVerified;
        }

        return (true, "Email address verified successfully. Country founding is now unlocked.");
    }

    /// <inheritdoc />
    public string GenerateVerificationToken()
    {
        // 6-digit numeric verification code (human-readable, easy to input)
        var code = RandomNumberGenerator.GetInt32(100000, 1000000);
        return code.ToString("D6");
    }
}

