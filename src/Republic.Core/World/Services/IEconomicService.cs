namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for GDP, treasury balance, inflation, and currency economics.
/// </summary>
public interface IEconomicService
{
    EconomicIndicator GetIndicators();
    Currency GetCurrency();
    void DepositTreasury(double amount);
    bool WithdrawTreasury(double amount);
    void AdvanceEconomyTick();
}
