namespace Republic.Core.Tests.Identity;

using System;
using Republic.Core.Identity.Models;
using Republic.Core.Identity.Services;
using Xunit;

public sealed class PlayerIdentityServiceTests
{
    private readonly PlayerIdentityService _service = new();
    private static readonly DateOnly ReferenceDate = new(2027, 2, 10);

    [Fact]
    public void ValidateRegistration_ValidDetails_ReturnsTrue()
    {
        var dob = new DateOnly(2000, 5, 15);
        var (isValid, message) = _service.ValidateRegistration("Elena", "Okafor", "elena.okafor@republic.net", dob, ReferenceDate);

        Assert.True(isValid);
        Assert.Equal("Registration details valid.", message);
    }

    [Theory]
    [InlineData("", "Okafor", "test@republic.net", "First name is required.")]
    [InlineData("   ", "Okafor", "test@republic.net", "First name is required.")]
    [InlineData("Elena", "", "test@republic.net", "Last name is required.")]
    [InlineData("Elena", "   ", "test@republic.net", "Last name is required.")]
    [InlineData("Elena", "Okafor", "", "Email address is required.")]
    [InlineData("Elena", "Okafor", "invalid-email", "Please provide a valid email address.")]
    [InlineData("Elena", "Okafor", "missing_domain@com", "Please provide a valid email address.")]
    [InlineData("Elena", "Okafor", "@no_username.com", "Please provide a valid email address.")]
    public void ValidateRegistration_MissingOrInvalidStrings_ReturnsFalse(string first, string last, string email, string expectedMsg)
    {
        var dob = new DateOnly(2000, 1, 1);
        var (isValid, message) = _service.ValidateRegistration(first, last, email, dob, ReferenceDate);

        Assert.False(isValid);
        Assert.Equal(expectedMsg, message);
    }

    [Fact]
    public void ValidateRegistration_FutureDateOfBirth_ReturnsFalse()
    {
        var futureDob = ReferenceDate.AddDays(1);
        var (isValid, message) = _service.ValidateRegistration("David", "Chen", "david.chen@republic.net", futureDob, ReferenceDate);

        Assert.False(isValid);
        Assert.Equal("Date of birth cannot be in the future.", message);
    }

    [Fact]
    public void ValidateRegistration_Under13YearsOld_ReturnsFalse()
    {
        // 12 years and 364 days old
        var underageDob = ReferenceDate.AddYears(-13).AddDays(1);
        var (isValid, message) = _service.ValidateRegistration("Young", "Leader", "young@republic.net", underageDob, ReferenceDate);

        Assert.False(isValid);
        Assert.Contains("Player must be at least 13 years of age to register", message);
    }

    [Fact]
    public void ValidateRegistration_Exact13thBirthday_ReturnsTrue()
    {
        // Exactly 13 years old on ReferenceDate
        var exact13Dob = ReferenceDate.AddYears(-13);
        var (isValid, message) = _service.ValidateRegistration("Teen", "President", "teen@republic.net", exact13Dob, ReferenceDate);

        Assert.True(isValid);
        Assert.Equal("Registration details valid.", message);
    }

    [Fact]
    public void ValidateRegistration_LeapYearBirthDate_HandledCorrectly()
    {
        // Leap day: 2008-02-29
        var leapDob = new DateOnly(2008, 2, 29);
        var testRef = new DateOnly(2027, 2, 28); // 18 years old
        var (isValid, _) = _service.ValidateRegistration("Leap", "Born", "leap@republic.net", leapDob, testRef);

        Assert.True(isValid);
    }

    [Fact]
    public void RegisterAccount_ValidInputs_Generates6DigitTokenAndUnverifiedStatus()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Marcus", "Sterling", "marcus@republic.org", dob, ReferenceDate);

        Assert.NotNull(account);
        Assert.StartsWith("acc_", account.AccountId);
        Assert.Equal("Marcus Sterling", account.DisplayName);
        Assert.Equal(OnboardingStep.AccountRegistered, account.CurrentStep);
        Assert.False(account.PersonalIdentity.IsEmailVerified);
        Assert.NotNull(account.PersonalIdentity.VerificationToken);
        Assert.Equal(6, account.PersonalIdentity.VerificationToken.Length);
        Assert.True(int.TryParse(account.PersonalIdentity.VerificationToken, out _));

        // Verifies official drafted email dispatch
        Assert.NotNull(account.PersonalIdentity.PendingVerificationEmail);
        Assert.Equal("marcus@republic.org", account.PersonalIdentity.PendingVerificationEmail.RecipientEmail);
        Assert.Equal("Verify Email", account.PersonalIdentity.PendingVerificationEmail.ButtonText);
        Assert.Contains("Marcus Sterling", account.PersonalIdentity.PendingVerificationEmail.Body);
    }

    [Fact]
    public void VerifyEmail_CorrectToken_AdvancesToEmailVerified()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Elena", "Vance", "elena@republic.org", dob, ReferenceDate);
        var token = account.PersonalIdentity.VerificationToken!;

        var (success, message) = _service.VerifyEmail(account, token);

        Assert.True(success);
        Assert.True(account.PersonalIdentity.IsEmailVerified);
        Assert.Equal(OnboardingStep.EmailVerified, account.CurrentStep);
        Assert.NotNull(account.PersonalIdentity.VerifiedAtUtc);
        Assert.Null(account.PersonalIdentity.VerificationToken); // Single-use: cleared
        Assert.True(account.PersonalIdentity.IsTokenUsed);
        Assert.Contains("verified successfully", message);
    }

    [Fact]
    public void VerifyEmail_IncorrectToken_FailsValidation()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Elena", "Vance", "elena@republic.org", dob, ReferenceDate);

        var (success, message) = _service.VerifyEmail(account, "999999");

        Assert.False(success);
        Assert.False(account.PersonalIdentity.IsEmailVerified);
        Assert.Equal(OnboardingStep.AccountRegistered, account.CurrentStep);
        Assert.Contains("Invalid verification token", message);
    }

    [Fact]
    public void VerifyEmail_ExpiredToken_IsRejected()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Elena", "Vance", "elena@republic.org", dob, ReferenceDate);
        var token = account.PersonalIdentity.VerificationToken!;

        // Simulate verification attempt 25 hours later (exceeding 24h lifetime)
        var expiredTime = DateTimeOffset.UtcNow.AddHours(25);
        var (success, message) = _service.VerifyEmail(account, token, expiredTime);

        Assert.False(success);
        Assert.False(account.PersonalIdentity.IsEmailVerified);
        Assert.Contains("expired", message);
    }

    [Fact]
    public void VerifyEmail_ReusedToken_IsRejected()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Elena", "Vance", "elena@republic.org", dob, ReferenceDate);
        var token = account.PersonalIdentity.VerificationToken!;

        // First verification succeeds
        var (success1, _) = _service.VerifyEmail(account, token);
        Assert.True(success1);

        // Reset verification flag to simulate malicious re-verification replay
        account.PersonalIdentity.IsEmailVerified = false;
        var (success2, message) = _service.VerifyEmail(account, token);

        Assert.False(success2);
        Assert.Contains("already been used", message);
    }

    [Fact]
    public void ResendVerificationEmail_EnforcesRateLimitCooldown()
    {
        var dob = new DateOnly(1995, 8, 24);
        var account = _service.RegisterAccount("Elena", "Vance", "elena@republic.org", dob, ReferenceDate);
        var initialToken = account.PersonalIdentity.VerificationToken;

        // Immediate resend attempt (within 10s cooldown) -> Rejected
        var (resendSuccess1, resendMsg1, _) = _service.ResendVerificationEmail(account, DateTimeOffset.UtcNow);
        Assert.False(resendSuccess1);
        Assert.Contains("wait", resendMsg1);

        // Resend after cooldown elapsed (e.g. 15s later) -> Succeeds with new token
        var (resendSuccess2, resendMsg2, newEmail) = _service.ResendVerificationEmail(account, DateTimeOffset.UtcNow.AddSeconds(15));
        Assert.True(resendSuccess2);
        Assert.NotNull(newEmail);
        Assert.NotEqual(initialToken, account.PersonalIdentity.VerificationToken);
        Assert.Equal("Verify Email", newEmail.ButtonText);
    }

    [Theory]
    [InlineData("Republic of Azuria", true)]
    [InlineData("Democratic Republic of Oakhaven", true)]
    [InlineData("Azuria", false)] // Missing "Republic"
    [InlineData("Rep", false)] // Too short (<4 chars)
    [InlineData("Republic of <Script>Alert</Script>", false)] // Invalid characters
    public void ValidateCountryName_EnforcesRules(string countryName, bool expectedValid)
    {
        var (isValid, _) = _service.ValidateCountryName(countryName);
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void GetPublicIdentity_DoesNotExposePII()
    {
        var dob = new DateOnly(1990, 1, 1);
        var account = _service.RegisterAccount("SecretFirst", "SecretLast", "secret.email@domain.com", dob, ReferenceDate);

        var publicId = account.GetPublicIdentity();

        // Should return formatted display name, never raw email or date of birth
        Assert.DoesNotContain("secret.email@domain.com", publicId);
        Assert.DoesNotContain("1990", publicId);
        Assert.Equal("SecretFirst SecretLast", publicId);
    }
}
