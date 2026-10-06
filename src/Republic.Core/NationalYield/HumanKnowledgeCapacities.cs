namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model encapsulating human, educational, scientific, and innovation capacities of a sovereign nation.
/// </summary>
public sealed class HumanKnowledgeCapacities
{
    /// <summary>
    /// Gets or sets national workforce skill level, educational attainment, technical literacy, and public health vitality.
    /// </summary>
    public double HumanCapital { get; set; }

    /// <summary>
    /// Gets or sets academic institution strength, laboratory infrastructure, scientific research bandwidth, and patent output.
    /// </summary>
    public double ScienceCapacity { get; set; }

    /// <summary>
    /// Gets or sets commercial technology adoption, entrepreneurial momentum, operational modernism, and efficiency gains.
    /// </summary>
    public double InnovationCapacity { get; set; }

    /// <summary>
    /// Creates a deep clone of the current human and knowledge capacities.
    /// </summary>
    public HumanKnowledgeCapacities Clone() => new()
    {
        HumanCapital = HumanCapital,
        ScienceCapacity = ScienceCapacity,
        InnovationCapacity = InnovationCapacity
    };
}
