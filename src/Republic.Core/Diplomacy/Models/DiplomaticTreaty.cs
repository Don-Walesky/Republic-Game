namespace Republic.Core.Diplomacy.Models;

/// <summary>
/// Domain model representing a binding diplomatic treaty.
/// </summary>
public sealed class DiplomaticTreaty
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public TreatyType Type { get; init; }
    public string SignatoryCountryA { get; init; } = string.Empty;
    public string SignatoryCountryB { get; init; } = string.Empty;
    public ulong SignedAtTick { get; set; }
    public bool IsActive { get; set; }
}
