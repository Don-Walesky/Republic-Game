namespace Republic.Core.World.Models;

/// <summary>
/// Defines the foundational lifecycle status of a sovereign country.
/// </summary>
public enum CountryFoundingStatus
{
    /// <summary>
    /// Newly proclaimed nation in its foundational period.
    /// </summary>
    NewlyFounded = 0,

    /// <summary>
    /// Established nation with mature institutional statehood.
    /// </summary>
    Established = 1
}
