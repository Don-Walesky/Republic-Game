namespace Republic.Core.Economy.Budget.Models;

/// <summary>
/// Domain model representing national taxation rates.
/// </summary>
public sealed class TaxPolicy
{
    public double IncomeTaxRate { get; set; } = 0.25; // 25%
    public double CorporateTaxRate { get; set; } = 0.20; // 20%
    public double ImportTariffRate { get; set; } = 0.05; // 5%
}
