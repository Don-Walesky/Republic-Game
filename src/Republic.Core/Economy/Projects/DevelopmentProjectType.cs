namespace Republic.Core.Economy.Projects;

/// <summary>
/// Categorical enumeration of executive development project types in Republic.
/// In this slice, Road and University are active types; establishes the foundational enum for future infrastructure expansion.
/// </summary>
public enum DevelopmentProjectType
{
    /// <summary>
    /// National road and transport artery program.
    /// </summary>
    Road = 0,

    /// <summary>
    /// Higher education and academic research university.
    /// </summary>
    University = 1,

    /// <summary>
    /// National electric power plant and energy generation facility.
    /// </summary>
    PowerPlant = 2,

    /// <summary>
    /// Private-sector co-investment or commercial facility project.
    /// </summary>
    PrivateInvestment = 3
}

