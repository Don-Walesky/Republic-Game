namespace Republic.Core.Tests.Identity;

using System;
using System.IO;
using Republic.Core.Client;
using Republic.Core.Identity.Models;
using Republic.Core.World.Factories;
using Xunit;

public sealed class OnboardingCountryCreationTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _statePath;
    private readonly string _savePath;
    private readonly string _requestPath;

    public OnboardingCountryCreationTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Republic_Onboarding_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        _statePath = Path.Combine(_testDir, "presidential_desk_state.json");
        _savePath = Path.Combine(_testDir, "session.sav");
        _requestPath = Path.Combine(_testDir, "presidential_desk_request.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // ignore cleanup failure
        }
    }

    [Fact]
    public void FullOnboardingFlow_RegisterVerifyAndFound_SucceedsWithStandardizedMetricsAndOwnership()
    {
        var refDate = new DateOnly(2027, 2, 10);

        // 1. Register Account
        var regResult = PresidentialDeskBridge.RegisterAccountDecision(
            firstName: "Elena",
            lastName: "Okafor",
            email: "elena@republic.net",
            dateOfBirthString: "1992-04-18",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_reg_1",
            referenceDate: refDate);

        Assert.True(regResult.Success);
        Assert.Equal("Success", regResult.Result);
        Assert.Equal("AccountRegistered", regResult.OnboardingStep);
        Assert.False(regResult.IsEmailVerified);
        Assert.False(string.IsNullOrWhiteSpace(regResult.VerificationToken));
        Assert.Equal("Verify Email", regResult.VerificationButtonText);
        Assert.Contains("Elena Okafor", regResult.VerificationEmailBody);

        // 2. Prevent Duplicate Account Registration with Same Email
        var dupResult = PresidentialDeskBridge.RegisterAccountDecision(
            firstName: "Elena",
            lastName: "Okafor",
            email: "elena@republic.net",
            dateOfBirthString: "1992-04-18",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_reg_dup",
            referenceDate: refDate);

        Assert.False(dupResult.Success);
        Assert.Equal("DuplicateAccount", dupResult.Result);

        // 3. Attempt Country Creation Before Verification -> Blocked
        var prematureResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Solaria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_premature");

        Assert.False(prematureResult.Success);
        Assert.Equal("EmailNotVerified", prematureResult.Result);
        Assert.Contains("Email verification must be completed", prematureResult.Message);

        // 4. Verify Email with valid token
        var verifyResult = PresidentialDeskBridge.VerifyEmailDecision(
            token: regResult.VerificationToken,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_verify_1");

        Assert.True(verifyResult.Success);
        Assert.Equal("Success", verifyResult.Result);
        Assert.Equal("EmailVerified", verifyResult.OnboardingStep);
        Assert.True(verifyResult.IsEmailVerified);

        // 5. Attempt Invalid Country Name (Missing 'Republic') -> Rejected
        var invalidNameResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Land of Nowhere",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_invalid_name");

        Assert.False(invalidNameResult.Success);
        Assert.Equal("InvalidName", invalidNameResult.Result);

        // 6. Country Creation After Verification -> Succeeds with authoritative standardized metrics
        var foundResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Solaria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_valid");

        Assert.True(foundResult.Success);
        Assert.Equal("Success", foundResult.Result);
        Assert.Equal("CountryCreated", foundResult.OnboardingStep);
        Assert.Equal("Republic of Solaria", foundResult.CountryName);
        Assert.Equal(CountryProfileFactory.StandardStartingTreasury, foundResult.TreasuryBalance);
        Assert.Equal(CountryProfileFactory.StandardStartingApproval, foundResult.ApprovalRating);
        Assert.Equal(CountryProfileFactory.StandardStartingStability, foundResult.StabilityRating);
        Assert.Equal(CountryProfileFactory.StandardStartingPopulation, foundResult.Population);

        // 7. Verify authoritative persistent state, player ownership link, and profile
        var envelope = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.State);
        Assert.NotNull(envelope.State.Account);
        Assert.Equal("Elena Okafor", envelope.State.Account.DisplayName);
        Assert.Equal(OnboardingStep.CountryCreated, envelope.State.OnboardingStep);

        var country = envelope.State.World.Countries[0];
        Assert.Equal("Republic of Solaria", country.Name);
        Assert.Equal(country.Id, envelope.State.Account.OwnedCountryId);
        Assert.NotNull(country.Profile);
        Assert.Equal(CountryProfileFactory.StandardStartingPopulation, country.Profile.Population);
        Assert.Equal(CountryProfileFactory.StandardStartingTreasury, country.Treasury.Balance);
        Assert.Equal(CountryProfileFactory.StandardStartingApproval, country.HappinessRating);
        Assert.Equal(CountryProfileFactory.StandardStartingStability, country.BaselineStability);

        // 8. Attempt Duplicate Country Creation in Same Session -> Prohibited
        var duplicateFoundResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Azuria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_duplicate");

        Assert.False(duplicateFoundResult.Success);
        Assert.Equal("DuplicateCountryCreation", duplicateFoundResult.Result);
        Assert.Contains("already been founded", duplicateFoundResult.Message);
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesRegistrationResendAndVerificationRequests()
    {
        // 1. Write registration request
        var regJson = """
        {
            "action": "register_account",
            "first_name": "Avery",
            "last_name": "Cole",
            "email": "avery.cole@republic.gov",
            "date_of_birth": "1988-11-05",
            "request_id": "req_reg_json"
        }
        """;
        File.WriteAllText(_requestPath, regJson);

        var regAction = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath);
        Assert.NotNull(regAction);
        Assert.True(regAction.Success);
        Assert.Equal("AccountRegistered", regAction.OnboardingStep);
        Assert.Equal("Verify Email", regAction.VerificationButtonText);

        // 2. Write verification request using the issued token
        var verJson = $$"""
        {
            "action": "verify_email",
            "token": "{{regAction.VerificationToken}}",
            "request_id": "req_ver_json"
        }
        """;
        File.WriteAllText(_requestPath, verJson);

        var verAction = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath);
        Assert.NotNull(verAction);
        Assert.True(verAction.Success);
        Assert.Equal("EmailVerified", verAction.OnboardingStep);
        Assert.True(verAction.IsEmailVerified);
    }
}
