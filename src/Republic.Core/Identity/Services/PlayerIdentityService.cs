namespace Republic.Core.Identity.Services;

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Republic.Core.Identity.Models;
using Republic.Core.World.Rules;

/// <summary>
/// Authoritative implementation of player registration, age-eligibility validation,
/// verification email generation, and email verification token management.
/// </summary>
public sealed class PlayerIdentityService : IPlayerIdentityService
{
    public const int MinimumAgeYears = 13;
    public const int MaximumAgeYears = 125;
    public static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(10);

    // Strict standard RFC-compatible email pattern
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    // Permitted country name characters: letters, spaces, hyphens, apostrophes, commas, periods
    private static readonly Regex PermittedCountryNameRegex = new(
        @"^[a-zA-Z0-9\s\-',.]+$",
        RegexOptions.Compiled,
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
    public (bool IsValid, string Message) ValidateCountryName(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return (false, "Country name is required.");
        }

        var trimmed = countryName.Trim();
        if (trimmed.Length < 4)
        {
            return (false, "Country name must be at least 4 characters.");
        }

        if (trimmed.Length > 60)
        {
            return (false, "Country name must not exceed 60 characters.");
        }

        if (!PermittedCountryNameRegex.IsMatch(trimmed))
        {
            return (false, "Country name contains invalid characters. Use letters, numbers, spaces, hyphens, and apostrophes.");
        }

        if (!CountryNameRule.Default.IsValid(trimmed))
        {
            return (false, "Country name must contain the whole word 'Republic'.");
        }

        return (true, "Country name is valid.");
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

        var now = DateTimeOffset.UtcNow;
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
            VerificationTokenExpiresAtUtc = now.Add(VerificationTokenLifetime),
            IsTokenUsed = false,
            LastVerificationRequestUtc = now,
            CreatedAtUtc = now
        };

        var account = new PlayerAccount
        {
            AccountId = accountId,
            DisplayName = $"{cleanFirst} {cleanLast}",
            CurrentStep = OnboardingStep.AccountRegistered,
            PersonalIdentity = identity,
            LastLoginUtc = now
        };

        var verificationEmail = DraftVerificationEmail(account, token, now);
        identity.PendingVerificationEmail = verificationEmail;

        return account;
    }

    /// <inheritdoc />
    public VerificationEmail DraftVerificationEmail(
        PlayerAccount account,
        string? token = null,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(account);

        var sentAt = now ?? DateTimeOffset.UtcNow;
        var code = token ?? account.PersonalIdentity.VerificationToken ?? GenerateVerificationToken();
        var expiresAt = sentAt.Add(VerificationTokenLifetime);
        var fullName = $"{account.PersonalIdentity.FirstName} {account.PersonalIdentity.LastName}".Trim();

        var body =
            $"Greetings {fullName},\n\n" +
            $"Welcome to Republic. Your executive account has been registered to this email address ({account.PersonalIdentity.EmailAddress}).\n\n" +
            $"To finalize your credentials and proceed to founding your sovereign nation, please verify your email address.\n\n" +
            $"Your 6-digit verification security code is: {code}\n" +
            $"This code is valid for 24 hours and is single-use only.\n\n" +
            $"You may also click the 'Verify Email' button in your console to complete verification immediately.\n\n" +
            $"If you did not initiate this sovereign registration, please disregard this dispatch.\n\n" +
            $"— Republic Sovereign Electoral & National Registration Commission";

        return new VerificationEmail
        {
            RecipientEmail = account.PersonalIdentity.EmailAddress,
            RecipientName = fullName,
            Subject = "Official Verification: Confirm Your Republic Sovereign Account",
            VerificationCode = code,
            ButtonText = "Verify Email",
            Body = body,
            SentAtUtc = sentAt,
            ExpiresAtUtc = expiresAt,
            IsUsed = false
        };
    }

    /// <inheritdoc />
    public (bool Success, string Message, VerificationEmail? Email) ResendVerificationEmail(
        PlayerAccount account,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.PersonalIdentity.IsEmailVerified)
        {
            return (false, "Email address is already verified.", null);
        }

        var currentTime = now ?? DateTimeOffset.UtcNow;
        if (account.PersonalIdentity.LastVerificationRequestUtc.HasValue)
        {
            var elapsed = currentTime - account.PersonalIdentity.LastVerificationRequestUtc.Value;
            if (elapsed < ResendCooldown)
            {
                var remaining = (int)Math.Ceiling((ResendCooldown - elapsed).TotalSeconds);
                return (false, $"Please wait {remaining} seconds before requesting another verification email.", null);
            }
        }

        var token = GenerateVerificationToken();
        account.PersonalIdentity.VerificationToken = token;
        account.PersonalIdentity.VerificationTokenExpiresAtUtc = currentTime.Add(VerificationTokenLifetime);
        account.PersonalIdentity.IsTokenUsed = false;
        account.PersonalIdentity.LastVerificationRequestUtc = currentTime;

        var email = DraftVerificationEmail(account, token, currentTime);
        account.PersonalIdentity.PendingVerificationEmail = email;

        return (true, "A new verification email has been dispatched.", email);
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

        var refTime = verificationTime ?? DateTimeOffset.UtcNow;

        if (account.PersonalIdentity.IsTokenUsed)
        {
            return (false, "Verification token has already been used. Please request a new verification code.");
        }

        if (account.PersonalIdentity.VerificationTokenExpiresAtUtc.HasValue &&
            refTime > account.PersonalIdentity.VerificationTokenExpiresAtUtc.Value)
        {
            return (false, "Verification token has expired. Please request a new verification code.");
        }

        var expected = account.PersonalIdentity.VerificationToken?.Trim();
        var actual = verificationToken.Trim();

        if (string.IsNullOrEmpty(expected) || !string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Invalid verification token. Please verify the code and try again.");
        }

        account.PersonalIdentity.IsEmailVerified = true;
        account.PersonalIdentity.VerifiedAtUtc = refTime;
        account.PersonalIdentity.IsTokenUsed = true;
        account.PersonalIdentity.VerificationToken = null; // Single-use: cleared on successful verification

        if (account.PersonalIdentity.PendingVerificationEmail != null)
        {
            account.PersonalIdentity.PendingVerificationEmail = account.PersonalIdentity.PendingVerificationEmail with { IsUsed = true };
        }

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

    /// <inheritdoc />
    public (bool IsValid, string Message, IReadOnlyList<LeadershipArchetype> Archetypes, string DoctrineTitle) ValidateLeadershipSelection(
        IEnumerable<string>? archetypeInputs)
    {
        if (archetypeInputs == null)
        {
            return (false, "Please select exactly two leadership archetypes to define your executive doctrine.", Array.Empty<LeadershipArchetype>(), string.Empty);
        }

        var rawList = archetypeInputs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        if (rawList.Count == 0)
        {
            return (false, "Please select exactly two leadership archetypes to define your executive doctrine.", Array.Empty<LeadershipArchetype>(), string.Empty);
        }

        var parsed = new List<LeadershipArchetype>();
        foreach (var item in rawList)
        {
            if (!LeadershipArchetypeRegistry.TryGet(item, out var def) || def == null || def.Archetype == LeadershipArchetype.None)
            {
                var validArchetypes = string.Join(", ", LeadershipArchetypeRegistry.All.Select(a => a.DisplayName));
                return (false, $"Unrecognized leadership archetype '{item}'. Valid archetypes are: {validArchetypes}.", Array.Empty<LeadershipArchetype>(), string.Empty);
            }

            parsed.Add(def.Archetype);
        }

        var distinct = parsed.Distinct().ToList();
        if (distinct.Count != parsed.Count)
        {
            return (false, "Leadership selection contains duplicate archetypes. You must select two distinct archetypes.", Array.Empty<LeadershipArchetype>(), string.Empty);
        }

        if (distinct.Count != 2)
        {
            return (false, $"Exactly two leadership archetypes must be selected (you provided {distinct.Count}).", Array.Empty<LeadershipArchetype>(), string.Empty);
        }

        var doctrineTitle = LeadershipArchetypeRegistry.ComputeDoctrineTitle(distinct[0], distinct[1]);
        return (true, $"Leadership doctrine '{doctrineTitle}' validated successfully.", distinct, doctrineTitle);
    }

    /// <inheritdoc />
    public PlayerAccount ApplyLeadershipSelection(
        PlayerAccount account,
        IReadOnlyList<LeadershipArchetype> archetypes,
        string? doctrineTitle = null)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(archetypes);

        var distinct = archetypes.Where(a => a != LeadershipArchetype.None).Distinct().ToList();
        if (distinct.Count != 2)
        {
            throw new ArgumentException($"Exactly two distinct leadership archetypes must be provided, got {distinct.Count}.", nameof(archetypes));
        }

        var title = string.IsNullOrWhiteSpace(doctrineTitle)
            ? LeadershipArchetypeRegistry.ComputeDoctrineTitle(distinct[0], distinct[1])
            : doctrineTitle.Trim();

        account.SelectedLeadershipArchetypes = distinct;
        account.LeadershipDoctrineTitle = title;

        if (account.CurrentStep < OnboardingStep.LeadershipSelected)
        {
            account.CurrentStep = OnboardingStep.LeadershipSelected;
        }

        return account;
    }

    /// <inheritdoc />
    public (bool IsValid, string Message, IReadOnlyList<NationalDevelopmentPriority> Priorities) ValidateDevelopmentPlansSelection(
        IEnumerable<string>? planInputs)
    {
        if (planInputs == null)
        {
            return (false, "Please select exactly four national development priorities to establish your administration's inaugural agenda.", Array.Empty<NationalDevelopmentPriority>());
        }

        var rawList = planInputs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        if (rawList.Count == 0)
        {
            return (false, "Please select exactly four national development priorities to establish your administration's inaugural agenda.", Array.Empty<NationalDevelopmentPriority>());
        }

        var parsed = new List<NationalDevelopmentPriority>();
        foreach (var item in rawList)
        {
            if (!NationalDevelopmentPlanRegistry.TryGet(item, out var def) || def == null || def.Priority == NationalDevelopmentPriority.None)
            {
                var validPlans = string.Join(", ", NationalDevelopmentPlanRegistry.All.Select(p => p.DisplayName));
                return (false, $"Unrecognized national development priority '{item}'. Valid priorities are: {validPlans}.", Array.Empty<NationalDevelopmentPriority>());
            }

            parsed.Add(def.Priority);
        }

        var distinct = parsed.Distinct().ToList();
        if (distinct.Count != parsed.Count)
        {
            return (false, "Development plan selection contains duplicate priorities. You must select four distinct priorities.", Array.Empty<NationalDevelopmentPriority>());
        }

        if (distinct.Count != 4)
        {
            return (false, $"Exactly four national development priorities must be selected (you provided {distinct.Count}).", Array.Empty<NationalDevelopmentPriority>());
        }

        return (true, "National development agenda with 4 priorities validated successfully.", distinct);
    }

    /// <inheritdoc />
    public PlayerAccount ApplyDevelopmentPlansSelection(
        PlayerAccount account,
        IReadOnlyList<NationalDevelopmentPriority> priorities)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(priorities);

        var distinct = priorities.Where(p => p != NationalDevelopmentPriority.None).Distinct().ToList();
        if (distinct.Count != 4)
        {
            throw new ArgumentException($"Exactly four distinct national development priorities must be provided, got {distinct.Count}.", nameof(priorities));
        }

        account.SelectedDevelopmentPlans = distinct;

        if (account.CurrentStep < OnboardingStep.DevelopmentPlansSelected)
        {
            account.CurrentStep = OnboardingStep.DevelopmentPlansSelected;
        }

        return account;
    }
}
