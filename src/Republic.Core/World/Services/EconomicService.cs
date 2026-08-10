namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for national GDP, fiscal management, and currency balances.
/// </summary>
public sealed class EconomicService : IEconomicService
{
    private readonly EconomicIndicator _indicators = new();
    private readonly Currency _currency = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;

    public EconomicService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public EconomicIndicator GetIndicators() => _indicators;

    public Currency GetCurrency() => _currency;

    public void DepositTreasury(double amount)
    {
        if (amount <= 0) return;
        _indicators.TreasuryBalance += amount;
        _logger?.LogInfo($"Treasury deposit: +{_currency.Symbol}{amount:N0} (New Balance: {_currency.Symbol}{_indicators.TreasuryBalance:N0})");
        _eventBus.PublishAsync(new EconomyUpdatedEvent(_indicators, DateTimeOffset.UtcNow));
    }

    public bool WithdrawTreasury(double amount)
    {
        if (amount <= 0 || _indicators.TreasuryBalance < amount)
        {
            return false;
        }

        _indicators.TreasuryBalance -= amount;
        _logger?.LogInfo($"Treasury withdrawal: -{_currency.Symbol}{amount:N0} (New Balance: {_currency.Symbol}{_indicators.TreasuryBalance:N0})");
        _eventBus.PublishAsync(new EconomyUpdatedEvent(_indicators, DateTimeOffset.UtcNow));
        return true;
    }

    public void AdvanceEconomyTick()
    {
        // Baseline economic expansion
        _indicators.GrossDomesticProduct *= 1.0 + (_indicators.InflationRate / 365.0);
    }
}
