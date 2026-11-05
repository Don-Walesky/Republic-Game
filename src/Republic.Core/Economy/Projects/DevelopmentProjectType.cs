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
    Mine = 5,

    /// <summary>
    /// National high-speed rail corridor and inter-city passenger transit network.
    /// </summary>
    HighSpeedRail = 6,

    /// <summary>
    /// Deepwater maritime seaport logistics hub and international container terminal.
    /// </summary>
    DeepwaterPort = 7,

    /// <summary>
    /// Clean solar and wind energy mega-grid facility.
    /// </summary>
    RenewableEnergy = 8,

    /// <summary>
    /// National teaching hospital and advanced medical healthcare center.
    /// </summary>
    TeachingHospital = 9,

    /// <summary>
    /// Special economic and industrial export processing zone.
    /// </summary>
    SpecialEconomicZone = 10,

    /// <summary>
    /// Sovereign AI and high-performance supercomputing data center.
    /// </summary>
    SupercomputingCenter = 11,

    /// <summary>
    /// Agro-industrial irrigation mega-dam and national food security reservoir.
    /// </summary>
    IrrigationDam = 12
}
