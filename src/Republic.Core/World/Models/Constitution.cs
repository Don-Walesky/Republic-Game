namespace Republic.Core.World.Models;

/// <summary>
/// Domain model representing government structure and constitutional laws.
/// </summary>
public sealed class Constitution
{
    public string GovernmentSystem { get; set; } = "Constitutional Republic";
    public double ExecutivePowerRating { get; set; } = 7.0;
    public double JudicialIndependenceRating { get; set; } = 8.0;
    public double CivilRightsRating { get; set; } = 8.5;
    public List<string> EnactedRights { get; init; } = new()
    {
        "Freedom of Speech",
        "Freedom of Assembly",
        "Right to Fair Trial",
        "Right to Vote"
    };
}
