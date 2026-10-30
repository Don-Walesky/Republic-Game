namespace Republic.Core.Economy.Projects;

/// <summary>
/// Categorical enumeration of executive development project types in Republic.
/// In this slice, Road, University, PowerPlant, PrivateInvestment, Refinery, and Mine are active types.
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
    PrivateInvestment = 3,

    /// <summary>
    /// National oil refinery and petrochemical processing facility.
    /// </summary>
    Refinery = 4,

    /// <summary>
    /// National mineral extraction and mining facility.
    /// </summary>
    Mine = 5
}
