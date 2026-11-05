namespace Republic.Core.Legislature.Models;

/// <summary>
/// Domain model representing an individual parliamentarian / lawmaker in the National Assembly.
/// Lawmakers hold seat influence and can be courted across the aisle to defect to the President's party
/// via technocratic developmental grants, committee chairs, or political patronage.
/// </summary>
public sealed class Legislator
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Constituency { get; set; } = string.Empty;
    public string OriginalParty { get; set; } = "Opposition";
    public string CurrentParty { get; set; } = "Opposition";
    public int SeatWeight { get; set; } = 1;
    public string Stance { get; set; } = string.Empty;
    public double LoyaltyToOriginal { get; set; } = 60.0;
    public bool HasDefectedToRulingParty { get; set; } = false;
    public string DefectionIncentive { get; set; } = string.Empty;
}

