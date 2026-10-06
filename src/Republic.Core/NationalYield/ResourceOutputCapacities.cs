namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model encapsulating natural resource extraction and raw material production output.
/// </summary>
public sealed class ResourceOutputCapacities
{
    /// <summary>
    /// Gets or sets sovereign raw natural resource output (hydrocarbons, industrial ores, timber, rare earths) where geographically applicable.
    /// </summary>
    public double NaturalResourceOutput { get; set; }

    /// <summary>
    /// Creates a deep clone of the current resource output capacities.
    /// </summary>
    public ResourceOutputCapacities Clone() => new()
    {
        NaturalResourceOutput = NaturalResourceOutput
    };
}
