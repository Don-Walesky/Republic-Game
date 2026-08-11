namespace Republic.Core.Scenarios.Models;

using System.Collections.Generic;

/// <summary>
/// Static registry of pre-packaged campaign scenarios for quick access and preset validation.
/// </summary>
public static class ScenarioPresets
{
    public const string ArcadiaDay1 = "arcadia-day1";
    public const string ResourceCrisis = "resource-crisis";
    public const string ColdWar = "cold-war";
    public const string PostCoup = "post-coup";

    public static IReadOnlyList<string> AllPresetIds => new[]
    {
        ArcadiaDay1,
        ResourceCrisis,
        ColdWar,
        PostCoup
    };
}
