namespace Republic.Core.Tests.World;

using System;
using System.Threading.Tasks;
using Republic.Core.Cabinet.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Legislature.Services;
using Republic.Core.Scenarios.Models;
using Republic.Core.Scenarios.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Rules;
using Republic.Core.World.Services;
using Republic.Core.Workspace.Services;
using Xunit;

public sealed class CountryNameRuleTests
{
    private readonly CountryNameRule _rule = new();
    private readonly EventBus _eventBus;
    private readonly TestLogger _logger;

    public CountryNameRuleTests()
    {
        _logger = new TestLogger();
        _eventBus = new EventBus(new EventBusOptions(), _logger);
    }

    [Fact]
    public void Test1_RepublicOfWalesky_IsAccepted()
    {
        Assert.True(_rule.IsValid("Republic of Walesky"));
        var validated = _rule.Validate("Republic of Walesky");
        Assert.Equal("Republic of Walesky", validated);
    }

    [Fact]
    public void Test2_KalakutaRepublic_IsAccepted()
    {
        Assert.True(_rule.IsValid("Kalakuta Republic"));
        var validated = _rule.Validate("Kalakuta Republic");
        Assert.Equal("Kalakuta Republic", validated);
    }

    [Fact]
    public void Test3_Walesky_IsRejected()
    {
        Assert.False(_rule.IsValid("walesky"));
        var ex = Assert.Throws<ArgumentException>(() => _rule.Validate("walesky"));
        Assert.Contains("Republic", ex.Message);
    }

    [Fact]
    public void Test4_Republican_IsRejected()
    {
        // "Republican" alone fails because "Republic" is not present as a whole word
        Assert.False(_rule.IsValid("Republican"));
        var ex = Assert.Throws<ArgumentException>(() => _rule.Validate("Republican"));
        Assert.Contains("Republic", ex.Message);
    }

    [Fact]
    public void Test5_TheRepublicanRepublic_IsAccepted()
    {
        // "The Republican Republic" passes because Republic is present as a standalone whole word
        Assert.True(_rule.IsValid("The Republican Republic"));
        var validated = _rule.Validate("The Republican Republic");
        Assert.Equal("The Republican Republic", validated);
    }

    [Fact]
    public void Test6_WhitespacePadded_RepublicOfWalesky_IsAcceptedAfterTrim()
    {
        Assert.True(_rule.IsValid("  republic of walesky  "));
        var validated = _rule.Validate("  republic of walesky  ");
        Assert.Equal("republic of walesky", validated);
    }

    [Theory]
    [InlineData("republic")]
    [InlineData("Republic")]
    [InlineData("REPUBLIC")]
    [InlineData("RePuBlIc")]
    public void Test7_CaseInsensitiveMatching_AllPass(string name)
    {
        Assert.True(_rule.IsValid(name));
        Assert.Equal(name, _rule.Validate(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Test8_EmptyOrWhitespace_IsRejected(string? emptyName)
    {
        Assert.False(_rule.IsValid(emptyName));
        Assert.Throws<ArgumentException>(() => _rule.Validate(emptyName));
    }

    [Fact]
    public void Test9_RejectedName_DoesNotChangeCountryCount()
    {
        var clock = RepublicClock.CreateControlled();
        var countryService = new CountryService(_eventBus, _logger, clock);

        Assert.Empty(countryService.GetAllCountries());

        // Attempting to found country with invalid name throws ArgumentException
        Assert.Throws<ArgumentException>(() => countryService.FoundCountry("walesky"));
        Assert.Empty(countryService.GetAllCountries());

        Assert.Throws<ArgumentException>(() => countryService.FoundCountry("Republican"));
        Assert.Empty(countryService.GetAllCountries());

        Assert.Throws<ArgumentException>(() => countryService.FoundCountry("   "));
        Assert.Empty(countryService.GetAllCountries());

        // Valid creation increments count
        var created = countryService.FoundCountry("Republic of Walesky");
        Assert.NotNull(created);
        Assert.Single(countryService.GetAllCountries());

        // Another invalid attempt does not change count or alter existing country
        Assert.Throws<ArgumentException>(() => countryService.FoundCountry("another invalid nation"));
        Assert.Single(countryService.GetAllCountries());
        Assert.Equal("Republic of Walesky", countryService.GetCountry(created.Id)?.Name);
    }

    [Fact]
    public void Test10_ValidNames_CreateCountry_WithTrimmedName()
    {
        var clock = RepublicClock.CreateControlled();
        var countryService = new CountryService(_eventBus, _logger, clock);

        var country = countryService.FoundCountry("  republic of walesky  ");
        Assert.NotNull(country);
        Assert.Equal("republic of walesky", country.Name);
    }

    [Fact]
    public async Task Test11_ScenarioBootstrap_KeepsArcadiaIfContainsRepublic_OrSetsToRepublicOfArcadia()
    {
        var clock = RepublicClock.CreateControlled();
        var world = new WorldManager(_eventBus, _logger, clock);
        var workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
        var cabinet = new CabinetService(world, _eventBus, workspace, _logger);
        var legislature = new LegislatureService(_eventBus, workspace, _logger);
        var bootstrapper = new ScenarioBootstrapper(world, workspace, cabinet, legislature, _logger);

        // Bootstrap default arcadia-day1 ("Republic of Arcadia")
        await bootstrapper.BootstrapScenarioAsync("arcadia-day1");
        var playerCountry = world.Countries.GetCountry("player-country");
        Assert.NotNull(playerCountry);
        Assert.Equal("Republic of Arcadia", playerCountry.Name);
    }

    [Fact]
    public async Task Test12_ScenarioBootstrap_SetsArcadiaToRepublicOfArcadia_IfLackingRepublic()
    {
        var clock = RepublicClock.CreateControlled();
        var world = new WorldManager(_eventBus, _logger, clock);
        var workspace = new WorkspaceManager(
            new VisitorService(_eventBus, _logger),
            new PhoneService(_eventBus, _logger),
            new EmailService(_eventBus, _logger),
            new NewsService(_eventBus, _logger),
            new CalendarService(_eventBus, _logger),
            _eventBus,
            _logger);
        var cabinet = new CabinetService(world, _eventBus, workspace, _logger);
        var legislature = new LegislatureService(_eventBus, workspace, _logger);
        var customPresets = new[]
        {
            new ScenarioPreset
            {
                Id = "custom-bare-arcadia",
                Name = "Bare Arcadia Scenario",
                PlayerCountryName = "Arcadia"
            }
        };
        var bootstrapper = new ScenarioBootstrapper(world, workspace, cabinet, legislature, _logger, customPresets);

        await bootstrapper.BootstrapScenarioAsync("custom-bare-arcadia");
        var playerCountry = world.Countries.GetCountry("player-country");
        Assert.NotNull(playerCountry);
        Assert.Equal("Republic of Arcadia", playerCountry.Name);
    }

    [Fact]
    public void Test13_PromptWorkflow_RejectsInvalidNames_UntilValidNameProvided()
    {
        var inputs = new[] { "walesky", "Republican", "   ", "Republic of Walesky" };
        var inputIndex = 0;
        var rejectionOutputs = new System.Collections.Generic.List<string>();

        string PromptLoop()
        {
            while (inputIndex < inputs.Length)
            {
                var input = inputs[inputIndex++];
                if (CountryNameRule.Default.IsValid(input))
                {
                    return CountryNameRule.Default.Validate(input);
                }

                rejectionOutputs.Add($"Invalid: {input}");
            }

            throw new InvalidOperationException("No valid input found.");
        }

        var result = PromptLoop();
        Assert.Equal("Republic of Walesky", result);
        Assert.Equal(3, rejectionOutputs.Count);
        Assert.Contains("Invalid: walesky", rejectionOutputs);
        Assert.Contains("Invalid: Republican", rejectionOutputs);
        Assert.Contains("Invalid:    ", rejectionOutputs);
    }
}
