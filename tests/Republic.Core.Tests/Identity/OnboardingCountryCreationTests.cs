namespace Republic.Core.Tests.Identity;

using System;
using System.IO;
using Republic.Core.Client;
using Republic.Core.Identity.Models;
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
    public void FullOnboardingFlow_RegisterVerifyAndFound_SucceedsWithProceduralData()
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

        // 2. Attempt Country Creation Before Verification -> Rejected
        var prematureResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Solaria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_premature");

        Assert.False(prematureResult.Success);
        Assert.Equal("EmailNotVerified", prematureResult.Result);
        Assert.Contains("Email verification must be completed", prematureResult.Message);

        // 3. Verify Email with valid token
        var verifyResult = PresidentialDeskBridge.VerifyEmailDecision(
            token: regResult.VerificationToken,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_verify_1");

        Assert.True(verifyResult.Success);
        Assert.Equal("Success", verifyResult.Result);
        Assert.Equal("EmailVerified", verifyResult.OnboardingStep);
        Assert.True(verifyResult.IsEmailVerified);

        // 4. Country Creation After Verification -> Succeeds with procedural generation
        var foundResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Solaria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_valid");

        Assert.True(foundResult.Success);
        Assert.Equal("Success", foundResult.Result);
        Assert.Equal("CountryCreated", foundResult.OnboardingStep);
        Assert.Equal("Republic of Solaria", foundResult.CountryName);

        // 5. Verify authoritative persistent state has authentic procedural profile
        var envelope = PresidentialDeskBridge.LoadAuthoritativeState(_savePath);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.State);
        Assert.NotNull(envelope.State.Account);
        Assert.Equal("Elena Okafor", envelope.State.Account.DisplayName);
        Assert.Equal(OnboardingStep.CountryCreated, envelope.State.OnboardingStep);

        var country = envelope.State.World.Countries[0];
        Assert.Equal("Republic of Solaria", country.Name);
        Assert.NotNull(country.Profile);
        Assert.False(string.IsNullOrWhiteSpace(country.Profile.Capital));
        Assert.False(string.IsNullOrWhiteSpace(country.Profile.ConstitutionName));
        Assert.False(string.IsNullOrWhiteSpace(country.Profile.PoliticalCulture));
        Assert.False(string.IsNullOrWhiteSpace(country.Profile.PrimaryResource));
        Assert.True(country.Profile.Population >= 1_500_000);
        Assert.Single(country.ResourceNodes);

        // 6. Attempt Duplicate Country Creation in Same Session -> Rejected
        var duplicateResult = PresidentialDeskBridge.CreateOrLoadCountry(
            countryName: "Republic of Azuria",
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_found_duplicate");

        Assert.False(duplicateResult.Success);
        Assert.Equal("DuplicateCountryCreation", duplicateResult.Result);
        Assert.Contains("already been founded", duplicateResult.Message);
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesRegistrationAndVerificationRequests()
    {
        // Write registration request
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
        var token = regAction.VerificationToken;

        // Write verification request
        var verJson = $$"""
        {
            "action": "verify_email",
            "token": "{{token}}",
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

