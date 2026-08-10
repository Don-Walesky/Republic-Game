namespace Republic.Core.AI.Services;

using Republic.Core.AI.Models;
using Republic.Core.Crises.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Models;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Events;
using Republic.Core.Intelligence.Models;
using Republic.Core.Intelligence.Services;
using Republic.Core.World;

/// <summary>
/// Service implementation evaluating rival AI sovereign decisions per frame tick.
/// </summary>
public sealed class RivalAIService : IRivalAIService
{
    private readonly List<RivalAIBot> _bots = new();
    private readonly IWorldManager _worldManager;
    private readonly IDiplomacyService _diplomacyService;
    private readonly IInterPlayerWarfareService _warfareService;
    private readonly IIntelligenceService _intelligenceService;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public RivalAIService(
        IWorldManager worldManager,
        IDiplomacyService diplomacyService,
        IInterPlayerWarfareService warfareService,
        IIntelligenceService intelligenceService,
        IEventBus eventBus,
        ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _diplomacyService = diplomacyService ?? throw new ArgumentNullException(nameof(diplomacyService));
        _warfareService = warfareService ?? throw new ArgumentNullException(nameof(warfareService));
        _intelligenceService = intelligenceService ?? throw new ArgumentNullException(nameof(intelligenceService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public void RegisterRivalBot(RivalAIBot bot)
    {
        ArgumentNullException.ThrowIfNull(bot);
        lock (_lock)
        {
            if (!_bots.Any(b => b.CountryId == bot.CountryId))
            {
                _bots.Add(bot);
            }
        }
    }

    public IReadOnlyList<RivalAIBot> GetRivalBots()
    {
        lock (_lock)
        {
            return _bots.ToList().AsReadOnly();
        }
    }

    public async Task<int> ProcessAITickAsync(ulong currentTick, CancellationToken cancellationToken = default)
    {
        // Run AI evaluation cycle every 100 ticks
        if (currentTick == 0 || currentTick % 100 != 0)
        {
            return 0;
        }

        List<RivalAIBot> bots;
        lock (_lock)
        {
            bots = _bots.ToList();
        }

        var actionsTaken = 0;
        var playerCountry = _worldManager.Countries.GetAllCountries().FirstOrDefault(c => c.Id == "player-country") ?? _worldManager.Countries.GetAllCountries().FirstOrDefault();

        if (playerCountry == null)
        {
            return 0;
        }

        foreach (var bot in bots)
        {
            var relation = _diplomacyService.GetRelation(bot.CountryId, playerCountry.Id);

            // 1. Aggressive AI Action
            if (bot.Behavior == RivalAIBehavior.Aggressive || bot.AggressionIndex >= 0.7)
            {
                if (playerCountry.BaselineStability < 50.0 && relation.Status != DiplomaticStatus.Allied)
                {
                    actionsTaken++;
                    _logger?.LogWarning($"RIVAL AI [{bot.Name}]: Launching Cyber Attack against destabilized player nation.");
                    await _warfareService.LaunchCyberAttackAsync(bot.CountryId, playerCountry.Id, "PowerGrid", cancellationToken).ConfigureAwait(false);
                }
            }
            // 2. Diplomatic AI Action
            else if (bot.Behavior == RivalAIBehavior.Diplomatic || bot.CooperationIndex >= 0.6)
            {
                if (relation.Status == DiplomaticStatus.Neutral)
                {
                    actionsTaken++;
                    _logger?.LogInfo($"RIVAL AI [{bot.Name}]: Proposing Trade Agreement to player nation.");
                    await _diplomacyService.ProposeTreatyAsync(bot.CountryId, playerCountry.Id, TreatyType.TradeAgreement, $"Pact of Amity ({bot.Name})", cancellationToken).ConfigureAwait(false);
                }
            }
            // 3. Opportunistic AI Action
            else if (bot.Behavior == RivalAIBehavior.Opportunistic)
            {
                actionsTaken++;
                await _intelligenceService.InfiltrateTargetAsync(playerCountry.Id, 1, cancellationToken).ConfigureAwait(false);
            }
        }

        return actionsTaken;
    }
}
