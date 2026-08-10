namespace Republic.Core.Scenarios.Services;

using Republic.Core.Cabinet.Models;
using Republic.Core.Cabinet.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Legislature.Models;
using Republic.Core.Legislature.Services;
using Republic.Core.Scenarios.Models;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Implementation bootstrapping starting scenario parameters into the simulation core.
/// </summary>
public sealed class ScenarioBootstrapper : IScenarioBootstrapper
{
    private readonly IWorldManager _worldManager;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ICabinetService _cabinetService;
    private readonly ILegislatureService _legislatureService;
    private readonly ILogger? _logger;

    private readonly List<ScenarioPreset> _presets = new()
    {
        new ScenarioPreset
        {
            Id = "arcadia-day1",
            Name = "Republic of Arcadia - Day 1",
            Description = "Assume executive office in a prosperous federal republic amidst rising regional tensions.",
            PlayerCountryName = "Republic of Arcadia",
            StartingTreasury = 15_000_000_000.0,
            StartingStability = 85.0,
            StartingHappiness = 78.0,
            NeighboringCountries = new List<string> { "Federation of Norse", "Aethel Kingdom" },
            PrimaryResourceNodes = new List<string> { "Arcadia Iron Basin", "Offshore Energy Well A" }
        },
        new ScenarioPreset
        {
            Id = "resource-crisis",
            Name = "Energy Crisis 2030",
            Description = "Steer a nation suffering severe fuel deficits and rampant inflation.",
            PlayerCountryName = "Republic of Solaria",
            StartingTreasury = 3_000_000_000.0,
            StartingStability = 50.0,
            StartingHappiness = 40.0,
            NeighboringCountries = new List<string> { "Imperium of Valeria" },
            PrimaryResourceNodes = new List<string> { "Depleted Solaria Refinery" }
        }
    };

    public ScenarioBootstrapper(
        IWorldManager worldManager,
        IWorkspaceManager workspaceManager,
        ICabinetService cabinetService,
        ILegislatureService legislatureService,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _workspaceManager = workspaceManager ?? throw new ArgumentNullException(nameof(workspaceManager));
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        _legislatureService = legislatureService ?? throw new ArgumentNullException(nameof(legislatureService));
        _logger = logger;
    }

    public IReadOnlyList<ScenarioPreset> GetAvailablePresets() => _presets.AsReadOnly();

    public async Task<ScenarioPreset> BootstrapScenarioAsync(string presetId, CancellationToken cancellationToken = default)
    {
        var preset = _presets.FirstOrDefault(p => p.Id == presetId) ?? _presets[0];

        // 1. Initialize World
        await _worldManager.CreateAsync(preset.Name, cancellationToken).ConfigureAwait(false);

        // Register main country
        var playerCountry = _worldManager.Countries.RegisterCountry(new Country
        {
            Id = "player-country",
            Name = preset.PlayerCountryName,
            BaselineStability = preset.StartingStability
        });

        // Register neighbors
        foreach (var neighbor in preset.NeighboringCountries)
        {
            _worldManager.Countries.RegisterCountry(new Country
            {
                Name = neighbor,
                BaselineStability = 70.0
            });
        }

        // Register resource nodes
        foreach (var node in preset.PrimaryResourceNodes)
        {
            _worldManager.Resources.RegisterNode(new ResourceNode
            {
                ResourceType = node,
                Abundance = 1000.0
            });
        }

        // 2. Bootstrap Parliamentary Parties
        _legislatureService.RegisterParty(new PoliticalParty { Name = "National Alliance", SeatCount = 55, IsGovernmentCoalition = true });
        _legislatureService.RegisterParty(new PoliticalParty { Name = "Social Democrats", SeatCount = 45, IsGovernmentCoalition = false });

        // 3. Bootstrap Starting Cabinet
        await _cabinetService.AppointMinisterAsync(new Minister { Name = "General Arthur Pendelton", CompetenceRating = 88.0, LoyaltyRating = 90.0 }, CabinetPortfolio.Defense, cancellationToken).ConfigureAwait(false);
        await _cabinetService.AppointMinisterAsync(new Minister { Name = "Dr. Elena Rostova", CompetenceRating = 92.0, LoyaltyRating = 80.0 }, CabinetPortfolio.Finance, cancellationToken).ConfigureAwait(false);

        // 4. Welcome Messages in Workspace
        _workspaceManager.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Chief of Staff",
            Subject = $"EXECUTIVE BRIEFING: Welcome Mr. President",
            Body = $"Welcome to the Presidential Suite. Scenario '{preset.Name}' has been initialized. Cabinet ministers and parliamentary leaders await your directives.",
            Folder = "Inbox",
            ActionRequired = true
        });

        _workspaceManager.News.PublishArticle(new NewsArticle
        {
            Source = "Presidential Press Corps",
            Headline = $"INCOMPLETION OF INAUGURATION: EXECUTIVE ADMINISTRATION ASSUMES CONTROL",
            Summary = $"The executive office has assumed formal leadership over {preset.PlayerCountryName}.",
            Category = "Politics",
            ImpactRating = 5
        });

        _logger?.LogInfo($"Scenario '{preset.Name}' successfully bootstrapped.");
        return preset;
    }
}
